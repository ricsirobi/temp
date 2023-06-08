using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "AT", Namespace = "")]
public class ItemAttribute
{
	[XmlElement(ElementName = "k")]
	public string Key;

	[XmlElement(ElementName = "v")]
	public string Value;
}
