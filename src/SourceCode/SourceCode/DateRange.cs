using System;
using System.Xml.Serialization;

[Flags]
public enum DateRange
{
	[XmlEnum("1")]
	Day = 1,
	[XmlEnum("2")]
	Week = 2,
	[XmlEnum("3")]
	Month = 3
}
