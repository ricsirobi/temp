using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine.Purchasing;

[Serializable]
public class IAPItemData
{
	[XmlElement(ElementName = "AppStoreID")]
	public string AppStoreID;

	[XmlElement(ElementName = "TermSKU", IsNullable = true)]
	public string TermSKU;

	[XmlElement(ElementName = "ItemName")]
	public LocaleString ItemName;

	[XmlElement(ElementName = "FormattedPrice")]
	public string FormattedPrice;

	[XmlElement(ElementName = "IconName")]
	public string IconName;

	[XmlElement(ElementName = "Description")]
	public LocaleString Description;

	[XmlElement(ElementName = "BillFrequency")]
	public int BillFrequency;

	[XmlElement(ElementName = "ItemType")]
	public ProductType ItemType;

	[XmlElement(ElementName = "Recurring")]
	public bool Recurring;

	[XmlElement(ElementName = "NumberOfCoins")]
	public int NumberOfCoins;

	[XmlElement(ElementName = "PreviewAsset", IsNullable = true)]
	public List<PreviewAssetData> PreviewAsset;

	[XmlElement(ElementName = "PurchaseType")]
	public int PurchaseType;

	[XmlElement(ElementName = "ItemID")]
	public int ItemID;

	[XmlElement(ElementName = "ISMStoreID")]
	public int ISMStoreID;

	[XmlElement(ElementName = "ItemAvailable")]
	public bool ItemAvailable = true;

	[XmlElement(ElementName = "SubscriptionOffer")]
	public string SubscriptionOffer;

	[XmlElement(ElementName = "SubscriptionOfferDescription")]
	public LocaleString SubscriptionOfferDescription;

	public string PriceInUSD;

	[XmlElement(ElementName = "PurchaseDateRange", IsNullable = true)]
	public string PurchaseDateRange;

	[XmlElement(ElementName = "Event", IsNullable = true)]
	public string Event;
}
