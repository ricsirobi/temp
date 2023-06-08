using System;
using System.Xml.Serialization;

[Serializable]
public enum SteamTxnMode
{
	[XmlEnum("1")]
	Init = 1,
	[XmlEnum("2")]
	Finalize
}
