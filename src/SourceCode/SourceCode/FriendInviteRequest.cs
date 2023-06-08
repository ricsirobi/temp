using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "FriendInviteRequest", Namespace = "")]
public class FriendInviteRequest
{
	[XmlElement(ElementName = "InviterName")]
	public string InviterName;

	[XmlElement(ElementName = "FriendEmailIDs")]
	public string[] FriendEmailIDs;

	[XmlElement(ElementName = "UserID")]
	public string UserID;

	[XmlElement(ElementName = "ProductGroupID")]
	public int? ProductGroupID;

	[XmlElement(ElementName = "ProductID")]
	public int? ProductID;

	[XmlElement(ElementName = "GroupID")]
	public string GroupID;

	[XmlElement(ElementName = "Secret")]
	public string Secret;

	[XmlElement(ElementName = "EmailType")]
	public EmailType? EmailType;
}
