using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiStorageContentMenu : KAUISelectMenu
{
	public string _ItemColorWidget = "CellBkg";

	public GameObject _StorageMenuContainner;

	public LocaleString _PurchaseFailText = new LocaleString("[Review] Transaction failed. Please try again.");

	public LocaleString _PurchaseConfirmationText = new LocaleString("[REVIEW] Are you sure you want to buy more slots?");

	public LocaleString _PurchaseProcessingText = new LocaleString("[Review] Processing purchase...");

	public LocaleString _NotEnoughCurrencyText = new LocaleString("[Review] You don't have enough Gems. Do you want to buy more Gems?");

	[SerializeField]
	private GameObject m_DragView;

	[SerializeField]
	private KAWidget m_SlotsInfoTxt;

	[SerializeField]
	private KAWidget m_MenuTitle;

	private UiBattleItemStorage mUiBattleItemStorage;

	private UiStatCompareDB mUiStatisticsDB;

	private UiBattleReadyItemMenu mItemMenu;

	private ItemPurchase mItemPurchase;

	private bool mShowStats;

	private int mTotalSlots;

	private int mSlotsUnlocked;

	private Color mItemDefaultColor = Color.white;

	private int pSlotsOccupied => pMyRoomBuilder.pSelectedObject.GetComponent<WeaponChestRoomItem>().pEquippedItems.Count;

	private UiMyRoomBuilder pMyRoomBuilder => MyRoomsIntMain.pInstance._UiMyRoomsInt._MyRoomBuilder;

	public override void Initialize(KAUI parentInt)
	{
		base.Initialize(parentInt);
		m_DragView.SetActive(value: true);
		mUiBattleItemStorage = parentInt as UiBattleItemStorage;
		mItemMenu = (UiBattleReadyItemMenu)mUiBattleItemStorage._MenuList[1];
		mUiStatisticsDB = (UiStatCompareDB)mUiBattleItemStorage._UiList[2];
		mItemDefaultColor = UiItemRarityColorSet.GetItemBackgroundColor(_Template, _ItemColorWidget);
		SetChestItemSlots();
		mUiBattleItemStorage.pItemDefaultColor = UiItemRarityColorSet.GetItemBackgroundColor(_Template, mUiBattleItemStorage._ItemColorWidget);
		if (m_MenuTitle != null)
		{
			m_MenuTitle.SetVisibility(inVisible: true);
		}
	}

	private bool SetUpgradeInfo()
	{
		_ = pMyRoomBuilder.pSelectedObject.GetComponent<MyRoomObject>().pUserItemData.UserItemAttributes;
		WeaponChestRoomItem.Upgrade upgrade = pMyRoomBuilder.pSelectedObject.GetComponent<WeaponChestRoomItem>().GetUpgrade();
		if (upgrade != null)
		{
			if (mItemPurchase == null)
			{
				mItemPurchase = new ItemPurchase();
			}
			mItemPurchase.SetMessages(null, _PurchaseFailText, _PurchaseConfirmationText, _PurchaseProcessingText, _NotEnoughCurrencyText);
			mItemPurchase.Init(upgrade.GetStoreID(), upgrade.GetItemID(), OnSlotPurchase, ItemPurchaseSource.STORAGE_CHEST_UPGRADE.ToString(), "Slot_Item");
			mItemPurchase.LoadItemInfo();
			return true;
		}
		return false;
	}

	public void SetChestItemSlots()
	{
		PairData userItemAttributes = pMyRoomBuilder.pSelectedObject.GetComponent<MyRoomObject>().pUserItemData.UserItemAttributes;
		WeaponChestRoomItem component = pMyRoomBuilder.pSelectedObject.GetComponent<WeaponChestRoomItem>();
		int intValue = userItemAttributes.GetIntValue("StorageItemSlotCount", 0);
		mTotalSlots = component._DefaultSlotCount + intValue;
		mSlotsUnlocked = 0;
		for (int i = 0; i < mTotalSlots; i++)
		{
			if (i < component.pEquippedItems.Count)
			{
				AddInvMenuItem(component.pEquippedItems[i].pUserItemData);
			}
		}
		ShowAvailableEmptySlots();
		UpdateSlotsInfo();
	}

	public void ShowAvailableEmptySlots()
	{
		WeaponChestRoomItem component = pMyRoomBuilder.pSelectedObject.GetComponent<WeaponChestRoomItem>();
		int num = (component.IsUpgradeAvailable() ? (mTotalSlots + 1) : mTotalSlots);
		if (num == 0 || pSlotsOccupied >= num)
		{
			return;
		}
		for (int i = pSlotsOccupied; i < num; i++)
		{
			KAWidget kAWidget = AddEmptySlot();
			if (kAWidget != null && i > mTotalSlots - 1)
			{
				if (component.IsUpgradeAvailable())
				{
					((KAUISelectItemData)kAWidget.GetUserData()).ShowSlotLock(kAWidget, addIcon: true);
					break;
				}
				((KAUISelectItemData)kAWidget.GetUserData()).ShowSlotLock(kAWidget);
			}
		}
	}

	public KAWidget AddEmptySlot()
	{
		KAWidget kAWidget = AddWidget("EmptySlot");
		if (kAWidget != null)
		{
			AddWidget(kAWidget, (KAUISelectItemData)null);
		}
		return kAWidget;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (!mShowStats)
		{
			StartCoroutine("CheckAndShowStatsDB", inWidget);
		}
	}

	public override void SelectItem(KAWidget inWidget)
	{
		if (inWidget != null)
		{
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)inWidget.GetUserData();
			if (kAUISelectItemData != null && kAUISelectItemData._SlotLocked)
			{
				SetSelectedItem(null);
				BuySlot();
			}
		}
		UnSelectItem();
		mCurSelectedItem = inWidget;
		if (_RemoveWhenSelected)
		{
			mCursorItem = RemoveOneItem(inWidget);
			base.pViewChanged = true;
		}
		else
		{
			mCursorItem = inWidget;
		}
		if (_UseIconCursor)
		{
			mMainUI.AddWidget(mCursorItem);
			mCursorItem.SetState(KAUIState.NOT_INTERACTIVE);
			mCursorItem.AttachToCursor(0f, 0f);
		}
	}

	private KAWidget RemoveOneItem(KAWidget item)
	{
		RemoveWidget(item);
		return item;
	}

	private void BuySlot()
	{
		if (SetUpgradeInfo())
		{
			mItemPurchase.ProcessPurchase();
		}
	}

	public override void AddInvMenuItem(ItemData item, int quantity = 1)
	{
		base.AddInvMenuItem(item, quantity);
	}

	public override void AddInvMenuItem(UserItemData userItem, int quantity = 1)
	{
		base.AddInvMenuItem(userItem, quantity);
	}

	private void AddWidget(KAWidget widget, KAUISelectItemData widgetData)
	{
		if (widgetData != null)
		{
			AddWidget(widget, widgetData._UserItemData);
			return;
		}
		KAUISelectItemData userData = new KAUISelectItemData(this, new ItemData(), 0, 0, InventoryTabType.ITEM);
		widget.SetInteractive(isInteractive: false);
		widget.SetTexture(null);
		widget.SetToolTipText("");
		widget.SetUserData(userData);
		widget.name = "EmptySlot";
	}

	private void AddWidget(KAWidget widget, UserItemData userItemData)
	{
		KAUISelectItemData kAUISelectItemData = new KAUISelectItemData(this, userItemData, _WHSize, 1);
		widget.SetUserData(kAUISelectItemData);
		if (!string.IsNullOrEmpty(userItemData.Item.ItemName))
		{
			string itemName = userItemData.Item.ItemName;
			widget.SetToolTipText(itemName);
		}
		widget.FindChildItem("AddIcon").SetVisibility(inVisible: false);
		UpdateWidget(kAUISelectItemData);
	}

	public override void UpdateWidget(KAUISelectItemData id)
	{
		base.UpdateWidget(id);
		bool show = id._UserItemData != null && id._UserItemData.pIsBattleReady;
		mUiBattleItemStorage.ShowBattleReadyIcon(id.GetItem(), show);
		mUiBattleItemStorage.ShowFlightReadyIcon(id.GetItem(), id._ItemData != null && id._ItemData.HasAttribute("FlightSuit"));
		mUiBattleItemStorage.UpdateWidgetBackground(id.GetItem(), id._ItemData, id._UserItemData != null && id._UserItemData.pIsBattleReady);
	}

	public override void OnDragStart(KAWidget inWidget)
	{
		if (!_AllowItemDrag)
		{
			return;
		}
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)inWidget.GetUserData();
		if (kAUISelectItemData == null || kAUISelectItemData.pEquippedItem || kAUISelectItemData == null || kAUISelectItemData._ItemData == null || kAUISelectItemData._ItemData.ItemID == 0 || (!(KAUIManager.pInstance.pDragItem == null) && KAUIManager.pInstance.pDragItem.pAttachToCursor))
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

	public override void OnDragEnd(KAWidget sourceWidget)
	{
		GameObject hoveredObject = UICamera.hoveredObject;
		bool flag = false;
		if (hoveredObject != null)
		{
			KAWidget component = hoveredObject.GetComponent<KAWidget>();
			if (component != null)
			{
				KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)component.GetUserData();
				if (kAUISelectItemData != null && kAUISelectItemData._Menu != null && kAUISelectItemData._Menu == mItemMenu && !kAUISelectItemData._Disabled)
				{
					flag = true;
					ProcessDrop(component, sourceWidget);
				}
			}
			if (!flag)
			{
				UIDragScrollView component2 = hoveredObject.GetComponent<UIDragScrollView>();
				if (component2 != null && component2.scrollView.GetComponent<UiBattleReadyItemMenu>() == mItemMenu)
				{
					flag = true;
					ProcessDrop(mUiBattleItemStorage.FindEmptySlotInItemMenu(), sourceWidget);
				}
			}
		}
		MoveCursorItemBack(sourceWidget);
	}

	public override void OnDoubleClick(KAWidget inWidget)
	{
		base.OnDoubleClick(inWidget);
		KAUIManager.pInstance.pDragItem = inWidget;
		ProcessDrop(mUiBattleItemStorage.FindEmptySlotInItemMenu(), inWidget);
	}

	public KAWidget FindEmptySlotInStorageMenu()
	{
		KAWidget result = null;
		foreach (KAWidget item in GetItems())
		{
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)item.GetUserData();
			if (kAUISelectItemData._ItemData.ItemID == 0 && !kAUISelectItemData._SlotLocked)
			{
				result = item;
				break;
			}
		}
		return result;
	}

	private void ProcessDrop(KAWidget inTargetWidget, KAWidget sourceWidget)
	{
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)KAUIManager.pInstance.pDragItem.GetUserData();
		if (kAUISelectItemData != null && kAUISelectItemData._ItemData != null && inTargetWidget != null)
		{
			KAUISelectItemData kAUISelectItemData2 = (KAUISelectItemData)inTargetWidget.GetUserData();
			WeaponChestRoomItem component = pMyRoomBuilder.pSelectedObject.GetComponent<WeaponChestRoomItem>();
			if (kAUISelectItemData2._ItemData != null && kAUISelectItemData2._ItemData.ItemID != 0)
			{
				UserItemData userItemData = kAUISelectItemData2._UserItemData;
				RemoveItemFromChest(component.GetStorageItem(sourceWidget));
				SetDroppedSlotData(inTargetWidget, kAUISelectItemData);
				AddWidget(sourceWidget, userItemData);
				CreateChestItem(sourceWidget);
			}
			else
			{
				SetDroppedSlotData(inTargetWidget, kAUISelectItemData);
				RemoveItemFromChest(component.GetStorageItem(sourceWidget));
				AddWidget(sourceWidget, (KAUISelectItemData)null);
				UpdateWidget((KAUISelectItemData)sourceWidget.GetUserData());
				mUiBattleItemStorage.UpdateSlotsInfo();
			}
		}
		UpdateSlotsInfo();
	}

	private void SetDroppedSlotData(KAWidget slot, KAUISelectItemData draggedWidgetData)
	{
		KAUISelectItemData kAUISelectItemData = new KAUISelectItemData(mItemMenu, draggedWidgetData._UserItemData, mItemMenu._WHSize, 1);
		slot.SetUserData(kAUISelectItemData);
		UpdateWidget(kAUISelectItemData);
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

	public void CreateChestItem(KAWidget slot)
	{
		KAUISelectItemData obj = (KAUISelectItemData)slot.GetUserData();
		mUiBattleItemStorage.SetState(KAUIState.NOT_INTERACTIVE);
		KAUICursorManager.SetDefaultCursor("Loading");
		string[] array = obj._ItemData.AssetName.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnChestItemLoaded, typeof(GameObject), inDontDestroy: false, slot);
	}

	private void OnChestItemLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)((KAWidget)inUserData).GetUserData();
			WeaponChestRoomItem component = pMyRoomBuilder.pSelectedObject.GetComponent<WeaponChestRoomItem>();
			GameObject gameObject = Object.Instantiate((GameObject)inObject);
			gameObject.name = kAUISelectItemData._ItemData.AssetName;
			gameObject.transform.parent = component.transform;
			gameObject.transform.position = component._Marker;
			gameObject.transform.localRotation = Quaternion.identity;
			MyRoomObject myRoomObject = gameObject.GetComponent<MyRoomObject>();
			if (myRoomObject == null)
			{
				myRoomObject = gameObject.AddComponent<MyRoomObject>();
			}
			myRoomObject.pUserItemData = kAUISelectItemData._UserItemData;
			component.AddToEquppedList(myRoomObject);
			CommonInventoryData.pInstance.RemoveItem(kAUISelectItemData._UserItemData, 1);
			mUiBattleItemStorage.UpdateSlotsInfo();
			MyRoomsIntMain.pInstance.AddRoomObject(gameObject, kAUISelectItemData._UserItemData, component.gameObject, isUpdateLocalList: false);
			KAUICursorManager.SetDefaultCursor("Arrow");
			mUiBattleItemStorage.SetState(KAUIState.INTERACTIVE);
			RsResourceManager.ReleaseBundleData(inURL);
			UpdateSlotsInfo();
			break;
		}
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Failed to load Avatar Equipment....");
			break;
		}
	}

	protected override void MoveCursorItemBack(KAWidget sourceWidget)
	{
		DestroyDragItem();
	}

	public void UpdateSlotsInfo()
	{
		if (m_SlotsInfoTxt != null)
		{
			int count = pMyRoomBuilder.pSelectedObject.GetComponent<WeaponChestRoomItem>().pEquippedItems.Count;
			int num = mTotalSlots;
			m_SlotsInfoTxt.SetText(count + "/" + num);
			m_SlotsInfoTxt.SetVisibility(inVisible: true);
		}
	}

	private void OnSlotPurchase(ItemPurchase.Status state)
	{
		if (state != ItemPurchase.Status.Success)
		{
			return;
		}
		WeaponChestRoomItem component = pMyRoomBuilder.pSelectedObject.GetComponent<WeaponChestRoomItem>();
		MyRoomObject component2 = pMyRoomBuilder.pSelectedObject.GetComponent<MyRoomObject>();
		PairData userItemAttributes = component2.pUserItemData.UserItemAttributes;
		WeaponChestRoomItem.Upgrade upgrade = component.GetUpgrade();
		UserItemData userItemData = CommonInventoryData.pInstance.FindItem(upgrade.GetItemID());
		if (userItemData != null)
		{
			int num = userItemAttributes.GetIntValue("ChestUpgradeCount", 0) + 1;
			mSlotsUnlocked = 0;
			if (userItemData.Item.HasAttribute("Slots"))
			{
				mSlotsUnlocked = userItemData.Item.GetAttribute("Slots", 0);
			}
			mTotalSlots += mSlotsUnlocked;
			component.UpgradeChest(upgrade.GetScale());
			userItemAttributes.SetValue("StorageItemSlotCount", (mSlotsUnlocked + userItemAttributes.GetIntValue("StorageItemSlotCount", 0)).ToString());
			userItemAttributes.SetValue("ChestUpgradeCount", num.ToString());
			userItemAttributes.PrepareArray();
			WsWebService.SetCommonInventoryAttribute(component2.pUserItemData.UserInventoryID, userItemAttributes, OnInventorySaveCallBack, null);
		}
	}

	public void OnInventorySaveCallBack(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			OnItemPurchaseComplete();
			break;
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
		mUiBattleItemStorage.SetState(KAUIState.INTERACTIVE);
	}

	private void OnItemPurchaseComplete()
	{
		RefreshSlotStatus();
		UpdateSlotsInfo();
	}

	private void RefreshSlotStatus()
	{
		if (GetItemCount() == 0)
		{
			return;
		}
		WeaponChestRoomItem component = pMyRoomBuilder.pSelectedObject.GetComponent<WeaponChestRoomItem>();
		int num = pSlotsOccupied;
		int num2 = mTotalSlots;
		int num3 = mSlotsUnlocked;
		if (num3 == 0 || num >= num2)
		{
			return;
		}
		if (num3 > 0)
		{
			KAWidget inWidget = GetItems()[GetItemCount() - 1];
			for (int i = 0; i < num3; i++)
			{
				AddEmptySlot();
			}
			MoveItemsToBottom(inWidget);
		}
		if (component.IsUpgradeAvailable())
		{
			KAWidget kAWidget = AddEmptySlot();
			((KAUISelectItemData)kAWidget.GetUserData()).ShowSlotLock(kAWidget, addIcon: true);
			return;
		}
		foreach (KAWidget item in GetItems())
		{
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)item.GetUserData();
			if (kAUISelectItemData._SlotLocked)
			{
				kAUISelectItemData.ShowSlotLock(item, addIcon: false, locked: false);
				item.SetVisibility(inVisible: false);
			}
		}
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
		mUiStatisticsDB.OpenStatsDB(inWidget, mUiStatisticsDB._EquippedSlots, base.gameObject, showDBRight: true);
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
