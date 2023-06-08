using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "CraftTraptionsData")]
public class CraftTraptionsData
{
	[XmlElement(ElementName = "CraftTraptionsLevelData")]
	public CraftTraptionsLevelData[] LevelData;

	[XmlElement(ElementName = "CraftTraptionsMissionLevel")]
	public CraftTraptionsMissionLevelData[] MissionLevel;
}
