using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "UAI", Namespace = "")]
public class UserAchievementInfo
{
	[XmlElement(ElementName = "u")]
	public Guid? UserID;

	[XmlElement(ElementName = "n")]
	public string UserName;

	[XmlElement(ElementName = "a")]
	public int? AchievementPointTotal;

	[XmlElement(ElementName = "r")]
	public int RankID;

	[XmlElement(ElementName = "p")]
	public int? PointTypeID;

	[XmlElement(ElementName = "FBUID", IsNullable = true)]
	public long? FacebookUserID;

	public UserAchievementInfo()
	{
		AchievementPointTotal = 0;
	}
}
