using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "RPEM", Namespace = "")]
public class RaisedPetEntityMap
{
	[XmlElement(ElementName = "RPID")]
	public int RaisedPetID { get; set; }

	[XmlElement(ElementName = "EID")]
	public Guid? EntityID { get; set; }
}
