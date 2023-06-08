using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "IRE", Namespace = "")]
public class ItemDataRelationship
{
	[XmlElement(ElementName = "t")]
	public string Type;

	[XmlElement(ElementName = "id")]
	public int ItemId;

	[XmlElement(ElementName = "wt")]
	public int Weight;

	[XmlElement(ElementName = "q")]
	public int Quantity;
}
