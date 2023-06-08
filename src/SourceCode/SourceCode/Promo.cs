using System;
using System.Xml.Serialization;

[Serializable]
public class Promo
{
	[XmlElement(ElementName = "SlotName")]
	public string SlotName;

	[XmlElement(ElementName = "SubTitleText")]
	public LocaleString SubTitleText;

	[XmlElement(ElementName = "ButtonText")]
	public LocaleString ButtonText;

	[XmlElement(ElementName = "BkgIconRes")]
	public string BkgIconRes;

	[XmlElement(ElementName = "IconRes")]
	public string IconRes;

	[XmlElement(ElementName = "DragonCheck")]
	public bool DragonCheck;

	[XmlElement(ElementName = "DragonBlockedStage")]
	public string[] BlockedStages;

	[XmlElement(ElementName = "DragonStageErrorText")]
	public LocaleString DragonStageErrorText;

	[XmlElement(ElementName = "DragonCheckErrorText")]
	public LocaleString DragonCheckErrorText;

	[XmlElement(ElementName = "MinDragonEnergy")]
	public int MinDragonEnergy;

	[XmlElement(ElementName = "MinDragonHappiness")]
	public int MinDragonHappiness;

	[XmlElement(ElementName = "NoHappyOrEnergyErrorText")]
	public LocaleString NoHappyOrEnergyErrorText;

	[XmlElement(ElementName = "MissionID")]
	public int MissionID;

	[XmlElement(ElementName = "MissionErrorText")]
	public LocaleString MissionErrorText;

	[XmlElement(ElementName = "TaskID")]
	public int TaskID;

	[XmlElement(ElementName = "TaskErrorText")]
	public LocaleString TaskErrorText;

	[XmlElement(ElementName = "DealOfTheDay")]
	public DealOfTheDayPromo[] Deal;

	[XmlElement(ElementName = "SceneChange")]
	public SceneChange SceneTransition;

	[XmlElement(ElementName = "LocationChange")]
	public LocationChange Location;
}
