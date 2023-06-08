using System;
using System.Xml.Serialization;

[Serializable]
public class MessageRequest
{
	[XmlElement(ElementName = "content")]
	public string content { get; set; }

	[XmlElement(ElementName = "level")]
	public MessageLevel level { get; set; }

	[XmlElement(ElementName = "targetUser")]
	public string targetUser { get; set; }

	[XmlElement(ElementName = "displayAttribute")]
	public string displayAttribute { get; set; }

	[XmlElement(ElementName = "replyTo")]
	public int? replyTo { get; set; }

	[XmlElement(ElementName = "isPrivate")]
	public bool isPrivate { get; set; }
}
