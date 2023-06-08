using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ArrayOfImageDataComplete", Namespace = "")]
public class ImageDataComplete : ImageData
{
	[XmlElement(ElementName = "ProductID")]
	public int ProductID;
}
