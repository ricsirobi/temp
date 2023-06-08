using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "GLRT")]
[Flags]
public enum LevelRewardType
{
	[XmlEnum("1")]
	LevelCompletion = 1,
	[XmlEnum("2")]
	LevelFailure = 2,
	[XmlEnum("3")]
	ExtraChest = 3
}
