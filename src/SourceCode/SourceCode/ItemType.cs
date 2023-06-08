using System;
using System.Xml.Serialization;

[Serializable]
public enum ItemType
{
	[XmlEnum("1")]
	VCash = 1,
	[XmlEnum("2")]
	Membership
}
