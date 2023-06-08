using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "PrerequisiteItem", Namespace = "")]
public class PrerequisiteItem
{
	[XmlElement(ElementName = "Type")]
	public PrerequisiteRequiredType Type;

	[XmlElement(ElementName = "Value")]
	public string Value;

	[XmlElement(ElementName = "Quantity")]
	public short Quantity;

	[XmlElement(ElementName = "ClientRule")]
	public bool ClientRule;

	public PrerequisiteItem()
	{
	}

	public PrerequisiteItem(PrerequisiteRequiredType type, object value)
	{
		Type = type;
		Value = value.ToString();
	}
}
