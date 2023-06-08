using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "InventoryData", Namespace = "")]
public class InventoryData
{
	[XmlElement(ElementName = "Item")]
	public InventoryDataItem[] Item;
}
