using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ArenaFrenzyMaps", Namespace = "")]
public class ArenaFrenzyMaps
{
	[XmlElement(ElementName = "ArenaFrenzyMapData")]
	public ArenaFrenzyMapData[] Maps;
}
