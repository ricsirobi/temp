using System.Collections.Generic;

public class UiMarket : KAUISelect
{
	public LocaleString _SellItemsConfirmationText = new LocaleString("[REVIEW] Are you sure you want to sell your items ?");

	public UiItemStats _UiItemsStats;

	public UiBlacksmith _UiBlackSmith;

	private KAWidget mBtnSellItems;

	private KAWidget mSellPriceTotal;

	private KAWidget mShardsTotal;

	private List<int> mItemsToSell = new List<int>();

	private int mTotalSellPrice;

	private int mGemsToCoinsFactor;

	private KAUISelectItemData mWidgetData;

	private int mTotalShardsRewarded;

	protected override void Start()
	{
		mBtnSellItems = FindItem("BtnSell");
		mSellPriceTotal = FindItem("SellPriceTotal");
		mShardsTotal = FindItem("Shards");
		if (InventorySetting.pInstance != null)
		{
			mGemsToCoinsFactor = InventorySetting.pInstance._GemsToCoinsFactor;
		}
	}

	public override void OnOpen()
	{
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		ResetMarket();
		if (inVisible)
		{
			Initialize();
		}
	}

	public override void AddWidgetData(KAWidget targetWidget, KAUISelectItemData widgetData)
	{
		if (widgetData != null && widgetData._ItemID != 0)
		{
			if (mItemsToSell.Count == 0)
			{
				mBtnSellItems.SetDisabled(isDisabled: false);
			}
			UpdateSellingPrice(GetSellingPrice(widgetData._ItemData));
			UpdateShards(GetSellingShardQuantity(widgetData._ItemData));
			mItemsToSell.Add(widgetData._UserInventoryID);
		}
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)targetWidget.GetUserData();
		if (kAUISelectItemData != null && kAUISelectItemData._ItemID != 0)
		{
			UpdateSellingPrice(-GetSellingPrice(kAUISelectItemData._ItemData));
			UpdateShards(-GetSellingShardQuantity(kAUISelectItemData._ItemData));
			mItemsToSell.Remove(kAUISelectItemData._UserInventoryID);
			if (mItemsToSell.Count == 0)
			{
				mBtnSellItems.SetDisabled(isDisabled: true);
			}
		}
		base.AddWidgetData(targetWidget, widgetData);
		bool flag = widgetData != null && widgetData._UserItemData != null && widgetData._UserItemData.pIsBattleReady;
		KAWidget kAWidget = targetWidget.FindChildItem("BattleReadyIcon");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(flag);
		}
		KAWidget kAWidget2 = targetWidget.FindChildItem("FlightReadyIcon");
		if (kAWidget2 != null)
		{
			kAWidget2.SetVisibility(widgetData != null && widgetData._ItemData != null && widgetData._ItemData.HasAttribute("FlightSuit"));
		}
		KAWidget kAWidget3 = targetWidget.FindChildItem(((UiMarketMenu)base.pKAUiSelectMenu)._ItemColorWidget);
		if (kAWidget3 != null)
		{
			if (flag)
			{
				UiItemRarityColorSet.SetItemBackgroundColor((widgetData == null || widgetData._ItemData == null || !widgetData._ItemData.ItemRarity.HasValue) ? ItemRarity.Common : widgetData._ItemData.ItemRarity.Value, kAWidget3);
			}
			else
			{
				UiItemRarityColorSet.SetItemBackgroundColor(((UiMarketMenu)base.pKAUiSelectMenu).pItemDefaultColor, kAWidget3);
			}
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnSellItems)
		{
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _SellItemsConfirmationText.GetLocalizedString(), "", base.gameObject, "OnSellItems", "OnDBClose", "", "", inDestroyOnClick: true);
		}
		ShowItemStats(inWidget);
	}

	private void ShowItemStats(KAWidget inWidget)
	{
		if (inWidget == null)
		{
			return;
		}
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)inWidget.GetUserData();
		if (kAUISelectItemData != null && mWidgetData != kAUISelectItemData)
		{
			mWidgetData = kAUISelectItemData;
			if (kAUISelectItemData._UserItemData != null)
			{
				_UiItemsStats.ShowStats(kAUISelectItemData._UserItemData, inWidget.GetTexture(), GetSellingPrice(kAUISelectItemData._ItemData), GetSellingShardQuantity(kAUISelectItemData._ItemData));
			}
		}
	}

	public override void SelectItem(KAWidget inWidget)
	{
		base.SelectItem(inWidget);
		ShowItemStats(inWidget);
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

	private void OnSellItems()
	{
		KAUICursorManager.SetExclusiveLoadingGear(status: true);
		foreach (int item in mItemsToSell)
		{
			CommonInventoryData.pInstance.AddSellItem(item, 1);
		}
		CommonInventoryData.pInstance.DoSell(OnItemsSold);
	}

	private void OnItemsSold(bool isSuccess, CommonInventoryResponse ret)
	{
		KAUICursorManager.SetExclusiveLoadingGear(status: false);
		if (isSuccess)
		{
			foreach (KAWidget item in mKAUiSelectMenu.GetItems())
			{
				if (((KAUISelectItemData)item.GetUserData())._ItemID != 0)
				{
					AddWidgetData(item, null);
				}
			}
			ResetMarket();
			_UiBlackSmith.UpdateShards();
		}
		UtDebug.Log("Blacksmith - Market : Items sold status " + isSuccess);
	}

	private void UpdateSellingPrice(int price)
	{
		mTotalSellPrice += price;
		if (mTotalSellPrice < 0)
		{
			mTotalSellPrice = 0;
		}
		mSellPriceTotal.SetVisibility(mTotalSellPrice > 0);
		mSellPriceTotal.SetText(mTotalSellPrice.ToString());
	}

	private void UpdateShards(int quantity)
	{
		mTotalShardsRewarded += quantity;
		if (mTotalShardsRewarded < 0)
		{
			mTotalShardsRewarded = 0;
		}
		mShardsTotal.SetVisibility(mTotalShardsRewarded > 0);
		mShardsTotal.SetText(mTotalShardsRewarded.ToString());
	}

	private int GetSellingShardQuantity(ItemData itemData)
	{
		int num = 0;
		if (itemData != null && itemData.ItemSaleConfigs != null)
		{
			for (int i = 0; i < itemData.ItemSaleConfigs.Length; i++)
			{
				if (itemData.ItemSaleConfigs[i].RewardItemID == _UiBlackSmith._ShardItemId)
				{
					num += itemData.ItemSaleConfigs[i].Quantity;
				}
			}
		}
		return num;
	}

	private int GetSellingPrice(ItemData itemData)
	{
		return ((itemData.GetPurchaseType() == 1) ? itemData.Cost : (itemData.CashCost * mGemsToCoinsFactor)) * itemData.SaleFactor / 100;
	}

	private void ResetMarket()
	{
		mItemsToSell.Clear();
		mTotalSellPrice = 0;
		mTotalShardsRewarded = 0;
		if (mSellPriceTotal != null)
		{
			mSellPriceTotal.SetVisibility(inVisible: false);
		}
		if (mShardsTotal != null)
		{
			mShardsTotal.SetVisibility(inVisible: false);
		}
		if (mBtnSellItems != null)
		{
			mBtnSellItems.SetDisabled(isDisabled: true);
		}
		if (mWidgetData != null && mWidgetData._Menu == mKAUiSelectMenu)
		{
			_UiItemsStats.SetVisibility(inVisible: false);
		}
		mWidgetData = null;
	}
}
