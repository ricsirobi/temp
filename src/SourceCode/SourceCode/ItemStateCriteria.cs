using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ItemStateCriteria", Namespace = "")]
[XmlInclude(typeof(ItemStateCriteriaLength))]
[XmlInclude(typeof(ItemStateCriteriaConsumable))]
[XmlInclude(typeof(ItemStateCriteriaReplenishable))]
[XmlInclude(typeof(ItemStateCriteriaOverride))]
[XmlInclude(typeof(ItemStateCriteriaSpeedUpItem))]
[XmlInclude(typeof(ItemStateCriteriaExpiry))]
public class ItemStateCriteria
{
	[XmlElement(ElementName = "Type")]
	public ItemStateCriteriaType Type;
}
