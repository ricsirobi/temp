using System.Xml.Serialization;

public enum VersionStatus
{
	[XmlEnum("0")]
	UnknownVersion,
	[XmlEnum("1")]
	MustUpgrade,
	[XmlEnum("2")]
	UpgradeRecommended,
	[XmlEnum("3")]
	NoUpgradeRequired,
	[XmlEnum("4")]
	UpgradeNotAvailable
}
