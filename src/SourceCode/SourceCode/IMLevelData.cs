using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "IMLevelData", Namespace = "")]
public class IMLevelData
{
	[XmlElement(ElementName = "Levels")]
	public LevelData[] Levels;
}
