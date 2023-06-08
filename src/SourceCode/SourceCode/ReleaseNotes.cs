using System;
using System.Xml.Serialization;

[Serializable]
public class ReleaseNotes
{
	[XmlElement(ElementName = "Title")]
	public string Title;

	[XmlElement(ElementName = "SubTitle")]
	public string SubTitle;

	[XmlElement(ElementName = "ContentList")]
	public string[] ContentList;
}
