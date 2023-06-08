using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "BuildSpot", Namespace = "")]
public class BuildSpot
{
	[XmlElement(ElementName = "Name")]
	public string Name;

	[XmlElement(ElementName = "Item")]
	public string Item;
}
