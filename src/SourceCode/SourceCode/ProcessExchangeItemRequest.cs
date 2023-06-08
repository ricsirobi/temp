using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ProcessExchangeItemRequest", Namespace = "")]
public class ProcessExchangeItemRequest
{
	[XmlElement(ElementName = "ExchangeGroupID")]
	public long ExchangeGroupID { get; set; }

	[XmlElement(ElementName = "ProductGroupID")]
	public int ProductGroupID { get; set; }

	[XmlElement(ElementName = "Repeat")]
	public int Repeat { get; set; }

	[XmlElement(ElementName = "InventoryIDArr")]
	public int[] InventoryIDArr { get; set; }
}
