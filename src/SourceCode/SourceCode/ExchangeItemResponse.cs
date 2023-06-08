using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ExchangeItemResponse", Namespace = "")]
public class ExchangeItemResponse
{
	[XmlElement(ElementName = "Status")]
	public UseInventoryStatus Status { get; set; }

	[XmlElement(ElementName = "Success")]
	public bool Success { get; set; }
}
