using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "SetImagesResponse", Namespace = "")]
public class SetImagesResponse
{
	[XmlElement(ElementName = "ImageSetResults")]
	public ImageSetResult[] ImageSetResults { get; set; }
}
