using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SquadTactics;

public class LevelManager : KAMonoBase
{
	public string _TutorialKeyName = "STTutorial";

	public LocaleString _MissionDTCompleteTitleText = new LocaleString("[REVIEW]Complete!");

	public LocaleString _MissionDTCompleteText = new LocaleString("[REVIEW]You finished the task. Do you want to keep playing Dragon Tactics?");

	private const string mRealmLevelsUnlockDataKey = "STRealmLevelsUnlockData";

	private const string mLastRealmLevelPlayedKey = "STLastRealmLevelPlayed";

	private const char mRealmDelimiter = '$';

	private int[] mRealmLevelsUnlockInfo;

	private int mRealmIndex = -1;

	private int mLevelIndex = -1;

	private Data mSTLevelData;

	private bool mLevelDataReady;

	private bool mPrefetchStarted;

	private bool mPrefetchDone;

	private bool mEnablePlayBtn;

	private string mLastLevel;

	private static LevelManager mInstance;

	private List<string> mLevelScenes;

	private List<UnitSelection> mSquadList;

	[Header("UI")]
	public UiLevelSelection _LevelSelection;

	public UiTeamSelection _TeamSelection;

	[Header("Achievement IDs")]
	public int _FieldMedicAchievementID = 2259;

	public int _SafetyFirstAchievementID = 2260;

	public int _BringThePainAchievementID = 2261;

	public int _ProtectVIPAchievementID = 2262;

	public int _FlawlessAchievementID = 2263;

	public int _SmoothMovesAchievementID = 2264;

	public int _StormHeartAchievementID = 2265;

	public int _TrainingCourseAchievementID = 2266;

	public int _PlayDTAchievementID = 2267;

	public int _WinDTAchievementID = 2268;

	public int _LoseDTAchievementID = 2269;

	public string _TrainingCourseRealm = "The Training Course";

	public string _StormHeartRealm = "Rise of Stormheart";

	public static string pRealmToSelect { get; set; }

	public static bool pUnlockLevels { get; set; }

	public static bool pRefreshLevels { get; set; }

	public static bool pRestartLevel { get; set; }

	public int[] pRealmLevelsUnlockInfo => mRealmLevelsUnlockInfo;

	public int pRealmIndex => mRealmIndex;

	public int pLevelIndex => mLevelIndex;

	public Data pSTLevelData => mSTLevelData;

	public bool pLevelDataReady => mLevelDataReady;

	public bool pPrefetchDone => mPrefetchDone;

	public bool pEnablePlayBtn
	{
		get
		{
			return mEnablePlayBtn;
		}
		set
		{
			mEnablePlayBtn = value;
		}
	}

	public string pLastLevel
	{
		get
		{
			return mLastLevel;
		}
		set
		{
			mLastLevel = value;
		}
	}

	public static LevelManager pInstance => mInstance;

