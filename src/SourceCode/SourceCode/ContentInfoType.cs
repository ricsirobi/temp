using System.Xml.Serialization;

public enum ContentInfoType
{
	[XmlEnum("1")]
	Music = 1,
	[XmlEnum("2")]
	Movie,
	[XmlEnum("3")]
	ArcadeGame,
	[XmlEnum("4")]
	LearningGame,
	[XmlEnum("5")]
	MathBlasterMovie,
	[XmlEnum("6")]
	SuperSecretFlashGame,
	[XmlEnum("7")]
	JumpStartFlashGame,
	[XmlEnum("8")]
	DWAMovie
}
