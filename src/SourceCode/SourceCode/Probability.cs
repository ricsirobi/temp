using System;
using System.Xml.Serialization;

[Serializable]
public class Probability
{
	[XmlAttribute("key")]
	public string key = string.Empty;

	[XmlText]
	public string Val;
}
