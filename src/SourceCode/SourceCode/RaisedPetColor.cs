using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "RPC", Namespace = "")]
public class RaisedPetColor
{
	[XmlElement(ElementName = "o")]
	public int Order;

	[XmlElement(ElementName = "r")]
	public float Red;

	[XmlElement(ElementName = "g")]
	public float Green;

	[XmlElement(ElementName = "b")]
	public float Blue;
}
