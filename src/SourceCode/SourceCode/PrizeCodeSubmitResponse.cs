using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "PrizeCodeSubmitResponse", Namespace = "")]
public class PrizeCodeSubmitResponse
{
	[XmlElement(ElementName = "SubmissionResult", IsNullable = true)]
	public SubmissionResult? Status;

	[XmlElement(ElementName = "AchievementReward", IsNullable = true)]
	public AchievementReward[] Rewards;
}
