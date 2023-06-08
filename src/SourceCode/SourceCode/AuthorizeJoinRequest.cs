using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "AuthorizeJoinRequest", IsNullable = true)]
public class AuthorizeJoinRequest
{
	[XmlElement(ElementName = "Approved")]
	public bool Approved;

	[XmlElement(ElementName = "JoineeID")]
	public string JoineeID;

	[XmlElement(ElementName = "UserID")]
	public string UserID;

	[XmlElement(ElementName = "GroupID")]
	public string GroupID;

	[XmlElement(ElementName = "ProductGroupID")]
	public int? ProductGroupID;

	[XmlElement(ElementName = "ProductID")]
	public int? ProductID;
}
