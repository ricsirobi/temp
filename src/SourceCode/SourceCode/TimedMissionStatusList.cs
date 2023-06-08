using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "TMSL", Namespace = "")]
public class TimedMissionStatusList
{
	[XmlElement(ElementName = "MSL")]
	public List<TimedMissionStatus> MissionStatusList;
}
