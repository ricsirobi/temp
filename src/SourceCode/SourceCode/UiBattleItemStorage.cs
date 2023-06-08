using System;
using System.Collections.Generic;
using UnityEngine;

public class UiBattleItemStorage : KAUISelect
{
	public InventoryTab _BattleReadyTab;

	public List<UiAvatarCustomization.AvatarPartCat> _ClothingPartCategory;

	public LocaleString _BattleStorageDBTitleText = new LocaleString("My Room");

	public LocaleString _UserItemSaveErrorText;

	public LocaleString _AvailableSlotsText = new LocaleString("[REVIEW]Open slots: ");

	public string _ItemColorWidget = "CellBkg";

	public string _DialogAssetName = "PfKAUIGenericDBSm";

	[NonSerialized]
	private UiBattleReadyItemMenu mItemMenu;

	private UiStorageContentMenu mStorageMenu;

	private UiRackMenu mRackMenu;

	private KAUIMenu mCategoryMenu;

	private KAUI mUiCategory;

	private KAWidget mCategoryMenuBtn;

	private KAWidget mSlotsInfoTxt;

	public bool _IsWeaponRack;

	private List<UiAvatarCustomization.AvatarPartCat> mSelectedCategory;

	private Color mItemDefaultColor = Color.white;

	private List<int> mCatToExclude;

	public Color pItemDefaultColor
	{
		get
		{
			return mItemDefaultColor;
		}
		set
		{
			mItemDefaultColor = value;
		}
	}

	public List<int> pCatToExclude => mCatToExclude;

	private UiMyRoomBuilder pMyRoomBuilder => MyRoomsIntMain.pInstance._UiMyRoomsInt._MyRoomBuilder;

	public override void OnOpen()
	{
		base.OnOpen();
		if (AvAvatar.pObject != null)
		{
			AvAvatar.pAvatarCam.SetActive(value: true);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mUiCategory != null && mUiCategory.IsActive() && Input.GetMouseButtonUp(0) && KAUIManager.pInstance.pSelectedWidget != mCategoryMenuBtn)
		{
			mUiCategory.SetVisibility(inVisible: false);
		}
	}

	public override void Initialize()
	{
		mUiCategory = _UiList[1];
		if (mUiCategory != null)
		{
			mCategoryMenu = mUiCategory._MenuList[0];
		}
		mCategoryMenuBtn = FindItem("CategoryMenuBtn");
		mSlotsInfoTxt = FindItem("SlotsInfo");
		SetBattleReadyTabData();
		base.Initialize();
		mItemMenu = (UiBattleReadyItemMenu)_MenuList[1];
		mRackMenu = (UiRackMenu)_UiList[0];
		mStorageMenu = (UiStorageContentMenu)_MenuList[2];
		PairData userItemAttributes = pMyRoomBuilder.pSelectedObject.GetComponent<MyRoomObject>().pUserItemData.UserItemAttributes;
		WeaponStorageRoomItem component = pMyRoomBuilder.pSelectedObject.GetComponent<WeaponStorageRoomItem>();
		if (userItemAttributes.GetStringValue("StorageItemType", string.Empty).Equals("WeaponChest"))
		{
			_IsWeaponRack = false;
			mSelectedCategory = _ClothingPartCategory;
			mStorageMenu._StorageMenuContainner.SetActive(value: true);
			mStorageMenu.gameObject.SetActive(value: true);
			mStorageMenu.SetVisibility(t: true);
		}
		else
		{
			_IsWeaponRack = true;
			WeaponRackRoomItem component2 = pMyRoomBuilder.pSelectedObject.GetComponent<WeaponRackRoomItem>();
			mCatToExclude = component2._CategoryToExclude;
			mSelectedCategory = SetSelectedCategory(component2._CategoryOverride);
			mRackMenu.SetVisibility(inVisible: true);
			mRackMenu.Initialize(component._DefaultSlotCount, this);
		}
		ChangeCategory(null);
	}

	private List<UiAvatarCustomization.AvatarPartCat> SetSelectedCategory(List<string> catPartList)
	{
		List<UiAvatarCustomization.AvatarPartCat> list = null;
		foreach (UiAvatarCustomization.AvatarPartCat item in _ClothingPartCategory)
		{
			if (catPartList.Contains(item._PartName))
			{
				if (list == null)
				{
					list = new List<UiAvatarCustomization.AvatarPartCat>();
				}
				list.Add(item);
			}
		}
		return list;
	}

