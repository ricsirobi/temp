using System.Xml.Serialization;

public enum RotateDirection
{
	[XmlEnum("0")]
	NONE,
	[XmlEnum("1")]
	CW,
	[XmlEnum("2")]
	CCW,
	[XmlEnum("3")]
	ALL
}
