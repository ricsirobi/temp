using System;
using System.Collections.Generic;
using UnityEngine;

public class UiMessageGifts : KAUI
{
	public class GiftItemData : KAWidgetUserData
	{
		private ItemTextureResData mItemTextureData = new ItemTextureResData();

		public ItemData _ItemData;

		public int _Quantity;

		private int mItemID;

		private Action<KAWidget, ItemData> mCallback;

		private Action<KAWidget> mFailCallback;

		public void Init(KAWidget widget, int itemID, int quantity, Action<KAWidget, ItemData> successCallback, Action<KAWidget> failCallback = null)
		{
			mItemID = itemID;
			_Quantity = ((quantity == 0) ? 1 : quantity);
			_Item = widget;
			mCallback = successCallback;
			mFailCallback = failCallback;
		}

		public void LoadData()
		{
			ItemData.Load(mItemID, OnLoadItemDataReady, null);
		}

		public void OnLoadItemDataReady(int itemID, ItemData dataItem, object inUserData)
		{
			if (dataItem != null)
			{
				_ItemData = dataItem;
				if (!ValidateGender(_ItemData))
				{
					mFailCallback?.Invoke(_Item);
					return;
				}
				mItemTextureData.Init(ItemTextureEventHandler, _ItemData.IconName);
				mItemTextureData.LoadData();
				_Item.SetText(_ItemData.ItemName);
				mCallback?.Invoke(_Item, _ItemData);
			}
		}

		private bool ValidateGender(ItemData item)
		{
			string text = "U";
			if (AvatarData.GetGender() == Gender.Male)
			{
				text = "M";
			}
			else if (AvatarData.GetGender() == Gender.Female)
			{
				text = "F";
			}
			string attribute = item.GetAttribute("Gender", "U");
			if (!(attribute == text))
			{
				return attribute == "U";
			}
			return true;
		}

		private void ItemTextureEventHandler(ItemResNameData resData)
		{
			if (resData is ItemTextureResData itemTextureResData)
			{
				_Item.FindChildItem("BkgIcon").SetTexture(itemTextureResData._Texture);
			}
		}
	}

	public InventoryTab _BattleReadyTab;

	public LocaleString _InventorySpaceText = new LocaleString("# slot(s) are needed.Buy more slots ? ");

	public LocaleString _CoinsText = new LocaleString("[Review] Coins");

	public LocaleString _GemsText = new LocaleString("[Review] Gems");

	[SerializeField]
	private KAWidget m_GiftTemplate;

	[SerializeField]
	private Texture m_IconTextureCoin;

	[SerializeField]
	private Texture m_IconTextureGems;

	private KAUIMenu mGiftMenu;

	private GiftData mGiftData;

	private KAWidget mInventoryFull;

	private KAWidget mBuySlotBtn;

	private KAWidget mClaimBtn;

	private KAWidget mCloseBtn;

	private KAWidget mMessageItem;

	private UiMessageInfoUserData mMessageInfoData;

	private string mGiftName = string.Empty;

	private int mItemsReady;

	private int mEmptySlotPrice = 10;

	private int mSlotsPurchaseCount = 1;

	private int mInvalidItems;

	private bool mInitialized;

	public static UiMessageGifts pInstance { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		pInstance = this;
	}

	public void Initialize(UiMessageInfoUserData messageInfoUserData, KAWidget messageItem)
	{
		KAUI.SetExclusive(this);
		mGiftMenu = _MenuList[0];
		mMessageItem = messageItem;
		mItemsReady = 0;
		mInvalidItems = 0;
		mInitialized = false;
		mInventoryFull = FindItem("InventoryFull");
		mInventoryFull.SetVisibility(inVisible: false);
		mBuySlotBtn = FindItem("BuySlotsBtn");
		mClaimBtn = FindItem("ClaimBtn");
		mCloseBtn = FindItem("CloseBtn");
		SetBattleReadyTabData();
		mMessageInfoData = messageInfoUserData;
		SetTaggedMessage();
		SetGiftData();
		SetGiftMenu();
		SetVisibility(inVisible: true);
	}

	protected override void Update()
	{
		base.Update();
		if (mGiftData != null && mItemsReady == mGiftData.GiftSetup.Count && !mInitialized)
		{
			mInitialized = true;
			CheckForAvailableSlots();
		}
		if (mGiftData != null && mInvalidItems == mGiftData.GiftSetup.Count)
		{
			mInvalidItems = 0;
			OnClose();
		}
	}

