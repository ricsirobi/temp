using System;
using System.Xml.Serialization;

[Serializable]
public class CommonInventoryConsumable
{
	[XmlElement(ElementName = "CIID")]
	public int CommonInventoryID;

	[XmlElement(ElementName = "IID")]
	public int ItemID;
}
