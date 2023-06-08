using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot("DragonAgeUpConfig", Namespace = "")]
public class DragonAgeUpConfig
{
	public delegate void OnDragonAgeUpCancel();

	public delegate void OnDragonAgeUpDone();

	public delegate void OnDragonAgeUpBuy();

	[XmlElement(ElementName = "AgeUpData")]
	public AgeUpData[] Data;

	public const string AGEUP_SHOW_ONCE_KEY = "SO";

	public const string AGEUP_LAST_HOUR_KEY = "LH";

	public const string AGEUP_LAST_FREQUENCY_KEY = "LF";

	public const string AGEUP_TASKID_KEY = "TID";

	public static DragonAgeUpConfig pDragonAgeUpConfig = null;

	private static Dictionary<string, int> mSessionShowList = new Dictionary<string, int>();

	private static string mCurrentTrigger;

	private static OnDragonAgeUpCancel mAgeUpCancelCallback;

	private static OnDragonAgeUpDone mAgeUpDoneCallback;

	private static OnDragonAgeUpBuy mAgeUpBuyCallback;

	private static RaisedPetStage mFromStage;

	private static RaisedPetStage[] mRequiredStages;

	private static RaisedPetData mRaisedPetData;

	private static int mPrevAge = 0;

	private static bool mAgeUpDone = false;

	private static bool mUnmountableAllowed = false;

	private static GameObject mMessageObj = null;

	public static bool pIsReady => pDragonAgeUpConfig != null;

	public static void Init()
	{
		if (pDragonAgeUpConfig == null)
		{
			RsResourceManager.Load(GameConfig.GetKeyData("DragonAgeUpConfigFile"), OnXMLDownloaded);
		}
	}