	private void Awake()
	{
		if (mInstance == null)
		{
			mInstance = this;
			Object.DontDestroyOnLoad(base.gameObject);
			MainStreetMMOClient.pInstance.SetJoinAllowed(MMOJoinStatus.NOT_ALLOWED);
			MainStreetMMOClient.pInstance.pIgnoreIdleTimeOut = true;
			MainStreetMMOClient.Init();
			UiChatHistory.Init();
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: false);
		}
	}

	private void OnDestroy()
	{
		if (mInstance == this)
		{
			mInstance = null;
			MainStreetMMOClient.pInstance.SetJoinAllowed(MMOJoinStatus.ALLOWED);
			MainStreetMMOClient.pInstance.pIgnoreIdleTimeOut = false;
			UiChatHistory.Init();
		}
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: true);
		}
	}

	public void DeleteInstance()
	{
		if (mInstance != null)
		{
			Object.Destroy(mInstance.gameObject);
		}
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void SetUIReferences(Scene newScene)
	{
		if (newScene.name == "STLevelSelectionDO")
		{
			_LevelSelection = (UiLevelSelection)Object.FindObjectOfType(typeof(UiLevelSelection));
			_TeamSelection = (UiTeamSelection)Object.FindObjectOfType(typeof(UiTeamSelection));
		}
	}

	private void OnSceneLoaded(Scene newScene, LoadSceneMode loadSceneMode)
	{
		if (newScene.name != "STLevelSelectionDO" && newScene.name != "Transition" && GameManager.pInstance == null)
		{
			Object.Destroy(base.gameObject);
		}
		CharacterDatabase.pInstance.Init();
		SetUIReferences(newScene);
		if (!(newScene.name == "STLevelSelectionDO") || !(MissionManager.pInstance != null))
		{
			return;
		}
		foreach (Task pActiveTask in MissionManager.pInstance.pActiveTasks)
		{
			if (pActiveTask._Active && pActiveTask.pCompleted)
			{
				GameUtilities.DisplayGenericDB("PfKAUIGenericDBSm", _MissionDTCompleteText.GetLocalizedString(), _MissionDTCompleteTitleText.GetLocalizedString(), base.gameObject, "OnClickYesMessageDB", "OnClickNoMessageDB", "", "", inDestroyOnClick: true);
				break;
			}
		}
	}

	public void OnClickYesMessageDB()
	{
		RsResourceManager.LoadLevel(RealmHolder.pInstance.pLastLevel);
	}

	public void OnClickNoMessageDB()
	{
	}

	private void Start()
	{
		if (mRealmIndex == -1)
		{
			if (RsResourceManager.pLastLevel == RealmHolder.pInstance._StoreSceneName)
			{
				mLastLevel = RealmHolder.pInstance.pLastLevel;
			}
			else
			{
				RealmHolder.pInstance.pLastLevel = (mLastLevel = RsResourceManager.pLastLevel);
			}
		}
		if (RealmHolder.pInstance.pSaveLevel)
		{
			mRealmIndex = RealmHolder.pInstance.pRealmIndex;
			mLevelIndex = RealmHolder.pInstance.pRealmLevelIndex;
		}
		if (mSTLevelData == null)
		{
			RsResourceManager.Load(GameConfig.GetKeyData("SquadTacticsLevelDataFile"), LoadXML);
		}
	}

	private void LoadXML(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inFile, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			mSTLevelData = UtUtilities.DeserializeFromXml<Data>((string)inFile);
			InitRealmData();
			mLevelDataReady = true;
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.Log("Failed to Load IM LEVEL DATA XML");
			break;
		}
	}

	public void Init()
	{
		mPrefetchStarted = false;
		mPrefetchDone = false;
	}

	private void Update()
	{
		if (mLevelDataReady && mRealmIndex != -1)
		{
			if (!mPrefetchStarted && !string.IsNullOrEmpty(mSTLevelData.CommonPrefetchList))
			{
				PrefetchManager.Init(new List<string>
				{
					mSTLevelData.CommonPrefetchList,
					pSTLevelData.RealmsData[mRealmIndex].LevelPrefetchList
				});
				mPrefetchStarted = true;
			}
			else if (mPrefetchStarted && PrefetchManager.pInstance != null && PrefetchManager.pInstance.pState == PrefetchManager.State.WAIT_FOR_START)
			{
				PrefetchManager.StartPrefetch();
				PrefetchManager.ShowDownloadProgress();
			}
			else if (mPrefetchStarted && !mPrefetchDone && PrefetchManager.pInstance != null && PrefetchManager.pIsReady)
			{
				mPrefetchDone = true;
				mEnablePlayBtn = true;
				_TeamSelection.ReadyButton();
			}
		}
	}

	public bool IsTutorialFinished()
	{
		return ProductData.TutorialComplete(_TutorialKeyName);
	}

	private void InitRealmData()
	{
		int num = mSTLevelData.RealmsData.Length;
		mRealmLevelsUnlockInfo = new int[num];
		string stringValue = ProductData.pPairData.GetStringValue("STRealmLevelsUnlockData", string.Empty);
		if (string.IsNullOrEmpty(stringValue))
		{
			ProductData.pPairData.SetValueAndSave("STRealmLevelsUnlockData", GetUnlockDataString(mRealmLevelsUnlockInfo));
			return;
		}
		string[] array = stringValue.Split(new char[1] { '$' });
		if (array.Length > num)
		{
			UtDebug.LogWarning(" Realm Levels Data mismatch: TotalRealms saved info more than XML Realms");
		}
		for (int i = 0; i < num; i++)
		{
			int result = 0;
			if (i < array.Length)
			{
				int.TryParse(array[i], out result);
			}
			mRealmLevelsUnlockInfo[i] = result;
		}
	}

	private string GetUnlockDataString(int[] realmArray)
	{
		string text = string.Empty;
		for (int i = 0; i < realmArray.Length; i++)
		{
			text = ((i == realmArray.Length - 1) ? (text + realmArray[i]) : (text + realmArray[i] + '$'));
		}
		return text;
	}

	public void UpdateUnlockedLevelData()
	{
		if (string.Compare(mSTLevelData.RealmsData[mRealmIndex].Name._Text, _TrainingCourseRealm, ignoreCase: true) == 0)
		{
			UserAchievementTask.Set(_TrainingCourseAchievementID, SceneManager.GetActiveScene().name);
		}
		else if (string.Compare(mSTLevelData.RealmsData[mRealmIndex].Name._Text, _StormHeartRealm, ignoreCase: true) == 0)
		{
			UserAchievementTask.Set(_StormHeartAchievementID, SceneManager.GetActiveScene().name);
		}
		bool flag = false;
		if (mRealmLevelsUnlockInfo[mRealmIndex] == mLevelIndex)
		{
			flag = true;
			mRealmLevelsUnlockInfo[mRealmIndex] = mLevelIndex + 1;
		}
		if (mRealmLevelsUnlockInfo.Length != 0 && flag && mRealmLevelsUnlockInfo != null)
		{
			ProductData.pPairData.SetValueAndSave("STRealmLevelsUnlockData", GetUnlockDataString(mRealmLevelsUnlockInfo));
		}
	}

	public void SetLevelPlaying(int inRealmIndex, int inLevelIndex)
	{
		if (inRealmIndex != mRealmIndex || inLevelIndex != mLevelIndex)
		{
			mRealmIndex = inRealmIndex;
			mLevelIndex = inLevelIndex;
			if (mLevelScenes == null)
			{
				mLevelScenes = new List<string>();
			}
			else
			{
				mLevelScenes.Clear();
			}
		}
	}

	public void LoadLevel()
	{
		if (RealmHolder.pInstance != null)
		{
			RealmHolder.pInstance.ResetRealmValue();
		}
		if (mLevelScenes.Count == 0)
		{
			mLevelScenes.AddRange(mSTLevelData.RealmsData[mRealmIndex].RealmLevelsInfo[mLevelIndex].Scenes);
			UtUtilities.Shuffle(mLevelScenes);
		}
		PrefetchManager.Kill();
		RsResourceManager.LoadLevel(mLevelScenes[0], skipMMOLogin: true);
		mLevelScenes.RemoveAt(0);
	}

	public static void UnlockLevels(bool inUnlock)
	{
		pUnlockLevels = inUnlock;
		pRefreshLevels = true;
	}

	public RealmLevel GetLevelInfo()
	{
		if (mRealmIndex == -1)
		{
			return null;
		}
		return mSTLevelData.RealmsData[mRealmIndex].RealmLevelsInfo[mLevelIndex];
	}

	public void SetSquad(List<UnitSelection> list)
	{
		mSquadList = list;
	}

	public List<UnitSelection> GetSquadList()
	{
		return mSquadList;
	}

	public void LevelPrefetch(int realmIndex)
	{
		mRealmIndex = realmIndex;
		mEnablePlayBtn = false;
		mLevelDataReady = true;
		Init();
		PrefetchManager.pInstance.Reset();
		PrefetchManager.pInstance.pState = PrefetchManager.State.NONE;
		_TeamSelection.ReadyButton();
	}
}
