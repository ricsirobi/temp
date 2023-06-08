using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "MP", Namespace = "")]
public class MissionPair
{
	[XmlElement(ElementName = "MID")]
	public int? MissionID { get; set; }

	[XmlElement(ElementName = "VID")]
	public int? VersionID { get; set; }
}
