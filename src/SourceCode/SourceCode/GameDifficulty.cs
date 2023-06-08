using System.Collections.Generic;
using System.Xml.Serialization;

[XmlRoot(ElementName = "difficulty")]
public class GameDifficulty
{
	[XmlElement(ElementName = "name")]
	public string Name;

	[XmlElement(ElementName = "level")]
	public GameLevel[] Level;

	[XmlElement(ElementName = "custom")]
	public string Custom;

	internal Dictionary<string, string> CustomDict = new Dictionary<string, string>();
}
