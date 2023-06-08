using System;
using System.Xml.Serialization;

[Serializable]
public enum StateTransition
{
	[XmlEnum("1")]
	NextState = 1,
	[XmlEnum("2")]
	Completion,
	[XmlEnum("3")]
	Deletion,
	[XmlEnum("4")]
	InitialState,
	[XmlEnum("5")]
	Expired
}
