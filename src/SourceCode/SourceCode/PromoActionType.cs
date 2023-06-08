using System.Xml.Serialization;

public enum PromoActionType
{
	[XmlEnum("0")]
	BuyItem,
	[XmlEnum("1")]
	LoadStore,
	[XmlEnum("2")]
	LoadScene,
	[XmlEnum("3")]
	View
}
