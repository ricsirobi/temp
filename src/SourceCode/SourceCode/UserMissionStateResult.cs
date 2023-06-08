using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "UserMissionStateResult", Namespace = "")]
public class UserMissionStateResult
{
	[XmlElement(ElementName = "UID")]
	public Guid UserID;

	[XmlElement(ElementName = "Mission")]
	public List<Mission> Missions;

	[XmlElement(ElementName = "UTA")]
	public List<UserTimedAchievement> UserTimedAchievement;

	[XmlElement(ElementName = "D")]
	public int Day;

	[XmlElement(ElementName = "MG")]
	public List<MissionGroup> MissionGroup;
}
