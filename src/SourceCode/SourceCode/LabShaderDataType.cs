using System;
using System.Xml.Serialization;

[Serializable]
public enum LabShaderDataType
{
	[XmlEnum("TEXTURE")]
	Texture,
	[XmlEnum("FLOAT")]
	Float,
	[XmlEnum("COLOR")]
	COLOR
}
