using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ItemStateCriteriaLength", Namespace = "")]
public class ItemStateCriteriaLength : ItemStateCriteria
{
	[XmlElement(ElementName = "Period")]
	public int Period;
}
