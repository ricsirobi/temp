using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "IISM", Namespace = "")]
public class InventoryItemStatsMap
{
	[XmlElement(ElementName = "CID", IsNullable = false)]
	public int CommonInventoryID { get; set; }

	[XmlElement(ElementName = "ITM", IsNullable = false)]
	public ItemData Item { get; set; }

	[XmlElement(ElementName = "ISM", IsNullable = false)]
	public ItemStatsMap ItemStatsMap { get; set; }
}
