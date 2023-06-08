using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "progress", Namespace = "")]
public class GameProgress
{
	[XmlElement(ElementName = "name")]
	public string Name;

	[XmlElement(ElementName = "version")]
	public string Version;

	[XmlElement(ElementName = "chapter")]
	public GameChapter[] Chapter;

	[XmlElement(ElementName = "custom")]
	public string Custom;

	internal Dictionary<string, string> CustomDict = new Dictionary<string, string>();
}
