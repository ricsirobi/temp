using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "Receipt", Namespace = "", IsNullable = true)]
public class Receipt
{
	[XmlElement(ElementName = "ProductGroupID")]
	public int? ProductGroupID;

	[XmlElement(ElementName = "ProductID")]
	public int? ProductID;

	[XmlElement(ElementName = "UserID")]
	public Guid? UserID;

	[XmlElement(ElementName = "ThirdPartyUserID")]
	public string ThirdPartyUserID;

	[XmlElement(ElementName = "VendorApplicationID")]
	public string VendorApplicationID;

	[XmlElement(ElementName = "TransactionID")]
	public string TransactionID;

	[XmlElement(ElementName = "UniqueID")]
	public string UniqueID;

	[XmlElement(ElementName = "Qty")]
	public int Quantity;

	[XmlElement(ElementName = "ItemID")]
	public string ItemID;

	[XmlElement(ElementName = "ItemTypeID")]
	public ItemType ItemTypeID;

	[XmlElement(ElementName = "VendorReceipt")]
	public string VendorReceipt;

	[XmlElement(ElementName = "Signature")]
	public string Signature;
}
