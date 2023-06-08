using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "RequestChatAuthorizationResponse", Namespace = "http://api.jumpstart.com/")]
public enum RequestChatAuthorizationResponse
{
	[XmlEnum("0")]
	ERROR,
	[XmlEnum("1")]
	ACCEPTED,
	[XmlEnum("2")]
	ALREADY_EXISTS,
	[XmlEnum("3")]
	INVALID_EMAIL_ADDRESS
}
