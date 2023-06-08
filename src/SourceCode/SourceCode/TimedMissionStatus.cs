using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "TMS", Namespace = "")]
public class TimedMissionStatus
{
	[XmlElement(ElementName = "MID")]
	public int MissionID;

	[XmlElement(ElementName = "WC")]
	public int WinCount;

	[XmlElement(ElementName = "PC")]
	public int PlayedCount;
}
