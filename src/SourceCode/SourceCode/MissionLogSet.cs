using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "MissionLogSet", Namespace = "")]
public class MissionLogSet
{
	[XmlElement(ElementName = "LogSetID")]
	public string LogSetID;

	[XmlElement(ElementName = "Random")]
	public bool IsRandom;

	[XmlElement(ElementName = "Count")]
	public int Count;

	[XmlElement(ElementName = "LogString")]
	public LocaleString[] LogString;
}
