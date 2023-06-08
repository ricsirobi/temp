using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "MasteryDataLesson", Namespace = "")]
public class MasteryDataLesson
{
	[XmlElement(ElementName = "ActivityId")]
	public int ActivityId;

	[XmlElement(ElementName = "LessonId")]
	public int LessonId;
}
