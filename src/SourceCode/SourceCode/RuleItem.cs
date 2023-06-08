using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "RuleItem", Namespace = "")]
public class RuleItem
{
	[XmlElement(ElementName = "Type")]
	public RuleItemType Type;

	[XmlElement(ElementName = "MissionID")]
	public int MissionID;

	[XmlElement(ElementName = "ID")]
	public int ID;

	[XmlElement(ElementName = "Complete")]
	public int Complete;
}
