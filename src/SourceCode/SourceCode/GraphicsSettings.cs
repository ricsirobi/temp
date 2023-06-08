using System;
using System.Xml.Serialization;

[Serializable]
public class GraphicsSettings
{
	[XmlElement(ElementName = "Texture")]
	public GraphicsProperty TextureSettings;

	[XmlElement(ElementName = "Shadow")]
	public GraphicsProperty ShadowSettings;

	[XmlElement(ElementName = "Effects")]
	public GraphicsProperty EffectSettings;
}
