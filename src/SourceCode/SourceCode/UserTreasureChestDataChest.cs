using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "UserTreasureChestDataChest", Namespace = "")]
public class UserTreasureChestDataChest
{
	public string Name;

	[XmlElement(ElementName = "ItemId", IsNullable = true)]
	public int? ItemId;

	[XmlElement(ElementName = "GameCurrency", IsNullable = true)]
	public int? GameCurrency;

	public bool Found;
}
