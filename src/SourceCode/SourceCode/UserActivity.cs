using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "UA", Namespace = "")]
public class UserActivity
{
	[XmlElement(ElementName = "id", IsNullable = true)]
	public int? UserActivityID;

	[XmlElement(ElementName = "uid", IsNullable = true)]
	public Guid? UserID;

	[XmlElement(ElementName = "rid", IsNullable = true)]
	public Guid? RelatedUserID;

	[XmlElement(ElementName = "aid", IsNullable = true)]
	public int? UserActivityTypeID;

	[XmlElement(ElementName = "d", IsNullable = true)]
	public DateTime? LastActivityDate;
}
