using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "DT")]
public enum DataTypeInfo
{
	[XmlEnum("I")]
	Int = 1,
	[XmlEnum("2")]
	Float,
	[XmlEnum("3")]
	Double,
	[XmlEnum("4")]
	String
}
