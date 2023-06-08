using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "UPT", Namespace = "")]
public class UserProfileTag
{
	[XmlElement(ElementName = "Date")]
	public DateTime CreateDate;

	[XmlElement(ElementName = "PGID")]
	public int ProductGroupID;

	[XmlElement(ElementName = "User")]
	public Guid UserID;

	[XmlElement(ElementName = "ID")]
	public int UserProfileTagID;

	[XmlElement(ElementName = "ProfileTag")]
	public List<ProfileTag> ProfileTags;
}
