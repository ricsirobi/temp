using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "UATRRS", Namespace = "")]
public class UserAchievementTaskRedeemableRewards
{
	[XmlElement(ElementName = "UATRR", IsNullable = true)]
	public UserAchievementTaskRedeemableReward[] RedeemableRewards;
}
