using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "IATM", IsNullable = true)]
public class ItemActionTypeMap
{
	[XmlElement(ElementName = "ID", IsNullable = false)]
	public int ID { get; set; }

	[XmlElement(ElementName = "IM", IsNullable = false)]
	public int InventoryMax { get; set; }

	[XmlElement(ElementName = "IU", IsNullable = false)]
	public int ItemUses { get; set; }

	[XmlElement(ElementName = "ACT", IsNullable = false)]
	public ActionType Action { get; set; }
}
