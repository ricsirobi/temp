using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "InventoryDataItem", Namespace = "")]
public class InventoryDataItem
{
	[XmlElement(ElementName = "Name")]
	public string Name;

	[XmlElement(ElementName = "Quantity")]
	public int Quantity;
}
