using System;
using UnityEngine;

public class UiItemTradeGenericDB : KAUIGenericDB
{
	public enum TradeType
	{
		Buy,
		Sell,
		Trash,
		None
	}

	[Serializable]
	public class TradeTypeInfo
	{
		public TradeType _Type;

		public LocaleString _FailToProceedText;

		public LocaleString _ConfirmText;

		public LocaleString _SuccessText;

		public LocaleString _FailText;
	}

	public LocaleString _ItemFailedToLoadText = new LocaleString("[REVIEW] Failed to load item information");

	public LocaleString _NotEnoughGemsText = new LocaleString("[REVIEW] You do not have enough gems to pay, Do you want to purchase ?");

	public LocaleString _NotEnoughCoinsText = new LocaleString("[REVIEW] You do not have enough coins to pay, Please buy more!");

	public LocaleString _ConfirmDBTitleText = new LocaleString("[REVIEW] Confirmation");

	public LocaleString _CurrentQuantityStatus = new LocaleString("[REVIEW] Current");

	public TradeTypeInfo[] _TradeTypeInfo;

	protected int mItemQuantity = 1;

	protected ItemData mItemData;

	protected bool mFixedQuantity;

	private KAWidget mBtnBuy;

	private KAWidget mBtnTrash;

	private KAWidget mBtnSell;

	private KAWidget mItemName;

	private KAWidget mItemDescription;

	private KAWidget mQuantityPlus;

	private KAWidget mGemsIcon;

	private KAWidget mCoinsIcon;

	private KAWidget mTotalPrice;

	private KAWidget mQuantityMinus;

	private KAEditBox mQuantity;

	private int mItemID;

	private int mStoreID;

	private int mInventoryQuantity;

	private int mGemsToCoinsFactor;

	private TradeType mTradeType = TradeType.None;

	private string mPurchaseStoreName;

	protected virtual void UpdateUI()
	{
		KAWidget kAWidget = FindItem("Quantity");
		mQuantity = (KAEditBox)kAWidget.FindChildItem("TxtQuantityValue");
		mQuantityMinus = kAWidget.FindChildItem("BtnNegative");
		mQuantityPlus = kAWidget.FindChildItem("BtnPositive");
		KAWidget kAWidget2 = FindItem("TotalPrice");
		mCoinsIcon = kAWidget2.FindChildItem("TotalPriceCoins");
		mGemsIcon = kAWidget2.FindChildItem("TotalPriceGems");
		mTotalPrice = kAWidget2.FindChildItem("ItemCostTotalPrice");
		mItemName = FindItem("ItemName");
		if (mItemName != null)
		{
			mItemName.SetText(mItemData.ItemName);
		}
		mItemDescription = FindItem("ItemDescription");
		if (mItemDescription != null)
		{
			mItemDescription.SetText(mItemData.Description);
		}
		switch (mTradeType)
		{
		case TradeType.Buy:
			mBtnBuy = FindItem("BtnBuy");
			if (mBtnBuy != null)
			{
				mBtnBuy.SetVisibility(inVisible: true);
			}
			if (mItemData.GetPurchaseType() == 2 && mGemsIcon != null)
			{
				mGemsIcon.SetVisibility(inVisible: true);
			}
			else if (mItemData.GetPurchaseType() == 1 && mCoinsIcon != null)
			{
				mCoinsIcon.SetVisibility(inVisible: true);
			}
			break;
		case TradeType.Sell:
			mBtnSell = FindItem("BtnSell");
			if (mBtnSell != null)
			{
				mBtnSell.SetVisibility(inVisible: true);
			}
			if (mCoinsIcon != null)
			{
				mCoinsIcon.SetVisibility(inVisible: true);
			}
			break;
		case TradeType.Trash:
			mBtnTrash = FindItem("BtnTrash");
			if (mBtnTrash != null)
			{
				mBtnTrash.SetVisibility(inVisible: true);
			}
			if (kAWidget2 != null)
			{
				kAWidget2.SetVisibility(inVisible: false);
			}
			break;
		}
	}

	public void SetMode(TradeType type, int itemID, int storeID = 0, string storeName = null)
	{
		mTradeType = type;
		UserItemData userItemData = CommonInventoryData.pInstance.FindItem(itemID);
		if (userItemData != null)
		{
			mInventoryQuantity = userItemData.Quantity;
			if (type == TradeType.Buy && mInventoryQuantity == userItemData.Item.InventoryMax)
			{
				ShowFailedToProceed();
				return;
			}
		}
		if ((type == TradeType.Sell || type == TradeType.Trash) && mInventoryQuantity == 0)
		{
			ShowFailedToProceed();
			return;
		}
		mItemID = itemID;
		mStoreID = storeID;
		mPurchaseStoreName = storeName;
		SetExclusive();
		ItemData.Load(itemID, OnItemLoaded, null);
	}

