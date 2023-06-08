using System;
using System.Xml.Serialization;

[Serializable]
public class PlatformSettings
{
	[XmlAttribute]
	public string Name;

	[XmlElement(ElementName = "Platform")]
	public string[] Platform;

	[XmlElement(ElementName = "GPU")]
	public string[] GPU;

	[XmlElement(ElementName = "MaxMMOData")]
	public MaxMMOData MaxMMOData;

	[XmlElement(ElementName = "GraphicsSettings")]
	public GraphicsSettings GraphicsSettings;

	[XmlElement(ElementName = "MMODefaultState")]
	public bool MMODefaultState = true;

	[XmlElement(ElementName = "DownloadTextureSize")]
	public int DownloadTextureSize = 256;
}
