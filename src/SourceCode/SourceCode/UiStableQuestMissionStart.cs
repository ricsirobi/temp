using UnityEngine;

public class UiStableQuestMissionStart : KAUI
{
	public LocaleString _MissionDurationText = new LocaleString("Mission Duration - {Result}");

	public LocaleString _MissionSuccessPercentageText = new LocaleString("{Result}% chance to win");

	public UiStableQuestDetail _ParentStableQuestDetailedUI;

	private KAUIMenu mMenu;

	protected override void Start()
	{
		base.Start();
		mMenu = _MenuList[0];
		if (UtPlatform.IsiOS())
		{
			KAWidget kAWidget = FindItem("YesBtn");
			KAWidget kAWidget2 = FindItem("NoBtn");
			if (kAWidget != null && kAWidget2 != null)
			{
				Vector3 localPosition = kAWidget2.transform.localPosition;
				kAWidget2.transform.localPosition = kAWidget.transform.localPosition;
				kAWidget.transform.localPosition = localPosition;
			}
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (inVisible)
		{
			_ParentStableQuestDetailedUI.SetInteractive(interactive: false);
			RefreshUi();
		}
		else
		{
			_ParentStableQuestDetailedUI.SetInteractive(interactive: true);
		}
	}

	private void RefreshUi()
	{
		mMenu.ClearItems();
		if (_ParentStableQuestDetailedUI.pLocalPetIDs == null)
		{
			return;
		}
		for (int i = 0; i < _ParentStableQuestDetailedUI.pLocalPetIDs.Count; i++)
		{
			KAWidget kAWidget = DuplicateWidget(mMenu._Template);
			kAWidget.SetVisibility(inVisible: true);
			mMenu.AddWidget(kAWidget);
			RaisedPetData byID = RaisedPetData.GetByID(_ParentStableQuestDetailedUI.pLocalPetIDs[i]);
			StablesPetUserData userData = new StablesPetUserData(byID);
			kAWidget.SetUserData(userData);
			if (TimedMissionManager.pInstance.GetWinProbabilityForPet(_ParentStableQuestDetailedUI.pCurrentSlotData.pMission, byID.RaisedPetID) > (float)_ParentStableQuestDetailedUI.pCurrentSlotData.pMission.WinFactorPerDragon)
			{
				KAWidget kAWidget2 = kAWidget.FindChildItem("SecondaryType");
				if (kAWidget2 != null)
				{
					SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(byID.PetTypeID);
					if (sanctuaryPetTypeInfo != null && sanctuaryPetTypeInfo.pSecondaryType != null && !string.IsNullOrEmpty(sanctuaryPetTypeInfo.pSecondaryType._IconSprite))
					{
						kAWidget2.FindChildItem("SecondaryTypeIco").pBackground.UpdateSprite(sanctuaryPetTypeInfo.pSecondaryType._IconSprite);
						kAWidget2.SetVisibility(inVisible: true);
					}
					else
					{
						kAWidget2.SetVisibility(inVisible: false);
					}
				}
			}
			int slotIdx = (byID.ImagePosition.HasValue ? byID.ImagePosition.Value : 0);
			ImageData.Load("EggColor", slotIdx, base.gameObject);
		}
		string localizedString = _MissionDurationText.GetLocalizedString();
		localizedString = localizedString.Replace("{Result}", _ParentStableQuestDetailedUI._StableQuestMainUI.GetTimerString(_ParentStableQuestDetailedUI.pCurrentMissionData.Duration * 60));
		FindItem("TxtDuration").SetText(localizedString);
		string localizedString2 = _MissionSuccessPercentageText.GetLocalizedString();
		localizedString2 = localizedString2.Replace("{Result}", TimedMissionManager.pInstance.GetWinProbability(_ParentStableQuestDetailedUI.pCurrentMissionData, _ParentStableQuestDetailedUI.pLocalPetIDs).ToString());
		FindItem("TxtSuccessPercentage").SetText(localizedString2);
	}

	public void OnImageLoaded(ImageDataInstance img)
	{
		if (img.mIconTexture == null)
		{
			return;
		}
		foreach (KAWidget item in mMenu.GetItems())
		{
			StablesPetUserData stablesPetUserData = (StablesPetUserData)item.GetUserData();
			if (stablesPetUserData != null && (stablesPetUserData.pData.ImagePosition.HasValue ? stablesPetUserData.pData.ImagePosition.Value : 0) == img.mSlotIndex)
			{
				KAWidget kAWidget = item.FindChildItem("DragonIco");
				kAWidget.SetTexture(img.mIconTexture);
				kAWidget.SetVisibility(inVisible: true);
				item.FindChildItem("TxtDragonName").SetText(stablesPetUserData.pData.Name);
				break;
			}
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name.Contains("Yes"))
		{
			for (int i = 0; i < _ParentStableQuestDetailedUI.pLocalPetIDs.Count; i++)
			{
				RaisedPetData byID = RaisedPetData.GetByID(_ParentStableQuestDetailedUI.pLocalPetIDs[i]);
				RaisedPetState raisedPetState = byID.FindStateData(SanctuaryPetMeterType.ENERGY.ToString());
				if (raisedPetState != null)
				{
					raisedPetState.Value -= _ParentStableQuestDetailedUI.pCurrentMissionData.DragonEnergyCost;
					float maxMeter = SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.ENERGY, byID);
					raisedPetState.Value = Mathf.Clamp(raisedPetState.Value, 0f, maxMeter);
					byID.SetStateData(SanctuaryPetMeterType.ENERGY.ToString(), raisedPetState.Value);
					byID.SaveDataReal(null, null, savePetMeterAlone: true);
				}
			}
			TimedMissionManager.pInstance.StartMission(_ParentStableQuestDetailedUI.pCurrentSlotData.SlotID, _ParentStableQuestDetailedUI.pLocalPetIDs);
			SetVisibility(inVisible: false);
		}
		else if (inWidget.name.Contains("No"))
		{
			SetVisibility(inVisible: false);
		}
	}
}
