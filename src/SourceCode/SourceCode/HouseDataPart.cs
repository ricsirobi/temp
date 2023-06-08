using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "HouseDataPart", Namespace = "")]
public class HouseDataPart
{
	[XmlElement(ElementName = "PartTypes")]
	public string PartTypes;

	[XmlElement(ElementName = "PartNames")]
	public string PartNames;
}
