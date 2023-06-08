using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ItemStateCriteriaExpiry", Namespace = "")]
public class ItemStateCriteriaExpiry : ItemStateCriteria
{
	[XmlElement(ElementName = "Period")]
	public int Period;

	[XmlElement(ElementName = "EndStateID")]
	public int EndStateID;
}
