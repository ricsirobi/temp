using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "pir", Namespace = "")]
public class PrizeItemResponse
{
	[XmlElement(ElementName = "i")]
	public int ItemID { get; set; }

	[XmlElement(ElementName = "pi")]
	public int PrizeItemID { get; set; }

	[XmlElement(ElementName = "pis", IsNullable = true)]
	public List<ItemData> MysteryPrizeItems { get; set; }
}
