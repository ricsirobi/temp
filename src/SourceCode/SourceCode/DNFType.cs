using System;
using System.Xml.Serialization;

[Flags]
public enum DNFType
{
	[XmlEnum("0")]
	Default = 0,
	[XmlEnum("1")]
	RaceTimeOut = 1,
	[XmlEnum("2")]
	RaceExit = 2,
	[XmlEnum("3")]
	RaceForceQuitGame = 3,
	[XmlEnum("4")]
	RaceNetworkDisconnect = 4,
	[XmlEnum("5")]
	LobbyExit = 5,
	[XmlEnum("6")]
	LobbyForceQuitGame = 6,
	[XmlEnum("7")]
	LobbyNetworkDisconnect = 7
}
