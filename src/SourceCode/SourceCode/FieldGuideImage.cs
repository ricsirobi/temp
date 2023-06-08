using System;
using System.Xml.Serialization;

[Serializable]
public class FieldGuideImage : FieldGuideItem
{
	[XmlElement(ElementName = "Name")]
	public LocaleString Name;
}
