using System;
using UnityEngine;

public class IMLevelProgress : KAMonoBase
{
	public int _PairDataID = 2014;

	public int _MaxMissionVisitedCount = 1;

	public CraftTraptionsHintData[] _CraftTraptionsHintData;

	public const string mLevelKey = "IMLevel-";

	public const string mHighScoreKey = "IMHighScore-";

	public const string mLastLevelPlayedKey = "IMLastLevel";

	public static Action<bool, PairData, object> OnPairDataSaved;

	private PairData mPairData;

	private static IMLevelProgress mInstance;

	private static bool mSaveToPlayerProfile;

	private int mCurrentLevel;

	private bool mFromPenguinHQ;

	private bool mMissionLevel;

	public LocaleString _NonMemberText = new LocaleString("This is available to members only. Do you want to become a member?");

	protected GameObject mUiGenericDB;

	public AudioClip _LockedVO;

	public Color _MaskColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	public PairData pPairData
	{
		get
		{
			return mPairData;
		}
		set
		{
			mPairData = value;
		}
	}

	public int pCurrentLevel
	{
		get
		{
			return mCurrentLevel;
		}
		set
		{
			mCurrentLevel = value;
		}
	}

	public bool pFromPenguinHQ
	{
		get
		{
			return mFromPenguinHQ;
		}
		set
		{
			mFromPenguinHQ = value;
		}
	}

	public bool pMissionLevel
	{
		get
		{
			return mMissionLevel;
		}
		set
		{
			mMissionLevel = value;
		}
	}

	public static IMLevelProgress pInstance => mInstance;

	public static bool pIsReady
	{
		get
		{
			if (mSaveToPlayerProfile)
			{
				if (mInstance != null)
				{
					return mInstance.mPairData != null;
				}
				return false;
			}
			return true;
		}
	}

	private void Awake()
	{
		mInstance = this;
		mSaveToPlayerProfile = !string.IsNullOrEmpty(RsResourceManager.pLastLevel);
		mFromPenguinHQ = true;
		PairData.Load(_PairDataID, OnPairDataLoaded, null);
	}

	private void OnPairDataLoaded(bool success, PairData pData, object inUserData)
	{
		mPairData = pData;
	}

	public void DisplayMembershipTextBox()
	{
		mUiGenericDB = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfUiMemberUpSellDBMO"));
		KAUIGenericDB component = mUiGenericDB.GetComponent<KAUIGenericDB>();
		component._MessageObject = base.gameObject;
		if (_LockedVO != null)
		{
			SnChannel.Play(_LockedVO, "VO_Pool", inForce: true);
		}
		component._CloseMessage = "CloseDialogBox";
		component.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: true);
		component.SetPriority(5);
		component.SetExclusive(_MaskColor);
	}

	public void CloseDialogBox()
	{
		SnChannel.StopPool("VO_Pool");
		if (mUiGenericDB != null)
		{
			UnityEngine.Object.Destroy(mUiGenericDB);
			mUiGenericDB = null;
		}
	}

	public void Save(string inLevelName, int numStarsCollected, int inLevelNum = -1)
	{
		if (mSaveToPlayerProfile)
		{
			if (inLevelNum > -1)
			{
				mInstance?.mPairData?.SetValue("IMLastLevel", inLevelNum.ToString());
			}
			if (mInstance.mPairData.GetIntValue("IMLevel-" + inLevelName, -1) < numStarsCollected)
			{
				mInstance.mPairData.SetValue("IMLevel-" + inLevelName, numStarsCollected.ToString());
				PairData.Save(mInstance.mPairData._DataID, OnPairDataSaveComplete);
				return;
			}
		}
		else
		{
			if (inLevelNum > -1)
			{
				PlayerPrefs.SetInt("IMLastLevel", inLevelNum);
			}
			if (PlayerPrefs.GetInt("IMLevel-" + inLevelName) < numStarsCollected)
			{
				PlayerPrefs.SetInt("IMLevel-" + inLevelName, numStarsCollected);
			}
			PlayerPrefs.Save();
		}
		OnPairDataSaved?.Invoke(arg1: true, null, null);
	}

	public virtual void OnPairDataSaveComplete(bool success, PairData pData, object inUserData)
	{
		OnPairDataSaved?.Invoke(success, pData, inUserData);
	}

	public static void SaveHighScore(string inLevelName, int highScore)
	{
		if (mSaveToPlayerProfile)
		{
			if (mInstance.mPairData.GetIntValue("IMHighScore-" + inLevelName, -1) < highScore)
			{
				mInstance.mPairData.SetValueAndSave("IMHighScore-" + inLevelName, highScore.ToString());
			}
		}
		else if (PlayerPrefs.GetInt("IMHighScore-" + inLevelName) < highScore)
		{
			PlayerPrefs.SetInt("IMHighScore-" + inLevelName, highScore);
			PlayerPrefs.Save();
		}
	}

	public static void SaveCurrentLevel(int inLevelNum)
	{
		if (mSaveToPlayerProfile)
		{
			mInstance?.mPairData?.SetValueAndSave("IMLastLevel", inLevelNum.ToString());
			return;
		}
		PlayerPrefs.SetInt("IMLastLevel", inLevelNum);
		PlayerPrefs.Save();
	}

	public static int GetStarsCollected(string inLevelName)
	{
		if (!pIsReady)
		{
			return -1;
		}
		if (mSaveToPlayerProfile)
		{
			return mInstance.mPairData.GetIntValue("IMLevel-" + inLevelName, -1);
		}
		return PlayerPrefs.GetInt("IMLevel-" + inLevelName);
	}

	public int GetLastLevelPlayed()
	{
		if (!pIsReady)
		{
			return 0;
		}
		if (mSaveToPlayerProfile)
		{
			return mInstance.mPairData.GetIntValue("IMLastLevel", 0);
		}
		return PlayerPrefs.GetInt("IMLastLevel", 0);
	}

	public static int GetHighScore(string inLevelName)
	{
		int result = 0;
		if (pIsReady)
		{
			result = ((!mSaveToPlayerProfile) ? PlayerPrefs.GetInt("IMLevel-" + inLevelName) : mInstance.mPairData.GetIntValue("IMHighScore-" + inLevelName, 0));
		}
		return result;
	}

	public CraftTraptionsHintData GetCraftTraptionHintData(int itemID)
	{
		if (_CraftTraptionsHintData != null)
		{
			for (int i = 0; i < _CraftTraptionsHintData.Length; i++)
			{
				if (_CraftTraptionsHintData[i]._ItemID == itemID)
				{
					return _CraftTraptionsHintData[i];
				}
			}
			UtDebug.LogError("Item ID not found in CraftTraptions Hint Data " + base.gameObject.name + ", ItemID" + itemID);
			return null;
		}
		UtDebug.LogError("CraftTraptions Hint data not setup: " + base.gameObject.name);
		return null;
	}

	public bool IsLevelUnlocked(LevelData level)
	{
		if (!level.MemberOnly)
		{
			return true;
		}
		if (SubscriptionInfo.pIsMember)
		{
			return !SubscriptionInfo.pIsTrialMember;
		}
		return false;
	}

	public bool HasPlayedTutorial()
	{
		return GetStarsCollected("Tutorial") >= 0;
	}
}
