using System.Xml.Serialization;

public enum SetSpeedUpItemStatus
{
	[XmlEnum("1")]
	Success = 1,
	[XmlEnum("2")]
	UserItemStateNotFound = 2,
	[XmlEnum("3")]
	SpeedUpCriteriaNotFound = 3,
	[XmlEnum("4")]
	AutomaticPurchaseFailed = 4,
	[XmlEnum("255")]
	Error = 255
}
