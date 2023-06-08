using System;
using System.Xml.Serialization;

[Serializable]
public class CombinedListMessage
{
	[XmlElement(ElementName = "MessageType")]
	public int MessageType;

	[XmlElement(ElementName = "MessageDate")]
	public DateTime MessageDate;

	[XmlElement(ElementName = "Body", IsNullable = true)]
	public string MessageBody;
}
