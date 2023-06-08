using System;
using System.Collections.Generic;
using UnityEngine;

public class UiPowerupSelect : KAUI
{
	[Serializable]
	public class WidgetMapping
	{
		public string _Index;

		public int _ItemID;

		[HideInInspector]
		public KAWidget _Widget;
	}

	public class PowerupData : KAWidgetUserData
	{
		public ItemData _ItemData;

		public int _Quantity;
	}

	public int _StoreID = 92;

	public string _GameName = "ArenaFrenzy";

	public List<WidgetMapping> _WidgetMappingList;

	public KAWidget _ParentWidget;

	public GameObject _MessageObject;

	public LocaleString _NoEnoughGemsText = new LocaleString("No Enough Gems. Buy more gems!");

	public LocaleString _NoEnoughGemsTitleText = new LocaleString("Gems Insufficient");

	public LocaleString _PurchaseSuccessText = new LocaleString("Purchased you powerups!");

	public LocaleString _PurchaseSuccessTitleText = new LocaleString("Purchase Success");

	public LocaleString _PurchaseFailedText = new LocaleString("Sorry, your purchase failed!");

	public LocaleString _PurchaseFailedTitleText = new LocaleString("Purchase Failed");

	private KAWidget mBuyBtn;

	private KAWidget mCloseBtn;

	private KAWidget mTxtTotalCostAmount;

	private KAUIGenericDB mKAUiGenericDB;

	private int mTotalCost;

	protected override void Start()
	{
		base.Start();
		mTxtTotalCostAmount = FindItem("TxtTotalPriceAmt");
		mBuyBtn = FindItem("BuyBtn");
		mCloseBtn = FindItem("BtnClose");
		ItemStoreDataLoader.Load(_StoreID, OnStoreLoaded);
	}

	public void OnStoreLoaded(StoreData sd)
	{
		ConsumableGame consumableGameData = ConsumableData.GetConsumableGameData(_GameName);
		if (consumableGameData == null)
		{
			return;
		}
		ConsumableType[] consumableTypes = consumableGameData.ConsumableTypes;
		for (int i = 0; i < consumableTypes.Length; i++)
		{
			Consumable[] consumables = consumableTypes[i].Consumables;
			foreach (Consumable consumable in consumables)
			{
				WidgetMapping inUserData = _WidgetMappingList.Find((WidgetMapping data) => data._ItemID == consumable.ItemID);
				if (sd == null)
				{
					continue;
				}
				ItemData[] items = sd._Items;
				foreach (ItemData itemData in items)
				{
					if (itemData.ItemID == consumable.ItemID)
					{
						OnItemDataReady(consumable.ItemID, itemData, inUserData);
						break;
					}
				}
			}
		}
	}

	public override void SetVisibility(bool visibility)
	{
		base.SetVisibility(visibility);
		if (visibility)
		{
			UpdateInventoryCount();
		}
	}

	public void OnItemDataReady(int itemID, ItemData dataItem, object inUserData)
	{
		WidgetMapping widgetMapping = (WidgetMapping)inUserData;
		KAWidget kAWidget = (widgetMapping._Widget = _ParentWidget.FindChildItem("PowerUp" + widgetMapping._Index));
		PowerupData powerupData = new PowerupData();
		powerupData._ItemData = dataItem;
		powerupData._Quantity = 0;
		powerupData._Item = kAWidget;
		if (kAWidget != null)
		{
			kAWidget.SetUserData(powerupData);
			UpdateCost(powerupData, widgetMapping._Index);
			UpdateInventoryCount(powerupData, widgetMapping._Index);
			UpdateQuanity(powerupData, widgetMapping._Index, add: false);
			UpdateTooltip(powerupData, widgetMapping._Index);
		}
		ItemStoreDataLoader.GetItem(itemID);
	}

	private void UpdateTooltip(PowerupData widgetData, string index)
	{
		if (widgetData == null || !(widgetData._Item != null) || widgetData._ItemData == null)
		{
			return;
		}
		string text = widgetData._ItemData.Description;
		KAWidget kAWidget = widgetData._Item.FindChildItem("IcoPowerUp" + index);
		if (text != null)
		{
			if (text.Contains("]"))
			{
				text = widgetData._ItemData.Description.Split(']')[1].Trim();
			}
			kAWidget.SetToolTipText(text);
		}
	}

	private void UpdateCost(PowerupData widgetData, string index)
	{
		widgetData._Item.FindChildItem("TxtCostPowerUp" + index).SetText(widgetData._ItemData.FinalCashCost.ToString());
	}

	private void UpdateInventoryCount(PowerupData widgetData, string index)
	{
		if (CommonInventoryData.pInstance != null)
		{
			KAWidget kAWidget = widgetData._Item.FindChildItem("TxtAvailablePowerUp" + index);
			if (widgetData._ItemData.ItemID > 0)
			{
				kAWidget.SetText(CommonInventoryData.pInstance.GetQuantity(widgetData._ItemData.ItemID).ToString());
			}
		}
	}

	private void UpdateQuanity(PowerupData widgetData, string index, bool add)
	{
		int quantity = widgetData._Quantity;
		if (add)
		{
			widgetData._Quantity++;
		}
		else if (widgetData._Quantity > 0)
		{
			widgetData._Quantity--;
		}
		widgetData._Item.FindChildItem("TxtQuantityPowerUp" + index).SetText(widgetData._Quantity.ToString());
		int amount = (widgetData._Quantity - quantity) * widgetData._ItemData.FinalCashCost;
		UpdateTotalCost(amount);
	}

