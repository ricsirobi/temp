using System.Collections.Generic;
using UnityEngine;

public class CmInterface
{
	private bool mInRound;

	private double mNumCorrect;

	private double mNumAttempts;

	private double mCurPercent;

	private bool mMasteredLesson;

	private bool mSetExposed;

	private List<string> pContentFiles;

	private int pCurrContentIndex;

	private string pCallback;

	private GameObject pTarget;

	private GameObject pFileReaderInstance;

	private UtData pDataFile;

	private Dictionary<CmRK, int> mSuccess;

	private Dictionary<CmRK, int> mFailure;

	private List<LessonDataLesson> mLessonResults;

	public CmInterface()
	{
		mInRound = false;
		mNumCorrect = 0.0;
		mNumAttempts = 0.0;
		mMasteredLesson = false;
		mSetExposed = false;
	}

	public void InitCM(string actName, int pack, int packet, int lesson, bool bForceInit)
	{
		InitCM(null, null);
	}

	public void InitCM(GameObject inTarget, string contentCallback)
	{
		mInRound = false;
		mNumCorrect = 0.0;
		mNumAttempts = 0.0;
		mSetExposed = false;
		mMasteredLesson = false;
		pCallback = contentCallback;
		pTarget = inTarget;
		mSuccess = new Dictionary<CmRK, int>();
		mFailure = new Dictionary<CmRK, int>();
		RsResourceManager.Load("RS_CONTENT/" + CmLessonData.pContentFile, ContentListDone);
	}

