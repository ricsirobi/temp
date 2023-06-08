using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ATSRQ", Namespace = "")]
public class AchievementTaskSetRequest
{
	[XmlElement(ElementName = "as")]
	public AchievementTask[] AchievementTaskSet { get; set; }
}
