using System.Xml.Serialization;

public enum FactorType
{
	[XmlEnum("PetType")]
	PetType,
	[XmlEnum("DragonClass")]
	DragonClass,
	[XmlEnum("PrimaryType")]
	PrimaryType,
	[XmlEnum("SecondaryType")]
	SecondaryType,
	[XmlEnum("Age")]
	Age,
	[XmlEnum("Energy")]
	Energy,
	[XmlEnum("Rank")]
	Rank,
	[XmlEnum("ItemEquipped")]
	ItemEquipped,
	[XmlEnum("Count")]
	Count
}
