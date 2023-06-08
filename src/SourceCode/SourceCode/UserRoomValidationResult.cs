using System.Xml.Serialization;

public enum UserRoomValidationResult
{
	[XmlEnum("1")]
	Valid = 1,
	[XmlEnum("2")]
	RMFValidationFailed
}
