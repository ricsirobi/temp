using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiRackMenu : KAUI
{
	public string _ItemColorWidget = "CellBkg";

	public int _WHSize = 80;

	[Header("Drag Drop")]
	public bool _AllowItemDrag;

	public List<string> _DisableDragWidgetList;

	[SerializeField]
	private KAWidget[] m_RackLayouts;

	private UiBattleReadyItemMenu mItemMenu;

	private UiBattleItemStorage mUiBattleItemStorage;

	private KAWidget mSelectedLayout;

	private KAWidget[] mRackSlots;

	private Color mItemDefaultColor = Color.white;

	private bool mShowStats;

	private UiStatCompareDB mUiStatisticsDB;

	protected KAUIDraggablePanel mDragPanel;

	protected UIPanel mPanel;

	public KAWidget[] pRackSlots
	{
		get
		{
			return mRackSlots;
		}
		set
		{
			mRackSlots = value;
		}
	}

	public KAUIDraggablePanel pDragPanel
	{
		get
		{
			if (mDragPanel == null)
			{
				mDragPanel = GetComponentInChildren<KAUIDraggablePanel>();
			}
			return mDragPanel;
		}
	}

	public UIPanel pPanel
	{
		get
		{
			if (mPanel == null)
			{
				mPanel = GetComponentInChildren<UIPanel>();
			}
			return mPanel;
		}
	}

	private UiMyRoomBuilder pMyRoomBuilder => MyRoomsIntMain.pInstance._UiMyRoomsInt._MyRoomBuilder;

	public void Initialize(int slotCount, UiBattleItemStorage uiBattleItemStorage)
	{
		mSelectedLayout = m_RackLayouts[slotCount - 1];
		mUiBattleItemStorage = uiBattleItemStorage;
		mItemMenu = (UiBattleReadyItemMenu)uiBattleItemStorage._MenuList[1];
		mUiStatisticsDB = (UiStatCompareDB)uiBattleItemStorage._UiList[2];
		mSelectedLayout.SetVisibility(inVisible: true);
		AddRackSlotData();
		FindItem("TxtName").SetVisibility(inVisible: true);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (!mShowStats)
		{
			StartCoroutine("CheckAndShowStatsDB", inWidget);
		}
	}

	private void AddRackSlotData()
	{
		mRackSlots = new KAWidget[mSelectedLayout.GetNumChildren()];
		for (int i = 0; i <= mRackSlots.Length - 1; i++)
		{
			if (mRackSlots[i] == null)
			{
				mRackSlots[i] = mSelectedLayout.FindChildItemAt(i);
			}
		}
		WeaponRackRoomItem component = pMyRoomBuilder.pSelectedObject.GetComponent<WeaponRackRoomItem>();
		if (component.pEquippedItems != null)
		{
			foreach (MyRoomObject pEquippedItem in component.pEquippedItems)
			{
				KAWidget rackSlot = GetRackSlot(GetNearestSlot(pEquippedItem)._SlotName);
				if (!(rackSlot == null))
				{
					AddWidget(rackSlot, pEquippedItem.pUserItemData);
				}
			}
		}
		foreach (WeaponRackRoomItem.SlotInfo item in component._SlotInfo)
		{
			KAWidget rackSlot2 = GetRackSlot(item._SlotName);
			if (!(rackSlot2 == null) && rackSlot2.GetUserData() == null)
			{
				AddWidget(rackSlot2, (KAUISelectItemData)null);
			}
		}
	}

	private WeaponRackRoomItem.SlotInfo GetNearestSlot(MyRoomObject roomObject)
	{
		WeaponRackRoomItem component = pMyRoomBuilder.pSelectedObject.GetComponent<WeaponRackRoomItem>();
		Vector3 vector = roomObject.transform.position - component.transform.position;
		List<float> list = new List<float>();
		foreach (WeaponRackRoomItem.SlotInfo item2 in component._SlotInfo)
		{
			float item = Vector3.Dot((item2._Marker.position - component.transform.position).normalized, vector.normalized);
			list.Add(item);
		}
		int index = 0;
		float num = list[0];
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] > num)
			{
				index = i;
				num = list[i];
			}
		}
		return component._SlotInfo[index];
	}

	private void AddWidget(KAWidget widget, KAUISelectItemData widgetData)
	{
		if (widgetData != null)
		{
			AddWidget(widget, widgetData._UserItemData);
			return;
		}
		widgetData = new KAUISelectItemData();
		widget.SetInteractive(isInteractive: true);
		widget.SetTexture(null);
		widget.SetToolTipText("");
		widget.SetUserData(widgetData);
	}

	private void AddWidget(KAWidget widget, UserItemData userItemData)
	{
		KAUISelectItemData kAUISelectItemData = new KAUISelectItemData();
		kAUISelectItemData._UserItemData = userItemData;
		kAUISelectItemData._ItemData = userItemData.Item;
		widget.SetUserData(kAUISelectItemData);
		widget.SetTextureFromBundle(kAUISelectItemData._UserItemData.Item.IconName);
		if (!string.IsNullOrEmpty(userItemData.Item.ItemName))
		{
			string itemName = userItemData.Item.ItemName;
			widget.SetToolTipText(itemName);
		}
		widget.FindChildItem("AddIcon").SetVisibility(inVisible: false);
		mItemMenu.UpdateWidget(kAUISelectItemData);
	}

	private KAWidget GetRackSlot(string slotName)
	{
		KAWidget kAWidget = Array.Find(mRackSlots, (KAWidget t) => t.gameObject.name == slotName);
		if (kAWidget != null)
		{
			return kAWidget;
		}
		return null;
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
		KAWidget kAWidget = CreateDragObject(inWidget, pPanel.depth + 1);
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

	public override void OnDoubleClick(KAWidget inWidget)
	{
		base.OnDoubleClick(inWidget);
		KAUIManager.pInstance.pDragItem = inWidget;
		ProcessDrop(mUiBattleItemStorage.FindEmptySlotInItemMenu(), inWidget);
	}

	private void EndDrag(KAWidget inWidget)
	{
		GameObject hoveredObject = UICamera.hoveredObject;
		if (!(hoveredObject != null))
		{
			return;
		}
		KAWidget component = hoveredObject.GetComponent<KAWidget>();
		bool flag = false;
		if (component != null)
		{
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)component.GetUserData();
			if (kAUISelectItemData != null && kAUISelectItemData._Menu != null && kAUISelectItemData._Menu == mItemMenu && !kAUISelectItemData._Disabled)
			{
				flag = true;
				ProcessDrop(component, inWidget);
			}
		}
		if (!flag)
		{
			UIDragScrollView component2 = hoveredObject.GetComponent<UIDragScrollView>();
			if (component2 != null && component2.scrollView.GetComponent<UiBattleReadyItemMenu>() == mItemMenu)
			{
				flag = true;
				ProcessDrop(mUiBattleItemStorage.FindEmptySlotInItemMenu(), inWidget);
			}
		}
	}

	public override void OnDragEnd(KAWidget inWidget)
	{
		EndDrag(inWidget);
		DestroyDragItem();
	}

	private void ProcessDrop(KAWidget inTargetWidget, KAWidget sourceWidget)
	{
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)KAUIManager.pInstance.pDragItem.GetUserData();
		if (kAUISelectItemData != null && kAUISelectItemData._ItemData != null && inTargetWidget != null)
		{
			KAUISelectItemData kAUISelectItemData2 = (KAUISelectItemData)inTargetWidget.GetUserData();
			WeaponRackRoomItem component = pMyRoomBuilder.pSelectedObject.GetComponent<WeaponRackRoomItem>();
			if (kAUISelectItemData2._ItemData != null && kAUISelectItemData2._ItemData.ItemID != 0)
			{
				UserItemData userItemData = kAUISelectItemData2._UserItemData;
				RemoveItemFromRack(component.GetStorageItem(sourceWidget));
				SetDroppedSlotData(inTargetWidget, kAUISelectItemData);
				AddWidget(sourceWidget, userItemData);
				EquipItemsOnRack(sourceWidget);
			}
			else
			{
				SetDroppedSlotData(inTargetWidget, kAUISelectItemData);
				RemoveItemFromRack(component.GetStorageItem(sourceWidget));
				AddWidget(sourceWidget, (KAUISelectItemData)null);
				mItemMenu.UpdateWidget((KAUISelectItemData)sourceWidget.GetUserData());
				mUiBattleItemStorage.UpdateSlotsInfo();
			}
			mUiBattleItemStorage._BattleReadyTab.mNumSlotOccupied++;
			mUiBattleItemStorage.UpdateSlotsInfo();
		}
	}

	private void SetDroppedSlotData(KAWidget slot, KAUISelectItemData draggedWidgetData)
	{
		KAUISelectItemData kAUISelectItemData = new KAUISelectItemData(mItemMenu, draggedWidgetData._UserItemData, mItemMenu._WHSize, 1);
		slot.SetUserData(kAUISelectItemData);
		mItemMenu.UpdateWidget(kAUISelectItemData);
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
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
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
			component.AddToEquppedList(myRoomObject);
			MyRoomsIntMain.pInstance.AddRoomObject(gameObject, kAUISelectItemData._UserItemData, component.gameObject, isUpdateLocalList: false);
			CommonInventoryData.pInstance.RemoveItem(kAUISelectItemData._UserItemData, 1);
			mUiBattleItemStorage.UpdateSlotsInfo();
			KAUICursorManager.SetDefaultCursor("Arrow");
			mUiBattleItemStorage.SetState(KAUIState.INTERACTIVE);
			RsResourceManager.ReleaseBundleData(inURL);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Failed to load Avatar Equipment....");
			break;
		}
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

	private void DestroyDragItem()
	{
		if (KAUIManager.pInstance.pDragItem != null)
		{
			KAUIManager.pInstance.pDragItem.DetachFromCursor();
			UnityEngine.Object.Destroy(KAUIManager.pInstance.pDragItem.gameObject);
		}
	}
}
