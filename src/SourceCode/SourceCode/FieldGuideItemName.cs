using System;
using System.Xml.Serialization;

[Serializable]
public class FieldGuideItemName
{
	[XmlElement(ElementName = "Name")]
	public LocaleString Name;
}
