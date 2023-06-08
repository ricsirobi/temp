using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot("Consumable", Namespace = "")]
public class Consumable
{
	[XmlAttribute("name")]
	public string name = string.Empty;

	[XmlElement(ElementName = "Mode")]
	public int Mode;

	[XmlElement(ElementName = "Sprite")]
	public string Sprite;

	[XmlElement(ElementName = "ItemID")]
	public int ItemID;

	[XmlElement(ElementName = "Probability")]
	public Probability[] Probabilities;

	[XmlElement(ElementName = "CoolDown")]
	public float CoolDown;

	[XmlElement(ElementName = "MaxCount")]
	public int MaxCount;

	[XmlIgnore]
	public string _Type;
}
