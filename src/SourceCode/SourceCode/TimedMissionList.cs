using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "TimedMissionList", Namespace = "")]
public class TimedMissionList
{
	[XmlElement(ElementName = "Disabled")]
	public bool Disabled;

	[XmlElement(ElementName = "Missions")]
	public List<TimedMission> Missions;

	[XmlElement(ElementName = "LogSets")]
	public List<MissionLogSet> LogSets;
}
