public class UiStableQuestDragonsMenu : KAUIMenu
{
	private UiStableQuestDragonSelect mDragonSelectUI;

	protected override void Awake()
	{
		base.Awake();
		mDragonSelectUI = (UiStableQuestDragonSelect)_ParentUi;
	}

	public void LoadDragonsList()
	{
		ClearItems();
		int num = -1;
		if (SanctuaryManager.pCurPetInstance != null)
		{
			num = SanctuaryManager.pCurPetInstance.pData.RaisedPetID;
		}
		if (RaisedPetData.pActivePets == null)
		{
			return;
		}
		foreach (RaisedPetData[] value in RaisedPetData.pActivePets.Values)
		{
			if (value == null)
			{
				continue;
			}
			RaisedPetData[] array = value;
			foreach (RaisedPetData raisedPetData in array)
			{
				if (StableData.GetByPetID(raisedPetData.RaisedPetID) != null && raisedPetData.pStage >= RaisedPetStage.BABY && raisedPetData.IsPetCustomized() && !TimedMissionManager.pInstance.IsPetEngaged(raisedPetData.RaisedPetID) && TimedMissionManager.pInstance.IsPetValid(mDragonSelectUI._ParentStableQuestDetailedUI.pCurrentMissionData, raisedPetData.RaisedPetID) && raisedPetData.RaisedPetID != num)
				{
					CreateDragonWiget(raisedPetData);
				}
			}
		}
		foreach (RaisedPetData[] value2 in RaisedPetData.pActivePets.Values)
		{
			if (value2 == null)
			{
				continue;
			}
			RaisedPetData[] array = value2;
			foreach (RaisedPetData raisedPetData2 in array)
			{
				if (StableData.GetByPetID(raisedPetData2.RaisedPetID) != null && raisedPetData2.pStage >= RaisedPetStage.BABY && raisedPetData2.IsPetCustomized() && !TimedMissionManager.pInstance.IsPetEngaged(raisedPetData2.RaisedPetID) && (!TimedMissionManager.pInstance.IsPetValid(mDragonSelectUI._ParentStableQuestDetailedUI.pCurrentMissionData, raisedPetData2.RaisedPetID) || raisedPetData2.RaisedPetID == num))
				{
					CreateDragonWiget(raisedPetData2);
				}
			}
		}
		foreach (RaisedPetData[] value3 in RaisedPetData.pActivePets.Values)
		{
			if (value3 == null)
			{
				continue;
			}
			RaisedPetData[] array = value3;
			foreach (RaisedPetData raisedPetData3 in array)
			{
				if (StableData.GetByPetID(raisedPetData3.RaisedPetID) != null && raisedPetData3.pStage >= RaisedPetStage.BABY && raisedPetData3.IsPetCustomized() && TimedMissionManager.pInstance.IsPetEngaged(raisedPetData3.RaisedPetID))
				{
					CreateDragonWiget(raisedPetData3);
				}
			}
		}
		mCurrentGrid.repositionNow = true;
	}

	private void CreateDragonWiget(RaisedPetData rpData)
	{
		KAWidget kAWidget = AddWidget(_Template.name, null);
		kAWidget.SetVisibility(inVisible: true);
		kAWidget.name = rpData.Name;
		kAWidget.FindChildItem("TxtDragonName").SetText(rpData.Name);
		StablesPetUserData userData = new StablesPetUserData(rpData);
		kAWidget.SetUserData(userData);
		RefreshItem(kAWidget);
		if (!TimedMissionManager.pInstance.IsPetEngaged(rpData.RaisedPetID) && TimedMissionManager.pInstance.IsPetValid(mDragonSelectUI._ParentStableQuestDetailedUI.pCurrentMissionData, rpData.RaisedPetID))
		{
			foreach (BonusFactor dragonFactor in mDragonSelectUI._ParentStableQuestDetailedUI.pCurrentSlotData.pMission.DragonFactors)
			{
				if (dragonFactor.Type != FactorType.SecondaryType)
				{
					continue;
				}
				KAWidget kAWidget2 = kAWidget.FindChildItem("SecondaryTypeIco");
				if (kAWidget2 != null)
				{
					SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(rpData.PetTypeID);
					if (sanctuaryPetTypeInfo != null && sanctuaryPetTypeInfo.pSecondaryType != null && !string.IsNullOrEmpty(sanctuaryPetTypeInfo.pSecondaryType._IconSprite))
					{
						kAWidget2.GetComponentInChildren<UISlicedSprite>().UpdateSprite(sanctuaryPetTypeInfo.pSecondaryType._IconSprite);
						kAWidget2.SetVisibility(inVisible: true);
					}
					else
					{
						kAWidget2.SetVisibility(inVisible: false);
					}
				}
			}
		}
		int slotIdx = (rpData.ImagePosition.HasValue ? rpData.ImagePosition.Value : 0);
		ImageData.Load("EggColor", slotIdx, base.gameObject);
	}

	private bool IsPetSelected(int petID)
	{
		foreach (int pLocalPetID in mDragonSelectUI._ParentStableQuestDetailedUI.pLocalPetIDs)
		{
			if (pLocalPetID == petID)
			{
				return true;
			}
		}
		return false;
	}

