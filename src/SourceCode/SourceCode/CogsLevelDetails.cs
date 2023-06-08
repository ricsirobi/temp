using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "CogsLevelDetails", Namespace = "")]
public class CogsLevelDetails
{
	[XmlElement(ElementName = "LevelName")]
	public string LevelName;

	[XmlElement(ElementName = "IsMissionLevel")]
	public bool IsMissionLevel;

	[XmlElement(ElementName = "MemberOnly")]
	public bool MemberOnly;

	[XmlElement(ElementName = "DifficultyBonus", IsNullable = true)]
	public int? DifficultyBonus;

	[XmlElement(ElementName = "InventoryItems")]
	public CogsInventoryItemData[] InventoryItems;

	[XmlElement(ElementName = "StaticItems")]
	public CogsStaticItemData StaticItems;

	[XmlElement(ElementName = "GoalMoves")]
	public int GoalMoves;

	[XmlElement(ElementName = "GoalTime")]
	public int GoalTime;
}
