using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ImageSetRequests", Namespace = "")]
public class ImageSetRequests
{
	[XmlElement(ElementName = "ImageRequests")]
	public ImageSetRequest[] ImageRequests { get; set; }
}
