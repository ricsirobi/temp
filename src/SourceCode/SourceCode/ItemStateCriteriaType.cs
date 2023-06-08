using System;
using System.Xml.Serialization;

[Serializable]
public enum ItemStateCriteriaType
{
	[XmlEnum("1")]
	Length = 1,
	[XmlEnum("2")]
	ConsumableItem,
	[XmlEnum("3")]
	ReplenishableItem,
	[XmlEnum("4")]
	SpeedUpItem,
	[XmlEnum("5")]
	StateExpiry
}
