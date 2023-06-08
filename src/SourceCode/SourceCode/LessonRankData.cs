using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "LessonRankData", Namespace = "")]
public class LessonRankData
{
	[XmlElement(ElementName = "UserID")]
	public Guid UserID;

	[XmlElement(ElementName = "DisplayName")]
	public string DisplayName;

	[XmlElement(ElementName = "CorrectAnswers")]
	public int CorrectAnswers;
}
