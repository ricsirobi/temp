using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "Attribute", Namespace = "")]
public class AvatarPartAttribute
{
	[XmlElement(ElementName = "K")]
	public string Key;

	[XmlElement(ElementName = "V")]
	public string Value;
}
