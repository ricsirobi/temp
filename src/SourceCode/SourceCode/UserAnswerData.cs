using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "Answer", IsNullable = true, Namespace = "")]
public class UserAnswerData
{
	[XmlElement(ElementName = "ID")]
	public string UserID;

	[XmlElement(ElementName = "Answers")]
	public ProfileUserAnswer[] Answers;
}
