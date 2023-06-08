using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "CogsInventoryItemData", Namespace = "")]
public class CogsInventoryItemData
{
	[XmlElement(ElementName = "InventoryCog")]
	public Cog InventoryCog;

	[XmlElement(ElementName = "Quantity")]
	public int Quantity;
}
