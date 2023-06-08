using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "URR", IsNullable = true)]
public class UserRatingRequest
{
	[XmlElement(ElementName = "PGID")]
	public int ProductGroupID;

	[XmlElement(ElementName = "CID")]
	public int CategoryID;

	[XmlElement(ElementName = "UID")]
	public Guid? UserID;

	[XmlElement(ElementName = "RID")]
	public string RoomID;

	[XmlElement(ElementName = "RtID")]
	public Guid? RatedUserID;

	[XmlElement(ElementName = "RtV")]
	public int RatedValue;

	[XmlElement(ElementName = "RtEID")]
	public int? RatedEntityID;
}
