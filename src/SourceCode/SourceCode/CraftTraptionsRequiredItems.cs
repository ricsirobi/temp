using System;
using System.Xml.Serialization;

[Serializable]
public class CraftTraptionsRequiredItems
{
	[XmlElement(ElementName = "ItemID")]
	public int? ItemID;

	[XmlElement(ElementName = "Quantity")]
	public int? Quantity;
}
