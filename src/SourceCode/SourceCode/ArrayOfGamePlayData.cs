using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ArrayOfGamePlayData", Namespace = "http://api.jumpstart.com/")]
public class ArrayOfGamePlayData
{
	[XmlElement(ElementName = "GamePlayData")]
	public GamePlayData[] GamePlayData;
}
