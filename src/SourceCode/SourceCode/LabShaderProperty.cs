using System;
using System.Xml.Serialization;

[Serializable]
public class LabShaderProperty
{
	[XmlElement(ElementName = "Name")]
	public string Name;

	[XmlElement(ElementName = "Type")]
	public LabShaderDataType Type;

	[XmlElement(ElementName = "Value")]
	public string Value;
}
