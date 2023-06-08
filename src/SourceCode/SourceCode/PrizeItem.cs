using System.Xml.Serialization;

[XmlRoot(ElementName = "pi", Namespace = "")]
public class PrizeItem
{
	[XmlElement(ElementName = "pi")]
	public ItemData Item { get; set; }

	[XmlElement(ElementName = "iq")]
	public int ItemQuantity { get; set; }

	[XmlElement(ElementName = "cid")]
	public int CommonInventoryID { get; set; }
}
