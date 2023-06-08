using System;
using System.Xml.Serialization;
using KA.Framework;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "ProductData", Namespace = "")]
public class ProductData
{
	public string LastScene;

	public int Level;

	public int Mission;

	public int Step;

	public int Experience;

	public int ExperienceUnused;

	[XmlArray]
	[XmlArrayItem(ElementName = "Task")]
	public int[] Tasks;

	[XmlArray]
	[XmlArrayItem(ElementName = "Tutorial")]
	public string[] Tutorials;

	public int MaxDrawings;

	public int MaxPhotos;

	[XmlElement(ElementName = "UpdateDate")]
	public DateTime UpdateDate;

	private const string LAST_SCENE_NAME_KEY = "sceneName";

	public const int mGameId = 5;

	private static PairData mPairData;

	private static bool mPairDataReady;

	private static bool mInitialized;

	public static bool pIsReady => mPairDataReady;

	public static PairData pPairData => mPairData;

	public static void Init(bool forcePairDataLoad = false)
	{
		if (!mInitialized)
		{
			mInitialized = true;
			PairData.Load(ProductSettings.pInstance._PairDataID, PairDataEventHandler, null, forcePairDataLoad);
		}
	}

	private static void PairDataEventHandler(bool success, PairData pData, object inUserData)
	{
		mPairData = pData;
		mPairDataReady = true;
	}

	public static void Reset()
	{
		mInitialized = false;
	}

	public static string GetSavedScene()
	{
		if (mPairData != null && mPairData.FindByKey("sceneName") != null)
		{
			return mPairData.GetValue("sceneName");
		}
		return null;
	}

	public static void SaveScene(string sceneName)
	{
		if (mPairData != null)
		{
			mPairData.SetValueAndSave("sceneName", sceneName);
		}
	}

	public static void Save()
	{
		if (pIsReady)
		{
			PairData.Save(ProductSettings.pInstance._PairDataID);
		}
	}

	public static bool AddTutorial(string tutName)
	{
		return AddTutorial(tutName, inUpdateServer: true);
	}

	public static bool AddTutorial(string tutName, bool inUpdateServer)
	{
		if (!TutorialComplete(tutName) && mPairData != null)
		{
			if (inUpdateServer)
			{
				mPairData.SetValueAndSave(tutName, "1");
			}
			else
			{
				mPairData.SetValue(tutName, "1");
			}
			return true;
		}
		return false;
	}

	public static bool TutorialComplete(string tutName)
	{
		if (mPairData != null)
		{
			return mPairData.FindByKey(tutName) != null;
		}
		return false;
	}

	public static bool ResetTutorial(string tutName)
	{
		if (!string.IsNullOrEmpty(tutName))
		{
			return mPairData.RemoveByKey(tutName);
		}
		PairData.DeleteData(ProductSettings.pInstance._PairDataID);
		PairData.Load(ProductSettings.pInstance._PairDataID, PairDataEventHandler, null);
		return true;
	}

	public static int GetNumberOfTimesMovieSeen(string movieName)
	{
		if (mPairData != null && mPairData.FindByKey(movieName) != null)
		{
			return int.Parse(mPairData.GetValue(movieName));
		}
		return PlayerPrefs.GetInt(movieName);
	}

	public static void OnMovieSeen(string movieName)
	{
		int value = GetNumberOfTimesMovieSeen(movieName) + 1;
		if (mPairData != null)
		{
			mPairData.SetValueAndSave(movieName, value.ToString());
		}
		else
		{
			PlayerPrefs.SetInt(movieName, value);
		}
	}
}
