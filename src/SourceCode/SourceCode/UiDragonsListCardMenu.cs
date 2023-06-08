using System;
using UnityEngine;

public class UiDragonsListCardMenu : KAUIMenu
{
	public class StablesPetUserData : KAWidgetUserData
	{
		public RaisedPetData pData;

		public StablesPetUserData(RaisedPetData data)
		{
			pData = data;
		}
	}

	private int mSelectedPetID = -1;

	public LocaleString _StableQuestAlertText = new LocaleString("Your Dragon is busy. Would you like to go to stable quest and manage dragons?");

	public UiDragonsListCard _UiDragonsListCard;

	protected GameObject mMsgObject;

	public Action OpenDragonListOnStableQuestsClose;

	public void SetMessageObject(GameObject msgObject)
	{
		mMsgObject = msgObject;
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (string.Equals(inWidget.name, "AgeUpIcon"))
		{
			base.OnClick(inWidget);
			inWidget = inWidget.transform.parent.GetComponent<KAButton>();
		}
		base.OnClick(inWidget);
		StablesPetUserData stablesPetUserData = (StablesPetUserData)inWidget.GetUserData();
		if (stablesPetUserData != null)
		{
			mSelectedPetID = stablesPetUserData.pData.RaisedPetID;
			if (TimedMissionManager.pInstance != null && TimedMissionManager.pInstance.IsPetEngaged(stablesPetUserData.pData.RaisedPetID))
			{
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _StableQuestAlertText.GetLocalizedString(), null, base.gameObject, "LoadStableQuest", "OnDBClose", null, null, inDestroyOnClick: true);
			}
			if (_UiDragonsListCard != null)
			{
				_UiDragonsListCard.SelectDragon(mSelectedPetID);
			}
		}
	}

	private void LoadStableQuest()
	{
		StableData byPetID = StableData.GetByPetID(mSelectedPetID);
		if (byPetID == null)
		{
			return;
		}
		if ((bool)StableManager.pInstance)
		{
			UiDragonsStable uiDragonsStable = (UiDragonsStable)_UiDragonsListCard._UiCardParent;
			if ((bool)uiDragonsStable)
			{
				StableManager.pInstance.OpenStableQuest();
				uiDragonsStable.Exit();
			}
		}
		else
		{
			StableManager.LoadStableWithJobBoard(byPetID.ID);
		}
	}

	public void ResetSelection()
	{
		KAWidget selectedItem = GetSelectedItem();
		SetSelectedItem(null);
		ResetHighlightWidgets(selectedItem);
	}

	public void LoadDragonsList()
	{
		ClearItems();
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
				if ((!(SanctuaryManager.pCurPetInstance == null) && SanctuaryManager.pCurPetInstance.pData != null && raisedPetData.RaisedPetID == SanctuaryManager.pCurPetInstance.pData.RaisedPetID) || raisedPetData.pStage < RaisedPetStage.BABY || !raisedPetData.IsPetCustomized())
				{
					continue;
				}
				if (_UiDragonsListCard.pCurrentMode == UiDragonsListCard.Mode.NestedDragons || _UiDragonsListCard.pCurrentMode == UiDragonsListCard.Mode.ForceDragonSelection)
				{
					if (StableData.GetByPetID(raisedPetData.RaisedPetID) == null)
					{
						continue;
					}
				}
				else if (_UiDragonsListCard.pCurrentMode == UiDragonsListCard.Mode.CurrentStableDragons)
				{
					StableData byPetID = StableData.GetByPetID(raisedPetData.RaisedPetID);
					if (byPetID == null || byPetID.ID != StableManager.pCurrentStableID)
					{
						continue;
					}
				}
				KAWidget kAWidget = AddWidget(_Template.name, null);
				kAWidget.SetVisibility(inVisible: true);
				kAWidget.name = raisedPetData.Name;
				kAWidget.FindChildItem("TxtDragonName").SetText(raisedPetData.Name);
				SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(raisedPetData.PetTypeID);
				int ageIndex = RaisedPetData.GetAgeIndex(raisedPetData.pStage);
				KAWidget kAWidget2 = kAWidget.FindChildItem("TxtAge");
				if (kAWidget2 != null)
				{
					kAWidget2.SetText(SanctuaryData.GetDisplayTextFromPetAge(raisedPetData.pStage) + " " + sanctuaryPetTypeInfo._NameText.GetLocalizedString());
				}
				KAWidget kAWidget3 = kAWidget.FindChildItem("LockedIcon");
				if (kAWidget3 != null)
				{
					kAWidget3.SetVisibility(SanctuaryManager.IsPetLocked(raisedPetData));
				}
				StablesPetUserData userData = new StablesPetUserData(raisedPetData);
				kAWidget.SetUserData(userData);
				int slotIdx = (raisedPetData.ImagePosition.HasValue ? raisedPetData.ImagePosition.Value : 0);
				ImageData.Load("EggColor", slotIdx, base.gameObject);
				if (TimedMissionManager.pInstance != null && TimedMissionManager.pInstance.IsPetEngaged(raisedPetData.RaisedPetID))
				{
					KAWidget kAWidget4 = kAWidget.FindChildItem("BusyIcon");
					if (kAWidget4 != null)
					{
						kAWidget4.SetVisibility(inVisible: true);
					}
				}
				else if (ageIndex < sanctuaryPetTypeInfo._AgeData.Length - 1)
				{
					KAWidget kAWidget5 = kAWidget.FindChildItem("AgeUpIcon");
					if (kAWidget5 != null)
					{
						kAWidget5.SetVisibility(inVisible: true);
					}
				}
			}
		}
		mCurrentGrid.repositionNow = true;
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
