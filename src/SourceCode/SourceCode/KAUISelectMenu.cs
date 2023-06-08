using System;
using System.Collections.Generic;
using UnityEngine;

public class KAUISelectMenu : KAUIMenu
{
	public class DBItem
	{
		public ItemData _Item;

		public UserItemData _UserItem;

		public int _Quantity = 1;

		public string GetItemPartType(ItemData item)
		{
			ItemDataCategory[] category = item.Category;
			if (category != null && category.Length != 0)
			{
				ItemDataCategory[] array = category;
				for (int i = 0; i < array.Length; i++)
				{
					string partType = AvatarData.GetPartType(array[i].CategoryId);
					if (partType.Length > 0)
					{
						return partType;
					}
				}
			}
			UtDebug.LogError(" Item is not one of the avatar part type");
			return "";
		}

		public DBItem(ItemData item, int quantity)
		{
			_Item = item;
			_Quantity = quantity;
		}

		public DBItem(UserItemData userItem, int quantity)
		{
			_Item = userItem.Item;
			_Quantity = quantity;
			_UserItem = userItem;
		}
	}

	public string _LockedIconName = "LockedIcon";

	public string _AddIconName = "AddIcon";

	public string _DisabledWidgetName = "GreyMask";

	public AudioClip _LockedVO;

	public string _QuantityWidgetName;

	public UIFont _QuanFont;

	public int[] _StoreIDs;

	public UIFont _ToolTipFont;

	public bool _DefaultFirstItem;

	public AudioClip _IntroVO;

	public AudioClip[] _LongIntroVO;

	public bool _RemoveWhenSelected;

	public bool _MultiInstance;

	public bool _UseIconCursor;

	[Header("Drag Drop")]
	public bool _AllowItemDrag;

	public List<string> _DisableDragWidgetList;

	public int _LockIconOffX = 25;

	public int _LockIconOffY = 25;

	[NonSerialized]
	public bool mItemInitialized;

	[NonSerialized]
	public KAUI mMainUI;

	public int _WHSize = 80;

	[NonSerialized]
	public KAWidget mCurSelectedItem;

	public int[] _LoadItemIdList;

	public LocaleString _LockedItemText;

	public LocaleString _TaskItemText = new LocaleString("[REVIEW] You cannot do that, yet!");

	public Color _MaskColor = Color.clear;

	[NonSerialized]
	public bool mMenuPopulated;

	protected KAWidget mCursorItem;

	protected bool mFirstItemProcessed;

	protected bool mNeedPageCheck = true;

	protected int mCurStoreIdx;

	protected bool mStoreLoading;

	protected int mNumItemToBeLoaded;

	protected bool mFinalized;

	protected KAUIGenericDB mKAUIGenericDB;

	private KAUISelect mKAUISelect;

	private List<int> mEquippedItems;

	protected int[] mCategoryIDs;

	protected List<DBItem> mDBItemList = new List<DBItem>();

	public List<int> pEquippedItems
	{
		get
		{
			if (mEquippedItems == null && AvatarData.pInstanceInfo != null)
			{
				mEquippedItems = AvatarData.pInstanceInfo.GetPartsInventoryIds();
			}
			return mEquippedItems;
		}
	}

	public KAUISelect pKAUISelect => mKAUISelect;

