using System;
using System.Xml.Serialization;

[Serializable]
public class LabShaderInterpolation
{
	[XmlElement(ElementName = "Name")]
	public string Name;

	[XmlElement(ElementName = "Min")]
	public float Min;

	[XmlElement(ElementName = "Max")]
	public float Max;
}
