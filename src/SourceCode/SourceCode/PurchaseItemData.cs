public class PurchaseItemData
{
	public ItemData _ItemData;

	public int _StoreID;

	public int _Quantity;

	public int pTotalCost
	{
		get
		{
			if (_ItemData.GetPurchaseType() != 1)
			{
				return _ItemData.FinalCashCost * _Quantity;
			}
			return _ItemData.FinalCost * _Quantity;
		}
	}

	public PurchaseItemData(ItemData itemData, int storeID, int quantity)
	{
		_ItemData = itemData;
		_StoreID = storeID;
		_Quantity = quantity;
	}

	public bool HasEnoughCurrency(ref LocaleString message)
	{
		KAUIStore pInstance = KAUIStore.pInstance;
		if (pInstance != null)
		{
			if (_ItemData.GetPurchaseType() == 1 && pTotalCost > Money.pGameCurrency)
			{
				message = pInstance._InsufficientGameCurrencyText;
				return false;
			}
			if (_ItemData.GetPurchaseType() == 2 && pTotalCost > Money.pCashCurrency)
			{
				message = pInstance._InsufficientCashCurrencyText;
				return false;
			}
		}
		return true;
	}
}
