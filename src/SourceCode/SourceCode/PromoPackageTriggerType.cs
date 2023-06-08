using System.Xml.Serialization;

public enum PromoPackageTriggerType
{
	[XmlEnum("0")]
	Session,
	[XmlEnum("1")]
	Scene,
	[XmlEnum("2")]
	Mission,
	[XmlEnum("3")]
	Task,
	[XmlEnum("4")]
	StoreEnter,
	[XmlEnum("5")]
	StorePurchase
}
