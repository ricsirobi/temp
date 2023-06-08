using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "UserRatingRankRequest", IsNullable = true, Namespace = "")]
public class UserRatingRankRequest
{
	[XmlElement(ElementName = "PGID")]
	public int ProductGroupID { get; set; }

	[XmlElement(ElementName = "ID")]
	public int CategoryID { get; set; }

	[XmlElement(ElementName = "C")]
	public int Count { get; set; }

	[XmlElement(ElementName = "UID")]
	public Guid UserID { get; set; }

	[XmlElement(ElementName = "GID")]
	public Guid? GroupID { get; set; }

	[XmlElement(ElementName = "FBIDS")]
	public List<long> FacebookUserIDs { get; set; }

	[XmlElement(ElementName = "T")]
	public RequestType Type { get; set; }
}
