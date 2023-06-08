using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ACT")]
public enum ActionType
{
	[XmlEnum("1")]
	MoveToInventory = 1,
	[XmlEnum("2")]
	SellInventoryItem,
	[XmlEnum("3")]
	SellRewardBinItem
}
