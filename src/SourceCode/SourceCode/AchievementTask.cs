using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ATS", Namespace = "")]
public class AchievementTask
{
	[XmlElement(ElementName = "oid")]
	public string OwnerID;

	[XmlElement(ElementName = "tid")]
	public int TaskID;

	[XmlElement(ElementName = "aiid")]
	public int AchievementInfoID;

	[XmlElement(ElementName = "rid")]
	public string RelatedID;

	[XmlElement(ElementName = "pts")]
	public int Points;

	[XmlElement(ElementName = "etid")]
	public int EntityTypeID;

	public AchievementTask()
	{
	}

	public AchievementTask(int taskID, string relatedID = "", int achInfoID = 0, int points = 0, int entityTypeID = 1)
	{
		OwnerID = UserInfo.pInstance.UserID;
		TaskID = taskID;
		AchievementInfoID = achInfoID;
		RelatedID = relatedID;
		Points = points;
		EntityTypeID = entityTypeID;
	}

	public AchievementTask(string ownerID, int taskID, string relatedID = "", int achInfoID = 0, int points = 0, int entityTypeID = 1)
	{
		OwnerID = ownerID;
		TaskID = taskID;
		AchievementInfoID = achInfoID;
		RelatedID = relatedID;
		Points = points;
		EntityTypeID = entityTypeID;
	}
}
