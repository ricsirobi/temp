using System;

public interface IAPProvider
{
	Action<bool> OnProductsReceived { get; set; }

	Action<PurchaseStatus, string, Receipt> OnPurchaseDone { get; set; }

	void Init(string[] productIdentifiers, IAPStoreData storeData);

	void Purchase(IAPItemData item, int quantity);
}
