public class UiFarmLeaderboardMenu : KAUIMenu
{
	public int _NumRecords = 100;

	public string _FillStarSprite = "IcoDWDragonsVeterancy04";

	protected RequestType mLaunchPage = RequestType.All;

	protected UiFarmLeaderboard mParentUI;

	private ArrayOfUserRatingRankInfo[] mFarmLeaderboardSummary;

	public virtual void Init()
	{
		mParentUI = (UiFarmLeaderboard)_ParentUi;
	}

	public virtual void GetLeaderboardData(RequestType type)
	{
		ClearItems();
		mLaunchPage = type;
		ArrayOfUserRatingRankInfo leaderboardSummary = GetLeaderboardSummary();
		if (leaderboardSummary != null)
		{
			SetMenuData(leaderboardSummary);
			return;
		}
		mParentUI.SetInteractive(interactive: false);
		if (mParentUI.pTxtGettingRankings != null)
		{
			mParentUI.pTxtGettingRankings.SetVisibility(inVisible: true);
		}
		string groupID = UserProfile.pProfileData.GetGroupID();
		if (type == RequestType.Group && string.IsNullOrEmpty(groupID))
		{
			ShowNoClanMessage();
			return;
		}
		FarmManager farmManager = MyRoomsIntMain.pInstance as FarmManager;
		WsWebService.GetTopRanksWithUserIDs(UserInfo.pInstance.UserID, farmManager._LeaderboardCategoryID, _NumRecords, type, null, ServiceEventHandler, null);
	}

	protected void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType != WsServiceType.GET_TOP_RANKS_WITH_USERID)
		{
			return;
		}
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			mParentUI.SetInteractive(interactive: true);
			if (mParentUI.pTxtGettingRankings != null)
			{
				mParentUI.pTxtGettingRankings.SetVisibility(inVisible: false);
			}
			ArrayOfUserRatingRankInfo arrayOfUserRatingRankInfo = (ArrayOfUserRatingRankInfo)inObject;
			if (arrayOfUserRatingRankInfo != null && arrayOfUserRatingRankInfo.UserRatingRankInfo != null)
			{
				SetMenuData(arrayOfUserRatingRankInfo);
				AddToLeaderboardSummary(arrayOfUserRatingRankInfo);
			}
			break;
		}
		case WsServiceEvent.ERROR:
			mParentUI.SetInteractive(interactive: true);
			if (mParentUI.pTxtGettingRankings != null)
			{
				mParentUI.pTxtGettingRankings.SetVisibility(inVisible: false);
			}
			UtDebug.Log("GetTopRanksWithUserIDs call failed ");
			break;
		}
	}

	protected virtual void AddData(string rank, string key, UserRatingRankInfo data)
	{
		KAWidget parent = null;
		AddDataCommon(ref parent, rank, key, data);
	}

	protected void AddDataCommon(ref KAWidget parent, string rank, string key, UserRatingRankInfo data)
	{
		parent = AddWidget(key, null);
		parent.SetVisibility(inVisible: true);
		KAWidget kAWidget = parent.FindChildItem("TxtPlayerRank");
		if (kAWidget != null)
		{
			kAWidget.SetText(rank);
		}
		kAWidget = parent.FindChildItem("TxtPlayerName");
		if (kAWidget != null)
		{
			kAWidget.SetText(data.UserName);
		}
		kAWidget = parent.FindChildItem("FarmStarsIcon");
		if (kAWidget != null)
		{
			UpdateStarClick((int)data.RankInfo.RatingAverage, kAWidget);
		}
		kAWidget = parent.FindChildItem("TxtPlayerVotes");
		if (kAWidget != null)
		{
			kAWidget.SetText(data.RankInfo.TotalVotes.ToString());
		}
	}

	protected void SetMenuData(ArrayOfUserRatingRankInfo gameData)
	{
		for (int i = 0; i < gameData.UserRatingRankInfo.Length; i++)
		{
			UserRatingRankInfo data = gameData.UserRatingRankInfo[i];
			AddData((i + 1).ToString(), gameData.UserRatingRankInfo[0].RatedUserID.ToString(), data);
		}
	}

	private void UpdateStarClick(int starIndex, KAWidget starsParent)
	{
		starsParent.SetVisibility(inVisible: true);
		UISprite[] componentsInChildren = starsParent.GetComponentsInChildren<UISprite>();
		if (starIndex > componentsInChildren.Length)
		{
			starIndex = componentsInChildren.Length;
		}
		for (int i = 0; i <= componentsInChildren.Length && i < starIndex; i++)
		{
			if (componentsInChildren[i] != null)
			{
				componentsInChildren[i].spriteName = _FillStarSprite;
			}
		}
	}

	protected void AddToLeaderboardSummary(ArrayOfUserRatingRankInfo dataSummary)
	{
		if (mFarmLeaderboardSummary == null)
		{
			mFarmLeaderboardSummary = new ArrayOfUserRatingRankInfo[6];
		}
		mFarmLeaderboardSummary[(int)mLaunchPage] = dataSummary;
	}

	protected ArrayOfUserRatingRankInfo GetLeaderboardSummary()
	{
		if (mFarmLeaderboardSummary == null || mFarmLeaderboardSummary[(int)mLaunchPage] == null)
		{
			return null;
		}
		return mFarmLeaderboardSummary[(int)mLaunchPage];
	}

	private void ShowNoClanMessage()
	{
		ClearItems();
		mParentUI.SetInteractive(interactive: true);
		if (mParentUI.pTxtGettingRankings != null)
		{
			mParentUI.pTxtGettingRankings.SetVisibility(inVisible: false);
		}
		KAWidget kAWidget = AddWidget("NoClan", null);
		kAWidget.SetVisibility(inVisible: true);
		KAWidget kAWidget2 = kAWidget.FindChildItem("TxtPlayerName");
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
			kAWidget2 = kAWidget.FindChildItem("TxtPlayerRank");
			if (kAWidget2 != null)
			{
				kAWidget2.SetVisibility(inVisible: false);
			}
			kAWidget2 = kAWidget.FindChildItem("TxtPlayerVotes");
			if (kAWidget2 != null)
			{
				kAWidget2.SetVisibility(inVisible: false);
			}
		}
	}
}