	private void UpdateTotalCost(int amount)
	{
		mTotalCost += amount;
		mTxtTotalCostAmount.SetText(mTotalCost.ToString());
		mBuyBtn.SetInteractive(mTotalCost > 0);
	}

	private void Reset()
	{
		mTotalCost = 0;
		foreach (WidgetMapping widgetMapping in _WidgetMappingList)
		{
			PowerupData powerupData = (PowerupData)widgetMapping._Widget.GetUserData();
			powerupData._Quantity = 0;
			widgetMapping._Widget.SetUserData(powerupData);
			UpdateQuanity(powerupData, widgetMapping._Index, add: false);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		KAWidget parentWidget = inWidget.GetParentItem();
		WidgetMapping widgetMapping = _WidgetMappingList.Find((WidgetMapping data) => data._Widget == parentWidget);
		if (widgetMapping != null)
		{
			PowerupData widgetData = (PowerupData)parentWidget.GetUserData();
			if (inWidget.name.Contains("BtnNegQuantity"))
			{
				UpdateQuanity(widgetData, widgetMapping._Index, add: false);
			}
			else if (inWidget.name.Contains("BtnPosQuantity"))
			{
				UpdateQuanity(widgetData, widgetMapping._Index, add: true);
			}
		}
		else if (inWidget == mBuyBtn)
		{
			SetInteractive(interactive: false);
			PurchasePowerups();
		}
		else if (inWidget == mCloseBtn)
		{
			CloseUI();
		}
	}

	private bool CanBuy()
	{
		return mTotalCost <= Money.pCashCurrency;
	}

	private void PurchasePowerups()
	{
		if (CanBuy())
		{
			foreach (WidgetMapping widgetMapping in _WidgetMappingList)
			{
				if (widgetMapping._Widget != null)
				{
					PowerupData powerupData = (PowerupData)widgetMapping._Widget.GetUserData();
					CommonInventoryData.pInstance.AddPurchaseItem(powerupData._ItemData.ItemID, powerupData._Quantity, ItemPurchaseSource.UI_POWER_UP.ToString());
				}
			}
			KAUICursorManager.SetDefaultCursor("Loading");
			CommonInventoryData.pInstance.DoPurchase(2, _StoreID, OnPurchaseDone);
		}
		else
		{
			ShowGenericDB("NotEnoughGemsDB", "OnNotEnoughGems", _NoEnoughGemsText, _NoEnoughGemsTitleText);
		}
	}

	private void OnPurchaseDone(CommonInventoryResponse response)
	{
		if (response != null && response.Success)
		{
			ShowGenericDB("SucessDB", "OnPurchaseSuccess", _PurchaseSuccessText, _PurchaseSuccessTitleText);
		}
		else
		{
			ShowGenericDB("SucessDB", "OnPurchaseFailed", _PurchaseFailedText, _PurchaseFailedTitleText);
		}
		SetInteractive(interactive: true);
		KAUICursorManager.SetDefaultCursor("Arrow");
	}

	private void ShowGenericDB(string inDBName, string okMessage, LocaleString inText, LocaleString inTitle)
	{
		if (_MessageObject != null)
		{
			_MessageObject.SendMessage("OnShowGenericDB");
		}
		if (mKAUiGenericDB != null)
		{
			DestroyDB();
		}
		mKAUiGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", inDBName);
		mKAUiGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mKAUiGenericDB._MessageObject = base.gameObject;
		mKAUiGenericDB._OKMessage = okMessage;
		mKAUiGenericDB.SetText(inText.GetLocalizedString(), interactive: false);
		mKAUiGenericDB.SetTitle(inTitle.GetLocalizedString());
		KAUI.SetExclusive(mKAUiGenericDB);
	}

	private void DestroyDB()
	{
		if (!(mKAUiGenericDB == null))
		{
			KAUI.RemoveExclusive(mKAUiGenericDB);
			UnityEngine.Object.Destroy(mKAUiGenericDB.gameObject);
			mKAUiGenericDB = null;
		}
	}

	private void OnNotEnoughGems()
	{
		if (_MessageObject != null)
		{
			_MessageObject.SendMessage("OnNotEnoughGems");
		}
		SetInteractive(interactive: true);
		DestroyDB();
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
	}

	private void OnIAPStoreClosed()
	{
		if (_MessageObject != null)
		{
			_MessageObject.SendMessage("OnIAPStoreClosed");
		}
		DestroyDB();
		if (CanBuy())
		{
			PurchasePowerups();
		}
	}

	private void OnPurchaseSuccess()
	{
		if (_MessageObject != null)
		{
			_MessageObject.SendMessage("OnPurchaseSuccess");
		}
		DestroyDB();
		Reset();
		UpdateInventoryCount();
	}

	private void OnPurchaseFailed()
	{
		DestroyDB();
		if (_MessageObject != null)
		{
			_MessageObject.SendMessage("OnPurchaseFailed");
		}
	}

	private void UpdateInventoryCount()
	{
		foreach (WidgetMapping widgetMapping in _WidgetMappingList)
		{
			if (widgetMapping._Widget != null)
			{
				UpdateInventoryCount((PowerupData)widgetMapping._Widget.GetUserData(), widgetMapping._Index);
			}
		}
	}

	private void CloseUI()
	{
		SetVisibility(visibility: false);
		if (_MessageObject != null)
		{
			_MessageObject.SendMessage("OnCloseUI");
		}
	}
}
