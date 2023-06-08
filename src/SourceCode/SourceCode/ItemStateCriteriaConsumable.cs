using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ItemStateCriteriaConsumable", Namespace = "")]
public class ItemStateCriteriaConsumable : ItemStateCriteria
{
	[XmlElement(ElementName = "ItemID")]
	public int ItemID;

	[XmlElement(ElementName = "ConsumeUses")]
	public bool ConsumeUses;

	[XmlElement(ElementName = "Amount")]
	public int Amount;
}
