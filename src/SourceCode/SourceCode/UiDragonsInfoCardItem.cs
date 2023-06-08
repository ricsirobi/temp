using System;
using UnityEngine;

public class UiDragonsInfoCardItem : KAWidget
{
	[Serializable]
	public class DragonImageMap
	{
		public int _PetTypeID;

		public string _PortraitSprite;
	}

	private bool mInitialized;

	protected GameObject mMsgObject;

	private int mSelectedPetID = -1;

	public LocaleString _DragonSelectErrorText = new LocaleString("Get to the walk mode, to make other dragon active.");

	public LocaleString _DragonLockedText = new LocaleString("Become at least a 3 months member to use {{Dragon}}!");

	private KAWidget mEnergyBar;

	private KAWidget mHappinessBar;

	private KAWidget mHappinessIco;

	private KAWidget mAccelerationBar;

	private KAWidget mAccelerationValue;

	private KAWidget mMaxSpeedBar;

	private KAWidget mMaxSpeedValue;

	private KAWidget mPitchRateBar;

	private KAWidget mPitchRateValue;

	private KAWidget mTurnRateBar;

	private KAWidget mTurnRateValue;

	private KAWidget mFirepowerBar;

	private KAWidget mFirepowerValue;

	private KAWidget mDragonIcon;

	private KAWidget mDragonPrimaryTypeIcon;

	private KAWidget mDragonSecondaryTypeIcon;

	private KAWidget mDragonPortrait;

	private KAWidget mDragonAdoptionDate;

	private KAWidget mDragonClassName;

	private KAWidget mDragonName;

	private KAWidget mDragonType;

	private KAWidget mDragonAttributesInfo;

	private KAWidget mRoomInfo;

	private KAWidget mDragonRoomName;

	private KAWidget mTxtPetLocked;

	private KAWidget mLvlXpMeterBar;

	private KAWidget mLvlXpText;

	private KAWidget mBtnSelectDragon;

	private KAWidget mBtnDragonMoveIn;

	private KAWidget mBtnStableQuest;

	private KAWidget mBtnVisitDragon;

	private KAWidget mBtnBecomeMember;

	private KAWidget mBtnUpgradeMember;

	private KAWidget mBtnChangeName;

	private RaisedPetData mSelectedPetData;

	public string pUserID;

	private bool mIsSelectBtnRequired;

	private bool mIsVisitBtnRequired;

	private bool mIsMoveInBtnRequired;

	public int pSelectedPetID
	{
		get
		{
			return mSelectedPetID;
		}
		set
		{
			mSelectedPetID = value;
			mSelectedPetData = null;
		}
	}

	public RaisedPetData pSelectedPetData
	{
		get
		{
			return mSelectedPetData;
		}
		set
		{
			mSelectedPetData = value;
			mSelectedPetID = mSelectedPetData.RaisedPetID;
		}
	}

