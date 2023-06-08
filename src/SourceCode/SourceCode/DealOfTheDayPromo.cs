using System;
using System.Xml.Serialization;

[Serializable]
public class DealOfTheDayPromo
{
	[XmlElement(ElementName = "ItemID")]
	public int ItemID;

	[XmlElement(ElementName = "StoreID")]
	public int StoreID;

	[XmlElement(ElementName = "Category")]
	public string Category;

	[XmlElement(ElementName = "Store")]
	public string Store;

	[XmlElement(ElementName = "Gender", IsNullable = true)]
	public Gender? GenderType;

	[XmlElement(ElementName = "DaysOlder", IsNullable = true)]
	public int? DaysOlder;
}
