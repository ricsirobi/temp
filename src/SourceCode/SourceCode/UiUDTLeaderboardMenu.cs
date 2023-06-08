using System;
using UnityEngine;

public class UiUDTLeaderboardMenu : KAUIMenu
{
	[Serializable]
	public class UDTLeaderboardColorData
	{
		public int _Count;

		public Color _TxtColor;
	}

	protected UiUDTLeaderboard mParentUI;

	protected RequestType mLaunchPage = RequestType.All;

	protected ModeType mUDTEventFilter = ModeType.AllTime;

	public UDTLeaderboardColorData[] _TextColorData;

	private UserAchievementInfoResponse[][] mUDTLeaderBoardSummary;

	private GameData mPlayerData;

	protected override void Start()
	{
		base.Start();
		if (_ParentUi != null)
		{
			mParentUI = (UiUDTLeaderboard)_ParentUi;
		}
	}

	public virtual void LoadGameDataSummary(RequestType highScoresDisplayPage, ModeType userMode)
	{
		SetVisibility(isVisible: true);
		mLaunchPage = highScoresDisplayPage;
		mUDTEventFilter = userMode;
		UserAchievementInfoResponse gameDataSummary = GetGameDataSummary();
		if (gameDataSummary != null)
		{
			SetMenuData(gameDataSummary);
			return;
		}
		string UserId = string.Empty;
		int gameID = 0;
		bool isMultiPlayer = false;
		int difficulty = 0;
		int levelID = 0;
		string groupID = string.Empty;
		bool AscendingOrder = false;
		int count = 100;
		string key = string.Empty;
		int Score = 0;
		GetGameData(ref UserId, ref gameID, ref isMultiPlayer, ref difficulty, ref levelID, ref groupID, ref AscendingOrder, ref count, ref key, ref Score);
		if (count == 0)
		{
			UtDebug.LogError("Requested Data Count for Highscore Is 0");
			return;
		}
		mParentUI.SetInteractive(inInteractive: false);
		if (mParentUI.pTxtGettingHighScores != null)
		{
			mParentUI.pTxtGettingHighScores.SetVisibility(inVisible: true);
		}
		if (highScoresDisplayPage == RequestType.Group && string.IsNullOrEmpty(groupID))
		{
			ShowNoClanMessage();
			return;
		}
		mParentUI.pIsBusy = true;
		WsWebService.GetTopAchievementPointUsers(UserId, 1, count, highScoresDisplayPage, userMode, 12, null, ServiceEventHandler, null);
	}

