using System;
using System.Xml.Serialization;

[Serializable]
public class DisciplinaryData
{
	[XmlElement(ElementName = "Title")]
	public LocaleString Title;

	[XmlElement(ElementName = "DisciplinaryDetail")]
	public DisciplinaryCoreDetail[] Details;
}
