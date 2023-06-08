using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ItemStateCriteriaOverride", Namespace = "")]
public class ItemStateCriteriaOverride : ItemStateCriteria
{
	[XmlElement(ElementName = "ItemID")]
	public int ItemID { get; set; }

	[XmlElement(ElementName = "ConsumeUses")]
	public bool ConsumeUses { get; set; }

	[XmlElement(ElementName = "Amount")]
	public int Amount { get; set; }
}