	private void SetBattleReadyTabData()
	{
		InventoryTab battleReadyTab = _BattleReadyTab;
		battleReadyTab.UpdateFromInventorySetting();
		ItemData.Load(battleReadyTab.pTabData._SlotItemID, OnSlotItemLoaded, null);
	}

	private void OnSlotItemLoaded(int itemID, ItemData dataItem, object inUserData)
	{
		if (dataItem != null)
		{
			mEmptySlotPrice = dataItem.GetFinalCost();
		}
	}

	private void CheckForAvailableSlots()
	{
		int num = 0;
		foreach (KAWidget item in mGiftMenu.GetItems())
		{
			if (item.GetUserData() != null)
			{
				GiftItemData giftItemData = (GiftItemData)item.GetUserData();
				ItemData itemData = giftItemData._ItemData;
				if (itemData != null && itemData.ItemStatsMap != null && itemData.ItemStatsMap.ItemStats != null && itemData.ItemStatsMap.ItemStats.Length != 0)
				{
					num += giftItemData._Quantity;
				}
			}
		}
		int num2 = _BattleReadyTab.pTabData.GetTotalSlots() - _BattleReadyTab.pTabData.GetOccupiedSlots();
		if (num == 0 || num2 >= num)
		{
			mInventoryFull.SetVisibility(inVisible: false);
		}
		else
		{
			mSlotsPurchaseCount = num - num2;
			KAWidget kAWidget = mInventoryFull.FindChildItem("TxtSpaceNeeded");
			string localizedString = _InventorySpaceText.GetLocalizedString();
			localizedString = localizedString.Replace("#", mSlotsPurchaseCount.ToString());
			kAWidget.SetText(localizedString);
			int num3 = mSlotsPurchaseCount * mEmptySlotPrice;
			mBuySlotBtn.SetText(num3.ToString());
			mInventoryFull.SetVisibility(inVisible: true);
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
		SetState(KAUIState.INTERACTIVE);
	}

	private void SetTaggedMessage()
	{
		string data = mMessageInfoData.GetMessageInfo().Data;
		if (!string.IsNullOrEmpty(data))
		{
			Dictionary<string, string> dictionary = TaggedMessageHelper.Match(data);
			if (dictionary.ContainsKey("name"))
			{
				mGiftName = dictionary["name"];
			}
		}
	}

	private void SetGiftData()
	{
		mGiftData = GiftManager.pInstance.GetGiftDataByName(mGiftName);
	}

	private void SetGiftMenu()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		SetState(KAUIState.NOT_INTERACTIVE);
		foreach (GiftSetup item in mGiftData.GiftSetup)
		{
			int num = UtStringUtil.Parse(item.Value, -1);
			if (item.Type == GiftType.Item && num != -1)
			{
				AddWidgetItem(item);
				continue;
			}
			string empty = string.Empty;
			Texture icon;
			if (item.Type == GiftType.Gems)
			{
				icon = m_IconTextureGems;
				empty = _GemsText.GetLocalizedString();
			}
			else
			{
				icon = m_IconTextureCoin;
				empty = _CoinsText.GetLocalizedString();
			}
			AddWidgetItem(icon, item.Quantity, empty);
			mItemsReady++;
		}
	}

	private void AddWidgetItem(GiftSetup gift)
	{
		int itemID = UtStringUtil.Parse(gift.Value, -1);
		KAWidget kAWidget = mGiftMenu.AddWidget(m_GiftTemplate.name);
		kAWidget.SetVisibility(inVisible: false);
		GiftItemData giftItemData = new GiftItemData();
		giftItemData.Init(kAWidget, itemID, gift.Quantity, OnItemReady, OnItemFail);
		giftItemData.LoadData();
		kAWidget.SetUserData(giftItemData);
		KAWidget kAWidget2 = kAWidget.FindChildItem("TxtQuanity");
		if (gift.Quantity > 0)
		{
			kAWidget2.SetText(gift.Quantity.ToString());
		}
		else
		{
			kAWidget2.SetVisibility(inVisible: false);
		}
	}

