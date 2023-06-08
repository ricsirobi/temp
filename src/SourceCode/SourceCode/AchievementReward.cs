using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "AR", Namespace = "")]
public class AchievementReward
{
	[XmlElement(ElementName = "a")]
	public int? Amount;

	[XmlElement(ElementName = "p", IsNullable = true)]
	public int? PointTypeID;

	[XmlElement(ElementName = "ii")]
	public int ItemID;

	[XmlElement(ElementName = "i", IsNullable = true)]
	public Guid? EntityID;

	[XmlElement(ElementName = "t")]
	public int EntityTypeID;

	[XmlElement(ElementName = "r")]
	public int RewardID;

	[XmlElement(ElementName = "ai")]
	public int AchievementID;

	[XmlElement(ElementName = "amulti")]
	public bool AllowMultiple;

	[XmlElement(ElementName = "mina", IsNullable = true)]
	public int? MinAmount;

	[XmlElement(ElementName = "maxa", IsNullable = true)]
	public int? MaxAmount;

	[XmlElement(ElementName = "d", IsNullable = true)]
	public DateTime? Date;

	[XmlElement(ElementName = "cid")]
	public int CommonInventoryID;

	[XmlElement(ElementName = "ui", IsNullable = true)]
	public UserItemData UserItem { get; set; }

	public AchievementReward()
	{
		Amount = 0;
		EntityID = null;
		EntityTypeID = 1;
	}
}
