using System.Xml.Serialization;

public enum IAPCategoryType
{
	[XmlEnum("0")]
	Unknown,
	[XmlEnum("1")]
	Gems,
	[XmlEnum("2")]
	Membership,
	[XmlEnum("3")]
	Item
}
