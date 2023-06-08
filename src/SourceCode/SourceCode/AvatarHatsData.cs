using System;
using System.Xml.Serialization;

[Serializable]
public class AvatarHatsData
{
	[XmlArray("NoHairHats")]
	public string[] NoHairHats;

	[XmlArray("NoHairScaleHats")]
	public string[] NoHairScaleHats;
}
