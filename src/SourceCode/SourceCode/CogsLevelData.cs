using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "CogsLevelData", Namespace = "")]
public class CogsLevelData
{
	[XmlElement(ElementName = "Levels")]
	public CogsLevelDetails[] Levels;
}
