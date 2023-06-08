using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "MissionReward", Namespace = "")]
public class MissionReward
{
	[XmlElement(ElementName = "Asset")]
	public string Asset;
}
