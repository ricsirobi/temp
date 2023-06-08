using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "MissionRemoveItem", Namespace = "")]
public class MissionRemoveItem
{
	[XmlElement(ElementName = "ItemID")]
	public int ItemID;

	[XmlElement(ElementName = "Quantity")]
	public int Quantity;
}
