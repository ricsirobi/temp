using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "UATRR", Namespace = "")]
public class UserAchievementTaskRedeemableReward
{
	[XmlElement(ElementName = "aii")]
	public int AchievementInfoID;

	[XmlElement(ElementName = "cd", IsNullable = true)]
	public DateTime? CompletedDate;

	[XmlElement(ElementName = "ir", IsNullable = true)]
	public bool? IsRedeemed;

	[XmlElement(ElementName = "ATR", IsNullable = true)]
	public AchievementTaskReward[] AchievementTaskRewards;
}
