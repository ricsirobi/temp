using System.Collections.Generic;
using System.Xml.Serialization;

[XmlRoot(ElementName = "chapter")]
public class GameChapter
{
	[XmlElement(ElementName = "name")]
	public string Name;

	[XmlElement(ElementName = "difficulty")]
	public GameDifficulty[] Difficulty;

	[XmlElement(ElementName = "custom")]
	public string Custom;

	internal Dictionary<string, string> CustomDict = new Dictionary<string, string>();
}