	private void RefreshItem(KAWidget widgetRefresh)
	{
		if (widgetRefresh != null)
		{
			StablesPetUserData stablesPetUserData = (StablesPetUserData)widgetRefresh.GetUserData();
			KAToggleButton kAToggleButton = (KAToggleButton)widgetRefresh.FindChildItem("BtnSelectToggle");
			kAToggleButton.SetVisibility(inVisible: true);
			KAWidget kAWidget = widgetRefresh.FindChildItem("ActiveIcon");
			kAWidget.SetVisibility(inVisible: false);
			KAWidget kAWidget2 = widgetRefresh.FindChildItem("InvalidIcon");
			kAWidget2.SetVisibility(inVisible: false);
			KAWidget kAWidget3 = widgetRefresh.FindChildItem("BusyIcon");
			kAWidget3.SetVisibility(inVisible: false);
			KAWidget kAWidget4 = widgetRefresh.FindChildItem("LockIcon");
			kAWidget4.SetVisibility(inVisible: false);
			kAToggleButton.SetChecked(IsPetSelected(stablesPetUserData.pData.RaisedPetID));
			if (stablesPetUserData.pData.RaisedPetID == SanctuaryManager.pCurPetData.RaisedPetID)
			{
				kAToggleButton.SetVisibility(inVisible: false);
				kAWidget.SetVisibility(inVisible: true);
			}
			else if (!TimedMissionManager.pInstance.IsPetValid(mDragonSelectUI._ParentStableQuestDetailedUI.pCurrentMissionData, stablesPetUserData.pData.RaisedPetID))
			{
				kAToggleButton.SetVisibility(inVisible: false);
				kAWidget2.SetVisibility(inVisible: true);
			}
			else if (SanctuaryManager.IsPetLocked(stablesPetUserData.pData))
			{
				kAToggleButton.SetVisibility(inVisible: false);
				kAWidget4.SetVisibility(inVisible: true);
			}
			else if (TimedMissionManager.pInstance.IsPetEngaged(stablesPetUserData.pData.RaisedPetID))
			{
				kAToggleButton.SetVisibility(inVisible: false);
				kAWidget3.SetVisibility(inVisible: true);
			}
		}
	}

	public void RefreshMenu(int petID = -1)
	{
		foreach (KAWidget item in GetItems())
		{
			StablesPetUserData stablesPetUserData = (StablesPetUserData)item.GetUserData();
			if (stablesPetUserData != null)
			{
				if (petID == -1)
				{
					RefreshItem(item);
				}
				else if (petID == stablesPetUserData.pData.RaisedPetID)
				{
					RefreshItem(item);
					break;
				}
			}
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		StablesPetUserData stablesPetUserData = (StablesPetUserData)inWidget.GetUserData();
		if (inWidget.name.Contains("BtnSelectToggle"))
		{
			stablesPetUserData = (StablesPetUserData)inWidget.GetParentItem().GetUserData();
			KAToggleButton kAToggleButton = (KAToggleButton)inWidget;
			int raisedPetID = stablesPetUserData.pData.RaisedPetID;
			if (!IsPetSelected(raisedPetID))
			{
				if (mDragonSelectUI._ParentStableQuestDetailedUI.pLocalPetIDs.Count < (int)mDragonSelectUI._ParentStableQuestDetailedUI.pCurrentMissionData.PetCount.Max)
				{
					mDragonSelectUI._ParentStableQuestDetailedUI.pLocalPetIDs.Add(raisedPetID);
				}
				else
				{
					kAToggleButton.SetChecked(isChecked: false);
				}
			}
			else
			{
				mDragonSelectUI._ParentStableQuestDetailedUI.pLocalPetIDs.Remove(raisedPetID);
			}
			mDragonSelectUI._ParentStableQuestDetailedUI.RefreshWinProbabilityBar();
		}
		if (stablesPetUserData == null)
		{
			return;
		}
		if (stablesPetUserData.pData.RaisedPetID == SanctuaryManager.pCurPetData.RaisedPetID)
		{
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", mDragonSelectUI._ActiveDragonSelectedText.GetLocalizedString(), base.gameObject, "");
		}
		else if (TimedMissionManager.pInstance.IsPetValid(mDragonSelectUI._ParentStableQuestDetailedUI.pCurrentMissionData, stablesPetUserData.pData.RaisedPetID) && SanctuaryManager.IsPetLocked(stablesPetUserData.pData))
		{
			string localizedString = mDragonSelectUI._NonMemberText.GetLocalizedString();
			localizedString = localizedString.Replace("{{Dragon}}", stablesPetUserData.pData.Name);
			if (SubscriptionInfo.pIsMember && !IAPManager.IsMembershipUpgradeable())
			{
				localizedString = localizedString + "\n\n" + IAPManager.GetMembershipUpgradeText();
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", localizedString, null, base.gameObject, null, null, "OnDBClose", null, inDestroyOnClick: true);
			}
			else
			{
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", localizedString, null, base.gameObject, "ShowMembershipPurchaseDB", "OnDBClose", null, null, inDestroyOnClick: true);
			}
		}
		mDragonSelectUI.SetSelectedPetUserData(stablesPetUserData);
	}

	private void ShowMembershipPurchaseDB()
	{
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.MEMBERSHIP, base.gameObject);
	}

	private void OnIAPStoreClosed()
	{
		RefreshMenu();
	}

	private void OnDBClose()
	{
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
			if ((stablesPetUserData.pData.ImagePosition.HasValue ? stablesPetUserData.pData.ImagePosition.Value : 0) == img.mSlotIndex)
			{
				item.FindChildItem("DragonIco").SetTexture(img.mIconTexture);
				break;
			}
		}
	}
}
