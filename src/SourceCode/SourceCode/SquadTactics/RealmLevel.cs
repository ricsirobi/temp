using System;
using System.Xml.Serialization;

namespace SquadTactics;

[Serializable]
public class RealmLevel
{
	[XmlElement(ElementName = "Name")]
	public LocaleString Name;

	[XmlElement(ElementName = "Description")]
	public LocaleString Description;

	[XmlElement(ElementName = "LockedDescription")]
	public LocaleString LockedDescription;

	[XmlElement(ElementName = "LevelIcon")]
	public string LevelIcon;

	[XmlElement(ElementName = "TaskID", IsNullable = true)]
	public int? TaskID;

	[XmlElement(ElementName = "Scene")]
	public string[] Scenes;

	[XmlElement(ElementName = "StageInfo")]
	public LocaleString StageInfo;

	[XmlElement(ElementName = "Unit")]
	public UnitInfo[] Units;

	[XmlElement(ElementName = "ElementName")]
	public string ElementName;

	[XmlElement(ElementName = "LevelID")]
	public int LevelID;

	[XmlElement(ElementName = "LevelDifficultyID")]
	public int LevelDifficultyID;
}
