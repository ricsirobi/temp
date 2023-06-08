using System;
using System.Xml.Serialization;

[Serializable]
public class LabShaderValue
{
	[XmlElement(ElementName = "Name")]
	public string Name;

	[XmlElement(ElementName = "Value")]
	public float Value;
}
