using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ArrayOfUserAchievementInfo", Namespace = "http://api.jumpstart.com/")]
public class ArrayOfUserAchievementInfo
{
	[XmlElement(ElementName = "UserAchievementInfo")]
	public UserAchievementInfo[] UserAchievementInfo;
}
