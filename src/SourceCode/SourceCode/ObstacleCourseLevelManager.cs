using System;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleCourseLevelManager : MonoBehaviour, IConsumable
{
	public class ScoreData
	{
		public int mScore;

		public static int mCount;

		public static bool mDataReturnFail;
	}

	public class DragonLevelUnlockingData
	{
		public int pLastUnlockedLevel;

		public int pLastUnlockedHeroLevel;

		public DateTime pLastPlayedTime;

		public int pNumLevels;

		public int pNumHeroLevels;

		public DragonLevelUnlockingData(int inLastUnlockedLevel, int inLastUnlockedHeroLevel, DateTime inLastPlayedtime, int inNumlevels, int inNumHeroLevels)
		{
			pLastUnlockedLevel = inLastUnlockedLevel;
			pLastUnlockedHeroLevel = inLastUnlockedHeroLevel;
			pLastPlayedTime = inLastPlayedtime;
			pNumLevels = inNumlevels;
			pNumHeroLevels = inNumHeroLevels;
		}

		public bool IsAllHeroLevelsPlayed()
		{
			return pLastUnlockedHeroLevel >= pNumHeroLevels;
		}

		public bool IsAllLevelsPlayed()
		{
			return pLastUnlockedLevel >= pNumLevels;
		}
	}

	public static FSMenuState mMenuState;

	public FlightSchoolIntroTutorial _FlightSchoolIntroTut;

	public SceneLevelGroupData[] _AdultData;

	public SceneLevelGroupData[] _TeenData;

	public SceneLevelGroupData[] _FlightSuitData;

	public GameObject _ActiveObject;

	public GameObject _LevelSelectionUi;

	public GameObject _DefaultLevelSelectionUi;

	public UiSelectGameModes _GameModeSelectionUi;

	public UiSelectHeroDragons _DragonSelectionUi;

	public int _ObsCourseLevelPairDataID = 98790;

	public AudioClip _MusDragFlightSchool;

	public AudioClip _SndDragVictory;

	public int _GameID = 14;

	public int _LevelUnlockTimeInMinutes = 300;

	public int _GemsNeededToUnlockLevel;

	public UiCountDown _UiCountdown;

	public int _InitialNonMemberUnlockedLevel;

	public int _StoreID = 93;

	public int _LevelUnlockItemID = 7109;

	public string _ExitMarker = "PfMarker_FlightSchoolDOExit";

	public string _ExitLevel = "HubSchoolDO";

	public LocaleString _LevelLoadFailedText = new LocaleString("Server error. Please retry.");

	public HeroPetDifficultyData[] _HeroDifficulty;

	public int _LevelCompleteAchievementTaskID = 159;

	public int _LevelCompleteClanAchievementTaskID = 187;

	public int _APlusAchievementTaskID = 160;

	public int _APlusClanAchievementTaskID = 188;

	public LocaleString _ScoreTitleText = new LocaleString("Score:");

	public LocaleString _TotalTimeTitleText = new LocaleString("Total Time:");

	public LocaleString _PickUpTitleText = new LocaleString("Pick Ups:");

	public LocaleString _RingTitleText = new LocaleString("Rings:");

	public LocaleString _TargetablesTitleText = new LocaleString("Targetables:");

	public LocaleString _CourseFailedTitleText = new LocaleString("Course Failed!");

	public LocaleString _CourseCompleteTitleText = new LocaleString("Course Complete!");

	[Header("Achievement IDs")]
	public AchievementTaskData _FightClubMasterAchievement;

	public AchievementTaskData _FightClubEnthusiastAchievement;

	public AchievementTaskData _HeroFightClubExpertAchievement;

	public AchievementTaskData _ItemHoarderAchievement;

	public List<AchievementTaskData> _FlightSchoolAchievements;

	public UiDragonsEndDB _EndDBUI;

	public int _PointsForNoCollectItems = 1000;

	public UiConsumable _UiConsumable;

	public PowerUpManager _PowerUpManager;

	public CaAvatarCam _AvatarCamera;

	public bool _DisableMoodParticle = true;

	private ObstacleCourseGame mCourseLevel;

	[HideInInspector]
	public bool mGradeDataReady;

	[HideInInspector]
	public int mCurrentLevelHighScore;

	[HideInInspector]
	public bool mIsCurrentLevelSpecial;

	private UiObstacleCourseMenu mLevelMenu;

	private PairData mLastLevelUnlockedData;

	private string mLevelUnlockedStr;

	private int mLastUnlockedLevel;

	private int mCurrentLevel;

	private bool mPlaying;

	private bool mLevelCompleted;

	private int mNextCheckPoint;

	private DateTime mLastPlayedTime = DateTime.Now;

	private ObStatus mStatus;

	private bool mDoLoadData;

	private bool mLevelReady;

	private bool mStoreItemsLoaded;

	private KAUIGenericDB mUiGenericDB;

	private int mLevelUnLockCost;

	private static GetGameDataResponse mFlightSchoolGameData;

	private PairData mFSScorePairData;

	private FlightSchoolScoreData mFSGameDataFromPairData;

	private GetGameDataResponse mFSDragonGameData;

	private bool mScorePairDataFail;

	private bool mScorePairDataReady;

	private Dictionary<int, DragonLevelUnlockingData> mFlightModeLevelUnlockingDataMap = new Dictionary<int, DragonLevelUnlockingData>();

	private float mNextCheckPtTimeleft;

	public int pRewardCoins;

	public int pDragonPoints;

	private static FSGameMode mGameMode;

	private PowerUpHelper mPowerUpHelper = new PowerUpHelper();

	public ObstacleCourseGame pCourseLevel
	{
		get
		{
			return mCourseLevel;
		}
		set
		{
			mCourseLevel = value;
		}
	}

	public bool pIsPairDataReady => mLastLevelUnlockedData != null;

	public int pLevelUnLockCost => mLevelUnLockCost;

	public GetGameDataResponse pFlightSchoolGameData
	{
		get
		{
			return mFlightSchoolGameData;
		}
		set
		{
			mFlightSchoolGameData = value;
		}
	}

	public Dictionary<int, DragonLevelUnlockingData> pFlightModeLevelUnlockingDataMap => mFlightModeLevelUnlockingDataMap;

	public int pNextCheckPoint => mNextCheckPoint;

	public int pLastUnlockedLevel
	{
		get
		{
			if (mGameMode == FSGameMode.FLIGHT_MODE)
			{
				if (_DragonSelectionUi.pSelectedTicketID != -1 && mFlightModeLevelUnlockingDataMap.ContainsKey(_DragonSelectionUi.pSelectedTicketID))
				{
					return mFlightModeLevelUnlockingDataMap[_DragonSelectionUi.pSelectedTicketID].pLastUnlockedLevel;
				}
				return 0;
			}
			return mLastUnlockedLevel;
		}
	}

	public int pLastUnlockedHeroLevel
	{
		get
		{
			if (mGameMode == FSGameMode.FLIGHT_MODE)
			{
				if (_DragonSelectionUi.pSelectedTicketID != -1 && mFlightModeLevelUnlockingDataMap.ContainsKey(_DragonSelectionUi.pSelectedTicketID))
				{
					return mFlightModeLevelUnlockingDataMap[_DragonSelectionUi.pSelectedTicketID].pLastUnlockedHeroLevel;
				}
				return 0;
			}
			return mLastUnlockedLevel;
		}
	}

	public UiObstacleCourseMenu pLevelMenu
	{
		get
		{
			return mLevelMenu;
		}
		set
		{
			mLevelMenu = value;
		}
	}

	public bool pLevelCompleted => mLevelCompleted;

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

	public DateTime pLastPlayedTime
	{
		get
		{
			if (mGameMode == FSGameMode.FLIGHT_MODE)
			{
				if (_DragonSelectionUi.pSelectedTicketID != -1 && mFlightModeLevelUnlockingDataMap.ContainsKey(_DragonSelectionUi.pSelectedTicketID))
				{
					return mFlightModeLevelUnlockingDataMap[_DragonSelectionUi.pSelectedTicketID].pLastPlayedTime;
				}
				return ServerTime.pCurrentTime;
			}
			return mLastPlayedTime;
		}
	}

	public FSGameMode pGameMode
	{
		get
		{
			return mGameMode;
		}
		set
		{
			mGameMode = value;
		}
	}

	public void SetLevelCompleted(bool bCompleted)
	{
		mLevelCompleted = bCompleted;
	}

	public virtual void Awake()
	{
		mStatus = GetComponent<ObStatus>();
		ItemStoreDataLoader.Load(_StoreID, OnStoreLoaded);
		if (mMenuState != FSMenuState.FS_STATE_LEVELSELECT)
		{
			pGameMode = FSGameMode.FLIGHT_MODE;
		}
		if (mCourseLevel != null && mCourseLevel.pTimeLimit >= 0f)
		{
			mPowerUpHelper._ApplyExtraTime = ApplyExtraTime;
		}
		if (_PowerUpManager != null)
		{
			_PowerUpManager.Init(this, mPowerUpHelper);
		}
	}

	public virtual void Start()
	{
		if (_UiConsumable != null)
		{
			_UiConsumable.SetGameData(this, "FlightClub");
			_UiConsumable.SetVisibility(inVisible: false);
		}
		_AvatarCamera.SetLookAt(null, null, 0f);
		if (mMenuState != FSMenuState.FS_STATE_LEVELSELECT)
		{
			ShowGameModeUI();
		}
		else
		{
			ShowDragonsUI();
		}
	}

	public void ShowTutorial()
	{
		if (!(_FlightSchoolIntroTut == null) && !_FlightSchoolIntroTut.TutorialComplete())
		{
			_FlightSchoolIntroTut.InitTutorial();
			_FlightSchoolIntroTut.ShowTutorial();
		}
	}

	private void OnLevelReady()
	{
		if (_ActiveObject != null)
		{
			_ActiveObject.SetActive(value: true);
		}
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.pObject.SetActive(value: false);
		if (SanctuaryManager.pInstance != null && SanctuaryManager.pInstance.pPetMeter != null)
		{
			SanctuaryManager.pInstance.pPetMeter.SetVisibility(inVisible: false);
		}
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: false);
		}
		mLevelReady = true;
		AvAvatar.pLevelState = AvAvatarLevelState.FLIGHTSCHOOL;
	}

	public void OnPairDataReady(bool success, PairData pData, object inUserData)
	{
		mLastLevelUnlockedData = pData;
		if (mLastLevelUnlockedData == null || mGameMode != FSGameMode.FLIGHT_MODE)
		{
			return;
		}
		ItemData[] items = _DragonSelectionUi.pStoreData._Items;
		foreach (ItemData itemData in items)
		{
			if (itemData.FinalCashCost >= 0)
			{
				UpdateHeroDragonPairData(itemData.ItemID, mLastLevelUnlockedData, mGameMode);
			}
		}
	}

	private void UpdateHeroDragonPairData(int inItemID, PairData inPairData, FSGameMode inGameMode)
	{
		HeroPetData heroDragonFromID = SanctuaryData.GetHeroDragonFromID(inItemID);
		if (heroDragonFromID != null)
		{
			int numLevels = 0;
			int numHeroLevels = 0;
			GetNumLevels(heroDragonFromID._Name, ref numLevels, ref numHeroLevels, inGameMode);
			int num = 0;
			int num2 = 0;
			string inLevelPairDataKey = "";
			string inHeroLevelPairDataKey = "";
			GetPairDataKeyByDragonName(heroDragonFromID._Name, ref inLevelPairDataKey, ref inHeroLevelPairDataKey);
			num = inPairData.GetIntValue(inLevelPairDataKey, 0);
			num2 = inPairData.GetIntValue(inHeroLevelPairDataKey, 0);
			mFlightModeLevelUnlockingDataMap[inItemID] = new DragonLevelUnlockingData(num, num2, ServerTime.pCurrentTime, numLevels, numHeroLevels);
		}
	}

	private void GetNumLevels(string inDragonName, ref int numLevels, ref int numHeroLevels, FSGameMode inGameMode)
	{
		SceneLevelGroupData[] array = ((inGameMode == FSGameMode.FLIGHT_MODE) ? _AdultData : _TeenData);
		numLevels = array[0]._GroupData.Length;
		SceneLevelGroupData[] array2 = array;
		foreach (SceneLevelGroupData sceneLevelGroupData in array2)
		{
			string[] dragonName = sceneLevelGroupData._DragonName;
			for (int j = 0; j < dragonName.Length; j++)
			{
				if (dragonName[j] == inDragonName)
				{
					numHeroLevels = sceneLevelGroupData._GroupData.Length;
				}
			}
		}
	}

	public void SaveGemUnlock()
	{
		DateTime dateTime = ServerTime.pCurrentTime.Add(new TimeSpan(0, -(_LevelUnlockTimeInMinutes + 1), 0));
		if (_DragonSelectionUi.pSelectedTicketID == 0)
		{
			switch (mGameMode)
			{
			case FSGameMode.GLIDE_MODE:
				mFSGameDataFromPairData.GlideModeLastPlayedTime = dateTime;
				mLastPlayedTime = dateTime;
				break;
			case FSGameMode.FLIGHT_SUIT_MODE:
				mFSGameDataFromPairData.FlightSuitModeLastPlayedTime = dateTime;
				mLastPlayedTime = dateTime;
				break;
			default:
			{
				DragonLevelUnlockingData dragonLevelUnlockingData = mFlightModeLevelUnlockingDataMap[0];
				if (dragonLevelUnlockingData != null)
				{
					mFSGameDataFromPairData.FlightModeLastPlayedTime = dateTime;
					dragonLevelUnlockingData.pLastPlayedTime = dateTime;
				}
				break;
			}
			}
			SaveDragonGameDataInPairData();
		}
		else
		{
			DragonLevelUnlockingData dragonLevelUnlockingData2 = mFlightModeLevelUnlockingDataMap[_DragonSelectionUi.pSelectedTicketID];
			if (dragonLevelUnlockingData2 != null)
			{
				mLastLevelUnlockedData.SetValueAndSave("LastAdultPlayedTime", dateTime.ToString());
				dragonLevelUnlockingData2.pLastPlayedTime = dateTime;
			}
		}
	}

	public void LoadData()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		mDoLoadData = true;
		mStatus.pIsReady = true;
		LoadPairData();
		mLevelMenu = GetComponentInChildren<UiObstacleCourseMenu>();
		if (mFlightSchoolGameData == null)
		{
			GetScoreData();
			return;
		}
		mGradeDataReady = true;
		ScoreData.mDataReturnFail = false;
	}

	public void LoadPairData()
	{
		mScorePairDataFail = false;
		mFSGameDataFromPairData = null;
		mFSDragonGameData = null;
		if (_DragonSelectionUi.pSelectedTicketID == 0 && SanctuaryManager.pCurPetData != null && SanctuaryManager.pCurPetData.EntityID.HasValue)
		{
			mScorePairDataReady = false;
			WsWebService.GetKeyValuePairByUserID(SanctuaryManager.pCurPetData.EntityID.ToString(), _ObsCourseLevelPairDataID, WsEventHandler, null);
		}
		else
		{
			mScorePairDataReady = true;
		}
		PairData.Load(_ObsCourseLevelPairDataID, OnPairDataReady, null);
	}

	public void WsEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			if (inType == WsServiceType.GET_KEY_VALUE_PAIR_BY_USER_ID)
			{
				PairData pairData;
				if (inObject != null)
				{
					pairData = (PairData)inObject;
					pairData.Init();
				}
				else
				{
					pairData = new PairData();
				}
				CreateGameDataFromPairData(pairData);
				mScorePairDataReady = true;
			}
			break;
		case WsServiceEvent.ERROR:
			mScorePairDataFail = true;
			UtDebug.LogError("Error loading pair data");
			break;
		}
	}

	private void CreateGameDataFromPairData(PairData pairData)
	{
		mFSScorePairData = pairData;
		if (pairData.KeyExists("FlightSchoolScoreData"))
		{
			string value = pairData.GetValue("FlightSchoolScoreData");
			mFSGameDataFromPairData = UtUtilities.DeserializeFromXml(value, typeof(FlightSchoolScoreData)) as FlightSchoolScoreData;
		}
		if (mFSGameDataFromPairData == null)
		{
			mFSGameDataFromPairData = new FlightSchoolScoreData();
			mFSGameDataFromPairData.GlideModeScores = new List<int>();
			mFSGameDataFromPairData.FlightModeScores = new List<int>();
			mFSGameDataFromPairData.FlightSuitModeScores = new List<int>();
			mFSGameDataFromPairData.GlideModeLastPlayedTime = ServerTime.pCurrentTime;
			mFSGameDataFromPairData.GlideModeLastUnlockedLevel = 0;
			mFSGameDataFromPairData.FlightModeLastPlayedTime = ServerTime.pCurrentTime;
			mFSGameDataFromPairData.FlightModeLastUnlockedLevel = 0;
			mFSGameDataFromPairData.FlightSuitModeLastPlayedTime = ServerTime.pCurrentTime;
			mFSGameDataFromPairData.FlightModeLastUnlockedLevel = 0;
			SaveDragonGameDataInPairData();
		}
		mFSDragonGameData = new GetGameDataResponse();
		mFSDragonGameData.GameDataSummaryList = new List<GameDataSummary>();
		int num = 0;
		foreach (int glideModeScore in mFSGameDataFromPairData.GlideModeScores)
		{
			mFSDragonGameData.GameDataSummaryList.Add(CreateGameDataSummary(1, num++, glideModeScore));
		}
		num = 0;
		foreach (int flightModeScore in mFSGameDataFromPairData.FlightModeScores)
		{
			mFSDragonGameData.GameDataSummaryList.Add(CreateGameDataSummary(3, num++, flightModeScore));
		}
		int flightModeLastUnlockedLevel = mFSGameDataFromPairData.FlightModeLastUnlockedLevel;
		DateTime flightModeLastPlayedTime = mFSGameDataFromPairData.FlightModeLastPlayedTime;
		int numLevels = 0;
		int numHeroLevels = 0;
		GetNumLevels("", ref numLevels, ref numHeroLevels, FSGameMode.FLIGHT_MODE);
		mFlightModeLevelUnlockingDataMap[0] = new DragonLevelUnlockingData(flightModeLastUnlockedLevel, 0, flightModeLastPlayedTime, numLevels, numHeroLevels);
		if (mGameMode == FSGameMode.FLIGHT_SUIT_MODE)
		{
			mLastUnlockedLevel = mFSGameDataFromPairData.FlightSuitModeLastUnlockedLevel;
			mLastPlayedTime = mFSGameDataFromPairData.FlightSuitModeLastPlayedTime;
		}
		else
		{
			mLastUnlockedLevel = mFSGameDataFromPairData.GlideModeLastUnlockedLevel;
			mLastPlayedTime = mFSGameDataFromPairData.GlideModeLastPlayedTime;
		}
	}

	private GameDataSummary CreateGameDataSummary(int difficulty, int level, int score)
	{
		GameDataSummary obj = new GameDataSummary
		{
			Difficulty = difficulty,
			GameLevel = level + 1,
			GameDataList = new GameData[1]
		};
		obj.GameDataList[0] = new GameData
		{
			Value = score
		};
		return obj;
	}

	private void GetScoreData()
	{
		int gameID = _GameID;
		if (_DragonSelectionUi.pSelectedTicketID > 0)
		{
			HeroPetData heroDragonFromID = SanctuaryData.GetHeroDragonFromID(_DragonSelectionUi.pSelectedTicketID);
			for (int i = 0; i < _HeroDifficulty.Length; i++)
			{
				if (!(_HeroDifficulty[i]._HeroPetName != heroDragonFromID._Name))
				{
					gameID = _HeroDifficulty[i]._GameID;
					break;
				}
			}
		}
		GetGameDataRequest dataRequest = new GetGameDataRequest
		{
			GameID = gameID,
			TopScoresOnly = true
		};
		mGradeDataReady = false;
		WsWebService.GetGameData(dataRequest, ServiceEventHandler, null);
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			mFlightSchoolGameData = (GetGameDataResponse)inObject;
			mGradeDataReady = true;
			ScoreData.mDataReturnFail = false;
			break;
		case WsServiceEvent.ERROR:
			mFlightSchoolGameData = null;
			mGradeDataReady = true;
			ScoreData.mDataReturnFail = true;
			break;
		}
	}

	public int GetPlayerHighScore(int level, bool isHeroDragon)
	{
		int result = 0;
		int num = 1;
		if (mGameMode == FSGameMode.FLIGHT_MODE)
		{
			num = 3;
		}
		if (mFlightSchoolGameData == null || mFlightSchoolGameData.GameDataSummaryList == null)
		{
			return result;
		}
		foreach (GameDataSummary gameDataSummary in mFlightSchoolGameData.GameDataSummaryList)
		{
			if (((isHeroDragon && gameDataSummary.Difficulty != 3) || (!isHeroDragon && gameDataSummary.Difficulty == num)) && gameDataSummary.GameLevel == level + 1)
			{
				result = gameDataSummary.GameDataList[0].Value;
				break;
			}
		}
		return result;
	}

	public int GetPlayerDragonScore(int level)
	{
		int result = 0;
		int num = 1;
		if (mGameMode == FSGameMode.FLIGHT_MODE)
		{
			num = 3;
		}
		if (mFSDragonGameData?.GameDataSummaryList == null)
		{
			return result;
		}
		foreach (GameDataSummary gameDataSummary in mFSDragonGameData.GameDataSummaryList)
		{
			if (gameDataSummary.Difficulty == num && gameDataSummary.GameLevel == level + 1)
			{
				result = gameDataSummary.GameDataList[0].Value;
				break;
			}
		}
		return result;
	}

	public void LevelEnd(int score, string grade, int type)
	{
		int num = 0;
		UpdateScoreData(score, type);
		switch (mGameMode)
		{
		case FSGameMode.FLIGHT_MODE:
			num = _AdultData[mLevelMenu.mCurrentSceneId]._GroupData.Length;
			break;
		case FSGameMode.GLIDE_MODE:
			num = _TeenData[mLevelMenu.mCurrentSceneId]._GroupData.Length;
			break;
		case FSGameMode.FLIGHT_SUIT_MODE:
			num = _FlightSuitData[mLevelMenu.mCurrentSceneId]._GroupData.Length;
			break;
		}
		if ((pLastUnlockedLevel < num && mCurrentLevel >= pLastUnlockedLevel - 1) || (mLevelCompleted && pLastUnlockedLevel < num && mCurrentLevel >= pLastUnlockedLevel - 1) || (pLastUnlockedHeroLevel < num && mCurrentLevel >= pLastUnlockedHeroLevel - 1 && type == 1))
		{
			if (_SndDragVictory != null)
			{
				SnChannel.Play(_SndDragVictory, "SFX_Pool", inForce: true, null);
			}
			if (grade != "B-" && grade != "C")
			{
				switch (mGameMode)
				{
				case FSGameMode.FLIGHT_MODE:
				{
					if (!(SanctuaryManager.pCurPetInstance != null) || _DragonSelectionUi.pSelectedTicketID == -1)
					{
						break;
					}
					HeroPetData heroDragonFromID = SanctuaryData.GetHeroDragonFromID(_DragonSelectionUi.pSelectedTicketID);
					if (heroDragonFromID == null || !mFlightModeLevelUnlockingDataMap.ContainsKey(_DragonSelectionUi.pSelectedTicketID))
					{
						break;
					}
					DragonLevelUnlockingData dragonLevelUnlockingData = mFlightModeLevelUnlockingDataMap[_DragonSelectionUi.pSelectedTicketID];
					if (dragonLevelUnlockingData.pLastUnlockedLevel < mCurrentLevel + 1)
					{
						dragonLevelUnlockingData.pLastUnlockedLevel = mCurrentLevel + 1;
						if (_DragonSelectionUi.pSelectedTicketID == 0)
						{
							dragonLevelUnlockingData.pLastPlayedTime = ServerTime.pCurrentTime;
							mFSGameDataFromPairData.FlightModeLastPlayedTime = dragonLevelUnlockingData.pLastPlayedTime;
						}
					}
					string inLevelPairDataKey = "";
					string inHeroLevelPairDataKey = "";
					GetPairDataKeyByDragonName(heroDragonFromID._Name, ref inLevelPairDataKey, ref inHeroLevelPairDataKey);
					if (_DragonSelectionUi.pSelectedTicketID == 0)
					{
						mFSGameDataFromPairData.FlightModeLastUnlockedLevel = dragonLevelUnlockingData.pLastUnlockedLevel;
					}
					else if (_DragonSelectionUi.pSelectedTicketID == heroDragonFromID._ItemID)
					{
						if (type == 0)
						{
							mLastLevelUnlockedData.SetValue(inLevelPairDataKey, dragonLevelUnlockingData.pLastUnlockedLevel.ToString());
						}
						else if (dragonLevelUnlockingData.pLastUnlockedHeroLevel < mCurrentLevel + 1)
						{
							dragonLevelUnlockingData.pLastUnlockedHeroLevel++;
							mLastLevelUnlockedData.SetValue(inHeroLevelPairDataKey, dragonLevelUnlockingData.pLastUnlockedHeroLevel.ToString());
						}
					}
					break;
				}
				case FSGameMode.GLIDE_MODE:
					if (mLastUnlockedLevel < mCurrentLevel + 1)
					{
						mLastUnlockedLevel = mCurrentLevel + 1;
						mLastPlayedTime = ServerTime.pCurrentTime;
						mFSGameDataFromPairData.GlideModeLastPlayedTime = mLastPlayedTime;
					}
					mFSGameDataFromPairData.GlideModeLastUnlockedLevel = mLastUnlockedLevel;
					break;
				case FSGameMode.FLIGHT_SUIT_MODE:
					if (mLastUnlockedLevel < mCurrentLevel + 1)
					{
						mLastUnlockedLevel = mCurrentLevel + 1;
						mLastPlayedTime = ServerTime.pCurrentTime;
						mFSGameDataFromPairData.FlightSuitModeLastPlayedTime = mLastPlayedTime;
					}
					mFSGameDataFromPairData.FlightSuitModeLastUnlockedLevel = mLastUnlockedLevel;
					break;
				}
				PairData.Save(_ObsCourseLevelPairDataID);
			}
		}
		if (mFSGameDataFromPairData != null)
		{
			SaveDragonGameDataInPairData();
		}
		mPlaying = false;
	}

	public void CheatToUnlockAllLevels()
	{
		int num = ((mGameMode == FSGameMode.FLIGHT_MODE) ? _AdultData[mLevelMenu.mCurrentSceneId]._GroupData.Length : _TeenData[mLevelMenu.mCurrentSceneId]._GroupData.Length);
		switch (mGameMode)
		{
		case FSGameMode.FLIGHT_MODE:
		{
			if (!(SanctuaryManager.pCurPetInstance != null) || _DragonSelectionUi.pSelectedTicketID == -1)
			{
				break;
			}
			HeroPetData heroDragonFromID = SanctuaryData.GetHeroDragonFromID(_DragonSelectionUi.pSelectedTicketID);
			if (heroDragonFromID != null && mFlightModeLevelUnlockingDataMap.ContainsKey(_DragonSelectionUi.pSelectedTicketID))
			{
				DragonLevelUnlockingData dragonLevelUnlockingData = mFlightModeLevelUnlockingDataMap[_DragonSelectionUi.pSelectedTicketID];
				dragonLevelUnlockingData.pLastUnlockedLevel = num - 1;
				string inLevelPairDataKey = "";
				string inHeroLevelPairDataKey = "";
				GetPairDataKeyByDragonName(heroDragonFromID._Name, ref inLevelPairDataKey, ref inHeroLevelPairDataKey);
				if (_DragonSelectionUi.pSelectedTicketID == 0)
				{
					mFSGameDataFromPairData.FlightModeLastUnlockedLevel = dragonLevelUnlockingData.pLastUnlockedLevel;
					dragonLevelUnlockingData.pLastPlayedTime = ServerTime.pCurrentTime;
					mFSGameDataFromPairData.FlightModeLastPlayedTime = dragonLevelUnlockingData.pLastPlayedTime;
				}
				else if (_DragonSelectionUi.pSelectedTicketID == heroDragonFromID._ItemID)
				{
					mLastLevelUnlockedData.SetValue(inLevelPairDataKey, dragonLevelUnlockingData.pLastUnlockedLevel.ToString());
					dragonLevelUnlockingData.pLastUnlockedHeroLevel = dragonLevelUnlockingData.pNumHeroLevels - 1;
					mLastLevelUnlockedData.SetValue(inHeroLevelPairDataKey, dragonLevelUnlockingData.pLastUnlockedHeroLevel.ToString());
				}
			}
			break;
		}
		case FSGameMode.GLIDE_MODE:
			mLastUnlockedLevel = num - 1;
			mLastPlayedTime = ServerTime.pCurrentTime;
			mFSGameDataFromPairData.GlideModeLastPlayedTime = mLastPlayedTime;
			mFSGameDataFromPairData.GlideModeLastUnlockedLevel = mLastUnlockedLevel;
			break;
		case FSGameMode.FLIGHT_SUIT_MODE:
			mLastUnlockedLevel = num - 1;
			mLastPlayedTime = ServerTime.pCurrentTime;
			mFSGameDataFromPairData.FlightSuitModeLastPlayedTime = mLastPlayedTime;
			mFSGameDataFromPairData.FlightSuitModeLastUnlockedLevel = mLastUnlockedLevel;
			break;
		}
		PairData.Save(_ObsCourseLevelPairDataID);
		if (mFSGameDataFromPairData != null)
		{
			SaveDragonGameDataInPairData();
		}
	}

	private void UpdateScoreData(int score, int type)
	{
		if (mFlightSchoolGameData != null)
		{
			UpdateGameData(mFlightSchoolGameData, score, type);
		}
		if (mFSGameDataFromPairData != null && mFSDragonGameData != null)
		{
			UpdateGameData(mFSDragonGameData, score, type);
		}
	}

	private void SaveDragonGameDataInPairData()
	{
		if (mFSDragonGameData != null && mFSDragonGameData.GameDataSummaryList != null)
		{
			foreach (GameDataSummary gameDataSummary in mFSDragonGameData.GameDataSummaryList)
			{
				if (gameDataSummary == null || gameDataSummary.GameDataList == null || gameDataSummary.GameDataList.Length == 0)
				{
					continue;
				}
				if (gameDataSummary.Difficulty == 1 && mFSGameDataFromPairData.GlideModeScores != null)
				{
					for (int i = mFSGameDataFromPairData.GlideModeScores.Count; i < gameDataSummary.GameLevel; i++)
					{
						mFSGameDataFromPairData.GlideModeScores.Add(0);
					}
					mFSGameDataFromPairData.GlideModeScores[gameDataSummary.GameLevel - 1] = gameDataSummary.GameDataList[0].Value;
				}
				else if (gameDataSummary.Difficulty == 3 && mFSGameDataFromPairData.FlightModeScores != null)
				{
					for (int j = mFSGameDataFromPairData.FlightModeScores.Count; j < gameDataSummary.GameLevel; j++)
					{
						mFSGameDataFromPairData.FlightModeScores.Add(0);
					}
					mFSGameDataFromPairData.FlightModeScores[gameDataSummary.GameLevel - 1] = gameDataSummary.GameDataList[0].Value;
				}
			}
		}
		mFSScorePairData.SetValue("FlightSchoolScoreData", UtUtilities.SerializeToXml(mFSGameDataFromPairData));
		mFSScorePairData.PrepareArray();
		WsWebService.SetKeyValuePairByUserID(SanctuaryManager.pCurPetData.EntityID.ToString(), _ObsCourseLevelPairDataID, mFSScorePairData, WsEventHandler, null);
	}

	private void UpdateGameData(GetGameDataResponse gameData, int score, int type)
	{
		int num = 1;
		if (mGameMode == FSGameMode.FLIGHT_MODE)
		{
			if (type == 0)
			{
				num = 3;
			}
			else if (_DragonSelectionUi.pSelectedTicketID > 0)
			{
				HeroPetData heroDragonFromID = SanctuaryData.GetHeroDragonFromID(_DragonSelectionUi.pSelectedTicketID);
				for (int i = 0; i < _HeroDifficulty.Length; i++)
				{
					if (_HeroDifficulty[i]._HeroPetName == heroDragonFromID._Name)
					{
						num = (int)_HeroDifficulty[i]._Difficulty;
						break;
					}
				}
			}
			else
			{
				num = 3;
			}
		}
		bool flag = false;
		foreach (GameDataSummary gameDataSummary in gameData.GameDataSummaryList)
		{
			if (gameDataSummary.Difficulty == num && gameDataSummary.GameLevel == mCurrentLevel + 1)
			{
				if (gameDataSummary.GameDataList[0].Value < score)
				{
					gameDataSummary.GameDataList[0].Value = score;
				}
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			gameData.GameDataSummaryList.Add(CreateGameDataSummary(num, mCurrentLevel, score));
		}
	}

	private void GetPairDataKeyByDragonName(string inDragonName, ref string inLevelPairDataKey, ref string inHeroLevelPairDataKey)
	{
		HeroPetDifficultyData[] heroDifficulty = _HeroDifficulty;
		foreach (HeroPetDifficultyData heroPetDifficultyData in heroDifficulty)
		{
			if (heroPetDifficultyData._HeroPetName == inDragonName)
			{
				inLevelPairDataKey = heroPetDifficultyData._LevelPairDataKey;
				inHeroLevelPairDataKey = heroPetDifficultyData._HeroLevelPairDataKey;
				break;
			}
		}
	}

	public void LevelStart()
	{
		mPlaying = true;
	}

	public void ShowGameModeUI()
	{
		if (_GameModeSelectionUi != null)
		{
			_GameModeSelectionUi.Init(this);
		}
		else
		{
			UtDebug.LogError("_GameModeSelectionUi is null");
		}
	}

	public void Update()
	{
		if (!mStatus.pIsReady)
		{
			mDoLoadData = false;
			mStatus.pIsReady = true;
		}
		if (mStoreItemsLoaded)
		{
			if (mStatus.pIsReady && mDoLoadData && !ScoreData.mDataReturnFail && mGradeDataReady && mLastLevelUnlockedData != null && mScorePairDataReady && !mScorePairDataFail)
			{
				KAUICursorManager.SetDefaultCursor("Arrow");
				switch (mGameMode)
				{
				case FSGameMode.FLIGHT_MODE:
					if (_DragonSelectionUi.pSelectedTicketID != 0)
					{
						_LevelSelectionUi.SetActive(value: true);
						break;
					}
					goto case FSGameMode.GLIDE_MODE;
				case FSGameMode.GLIDE_MODE:
				case FSGameMode.FLIGHT_SUIT_MODE:
					_DefaultLevelSelectionUi.SetActive(value: true);
					break;
				}
				mDoLoadData = false;
			}
			else if ((ScoreData.mDataReturnFail && mGradeDataReady) || (mScorePairDataFail && !mScorePairDataReady && mDoLoadData))
			{
				mDoLoadData = false;
				ShowWarningDB();
			}
		}
		if (mPlaying)
		{
			mNextCheckPtTimeleft -= Time.deltaTime;
			if (mNextCheckPtTimeleft < 0f)
			{
				Debug.Log("failed to reach check point");
			}
		}
		if (Application.isEditor && mLastLevelUnlockedData != null && Input.GetKeyUp(KeyCode.F9))
		{
			CheatToUnlockAllLevels();
		}
	}

	public void OnStoreLoaded(StoreData sd)
	{
		ItemData[] items = sd._Items;
		foreach (ItemData itemData in items)
		{
			if (itemData.ItemID == _LevelUnlockItemID)
			{
				mLevelUnLockCost = itemData.FinalCashCost;
				mStoreItemsLoaded = true;
			}
		}
	}

	public void OnHighScoresDone()
	{
		AvAvatar.pInputEnabled = true;
		SanctuaryManager.pMountedState = UiSelectGameModes.pIsMounted;
		RsResourceManager.LoadLevel(RsResourceManager.pCurrentLevel);
	}

	public void OnDestroy()
	{
		AvAvatar.pLevelState = AvAvatarLevelState.NORMAL;
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: true);
		}
	}

	public void ShowDragonsUI()
	{
		if (_DragonSelectionUi != null)
		{
			_DragonSelectionUi.Init(this);
		}
		mLevelReady = false;
	}

	public void ShowLevelSelectionUI()
	{
		bool flag = true;
		UiObstacleCourseMain uiObstacleCourseMain = null;
		if (mGameMode == FSGameMode.FLIGHT_SUIT_MODE)
		{
			_DragonSelectionUi.pSelectedTicketID = 0;
		}
		if (mGameMode == FSGameMode.GLIDE_MODE || mGameMode == FSGameMode.FLIGHT_SUIT_MODE)
		{
			if (_LevelSelectionUi != null)
			{
				uiObstacleCourseMain = _DefaultLevelSelectionUi.GetComponent<UiObstacleCourseMain>();
			}
		}
		else if (mGameMode == FSGameMode.FLIGHT_MODE)
		{
			if (_LevelSelectionUi != null)
			{
				uiObstacleCourseMain = _LevelSelectionUi.GetComponent<UiObstacleCourseMain>();
			}
			if (_DragonSelectionUi != null)
			{
				flag = _DragonSelectionUi.ShowPlayerFlightModeLevels();
			}
		}
		if (flag)
		{
			LoadData();
		}
		if (_GameModeSelectionUi != null && flag)
		{
			_GameModeSelectionUi.SetInteractive(interactive: true);
			_GameModeSelectionUi.SetVisibility(inVisible: false);
		}
		if (uiObstacleCourseMain != null)
		{
			uiObstacleCourseMain._LevelManager = this;
		}
	}

	private void ShowWarningDB()
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		mUiGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "WarningDB");
		mUiGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mUiGenericDB._MessageObject = base.gameObject;
		mUiGenericDB._OKMessage = "OnOkMsg";
		mUiGenericDB.SetTextByID(_LevelLoadFailedText._ID, _LevelLoadFailedText._Text, interactive: false);
		KAUI.SetExclusive(mUiGenericDB);
	}

	private void OnOkMsg()
	{
		KAUI.RemoveExclusive(mUiGenericDB);
		UnityEngine.Object.Destroy(mUiGenericDB.gameObject);
		mUiGenericDB = null;
		ScoreData.mCount = 0;
		ScoreData.mDataReturnFail = false;
		if (mGameMode == FSGameMode.GLIDE_MODE || mGameMode == FSGameMode.FLIGHT_SUIT_MODE)
		{
			mMenuState = FSMenuState.FS_STATE_MODESELECT;
		}
		else if (mGameMode == FSGameMode.FLIGHT_MODE)
		{
			mMenuState = FSMenuState.FS_STATE_DRAGONSELECT;
		}
		RsResourceManager.LoadLevel(RsResourceManager.pCurrentLevel);
	}

	public void ShowEndGameUI(ObstacleCourseGame inGameLevel, string grade, string bgColor, AchievementReward[] inRewards, bool isCourseComplete)
	{
		mCourseLevel = inGameLevel;
		if (_EndDBUI != null)
		{
			_EndDBUI.SetGameSettings(mCourseLevel._GameModuleName, base.gameObject, "any");
			ResetEndDBResultData();
			_EndDBUI.SetResultData("Data0", _ScoreTitleText.GetLocalizedString(), mCourseLevel.pScore.ToString());
			_EndDBUI.SetResultData("Data1", _TotalTimeTitleText.GetLocalizedString(), GameUtilities.FormatTime(mCourseLevel.pFlightTime));
			int num = 2;
			if (mCourseLevel.pNumRingsAvailable > 0)
			{
				string text = mCourseLevel.pNumRingsCollected.ToString();
				text = text + " / " + mCourseLevel.pNumRingsAvailable;
				_EndDBUI.SetResultData("Data2", _RingTitleText.GetLocalizedString(), text);
				num++;
			}
			if (mCourseLevel.pNumPickupsAvailable > 0)
			{
				string text2 = mCourseLevel.pNumPickupsCollected.ToString();
				text2 = text2 + "/" + mCourseLevel.pNumPickupsAvailable;
				_EndDBUI.SetResultData("Data" + num, _PickUpTitleText.GetLocalizedString(), text2);
				num++;
			}
			if (mCourseLevel.pNumTargetablesAvailable > 0)
			{
				string text3 = mCourseLevel.pNumTargetablesCollected.ToString();
				text3 = text3 + " / " + mCourseLevel.pNumTargetablesAvailable;
				_EndDBUI.SetResultData("Data" + num, _TargetablesTitleText.GetLocalizedString(), text3);
			}
			_EndDBUI.SetVisibility(Visibility: true);
			if (!string.IsNullOrEmpty(grade))
			{
				_EndDBUI.SetGrade(grade, bgColor);
			}
			_EndDBUI.SetTitle(isCourseComplete ? _CourseCompleteTitleText.GetLocalizedString() : _CourseFailedTitleText.GetLocalizedString());
			if (inRewards != null)
			{
				_EndDBUI.SetRewardDisplay(inRewards);
			}
			if (!(_FlightSchoolIntroTut == null) && !_FlightSchoolIntroTut.TutorialComplete() && _FlightSchoolIntroTut.CanShowExitTutorial())
			{
				_FlightSchoolIntroTut.SetTutorialBoardVisible(flag: true);
				_FlightSchoolIntroTut.StartNextTutorial();
			}
		}
		else
		{
			Debug.LogError("NO END RESULT COMPONENT FOUND!!!");
		}
	}

	private void ResetEndDBResultData()
	{
		for (int i = 0; i < 4; i++)
		{
			_EndDBUI.SetResultData("Data" + i, "", "");
		}
	}

	private void OnEndDBClose()
	{
		OnHighScoresDone();
	}

	private void OnReplayGame()
	{
		if (_FlightSchoolIntroTut != null)
		{
			_FlightSchoolIntroTut.SetTutorialBoardVisible(flag: false);
		}
		if (!(mCourseLevel == null))
		{
			_EndDBUI.SetVisibility(Visibility: false);
			mCourseLevel.RestartLevel(bFromLevelSelection: false);
		}
	}

	private void OnStoreClosed()
	{
		if (mCourseLevel != null)
		{
			mCourseLevel.MountDragon();
		}
	}

	public void OnConsumableUpdated(Consumable inConsumable)
	{
		if (!(_PowerUpManager == null))
		{
			PowerUp powerUp = _PowerUpManager.InitPowerUp(inConsumable.name, isMMO: false);
			if (powerUp != null)
			{
				powerUp.Activate();
			}
			else
			{
				UtDebug.LogError("Power Up is null");
			}
		}
	}

	public void ApplyExtraTime(bool extraTime, float duration)
	{
		pCourseLevel.ExtraTime(extraTime, duration);
	}

	public void PreLoadStore()
	{
		if (_DragonSelectionUi != null)
		{
			_DragonSelectionUi.LoadTutStore();
		}
	}
}
