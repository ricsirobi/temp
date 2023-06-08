using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ItemState", Namespace = "")]
public class ItemState
{
	[XmlElement(ElementName = "ItemStateID")]
	public int ItemStateID;

	[XmlElement(ElementName = "Name")]
	public string Name;

	[XmlElement(ElementName = "Rule")]
	public ItemStateRule Rule;

	[XmlElement(ElementName = "Order")]
	public int Order;

	[XmlElement(ElementName = "AchievementID", IsNullable = true)]
	public int? AchievementID;

	[XmlElement(ElementName = "Rewards")]
	public AchievementReward[] Rewards;
}
