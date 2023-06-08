using System;
using System.Xml.Serialization;

[Serializable]
public class FieldGuideSubTopic : FieldGuideItemName
{
	[XmlElement(ElementName = "Lesson")]
	public FieldGuideLesson[] Lessons;

	public bool IsUnlocked()
	{
		FieldGuideLesson[] lessons = Lessons;
		for (int i = 0; i < lessons.Length; i++)
		{
			if (lessons[i].IsUnlocked())
			{
				return true;
			}
		}
		return false;
	}
}
