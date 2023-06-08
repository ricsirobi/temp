using System;
using System.Xml.Serialization;

[Serializable]
public enum EmailNotification
{
	[XmlEnum("0")]
	None,
	[XmlEnum("1")]
	Optin,
	[XmlEnum("2")]
	Optout
}
