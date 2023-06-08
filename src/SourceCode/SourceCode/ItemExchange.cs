using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ItmExch", Namespace = "", IsNullable = true)]
public class ItemExchange
{
	[XmlElement(ElementName = "Item", IsNullable = true)]
	public ItemData Item { get; set; }

	[XmlElement(ElementName = "EGID")]
	public long ExchangeGroupID { get; set; }

	[XmlElement(ElementName = "PGID")]
	public int ProductGroupID { get; set; }

	[XmlElement(ElementName = "Qty")]
	public int Quantity { get; set; }

	[XmlElement(ElementName = "ET")]
	public ExchangeType Type { get; set; }
}
