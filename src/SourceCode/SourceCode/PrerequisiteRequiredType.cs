using System.Xml.Serialization;

public enum PrerequisiteRequiredType
{
	[XmlEnum("1")]
	Member = 1,
	[XmlEnum("2")]
	Accept = 2,
	[XmlEnum("3")]
	Mission = 3,
	[XmlEnum("4")]
	Rank = 4,
	[XmlEnum("5")]
	DateRange = 5,
	[XmlEnum("7")]
	Item = 7,
	[XmlEnum("8")]
	Event = 8
}
