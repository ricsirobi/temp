using System;
using UnityEngine;

public class UiProfileXPMeters : KAUI
{
	private class ProfilePetMeterData
	{
		public KAWidget _MeterWidget;

		public KAWidget _TitleWidget;

		public Guid? _PetID;

		public ProfilePetMeterData(KAWidget meterWidget, KAWidget titleWidget)
		{
			_MeterWidget = meterWidget;
			_TitleWidget = titleWidget;
		}
	}

	public Texture2D _XPMetersMinimizedBkg;

	public Texture2D _XPMetersExpandedBkg;

	private KAWidget mDragonXPMeter;

	private KAWidget mFarmingXPMeter;

	private KAWidget mFishingXPMeter;

	private KAWidget mPlayerXPMeter;

	private KAWidget mDragonLevelTitle;

	private KAWidget mFishingLevelTitle;

	private KAWidget mFarmingLevelTitle;

	private KAWidget mPlayerLevelTitle;

	private KAWidget mExpandBtn;

	private KAWidget mMinimizeBtn;

	private KAWidget mXPMetersBkg;

	private KAWidget mDoubleXPBtn;

	private KAWidget mBtnClose;

	private bool mExpandXPMeters;

	public void OnSetVisibility(bool t)
	{
		SetVisibility(t);
	}

	private void ExpandOrMinimizeProfileXpMeters()
	{
		mExpandBtn.SetVisibility(!mExpandXPMeters);
		mMinimizeBtn.SetVisibility(mExpandXPMeters);
		mFarmingXPMeter.SetVisibility(mExpandXPMeters);
		mFarmingLevelTitle.SetVisibility(mExpandXPMeters);
		mFishingXPMeter.SetVisibility(mExpandXPMeters);
		mFishingLevelTitle.SetVisibility(mExpandXPMeters);
		if (mDoubleXPBtn != null)
		{
			if (mExpandXPMeters)
			{
				mDoubleXPBtn.SetPosition(mDoubleXPBtn.transform.position.x, -110f);
			}
			else
			{
				mDoubleXPBtn.SetPosition(mDoubleXPBtn.transform.position.x, -46f);
			}
		}
	}

	protected override void Start()
	{
		base.Start();
		mDragonXPMeter = FindItem("DragonXPMeter");
		mFarmingXPMeter = FindItem("FarmingXPMeter");
		mFishingXPMeter = FindItem("FishingXPMeter");
		mPlayerXPMeter = FindItem("PlayerXPMeter");
		mDragonLevelTitle = FindItem("TxtDragonLevelTitle");
		mFishingLevelTitle = FindItem("TxtFishingLevelTitle");
		mFarmingLevelTitle = FindItem("TxtFarmingLevelTitle");
		mPlayerLevelTitle = FindItem("TxtPlayerLevelTitle");
		mExpandBtn = FindItem("ExpandBtn");
		mMinimizeBtn = FindItem("MinimizeBtn");
		mXPMetersBkg = FindItem("BkgMeters");
		mDoubleXPBtn = FindItem("DoubleXPBtn");
		mBtnClose = FindItem("BtnClose");
	}

