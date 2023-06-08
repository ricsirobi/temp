using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "DisplayNameUniqueResponse", Namespace = "")]
public class DisplayNameUniqueResponse
{
	[XmlElement(ElementName = "suggestions", IsNullable = true)]
	public SuggestionResult Suggestions { get; set; }
}
