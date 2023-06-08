using System;
using System.Xml.Serialization;

[Serializable]
public class IAPItemCategory
{
	[XmlElement(ElementName = "CategoryType")]
	public IAPCategoryType CategoryType;

	[XmlElement(ElementName = "CategoryName")]
	public string CategoryName;

	[XmlElement(ElementName = "ItemData")]
	public IAPItemData[] ItemDataList;
}
