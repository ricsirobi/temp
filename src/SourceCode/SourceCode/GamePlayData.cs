using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "GMPD", Namespace = "")]
public class GamePlayData
{
	[XmlElement(ElementName = "GMID")]
	public int GameID;

	[XmlElement(ElementName = "PLCT")]
	public int PlayCount;
}
