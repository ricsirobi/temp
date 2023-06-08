using System;
using System.Xml.Serialization;

[Serializable]
public class SetNextItemStateResult
{
	[XmlElement(ElementName = "UIS")]
	public UserItemState UserItemState;

	[XmlElement(ElementName = "RS")]
	public AchievementReward[] Rewards;

	[XmlElement(ElementName = "S")]
	public bool Success;

	[XmlElement(ElementName = "EC")]
	public ItemStateChangeError ErrorCode { get; set; }
}
