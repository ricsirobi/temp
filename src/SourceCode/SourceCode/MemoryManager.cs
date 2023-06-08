using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MemoryManager : MonoBehaviour
{
	public MinMax _LowMemoryThreshold = new MinMax(0f, 15f);

	public MinMax _MediumMemoryThreshold = new MinMax(20f, 45f);

	public MinMax _HighMemoryThreshold = new MinMax(50f, 99999f);

	public float _UpdateInterval = 2.5f;

	public float _LowMemWarningWaitTime = 600f;

	public LocaleString _LowMemoryWarningText = new LocaleString("Your device is running low on memory. Please kill apps that you don`t need.");

	public LocaleString _WarningHeaderText = new LocaleString("Warning!");

	private float mTimeOfLastMemoryMessage = -9999f;

	private static MemoryManager mInstance = null;

	private KAUIGenericDB mKAUIGenericDB;

	private List<GameObject> mEventListeners = new List<GameObject>();

	private const string OnMemoryThresholdChanged = "OnMemoryThresholdChanged";

	private MemoryThreshold mMemoryThreshold = MemoryThreshold.NONE;

	private const float mOneMB = 1048576f;

	private float mLastTimeMMOincreased;

	private float mTimeBetweenMMOIncreases = 5f;

	private float TimeOfLastMemoryWarning;

	public static int MemoryWarningCleanupLevel = 0;

	private static bool firstTime = true;

	public static MemoryManager pInstance => mInstance;

	public KAUIGenericDB pKAUIGenericDB => mKAUIGenericDB;

	public MemoryThreshold pMemoryThreshold
	{
		get
		{
			return mMemoryThreshold;
		}
		set
		{
			if (mMemoryThreshold == value)
			{
				return;
			}
			mMemoryThreshold = value;
			foreach (GameObject mEventListener in mEventListeners)
			{
				if (mEventListener != null)
				{
					mEventListener.SendMessage("OnMemoryThresholdChanged", mMemoryThreshold, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
	}

	private void Start()
	{
		mInstance = this;
		Object.DontDestroyOnLoad(base.gameObject);
		mLastTimeMMOincreased = 0f;
	}

	private void Update()
	{
		if (MainStreetMMOClient.pIsMMOEnabled && MainStreetMMOClient.mMMOAvatarLimit < UtPlatform.GetDeviceAvatarLimit() && Time.realtimeSinceStartup - mLastTimeMMOincreased > mTimeBetweenMMOIncreases)
		{
			mLastTimeMMOincreased = Time.realtimeSinceStartup;
			MainStreetMMOClient.mMMOAvatarLimit++;
		}
	}

	public static void AddListener(GameObject listener)
	{
		if (mInstance != null && listener != null)
		{
			mInstance.mEventListeners.Add(listener);
			listener.SendMessage("OnMemoryThresholdChanged", pInstance.pMemoryThreshold, SendMessageOptions.DontRequireReceiver);
		}
	}

	public static bool RemoveListener(GameObject listener)
	{
		if (mInstance == null || listener == null)
		{
			return false;
		}
		return mInstance.mEventListeners.Remove(listener);
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene newScene, LoadSceneMode loadSceneMode)
	{
		mEventListeners.RemoveAll((GameObject listener) => listener == null);
		MainStreetMMOClient.mMMOAvatarLimit = UtPlatform.GetDeviceAvatarLimit();
	}

	private void OnReceivedMemoryWarning(string msg)
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		float num = realtimeSinceStartup - TimeOfLastMemoryWarning;
		if (num < 1f)
		{
			return;
		}
		TimeOfLastMemoryWarning = realtimeSinceStartup;
		if (num > 10f)
		{
			MemoryWarningCleanupLevel = 0;
			MainStreetMMOClient.mMMOAvatarLimit = UtPlatform.GetDeviceAvatarLimit();
		}
		else
		{
			MemoryWarningCleanupLevel++;
		}
		MemoryProfiler.ForceMemoryCleanUp();
		if (MainStreetMMOClient.pIsMMOEnabled)
		{
			if (MemoryWarningCleanupLevel == 1)
			{
				MainStreetMMOClient.mMMOAvatarLimit = UtPlatform.GetDeviceAvatarLimit() / 2;
				if (MainStreetMMOClient.pInstance != null)
				{
					MainStreetMMOClient.pInstance.DeleteAllExtraPlayers();
				}
			}
			if (MemoryWarningCleanupLevel >= 2)
			{
				MainStreetMMOClient.mMMOAvatarLimit = 0;
				if (MainStreetMMOClient.pInstance != null)
				{
					MainStreetMMOClient.pInstance.DeleteAllPlayers();
				}
				Invoke("EnableMMO", 10f);
			}
		}
		if (!RsResourceManager.pLevelLoading && KAUI._GlobalExclusiveUI == null && mKAUIGenericDB == null && GameDataConfig.pIsReady && GameDataConfig.pInstance.ShowLowMemoryWarning)
		{
			float num2 = Time.realtimeSinceStartup - mTimeOfLastMemoryMessage;
			if (firstTime || num2 >= _LowMemWarningWaitTime)
			{
				firstTime = false;
				ShowLowMemoryWarning(_LowMemoryWarningText.GetLocalizedString());
			}
		}
	}

	private void EnableMMO()
	{
		MainStreetMMOClient.mMMOAvatarLimit = UtPlatform.GetDeviceAvatarLimit();
	}

	public void FreeMemory()
	{
		long freeMemory = UtMobileUtilities.GetFreeMemory();
		RsResourceManager.UnloadUnusedAssets(canGCCollect: true);
		UtDebug.Log("Memory recovered : " + ((float)UtMobileUtilities.GetFreeMemory() - (float)freeMemory) / 1048576f + " MB");
	}

	private void ShowLowMemoryWarning(string inText)
	{
		mTimeOfLastMemoryMessage = Time.realtimeSinceStartup;
		UtDebug.LogError("Not enough memory. Warn user");
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Warning!");
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mKAUIGenericDB._OKMessage = "OnClickedOK";
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB.SetTitle(_WarningHeaderText.GetLocalizedString());
		mKAUIGenericDB.SetText(inText, interactive: false);
		KAUI.SetExclusive(mKAUIGenericDB);
	}

	private void OnClickedOK()
	{
		if (mKAUIGenericDB != null)
		{
			mTimeOfLastMemoryMessage = 0f;
			KAUI.RemoveExclusive(mKAUIGenericDB);
			Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}
}
