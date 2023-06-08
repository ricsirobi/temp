using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "STAT", Namespace = "", IsNullable = false)]
public class Stat
{
	[XmlElement(ElementName = "IID", IsNullable = false)]
	public int ItemID { get; set; }

	[XmlElement(ElementName = "ISID", IsNullable = false)]
	public int ItemStatsID { get; set; }

	[XmlElement(ElementName = "SID", IsNullable = false)]
	public int SetID { get; set; }

	[XmlElement(ElementName = "PROB", IsNullable = false)]
	public int Probability { get; set; }

	[XmlElement(ElementName = "ISRM", IsNullable = false)]
	public List<StatRangeMap> ItemStatsRangeMaps { get; set; }
}
