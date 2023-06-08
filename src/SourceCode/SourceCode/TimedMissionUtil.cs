using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "TimedMission", Namespace = "")]
public class TimedMissionUtil
{
	[XmlElement(ElementName = "MissionID")]
	public string MissionID;

	[XmlElement(ElementName = "Title")]
	public LocaleString Title;

	[XmlElement(ElementName = "Description")]
	public LocaleString Description;

	[XmlElement(ElementName = "Icon")]
	public string Icon;

	[XmlElement(ElementName = "Probability")]
	public int Probability;

	[XmlElement(ElementName = "Difficulty")]
	public int Difficulty;

	[XmlElement(ElementName = "Hint")]
	public LocaleString Hint;

	[XmlElement(ElementName = "HintIcon")]
	public string HintIcon;

	[XmlElement(ElementName = "CostList")]
	public CostPerTime CostOfCompletionList;

	[XmlElement(ElementName = "Duration")]
	public int Duration;

	[XmlElement(ElementName = "PetCount")]
	public MinMax PetCount;

	[XmlElement(ElementName = "Logs")]
	public List<string> LogSetIDs;

	[XmlElement(ElementName = "MaxNoOfTimes")]
	public int MaxNoOfTimes;

	[XmlElement(ElementName = "Type")]
	public SlotType Type;

	[XmlElement(ElementName = "Prerequisites")]
	public List<PrerequisiteItemUtil> Prerequisites;

	[XmlElement(ElementName = "Qualify")]
	public QualifyFactorList Qualify;

	[XmlElement(ElementName = "WinFactor")]
	public int WinFactor;

	[XmlElement(ElementName = "WinFactorPerDragon")]
	public int WinFactorPerDragon;

	[XmlElement(ElementName = "DragonFactors")]
	public List<BonusFactor> DragonFactors;

	[XmlElement(ElementName = "DragonEnergyCost")]
	public float DragonEnergyCost;

	[XmlElement(ElementName = "WinAchID")]
	public int WinAchID;

	[XmlElement(ElementName = "WinAchievements")]
	public List<AchievementsPerPet> WinAchievements;

	[XmlElement(ElementName = "WinRewards")]
	public List<AchievementRewardUtil> WinRewards;

	[XmlElement(ElementName = "LoseAchID")]
	public int LoseAchID;

	[XmlElement(ElementName = "LoseAchievements")]
	public List<AchievementsPerPet> LoseAchievements;

	[XmlElement(ElementName = "LoseRewards")]
	public List<AchievementRewardUtil> LoseRewards;
}
