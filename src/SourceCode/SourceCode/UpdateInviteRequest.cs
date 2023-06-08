using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "UpdateInviteRequest", IsNullable = true)]
public class UpdateInviteRequest
{
	[XmlElement(ElementName = "UserID")]
	public string UserID;

	[XmlElement(ElementName = "GroupID")]
	public string GroupID;

	[XmlElement(ElementName = "FromUserID")]
	public string FromUserID;

	[XmlElement(ElementName = "ProductGroupID")]
	public int? ProductGroupID;

	[XmlElement(ElementName = "ProductID")]
	public int? ProductID;

	[XmlElement(ElementName = "Message")]
	public string Message;

	[XmlElement(ElementName = "StatusID")]
	public GroupJoinRequestStatus StatusID;
}
