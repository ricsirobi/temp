using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "RPST", Namespace = "")]
public class RaisedPetState
{
	[XmlElement(ElementName = "k")]
	public string Key;

	[XmlElement(ElementName = "v")]
	public float Value;
}
