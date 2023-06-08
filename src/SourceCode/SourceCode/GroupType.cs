using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "GRTP", Namespace = "")]
public enum GroupType
{
	[XmlEnum("0")]
	None,
	[XmlEnum("1")]
	System,
	[XmlEnum("2")]
	Public,
	[XmlEnum("3")]
	InviteOnly,
	[XmlEnum("4")]
	Closed
}
