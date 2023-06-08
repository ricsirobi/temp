using System;
using System.Collections.Generic;
using UnityEngine;

public class ScientificExperiment : MonoBehaviour
{
	[Serializable]
	public class LabItemProceduralMat
	{
		public string _ItemName;
	}

	[Serializable]
	public class LabDragonData
	{
		public string _Name;

		public LabDragonAgeData[] _AgeData;

		public AvAvatarAnimEvent[] _AnimEvents;

		public float _MaxTopLookAtAngle = 100f;

		public float _MaxDownLookAtAngle = 100f;

		public float _MaxLeftLookAtAngle = 100f;

		public float _MaxRightLookAtAngle = 100f;

		public LabDragonAgeData GetAgeData(string inAgeName)
		{
			LabDragonAgeData[] ageData = _AgeData;
			foreach (LabDragonAgeData labDragonAgeData in ageData)
			{
				if (labDragonAgeData != null && labDragonAgeData._Name == inAgeName)
				{
					return labDragonAgeData;
				}
			}
			return null;
		}
	}

	[Serializable]
	public class LabDragonAgeData
	{
		public string _Name;

		public string _BreathFireParticleRes;

		public string _Bone = "ShootingPoint";

		public Vector3 _OffsetPos;

		public Vector3 _OffsetRotation;

		public MarkerMap[] _MarkerMap;

		public Transform _Marker;

		public Transform _IdlePetMarker;
	}

	[Serializable]
	public class MarkerMap
	{
		public ExperimentType _Experiment;

		public Transform _Marker;
	}

	[Serializable]
	public class LabSubstanceMap
	{
		public string _TestItemName;

		public string _MaterialName;
	}

	[Serializable]
	public class LabTutorialData
	{
		public int _TaskID;

		public LabTutorial _Tutorial;
	}

	[Serializable]
	public class PetSkinMapping
	{
		public int _PetTypeID;

		[Tooltip("Skin will not be applied to Player's Current Pet")]
		public DragonSkin _Skin;
	}

	[Serializable]
	public class BreatheInfo
	{
		public ExperimentType _Experiment;

		public Transform _MarkerDragonHeadTarget;

		public Transform _MarkerBreatheTarget;

		public string _BreatheAnimation;
	}

	public const uint LOG_MASK = 8u;

	private const float DEFAULT_HEATTIME = 1.5f;

	public UiScienceExperiment _MainUI;

	public string _TestItemLayer = "DraggedObject";

	public string _ExitMarker;

	public float _CoolDuration = 2f;

	public Transform _KillMarker;

	public Transform _TestItemResetMarker;

	public float _CrucibleRadius = 0.35f;

	public int _MaxNumItemsAllowedInCrucible = 10;

	public LocaleString _TapTimeToStartText = new LocaleString("Click timer to start");

	public LocaleString _TapTimeToStartMobText = new LocaleString("Tap timer to start");

	public LocaleString _RecordInJournalText = new LocaleString("Click to record your observation in the Journal.");

	public LocaleString _RecordInJournalMobText = new LocaleString("Tap to record your observation in the Journal.");

	public LocaleString _ProcedureHalted = new LocaleString("Procedure halted.");

	public LocaleString _TitrationBaseNeutralText = new LocaleString("You've neutralized the {{ITEM}} by adding {{ACIDITY}} base droplets!");

	public LocaleString _TitrationAcidNeutralText = new LocaleString("You've neutralized the {{ITEM}} by adding {{ACIDITY}} acid droplets!");

	public float _DragonHeatTemperature = 50f;

	public float _IceScoopTemeprature = -50f;

	public float _DragonHeatMultiplier = 1f;

	public float _DragonCoolMultiplier = -1f;

	public GameObject _IceSet;

	public Collider _IceBox;

	public GameObject _IceOnCursor;

	public Transform _DragonMarkers;

	public float _WateringTime = 5f;

	public Transform[] _CrucibleMarkers;

	public Transform[] _SolidPowderMarkers;

	public BreatheInfo[] _BreatheInfo;

	public float _CoolingConstant;

	public float _WarmingConstant;

	public float _FreezeRate;

	public float _WeighingMachineSpeed = 25f;

	public float _WeighingMachineBrake = 250f;

	public float _DefaultMaxWeight = 1000f;

	public float _WeighMachineLength = 1000f;

	public float _TemperatureResetTime = 3f;

	public ParticleSystem _WaterStream;

	public ParticleSystem _AcidStream;

	public ParticleSystem _BaseStream;

	public ParticleSystem _WaterSplashSteam;

	public Animation _WaterPull;

	public Vector3 _LiquidItemDefaultPos = new Vector3(0f, 0.862f, 0f);

	public float _ScaleTime = 5f;

	public Transform _ToolboxTrigger;

	public Transform _Toolbox;

	public Transform _Crucible;

	public Transform _SpectrumCrucible;

	public Transform _CrucibleTriggerSmall;

	public Transform _CrucibleTriggerBig;

	public Camera _MainCamera;

	public LabDragonData[] _DragonData;

	public float _TestItemLifeTimeOnFloor = 6f;

	public LabGronckle _Gronckle;

	public LabSpectrum _Spectrum;

	public LabTitration _Titration;

	public int _GronckleId = 13;

	public LabSubstanceMap[] _SubstanceMap;

	public Material _ShaderMaterial;

	public LabTutorial _Tutorial;

	public List<LabTutorialData> _TutorialList;

	public List<PetSkinMapping> _PetSkinMapping;

	public AudioClip _DragonFireSFX;

	public AudioClip _SkrillBreatheSFX;

	public AudioClip _AddWaterSFX;

	public AudioClip _CrucibleClickSFX;

	public AudioClip _TitrationClickSFX;

	public AudioClip _DragonExcitedSFX;

	public AudioClip _TaskCompletedSFX;

	public AudioClip _DragonDropSolidSFX;

	public AudioClip _ExperimentCompleteSFX;

	public AudioClip _FlameStartSFX;

	public AudioClip _FlameSFX;

	public AudioClip _IceButtonSFX;

	public AudioClip _IceDropSFX;

	public AudioClip _IceMeltSFX;

	public AudioClip _IceScoopSFX;

	public AudioClip _RemoveToolSFX;

	public AudioClip _SelectToolSFX;

	public AudioClip _TimeStartSFX;

	public AudioClip _TimeTickSFX;

	public AudioClip _TimeUpSFX;

	public AudioClip _ToolboxClickSFX;

	public AudioClip _WaterSteamSFX;

	public AudioClip _WaterSteamSuddenSFX;

	public AudioClip _PickupSFX;

	public AudioClip _ApprovalSFX;

	public AudioClip _DragonDisapprovalSFX;

	public AudioClip[] _SolidMoveSFX;

	public AudioClip[] _LiquidMoveSFX;

	public float _MinTemperartureToStartCooldownSFX = 160f;

	public string _WaterItemName = "Water";

