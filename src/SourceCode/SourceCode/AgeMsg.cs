using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "AgeMsg", Namespace = "")]
public class AgeMsg
{
	[XmlElement(ElementName = "Age")]
	public string mAge = "";

	[XmlElement(ElementName = "Message")]
	public string mMessage = "";
}
