using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "mpir", Namespace = "")]
public class MultiplePrizeItemResponse
{
	[XmlElement(ElementName = "i")]
	public int ItemID { get; set; }

	[XmlElement(ElementName = "mpis", IsNullable = true)]
	public List<PrizeItem> MysteryPrizeItems { get; set; }
}
