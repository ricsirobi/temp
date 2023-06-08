using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ArrayOfUserAchievementTask", Namespace = "http://api.jumpstart.com/")]
public class ArrayOfUserAchievementTask
{
	[XmlElement(ElementName = "UserAchievementTask")]
	public UserAchievementTask[] UserAchievementTask;
}
