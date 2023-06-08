using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "MissionCompletedResult", Namespace = "")]
public class MissionCompletedResult
{
	[XmlElement(ElementName = "M")]
	public int MissionID;

	[XmlElement(ElementName = "A")]
	public AchievementReward[] Rewards;
}
