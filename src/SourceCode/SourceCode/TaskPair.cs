using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "TaskPair", Namespace = "")]
public class TaskPair
{
	[XmlElement(ElementName = "Key")]
	public string Key;

	[XmlElement(ElementName = "Value")]
	public string Value;
}
