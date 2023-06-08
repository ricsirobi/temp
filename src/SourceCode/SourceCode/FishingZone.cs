using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingZone : KAMonoBase, IAdResult
{
	public enum FishState
	{
		NORMAL,
		FAST,
		STATIC
	}

	[Serializable]
	public class FishStateData
	{
		public FishState _State;

		public float _Tension;

		public float _Duration = 3f;
	}

	public delegate void TutYesNoHandler(bool yes);

	public delegate void TutOkCancelHandler();

	[Serializable]
	public class TutMessage
	{
		public LocaleString _LocaleText;

		public LocaleString _TutStepTextMobile;

		public Vector2 _Position;
	}

	public static UiFishing _FishingZoneUi;

	protected bool mIsTutAvailable = true;

	public static bool pIsTutDone = false;

	public int _ActivationMissionID = -1;

	public int _RodCategoryID = 406;

	public float _TriggerThreshold = 0.1f;

	public float _EngageFloatSpeed = 50f;

	public float _EngageDelayMin = 2f;

	public float _EngageDelayMax = 5f;

	public float _EngageDurationMin = 2f;

	public float _EngageDurationMax = 5f;

	public float _EngageFrequency = 1f;

	public float _FloatOffset;

	public float _StrikeDuration = 2f;

	public float _StrikeFloatOffset = -0.1f;

	public float _BaitLoseTime = 5f;

	public float _BaitLoseZoneWidth = 0.25f;

	public float _LineSnapTime = 6f;

	public float _LineSnapZoneWidth = 0.25f;

	public Color _LoseBaitColor = Color.red;

	public Color _LineSnapColor = Color.red;

	public int _LineSnapStoreReminderCount = 5;

	public float _ReelTimeout;

	public float _PlayerTensionPercentage = 0.12f;

	public ReelFloat _ReelFloat;

	public float _ReelFloatSpeed = 1.125f;

	public float _ReelInterpolationRate = 1f;

	public float _ReelLeftInterpolationRate = 0.75f;

	public int _CaughtFishAchievement = 174;

	public int _CaughtFishClanAchievement = 198;

	public static float _CheatFishWeight = -1f;

	public static float _CheatRodPower = -1f;

	public static int _CheatFishRank = -1;

	public static int _CheatPoleID = -1;

	private bool mCheatBait;

	public string _EndDBPath = "RS_DATA/PfUiDragonsResultsFishingDBDO.unity3d/PfUiDragonsResultsFishingDBDO";

	public const string WIN = "win";

	public const string LOSE = "lose";

	private UiDragonsEndDBFishing mEndDBUI;

	private string mEndDBText;

	private string mEndDBTitle;

	private string mEndDBResult;

	private List<AchievementReward> mFishingRewards;

	public string[] _StateAnimations;

	[HideInInspector]
	public string mPlayerAnimState = "";

	[HideInInspector]
	public bool mReelAnimHigh;

	[HideInInspector]
	public float mFalseStrikeTimer = -1f;

	public string _AvatarWristBone = "Wrist_R_J";

	public Vector3 _FishingRodOffset = Vector3.zero;

	public Fish[] _Fish;

	public string _BaitBarrelPath;

	public GameObject _CurrentFishingRod;

	[HideInInspector]
	public Fish _CurrentFish;

	public Vector3 _CameraOffset = new Vector3(0f, 3f, -4f);

	public Vector3 _ReelCameraOffset = new Vector3(-2f, 1f, -5f);

	public float _BaitBucketDistance = 0.6f;

	public Vector3 _FishDBPosition = new Vector3(0f, 200f, -40f);

	public Vector3 _FishDBRotation = new Vector3(0f, -90f, 0f);

	public float _FishDBScale = 200f;

	public string _GameModuleName = "Fishing";

	protected string mFishingRodBundle;

	private GameObject mCurrentFishGameObject;

	private Animation mCurrentFishAnim;

	protected FishingStates mState;

	protected Fish mCurrentFish;

	protected FishingState mCurrState;

	protected float mTimer;

	protected int mNumCatches;

	protected GameObject mAvatarCam;

	protected CaAvatarCam mCaCam;

	private string mLastPlayedFishAnim;

	private int mLevel;

	private int mXPOffset;

	private Dictionary<FishingStates, Type> mStateHandlers = new Dictionary<FishingStates, Type>();

	private float mFov;

	private Bait mCurrentBait;

	private bool mIsDBShown;

	private GameObject mUiFishModel;

	public TutMessage[] _TutMessages;

	public string _TutorialName = "FishingTutorialKey";

	private UiFishingTutorialDB mTutDB;

	private PairData mPairData;

	private bool mWasFollowAvatar;

	private bool mTutorialRunning;

	[HideInInspector]
	public bool _StrikeFailed;

	public LocaleString _CaughtFishText = new LocaleString("You caught one %fishName%!");

	public LocaleString _EarnedFishText = new LocaleString("You earned one %fishName%");

	public LocaleString _LineSnappedText = new LocaleString("Your fishing line snapped! Try again.");

	public LocaleString _LineSnappedStoreText = new LocaleString("Your fishing line snapped! Try again. If you are having a hard time, visit the store and get a stronger fishing rod!");

	public LocaleString _FishCaughtSuccessTitleText = new LocaleString("Nice Catch");

	public LocaleString _FishCaughtFailureTitleText = new LocaleString("Nice Try");

	public LocaleString _FishEarnedSuccessTitleText = new LocaleString("Good Job");

	public LocaleString _EngageStateText = new LocaleString("Wait!");

	public LocaleString _ReelStateText = new LocaleString("Reel!");

	public LocaleString _LostBaitText = new LocaleString("You lost your bait. Try again.");

	public LocaleString _LostBaitTutorialText = new LocaleString("The fish got away. Try again.");

	public LocaleString _NoBaitText = new LocaleString("You have run out of bait. Buy more bait at the store or find quests with bait rewards.");

	public GameObject _FloatSplash;

	public GameObject _WaterSplash;

	public GameObject _WaterRipple;

	public GameObject _RippleFloatMove;

	public GameObject _FishHitWaterSplash;

	private GameObject mFloatMoveRipple;

	private GameObject mFloatSplash;

	private GameObject mSplash;

	private GameObject mRipple;

	public AudioClip _SndCastLine;

	public AudioClip _SndFloatSplash;

	public AudioClip _SndFishFloatTug;

	public AudioClip _SndStrike;

	public AudioClip _SndCaught;

	public AudioClip _SndFishRodPull;

	public AudioClip _SndFishReelIn;

	public AudioClip _SndFishReelOut;

	public AudioClip _SndFishSplash;

	public AudioClip _SndLineSnap;

	public AudioClip _SndBaitLost;

	private SnChannel mSndStrikeMusic;

	private SnChannel mSndReelIn;

	private SnChannel mSndReelOut;

	public string _DefaultRod = "RS_DATA/PfFishingPole05.unity3d/PfFishingPole05";

	public string _AmbientMusicPool;

	protected bool mIsActive;

	public bool pIsTutAvailable
	{
		get
		{
			if (mIsTutAvailable)
			{
				return !pIsTutDone;
			}
			return false;
		}
	}

	public string pEndDBResult => mEndDBResult;

	public GameObject pCurrentFishGameObject => mCurrentFishGameObject;

	public bool pIsTutorialRunning
	{
		get
		{
			return mTutorialRunning;
		}
		set
		{
			mTutorialRunning = value;
		}
	}

	public UiFishingTutorialDB pFishingTutDB => mTutDB;

	public bool _IsActive => mIsActive;

	public event TutYesNoHandler OnTutYesNo;

	public event TutOkCancelHandler OkCancelHandler;

	private void Awake()
	{
		if (KAInput.pInstance.IsTouchInput())
		{
			for (int i = 0; i < _TutMessages.Length; i++)
			{
				_TutMessages[i]._LocaleText = _TutMessages[i]._TutStepTextMobile;
			}
		}
	}

	public virtual void Start()
	{
		Initialize();
	}

	public void StartAbility()
	{
		if (!mIsActive)
		{
			mIsActive = true;
			if (mCurrState == null)
			{
				mCurrState = GetState(0);
			}
			mCurrState.Enter();
			if (AvAvatar.pObject != null)
			{
				AvAvatar.pObject.GetComponent<AvAvatarController>().pActiveFishingZone = this;
			}
		}
	}

	public void SetState(int newState)
	{
		FishingState prevState = mCurrState;
		if (mCurrState != null)
		{
			mCurrState.Exit();
		}
		mCurrState = GetState(newState);
		if (mCurrState != null)
		{
			mCurrState.Enter();
		}
		else
		{
			UtDebug.LogWarning("Couldn't Find state: " + newState + " for Ability: . Maybe an invalid state was called or the GetState function was not implemented in your Ability subclass.");
			StopAbility();
		}
		OnStateChanged(prevState, mCurrState);
	}

	public void StopAbility()
	{
		mIsActive = false;
		mCurrState.Exit();
		if (_CurrentFishingRod != null)
		{
			_FishingZoneUi.ShowFishingRodUI(bShow: false, reel: false, strike: false);
			UnityEngine.Object.Destroy(_CurrentFishingRod);
			_CurrentFishingRod = null;
			mFishingRodBundle = null;
			RsResourceManager.Unload(mFishingRodBundle);
			RsResourceManager.UnloadUnusedAssets();
		}
		mCurrState = null;
		if (AvAvatar.pObject != null)
		{
			AvAvatar.pObject.GetComponent<AvAvatarController>().pActiveFishingZone = null;
		}
	}

	public virtual void Update()
	{
		if (mIsActive)
		{
			if ((bool)SanctuaryManager.pCurPetInstance && SanctuaryManager.pCurPetInstance.pIsMounted)
			{
				ExitZone(null);
			}
			else
			{
				mCurrState.Execute();
			}
		}
		if (Application.isEditor && Input.GetKey(KeyCode.F) && Input.GetKeyDown(KeyCode.Z))
		{
			AvAvatar.TeleportTo(new Vector3(-28f, 3f, 48f));
		}
		mFalseStrikeTimer -= Time.deltaTime;
	}

	public void SetActive(bool isActive)
	{
		mIsActive = isActive;
	}

	public int GetCurrentStateId()
	{
		if (mCurrState == null)
		{
			return 0;
		}
		return mCurrState.pStateId;
	}

	protected bool PlayerOnRide()
	{
		if (null != SanctuaryManager.pCurPetInstance && SanctuaryManager.pCurPetInstance.GetState() == Character_State.attached)
		{
			return true;
		}
		return false;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!FishingData.pIsReady && FishingData.pInstance != null && other.gameObject.CompareTag("Player"))
		{
			FishingData.pInstance.LoadStoreData();
		}
	}

	protected virtual void OnTriggerStay(Collider other)
	{
		if (RsResourceManager.pLevelLoadingScreen || !FishingData.pIsReady)
		{
			return;
		}
		bool flag = true;
		SphereCollider sphereCollider = collider as SphereCollider;
		if (sphereCollider != null)
		{
			float magnitude = (other.transform.position - (sphereCollider.transform.position + sphereCollider.center)).magnitude;
			if (1f - magnitude / sphereCollider.radius > _TriggerThreshold)
			{
				flag = false;
			}
		}
		if (mIsActive)
		{
			if (!flag)
			{
				ExitZone(other);
			}
		}
		else if (flag && other.gameObject.CompareTag("Player") && AvAvatarSubState.GLIDING != AvAvatar.pSubState && AvAvatarSubState.FLYING != AvAvatar.pSubState && !PlayerOnRide() && CanActivate())
		{
			OnAbilityZoneEntered(null);
			if (_FishingZoneUi == null)
			{
				GameObject obj = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfUiFishing"));
				obj.name = "PfUiFishing";
				_FishingZoneUi = obj.GetComponent<UiFishing>();
			}
			if (_FishingZoneUi != null)
			{
				_FishingZoneUi.SetFishingZone(this);
				_FishingZoneUi.SetVisibility(inVisible: true);
			}
		}
	}

	protected virtual void OnTriggerExit(Collider other)
	{
		ExitZone(other);
	}

	protected void ExitZone(Collider other)
	{
		if ((other == null || other.gameObject.CompareTag("Player")) && mIsActive && CanActivate())
		{
			OnAbilityZoneExit(null);
			if (null != _FishingZoneUi)
			{
				_FishingZoneUi.SetVisibility(inVisible: false);
			}
			mTutorialRunning = false;
		}
	}

	private IEnumerator DestroyTutorialDB()
	{
		yield return new WaitForEndOfFrame();
		if (null != mTutDB)
		{
			UnityEngine.Object.Destroy(mTutDB.gameObject);
		}
	}

	protected bool CanActivate()
	{
		if (_ActivationMissionID > 0)
		{
			if (MissionManager.pInstance != null && MissionManager.pIsReady)
			{
				Mission mission = MissionManager.pInstance.GetMission(_ActivationMissionID);
				if (mission != null && (mission.Accepted || mission.pCompleted))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public void Initialize()
	{
		mStateHandlers.Add(FishingStates.FS_INITIAL, typeof(FishingInitialState));
		mStateHandlers.Add(FishingStates.FS_EQUIP, typeof(FishingEquippedState));
		mStateHandlers.Add(FishingStates.FS_READY, typeof(FishingReadyState));
		mStateHandlers.Add(FishingStates.FS_READYTOCAST, typeof(FishingReadyToCastState));
		mStateHandlers.Add(FishingStates.FS_ENGAGE, typeof(FishingEngageState));
		mStateHandlers.Add(FishingStates.FS_CAST, typeof(FishingCastState));
		mStateHandlers.Add(FishingStates.FS_NIBBLE, typeof(FishingNibbleState));
		mStateHandlers.Add(FishingStates.FS_REEL, typeof(FishingReelState));
		mStateHandlers.Add(FishingStates.FS_SNAP, typeof(FishingSnapState));
		mStateHandlers.Add(FishingStates.FS_CATCH, typeof(FishingCatchState));
		mStateHandlers.Add(FishingStates.FS_ESCAPE, typeof(FishingEscapeState));
		mStateHandlers.Add(FishingStates.FS_SCARE, typeof(FishingScareState));
		pIsTutDone = ProductData.TutorialComplete(_TutorialName);
		if (pIsTutAvailable)
		{
			mTutDB = UiFishingTutorialDB.pInstance;
		}
	}

	public void OnEquipmentChanged()
	{
	}

	public void OnAbilityZoneEntered(object zoneInstance)
	{
		StartAbility();
	}

	public void OnAbilityZoneExit(object zoneInstance)
	{
		StopAbility();
		if (pIsTutAvailable && !FUEManager.pIsFUERunning)
		{
			MainStreetMMOClient.pInstance.ActivateAll(active: true);
		}
	}

	public FishingState GetState(int nState)
	{
		if (mStateHandlers.ContainsKey((FishingStates)nState))
		{
			FishingState obj = (FishingState)Activator.CreateInstance(mStateHandlers[(FishingStates)nState]);
			obj.Initialize(this, nState);
			return obj;
		}
		UtDebug.LogWarning("Invalid state requested: " + nState);
		return null;
	}

	public void OnStateChanged(FishingState prevState, FishingState nextState)
	{
	}

	public void OnUpdate(FishingState currState)
	{
	}

	public bool IsEquipped()
	{
		if (IsFishingRodEquipped())
		{
			return IsBaitEquipped();
		}
		return false;
	}

	public bool IsFishingRodEquipped()
	{
		return GetFishingEquipment() != null;
	}

	public bool IsBaitEquipped()
	{
		if (mCheatBait)
		{
			return true;
		}
		if (AvatarEquipment.pInstance.GetItem(EquipmentParts.BAIT) != null)
		{
			return true;
		}
		return false;
	}

	public FishingRod GetFishingEquipment()
	{
		if (_CurrentFishingRod != null)
		{
			FishingRod component = _CurrentFishingRod.GetComponent<FishingRod>();
			if (component != null && _CheatRodPower > 0f)
			{
				component._RodPower = _CheatRodPower;
				_CheatRodPower = -1f;
			}
			return _CurrentFishingRod.GetComponent<FishingRod>();
		}
		return null;
	}

	public void SnapLine()
	{
	}

	public void UseBait()
	{
		LoseBait();
	}

	public void LoseBait()
	{
		mCurrentBait = null;
		_FishingZoneUi.ShowStartFishingButton(show: false);
		if (mCheatBait)
		{
			mCheatBait = false;
			return;
		}
		AvatarEquipment.pInstance.RemoveItem(EquipmentParts.BAIT.ToString(), removeFromInventory: true);
		AvatarEquipment.pInstance.Save();
	}

	public void DamageRod()
	{
	}

	public Fish SpawnTutorialFish()
	{
		Fish result = null;
		Fish[] fish = _Fish;
		foreach (Fish fish2 in fish)
		{
			if ("Perch" == fish2._Name)
			{
				return fish2;
			}
		}
		return result;
	}

	public Fish SpawnFish(float currTime, float totalTime)
	{
		Fish fish = null;
		if (GetFishingEquipment() == null)
		{
			return null;
		}
		if (pIsTutAvailable && mTutorialRunning)
		{
			return SpawnTutorialFish();
		}
		float num = 0f;
		Fish[] fish2 = _Fish;
		foreach (Fish fish3 in fish2)
		{
			float num2 = Mathf.Max(0f, fish3._AppearanceProbablility + mCurrentBait.GetModifier(fish3._Name));
			if (!(num2 <= 0f))
			{
				num += num2;
			}
		}
		float num3 = UnityEngine.Random.value * num;
		num = 0f;
		fish2 = _Fish;
		foreach (Fish fish4 in fish2)
		{
			float num4 = Mathf.Max(0f, fish4._AppearanceProbablility + mCurrentBait.GetModifier(fish4._Name));
			if (!(num4 <= 0f))
			{
				num += num4;
				if (num3 < num)
				{
					UtDebug.Log("Got: " + num3 + ", " + num);
					fish = fish4;
					break;
				}
			}
		}
		if (fish != null)
		{
			if (_CheatFishRank > 0)
			{
				fish._Rank = _CheatFishRank;
				_CheatFishRank = -1;
			}
			if (_CheatFishWeight > 0f)
			{
				fish._Weight = _CheatFishWeight;
				_CheatFishWeight = -1f;
			}
		}
		return fish;
	}

	public void MakeFishModelActive(bool isActive)
	{
		if (mUiFishModel != null)
		{
			mUiFishModel.SetActive(isActive);
		}
	}

	public void CaughtFishMessage(List<AchievementReward> fishingRewards = null)
	{
		mFishingRewards = fishingRewards;
		string localizedString = _CaughtFishText.GetLocalizedString();
		string fishName = FishingData.pInstance.GetFishName(_CurrentFish.pItemID);
		localizedString = localizedString.Replace("%fishName%", fishName);
		ShowResultDB(localizedString, _FishCaughtSuccessTitleText.GetLocalizedString(), "win");
	}

	public void ShowResultDB(string inText, string inTitle, string inResult)
	{
		if (mEndDBUI == null)
		{
			mEndDBText = inText;
			mEndDBTitle = inTitle;
			mEndDBResult = inResult;
			string[] array = _EndDBPath.Split('/');
			UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnResultsDBLoadedEventHandler, typeof(GameObject));
		}
		else
		{
			ShowDB(inText, inTitle, inResult);
		}
		mIsDBShown = true;
	}

	private void OnResultsDBLoadedEventHandler(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
			if (gameObject != null)
			{
				mEndDBUI = gameObject.GetComponent<UiDragonsEndDBFishing>();
			}
			if (mEndDBUI != null)
			{
				ShowDB(mEndDBText, mEndDBTitle, mEndDBResult);
			}
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("Failed to load...." + inURL);
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			break;
		}
	}

	private void ShowDB(string inText, string inTitle, string inResult)
	{
		_FishingZoneUi.ShowStopFishingButton(show: false);
		mEndDBUI.Initialize();
		mEndDBUI.SetVisibility(Visibility: true);
		mEndDBUI.SetGameSettings(_GameModuleName, base.gameObject, inResult, inShowHighScore: false);
		mEndDBUI.SetResultData("Data0", inText);
		mEndDBUI.SetTitle(inTitle);
		if (inResult == "win")
		{
			OnDBShowAndForFishCaught(mFishingRewards);
		}
		else if (!FUEManager.pIsFUERunning)
		{
			mEndDBUI.EnableAdWidgets();
		}
	}

	public void OnAdWatched()
	{
		AdManager.pInstance.LogAdWatchedEvent(mEndDBUI._AdEventType, "RewardFish");
		UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
		mEndDBUI.SetInteractive(interactive: false);
		CommonInventoryData.pInstance.AddItem(_CurrentFish._ItemID, updateServer: true);
		CommonInventoryData.pInstance.Save(InventorySaveEventHandler, null);
	}

	public void OnAdFailed()
	{
		UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
		mEndDBUI.SetInteractive(interactive: true);
		UtDebug.LogError("OnAdFailed for event:- " + mEndDBUI._AdEventType);
	}

	public void OnAdSkipped()
	{
		UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
		mEndDBUI.SetInteractive(interactive: true);
	}

	public void OnAdClosed()
	{
	}

	public void OnAdFinished(string eventDataRewardString)
	{
	}

	public void OnAdCancelled()
	{
	}

	private void InventorySaveEventHandler(bool success, object inUserData)
	{
		if (success)
		{
			string localizedString = _EarnedFishText.GetLocalizedString();
			string fishName = FishingData.pInstance.GetFishName(_CurrentFish.pItemID);
			localizedString = localizedString.Replace("%fishName%", fishName);
			mEndDBUI.SetResultData("Data0", localizedString);
			mEndDBUI.SetTitle(_FishEarnedSuccessTitleText.GetLocalizedString());
			LoadFish(isAdRewardFish: true);
		}
		else
		{
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			mEndDBUI.SetInteractive(interactive: true);
			CommonInventoryData.pInstance.RemoveItem(_CurrentFish._ItemID, updateServer: false);
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", mEndDBUI._AdRewardFailedText.GetLocalizedString(), null, "");
		}
		AdManager.pInstance.SyncAdAvailableCount(mEndDBUI._AdEventType, success);
	}

	private void OnDBShowAndForFishCaught(List<AchievementReward> fishingRewards)
	{
		ShowFish(updateLayer: false);
		ShowRewards(fishingRewards);
	}

	private void ShowFish(bool updateLayer)
	{
		Vector3 position = mEndDBUI.transform.position;
		position.z = 100f;
		mEndDBUI.transform.position = position;
		if (!mUiFishModel.activeSelf)
		{
			mUiFishModel.SetActive(value: true);
		}
		if (updateLayer)
		{
			UtUtilities.SetLayerRecursively(mUiFishModel, mEndDBUI.gameObject.layer);
		}
		mUiFishModel.transform.parent = mEndDBUI.transform;
		mUiFishModel.GetComponent<Animation>().Play("Idle", PlayMode.StopAll);
		mUiFishModel.transform.localPosition = _FishDBPosition;
		mUiFishModel.transform.rotation = Quaternion.Euler(_FishDBRotation);
		mUiFishModel.transform.localScale = Vector3.one * _FishDBScale;
		mEndDBUI.ShowFishFacts(_CurrentFish);
	}

	private void ShowRewards(List<AchievementReward> fishingRewards = null)
	{
		if (fishingRewards != null && !(mEndDBUI == null))
		{
			Transform transform = mEndDBUI.transform.Find("PfUiGameResults");
			UiGameResults uiGameResults = null;
			if (transform != null)
			{
				uiGameResults = transform.GetComponent<UiGameResults>();
			}
			if (uiGameResults != null)
			{
				uiGameResults.SetRewardDisplay(fishingRewards.ToArray());
			}
		}
	}

	private void OnEndDBClose()
	{
		if (!IsFishingRodEquipped())
		{
			if (_FishingZoneUi != null)
			{
				_FishingZoneUi.ShowStopFishingButton(show: false);
			}
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
		}
		else if (_FishingZoneUi != null)
		{
			_FishingZoneUi.ShowStopFishingButton(show: true);
		}
		if (_FishingZoneUi != null)
		{
			_FishingZoneUi.RemoveFish();
		}
		if (_CurrentFishingRod != null)
		{
			_CurrentFishingRod.GetComponent<FishingRod>().LineSetVisible(visible: false);
		}
		if (_CurrentFish != null && FishingData.pInstance != null)
		{
			RsResourceManager.Unload(FishingData.pInstance.GetFishAssetPath(_CurrentFish._ItemID));
		}
		_CurrentFish = null;
		SetState(0);
		mIsDBShown = false;
		if (mEndDBUI != null)
		{
			UnityEngine.Object.Destroy(mEndDBUI.gameObject);
		}
		if (mUiFishModel != null)
		{
			UnityEngine.Object.Destroy(mUiFishModel);
		}
	}

	private void OnReplayGame()
	{
		OnEndDBClose();
	}

	public bool IsMessageDBShown()
	{
		return mIsDBShown;
	}

	public void StartFishingCam()
	{
		mAvatarCam = AvAvatar.pAvatarCam;
		if (null != mAvatarCam)
		{
			mCaCam = mAvatarCam.GetComponent<CaAvatarCam>();
		}
		if (mCaCam != null)
		{
			mCaCam.SetLayer(CaAvatarCam.CameraLayer.LAYER_FISHING);
			Vector3 position = AvAvatar.GetPosition();
			Vector3 vector = AvAvatar.mTransform.TransformDirection(_CameraOffset);
			position += vector;
			mCaCam.SetMode(CaAvatarCam.CameraMode.MODE_ABSOLUTE);
			mCaCam.SetPosition(position, 0f);
			mCaCam.SetLookAt(_ReelFloat.transform, null, 0f);
			mCaCam.SetSpeed(0.5f);
		}
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (null != component)
		{
			component.pVelocity = Vector3.zero;
		}
	}

	public void StartReelCam()
	{
		if (null != mAvatarCam)
		{
			mCaCam = mAvatarCam.GetComponent<CaAvatarCam>();
		}
		if (mCaCam != null)
		{
			mCaCam.SetLayer(CaAvatarCam.CameraLayer.LAYER_FISHING);
			Vector3 position = AvAvatar.GetPosition();
			Quaternion quaternion = Quaternion.Euler(new Vector3(mCaCam.transform.eulerAngles.x, mCaCam.transform.eulerAngles.y, 0f));
			position += quaternion * _ReelCameraOffset;
			mCaCam.SetMode(CaAvatarCam.CameraMode.MODE_ABSOLUTE);
			mCaCam.SetPosition(position, 0f, 0.5f);
			mCaCam.SetLookAt(_ReelFloat.transform, null, 0f);
			mCaCam.SetSpeed(0.5f);
		}
	}

	public void Reset()
	{
		if (mTutDB != null)
		{
			mTutDB.SetVisibility(inVisible: false);
		}
		if (mCaCam != null)
		{
			mCaCam.SetLayer(CaAvatarCam.CameraLayer.LAYER_AVATAR);
		}
		_FishingZoneUi.SetFishName(null);
		_FishingZoneUi.ShowFishingRodUI(bShow: false, reel: false, strike: false);
		RemoveFish();
		_ReelFloat.HideFloat();
		UnityEngine.Object.Destroy(mFloatMoveRipple);
		UnityEngine.Object.Destroy(mFloatSplash);
		UnityEngine.Object.Destroy(mSplash);
		UnityEngine.Object.Destroy(mRipple);
		_FishingZoneUi.ShowStrikePopupText(show: false);
	}

	public void Exit()
	{
		Reset();
		if (!string.IsNullOrEmpty(_AmbientMusicPool))
		{
			SnChannel.PlayPool(_AmbientMusicPool);
		}
		if (!IsMessageDBShown())
		{
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			if (AvAvatar.pToolbar != null)
			{
				AvAvatar.pToolbar.SendMessage("OnUpdateRank", SendMessageOptions.DontRequireReceiver);
			}
		}
		if (SanctuaryManager.pCurPetInstance != null && mWasFollowAvatar)
		{
			SanctuaryManager.pCurPetInstance.SetFollowAvatar(follow: true);
		}
		if (_CurrentFishingRod != null)
		{
			UnityEngine.Object.Destroy(_CurrentFishingRod);
			_CurrentFishingRod = null;
		}
		RsResourceManager.Unload(mFishingRodBundle);
		RsResourceManager.UnloadUnusedAssets();
	}

	public void StartFishing()
	{
		if (!mCheatBait)
		{
			UserItemData item = AvatarEquipment.pInstance.GetItem(EquipmentParts.BAIT);
			mCurrentBait = new Bait(item.Item);
		}
		SetState(4);
		if (pIsTutAvailable)
		{
			mTutDB.SetVisibility(inVisible: false);
			_FishingZoneUi.ShowBaitPointer(show: false);
			_FishingZoneUi.ShowCastPointer(show: false);
		}
		if (null != _FishingZoneUi)
		{
			_FishingZoneUi.ShowStartFishingButton(show: false);
		}
	}

	public virtual void StopFishing()
	{
		if (7 == GetCurrentStateId())
		{
			if (pIsTutAvailable)
			{
				_StrikeFailed = true;
			}
			SetState(10);
		}
		else
		{
			SetState(0);
		}
		Exit();
	}

	public void ShowFishingButton(bool show)
	{
		if (null != _FishingZoneUi)
		{
			_FishingZoneUi.ShowStartFishingButton(show);
			_FishingZoneUi.ShowFishingRodUI(bShow: true, reel: false, strike: false);
		}
	}

	public void ShowFishingRodButton(bool show)
	{
		if (null != _FishingZoneUi)
		{
			_FishingZoneUi.ShowFishingRodButton(show);
		}
	}

	public void ShowReelbar(bool show, float baitLoseWidth, float lineSnapWidth)
	{
		if (null != _FishingZoneUi)
		{
			_FishingZoneUi.ShowReelbar(show, baitLoseWidth, lineSnapWidth);
		}
	}

	public void UpdateZoneSize(float baitLoseWidth, float lineSnapWidth)
	{
		_FishingZoneUi.SetZoneWidth(baitLoseWidth, lineSnapWidth);
	}

	public virtual void EquipFishingRod()
	{
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		if (!string.IsNullOrEmpty(_AmbientMusicPool))
		{
			SnChannel.PausePool(_AmbientMusicPool);
		}
		if (SanctuaryManager.pCurPetInstance != null)
		{
			mWasFollowAvatar = SanctuaryManager.pCurPetInstance._FollowAvatar;
			if (mWasFollowAvatar)
			{
				SanctuaryManager.pCurPetInstance.SetFollowAvatar(follow: false);
				SanctuaryManager.pCurPetInstance.AIActor.SetState(AISanctuaryPetFSM.IDLE);
			}
		}
		UserItemData[] items = CommonInventoryData.pInstance.GetItems(_RodCategoryID);
		if (items != null)
		{
			ItemData itemData = null;
			int num = -1;
			UserItemData[] array = items;
			foreach (UserItemData userItemData in array)
			{
				int value = userItemData.Item.RankId.Value;
				if (value >= num)
				{
					num = value;
					itemData = userItemData.Item;
				}
			}
			mFishingRodBundle = itemData.AssetName;
		}
		else
		{
			mFishingRodBundle = _DefaultRod;
		}
		if (_CheatPoleID != -1)
		{
			UserItemData userItemData2 = CommonInventoryData.pInstance.FindItem(_CheatPoleID);
			if (userItemData2 != null)
			{
				mFishingRodBundle = userItemData2.Item.AssetName;
			}
		}
		string[] array2 = mFishingRodBundle.Split('/');
		RsResourceManager.LoadAssetFromBundle(array2[0] + "/" + array2[1], array2[2], OnRodLoadingEvent, typeof(GameObject));
	}

	public virtual void SetupBaitBucket(GameObject baitBucket, BaitMenu.OnShowHandler handler)
	{
		baitBucket.transform.position = AvAvatar.GetPosition() + -AvAvatar.mTransform.right * _BaitBucketDistance + Vector3.up * 0.03f;
		baitBucket.transform.parent = base.transform;
		BaitMenu componentInChildren = baitBucket.GetComponentInChildren<BaitMenu>();
		if (componentInChildren != null)
		{
			componentInChildren._FishingZone = this;
			componentInChildren.SetNoBaitText(_NoBaitText);
			componentInChildren.OnShow += handler;
		}
	}

	public virtual void DestroyBaitBucket(GameObject baitBucket, BaitMenu.OnShowHandler handler)
	{
		BaitMenu componentInChildren = baitBucket.GetComponentInChildren<BaitMenu>();
		if (componentInChildren != null)
		{
			componentInChildren.OnShow -= handler;
		}
		UnityEngine.Object.Destroy(baitBucket);
	}

	public void OnRodLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent != RsResourceLoadEvent.COMPLETE)
		{
			_ = 3;
			return;
		}
		_CurrentFishingRod = UnityEngine.Object.Instantiate((GameObject)inObject);
		Transform transform = UtUtilities.FindChildTransform(AvAvatar.pObject, _AvatarWristBone);
		_CurrentFishingRod.transform.parent = transform.transform;
		_CurrentFishingRod.transform.localPosition = _FishingRodOffset;
		_CurrentFishingRod.transform.localRotation = Quaternion.identity;
	}

	public void DoReel(float reelvalue)
	{
		_FishingZoneUi.SetMarkerPosition(reelvalue);
		if (_ReelFloat.pIsCaptured)
		{
			SetState(8);
		}
	}

	public void RotateReel(float force)
	{
		_FishingZoneUi.RotateReelGear(force);
	}

	public void RandomBait()
	{
		mCurrentBait = new Bait(null);
		mCheatBait = true;
	}

	private void OnDestroy()
	{
		if (null != _FishingZoneUi)
		{
			UnityEngine.Object.Destroy(_FishingZoneUi.gameObject);
		}
		_FishingZoneUi = null;
	}

	public void BaitSelected()
	{
		UserItemData item = AvatarEquipment.pInstance.GetItem(EquipmentParts.BAIT);
		mCurrentBait = new Bait(item.Item);
	}

	public void LoadFish(bool isAdRewardFish = false)
	{
		string[] array = FishingData.pInstance.GetFishAssetPath(_CurrentFish._ItemID).Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnAssetLoadingEvent, typeof(GameObject), inDontDestroy: false, isAdRewardFish);
	}

	public void OnAssetLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (!(bool)inUserData)
			{
				mCurrentFishGameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
				mCurrentFishAnim = mCurrentFishGameObject.GetComponent<Animation>();
				if (mCurrentFishGameObject != null && mCurrentFishAnim != null && _CurrentFishingRod != null)
				{
					mCurrentFishAnim.Stop();
					mCurrentFishAnim.playAutomatically = false;
					mCurrentFishGameObject.name = _CurrentFish.pName;
					_ReelFloat.AttachFish(mCurrentFishGameObject.transform);
					Vector3 position = _CurrentFishingRod.transform.position;
					position.y = mCurrentFishGameObject.transform.position.y;
					mCurrentFishGameObject.transform.LookAt(position);
					mCurrentFishGameObject.transform.localScale = Vector3.one;
					_CurrentFishingRod.GetComponent<FishingRod>()._Fish = mCurrentFishGameObject.transform;
					mUiFishModel = UnityEngine.Object.Instantiate((GameObject)inObject);
					if (_FishingZoneUi != null)
					{
						_FishingZoneUi.ShowFish(mUiFishModel);
					}
				}
			}
			else
			{
				mUiFishModel = UnityEngine.Object.Instantiate((GameObject)inObject);
				UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
				mEndDBUI.SetInteractive(interactive: true);
				ShowFish(updateLayer: true);
			}
			break;
		case RsResourceLoadEvent.ERROR:
			if ((bool)inUserData)
			{
				UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
				mEndDBUI.SetInteractive(interactive: true);
			}
			break;
		}
	}

	public void RemoveFish()
	{
		_ReelFloat.RemoveFish();
		mCurrentFishGameObject = null;
		if (null != _CurrentFishingRod)
		{
			FishingRod component = _CurrentFishingRod.GetComponent<FishingRod>();
			if (null != component)
			{
				component._Fish = null;
			}
		}
	}

	public float PlayFishAnim(string animName)
	{
		if (null != mCurrentFishGameObject && mCurrentFishAnim[animName] != null)
		{
			if (!mCurrentFishAnim.IsPlaying(animName))
			{
				mCurrentFishAnim.CrossFade(animName, 0.25f);
			}
			return mCurrentFishAnim[animName].length;
		}
		return 0f;
	}

	public void MoveFishUpToSurface(float speed)
	{
		if ((bool)mCurrentFishGameObject)
		{
			Vector3 position = mCurrentFishGameObject.transform.position;
			position.y = Mathf.MoveTowards(position.y, _ReelFloat._Float.position.y + 0.3f, Time.deltaTime * speed);
			mCurrentFishGameObject.transform.position = position;
		}
	}

	public void PlayRodAnim(float weight)
	{
		if (_CurrentFishingRod != null)
		{
			string text = "Bent";
			Animation component = _CurrentFishingRod.GetComponent<Animation>();
			if (component != null && component.GetClip("Bent") != null)
			{
				component["Bent"].layer = 1;
				component["Bent"].enabled = true;
				component.Blend("Straight", 1f - weight, 0.05f);
				component.Blend("Bent", weight, 0.05f);
			}
			else
			{
				UtDebug.LogError("_CurrentFishingRod does not have animation : " + text);
			}
		}
		else
		{
			UtDebug.LogError("_CurrentFishingRod is null...");
		}
	}

	public void StartedReel()
	{
	}

	public void CastLine()
	{
	}

	public void Strike()
	{
	}

	public void StartTutorial()
	{
		mTutDB = UiFishingTutorialDB.pInstance;
		if (mTutDB != null)
		{
			mTutDB.mGameMgrObj = base.gameObject;
			mTutDB.SetVisibility(inVisible: true);
		}
	}

	public void OnTutorialDone()
	{
		if (mTutDB != null)
		{
			UnityEngine.Object.Destroy(mTutDB.gameObject);
			mTutDB = null;
		}
		pIsTutDone = true;
		mTutorialRunning = false;
		if (!FUEManager.pIsFUERunning)
		{
			MainStreetMMOClient.pInstance.ActivateAll(active: true);
		}
		ProductData.AddTutorial(_TutorialName);
	}

	private void OnGenericInfoBoxYes()
	{
		mTutDB = null;
		if (this.OnTutYesNo != null)
		{
			this.OnTutYesNo(yes: true);
		}
	}

	private void TutOnDone()
	{
		mTutDB = null;
		if (this.OkCancelHandler != null)
		{
			this.OkCancelHandler();
		}
	}

	public void PlayRipple(bool bPlay)
	{
		if (null == _WaterRipple)
		{
			return;
		}
		if (bPlay)
		{
			if (null == mRipple)
			{
				mRipple = UnityEngine.Object.Instantiate(_WaterRipple);
			}
			mRipple.transform.position = _ReelFloat._Float.transform.position;
			Transform transform = _ReelFloat.transform.Find("WaterLevel");
			if (null != transform)
			{
				Vector3 position = mRipple.transform.position;
				position.y = transform.transform.position.y;
				mRipple.transform.position = position;
			}
			mRipple.SetActive(bPlay);
		}
		else if (null != mRipple)
		{
			mRipple.SetActive(bPlay);
		}
	}

	public void PlaySplash(bool bPlay)
	{
		if (null == _WaterSplash || mCurrentFishGameObject == null || _ReelFloat == null)
		{
			return;
		}
		Transform transform = mCurrentFishGameObject.transform.Find("MainRoot/Root/Tail");
		if (null == transform)
		{
			return;
		}
		ParticleSystem particleSystem = null;
		if (mSplash != null)
		{
			particleSystem = mSplash.GetComponent<ParticleSystem>();
		}
		if (bPlay && (!mCurrentFishAnim.IsPlaying("Jump") || !(transform.position.y <= _ReelFloat._Float.position.y)))
		{
			if (null == mSplash)
			{
				mSplash = UnityEngine.Object.Instantiate(_WaterSplash);
				particleSystem = mSplash.GetComponent<ParticleSystem>();
			}
			mSplash.transform.position = mCurrentFishGameObject.transform.position;
			Transform transform2 = _ReelFloat.transform.Find("WaterLevel");
			if (null != transform2)
			{
				Vector3 position = mSplash.transform.position;
				position.y = transform2.transform.position.y;
				mSplash.transform.position = position;
			}
			if (!mSplash.gameObject.activeSelf)
			{
				mSplash.gameObject.SetActive(value: true);
			}
			particleSystem.time = 0f;
			particleSystem.Play();
			if (null != _SndFishSplash)
			{
				SnChannel.Play(_SndFishSplash, "", inForce: true).pLoop = false;
			}
		}
		else if (null != mSplash)
		{
			particleSystem.time = 0f;
			particleSystem.Stop();
			mSplash.SetActive(value: false);
		}
	}

	private void PlayFloatSplashSound()
	{
		if (null != _SndFloatSplash)
		{
			SnChannel.Play(_SndFloatSplash, "SFX_Pool", inForce: true, null).pLoop = false;
		}
	}

	public void PlayFloatSplash(bool bPlay)
	{
		if (null == _FloatSplash)
		{
			return;
		}
		if (bPlay)
		{
			mFloatSplash = UnityEngine.Object.Instantiate(_FloatSplash);
			mFloatSplash.transform.position = _ReelFloat._Float.position;
			Transform transform = _ReelFloat.transform.Find("WaterLevel");
			if (null != transform)
			{
				Vector3 position = mFloatSplash.transform.position;
				position.y = transform.transform.position.y;
				mFloatSplash.transform.position = position;
			}
			Invoke("PlayFloatSplashSound", 0.5f);
			mFloatSplash.SetActive(bPlay);
		}
		else if (mFloatSplash != null)
		{
			mFloatSplash.SetActive(bPlay);
			UnityEngine.Object.Destroy(mFloatSplash);
		}
	}

	public void PlayFloatMoveRipple(bool bPlay)
	{
		if (null == _RippleFloatMove)
		{
			return;
		}
		if (bPlay)
		{
			if (null == mFloatMoveRipple)
			{
				mFloatMoveRipple = UnityEngine.Object.Instantiate(_RippleFloatMove);
			}
			mFloatMoveRipple.transform.position = _ReelFloat._Float.position;
			Transform transform = _ReelFloat.transform.Find("WaterLevel");
			if (null != transform)
			{
				Vector3 position = mFloatMoveRipple.transform.position;
				position.y = transform.transform.position.y;
				mFloatMoveRipple.transform.position = position;
			}
			mFloatMoveRipple.SetActive(bPlay);
		}
		else if (mFloatMoveRipple != null)
		{
			mFloatMoveRipple.SetActive(bPlay);
		}
	}

	public void PlayStopStrikeMusic(bool Play, bool loop = true)
	{
		if (Play)
		{
			if (null != _SndStrike)
			{
				mSndStrikeMusic = SnChannel.Play(_SndStrike, "Music_Pool", inForce: true, null);
			}
			if (loop && null != mSndStrikeMusic)
			{
				mSndStrikeMusic.pLoop = loop;
			}
		}
		else if (null != mSndStrikeMusic)
		{
			mSndStrikeMusic.pLoop = false;
			mSndStrikeMusic.Stop();
		}
	}

	public void OnStoreOpened()
	{
		if (pIsTutAvailable && mTutorialRunning && null != mTutDB)
		{
			mTutDB.SetVisibility(inVisible: false);
		}
		if (mUiFishModel != null)
		{
			mUiFishModel.SetActive(value: false);
		}
		if (_FishingZoneUi != null)
		{
			_FishingZoneUi.SetVisibility(inVisible: false);
		}
		AvAvatar.pState = AvAvatarState.PAUSED;
		if (!UtMobileUtilities.CanLoadInCurrentScene(UiType.Store, UILoadOptions.AUTO))
		{
			StopFishing();
			OnEndDBClose();
		}
	}

	public void OnStoreClosed()
	{
		if (pIsTutAvailable && mTutorialRunning && null != mTutDB)
		{
			mTutDB.SetVisibility(inVisible: true);
		}
		if (mUiFishModel != null)
		{
			mUiFishModel.SetActive(value: true);
		}
		if (_FishingZoneUi != null)
		{
			_FishingZoneUi.SetVisibility(inVisible: true);
		}
		if (_CurrentFishingRod == null)
		{
			EquipFishingRod();
		}
		AvAvatar.pState = AvAvatarState.PAUSED;
	}
}
