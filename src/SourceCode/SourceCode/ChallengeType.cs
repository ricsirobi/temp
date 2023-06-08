using System;
using System.Xml.Serialization;

[Serializable]
public enum ChallengeType
{
	[XmlEnum("1")]
	Points = 1,
	[XmlEnum("2")]
	Time
}
