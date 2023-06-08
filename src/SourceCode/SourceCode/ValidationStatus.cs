using System.Xml.Serialization;

public enum ValidationStatus
{
	[XmlEnum("0")]
	Success = 0,
	[XmlEnum("1")]
	InputValidationError = 1,
	[XmlEnum("2")]
	InputDeserializeError = 2,
	[XmlEnum("3")]
	InvalidToken = 3,
	[XmlEnum("4")]
	NoItemFound = 4,
	[XmlEnum("5")]
	InvalidQuantity = 5,
	[XmlEnum("6")]
	InvalidConfiguration = 6,
	[XmlEnum("7")]
	AuthenticationFailed = 7,
	[XmlEnum("8")]
	PrizeItemsNotFound = 8,
	[XmlEnum("9")]
	ExceededMaxRedeemCount = 9,
	[XmlEnum("10")]
	CollectableQuantityNotFound = 10,
	[XmlEnum("11")]
	MaxRedeemCountNotConfigured = 11,
	[XmlEnum("255")]
	UnknownError = 255
}
