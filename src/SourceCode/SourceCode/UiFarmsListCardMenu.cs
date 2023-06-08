using System.Collections.Generic;

public class UiFarmsListCardMenu : KAUIMenu
{
	public class UserRoomWidgetData : KAWidgetUserData
	{
		public string _RoomID;

		public UserRoomWidgetData(string roomID)
		{
			_RoomID = roomID;
		}
	}

	public UiFarmsListCard _UiFarmsListCard;

	public KAWidget _StoreItem;

	public static OnFarmsListUILoad pOnFarmsListUILoaded;

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == _StoreItem.name)
		{
			_UiFarmsListCard.OpenStore();
		}
		else if (_UiFarmsListCard != null)
		{
			UserRoomWidgetData userRoomWidgetData = (UserRoomWidgetData)inWidget.GetUserData();
			_UiFarmsListCard.SelectRoom(userRoomWidgetData._RoomID);
		}
	}

	public void ResetSelection()
	{
		KAWidget selectedItem = GetSelectedItem();
		SetSelectedItem(null);
		ResetHighlightWidgets(selectedItem);
	}

	public void LoadFarmsList(List<UserRoom> userRoomList)
	{
		ClearItems();
		if (userRoomList != null)
		{
			for (int i = 0; i < userRoomList.Count; i++)
			{
				UserRoom userRoom = userRoomList[i];
				if (userRoom == null || (_UiFarmsListCard.pHeaderRoomData != null && userRoom.RoomID.Equals(_UiFarmsListCard.pHeaderRoomData.RoomID)))
				{
					continue;
				}
				KAWidget kAWidget = AddWidget(_Template.name);
				kAWidget.SetVisibility(inVisible: true);
				kAWidget.name = userRoom.pLocaleName;
				KAWidget kAWidget2 = kAWidget.FindChildItem("TxtRoomName");
				if (kAWidget2 != null)
				{
					kAWidget2.SetText(userRoom.pLocaleName);
				}
				KAWidget kAWidget3 = kAWidget.FindChildItem("RoomIco");
				if (kAWidget3 != null)
				{
					UISlicedSprite componentInChildren = kAWidget3.GetComponentInChildren<UISlicedSprite>();
					string farmIcon = _UiFarmsListCard.GetFarmIcon(userRoom.pItemID);
					if (!string.IsNullOrEmpty(farmIcon))
					{
						componentInChildren.UpdateSprite(farmIcon);
					}
				}
				kAWidget.SetUserData(new UserRoomWidgetData(userRoom.RoomID));
			}
		}
		if (_UiFarmsListCard.pIsUserFarm)
		{
			KAWidget kAWidget4 = DuplicateWidget(_StoreItem);
			AddWidget(kAWidget4);
			kAWidget4.SetVisibility(inVisible: true);
			kAWidget4.name = "StoreItem";
		}
		if (pOnFarmsListUILoaded != null)
		{
			pOnFarmsListUILoaded(isSuccess: true);
		}
	}
}