	protected void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.GET_TOP_ACHIEVEMENT_POINT_USERS:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				mParentUI.SetInteractive(inInteractive: true);
				if (mParentUI.pTxtGettingHighScores != null)
				{
					mParentUI.pTxtGettingHighScores.SetVisibility(inVisible: false);
				}
				UserAchievementInfoResponse userAchievementInfoResponse = (UserAchievementInfoResponse)inObject;
				if (userAchievementInfoResponse != null)
				{
					SetMenuData(userAchievementInfoResponse);
					AddToGameDataSummary(userAchievementInfoResponse);
				}
				mParentUI.pIsBusy = false;
				break;
			}
			case WsServiceEvent.ERROR:
				mParentUI.SetInteractive(inInteractive: true);
				if (mParentUI.pTxtGettingHighScores != null)
				{
					mParentUI.pTxtGettingHighScores.SetVisibility(inVisible: false);
				}
				mParentUI.pIsBusy = false;
				break;
			}
			break;
		case WsServiceType.GET_DISPLAYNAME_BY_USER_ID:
		{
			if (inEvent != WsServiceEvent.COMPLETE)
			{
				break;
			}
			string inWidgetName = (string)inUserData;
			KAWidget kAWidget = FindItem(inWidgetName);
			if (kAWidget != null)
			{
				KAWidget kAWidget2 = kAWidget.FindChildItem("TxtPlayerName");
				if (kAWidget2 != null)
				{
					kAWidget2.SetText((string)inObject);
				}
			}
			break;
		}
		}
	}

	private Color GetTextColor(int rank)
	{
		Color result = Color.black;
		if (_TextColorData != null)
		{
			int num = -1;
			for (int i = 0; i < _TextColorData.Length; i++)
			{
				if (rank <= _TextColorData[i]._Count && (num == -1 || _TextColorData[i]._Count < _TextColorData[num]._Count))
				{
					num = i;
				}
			}
			if (num != -1)
			{
				result = _TextColorData[num]._TxtColor;
			}
		}
		return result;
	}

	protected void SetMenuData(UserAchievementInfoResponse dataSummary)
	{
		ClearItems();
		if (dataSummary == null || dataSummary.AchievementInfo == null)
		{
			return;
		}
		int rank = 0;
		if (dataSummary.DateRange.EndDate.HasValue)
		{
			mParentUI.ShowProgressInfo(dataSummary.DateRange);
		}
		for (int i = 0; i < dataSummary.AchievementInfo.Length; i++)
		{
			UserAchievementInfo userAchievementInfo = dataSummary.AchievementInfo[i];
			AddData((i + 1).ToString(), userAchievementInfo.UserID.Value.ToString(), userAchievementInfo, GetTextColor(i + 1));
			if (userAchievementInfo.UserID.Value.ToString() == UserInfo.pInstance.UserID)
			{
				rank = i + 1;
			}
		}
		if (UserRankData.pIsReady)
		{
			int score = 0;
			UserAchievementInfo userAchievementInfoByType = UserRankData.GetUserAchievementInfoByType(12);
			if (userAchievementInfoByType != null && userAchievementInfoByType.AchievementPointTotal.HasValue)
			{
				score = userAchievementInfoByType.AchievementPointTotal.Value;
			}
			mParentUI.SetPlayerData(rank, score);
		}
	}

	protected void AddDataCommon(ref KAWidget parent, string key, UserAchievementInfo data, string rank, Color txtColor)
	{
		parent = AddWidget(key, null);
		parent.SetVisibility(inVisible: true);
		KAWidget kAWidget = parent.FindChildItem("TxtPlayerRank");
		if (kAWidget != null)
		{
			kAWidget.SetText(rank);
			kAWidget.GetLabel().color = txtColor;
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = parent.FindChildItem("TxtPlayerName");
		if (kAWidget != null)
		{
			kAWidget.SetText(data.UserName);
			kAWidget.GetLabel().color = txtColor;
		}
		string inWidgetName = "TxtScoreLabel";
		kAWidget = parent.FindChildItem("TxtPlayerScore");
		if (kAWidget != null)
		{
			kAWidget.SetText(data.AchievementPointTotal.ToString());
			kAWidget.SetVisibility(inVisible: true);
			kAWidget.GetLabel().color = txtColor;
		}
		KAWidget kAWidget2 = parent.FindChildItem("UDTStarsIcon");
		if (kAWidget2 != null)
		{
			kAWidget2.SetVisibility(inVisible: true);
			UDTUtilities.UpdateUDTStars(kAWidget2.transform, "ProfileStarsIconBkg", data.AchievementPointTotal.Value);
		}
		if (mParentUI != null)
		{
			KAWidget kAWidget3 = mParentUI.FindItem(inWidgetName);
			if (kAWidget3 != null)
			{
				kAWidget3.SetVisibility(inVisible: true);
			}
		}
	}

	protected virtual void AddData(string rank, string key, UserAchievementInfo data, Color txtColor)
	{
		KAWidget parent = null;
		AddDataCommon(ref parent, key, data, rank, txtColor);
	}

	protected void AddToGameDataSummary(UserAchievementInfoResponse dataSummary)
	{
		if (mUDTLeaderBoardSummary == null)
		{
			mUDTLeaderBoardSummary = new UserAchievementInfoResponse[6][];
		}
		if (mUDTLeaderBoardSummary[(int)mLaunchPage] == null)
		{
			mUDTLeaderBoardSummary[(int)mLaunchPage] = new UserAchievementInfoResponse[5];
		}
		mUDTLeaderBoardSummary[(int)mLaunchPage][(int)mUDTEventFilter] = dataSummary;
	}

	protected UserAchievementInfoResponse GetGameDataSummary()
	{
		if (mUDTLeaderBoardSummary == null || mUDTLeaderBoardSummary[(int)mLaunchPage] == null)
		{
			return null;
		}
		return mUDTLeaderBoardSummary[(int)mLaunchPage][(int)mUDTEventFilter];
	}

	private void ShowNoClanMessage()
	{
		ClearItems();
		mParentUI.SetInteractive(inInteractive: true);
		if (mParentUI.pTxtGettingHighScores != null)
		{
			mParentUI.pTxtGettingHighScores.SetVisibility(inVisible: false);
		}
		KAWidget kAWidget = AddWidget("NoClan", null);
		kAWidget.SetVisibility(inVisible: true);
		KAWidget kAWidget2 = kAWidget.FindChildItem("TxtPlayerRank");
		if (kAWidget2 != null)
		{
			kAWidget2.SetVisibility(inVisible: false);
		}
		kAWidget2 = kAWidget.FindChildItem("TxtPlayerName");
		if (kAWidget2 != null)
		{
			if (mParentUI == null)
			{
				kAWidget2.SetText("You are not a clan member.");
			}
			else
			{
				kAWidget2.SetTextByID(mParentUI._NoClanMessageText._ID, mParentUI._NoClanMessageText._Text);
			}
			kAWidget2.SetVisibility(inVisible: true);
		}
		kAWidget2 = kAWidget.FindChildItem("TxtPlayerTime");
		if (kAWidget2 != null)
		{
			kAWidget2.SetVisibility(inVisible: false);
		}
		kAWidget2 = kAWidget.FindChildItem("TxtPlayerScore");
		if (kAWidget2 != null)
		{
			kAWidget2.SetVisibility(inVisible: false);
		}
		if (mParentUI != null)
		{
			KAWidget kAWidget3 = mParentUI.FindItem("TxtScoreLabel");
			if (kAWidget3 != null)
			{
				kAWidget3.SetVisibility(inVisible: true);
			}
		}
		kAWidget2 = kAWidget.FindChildItem("TxtPlayerDate");
		if (kAWidget2 != null)
		{
			kAWidget2.SetVisibility(inVisible: false);
		}
		kAWidget2 = kAWidget.FindChildItem("MemberIcon");
		if (kAWidget2 != null)
		{
			kAWidget2.SetVisibility(inVisible: false);
		}
	}

	protected void GetGameData(ref string UserId, ref int gameID, ref bool isMultiPlayer, ref int difficulty, ref int levelID, ref string groupID, ref bool AscendingOrder, ref int count, ref string key, ref int Score)
	{
		UserId = UserInfo.pInstance.UserID;
		gameID = HighScores.pInstance.GameID;
		isMultiPlayer = HighScores.pInstance.IsMultiPlayer;
		difficulty = HighScores.pInstance.Difficulty;
		levelID = HighScores.pInstance.Level;
		groupID = UserProfile.pProfileData.GetGroupID();
		AscendingOrder = false;
		count = 100;
		key = string.Empty;
		if (mParentUI != null)
		{
			AscendingOrder = mParentUI.pAscendingOrder;
			count = mParentUI.pCurrentTab._Count;
			key = mParentUI.pHighScoreKey;
			Score = mParentUI.pPlayerScore;
		}
	}

	public override void SetVisibility(bool isVisible)
	{
		base.SetVisibility(isVisible);
	}
}
