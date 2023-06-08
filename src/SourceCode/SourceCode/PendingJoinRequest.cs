using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "PendingJoinRequest", IsNullable = true)]
public class PendingJoinRequest
{
	[XmlElement(ElementName = "UserID")]
	public string UserID;

	[XmlElement(ElementName = "GroupID")]
	public string GroupID;

	[XmlElement(ElementName = "FromUserID")]
	public string FromUserID;

	[XmlElement(ElementName = "Message")]
	public string Message;

	[XmlElement(ElementName = "StatusID", IsNullable = true)]
	public GroupJoinRequestStatus? StatusID;
}
