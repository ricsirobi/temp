using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "Reward", Namespace = "")]
public class Reward
{
	[XmlElement(ElementName = "Type")]
	public string Type;

	[XmlElement(ElementName = "Amount")]
	public int Amount;

	[XmlElement(ElementName = "ItemID", IsNullable = true)]
	public int? ItemID;
}
