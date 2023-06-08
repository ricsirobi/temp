using System;
using System.Xml.Serialization;

[Serializable]
public class LabSubstanceProperty
{
	[XmlElement(ElementName = "Name")]
	public string Name;

	[XmlElement(ElementName = "Min")]
	public string Min;

	[XmlElement(ElementName = "Max")]
	public string Max;

	[XmlElement(ElementName = "Type")]
	public string Type;

	public bool pInitialized;

	public bool pIsSubstance = true;

	[NonSerialized]
	public object pMin;

	public object pMax;
}
