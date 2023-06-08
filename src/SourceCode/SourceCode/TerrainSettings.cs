using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
public class TerrainSettings
{
	[XmlAttribute]
	public string BundleType;

	[XmlElement(ElementName = "TerrainSetting")]
	public List<TerrainSetting> TerrainSetting;
}
