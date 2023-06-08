using System;
using System.Xml.Serialization;

[Serializable]
public class CraftTraptionsLevelData
{
	[XmlElement(ElementName = "Level")]
	public int Level;

	[XmlElement(ElementName = "Tutorial")]
	public bool Tutorial;

	[XmlElement(ElementName = "CraftTraptionsRequiredItems")]
	public CraftTraptionsRequiredItems[] RequiredItems;

	[XmlElement(ElementName = "LevelName")]
	public string LevelName;

	[XmlElement(ElementName = "Unlocked")]
	public bool Unlocked;

	[XmlElement(ElementName = "CashCurrency")]
	public int CashCurrency;

	[XmlElement(ElementName = "GameCurrency")]
	public int GameCurrency;
}
