using System.Xml.Serialization;

public enum RegistrationValidateResult
{
	[XmlEnum("1")]
	DeviceTokenRegistered = 1,
	[XmlEnum("2")]
	DeviceTokenUpdated = 2,
	[XmlEnum("3")]
	DeviceTokenLengthValid = 3,
	[XmlEnum("4")]
	DeviceTokenLengthInvalid = 4,
	[XmlEnum("5")]
	DataValidationError = 5,
	[XmlEnum("255")]
	UnknownError = 255
}
