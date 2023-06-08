using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "UISS", Namespace = "")]
public class UserItemStat
{
	[XmlElement(ElementName = "iss", IsNullable = true)]
	public ItemStat[] ItemStats { get; set; }

	[XmlElement(ElementName = "it", IsNullable = true)]
	public ItemTier? ItemTier { get; set; }
}
