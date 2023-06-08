using System;
using System.Xml.Serialization;

[Serializable]
public enum ReceiptRedemptionStatus
{
	[XmlEnum("1")]
	Success = 1,
	[XmlEnum("2")]
	UnableToValidateReceipt = 2,
	[XmlEnum("3")]
	VendorApplicationIDInvalid = 3,
	[XmlEnum("4")]
	ItemIDInvalid = 4,
	[XmlEnum("5")]
	QtyInvalid = 5,
	[XmlEnum("6")]
	ItemNotFoundInVendorList = 6,
	[XmlEnum("7")]
	ProductPlatformMapNotFound = 7,
	[XmlEnum("8")]
	NoProductPlatformMaps = 8,
	[XmlEnum("9")]
	NotSupportedPlatform = 9,
	[XmlEnum("10")]
	DuplicateReceipt = 10,
	[XmlEnum("11")]
	UnableToProcessReceipt = 11,
	[XmlEnum("12")]
	UnableToCreateMembership = 12,
	[XmlEnum("13")]
	PayloadMismatch = 13,
	[XmlEnum("42")]
	SubscriptionStatusExpired = 42,
	[XmlEnum("74")]
	ReceiptIncomplete = 74,
	[XmlEnum("255")]
	UnknownError = 255
}