	public int pCategoryID
	{
		get
		{
			if (mCategoryIDs == null)
			{
				return 0;
			}
			return mCategoryIDs[0];
		}
		set
		{
			if (mCategoryIDs == null)
			{
				mCategoryIDs = new int[1];
			}
			mCategoryIDs[0] = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Initialize(_ParentUi);
	}

	public virtual void Initialize(KAUI parentInt)
	{
		mMainUI = parentInt;
		mKAUISelect = parentInt as KAUISelect;
		if (AvatarData.pInstanceInfo != null)
		{
			mEquippedItems = AvatarData.pInstanceInfo.GetPartsInventoryIds();
		}
		if (_ToolTipFont == null)
		{
			_ToolTipFont = _QuanFont;
		}
	}

	public virtual void AddInvMenuItem(ItemData item, int quantity = 1)
	{
		if (mKAUISelect != null && mKAUISelect.pKAUiSelectTabMenu != null && mKAUISelect.pKAUiSelectTabMenu.pSelectedTab != null)
		{
			if (mKAUISelect.pKAUiSelectTabMenu.pSelectedTab.pTabData != null && !mKAUISelect.pKAUiSelectTabMenu.pSelectedTab.pTabData.ValidateItem(item))
			{
				return;
			}
			mKAUISelect.pKAUiSelectTabMenu.pSelectedTab.mNumSlotOccupied++;
		}
		KAUISelectItemData widgetData = new KAUISelectItemData(this, item, _WHSize, quantity);
		AddWidgetItem(widgetData);
	}

	public virtual void AddInvMenuItem(UserItemData userItem, int quantity = 1)
	{
		InventoryTab inventoryTab = null;
		if (mKAUISelect != null && mKAUISelect.pKAUiSelectTabMenu != null && mKAUISelect.pKAUiSelectTabMenu.pSelectedTab != null)
		{
			inventoryTab = mKAUISelect.pKAUiSelectTabMenu.pSelectedTab;
		}
		if (inventoryTab != null && inventoryTab.pTabData != null)
		{
			if (!inventoryTab.pTabData.ValidateItem(userItem))
			{
				return;
			}
			inventoryTab.mNumSlotOccupied++;
		}
		KAUISelectItemData kAUISelectItemData = new KAUISelectItemData(this, userItem, _WHSize, quantity);
		if (inventoryTab != null && inventoryTab.pTabData != null && inventoryTab.pTabData._DisableEquipped && mEquippedItems != null && mEquippedItems.Contains(userItem.UserInventoryID))
		{
			kAUISelectItemData._Disabled = true;
		}
		AddWidgetItem(kAUISelectItemData);
	}

	protected void AddWidgetItem(KAUISelectItemData widgetData)
	{
		KAWidget kAWidget = AddWidget(widgetData._ItemData.ItemID.ToString());
		if (kAWidget != null)
		{
			kAWidget.SetTexture(null);
			widgetData.ShowLoadingItem(inShow: true);
			kAWidget.SetUserData(widgetData);
			kAWidget.SetVisibility(inVisible: true);
			if (!string.IsNullOrEmpty(widgetData._ItemData.ItemName))
			{
				string text = widgetData._ItemData.ItemName;
				if (CommonInventoryData.pShowItemID)
				{
					text = text + "(" + widgetData._ItemData.ItemID + ")";
				}
				kAWidget.SetToolTipText(text);
			}
		}
		UpdateWidget(widgetData);
	}

	public virtual void UpdateWidget(KAUISelectItemData id)
	{
		id.LoadResource();
	}

	public virtual void ChangeCategory(int c)
	{
		ChangeCategory(c, forceChange: false);
	}

	public virtual void ChangeCategory(int c, bool forceChange)
	{
		ChangeCategory(new int[1] { c }, forceChange);
	}

	public virtual void ChangeCategory(int[] categories, bool forceChange)
	{
		if (categories == null)
		{
			return;
		}
		bool flag = true;
		if (mCategoryIDs != null)
		{
			if (categories.Length == mCategoryIDs.Length)
			{
				for (int i = 0; i < categories.Length; i++)
				{
					if (categories[i] != mCategoryIDs[i])
					{
						flag = false;
					}
				}
			}
			else
			{
				flag = false;
			}
		}
		if (!forceChange && flag)
		{
			FinishMenuItems();
			return;
		}
		mMenuPopulated = false;
		mFirstItemProcessed = false;
		mCategoryIDs = categories;
		mItemInitialized = true;
		ClearItems();
		mDBItemList.Clear();
		mFinalized = false;
		base.pViewChanged = true;
		mNumItemToBeLoaded = 0;
		mStoreLoading = true;
		if (_LoadItemIdList != null && _LoadItemIdList.Length != 0)
		{
			mNumItemToBeLoaded = _LoadItemIdList.Length;
			int[] loadItemIdList = _LoadItemIdList;
			for (int j = 0; j < loadItemIdList.Length; j++)
			{
				ItemData.Load(loadItemIdList[j], OnLoadItemDataReady, null);
			}
		}
		if (_StoreIDs != null && _StoreIDs.Length != 0)
		{
			mCurStoreIdx = 0;
			ItemStoreDataLoader.Load(_StoreIDs[mCurStoreIdx], OnStoreLoaded);
		}
		else
		{
			mStoreLoading = false;
		}
		if (!mStoreLoading && mNumItemToBeLoaded == 0)
		{
			mStoreLoading = false;
			FinishMenuItems();
		}
	}

	public void OnLoadItemDataReady(int itemID, ItemData dataItem, object inUserData)
	{
		mDBItemList.Add(new DBItem(dataItem, 1));
		mNumItemToBeLoaded--;
		if (mNumItemToBeLoaded == 0 && !mStoreLoading)
		{
			FinishMenuItems();
		}
	}

	public virtual void FinishMenuItems(bool addParentItems = false)
	{
		if (mFinalized)
		{
			return;
		}
		mFinalized = true;
		InventoryTab inventoryTab = null;
		if (CommonInventoryData.pIsReady)
		{
			UserItemData[] items = CommonInventoryData.pInstance.GetItems(mCategoryIDs);
			if (mKAUISelect != null && mKAUISelect.pKAUiSelectTabMenu != null && mKAUISelect.pKAUiSelectTabMenu.pSelectedTab != null)
			{
				inventoryTab = mKAUISelect.pKAUiSelectTabMenu.pSelectedTab;
			}
			if (inventoryTab != null && inventoryTab.pTabData != null)
			{
				inventoryTab.mNumSlotUnlocked = inventoryTab.pTabData.GetTotalSlots();
			}
			if (items != null)
			{
				UserItemData[] array = items;
				foreach (UserItemData userItemData in array)
				{
					if (!userItemData.Item.IsSubPart() && (AllowDuplicateItems() || !IsInList(userItemData.Item.ItemID)))
					{
						mDBItemList.Add(new DBItem(userItemData, userItemData.Quantity));
					}
				}
			}
		}
		if (addParentItems && ParentData.pIsReady)
		{
			UserItemData[] items2 = ParentData.pInstance.pInventory.GetItems(mCategoryIDs);
			if (items2 != null)
			{
				UserItemData[] array = items2;
				foreach (UserItemData userItemData2 in array)
				{
					if (!userItemData2.Item.IsSubPart() && (AllowDuplicateItems() || !IsInList(userItemData2.Item.ItemID)))
					{
						mDBItemList.Add(new DBItem(userItemData2, userItemData2.Quantity));
					}
				}
			}
		}
		if (inventoryTab != null)
		{
			inventoryTab.mNumSlotOccupied = 0;
		}
		foreach (DBItem mDBItem in mDBItemList)
		{
			if (_MultiInstance)
			{
				for (int j = 0; j < mDBItem._Quantity; j++)
				{
					if (mDBItem._UserItem != null)
					{
						AddInvMenuItem(mDBItem._UserItem);
					}
					else
					{
						AddInvMenuItem(mDBItem._Item);
					}
				}
			}
			else if (mDBItem._UserItem != null)
			{
				AddInvMenuItem(mDBItem._UserItem, mDBItem._Quantity);
			}
			else
			{
				AddInvMenuItem(mDBItem._Item, mDBItem._Quantity);
			}
		}
		UpdateSelectedWithCurrent();
		if (inventoryTab != null)
		{
			mKAUISelect.ShowAvailableEmptySlots(inventoryTab);
		}
		base.pViewChanged = true;
		mMenuPopulated = true;
	}

	public virtual bool AllowDuplicateItems()
	{
		return true;
	}

	public virtual void UpdateSelectedWithCurrent()
	{
		KAWidget kAWidget = null;
		foreach (KAWidget item in mItemInfo)
		{
			if (IsItemCurrent(item))
			{
				kAWidget = item;
			}
		}
		if (kAWidget != null)
		{
			SelectItem(kAWidget);
		}
	}

	public virtual void SelectItem(KAWidget inWidget)
	{
		if (mKAUISelect != null)
		{
			mKAUISelect.SelectItem(inWidget);
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

	public virtual void UnSelectItem()
	{
		if (!(mCurSelectedItem == null))
		{
			if (_UseIconCursor)
			{
				mCursorItem.SetState(KAUIState.INTERACTIVE);
				mMainUI.RemoveWidget(mCursorItem);
				mCursorItem.DetachFromCursor();
			}
			if (_RemoveWhenSelected)
			{
				base.pViewChanged = true;
				AddOneItem(mCurSelectedItem);
			}
			mCurSelectedItem = null;
			mCursorItem = null;
		}
	}

	private KAWidget RemoveOneItem(KAWidget item)
	{
		RemoveWidget(item);
		return item;
	}

	private void AddOneItem(KAWidget item)
	{
		AddWidget(item);
		item.SetPosition(0f, 0f);
		item.SetState(KAUIState.INTERACTIVE);
	}

	public override void SetVisibility(bool t)
	{
		base.SetVisibility(t);
		if (!t)
		{
			UnSelectItem();
		}
	}

	public override void OnHover(KAWidget inWidget, bool inIsHover)
	{
		if (!UtPlatform.IsMobile())
		{
			base.OnHover(inWidget, inIsHover);
		}
	}

	public virtual void OnStoreLoaded(StoreData sd)
	{
		if (sd != null && sd._ID == _StoreIDs[mCurStoreIdx])
		{
			int[] array = mCategoryIDs;
			foreach (int cat in array)
			{
				StoreCategoryData storeCategoryData = sd.FindCategoryData(cat);
				if (storeCategoryData == null || storeCategoryData._Items == null)
				{
					continue;
				}
				foreach (ItemData item in storeCategoryData._Items)
				{
					if (item.IsSubPart() || IsInList(item.ItemID))
					{
						continue;
					}
					bool flag = true;
					if (item.Relationship != null)
					{
						ItemDataRelationship[] relationship = item.Relationship;
						foreach (ItemDataRelationship itemDataRelationship in relationship)
						{
							if (itemDataRelationship.Type == "Prereq")
							{
								flag = false;
								if ((CommonInventoryData.pIsReady && CommonInventoryData.pInstance.FindItem(itemDataRelationship.ItemId) != null) || (ParentData.pIsReady && ParentData.pInstance.HasItem(itemDataRelationship.ItemId)))
								{
									flag = true;
								}
								break;
							}
						}
					}
					if (flag)
					{
						mDBItemList.Add(new DBItem(item, 1));
					}
				}
			}
		}
		if (mCurStoreIdx == _StoreIDs.Length - 1)
		{
			mStoreLoading = false;
			if (mNumItemToBeLoaded == 0)
			{
				FinishMenuItems();
			}
		}
		else
		{
			mCurStoreIdx++;
			ItemStoreDataLoader.Load(_StoreIDs[mCurStoreIdx], OnStoreLoaded);
		}
	}

	protected bool IsInList(int itemID)
	{
		foreach (DBItem mDBItem in mDBItemList)
		{
			if (itemID == mDBItem._Item.ItemID)
			{
				return true;
			}
		}
		return false;
	}

	public void ReloadMenu()
	{
		mItemInitialized = false;
	}

	public virtual void MenuOnGUI()
	{
		if (GetVisibility() && mItemInitialized && (mNeedPageCheck || base.pViewChanged))
		{
			mNeedPageCheck = false;
			base.pViewChanged = false;
			UpdateRange(GetTopItemIdx(), GetNumItemsPerPage());
		}
	}

	public virtual void UpdateRange(int s, int e)
	{
		CoBundleLoader.SetVisibleRangeStatic(this, s, e);
	}

	private void ShowDialog(int id, string text)
	{
		if (mKAUIGenericDB != null)
		{
			mKAUIGenericDB.Destroy();
		}
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "PfKAUIGenericDB");
		if (mKAUIGenericDB != null)
		{
			mKAUIGenericDB._MessageObject = base.gameObject;
			mKAUIGenericDB._CloseMessage = "OnCloseDialog";
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: true);
			mKAUIGenericDB.SetTextByID(id, text, interactive: false);
			KAUI.SetExclusive(mKAUIGenericDB, _MaskColor);
		}
	}

	private void ShowMembershipDialog(int id, string text)
	{
		if (mKAUIGenericDB != null)
		{
			mKAUIGenericDB.Destroy();
		}
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "PfKAUIGenericDB");
		if (mKAUIGenericDB != null)
		{
			mKAUIGenericDB._MessageObject = base.gameObject;
			mKAUIGenericDB._OKMessage = "BecomeAMember";
			mKAUIGenericDB._CloseMessage = "OnCloseDialog";
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: true);
			mKAUIGenericDB.SetTextByID(id, text, interactive: false);
			KAUI.SetExclusive(mKAUIGenericDB, _MaskColor);
		}
	}

	private void BecomeAMember()
	{
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.MEMBERSHIP, base.gameObject);
		OnCloseDialog();
	}

	private void OnCloseDialog()
	{
		if (mKAUIGenericDB != null)
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (!(inWidget == null))
		{
			base.OnClick(inWidget);
			KAUISelectItemData widgetData = (KAUISelectItemData)inWidget.GetUserData();
			if (ItemLocked(widgetData))
			{
				SetSelectedItem(null);
			}
			else
			{
				SelectItem(inWidget);
			}
		}
	}

	protected bool ItemLocked(KAUISelectItemData widgetData)
	{
		if (widgetData != null && widgetData._Locked)
		{
			if (_LockedVO != null)
			{
				SnChannel.Play(_LockedVO, "VO_Pool", inForce: true, null);
			}
			else
			{
				ShowMembershipDialog(_LockedItemText._ID, _LockedItemText._Text);
			}
			return true;
		}
		return false;
	}

	public void RefreshItemLockedState()
	{
		foreach (KAWidget item in mItemInfo)
		{
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)item.GetUserData();
			if (kAUISelectItemData != null && kAUISelectItemData._ItemData != null)
			{
				kAUISelectItemData._Locked = kAUISelectItemData._ItemData.Locked && !SubscriptionInfo.pIsMember;
			}
		}
	}

	public virtual void OnIAPStoreClosed()
	{
		RefreshItemLockedState();
	}

	public virtual void PlayIntro()
	{
		if (_LongIntroVO != null && _LongIntroVO.Length != 0 && !ProductData.TutorialComplete(base.gameObject.name + "_INTRO"))
		{
			SnChannel.Play(_LongIntroVO, "VO_Pool", 0, inForce: true, null);
			ProductData.AddTutorial(base.gameObject.name + "_INTRO");
		}
		else if (_IntroVO != null)
		{
			SnChannel.Play(_IntroVO, "VO_Pool", inForce: true, null);
		}
	}

	protected override void Update()
	{
		if (!mItemInitialized && IsCurrentDataReady() && CommonInventoryData.pIsReady)
		{
			ChangeCategory(mCategoryIDs, forceChange: true);
		}
		if (mItemInitialized && _DefaultFirstItem && !mFirstItemProcessed)
		{
			KAWidget kAWidget = FindItemAt(0);
			if (kAWidget != null && ((KAUISelectItemData)kAWidget.GetUserData()).pIsReady)
			{
				OnClick(kAWidget);
				mFirstItemProcessed = true;
			}
		}
	}

	public virtual bool IsItemCurrent(KAWidget item)
	{
		return false;
	}

	public virtual void SaveSelection()
	{
	}

	public virtual bool IsCurrentDataReady()
	{
		return true;
	}

	public virtual void OnItemLoaded(KAUISelectItemData idata)
	{
	}

	public virtual void LoadCurrentData(KAUISelect mainUI)
	{
	}

	public override void OnDragStart(KAWidget inWidget)
	{
		if (!_AllowItemDrag)
		{
			return;
		}
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)inWidget.GetUserData();
		if (kAUISelectItemData == null || kAUISelectItemData._Disabled)
		{
			return;
		}
		base.OnDragStart(inWidget);
		if (kAUISelectItemData == null || kAUISelectItemData._ItemData == null || kAUISelectItemData._ItemData.ItemID == 0 || (!(KAUIManager.pInstance.pDragItem == null) && KAUIManager.pInstance.pDragItem.pAttachToCursor))
		{
			return;
		}
		KAWidget kAWidget = CreateDragObject(inWidget, base.pPanel.depth + 1);
		mKAUISelect.AddWidgetData(inWidget, null);
		mKAUISelect.UpdateOccupiedSlots(-1);
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
		if (!_AllowItemDrag)
		{
			return;
		}
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)sourceWidget.GetUserData();
		if (kAUISelectItemData != null && kAUISelectItemData._Disabled)
		{
			return;
		}
		GameObject hoveredObject = UICamera.hoveredObject;
		if (hoveredObject != null)
		{
			KAUISelectMenu kAUISelectMenu = null;
			KAWidget component = hoveredObject.GetComponent<KAWidget>();
			if (component != null)
			{
				KAUISelectItemData kAUISelectItemData2 = (KAUISelectItemData)component.GetUserData();
				if (kAUISelectItemData2 != null)
				{
					kAUISelectMenu = kAUISelectItemData2._Menu;
				}
			}
			else
			{
				KAUISelect component2 = hoveredObject.GetComponent<KAUISelect>();
				if (component2 != null)
				{
					kAUISelectMenu = component2.pKAUiSelectMenu;
				}
			}
			if (kAUISelectMenu != null && kAUISelectMenu != this)
			{
				foreach (KAWidget item in kAUISelectMenu.GetItems())
				{
					KAUISelectItemData kAUISelectItemData3 = (KAUISelectItemData)item.GetUserData();
					if (kAUISelectItemData3 != null && kAUISelectItemData3._ItemID == 0 && !kAUISelectItemData3._SlotLocked)
					{
						ProcessDrop(item);
						MoveItemToBottom(sourceWidget);
						kAUISelectMenu.SetTopItemIdx(kAUISelectMenu.FindItemIndex(item));
						return;
					}
				}
				if (kAUISelectMenu.mKAUISelect != null)
				{
					kAUISelectMenu.mKAUISelect.CheckInventoryFull();
				}
			}
		}
		MoveCursorItemBack(sourceWidget);
	}

	public void MoveItemToBottom(KAWidget inWidget)
	{
		int num = 0;
		for (int num2 = mItemInfo.Count - 1; num2 >= 0; num2--)
		{
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)mItemInfo[num2].GetUserData();
			if (kAUISelectItemData != null && !kAUISelectItemData._Locked && !kAUISelectItemData._SlotLocked)
			{
				num = num2;
				break;
			}
		}
		mItemInfo.Insert(num + 1, inWidget);
		mItemInfo.Remove(inWidget);
		RepositionMenu();
	}

	public void RepositionMenu()
	{
		KAUIMenuGrid kAUIMenuGrid = mCurrentGrid;
		kAUIMenuGrid.onReposition = (UITable.OnReposition)Delegate.Remove(kAUIMenuGrid.onReposition, new UITable.OnReposition(OnGridReposition));
		mCurrentGrid.Reposition();
		KAUIMenuGrid kAUIMenuGrid2 = mCurrentGrid;
		kAUIMenuGrid2.onReposition = (UITable.OnReposition)Delegate.Combine(kAUIMenuGrid2.onReposition, new UITable.OnReposition(OnGridReposition));
	}

	protected virtual void MoveCursorItemBack(KAWidget sourceWidget)
	{
		if (!(KAUIManager.pInstance.pDragItem != null) || !(KAUIManager.pInstance.pDragItem.GetUserData() is KAUISelectItemData kAUISelectItemData) || kAUISelectItemData._ItemData == null)
		{
			return;
		}
		KAUISelectMenu menu = kAUISelectItemData._Menu;
		if (menu != null)
		{
			KAUISelect kAUISelect = menu._ParentUi as KAUISelect;
			if (kAUISelect != null)
			{
				kAUISelect.UpdateOccupiedSlots(1);
				kAUISelect.ResetDragItem(sourceWidget, KAUIManager.pInstance.pDragItem);
			}
			DestroyDragItem();
		}
	}

	protected void DestroyDragItem()
	{
		if (KAUIManager.pInstance.pDragItem != null)
		{
			KAUIManager.pInstance.pDragItem.DetachFromCursor();
			UnityEngine.Object.Destroy(KAUIManager.pInstance.pDragItem.gameObject);
		}
	}

	protected virtual void ProcessDrop(KAWidget inTargetWidget)
	{
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)KAUIManager.pInstance.pDragItem.GetUserData();
		if (kAUISelectItemData != null && kAUISelectItemData._ItemData != null && inTargetWidget != null)
		{
			KAUISelectItemData kAUISelectItemData2 = (KAUISelectItemData)inTargetWidget.GetUserData();
			if (kAUISelectItemData2 != null)
			{
				kAUISelectItemData._Menu = kAUISelectItemData2._Menu;
				KAUISelect obj = (KAUISelect)kAUISelectItemData2._Menu._ParentUi;
				obj.AddWidgetData(inTargetWidget, kAUISelectItemData);
				obj.CheckForAdditionalEmptySlots();
				obj.UpdateOccupiedSlots(1);
				kAUISelectItemData2._Menu.SetSelectedItem(inTargetWidget);
				obj.SelectItem(inTargetWidget);
			}
			KAUIManager.pInstance.pDragItem.DetachFromCursor();
			UnityEngine.Object.Destroy(KAUIManager.pInstance.pDragItem.gameObject);
		}
	}

	protected virtual void AddToTargetMenu(KAWidget inWidget, KAUIMenu targetMenu)
	{
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)inWidget.GetUserData();
		if (!kAUISelectItemData._Disabled && AddToTargetMenu(kAUISelectItemData, targetMenu))
		{
			mKAUISelect.UpdateOccupiedSlots(-1);
			mKAUISelect.AddWidgetData(inWidget, null);
			MoveItemToBottom(inWidget);
		}
	}

	public virtual bool AddToTargetMenu(KAUISelectItemData widgetData, KAUIMenu targetMenu, bool inventoryCheck = true)
	{
		if (targetMenu._ParentUi == null)
		{
			return false;
		}
		if (widgetData != null && widgetData._ItemData != null && widgetData._ItemData.ItemID != 0)
		{
			foreach (KAWidget item in targetMenu.GetItems())
			{
				KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)item.GetUserData();
				if (kAUISelectItemData != null && kAUISelectItemData._ItemID == 0 && !kAUISelectItemData._SlotLocked)
				{
					widgetData._Menu = kAUISelectItemData._Menu;
					if (widgetData._Item == null)
					{
						widgetData._Item = kAUISelectItemData._Item;
					}
					((KAUISelect)targetMenu._ParentUi).AddWidgetData(item, widgetData);
					((KAUISelect)targetMenu._ParentUi).CheckForAdditionalEmptySlots();
					((KAUISelect)targetMenu._ParentUi).UpdateOccupiedSlots(1);
					targetMenu.SetTopItemIdx(targetMenu.FindItemIndex(item));
					targetMenu.SetSelectedItem(item);
					((KAUISelect)targetMenu._ParentUi).SelectItem(item);
					return true;
				}
			}
		}
		if (inventoryCheck)
		{
			((KAUISelect)targetMenu._ParentUi).CheckInventoryFull();
		}
		return false;
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		UICamera.MouseOrTouch mouseOrTouch = ((UICamera.currentScheme == UICamera.ControlScheme.Mouse) ? UICamera.mouse0 : UICamera.currentTouch);
		if (!hasFocus && mouseOrTouch != null && mouseOrTouch.dragged != null)
		{
			KAWidget component = mouseOrTouch.dragged.GetComponent<KAWidget>();
			if (component != null && component.GetUserData() is KAUISelectItemData kAUISelectItemData && kAUISelectItemData._Menu == this)
			{
				MoveCursorItemBack(component);
			}
		}
	}

	public bool IsTaskComplete(KAUISelectItemData data)
	{
		if (data != null && data._ItemData != null && !data._ItemData.HasAttribute("PrereqTask"))
		{
			return true;
		}
		int attribute = data._ItemData.GetAttribute("PrereqTask", 0);
		if (attribute > 0)
		{
			Task task = MissionManager.pInstance.GetTask(attribute);
			if (task != null && task.pCompleted)
			{
				return true;
			}
		}
		return false;
	}

	public void ShowTaskItemPopUp()
	{
		GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _TaskItemText.GetLocalizedString(), "", null, "", "", "Ok", "", inDestroyOnClick: true);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		DestroyDragItem();
	}
}
