using System.Xml.Serialization;

public enum ItemStateChangeError
{
	[XmlEnum("1")]
	Success = 1,
	[XmlEnum("2")]
	CannotOverrideCriteria = 2,
	[XmlEnum("3")]
	TimeNotReached = 3,
	[XmlEnum("4")]
	CannotFindInventoryItem = 4,
	[XmlEnum("5")]
	UsesLessThanRequired = 5,
	[XmlEnum("6")]
	UnableToGetItem = 6,
	[XmlEnum("7")]
	QtyLessThanRequired = 7,
	[XmlEnum("8")]
	ItemNotInUserInventory = 8,
	[XmlEnum("9")]
	TransitionFailed = 9,
	[XmlEnum("10")]
	ItemStateNotInStateList = 10,
	[XmlEnum("11")]
	OverrideCriteriaNotFound = 11,
	[XmlEnum("12")]
	NoCriteriasFound = 12,
	[XmlEnum("13")]
	AutomaticPurchaseFailed = 13,
	[XmlEnum("14")]
	ItemStateExpired = 14,
	[XmlEnum("255")]
	Error = 255
}
