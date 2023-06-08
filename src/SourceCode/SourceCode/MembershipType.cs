using System.Xml.Serialization;

public enum MembershipType
{
	[XmlEnum("0")]
	Any,
	[XmlEnum("1")]
	NonMember,
	[XmlEnum("2")]
	Member
}
