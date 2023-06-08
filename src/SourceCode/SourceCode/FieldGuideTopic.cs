using System;
using System.Xml.Serialization;

[Serializable]
public class FieldGuideTopic : FieldGuideItemName
{
	[XmlAttribute("iconname")]
	public string IconName;

	[XmlElement(ElementName = "SubTopic")]
	public FieldGuideSubTopic[] SubTopics;
}
