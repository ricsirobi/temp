using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ATR", Namespace = "")]
public class AchievementTaskReward
{
	[XmlElement(ElementName = "q")]
	public int RewardQuantity;

	[XmlElement(ElementName = "p")]
	public int PointTypeID;

	[XmlElement(ElementName = "r")]
	public int RewardID;

	[XmlElement(ElementName = "pg")]
	public int ProductGroupID;

	[XmlElement(ElementName = "a")]
	public int AchievementInfoID;

	[XmlElement(ElementName = "ii", IsNullable = true)]
	public int? ItemID;
}
