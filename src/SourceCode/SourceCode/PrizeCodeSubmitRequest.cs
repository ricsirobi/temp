using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "PrizeCodeSubmitRequest", Namespace = "")]
public class PrizeCodeSubmitRequest
{
	[XmlElement(ElementName = "code", IsNullable = true)]
	public string Code;

	[XmlElement(ElementName = "redeemfromGame", IsNullable = true)]
	public bool? RedeemFromGame;
}
