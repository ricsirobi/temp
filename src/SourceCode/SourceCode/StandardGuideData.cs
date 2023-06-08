using System;
using System.Xml.Serialization;

[Serializable]
public class StandardGuideData
{
	[XmlElement(ElementName = "Topic")]
	public LocaleString Topic;

	[XmlElement(ElementName = "QuestData")]
	public StandardQuestData[] QuestDatas;
}
