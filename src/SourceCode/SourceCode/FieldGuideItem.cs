using System.Xml.Serialization;

public class FieldGuideItem : GuideLockData
{
	[XmlElement(ElementName = "Data")]
	public LocaleString Data;
}
