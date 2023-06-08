using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ItemStateCriteriaReplenishable", Namespace = "")]
public class ItemStateCriteriaReplenishable : ItemStateCriteria
{
	[XmlElement(ElementName = "ApplyRank")]
	public bool ApplyRank;

	[XmlElement(ElementName = "PointTypeID", IsNullable = true)]
	public int? PointTypeID;

	[XmlElement(ElementName = "ReplenishableRates")]
	public List<ReplenishableRate> ReplenishableRates;
}
