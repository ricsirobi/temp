using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "CIRBI", Namespace = "")]
public class CommonInventoryResponseRewardBinItem : CommonInventoryResponseItem
{
	[XmlElement(ElementName = "UISMID", IsNullable = false)]
	public int UserItemStatsMapID { get; set; }
}
