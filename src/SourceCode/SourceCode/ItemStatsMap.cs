using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ISM", Namespace = "", IsNullable = false)]
public class ItemStatsMap
{
	[XmlElement(ElementName = "IID", IsNullable = false)]
	public int ItemID { get; set; }

	[XmlElement(ElementName = "IT", IsNullable = false)]
	public ItemTier ItemTier { get; set; }

	[XmlElement(ElementName = "ISS", IsNullable = false)]
	public ItemStat[] ItemStats { get; set; }
}
