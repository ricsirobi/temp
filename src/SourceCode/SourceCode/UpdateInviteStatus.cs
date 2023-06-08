using System.Xml.Serialization;

public enum UpdateInviteStatus
{
	[XmlEnum("1")]
	Success = 1,
	[XmlEnum("2")]
	Error,
	[XmlEnum("3")]
	InviteNotFound,
	[XmlEnum("4")]
	InvalidInviteStatus
}
