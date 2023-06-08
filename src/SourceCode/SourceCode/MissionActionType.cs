using System.Xml.Serialization;

public enum MissionActionType
{
	[XmlEnum("CutScene")]
	CutScene = 1,
	[XmlEnum("VO")]
	VO,
	[XmlEnum("Popup")]
	Popup,
	[XmlEnum("Rebus")]
	Rebus,
	[XmlEnum("Movie")]
	Movie
}