	public void ProfileDataReady(UserProfile p)
	{
		SetLevelandRankProgress(1, mPlayerXPMeter, mPlayerLevelTitle, "TxtPlayerLevelNum");
		SetLevelandRankProgress(8, mDragonXPMeter, mDragonLevelTitle, "TxtDragonLevelNum");
		SetLevelandRankProgress(10, mFishingXPMeter, mFishingLevelTitle, "TxtFishingLevelNum");
		SetLevelandRankProgress(9, mFarmingXPMeter, mFarmingLevelTitle, "TxtFarmingLevelNum");
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mExpandBtn)
		{
			mExpandXPMeters = true;
			mXPMetersBkg.SetTexture(_XPMetersExpandedBkg, inPixelPerfect: true);
			ExpandOrMinimizeProfileXpMeters();
		}
		else if (inWidget == mMinimizeBtn)
		{
			mExpandXPMeters = false;
			mXPMetersBkg.SetTexture(_XPMetersMinimizedBkg, inPixelPerfect: true);
			ExpandOrMinimizeProfileXpMeters();
		}
		else if (inWidget == mBtnClose)
		{
			OnSetVisibility(t: false);
			KAUI.RemoveExclusive(this);
		}
	}

	private void SetLevelandRankProgress(int inType, KAWidget inMeter, KAWidget inTitle, string inTitleValueName)
	{
		if (inType == 8)
		{
			KAWidget titleWidget = ((inTitle != null) ? inTitle.FindChildItem(inTitleValueName) : null);
			ProfilePetMeterData profilePetMeterData = new ProfilePetMeterData(inMeter, titleWidget);
			if (UiProfile.pUserProfile.UserID == UserInfo.pInstance.UserID)
			{
				if (SanctuaryManager.pCurPetData != null)
				{
					profilePetMeterData._PetID = SanctuaryManager.pCurPetData.EntityID;
					PetRankData.LoadUserRank(SanctuaryManager.pCurPetData, OnUserRankReady, forceLoad: false, profilePetMeterData);
				}
				else
				{
					OnUserRankReady(UserRankData.GetUserRankByType(8, 1), profilePetMeterData);
				}
			}
			else
			{
				WsWebService.GetSelectedRaisedPet(UiProfile.pUserProfile.UserID, selected: true, ServiceEventHandler, profilePetMeterData);
			}
			return;
		}
		UserRank userRankByType = UserRankData.GetUserRankByType(inType, UiProfile.pUserProfile.AvatarInfo.Achievements);
		if (inTitle != null)
		{
			KAWidget kAWidget = inTitle.FindChildItem(inTitleValueName);
			if (kAWidget != null)
			{
				kAWidget.SetText(userRankByType.RankID.ToString());
			}
		}
		if (inMeter != null)
		{
			UserRank nextRankByType = UserRankData.GetNextRankByType(inType, userRankByType.RankID);
			float progressLevel = 1f;
			if (userRankByType.RankID != nextRankByType.RankID)
			{
				UserAchievementInfo userAchievementInfoByType = UserRankData.GetUserAchievementInfoByType(UiProfile.pUserProfile.AvatarInfo.Achievements, inType);
				progressLevel = ((userAchievementInfoByType == null) ? 0f : ((float)(userAchievementInfoByType.AchievementPointTotal.Value - userRankByType.Value) / (float)(nextRankByType.Value - userRankByType.Value)));
			}
			inMeter.SetProgressLevel(progressLevel);
		}
	}

	private static void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE && inType == WsServiceType.GET_SELECTED_RAISED_PET)
		{
			RaisedPetData[] array = (RaisedPetData[])inObject;
			if (array.Length != 0)
			{
				((ProfilePetMeterData)inUserData)._PetID = array[0].EntityID;
				PetRankData.LoadUserRank(array[0], OnUserRankReady, forceLoad: false, inUserData);
			}
		}
	}

	private static void OnUserRankReady(UserRank rank, object userData)
	{
		if (rank == null || userData == null)
		{
			return;
		}
		ProfilePetMeterData profilePetMeterData = (ProfilePetMeterData)userData;
		if (profilePetMeterData._TitleWidget != null)
		{
			profilePetMeterData._TitleWidget.SetText(rank.RankID.ToString());
		}
		if (profilePetMeterData._MeterWidget != null)
		{
			UserRank nextRankByType = UserRankData.GetNextRankByType(8, rank.RankID);
			float progressLevel = 1f;
			if (rank.RankID != nextRankByType.RankID)
			{
				UserAchievementInfo userAchievementInfo = PetRankData.GetUserAchievementInfo(profilePetMeterData._PetID);
				progressLevel = ((userAchievementInfo == null) ? 0f : ((float)(userAchievementInfo.AchievementPointTotal.Value - rank.Value) / (float)(nextRankByType.Value - rank.Value)));
			}
			profilePetMeterData._MeterWidget.SetProgressLevel(progressLevel);
		}
	}
}
