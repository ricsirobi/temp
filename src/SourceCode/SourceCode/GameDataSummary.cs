using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "GameDataSummary", Namespace = "")]
public class GameDataSummary
{
	[XmlElement(ElementName = "GameDataList")]
	public GameData[] GameDataList;

	[XmlElement(ElementName = "UserPosition", IsNullable = true)]
	public int? UserPosition;

	[XmlElement(ElementName = "GameID")]
	public int GameID;

	[XmlElement(ElementName = "IsMultiplayer")]
	public bool IsMultiplayer;

	[XmlElement(ElementName = "Difficulty")]
	public int Difficulty;

	[XmlElement(ElementName = "GameLevel")]
	public int GameLevel;

	[XmlElement(ElementName = "Key")]
	public string Key;
}
