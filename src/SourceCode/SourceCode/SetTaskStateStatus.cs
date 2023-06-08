using System.Xml.Serialization;

public enum SetTaskStateStatus
{
	[XmlEnum("1")]
	RequiresMembership = 1,
	[XmlEnum("2")]
	RequiresAcceptance = 2,
	[XmlEnum("3")]
	NotWithinDateRange = 3,
	[XmlEnum("4")]
	PreRequisiteMissionIncomplete = 4,
	[XmlEnum("5")]
	UserRankLessThanMinRank = 5,
	[XmlEnum("6")]
	UserRankGreaterThanMaxRank = 6,
	[XmlEnum("7")]
	UserHasNoRankData = 7,
	[XmlEnum("8")]
	MissionStateNotFound = 8,
	[XmlEnum("9")]
	RequiredPriorTaskIncomplete = 9,
	[XmlEnum("10")]
	ParentsTaskIncomplete = 10,
	[XmlEnum("11")]
	ParentsSubMissionIncomplete = 11,
	[XmlEnum("12")]
	TaskCanBeDone = 12,
	[XmlEnum("13")]
	OneOrMoreMissionsHaveNoRewardsAttached = 13,
	[XmlEnum("14")]
	PayLoadUpdated = 14,
	[XmlEnum("15")]
	NonRepeatableMission = 15,
	[XmlEnum("255")]
	Unknown = 255
}
