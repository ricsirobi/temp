using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ReceiptRedemptionRequest", Namespace = "", IsNullable = true)]
public class ReceiptRedemptionRequest
{
	[XmlElement(ElementName = "PaymentReceipt")]
	public Receipt PaymentReceipt;
}
