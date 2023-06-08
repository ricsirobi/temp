using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ATSR", Namespace = "")]
public class AchievementTaskSetResponse
{
	[XmlElement(ElementName = "s")]
	public bool Success;

	[XmlElement(ElementName = "u")]
	public bool UserMessage;

	[XmlElement(ElementName = "a")]
	public string AchievementName;

	[XmlElement(ElementName = "l")]
	public int Level;

	[XmlElement(ElementName = "AR", IsNullable = true)]
	public AchievementReward[] AchievementRewards;

	[XmlElement(ElementName = "aid", IsNullable = true)]
	public int? AchievementTaskGroupID;

	[XmlElement(ElementName = "LL", IsNullable = true)]
	public bool? LastLevelCompleted;

	[XmlElement(ElementName = "aiid", IsNullable = true)]
	public int? AchievementInfoID;

	public AchievementTaskSetResponse()
	{
		AchievementRewards = null;
	}
}
