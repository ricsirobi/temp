using System.Xml.Serialization;

public class MissionRequestFilter
{
	[XmlElement(ElementName = "PGID")]
	public int ProductGroupID;

	[XmlElement(ElementName = "MGID")]
	public int? MissionGroupID { get; set; }

	[XmlElement(ElementName = "MID")]
	public int? MissionID { get; set; }

	[XmlElement(ElementName = "PID")]
	public int? ProductID { get; set; }

	[XmlElement(ElementName = "S")]
	public int? SearchDepth { get; set; }

	[XmlElement(ElementName = "GCM")]
	public bool? GetCompletedMission { get; set; }
}
