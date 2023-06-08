using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot("ConsumableType", Namespace = "")]
public class ConsumableType
{
	[XmlAttribute("type")]
	public string type = string.Empty;

	[XmlElement(ElementName = "Consumable")]
	public Consumable[] Consumables;
}
