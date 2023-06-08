using System;
using System.Xml.Serialization;

[Serializable]
public class SpecialZoneData
{
	[XmlElement("SpecialZone")]
	public string SpecialZone;

	[XmlElement("ReloadScene")]
	public bool ReloadScene;

	[XmlElement("ForceJoin")]
	public bool ForceJoin;
}
