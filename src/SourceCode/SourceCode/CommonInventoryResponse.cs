using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "CIRS", Namespace = "")]
public class CommonInventoryResponse
{
	[XmlElement(ElementName = "s")]
	public bool Success;

	[XmlElement(ElementName = "cids")]
	public CommonInventoryResponseItem[] CommonInventoryIDs;

	[XmlElement(ElementName = "ugc", IsNullable = true)]
	public UserGameCurrency UserGameCurrency;

	[XmlElement(ElementName = "pir", IsNullable = true)]
	public List<PrizeItemResponse> PrizeItems { get; set; }
}
