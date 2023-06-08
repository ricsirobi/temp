using System;

public class CogsLevelProgress : KAMonoBase
{
	public int _PairDataID = 2014;

	private const string mStarsKey = "Stars";

	private const string mLastLevelPlayedKey = "CogLastLevelPlayed";

	public static Action<bool, PairData, object> OnPairDataSaved;

	private PairData mCogLevelPairData;

	private static CogsLevelProgress mInstance;

	public PairData pCogLevelPairData
	{
		get
		{
			return mCogLevelPairData;
		}
		set
		{
			mCogLevelPairData = value;
		}
	}

	public static CogsLevelProgress pInstance => mInstance;

	public bool pIsReady
	{
		get
		{
			if (mInstance != null && mCogLevelPairData != null)
			{
				return true;
			}
			return false;
		}
	}

	private void Awake()
	{
		if (mInstance == null)
		{
			mInstance = this;
		}
	}

	public void Init()
	{
		PairData.Load(_PairDataID, OnPairDataLoaded, null);
	}

	private void OnPairDataLoaded(bool success, PairData pData, object inUserData)
	{
		mCogLevelPairData = pData;
	}

	public int GetStarsCollected(string levelName)
	{
		if (mCogLevelPairData == null)
		{
			return 0;
		}
		return mCogLevelPairData.GetIntValue(levelName + "Stars", 0);
	}

	public int GetLastLevelPlayed()
	{
		if (mCogLevelPairData == null)
		{
			return 0;
		}
		return mCogLevelPairData.GetIntValue("CogLastLevelPlayed", 0);
	}

	public void Save(int stars)
	{
		if (mCogLevelPairData == null)
		{
			OnPairDataSaveComplete(success: false, null, null);
			return;
		}
		int num = -1;
		num = CogsLevelManager.pInstance.GetLevelNumber(CogsLevelManager.pInstance.pCurrentLevelIndex);
		if (num > -1)
		{
			mCogLevelPairData.SetValue("CogLastLevelPlayed", num.ToString());
		}
		string levelName = CogsLevelManager.pInstance.pCurrentLevelData.LevelName;
		if (GetStarsCollected(levelName) < stars)
		{
			mCogLevelPairData.SetValue(levelName + "Stars", stars.ToString());
		}
		mCogLevelPairData._IsDirty = true;
		PairData.Save(_PairDataID, OnPairDataSaveComplete);
	}

	private void OnPairDataSaveComplete(bool success, PairData pData, object inUserData)
	{
		OnPairDataSaved?.Invoke(success, pData, inUserData);
	}

	public void SetLevelUnlocked(int level)
	{
		if (level > GetLastLevelPlayed())
		{
			mCogLevelPairData.SetValue("CogLastLevelPlayed", level.ToString());
		}
		PairData.Save(_PairDataID);
	}
}
