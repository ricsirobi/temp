using System;
using System.Xml.Serialization;

[Serializable]
public enum ChallengeState
{
	[XmlEnum("1")]
	Initiated = 1,
	[XmlEnum("2")]
	Accepted,
	[XmlEnum("3")]
	Rejected,
	[XmlEnum("4")]
	Won,
	[XmlEnum("5")]
	Lost,
	[XmlEnum("6")]
	Expired
}
