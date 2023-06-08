using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "BITM", Namespace = "")]
public class BattleItemTierMap
{
	[XmlElement(ElementName = "IID", IsNullable = false)]
	public int ItemID { get; set; }

	[XmlElement(ElementName = "T", IsNullable = true)]
	public ItemTier? Tier { get; set; }

	[XmlElement(ElementName = "QTY", IsNullable = true)]
	public int? Quantity { get; set; }
}
