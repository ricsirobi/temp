using System;
using System.Xml.Serialization;

[Serializable]
public class CraftTraptionsMissionLevelData
{
	[XmlElement(ElementName = "LevelName")]
	public string LevelName;

	[XmlElement(ElementName = "CheckMembership")]
	public bool CheckMembership;

	[XmlElement(ElementName = "RequiredItem")]
	public int RequiredItem;
}