	protected void InitWidget()
	{
		mEnergyBar = FindChildItem("EnergyXPMeterBar");
		mHappinessBar = FindChildItem("HappinessXPMeterBar");
		mHappinessIco = FindChildItem("HappinessIco");
		mAccelerationBar = FindChildItem("AccelerationXPMeterBar");
		mAccelerationValue = FindChildItem("TxtAccelerationValue");
		mMaxSpeedBar = FindChildItem("MaxSpeedXPMeterBar");
		mMaxSpeedValue = FindChildItem("TxtMaxSpeedValue");
		mPitchRateBar = FindChildItem("PitchRateXPMeterBar");
		mPitchRateValue = FindChildItem("TxtPitchRateValue");
		mTurnRateBar = FindChildItem("TurnRateXPMeterBar");
		mTurnRateValue = FindChildItem("TxtTurnRateValue");
		mFirepowerBar = FindChildItem("FirepowerMeter");
		mFirepowerValue = FindChildItem("TxtFirepowerValue");
		mDragonAttributesInfo = FindChildItem("DragonAttributesInfo");
		mRoomInfo = FindChildItem("RoomInfo");
		if (SanctuaryData.pInstance != null)
		{
			KAWidget kAWidget = FindChildItem("TxtAcceleration");
			if (kAWidget != null)
			{
				kAWidget.SetText(SanctuaryData.GetDisplayTextFromPetStat(PetStatType.ACCELERATION));
			}
			KAWidget kAWidget2 = FindChildItem("TxtMaxSpeed");
			if (kAWidget2 != null)
			{
				kAWidget2.SetText(SanctuaryData.GetDisplayTextFromPetStat(PetStatType.MAXSPEED));
			}
			KAWidget kAWidget3 = FindChildItem("TxtPitchRate");
			if (kAWidget3 != null)
			{
				kAWidget3.SetText(SanctuaryData.GetDisplayTextFromPetStat(PetStatType.PITCHRATE));
			}
			KAWidget kAWidget4 = FindChildItem("TxtTurnRate");
			if (kAWidget4 != null)
			{
				kAWidget4.SetText(SanctuaryData.GetDisplayTextFromPetStat(PetStatType.TURNRATE));
			}
			KAWidget kAWidget5 = FindChildItem("TxtFirepower");
			if (kAWidget5 != null)
			{
				kAWidget5.SetText(SanctuaryData.GetDisplayTextFromPetStat(PetStatType.FIREPOWER));
			}
		}
		mDragonIcon = FindChildItem("DragonIco");
		mDragonPrimaryTypeIcon = FindChildItem("PrimaryTypeIco");
		mDragonSecondaryTypeIcon = FindChildItem("SecondaryTypeIco");
		mDragonPortrait = FindChildItem("DragonPortrait");
		mDragonAdoptionDate = FindChildItem("TxtAdoptionDate");
		mDragonClassName = FindChildItem("TxtClassName");
		mDragonName = FindChildItem("TxtMyDragonName");
		mDragonType = FindChildItem("TxtDragonName");
		mDragonRoomName = FindChildItem("TxtRoomName");
		mTxtPetLocked = FindChildItem("TxtPetLocked");
		mLvlXpMeterBar = FindChildItem("LvlXPMeterBar");
		mLvlXpText = FindChildItem("TxtLvl");
		mBtnSelectDragon = FindChildItem("BtnSelectDragon");
		mBtnDragonMoveIn = FindChildItem("BtnDragonMoveIn");
		mBtnVisitDragon = FindChildItem("BtnDragonVisit");
		mBtnBecomeMember = FindChildItem("BtnBecomeMember");
		mBtnUpgradeMember = FindChildItem("BtnUpgradeMember");
		mBtnChangeName = FindChildItem("BtnChangeName");
		if (mBtnChangeName != null)
		{
			mBtnChangeName.SetVisibility(inVisible: false);
		}
		if (TimedMissionManager.pInstance.pIsEnabled)
		{
			mBtnStableQuest = FindChildItem("BtnStableQuest");
		}
		mInitialized = true;
	}

	public void SetMessageObject(GameObject msg)
	{
		mMsgObject = msg;
	}

