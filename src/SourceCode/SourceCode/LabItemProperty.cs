using System;
using System.Xml.Serialization;

[Serializable]
public class LabItemProperty
{
	[XmlElement(ElementName = "Key")]
	public string Key;

	[XmlElement(ElementName = "Value")]
	public string Value;
}