	public string _WaterPullChainAnim = "LabPullChain";

	public string _BreatheAnim = "LabBlowFire";

	public string _IdleAnim = "LabIdle";

	public GameObject _MixingEffect;

	public ParticleSystem _Splash;

	public float _MaxClockTimeInMinutes = 99f;

	public Transform _ColliderGroup;

	public ParticleSystem _RemoveTestItemFx;

	public GameObject _RopePullGameObject;

	public GameObject _MagnetGameObject;

	public GameObject _SpectrumGameObject;

	public GameObject _TitrationGameObject;

	public GameObject _MagnetAttachmentNode;

	public GameObject[] _ObjsToDisableForMagnetismLab;

	public GameObject[] _ObjsToDisableForSpectrumLab;

	public GameObject[] _ObjsToDisableForTitraionLab;

	public float _TitrationMixTime = 2f;

	public float _MagnetMoveTime = 0.6f;

	public Vector3 _MagnetTargetOffset = new Vector3(0f, -0.01f, 0f);

	private Vector3 mMagnetOrgPos;

	private bool mMagnetActivated;

	private float mTimeSinceExperimentStarted;

	private bool mShowClock;

	private bool mFireAtTarget;

	private float mAcidTitrationTimer;

	private float mBaseTitrationTimer;

	private Experiment mExperiment;

	private LabCrucible mCrucible;

	private LabThermometer mThermometer;

	private bool mInitialized;

	private float mIceSetTimer;

	private KAUIGenericDB mKAUIGenericDB;

	private bool mXMLLoaded;

	private static LabData mLabData;

	private bool mTimeEnabled = true;

	private bool mDragonPositioned;

	private bool mExperimentIntialized;

	private bool mTutorialInitialized;

	private Transform mBreatheTargetMarker;

	private Transform mDragonHeadTargetMarker;

	private LabTutorial mCurrPlayingTutorial;

	private static string mLastScene = string.Empty;

	private float mTimer;

	private SanctuaryPet mCurrentDragon;

	private const float INVALID_ANIM_TIMER = -999f;

	private LabItem mWaterLabItem;

	private GameObject mWaterGameObject;

	private LabItem mWaterObjectParent;

	private float mAnimTimer;

	private GameObject mDragonFlameParticle;

	private bool mDragonDataInitializing;

	private bool mBreathParticleLoaded;

	private bool mDragonLookAtMouse = true;

	private LabDragonData mDragonData;

	private bool mExiting;

	private bool mCreatedDragon;

	private bool mWaitingForAnimEvent;

	public static int pActiveExperimentID = 1009;

	public static bool pUseExperimentCheat = false;

	private static ScientificExperiment mInstance = null;

	private float mHeatTime;

	public bool pMagnetActivated
	{
		get
		{
			return mMagnetActivated;
		}
		set
		{
			mMagnetActivated = value;
		}
	}

	public ParticleSystem pSplash { get; set; }

	public bool pTitrationActive
	{
		get
		{
			if (!(mAcidTitrationTimer > 0f))
			{
				return mBaseTitrationTimer > 0f;
			}
			return true;
		}
	}

	public LabTask pTimerActivatedTask { get; set; }

	public bool pUserEnabledTimer => pTimerActivatedTask == null;

	public bool pTimeEnabled => mTimeEnabled;

	public GameObject pDragonFlameParticle => mDragonFlameParticle;

	private LabDragonAgeData pDragonAgeData
	{
		get
		{
			if (mDragonData == null)
			{
				SanctuaryPetTypeInfo typeInfo = mCurrentDragon.GetTypeInfo();
				if (typeInfo == null)
				{
					return null;
				}
				mDragonData = GetDragonData(typeInfo._Name);
			}
			return mDragonData.GetAgeData(mCurrentDragon.pCurAgeData._Name);
		}
	}

	public Experiment pExperiment => mExperiment;

	public ExperimentType pExperimentType
	{
		get
		{
			if (pExperiment != null)
			{
				return (ExperimentType)pExperiment.Type;
			}
			return ExperimentType.UNKNOWN;
		}
	}

	public bool pWaitingForAnimEvent
	{
		get
		{
			return mWaitingForAnimEvent;
		}
		set
		{
			mWaitingForAnimEvent = value;
		}
	}

	public static ScientificExperiment pInstance => mInstance;

	private bool pUsingFreezeIces
	{
		get
		{
			if (pCrucible != null)
			{
				return pCrucible.pFreezing;
			}
			return false;
		}
	}

	public LabThermometer pThermometer => mThermometer;

	public bool pShowClock
	{
		get
		{
			return mShowClock;
		}
		set
		{
			if (value && mShowClock != value)
			{
				ResetTime();
			}
			DisableTime();
			mShowClock = value;
			if (_MainUI != null && _MainUI.gameObject != null)
			{
				_MainUI.TriggerTimerObject(mShowClock);
			}
		}
	}

	public float pOneClockTime => 60f;

	public LabCrucible pCrucible
	{
		get
		{
			if (pExperimentType == ExperimentType.GRONCKLE_IRON && _Gronckle != null)
			{
				return _Gronckle.pCrucible ?? mCrucible;
			}
			if (pExperimentType == ExperimentType.SPECTRUM_LAB && _Spectrum != null)
			{
				return _Spectrum.pCrucible ?? mCrucible;
			}
			if (pExperimentType == ExperimentType.TITRATION_LAB && _Titration != null)
			{
				return _Titration.pCrucible ?? mCrucible;
			}
			return mCrucible;
		}
	}

	private bool pDragonDataInitialized => mBreathParticleLoaded;

	public int pCrucibleItemCount
	{
		get
		{
			if (pCrucible != null && pCrucible.pTestItems != null)
			{
				return pCrucible.pTestItems.Count;
			}
			return 0;
		}
	}

	public SanctuaryPet pCurrentDragon => mCurrentDragon;

	public float pTimer => mTimer;

	public float pHeatTime => mHeatTime;

