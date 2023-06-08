using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "UAIR", Namespace = "")]
public class UserAchievementInfoResponse
{
	[XmlElement(ElementName = "UAI")]
	public UserAchievementInfo[] AchievementInfo;

	[XmlElement(ElementName = "DR")]
	public Range DateRange;
}
