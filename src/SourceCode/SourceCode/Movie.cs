using System;
using System.Xml.Serialization;

[Serializable]
public class Movie
{
	[XmlElement(ElementName = "Name")]
	public string Name;

	[XmlElement(ElementName = "Repeat", IsNullable = true)]
	public int? Repeat;

	[XmlElement(ElementName = "Achievement", IsNullable = true)]
	public int? Achievement;

	[XmlElement(ElementName = "SceneName", IsNullable = true)]
	public string SceneName;
}
