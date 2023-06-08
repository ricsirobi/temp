using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "rr", Namespace = "")]
public class RedeemRequest
{
	[XmlElement(ElementName = "i")]
	public int ItemID { get; set; }

	[XmlElement(ElementName = "rc", IsNullable = true)]
	public int? RedeemItemFetchCount { get; set; }
}
