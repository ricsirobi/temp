using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "PrerequisiteItem", Namespace = "")]
public class PrerequisiteItemUtil
{
	[XmlElement(ElementName = "Type")]
	public PrerequisiteRequiredTypeUtil Type;

	[XmlElement(ElementName = "Value")]
	public string Value;

	[XmlElement(ElementName = "Quantity")]
	public int Quantity;
}
