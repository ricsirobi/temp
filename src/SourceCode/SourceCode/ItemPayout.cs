using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "IPT")]
public enum ItemPayout
{
	[XmlEnum("0")]
	Equipment,
	[XmlEnum("1")]
	Gems,
	[XmlEnum("2")]
	Scroll
}
