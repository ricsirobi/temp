using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "Description", Namespace = "", IsNullable = true)]
public class Description
{
	[XmlElement(ElementName = "value")]
	public string Value;
}
