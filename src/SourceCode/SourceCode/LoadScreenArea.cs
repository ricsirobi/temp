using System;
using System.Xml.Serialization;

[Serializable]
public class LoadScreenArea
{
	[XmlAttribute("X")]
	public int X;

	[XmlAttribute("Y")]
	public int Y;

	[XmlAttribute("Width")]
	public int Width;

	[XmlAttribute("Height")]
	public int Height;

	[XmlText]
	public string URL;
}
