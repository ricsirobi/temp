using System.Xml.Serialization;

public enum TimedMissionState
{
	[XmlEnum("0")]
	None,
	[XmlEnum("1")]
	Alotted,
	[XmlEnum("2")]
	Started,
	[XmlEnum("3")]
	Won,
	[XmlEnum("4")]
	Lost,
	[XmlEnum("5")]
	Ended,
	[XmlEnum("6")]
	CoolDown,
	[XmlEnum("7")]
	Default
}
