using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "RUATR", Namespace = "")]
public class RedeemUserAchievementTaskResponse
{
	[XmlElement(ElementName = "s")]
	public bool Success;

	[XmlElement(ElementName = "u")]
	public bool UserMessage;

	[XmlElement(ElementName = "AR", IsNullable = true)]
	public AchievementReward[] AchievementRewards;
}
