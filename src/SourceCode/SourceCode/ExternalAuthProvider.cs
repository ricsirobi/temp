using System;
using System.Xml.Serialization;

[Serializable]
public enum ExternalAuthProvider
{
	[XmlEnum("1")]
	Facebook = 1,
	[XmlEnum("2")]
	VK,
	[XmlEnum("3")]
	Kongregate
}