	private void AddWidgetItem(Texture icon, int quantity, string name)
	{
		KAWidget kAWidget = mGiftMenu.AddWidget(name);
		kAWidget.SetText(name);
		KAWidget kAWidget2 = kAWidget.FindChildItem("BkgIcon");
		if (kAWidget2 != null)
		{
			kAWidget2.SetTexture(icon);
		}
		kAWidget.FindChildItem("TxtQuanity").SetText(quantity.ToString());
		kAWidget.FindChildItem("TxtOwned").SetVisibility(inVisible: false);
		kAWidget.FindChildItem("CellBkg").SetVisibility(inVisible: false);
		kAWidget.SetVisibility(inVisible: true);
		mGiftMenu.AddWidget(kAWidget);
	}

	private void OnItemReady(KAWidget widget, ItemData item)
	{
		widget.SetVisibility(inVisible: true);
		UpdateWidget(widget, item);
		MarkItemOwned(widget, item.ItemID);
		mGiftMenu.AddWidget(widget);
		mItemsReady++;
	}

	private void OnItemFail(KAWidget widget)
	{
		mGiftMenu.RemoveWidget(widget);
		mItemsReady++;
		mInvalidItems++;
	}

	private void MarkItemOwned(KAWidget widget, int itemID)
	{
		bool flag = false;
		UserItemData userItemData = null;
		if (CommonInventoryData.pIsReady)
		{
			userItemData = CommonInventoryData.pInstance.FindItem(itemID);
			if (userItemData != null && userItemData.Quantity == userItemData.Item.InventoryMax)
			{
				flag = true;
			}
		}
		if (!flag && ParentData.pIsReady)
		{
			userItemData = ParentData.pInstance.pInventory.pData.FindItem(itemID);
			if (userItemData != null && userItemData.Quantity == userItemData.Item.InventoryMax)
			{
				flag = true;
			}
		}
		widget.FindChildItem("TxtOwned").SetVisibility(flag);
	}

	public void UpdateWidget(KAWidget widget, ItemData item)
	{
		bool flag = item.ItemStatsMap != null && item.ItemStatsMap.ItemStats != null && item.ItemStatsMap.ItemStats.Length != 0;
		widget.name = item.ItemName;
		ShowBattleReadyIcon(widget, flag);
		ShowFlightReadyIcon(widget, item.HasAttribute("FlightSuit"));
		UpdateWidgetBackground(widget, item, flag);
	}

	private void ShowBattleReadyIcon(KAWidget widget, bool show)
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
		if (widget != null && isBattleReady)
		{
			widget.FindChildItem("CellBkg").SetVisibility(inVisible: true);
			UiItemRarityColorSet.SetItemBackgroundColor((itemData == null || !itemData.ItemRarity.HasValue) ? ItemRarity.Common : itemData.ItemRarity.Value, widget);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mClaimBtn)
		{
			ClaimGifts();
		}
		else if (inWidget == mCloseBtn)
		{
			OnClose();
		}
		else if (inWidget == mBuySlotBtn)
		{
			_BattleReadyTab.pTabData.BuySlot(base.gameObject, mSlotsPurchaseCount, 0);
		}
	}

	private void ClaimGifts()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		SetState(KAUIState.NOT_INTERACTIVE);
		WsWebService.SetAchievementAndGetReward(mGiftData.AchievementID, "", ServiceEventHandler, false);
	}

	public void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			SetState(KAUIState.INTERACTIVE);
			if (inObject != null)
			{
				AchievementReward[] array = (AchievementReward[])inObject;
				if (array != null)
				{
					GameUtilities.AddRewards(array, inUseRewardManager: false, inImmediateShow: false);
				}
			}
			KAWidget kAWidget = mMessageItem.FindChildItem("OpenBtn");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
			}
			if (GiftManager.pIsReady)
			{
				GiftManager.pInstance.UpdateMessageTag(mGiftData, "Claim");
			}
			OnClose();
			break;
		}
		case WsServiceEvent.ERROR:
			SetState(KAUIState.INTERACTIVE);
			KAUICursorManager.SetDefaultCursor("Arrow");
			UtDebug.LogError("Unable to process Gift with Achievement ID :: " + mGiftData.AchievementID);
			OnClose();
			break;
		}
	}

	private void OnItemPurchaseComplete()
	{
		mInventoryFull.SetVisibility(inVisible: false);
	}

	private void OnClose()
	{
		SetVisibility(inVisible: false);
		KAUI.RemoveExclusive(this);
		mGiftMenu.ClearItems();
	}

	protected override void OnDestroy()
	{
		pInstance = null;
		base.OnDestroy();
	}
}
