using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "SetTimedMissionTaskStateResult", Namespace = "")]
public class SetTimedMissionTaskStateResult : SetTaskStateResult
{
	[XmlElement(ElementName = "D")]
	public AchievementReward[] DailyReward;
}
