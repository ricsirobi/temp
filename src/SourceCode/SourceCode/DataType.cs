using System;
using System.Xml.Serialization;

[Flags]
public enum DataType
{
	[XmlEnum("1")]
	BOOL = 1,
	[XmlEnum("2")]
	BYTE = 2,
	[XmlEnum("3")]
	CHAR = 3,
	[XmlEnum("4")]
	DECIMAL = 4,
	[XmlEnum("5")]
	DOUBLE = 5,
	[XmlEnum("6")]
	FLOAT = 6,
	[XmlEnum("7")]
	INT = 7,
	[XmlEnum("8")]
	LONG = 8,
	[XmlEnum("9")]
	SBYTE = 9,
	[XmlEnum("10")]
	SHORT = 0xA,
	[XmlEnum("11")]
	STRING = 0xB,
	[XmlEnum("12")]
	UINT = 0xC,
	[XmlEnum("13")]
	ULONG = 0xD,
	[XmlEnum("14")]
	USHORT = 0xE
}
