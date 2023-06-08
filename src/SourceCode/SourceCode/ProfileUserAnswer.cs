using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "Answers", IsNullable = true, Namespace = "")]
public class ProfileUserAnswer
{
	[XmlElement(ElementName = "AID")]
	public int AnswerID;

	[XmlElement(ElementName = "QID")]
	public int QuestionID;

	public ProfileUserAnswer()
	{
	}

	public ProfileUserAnswer(int qid, int aid)
	{
		QuestionID = qid;
		AnswerID = aid;
	}

	public ProfileUserAnswer(ProfileUserAnswer ins)
	{
		QuestionID = ins.QuestionID;
		AnswerID = ins.AnswerID;
	}
}
