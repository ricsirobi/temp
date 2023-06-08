using System.Collections.Generic;
using UnityEngine;

public class UiLobbyBase : KAUI
{
	public bool _AllowFlightlessDragons;

	protected UiDragonSelectionMenu mDragonSelectionMenu;

	public virtual void OnDragonSelectionMenuScroll(UIScrollBar scroll = null)
	{
	}

	public virtual void OnSwipe(Vector2 dir)
	{
	}

	protected override void Start()
	{
		base.Start();
		mDragonSelectionMenu = (UiDragonSelectionMenu)GetMenu("UiDragonSelectionMenu");
	}

	protected virtual List<RaisedPetData> GetPetDataList()
	{
		List<RaisedPetData> list = new List<RaisedPetData>();
		foreach (RaisedPetData[] value in RaisedPetData.pActivePets.Values)
		{
			if (value == null)
			{
				continue;
			}
			RaisedPetData[] array = value;
			foreach (RaisedPetData raisedPetData in array)
			{
				SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(raisedPetData.PetTypeID);
				if ((int)raisedPetData.pStage >= sanctuaryPetTypeInfo._MinAgeToFly && RaisedPetData.GetAgeIndex(raisedPetData.pStage) >= sanctuaryPetTypeInfo._MinAgeToMount && (_AllowFlightlessDragons || sanctuaryPetTypeInfo == null || !sanctuaryPetTypeInfo._Flightless) && !TimedMissionManager.pInstance.IsPetEngaged(raisedPetData.RaisedPetID))
				{
					list.Add(raisedPetData);
				}
			}
		}
		return list;
	}

	protected virtual int GetActivePetShowIndex()
	{
		return 0;
	}

	protected void SetDragonSelectionMenu()
	{
		if (mDragonSelectionMenu == null)
		{
			return;
		}
		mDragonSelectionMenu.ClearItems();
		List<RaisedPetData> petDataList = GetPetDataList();
		if (petDataList == null)
		{
			return;
		}
		int num = 0;
		UiDragonsInfoCardItem uiDragonsInfoCardItem = null;
		foreach (RaisedPetData item in petDataList)
		{
			UiDragonsInfoCardItem uiDragonsInfoCardItem2 = (UiDragonsInfoCardItem)mDragonSelectionMenu.DuplicateWidget(mDragonSelectionMenu._Template);
			uiDragonsInfoCardItem2.name = item.Name;
			uiDragonsInfoCardItem2.SetMessageObject(base.gameObject);
			uiDragonsInfoCardItem2.SetVisibility(inVisible: true);
			uiDragonsInfoCardItem2.pSelectedPetID = item.RaisedPetID;
			uiDragonsInfoCardItem2.RefreshUI();
			uiDragonsInfoCardItem2.SetButtons(selectBtn: false, visitBtn: false, moveInBtn: false);
			if (SanctuaryManager.pCurPetData.RaisedPetID == item.RaisedPetID)
			{
				uiDragonsInfoCardItem = uiDragonsInfoCardItem2;
				continue;
			}
			mDragonSelectionMenu.AddWidgetAt(num, uiDragonsInfoCardItem2);
			num++;
		}
		if (uiDragonsInfoCardItem != null)
		{
			mDragonSelectionMenu.AddWidgetAt(GetActivePetShowIndex(), uiDragonsInfoCardItem);
		}
	}

	protected void LoadSelectedDragon(KAWidget selectedWidget)
	{
		UiDragonsInfoCardItem obj = (UiDragonsInfoCardItem)selectedWidget;
		SetInteractive(interactive: false);
		obj.MakeActiveDragon();
	}
}
