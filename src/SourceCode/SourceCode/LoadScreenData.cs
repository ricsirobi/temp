using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot("LoadScreenData")]
public class LoadScreenData
{
	[XmlElement(ElementName = "LoadScreen")]
	public LoadScreen[] LoadScreens;

	[XmlElement(ElementName = "PriorityLoadScreen")]
	public PriorityLoadScreen[] PriorityLoadScreens;

	private const string Tag_Any = "Any";

	public static LoadScreenData _LoadScreenData = null;

	private static bool mGetGenderFromUserInfo = false;

	private static string mLoadScreenDataFilename = string.Empty;

	private static Dictionary<string, int> mPlayerPrefsTagDictionary = new Dictionary<string, int>();

	private static bool mInitialized = false;

	private static string mPrevLSName = string.Empty;

	public static bool pIsReady
	{
		get
		{
			if (_LoadScreenData != null)
			{
				return UserInfo.pIsReady;
			}
			return false;
		}
	}

	public static void Init(string inFilename, bool inGetGenderFromUserInfo)
	{
		mGetGenderFromUserInfo = inGetGenderFromUserInfo;
		mLoadScreenDataFilename = inFilename;
		if (_LoadScreenData == null)
		{
			RsResourceManager.Load(mLoadScreenDataFilename, XmlLoadEventHandler, RsResourceType.NONE, inDontDestroy: true);
		}
	}

	private static Gender GetGender()
	{
		if (mGetGenderFromUserInfo)
		{
			return UserInfo.GetGender();
		}
		return AvatarData.pInstance.GenderType;
	}

	private static void XmlLoadEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (!inEvent.Equals(RsResourceLoadEvent.COMPLETE))
		{
			return;
		}
		_LoadScreenData = UtUtilities.DeserializeFromXml<LoadScreenData>((string)inObject);
		if (_LoadScreenData != null && _LoadScreenData.PriorityLoadScreens != null)
		{
			PriorityLoadScreen[] priorityLoadScreens = _LoadScreenData.PriorityLoadScreens;
			foreach (PriorityLoadScreen obj in priorityLoadScreens)
			{
				obj.pCurrentFrequency = obj.Frequency;
			}
		}
	}

	private static void Initialize()
	{
		mInitialized = true;
		ReadFromPlayerPrefs();
	}

	private static LoadScreen GetPriorityLoadScreen(bool inIsExitScreen)
	{
		string empty = string.Empty;
		empty = (inIsExitScreen ? RsResourceManager.pLastLevel : (string.IsNullOrEmpty(RsResourceManager.pLevelToLoad) ? RsResourceManager.pCurrentLevel : RsResourceManager.pLevelToLoad));
		int age = UserInfo.GetAge();
		Gender gender = GetGender();
		PriorityLoadScreen[] priorityLoadScreens = _LoadScreenData.PriorityLoadScreens;
		foreach (PriorityLoadScreen priorityLoadScreen in priorityLoadScreens)
		{
			if (!priorityLoadScreen.IsSceneNameMatching(inIsExitScreen, empty))
			{
				continue;
			}
			if (priorityLoadScreen.pCurrentFrequency <= 0)
			{
				priorityLoadScreen.pCurrentFrequency = priorityLoadScreen.Frequency;
				int num = 0;
				LoadScreen[] loadScreens = priorityLoadScreen.LoadScreens;
				foreach (LoadScreen loadScreen in loadScreens)
				{
					if (num >= priorityLoadScreen.pCurrentLoadScreenIndex && loadScreen.IsAgeMatching(age, inMatchType: false) && loadScreen.IsGenderMatching(gender, inMatchType: false) && !mPrevLSName.Equals(loadScreen.Name))
					{
						mPrevLSName = loadScreen.Name;
						priorityLoadScreen.pCurrentLoadScreenIndex++;
						return loadScreen;
					}
					num++;
				}
			}
			else
			{
				priorityLoadScreen.pCurrentFrequency--;
			}
		}
		return null;
	}

	public static LoadScreen GetLoadScreenWithTag(string inTag)
	{
		if (_LoadScreenData == null)
		{
			return null;
		}
		LoadScreen result = null;
		int num = 0;
		if (mPlayerPrefsTagDictionary.ContainsKey(inTag))
		{
			num = mPlayerPrefsTagDictionary[inTag];
			num++;
		}
		else
		{
			mPlayerPrefsTagDictionary.Add(inTag, 0);
		}
		List<LoadScreen> list = new List<LoadScreen>();
		if (!inTag.Equals("Any"))
		{
			list = new List<LoadScreen>();
			LoadScreen[] loadScreens = _LoadScreenData.LoadScreens;
			foreach (LoadScreen loadScreen in loadScreens)
			{
				if (loadScreen.IsTagMatching(inTag))
				{
					list.Add(loadScreen);
				}
			}
		}
		else
		{
			list = new List<LoadScreen>();
			list.AddRange(_LoadScreenData.LoadScreens);
		}
		if (num >= list.Count)
		{
			num = 0;
		}
		for (int j = 0; j < list.Count; j++)
		{
			LoadScreen loadScreen2 = list[num];
			if (loadScreen2.IsMembershipStatusMatching() && !mPrevLSName.Equals(loadScreen2.Name))
			{
				string value = ((!UserInfo.pIsReady || UserInfo.pInstance.Partner == null) ? "" : UserInfo.pInstance.Partner);
				if (loadScreen2.Partner == null || loadScreen2.Partner.Equals(value))
				{
					result = loadScreen2;
					mPrevLSName = loadScreen2.Name;
					mPlayerPrefsTagDictionary[inTag] = num;
					break;
				}
			}
			num++;
			if (num == list.Count)
			{
				num = 0;
			}
		}
		return result;
	}

	public static LoadScreen GetLoadScreen(bool getPriorityLoadScreen = true)
	{
		LoadScreen loadScreen = null;
		if (!pIsReady)
		{
			return loadScreen;
		}
		if (!mInitialized)
		{
			Initialize();
		}
		if (getPriorityLoadScreen)
		{
			if (_LoadScreenData.PriorityLoadScreens != null)
			{
				loadScreen = GetPriorityLoadScreen(inIsExitScreen: true);
			}
			if (loadScreen != null)
			{
				return loadScreen;
			}
			if (_LoadScreenData.PriorityLoadScreens != null)
			{
				loadScreen = GetPriorityLoadScreen(inIsExitScreen: false);
			}
			if (loadScreen != null)
			{
				return loadScreen;
			}
		}
		loadScreen = GetLoadScreenWithTag("Any");
		if (UserInfo.pIsReady)
		{
			SaveToPlayerPrefs();
		}
		return loadScreen;
	}

	private static void ReadFromPlayerPrefs()
	{
		string @string = PlayerPrefs.GetString("LoadScreen3.0_" + UserInfo.pInstance.UserID, "");
		char[] separator = new char[1] { ';' };
		string[] array = @string.Split(separator, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length % 2 == 0)
		{
			string empty = string.Empty;
			int num = 0;
			for (int i = 0; i < array.Length; i += 2)
			{
				empty = array[i];
				num = Convert.ToInt32(array[i + 1]);
				mPlayerPrefsTagDictionary.Add(empty, num);
			}
		}
	}

	private static void SaveToPlayerPrefs()
	{
		string text = string.Empty;
		foreach (KeyValuePair<string, int> item in mPlayerPrefsTagDictionary)
		{
			text = text + item.Key + ";" + item.Value + ";";
		}
		PlayerPrefs.SetString("LoadScreen3.0_" + UserInfo.pInstance.UserID, text);
	}
}
