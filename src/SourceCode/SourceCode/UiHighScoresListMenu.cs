using System;

public class UiHighScoresListMenu : KAUIMenu
{
	protected UiHighScoresBox mParentUI;

	protected eLaunchPage mLaunchPage = eLaunchPage.HIGHSCORE;

	private GameDataSummary mGameDataSummary;

	private GameDataSummary mMyBuddyScoreSummary;

	private GameDataSummary mClanScoreSummary;

	private bool mRaceCompleted = true;

	public string _HighlightHexColor = "[FFA500]";

	private GameData mPlayerData;

	protected override void Start()
	{
		base.Start();
		if (_ParentUi != null)
		{
			mParentUI = (UiHighScoresBox)_ParentUi;
		}
	}

	public virtual void LoadGameDataSummary(eLaunchPage HighScoresDisplayPage)
	{
		if (HighScores.pInstance == null)
		{
			UtDebug.LogError("HighScore Instance Not Ready!!!");
			return;
		}
		SetVisibility(isVisible: true);
		mLaunchPage = HighScoresDisplayPage;
		GameDataSummary gameDataSummary = GetGameDataSummary();
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
		if (string.IsNullOrEmpty(key))
		{
			UtDebug.LogError("No Key For Highscore Data  Is Found");
			return;
		}
		if (count == 0)
		{
			UtDebug.LogError("Requested Data Count for Highscore Is 0");
			return;
		}
		mParentUI.pParentUI.SetInteractive(interactive: false);
		if (mParentUI.pTxtGettingHighScores != null)
		{
			mParentUI.pTxtGettingHighScores.SetVisibility(inVisible: true);
		}
		int value = (mRaceCompleted ? Score : (-1));
		switch (HighScoresDisplayPage)
		{
		case eLaunchPage.BUDDYSCORE:
			WsWebService.GetGameDataByGame(UserId, gameID, isMultiPlayer, difficulty, levelID, key, count, AscendingOrder, value, buddyFilter: true, ServiceEventHandler, null);
			break;
		case eLaunchPage.HIGHSCORE:
			WsWebService.GetGameDataByGameForDayRange(UserId, gameID, isMultiPlayer, difficulty, levelID, key, count, AscendingOrder, value, buddyFilter: false, ServerTime.pCurrentTime.AddDays(-30.0), ServerTime.pCurrentTime, ServiceEventHandler, null);
			break;
		case eLaunchPage.CLANSCORE:
			if (!string.IsNullOrEmpty(groupID))
			{
				WsWebService.GetGameDataByGroup(UserId, groupID, gameID, isMultiPlayer, difficulty, levelID, key, count, AscendingOrder, value, ServiceEventHandler, null);
			}
			else
			{
				ShowNoClanMessage();
			}
			break;
		}
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			mParentUI.pParentUI.SetInteractive(interactive: true);
			if (mParentUI.pTxtGettingHighScores != null)
			{
				mParentUI.pTxtGettingHighScores.SetVisibility(inVisible: false);
			}
			GameDataSummary gameDataSummary = (GameDataSummary)inObject;
			if (gameDataSummary != null)
			{
				SetMenuData(gameDataSummary);
				AddToGameDataSummary(gameDataSummary);
			}
			break;
		}
		case WsServiceEvent.ERROR:
			mParentUI.pParentUI.SetInteractive(interactive: true);
			if (mParentUI.pTxtGettingHighScores != null)
			{
				mParentUI.pTxtGettingHighScores.SetVisibility(inVisible: false);
			}
			break;
		}
	}

	protected void SetMenuData(GameDataSummary dataSummary)
	{
		string key = string.Empty;
		ClearItems();
		if (dataSummary.GameDataList == null || dataSummary.GameDataList.Length == 0)
		{
			mPlayerData = null;
		}
		else
		{
			key = dataSummary.Key;
			if (dataSummary.UserPosition.HasValue && dataSummary.UserPosition.Value < dataSummary.GameDataList.Length && mRaceCompleted)
			{
				mPlayerData = dataSummary.GameDataList[dataSummary.UserPosition.Value];
			}
			else
			{
				mPlayerData = null;
			}
			bool flag = false;
			for (int i = 0; i < dataSummary.GameDataList.Length && i < GetNumItemsPerPage(); i++)
			{
				GameData gameData = dataSummary.GameDataList[i];
				if (gameData.UserName == AvatarData.pInstance.DisplayName)
				{
					if (flag)
					{
						for (int j = i; j < dataSummary.GameDataList.Length; j++)
						{
							dataSummary.GameDataList[j].RankID--;
						}
						continue;
					}
					flag = true;
				}
				AddData(dataSummary.Key, gameData);
			}
		}
		if (mParentUI != null)
		{
			mParentUI.SetPlayerData(mPlayerData, key);
		}
	}

	protected void AddDataCommon(ref KAWidget parent, string key, GameData data)
	{
		parent = AddWidget(data.UserName, null);
		parent.SetVisibility(inVisible: true);
		KAWidget kAWidget = parent.FindChildItem("TxtPlayerRank");
		if (kAWidget != null)
		{
			if (data.RankID.HasValue)
			{
				kAWidget.SetText(data.RankID.Value.ToString());
			}
			else
			{
				kAWidget.SetText("---");
			}
			kAWidget.SetVisibility(inVisible: true);
			if (data.UserName == AvatarData.pInstance.DisplayName)
			{
				kAWidget.SetText(_HighlightHexColor + kAWidget.GetText() + "[-]");
			}
		}
		kAWidget = parent.FindChildItem("TxtPlayerName");
		if (kAWidget != null)
		{
			kAWidget.SetText(data.UserName);
			if (data.UserName == AvatarData.pInstance.DisplayName)
			{
				kAWidget.SetText(_HighlightHexColor + kAWidget.GetText() + "[-]");
			}
		}
		string empty = string.Empty;
		if (key == "time")
		{
			empty = "TxtTimeLabel";
			float time = (float)data.Value / 100f;
			kAWidget = parent.FindChildItem("TxtPlayerTime");
			if (kAWidget != null)
			{
				kAWidget.SetText(GameUtilities.FormatTime(time));
				kAWidget.SetVisibility(inVisible: true);
				if (data.UserName == AvatarData.pInstance.DisplayName)
				{
					kAWidget.SetText(_HighlightHexColor + kAWidget.GetText() + "[-]");
				}
			}
		}
		else
		{
			empty = "TxtScoreLabel";
			kAWidget = parent.FindChildItem("TxtPlayerScore");
			if (kAWidget != null)
			{
				kAWidget.SetText(data.Value.ToString());
				kAWidget.SetVisibility(inVisible: true);
				if (data.UserName == AvatarData.pInstance.DisplayName)
				{
					kAWidget.SetText(_HighlightHexColor + kAWidget.GetText() + "[-]");
				}
			}
		}
		if (mParentUI != null)
		{
			KAWidget kAWidget2 = mParentUI.FindItem(empty);
			if (kAWidget2 != null)
			{
				kAWidget2.SetVisibility(inVisible: true);
			}
		}
	}

	protected virtual void AddData(string key, GameData data)
	{
		KAWidget parent = null;
		AddDataCommon(ref parent, key, data);
		DateTime value = data.DatePlayed.Value;
		KAWidget kAWidget = parent.FindChildItem("TxtPlayerDate");
		if (kAWidget != null)
		{
			kAWidget.SetText(value.Month + "/" + value.Day + "/" + value.Year);
		}
		kAWidget = parent.FindChildItem("MemberIcon");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(data.IsMember);
		}
	}

	protected void AddToGameDataSummary(GameDataSummary dataSummary)
	{
		if (mLaunchPage == eLaunchPage.HIGHSCORE)
		{
			mGameDataSummary = dataSummary;
		}
		else if (mLaunchPage == eLaunchPage.BUDDYSCORE)
		{
			mMyBuddyScoreSummary = dataSummary;
		}
		else if (mLaunchPage == eLaunchPage.CLANSCORE)
		{
			mClanScoreSummary = dataSummary;
		}
	}

	protected GameDataSummary GetGameDataSummary()
	{
		if (mLaunchPage == eLaunchPage.HIGHSCORE)
		{
			return mGameDataSummary;
		}
		if (mLaunchPage == eLaunchPage.BUDDYSCORE)
		{
			return mMyBuddyScoreSummary;
		}
		if (mLaunchPage == eLaunchPage.CLANSCORE)
		{
			return mClanScoreSummary;
		}
		return null;
	}

	private void ShowNoClanMessage()
	{
		ClearItems();
		mParentUI.pParentUI.SetInteractive(interactive: true);
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
		if (!isVisible)
		{
			mGameDataSummary = null;
			mMyBuddyScoreSummary = null;
			mClanScoreSummary = null;
		}
	}

	public void UpdateRaceCompleteStatus(bool isComplete)
	{
		mRaceCompleted = isComplete;
	}
}
