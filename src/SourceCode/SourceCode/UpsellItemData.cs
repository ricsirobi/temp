using System;
using System.Xml.Serialization;

[Serializable]
public class UpsellItemData
{
	[XmlElement(ElementName = "CheckInventory")]
	public bool CheckInventory;

	[XmlElement(ElementName = "ItemID")]
	public int[] ItemID;

	[XmlElement(ElementName = "PetTypeID", IsNullable = true)]
	public int? PetTypeID;

	[XmlElement(ElementName = "Category")]
	public string Category;

	[XmlElement(ElementName = "Store")]
	public string Store;
}
