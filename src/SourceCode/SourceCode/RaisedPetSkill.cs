using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "RPSK", Namespace = "")]
public class RaisedPetSkill
{
	[XmlElement(ElementName = "k")]
	public string Key;

	[XmlElement(ElementName = "v")]
	public float Value;

	[XmlElement(ElementName = "ud")]
	public DateTime UpdateDate;
}
