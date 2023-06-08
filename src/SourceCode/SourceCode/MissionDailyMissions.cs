using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "MDM", Namespace = "")]
public class MissionDailyMissions
{
	[XmlElement(ElementName = "T")]
	public DateTime Time;

	[XmlElement(ElementName = "M")]
	public List<MissionDailyMission> Missions;

	[XmlElement(ElementName = "UM")]
	public List<int> UsedMissions;
}
