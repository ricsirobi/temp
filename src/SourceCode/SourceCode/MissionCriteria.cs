using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "MissionCriteria", Namespace = "")]
public class MissionCriteria
{
	[XmlElement(ElementName = "Type")]
	public string Type;

	[XmlElement(ElementName = "Ordered")]
	public bool Ordered;

	[XmlElement(ElementName = "Min")]
	public int Min;

	[XmlElement(ElementName = "Repeat")]
	public int Repeat;

	[XmlElement(ElementName = "RuleItems")]
	public List<RuleItem> RuleItems;

	public MissionCriteria()
	{
		Type = "all";
		Ordered = false;
		Min = 1;
		Repeat = 1;
		RuleItems = new List<RuleItem>();
	}
}
