using System;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "AR", Namespace = "")]
public class AchievementRewardUtil
{
	[XmlElement(ElementName = "a")]
	public int Amount;

	[XmlElement(ElementName = "p")]
	public RewardTypeUtil PointTypeID;

	[XmlElement(ElementName = "ii")]
	public int ItemID;

	[XmlElement(ElementName = "i", IsNullable = true)]
	public Guid? EntityID;

	[HideInInspector]
	[XmlElement(ElementName = "t")]
	public int EntityTypeID;

	[HideInInspector]
	[XmlElement(ElementName = "r")]
	public int RewardID;
}
