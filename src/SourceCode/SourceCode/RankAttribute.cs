using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "RA", Namespace = "")]
public class RankAttribute
{
	[XmlElement(ElementName = "k")]
	public string Key;

	[XmlElement(ElementName = "v")]
	public string Value;
}
