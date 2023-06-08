using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "IPSM", Namespace = "", IsNullable = false)]
public class ItemPossibleStatsMap
{
	[XmlElement(ElementName = "IID", IsNullable = false)]
	public int ItemID { get; set; }

	[XmlElement(ElementName = "SC", IsNullable = false)]
	public int ItemStatsCount { get; set; }

	[XmlElement(ElementName = "SID", IsNullable = false)]
	public int SetID { get; set; }

	[XmlElement(ElementName = "SS", IsNullable = false)]
	public List<Stat> Stats { get; set; }
}
