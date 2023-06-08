using System.Xml.Serialization;

public class FieldGuideVocabulary : FieldGuideItem
{
	[XmlElement(ElementName = "Definition")]
	public LocaleString Definition;
}
