using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ActivationReminderResponse", IsNullable = true)]
public enum ActivationReminderResponse
{
	[XmlEnum("1")]
	ACTIVATION_REMINDER_SENT = 1,
	[XmlEnum("2")]
	ACCOUNT_ALREADY_ACTIVATED = 2,
	[XmlEnum("3")]
	ACCOUNT_DELETED = 3,
	[XmlEnum("4")]
	NOT_YET_TIME_TO_REMIND = 4,
	[XmlEnum("5")]
	ACCOUNT_ACTIVATION_DENIED = 5,
	[XmlEnum("6")]
	EMAIL_AUTHORIZATION_FAILED = 6,
	[XmlEnum("7")]
	EMAIL_NOT_MATCHED = 7,
	[XmlEnum("8")]
	SUPER_PARENT_CREATION_FAILED = 8,
	[XmlEnum("255")]
	ERROR = 255
}
