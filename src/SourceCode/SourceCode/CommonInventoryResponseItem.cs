using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "CIRI", Namespace = "")]
public class CommonInventoryResponseItem
{
	[XmlElement(ElementName = "iid")]
	public int ItemID;

	[XmlElement(ElementName = "cid")]
	public int CommonInventoryID;

	[XmlElement(ElementName = "qty")]
	public int Quantity;
}
