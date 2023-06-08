using System.Collections.Generic;
using UnityEngine;

public class UiStableQuestDragonSelect : KAUI
{
	public LocaleString _ActiveDragonSelectedText = new LocaleString("Active Dragon cannot be used for the quest, make some other dragon active to use this one for the quest.");

	public LocaleString _NonMemberText = new LocaleString("Become at least a 3 months member, and get NightFury to play this quest!");

	public bool _DragonStatusIconColorChange = true;

	public UiStableQuestDetail _ParentStableQuestDetailedUI;

	private KAWidget mDragonStatsParentWidget;

	private KAWidget mDragonStatsClose;

	private KAWidget mStatsDragonIcon;

	private KAWidget mStatsDragonName;

	private KAWidget mStatsDragonLevel;

	private KAWidget mStatsDragonAge;

	private KAWidget mStatsDragonClass;

	private KAWidget mStatsDragonType;

	private KAWidget mStatsMeterBarEnergyProgress;

	private KAWidget mStatsMeterBarEnergyTxt;

	private KAWidget mStatsMeterBarHealthProgress;

	private KAWidget mStatsMeterBarHealthTxt;

	private KAWidget mStatsPetXpMeterProgress;

	private KAWidget mStatsBtnRefillEnergy;

	private KAWidget mAgeUpBtn;

	private KAWidget mRefillEnergyBtn;

	private KAWidget mMakeActiveDragonBtn;

	private KAWidget mDragonsCount;

	private KAWidget mStatsHeader;

	private KAWidget mDragonPrimaryTypeIcon;

	private KAWidget mDragonPrimaryTypeFrame;

	private KAWidget mDragonSecondaryTypeIcon;

	private KAWidget mDragonSecondaryTypeFrame;

	public Color _ValidColor = new Color(0f, 255f, 0f);

	public Color _InvalidColor = new Color(255f, 0f, 0f);

	public LocaleString _LevelText = new LocaleString("Level - ");

	public LocaleString _InTeamText = new LocaleString("In Team");

	public LocaleString _AvailableText = new LocaleString("Available");

	public LocaleString _NotAvailableText = new LocaleString("Not Available");

	private UiStableQuestMain mStableQuestMainUI;

	private UiStableQuestDragonsMenu mMenu;

	private StablesPetUserData mCurrentPetUserData;

	private List<int> mRequiredAges = new List<int>();

	public UiStableQuestMain pStableQuestMainUI => mStableQuestMainUI;

