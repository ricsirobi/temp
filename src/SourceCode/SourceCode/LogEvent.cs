using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "LogEvent", Namespace = "")]
public class LogEvent
{
	[XmlElement(ElementName = "ApiKey")]
	public string ApiKey;

	[XmlElement(ElementName = "ApiToken")]
	public string ApiToken;

	[XmlElement(ElementName = "UserID")]
	public string UserID;

	[XmlElement(ElementName = "EventTypeID")]
	public int EventTypeID;

	[XmlElement(ElementName = "EventCategoryID")]
	public int EventCategoryID;

	[XmlElement(ElementName = "Data")]
	public string Data;
}
