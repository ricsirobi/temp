using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ServerDownMessage", Namespace = "")]
public class ServerDownMessage
{
	[XmlElement(ElementName = "ProductGroupID")]
	public int? ProductGroupID;

	[XmlElement(ElementName = "ProductID")]
	public int? ProductID;

	[XmlElement(ElementName = "Locale")]
	public string Locale;

	[XmlElement(ElementName = "Title")]
	public string Title;

	[XmlElement(ElementName = "Text")]
	public string Text;

	[XmlElement(ElementName = "TimeFormat")]
	public string TimeFormat;
}
