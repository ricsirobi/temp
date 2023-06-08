using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "UTI", Namespace = "")]
public class UserTimedItem
{
	[XmlElement(ElementName = "id", IsNullable = true)]
	public int? UserTimedItemID;

	[XmlElement(ElementName = "uid", IsNullable = true)]
	public Guid? UserID;

	[XmlElement(ElementName = "usid", IsNullable = true)]
	public int? UserStaffID;

	[XmlElement(ElementName = "uipid", IsNullable = true)]
	public int? UserItemPositionID;

	[XmlElement(ElementName = "uicid", IsNullable = true)]
	public int? UserInventoryCommonID;

	[XmlElement(ElementName = "i", IsNullable = true)]
	public ItemData Item;

	[XmlElement(ElementName = "ts", IsNullable = true)]
	public DateTime? TimeStarted;

	[XmlElement(ElementName = "s", IsNullable = true)]
	public int? State;

	[XmlElement(ElementName = "y", IsNullable = true)]
	public int? Yield;

	[XmlElement(ElementName = "ud", IsNullable = true)]
	public DateTime? UpdateDate;
}
