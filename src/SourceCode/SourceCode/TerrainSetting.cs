using System;
using System.Xml.Serialization;

[Serializable]
public class TerrainSetting
{
	[XmlAttribute]
	public string Name;

	[XmlElement(ElementName = "BaseMapDistance")]
	public float BaseMapDistance;

	[XmlElement(ElementName = "HeightmapPixelError")]
	public float HeightmapPixelError;

	[XmlElement(ElementName = "DetailObjectDistance")]
	public float DetailObjectDistance;

	[XmlElement(ElementName = "DetailObjectDensity")]
	public float DetailObjectDensity;

	[XmlElement(ElementName = "TreeDistance")]
	public float TreeDistance;

	[XmlElement(ElementName = "TreeBillboardDistance")]
	public float TreeBillboardDistance;

	[XmlElement(ElementName = "TreeCrossFadeLength")]
	public float TreeCrossFadeLength;

	[XmlElement(ElementName = "TreeMaximumFullLODCount")]
	public int TreeMaximumFullLODCount;
}
