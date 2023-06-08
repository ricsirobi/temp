using System;
using System.Xml.Serialization;

[Serializable]
public class PreviewAssetData
{
	[XmlElement(ElementName = "Name")]
	public string Name;

	[XmlElement(ElementName = "Size")]
	public string Size;
}
