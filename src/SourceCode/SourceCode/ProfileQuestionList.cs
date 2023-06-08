using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "QL", IsNullable = true, Namespace = "")]
public class ProfileQuestionList
{
	[XmlElement(ElementName = "ID", IsNullable = false)]
	public int ID;

	[XmlElement(ElementName = "Qs")]
	public ProfileQuestion[] Questions;

	public string Name;

	public string ImageURL;

	public ProfileQuestion GetQuestion(string s)
	{
		ProfileQuestion[] questions = Questions;
		foreach (ProfileQuestion profileQuestion in questions)
		{
			if (profileQuestion.DisplayText == s)
			{
				return profileQuestion;
			}
		}
		return null;
	}

	public ProfileQuestion GetQuestion(int qid)
	{
		ProfileQuestion[] questions = Questions;
		foreach (ProfileQuestion profileQuestion in questions)
		{
			if (profileQuestion.ID == qid)
			{
				return profileQuestion;
			}
		}
		return null;
	}
}
