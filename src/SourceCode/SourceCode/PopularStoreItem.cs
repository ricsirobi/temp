using System;
using System.Xml.Serialization;

[Serializable]
public class PopularStoreItem
{
	[XmlElement(ElementName = "id")]
	public int ItemID;

	[XmlElement(ElementName = "c")]
	public int Rank;
}
