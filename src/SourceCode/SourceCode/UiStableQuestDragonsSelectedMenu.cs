using System.Collections.Generic;

public class UiStableQuestDragonsSelectedMenu : KAUIMenu
{
	private UiStableQuestDetail mUiStableQuestDetail;

	private TimedMissionSlotData mCurrentSlotData;

	public LocaleString _LevelText = new LocaleString("Level - ");

	protected override void Start()
	{
		base.Start();
		mUiStableQuestDetail = (UiStableQuestDetail)_ParentUi;
	}

	public void Init(TimedMissionSlotData SlotData)
	{
		mCurrentSlotData = SlotData;
		InitMenu();
	}

	private void InitMenu()
	{
		ClearItems();
		for (int i = 0; i < (int)mCurrentSlotData.pMission.PetCount.Max; i++)
		{
			KAWidget kAWidget = DuplicateWidget(_Template);
			kAWidget.SetVisibility(inVisible: true);
			AddWidget(kAWidget);
			if (i > mUiStableQuestDetail.pLocalPetIDs.Count - 1)
			{
				RefreshPetWidget(kAWidget, -1);
			}
			else
			{
				RefreshPetWidget(kAWidget, mUiStableQuestDetail.pLocalPetIDs[i]);
			}
		}
	}

	private void ShowPetBackground(KAWidget menuItem, bool empty, bool special)
	{
		menuItem.FindChildItem("BkgEmpty").SetVisibility(empty);
		menuItem.FindChildItem("BkgNormal").SetVisibility(!empty && !special);
		menuItem.FindChildItem("BkgSpecial").SetVisibility(!empty && special);
		menuItem.FindChildItem("IcoDragon").SetVisibility(!empty);
	}

	private void RefreshPetWidget(KAWidget petWidget, int petID)
	{
		if (petID >= 0)
		{
			RaisedPetData byID = RaisedPetData.GetByID(petID);
			StablesPetUserData stablesPetUserData = new StablesPetUserData(byID);
			petWidget.SetUserData(stablesPetUserData);
			if (stablesPetUserData != null)
			{
				bool flag = false;
				if (TimedMissionManager.pInstance.GetWinProbabilityForPet(mCurrentSlotData.pMission, byID.RaisedPetID) > (float)mCurrentSlotData.pMission.WinFactorPerDragon)
				{
					flag = true;
				}
				KAWidget kAWidget = petWidget.FindChildItem("TxtDragonName");
				if (kAWidget != null)
				{
					kAWidget.SetText(byID.Name);
				}
				KAWidget kAWidget2 = petWidget.FindChildItem("TxtDragonExp");
				if (kAWidget != null)
				{
					int rankID = PetRankData.GetUserRank(byID).RankID;
					kAWidget2.SetText(_LevelText.GetLocalizedString() + rankID);
				}
				KAWidget kAWidget3 = petWidget.FindChildItem("SecondaryTypeIco");
				if (kAWidget3 != null && flag)
				{
					SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(byID.PetTypeID);
					if (sanctuaryPetTypeInfo != null && sanctuaryPetTypeInfo.pSecondaryType != null && !string.IsNullOrEmpty(sanctuaryPetTypeInfo.pSecondaryType._IconSprite))
					{
						kAWidget3.pBackground.UpdateSprite(sanctuaryPetTypeInfo.pSecondaryType._IconSprite);
						kAWidget3.SetVisibility(inVisible: true);
					}
					else
					{
						kAWidget3.SetVisibility(inVisible: false);
					}
				}
				ShowPetBackground(petWidget, empty: false, flag);
				int slotIdx = 0;
				if (byID.ImagePosition.HasValue)
				{
					slotIdx = byID.ImagePosition.Value;
				}
				ImageData.Load("EggColor", slotIdx, base.gameObject);
			}
			else
			{
				ShowPetBackground(petWidget, empty: true, special: false);
			}
		}
		else
		{
			ShowPetBackground(petWidget, empty: true, special: false);
		}
	}

	private void RefreshMenu()
	{
		List<KAWidget> items = GetItems();
		for (int i = 0; i < items.Count; i++)
		{
			KAWidget petWidget = items[i];
			if (i < mUiStableQuestDetail.pLocalPetIDs.Count)
			{
				RefreshPetWidget(petWidget, mUiStableQuestDetail.pLocalPetIDs[i]);
			}
			else
			{
				RefreshPetWidget(petWidget, -1);
			}
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (mCurrentSlotData.State != TimedMissionState.Started)
		{
			mUiStableQuestDetail.SetDragonSelectionMode(enable: true);
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (inVisible)
		{
			RefreshMenu();
		}
	}

	public void OnImageLoaded(ImageDataInstance img)
	{
		if (img.mIconTexture == null)
		{
			return;
		}
		foreach (KAWidget item in GetItems())
		{
			StablesPetUserData stablesPetUserData = (StablesPetUserData)item.GetUserData();
			if (stablesPetUserData != null && (stablesPetUserData.pData.ImagePosition.HasValue ? stablesPetUserData.pData.ImagePosition.Value : 0) == img.mSlotIndex)
			{
				KAWidget kAWidget = item.FindChildItem("IcoDragon");
				kAWidget.SetTexture(img.mIconTexture);
				kAWidget.SetVisibility(inVisible: true);
				break;
			}
		}
	}
}
