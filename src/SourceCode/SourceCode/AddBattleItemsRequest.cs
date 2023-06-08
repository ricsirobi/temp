using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ABIR", Namespace = "")]
public class AddBattleItemsRequest
{
	[XmlElement(ElementName = "BITM", IsNullable = false)]
	public List<BattleItemTierMap> BattleItemTierMaps { get; set; }
}
