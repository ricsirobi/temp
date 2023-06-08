public class UiProfileGameMenu : KAUIMenu
{
	private KAWidget mHighScore;

	private KAWidget mRank;

	private KAWidget mCoins;

	private KAWidget mAchievements;

	private KAWidget mFriends;

	private KAWidget mMythies;

	private KAWidget mSpells;

	private KAWidget mRoomVisits;

	private KAWidget mSocialRank;

	private KAWidget mStarPoints;

	private int mMorphCount;

	private bool mIsDataUpdated;

	public CommonInventoryDataInstance mInvData;

	protected override void Start()
	{
		base.Start();
		mHighScore = FindItem("txtHighScore");
		mRank = FindItem("txtRank");
		mCoins = FindItem("txtCoins");
		mAchievements = FindItem("txtAchievements");
		mFriends = FindItem("txtFriends");
		mMythies = FindItem("txtMythies");
		mSpells = FindItem("txtSpells");
		mRoomVisits = FindItem("txtRoomVisits");
		mSocialRank = FindItem("txtSocialRank");
		mStarPoints = FindItem("txtStarPoints");
	}

	public string GetGameString(UserProfile p)
	{
		return "";
	}

	public void ProfileDataReady(UserProfile p)
	{
		if (mHighScore != null)
		{
			mHighScore.FindChildItem("txtData").SetText(GetGameString(p));
		}
		UserAchievementInfo userAchievementInfoByType = UserRankData.GetUserAchievementInfoByType(p.AvatarInfo.Achievements, 1);
		mAchievements.FindChildItem("txtData").SetText(userAchievementInfoByType.AchievementPointTotal.HasValue ? userAchievementInfoByType.AchievementPointTotal.Value.ToString() : "0");
		UserRank userRank = UserRankData.GetUserRank(userAchievementInfoByType.RankID);
		if (userRank != null)
		{
			mRank.FindChildItem("txtData").SetText(userRank.Name);
		}
		if (mCoins != null)
		{
			mCoins.FindChildItem("txtData").SetText(p.GameCurrency.ToString());
		}
		if (mMythies != null)
		{
			mMythies.FindChildItem("txtData").SetText(p.MythieCount.ToString());
		}
		if (mFriends != null)
		{
			mFriends.FindChildItem("txtData").SetText(p.BuddyCount.ToString());
		}
		mMorphCount = 0;
		mIsDataUpdated = false;
		mSpells.FindChildItem("txtData").SetText(mMorphCount.ToString());
		if (mRoomVisits != null)
		{
			mRoomVisits.FindChildItem("txtData").SetText(p.RoomVisitCount.ToString());
		}
		UserRank userRankByType = UserRankData.GetUserRankByType(3, p.AvatarInfo.Achievements);
		if (userRankByType != null)
		{
			mSocialRank.FindChildItem("txtData").SetText(userRankByType.Name);
			if (mStarPoints != null)
			{
				KAWidget kAWidget = mStarPoints.FindChildItem("txtData");
				if (kAWidget != null)
				{
					userAchievementInfoByType = UserRankData.GetUserAchievementInfoByType(p.AvatarInfo.Achievements, 3);
					kAWidget.SetText((userAchievementInfoByType != null && userAchievementInfoByType.AchievementPointTotal.HasValue) ? userAchievementInfoByType.AchievementPointTotal.Value.ToString() : "0");
				}
			}
		}
		mInvData = CommonInventoryDataInstance.LoadByUserID(p.UserID);
	}

	protected override void Update()
	{
		base.Update();
		if (mIsDataUpdated || mInvData == null || !mInvData.pIsReady)
		{
			return;
		}
		mIsDataUpdated = true;
		UserItemData[] items = mInvData.pData.GetItems(321);
		if (items != null)
		{
			UserItemData[] array = items;
			foreach (UserItemData userItemData in array)
			{
				mMorphCount += userItemData.Quantity;
			}
			mSpells.FindChildItem("txtData").SetText(mMorphCount.ToString());
		}
	}
}
