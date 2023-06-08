using System;
using System.Xml.Serialization;

[Serializable]
public class LSAge
{
	[XmlAttribute("min")]
	public int Min = -1;

	[XmlAttribute("max")]
	public int Max = -1;
}
