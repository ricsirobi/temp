using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "NestData", Namespace = "")]
public class NestData
{
	[XmlElement(ElementName = "PetID")]
	public int PetID;

	[XmlElement(ElementName = "ID")]
	public int ID;
}
