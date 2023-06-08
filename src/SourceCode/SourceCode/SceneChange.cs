using System;
using System.Xml.Serialization;

[Serializable]
public class SceneChange
{
	[XmlElement(ElementName = "Scene")]
	public string Scene;

	[XmlElement(ElementName = "IsRace")]
	public bool IsRace;
}
