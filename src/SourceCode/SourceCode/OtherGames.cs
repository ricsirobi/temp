using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "OtherGames", Namespace = "")]
public class OtherGames
{
	[XmlElement(ElementName = "Name")]
	public string mName = "";

	[XmlElement(ElementName = "DefaultUrl")]
	public string mDefaultURL = "";

	[XmlElement(ElementName = "urls")]
	public OtherGamesURLs[] mURLs;

	[XmlElement(ElementName = "IconAssetBundle")]
	public string mIcon = "";

	[XmlElement(ElementName = "IconHighlight")]
	public string mIconHighlight = "";

	[XmlElement(ElementName = "IconNormal")]
	public string mIconNormal = "";
}
