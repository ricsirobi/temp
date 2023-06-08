using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "RPAT", Namespace = "")]
public class RaisedPetAttribute
{
	[XmlElement(ElementName = "k")]
	public string Key;

	[XmlElement(ElementName = "v")]
	public string Value;

	[XmlElement(ElementName = "dt")]
	public DataType Type;
}
