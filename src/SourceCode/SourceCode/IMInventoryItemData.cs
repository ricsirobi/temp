using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "IMInventoryItemData", Namespace = "")]
public class IMInventoryItemData
{
	[XmlElement(ElementName = "AssetName")]
	public string AssetName;

	[XmlElement(ElementName = "Icon")]
	public string Icon;

	[XmlElement(ElementName = "Asset")]
	public string Asset;

	[XmlElement(ElementName = "Quantity")]
	public int Quantity;
}
