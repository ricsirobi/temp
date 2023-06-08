using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(Namespace = "http://api.jumpstart.com/", IsNullable = true)]
public class ArrayOfMessageInfo
{
	[XmlElement(ElementName = "MessageInfo")]
	public MessageInfo[] MessageInfo;
}
