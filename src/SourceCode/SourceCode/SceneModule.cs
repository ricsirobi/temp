using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "Module", Namespace = "")]
public class SceneModule
{
	[XmlElement(ElementName = "Name")]
	public string Name;

	[XmlElement(ElementName = "Stars")]
	public int Stars;
}