	private static void OnXMLDownloaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent != RsResourceLoadEvent.COMPLETE)
		{
			_ = 3;
		}
		else
		{
			pDragonAgeUpConfig = UtUtilities.DeserializeFromXml((string)inObject, typeof(DragonAgeUpConfig)) as DragonAgeUpConfig;
		}
	}

	public static AgeUpData GetAgeUpDataFromTrigger(string inTrigger, RaisedPetStage inStage)
	{
		if (pDragonAgeUpConfig == null || pDragonAgeUpConfig.Data == null || pDragonAgeUpConfig.Data.Length == 0)
		{
			return null;
		}
		AgeUpData result = null;
		AgeUpData[] data = pDragonAgeUpConfig.Data;
		foreach (AgeUpData ageUpData in data)
		{
			if (ageUpData.Trigger == inTrigger)
			{
				if (ageUpData.PetStage == inStage)
				{
					result = ageUpData;
					break;
				}
				if (ageUpData.PetStage == RaisedPetStage.NONE)
				{
					result = ageUpData;
				}
			}
		}
		return result;
	}

	public static void SavePairData(string inKey, string inValue)
	{
		ProductData.pPairData.SetValue(inKey, inValue);
	}

	public static bool Trigger(string inTrigger, OnDragonAgeUpDone inCallback, RaisedPetData inData = null, RaisedPetStage newStage = RaisedPetStage.NONE, bool isUnmountableAllowed = true, bool alertUser = true, GameObject messageObj = null, string assetName = "")
	{
		if (mCurrentTrigger != inTrigger)
		{
			mCurrentTrigger = inTrigger;
		}
		if (!pIsReady)
		{
			return false;
		}
		if (SanctuaryManager.pCurPetData == null)
		{
			return false;
		}
		if (inData == null)
		{
			inData = SanctuaryManager.pCurPetData;
		}
		if (newStage != 0 && RaisedPetData.GetAgeIndex(inData.pStage) >= RaisedPetData.GetAgeIndex(newStage))
		{
			return false;
		}
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(inData.PetTypeID);
		if (newStage == RaisedPetStage.NONE && RaisedPetData.GetAgeIndex(inData.pStage) >= sanctuaryPetTypeInfo._AgeData.Length - 1)
		{
			return false;
		}
		List<RaisedPetStage> list = new List<RaisedPetStage>();
		if (!isUnmountableAllowed && SanctuaryManager.pCurPetInstance.pAge < sanctuaryPetTypeInfo._MinAgeToMount)
		{
			for (int i = sanctuaryPetTypeInfo._MinAgeToMount; i < sanctuaryPetTypeInfo._AgeData.Length; i++)
			{
				list.Add(RaisedPetData.GetGrowthStage(i));
			}
		}
		if (!string.IsNullOrEmpty(inTrigger))
		{
			AgeUpData ageUpDataFromTrigger = GetAgeUpDataFromTrigger(inTrigger, inData.pStage);
			if (ageUpDataFromTrigger != null && (CheckRepeatAttribute(ageUpDataFromTrigger) || list.Count != 0))
			{
				if (list.Count == 0)
				{
					list.Add(newStage);
				}
				if (RaisedPetData.GetAgeIndex(inData.pStage) < sanctuaryPetTypeInfo._MinAgeToFly)
				{
					ShowAgeUpUI(inCallback, inData.pStage, inData, list.ToArray(), ageUpDone: false, isUnmountableAllowed, messageObj, assetName);
					return true;
				}
			}
		}
		return false;
	}

	private static bool CheckRepeatAttribute(AgeUpData inData)
	{
		if (string.IsNullOrEmpty(inData.type))
		{
			return CheckShowOnce();
		}
		if (inData.type == "Hour")
		{
			return CheckRepeatOnHour(inData);
		}
		if (inData.type == "Frequency")
		{
			return CheckRepeatOnFrequency(inData);
		}
		if (inData.type == "Session")
		{
			return CheckRepeatOnSession(inData);
		}
		if (inData.type == "Mission")
		{
			return CheckRepeatOnTaskCompletion(inData);
		}
		if (inData.type == "Always")
		{
			return true;
		}
		return false;
	}

	private static bool CheckShowOnce()
	{
		int intValue = ProductData.pPairData.GetIntValue(mCurrentTrigger + "_SO", 0);
		if (intValue == 0)
		{
			string inKey = mCurrentTrigger + "_SO";
			string inValue = (intValue + 1).ToString();
			SavePairData(inKey, inValue);
			return true;
		}
		return false;
	}

	private static bool CheckRepeatOnHour(AgeUpData inData)
	{
		string value = ProductData.pPairData.GetValue(mCurrentTrigger + "_LH");
		if (!string.IsNullOrEmpty(value) && value != "___VALUE_NOT_FOUND___")
		{
			DateTime minValue = DateTime.MinValue;
			minValue = DateTime.Parse(value, UtUtilities.GetCultureInfo("en-US"));
			if ((ServerTime.pCurrentTime - minValue).TotalHours <= (double)inData.Repeat)
			{
				return false;
			}
		}
		string inKey = mCurrentTrigger + "_LH";
		string inValue = ServerTime.pCurrentTime.ToString(UtUtilities.GetCultureInfo("en-US"));
		SavePairData(inKey, inValue);
		return true;
	}

	private static bool CheckRepeatOnFrequency(AgeUpData inData)
	{
		string inKey = mCurrentTrigger + "_LF";
		int intValue = ProductData.pPairData.GetIntValue(inKey, 0);
		if (intValue == 0 || intValue > inData.Repeat)
		{
			string inValue = "1";
			SavePairData(inKey, inValue);
			return true;
		}
		string inValue2 = (intValue + 1).ToString();
		SavePairData(inKey, inValue2);
		return false;
	}

	private static bool CheckRepeatOnSession(AgeUpData inData)
	{
		int num = 0;
		if (mSessionShowList.ContainsKey(mCurrentTrigger))
		{
			num = mSessionShowList[mCurrentTrigger];
		}
		else
		{
			mSessionShowList.Add(mCurrentTrigger, num);
		}
		if (num < inData.Repeat)
		{
			mSessionShowList[mCurrentTrigger] = num + 1;
			return true;
		}
		return false;
	}

	private static bool CheckRepeatOnTaskCompletion(AgeUpData inData)
	{
		int[] taskID = inData.TaskID;
		for (int i = 0; i < taskID.Length; i++)
		{
			int taskID2 = taskID[i];
			if (MissionManager.pInstance.GetTask(taskID2).pCompleted)
			{
				string inKey = mCurrentTrigger + "_" + taskID2 + "_TID";
				if (ProductData.pPairData.GetIntValue(inKey, 0) <= 0)
				{
					string inValue = "1";
					SavePairData(inKey, inValue);
					return true;
				}
			}
		}
		return false;
	}

	public static void ShowAgeUpUI(OnDragonAgeUpDone inOnDoneCallback = null, RaisedPetStage fromStage = RaisedPetStage.NONE, RaisedPetData inData = null, RaisedPetStage[] requiredStages = null, bool ageUpDone = false, bool isUnmountableAllowed = false, GameObject messageObj = null, string assetName = "")
	{
		ShowAgeUpUI(null, inOnDoneCallback, null, fromStage, inData, requiredStages);
	}

	public static void ShowAgeUpUI(OnDragonAgeUpCancel inOnCancelCallback, OnDragonAgeUpDone inOnDoneCallback, OnDragonAgeUpBuy inOnBuyCallback, RaisedPetStage fromStage = RaisedPetStage.NONE, RaisedPetData inData = null, RaisedPetStage[] requiredStages = null, bool ageUpDone = false, bool isUnmountableAllowed = false, GameObject messageObj = null, string assetName = "")
	{
		mAgeUpCancelCallback = inOnCancelCallback;
		mAgeUpDoneCallback = inOnDoneCallback;
		mAgeUpBuyCallback = inOnBuyCallback;
		mRequiredStages = requiredStages;
		mFromStage = fromStage;
		mUnmountableAllowed = isUnmountableAllowed;
		mMessageObj = messageObj;
		if (inData != null)
		{
			mRaisedPetData = inData;
		}
		else
		{
			mRaisedPetData = SanctuaryManager.pCurPetData;
		}
		if (fromStage == RaisedPetStage.NONE)
		{
			mFromStage = mRaisedPetData.pStage;
		}
		mPrevAge = RaisedPetData.GetAgeIndex(mFromStage);
		mAgeUpDone = ageUpDone;
		KAUICursorManager.SetDefaultCursor("Loading");
		string[] array = GameConfig.GetKeyData(string.IsNullOrEmpty(assetName) ? "AgeUpAsset" : assetName).Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnDragonAgeUpLoaded, typeof(GameObject), inDontDestroy: false, mMessageObj);
	}

	private static void OnDragonAgeUpLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			KAUICursorManager.SetDefaultCursor("Arrow");
			UnityEngine.Object.Instantiate((GameObject)inObject).GetComponent<UiDragonAgeUp>().Init(OnUiDragonAgeUpCancel, OnUiDragonAgeUpClose, OnUiDragonAgeUpBuy, mFromStage, mRaisedPetData, mRequiredStages, mAgeUpDone, mUnmountableAllowed, (GameObject)inUserData);
			RsResourceManager.ReleaseBundleData(inURL);
			break;
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			mAgeUpCancelCallback?.Invoke();
			mAgeUpCancelCallback = null;
			mRaisedPetData = null;
			break;
		}
	}

	private static void OnUiDragonAgeUpCancel()
	{
		mAgeUpCancelCallback?.Invoke();
		CleanupCallbacks();
	}

	private static void OnUiDragonAgeUpClose()
	{
		if (mRaisedPetData != null && mPrevAge < RaisedPetData.GetAgeIndex(mRaisedPetData.pStage))
		{
			ShowAgeUpCustomizedCutscene();
			return;
		}
		mAgeUpDoneCallback?.Invoke();
		CleanupCallbacks();
		mRaisedPetData = null;
	}

	private static void OnUiDragonAgeUpBuy()
	{
		mAgeUpBuyCallback?.Invoke();
		CleanupCallbacks();
		mRaisedPetData = null;
	}

	public static void ShowAgeUpCutscene(RaisedPetData pData, RaisedPetStage fromStage = RaisedPetStage.NONE, OnDragonAgeUpDone inCallback = null)
	{
		mRaisedPetData = pData;
		mFromStage = fromStage;
		mPrevAge = RaisedPetData.GetAgeIndex(mFromStage);
		mAgeUpDoneCallback = inCallback;
		ShowAgeUpCustomizedCutscene();
	}

	private static void OnCustomizedCutSceneDone()
	{
		mAgeUpDoneCallback?.Invoke();
		mAgeUpDoneCallback = null;
		if (mRaisedPetData.RaisedPetID == SanctuaryManager.pCurPetInstance.pData.RaisedPetID)
		{
			SanctuaryManager.pCurPetInstance.SetFollowAvatar(follow: true);
			SanctuaryManager.pInstance.pSetFollowAvatar = false;
		}
		mRaisedPetData = null;
	}

	public static void ShowAgeUpCustomizedCutscene()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		string[] array = GameConfig.GetKeyData("AgeUpCutsceneAsset").Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnAgeUpCutSceneLoaded, typeof(GameObject), inDontDestroy: false, mMessageObj);
	}

	private static void OnAgeUpCutSceneLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			AgeUpCustomizedCutscene component = UnityEngine.Object.Instantiate((GameObject)inObject).GetComponent<AgeUpCustomizedCutscene>();
			RsResourceManager.ReleaseBundleData(inURL);
			component.Init(mFromStage, mRaisedPetData, mAgeUpDoneCallback, (GameObject)inUserData);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Error loading cutscene prefab!");
			KAUICursorManager.SetDefaultCursor("Arrow");
			mAgeUpDoneCallback?.Invoke();
			break;
		}
	}

	private static void CleanupCallbacks()
	{
		mAgeUpBuyCallback = null;
		mAgeUpCancelCallback = null;
		mAgeUpDoneCallback = null;
	}
}
