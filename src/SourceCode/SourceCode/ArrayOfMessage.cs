using System.Xml.Serialization;

public class ArrayOfMessage
{
	[XmlElement(ElementName = "Message")]
	public Message[] Messages;
}
