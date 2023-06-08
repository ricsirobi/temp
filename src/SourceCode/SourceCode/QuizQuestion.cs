using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "QuizQuestion", Namespace = "")]
public class QuizQuestion
{
	[XmlElement(ElementName = "ID")]
	public string ID;

	[XmlElement(ElementName = "Type")]
	public string Type;

	[XmlElement(ElementName = "Category")]
	public string[] Category;

	[XmlElement(ElementName = "QuestionText")]
	public LocaleString QuestionText;

	[XmlElement(ElementName = "QuestionInstructionText")]
	public LocaleString QuestionInstructionText;

	[XmlElement(ElementName = "CorrectAnswerText")]
	public LocaleString CorrectAnswerText;

	[XmlElement(ElementName = "IncorrectAnswerText")]
	public LocaleString IncorrectAnswerText;

	[XmlElement(ElementName = "ImageURL")]
	public string ImageURL;

	[XmlElement(ElementName = "Answers")]
	public QuizAnswer[] Answers;
}
