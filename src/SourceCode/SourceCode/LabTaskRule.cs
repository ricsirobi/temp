using System;
using System.Xml.Serialization;

[Serializable]
public class LabTaskRule
{
	[XmlElement(ElementName = "Name")]
	public string Name;

	[XmlElement(ElementName = "Value")]
	public string Value;

	[XmlElement(ElementName = "AdditionalValue")]
	public string AdditionalValue;
}
