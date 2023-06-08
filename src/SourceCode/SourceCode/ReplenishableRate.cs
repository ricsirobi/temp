using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ReplenishableRate", Namespace = "")]
public class ReplenishableRate
{
	[XmlElement(ElementName = "Uses")]
	public int Uses;

	[XmlElement(ElementName = "Rate")]
	public double Rate;

	[XmlElement(ElementName = "MaxUses")]
	public int MaxUses;

	[XmlElement(ElementName = "Rank", IsNullable = true)]
	public int? Rank;
}
