using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ItemList", Namespace = "", IsNullable = true)]
public class ArrayOfItemData
{
	[XmlElement(ElementName = "Items")]
	public ItemData[] Items;
}
