using System;
using System.Xml.Serialization;

[Serializable]
public class FieldGuideLesson : FieldGuideItemName
{
	[XmlElement(ElementName = "Chapter")]
	public FieldGuideChapter[] Chapters;

	public bool IsUnlocked()
	{
		FieldGuideChapter[] chapters = Chapters;
		for (int i = 0; i < chapters.Length; i++)
		{
			if (chapters[i].IsUnlocked())
			{
				return true;
			}
		}
		return false;
	}
}
