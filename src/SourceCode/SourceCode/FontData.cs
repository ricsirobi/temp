using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "FontData", IsNullable = true, Namespace = "")]
public class FontData
{
	[XmlElement(ElementName = "l")]
	public string Locale;

	[XmlElement(ElementName = "bf")]
	public string BaseFont;

	[XmlElement(ElementName = "bfs")]
	public int BaseFontSize;

	[XmlElement(ElementName = "lf")]
	public string LocaleFont;

	[XmlElement(ElementName = "lfs")]
	public int LocaleFontSize;

	[XmlElement(ElementName = "pid")]
	public int ProductID;
}