	public override void OnClick()
	{
		base.OnClick();
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnVisitDragon)
		{
			if (mMsgObject != null)
			{
				mMsgObject.SendMessage("OnVisitDragon", mSelectedPetID, SendMessageOptions.DontRequireReceiver);
			}
			StableData byPetID = StableData.GetByPetID(mSelectedPetID);
			if (byPetID != null)
			{
				if (UiJournal.pInstance != null && UiJournal.pIsJournalActive)
				{
					UiJournal.pInstance.CloseJournal();
				}
				int nestID = byPetID.GetNestByPetID(mSelectedPetID)?.ID ?? (-1);
				StableManager.LoadStable(byPetID.ID, nestID);
			}
		}
		else if (inWidget == mBtnSelectDragon)
		{
			if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pIsMounted)
			{
				SanctuaryManager.pCurPetInstance.OnFlyDismount(AvAvatar.pObject);
			}
			if (mMsgObject != null)
			{
				mMsgObject.SendMessage("OnSelectDragonStart", mSelectedPetID, SendMessageOptions.DontRequireReceiver);
			}
			MakeActiveDragon();
			RefreshButtons();
			if (UiAvatarControls.pInstance != null)
			{
				UiAvatarControls.pInstance.pIsReady = false;
			}
		}
		else if (inWidget == mBtnDragonMoveIn)
		{
			if (mMsgObject != null)
			{
				mMsgObject.SendMessage("OnDragonMoveIn", mSelectedPetID, SendMessageOptions.DontRequireReceiver);
			}
			RefreshButtons();
		}
		else if (inWidget == mBtnBecomeMember || inWidget == mBtnUpgradeMember)
		{
			IAPManager.pInstance.InitPurchase(IAPStoreCategory.MEMBERSHIP, base.gameObject);
		}
		else if (inWidget == mBtnStableQuest)
		{
			if (UiJournal.pInstance != null && UiJournal.pIsJournalActive)
			{
				UiJournal.pInstance.CloseJournal();
			}
			if (mMsgObject != null)
			{
				mMsgObject.SendMessage("OnInteractivity", false, SendMessageOptions.DontRequireReceiver);
			}
			StableData byPetID2 = StableData.GetByPetID(mSelectedPetID);
			if (byPetID2 != null)
			{
				StableManager.LoadStableWithJobBoard(byPetID2.ID);
			}
			else
			{
				StableManager.LoadStableWithJobBoard(0);
			}
		}
		else if (inWidget == mBtnChangeName && SanctuaryManager.pInstance != null)
		{
			SanctuaryManager.pInstance.InitNameChange(mSelectedPetData, base.gameObject);
		}
	}

	private void OnNameChangeDone()
	{
		base.transform.root.gameObject.BroadcastMessage("RefreshUI", SendMessageOptions.DontRequireReceiver);
	}

	public void MakeActiveDragon()
	{
		if (SanctuaryManager.pCurPetData == null || SanctuaryManager.pCurPetData.RaisedPetID != mSelectedPetID)
		{
			if (mSelectedPetData == null)
			{
				mSelectedPetData = RaisedPetData.GetByID(mSelectedPetID);
			}
			if (SanctuaryManager.IsPetLocked(mSelectedPetData))
			{
				MembershipItemsInfo.ShowMembershipDB(base.gameObject, mSelectedPetData.Name);
				mMsgObject.SendMessage("OnSelectDragonFailed", mSelectedPetID, SendMessageOptions.DontRequireReceiver);
				return;
			}
			if (mMsgObject != null)
			{
				mMsgObject.SendMessage("OnInteractivity", false, SendMessageOptions.DontRequireReceiver);
			}
			KAUICursorManager.SetDefaultCursor("Loading");
			RaisedPetData.SetSelectedPet(mSelectedPetID, unselectOtherPets: true, SetPetHandler, mSelectedPetData);
		}
		else
		{
			OnPetReloaded(success: true);
		}
	}

	public void SetPetHandler(bool success)
	{
		if (success)
		{
			if (mSelectedPetData == null)
			{
				mSelectedPetData = RaisedPetData.GetByID(mSelectedPetID);
			}
			SanctuaryManager.SetAndSaveCurrentType(mSelectedPetData.PetTypeID);
			SanctuaryManager.pCurPetData = mSelectedPetData;
			if (SanctuaryManager.pInstance._CreateInstance)
			{
				if (SanctuaryManager.pCurPetInstance != null)
				{
					UnityEngine.Object.Destroy(SanctuaryManager.pCurPetInstance.gameObject);
					SanctuaryManager.pCurPetInstance = null;
				}
				SanctuaryManager.pInstance.ReloadPet(resetFollowFlag: true, base.gameObject);
			}
			else
			{
				OnPetReloaded(success: true);
			}
		}
		else
		{
			UtDebug.Log("Set Current pet failed");
			OnPetReloaded(success: false);
		}
	}

	private void OnPetReloaded(bool success)
	{
		if (mMsgObject != null)
		{
			if (success)
			{
				if (SanctuaryManager.pCurPetInstance != null)
				{
					SanctuaryManager.pCurPetInstance.gameObject.SetActive(value: true);
					SanctuaryManager.pCurPetInstance.SetAvatar(AvAvatar.mTransform);
					SanctuaryManager.pCurPetInstance.SetFollowAvatar(follow: true);
					SanctuaryManager.pCurPetInstance.MoveToAvatar(postponed: true);
				}
				mMsgObject.SendMessage("OnSelectDragonFinish", mSelectedPetID, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				mMsgObject.SendMessage("OnSelectDragonFailed", mSelectedPetID, SendMessageOptions.DontRequireReceiver);
			}
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
		RefreshButtons();
		if (mMsgObject != null)
		{
			mMsgObject.SendMessage("OnInteractivity", true, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void SetButtons(bool selectBtn, bool visitBtn, bool moveInBtn)
	{
		mIsSelectBtnRequired = selectBtn;
		mIsVisitBtnRequired = visitBtn;
		mIsMoveInBtnRequired = moveInBtn;
		RefreshButtons();
	}

	public void RefreshButtons()
	{
		bool flag = false;
		if (mSelectedPetData != null && SanctuaryManager.IsPetLocked(mSelectedPetData))
		{
			mDragonAttributesInfo.SetVisibility(inVisible: false);
			mRoomInfo.SetVisibility(inVisible: false);
			flag = true;
			string localizedString = _DragonLockedText.GetLocalizedString();
			localizedString = localizedString.Replace("{{Dragon}}", mSelectedPetData.Name);
			if (SubscriptionInfo.pIsMember && !IAPManager.IsMembershipUpgradeable())
			{
				localizedString = localizedString + "\n\n" + IAPManager.GetMembershipUpgradeText();
			}
			mTxtPetLocked.SetText(localizedString);
			mTxtPetLocked.SetVisibility(inVisible: true);
		}
		bool flag2 = false;
		if (TimedMissionManager.pInstance != null && TimedMissionManager.pInstance.IsPetEngaged(mSelectedPetID))
		{
			flag2 = true;
		}
		StableData byPetID = StableData.GetByPetID(mSelectedPetID);
		bool flag3 = mIsSelectBtnRequired && (SanctuaryManager.pCurPetData == null || SanctuaryManager.pCurPetData.RaisedPetID != mSelectedPetID) && byPetID != null;
		mBtnSelectDragon.SetVisibility(flag3 && !flag && !flag2);
		mBtnSelectDragon.SetDisabled(SanctuaryManager.pInstance != null && SanctuaryManager.pInstance.pDisablePetSwitch);
		mBtnDragonMoveIn.SetVisibility(mIsMoveInBtnRequired);
		if (TimedMissionManager.pInstance.pIsEnabled)
		{
			if (mBtnStableQuest != null)
			{
				mBtnStableQuest.SetVisibility(UiDragonsStable.pCurrentMode != UiDragonsStable.Mode.DragonInfo);
			}
			mBtnVisitDragon.SetVisibility(inVisible: false);
		}
		else
		{
			bool flag4 = mIsVisitBtnRequired && byPetID != null && byPetID != StableManager.pCurrentStableData && (SanctuaryManager.pCurPetData == null || SanctuaryManager.pCurPetData.RaisedPetID != mSelectedPetID);
			mBtnVisitDragon.SetVisibility(flag4 && !flag);
		}
		if (flag && !mIsMoveInBtnRequired)
		{
			if (SubscriptionInfo.pIsMember)
			{
				mBtnUpgradeMember.SetVisibility(IAPManager.IsMembershipUpgradeable());
			}
			else
			{
				mBtnBecomeMember.SetVisibility(inVisible: true);
			}
		}
		else
		{
			mBtnUpgradeMember.SetVisibility(inVisible: false);
			mBtnBecomeMember.SetVisibility(inVisible: false);
		}
	}

	public void RefreshUI()
	{
		if (!mInitialized)
		{
			InitWidget();
		}
		if (mSelectedPetID == -1 && mSelectedPetData == null)
		{
			return;
		}
		if (mSelectedPetData == null)
		{
			mSelectedPetData = RaisedPetData.GetByID(mSelectedPetID);
		}
		else
		{
			mSelectedPetID = mSelectedPetData.RaisedPetID;
		}
		if (mSelectedPetData == null)
		{
			return;
		}
		mDragonName.SetText(mSelectedPetData.Name);
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(mSelectedPetData.PetTypeID);
		mDragonType.SetText(sanctuaryPetTypeInfo._NameText.GetLocalizedString());
		DragonClassInfo dragonClassInfo = SanctuaryData.GetDragonClassInfo(sanctuaryPetTypeInfo._DragonClass);
		mDragonClassName.SetText(dragonClassInfo._InfoText.GetLocalizedString());
		StableData byPetID = StableData.GetByPetID(mSelectedPetData.RaisedPetID);
		if (byPetID != null)
		{
			mDragonRoomName.SetText(byPetID.pLocaleName);
		}
		else
		{
			mDragonRoomName.SetText("");
		}
		mDragonPortrait.SetVisibility(inVisible: false);
		int num = (mSelectedPetData.ImagePosition.HasValue ? mSelectedPetData.ImagePosition.Value : 0);
		if (!string.IsNullOrEmpty(pUserID) && UserInfo.pInstance.UserID != pUserID)
		{
			WsWebService.GetImageDataByUserId(pUserID, "EggColor", num, ServiceEventHandler, null);
		}
		else
		{
			ImageData.Load("EggColor", num, base.gameObject);
		}
		string iconSprite = SanctuaryData.GetDragonClassInfo(sanctuaryPetTypeInfo._DragonClass)._IconSprite;
		if (!string.IsNullOrEmpty(iconSprite))
		{
			mDragonIcon.pBackground.UpdateSprite(iconSprite);
		}
		if (mDragonPrimaryTypeIcon != null)
		{
			if (sanctuaryPetTypeInfo != null && sanctuaryPetTypeInfo.pPrimaryType != null && !string.IsNullOrEmpty(sanctuaryPetTypeInfo.pPrimaryType._IconSprite))
			{
				mDragonPrimaryTypeIcon.pBackground.UpdateSprite(sanctuaryPetTypeInfo.pPrimaryType._IconSprite);
				mDragonPrimaryTypeIcon.SetVisibility(inVisible: true);
			}
			else
			{
				mDragonPrimaryTypeIcon.SetVisibility(inVisible: false);
			}
		}
		if (mDragonSecondaryTypeIcon != null)
		{
			if (sanctuaryPetTypeInfo != null && sanctuaryPetTypeInfo.pSecondaryType != null && !string.IsNullOrEmpty(sanctuaryPetTypeInfo.pSecondaryType._IconSprite))
			{
				mDragonSecondaryTypeIcon.pBackground.UpdateSprite(sanctuaryPetTypeInfo.pSecondaryType._IconSprite);
				mDragonSecondaryTypeIcon.SetVisibility(inVisible: true);
			}
			else
			{
				mDragonSecondaryTypeIcon.SetVisibility(inVisible: false);
			}
		}
		mRoomInfo.SetVisibility(inVisible: true);
		mDragonAttributesInfo.SetVisibility(inVisible: true);
		mTxtPetLocked.SetVisibility(inVisible: false);
		if (mSelectedPetData.EntityID.HasValue)
		{
			PetRankData.LoadUserAchievementInfo(mSelectedPetData, OnPetInfoReady, forceLoad: false, mSelectedPetData);
		}
		else
		{
			mLvlXpMeterBar.SetProgressLevel(0f);
			mLvlXpText.SetText(mSelectedPetData.pRank.ToString());
			RaisedPetState raisedPetState = mSelectedPetData.FindStateData(SanctuaryPetMeterType.HAPPINESS.ToString());
			float num2 = 0f;
			float num3 = 1f;
			if (raisedPetState != null)
			{
				num2 = raisedPetState.Value;
				num3 = raisedPetState.Value;
			}
			num3 = SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.HAPPINESS, mSelectedPetData);
			float progressLevel = num2 / num3;
			mHappinessBar.SetProgressLevel(progressLevel);
			raisedPetState = mSelectedPetData.FindStateData(SanctuaryPetMeterType.ENERGY.ToString());
			float num4 = 0f;
			float num5 = 1f;
			if (raisedPetState != null)
			{
				num4 = raisedPetState.Value;
			}
			num5 = SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.ENERGY, mSelectedPetData);
			float progressLevel2 = num4 / num5;
			mEnergyBar.SetProgressLevel(progressLevel2);
		}
		string text = mSelectedPetData.CreateDate.ToString("MM/dd/yyyy");
		mDragonAdoptionDate.SetText(text);
		UpdateFlyingParams(mSelectedPetData);
		if (mBtnChangeName != null)
		{
			mBtnChangeName.SetVisibility(CanShowNameChangeButton());
		}
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE && inType == WsServiceType.GET_IMAGE_BY_USER_ID && inObject != null)
		{
			ImageData imageData = (ImageData)inObject;
			mDragonPortrait.SetTextureFromURL(imageData.ImageURL, base.gameObject);
		}
	}

	private void OnTextureLoaded(KAWidget inWidget)
	{
		inWidget.SetVisibility(inVisible: true);
	}

	private void OnPetInfoReady(UserAchievementInfo achievementInfo, object userData)
	{
		int num = achievementInfo?.AchievementPointTotal.Value ?? 0;
		RaisedPetData raisedPetData = (RaisedPetData)userData;
		UserRank userRank = PetRankData.GetUserRank(raisedPetData);
		UserRank nextRankByType = UserRankData.GetNextRankByType(8, userRank.RankID);
		float progressLevel = 1f;
		if (userRank.RankID != nextRankByType.RankID)
		{
			progressLevel = (float)(num - userRank.Value) / (float)(nextRankByType.Value - userRank.Value);
		}
		mLvlXpMeterBar.SetProgressLevel(progressLevel);
		mLvlXpText.SetText(userRank.RankID.ToString());
		RaisedPetState raisedPetState = raisedPetData.FindStateData(SanctuaryPetMeterType.HAPPINESS.ToString());
		float num2 = 0f;
		float num3 = 1f;
		if (raisedPetState != null)
		{
			num2 = raisedPetState.Value;
			num3 = raisedPetState.Value;
		}
		num3 = SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.HAPPINESS, raisedPetData);
		float num4 = num2 / num3;
		mHappinessBar.SetProgressLevel(num4);
		SanctuaryPetTypeSettings sanctuaryPetSettings = SanctuaryData.GetSanctuaryPetSettings(SanctuaryData.FindSanctuaryPetTypeInfo(mSelectedPetData.PetTypeID)._Settings);
		if (num4 >= sanctuaryPetSettings._FiredUpThreshold)
		{
			if (mHappinessIco.GetCurrentAnim() != "FiredUp")
			{
				mHappinessIco.PlayAnim("FiredUp");
			}
		}
		else if (num4 >= sanctuaryPetSettings._HappyThreshold)
		{
			if (mHappinessIco.GetCurrentAnim() != "Happy")
			{
				mHappinessIco.PlayAnim("Happy");
			}
		}
		else if (mHappinessIco.GetCurrentAnim() != "Angry")
		{
			mHappinessIco.PlayAnim("Angry");
		}
		raisedPetState = raisedPetData.FindStateData(SanctuaryPetMeterType.ENERGY.ToString());
		float num5 = 0f;
		float num6 = 1f;
		if (raisedPetState != null)
		{
			num5 = raisedPetState.Value;
		}
		num6 = SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.ENERGY, raisedPetData);
		float progressLevel2 = num5 / num6;
		mEnergyBar.SetProgressLevel(progressLevel2);
	}

	public void OnImageLoaded(ImageDataInstance img)
	{
		if (!(img.mIconTexture == null) && mSelectedPetData != null && mSelectedPetData.ImagePosition.Value == img.mSlotIndex)
		{
			mDragonPortrait.SetTexture(img.mIconTexture);
			mDragonPortrait.SetVisibility(inVisible: true);
		}
	}

	public void UpdateFlyingParams(RaisedPetData pData)
	{
		float progressLevel = 0f;
		float progressLevel2 = 0f;
		float progressLevel3 = 0f;
		float progressLevel4 = 0f;
		float progressLevel5 = 0f;
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(pData.PetTypeID);
		if (sanctuaryPetTypeInfo != null)
		{
			progressLevel = SanctuaryData.pInstance.GetFinalStatsValue(pData.pStage, PetStatType.ACCELERATION, sanctuaryPetTypeInfo._Stats._Acceleration) / SanctuaryData.pInstance.GetFinalStatsValue(pData.pStage, PetStatType.MAXVALUE, sanctuaryPetTypeInfo._Stats._MaxValue);
			progressLevel2 = SanctuaryData.pInstance.GetFinalStatsValue(pData.pStage, PetStatType.MAXSPEED, sanctuaryPetTypeInfo._Stats._MaxSpeed) / SanctuaryData.pInstance.GetFinalStatsValue(pData.pStage, PetStatType.MAXVALUE, sanctuaryPetTypeInfo._Stats._MaxValue);
			progressLevel3 = SanctuaryData.pInstance.GetFinalStatsValue(pData.pStage, PetStatType.PITCHRATE, sanctuaryPetTypeInfo._Stats._PitchRate) / SanctuaryData.pInstance.GetFinalStatsValue(pData.pStage, PetStatType.MAXVALUE, sanctuaryPetTypeInfo._Stats._MaxValue);
			progressLevel4 = SanctuaryData.pInstance.GetFinalStatsValue(pData.pStage, PetStatType.TURNRATE, sanctuaryPetTypeInfo._Stats._TurnRate) / SanctuaryData.pInstance.GetFinalStatsValue(pData.pStage, PetStatType.MAXVALUE, sanctuaryPetTypeInfo._Stats._MaxValue);
			progressLevel5 = SanctuaryData.pInstance.GetFinalStatsValue(pData.pStage, PetStatType.FIREPOWER, sanctuaryPetTypeInfo._Stats._FirePower) / SanctuaryData.pInstance.GetFinalStatsValue(pData.pStage, PetStatType.MAXVALUE, sanctuaryPetTypeInfo._Stats._MaxValue);
		}
		mAccelerationBar.SetProgressLevel(progressLevel);
		mAccelerationValue.SetText($"{SanctuaryData.pInstance.GetFinalStatsValue(pData.pStage, PetStatType.ACCELERATION, sanctuaryPetTypeInfo._Stats._Acceleration):F1}");
		mPitchRateBar.SetProgressLevel(progressLevel3);
		mPitchRateValue.SetText($"{SanctuaryData.pInstance.GetFinalStatsValue(pData.pStage, PetStatType.PITCHRATE, sanctuaryPetTypeInfo._Stats._PitchRate):F1}");
		mTurnRateBar.SetProgressLevel(progressLevel4);
		mTurnRateValue.SetText($"{SanctuaryData.pInstance.GetFinalStatsValue(pData.pStage, PetStatType.TURNRATE, sanctuaryPetTypeInfo._Stats._TurnRate):F1}");
		mMaxSpeedBar.SetProgressLevel(progressLevel2);
		mMaxSpeedValue.SetText($"{SanctuaryData.pInstance.GetFinalStatsValue(pData.pStage, PetStatType.MAXSPEED, sanctuaryPetTypeInfo._Stats._MaxSpeed):F1}");
		mFirepowerBar.SetProgressLevel(progressLevel5);
		mFirepowerValue.SetText($"{SanctuaryData.pInstance.GetFinalStatsValue(pData.pStage, PetStatType.FIREPOWER, sanctuaryPetTypeInfo._Stats._FirePower):F1}");
	}

	private void OnIAPStoreClosed()
	{
		if (SubscriptionInfo.pIsMember)
		{
			RefreshUI();
			RefreshButtons();
			if (mMsgObject != null)
			{
				mMsgObject.SendMessage("OnIAPStoreClosed", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	private bool CanShowNameChangeButton()
	{
		bool num = SanctuaryData.IsNameChangeAllowed(mSelectedPetData);
		bool flag = UiProfile.pUserProfile == null || UiProfile.pUserProfile.UserID == UserInfo.pInstance.UserID;
		return num && AvAvatar.pLevelState != AvAvatarLevelState.FLIGHTSCHOOL && AvAvatar.pLevelState != AvAvatarLevelState.RACING && flag;
	}
}
