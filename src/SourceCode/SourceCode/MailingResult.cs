using System.Xml.Serialization;

public enum MailingResult
{
	[XmlEnum("Sent")]
	Sent = 0,
	[XmlEnum("InvalidEmailAddress")]
	InvalidEmailAddress = 1,
	[XmlEnum("CannotParseAuthorizationKey")]
	CannotParseAuthorizationKey = 2,
	[XmlEnum("SentNoReward")]
	SentNoReward = 3,
	[XmlEnum("GenericError")]
	GenericError = 99
}
