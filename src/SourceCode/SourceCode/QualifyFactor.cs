using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "QualifyFactor", Namespace = "")]
public class QualifyFactor
{
	[XmlElement(ElementName = "Type")]
	public FactorType Type;

	[XmlElement(ElementName = "Value")]
	public string Value;
}
