using System;
using System.Collections.Generic;
using UnityEngine;

public class UiStatCompareDB : KAUI
{
	public class ItemCompareDetails
	{
		public ItemData _ItemData;

		public Texture _ItemTexture;

		public ItemStat[] _ItemStats;

		public ItemTier? _Itemtier;
	}

	public class DBInfoDetails
	{
		public bool _ShowEquip = true;

		public bool _CanUnequip = true;

		public bool _ShowSellBtn;

		public bool _ShowDiscardBtn;

		public bool _ShowDBRightSide;
	}

	public KAWidget _BGWidget;

	public float _LowerOffset;

	public float _LeftOffset;

	public float _RightOffset;

	public float _SellBtnLeftPos = -112f;

	public float _SellBtnRightPos = 125f;

	public LocaleString _InfoText = new LocaleString("Tier: {x} {rarity}");

	public string _ItemColorWidget = "CellBkg";

	public EquippedSlotWidget[] _EquippedSlots;

	private KAWidget mCloseBtn;

	private KAWidget mUnequipBtn;

	private KAWidget mEquipBtn;

	private KAWidget mSellBtn;

	private KAWidget mDiscardBtn;

	private KAWidget mEquippedItem;

	private KAWidget mUnequippedItem;

	private KAWidget mEquippedInfo;

	private KAWidget mUnequippedInfo;

	private KAWidget mLineWidget;

	private UiStatsCompareMenu mMenu;

	private UISlicedSprite mLine;

	private UISlicedSprite mBackgroundSprite;

	private KAWidget mWidgetSelected;

	private GameObject mMessageObject;

	private Vector3 mEquipBtnOffset;

	private int mDefaultBGHeight;

	private Color mItemDefaultColor = Color.white;

	protected override void Awake()
	{
		base.Awake();
		mMenu = (UiStatsCompareMenu)_MenuList[0];
		if (!(_BGWidget != null))
		{
			return;
		}
		Transform transform = _BGWidget.transform.Find("Background");
		if (transform != null)
		{
			mBackgroundSprite = transform.GetComponent<UISlicedSprite>();
			if (mBackgroundSprite != null)
			{
				mDefaultBGHeight = mBackgroundSprite.height;
			}
		}
	}

