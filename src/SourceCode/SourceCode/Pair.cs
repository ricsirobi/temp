using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "Pair", Namespace = "")]
public class Pair
{
	[XmlElement(ElementName = "PairKey")]
	public string PairKey;

	[XmlElement(ElementName = "PairValue")]
	public string PairValue;

	[XmlElement(ElementName = "UpdateDate")]
	public DateTime UpdateDate;
}
