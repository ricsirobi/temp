using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "AchievementTaskInfo")]
public class AchievementTaskInfo
{
	[XmlElement(ElementName = "AchievementInfoID")]
	public int AchievementInfoID;

	[XmlElement(ElementName = "AchievementTaskID")]
	public int AchievementTaskID;

	[XmlElement(ElementName = "AchievementTaskGroupID")]
	public int AchievementTaskGroupID;

	[XmlElement(ElementName = "AchievementTaskGroupName")]
	public string AchievementTaskGroupName;

	[XmlElement(ElementName = "Level")]
	public int Level;

	[XmlElement(ElementName = "MessageID")]
	public int MessageID;

	[XmlElement(ElementName = "PointValue")]
	public int PointValue;

	[XmlElement(ElementName = "Reproducible")]
	public bool Reproducible;

	[XmlElement(ElementName = "AllowNonMembers")]
	public bool AllowNonMembers;

	[XmlElement(ElementName = "Distinct")]
	public bool Distinct;

	[XmlElement(ElementName = "ProductGroupID")]
	public int ProductGroupID;

	[XmlElement(ElementName = "TaskEvent_Points")]
	public int TaskEvent_Points;

	[XmlElement(ElementName = "Cumulative")]
	public bool Cumulative;

	[XmlElement(ElementName = "PreredeemMessageID")]
	public int PreredeemMessageID;

	[XmlElement(ElementName = "ValidityFromDate", IsNullable = true)]
	public DateTime? ValidityFromDate;

	[XmlElement(ElementName = "ValidityToDate", IsNullable = true)]
	public DateTime? ValidityToDate;

	[XmlElement(ElementName = "VisibilityFromDate", IsNullable = true)]
	public DateTime? VisibilityFromDate;

	[XmlElement(ElementName = "VisibilityToDate", IsNullable = true)]
	public DateTime? VisibilityToDate;

	[XmlElement(ElementName = "AchieventTaskReward")]
	public AchievementReward[] AchievementReward;
}
