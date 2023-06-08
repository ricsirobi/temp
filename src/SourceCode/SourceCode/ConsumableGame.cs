using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot("ConsumableGame", Namespace = "")]
public class ConsumableGame
{
	[XmlAttribute("type")]
	public string type = string.Empty;

	[XmlElement(ElementName = "ConsumableType")]
	public ConsumableType[] ConsumableTypes;
}
