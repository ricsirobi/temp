using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "SuggestionResult", Namespace = "", IsNullable = false)]
public class SuggestionResult
{
	[XmlElement(ElementName = "Suggestion", IsNullable = true)]
	public string[] Suggestion;
}
