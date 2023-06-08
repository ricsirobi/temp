using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "IS", Namespace = "")]
public class ItemStat
{
	[XmlElement(ElementName = "ID")]
	public int ItemStatID { get; set; }

	[XmlElement(ElementName = "N")]
	public string Name { get; set; }

	[XmlElement(ElementName = "V")]
	public string Value { get; set; }

	[XmlElement(ElementName = "DTI")]
	public DataTypeInfo DataType { get; set; }

	public ItemStat()
	{
	}

	public ItemStat(ItemStat stat)
	{
		ItemStatID = stat.ItemStatID;
		Name = stat.Name;
		Value = stat.Value;
		DataType = stat.DataType;
	}
}
