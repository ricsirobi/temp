using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "DM", Namespace = "")]
public class MissionDailyMission
{
	[XmlElement(ElementName = "M")]
	public int MissionID;

	[XmlElement(ElementName = "C")]
	public int Completed;
}
