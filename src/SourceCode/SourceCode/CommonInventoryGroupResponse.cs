using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "CIRS", Namespace = "")]
public class CommonInventoryGroupResponse
{
	[XmlElement(ElementName = "s")]
	public bool Success;

	[XmlElement(ElementName = "ugc", IsNullable = true)]
	public UserGameCurrency UserGameCurrency;

	[XmlElement(ElementName = "pir", IsNullable = true)]
	public List<MultiplePrizeItemResponse> PrizeItems { get; set; }

	[XmlElement(ElementName = "ValidationMessage", IsNullable = true)]
	public List<ValidationStatusResponse> ValidationResponse { get; set; }
}
