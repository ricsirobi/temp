using System;
using System.Xml.Serialization;

[Serializable]
public class MovieData
{
	[XmlElement(ElementName = "Type")]
	public string Type;

	[XmlElement(ElementName = "Movie")]
	public Movie[] Movie;
}