	public void Start()
	{
		if (KAInput.pInstance.IsTouchInput())
		{
			_TapTimeToStartText = _TapTimeToStartMobText;
			_RecordInJournalText = _RecordInJournalMobText;
		}
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: false);
		}
		if (_WaterStream != null)
		{
			_WaterStream.Stop();
		}
		if (_AcidStream != null)
		{
			ParticleSystem.EmissionModule emission = _AcidStream.emission;
			emission.enabled = false;
		}
		if (_BaseStream != null)
		{
			ParticleSystem.EmissionModule emission2 = _BaseStream.emission;
			emission2.enabled = false;
		}
		if (_WaterSplashSteam != null)
		{
			_WaterSplashSteam.Stop();
		}
		mXMLLoaded = false;
		LabTutorial.InTutorial = false;
		mCurrentDragon = null;
		LabData.Load(XMLLoaded);
		pShowClock = false;
		mInstance = this;
		SanctuaryManager.pInstance.pDisablePetSwitch = true;
	}

	private void XMLLoaded(bool inSuccess)
	{
		mLabData = LabData.pInstance;
		mXMLLoaded = inSuccess;
	}

	private void Initialize()
	{
		if (!mXMLLoaded || UICursorManager.pCursorManager == null || _MainUI == null || SanctuaryData.pInstance == null)
		{
			return;
		}
		if (pUseExperimentCheat)
		{
			mExperiment = mLabData.GetLabExperimentByID(pActiveExperimentID);
		}
		else
		{
			mExperiment = GetActiveExperiment();
			if (mExperiment == null && mKAUIGenericDB == null)
			{
				mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "NoActive");
				if (mKAUIGenericDB != null)
				{
					mKAUIGenericDB.SetMessage(base.gameObject, string.Empty, string.Empty, string.Empty, string.Empty);
					mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
					mKAUIGenericDB.SetText("No active scientific quest..", interactive: true);
					mKAUIGenericDB.SetMessage(base.gameObject, string.Empty, string.Empty, "OnNoActiveQuestOk", string.Empty);
					KAUI.SetExclusive(mKAUIGenericDB);
				}
			}
		}
		if (mCurrentDragon == null)
		{
			if (!mCreatedDragon && mExperiment != null)
			{
				if (mExperiment.Type == 1)
				{
					InitGronckleExp();
				}
				else
				{
					CreateDragon(mExperiment.DragonType, mExperiment.DragonStage, mExperiment.DragonGender);
				}
				mCreatedDragon = true;
			}
			if (mCurrentDragon == null)
			{
				return;
			}
		}
		if (!pDragonDataInitialized)
		{
			InitializeDragonData();
		}
		if (!mExperimentIntialized)
		{
			mExperimentIntialized = _MainUI.InitializeExperiment(mExperiment);
		}
		if (!mTutorialInitialized && mExperimentIntialized && _MainUI.pIsReady)
		{
			mCurrPlayingTutorial = null;
			if (_TutorialList != null && mExperiment != null)
			{
				LabTutorial tutorial = GetTutorial(mExperiment.ID);
				if (tutorial != null)
				{
					tutorial.gameObject.SetActive(value: true);
					mCurrPlayingTutorial = tutorial;
				}
			}
			mTutorialInitialized = true;
		}
		if (!mDragonPositioned || !mExperimentIntialized || !mTutorialInitialized)
		{
			return;
		}
		if (mCurrPlayingTutorial != null)
		{
			ObStatus component = mCurrPlayingTutorial.GetComponent<ObStatus>();
			if (component != null && !component.pIsReady)
			{
				return;
			}
		}
		OnLevelReady();
	}

	private void InitGronckleExp()
	{
		if (_Gronckle != null)
		{
			_Gronckle.gameObject.SetActive(value: true);
			_Gronckle.Init(this);
		}
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(_GronckleId);
		string empty = string.Empty;
		int ageIndex = RaisedPetData.GetAgeIndex(RaisedPetStage.ADULT);
		RaisedPetData pdata = RaisedPetData.CreateCustomizedPetData(resName: (sanctuaryPetTypeInfo._AgeData[ageIndex]._PetResList[0]._Gender != Gender.Male) ? sanctuaryPetTypeInfo._AgeData[ageIndex]._PetResList[1]._Prefab : sanctuaryPetTypeInfo._AgeData[ageIndex]._PetResList[0]._Prefab, ptype: _GronckleId, stage: RaisedPetStage.ADULT, gender: Gender.Male, colorMap: null, noColorMap: true);
		SanctuaryPet component = _Gronckle.GetComponent<SanctuaryPet>();
		component.Init(pdata, noHat: false);
		SetCurrentDragon(component);
	}

	private void InitializeDragonData()
	{
		if (pDragonDataInitialized || mDragonDataInitializing || mCurrentDragon == null)
		{
			return;
		}
		mDragonDataInitializing = true;
		SanctuaryPetTypeInfo typeInfo = mCurrentDragon.GetTypeInfo();
		if (typeInfo != null && mCurrentDragon.pCurAgeData != null)
		{
			mDragonData = GetDragonData(typeInfo._Name);
			if (pDragonAgeData != null)
			{
				string[] array = pDragonAgeData._BreathFireParticleRes.Split('/');
				if (array.Length == 3)
				{
					RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnBreathFireParticleDownloaded, typeof(GameObject));
				}
			}
		}
		LabDragonAnimEvents component = mCurrentDragon.GetComponent<LabDragonAnimEvents>();
		if (component == null)
		{
			component = mCurrentDragon.gameObject.AddComponent<LabDragonAnimEvents>();
			if (mDragonData != null)
			{
				component._Events = mDragonData._AnimEvents;
			}
		}
		PlayDragonAnim(_IdleAnim, inPlayOnce: true);
	}

	public void Update()
	{
		if (!mInitialized)
		{
			Initialize();
			return;
		}
		if (!mExiting && MainStreetMMOClient.pInstance != null && !MainStreetMMOClient.pInstance.pAllDeactivated)
		{
			MainStreetMMOClient.pInstance.ActivateAll(active: false);
		}
		if (pUsingFreezeIces && !_MainUI.pUserPromptOn)
		{
			if (mIceSetTimer < 0f)
			{
				UseIceSet(inUseIceScoop: false);
			}
			else
			{
				mIceSetTimer -= Time.deltaTime;
				if (_CoolDuration - mIceSetTimer >= 1f)
				{
					_IceSet.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.75f, 1f - mIceSetTimer / (_CoolDuration - 1f));
				}
			}
		}
		mTimeSinceExperimentStarted += Time.deltaTime;
		if (pCrucible != null)
		{
			pCrucible.DoUpdate();
		}
		if (mThermometer != null)
		{
			mThermometer.DoUpdate();
		}
		UpdateTime();
		UpdateAnimTimer();
		if (pExperimentType == ExperimentType.TITRATION_LAB)
		{
			UpdateTitration();
		}
		if (mFireAtTarget && mBreatheTargetMarker != null)
		{
			mDragonFlameParticle.transform.LookAt(mBreatheTargetMarker);
		}
		if (mCurrentDragon != null && mDragonData != null && mCurrentDragon.pHeadBonesLookAtData != null && mCurrentDragon.pHeadBonesLookAtData.Count > 0 && mCurrentDragon.pHeadBonesLookAtData[0].mHeadBone != null && _Crucible != null && _MainCamera != null && mDragonLookAtMouse)
		{
			Vector3 mousePosition = Input.mousePosition;
			mousePosition.z = _Crucible.position.z + 2f;
			Vector3 vector = _MainCamera.ScreenToWorldPoint(mousePosition);
			Vector3 lhs = vector - mCurrentDragon.pHeadBonesLookAtData[0].mHeadBone.transform.position;
			lhs.Normalize();
			float f = Vector3.Dot(lhs, mCurrentDragon.transform.right);
			f = 90f - Mathf.Acos(f) * 57.29578f;
			float f2 = Vector3.Dot(lhs, mCurrentDragon.transform.up);
			f2 = 90f - Mathf.Acos(f2) * 57.29578f;
			if (f2 <= mDragonData._MaxTopLookAtAngle && f2 >= mDragonData._MaxDownLookAtAngle * -1f && f <= mDragonData._MaxLeftLookAtAngle && f >= mDragonData._MaxRightLookAtAngle * -1f)
			{
				mCurrentDragon.SetLookAt(vector, tween: true);
			}
		}
	}

	private void UpdateTitration()
	{
		if (!(mBaseTitrationTimer > 0f) && !(mAcidTitrationTimer > 0f))
		{
			return;
		}
		mAcidTitrationTimer -= Time.deltaTime;
		mBaseTitrationTimer -= Time.deltaTime;
		if (mAcidTitrationTimer <= 0f)
		{
			mAcidTitrationTimer = 0f;
			if (_AcidStream != null)
			{
				ParticleSystem.EmissionModule emission = _AcidStream.emission;
				emission.enabled = false;
			}
		}
		if (mBaseTitrationTimer <= 0f)
		{
			mBaseTitrationTimer = 0f;
			if (_BaseStream != null)
			{
				ParticleSystem.EmissionModule emission2 = _BaseStream.emission;
				emission2.enabled = false;
			}
		}
	}

	public void ResetTime()
	{
		mTimer = 0f;
	}

	public void DisableTime()
	{
		if (mTimeEnabled)
		{
			CheckForProcedureHalt("Action", "Time");
			if (_MainUI._TimerArrow != null)
			{
				_MainUI._TimerArrow.enabled = false;
			}
			mTimeEnabled = false;
			SnChannel.StopPool("Timer_Pool");
			SnChannel.Play(_TimeUpSFX, "SFX_Pool", inForce: true, null);
		}
	}

	public void EnableTime(bool inUserActivated)
	{
		if (!mTimeEnabled)
		{
			if (_MainUI._TimerArrow != null)
			{
				_MainUI._TimerArrow.enabled = true;
			}
			mTimeEnabled = true;
			SnChannel.Play(_TimeStartSFX, "SFX_Pool", inForce: true, null);
			SnChannel snChannel = SnChannel.Play(_TimeTickSFX, "Timer_Pool", inForce: true, null);
			if (snChannel != null)
			{
				snChannel.pLoop = true;
			}
			if (inUserActivated)
			{
				pTimerActivatedTask = null;
			}
		}
	}

	public void UpdateTime()
	{
		if (mTimeEnabled && !_MainUI.pUserPromptOn)
		{
			mTimer = Mathf.Min(_MaxClockTimeInMinutes * 60f, mTimer + Time.deltaTime);
		}
	}

	private LabTutorial GetTutorial(int experimentID)
	{
		return _TutorialList.Find((LabTutorialData data) => data._TaskID == experimentID)?._Tutorial;
	}

	public void OnLevelReady()
	{
		bool flag = false;
		if (mCurrPlayingTutorial != null)
		{
			mCurrPlayingTutorial.ShowTutorial();
			flag = true;
		}
		RsResourceManager.DestroyLoadScreen();
		UICursorManager.pVisibility = true;
		KAUICursorManager.SetDefaultCursor("Activate");
		if (RsResourceManager.pLastLevel != GameConfig.GetKeyData("ProfileScene") && RsResourceManager.pLastLevel != GameConfig.GetKeyData("StoreScene") && RsResourceManager.pLastLevel != GameConfig.GetKeyData("JournalScene"))
		{
			mLastScene = RsResourceManager.pLastLevel;
		}
		if (mKAUIGenericDB != null)
		{
			UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
		}
		mKAUIGenericDB = null;
		mHeatTime = GetHeatTime();
		mCrucible = new LabCrucible(this);
		if (mExperiment != null)
		{
			mThermometer = new LabThermometer(pCrucible, mExperiment.ThermometerMin, mExperiment.ThermometerMax);
		}
		else
		{
			mThermometer = new LabThermometer(pCrucible, 0f, 100f);
		}
		if (pExperimentType == ExperimentType.MAGNETISM_LAB)
		{
			_MagnetGameObject.SetActive(value: true);
			for (int i = 0; i < _ObjsToDisableForMagnetismLab.Length; i++)
			{
				_ObjsToDisableForMagnetismLab[i].SetActive(value: false);
			}
			mMagnetOrgPos = _MagnetGameObject.transform.position;
		}
		if (pExperimentType == ExperimentType.SPECTRUM_LAB)
		{
			Physics.IgnoreLayerCollision(LayerMask.NameToLayer(_TestItemLayer), LayerMask.NameToLayer(_TestItemLayer), ignore: true);
			_SpectrumGameObject.SetActive(value: true);
			for (int j = 0; j < _ObjsToDisableForSpectrumLab.Length; j++)
			{
				_ObjsToDisableForSpectrumLab[j].SetActive(value: false);
			}
			if (_SpectrumCrucible != null)
			{
				_MainUI._Crucible = _SpectrumCrucible;
				_Crucible = _SpectrumCrucible;
			}
			if (_Spectrum != null)
			{
				_Spectrum.gameObject.SetActive(value: true);
				_Spectrum.Init(this);
			}
		}
		if (pExperimentType == ExperimentType.TITRATION_LAB)
		{
			_TitrationGameObject.SetActive(value: true);
			for (int k = 0; k < _ObjsToDisableForTitraionLab.Length; k++)
			{
				_ObjsToDisableForTitraionLab[k].SetActive(value: false);
			}
			if (_Titration != null)
			{
				_Titration.Init(this);
				if (_MainUI._TitrationWidgetGroup != null)
				{
					_MainUI._TitrationWidgetGroup.SetActive(value: true);
				}
			}
		}
		if (!flag)
		{
			SyncTasksWithMissionData();
		}
		_MainUI.OnLevelReady();
		AvAvatar.SetActive(inActive: false);
		if (_MainUI != null)
		{
			LabTask anyIncompleteTask = pExperiment.GetAnyIncompleteTask();
			if (anyIncompleteTask != null && !flag)
			{
				_MainUI.ShowExperimentDirection(anyIncompleteTask);
			}
			else
			{
				_MainUI.ShowExperimentDirection();
			}
		}
		StopDragonAnim();
		InitBreatheAtTargetInfo();
		mInitialized = true;
	}

	public void BreathFlame(bool inBreath)
	{
		if (pCrucible == null)
		{
			return;
		}
		if (inBreath)
		{
			if (pCrucible.CanHeat())
			{
				PlayDragonAnim(_BreatheAnim, mDragonHeadTargetMarker, inPlayOnce: true);
			}
		}
		else
		{
			pCrucible.StopHeat();
		}
	}

	public void UseIceSet(bool inUseIceScoop)
	{
		if (pCrucible != null)
		{
			if (inUseIceScoop)
			{
				pCrucible.Freeze();
				mIceSetTimer = _CoolDuration;
				_IceSet.transform.localScale = Vector3.one;
			}
			else
			{
				pCrucible.Freeze(inFreeze: false);
				mIceSetTimer = 0f;
			}
		}
	}

	public void BreatheElectricity(bool inBreathe)
	{
		if (inBreathe && !_MainUI.pElectricFlow)
		{
			PlayDragonAnim(_BreatheAnim, inPlayOnce: true);
		}
	}

	public void AddWater()
	{
		if (pCrucible != null)
		{
			pCrucible.AddWater(LabData.pInstance.GetItem("Water"));
		}
	}

	public void AddAcidity(int inUnit)
	{
		if (pCrucible is LabTitrationCrucible labTitrationCrucible)
		{
			labTitrationCrucible.AddAcidity(inUnit);
		}
		if (inUnit < 0)
		{
			if (_AcidStream != null)
			{
				ParticleSystem.EmissionModule emission = _AcidStream.emission;
				emission.enabled = true;
			}
			mAcidTitrationTimer += _TitrationMixTime * (float)Mathf.Abs(inUnit);
		}
		else
		{
			if (_BaseStream != null)
			{
				ParticleSystem.EmissionModule emission2 = _BaseStream.emission;
				emission2.enabled = true;
			}
			mBaseTitrationTimer += _TitrationMixTime * (float)inUnit;
		}
	}

	public void OnWaterLoaded(LabItem inLabItem, GameObject inGameObj, LabItem inParent)
	{
		_MainUI.ActivateCursor(UiScienceExperiment.Cursor.DEFAULT);
		mWaterLabItem = inLabItem;
		mWaterGameObject = inGameObj;
		mWaterObjectParent = inParent;
		EnableUI(inEnable: true);
		if (_WaterPull != null)
		{
			_WaterPull.Play();
		}
		PlayDragonAnim(_WaterPullChainAnim, inPlayOnce: true);
	}

	private void OnClick(GameObject inGameObject)
	{
		if (pExperimentType != ExperimentType.TITRATION_LAB)
		{
			EnableClickOnPullDown(isEnable: false);
			AddMagnet();
		}
	}

	public void AddMagnet()
	{
		if (_WaterPull != null)
		{
			_WaterPull.Play();
		}
		PlayDragonAnim(_WaterPullChainAnim, inPlayOnce: true);
	}

	public void Reset()
	{
		if (_MagnetGameObject != null)
		{
			_MagnetGameObject.transform.position = mMagnetOrgPos;
		}
	}

	public void Pestle()
	{
		if (pCrucible != null)
		{
			pCrucible.Mix();
		}
	}

	public void EnableClickOnPullDown(bool isEnable)
	{
		if (_RopePullGameObject != null)
		{
			ObClickable component = _RopePullGameObject.GetComponent<ObClickable>();
			component.enabled = true;
			component._Active = isEnable;
		}
	}

	public void OnExperimentTaskDone(LabTask inExpTask)
	{
		if (inExpTask == null)
		{
			return;
		}
		if (_MainUI != null)
		{
			_MainUI.OnExperimentTaskDone(inExpTask);
		}
		if (pExperiment != null)
		{
			if (pExperiment.AreAllTasksDone())
			{
				SnChannel.Play(_ExperimentCompleteSFX, "SFX_Pool", inForce: true, null);
				_MainUI.OnExperimentCompleted();
				mCurrentDragon.UpdateActionMeters(PetActions.LAB, 1f, doUpdateSkill: true);
			}
			else
			{
				SnChannel.Play(_TaskCompletedSFX, "SFX_Pool", inForce: true, null);
			}
		}
		if (inExpTask.StopExciteOnRecordingInJournal)
		{
			if (mCurrentDragon.pAnimToPlay == "LabExcited")
			{
				StopDragonAnim();
			}
			SnChannel.StopPool("Default_Pool2");
		}
		if (pUseExperimentCheat || MissionManager.pInstance == null || !(MissionManager.pInstance != null))
		{
			return;
		}
		List<Task> tasks = MissionManager.pInstance.GetTasks("Action", "Name", inExpTask.Action);
		if (tasks != null)
		{
			foreach (Task item in tasks)
			{
				item.pPayload.Set("Lab_Result", inExpTask.ResultText._Text);
				item.pPayload.Set("Lab_Result_ID", inExpTask.ResultText._ID.ToString());
			}
		}
		MissionManager.pInstance.CheckForTaskCompletion("Action", inExpTask.Action);
		Mission mission = MissionManager.pInstance.GetMission(pExperiment.ID);
		if (pExperiment.AreAllTasksDone() && !string.IsNullOrEmpty(pExperiment.ResultImage) && mission != null)
		{
			object outPhase = null;
			RuleItemType outType = RuleItemType.Mission;
			if (MissionManagerDO.GetScientificQuestPhase(mission, RuleItemType.Mission, 2, out outPhase, out outType) && outPhase is Task task && task.Completed > 0)
			{
				task.pPayload.Set("Lab_Image_Url", pExperiment.ResultImage);
				task.Save(completed: false, null);
			}
		}
	}

	public static Experiment GetActiveExperiment()
	{
		if (MissionManager.pInstance == null)
		{
			return null;
		}
		List<Task> pActiveTasks = MissionManager.pInstance.pActiveTasks;
		if (pActiveTasks == null || pActiveTasks.Count == 0)
		{
			return null;
		}
		Experiment experiment = null;
		foreach (Task item in pActiveTasks)
		{
			if (item == null || item.pData == null || item.pData.Objectives == null)
			{
				continue;
			}
			Experiment[] experiments = LabData.pInstance.Experiments;
			foreach (Experiment experiment2 in experiments)
			{
				if (experiment2 == null || experiment2.Tasks == null || experiment2.Tasks.Length == 0)
				{
					continue;
				}
				LabTask[] tasks = experiment2.Tasks;
				foreach (LabTask labTask in tasks)
				{
					if (labTask == null || string.IsNullOrEmpty(labTask.Action))
					{
						continue;
					}
					foreach (TaskObjective objective in item.pData.Objectives)
					{
						if (objective == null)
						{
							continue;
						}
						string text = objective.Get<string>("Name");
						if (!string.IsNullOrEmpty(text) && labTask.Action == text)
						{
							Experiment labExperimentByID = LabData.pInstance.GetLabExperimentByID(item._Mission.MissionID);
							if (experiment == null || experiment.Priority > labExperimentByID.Priority)
							{
								experiment = labExperimentByID;
							}
						}
					}
				}
			}
		}
		return experiment;
	}

	private void OnNoActiveQuestOk()
	{
		Exit();
	}

	public void Exit()
	{
		mExiting = true;
		mCurrentDragon.AIActor.SetState(AISanctuaryPetFSM.NORMAL);
		pUseExperimentCheat = false;
		StopDragonAnim();
		_MainUI.SetVisibility(inVisible: false);
		_MainUI._ExperimentItemMenu.SetVisibility(inVisible: false);
		if (!string.IsNullOrEmpty(_ExitMarker))
		{
			AvAvatar.pStartLocation = _ExitMarker;
		}
		RsResourceManager.LoadLevel(mLastScene);
		if (mDragonFlameParticle != null)
		{
			UtDebug.Log("Lab: Destroying the particle " + mDragonFlameParticle.name, 8u);
			mDragonFlameParticle.transform.parent = null;
			UnityEngine.Object.Destroy(mDragonFlameParticle);
		}
		if (mCrucible != null)
		{
			mCrucible.OnExit();
		}
		LabData.pInstance = null;
		if (MainStreetMMOClient.pInstance != null && MainStreetMMOClient.pInstance.pAllDeactivated)
		{
			MainStreetMMOClient.pInstance.ActivateAll(active: true);
		}
	}

	private void SyncTasksWithMissionData()
	{
		if (mExperiment == null || MissionManager.pInstance == null || !MissionManager.pIsReady)
		{
			return;
		}
		MissionManager.pInstance.SetTimedTaskUpdate(inState: true, inForceUpdate: true);
		Mission mission = MissionManager.pInstance.GetMission(mExperiment.ID);
		if (mission == null || mission.Tasks == null || mission.Tasks.Count == 0)
		{
			return;
		}
		LabTask[] tasks = mExperiment.Tasks;
		foreach (LabTask labTask in tasks)
		{
			if (labTask != null)
			{
				if (pUseExperimentCheat)
				{
					labTask.pDone = false;
				}
				else if (!MissionManager.IsTaskActive("Action", "Name", labTask.Action))
				{
					labTask.pDone = true;
				}
			}
		}
	}

	public static LabTool GetMappedLabTool(string inTool)
	{
		if (string.IsNullOrEmpty(inTool))
		{
			return LabTool.NONE;
		}
		return inTool switch
		{
			"CLOCK" => LabTool.CLOCK, 
			"THERMOMETER" => LabTool.THERMOMETER, 
			"WEIGHINGMACHINE" => LabTool.WEIGHINGMACHINE, 
			"OHMMETER" => LabTool.OHMMETER, 
			_ => LabTool.NONE, 
		};
	}

	public void EnableUI(bool inEnable)
	{
		if (_MainUI != null)
		{
			_MainUI.SetInteractive(inEnable);
			if (_MainUI._ExperimentItemMenu != null)
			{
				_MainUI._ExperimentItemMenu.SetInteractive(inEnable);
			}
		}
	}

	private void UpdateAnimTimer()
	{
		if (mAnimTimer != -999f)
		{
			mAnimTimer -= Time.deltaTime;
			if (mAnimTimer <= 0f)
			{
				StopDragonAnim();
			}
		}
	}

	public void StopDragonAnim()
	{
		mAnimTimer = -999f;
		if (mCurrentDragon != null)
		{
			AvAvatarAnimEvent avAvatarAnimEvent = new AvAvatarAnimEvent();
			avAvatarAnimEvent._Animation = mCurrentDragon.pAnimToPlay;
			avAvatarAnimEvent.mData = new AnimData();
			avAvatarAnimEvent.mData._DataString = "Finished";
			OnAnimEvent(avAvatarAnimEvent);
			mCurrentDragon.pAnimToPlay = _IdleAnim;
			mDragonLookAtMouse = true;
		}
	}

	public float GetAnimLength(string inAnimName)
	{
		if (mCurrentDragon != null && !string.IsNullOrEmpty(inAnimName) && mCurrentDragon.animation != null && mCurrentDragon.animation[inAnimName] != null)
		{
			return mCurrentDragon.animation[inAnimName].length;
		}
		return -1f;
	}

	public bool PlayDragonAnim(string inAnimName, bool inPlayOnce = false, bool playIdleNext = true, float animSpeed = 1f, Transform lookAtObject = null)
	{
		if (mCurrentDragon != null && !string.IsNullOrEmpty(inAnimName) && mCurrentDragon.animation != null && mCurrentDragon.animation[inAnimName] != null && mCurrentDragon.pAnimToPlay != inAnimName && !pWaitingForAnimEvent)
		{
			StopDragonAnim();
			if (inPlayOnce)
			{
				mCurrentDragon.animation[inAnimName].time = 0f;
				if (playIdleNext)
				{
					mAnimTimer = mCurrentDragon.animation[inAnimName].length;
				}
			}
			mCurrentDragon.animation[inAnimName].speed = animSpeed;
			mCurrentDragon.pAnimToPlay = inAnimName;
			mDragonLookAtMouse = false;
			mCurrentDragon.SetLookAtObject(lookAtObject, tween: true, Vector3.zero);
			mCurrentDragon.SendMessage("StartAnimTrigger", inAnimName, SendMessageOptions.DontRequireReceiver);
			return true;
		}
		return false;
	}

	public bool PlayDragonAnim(string inAnimName, Transform lookAtObject, bool inPlayOnce = false)
	{
		return PlayDragonAnim(inAnimName, inPlayOnce, playIdleNext: true, 1f, lookAtObject);
	}

	private LabDragonData GetDragonData(string inDragonTypeName)
	{
		LabDragonData[] dragonData = _DragonData;
		foreach (LabDragonData labDragonData in dragonData)
		{
			if (labDragonData != null && labDragonData._Name == inDragonTypeName)
			{
				return labDragonData;
			}
		}
		return null;
	}

	private void OnBreathFireParticleDownloaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			Transform transform = UtUtilities.FindChildTransform(mCurrentDragon.gameObject, pDragonAgeData._Bone);
			if (transform != null)
			{
				mDragonFlameParticle = UnityEngine.Object.Instantiate((GameObject)inObject);
				UtDebug.Log("Lab: Instantiated particle " + inURL, 8u);
				PlayParticle(mDragonFlameParticle, inPlay: false);
				mDragonFlameParticle.transform.parent = transform;
				mDragonFlameParticle.transform.localPosition = pDragonAgeData._OffsetPos;
				mDragonFlameParticle.transform.localRotation = Quaternion.Euler(pDragonAgeData._OffsetRotation);
			}
			else
			{
				UtDebug.Log("Lab: The parent is null " + pDragonAgeData._Bone, 8u);
			}
			mBreathParticleLoaded = true;
			break;
		}
		case RsResourceLoadEvent.ERROR:
			UtDebug.Log("Lab: the particle bundle loaded error " + inURL, 8u);
			mBreathParticleLoaded = true;
			break;
		}
	}

	private void PlayParticle(GameObject inObj, bool inPlay)
	{
		UtDebug.Log("Lab: Play particle ", 8u);
		if (inObj == null)
		{
			UtDebug.Log("Lab: Error!! Unable to play the particle ", 8u);
			return;
		}
		ParticleSystem component = inObj.GetComponent<ParticleSystem>();
		if (component != null)
		{
			UtDebug.Log("Lab: Emitting particle " + inObj.name + " - " + inPlay, 8u);
			mFireAtTarget = inPlay;
			if (inPlay)
			{
				component.Play(withChildren: true);
			}
			else
			{
				component.Stop(withChildren: true);
			}
		}
	}

	private void OnAnimEvent(AvAvatarAnimEvent inEvent)
	{
		if (inEvent == null || inEvent.mData == null)
		{
			return;
		}
		switch (inEvent.mData._DataString)
		{
		case "BlowFire":
			if (SECrucibleContextSensitive.pInstance != null && SECrucibleContextSensitive.pInstance.pUI != null)
			{
				SECrucibleContextSensitive.pInstance.pUI.DisableWidget("Heat", disable: true);
			}
			PlayParticle(mDragonFlameParticle, inPlay: true);
			pCrucible.Heat();
			SnChannel.Play(_DragonFireSFX, "Default_Pool3", inForce: true, null).pLoop = false;
			break;
		case "StopFire":
			if (mDragonFlameParticle != null)
			{
				PlayParticle(mDragonFlameParticle, inPlay: false);
			}
			pCrucible.StopHeat();
			break;
		case "Finished":
			if (inEvent._Animation == "LabBlowFire" && SECrucibleContextSensitive.pInstance != null && SECrucibleContextSensitive.pInstance.pUI != null)
			{
				SECrucibleContextSensitive.pInstance.pUI.DisableWidget("Heat", disable: false);
			}
			break;
		case "BreatheElectricity":
			PlayParticle(mDragonFlameParticle, inPlay: true);
			SnChannel.Play(_SkrillBreatheSFX, "Default_Pool3", inForce: true, null).pLoop = false;
			_MainUI.pElectricFlow = true;
			break;
		case "StopElectricity":
			if (mDragonFlameParticle != null)
			{
				PlayParticle(mDragonFlameParticle, inPlay: false);
			}
			_MainUI.pElectricFlow = false;
			break;
		case "PourWater":
			if (pExperimentType == ExperimentType.MAGNETISM_LAB)
			{
				Vector3 pos = mMagnetOrgPos + _MagnetTargetOffset;
				TweenPosition tweenPosition = TweenPosition.Begin(_MagnetGameObject.gameObject, _MagnetMoveTime, pos);
				tweenPosition.eventReceiver = base.gameObject;
				tweenPosition.callWhenFinished = "MagnetTweenDone";
			}
			else if (_WaterStream != null)
			{
				_WaterStream.Play();
				if (_AddWaterSFX != null)
				{
					SnChannel.Play(_AddWaterSFX, "Default_Pool4", inForce: true, null);
				}
				if (_WaterSplashSteam != null)
				{
					_WaterSplashSteam.Play();
				}
				pCrucible.AddWaterReal(mWaterLabItem, mWaterGameObject, mWaterObjectParent);
			}
			break;
		case "Positioned":
			mDragonPositioned = true;
			break;
		}
	}

	private void MagnetTweenDone()
	{
		if (mCrucible.pTestItems == null)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < mCrucible.pTestItems.Count; i++)
		{
			if (IsMagneticObject(mCrucible.pTestItems[i]))
			{
				mCrucible.pTestItems[i].gameObject.GetComponent<PendulumEffect>().enabled = true;
				flag = true;
			}
		}
		if (flag)
		{
			Animation component = _MagnetGameObject.GetComponent<Animation>();
			component.Play("Attract");
			Invoke("OnAttractAnimEnd", component.clip.length / 2f);
		}
		else
		{
			SetMagnetTutorialComplete();
			mMagnetActivated = true;
		}
	}

	private void OnAttractAnimEnd()
	{
		for (int i = 0; i < mCrucible.pTestItems.Count; i++)
		{
			LabTestObject labTestObject = mCrucible.pTestItems[i];
			if (IsMagneticObject(labTestObject))
			{
				labTestObject.gameObject.GetComponent<PendulumEffect>().enabled = false;
				AttractableObject component = labTestObject.GetComponent<AttractableObject>();
				component._AttractiveGameObj = _MagnetAttachmentNode;
				component._OnAttractiveObjHit = OnTargetObjectHit;
				component.enabled = true;
			}
		}
	}

	private void OnTargetObjectHit(GameObject inSourceObject, GameObject inTargetObject)
	{
		if (inTargetObject.gameObject.name == "AttractiveNode")
		{
			SetMagnetTutorialComplete();
			mMagnetActivated = true;
		}
	}

	private void SetMagnetTutorialComplete()
	{
		if (mCurrPlayingTutorial != null)
		{
			mCurrPlayingTutorial.TutorialManagerAsyncMessage("ObjectToMagnetTutComplete");
		}
	}

	private bool IsMagneticObject(LabTestObject inLabTestObject)
	{
		string propertyValueForKey = inLabTestObject.pTestItem.GetPropertyValueForKey("IsMagnetic");
		if (!string.IsNullOrEmpty(propertyValueForKey) && bool.TryParse(propertyValueForKey, out var result) && result)
		{
			return true;
		}
		return false;
	}

	private void CreateDragon(int inPetTypeID, RaisedPetStage inStage, Gender inGender)
	{
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(inPetTypeID);
		string empty = string.Empty;
		int ageIndex = RaisedPetData.GetAgeIndex(inStage);
		empty = ((sanctuaryPetTypeInfo._AgeData[ageIndex]._PetResList[0]._Gender != inGender) ? sanctuaryPetTypeInfo._AgeData[ageIndex]._PetResList[1]._Prefab : sanctuaryPetTypeInfo._AgeData[ageIndex]._PetResList[0]._Prefab);
		RaisedPetData pdata = RaisedPetData.InitDefault(inPetTypeID, inStage, empty, inGender, addToActivePets: false);
		Transform dragonMarker = GetDragonMarker(sanctuaryPetTypeInfo._Name, sanctuaryPetTypeInfo._AgeData[ageIndex]._Name);
		if (dragonMarker == null)
		{
			SanctuaryManager.CreatePet(pdata, Vector3.zero, Quaternion.identity, base.gameObject, "Full");
		}
		else
		{
			SanctuaryManager.CreatePet(pdata, dragonMarker.position, dragonMarker.rotation, base.gameObject, "Full");
		}
	}

	public void OnPetReady(SanctuaryPet pet)
	{
		if (pet != null)
		{
			SetCurrentDragon(pet);
		}
	}

	private Transform GetDragonMarker(string inTypeName, string inAge, bool getIdlePetMarker = false)
	{
		LabDragonData[] dragonData = _DragonData;
		foreach (LabDragonData labDragonData in dragonData)
		{
			if (!(labDragonData._Name == inTypeName))
			{
				continue;
			}
			LabDragonAgeData[] ageData = labDragonData._AgeData;
			foreach (LabDragonAgeData labDragonAgeData in ageData)
			{
				if (!(labDragonAgeData._Name == inAge))
				{
					continue;
				}
				if (getIdlePetMarker)
				{
					return labDragonAgeData._IdlePetMarker;
				}
				MarkerMap[] markerMap = labDragonAgeData._MarkerMap;
				foreach (MarkerMap markerMap2 in markerMap)
				{
					if (markerMap2._Experiment == pExperimentType && markerMap2._Marker != null)
					{
						return markerMap2._Marker;
					}
				}
				return labDragonAgeData._Marker;
			}
			break;
		}
		return null;
	}

	private void InitBreatheAtTargetInfo()
	{
		BreatheInfo[] breatheInfo = _BreatheInfo;
		foreach (BreatheInfo breatheInfo2 in breatheInfo)
		{
			if (breatheInfo2._Experiment == pExperimentType)
			{
				if (breatheInfo2._MarkerDragonHeadTarget != null)
				{
					mDragonHeadTargetMarker = breatheInfo2._MarkerDragonHeadTarget;
				}
				if (breatheInfo2._MarkerBreatheTarget != null)
				{
					mBreatheTargetMarker = breatheInfo2._MarkerBreatheTarget;
				}
				if (breatheInfo2._BreatheAnimation != null)
				{
					_BreatheAnim = breatheInfo2._BreatheAnimation;
				}
			}
		}
	}

	public LabSubstanceMap GetProceduralMaterial(string inName)
	{
		if (_SubstanceMap == null || _SubstanceMap.Length == 0 || string.IsNullOrEmpty(inName))
		{
			return null;
		}
		LabSubstanceMap[] substanceMap = _SubstanceMap;
		foreach (LabSubstanceMap labSubstanceMap in substanceMap)
		{
			if (labSubstanceMap != null && labSubstanceMap._TestItemName == inName)
			{
				return labSubstanceMap;
			}
		}
		return null;
	}

	private float GetHeatTime()
	{
		if (mCurrentDragon == null)
		{
			return 1.5f;
		}
		LabDragonAnimEvents component = mCurrentDragon.GetComponent<LabDragonAnimEvents>();
		if (component == null)
		{
			return 1.5f;
		}
		float num = -1f;
		float num2 = -1f;
		AvAvatarAnimEvent[] events = component._Events;
		foreach (AvAvatarAnimEvent avAvatarAnimEvent in events)
		{
			if (avAvatarAnimEvent == null || avAvatarAnimEvent._Animation != _BreatheAnim || avAvatarAnimEvent._Times == null || avAvatarAnimEvent._Times.Length < 2)
			{
				continue;
			}
			AnimData[] times = avAvatarAnimEvent._Times;
			foreach (AnimData animData in times)
			{
				if (animData == null)
				{
					continue;
				}
				if (animData._DataString == "BlowFire")
				{
					num = animData._Time;
				}
				else if (animData._DataString == "StopFire")
				{
					num2 = animData._Time;
				}
				if (num != -1f && num2 != -1f)
				{
					float num3 = num2 - num;
					if (!(num3 < 0f))
					{
						return num3;
					}
					return 1.5f;
				}
			}
			return 1.5f;
		}
		return 1.5f;
	}

	private void SetCurrentDragon(SanctuaryPet inDragon)
	{
		mCurrentDragon = inDragon;
		if (!(mCurrentDragon != null))
		{
			return;
		}
		if (_PetSkinMapping != null)
		{
			for (int i = 0; i < _PetSkinMapping.Count; i++)
			{
				if (mCurrentDragon.GetTypeInfo()._TypeID == _PetSkinMapping[i]._PetTypeID)
				{
					mCurrentDragon.SetAccessory(RaisedPetAccType.Materials, _PetSkinMapping[i]._Skin.gameObject, null);
					break;
				}
			}
		}
		if (mCurrentDragon._PlayMoodParticleInLab)
		{
			mCurrentDragon.PlayPetMoodParticle(SanctuaryPetMeterType.HAPPINESS, isForcePlay: false);
		}
		if (mCurrentDragon.collider != null)
		{
			mCurrentDragon.collider.enabled = false;
		}
		if (_DragonMarkers != null)
		{
			Transform dragonMarker = GetDragonMarker(mCurrentDragon.pTypeInfo._Name, mCurrentDragon.pCurAgeData._Name);
			if (dragonMarker != null)
			{
				mCurrentDragon.SetPosition(dragonMarker.position);
				mCurrentDragon.transform.rotation = dragonMarker.rotation;
			}
			else
			{
				mCurrentDragon.SetPosition(Vector3.zero);
				mCurrentDragon.transform.rotation = Quaternion.identity;
			}
			mCurrentDragon.transform.localScale = Vector3.one * mCurrentDragon.pCurAgeData._LabScale;
		}
		mCurrentDragon.SetState(Character_State.unknown);
		if (mCurrentDragon.AIActor != null)
		{
			mCurrentDragon.AIActor.SetState(AISanctuaryPetFSM.SCIENCE_LAB);
		}
	}

	public bool CheckForProcedureHalt(string inName, string inValue)
	{
		if (pTimeEnabled && pTimerActivatedTask != null && pTimerActivatedTask.NeedHalt(inName, inValue))
		{
			_MainUI.ShowUserPromptText(_ProcedureHalted.GetLocalizedString(), inClickable: true, null, inShowCloseBtn: false, inSetTimerInteractive: true, null);
			pTimerActivatedTask = null;
			return true;
		}
		return false;
	}

	public static bool IsSolid(LabItemCategory inCategory)
	{
		if (inCategory != 0 && inCategory != LabItemCategory.SOLID_COMBUSTIBLE)
		{
			return inCategory == LabItemCategory.SOLID_POWDER;
		}
		return true;
	}

	private void OnDestroy()
	{
		_SubstanceMap = null;
		if (SanctuaryManager.pInstance != null)
		{
			SanctuaryManager.pInstance.pDisablePetSwitch = false;
		}
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: true);
		}
	}

	public void OpenJournal(bool open)
	{
		if ((bool)_Gronckle && pExperimentType == ExperimentType.GRONCKLE_IRON && _Gronckle.pBellyItemCount > 0)
		{
			_Gronckle.SetBellyPopup(!open);
		}
	}

	public void ShowRemoveFx(Transform pivot)
	{
		if (!(_RemoveTestItemFx == null) && !(pivot == null))
		{
			ParticleSystem particleSystem = UnityEngine.Object.Instantiate(_RemoveTestItemFx, pivot.position, pivot.rotation);
			if (particleSystem != null)
			{
				particleSystem.Play();
				UnityEngine.Object.Destroy(particleSystem.gameObject, particleSystem.main.startDelay.constant + particleSystem.main.duration);
			}
		}
	}
}
