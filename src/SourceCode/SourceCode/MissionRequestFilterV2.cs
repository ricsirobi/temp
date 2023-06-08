using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "RequestFilter", Namespace = "")]
public class MissionRequestFilterV2
{
	[XmlElement(ElementName = "PGID")]
	public int ProductGroupID;

	[XmlElement(ElementName = "MGID")]
	public int[] MissionGroupIDs { get; set; }

	[XmlElement(ElementName = "MP")]
	public List<MissionPair> MissionPair { get; set; }

	[XmlElement(ElementName = "TID")]
	public int[] TaskIDList { get; set; }

	[XmlElement(ElementName = "PID")]
	public int? ProductID { get; set; }

	[XmlElement(ElementName = "S")]
	public int? SearchDepth { get; set; }

	[XmlElement(ElementName = "GCM")]
	public bool? GetCompletedMission { get; set; }
}
