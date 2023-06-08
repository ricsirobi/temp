using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "SteamReceipt", Namespace = "", IsNullable = true)]
public class SteamReceipt
{
	[XmlElement(ElementName = "Language")]
	public string Language;

	[XmlElement(ElementName = "OrderId")]
	public ulong? OrderId { get; set; }

	[XmlElement(ElementName = "VendorApplicationID")]
	public string VendorApplicationID { get; set; }

	[XmlElement(ElementName = "Qty")]
	public int Quantity { get; set; }

	[XmlElement(ElementName = "ItemID")]
	public string ItemID { get; set; }

	[XmlElement(ElementName = "ItemTypeID")]
	public ItemType ItemTypeID { get; set; }

	[XmlElement(ElementName = "Description")]
	public string Description { get; set; }

	[XmlElement(ElementName = "UserID")]
	public Guid? UserID { get; set; }

	[XmlElement(ElementName = "SteamId")]
	public string SteamId { get; set; }

	[XmlElement(ElementName = "SteamTxnMode")]
	public SteamTxnMode SteamTxnMode { get; set; }
}
