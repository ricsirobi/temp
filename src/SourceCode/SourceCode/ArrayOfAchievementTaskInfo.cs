using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ArrayOfAchievementTaskInfo", Namespace = "http://api.jumpstart.com/")]
public class ArrayOfAchievementTaskInfo
{
	[XmlElement(ElementName = "AchievementTaskInfo")]
	public AchievementTaskInfo[] AchievementTaskInfo;
}
