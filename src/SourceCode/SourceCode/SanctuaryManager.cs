using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SanctuaryManager : MonoBehaviour
{
	public delegate void PetChanged(SanctuaryPet pet);

	public const int MAT_IDX_BODY = 0;

	public const int MAT_IDX_PATTERN = 0;

	public const int MAT_IDX_PATTERN_LUMINOSITY = 0;

	public const int MAT_IDX_SPIKES = 0;

	public const string ATTR_NAME_BODY_COLOR = "_PetBodyColor";

	public const string ATTR_NAME_PATTERN_COLOR = "_PetPatternColor";

	public const string ATTR_NAME_PATTERN_LUMINOSITY = "_PetPatternLuminosity";

	public const string ATTR_NAME_SPIKES_COLOR = "_PetSpikesColor";

	public const string COLOR_PARAM_BODY = "Color_Skin";

	public const string COLOR_PARAM_PATTERN = "Color_Belly";

	public const string PARAM_PATTERN_LUMINOSITY = "Hue_Belly";

	public const string COLOR_PARAM_SPIKES = "";

	public const string PARAM_PATTERN_TEXTURE = "_MainTex";

	public static string pPetStartMarkerName = "";

	public static SanctuaryManager pInstance = null;

	public static bool pCheckPetAge = false;

	public static bool pCheatGrowth = false;

	public static int pCurrentPetType = -1;

	public static SanctuaryPet pCurPetInstance = null;

	public static SanctuaryPet pPrevPetInstance = null;

	public static bool pRequestSkillSync = false;

	private static RaisedPetData mCurPetData = null;

	public static RaisedPetData mUnselectedPet = null;

	public static bool pLevelReady = false;

	public static int pNextRaisedPetIndex = 0;

	public static string pLevelName = "";

	public static bool pPendingMMOPetCheck = false;

	private static bool mIsReady = false;

	public bool _PoolingEnabled;

	private static bool mAchievementInfoReady = false;

	private static bool mLowStatWarningShown;

	public static bool mMountedState = false;

	public string _SanctuaryDataURL = "RS_DATA/PfSanctuaryData.unity3d/PfSanctuaryData";

	public bool _MeterPaused;

	public bool _CreateInstance = true;

	public Transform _PetStartMarker;

	public string _PetNameSelectBundlePath = "RS_DATA/PfUiDragonNameDO.unity3d/PfUiDragonNameDO";

	public string _PetCustomizationBundlePath = "RS_DATA/PfUiDragonCustomizationDO.unity3d/PfUiDragonCustomizationDO";

	public PetNameChange _PetNameChange;

	public Transform _PetSleepMarkerMale;

	public Transform _PetSleepMarkerFemale;

	public bool _AttachPetToBed = true;

	[HideInInspector]
	public UiPetMeter pPetMeter;

	public GameObject _PetClickActivateObject;

	public GameObject _NpcPetClickActivateObject;

	public GameObject _PetFlyUIObject;

	public bool _FollowAvatar = true;

	public Transform _PetZZZParticle;

	public Transform _PetSwimFx;

	public GameObject _PetSwimIdleFx;

	public Vector3 _PetOffScreenPosition = new Vector3(0f, -1000f, 0f);

	public bool _CanCheckPetAge;

	public bool _MultiPets;

	public bool _UseMeterAutoSaving;

	public float _MeterAutoSaveFrequency = 60f;

	public Vector3 _PicturePositionOffset = new Vector3(0f, 3f, 3f);

	public Vector3 _PictureLookAtOffset = new Vector3(0.05f, 1.5f, 0f);

	public int[] _NeedEnergyMeterTutorialTask;

	public int _TeenMission = 1044;

	public InteractiveTutManager _MountTutorial;

	public string _MountTutorialRes = "RS_DATA/PfMountInteractiveTut.unity3d/PfMountInteractiveTut";

	public int _PetCollectItemAchievementID = 201712;

	public int _FirePitFireAchievementID = 201711;

	public RaisedPetStage[] _BlockedStages;

	public RaisedPetStage[] _NurseryAges;

	public bool _NoHat;

	public bool _ForceLoadPetData;

	public AudioClip _LowEnergyVO;

	public LocaleString _LowEnergyText = new LocaleString("Your pet is tired");

	public AudioClip _LowHappinessVO;

	public LocaleString _LowHappinessText = new LocaleString("Your pet is angry");

	public PetNameSelectionDoneEvent OnNameSelectionDone;

	public int _DragonChampionAchievementID = 628;

	public int _DragonChampionAchievementLevel = 20;

	public int _DragonTitanAchievemetID = 2102;

	public LocaleString _CreatePetFailureText = new LocaleString("Create pet failed.");

	public string[] _MountStateIgnoreScenes;

	public string[] _FullAnimLevels;

	public string _PetMeterPrefab = "PfUiPetMeterDO";

	private bool mLoadData;

	private bool mAllPetLoaded;

	private KAUIGenericDB mKAUIGenericDB;

	private bool mSetFollowAvatar;

	private GameObject mTakePictureReceiver;

	private bool mCreateInstance;

	private bool mDisablePetSwitch;

	private bool mPetNeedFinalize;

	private bool mDlgUp;

	private bool mSanctuaryDataReady;

	private UiToolbar mToolbar;

	private bool mToolbarInformed = true;

	private bool mPetPictureInitialized;

	private GameObject mReloadPetMsgObj;

	private Dictionary<int, WeaponRechargeData> mPetWeaponRechargeMap = new Dictionary<int, WeaponRechargeData>();

	public static RaisedPetData pCurPetData
	{
		get
		{
			return mCurPetData;
		}
		set
		{
			if (mCurPetData != value)
			{
				mCurPetData = value;
				if (MissionManager.pInstance != null)
				{
					MissionManager.pInstance.RefreshMissions();
				}
			}
		}
	}

	public static bool pIsReady
	{
		get
		{
			if (!mIsReady)
			{
				return pInstance == null;
			}
			return true;
		}
	}

	public static bool pMountedState
	{
		get
		{
			return mMountedState;
		}
		set
		{
			mMountedState = value;
		}
	}

	public bool pSetFollowAvatar
	{
		get
		{
			return mSetFollowAvatar;
		}
		set
		{
			mSetFollowAvatar = value;
		}
	}

	public bool pCreateInstance
	{
		get
		{
			return mCreateInstance;
		}
		set
		{
			mCreateInstance = value;
		}
	}

	public bool pDisablePetSwitch
	{
		get
		{
			if (!mDisablePetSwitch)
			{
				if (pCurPetInstance != null)
				{
					return pCurPetInstance.pIsFlying;
				}
				return false;
			}
			return true;
		}
		set
		{
			mDisablePetSwitch = value;
		}
	}

	public event PetChanged OnPetChanged;

	public static void SetAndSaveCurrentType(int typeID)
	{
		pCurrentPetType = typeID;
	}

	public static bool IsActivePetDataReady()
	{
		if (pCurrentPetType < 0)
		{
			return false;
		}
		return RaisedPetData.IsActivePetDataReady(pCurrentPetType);
	}

	public void ResetData()
	{
		pCurPetInstance = null;
		if (pCurPetData != null)
		{
			if (!_ForceLoadPetData && pCurrentPetType == pCurPetData.PetTypeID)
			{
				pCurPetData = RaisedPetData.GetCurrentInstance(pCurrentPetType);
			}
			else
			{
				mLoadData = true;
			}
		}
		else
		{
			mLoadData = true;
		}
	}

	private void Awake()
	{
		mAllPetLoaded = false;
		pLevelName = RsResourceManager.pCurrentLevel;
	}

	private void OnSanctuaryDataEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			string[] array = _SanctuaryDataURL.Split('/');
			RsResourceManager.SetDontDestroy(array[0] + "/" + array[1], inDontDestroy: true);
			mSanctuaryDataReady = true;
			UnityEngine.Object.Instantiate((GameObject)inObject);
		}
	}

	private void Start()
	{
		if (_CanCheckPetAge)
		{
			pCheckPetAge = true;
		}
		pLevelReady = false;
		mAllPetLoaded = false;
		if (pInstance == null)
		{
			pInstance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			UserInfo.Init();
			mLoadData = true;
			ImageData.Init("EggColor", 512);
			if (SanctuaryData.pInstance == null && _SanctuaryDataURL.Length > 0)
			{
				string[] array = _SanctuaryDataURL.Split('/');
				if (array.Length == 3)
				{
					RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnSanctuaryDataEventHandler, typeof(GameObject));
				}
			}
			else
			{
				mSanctuaryDataReady = true;
			}
		}
		else
		{
			if (null != pCurPetInstance)
			{
				pCurPetInstance.OnFlyDismountImmediate(AvAvatar.pObject);
				AvAvatar.pSubState = AvAvatarSubState.NORMAL;
				UnityEngine.Object.Destroy(pCurPetInstance.gameObject);
			}
			if (!_CreateInstance && AvAvatar.pObject != null)
			{
				AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
				if (component != null)
				{
					component.SoftReset();
				}
			}
			pInstance.ResetData();
			pInstance._MeterPaused = _MeterPaused;
			pInstance._CreateInstance = _CreateInstance;
			pInstance._PetStartMarker = _PetStartMarker;
			pInstance._PetClickActivateObject = _PetClickActivateObject;
			pInstance._NpcPetClickActivateObject = _NpcPetClickActivateObject;
			pInstance._PetFlyUIObject = _PetFlyUIObject;
			pInstance._FollowAvatar = _FollowAvatar;
			pInstance._CanCheckPetAge = _CanCheckPetAge;
			pInstance._PetSleepMarkerMale = _PetSleepMarkerMale;
			pInstance._PetSleepMarkerFemale = _PetSleepMarkerFemale;
			pInstance._BlockedStages = _BlockedStages;
			pInstance._NurseryAges = _NurseryAges;
			pInstance._NoHat = _NoHat;
			pInstance._UseMeterAutoSaving = _UseMeterAutoSaving;
			pInstance._MeterAutoSaveFrequency = _MeterAutoSaveFrequency;
			pInstance._MountTutorial = _MountTutorial;
			pInstance.pPetMeter = null;
			pInstance.mSetFollowAvatar = false;
			UnityEngine.Object.Destroy(base.gameObject);
		}
		MissionManager.AddMissionEventHandler(pInstance.OnMissionEvent);
		pInstance.mCreateInstance = pInstance._CreateInstance;
	}

	public void ReloadPet(bool resetFollowFlag = false, GameObject msgObj = null)
	{
		if (resetFollowFlag)
		{
			mSetFollowAvatar = false;
		}
		ResetData();
		UserInfo.Init();
		mLoadData = true;
		if (SanctuaryData.pInstance == null && _SanctuaryDataURL.Length > 0)
		{
			string[] array = _SanctuaryDataURL.Split('/');
			if (array.Length == 3)
			{
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnSanctuaryDataEventHandler, typeof(GameObject));
			}
		}
		else
		{
			mSanctuaryDataReady = true;
		}
		pLevelName = RsResourceManager.pCurrentLevel;
		mCreateInstance = true;
		mReloadPetMsgObj = msgObj;
	}

	public static SanctuaryPetItemData CreatePet(string userID, RaisedPetData pdata, Vector3 pos, Quaternion rot, GameObject msgObj, string animSet, bool applyCustomSkin = false)
	{
		bool poolDragons = pInstance != null && pInstance._PoolingEnabled;
		SanctuaryPetItemData sanctuaryPetItemData = new SanctuaryPetItemData(userID, pdata, pos, rot, msgObj, animSet, applyCustomSkin, poolDragons);
		sanctuaryPetItemData.LoadResource();
		return sanctuaryPetItemData;
	}

	public static SanctuaryPetItemData CreatePet(RaisedPetData pdata, Vector3 pos, Quaternion rot, GameObject msgObj, string animSet, bool applyCustomSkin = false)
	{
		string userID = string.Empty;
		if (UserInfo.pInstance != null)
		{
			userID = UserInfo.pInstance.UserID;
		}
		bool poolDragons = pInstance != null && pInstance._PoolingEnabled;
		SanctuaryPetItemData sanctuaryPetItemData = new SanctuaryPetItemData(userID, pdata, pos, rot, msgObj, animSet, applyCustomSkin, poolDragons);
		sanctuaryPetItemData.LoadResource();
		return sanctuaryPetItemData;
	}

	public static void StartHatching(RaisedPetData petData)
	{
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(petData.PetTypeID);
		if (sanctuaryPetTypeInfo != null)
		{
			pCurPetData.StartHatching(sanctuaryPetTypeInfo._HatchDuration);
		}
		else
		{
			Debug.LogError("Pet Type not found, hatching failed");
		}
	}

	public static void StartHatching(string petTypeName)
	{
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(petTypeName);
		if (sanctuaryPetTypeInfo != null)
		{
			pCurPetData.PetTypeID = sanctuaryPetTypeInfo._TypeID;
			pCurPetData.StartHatching(sanctuaryPetTypeInfo._HatchDuration);
		}
	}

	public static void StartHatching(RaisedPetData pData, int incubatorID, float hatchDuration = -1f)
	{
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(pData.PetTypeID);
		if (sanctuaryPetTypeInfo != null)
		{
			if (hatchDuration == -1f)
			{
				hatchDuration = sanctuaryPetTypeInfo._HatchDuration;
			}
			pData.StartHatching(hatchDuration, incubatorID);
		}
		else
		{
			Debug.LogError("Pet Type not found, hatching failed");
		}
	}

	private void FinalizeAllPet()
	{
		mPetNeedFinalize = false;
		if (!_MultiPets)
		{
			FinalizePet(pCurPetInstance);
			return;
		}
		RaisedPetData[] array = null;
		if (RaisedPetData.pActivePets.ContainsKey(pCurrentPetType))
		{
			array = RaisedPetData.pActivePets[pCurrentPetType];
		}
		if (array == null)
		{
			return;
		}
		RaisedPetData[] array2 = array;
		foreach (RaisedPetData raisedPetData in array2)
		{
			if (raisedPetData.pObject != null)
			{
				SanctuaryPet pet = (SanctuaryPet)raisedPetData.pObject.GetComponent(typeof(SanctuaryPet));
				FinalizePet(pet);
			}
		}
	}

	private void FinalizePet(SanctuaryPet pet)
	{
		if (pet.pData.pIsSleeping && pet.pData.pStage != RaisedPetStage.HATCHING)
		{
			if (_FollowAvatar)
			{
				pet.SetAvatar(AvAvatar.mTransform);
				pet.SetFollowAvatar(follow: true);
			}
			SendPetToBed(pet);
		}
		else if (_PetStartMarker != null)
		{
			pet.transform.position = _PetStartMarker.transform.position;
			pet.transform.rotation = _PetStartMarker.transform.rotation;
		}
	}

	public void EnableAllPets(bool enable, Transform marker = null)
	{
		pDisablePetSwitch = !enable;
		if (!(pCurPetInstance != null))
		{
			return;
		}
		if (!enable)
		{
			if (pCurPetInstance.pIsMounted || pMountedState)
			{
				pCurPetInstance.OnFlyDismount(AvAvatar.pObject);
			}
			if (marker != null)
			{
				pCurPetInstance.TeleportTo(marker.position, marker.rotation);
			}
			if (pCurPetInstance.AIActor != null)
			{
				AISanctuaryPetFSM state = (pCurPetInstance.IsSwimming() ? AISanctuaryPetFSM.IDLE : AISanctuaryPetFSM.REST);
				pCurPetInstance.AIActor.SetState(state);
			}
			MainStreetMMOClient.pInstance.SetRaisedPetString("");
		}
		else
		{
			MainStreetMMOClient.pInstance.SetRaisedPet(pCurPetInstance.pData, -1);
		}
		pCurPetInstance.mMountDisabled = !enable;
		pInstance._FollowAvatar = enable;
		pCurPetInstance.SetFollowAvatar(enable);
		pCurPetInstance.SetClickableActive(enable);
		if (AvAvatar.pSubState == AvAvatarSubState.FLYING)
		{
			KAInput.pInstance.EnableInputType("DragonBrake", InputType.ALL, enable);
			KAInput.pInstance.EnableInputType("WingFlap", InputType.ALL, enable);
		}
		else
		{
			KAInput.pInstance.EnableInputType("DragonMount", InputType.ALL, enable && pCurPetInstance.IsMountAllowed() && FUEManager.IsInputEnabled("DragonMount"));
		}
	}

	public void OnPetReady(SanctuaryPet pet)
	{
		if (pCurPetData == pet.pData || (mUnselectedPet != null && mUnselectedPet == pet.pData))
		{
			pCurPetInstance = pet;
			SceneManager.activeSceneChanged += pCurPetInstance.OnSceneChanged;
			pDisablePetSwitch = false;
			pet.PlayAnimation(pet._AnimNameIdle, WrapMode.Loop);
			SetWeaponRechargeData(pCurPetInstance);
			PetWeaponManager component = pCurPetInstance.gameObject.GetComponent<PetWeaponManager>();
			if (component != null)
			{
				component.pUserControlledWeapon = true;
			}
			if (!_MultiPets)
			{
				mAllPetLoaded = true;
			}
			if (MainStreetMMOClient.pInstance != null)
			{
				if (MainStreetMMOClient.pInstance.IsLevelMMO(RsResourceManager.pCurrentLevel))
				{
					if (MainStreetMMOClient.pInstance.pState == MMOClientState.IN_ROOM)
					{
						MainStreetMMOClient.pInstance.SetRaisedPet(pCurPetData, -1);
						pPendingMMOPetCheck = false;
					}
					else
					{
						pPendingMMOPetCheck = true;
					}
				}
				else
				{
					pPendingMMOPetCheck = false;
				}
			}
			else
			{
				pPendingMMOPetCheck = true;
			}
			if (this.OnPetChanged != null)
			{
				this.OnPetChanged(pet);
			}
		}
		if (pet != null)
		{
			if (_MultiPets)
			{
				Pet.pAvatarOffSetOffSetNow += 30f;
			}
			FinalizePet(pet);
			pet.SetClickActivateObject(_PetClickActivateObject);
			pet.SetFlyUIObject(_PetFlyUIObject);
			if (pCurPetInstance == pet)
			{
				pet.RegisterAvatarDamage();
				pet.pMeterPaused = _MeterPaused;
				pet._UseMeterAutoSaving = _UseMeterAutoSaving;
				pet._MeterAutoSaveFrequency = _MeterAutoSaveFrequency;
				if (!_MeterPaused)
				{
					InitPetMeter();
					pet.pMeterPaused = (_MeterPaused = pCurPetData.pIsSleeping);
				}
				else
				{
					pPetMeter = null;
				}
				pet.SetMeterUI(pPetMeter);
				pet.UpdateAllMeters();
				mToolbarInformed = false;
			}
			else
			{
				pet.pMeterPaused = true;
			}
			if (mTakePictureReceiver != null)
			{
				TakePicture(pet.gameObject, mTakePictureReceiver);
				mTakePictureReceiver = null;
			}
		}
		if (_MultiPets)
		{
			mAllPetLoaded = CheckAllPetsLoaded();
		}
		ApplyPetPartColors(pet);
		if (mAllPetLoaded)
		{
			mIsReady = true;
		}
		if (pCurPetInstance == pet && mReloadPetMsgObj != null)
		{
			mReloadPetMsgObj.SendMessage("OnPetReloaded", true, SendMessageOptions.DontRequireReceiver);
			mReloadPetMsgObj = null;
		}
		StableManager.RefreshPets();
	}

	public void OnNextRank(Dictionary<string, object> petRankID)
	{
		Guid? entityID = (Guid?)petRankID["PetID"];
		int num = (int)petRankID["Rank"];
		RaisedPetData byEntityID = RaisedPetData.GetByEntityID(entityID);
		if (byEntityID != null)
		{
			SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(byEntityID.PetTypeID);
			for (int num2 = sanctuaryPetTypeInfo._AchievementTitle.Length - 1; num2 >= 0; num2--)
			{
				if (num == sanctuaryPetTypeInfo._AchievementTitle[num2]._Threshold)
				{
					WsWebService.SetAchievementAndGetReward(sanctuaryPetTypeInfo._AchievementTitle[num2]._AchievementID, "", ServiceEventHandler, null);
					break;
				}
			}
		}
		if (num == _DragonChampionAchievementLevel)
		{
			UserAchievementTask.Set(_DragonChampionAchievementID);
		}
	}

	public void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE && inObject != null)
		{
			AchievementReward[] array = (AchievementReward[])inObject;
			if (array != null)
			{
				GameUtilities.AddRewards(array);
			}
		}
	}

	private Renderer FindRenderer(SanctuaryPet pet)
	{
		Renderer componentInChildren = pet.transform.GetComponentInChildren<SkinnedMeshRenderer>();
		if (componentInChildren == null)
		{
			componentInChildren = pet.transform.GetComponentInChildren<MeshRenderer>();
		}
		return componentInChildren;
	}

	public void ApplyRaisedPetColorAttr(SanctuaryPet pet, string attrName, int matIdx, string attrParamName)
	{
		Renderer renderer = FindRenderer(pet);
		if (!(renderer == null) && pet.pData.FindAttrData(attrName) != null)
		{
			Color white = Color.white;
			if (renderer.materials[matIdx].HasProperty(attrParamName))
			{
				renderer.materials[matIdx].SetColor(attrParamName, white);
			}
			else
			{
				renderer.materials[matIdx].color = white;
			}
		}
	}

	public void ApplyPetPartColors(SanctuaryPet pet)
	{
		ApplyRaisedPetColorAttr(pet, "_PetBodyColor", 0, "Color_Skin");
		ApplyRaisedPetColorAttr(pet, "_PetPatternColor", 0, "Color_Belly");
		Renderer renderer = FindRenderer(pet);
		if (!(renderer != null))
		{
			return;
		}
		RaisedPetAttribute raisedPetAttribute = pet.pData.FindAttrData("_PetPatternLuminosity");
		if (raisedPetAttribute != null)
		{
			float result = 0.5f;
			if (float.TryParse(raisedPetAttribute.Value, out result))
			{
				renderer.materials[0].SetFloat("Hue_Belly", result);
			}
		}
	}

	public void InitPetMeter()
	{
		if (pPetMeter != null)
		{
			pPetMeter.RefreshAll();
			return;
		}
		if (AvAvatar.pToolbar != null)
		{
			UiToolbar component = AvAvatar.pToolbar.GetComponent<UiToolbar>();
			if (component != null && component._UiPetMeter != null)
			{
				pPetMeter = component._UiPetMeter;
			}
		}
		else
		{
			GameObject gameObject = GameObject.Find(_PetMeterPrefab);
			if (gameObject != null)
			{
				pPetMeter = gameObject.GetComponent<UiPetMeter>();
			}
		}
		if (pPetMeter != null)
		{
			pPetMeter.SetPetName(pCurPetInstance.pData.Name);
			pPetMeter.SetLevelandRankProgress();
			pCurPetInstance.SetMeterUI(pPetMeter);
			pCurPetInstance.UpdateAllMeters();
			if (AvAvatar.pLevelState != AvAvatarLevelState.FLIGHTSCHOOL)
			{
				pPetMeter.SetVisibility(inVisible: true);
			}
		}
	}

	private bool CheckAllPetsLoaded()
	{
		bool result = true;
		RaisedPetData[] array = null;
		if (RaisedPetData.pActivePets.ContainsKey(pCurrentPetType))
		{
			array = RaisedPetData.pActivePets[pCurrentPetType];
		}
		if (array == null)
		{
			return result;
		}
		RaisedPetData[] array2 = array;
		foreach (RaisedPetData raisedPetData in array2)
		{
			if (IsPetInstanceAllowed(raisedPetData) && raisedPetData.pObject == null)
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsPetHatched(RaisedPetData pd)
	{
		if (pd == null)
		{
			return false;
		}
		if (!pd.IsValid())
		{
			return false;
		}
		return pd.pStage >= RaisedPetStage.BABY;
	}

	public void SendPetToBed(SanctuaryPet pet)
	{
		pet.SetClickableActive(active: false);
		pet.SetState(Character_State.sleep);
		Transform transform = _PetSleepMarkerMale;
		if (pet.pData.Gender == Gender.Female)
		{
			transform = _PetSleepMarkerFemale;
		}
		bool flag = false;
		RaisedPetStage[] nurseryAges = _NurseryAges;
		foreach (RaisedPetStage raisedPetStage in nurseryAges)
		{
			if (pet.pData.pStage == raisedPetStage)
			{
				flag = true;
			}
		}
		if (transform != null)
		{
			pet.transform.rotation = transform.rotation;
			if (!flag)
			{
				pet.transform.position = transform.position + new Vector3(0f, -500f, 0f);
			}
			else if (_AttachPetToBed)
			{
				pet.transform.parent = transform;
				pet.transform.localPosition = new Vector3(0f, 0.25f, 0f);
			}
			else
			{
				pet.transform.position = transform.position + new Vector3(0f, 0.25f, 0f);
			}
		}
		else if (!flag)
		{
			pet.transform.position = new Vector3(0f, -500f, 0f);
		}
		pet.PlayAnim("Sleep", -1, 1f, 1);
		if (pet.pUiPetMeter != null)
		{
			pet.pUiPetMeter.SetVisibility(inVisible: false);
		}
		HandleZzzParticles(pCurPetData.pIsSleeping, pCurPetInstance);
	}

	public void OnSleepDBYes()
	{
		mDlgUp = false;
		Input.ResetInputAxes();
		KAUI.RemoveExclusive(mKAUIGenericDB);
		mKAUIGenericDB.Destroy();
		mKAUIGenericDB = null;
		SnChannel.StopPool("VO_Pool");
		if (pCurPetData.pIsSleeping)
		{
			if (_AttachPetToBed)
			{
				pCurPetInstance.transform.parent = null;
			}
			pCurPetInstance.pMeterPaused = false;
			pCurPetInstance.DoWakeUp();
			pCurPetInstance.SetState(Character_State.idle);
			pCurPetInstance.MoveToAvatar();
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			if (MainStreetMMOClient.pInstance != null)
			{
				MainStreetMMOClient.pInstance.SetRaisedPet(pCurPetInstance.pData, -1);
			}
			pCurPetInstance.gameObject.GetComponent<ObClickable>()._Active = true;
			EnableClicksOnPets(isClicksEnabled: true);
			pCheckPetAge = true;
			HandleZzzParticles(pCurPetData.pIsSleeping, pCurPetInstance);
		}
		else
		{
			pCurPetInstance.pMeterPaused = true;
			pCurPetInstance.DoSleep();
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			SendPetToBed(pCurPetInstance);
			EnableClicksOnPets(isClicksEnabled: false);
			if (mToolbar != null)
			{
				mToolbar.pAutoExit = true;
				mToolbar = null;
			}
			if (MainStreetMMOClient.pInstance != null)
			{
				MainStreetMMOClient.pInstance.SetRaisedPet(pCurPetInstance.pData, -1);
			}
		}
		if (AvAvatar.pToolbar != null)
		{
			AvAvatar.pToolbar.SendMessage("OnPetReady", pCurPetInstance, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void EnableClicksOnPets(bool isClicksEnabled)
	{
		foreach (MMOAvatar value in MainStreetMMOClient.pInstance.pPlayerList.Values)
		{
			if (value.pSanctuaryPet != null)
			{
				value.pSanctuaryPet.SetClickableActive(isClicksEnabled);
			}
		}
	}

	public void OnSleepDBNo()
	{
		mDlgUp = false;
		Input.ResetInputAxes();
		KAUI.RemoveExclusive(mKAUIGenericDB);
		mKAUIGenericDB.Destroy();
		mKAUIGenericDB = null;
		SnChannel.StopPool("VO_Pool");
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
	}

	public bool DoExitSleepPrompt(SanctuaryPet pet, UiToolbar toolBar, AudioClip[] vos, string textMsg)
	{
		if (mDlgUp)
		{
			return false;
		}
		mToolbar = null;
		Input.ResetInputAxes();
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		mDlgUp = true;
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDBSm", "PetExitSleepConfirm");
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._YesMessage = "OnSleepDBYes";
		mKAUIGenericDB._NoMessage = "OnSleepDBNo";
		mKAUIGenericDB.SetText(textMsg, interactive: false);
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
		KAUI.SetExclusive(mKAUIGenericDB);
		SnChannel.Play(vos, "VO_Pool", 0, inForce: true, null);
		mToolbar = toolBar;
		return true;
	}

	public bool DoSleepPrompt(SanctuaryPet pet, bool sleepOnly)
	{
		if (mDlgUp)
		{
			return false;
		}
		mToolbar = null;
		if (sleepOnly && pet.pData.pIsSleeping)
		{
			return false;
		}
		Input.ResetInputAxes();
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		mDlgUp = true;
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDBSm", "PetSleepConfirm");
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._YesMessage = "OnSleepDBYes";
		mKAUIGenericDB._NoMessage = "OnSleepDBNo";
		AudioClip vo;
		if (pet.pData.pIsSleeping)
		{
			mKAUIGenericDB.SetText(pet.GetWakeMsg(out vo), interactive: false);
			if (vo != null)
			{
				SnChannel.Play(vo, "VO_Pool", inForce: true, null);
			}
		}
		else
		{
			mKAUIGenericDB.SetText(pet.GetSleepMsg(out vo), interactive: false);
		}
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
		KAUI.SetExclusive(mKAUIGenericDB);
		return true;
	}

	public void OnGrowDBClose()
	{
		mDlgUp = false;
		Input.ResetInputAxes();
		KAUI.RemoveExclusive(mKAUIGenericDB);
		mKAUIGenericDB.Destroy();
		mKAUIGenericDB = null;
		SnChannel.StopPool("VO_Pool");
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
	}

	private void DoGrowPrompt(SanctuaryPet pet)
	{
		if (!mDlgUp)
		{
			mDlgUp = true;
			pet.GrowToNextAge();
			ShowGrowthPrompt();
		}
	}

	public void AddXP(int inAmount)
	{
		int num = 1;
		float maxMeter = SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.ENERGY, pCurPetData);
		UserAchievementInfo userAchievementInfo = PetRankData.GetUserAchievementInfo(pCurPetData);
		if (userAchievementInfo != null)
		{
			num = userAchievementInfo.RankID;
		}
		PetRankData.AddPoints(pCurPetData, inAmount);
		if (pPetMeter != null)
		{
			pPetMeter.SetLevelandRankProgress();
		}
		if (!(pCurPetInstance != null))
		{
			return;
		}
		userAchievementInfo = PetRankData.GetUserAchievementInfo(pCurPetData);
		if (userAchievementInfo != null)
		{
			if (pCurPetInstance.pData != null)
			{
				pCurPetInstance.pData.pRank = userAchievementInfo.RankID;
			}
			if (num != userAchievementInfo.RankID)
			{
				pCurPetInstance.SetMeter(SanctuaryPetMeterType.HAPPINESS, SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.HAPPINESS, pCurPetData));
				pCurPetInstance.UpdateMeter(SanctuaryPetMeterType.ENERGY, SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.ENERGY, pCurPetData) - maxMeter);
			}
		}
	}

	public void ShowGrowthPrompt()
	{
		ShowGrowthPrompt(base.gameObject);
	}

	public void ShowGrowthPrompt(GameObject go)
	{
		Debug.LogError("The Asset should be loaded as a bundle. This will not work. Implement it iff needed");
	}

	public static bool IsPetInstanceAllowed(RaisedPetData petData)
	{
		if (petData == null)
		{
			return false;
		}
		if (!petData.IsValid())
		{
			return false;
		}
		RaisedPetStage[] blockedStages = pInstance._BlockedStages;
		foreach (RaisedPetStage raisedPetStage in blockedStages)
		{
			if (petData.pStage == raisedPetStage)
			{
				return false;
			}
		}
		if (petData.pStage == RaisedPetStage.EGGINHAND)
		{
			return true;
		}
		if (IsPetHatched(petData))
		{
			return true;
		}
		return false;
	}

	public static bool IsActionAllowed(RaisedPetData petData, PetActions inActionID)
	{
		if (petData == null)
		{
			return false;
		}
		SanctuaryPetTypeSettings sanctuaryPetSettings = SanctuaryData.GetSanctuaryPetSettings(SanctuaryData.FindSanctuaryPetTypeInfo(petData.PetTypeID)._Settings);
		PetMeterActionData[] actionMeterData = sanctuaryPetSettings._ActionMeterData;
		SanctuaryPetMeterInstance[] array = new SanctuaryPetMeterInstance[sanctuaryPetSettings._Meters.Length];
		for (int i = 0; i < sanctuaryPetSettings._Meters.Length; i++)
		{
			array[i] = new SanctuaryPetMeterInstance();
			array[i].mMeterTypeInfo = sanctuaryPetSettings._Meters[i];
			array[i].mMeterValData = petData.FindStateData(sanctuaryPetSettings._Meters[i]._Type.ToString());
			if (array[i].mMeterValData == null)
			{
				float maxMeter = SanctuaryData.GetMaxMeter(sanctuaryPetSettings._Meters[i]._Type, petData);
				array[i].mMeterValData = petData.SetStateData(sanctuaryPetSettings._Meters[i]._Type.ToString(), maxMeter);
			}
		}
		if (array == null || array.Length == 0)
		{
			return false;
		}
		PetMeterActionData[] array2 = actionMeterData;
		foreach (PetMeterActionData petMeterActionData in array2)
		{
			if (petMeterActionData._ID != inActionID || !(petMeterActionData._Delta < 0f))
			{
				continue;
			}
			SanctuaryPetMeterInstance[] array3 = array;
			foreach (SanctuaryPetMeterInstance sanctuaryPetMeterInstance in array3)
			{
				if (sanctuaryPetMeterInstance.mMeterTypeInfo._Type == petMeterActionData._MeterType && sanctuaryPetMeterInstance.mMeterValData.Value < 0f - petMeterActionData._Delta)
				{
					return false;
				}
			}
		}
		return true;
	}

	public string PetAnimOverride(string inLevel)
	{
		string pCurrentLevel = RsResourceManager.pCurrentLevel;
		if (_FullAnimLevels != null)
		{
			string[] fullAnimLevels = _FullAnimLevels;
			foreach (string value in fullAnimLevels)
			{
				if (pCurrentLevel.Contains(value))
				{
					return "Full";
				}
			}
		}
		return inLevel;
	}

	public void RaisedPetGetCallback(int ptype, RaisedPetData[] pdata, object inUserData)
	{
		pCurrentPetType = ptype;
		if (pdata != null)
		{
			for (int i = 0; i < pdata.Length; i++)
			{
				pdata[i].DumpData();
			}
		}
		pCurPetData = RaisedPetData.GetCurrentInstance(ptype);
		if (pCurPetData == null)
		{
			mIsReady = true;
		}
		mAchievementInfoReady = false;
		PetRankData.LoadUserAchievementInfo(pCurPetData, OnPetAchievementInfoReady);
	}

	private void OnPetAchievementInfoReady(UserAchievementInfo petInfo, object userData)
	{
		mAchievementInfoReady = true;
	}

	private void PetPicServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE && inObject == null && pCurPetInstance != null)
		{
			TakePicture(pCurPetInstance.gameObject);
		}
	}

	private void Update()
	{
		if (this != pInstance)
		{
			Debug.LogError("Update called from bad instance");
			return;
		}
		UpdateWeaponRechargeData();
		if (!mSanctuaryDataReady)
		{
			return;
		}
		RaisedPetData.FlushSaveCache(pCurrentPetType);
		if (pLevelName != RsResourceManager.pCurrentLevel)
		{
			return;
		}
		if (mLoadData)
		{
			pPendingMMOPetCheck = false;
			if (pCurrentPetType >= 0)
			{
				if (IsActivePetDataReady())
				{
					mLoadData = false;
					if (RaisedPetData.pActivePets.ContainsKey(pCurrentPetType))
					{
						RaisedPetGetCallback(pCurrentPetType, RaisedPetData.pActivePets[pCurrentPetType], null);
					}
				}
			}
			else if (UserInfo.pIsReady)
			{
				mLoadData = false;
				RaisedPetData.GetSelectedRaisedPet(selected: true, RaisedPetGetCallback, null);
				return;
			}
		}
		if (mCreateInstance)
		{
			if (mUnselectedPet != null)
			{
				pCurPetData = mUnselectedPet;
			}
			if (!IsActivePetDataReady() || !UserRankData.pIsReady || !mAchievementInfoReady)
			{
				return;
			}
			mCreateInstance = false;
			mAllPetLoaded = false;
			if (pCurPetData != null)
			{
				if (pPetStartMarkerName != null && pPetStartMarkerName.Length > 0)
				{
					GameObject gameObject = GameObject.Find(pPetStartMarkerName);
					if (gameObject != null)
					{
						_PetStartMarker = gameObject.transform;
					}
				}
				if (pCurPetData.pStage == RaisedPetStage.HATCHING)
				{
					_MeterPaused = true;
				}
				if (_MultiPets)
				{
					RaisedPetData[] array = null;
					if (RaisedPetData.pActivePets.ContainsKey(pCurrentPetType))
					{
						array = RaisedPetData.pActivePets[pCurrentPetType];
					}
					if (array != null)
					{
						RaisedPetData[] array2 = array;
						foreach (RaisedPetData raisedPetData in array2)
						{
							if (IsPetInstanceAllowed(raisedPetData))
							{
								mPetNeedFinalize = true;
								CreatePet(raisedPetData, _PetOffScreenPosition, Quaternion.identity, base.gameObject, PetAnimOverride("Player"), applyCustomSkin: true);
							}
						}
					}
				}
				else if (!IsPetInstanceAllowed(pCurPetData))
				{
					UtDebug.Log("Pet cannot be instanced here");
					mIsReady = true;
					mPetNeedFinalize = false;
				}
				else
				{
					mPetNeedFinalize = true;
					UserRank userRank = PetRankData.GetUserRank(pCurPetData);
					pCurPetData.pRank = userRank.RankID;
					CreatePet(pCurPetData, _PetOffScreenPosition, Quaternion.identity, base.gameObject, PetAnimOverride("Player"), applyCustomSkin: true);
				}
			}
			else
			{
				mIsReady = true;
			}
			if (!mPetNeedFinalize && mReloadPetMsgObj != null)
			{
				mReloadPetMsgObj.SendMessage("OnPetReloaded", false, SendMessageOptions.DontRequireReceiver);
				mReloadPetMsgObj = null;
			}
			return;
		}
		mIsReady = true;
		if (pLevelReady && mPetNeedFinalize && mAllPetLoaded)
		{
			FinalizeAllPet();
		}
		if (!(pCurPetInstance != null))
		{
			return;
		}
		if (pPendingMMOPetCheck && MainStreetMMOClient.pInstance != null)
		{
			if (MainStreetMMOClient.pInstance.IsLevelMMO(RsResourceManager.pCurrentLevel))
			{
				if (MainStreetMMOClient.pInstance.pState == MMOClientState.IN_ROOM)
				{
					int mount = -1;
					if (pCurPetInstance.pIsMounted)
					{
						mount = (int)pCurPetInstance.pCurrentSkillType;
					}
					MainStreetMMOClient.pInstance.SetRaisedPet(pCurPetData, mount);
					pPendingMMOPetCheck = false;
				}
			}
			else
			{
				pPendingMMOPetCheck = false;
			}
		}
		if (pCheckPetAge && _CanCheckPetAge && mAllPetLoaded && pLevelReady)
		{
			pCheckPetAge = false;
			if (pCurPetInstance.CheckForSkillCompleted() || pCheatGrowth)
			{
				pCheatGrowth = false;
				if (pCurPetData.pStage != RaisedPetStage.EGGINHAND)
				{
					DoGrowPrompt(pCurPetInstance);
				}
			}
		}
		if (!mPetPictureInitialized && pCurPetData != null && !pCurPetData.pNoSave)
		{
			mPetPictureInitialized = true;
			UpdatePetPicture();
		}
		if (Application.isEditor && Input.GetKeyUp(KeyCode.T))
		{
			TakePicture(pCurPetInstance.gameObject, base.gameObject, inSendPicture: false);
		}
		if (!mToolbarInformed && AvAvatar.pToolbar != null && AvAvatar.pToolbar.activeInHierarchy)
		{
			mToolbarInformed = true;
			AvAvatar.pToolbar.SendMessage("OnPetReady", pCurPetInstance, SendMessageOptions.DontRequireReceiver);
		}
		if (!mLowStatWarningShown && AvAvatar.pToolbar != null && AvAvatar.pToolbar.activeInHierarchy && !RsResourceManager.pLevelLoadingScreen)
		{
			mLowStatWarningShown = true;
			if (pCurPetInstance.HasMood(Character_Mood.tired) && _LowEnergyVO != null)
			{
				SnChannel.Play(_LowEnergyVO, "VO_Pool", 0, inForce: true);
			}
			if (pCurPetInstance.HasMood(Character_Mood.angry) && _LowHappinessVO != null)
			{
				SnChannel.Play(_LowHappinessVO, "VO_Pool", 0, inForce: true);
			}
		}
		if (mSetFollowAvatar || !AvAvatar.pObject.activeInHierarchy || RsResourceManager.pLevelLoadingScreen || AvAvatar.pSubState == AvAvatarSubState.UWSWIMMING)
		{
			return;
		}
		mSetFollowAvatar = true;
		bool flag = false;
		if (_MountStateIgnoreScenes != null && _MountStateIgnoreScenes.Length != 0)
		{
			string[] mountStateIgnoreScenes = _MountStateIgnoreScenes;
			for (int i = 0; i < mountStateIgnoreScenes.Length; i++)
			{
				if (mountStateIgnoreScenes[i] == RsResourceManager.pCurrentLevel)
				{
					flag = true;
					break;
				}
			}
		}
		if (AvAvatar.pLevelState == AvAvatarLevelState.RACING)
		{
			flag = true;
		}
		CoCommonLevel coCommonLevel = UnityEngine.Object.FindObjectOfType(typeof(CoCommonLevel)) as CoCommonLevel;
		if (coCommonLevel != null)
		{
			mMountedState = mMountedState || (coCommonLevel._AvatarStartSubState == AvAvatarSubState.FLYING && coCommonLevel.IsSubstateValid(coCommonLevel._AvatarStartSubState));
		}
		if (!flag && mMountedState && pCurPetInstance.pAge >= pCurPetInstance.pTypeInfo._MinAgeToMount)
		{
			_FollowAvatar = false;
			if (coCommonLevel != null)
			{
				if (coCommonLevel._AvatarStartSubState == AvAvatarSubState.FLYING)
				{
					pCurPetInstance.Mount(AvAvatar.pObject, PetSpecialSkillType.FLY);
					AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
					if (component != null)
					{
						component.SetFlyingHover(inHover: true);
					}
				}
				else if (coCommonLevel._AvatarStartSubState == AvAvatarSubState.SWIMMING)
				{
					pCurPetInstance.Mount(AvAvatar.pObject, PetSpecialSkillType.SWIM);
				}
				else
				{
					pCurPetInstance.Mount(AvAvatar.pObject, PetSpecialSkillType.RUN);
				}
			}
			else
			{
				pCurPetInstance.Mount(AvAvatar.pObject, PetSpecialSkillType.RUN);
			}
			return;
		}
		if (_FollowAvatar && !pCurPetInstance.pData.pIsSleeping)
		{
			pCurPetInstance.SetAvatar(AvAvatar.mTransform);
			pCurPetInstance.SetFollowAvatar(follow: true);
			if (_PetStartMarker == null)
			{
				pCurPetInstance.MoveToAvatar(postponed: true);
			}
			return;
		}
		AvAvatarController component2 = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (!(component2 == null))
		{
			if (component2.pIsPlayerGliding)
			{
				component2.OnGlideLanding();
			}
			component2.SoftReset();
			component2.pState = AvAvatarState.IDLE;
		}
	}

	private void OnCloseDragonAgeUI()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		KAUI.RemoveExclusive(mKAUIGenericDB);
		mKAUIGenericDB.Destroy();
		mKAUIGenericDB = null;
	}

	public void UpdatePetPicture()
	{
		WsWebService.GetImageData("EggColor", GetCurrentPetImagePosition(), PetPicServiceEventHandler, null);
	}

	public int GetCurrentPetImagePosition()
	{
		if (!pCurPetData.ImagePosition.HasValue)
		{
			pCurPetData.ImagePosition = pNextRaisedPetIndex;
			pCurPetData.SaveDataReal();
			pNextRaisedPetIndex++;
		}
		return pCurPetData.ImagePosition.Value;
	}

	public void HandleZzzParticles(bool emit, SanctuaryPet Dragon)
	{
		Dragon.SetSleepParticle(emit, _PetZZZParticle);
	}

	public void TakePicture(GameObject inGameObject, GameObject inReceiver = null, bool inSendPicture = true)
	{
		if (inReceiver == null)
		{
			inReceiver = base.gameObject;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(inGameObject, _PetOffScreenPosition, Quaternion.identity);
		SanctuaryPet component = inGameObject.GetComponent<SanctuaryPet>();
		if (gameObject != null)
		{
			SanctuaryPet component2 = gameObject.GetComponent<SanctuaryPet>();
			if (component2 != null)
			{
				component2.mAvatarTarget = null;
			}
			if (component2 != null && component2.pData != null)
			{
				component2.RemoveAllEffects();
				component2.pData.ImagePosition = component.pData.ImagePosition;
				component2.pData.EntityID = component.pData.EntityID;
				component2.SetMood(Character_Mood.firedup, t: false);
				component2.animation.cullingType = AnimationCullingType.AlwaysAnimate;
				component2.animation.Play("PhotoPose");
				StartCoroutine(TakePictureDelayed(gameObject, inReceiver, inSendPicture));
			}
			else
			{
				UnityEngine.Object.Destroy(gameObject);
				if (inReceiver != null)
				{
					inReceiver.SendMessage("OnPetPictureDoneFailed", null, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		else if (inReceiver != null)
		{
			inReceiver.SendMessage("OnPetPictureDoneFailed", null, SendMessageOptions.DontRequireReceiver);
		}
	}

	private IEnumerator TakePictureDelayed(GameObject inGameObject, GameObject inReceiver, bool inSendPicture = false)
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		if (inGameObject != null)
		{
			SanctuaryPet component = inGameObject.GetComponent<SanctuaryPet>();
			if (component != null)
			{
				component.mAvatarTarget = null;
				string text = (string.IsNullOrEmpty(component._PictureTargetTransformName) ? component.GetHeadBoneName() : component._PictureTargetTransformName);
				if (!string.IsNullOrEmpty(text))
				{
					Transform transform = component.FindBoneTransform(text);
					if (transform != null)
					{
						AvPhotoManager avPhotoManager = AvPhotoManager.Init("PfPetPhotoMgr");
						Texture2D dstTexture = new Texture2D(256, 256, TextureFormat.RGBA4444, mipChain: false);
						avPhotoManager._HeadShotCamOffset = component.pCurAgeData._HUDPictureCameraOffset;
						avPhotoManager.TakeAShot(component.gameObject, ref dstTexture, transform);
						if (inSendPicture)
						{
							int slotIdx = (component.pData.ImagePosition.HasValue ? component.pData.ImagePosition.Value : 0);
							ImageData.Save("EggColor", slotIdx, dstTexture);
							ImageData.UpdateImages("EggColor");
						}
						if (inReceiver != null)
						{
							inReceiver.SendMessage("OnPetPictureDone", dstTexture, SendMessageOptions.DontRequireReceiver);
						}
					}
					else
					{
						if (inReceiver != null)
						{
							inReceiver.SendMessage("OnPetPictureDoneFailed", null, SendMessageOptions.DontRequireReceiver);
						}
						UtDebug.LogError("NO HEAD BONE FOUND!!!");
					}
				}
				else
				{
					if (inReceiver != null)
					{
						inReceiver.SendMessage("OnPetPictureDoneFailed", null, SendMessageOptions.DontRequireReceiver);
					}
					UtDebug.LogError("NO HEAD BONE NAME PROVIDED!!");
				}
			}
			else if (inReceiver != null)
			{
				inReceiver.SendMessage("OnPetPictureDoneFailed", null, SendMessageOptions.DontRequireReceiver);
			}
			UnityEngine.Object.Destroy(inGameObject);
		}
		else if (inReceiver != null)
		{
			inReceiver.SendMessage("OnPetPictureDoneFailed", null, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void LoadPetNameSelectionScreen()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		PauseAvatar();
		string[] array = _PetNameSelectBundlePath.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], PetNameUiHandler, typeof(GameObject));
	}

	private void PetNameUiHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			KAUICursorManager.SetDefaultCursor("Arrow");
			UnityEngine.Object.Instantiate((GameObject)inObject).name = "PfUiDragonName";
			break;
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			EnableAvatar();
			break;
		}
	}

	public void LoadPetCustomizationScreen(string inBundlePath = "")
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		PauseAvatar();
		if (string.IsNullOrEmpty(inBundlePath))
		{
			inBundlePath = _PetCustomizationBundlePath;
		}
		string[] array = inBundlePath.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], PetCustomizationUiHandler, typeof(GameObject));
	}

	private void PetCustomizationUiHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.name = "PfUiDragonCustomization";
			UiDragonCustomization component = obj.GetComponent<UiDragonCustomization>();
			component.pPetData = pCurPetInstance.pData;
			component.SetUiForJournal(isJournal: false);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			EnableAvatar();
			break;
		}
	}

	public void PlayNeedMeterTutorial(InteractiveTutManager mTutorial, int[] mTaskList, int mTaskID)
	{
		if (!(mTutorial != null) || mTutorial.TutorialComplete())
		{
			return;
		}
		for (int i = 0; i < mTaskList.Length; i++)
		{
			if (mTaskID == mTaskList[i])
			{
				PauseAvatar();
				pPetMeter.DetachFromToolbar();
				mTutorial._StepEndedEvent = (StepEndedEvent)Delegate.Combine(mTutorial._StepEndedEvent, new StepEndedEvent(OnStepEnded));
				mTutorial.ShowTutorial();
				Vector3 position = pPetMeter.transform.position;
				position.x = (position.y = 0f);
				pPetMeter.transform.position = position;
			}
		}
	}

	public void OnStepEnded(int stepIdx, string stepName, bool tutQuit)
	{
		if (InteractiveTutManager._CurrentActiveTutorialObject != null)
		{
			InteractiveTutManager component = InteractiveTutManager._CurrentActiveTutorialObject.GetComponent<InteractiveTutManager>();
			if (component != null && stepIdx == component._TutSteps.Length - 1)
			{
				component._StepEndedEvent = (StepEndedEvent)Delegate.Remove(component._StepEndedEvent, new StepEndedEvent(OnStepEnded));
				EnableAvatar();
				pPetMeter.AttachToToolbar();
			}
		}
	}

	public void PlayMountTutorial()
	{
		if (pCurPetData.pStage == RaisedPetStage.TEEN && _MountTutorial != null && !_MountTutorial.TutorialComplete())
		{
			PauseAvatar();
			UiAvatarControls.pInstance.DetachFromToolbar();
			InteractiveTutManager mountTutorial = _MountTutorial;
			mountTutorial._StepStartedEvent = (StepStartedEvent)Delegate.Combine(mountTutorial._StepStartedEvent, new StepStartedEvent(OnMountStepStarted));
			InteractiveTutManager mountTutorial2 = _MountTutorial;
			mountTutorial2._StepEndedEvent = (StepEndedEvent)Delegate.Combine(mountTutorial2._StepEndedEvent, new StepEndedEvent(OnMountStepEnded));
			_MountTutorial.ShowTutorial();
		}
		else
		{
			UnityEngine.Object.Destroy(_MountTutorial);
			_MountTutorial = null;
		}
	}

	public void OnMountStepStarted(int stepIdx, string stepName)
	{
		if (_MountTutorial != null && stepIdx == _MountTutorial._TutSteps.Length - 1)
		{
			InteractiveTutManager mountTutorial = _MountTutorial;
			mountTutorial._StepStartedEvent = (StepStartedEvent)Delegate.Remove(mountTutorial._StepStartedEvent, new StepStartedEvent(OnMountStepStarted));
			UiAvatarControls.pInstance.SetVisibility(inVisible: true);
			KAWidget kAWidget = UiAvatarControls.pInstance.FindItem("AniBrakePointer");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: true);
			}
			KAInput.pInstance.EnableInputType("DragonMount", InputType.ALL, inEnable: true);
			KAInput.pInstance.EnableInputType("Jump", InputType.ALL, inEnable: false);
		}
	}

	public void OnMountStepEnded(int stepIdx, string stepName, bool tutQuit)
	{
		if (_MountTutorial != null && stepIdx == _MountTutorial._TutSteps.Length - 1)
		{
			InteractiveTutManager mountTutorial = _MountTutorial;
			mountTutorial._StepEndedEvent = (StepEndedEvent)Delegate.Remove(mountTutorial._StepEndedEvent, new StepEndedEvent(OnStepEnded));
			KAWidget kAWidget = UiAvatarControls.pInstance.FindItem("AniBrakePointer");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
			}
			KAInput.pInstance.EnableInputType("Jump", InputType.ALL, inEnable: true);
			EnableAvatar();
		}
	}

	private void PauseAvatar()
	{
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
	}

	private void EnableAvatar()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
	}

	public void SetAge(RaisedPetData inData, int inAge, bool inSave = true)
	{
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(inData.PetTypeID);
		if (sanctuaryPetTypeInfo == null)
		{
			return;
		}
		if (inData.Accessories != null)
		{
			string attribute = RaisedPetData.GetGrowthStage(inAge).ToString()[0] + RaisedPetData.GetGrowthStage(inAge).ToString().Substring(1)
				.ToLower() + "Mesh";
			string attribute2 = RaisedPetData.GetGrowthStage(inAge).ToString()[0] + RaisedPetData.GetGrowthStage(inAge).ToString().Substring(1)
				.ToLower() + "Tex";
			RaisedPetAccessory[] accessories = inData.Accessories;
			foreach (RaisedPetAccessory raisedPetAccessory in accessories)
			{
				int accessoryItemID = inData.GetAccessoryItemID(raisedPetAccessory);
				UserItemData userItemData = CommonInventoryData.pInstance.FindItem(accessoryItemID);
				if (userItemData != null)
				{
					RaisedPetAccType accessoryType = RaisedPetData.GetAccessoryType(raisedPetAccessory.Type);
					inData.SetAccessory(accessoryType, userItemData.Item.GetAttribute(attribute, userItemData.Item.AssetName), userItemData.Item.GetAttribute(attribute2, (userItemData.Item.Texture != null) ? userItemData.Item.Texture[0].TextureName : raisedPetAccessory.Texture), userItemData);
				}
			}
			pCurPetInstance.UpdateAccessories();
		}
		SantuayPetResourceInfo[] petResList = sanctuaryPetTypeInfo._AgeData[inAge]._PetResList;
		foreach (SantuayPetResourceInfo santuayPetResourceInfo in petResList)
		{
			if (santuayPetResourceInfo._Gender == inData.Gender)
			{
				inData.Geometry = santuayPetResourceInfo._Prefab;
			}
		}
		inData.SetState(RaisedPetData.GetGrowthStage(inAge), inSave);
	}

	public void CreateCurrentPet(RaisedPetData inData, int inAge, GameObject inTakePictureReceiver, bool inSave = true, RaisedPetSaveEventHandler callback = null)
	{
		if (RaisedPetData.GetAgeIndex(inData.pStage) != inAge)
		{
			SetAge(inData, inAge, inSave);
			if (inSave)
			{
				inData.SaveDataReal(callback);
			}
		}
		if (pCurPetInstance != null)
		{
			UnityEngine.Object.Destroy(pCurPetInstance.gameObject);
		}
		pCurPetInstance = null;
		mSetFollowAvatar = false;
		pCurPetData = inData;
		mTakePictureReceiver = inTakePictureReceiver;
		CreatePet(inData, _PetOffScreenPosition, Quaternion.identity, base.gameObject, "Full", applyCustomSkin: true);
	}

	public void OnDestroy()
	{
		if (pInstance == this)
		{
			pPetStartMarkerName = "";
			pCheckPetAge = false;
			pCheatGrowth = false;
			pCurrentPetType = -1;
			pCurPetInstance = null;
			pCurPetData = null;
			pLevelReady = false;
			pNextRaisedPetIndex = 0;
			pLevelName = "";
			pPendingMMOPetCheck = false;
			string[] array = _SanctuaryDataURL.Split('/');
			RsResourceManager.SetDontDestroy(array[0] + "/" + array[1], inDontDestroy: false);
			MissionManager.RemoveMissionEventHandler(OnMissionEvent);
			pInstance = null;
			base.enabled = false;
		}
	}

	public static void Reset()
	{
		if (pInstance != null)
		{
			UnityEngine.Object.Destroy(pInstance.gameObject);
		}
	}

	public void OnMissionEvent(MissionEvent inEvent, object inObject)
	{
		switch (inEvent)
		{
		case MissionEvent.OFFER_COMPLETE:
		{
			MissionManager.Action action = (MissionManager.Action)inObject;
			if (action._Object != null && action._Object.GetType() == typeof(Task) && pPetMeter != null)
			{
				Task task = (Task)action._Object;
				PlayNeedMeterTutorial(pPetMeter._EnergyTutorial, _NeedEnergyMeterTutorialTask, task.TaskID);
			}
			break;
		}
		case MissionEvent.MISSION_REWARDS_COMPLETE:
		{
			Mission mission = (Mission)inObject;
			if (mission == null || mission.MissionID != _TeenMission)
			{
				break;
			}
			if (_MountTutorial != null)
			{
				PlayMountTutorial();
			}
			else if (!string.IsNullOrEmpty(_MountTutorialRes))
			{
				string[] array = _MountTutorialRes.Split('/');
				if (array.Length == 3)
				{
					KAUICursorManager.SetDefaultCursor("Loading");
					RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnMountTutorialReady, typeof(GameObject));
				}
			}
			else
			{
				UtDebug.LogError("_MountTutorialRes is null");
			}
			break;
		}
		}
	}

	private void OnMountTutorialReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			string[] array = inURL.Split('/');
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
			gameObject.name = array[^1];
			_MountTutorial = gameObject.GetComponent<InteractiveTutManager>();
			pInstance._MountTutorial = _MountTutorial;
			PlayMountTutorial();
		}
	}

	private void ShowDialog(string inText)
	{
		if (!string.IsNullOrEmpty(inText))
		{
			if (mKAUIGenericDB != null)
			{
				KAUI.RemoveExclusive(mKAUIGenericDB);
				mKAUIGenericDB.Destroy();
				mKAUIGenericDB = null;
			}
			PauseAvatar();
			mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "PetUnhappyDB");
			mKAUIGenericDB._MessageObject = base.gameObject;
			mKAUIGenericDB.SetText(inText, interactive: false);
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			mKAUIGenericDB._OKMessage = "OkMessage";
			KAUI.SetExclusive(mKAUIGenericDB);
		}
	}

	private void OkMessage()
	{
		EnableAvatar();
		if (mKAUIGenericDB != null)
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			mKAUIGenericDB.Destroy();
			mKAUIGenericDB = null;
		}
	}

	public static void LoadTempPet(SanctuaryPet tempPet, bool cacheInstance = false)
	{
		if (!UtMobileUtilities.CanLoadInCurrentScene(UiType.NONE, UILoadOptions.AUTO) && !cacheInstance)
		{
			mUnselectedPet = tempPet.pData;
			return;
		}
		pPrevPetInstance = pCurPetInstance;
		pCurPetInstance = tempPet;
		PetWeaponManager component = pCurPetInstance.gameObject.GetComponent<PetWeaponManager>();
		if (component != null)
		{
			component.pUserControlledWeapon = true;
		}
		if (pInstance.pPetMeter != null)
		{
			pCurPetInstance.UpdateAllMeters();
			pInstance.pPetMeter.RefreshAll();
		}
		else
		{
			pInstance.InitPetMeter();
		}
	}

	public static void ResetToActivePet()
	{
		if (mUnselectedPet != null)
		{
			mUnselectedPet.SaveDataReal();
			mUnselectedPet = null;
			return;
		}
		if (pCurPetInstance != null)
		{
			pCurPetInstance.pData.SaveDataReal(null, null, savePetMeterAlone: true);
		}
		if (pPrevPetInstance != null)
		{
			pCurPetInstance = pPrevPetInstance;
			pPrevPetInstance = null;
			RefreshPetMeter();
		}
	}

	public static void RefreshPetMeter()
	{
		if (pInstance.pPetMeter != null)
		{
			pInstance.pPetMeter.SetPetName(pCurPetInstance.pData.Name);
			pInstance.pPetMeter.SetLevelandRankProgress();
			pCurPetInstance.SetMeterUI(pInstance.pPetMeter);
			pCurPetInstance.UpdateAllMeters();
			pInstance.pPetMeter.RefreshAll();
		}
		else
		{
			pInstance.InitPetMeter();
		}
	}

	public static bool IsPetLocked(RaisedPetData petData, string key = "TicketID")
	{
		bool flag = false;
		if (!SubscriptionInfo.pIsMember || SubscriptionInfo.IsOneMonthMembership())
		{
			RaisedPetAttribute raisedPetAttribute = petData.FindAttrData(key);
			if (raisedPetAttribute == null)
			{
				return false;
			}
			int itemID = int.Parse(raisedPetAttribute.Value);
			flag = true;
			UserItemData userItemData = null;
			if (CommonInventoryData.pIsReady)
			{
				userItemData = CommonInventoryData.pInstance.FindItem(itemID);
			}
			if (userItemData == null && ParentData.pInstance.pInventory.pIsReady)
			{
				userItemData = ParentData.pInstance.pInventory.pData.FindItem(itemID);
			}
			if (userItemData != null && !userItemData.Item.Locked)
			{
				flag = false;
			}
			if (flag)
			{
				List<string> list = null;
				if (SanctuaryData.pInstance != null)
				{
					list = SanctuaryData.GetUniquePetTicketItemsList(petData.PetTypeID);
				}
				if (list != null)
				{
					UserItemData[] items = ParentData.pInstance.pInventory.GetItems(454);
					if (items != null)
					{
						UserItemData[] array = items;
						foreach (UserItemData userItemData2 in array)
						{
							if (list.Contains(userItemData2.Item.ItemID.ToString()) && !userItemData2.Item.Locked)
							{
								raisedPetAttribute.Value = userItemData2.Item.ItemID.ToString();
								petData.SaveDataReal();
								return false;
							}
						}
					}
				}
			}
		}
		return flag;
	}

	public void SetWeaponRechargeData(SanctuaryPet pet)
	{
		WeaponRechargeData weaponRechargeData = null;
		int num = pet.pData.RaisedPetID;
		if (num == 0)
		{
			num = pet.pData.PetTypeID;
		}
		if (pet.mNPC)
		{
			num = -1;
		}
		mPetWeaponRechargeMap.Remove(-1);
		if (!mPetWeaponRechargeMap.ContainsKey(num))
		{
			weaponRechargeData = new WeaponRechargeData();
			weaponRechargeData.mNumTotalShots = pet.GetWeaponTotalShots();
			weaponRechargeData.mShotsAvailable = weaponRechargeData.mNumTotalShots;
			weaponRechargeData.mWeaponRechargeRange = pet.GetWeaponRechargeRange();
			weaponRechargeData.mTotalShotsProgress = 1f;
			weaponRechargeData.mNPC = pet.mNPC;
			mPetWeaponRechargeMap[num] = weaponRechargeData;
		}
		else
		{
			weaponRechargeData = mPetWeaponRechargeMap[num];
			weaponRechargeData.mNumTotalShots = pet.GetWeaponTotalShots();
			weaponRechargeData.mWeaponRechargeRange = pet.GetWeaponRechargeRange();
			weaponRechargeData.OnAvailableShotUpdated = null;
		}
		if (pet.pWeaponManager != null)
		{
			WeaponTuneData.Weapon currentWeapon = pet.pWeaponManager.GetCurrentWeapon();
			if (currentWeapon != null)
			{
				currentWeapon._AvailableShots = weaponRechargeData.mShotsAvailable;
			}
			PetWeaponManager pWeaponManager = pet.pWeaponManager;
			pWeaponManager.OnAvailableShotUpdated = (WeaponManager.AvailableShotUpdated)Delegate.Combine(pWeaponManager.OnAvailableShotUpdated, new WeaponManager.AvailableShotUpdated(weaponRechargeData.UpdateShotsAvailable));
			WeaponRechargeData weaponRechargeData2 = weaponRechargeData;
			weaponRechargeData2.OnAvailableShotUpdated = (WeaponManager.AvailableShotUpdated)Delegate.Combine(weaponRechargeData2.OnAvailableShotUpdated, new WeaponManager.AvailableShotUpdated(pet.pWeaponManager.UpdateShotsAvailable));
		}
		if (pet.mNPC)
		{
			pet.pWeaponShotsAvailable = weaponRechargeData.mShotsAvailable;
		}
	}

	private void UpdateWeaponRechargeData()
	{
		if (pCurPetInstance == null || pCurPetInstance.pData == null)
		{
			return;
		}
		int num = pCurPetInstance.pData.RaisedPetID;
		if (num == 0)
		{
			num = pCurPetInstance.pData.PetTypeID;
		}
		foreach (KeyValuePair<int, WeaponRechargeData> item in mPetWeaponRechargeMap)
		{
			bool flag = item.Key == num;
			if (flag || item.Value.mNPC)
			{
				item.Value.mFireCoolDownRegenRate = pCurPetInstance.pWeaponManager._FireCoolDownRegenRate;
			}
			item.Value.Update(Time.deltaTime, item.Value.mNPC || flag);
		}
	}

	public WeaponRechargeData GetWeaponRechargeData(SanctuaryPet pet)
	{
		int num = pCurPetInstance.pData.RaisedPetID;
		if (num == 0)
		{
			num = pCurPetInstance.pData.PetTypeID;
		}
		if (pet.mNPC && mPetWeaponRechargeMap.ContainsKey(-1))
		{
			return mPetWeaponRechargeMap[-1];
		}
		if (!mPetWeaponRechargeMap.ContainsKey(num))
		{
			return null;
		}
		return mPetWeaponRechargeMap[num];
	}

	public void InitNameChange(RaisedPetData petData, GameObject inMessageObject)
	{
		_PetNameChange.mPetData = petData;
		_PetNameChange.mMessageObject = inMessageObject;
		KAUICursorManager.SetDefaultCursor("Loading");
		ItemStoreDataLoader.Load(_PetNameChange._TicketStoreID, OnStoreLoaded);
	}

	private void OnStoreLoaded(StoreData sd)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		ItemData itemData = sd.FindItem(_PetNameChange._TicketItemID);
		if (itemData != null)
		{
			_PetNameChange.mTicketCost = itemData.FinalCashCost;
		}
		VerifyDragonNameChangeTicket();
	}

	private void VerifyDragonNameChangeTicket()
	{
		if (CommonInventoryData.pInstance.GetQuantity(_PetNameChange._TicketItemID) <= 0)
		{
			mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
			mKAUIGenericDB._MessageObject = base.gameObject;
			mKAUIGenericDB._NoMessage = "KillGenericDB";
			KAUI.SetExclusive(mKAUIGenericDB);
			if (Money.pCashCurrency < _PetNameChange.mTicketCost)
			{
				mKAUIGenericDB.SetText(_PetNameChange._NotEnoughVCashText.GetLocalizedString(), interactive: false);
				mKAUIGenericDB._YesMessage = "ProceedToStore";
			}
			else
			{
				mKAUIGenericDB.SetText(_PetNameChange._UseGemsForNameChangeText.GetLocalizedString().Replace("{cost}", _PetNameChange.mTicketCost.ToString()), interactive: false);
				mKAUIGenericDB._YesMessage = "PurchaseDragonCustomizeTicket";
			}
		}
		else
		{
			ProceedToNameChange();
		}
	}

	private void PurchaseDragonCustomizeTicket()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		CommonInventoryData.pInstance.AddPurchaseItem(_PetNameChange._TicketItemID, 1, ItemPurchaseSource.DRAGON_CUSTOMIZATION.ToString());
		CommonInventoryData.pInstance.DoPurchase(2, _PetNameChange._TicketStoreID, TicketPurchaseHandler);
	}

	private void ProceedToStore()
	{
		KillGenericDB();
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
	}

	private void KillGenericDB()
	{
		if (mKAUIGenericDB != null)
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}

	private void TicketPurchaseHandler(CommonInventoryResponse ret)
	{
		KillGenericDB();
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (ret == null || !ret.Success)
		{
			mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			mKAUIGenericDB.SetText(_PetNameChange._TicketPurchaseFailedText.GetLocalizedString(), interactive: false);
			mKAUIGenericDB._MessageObject = base.gameObject;
			mKAUIGenericDB._OKMessage = "KillGenericDB";
			KAUI.SetExclusive(mKAUIGenericDB);
		}
		else
		{
			ProceedToNameChange();
		}
	}

	private void ProceedToNameChange()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		string[] array = _PetNameSelectBundlePath.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnNameChangeDBLoaded, typeof(GameObject));
	}

	public void OnNameChangeDBLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KillGenericDB();
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.name = "PfUiDragonNameDO";
			UiDragonName component = obj.GetComponent<UiDragonName>();
			if (component != null)
			{
				component.SetNameChangeTicket(_PetNameChange._TicketItemID, _PetNameChange.mPetData);
			}
			OnNameSelectionDone = (PetNameSelectionDoneEvent)Delegate.Combine(OnNameSelectionDone, new PetNameSelectionDoneEvent(OnNameChangeDone));
			RsResourceManager.ReleaseBundleData(inURL);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
		case RsResourceLoadEvent.ERROR:
			KillGenericDB();
			KAUICursorManager.SetDefaultCursor("Arrow");
			Debug.LogError("Failed to load Avatar Equipment....");
			break;
		}
	}

	private void OnNameChangeDone()
	{
		OnNameSelectionDone = (PetNameSelectionDoneEvent)Delegate.Remove(OnNameSelectionDone, new PetNameSelectionDoneEvent(OnNameChangeDone));
		if (_PetNameChange.mMessageObject != null)
		{
			_PetNameChange.mMessageObject.SendMessage("OnNameChangeDone", SendMessageOptions.DontRequireReceiver);
		}
	}

	public GameObject CloneCurrentPet(bool isNullyfyAvtarFollow, Vector3 position, Quaternion rotation)
	{
		if (pCurPetInstance.gameObject != null)
		{
			GameObject obj = UnityEngine.Object.Instantiate(pCurPetInstance.gameObject, position, rotation);
			SanctuaryPet component = obj.GetComponent<SanctuaryPet>();
			if (isNullyfyAvtarFollow && component != null)
			{
				component.mAvatarTarget = null;
			}
			return obj;
		}
		return null;
	}

	public void DestoryCurrentPet()
	{
		if (pCurPetInstance != null)
		{
			UnityEngine.Object.Destroy(pCurPetInstance.gameObject);
			pCurPetInstance = null;
		}
	}
}
