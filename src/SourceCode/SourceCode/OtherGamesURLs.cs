using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "urls", Namespace = "")]
public class OtherGamesURLs
{
	[XmlElement(ElementName = "url")]
	public string mURL;

	[XmlElement(ElementName = "Priority")]
	public int mPriority;
}
