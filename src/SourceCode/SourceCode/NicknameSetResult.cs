using System.Xml.Serialization;

public enum NicknameSetResult
{
	[XmlEnum("1")]
	Success = 1,
	[XmlEnum("2")]
	Failure,
	[XmlEnum("3")]
	Invalid
}
