using System.Collections.Generic;
using UnityEngine;

public class UiItemSellInfoDB : KAUI
{
	public int _ShardItemId;

	public LocaleString _SellText = new LocaleString("[REVIEW] Dismantle ");

	private KAWidget mYesBtn;

	private KAWidget mNoBtn;

	private KAWidget mCoinsTotal;

	private KAWidget mShardsTotal;

	private KAWidget mTxtSellInfo;

	private KAWidget mTxtTitle;

	private GameObject mMessageObject;

	private string mYesMessage;

	protected override void Start()
	{
		mYesBtn = FindItem("YesBtn");
		mNoBtn = FindItem("NoBtn");
		mCoinsTotal = FindItem("SellPriceTotal");
		mShardsTotal = FindItem("Shards");
		mTxtTitle = FindItem("TxtTitle");
		mTxtSellInfo = FindItem("TxtSellInfo");
		base.Start();
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mYesBtn)
		{
			if (mMessageObject != null && !string.IsNullOrEmpty(mYesMessage))
			{
				mMessageObject.SendMessage(mYesMessage);
			}
			SetVisibility(inVisible: false);
		}
		else if (inWidget == mNoBtn)
		{
			SetVisibility(inVisible: false);
		}
	}

	protected override void UpdateVisibility(bool visible)
	{
		base.UpdateVisibility(visible);
		if (visible)
		{
			KAUI.SetExclusive(this);
		}
		else
		{
			KAUI.RemoveExclusive(this);
		}
	}

	private int GetSellingCoinQuantity(ItemData itemData)
	{
		int num = 0;
		if (InventorySetting.pInstance != null)
		{
			num = InventorySetting.pInstance._GemsToCoinsFactor;
		}
		return ((itemData.GetPurchaseType() == 1) ? itemData.Cost : (itemData.CashCost * num)) * itemData.SaleFactor / 100;
	}

	private int GetSellingCoinQuantity(List<ItemData> itemDataList)
	{
		int num = 0;
		foreach (ItemData itemData in itemDataList)
		{
			num += GetSellingCoinQuantity(itemData);
		}
		return num;
	}

	private int GetSellingShardQuantity(ItemData itemData)
	{
		int num = 0;
		if (itemData != null && itemData.ItemSaleConfigs != null)
		{
			for (int i = 0; i < itemData.ItemSaleConfigs.Length; i++)
			{
				if (itemData.ItemSaleConfigs[i].RewardItemID == _ShardItemId)
				{
					num += itemData.ItemSaleConfigs[i].Quantity;
				}
			}
		}
		return num;
	}

	private int GetSellingShardQuantity(List<ItemData> itemDataList)
	{
		int num = 0;
		foreach (ItemData itemData in itemDataList)
		{
			num += GetSellingShardQuantity(itemData);
		}
		return num;
	}

	public void Initialize(ItemData itemData, GameObject messageObject, string yesMessage)
	{
		mYesMessage = yesMessage;
		mMessageObject = messageObject;
		int sellingCoinQuantity = GetSellingCoinQuantity(itemData);
		int sellingShardQuantity = GetSellingShardQuantity(itemData);
		mTxtTitle.SetText(_SellText.GetLocalizedString() + " " + itemData.ItemName + "?");
		if (mTxtSellInfo != null)
		{
			mTxtSellInfo.SetVisibility(sellingCoinQuantity > 0 || sellingShardQuantity > 0);
		}
		mCoinsTotal.SetVisibility(sellingCoinQuantity > 0);
		mCoinsTotal.SetText(sellingCoinQuantity.ToString());
		mShardsTotal.SetVisibility(sellingShardQuantity > 0);
		mShardsTotal.SetText(sellingShardQuantity.ToString());
		SetVisibility(inVisible: true);
	}

	public void Initialize(KAUISelectMenu selectMenu, GameObject messageObject, string yesMessage, string displayText)
	{
		List<ItemData> list = new List<ItemData>();
		foreach (KAWidget item in selectMenu.GetItems())
		{
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)item.GetUserData();
			if (kAUISelectItemData._ItemData != null)
			{
				list.Add(kAUISelectItemData._ItemData);
			}
		}
		mYesMessage = yesMessage;
		mMessageObject = messageObject;
		int sellingCoinQuantity = GetSellingCoinQuantity(list);
		int sellingShardQuantity = GetSellingShardQuantity(list);
		mTxtTitle.SetText(displayText);
		mCoinsTotal.SetVisibility(sellingCoinQuantity > 0);
		mCoinsTotal.SetText(sellingCoinQuantity.ToString());
		mShardsTotal.SetVisibility(sellingShardQuantity > 0);
		mShardsTotal.SetText(sellingShardQuantity.ToString());
		SetVisibility(inVisible: true);
	}
}
