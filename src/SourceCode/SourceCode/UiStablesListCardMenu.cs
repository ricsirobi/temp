public class UiStablesListCardMenu : KAUIMenu
{
	public UiStablesListCard _UiStablesListCard;

	public KAWidget _StoreItem;

	public static OnStablesListUILoad pOnStablesListUILoaded;

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == _StoreItem.name)
		{
			_UiStablesListCard.OpenStore();
		}
		else if (_UiStablesListCard != null)
		{
			int userDataInt = inWidget.GetUserDataInt();
			_UiStablesListCard.SelectRoom(userDataInt);
		}
	}

	public void ResetSelection()
	{
		KAWidget selectedItem = GetSelectedItem();
		SetSelectedItem(null);
		ResetHighlightWidgets(selectedItem);
	}

	public void LoadStablesList()
	{
		ClearItems();
		if (StableData.pStableList != null)
		{
			for (int i = 0; i < StableData.pStableList.Count; i++)
			{
				StableData stableData = StableData.pStableList[i];
				if (stableData == null || _UiStablesListCard.pHeaderStableData == null || stableData.ID == _UiStablesListCard.pHeaderStableData.ID)
				{
					continue;
				}
				KAWidget kAWidget = AddWidget(_Template.name);
				kAWidget.SetVisibility(inVisible: true);
				kAWidget.name = stableData.pLocaleName;
				KAWidget kAWidget2 = kAWidget.FindChildItem("TxtRoomName");
				if (kAWidget2 != null)
				{
					kAWidget2.SetText(stableData.pLocaleName);
				}
				KAWidget kAWidget3 = kAWidget.FindChildItem("RoomIco");
				if (kAWidget3 != null)
				{
					UISlicedSprite componentInChildren = kAWidget3.GetComponentInChildren<UISlicedSprite>();
					string stableIcon = _UiStablesListCard.GetStableIcon(stableData.ItemID);
					if (!string.IsNullOrEmpty(stableIcon))
					{
						componentInChildren.UpdateSprite(stableIcon);
					}
				}
				kAWidget.SetUserDataInt(stableData.ID);
			}
		}
		KAWidget kAWidget4 = DuplicateWidget(_StoreItem);
		AddWidget(kAWidget4);
		kAWidget4.SetVisibility(inVisible: true);
		kAWidget4.name = "StoreItem";
		if (pOnStablesListUILoaded != null)
		{
			pOnStablesListUILoaded(isSuccess: true);
		}
	}
}
