using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "BonusFactor", Namespace = "")]
public class BonusFactor
{
	[XmlElement(ElementName = "Type")]
	public FactorType Type;

	[XmlElement(ElementName = "Value")]
	public string Value;

	[XmlElement(ElementName = "Factor")]
	public int Factor;
}
