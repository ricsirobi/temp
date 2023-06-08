using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "RAD", Namespace = "")]
public class RankAttributeData
{
	[XmlElement(ElementName = "r")]
	public int RankID;

	[XmlElement(ElementName = "a", IsNullable = true)]
	public RankAttribute[] Attributes;
}
