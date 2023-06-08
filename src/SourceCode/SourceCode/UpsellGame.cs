using System;
using System.Xml.Serialization;

[Serializable]
public class UpsellGame
{
	[XmlAttribute("type")]
	public string type = string.Empty;

	[XmlElement(ElementName = "Rule")]
	public UpsellRule[] Rule;

	public bool IsGameTypeMatching(string inType)
	{
		string text = type.ToLower();
		string text2 = inType.ToLower();
		return text == text2;
	}
}
