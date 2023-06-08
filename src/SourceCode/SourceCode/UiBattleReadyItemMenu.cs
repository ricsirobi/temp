using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiBattleReadyItemMenu : KAUISelectAvatarMenu
{
	private UiBattleItemStorage mUiBattleItemStorage;

	private UiStorageContentMenu mStorageMenu;

	private UiStatCompareDB mUiStatisticsDB;

	private bool mShowStats;

	private UiMyRoomBuilder pMyRoomBuilder => MyRoomsIntMain.pInstance._UiMyRoomsInt._MyRoomBuilder;

	public override void Initialize(KAUI parentInt)
	{
		base.Initialize(parentInt);
		mUiBattleItemStorage = parentInt as UiBattleItemStorage;
		mStorageMenu = (UiStorageContentMenu)mUiBattleItemStorage._MenuList[2];
		mUiStatisticsDB = (UiStatCompareDB)mUiBattleItemStorage._UiList[2];
		mUiBattleItemStorage.pItemDefaultColor = UiItemRarityColorSet.GetItemBackgroundColor(_Template, mUiBattleItemStorage._ItemColorWidget);
	}

	public override void ChangeCategory(string pType, bool forceChange)
	{
		if (pType == "All")
		{
			int[] allCategories = mUiBattleItemStorage.GetAllCategories();
			ChangeCategory(allCategories, forceChange);
		}
		else
		{
			int categoryID = AvatarData.GetCategoryID(pType);
			ChangeCategory(new int[1] { categoryID }, forceChange);
		}
	}

	public override void FinishMenuItems(bool addParentItems = false)
	{
		mUiBattleItemStorage._BattleReadyTab.mNumSlotUnlocked = mUiBattleItemStorage._BattleReadyTab.pTabData.GetTotalSlots();
		base.FinishMenuItems(addParentItems);
		base.pKAUISelect.ShowAvailableEmptySlots(mUiBattleItemStorage._BattleReadyTab);
	}

	public override void UpdateWidget(KAUISelectItemData id)
	{
		if (id._UserItemData != null && base.pEquippedItems.Contains(id._UserItemData.UserInventoryID))
		{
			id._Disabled = true;
		}
		base.UpdateWidget(id);
		bool show = id._UserItemData != null && id._UserItemData.pIsBattleReady;
		mUiBattleItemStorage.ShowBattleReadyIcon(id.GetItem(), show);
		mUiBattleItemStorage.ShowFlightReadyIcon(id.GetItem(), id._ItemData != null && id._ItemData.HasAttribute("FlightSuit"));
		mUiBattleItemStorage.UpdateWidgetBackground(id.GetItem(), id._ItemData, id._UserItemData != null && id._UserItemData.pIsBattleReady);
	}

	public override void ChangeCategory(int[] categories, bool forceChange)
	{
		_LoadItemIdList = null;
		base.ChangeCategory(categories, forceChange);
	}

	public override void AddInvMenuItem(ItemData item, int quantity)
	{
		if (item.ItemStatsMap != null && IsValidItem(item))
		{
			base.AddInvMenuItem(item, quantity);
			InventoryTab battleReadyTab = mUiBattleItemStorage._BattleReadyTab;
			if (battleReadyTab != null && battleReadyTab.pTabData != null && battleReadyTab.pTabData.ValidateItem(item))
			{
				battleReadyTab.mNumSlotOccupied++;
			}
		}
	}

	public override void AddInvMenuItem(UserItemData item, int quantity)
	{
		if (item.pIsBattleReady && IsValidItem(item.Item) && !base.pEquippedItems.Contains(item.UserInventoryID))
		{
			base.AddInvMenuItem(item, quantity);
			InventoryTab battleReadyTab = mUiBattleItemStorage._BattleReadyTab;
			if (battleReadyTab != null && battleReadyTab.pTabData != null && battleReadyTab.pTabData.ValidateItem(item))
			{
				battleReadyTab.mNumSlotOccupied++;
			}
		}
	}

	private bool IsValidItem(ItemData item)
	{
		bool result = true;
		if (mUiBattleItemStorage.pCatToExclude != null)
		{
			foreach (int item2 in mUiBattleItemStorage.pCatToExclude)
			{
				result = !item.HasCategory(item2);
			}
		}
		return result;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (!mShowStats)
		{
			StartCoroutine("CheckAndShowStatsDB", inWidget);
		}
	}

	public override void OnDoubleClick(KAWidget inWidget)
	{
		base.OnDoubleClick(inWidget);
		GameObject hoveredObject = null;
		KAUISelectItemData kAUISelectItemData = null;
		UiRackMenu uiRackMenu = (UiRackMenu)mUiBattleItemStorage._UiList[0];
		if ((bool)uiRackMenu && uiRackMenu.GetVisibility())
		{
			for (int i = 0; i <= uiRackMenu.pRackSlots.Length - 1; i++)
			{
				if ((bool)uiRackMenu.pRackSlots[i])
				{
					kAUISelectItemData = (KAUISelectItemData)uiRackMenu.pRackSlots[i].GetUserData();
					if (kAUISelectItemData != null && kAUISelectItemData._ItemData == null && uiRackMenu.pRackSlots[i].IsActive())
					{
						hoveredObject = uiRackMenu.pRackSlots[i].gameObject;
						break;
					}
				}
			}
		}
		else if ((bool)mStorageMenu)
		{
			hoveredObject = mStorageMenu.FindEmptySlotInStorageMenu().gameObject;
		}
		UICamera.hoveredObject = hoveredObject;
		KAUIManager.pInstance.pDragItem = inWidget;
		EndDrag(inWidget);
		mShowStats = false;
	}

	public override void OnDragStart(KAWidget inWidget)
	{
		if (!_AllowItemDrag)
		{
			return;
		}
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)inWidget.GetUserData();
		if (kAUISelectItemData == null || kAUISelectItemData.pEquippedItem || kAUISelectItemData._Disabled || kAUISelectItemData._SlotLocked || kAUISelectItemData == null || kAUISelectItemData._ItemData == null || kAUISelectItemData._ItemData.ItemID == 0 || (!(KAUIManager.pInstance.pDragItem == null) && KAUIManager.pInstance.pDragItem.pAttachToCursor))
		{
			return;
		}
		KAWidget kAWidget = CreateDragObject(inWidget, base.pPanel.depth + 1);
		if (_DisableDragWidgetList != null)
		{
			foreach (string disableDragWidget in _DisableDragWidgetList)
			{
				KAWidget kAWidget2 = kAWidget.FindChildItem(disableDragWidget);
				if (kAWidget2 != null)
				{
					kAWidget2.SetVisibility(inVisible: false);
				}
			}
		}
		kAUISelectItemData.ShowLoadingItem(inShow: false);
	}

	private void EndDrag(KAWidget inWidget)
	{
		GameObject hoveredObject = UICamera.hoveredObject;
		if (!(hoveredObject != null))
		{
			return;
		}
		if (mUiBattleItemStorage._IsWeaponRack)
		{
			if (ManageRackDragData(hoveredObject, inWidget))
			{
				mUiBattleItemStorage._BattleReadyTab.mNumSlotOccupied--;
				mUiBattleItemStorage.UpdateSlotsInfo();
			}
		}
		else
		{
			ManageChestDragData(hoveredObject, inWidget);
		}
	}

	public override void OnDragEnd(KAWidget sourceWidget)
	{
		EndDrag(sourceWidget);
		if ((bool)KAUIManager.pInstance.pDragItem)
		{
			Object.Destroy(KAUIManager.pInstance.pDragItem.gameObject);
		}
		MoveCursorItemBack(sourceWidget);
	}

	private bool ManageRackDragData(GameObject hoverObj, KAWidget sourceWidget)
	{
		KAWidget component = hoverObj.GetComponent<KAWidget>();
		if (component != null)
		{
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)component.GetUserData();
			if (kAUISelectItemData._Disabled)
			{
				return false;
			}
			if (kAUISelectItemData._Menu == null || (kAUISelectItemData._Menu != null && kAUISelectItemData._Menu != this))
			{
				ProcessDropOnRack(component, sourceWidget);
				MoveItemsToBottom(sourceWidget);
				return true;
			}
		}
		return false;
	}

	private bool ManageChestDragData(GameObject hoverObj, KAWidget sourceWidget)
	{
		KAWidget component = hoverObj.GetComponent<KAWidget>();
		bool flag = false;
		if (component != null)
		{
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)component.GetUserData();
			if (kAUISelectItemData._Disabled)
			{
				return false;
			}
			if (kAUISelectItemData != null && kAUISelectItemData._Menu != null && kAUISelectItemData._Menu == mStorageMenu)
			{
				flag = true;
				ProcessDropOnChest(component, sourceWidget);
				MoveItemsToBottom(sourceWidget);
				mStorageMenu.UpdateSlotsInfo();
				return true;
			}
		}
		if (!flag)
		{
			UIDragScrollView component2 = hoverObj.GetComponent<UIDragScrollView>();
			if (component2 != null && component2.scrollView.GetComponent<UiStorageContentMenu>() == mStorageMenu)
			{
				flag = true;
				ProcessDropOnChest(mUiBattleItemStorage.FindEmptySlotInStorageMenu(), sourceWidget);
				MoveItemsToBottom(sourceWidget);
				mUiBattleItemStorage.UpdateSlotsInfo();
				mStorageMenu.UpdateSlotsInfo();
				return true;
			}
		}
		return false;
	}

	public void MoveItemsToBottom(KAWidget inWidget)
	{
		int num = 0;
		bool flag = false;
		List<KAWidget> list = new List<KAWidget>();
		KAWidget kAWidget = null;
		int index = 0;
		for (int num2 = mItemInfo.Count - 1; num2 >= 0; num2--)
		{
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)mItemInfo[num2].GetUserData();
			if (kAUISelectItemData != null && !kAUISelectItemData._Locked && !kAUISelectItemData._SlotLocked && !flag)
			{
				num = num2;
				flag = true;
			}
			if (kAUISelectItemData == null || kAUISelectItemData._ItemData.ItemID == 0)
			{
				list.Add(mItemInfo[num2]);
			}
			if (kAUISelectItemData._ItemData.ItemID == 0 && kAUISelectItemData._SlotLocked)
			{
				kAWidget = mItemInfo[num2];
				index = list.Count - 1;
			}
		}
		mItemInfo.Insert(num + 1, inWidget);
		mItemInfo.Remove(inWidget);
		if (kAWidget != null)
		{
			list.RemoveAt(index);
			list.Add(kAWidget);
		}
		for (int i = 0; i < list.Count; i++)
		{
			mItemInfo.Add(list[i]);
			mItemInfo.Remove(list[i]);
		}
		RepositionMenu();
	}

	protected override void MoveCursorItemBack(KAWidget sourceWidget)
	{
		DestroyDragItem();
	}

	private void ProcessDropOnRack(KAWidget inTargetWidget, KAWidget sourceWidget)
	{
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)KAUIManager.pInstance.pDragItem.GetUserData();
		if (kAUISelectItemData != null && kAUISelectItemData._ItemData != null && inTargetWidget != null)
		{
			KAUISelectItemData kAUISelectItemData2 = (KAUISelectItemData)inTargetWidget.GetUserData();
			if (kAUISelectItemData2._ItemData != null)
			{
				KAUISelectItemData kAUISelectItemData3 = new KAUISelectItemData(kAUISelectItemData._Menu, kAUISelectItemData2._UserItemData, _WHSize, 1);
				WeaponRackRoomItem component = pMyRoomBuilder.pSelectedObject.GetComponent<WeaponRackRoomItem>();
				RemoveItemFromRack(component.GetStorageItem(inTargetWidget));
				SetDroppedSlotDataOnRack(inTargetWidget, kAUISelectItemData);
				sourceWidget.SetUserData(kAUISelectItemData3);
				UpdateWidget(kAUISelectItemData3);
			}
			else
			{
				SetDroppedSlotDataOnRack(inTargetWidget, kAUISelectItemData);
				mUiBattleItemStorage.AddWidgetData(sourceWidget, null);
				sourceWidget.SetInteractive(isInteractive: true);
				UpdateWidget((KAUISelectItemData)sourceWidget.GetUserData());
			}
		}
		KAUIManager.pInstance.pDragItem.DetachFromCursor();
	}

	private void ProcessDropOnChest(KAWidget inTargetWidget, KAWidget sourceWidget)
	{
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)KAUIManager.pInstance.pDragItem.GetUserData();
		if (kAUISelectItemData != null && kAUISelectItemData._ItemData != null && inTargetWidget != null)
		{
			KAUISelectItemData kAUISelectItemData2 = (KAUISelectItemData)inTargetWidget.GetUserData();
			if (kAUISelectItemData2._ItemData != null && kAUISelectItemData2._ItemData.ItemID != 0)
			{
				KAUISelectItemData kAUISelectItemData3 = new KAUISelectItemData(kAUISelectItemData._Menu, kAUISelectItemData2._UserItemData, _WHSize, 1);
				WeaponChestRoomItem component = pMyRoomBuilder.pSelectedObject.GetComponent<WeaponChestRoomItem>();
				RemoveItemFromChest(component.GetStorageItem(inTargetWidget));
				SetDroppedSlotDataOnChest(inTargetWidget, kAUISelectItemData);
				sourceWidget.SetUserData(kAUISelectItemData3);
				UpdateWidget(kAUISelectItemData3);
			}
			else
			{
				SetDroppedSlotDataOnChest(inTargetWidget, kAUISelectItemData);
				mUiBattleItemStorage.AddWidgetData(sourceWidget, null);
				UpdateWidget((KAUISelectItemData)sourceWidget.GetUserData());
			}
		}
		KAUIManager.pInstance.pDragItem.DetachFromCursor();
	}

	private void SetDroppedSlotDataOnRack(KAWidget slot, KAUISelectItemData draggedWidgetData)
	{
		KAUISelectItemData kAUISelectItemData = new KAUISelectItemData();
		kAUISelectItemData._UserItemData = draggedWidgetData._UserItemData;
		kAUISelectItemData._ItemData = kAUISelectItemData._UserItemData.Item;
		slot.SetUserData(kAUISelectItemData);
		slot.SetTextureFromBundle(kAUISelectItemData._UserItemData.Item.IconName);
		UpdateWidget(kAUISelectItemData);
		EquipItemsOnRack(slot);
	}

	private void SetDroppedSlotDataOnChest(KAWidget slot, KAUISelectItemData draggedWidgetData)
	{
		KAUISelectItemData kAUISelectItemData = new KAUISelectItemData(mStorageMenu, draggedWidgetData._UserItemData, mStorageMenu._WHSize, 1);
		slot.SetUserData(kAUISelectItemData);
		UpdateWidget(kAUISelectItemData);
		mStorageMenu.CreateChestItem(slot);
	}

	private void EquipItemsOnRack(KAWidget slot)
	{
		KAUISelectItemData obj = (KAUISelectItemData)slot.GetUserData();
		mUiBattleItemStorage.SetState(KAUIState.NOT_INTERACTIVE);
		KAUICursorManager.SetDefaultCursor("Loading");
		string[] array = obj._ItemData.AssetName.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnRackItemLoaded, typeof(GameObject), inDontDestroy: false, slot);
	}

	private void OnRackItemLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAWidget kAWidget = (KAWidget)inUserData;
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)kAWidget.GetUserData();
			WeaponRackRoomItem component = pMyRoomBuilder.pSelectedObject.GetComponent<WeaponRackRoomItem>();
			WeaponRackRoomItem.SlotInfo selectedSlotInfo = component.GetSelectedSlotInfo(kAWidget.name);
			GameObject gameObject = Object.Instantiate((GameObject)inObject);
			gameObject.name = kAUISelectItemData._ItemData.AssetName;
			gameObject.transform.parent = selectedSlotInfo._Marker;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			MyRoomObject myRoomObject = gameObject.GetComponent<MyRoomObject>();
			if (myRoomObject == null)
			{
				myRoomObject = gameObject.AddComponent<MyRoomObject>();
			}
			myRoomObject.pUserItemData = kAUISelectItemData._UserItemData;
			CommonInventoryData.pInstance.RemoveItem(kAUISelectItemData._UserItemData, 1);
			mUiBattleItemStorage.UpdateSlotsInfo();
			component.AddToEquppedList(myRoomObject);
			MyRoomsIntMain.pInstance.AddRoomObject(gameObject, kAUISelectItemData._UserItemData, component.gameObject, isUpdateLocalList: false);
			mUiBattleItemStorage.SetState(KAUIState.INTERACTIVE);
			RsResourceManager.ReleaseBundleData(inURL);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Failed to load Avatar Equipment....");
			break;
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
	}

	private void RemoveItemFromRack(MyRoomObject rackItem)
	{
		if (!(rackItem == null))
		{
			pMyRoomBuilder.pSelectedObject.GetComponent<WeaponRackRoomItem>().RemoveFromEquippedList(rackItem);
			rackItem.pUserItemData.Quantity = 1;
			CommonInventoryData.pInstance.AddUserItem(rackItem.pUserItemData);
			MyRoomsIntMain.pInstance.RemoveRoomObject(rackItem.gameObject, isDestroy: true);
		}
	}

	private void RemoveItemFromChest(MyRoomObject chestItem)
	{
		if (!(chestItem == null))
		{
			pMyRoomBuilder.pSelectedObject.GetComponent<WeaponChestRoomItem>().RemoveFromEquippedList(chestItem);
			chestItem.pUserItemData.Quantity = 1;
			CommonInventoryData.pInstance.AddUserItem(chestItem.pUserItemData);
			MyRoomsIntMain.pInstance.RemoveRoomObject(chestItem.gameObject, isDestroy: true);
		}
	}

	public IEnumerator CheckAndShowStatsDB(KAWidget inWidget)
	{
		mShowStats = true;
		float waitTime = 0.4f;
		float deltaTime = 0f;
		while (deltaTime < waitTime && (UtPlatform.IsTouchInput() || UICamera.hoveredObject == inWidget.gameObject))
		{
			deltaTime += Time.deltaTime;
			yield return null;
		}
		if (mShowStats)
		{
			OpenStatsDB(inWidget);
			mShowStats = false;
		}
	}

	private void OpenStatsDB(KAWidget inWidget)
	{
		mUiStatisticsDB.OpenStatsDB(inWidget, mUiStatisticsDB._EquippedSlots, base.gameObject);
	}

	private ItemStat[] GetItemStatsFromUserItemData(UserItemData userItemData)
	{
		return userItemData?.ItemStats;
	}

	private ItemTier? GetItemTierFromUserItemData(UserItemData userItemData)
	{
		return userItemData?.ItemTier;
	}
}
