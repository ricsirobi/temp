using System.Collections.Generic;
using System.Xml.Serialization;

[XmlRoot(ElementName = "level")]
public class GameLevel
{
	[XmlElement(ElementName = "name")]
	public string Name;

	[XmlElement(ElementName = "score")]
	public int Score;

	[XmlElement(ElementName = "star")]
	public int Star;

	[XmlElement(ElementName = "custom")]
	public string Custom;

	internal Dictionary<string, string> CustomDict = new Dictionary<string, string>();
}
