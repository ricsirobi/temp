using System.Xml.Serialization;

public enum AvatarValidationResult
{
	[XmlEnum("1")]
	Valid = 1,
	[XmlEnum("2")]
	PartInvalid = 2,
	[XmlEnum("3")]
	PartTypeDuplicate = 3,
	[XmlEnum("4")]
	PartTypeEmpty = 4,
	[XmlEnum("5")]
	PartTypeLengthInvalid = 5,
	[XmlEnum("6")]
	TexturesInvalid = 6,
	[XmlEnum("7")]
	GeometriesInvalid = 7,
	[XmlEnum("8")]
	OffsetsInvalid = 8,
	[XmlEnum("9")]
	OffsetsNotFloat = 9,
	[XmlEnum("10")]
	AvatarDisplayNameInvalid = 10,
	[XmlEnum("255")]
	Error = 255
}