	private void SetBattleReadyTabData()
	{
		_BattleReadyTab.UpdateFromInventorySetting();
	}

	public override void ChangeCategory(KAWidget item)
	{
		mKAUiSelectMenu.ChangeCategory(GetAllCategories(), forceChange: true);
		OpenClothesPartCategory();
		UpdateCategoryMenuButton(mCategoryMenu.GetItemAt(0));
		UpdateSlotsInfo();
	}

	private void OpenClothesPartCategory()
	{
		if (mCategoryMenu != null)
		{
			mCategoryMenu.ClearItems();
		}
		foreach (UiAvatarCustomization.AvatarPartCat item in mSelectedCategory)
		{
			if (!(item._PartName != "All") || _BattleReadyTab.pTabData.HasCategory(AvatarData.GetCategoryID(item._PartName)))
			{
				KAWidget kAWidget = DuplicateWidget("CatTemplateBtn");
				kAWidget.gameObject.name = item._BtnName;
				kAWidget.SetTexture(item._Icon, inPixelPerfect: true);
				if (!string.IsNullOrEmpty(item._DisplayText._Text))
				{
					kAWidget.SetText(item._DisplayText.GetLocalizedString());
				}
				kAWidget._TooltipInfo._Text = item._ToolTipText;
				if (mCategoryMenu != null)
				{
					mCategoryMenu.AddWidget(kAWidget);
				}
				kAWidget.SetVisibility(inVisible: true);
			}
		}
		if (mCategoryMenu != null && mCategoryMenu.GetNumItems() > 0)
		{
			mCategoryMenu.GetItemAt(0).SetDisabled(isDisabled: true);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mCategoryMenuBtn)
		{
			mUiCategory.SetVisibility(!mUiCategory.GetVisibility());
			return;
		}
		foreach (UiAvatarCustomization.AvatarPartCat item in mSelectedCategory)
		{
			if (item._BtnName == inWidget.name)
			{
				mItemMenu.ChangeCategory(item._PartName, forceChange: false);
				break;
			}
		}
		bool flag = false;
		for (int i = 0; i < mCategoryMenu.GetNumItems(); i++)
		{
			KAWidget kAWidget = mCategoryMenu.FindItemAt(i);
			if (kAWidget != null && inWidget.name == kAWidget.name)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		UpdateCategoryMenuButton(inWidget);
		if (!(mCategoryMenu != null))
		{
			return;
		}
		for (int j = 0; j < mCategoryMenu.GetNumItems(); j++)
		{
			KAWidget kAWidget2 = mCategoryMenu.FindItemAt(j);
			if (kAWidget2 != null)
			{
				kAWidget2.SetDisabled(inWidget.name == kAWidget2.name);
			}
		}
	}

	public KAWidget FindEmptySlotInItemMenu()
	{
		KAWidget result = null;
		foreach (KAWidget item in mItemMenu.GetItems())
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

	public KAWidget FindEmptySlotInStorageMenu()
	{
		KAWidget result = null;
		foreach (KAWidget item in mStorageMenu.GetItems())
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

	private void UpdateCategoryMenuButton(KAWidget inItem)
	{
		if (inItem == null)
		{
			return;
		}
		bool flag = inItem.name.Equals("All", StringComparison.OrdinalIgnoreCase);
		mCategoryMenuBtn.SetText(flag ? inItem.GetText() : string.Empty);
		Transform transform = mCategoryMenuBtn.transform.Find("Icon");
		if (!(transform != null))
		{
			return;
		}
		UITexture component = transform.GetComponent<UITexture>();
		if (component != null)
		{
			component.enabled = !flag;
		}
		if (flag)
		{
			return;
		}
		transform = inItem.transform.Find("Icon");
		if (transform != null)
		{
			UITexture component2 = transform.GetComponent<UITexture>();
			if (component2 != null)
			{
				component.mainTexture = component2.mainTexture;
			}
		}
	}

	public override void SelectItem(KAWidget item)
	{
		base.SelectItem(item);
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)item.GetUserData();
		if (kAUISelectItemData != null && kAUISelectItemData._SlotLocked)
		{
			_BattleReadyTab.pTabData.BuySlot(base.gameObject);
		}
	}

	public void UpdateOccupiedItemSlots(int count)
	{
		InventoryTab battleReadyTab = _BattleReadyTab;
		if (battleReadyTab != null)
		{
			battleReadyTab.mNumSlotOccupied += count;
			if (battleReadyTab.mNumSlotOccupied <= 0)
			{
				battleReadyTab.mNumSlotOccupied = 0;
			}
		}
	}

	public override KAWidget AddEmptySlot()
	{
		KAWidget kAWidget = mKAUiSelectMenu.AddWidget("EmptySlot");
		if (kAWidget != null)
		{
			AddWidgetData(kAWidget, null);
		}
		return kAWidget;
	}

	protected override void OnItemPurchaseComplete()
	{
		RefreshSlotStatus();
		UpdateSlotsInfo();
	}

	private void RefreshSlotStatus()
	{
		if (_BattleReadyTab.pTabData == null || mKAUiSelectMenu.GetItemCount() == 0)
		{
			return;
		}
		InventorySetting.TabData pTabData = _BattleReadyTab.pTabData;
		int occupiedSlots = pTabData.GetOccupiedSlots();
		int totalSlots = pTabData.GetTotalSlots();
		int num = totalSlots - _BattleReadyTab.mNumSlotUnlocked;
		if (num == 0)
		{
			return;
		}
		_BattleReadyTab.mNumSlotUnlocked = totalSlots;
		if (occupiedSlots >= totalSlots)
		{
			return;
		}
		if (pTabData._MaxNumSlots == -1)
		{
			KAWidget inWidget = mKAUiSelectMenu.GetItems()[mKAUiSelectMenu.GetItemCount() - 1];
			for (int i = 0; i < num; i++)
			{
				AddEmptySlot();
			}
			mItemMenu.MoveItemsToBottom(inWidget);
			return;
		}
		foreach (KAWidget item in mKAUiSelectMenu.GetItems())
		{
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)item.GetUserData();
			if (num > 0 && kAUISelectItemData._SlotLocked)
			{
				kAUISelectItemData.ShowSlotLock(item, addIcon: false, locked: false);
				num--;
			}
		}
	}

	public override void ShowAvailableEmptySlots(InventoryTab inTabData)
	{
		if (inTabData.pTabData == null)
		{
			return;
		}
		int num = ((inTabData.pTabData._MaxNumSlots == -1) ? (inTabData.mNumSlotOccupied + inTabData.mNumSlotUnlocked + 1) : inTabData.pTabData._MaxNumSlots);
		if (num == 0 || inTabData.mNumSlotOccupied >= num)
		{
			return;
		}
		int occupiedSlots = inTabData.pTabData.GetOccupiedSlots();
		if (occupiedSlots > num)
		{
			KAWidget kAWidget = AddEmptySlot();
			((KAUISelectItemData)kAWidget.GetUserData()).ShowSlotLock(kAWidget, addIcon: true);
			return;
		}
		for (int i = occupiedSlots; i < num; i++)
		{
			KAWidget kAWidget2 = AddEmptySlot();
			if (inTabData.pTabData._SlotItemID != 0 && kAWidget2 != null && i > inTabData.mNumSlotUnlocked - 1)
			{
				if (inTabData.pTabData._MaxNumSlots == -1)
				{
					((KAUISelectItemData)kAWidget2.GetUserData()).ShowSlotLock(kAWidget2, addIcon: true);
					break;
				}
				((KAUISelectItemData)kAWidget2.GetUserData()).ShowSlotLock(kAWidget2);
			}
		}
	}

	public void UpdateSlotsInfo()
	{
		if (!(mSlotsInfoTxt != null))
		{
			return;
		}
		if (_BattleReadyTab.pTabData != null)
		{
			int num = (_IsWeaponRack ? _BattleReadyTab.mNumSlotOccupied : _BattleReadyTab.pTabData.GetOccupiedSlots());
			int num2 = (_IsWeaponRack ? (_BattleReadyTab.mNumSlotUnlocked - _BattleReadyTab.pTabData.GetOccupiedSlots()) : _BattleReadyTab.pTabData.GetTotalSlots());
			if (_IsWeaponRack)
			{
				mSlotsInfoTxt.SetText(_AvailableSlotsText.GetLocalizedString() + num2);
			}
			else
			{
				mSlotsInfoTxt.SetText(num + "/" + num2);
			}
		}
		mSlotsInfoTxt.SetVisibility(inVisible: true);
	}

	public int[] GetAllCategories()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < mSelectedCategory.Count; i++)
		{
			list.Add(AvatarData.GetCategoryID(mSelectedCategory[i]._PartName));
		}
		return list.ToArray();
	}

	public void ShowBattleReadyIcon(KAWidget widget, bool show)
	{
		if (!(widget == null))
		{
			KAWidget kAWidget = widget.FindChildItem("BattleReadyIcon");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(show);
			}
		}
	}

	public void ShowFlightReadyIcon(KAWidget widget, bool show)
	{
		if (!(widget == null))
		{
			KAWidget kAWidget = widget.FindChildItem("FlightReadyIcon");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(show);
			}
		}
	}

	public void UpdateWidgetBackground(KAWidget widget, ItemData itemData, bool isBattleReady)
	{
		if (!(widget == null))
		{
			KAWidget widget2 = widget.FindChildItem(_ItemColorWidget);
			UpdateWidgetBackgroundColor(widget2, itemData, isBattleReady);
		}
	}

	public void UpdateWidgetBackgroundColor(KAWidget widget, ItemData itemData, bool isBattleReady)
	{
		if (widget != null)
		{
			if (isBattleReady)
			{
				UiItemRarityColorSet.SetItemBackgroundColor((itemData == null || !itemData.ItemRarity.HasValue) ? ItemRarity.Common : itemData.ItemRarity.Value, widget);
			}
			else
			{
				UiItemRarityColorSet.SetItemBackgroundColor(pItemDefaultColor, widget);
			}
		}
	}

	public override void OnClose()
	{
		SetInteractive(interactive: false);
		if (!_IsWeaponRack)
		{
			pMyRoomBuilder.pSelectedObject.GetComponent<WeaponChestRoomItem>().PlayCloseAnimation();
		}
		if (MyRoomsIntMain.pInstance != null)
		{
			MyRoomsIntMain.pInstance.SaveExplicit(new MyRoomsIntMain.SaveCallbackMessageData(base.gameObject, "OnItemsSaveComplete", "OnItemsSaveError"));
		}
	}

	private void OnItemsSaveComplete()
	{
		base.OnClose();
		pMyRoomBuilder.pSelectedObject = null;
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void ShowDialog(string assetName, string dbName, LocaleString title, string yesMessage, string noMessage, string okMessage, string closeMessage, bool destroyDB, LocaleString text, GameObject messageObject)
	{
		ShowDialog(assetName, dbName, title, yesMessage, noMessage, okMessage, closeMessage, destroyDB, text.GetLocalizedString(), messageObject);
	}

	public void ShowDialog(string assetName, string dbName, LocaleString title, string yesMessage, string noMessage, string okMessage, string closeMessage, bool destroyDB, string text, GameObject messageObject)
	{
		if (mKAUIGenericDB != null)
		{
			mKAUIGenericDB.Destroy();
		}
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB(assetName, dbName);
		if (mKAUIGenericDB != null)
		{
			mKAUIGenericDB.SetMessage(messageObject, yesMessage, noMessage, okMessage, closeMessage);
			mKAUIGenericDB.SetDestroyOnClick(destroyDB);
			mKAUIGenericDB.SetButtonVisibility(!string.IsNullOrEmpty(yesMessage), !string.IsNullOrEmpty(noMessage), !string.IsNullOrEmpty(okMessage), !string.IsNullOrEmpty(closeMessage));
			mKAUIGenericDB.SetText(text, interactive: false);
			mKAUIGenericDB.SetTitle(title.GetLocalizedString());
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
			KAUI.SetExclusive(mKAUIGenericDB);
		}
	}

	private void OnItemsSaveError()
	{
		ShowDialog(_DialogAssetName, "SaveError", _BattleStorageDBTitleText, string.Empty, string.Empty, "OnSaveErrorDBClose", string.Empty, destroyDB: true, _UserItemSaveErrorText, base.gameObject);
	}
}
