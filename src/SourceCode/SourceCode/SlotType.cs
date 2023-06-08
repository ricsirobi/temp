using System.Xml.Serialization;

public enum SlotType
{
	[XmlEnum("0")]
	Easy,
	[XmlEnum("1")]
	Medium,
	[XmlEnum("2")]
	Hard,
	[XmlEnum("3")]
	Timed,
	[XmlEnum("4")]
	Toothless,
	[XmlEnum("5")]
	Member
}
