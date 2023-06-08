using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ReceiptRedemptionResult", Namespace = "", IsNullable = true)]
public class ReceiptRedemptionResult
{
	[XmlElement(ElementName = "Success")]
	public bool Success;

	[XmlElement(ElementName = "Status")]
	public ReceiptRedemptionStatus Status;
}
