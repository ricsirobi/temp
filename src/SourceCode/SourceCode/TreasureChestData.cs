using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "TreasureChestData", Namespace = "")]
public class TreasureChestData
{
	public int TreasureChestId;

	[XmlElement(ElementName = "StartDate", IsNullable = true)]
	public string StartDate;

	[XmlElement(ElementName = "EndDate", IsNullable = true)]
	public string EndDate;

	public string ServerTime;

	public float RespawnTime;

	public int ChestMin;

	public int ChestMax;

	[XmlElement(ElementName = "GameCurrencyMin", IsNullable = true)]
	public int? GameCurrencyMin;

	[XmlElement(ElementName = "GameCurrencyMax", IsNullable = true)]
	public int? GameCurrencyMax;

	[XmlElement(ElementName = "ItemId")]
	public int[] ItemId;
}
