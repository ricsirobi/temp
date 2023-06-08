using System.Xml.Serialization;

public enum UseInventoryStatus
{
	[XmlEnum("0")]
	Success = 0,
	[XmlEnum("1")]
	InputValidationError = 1,
	[XmlEnum("2")]
	UserInventoryItemNotFound = 2,
	[XmlEnum("3")]
	ReducingUsesFailed = 3,
	[XmlEnum("4")]
	SettingUserRewardMultiplierFailed = 4,
	[XmlEnum("5")]
	InsufficientQuantity = 5,
	[XmlEnum("6")]
	NotFound = 6,
	[XmlEnum("255")]
	UnknownError = 255
}
