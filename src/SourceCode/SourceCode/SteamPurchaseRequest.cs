using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "SteamPurchaseRequest", Namespace = "", IsNullable = true)]
public class SteamPurchaseRequest
{
	[XmlElement(ElementName = "PaymentReceipt")]
	public SteamReceipt PaymentReceipt { get; set; }
}
