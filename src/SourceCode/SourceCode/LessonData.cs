using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "LessonData", Namespace = "")]
public class LessonData
{
	[XmlElement(ElementName = "Lesson")]
	public LessonDataLesson[] Lesson;

	private static LessonData mInstance;

	private static List<LessonDataLesson> mLessonResults;

	public static LessonData pInstance
	{
		get
		{
			if (!pIsReady)
			{
				Init();
			}
			return mInstance;
		}
	}

	public static bool pIsReady => mInstance != null;

	public static List<LessonDataLesson> pLessonResults
	{
		get
		{
			return mLessonResults;
		}
		set
		{
			mLessonResults = value;
		}
	}

	public static void Init()
	{
		if (mInstance == null)
		{
			mInstance = new LessonData();
			mInstance.Lesson = null;
			if (mInstance.Lesson == null)
			{
				mLessonResults = new List<LessonDataLesson>();
			}
			else
			{
				mLessonResults = new List<LessonDataLesson>(mInstance.Lesson);
			}
		}
	}

	public static LessonDataLesson[] ToArray()
	{
		return mLessonResults.ToArray();
	}

	public static void AddResult(int actID, int lessonID, string skillAddress, int level, int subLevelId, int numQuestions, bool bCorrect)
	{
		LessonDataLesson lessonDataLesson = new LessonDataLesson();
		lessonDataLesson.ActivityId = actID;
		lessonDataLesson.LessonId = lessonID;
		lessonDataLesson.SkillAddress = skillAddress;
		lessonDataLesson.Level = level;
		lessonDataLesson.SubLevelId = subLevelId;
		lessonDataLesson.NumQuestions = numQuestions;
		lessonDataLesson.Status = (bCorrect ? 1 : 0);
		mLessonResults.Add(lessonDataLesson);
	}
}
