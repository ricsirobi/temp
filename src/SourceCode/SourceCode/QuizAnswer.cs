using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "QuizAnswer")]
public class QuizAnswer
{
	[XmlElement(ElementName = "AnswerText")]
	public LocaleString AnswerText;

	[XmlElement(ElementName = "SelectedResponseText")]
	public LocaleString _SelectedResponseText;

	[XmlElement(ElementName = "IsCorrect")]
	public bool IsCorrect;

	[XmlElement(ElementName = "ImageURL")]
	public string ImageURL;

	[XmlElement(ElementName = "SpriteName")]
	public string _SpriteName;
}