	public void ContentListDone(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inFile, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			string fileData = (string)inFile;
			UtTableXMLReader utTableXMLReader = new UtTableXMLReader();
			utTableXMLReader.LoadString(fileData);
			UtTable utTable = utTableXMLReader[CmLessonData.pContentTableName];
			int fieldIndex = utTable.GetFieldIndex("tfFileName");
			int recordCount = utTable.GetRecordCount();
			pContentFiles = new List<string>();
			for (int i = 0; i < recordCount; i++)
			{
				string value = utTable.GetValue<string>(fieldIndex, i);
				pContentFiles.Add(value);
			}
			pContentFiles.Sort(CmContentReaders.RandomCompare);
			pCurrContentIndex = 0;
			if (pTarget != null)
			{
				pTarget.SendMessage(pCallback);
			}
		}
	}

	public void BeginRound()
	{
		mInRound = true;
		mNumCorrect = 0.0;
		mNumAttempts = 0.0;
		mSetExposed = false;
		mMasteredLesson = false;
		mSuccess.Clear();
		mFailure.Clear();
	}

	public bool EndRound(bool completed)
	{
		if (!completed)
		{
			return false;
		}
		mInRound = false;
		if (mSetExposed)
		{
			mMasteredLesson = true;
		}
		else if (mNumAttempts != 0.0)
		{
			mCurPercent = 100.0 * (mNumCorrect / mNumAttempts);
			if (mCurPercent >= CmLessonData.pTargetMasteryPercentage)
			{
				mMasteredLesson = true;
			}
			else
			{
				mMasteredLesson = false;
			}
		}
		else
		{
			mMasteredLesson = false;
		}
		CmLessonData.pCurMastered = mMasteredLesson;
		if (!mSetExposed)
		{
			mLessonResults = new List<LessonDataLesson>();
			if (mFailure.Count != 0)
			{
				foreach (KeyValuePair<CmRK, int> item in mFailure)
				{
					AddResult(CmLessonData.pActivityID, CmLessonData.pLessonID, item.Key.pSkillAddress, (int)item.Key.pSubLevelID, (int)item.Key.pLevel, item.Value, bCorrect: false);
				}
			}
			if (mSuccess.Count != 0)
			{
				foreach (KeyValuePair<CmRK, int> item2 in mSuccess)
				{
					AddResult(CmLessonData.pActivityID, CmLessonData.pLessonID, item2.Key.pSkillAddress, (int)item2.Key.pSubLevelID, (int)item2.Key.pLevel, item2.Value, bCorrect: true);
				}
			}
			WsWebService.SetLessonData(new LessonData
			{
				Lesson = mLessonResults.ToArray()
			}, null, null);
		}
		return mMasteredLesson;
	}

	private void AddResult(int actID, int lessonID, string skillAddress, int level, int subLevelId, int numQuestions, bool bCorrect)
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

	public int GetMinQuestionsPerRound()
	{
		return CmLessonData.pMinQuestionsPerRound;
	}

	public bool GetLessonEverCompleted()
	{
		return CmLessonData.pPrevMastered;
	}

	public bool GetLessonCompleted()
	{
		return mMasteredLesson;
	}

	public bool GetCurrRoundAchievedMasteryPct()
	{
		return mMasteredLesson;
	}

	public void LoadContentFile(string contentCallback)
	{
		pCallback = contentCallback;
		string text = pContentFiles[pCurrContentIndex];
		string text2 = "Captures/" + text;
		pCurrContentIndex++;
		if (pCurrContentIndex >= pContentFiles.Count)
		{
			pCurrContentIndex = 0;
		}
		RsResourceManager.Load("RS_CONTENT/" + text2, ContentDataDone);
	}

	public void ContentDataDone(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inFile, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			byte[] inResource = (byte[])inFile;
			pDataFile = new UtData();
			pDataFile.ReadFromResource(inResource, inReadOnly: true);
			if (pTarget != null)
			{
				pTarget.SendMessage(pCallback);
			}
		}
	}

	public TYPE GetContent<TYPE>(int numItems, bool bUnique, string parserName, string[] ActorName, string[] ImgSize, Dictionary<string, object> parserData) where TYPE : CmContentBase
	{
		switch (parserName)
		{
		case "MCContentTraditional":
		{
			int numDistract = (int)parserData["NumDistract"];
			return (TYPE)(CmContentBase)CmParsers.GetMCContentTraditional(pDataFile, numDistract, numItems, ImgSize, ActorName);
		}
		case "MCContentOneToOne":
			return (TYPE)(CmContentBase)CmParsers.GetMCContentOneToOne(pDataFile, numItems, ImgSize, ActorName);
		case "MCContentMany":
		{
			int numCorrect = (int)parserData["NumCorrect"];
			int numDistract = (int)parserData["NumDistract"];
			bool useDisplay = (bool)parserData["UseDisplay"];
			bool bAllowDups = (bool)parserData["AllowDups"];
			return (TYPE)(CmContentBase)CmParsers.GetMCContentMany(pDataFile, numCorrect, numDistract, numItems, ImgSize, ActorName, useDisplay, bAllowDups);
		}
		case "SeqContent":
		{
			int maxSeqLength = (int)parserData["MaxSeqLength"];
			return (TYPE)(CmContentBase)CmParsers.GetSeqContent(pDataFile, maxSeqLength, ImgSize, ActorName);
		}
		case "BasicContent":
			return (TYPE)(CmContentBase)CmParsers.GetBasicContent(pDataFile, ImgSize, ActorName, null);
		default:
			return null;
		}
	}

	public void SetResultSuccess(CmRK rk)
	{
		if (!mInRound)
		{
			return;
		}
		if (mSuccess != null)
		{
			CmRK cmRK = ContainsRK(mSuccess, rk);
			if (cmRK != null)
			{
				mSuccess[cmRK]++;
			}
			else
			{
				mSuccess.Add(rk, 1);
			}
		}
		mNumCorrect += 1.0;
		mNumAttempts += 1.0;
	}

	public void SetResultFailure(CmRK rk)
	{
		if (!mInRound)
		{
			return;
		}
		if (mFailure != null)
		{
			CmRK cmRK = ContainsRK(mFailure, rk);
			if (cmRK != null)
			{
				mFailure[cmRK]++;
			}
			else
			{
				mFailure.Add(rk, 1);
			}
		}
		mNumAttempts += 1.0;
	}

	private CmRK ContainsRK(Dictionary<CmRK, int> inList, CmRK rk)
	{
		if (rk == null)
		{
			return null;
		}
		if (inList.Count != 0)
		{
			foreach (KeyValuePair<CmRK, int> @in in inList)
			{
				if (@in.Key.pSkillAddress == rk.pSkillAddress && @in.Key.pSubLevelID == rk.pSubLevelID && @in.Key.pLevel == rk.pLevel)
				{
					return @in.Key;
				}
			}
		}
		return null;
	}

	public void SetResultExposed(CmRK rk)
	{
		mSetExposed = true;
	}

	public void GetProductValue()
	{
	}

	public void SetProductValue()
	{
	}

	public void GetEquation()
	{
	}

	public void GetPlayerName()
	{
	}

	public void GetGameParams()
	{
	}

	public void GetActivityParams()
	{
	}

	public void GetLessonNumber()
	{
	}
}
