using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "QuestionData", IsNullable = true, Namespace = "")]
public class ProfileQuestionData
{
	[XmlElement(ElementName = "QL")]
	public ProfileQuestionList[] Lists;

	public const int ANSWER_BOY = 227;

	public const int ANSWER_GIRL = 228;

	public const int ANSWER_UNKNOWN = 229;

	public const int ANSWER_TRUE = 32;

	public const int ANSWER_FALSE = 33;

	public const int COUNTRY_QUESTIONS = 33;

	public const int GENDER_QUESTIONS = 32;

	public const int LIST_ID_FAVORITES = 1;

	public const int LIST_ID_STATUS = 2;

	public const int LIST_ID_GROUPS = 3;

	public static int ConvertGenderAnswerID(Gender gen)
	{
		return gen switch
		{
			Gender.Male => 227, 
			Gender.Female => 228, 
			_ => 229, 
		};
	}

	public void InitTestData()
	{
		Lists = new ProfileQuestionList[3];
		Lists[0] = GetTestFavoriteList();
		Lists[1] = GetTestStatusList();
		Lists[2] = GetTestGroupList();
	}

	public ProfileQuestionList GetTestStatusList()
	{
		return new ProfileQuestionList();
	}

	public ProfileQuestionList GetTestGroupList()
	{
		return new ProfileQuestionList();
	}

	public ProfileQuestionList GetTestFavoriteList()
	{
		return new ProfileQuestionList();
	}
}
