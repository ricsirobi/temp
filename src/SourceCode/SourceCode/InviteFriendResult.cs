using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "InviteFriendResult", Namespace = "")]
public class InviteFriendResult
{
	[XmlElement(ElementName = "Email")]
	public string Email;

	[XmlElement(ElementName = "MailingResult")]
	public MailingResult MailingResult;
}
