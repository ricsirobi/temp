using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "MasteryData", Namespace = "", IsNullable = false)]
public class MasteryData
{
	[XmlElement(ElementName = "Lesson")]
	public MasteryDataLesson[] Lesson;

	private static MasteryData mInstance;

	private static bool mInitialized;

	private static List<MasteryDataLesson> mLessonList;

	public static MasteryData pInstance
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

	public static List<MasteryDataLesson> pLessonList
	{
		get
		{
			return mLessonList;
		}
		set
		{
			mLessonList = value;
		}
	}

	public static void Init()
	{
		if (!mInitialized)
		{
			mInitialized = true;
			WsWebService.GetMasteryData(ServiceEventHandler, null);
		}
	}

	public static void InitDefault()
	{
		mInstance = new MasteryData();
		mInstance.Lesson = null;
		mLessonList = new List<MasteryDataLesson>();
	}

	public static void Save()
	{
		if (pIsReady)
		{
			mInstance.Lesson = mLessonList.ToArray();
			WsWebService.SetMasteryData(mInstance, ServiceEventHandler, null);
		}
	}

	private static void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType != WsServiceType.GET_LESSON_MASTERY)
		{
			return;
		}
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			mInstance = (MasteryData)inObject;
			if (mInstance != null && mInstance.Lesson != null)
			{
				mLessonList = new List<MasteryDataLesson>(mInstance.Lesson);
				break;
			}
			UtDebug.Log("WEB SERVICE CALL GetMasteryData RETURNED NO DATA!!!");
			InitDefault();
			break;
		case WsServiceEvent.ERROR:
			UtDebug.LogError("WEB SERVICE CALL GetMasteryData FAILED!!!");
			InitDefault();
			break;
		}
	}

	public static int AddLesson(int actID, int lessonID)
	{
		if (mInstance != null && !IsMastered(actID, lessonID))
		{
			MasteryDataLesson masteryDataLesson = new MasteryDataLesson();
			masteryDataLesson.ActivityId = actID;
			masteryDataLesson.LessonId = lessonID;
			mLessonList.Add(masteryDataLesson);
			Save();
			return mLessonList.Count;
		}
		return -1;
	}

	public static bool IsMastered(int actID, int lessonID)
	{
		if (mInstance != null)
		{
			for (int i = 0; i < mLessonList.Count; i++)
			{
				if (mLessonList[i].ActivityId == actID && mLessonList[i].LessonId == lessonID)
				{
					return true;
				}
			}
		}
		return false;
	}
}
