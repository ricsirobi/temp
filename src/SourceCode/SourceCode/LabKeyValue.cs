using System;
using System.Xml.Serialization;

[Serializable]
public class LabKeyValue
{
	[XmlElement(ElementName = "Name")]
	public string Name;

	[XmlElement(ElementName = "Value")]
	public string Value;
}
