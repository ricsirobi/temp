using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "Nickname", Namespace = "")]
public class Nickname
{
	[XmlElement(ElementName = "ID")]
	public Guid UserID;

	[XmlElement(ElementName = "N")]
	public string Name;
}
