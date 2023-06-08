using UnityEngine;

public class UiDragonSelection : KAUI
{
	public bool _AllowFlightlessDragons;

	private UiDragonSelectionMenu mDragonSelectionMenu;

	[HideInInspector]
	public UiDragonsInfoCardItem SelectedDragonInfoCardItem;

	protected override void Start()
	{
		base.Start();
		mDragonSelectionMenu = (UiDragonSelectionMenu)GetMenu("UiDragonSelectionMenu");
	}

	public void SetDragonSelectionMenu()
	{
		mDragonSelectionMenu.ClearItems();
		if (RaisedPetData.pActivePets != null)
		{
			int num = 0;
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
					if (raisedPetData.pStage >= RaisedPetStage.BABY && (_AllowFlightlessDragons || sanctuaryPetTypeInfo == null || !sanctuaryPetTypeInfo._Flightless))
					{
						UiDragonsInfoCardItem uiDragonsInfoCardItem = (UiDragonsInfoCardItem)mDragonSelectionMenu.DuplicateWidget(mDragonSelectionMenu._Template);
						uiDragonsInfoCardItem.name = raisedPetData.Name;
						uiDragonsInfoCardItem.SetMessageObject(base.gameObject);
						uiDragonsInfoCardItem.SetVisibility(inVisible: true);
						uiDragonsInfoCardItem.pSelectedPetID = raisedPetData.RaisedPetID;
						uiDragonsInfoCardItem.RefreshUI();
						uiDragonsInfoCardItem.SetButtons(selectBtn: false, visitBtn: false, moveInBtn: false);
						if (SanctuaryManager.pCurPetData.RaisedPetID == raisedPetData.RaisedPetID)
						{
							mDragonSelectionMenu.AddWidgetAt(0, uiDragonsInfoCardItem);
						}
						else
						{
							mDragonSelectionMenu.AddWidgetAt(num, uiDragonsInfoCardItem);
						}
						num++;
					}
				}
			}
		}
		SelectedDragonInfoCardItem = (UiDragonsInfoCardItem)mDragonSelectionMenu.GetSelectedItem();
		SetVisibility(inVisible: true);
	}
}
