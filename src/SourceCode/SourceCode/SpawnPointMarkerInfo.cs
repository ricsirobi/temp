using System;
using System.Xml.Serialization;

[Serializable]
public class SpawnPointMarkerInfo
{
	[XmlElement(ElementName = "Name")]
	public string Name;

	[XmlElement(ElementName = "Team")]
	public int Team;
}