	protected override void Start()
	{
		base.Start();
		mStableQuestMainUI = _ParentStableQuestDetailedUI._StableQuestMainUI;
		mMenu = (UiStableQuestDragonsMenu)_MenuList[0];
		mDragonStatsParentWidget = FindItem("DragonStats");
		mDragonStatsClose = FindItem("BtnDetailClose");
		mStatsDragonIcon = mDragonStatsParentWidget.FindChildItem("DragonInfo");
		mStatsDragonName = mDragonStatsParentWidget.FindChildItem("TxtDragonName");
		mStatsDragonLevel = mDragonStatsParentWidget.FindChildItem("TxtMeterBarXP");
		mStatsDragonAge = mDragonStatsParentWidget.FindChildItem("TxtAge");
		mStatsDragonClass = mDragonStatsParentWidget.FindChildItem("TxtClass");
		mStatsDragonType = mDragonStatsParentWidget.FindChildItem("TxtType");
		mStatsMeterBarEnergyProgress = mDragonStatsParentWidget.FindChildItem("EnergyRefillBar");
		mStatsMeterBarEnergyTxt = mDragonStatsParentWidget.FindChildItem("TxtEnergyCount");
		mStatsMeterBarHealthProgress = mDragonStatsParentWidget.FindChildItem("BkgHealthBar");
		mStatsMeterBarHealthTxt = mDragonStatsParentWidget.FindChildItem("TxtHealthCount");
		mStatsPetXpMeterProgress = mDragonStatsParentWidget.FindChildItem("DragonXpMeter");
		mAgeUpBtn = mDragonStatsParentWidget.FindChildItem("AgeUpBtn");
		mRefillEnergyBtn = mDragonStatsParentWidget.FindChildItem("BtnEnergyRefill");
		mMakeActiveDragonBtn = mDragonStatsParentWidget.FindChildItem("MakeActiveDragonBtn");
		mDragonsCount = mDragonStatsParentWidget.FindChildItem("TxtDragonCount");
		mStatsHeader = mDragonStatsParentWidget.FindChildItem("TxtAvailability");
		mDragonPrimaryTypeIcon = FindItem("PrimaryTypeIco");
		mDragonPrimaryTypeFrame = mDragonPrimaryTypeIcon.FindChildItem("Highlight");
		if (!_DragonStatusIconColorChange && mDragonPrimaryTypeFrame != null)
		{
			mDragonPrimaryTypeFrame.SetVisibility(inVisible: true);
		}
		mDragonSecondaryTypeIcon = FindItem("SecondaryTypeIco");
		mDragonSecondaryTypeFrame = mDragonSecondaryTypeIcon.FindChildItem("Highlight");
		if (!_DragonStatusIconColorChange && mDragonSecondaryTypeFrame != null)
		{
			mDragonSecondaryTypeFrame.SetVisibility(inVisible: true);
		}
		mDragonStatsParentWidget.SetVisibility(inVisible: false);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name.Contains("BtnConfirm"))
		{
			_ParentStableQuestDetailedUI.SetDragonSelectionMode(enable: false);
			mDragonStatsParentWidget.SetVisibility(inVisible: false);
		}
		else if (inWidget == mAgeUpBtn && mCurrentPetUserData != null)
		{
			_ParentStableQuestDetailedUI.SetVisibility(inVisible: false);
			RaisedPetStage[] array = new RaisedPetStage[mRequiredAges.Count];
			for (int i = 0; i < mRequiredAges.Count; i++)
			{
				array[i] = RaisedPetData.GetGrowthStage(mRequiredAges[i]);
			}
			DragonAgeUpConfig.ShowAgeUpUI(OnUiDragonAgeUpDone, null, OnUiDragonAgeUpBuy, mCurrentPetUserData.pData.pStage, mCurrentPetUserData.pData, array);
		}
		else if (inWidget == mRefillEnergyBtn && mCurrentPetUserData != null)
		{
			UiPetEnergyGenericDB.Show(base.gameObject, "OnEnergyUpdated", null, isLowEnergy: true);
		}
		else if (inWidget == mMakeActiveDragonBtn)
		{
			MakeActiveDragon();
		}
		else if (inWidget == mDragonStatsClose)
		{
			mDragonStatsParentWidget.SetVisibility(inVisible: false);
		}
	}

	private void MakeActiveDragon()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		SetInteractive(interactive: false);
		RaisedPetData.SetSelectedPet(mCurrentPetUserData.pData.RaisedPetID, unselectOtherPets: true, SetPetHandler, mCurrentPetUserData.pData);
	}

	private void SetPetHandler(bool success)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		SetInteractive(interactive: true);
		if (success)
		{
			if (mCurrentPetUserData.pData == null)
			{
				mCurrentPetUserData.pData = RaisedPetData.GetByID(mCurrentPetUserData.pData.RaisedPetID);
			}
			SanctuaryManager.SetAndSaveCurrentType(mCurrentPetUserData.pData.PetTypeID);
			SanctuaryManager.pCurPetData = mCurrentPetUserData.pData;
			if (SanctuaryManager.pCurPetInstance != null)
			{
				Object.Destroy(SanctuaryManager.pCurPetInstance.gameObject);
				SanctuaryManager.pCurPetInstance = null;
			}
			SanctuaryManager.pInstance.ReloadPet(resetFollowFlag: true, base.gameObject);
			RefreshUIWidgets();
			RefreshMenu();
			StableManager.RefreshActivePet();
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (inVisible)
		{
			mMenu = (UiStableQuestDragonsMenu)_MenuList[0];
			mMenu.LoadDragonsList();
		}
	}

	public void SetSelectedPetUserData(StablesPetUserData petUserData)
	{
		mCurrentPetUserData = petUserData;
		RefreshUIWidgets();
	}

	private void RefreshUIWidgets()
	{
		if (mCurrentPetUserData == null)
		{
			return;
		}
		mStatsDragonIcon.SetUserData(mCurrentPetUserData);
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(mCurrentPetUserData.pData.PetTypeID);
		int slotIdx = (mCurrentPetUserData.pData.ImagePosition.HasValue ? mCurrentPetUserData.pData.ImagePosition.Value : 0);
		ImageData.Load("EggColor", slotIdx, base.gameObject);
		mStatsDragonName.SetText(mCurrentPetUserData.pData.Name);
		if (mStatsDragonAge != null)
		{
			string text = mCurrentPetUserData.pData.pStage.ToString().ToLower();
			text = char.ToUpper(text[0]) + text.Substring(1);
			mStatsDragonAge.SetText(text);
		}
		if (mStatsDragonClass != null)
		{
			DragonClassInfo dragonClassInfo = SanctuaryData.GetDragonClassInfo(sanctuaryPetTypeInfo._DragonClass);
			mStatsDragonClass.SetText(dragonClassInfo._InfoText.GetLocalizedString());
		}
		if (mStatsDragonType != null)
		{
			mStatsDragonType.SetText(sanctuaryPetTypeInfo._NameText.GetLocalizedString());
		}
		int rankID = PetRankData.GetUserRank(mCurrentPetUserData.pData).RankID;
		mStatsDragonLevel.SetText(_LevelText.GetLocalizedString() + rankID);
		float num = SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.ENERGY, mCurrentPetUserData.pData);
		RaisedPetState raisedPetState = mCurrentPetUserData.pData.FindStateData(SanctuaryPetMeterType.ENERGY.ToString());
		float num2 = 0f;
		if (raisedPetState != null)
		{
			num2 = raisedPetState.Value;
		}
		if (num == 0f)
		{
			num = 1f;
		}
		float progressLevel = num2 / num;
		if (mStatsMeterBarEnergyProgress != null)
		{
			mStatsMeterBarEnergyProgress.SetProgressLevel(progressLevel);
		}
		if (num <= 1f)
		{
			num2 *= 100f;
			num *= 100f;
		}
		if ((bool)mStatsMeterBarEnergyTxt)
		{
			mStatsMeterBarEnergyTxt.SetText(Mathf.RoundToInt(num2) + " / " + Mathf.RoundToInt(num));
		}
		float num3 = SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.HEALTH, mCurrentPetUserData.pData);
		raisedPetState = mCurrentPetUserData.pData.FindStateData(SanctuaryPetMeterType.HEALTH.ToString());
		float num4 = 0f;
		if (raisedPetState != null)
		{
			num4 = raisedPetState.Value;
		}
		if (num3 == 0f)
		{
			num3 = 1f;
		}
		float progressLevel2 = num4 / num3;
		if (mStatsMeterBarHealthProgress != null)
		{
			mStatsMeterBarHealthProgress.SetProgressLevel(progressLevel2);
			mStatsMeterBarHealthProgress.GetProgressBar().color = _ValidColor;
		}
		if (num3 <= 1f)
		{
			num4 *= 100f;
			num3 *= 100f;
		}
		if ((bool)mStatsMeterBarHealthTxt)
		{
			mStatsMeterBarHealthTxt.SetText(Mathf.RoundToInt(num4) + " / " + Mathf.RoundToInt(num3));
		}
		int num5 = PetRankData.GetUserAchievementInfo(mCurrentPetUserData.pData)?.AchievementPointTotal.Value ?? 0;
		UserRank userRank = PetRankData.GetUserRank(mCurrentPetUserData.pData);
		UserRank nextRankByType = UserRankData.GetNextRankByType(8, userRank.RankID);
		float progressLevel3 = 1f;
		if (userRank.RankID != nextRankByType.RankID)
		{
			progressLevel3 = (float)(num5 - userRank.Value) / (float)(nextRankByType.Value - userRank.Value);
		}
		mStatsPetXpMeterProgress.SetProgressLevel(progressLevel3);
		mDragonStatsParentWidget.SetVisibility(inVisible: true);
		bool flag = false;
		foreach (int pLocalPetID in _ParentStableQuestDetailedUI.pLocalPetIDs)
		{
			if (pLocalPetID == mCurrentPetUserData.pData.RaisedPetID)
			{
				flag = true;
			}
		}
		if (mCurrentPetUserData.pData.RaisedPetID == SanctuaryManager.pCurPetData.RaisedPetID || TimedMissionManager.pInstance.IsPetEngaged(mCurrentPetUserData.pData.RaisedPetID) || flag || SanctuaryManager.IsPetLocked(mCurrentPetUserData.pData))
		{
			mMakeActiveDragonBtn.SetVisibility(inVisible: false);
		}
		else
		{
			mMakeActiveDragonBtn.SetVisibility(inVisible: true);
		}
		int num6 = (int)_ParentStableQuestDetailedUI.pCurrentMissionData.PetCount.Max;
		int count = _ParentStableQuestDetailedUI.pLocalPetIDs.Count;
		mDragonsCount.SetText(count + "/" + num6);
		bool flag2 = true;
		mAgeUpBtn.SetVisibility(inVisible: false);
		mRefillEnergyBtn.SetVisibility(inVisible: false);
		if (sanctuaryPetTypeInfo.pPrimaryType != null)
		{
			if (mDragonPrimaryTypeIcon != null)
			{
				string iconSprite = sanctuaryPetTypeInfo.pPrimaryType._IconSprite;
				if (!string.IsNullOrEmpty(iconSprite))
				{
					mDragonPrimaryTypeIcon.pBackground.UpdateSprite(iconSprite);
					mDragonPrimaryTypeIcon.SetText(sanctuaryPetTypeInfo.pPrimaryType._DisplayText.GetLocalizedString());
					mDragonPrimaryTypeIcon.SetVisibility(inVisible: true);
				}
				else
				{
					mDragonPrimaryTypeIcon.SetVisibility(inVisible: false);
				}
			}
		}
		else if (mDragonPrimaryTypeIcon != null)
		{
			mDragonPrimaryTypeIcon.SetVisibility(inVisible: false);
		}
		if (sanctuaryPetTypeInfo.pSecondaryType != null)
		{
			if (mDragonSecondaryTypeIcon != null)
			{
				string iconSprite2 = sanctuaryPetTypeInfo.pSecondaryType._IconSprite;
				if (!string.IsNullOrEmpty(iconSprite2))
				{
					mDragonSecondaryTypeIcon.pBackground.UpdateSprite(iconSprite2);
					mDragonSecondaryTypeIcon.SetText(sanctuaryPetTypeInfo.pSecondaryType._DisplayText.GetLocalizedString());
					mDragonSecondaryTypeIcon.SetVisibility(inVisible: true);
				}
				else
				{
					mDragonSecondaryTypeIcon.SetVisibility(inVisible: false);
				}
			}
		}
		else if (mDragonSecondaryTypeIcon != null)
		{
			mDragonSecondaryTypeIcon.SetVisibility(inVisible: false);
		}
		if (!TimedMissionManager.pInstance.IsPetValid(_ParentStableQuestDetailedUI.pCurrentMissionData, mCurrentPetUserData.pData.RaisedPetID, 0))
		{
			flag2 = false;
			mStatsDragonType.GetLabel().color = _InvalidColor;
		}
		else
		{
			mStatsDragonType.GetLabel().color = _ValidColor;
		}
		if (!TimedMissionManager.pInstance.IsPetValid(_ParentStableQuestDetailedUI.pCurrentMissionData, mCurrentPetUserData.pData.RaisedPetID, 1))
		{
			flag2 = false;
			mStatsDragonClass.GetLabel().color = _InvalidColor;
		}
		else
		{
			mStatsDragonClass.GetLabel().color = _ValidColor;
		}
		if (!TimedMissionManager.pInstance.IsPetValid(_ParentStableQuestDetailedUI.pCurrentMissionData, mCurrentPetUserData.pData.RaisedPetID, 2))
		{
			flag2 = false;
			mDragonPrimaryTypeIcon.GetLabel().color = _InvalidColor;
			if (_DragonStatusIconColorChange)
			{
				mDragonPrimaryTypeIcon.pBackground.color = _InvalidColor;
			}
			else
			{
				mDragonPrimaryTypeFrame.pBackground.color = _InvalidColor;
			}
		}
		else
		{
			mDragonPrimaryTypeIcon.GetLabel().color = _ValidColor;
			if (_DragonStatusIconColorChange)
			{
				mDragonPrimaryTypeIcon.pBackground.color = _ValidColor;
			}
			else
			{
				mDragonPrimaryTypeFrame.pBackground.color = _ValidColor;
			}
		}
		if (!TimedMissionManager.pInstance.IsPetValid(_ParentStableQuestDetailedUI.pCurrentMissionData, mCurrentPetUserData.pData.RaisedPetID, 3))
		{
			flag2 = false;
			mDragonSecondaryTypeIcon.GetLabel().color = _InvalidColor;
			if (_DragonStatusIconColorChange)
			{
				mDragonSecondaryTypeIcon.pBackground.color = _InvalidColor;
			}
			else
			{
				mDragonSecondaryTypeFrame.pBackground.color = _InvalidColor;
			}
		}
		else
		{
			mDragonSecondaryTypeIcon.GetLabel().color = _ValidColor;
			if (_DragonStatusIconColorChange)
			{
				mDragonSecondaryTypeIcon.pBackground.color = _ValidColor;
			}
			else
			{
				mDragonSecondaryTypeFrame.pBackground.color = _ValidColor;
			}
		}
		if (!TimedMissionManager.pInstance.IsPetValid(_ParentStableQuestDetailedUI.pCurrentMissionData, mCurrentPetUserData.pData.RaisedPetID, 6))
		{
			flag2 = false;
			mStatsDragonLevel.GetLabel().color = _InvalidColor;
		}
		else
		{
			mStatsDragonLevel.GetLabel().color = _ValidColor;
		}
		if (!TimedMissionManager.pInstance.IsPetValid(_ParentStableQuestDetailedUI.pCurrentMissionData, mCurrentPetUserData.pData.RaisedPetID, 4))
		{
			List<object> petValidityRequirement = TimedMissionManager.pInstance.GetPetValidityRequirement(_ParentStableQuestDetailedUI.pCurrentMissionData, mCurrentPetUserData.pData.RaisedPetID, FactorType.Age);
			if (petValidityRequirement.Count > 0 && flag2)
			{
				mRequiredAges.Clear();
				foreach (int item in petValidityRequirement)
				{
					mRequiredAges.Add(item);
				}
				mRequiredAges.Sort();
				if (mRequiredAges[0] > RaisedPetData.GetAgeIndex(mCurrentPetUserData.pData.pStage) && flag2)
				{
					mAgeUpBtn.SetVisibility(inVisible: true);
				}
			}
			flag2 = false;
			mStatsDragonAge.GetLabel().color = _InvalidColor;
		}
		else
		{
			mStatsDragonAge.GetLabel().color = _ValidColor;
		}
		if (!TimedMissionManager.pInstance.IsPetValid(_ParentStableQuestDetailedUI.pCurrentMissionData, mCurrentPetUserData.pData.RaisedPetID, 5))
		{
			float maxMeter = SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.ENERGY, mCurrentPetUserData.pData);
			List<object> petValidityRequirement2 = TimedMissionManager.pInstance.GetPetValidityRequirement(_ParentStableQuestDetailedUI.pCurrentMissionData, mCurrentPetUserData.pData.RaisedPetID, FactorType.Energy);
			RaisedPetState raisedPetState2 = mCurrentPetUserData.pData.FindStateData(SanctuaryPetMeterType.ENERGY.ToString());
			float num7 = 0f;
			if (raisedPetState2 != null)
			{
				num7 = raisedPetState2.Value;
			}
			int num8 = 0;
			if (petValidityRequirement2.Count > 0)
			{
				num8 = (int)petValidityRequirement2[0];
			}
			if (maxMeter >= (float)num8 && num7 < (float)num8 && flag2)
			{
				mRefillEnergyBtn.SetVisibility(inVisible: true);
			}
			flag2 = false;
			mStatsMeterBarEnergyProgress.GetProgressBar().color = _InvalidColor;
		}
		else
		{
			mStatsMeterBarEnergyProgress.GetProgressBar().color = _ValidColor;
		}
		if (flag2)
		{
			bool flag3 = false;
			foreach (int pLocalPetID2 in _ParentStableQuestDetailedUI.pLocalPetIDs)
			{
				if (pLocalPetID2 == mCurrentPetUserData.pData.RaisedPetID)
				{
					flag3 = true;
				}
			}
			if (flag3)
			{
				mStatsHeader.SetText(_InTeamText.GetLocalizedString());
			}
			else
			{
				mStatsHeader.SetText(_AvailableText.GetLocalizedString());
			}
		}
		else
		{
			mStatsHeader.SetText(_NotAvailableText.GetLocalizedString());
		}
	}

	private void RefreshMenu(int petID = -1)
	{
		if (!(mMenu == null))
		{
			mMenu.RefreshMenu(petID);
		}
	}

	public void OnImageLoaded(ImageDataInstance img)
	{
		if (!(img.mIconTexture == null))
		{
			StablesPetUserData stablesPetUserData = (StablesPetUserData)mStatsDragonIcon.GetUserData();
			if ((stablesPetUserData.pData.ImagePosition.HasValue ? stablesPetUserData.pData.ImagePosition.Value : 0) == img.mSlotIndex)
			{
				mStatsDragonIcon.FindChildItem("DragonIco").SetTexture(img.mIconTexture);
			}
		}
	}

	private void OnUiDragonAgeUpDone()
	{
		_ParentStableQuestDetailedUI.SetVisibility(inVisible: true);
		RefreshUIWidgets();
		RefreshMenu(mCurrentPetUserData.pData.RaisedPetID);
		StableManager.RefreshPets();
	}

	private void OnUiDragonAgeUpBuy()
	{
		UiStableQuestMain.pInstance.DestroyUI();
	}

	private void OnEnergyUpdated()
	{
		float num = 1f;
		num = SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.ENERGY, mCurrentPetUserData.pData);
		mCurrentPetUserData.pData.SetStateData(SanctuaryPetMeterType.ENERGY.ToString(), num);
		mCurrentPetUserData.pData.SaveDataReal(null, null, savePetMeterAlone: true);
		RefreshUIWidgets();
		RefreshMenu(mCurrentPetUserData.pData.RaisedPetID);
	}
}
