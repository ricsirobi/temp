using System.Xml.Serialization;

public enum RewardTypeUtil
{
	[XmlEnum("1")]
	Points = 1,
	[XmlEnum("2")]
	Coins = 2,
	[XmlEnum("3")]
	SocialPoints = 3,
	[XmlEnum("4")]
	PetPoints = 4,
	[XmlEnum("5")]
	VCash = 5,
	[XmlEnum("6")]
	Item = 6,
	[XmlEnum("7")]
	ContestPoints = 7,
	[XmlEnum("8")]
	DragonPoints = 8,
	[XmlEnum("9")]
	FarmingPoints = 9,
	[XmlEnum("10")]
	FishingPoints = 10,
	[XmlEnum("11")]
	Trophies = 11,
	[XmlEnum("12")]
	UDTPoints = 12,
	[XmlEnum("21")]
	WorldEventPoints = 21
}