	private void OnItemLoaded(int itemID, ItemData itemData, object userData)
	{
		KAUICursorManager.SetExclusiveLoadingGear(status: false);
		if (itemData != null)
		{
			if (InventorySetting.pInstance != null)
			{
				mGemsToCoinsFactor = InventorySetting.pInstance._GemsToCoinsFactor;
			}
			mItemData = itemData;
			UpdateUI();
			RefreshQuantity();
			SetVisibility(inVisible: true);
			KAUI.SetExclusive(this);
		}
		else
		{
			DestroyDB(_ItemFailedToLoadText);
		}
	}

	public void SetFixedQuantity(int quantity, bool showArrows = false)
	{
		mFixedQuantity = true;
		if (mQuantity != null)
		{
			mQuantity.SetText(quantity.ToString());
			mQuantity.SetDisabled(isDisabled: true);
		}
		mQuantityMinus.gameObject.SetActive(showArrows);
		mQuantityPlus.gameObject.SetActive(showArrows);
	}

	public override void OnClick(KAWidget widget)
	{
		base.OnClick(widget);
		if (widget.name == _BackButtonName)
		{
			DestroyDB(null);
		}
		else if (widget == mBtnBuy)
		{
			if (mItemData.GetPurchaseType() == 2 && Money.pCashCurrency < mItemData.GetFinalCost() * mItemQuantity)
			{
				ShowConfirmationDB(_NotEnoughGemsText, "OnBuyGems");
			}
			else if (mItemData.GetPurchaseType() == 1 && Money.pGameCurrency < mItemData.GetFinalCost() * mItemQuantity)
			{
				DestroyDB(_NotEnoughCoinsText);
			}
			else
			{
				ShowTradeConfirmationDB();
			}
		}
		else if (widget == mBtnSell || widget == mBtnTrash)
		{
			ShowTradeConfirmationDB();
		}
		else if (widget == mQuantityPlus)
		{
			mItemQuantity++;
			RefreshQuantity();
		}
		else if (widget == mQuantityMinus && mItemQuantity > 1)
		{
			mItemQuantity--;
			RefreshQuantity();
		}
	}

	private void ShowTradeConfirmationDB()
	{
		if (_TradeTypeInfo.Length != 0)
		{
			TradeTypeInfo tradeTypeInfo = Array.Find(_TradeTypeInfo, (TradeTypeInfo x) => x._Type == mTradeType);
			if (!string.IsNullOrEmpty(tradeTypeInfo._ConfirmText._Text))
			{
				ShowConfirmationDB(tradeTypeInfo._ConfirmText, "OnConfirmTrade");
			}
			else
			{
				OnConfirmTrade();
			}
		}
	}

	private void ShowConfirmationDB(LocaleString message, string callbackMessage)
	{
		KAUIGenericDB kAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDBSm", "ConfirmationDB");
		kAUIGenericDB.SetMessage(base.gameObject, callbackMessage, "OnCloseDB", null, null);
		kAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
		kAUIGenericDB.SetText(message.GetLocalizedString(), interactive: false);
		kAUIGenericDB.SetDestroyOnClick(isDestroy: true);
		KAUI.SetExclusive(kAUIGenericDB);
	}

	private void OnBuyGems()
	{
		SetVisibility(inVisible: false);
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
	}

	private void OnIAPStoreClosed()
	{
		SetVisibility(inVisible: true);
	}

