using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ImageDataDecal", Namespace = "")]
public class ImageDataDecal
{
	[XmlElement(ElementName = "Name")]
	public string Name;

	[XmlElement(ElementName = "Position")]
	public ImageDataDecalPosition Position;

	[XmlElement(ElementName = "Width")]
	public int Width;

	[XmlElement(ElementName = "Height")]
	public int Height;
}
