using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ImageSetResult", Namespace = "")]
public class ImageSetResult
{
	[XmlElement(ElementName = "Success")]
	public bool Success { get; set; }

	[XmlElement(ElementName = "ImageType")]
	public string ImageType { get; set; }

	[XmlElement(ElementName = "ImageSlot")]
	public byte ImageSlot { get; set; }

	[XmlElement(ElementName = "ImageURL")]
	public string ImageURL { get; set; }
}
