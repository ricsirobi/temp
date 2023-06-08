using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "StringData", IsNullable = true, Namespace = "")]
public class StringData
{
	[XmlElement(ElementName = "rid")]
	public long ResID;

	[XmlElement(ElementName = "s")]
	public string LocaleString;

	[XmlElement(ElementName = "l")]
	public string Locale;

	[XmlElement(ElementName = "f", IsNullable = true)]
	public string FontName;

	[XmlElement(ElementName = "cid")]
	public int CategoryID;

	[XmlElement(ElementName = "pid")]
	public int ProductID;
}
