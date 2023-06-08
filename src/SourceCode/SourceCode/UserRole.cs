using System.Xml.Serialization;

public enum UserRole
{
	[XmlEnum("1")]
	Member = 1,
	[XmlEnum("2")]
	Elder,
	[XmlEnum("3")]
	Leader
}
