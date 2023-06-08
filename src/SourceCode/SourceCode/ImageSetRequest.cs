using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ImageSetRequest", Namespace = "")]
public class ImageSetRequest
{
	[XmlElement(ElementName = "UserID")]
	public string UserID { get; set; }

	[XmlElement(ElementName = "ParentID")]
	public string ParentID { get; set; }

	[XmlElement(ElementName = "ProductID")]
	public int? ProductID { get; set; }

	[XmlElement(ElementName = "ImageType")]
	public string ImageType { get; set; }

	[XmlElement(ElementName = "ImageSlot")]
	public byte ImageSlot { get; set; }

	[XmlElement(ElementName = "ImageFile")]
	public byte[] ImageFile { get; set; }

	[XmlElement(ElementName = "ContentXml")]
	public string ContentXml { get; set; }
}
