using System;
using System.Xml.Serialization;

[Serializable]
public class AvatarHeadData
{
	[XmlArray("NoBlinkGeometries")]
	public string[] NoBlinkGeometries;
}
