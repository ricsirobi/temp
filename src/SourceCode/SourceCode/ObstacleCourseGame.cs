using System;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleCourseGame : MonoBehaviour
{
	[Serializable]
	public class TimeBonus
	{
		public float _TimeLeft;

		public int _Point;
	}

	[Serializable]
	public class HeroDragonRacingDragonMap
	{
		public int _HeroDragonItemID;

		public GameObject _RacingObject;

		public GameObject _TutorialUI;
	}

	[Serializable]
	public class TutorialData
	{
		public InteractiveTutStep[] _Steps;

		public GameObject _Camera;
	}

	public enum PowerMeterFullAction
	{
		NONE,
		POWERSHOT_AFTER_METERFULL,
		CUT_ENABLE_AFTER_METERFULL
	}

	public float _TimeLimitSeconds = 90f;

	private float mHandicapTime;

	public int _CollectableObjectTarget = 1;

	public float _TimeUntilAlertSound = 70f;

	public int _RingObjectCount;

	public SnSound _TimeAlertSound;

	public GameObject _StartMarker;

	public GameObject _FlightSchoolUI;

	public GameObject _Breakables;

	public float _TimberjackOffset = 5f;

	public int _HighScoreGameID = 14;

	public string _GameModuleName = "DOFlightSchool";

	public int _DefaultScoreDelta;

	public GameObject _TutorialUI;

	public GameObject _GenericTutorialUI;

	public TutorialData[] _TutorialData;

	public GameObject _LevelEndMarker;

	public GameObject _RacingObject;

	public HeroDragonRacingDragonMap[] _HeroDragonRacingDragonMap;

	public GameObject _PathSpline;

	public AudioClip _MusicBackground;

	public InteractiveTutManager _InteractiveTutUI;

	public bool _TimeTrial;

	public Transform _NextMarker;

	public PowerMeterFullAction _PowerMeterFullAction;

	public bool _IsTreesEnabledForIOS = true;

	private float mGameStartTime;

	private float mTimeLeft;

	private SnChannel mTimeAlertSnChannel;

	private UiFlightSchoolHUD mKAUIHud;

	private float mFlightTime;

	private GameObject mEndMarker;

	private bool mbGameRunning;

	private bool mbOnCliff;

	private GameObject mInitialAvatarToolbar;

	private int mNumPickupsAvailable;

	private int mNumRingsAvailable;

	private int mNumTargetablesAvailable;

	private int mNumPickupsCollected;

	private int mNumRingsCollected;

	private int mNumTargetablesCollected;

	private int mCollectedObjectsScore;

	private AchievementReward[] mAchievementRewards;

	private ObstacleCourseLevelManager mLevelManager;

	private float mPauseStartTime;

	private GameObject mCurrentRacingObject;

	private GameObject mCurrentTutorialUI;

	private InteractiveTutManager mCurrentGenericTutorialUI;

	private int mCurrentTutorialIndex;

	private int mTotalScore;

	protected int mGameID;

	protected string mGameName;

	private float mNextCheckPtTimeleft = 10f;

	private int mNextCheckPoint = 1;

	private float mTotalTimeTaken = float.PositiveInfinity;

	private bool mStartGame;

	private bool mQuitGame;

	private bool mGameIsPaused;

	private float mExtraTime;

	private bool mIsExtraTime;

	private float mTime;

	private Transform mAvatarTransform;

	public float _DistanceThreshold = 200f;

	public float _VerticalThreshold = 60f;

	public LocaleString _ThresholdCrossedText = new LocaleString("You are going away from the race track. You will reset in {{NumSeconds}} seconds.");

	public float _WrongWayTimer = 8f;

	public float _WrongWayDelay = 3f;

	public GameObject _WaypointHighLightObj;

	public string _SpecialAttackAnim = "";

	public GameObject _SpecialAttackWeaponPf;

	public GameObject _SpecialAttackWeaponHitPf;

	private GameObject mCurrWaypointHighlight;

	private int mCurrentPathID;

	private const int mResumeNode = 0;

	private int mCurrentWaypointIdx;

	private int mSplineCoveredCnt;

	private float mResetTimer;

	private bool mIsAway;

	private bool mGoingWrongWay;

	protected int mLapNodeCount;

	protected int mNumWayPointsCovered;

	protected int mLastWayPointIndex;

	private float mDistanceForward;

	private bool mDragonMountInitForTutorial;

	private bool mEnableCheatToEndGame;

	private bool mIsDragonMeterReady;

	private string mPrevDragonAnimHeroLevel = "";

	private List<Transform> mPrevShootingPointHeroLevel;

	private GameObject mPrevDragonWeaponPf;

	private GameObject mPrevDragonWeaponHitPf;

	private AvAvatarController mAvatarController;

	public float pTimeLimit
	{
		get
		{
			if (!(_TimeLimitSeconds < 0f))
			{
				return _TimeLimitSeconds + mHandicapTime;
			}
			return -1f;
		}
	}

	public float pFlightTime => mFlightTime;

	public int pNumPickupsAvailable => mNumPickupsAvailable;

	public int pNumRingsAvailable => mNumRingsAvailable;

	public int pNumTargetablesAvailable => mNumTargetablesAvailable;

	public int pNumCollectablesAvailable => mNumPickupsAvailable + mNumRingsAvailable + mNumTargetablesAvailable;

	public int pNumPickupsCollected => mNumPickupsCollected;

	public int pNumRingsCollected => mNumRingsCollected;

	public int pNumTargetablesCollected => mNumTargetablesCollected;

	private int pNumCollectablesCollected => mNumPickupsCollected + mNumRingsCollected + mNumTargetablesCollected;

	public int pScore
	{
		get
		{
			return mTotalScore;
		}
		set
		{
			mTotalScore = value;
		}
	}

	public int pGameID => mGameID;

	public string pGameName => mGameName;

	public GameObject pCurrentTutorialUI
	{
		get
		{
			return mCurrentTutorialUI;
		}
		set
		{
			mCurrentTutorialUI = value;
		}
	}

	public int pCurrentWaypointIdx
	{
		get
		{
			return mCurrentWaypointIdx;
		}
		set
		{
			mCurrentWaypointIdx = value;
		}
	}

	public int pNextWaypointIdx
	{
		get
		{
			int num = mCurrentWaypointIdx + 1;
			if (num == GetNodeCount())
			{
				num = ((pCurrentPathID != 0) ? mCurrentWaypointIdx : 0);
			}
			return num;
		}
	}

	public int pPrevWaypointIdx
	{
		get
		{
			int num = mCurrentWaypointIdx - 1;
			if (num < 0)
			{
				num = GetNodeCount() - 1;
			}
			return num;
		}
	}

	public int pCurrentPathID
	{
		get
		{
			return mCurrentPathID;
		}
		set
		{
			mCurrentPathID = value;
		}
	}

	public virtual void Awake()
	{
		mGameID = _HighScoreGameID;
		mGameName = _GameModuleName;
		if (_StartMarker == null)
		{
			_StartMarker = GameObject.Find("PfMarker_AvatarStart01");
		}
		mEndMarker = GameObject.Find("PfMarker_FlightReturn");
		if (null != _PathSpline)
		{
			PathManager.pInstance.PushNodeInList(_PathSpline);
			PathManager.pInitialized = true;
			ShortcutPath.GenerateShortCutNodeData();
		}
		ObCollectFS[] componentsInChildren = GetComponentsInChildren<ObCollectFS>();
		ObCollectFS[] array = Array.FindAll(componentsInChildren, (ObCollectFS element) => element._Type == ObCollectFS.Type.PICKUP);
		mNumPickupsAvailable = array.Length;
		mNumRingsAvailable = componentsInChildren.Length - mNumPickupsAvailable;
		ObTargetable[] componentsInChildren2 = GetComponentsInChildren<ObTargetable>();
		mNumTargetablesAvailable = componentsInChildren2.Length;
		mNumPickupsCollected = 0;
		mNumRingsCollected = 0;
		mNumTargetablesCollected = 0;
		mCollectedObjectsScore = 0;
		if (UiAvatarControls.pInstance != null)
		{
			UiAvatarControls.pInstance.pEnableFireOnButtonUp = _PowerMeterFullAction != PowerMeterFullAction.NONE;
			UiAvatarControls.pInstance.pEnableFireOnButtonDown = !UiAvatarControls.pInstance.pEnableFireOnButtonUp;
		}
		if (UtPlatform.IsiOS() && !_IsTreesEnabledForIOS)
		{
			Terrain componentInChildren = GetComponentInChildren<Terrain>();
			if (componentInChildren != null)
			{
				componentInChildren.terrainData.treeInstances = new List<TreeInstance>().ToArray();
			}
		}
	}

	public void InitGame()
	{
		GameObject gameObject = GameObject.Find("ObstacleCourse");
		mLevelManager = gameObject.GetComponent<ObstacleCourseLevelManager>();
		mLevelManager.pCourseLevel = this;
		if (mLevelManager._LevelSelectionUi != null)
		{
			mLevelManager._LevelSelectionUi.SetActive(value: false);
		}
		if (mLevelManager._DefaultLevelSelectionUi != null)
		{
			mLevelManager._DefaultLevelSelectionUi.SetActive(value: false);
		}
		if (_RacingObject != null)
		{
			_RacingObject.SetActive(value: false);
		}
		mCurrentRacingObject = _RacingObject;
		mCurrentTutorialUI = _TutorialUI;
		if (_HeroDragonRacingDragonMap != null && _HeroDragonRacingDragonMap.Length != 0)
		{
			HeroDragonRacingDragonMap[] heroDragonRacingDragonMap = _HeroDragonRacingDragonMap;
			foreach (HeroDragonRacingDragonMap heroDragonRacingDragonMap2 in heroDragonRacingDragonMap)
			{
				if (heroDragonRacingDragonMap2._RacingObject != null)
				{
					heroDragonRacingDragonMap2._RacingObject.SetActive(value: false);
				}
				if (mLevelManager._DragonSelectionUi != null && mLevelManager._DragonSelectionUi.pSelectedTicketID > 0 && mLevelManager._DragonSelectionUi.pSelectedTicketID == heroDragonRacingDragonMap2._HeroDragonItemID)
				{
					mCurrentRacingObject = ((heroDragonRacingDragonMap2._RacingObject != null) ? heroDragonRacingDragonMap2._RacingObject : _RacingObject);
					mCurrentTutorialUI = ((heroDragonRacingDragonMap2._TutorialUI != null) ? heroDragonRacingDragonMap2._TutorialUI : _TutorialUI);
				}
			}
		}
		if (mCurrentRacingObject != null)
		{
			mCurrentRacingObject.GetComponent<SplineControl>().SetPosOnSpline(0f);
		}
		mNumPickupsCollected = 0;
		mNumRingsCollected = 0;
		mNumTargetablesCollected = 0;
		mCollectedObjectsScore = 0;
		if (mLevelManager.pGameMode != FSGameMode.FLIGHT_SUIT_MODE)
		{
			KAInput.pInstance.EnableInputType("Jump", InputType.ALL, inEnable: false);
		}
		if (null != _GenericTutorialUI)
		{
			ShowGenericTutorial();
		}
		else
		{
			ShowLevelTutorial();
		}
		if (!mAvatarController)
		{
			mAvatarController = AvAvatar.pObject.GetComponent<AvAvatarController>();
		}
		if ((bool)mAvatarController)
		{
			if (mLevelManager.pGameMode != FSGameMode.FLIGHT_SUIT_MODE)
			{
				mAvatarController.ShowSoarButton(inShow: false);
				mAvatarController.DisableSoarButtonUpdate = true;
			}
			mAvatarController.pFlyingRoll = 0f;
			mAvatarController.pFlyingPitch = 0f;
		}
		if (mLevelManager.mIsCurrentLevelSpecial)
		{
			UpdateHeroLevelDragonData(enableHeroLevelDragonData: true);
		}
	}

	private void ShowLevelTutorial()
	{
		if (null != mCurrentTutorialUI)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(mCurrentTutorialUI, Vector3.zero, Quaternion.identity);
			if (null != gameObject)
			{
				KAUIFlightSchoolTutScreen component = gameObject.GetComponent<KAUIFlightSchoolTutScreen>();
				if (null != component)
				{
					component._Game = this;
					MountDragon();
				}
			}
		}
		else
		{
			SetupGame();
		}
	}

	private void ShowGenericTutorial()
	{
		string text = "Id" + mCurrentTutorialIndex + "_" + base.gameObject.name + "_" + mLevelManager._DragonSelectionUi.pSelectedTicketID;
		if (mCurrentTutorialIndex >= _TutorialData.Length || ProductData.TutorialComplete(text))
		{
			ShowLevelTutorial();
			return;
		}
		if (_TutorialData[mCurrentTutorialIndex]._Steps.Length == 0)
		{
			SwitchToTutorialCamera(mCurrentTutorialIndex);
			mCurrentTutorialIndex++;
			ShowGenericTutorial();
			return;
		}
		_GenericTutorialUI.GetComponent<InteractiveTutManager>()._TutIndexKeyName = text;
		GameObject gameObject = UnityEngine.Object.Instantiate(_GenericTutorialUI, Vector3.zero, Quaternion.identity);
		if (!(null != gameObject))
		{
			return;
		}
		mCurrentGenericTutorialUI = gameObject.GetComponent<InteractiveTutManager>();
		if (null != mCurrentGenericTutorialUI)
		{
			mCurrentGenericTutorialUI._TutSteps = _TutorialData[mCurrentTutorialIndex]._Steps;
			mCurrentGenericTutorialUI.InitTutorial(isFilterSteps: true);
			mCurrentGenericTutorialUI.ShowTutorial();
			InteractiveTutManager interactiveTutManager = mCurrentGenericTutorialUI;
			interactiveTutManager._StepEndedEvent = (StepEndedEvent)Delegate.Combine(interactiveTutManager._StepEndedEvent, new StepEndedEvent(OnTutorialStepEndEvent));
			if (mLevelManager.pGameMode != FSGameMode.FLIGHT_SUIT_MODE || mCurrentTutorialIndex == 0)
			{
				MountDragon();
			}
			else
			{
				AvAvatar.pObject.SetActive(value: true);
			}
			mCurrentTutorialIndex++;
		}
	}

	private void OnTutorialStepEndEvent(int stepIdx, string stepName, bool tutQuit)
	{
		if (mCurrentGenericTutorialUI != null && stepIdx >= mCurrentGenericTutorialUI._TutSteps.Length - 1)
		{
			SwitchToTutorialCamera(mCurrentTutorialIndex - 1);
			UnityEngine.Object.Destroy(mCurrentGenericTutorialUI.gameObject);
			ShowGenericTutorial();
		}
	}

	private void SwitchToTutorialCamera(int inIndex)
	{
		for (int i = 0; i < _TutorialData.Length; i++)
		{
			bool active = inIndex == i && _TutorialData[inIndex]._Camera != null;
			if (_TutorialData[i]._Camera != null)
			{
				_TutorialData[i]._Camera.SetActive(active);
			}
		}
	}

	public void MountDragon()
	{
		AvAvatar.pObject.SetActive(value: true);
		bool num = mLevelManager.pGameMode == FSGameMode.FLIGHT_SUIT_MODE;
		if (_StartMarker != null)
		{
			AvAvatar.TeleportToObject(_StartMarker);
			AdjustPosition();
		}
		if (num)
		{
			return;
		}
		SanctuaryPet.AddMountEvent(SanctuaryManager.pCurPetInstance, DragonMounted);
		SanctuaryManager.pCurPetInstance.OnFlyDismountImmediate(AvAvatar.pObject);
		SanctuaryManager.pCurPetInstance.Mount(AvAvatar.pObject, PetSpecialSkillType.FLY);
		if (!mAvatarController)
		{
			mAvatarController = AvAvatar.pObject.GetComponent<AvAvatarController>();
		}
		if ((bool)mAvatarController)
		{
			if (mLevelManager.pGameMode == FSGameMode.FLIGHT_MODE)
			{
				mAvatarController.pFlyingData = FlightData.GetFlightData(SanctuaryManager.pCurPetInstance.gameObject, FlightDataType.RACING);
			}
			else
			{
				mAvatarController.pFlyingData = FlightData.GetFlightData(SanctuaryManager.pCurPetInstance.gameObject, FlightDataType.GLIDING);
			}
		}
		if ((UtPlatform.IsMobile() || UtPlatform.IsWSA()) && UiJoystick.pInstance != null)
		{
			UiJoystick.pInstance.SetVisibility(isVisible: false);
		}
		mDragonMountInitForTutorial = true;
	}

	public void SetupGame()
	{
		mbGameRunning = false;
		mNextCheckPtTimeleft = 10f;
		mNextCheckPoint = 1;
		if (mKAUIHud == null)
		{
			mKAUIHud = _FlightSchoolUI.GetComponent<UiFlightSchoolHUD>();
		}
		if (mKAUIHud == null)
		{
			Debug.LogError("Unable to find UiFlightSchoolHUD");
		}
		else
		{
			AssignHUDToToolbar(isAssign: true);
			EnableHUD(isEnable: false);
		}
		StopTimer();
		mAvatarTransform = AvAvatar.mTransform;
		AvAvatar.pObject.SetActive(value: true);
		if (_StartMarker != null)
		{
			AvAvatar.pSubState = AvAvatarSubState.NORMAL;
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.TeleportToObject(_StartMarker);
			AdjustPosition();
		}
		if (mLevelManager.pGameMode != FSGameMode.FLIGHT_SUIT_MODE)
		{
			SanctuaryPet.AddMountEvent(SanctuaryManager.pCurPetInstance, DragonMounted);
			SanctuaryManager.pCurPetInstance.Mount(AvAvatar.pObject, PetSpecialSkillType.FLY);
			mHandicapTime = SanctuaryManager.pCurPetInstance._FlightClubHandicap;
		}
		if (!mAvatarController)
		{
			mAvatarController = AvAvatar.pObject.GetComponent<AvAvatarController>();
		}
		if ((bool)mAvatarController)
		{
			if (mLevelManager.pGameMode == FSGameMode.FLIGHT_MODE)
			{
				mAvatarController.SetFlyingState(FlyingState.Hover);
			}
			else if (mLevelManager.pGameMode == FSGameMode.GLIDE_MODE)
			{
				mAvatarController.SetFlyingState(FlyingState.TakeOffGliding);
			}
			else if (mLevelManager.pGameMode == FSGameMode.FLIGHT_SUIT_MODE)
			{
				mAvatarController.OnGlideLanding();
				mAvatarController.PlayingFlightSchoolFlightSuit = true;
			}
		}
		if ((UtPlatform.IsMobile() || UtPlatform.IsWSA()) && UiJoystick.pInstance != null)
		{
			UiJoystick.pInstance.SetVisibility(isVisible: false);
		}
		mLevelManager._UiCountdown.StartCountDown(inStart: true);
		if (!ProductData.pPairData.GetBoolValue(AnalyticEvent.PLAY_FLIGHTSCHOOL.ToString(), defaultVal: false))
		{
			AnalyticAgent.LogEvent("AppsFlyer", AnalyticEvent.PLAY_FLIGHTSCHOOL, new Dictionary<string, string>());
			ProductData.pPairData.SetValueAndSave(AnalyticEvent.PLAY_FLIGHTSCHOOL.ToString(), true.ToString());
		}
	}

	public void DragonMounted(bool mount, PetSpecialSkillType skill)
	{
		if (mDragonMountInitForTutorial)
		{
			EnableHUD(isEnable: false);
			AvAvatar.pState = AvAvatarState.PAUSED;
			if (SanctuaryManager.pCurPetInstance != null)
			{
				SanctuaryManager.pCurPetInstance.pMeterPaused = true;
			}
			mDragonMountInitForTutorial = false;
		}
	}

	public void StartGame()
	{
		TimeHackPrevent.Set(AvAvatarLevelState.FLIGHTSCHOOL.ToString());
		if (mLevelManager.pGameMode != FSGameMode.FLIGHT_SUIT_MODE)
		{
			mLevelManager._UiConsumable.SetVisibility(inVisible: true);
		}
		mLevelManager._UiConsumable.ResetButtonStates();
		if (mLevelManager._DragonSelectionUi.pSelectedTicketID > 0 || mLevelManager.pGameMode != FSGameMode.FLIGHT_MODE)
		{
			mLevelManager._UiConsumable.AddDisabledConsumables(new List<string> { "Energy Boost" }, new List<string>());
		}
		else
		{
			mLevelManager._UiConsumable.AddDisabledConsumables(new List<string>(), new List<string>());
		}
		if (MissionManager.pInstance != null && mCurrentTutorialUI == null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: true, inForceUpdate: true);
		}
		mbGameRunning = true;
		mbOnCliff = false;
		mGameStartTime = Time.time;
		if (mLevelManager.pGameMode == FSGameMode.FLIGHT_SUIT_MODE)
		{
			KAInput.pInstance.EnableInputType("Jump", InputType.ALL, inEnable: true);
		}
		ResetScore();
		EnableHUD(isEnable: true);
		StartTimer();
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pCurPetInstance.pMeterPaused = false;
		}
		if (null == _PathSpline)
		{
			BroadcastMessage("ResetSplinePosition", SendMessageOptions.DontRequireReceiver);
		}
		if (pTimeLimit >= 0f)
		{
			Invoke("StartAlertSound", _TimeUntilAlertSound);
			Invoke("EndGame", pTimeLimit);
		}
		if (_StartMarker != null)
		{
			AvAvatar.SetPosition(_StartMarker.transform);
			AdjustPosition();
			ObAvatarRespawn._Marker = _StartMarker;
		}
		if (_LevelEndMarker != null)
		{
			_LevelEndMarker.GetComponent<LevelEndTrigger>().SetMessageObject(base.gameObject);
		}
		SnChannel.StopPool("Music_Pool");
		SnChannel.StopPool("SFX_Pool");
		SnChannel.StopPool("VO_Pool");
		SnChannel.StopPool("DEFAULT_POOL");
		AudioClip audioClip = ((_MusicBackground != null) ? _MusicBackground : mLevelManager._MusDragFlightSchool);
		if (audioClip != null)
		{
			SnChannel.Play(audioClip, "Music_Pool", inForce: true, null);
		}
		if (mCurrentRacingObject != null)
		{
			mCurrentRacingObject.SetActive(value: true);
		}
		AvAvatar.pState = AvAvatarState.IDLE;
		if (!mAvatarController)
		{
			mAvatarController = AvAvatar.pObject.GetComponent<AvAvatarController>();
		}
		if ((bool)mAvatarController)
		{
			mAvatarController.UpdateGliding();
		}
		if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pIsMounted && UiAvatarControls.pInstance != null)
		{
			UiAvatarControls.pInstance.EnableDragonFireButton(inEnable: true);
			UiAvatarControls.pInstance.DisableAllDragonControls(inDisable: false);
			UiAvatarControls.EnableTiltControls(enable: true, recalibrate: false);
		}
	}

	private void AssignHUDToToolbar(bool isAssign)
	{
		GameObject pToolbar = (isAssign ? mKAUIHud.gameObject : mInitialAvatarToolbar);
		if (isAssign)
		{
			mInitialAvatarToolbar = AvAvatar.pToolbar;
		}
		AvAvatar.pToolbar = pToolbar;
		mKAUIHud.pGame = (isAssign ? this : null);
	}

	public void ResetScore()
	{
		mTotalScore = 0;
	}

	public void OnObjectCollected(GameObject go)
	{
		ObCollectFS component = go.GetComponent<ObCollectFS>();
		ObTargetable obTargetable = null;
		if (component != null)
		{
			if (component._Type == ObCollectFS.Type.PICKUP)
			{
				mNumPickupsCollected++;
			}
			else if (component._Type == ObCollectFS.Type.RING)
			{
				mNumRingsCollected++;
			}
		}
		else
		{
			obTargetable = go.GetComponent<ObTargetable>();
			if (obTargetable != null)
			{
				mNumTargetablesCollected++;
			}
		}
		FreeFallObjScore freeFallObjScore = (FreeFallObjScore)go.GetComponent(typeof(FreeFallObjScore));
		if (freeFallObjScore != null)
		{
			mCollectedObjectsScore += freeFallObjScore._Score;
		}
		else if (obTargetable != null)
		{
			mCollectedObjectsScore += obTargetable._ScoreOnHit;
		}
		UpdateHUDUIData();
		if (_CollectableObjectTarget > 0 && pNumCollectablesCollected >= _CollectableObjectTarget)
		{
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.CheckForTaskCompletion("Game", mGameName);
			}
			EndGame(bForceEnd: false);
		}
	}

	public void OnObjectiveCompleted()
	{
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.CheckForTaskCompletion("Game", mGameName);
		}
		if (mLevelManager != null)
		{
			mLevelManager.SetLevelCompleted(bCompleted: true);
		}
	}

	public void EndGame()
	{
		EndGame(bForceEnd: false);
	}

	public void EndGame(bool bForceEnd)
	{
		TimeHackPrevent.Reset();
		mLevelManager._UiConsumable.SetVisibility(inVisible: false);
		mLevelManager._PowerUpManager.DeactivateAllPowerUps();
		CancelInvoke();
		float num = ((pTimeLimit >= 0f) ? (pTimeLimit - (Time.time - mGameStartTime)) : 0f);
		if (pNumCollectablesAvailable == 0)
		{
			mTotalScore = mLevelManager._PointsForNoCollectItems + (int)num * 10;
		}
		else if (pNumCollectablesAvailable > 0)
		{
			mTotalScore = (int)(num * 10f) + mCollectedObjectsScore;
		}
		if (mEnableCheatToEndGame)
		{
			mTotalScore = 10000;
			mEnableCheatToEndGame = false;
		}
		SanctuaryPet.RemoveMountEvent(SanctuaryManager.pCurPetInstance, DragonMounted);
		if (UiAvatarControls.pInstance != null)
		{
			UiAvatarControls.pInstance.EnableDragonFireButton(inEnable: false);
		}
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: false);
		}
		mbGameRunning = false;
		mbOnCliff = false;
		StopAlertSound();
		StopTimer();
		EnableHUD(isEnable: false);
		AssignHUDToToolbar(isAssign: false);
		DisableLevelGroup();
		ResetWrongWayData();
		ProcessEndUI(_TimeTrial, bForceEnd);
		if ((UtPlatform.IsMobile() || UtPlatform.IsWSA()) && UiJoystick.pInstance != null)
		{
			UiJoystick.pInstance.SetVisibility(isVisible: false);
		}
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pCurPetInstance.OnFlyDismountImmediate(AvAvatar.pObject);
			if (mLevelManager != null && mLevelManager._DragonSelectionUi != null && mLevelManager._DragonSelectionUi.pSelectedTicketID <= 0)
			{
				SanctuaryManager.pCurPetInstance.UpdateActionMeters(PetActions.FLIGHTSCHOOL, 1f, doUpdateSkill: true);
			}
		}
		TeleportPlayerToStartMarker();
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pCurPetInstance.SetAvatar(AvAvatar.mTransform);
			SanctuaryManager.pCurPetInstance.MoveToAvatar();
			SanctuaryManager.pCurPetInstance.SetFollowAvatar(follow: false);
		}
		AvAvatar.pState = AvAvatarState.PAUSED;
		if (mEndMarker != null)
		{
			AvAvatar.TeleportToObject(mEndMarker);
			AdjustPosition();
		}
		if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance._BreathAttackBall != null)
		{
			SanctuaryManager.pCurPetInstance.BreathAttack(t: false);
			SanctuaryManager.pCurPetInstance.SetBreathAttackBall(null);
		}
		if (mLevelManager.mIsCurrentLevelSpecial)
		{
			UpdateHeroLevelDragonData(enableHeroLevelDragonData: false);
		}
		mQuitGame = true;
	}

	private void ResetWrongWayData()
	{
		ShowWrongWayMessage(string.Empty, isShow: true);
		mCurrentWaypointIdx = 0;
		mLapNodeCount = 0;
		mNumWayPointsCovered = 0;
		mLastWayPointIndex = 0;
		mResetTimer = 0f;
		mIsAway = false;
		mGoingWrongWay = false;
		PathManager.Destroy();
	}

	private void ProcessEndUI(bool isTimeTrial, bool bForceEnd)
	{
		TeleportPlayerToStartMarker();
		AvAvatar.pObject.SetActive(value: false);
		AvAvatar.EnableAllInputs(inActive: false);
		float num = mTotalTimeTaken;
		if (isTimeTrial)
		{
			num = ((!(num > 0f)) ? 0f : ((float)((num != float.PositiveInfinity) ? ((int)(num * 100f)) : 0)));
		}
		if (!mLevelManager.pLevelCompleted && !bForceEnd && (pNumCollectablesAvailable <= 0 || pNumCollectablesCollected < pNumCollectablesAvailable))
		{
			if (mLevelManager != null)
			{
				LaunchHighScoresMenu(isTimeTrial, num);
				mAchievementRewards = new AchievementReward[2];
				mAchievementRewards[0] = new AchievementReward();
				mAchievementRewards[0].Amount = 0;
				mAchievementRewards[0].PointTypeID = 8;
				mAchievementRewards[1] = new AchievementReward();
				mAchievementRewards[1].Amount = 0;
				mAchievementRewards[1].PointTypeID = 2;
				ShowReward(showGrades: false);
				if (UtPlatform.IsMobile())
				{
					AdManager.DisplayAd(AdEventType.LEVEL_FAILED, AdOption.FULL_SCREEN);
				}
			}
			return;
		}
		if (UtPlatform.IsMobile())
		{
			AdManager.DisplayAd(AdEventType.LEVEL_COMPLETE, AdOption.FULL_SCREEN);
		}
		LaunchHighScoresMenu(isTimeTrial, num);
		string text = _GameModuleName;
		if (SubscriptionInfo.pIsMember)
		{
			text += "Member";
		}
		int gradeIndex = GetGradeIndex(mTotalScore);
		if (mLevelManager != null)
		{
			AchievementTask achievementTask = new AchievementTask(mLevelManager._LevelCompleteAchievementTaskID, GetCurrentRelatedAchievementID());
			AchievementTask groupAchievement = UserProfile.pProfileData.GetGroupAchievement(mLevelManager._LevelCompleteClanAchievementTaskID, GetCurrentRelatedAchievementID());
			AchievementTask achievementTask2 = null;
			AchievementTask achievementTask3 = null;
			if (gradeIndex == 0)
			{
				achievementTask2 = new AchievementTask(mLevelManager._APlusAchievementTaskID, GetCurrentRelatedAchievementID());
				achievementTask3 = UserProfile.pProfileData.GetGroupAchievement(mLevelManager._APlusClanAchievementTaskID, GetCurrentRelatedAchievementID());
			}
			UserAchievementTask.Set(achievementTask, groupAchievement, achievementTask2, achievementTask3);
		}
		List<AchievementTask> list = new List<AchievementTask>();
		string relatedID = "";
		string grade = null;
		int pCurrentLevel = mLevelManager.pCurrentLevel;
		GradeSystem[] array = null;
		if (mLevelManager.pGameMode == FSGameMode.FLIGHT_MODE)
		{
			array = mLevelManager._AdultData[mLevelManager.pLevelMenu.mCurrentSceneId]._GroupData[pCurrentLevel]._FSGrades;
			relatedID = mLevelManager._AdultData[mLevelManager.pLevelMenu.mCurrentSceneId]._GroupData[pCurrentLevel]._GroupName;
		}
		else if (mLevelManager.pGameMode == FSGameMode.GLIDE_MODE)
		{
			array = mLevelManager._TeenData[mLevelManager.pLevelMenu.mCurrentSceneId]._GroupData[pCurrentLevel]._FSGrades;
			relatedID = mLevelManager._TeenData[mLevelManager.pLevelMenu.mCurrentSceneId]._GroupData[pCurrentLevel]._GroupName;
		}
		else if (mLevelManager.pGameMode == FSGameMode.FLIGHT_SUIT_MODE)
		{
			array = mLevelManager._FlightSuitData[mLevelManager.pLevelMenu.mCurrentSceneId]._GroupData[pCurrentLevel]._FSGrades;
			relatedID = mLevelManager._FlightSuitData[mLevelManager.pLevelMenu.mCurrentSceneId]._GroupData[pCurrentLevel]._GroupName;
		}
		int gradeIndex2 = GetGradeIndex(mLevelManager.mCurrentLevelHighScore);
		if (gradeIndex < gradeIndex2 || gradeIndex == 0)
		{
			if (mLevelManager != null && mLevelManager._DragonSelectionUi != null)
			{
				if (mLevelManager._DragonSelectionUi.pSelectedTicketID > 0)
				{
					HeroPetData heroDragonFromID = SanctuaryData.GetHeroDragonFromID(mLevelManager._DragonSelectionUi.pSelectedTicketID);
					if (heroDragonFromID != null)
					{
						for (int i = 0; i < mLevelManager._HeroDifficulty.Length; i++)
						{
							if (!(heroDragonFromID._Name == mLevelManager._HeroDifficulty[i]._HeroPetName))
							{
								continue;
							}
							if (gradeIndex < gradeIndex2 && !string.IsNullOrEmpty(mLevelManager._HeroDifficulty[i]._GameModuleName))
							{
								text = mLevelManager._HeroDifficulty[i]._GameModuleName;
								if (SubscriptionInfo.pIsMember)
								{
									text += "Member";
								}
							}
							if (gradeIndex == 0)
							{
								AchievementTask achievementTask4 = new AchievementTask(mLevelManager._HeroDifficulty[i]._AchievementTaskID, mLevelManager.pCurrentLevel.ToString());
								UserAchievementTask.Set(achievementTask4, UserProfile.pProfileData.GetGroupAchievement(mLevelManager._HeroDifficulty[i]._ClanAchievementTaskID, mLevelManager.pCurrentLevel.ToString()));
							}
							break;
						}
						foreach (AchievementTaskData flightSchoolAchievement in mLevelManager._FlightSchoolAchievements)
						{
							if (heroDragonFromID._Name.Equals(flightSchoolAchievement._Value))
							{
								list.Add(new AchievementTask(flightSchoolAchievement._ID, relatedID));
							}
						}
						list.Add(new AchievementTask(mLevelManager._HeroFightClubExpertAchievement._ID, relatedID));
					}
					list.Add(new AchievementTask(mLevelManager._FightClubEnthusiastAchievement._ID));
				}
				if (gradeIndex < gradeIndex2)
				{
					int percentageScore = GetPercentageScore();
					mLevelManager._EndDBUI.SetAdRewardData(text, percentageScore);
					WsWebService.ApplyPayout(text, percentageScore, ServiceEventHandler, null);
				}
				else
				{
					ShowReward();
				}
			}
			else
			{
				ShowReward();
			}
		}
		else
		{
			ShowReward();
		}
		foreach (AchievementTaskData flightSchoolAchievement2 in mLevelManager._FlightSchoolAchievements)
		{
			if (mLevelManager.pGameMode.ToString().Equals(flightSchoolAchievement2._Value))
			{
				list.Add(new AchievementTask(flightSchoolAchievement2._ID, relatedID));
			}
		}
		UserAchievementTask.Set(list.ToArray());
		for (int j = 0; j < array.Length; j++)
		{
			if (mTotalScore >= array[j]._PointNeeded)
			{
				grade = array[j]._Grade;
				break;
			}
			grade = array[^1]._Grade;
		}
		if (mLevelManager != null)
		{
			mLevelManager.LevelEnd(isTimeTrial ? 1 : mTotalScore, grade, GetLevelType());
		}
	}

	private HighScoresDifficulty GetDifficulty()
	{
		if (mLevelManager == null)
		{
			return HighScoresDifficulty.UNKNOWN;
		}
		switch (mLevelManager.pGameMode)
		{
		case FSGameMode.GLIDE_MODE:
			return HighScoresDifficulty.EASY;
		case FSGameMode.FLIGHT_SUIT_MODE:
			return HighScoresDifficulty.EASY;
		case FSGameMode.FLIGHT_MODE:
		{
			if (mLevelManager._DragonSelectionUi == null)
			{
				return HighScoresDifficulty.UNKNOWN;
			}
			if (mLevelManager._DragonSelectionUi.pSelectedTicketID == 0)
			{
				return HighScoresDifficulty.HARD;
			}
			HeroPetData heroDragonFromID = SanctuaryData.GetHeroDragonFromID(mLevelManager._DragonSelectionUi.pSelectedTicketID);
			if (heroDragonFromID == null)
			{
				return HighScoresDifficulty.UNKNOWN;
			}
			for (int i = 0; i < mLevelManager._HeroDifficulty.Length; i++)
			{
				if (mLevelManager._HeroDifficulty[i] != null && heroDragonFromID._Name == mLevelManager._HeroDifficulty[i]._HeroPetName)
				{
					if (GetLevelType() != 0)
					{
						return mLevelManager._HeroDifficulty[i]._Difficulty;
					}
					return HighScoresDifficulty.HARD;
				}
			}
			break;
		}
		}
		return HighScoresDifficulty.UNKNOWN;
	}

	private string GetCurrentRelatedAchievementID()
	{
		HighScoresDifficulty difficulty = GetDifficulty();
		if (difficulty != 0)
		{
			string[] obj = new string[5]
			{
				mLevelManager._DragonSelectionUi.pSelectedTicketID.ToString(),
				"_",
				null,
				null,
				null
			};
			int num = (int)difficulty;
			obj[2] = num.ToString();
			obj[3] = "_";
			obj[4] = mLevelManager.pCurrentLevel.ToString();
			return string.Concat(obj);
		}
		return string.Empty;
	}

	private int GetGradeIndex(int score)
	{
		int pCurrentLevel = mLevelManager.pCurrentLevel;
		GradeSystem[] array = null;
		if (mLevelManager.pGameMode == FSGameMode.FLIGHT_MODE)
		{
			array = mLevelManager._AdultData[mLevelManager.pLevelMenu.mCurrentSceneId]._GroupData[pCurrentLevel]._FSGrades;
		}
		else if (mLevelManager.pGameMode == FSGameMode.GLIDE_MODE)
		{
			array = mLevelManager._TeenData[mLevelManager.pLevelMenu.mCurrentSceneId]._GroupData[pCurrentLevel]._FSGrades;
		}
		else if (mLevelManager.pGameMode == FSGameMode.FLIGHT_SUIT_MODE)
		{
			array = mLevelManager._FlightSuitData[mLevelManager.pLevelMenu.mCurrentSceneId]._GroupData[pCurrentLevel]._FSGrades;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (score >= array[i]._PointNeeded)
			{
				return i;
			}
		}
		return array.Length;
	}

	public void RestartLevel(bool bFromLevelSelection)
	{
		mExtraTime = 0f;
		mTime = 0f;
		mIsExtraTime = false;
		mKAUIHud.ExtraTimeAlert(play: false, mExtraTime);
		mIsDragonMeterReady = false;
		if (bFromLevelSelection)
		{
			DisableLevelGroup();
			UiAvatarControls.pInstance.SetVisibility(inVisible: false);
			AssignHUDToToolbar(isAssign: false);
			if (mLevelManager != null)
			{
				RsResourceManager.LoadLevel(RsResourceManager.pCurrentLevel);
			}
		}
		else
		{
			if (!(mLevelManager != null))
			{
				return;
			}
			base.gameObject.SetActive(value: true);
			if (mLevelManager.pGameMode == FSGameMode.FLIGHT_SUIT_MODE || SanctuaryManager.pCurPetInstance.IsActionAllowed(PetActions.FLIGHTSCHOOL))
			{
				InitGame();
				if (_Breakables != null)
				{
					_Breakables.SendMessage("Respawn");
				}
			}
			else if (mLevelManager.pLevelMenu != null)
			{
				UiPetEnergyGenericDB.Show(base.gameObject, "OnEnergyPurchaseSuccess", "OnEnergyPurchaseFail", isLowEnergy: true);
			}
		}
	}

	public void Quit()
	{
		mGameIsPaused = true;
		mLevelManager._PowerUpManager.GamePause(pause: true);
		mLevelManager._UiConsumable._IsGamePaused = true;
		mLevelManager._UiConsumable.SetVisibility(inVisible: false);
		CancelInvoke();
		EnableHUD(isEnable: false);
		AvAvatar.pObject.SetActive(value: false);
		if (UiAvatarControls.pInstance != null)
		{
			UiAvatarControls.pInstance.EnableDragonFireButton(inEnable: false);
			UiAvatarControls.pInstance.DisableAllDragonControls();
		}
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: false);
		}
		SnChannel.MuteAll(mute: true);
		mPauseStartTime = Time.time;
		if (pTimeLimit >= 0f)
		{
			mTimeLeft = pTimeLimit - (Time.time - mGameStartTime);
		}
		if (mCurrentRacingObject != null)
		{
			mCurrentRacingObject.SetActive(value: false);
		}
		if (!(mLevelManager != null) || !(null != mLevelManager.pLevelMenu._LevelQuitUI))
		{
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(mLevelManager.pLevelMenu._LevelQuitUI, Vector3.zero, Quaternion.identity);
		if (null != gameObject)
		{
			gameObject.gameObject.SetActive(value: true);
			KAUIFlightSchoolQuit component = gameObject.GetComponent<KAUIFlightSchoolQuit>();
			KAUI.SetExclusive(component, new Color(0.5f, 0.5f, 0.5f, 0.5f));
			if (null != component)
			{
				component._Game = this;
			}
		}
	}

	public void OnQuitYes()
	{
		TimeHackPrevent.Reset();
		mKAUIHud.enabled = false;
		SnChannel.MuteAll(mute: false);
		ResetWrongWayData();
		mQuitGame = true;
		if (UiAvatarControls.pInstance != null)
		{
			UiAvatarControls.pInstance.EnableDragonFireButton(inEnable: false);
		}
		if (mLevelManager.mIsCurrentLevelSpecial)
		{
			UpdateHeroLevelDragonData(enableHeroLevelDragonData: false);
		}
		ResetDragonData();
	}

	private void ResetDragonData()
	{
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pCurPetInstance.OnFlyDismountImmediate(AvAvatar.pObject);
			SanctuaryPet.RemoveMountEvent(SanctuaryManager.pCurPetInstance, DragonMounted);
		}
		TeleportPlayerToStartMarker();
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pCurPetInstance.SetAvatar(AvAvatar.mTransform);
			SanctuaryManager.pCurPetInstance.MoveToAvatar();
		}
		AvAvatar.pState = AvAvatarState.PAUSED;
		if (mEndMarker != null)
		{
			AvAvatar.TeleportToObject(mEndMarker);
			AdjustPosition();
		}
	}

	public void OnQuitNo()
	{
		float num = Time.time - mPauseStartTime;
		mGameStartTime += num;
		mGameIsPaused = false;
		mLevelManager._PowerUpManager.GamePause(pause: false);
		mLevelManager._UiConsumable._IsGamePaused = false;
		if (mLevelManager.pGameMode != FSGameMode.FLIGHT_SUIT_MODE)
		{
			mLevelManager._UiConsumable.SetVisibility(inVisible: true);
		}
		SnChannel.MuteAll(mute: false);
		if (pTimeLimit >= 0f)
		{
			mTimeLeft = pTimeLimit - (Time.time - mGameStartTime);
			Invoke("EndGame", mTimeLeft);
		}
		AvAvatar.pObject.SetActive(value: true);
		EnableHUD(isEnable: true);
		if (MissionManager.pInstance != null && mCurrentTutorialUI == null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: true, inForceUpdate: true);
		}
		if (mCurrentRacingObject != null)
		{
			mCurrentRacingObject.SetActive(value: true);
		}
		if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pIsMounted && UiAvatarControls.pInstance != null)
		{
			UiAvatarControls.pInstance.EnableDragonFireButton(inEnable: true);
		}
		if (!mAvatarController)
		{
			mAvatarController = AvAvatar.pObject.GetComponent<AvAvatarController>();
		}
		if (mAvatarController != null && mLevelManager.pGameMode == FSGameMode.FLIGHT_MODE)
		{
			mAvatarController.SetFlyingState(FlyingState.Normal);
		}
		if (UiAvatarControls.pInstance != null)
		{
			UiAvatarControls.pInstance.DisableAllDragonControls(inDisable: false);
		}
	}

	private int GetPercentageScore()
	{
		int num = 0;
		int num2 = -1;
		if (mLevelManager != null && mLevelManager.pLevelMenu != null)
		{
			num2 = mLevelManager.pCurrentLevel;
			if (mLevelManager.pGameMode == FSGameMode.FLIGHT_MODE)
			{
				num = mLevelManager._AdultData[mLevelManager.pLevelMenu.mCurrentSceneId]._GroupData[num2]._MaxScore;
			}
			else if (mLevelManager.pGameMode == FSGameMode.GLIDE_MODE)
			{
				num = mLevelManager._TeenData[mLevelManager.pLevelMenu.mCurrentSceneId]._GroupData[num2]._MaxScore;
			}
			else if (mLevelManager.pGameMode == FSGameMode.FLIGHT_SUIT_MODE)
			{
				num = mLevelManager._FlightSuitData[mLevelManager.pLevelMenu.mCurrentSceneId]._GroupData[num2]._MaxScore;
			}
		}
		int num3 = mTotalScore * 100 / num;
		if (num3 > 100)
		{
			num3 = 100;
			UtDebug.Log("Error in computing score for level = " + num2);
		}
		return num3;
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType != WsServiceType.APPLY_PAYOUT)
		{
			return;
		}
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			if (inObject != null)
			{
				mAchievementRewards = (AchievementReward[])inObject;
				if (mAchievementRewards != null)
				{
					AchievementReward[] array = mAchievementRewards;
					foreach (AchievementReward achievementReward in array)
					{
						switch (achievementReward.PointTypeID)
						{
						case 2:
							mLevelManager.pRewardCoins = achievementReward.Amount.Value;
							Money.AddToGameCurrency(achievementReward.Amount.Value);
							break;
						case 8:
							mLevelManager.pDragonPoints = achievementReward.Amount.Value;
							SanctuaryManager.pInstance.AddXP(mLevelManager.pDragonPoints);
							break;
						}
					}
					ShowReward();
				}
				else
				{
					ShowReward();
				}
			}
			else
			{
				ShowReward();
			}
			break;
		case WsServiceEvent.ERROR:
			ShowReward();
			UtDebug.Log("reward data is null!!!");
			break;
		}
	}

	private void ShowReward(bool showGrades = true)
	{
		List<AchievementTask> list = new List<AchievementTask>();
		string text = "";
		string bgColor = null;
		if (showGrades)
		{
			int pCurrentLevel = mLevelManager.pCurrentLevel;
			GradeSystem[] array = null;
			if (mLevelManager.pGameMode == FSGameMode.FLIGHT_MODE)
			{
				array = mLevelManager._AdultData[mLevelManager.pLevelMenu.mCurrentSceneId]._GroupData[pCurrentLevel]._FSGrades;
			}
			if (mLevelManager.pGameMode == FSGameMode.GLIDE_MODE)
			{
				array = mLevelManager._TeenData[mLevelManager.pLevelMenu.mCurrentSceneId]._GroupData[pCurrentLevel]._FSGrades;
			}
			if (mLevelManager.pGameMode == FSGameMode.FLIGHT_SUIT_MODE)
			{
				array = mLevelManager._FlightSuitData[mLevelManager.pLevelMenu.mCurrentSceneId]._GroupData[pCurrentLevel]._FSGrades;
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (mTotalScore >= array[i]._PointNeeded)
				{
					text = array[i]._Grade;
					bgColor = array[i]._BGColor;
					break;
				}
				text = array[^1]._Grade;
				bgColor = array[^1]._BGColor;
			}
		}
		mLevelManager.ShowEndGameUI(this, text, bgColor, mAchievementRewards, showGrades);
		if (text.Equals(mLevelManager._FightClubMasterAchievement._Value))
		{
			list.Add(new AchievementTask(mLevelManager._FightClubMasterAchievement._ID));
		}
		UserAchievementTask.Set(list.ToArray());
	}

	private void LaunchHighScoresMenu(bool isTimeTrial, float inTotalTime)
	{
		if (mLevelManager.pGameMode == FSGameMode.FLIGHT_MODE)
		{
			HeroPetData heroDragonFromID = SanctuaryData.GetHeroDragonFromID(mLevelManager._DragonSelectionUi.pSelectedTicketID);
			if (mLevelManager._DragonSelectionUi.pSelectedTicketID == 0)
			{
				HighScores.SetCurrentGameSettings(mGameName, mGameID, isMultiPlayer: false, 3, GetLevel());
			}
			else
			{
				for (int i = 0; i < mLevelManager._HeroDifficulty.Length; i++)
				{
					if (heroDragonFromID._Name == mLevelManager._HeroDifficulty[i]._HeroPetName)
					{
						HighScoresDifficulty difficulty = HighScoresDifficulty.HARD;
						if (GetLevelType() != 0)
						{
							difficulty = mLevelManager._HeroDifficulty[i]._Difficulty;
						}
						HighScores.SetCurrentGameSettings(mLevelManager._HeroDifficulty[i]._GameModuleName, mLevelManager._HeroDifficulty[i]._GameID, isMultiPlayer: false, (int)difficulty, GetLevel());
						break;
					}
				}
			}
		}
		else if (mLevelManager.pGameMode == FSGameMode.GLIDE_MODE)
		{
			HighScores.SetCurrentGameSettings(mGameName, mGameID, isMultiPlayer: false, 1, GetLevel());
		}
		else if (mLevelManager.pGameMode == FSGameMode.FLIGHT_SUIT_MODE)
		{
			HighScores.SetCurrentGameSettings(mGameName, mGameID, isMultiPlayer: false, 1, GetLevel());
		}
		if (!isTimeTrial)
		{
			HighScores.AddGameData("highscore", mTotalScore.ToString());
			mLevelManager._EndDBUI.SetHighScoreData(mTotalScore, "highscore");
		}
		else if (inTotalTime > 0f)
		{
			int inScore = (int)(inTotalTime * 100f);
			HighScores.AddGameData("time", inScore.ToString());
			mLevelManager._EndDBUI.SetHighScoreData(inScore, "time", inAscendingOrder: true);
		}
	}

	private int GetLevel()
	{
		string text = base.transform.gameObject.name.Replace("_Adult", "");
		return int.Parse(text.Substring(text.Length - 2));
	}

	private int GetLevelType()
	{
		if (base.transform.gameObject.name.Contains("Adult"))
		{
			return 0;
		}
		return 1;
	}

	public void TeleportPlayerToStartMarker()
	{
		if (_StartMarker != null)
		{
			if (!mAvatarController)
			{
				mAvatarController = AvAvatar.pObject.GetComponent<AvAvatarController>();
			}
			if (mAvatarController != null)
			{
				mAvatarController.OnGlideLanding();
			}
			AvAvatar.TeleportToObject(_StartMarker);
			AdjustPosition();
		}
	}

	public bool IsGameRunning()
	{
		return mbGameRunning;
	}

	public void Update()
	{
		if (!mIsDragonMeterReady && SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pInstance != null && SanctuaryManager.pInstance.pPetMeter != null && mLevelManager._DragonSelectionUi.pSelectedTicketID <= 0)
		{
			mIsDragonMeterReady = true;
			SanctuaryManager.pInstance.pPetMeter.SetPetName(SanctuaryManager.pCurPetInstance.pData.Name);
		}
		if (mStartGame && (mLevelManager.pGameMode == FSGameMode.FLIGHT_SUIT_MODE || (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.mAvatarAnimBundle != null)))
		{
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			mStartGame = false;
			StartGame();
		}
		if (!mbGameRunning && !mGameIsPaused && mLevelManager != null && mLevelManager._UiCountdown != null && mLevelManager._UiCountdown.enabled && mLevelManager._UiCountdown.IsCountDownOver())
		{
			mLevelManager._UiCountdown.StartCountDown(inStart: false);
			StartGame();
		}
		else if (mLevelManager != null && mLevelManager._UiCountdown != null && mLevelManager._UiCountdown.enabled && !mLevelManager._UiCountdown.IsCountDownOver())
		{
			if (!mAvatarController)
			{
				mAvatarController = AvAvatar.pObject.GetComponent<AvAvatarController>();
			}
			if (mAvatarController != null)
			{
				mAvatarController.StopFlying();
			}
			AvAvatar.pState = AvAvatarState.PAUSED;
			if (UiAvatarControls.pInstance != null)
			{
				UiAvatarControls.pInstance.EnableDragonFireButton(inEnable: false);
			}
		}
		if (IsGameRunning())
		{
			mFlightTime = Time.time - mGameStartTime;
			if (_TimeTrial)
			{
				mNextCheckPtTimeleft -= Time.deltaTime;
				mKAUIHud.UpdateTimeDisplay(mNextCheckPtTimeleft);
				if (mNextCheckPtTimeleft <= 0f)
				{
					mTotalTimeTaken = -1f;
					EndGame(bForceEnd: false);
				}
			}
			else if (pTimeLimit >= 0f)
			{
				mTimeLeft = pTimeLimit - (Time.time - mGameStartTime);
				if (!mIsExtraTime)
				{
					mKAUIHud.UpdateTimeDisplay(mTimeLeft);
				}
				else if (mIsExtraTime && !mGameIsPaused)
				{
					mTime += Time.deltaTime;
					mKAUIHud.ExtraTimeAlert(play: true, mExtraTime - mTime);
					if (mTime >= mExtraTime)
					{
						mTime = 0f;
						mIsExtraTime = false;
						mExtraTime = 0f;
						mKAUIHud.ExtraTimeAlert(play: false, mExtraTime);
					}
				}
			}
			if (AvAvatar.pSubState != AvAvatarSubState.FLYING && AvAvatar.pSubState != AvAvatarSubState.GLIDING)
			{
				mbOnCliff = true;
			}
			else if (mbOnCliff)
			{
				mbOnCliff = false;
				AvAvatar.pToolbar.SetActive(value: true);
			}
			if (null != _PathSpline && !mQuitGame)
			{
				NavigateTowardsWaypoint();
			}
		}
		else if (AvAvatar.pSubState == AvAvatarSubState.FLYING || AvAvatar.pSubState == AvAvatarSubState.GLIDING)
		{
			mStartGame = true;
		}
		if (Application.isEditor && Input.GetKeyUp(KeyCode.F8))
		{
			OnObjectiveCompleted();
			mEnableCheatToEndGame = true;
			EndGame();
		}
	}

	protected virtual void NavigateTowardsWaypoint()
	{
		if (mbGameRunning && PathManager.pInstance != null)
		{
			MoveToNextNode();
			DistanceUpdate(0f);
			CheckConstraints();
		}
	}

	protected void MoveToNextNode()
	{
		Vector3 wayPointAt = GetWayPointAt(mCurrentWaypointIdx);
		Vector3 lhs = mAvatarTransform.position - wayPointAt;
		Vector3 rhs = GetWayPointAt(pNextWaypointIdx) - wayPointAt;
		if (Vector3.Dot(lhs, rhs) >= 0f)
		{
			Vector3 wayPointAt2 = GetWayPointAt(mCurrentWaypointIdx);
			Vector3 wayPointAt3 = GetWayPointAt(pPrevWaypointIdx);
			wayPointAt2.y = (wayPointAt3.y = 0f);
			float magnitude = (wayPointAt2 - wayPointAt3).magnitude;
			DistanceUpdate(magnitude);
			mGoingWrongWay = false;
			mLastWayPointIndex = mCurrentWaypointIdx;
			mCurrentWaypointIdx++;
			if (pCurrentPathID == 0)
			{
				mNumWayPointsCovered++;
				mLapNodeCount++;
			}
			int nodeCount = GetNodeCount();
			if (mCurrentWaypointIdx == nodeCount)
			{
				mCurrentWaypointIdx = 0;
			}
			else if (pCurrentPathID != 0 && mCurrentWaypointIdx >= nodeCount - 1)
			{
				mCurrentWaypointIdx = 0;
				mLapNodeCount = (mNumWayPointsCovered = 0);
				pCurrentPathID = 0;
			}
			if (mNumWayPointsCovered == PathManager.GetNodeCount(0))
			{
				mNumWayPointsCovered = 0;
				mSplineCoveredCnt++;
			}
			SetWaypointHighLight();
		}
	}

	protected void DistanceUpdate(float nodeDistance)
	{
		mDistanceForward += nodeDistance;
	}

	private void SetWaypointHighLight()
	{
		if (!(_WaypointHighLightObj == null))
		{
			if (mCurrWaypointHighlight != null)
			{
				UnityEngine.Object.Destroy(mCurrWaypointHighlight);
				mCurrWaypointHighlight = null;
			}
			mCurrWaypointHighlight = UnityEngine.Object.Instantiate(_WaypointHighLightObj);
			if (mCurrWaypointHighlight != null)
			{
				mCurrWaypointHighlight.name = "WaypointHighlight";
				Vector3 wayPointAt = GetWayPointAt(pNextWaypointIdx);
				wayPointAt.y = AvAvatar.position.y;
				mCurrWaypointHighlight.transform.position = wayPointAt;
			}
		}
	}

	private void CheckConstraints()
	{
		if (PathManager.pInstance == null)
		{
			return;
		}
		Vector3 wayPointAt = GetWayPointAt(pCurrentWaypointIdx);
		Vector3 wayPointAt2 = GetWayPointAt(pPrevWaypointIdx);
		Vector3 vector = UtUtilities.ClosestPointOnLine(wayPointAt, wayPointAt2, mAvatarTransform.position);
		float magnitude = (mAvatarTransform.position - vector).magnitude;
		float num = Mathf.Abs(mAvatarTransform.position.y - vector.y);
		mIsAway = magnitude > _DistanceThreshold || num > _VerticalThreshold;
		mGoingWrongWay = !IsMovingForward();
		if (mGoingWrongWay || mIsAway)
		{
			if (mResetTimer <= _WrongWayTimer)
			{
				mResetTimer += Time.deltaTime;
				if (mResetTimer > _WrongWayDelay)
				{
					string stringData = StringTable.GetStringData(_ThresholdCrossedText._ID, _ThresholdCrossedText._Text);
					stringData = stringData.Replace("{{NumSeconds}}", ((int)(_WrongWayTimer - mResetTimer) + 1).ToString());
					ShowWrongWayMessage(stringData, isShow: true);
				}
				return;
			}
			if (mIsAway)
			{
				AvAvatar.TeleportTo(wayPointAt2);
				AdjustPosition();
			}
			Vector3 normalized = (wayPointAt - wayPointAt2).normalized;
			mAvatarTransform.rotation = Quaternion.LookRotation(normalized);
			mResetTimer = 0f;
			mGoingWrongWay = false;
			mIsAway = false;
			ShowWrongWayMessage(string.Empty, isShow: true);
		}
		else
		{
			mResetTimer = 0f;
			ShowWrongWayMessage(string.Empty, isShow: false);
		}
	}

	public void ShowWrongWayMessage(string inText, bool isShow)
	{
		if (mKAUIHud != null)
		{
			mKAUIHud.ShowWrongWayText(inText);
		}
	}

	protected bool IsMovingForward()
	{
		if (PathManager.pInstance == null)
		{
			return true;
		}
		return Vector3.Dot(GetWayPointAt(mCurrentWaypointIdx) - mAvatarTransform.position, mAvatarTransform.forward) > -0.707f;
	}

	public Vector3 GetClosestPointOnLine(Vector3 nodeA, Vector3 nodeB, Vector3 pos)
	{
		Vector3 lhs = pos - nodeA;
		Vector3 vector = nodeB - nodeA;
		float sqrMagnitude = vector.sqrMagnitude;
		float num = Vector3.Dot(lhs, vector) / sqrMagnitude;
		if (num < 0f)
		{
			return nodeA;
		}
		if (num > 1f)
		{
			return nodeB;
		}
		return nodeA + vector * num;
	}

	public Vector3 GetWayPointAt(int index)
	{
		return PathManager.GetNodePosition(pCurrentPathID, index);
	}

	public int GetNodeCount()
	{
		return PathManager.GetNodeCount(pCurrentPathID);
	}

	public Quaternion GetWayPointRotation(int index)
	{
		return PathManager.GetNodeRot(pCurrentPathID, index);
	}

	public void DisableLevelGroup()
	{
		base.gameObject.SetActive(value: false);
	}

	private void EnableHUD(bool isEnable)
	{
		AvAvatar.SetUIActive(isEnable);
		if (mIsDragonMeterReady && SanctuaryManager.pInstance != null && SanctuaryManager.pInstance.pPetMeter != null)
		{
			SanctuaryManager.pInstance.pPetMeter.SetVisibility(isEnable);
		}
		if (mKAUIHud != null)
		{
			mKAUIHud.pGame = (isEnable ? this : null);
			mKAUIHud.enabled = isEnable;
			if ((UtPlatform.IsMobile() || UtPlatform.IsWSA()) && UiJoystick.pInstance != null)
			{
				UiJoystick.pInstance.SetVisibility(isEnable);
			}
			bool bCollection = _CollectableObjectTarget > 0;
			mKAUIHud.ShowButtons(bCollection);
			if (isEnable)
			{
				UpdateHUDUIData();
			}
		}
	}

	public void UpdateHUDUIData()
	{
		if (mKAUIHud != null)
		{
			if (_CollectableObjectTarget > 0)
			{
				mKAUIHud.UpdateCounterDispay(mNumPickupsCollected + " / " + _CollectableObjectTarget);
				return;
			}
			string inNumPickupsCollectedStr = mNumPickupsCollected + " / " + mNumPickupsAvailable;
			string inNumRingsCollectedStr = mNumRingsCollected + " / " + mNumRingsAvailable;
			string inNumTargetablesCollectedStr = mNumTargetablesCollected + " / " + mNumTargetablesAvailable;
			mKAUIHud.UpdateCollectablesCount(inNumPickupsCollectedStr, mNumPickupsAvailable > 0, inNumRingsCollectedStr, mNumRingsAvailable > 0, inNumTargetablesCollectedStr, mNumTargetablesAvailable > 0);
		}
	}

	public void StartTimer()
	{
		if (mKAUIHud != null)
		{
			mKAUIHud.StartTimer(pTimeLimit);
		}
	}

	public void StopTimer()
	{
		if (mKAUIHud != null)
		{
			mKAUIHud.StopTimer();
		}
	}

	public void StartAlertSound()
	{
		mTimeAlertSnChannel = null;
		if (_TimeAlertSound == null)
		{
			UtDebug.LogWarning("Time Alert Sound is null. Timer Alert SFX will not play");
		}
		else if (_TimeAlertSound._AudioClip == null)
		{
			UtDebug.LogWarning("Time Alert Sound Audio Clip is null. Timer Alert SFX will not play");
		}
		else
		{
			mTimeAlertSnChannel = _TimeAlertSound.Play(inForce: true);
		}
	}

	public void StopAlertSound()
	{
		if (mTimeAlertSnChannel != null)
		{
			mTimeAlertSnChannel.Stop();
		}
	}

	public bool IsTimeToPlayAlertSound()
	{
		if (Time.time > mGameStartTime + _TimeUntilAlertSound)
		{
			return true;
		}
		return false;
	}

	public void PauseGame()
	{
		Time.timeScale = 0f;
		if (IsTimeToPlayAlertSound())
		{
			StopAlertSound();
		}
	}

	public void ResumeGame()
	{
		Time.timeScale = 1f;
		if (IsTimeToPlayAlertSound())
		{
			StartAlertSound();
		}
	}

	public bool ReachedCheckpoint(int index, float timeToReachNext)
	{
		if (mNextCheckPoint == index)
		{
			if (Mathf.Approximately(timeToReachNext, 0f))
			{
				mTotalTimeTaken = Time.time - mGameStartTime;
				EndGame(bForceEnd: false);
			}
			mNextCheckPtTimeleft += timeToReachNext;
			mNextCheckPoint++;
			return true;
		}
		return false;
	}

	private void OnEnergyPurchaseSuccess()
	{
		RestartLevel(bFromLevelSelection: false);
	}

	private void OnEnergyPurchaseFail()
	{
		RestartLevel(bFromLevelSelection: true);
	}

	private void AdjustPosition()
	{
		if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pData != null && SanctuaryManager.pCurPetInstance.pData.PetTypeID == 18)
		{
			AvAvatar.pObject.transform.Translate(0f, _TimberjackOffset, 0f);
		}
	}

	public void ExtraTime(bool extraTime, float duration)
	{
		float time = mTimeLeft + duration;
		mIsExtraTime = extraTime;
		mExtraTime += duration;
		mGameStartTime += duration;
		mKAUIHud.ExtraTimeAlert(extraTime, mExtraTime);
		CancelInvoke("EndGame");
		Invoke("EndGame", time);
	}

	public void Collect(GameObject powerUp)
	{
		if (mLevelManager._UiConsumable != null)
		{
			Consumable consumableOnProbability = ConsumableData.GetConsumableOnProbability("FlightClub", "Game", 1);
			if (consumableOnProbability != null)
			{
				UserAchievementTask.Set(mLevelManager._ItemHoarderAchievement._ID);
				mLevelManager._UiConsumable.RegisterConsumable(consumableOnProbability);
			}
			else
			{
				Debug.LogError("ERROR! did not find a suitable consumable");
			}
		}
	}

	private void UpdateHeroLevelDragonData(bool enableHeroLevelDragonData)
	{
		if (enableHeroLevelDragonData)
		{
			if (!string.IsNullOrEmpty(_SpecialAttackAnim))
			{
				mPrevDragonAnimHeroLevel = SanctuaryManager.pCurPetInstance._FlyAttackAnim;
				SanctuaryManager.pCurPetInstance._FlyAttackAnim = _SpecialAttackAnim;
			}
			if (SanctuaryManager.pCurPetInstance.pWeaponManager._SpecialAttackShootingPoints != null && SanctuaryManager.pCurPetInstance.pWeaponManager._SpecialAttackShootingPoints.Count > 0)
			{
				mPrevShootingPointHeroLevel = SanctuaryManager.pCurPetInstance.pWeaponManager._ShootingPoints;
				SanctuaryManager.pCurPetInstance.pWeaponManager._ShootingPoints = SanctuaryManager.pCurPetInstance.pWeaponManager._SpecialAttackShootingPoints;
				SanctuaryManager.pCurPetInstance.pWeaponManager.ShootPointTrans = SanctuaryManager.pCurPetInstance.pWeaponManager._SpecialAttackShootingPoints[0];
			}
			if (_SpecialAttackWeaponPf != null)
			{
				mPrevDragonWeaponPf = SanctuaryManager.pCurPetInstance.pWeaponManager._WeaponPfOverride;
				SanctuaryManager.pCurPetInstance.pWeaponManager._WeaponPfOverride = _SpecialAttackWeaponPf;
			}
			if (_SpecialAttackWeaponHitPf != null)
			{
				mPrevDragonWeaponHitPf = SanctuaryManager.pCurPetInstance.pWeaponManager._WeaponHitPfOverride;
				SanctuaryManager.pCurPetInstance.pWeaponManager._WeaponHitPfOverride = _SpecialAttackWeaponHitPf;
			}
		}
		else
		{
			if (!string.IsNullOrEmpty(mPrevDragonAnimHeroLevel))
			{
				SanctuaryManager.pCurPetInstance._FlyAttackAnim = mPrevDragonAnimHeroLevel;
				mPrevDragonAnimHeroLevel = null;
			}
			if (mPrevShootingPointHeroLevel != null && mPrevShootingPointHeroLevel.Count > 0)
			{
				SanctuaryManager.pCurPetInstance.pWeaponManager._ShootingPoints = mPrevShootingPointHeroLevel;
				SanctuaryManager.pCurPetInstance.pWeaponManager.ShootPointTrans = mPrevShootingPointHeroLevel[0];
				mPrevShootingPointHeroLevel = null;
			}
			if (_SpecialAttackWeaponPf != null)
			{
				SanctuaryManager.pCurPetInstance.pWeaponManager._WeaponPfOverride = mPrevDragonWeaponPf;
				mPrevDragonWeaponPf = null;
			}
			if (_SpecialAttackWeaponHitPf != null)
			{
				SanctuaryManager.pCurPetInstance.pWeaponManager._WeaponHitPfOverride = mPrevDragonWeaponHitPf;
				mPrevDragonWeaponHitPf = null;
			}
		}
	}

	private void OnDestroy()
	{
		if (mLevelManager.pGameMode == FSGameMode.FLIGHT_SUIT_MODE)
		{
			if (!mAvatarController)
			{
				mAvatarController = AvAvatar.pObject.GetComponent<AvAvatarController>();
			}
			if (mAvatarController != null)
			{
				mAvatarController.PlayingFlightSchoolFlightSuit = false;
			}
		}
	}
}
