using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "RPGS", Namespace = "")]
public class RaisedPetGrowthState
{
	[XmlElement(ElementName = "id")]
	public int GrowthStateID;

	[XmlElement(ElementName = "n")]
	public string Name;

	[XmlElement(ElementName = "ptid")]
	public int PetTypeID;

	[XmlElement(ElementName = "o")]
	public int Order;
}
