using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "TimedMissionList", Namespace = "")]
public class TimedMissionListUtil
{
	[XmlElement(ElementName = "Disabled")]
	public bool Disabled;

	[XmlElement(ElementName = "Missions")]
	public List<TimedMissionUtil> Missions;

	[XmlElement(ElementName = "LogSets")]
	public MissionLogSet[] LogSets;
}
