using System.Xml.Serialization;

public enum InvitePlayerStatus
{
	[XmlEnum("1")]
	Success = 1,
	[XmlEnum("2")]
	Error,
	[XmlEnum("3")]
	InviterNotMemberOfTheGroup,
	[XmlEnum("4")]
	InviterHasNoPermission,
	[XmlEnum("5")]
	InviteeAlreadyMemberOfTheGroup,
	[XmlEnum("6")]
	InviteeListIsEmpty,
	[XmlEnum("7")]
	InvalidBuddyCode,
	[XmlEnum("8")]
	InviteAlreadySent
}
