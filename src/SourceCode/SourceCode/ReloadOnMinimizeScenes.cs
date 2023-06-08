using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ReloadOnMinimizeScenes", Namespace = "")]
public class ReloadOnMinimizeScenes
{
	[XmlElement(ElementName = "SceneNames")]
	public string[] SceneNames;
}
