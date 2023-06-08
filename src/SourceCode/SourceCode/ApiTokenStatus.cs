using System.Xml.Serialization;

public enum ApiTokenStatus
{
	[XmlEnum("1")]
	TokenValid = 1,
	[XmlEnum("2")]
	TokenExpired,
	[XmlEnum("3")]
	TokenNotFound,
	[XmlEnum("4")]
	UserLoggedInFromAnotherLocation,
	[XmlEnum("5")]
	ApiKeyInvalid
}
