public class BuddyListData : KAWidgetUserData
{
	public string _UserID;

	public bool _ShowAchievements;

	public ClanData _ClanData;

	private bool mLoaded;

	private UserAchievementInfo[] mAchievementInfo;

	public BuddyListData(string inUserID, bool inShowAchievements = false)
	{
		_UserID = inUserID;
		_ShowAchievements = inShowAchievements;
	}

	public void Load()
	{
		if (mLoaded)
		{
			return;
		}
		mLoaded = true;
		if (!string.IsNullOrEmpty(_UserID))
		{
			Group.Get(_UserID, OnGroupGet);
			if (_ShowAchievements)
			{
				WsWebService.GetAchievementsByUserID(_UserID, ServiceEventHandler, null);
			}
			else
			{
				SetAchievementDefault();
			}
		}
	}

	private void OnGroupGet(GetGroupsResult result, object inUserData)
	{
		if (result == null || !result.Success)
		{
			return;
		}
		Group.AddGroup(result.Groups[0]);
		if (!(_Item == null))
		{
			_ClanData = new ClanData(result.Groups[0]);
			_ClanData._Item = _Item;
			_ClanData.Load();
			if (UiBuddyList.pInstance != null)
			{
				UiBuddyList.pInstance.ClanDataUpdated(_Item);
			}
		}
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			ArrayOfUserAchievementInfo arrayOfUserAchievementInfo = (ArrayOfUserAchievementInfo)inObject;
			if (arrayOfUserAchievementInfo != null && arrayOfUserAchievementInfo.UserAchievementInfo.Length != 0)
			{
				mAchievementInfo = arrayOfUserAchievementInfo.UserAchievementInfo;
				UserAchievementInfo userAchievementInfoByType = UserRankData.GetUserAchievementInfoByType(mAchievementInfo, 11);
				KAWidget kAWidget = _Item.FindChildItem("AniTrophies");
				if (userAchievementInfoByType != null && kAWidget != null)
				{
					kAWidget.SetText(userAchievementInfoByType.AchievementPointTotal.HasValue ? userAchievementInfoByType.AchievementPointTotal.Value.ToString() : "0");
				}
			}
			else
			{
				SetAchievementDefault();
			}
			break;
		}
		case WsServiceEvent.ERROR:
			SetAchievementDefault();
			break;
		}
	}

	private void SetAchievementDefault()
	{
		KAWidget kAWidget = _Item.FindChildItem("AniTrophies");
		if (kAWidget != null)
		{
			kAWidget.SetText("0");
		}
	}
}
