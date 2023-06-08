using System;
using System.Xml.Serialization;

[Serializable]
public class LocationChange
{
	[XmlElement(ElementName = "PosX")]
	public float PosX;

	[XmlElement(ElementName = "PosY")]
	public float PosY;

	[XmlElement(ElementName = "PosZ")]
	public float PosZ;
}
