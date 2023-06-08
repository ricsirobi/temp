using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "Locale", Namespace = "")]
public class Locale
{
	[XmlElement(ElementName = "ID")]
	public string ID = "en-US";

	[XmlElement(ElementName = "Variant")]
	public string[] Variant;
}
