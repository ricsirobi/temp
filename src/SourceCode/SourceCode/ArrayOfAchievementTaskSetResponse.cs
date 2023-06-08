using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ArrayOfAchievementTaskSetResponse", Namespace = "http://api.jumpstart.com/")]
public class ArrayOfAchievementTaskSetResponse
{
	[XmlElement(ElementName = "AchievementTaskSetResponse")]
	public AchievementTaskSetResponse[] AchievementTaskSetResponse;
}
