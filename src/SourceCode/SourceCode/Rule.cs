using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "rule", Namespace = "")]
public class Rule
{
	[XmlAttribute(AttributeName = "urlContains")]
	public string UrlContains { get; set; }

	[XmlAttribute(AttributeName = "enableAll")]
	public bool EnableAll { get; set; }

	[XmlElement(ElementName = "enable")]
	public string[] Enable { get; set; }
}
