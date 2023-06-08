using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "AvatarDataPartOffset", Namespace = "")]
public class AvatarDataPartOffset
{
	public float X;

	public float Y;

	public float Z;
}
