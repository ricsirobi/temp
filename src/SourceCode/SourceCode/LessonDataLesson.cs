using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "LessonDataLesson", Namespace = "")]
public class LessonDataLesson
{
	[XmlElement(ElementName = "ActivityId")]
	public int ActivityId;

	[XmlElement(ElementName = "LessonId")]
	public int LessonId;

	[XmlElement(ElementName = "SkillAddress")]
	public string SkillAddress;

	[XmlElement(ElementName = "Level")]
	public int Level;

	[XmlElement(ElementName = "SubLevelId")]
	public int SubLevelId;

	[XmlElement(ElementName = "NumQuestions")]
	public int NumQuestions;

	[XmlElement(ElementName = "Status")]
	public int Status;
}
