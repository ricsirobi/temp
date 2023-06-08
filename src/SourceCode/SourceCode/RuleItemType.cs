using System.Xml.Serialization;

public enum RuleItemType
{
	[XmlEnum("1")]
	Task = 1,
	[XmlEnum("2")]
	Mission
}