	protected override void Start()
	{
		mCloseBtn = FindItem("CloseBtn");
		mUnequipBtn = FindItem("UnequipBtn");
		mEquipBtn = FindItem("EquipBtn");
		mSellBtn = FindItem("SellBtn");
		mEquippedItem = FindItem("EquippedItem");
		mUnequippedItem = FindItem("UnequippedItem");
		if (mEquippedItem != null)
		{
			mEquippedInfo = mEquippedItem.FindChildItem("TxtInfo");
		}
		if (mUnequippedItem != null)
		{
			mUnequippedInfo = mUnequippedItem.FindChildItem("TxtInfo");
		}
		mDiscardBtn = FindItem("DiscardBtn");
		if (mEquipBtn != null)
		{
			mEquipBtnOffset = mEquipBtn.transform.localPosition;
		}
		base.Start();
		mLineWidget = FindItem("VerticalLine");
		if (mLineWidget != null)
		{
			mLine = mLineWidget.GetComponentInChildren<UISlicedSprite>();
		}
		mItemDefaultColor = UiItemRarityColorSet.GetItemBackgroundColor(mEquippedItem, _ItemColorWidget);
		LoadEquippedItemsData();
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mCloseBtn)
		{
			SetVisibility(inVisible: false);
		}
		else if (inWidget == mDiscardBtn)
		{
			mMessageObject.SendMessage("OnDiscardClicked", mWidgetSelected);
		}
		else if (inWidget == mSellBtn)
		{
			mMessageObject.SendMessage("OnSellClicked", mWidgetSelected);
			SetVisibility(inVisible: false);
		}
		else if (inWidget == mEquipBtn || inWidget == mUnequipBtn)
		{
			mMessageObject.SendMessage("OnEquipClicked", mWidgetSelected);
			SetVisibility(inVisible: false);
		}
	}

	private bool IsRankLocked(ItemData itemData)
	{
		int num = 0;
		if (itemData.RewardTypeID > 0)
		{
			num = itemData.RewardTypeID;
		}
		if (itemData.Points.HasValue && itemData.Points.Value > 0)
		{
			UserAchievementInfo userAchievementInfoByType = UserRankData.GetUserAchievementInfoByType(num);
			if (userAchievementInfoByType != null && userAchievementInfoByType.AchievementPointTotal.HasValue)
			{
				return itemData.Points.Value > userAchievementInfoByType.AchievementPointTotal.Value;
			}
			return true;
		}
		if (itemData.RankId.HasValue && itemData.RankId.Value > 0)
		{
			UserRank userRank = ((num == 8) ? PetRankData.GetUserRank(SanctuaryManager.pCurPetData) : UserRankData.GetUserRankByType(num));
			if (userRank != null)
			{
				return itemData.RankId.Value > userRank.RankID;
			}
			return true;
		}
		return false;
	}

	private void UpdateInfo(KAWidget widget, ItemData itemData, ItemTier? itemTier)
	{
		if (widget != null)
		{
			if (itemTier.HasValue && itemData.ItemRarity.HasValue)
			{
				string localizedString = _InfoText.GetLocalizedString();
				localizedString = localizedString.Replace("{x}", ((int)itemTier.Value).ToString());
				localizedString = localizedString.Replace("{rarity}", InventorySetting.pInstance.GetItemRarityText(itemData.ItemRarity.Value));
				widget.SetText(localizedString);
				widget.SetVisibility(inVisible: true);
			}
			else
			{
				widget.SetText("");
				widget.SetVisibility(inVisible: false);
			}
		}
	}

	public void Initialize(ItemCompareDetails inLeftItem, ItemCompareDetails inRightItem, GameObject messageObject, DBInfoDetails inDBInfoDetails, KAWidget inWidget = null)
	{
		if (inLeftItem == null || inLeftItem._ItemData == null)
		{
			return;
		}
		if (mMenu != null)
		{
			mMenu.ClearItems();
		}
		bool flag = inLeftItem._ItemData.Locked && !SubscriptionInfo.pIsMember;
		bool flag2 = IsRankLocked(inLeftItem._ItemData);
		if (!flag && !flag2 && inRightItem != null && inRightItem._ItemData != null)
		{
			flag = inRightItem._ItemData.Locked && !SubscriptionInfo.pIsMember;
			flag2 = IsRankLocked(inRightItem._ItemData);
		}
		if (flag || flag2)
		{
			if (GetVisibility())
			{
				SetVisibility(inVisible: false);
			}
			return;
		}
		mMessageObject = messageObject;
		if (inLeftItem._ItemTexture != null)
		{
			mEquippedItem.SetTexture(inLeftItem._ItemTexture);
		}
		else if (inLeftItem._ItemData.IconName != null)
		{
			mEquippedItem.SetTextureFromBundle(inLeftItem._ItemData.IconName);
		}
		mEquippedItem.SetText(inLeftItem._ItemData.ItemName);
		mWidgetSelected = inWidget;
		bool flag3 = false;
		ShowBattleReadyIcon(mEquippedItem, inLeftItem._ItemStats != null);
		ShowFlightReadyIcon(mEquippedItem, inLeftItem._ItemData.HasAttribute("FlightSuit"));
		UpdateWidgetBackground(mEquippedItem, inLeftItem._ItemData, inLeftItem._ItemStats != null);
		UpdateInfo(mEquippedInfo, inLeftItem._ItemData, inLeftItem._Itemtier);
		if (inRightItem != null && inRightItem._ItemData != null)
		{
			UpdateInfo(mUnequippedInfo, inRightItem._ItemData, inRightItem._Itemtier);
			if (inRightItem._ItemTexture != null)
			{
				mUnequippedItem.SetTexture(inRightItem._ItemTexture);
			}
			else if (inWidget.GetTexture() != null)
			{
				mUnequippedItem.SetTexture(inWidget.GetTexture());
			}
			else if (inRightItem._ItemData != null && inRightItem._ItemData.IconName != null)
			{
				mEquippedItem.SetTextureFromBundle(inRightItem._ItemData.IconName);
			}
			mUnequippedItem.SetText(inRightItem._ItemData.ItemName);
			mUnequippedItem.SetVisibility(inVisible: true);
			flag3 = true;
			ShowBattleReadyIcon(mUnequippedItem, inRightItem._ItemStats != null);
			ShowFlightReadyIcon(mUnequippedItem, inRightItem._ItemData.HasAttribute("FlightSuit"));
			UpdateWidgetBackground(mUnequippedItem, inRightItem._ItemData, inRightItem._ItemStats != null);
		}
		else
		{
			mUnequippedItem.SetVisibility(inVisible: false);
		}
		if (mLineWidget != null)
		{
			mLineWidget.SetVisibility(inRightItem != null);
		}
		mMenu.Populate(inLeftItem._ItemStats, inRightItem?._ItemStats, flag3);
		if (mEquipBtn != null)
		{
			mEquipBtn.SetVisibility(inDBInfoDetails._ShowEquip);
		}
		if (inDBInfoDetails._ShowSellBtn)
		{
			Vector3 position = mSellBtn.GetPosition();
			mSellBtn.SetPosition(flag3 ? _SellBtnRightPos : _SellBtnLeftPos, position.y);
			mSellBtn.SetVisibility(inVisible: true);
		}
		else
		{
			mSellBtn.SetVisibility(inVisible: false);
		}
		if (mDiscardBtn != null)
		{
			mDiscardBtn.SetVisibility(inDBInfoDetails._ShowDiscardBtn);
		}
		if (mUnequipBtn != null)
		{
			mUnequipBtn.SetVisibility(inDBInfoDetails._CanUnequip && !inDBInfoDetails._ShowEquip);
		}
		SetVisibility(inVisible: true);
		ResizeDB();
		Reposition(inWidget, inDBInfoDetails._ShowDBRightSide);
	}

	private void ShowBattleReadyIcon(KAWidget widget, bool show)
	{
		KAWidget kAWidget = widget.FindChildItem("BattleReadyIcon");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(show);
		}
	}

	private void ShowFlightReadyIcon(KAWidget widget, bool show)
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

	private void UpdateWidgetBackground(KAWidget widget, ItemData itemData, bool isBattleReady)
	{
		KAWidget kAWidget = widget.FindChildItem(_ItemColorWidget);
		if (kAWidget != null)
		{
			if (isBattleReady)
			{
				UiItemRarityColorSet.SetItemBackgroundColor((itemData == null || !itemData.ItemRarity.HasValue) ? ItemRarity.Common : itemData.ItemRarity.Value, kAWidget);
			}
			else
			{
				UiItemRarityColorSet.SetItemBackgroundColor(mItemDefaultColor, kAWidget);
			}
		}
	}

	private void ResizeDB()
	{
		float num = (float)mMenu.GetItemCount() * mMenu._DefaultGrid.cellHeight;
		float num2 = (float)mDefaultBGHeight + num - (float)mBackgroundSprite.height;
		UISlicedSprite[] componentsInChildren = _BGWidget.transform.GetComponentsInChildren<UISlicedSprite>();
		if (componentsInChildren != null)
		{
			UISlicedSprite[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].height += (int)num2;
			}
		}
		Vector3 localPosition = mEquipBtnOffset + Vector3.down * num;
		if (mEquipBtn != null)
		{
			mEquipBtn.transform.localPosition = localPosition;
		}
		if (mDiscardBtn != null)
		{
			mDiscardBtn.transform.localPosition = localPosition;
		}
		if (mUnequipBtn != null)
		{
			mUnequipBtn.transform.localPosition = localPosition;
		}
		mLine.height = (int)num;
	}

	private void AdjustLine()
	{
		mLine.height = (int)mMenu._DefaultGrid.cellHeight * mMenu.GetItemCount();
		Vector3 zero = Vector3.zero;
		zero.y = mMenu._DefaultGrid.transform.position.y - mMenu._DefaultGrid.cellHeight * (float)(mMenu.GetItemCount() - 1) / 2f;
		mLineWidget.SetPosition(0f, zero.y);
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (!inVisible)
		{
			if (mMenu != null)
			{
				mMenu.ClearItems();
			}
			if (mEquipBtn != null)
			{
				mEquipBtn.SetVisibility(inVisible: false);
			}
			if (mUnequipBtn != null)
			{
				mUnequipBtn.SetVisibility(inVisible: false);
			}
			mUnequippedItem.SetTexture(null);
			mEquippedItem.SetTexture(null);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (GetVisibility() && KAInput.GetMouseButtonDown(0) && (KAUI.GetGlobalMouseOverItem() == null || KAUI.GetGlobalMouseOverItem().pUI != this))
		{
			SetVisibility(inVisible: false);
		}
	}

	public void Reposition(KAWidget inWidget, bool rhs)
	{
		Vector3 zero = Vector3.zero;
		Bounds bounds = NGUIMath.CalculateAbsoluteWidgetBounds(base.transform);
		Bounds bounds2 = NGUIMath.CalculateAbsoluteWidgetBounds(inWidget.transform);
		Vector3 vector = KAUIManager.pInstance.camera.ScreenToWorldPoint(Vector3.zero);
		Vector3 vector2 = KAUIManager.pInstance.camera.ScreenToWorldPoint(new Vector3(Screen.safeArea.width, Screen.safeArea.height, 0f));
		if (rhs)
		{
			zero.x = inWidget.transform.position.x + bounds.extents.x + bounds2.extents.x;
			if (zero.x + bounds.extents.x + _RightOffset > vector2.x)
			{
				float num = zero.x + bounds.extents.x + _RightOffset - vector2.x;
				zero.x -= num;
			}
		}
		else
		{
			zero.x = inWidget.transform.position.x - bounds.extents.x - bounds2.extents.x;
			if (zero.x - bounds.extents.x - _LeftOffset < vector.x)
			{
				float num2 = vector.x - (zero.x - bounds.extents.x - _LeftOffset);
				zero.x += num2;
			}
		}
		zero.y = inWidget.transform.position.y + bounds2.extents.y;
		if (zero.y - bounds.extents.y * 2f - _LowerOffset < vector.y)
		{
			zero.y += vector.y - (zero.y - bounds.extents.y * 2f - _LowerOffset);
		}
		base.transform.position = zero;
	}

	private void LoadEquippedItemsData()
	{
		if (_EquippedSlots == null || _EquippedSlots.Length == 0)
		{
			return;
		}
		List<int> list = new List<int>();
		if (AvatarData.pInstanceInfo != null)
		{
			list = AvatarData.pInstanceInfo.GetPartsInventoryIds();
		}
		if (list.Count == 0)
		{
			return;
		}
		for (int i = 0; i < _EquippedSlots.Length; i++)
		{
			if (_EquippedSlots[i]._WidgetSlot == null)
			{
				GameObject gameObject = new GameObject();
				gameObject.transform.parent = base.transform;
				_EquippedSlots[i]._WidgetSlot = gameObject.AddComponent<KAWidget>();
				gameObject.SetActive(value: false);
			}
			UpdateEquippedSlotsInfo(_EquippedSlots[i]._PartType, _EquippedSlots[i]);
			if (_EquippedSlots[i].pEquippedItemData == null && AvatarData.IsDefaultSaved(_EquippedSlots[i]._PartType))
			{
				UpdateEquippedSlotsInfo("DEFAULT_" + _EquippedSlots[i]._PartType, _EquippedSlots[i]);
			}
		}
	}

	private void UpdateEquippedSlotsInfo(string avatarPartName, EquippedSlotWidget equipSlotwidget)
	{
		AvatarDataPart avatarDataPart = AvatarData.FindPart(avatarPartName);
		if (avatarDataPart == null || (avatarDataPart.Textures.Length == 0 && !avatarDataPart.UserInventoryId.HasValue))
		{
			return;
		}
		int categoryID = AvatarData.GetCategoryID(equipSlotwidget._PartType);
		UserItemData userItemData = null;
		if (CommonInventoryData.pIsReady)
		{
			if (!avatarDataPart.UserInventoryId.HasValue || avatarDataPart.UserInventoryId.Value <= 0)
			{
				userItemData = CommonInventoryData.pInstance.GetUserItemDataFromGeometryAndTexture(avatarDataPart.Geometries, avatarDataPart.Textures, categoryID);
				if (userItemData != null)
				{
					avatarDataPart.UserInventoryId = userItemData.UserInventoryID;
				}
			}
			else
			{
				userItemData = CommonInventoryData.pInstance.FindItemByUserInventoryID(avatarDataPart.UserInventoryId.Value);
			}
			if (userItemData != null)
			{
				equipSlotwidget.pEquippedItemData = userItemData.Item;
				equipSlotwidget.pEquippedUserItemData = userItemData;
				equipSlotwidget.pPartUiid = userItemData.UserInventoryID;
			}
		}
		if (userItemData != null)
		{
			KAUISelectItemData userData = new KAUISelectItemData(null, userItemData, 90, 1);
			equipSlotwidget._WidgetSlot.SetUserData(userData);
		}
	}

	public void OpenStatsDB(KAWidget inWidget, EquippedSlotWidget[] equippedSlots, GameObject messageObject, bool showDBRight = false, bool showEquip = false, bool CanUnequip = false, bool showSellBtn = false, bool showDiscardBtn = false, string partType = "")
	{
		EquippedSlotWidget equippedSlotWidget = null;
		KAUISelectItemData selectedItemData = (KAUISelectItemData)inWidget.GetUserData();
		if (selectedItemData != null && selectedItemData._ItemData != null)
		{
			partType = ((partType == string.Empty) ? AvatarData.pInstanceInfo.PartType(selectedItemData._ItemData) : partType);
			equippedSlotWidget = Array.Find(equippedSlots, (EquippedSlotWidget x) => x._PartType == partType && x.pEquippedUserItemData != null && x.pEquippedUserItemData.UserInventoryID.Equals(selectedItemData._UserInventoryID));
		}
		else
		{
			equippedSlotWidget = Array.Find(equippedSlots, (EquippedSlotWidget x) => x._PartType == partType);
		}
		ItemCompareDetails itemCompareDetails = null;
		ItemCompareDetails itemCompareDetails2 = null;
		DBInfoDetails dBInfoDetails = new DBInfoDetails();
		if (equippedSlotWidget == null)
		{
			if (selectedItemData._ItemData == null || selectedItemData._ItemData.AssetName == null)
			{
				return;
			}
			if (string.IsNullOrEmpty(partType))
			{
				partType = AvatarData.pInstanceInfo.PartType(selectedItemData._ItemData);
			}
			EquippedSlotWidget equippedSlotWidget2 = Array.Find(equippedSlots, (EquippedSlotWidget x) => x._PartType == partType);
			if (equippedSlotWidget2 != null && equippedSlotWidget2.pEquippedItemData != null)
			{
				itemCompareDetails = new ItemCompareDetails();
				itemCompareDetails._ItemData = equippedSlotWidget2.pEquippedItemData;
				itemCompareDetails._ItemTexture = equippedSlotWidget2._WidgetSlot.GetTexture();
				itemCompareDetails._ItemStats = GetItemStatsFromUserItemData(equippedSlotWidget2.pEquippedUserItemData);
				itemCompareDetails._Itemtier = GetItemTierFromUserItemData(equippedSlotWidget2.pEquippedUserItemData);
				if (selectedItemData.pEquippedItem)
				{
					dBInfoDetails._ShowEquip = false;
					dBInfoDetails._CanUnequip = CanUnequip;
					dBInfoDetails._ShowSellBtn = false;
				}
				else
				{
					itemCompareDetails2 = new ItemCompareDetails();
					itemCompareDetails2._ItemData = selectedItemData._ItemData;
					itemCompareDetails2._ItemTexture = ((selectedItemData._IconTexture == null) ? equippedSlotWidget2._DefaultTexture : selectedItemData._IconTexture);
					if (selectedItemData._UserItemData != null)
					{
						itemCompareDetails2._ItemStats = selectedItemData._UserItemData.ItemStats;
						itemCompareDetails2._Itemtier = selectedItemData._UserItemData.ItemTier;
					}
					dBInfoDetails._ShowEquip = showEquip;
					dBInfoDetails._CanUnequip = CanUnequip;
					dBInfoDetails._ShowDBRightSide = showDBRight;
					dBInfoDetails._ShowSellBtn = showSellBtn;
					dBInfoDetails._ShowDiscardBtn = showDiscardBtn;
				}
			}
			else
			{
				itemCompareDetails = new ItemCompareDetails();
				itemCompareDetails._ItemData = selectedItemData._ItemData;
				itemCompareDetails._ItemTexture = inWidget.GetTexture();
				itemCompareDetails._ItemStats = GetItemStatsFromUserItemData(selectedItemData._UserItemData);
				itemCompareDetails._Itemtier = GetItemTierFromUserItemData(selectedItemData._UserItemData);
				dBInfoDetails._ShowEquip = showEquip;
				dBInfoDetails._CanUnequip = false;
				dBInfoDetails._ShowDBRightSide = showDBRight;
				dBInfoDetails._ShowSellBtn = showSellBtn;
				dBInfoDetails._ShowDiscardBtn = showDiscardBtn;
			}
		}
		else if (equippedSlotWidget.pEquippedItemData != null && equippedSlotWidget.pEquippedItemData.AssetName != null && equippedSlotWidget.pEquippedItemData.AssetName != "NULL")
		{
			itemCompareDetails = new ItemCompareDetails();
			itemCompareDetails._ItemData = equippedSlotWidget.pEquippedItemData;
			itemCompareDetails._ItemTexture = inWidget.GetTexture();
			itemCompareDetails._ItemStats = GetItemStatsFromUserItemData(equippedSlotWidget.pEquippedUserItemData);
			itemCompareDetails._Itemtier = GetItemTierFromUserItemData(equippedSlotWidget.pEquippedUserItemData);
			dBInfoDetails._ShowEquip = false;
			dBInfoDetails._CanUnequip = CanUnequip;
			dBInfoDetails._ShowDBRightSide = true;
		}
		if (itemCompareDetails != null)
		{
			Initialize(itemCompareDetails, itemCompareDetails2, messageObject, dBInfoDetails, inWidget);
		}
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
