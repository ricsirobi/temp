using System;
using System.Xml.Serialization;

[Serializable]
public class FieldGuideSubject : FieldGuideItemName
{
	[XmlElement(ElementName = "Topic")]
	public FieldGuideTopic[] Topics;
}
