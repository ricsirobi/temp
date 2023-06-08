using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ImageDataDecalPosition", Namespace = "")]
public class ImageDataDecalPosition
{
	[XmlElement(ElementName = "X")]
	public int X;

	[XmlElement(ElementName = "Y")]
	public int Y;
}
