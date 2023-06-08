using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "LevelData", Namespace = "")]
public class LevelData
{
	[XmlElement(ElementName = "LevelName")]
	public string LevelName;

	[XmlElement(ElementName = "IsMissionLevel")]
	public bool IsMissionLevel;

	[XmlElement(ElementName = "MemberOnly")]
	public bool MemberOnly;

	[XmlElement(ElementName = "DifficultyBonus", IsNullable = true)]
	public int? DifficultyBonus;

	[XmlElement(ElementName = "IMInventoryItems")]
	public IMInventoryItemData[] IMInventoryItems;

	[XmlElement(ElementName = "StaticItems")]
	public IMStaticItemData[] StaticItems;
}
