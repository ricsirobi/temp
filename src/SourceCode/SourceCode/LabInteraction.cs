using System;
using System.Xml.Serialization;

[Serializable]
public class LabInteraction
{
	[XmlElement(ElementName = "Name")]
	public string Name;

	[XmlElement(ElementName = "Duration")]
	public float Duration;

	[XmlElement(ElementName = "SubstanceProperty")]
	public LabSubstanceProperty[] Properties;

	[XmlElement(ElementName = "ShaderInterpolation")]
	public LabShaderInterpolation[] Interpolations;
}
