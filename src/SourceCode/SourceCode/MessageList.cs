using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "MessageList", IsNullable = true, Namespace = "")]
public class MessageList
{
	public int? ID;

	public string UserID;

	public MessageListType Type;

	public int LastMessageID;

	public Message[] Messages;
}
