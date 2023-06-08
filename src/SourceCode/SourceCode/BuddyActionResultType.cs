using System;
using System.Xml.Serialization;

[Flags]
public enum BuddyActionResultType
{
	[XmlEnum("0")]
	Unknown = 0,
	[XmlEnum("1")]
	Success = 1,
	[XmlEnum("2")]
	BuddyListFull = 2,
	[XmlEnum("3")]
	FriendBuddyListFull = 3,
	[XmlEnum("4")]
	AlreadyInList = 4,
	[XmlEnum("5")]
	InvalidFriendCode = 5,
	[XmlEnum("6")]
	CannotAddSelf = 6
}
