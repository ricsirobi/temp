using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ET")]
public enum ExchangeType
{
	[XmlEnum("1")]
	From = 1,
	[XmlEnum("2")]
	To
}
