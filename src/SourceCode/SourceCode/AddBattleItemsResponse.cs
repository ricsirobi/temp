using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ABIRES", Namespace = "")]
public class AddBattleItemsResponse
{
	[XmlElement(ElementName = "ST", IsNullable = false)]
	public Status Status { get; set; }

	[XmlElement(ElementName = "IISM", IsNullable = true)]
	public List<InventoryItemStatsMap> InventoryItemStatsMaps { get; set; }
}
