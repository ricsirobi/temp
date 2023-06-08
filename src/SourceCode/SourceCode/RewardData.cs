using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "RewardData", Namespace = "", IsNullable = false)]
public class RewardData
{
	[XmlElement(ElementName = "Reward")]
	public Reward[] Rewards;

	[XmlElement(ElementName = "AchGrpID", IsNullable = true)]
	public int? TaskGroupID;

	[XmlElement(ElementName = "EntityID")]
	public string EntityID;
}