	private void OnConfirmTrade()
	{
		SetInteractive(interactive: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		switch (mTradeType)
		{
		case TradeType.Buy:
			CommonInventoryData.pInstance.AddPurchaseItem(mItemData.ItemID, mItemQuantity, mPurchaseStoreName);
			CommonInventoryData.pInstance.DoPurchase(mItemData.GetPurchaseType(), mStoreID, PurchaseDoneCallback);
			break;
		case TradeType.Sell:
		{
			UserItemData userItemData = CommonInventoryData.pInstance.FindItem(mItemID);
			CommonInventoryData.pInstance.AddSellItem(userItemData.UserInventoryID, mItemQuantity);
			CommonInventoryData.pInstance.DoSell(ItemSoldCallback);
			break;
		}
		case TradeType.Trash:
			CommonInventoryData.pInstance.RemoveItem(mItemID, updateServer: true, mItemQuantity);
			CommonInventoryData.pInstance.Save(SaveDoneCallback, null);
			break;
		}
	}

	protected virtual void RefreshQuantity()
	{
		switch (mTradeType)
		{
		case TradeType.Buy:
		{
			int num = ((mItemData.InventoryMax > 0) ? (mItemData.InventoryMax - mInventoryQuantity) : ((int)(Mathf.Pow(10f, mQuantity.pInput.characterLimit) - 1f)));
			if (mItemQuantity > num)
			{
				mItemQuantity = num;
			}
			mQuantity.SetDisabled(num == 1);
			mQuantityPlus.SetDisabled(mItemQuantity >= num);
			int num2 = mItemQuantity * mItemData.GetFinalCost();
			mTotalPrice.SetText(num2.ToString());
			break;
		}
		case TradeType.Sell:
		{
			if (mItemQuantity > mInventoryQuantity)
			{
				mItemQuantity = mInventoryQuantity;
			}
			mQuantityPlus.SetDisabled(mItemQuantity == mInventoryQuantity);
			mQuantity.SetDisabled(mInventoryQuantity == 1);
			int num3 = ((mItemData.GetPurchaseType() == 1) ? mItemData.Cost : (mItemData.CashCost * mGemsToCoinsFactor)) * mItemData.SaleFactor / 100;
			int num4 = mItemQuantity * num3;
			mTotalPrice.SetText(num4.ToString());
			break;
		}
		case TradeType.Trash:
			if (mItemQuantity > mInventoryQuantity)
			{
				mItemQuantity = mInventoryQuantity;
			}
			mQuantity.SetDisabled(mInventoryQuantity == 1);
			mQuantityPlus.SetDisabled(mItemQuantity == mInventoryQuantity);
			break;
		}
		mQuantityMinus.SetDisabled(mItemQuantity <= 1);
		if (!mFixedQuantity)
		{
			mQuantity.SetText(mItemQuantity.ToString());
		}
		else
		{
			mQuantity.SetDisabled(isDisabled: true);
		}
	}

	private void PurchaseDoneCallback(CommonInventoryResponse response)
	{
		ShowStatus(response.Success);
		if (response.Success && _MessageObject != null)
		{
			_MessageObject.SendMessage("OnItemPurchaseComplete");
		}
	}

	private void SaveDoneCallback(bool success, object userData)
	{
		ShowStatus(success);
		if (success)
		{
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage("OnItemTrashComplete");
			}
		}
		else
		{
			CommonInventoryData.pInstance.AddItem(mItemID, mItemQuantity);
		}
	}

	private void ItemSoldCallback(bool isSuccess, CommonInventoryResponse ret)
	{
		ShowStatus(isSuccess);
		if (_MessageObject != null)
		{
			_MessageObject.SendMessage("OnItemSellComplete", ret);
		}
	}

	private void ShowStatus(bool success = false)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (_TradeTypeInfo.Length != 0)
		{
			TradeTypeInfo tradeTypeInfo = Array.Find(_TradeTypeInfo, (TradeTypeInfo x) => x._Type == mTradeType);
			if (success)
			{
				DestroyDB(tradeTypeInfo._SuccessText);
				return;
			}
			SetInteractive(interactive: true);
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", tradeTypeInfo._FailText.GetLocalizedString(), null, "");
		}
	}

	private void ShowFailedToProceed()
	{
		if (_TradeTypeInfo.Length != 0)
		{
			TradeTypeInfo tradeTypeInfo = Array.Find(_TradeTypeInfo, (TradeTypeInfo x) => x._Type == mTradeType);
			if (tradeTypeInfo != null)
			{
				DestroyDB(tradeTypeInfo._FailToProceedText);
			}
		}
	}

	private void DestroyDB(LocaleString closeMessage)
	{
		if (closeMessage != null)
		{
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", closeMessage.GetLocalizedString(), null, "");
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		RemoveExclusive();
	}

	public override void OnSelect(KAWidget widget, bool inSelected)
	{
		base.OnSelect(widget, inSelected);
		if (widget == mQuantity && !inSelected)
		{
			if (string.IsNullOrEmpty(mQuantity.GetText()))
			{
				mItemQuantity = 1;
			}
			RefreshQuantity();
		}
	}

	public override void OnInput(KAWidget widget, string text)
	{
		base.OnInput(widget, text);
		if (widget == mQuantity && !string.IsNullOrEmpty(mQuantity.GetText()))
		{
			mItemQuantity = int.Parse(mQuantity.GetText());
			RefreshQuantity();
		}
	}
}
