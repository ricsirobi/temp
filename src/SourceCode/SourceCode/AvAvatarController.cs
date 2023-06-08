using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KA.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class AvAvatarController : SplineControl
{
	[Serializable]
	public class FxData
	{
		public GameObject _Fx;

		public Transform _Bone;

		public Vector3 _Offset;
	}

	public class CarriedObject
	{
		public GameObject _CarriedObject;

		public bool _IsDuplicated;

		public CarriedObject(GameObject go, bool duplicate)
		{
			if (duplicate)
			{
				_CarriedObject = UnityEngine.Object.Instantiate(go);
			}
			else
			{
				_CarriedObject = go;
			}
			_IsDuplicated = duplicate;
		}

		public void Delete(bool forceDestroy = false)
		{
			if (_IsDuplicated || forceDestroy)
			{
				UnityEngine.Object.Destroy(_CarriedObject);
				return;
			}
			_CarriedObject.SendMessage("DropObject", SendMessageOptions.DontRequireReceiver);
			_CarriedObject = null;
		}
	}

	public enum SwimAnimState
	{
		NONE,
		SPLASH,
		SWIM,
		IDLE,
		UWSWIM,
		UWIDLE,
		UWBRAKE
	}

	public delegate void OnTakeDamageDelegate(GameObject go, ref float damage);

	public delegate void OnAvatarCollisionDelegate(GameObject avatarObj, GameObject hitObject);

	public delegate void OnAvatarStateChanged();

	public delegate void OnAvatarHealthDelegate(float currentHealth);

	private enum AvatarControls
	{
		MOVE_FORWARD,
		MOVE_BACKWARD,
		TURN_LEFT,
		TURN_RIGHT
	}

	public delegate void PetFlyingStateChanged(FlyingState newState);

	public float _ObjectDragAngleLimit = 50f;

	public float _ObjectDragSnapPosLimit = 0.3f;

	public string _ObjectDragTag = "Draggable";

	public string _MainRootName = "";

	public Transform _MainRoot;

	public AvAvatarControlMode _ControlMode;

	public AvAvatarStateData[] _StateData;

	public float _Height;

	public float _GroundSnapThreshold = 0.6f;

	public float _FireballImmuneTime = 1f;

	public int _AttackDamage = 1;

	public AvAvatarShooting _AvatarShootingData;

	public string[] _ProjectileFiringAreas;

	public string[] _AvatarAttackingAreas;

	public GameObject _CoinsReceivedPrtPrefab;

	public Vector3 _CoinsReceivedPrtOffset = Vector3.up * 1.5f;

	public bool _ActivateOnAwake;

	public float _GlideModeAutoHeight = 2f;

	public Transform _FlyingBone;

	public bool _NoFlyingLanding;

	public AvAvatarBounceParams _AvatarBounceParams;

	public SnRandomSound _JumpTriggerSound;

	public string _CarryAnim;

	public Vector3 _CarryOffset;

	public AvAvatarWallClimbParams _WallClimbParams;

	public Vector3 _DisplayNameOffset = new Vector3(0f, 2f, 0f);

	public Vector3 _EmoticonOffset = new Vector3(0f, 2f, 0f);

	public LocaleString _NoOpenChatText;

	public AvAvatarChatSizes[] _ChatSizes;

	public Transform _ChatBubbleParent;

	public List<ModifierAttributes> _ModifierAttributes = new List<ModifierAttributes>();

	public int _GlidingAngle = 60;

	[Range(-1f, 1f)]
	public float _InitialGlideThrust = -0.4f;

	public float _RemoveButtonTimer = 10f;

	public int _FlightSuitCategoryIDs = 228;

	public FxData[] _GlidingFxData;

	public bool _ApplyGlideFxOnDive;

	public float _FlySpeedLimitToSwim = 2f;

	public string _SoarTutorialName = "Soar";

	public string _RemoveTutorialName = "RemoveFlightSuit";

	public string _LastEquippedFlightSuitKey = "LEFS";

	public GameObject _WaterSplashParticle;

	public GameObject _SwimParticle;

	public GameObject _SwimIdleParticle;

	public GameObject _WaterSkimParticle;

	public Transform _SwimParticleBone;

	public Transform _HandBone;

	public AvAvatarBlink _Blink;

	public AvAvatarStats _Stats = new AvAvatarStats();

	public Vector3 _CSMColliderMountedPosition;

	public Vector3 _CSMColliderUnmountedPosition;

	public float _CSMColliderMountedHeight = 1f;

	public float _CSMColliderUnmountedHeight = 1.6f;

	public float _CSMColliderRadius = 0.3f;

	public string _CSMRollOverCurserName = "Activate";

	public const string AVATAR_CSM_COLLIDER_TAG = "AvatarCSMCollider";

	public float _DisplayNameStartHeight = 1.5f;

	public float _DisplayNameSpacing = 0.2f;

	private ItemData mMemberFlightSuit;

	private bool mIsSoarTutorialDone;

	private bool mIsRemoveTutorialDone;

	private float mGlideEndTime;

	private bool mWasPlayerGliding;

	private bool mHealthInitialized;

	private bool mWasPlayerMounted;

	private bool mWasDragonMountEnabled = true;

	private string mGender = "";

	private AvatarCustomization mAvatarCustomization;

	private bool mLockVelocity;

	private Vector3 mLockedVelocity;

	private UiToolbar mToolBar;

	private List<GameObject> mGlidingFxList;

	private float mMMOPitch;

	private float mMMORoll;

	private int mFlightSuitStoreID = -1;

	private bool mRefreshFlightData;

	private bool mIsPlayerGliding;

	private bool mIsInitialGlideThrustStarted;

	private AvAvatarFlyingData mFlyingData;

	private AvAvatarUWSwimmingData mUWSwimmingData;

	private bool mPlayerMounted;

	private bool mPlayerCarrying;

	private bool mPlayerDragging;

	private GameObject mEndSplineMessageObject;

	private CharacterController mController;

	private AvAvatarProperties mProperties;

	private AvAvatarState mState;

	private AvAvatarState mPrevState;

	private AvAvatarSubState mSubState;

	private AvAvatarStateData mCurrentStateData;

	private Vector3 mVelocity;

	private Vector3 mDirection;

	private float mCurSpeed;

	private float mRotation;

	private float mRotVel;

	private CollisionFlags collisionFlags;

	private float mLastTimeOnGround;

	private float mFidgetTime;

	private float mVi;

	private Vector3 mForwardCollisionNormal;

	private Vector3 mGroundCollisionNormal;

	private bool mCanJump;

	private GameObject mActivateObject;

	private GameObject mWaterObject;

	private string mCurrentAnim = "none";

	private string mChatBubbleSize = "";

	private float mSlopeLimit;

	private float mStepOffset;

	private float mImmuneTimer;

	private float mPlatformHitTime;

	private bool mTakeAvatarMoney = true;

	private float mMeanGlideSpeed;

	private float mSkateSpeed;

	private float mFlyTakeOffTimer = 3f;

	private bool mIsBouncing;

	private float mBounceVelocity;

	private GameObject mBouncingObject;

	private bool mTriggerJump;

	private SnISound mJumpSound;

	private CarriedObject mCarriedObject;

	private GameObject mDragObject;

	private ObDraggable mDraggable;

	private Transform mHandleMarker;

	private Transform mNewParent;

	private float mGravityMultiplier = 1f;

	private float mGlideModeSpeedMultiplier = 1f;

	private AvAvatarStateData[] mModifiedStateData;

	private static Dictionary<string, string> mModifierFieldMap = new Dictionary<string, string>();

	private GameObject mGlidingParentObject;

	private SwimAnimState mSwimAnimState;

	private float mWaterSplashInterval = 1f;

	private float mWaterCheckTime;

	private GameObject mWaterParticles;

	private Vector3 mPushVelocity = Vector3.zero;

	private float mPushTimer;

	private bool mCanRegenHealth;

	private bool mCanGlide = true;

	private SpawnPool mParticlePool;

	private SphereCollider mProjCollider;

	private CapsuleCollider mCSMCollider;

	private FishingZone mActiveFishingZone;

	private Transform mFlyingRotationPivot;

	private FlyingState mFlyingState = FlyingState.TakeOff;

	private Vector3 mMouseStart;

	private PropStates mCurrentUsedProp;

	private Vector3 DEFAULT_GROUND_POS = Vector3.up * 5000f;

	private Vector3 mLastPositionOnGround;

	private float mFlashingTimer;

	private float mFlashingInterval;

	private List<Renderer> mActiveRenderers = new List<Renderer>();

	private Animator mAnimator;

	private AvAvatarAnim mAvAvatarAnim;

	private float mClimbAnimTime;

	private SkinnedMeshRenderer mFlightArmorWing;

	private bool mDragSmall;

	private bool mAvatarHidden;

	private bool mAvatarFlashing;

	private PsPet mPetObject;

	public OnTakeDamageDelegate OnTakeDamage;

	public OnAvatarCollisionDelegate OnAvatarCollision;

	public OnAvatarStateChanged OnAvatarStateChange;

	public OnAvatarHealthDelegate OnAvatarHealth;

	public const float MinHoverSpeed = 1f;

	public static bool mAimControlMode = true;

	public static bool mForceBraking = false;

	public float _DefaultFlightPitch = 15f;

	public float _DefaultFlightDistance = 8f;

	public float _GlidingTakeOffSpeed = 20f;

	public float _GlideTakeOffTimer = 0.5f;

	public float _JoySensitivity = 0.7f;

	public float _BonusMaxSpeedWithBoost = 0.2f;

	public float _AccelValueWithBoost = 2f;

	public float _WaterDrag = 1f;

	[HideInInspector]
	public bool mDisplayFlyingData;

	private float mFlyingFlapCooldownTimer;

	private float mMaxFlightSpeed;

	private float mFlyingBoostTimer;

	private bool mIsBraking;

	private bool mFlyingGlidingMode;

	private float mRoll;

	private float mPitch;

	private float mFlightSpeed;

	private float mOriginalBonusMaxSpeedWithBoost;

	private float mFlyingPositionBoost;

	private float mTurnFactor = 1f;

	private float mGlideTakeOffTimer;

	private float mSlowPenaltyStrength;

	private float mSlowPenaltyStep;

	public static bool mForceBrakeUWSwimming = false;

	public float _DefaultUWSwimmingPitch = 15f;

	public float _DefaultUWSwimDistance = 8f;

	public float _MaxUWSwimSpeedWithBoost = 0.2f;

	public float _UWSwimAccelValueWithBoost = 2f;

	public AudioClip _PlayerHealthZeroSFX;

	public GameObject _UWSwimParticle;

	public GameObject _UWSwimIdleParticle;

	public GameObject _UWSwimBrakeParticle;

	private float mUWSwimmingBoostCooldownTimer;

	private float mMaxUWSwimmingSpeed;

	private float mUWSwimmingBoostTimer;

	private bool mIsUWSwimBraking;

	private float mUWSwimmingRoll;

	private float mUWSwimmingPitch;

	private float mUWSwimmingSpeed;

	private float mOriginalMaxUWSwimSpeedWithBoost;

	private float mUWSwimmingTurnFactor = 1f;

	public bool DisableSoarButtonUpdate { get; set; }

	public bool PlayingFlightSchoolFlightSuit { get; set; }

	public bool AvatarHidden
	{
		get
		{
			return mAvatarHidden;
		}
		set
		{
			mAvatarHidden = value;
			if (_Blink != null)
			{
				_Blink.enabled = !mAvatarHidden;
			}
		}
	}

	public bool pCanRegenHealth
	{
		get
		{
			return mCanRegenHealth;
		}
		set
		{
			mCanRegenHealth = value;
		}
	}

	public bool pCanGlide
	{
		get
		{
			return mCanGlide;
		}
		set
		{
			mCanGlide = value;
		}
	}

	public bool pCanJump => mCanJump;

	public FishingZone pActiveFishingZone
	{
		get
		{
			return mActiveFishingZone;
		}
		set
		{
			if (mPlayerCarrying)
			{
				RemoveCarriedObject();
			}
			mActiveFishingZone = value;
		}
	}

	public bool pIsFishing
	{
		get
		{
			if (mActiveFishingZone != null && mActiveFishingZone.GetCurrentStateId() >= 0 && pState != AvAvatarState.FALLING)
			{
				if (pSubState == AvAvatarSubState.SWIMMING)
				{
					return pPlayerMounted;
				}
				return true;
			}
			return false;
		}
	}

	public Transform pFlyingRotationPivot
	{
		get
		{
			return mFlyingRotationPivot;
		}
		set
		{
			mFlyingRotationPivot = value;
		}
	}

	public float pFidgetTime
	{
		get
		{
			return mFidgetTime;
		}
		set
		{
			mFidgetTime = value;
		}
	}

	public float pFlyTakeOffTimer
	{
		get
		{
			return mFlyTakeOffTimer;
		}
		set
		{
			mFlyTakeOffTimer = value;
		}
	}

	public GameObject pEndSplineMessageObject
	{
		get
		{
			return mEndSplineMessageObject;
		}
		set
		{
			mEndSplineMessageObject = value;
		}
	}

	public GameObject pCarriedObject
	{
		get
		{
			if (mCarriedObject == null)
			{
				return null;
			}
			return mCarriedObject._CarriedObject;
		}
	}

	public float pGravityMultiplier
	{
		get
		{
			return mGravityMultiplier;
		}
		set
		{
			mGravityMultiplier = value;
		}
	}

	public float pGlideModeSpeedMultiplier
	{
		get
		{
			return mGlideModeSpeedMultiplier;
		}
		set
		{
			mGlideModeSpeedMultiplier = value;
		}
	}

	public Vector3 pVelocity
	{
		get
		{
			return mVelocity;
		}
		set
		{
			mVelocity = value;
		}
	}

	public float pRotation
	{
		get
		{
			return mRotation;
		}
		set
		{
			mRotation = value;
		}
	}

	public float pRotVel
	{
		get
		{
			return mRotVel;
		}
		set
		{
			mRotVel = value;
		}
	}

	public float pGravity
	{
		get
		{
			return mCurrentStateData._Gravity;
		}
		set
		{
			mCurrentStateData._Gravity = value;
		}
	}

	public float pMaxForwardSpeed => mCurrentStateData._MaxForwardSpeed;

	public bool pImmune
	{
		get
		{
			return mImmuneTimer > 0f;
		}
		private set
		{
		}
	}

	public CharacterController pController => mController;

	public AvAvatarProperties pProperties => mProperties;

	public AvAvatarState pState
	{
		get
		{
			return mState;
		}
		set
		{
			if (AvAvatar.IsCurrentPlayer(base.gameObject))
			{
				AvAvatar.pState = value;
				return;
			}
			mPrevState = mState;
			mState = value;
			OnSetState();
		}
	}

	public AvAvatarSubState pSubState
	{
		get
		{
			return mSubState;
		}
		set
		{
			if (AvAvatar.IsCurrentPlayer(base.gameObject))
			{
				AvAvatar.pSubState = value;
				return;
			}
			AvAvatarSubState oldSubState = mSubState;
			mSubState = value;
			OnSetSubState(oldSubState);
		}
	}

	public AvAvatarStateData pCurrentStateData
	{
		get
		{
			return mCurrentStateData;
		}
		set
		{
			mCurrentStateData = value;
		}
	}

	public float pLastTimeOnGround
	{
		get
		{
			return mLastTimeOnGround;
		}
		set
		{
			mLastTimeOnGround = value;
		}
	}

	public string pCurrentAnim
	{
		get
		{
			return mCurrentAnim;
		}
		set
		{
			mCurrentAnim = value;
		}
	}

	public Vector3 pLastPositionOnGround
	{
		get
		{
			return mLastPositionOnGround;
		}
		set
		{
			mLastPositionOnGround = value;
		}
	}

	public bool pPlayerMounted
	{
		get
		{
			return mPlayerMounted;
		}
		set
		{
			mPlayerMounted = value;
		}
	}

	public bool pPlayerDragging
	{
		get
		{
			return mPlayerDragging;
		}
		set
		{
			mPlayerDragging = value;
		}
	}

	public bool pPlayerCarrying
	{
		get
		{
			return mPlayerCarrying;
		}
		set
		{
			mPlayerCarrying = value;
		}
	}

	public bool pIsPlayerGliding
	{
		get
		{
			return mIsPlayerGliding;
		}
		set
		{
			mIsPlayerGliding = value;
		}
	}

	public AvAvatarFlyingData pFlyingData
	{
		get
		{
			return mFlyingData;
		}
		set
		{
			mFlyingData = value;
			_BonusMaxSpeedWithBoost = mOriginalBonusMaxSpeedWithBoost;
		}
	}

	public FlyingState pFlyingState => mFlyingState;

	public AvAvatarUWSwimmingData pUWSwimmingData
	{
		get
		{
			return mUWSwimmingData;
		}
		set
		{
			mUWSwimmingData = value;
			_MaxUWSwimSpeedWithBoost = mOriginalMaxUWSwimSpeedWithBoost;
		}
	}

	public AvatarCustomization pAvatarCustomization
	{
		get
		{
			if (mAvatarCustomization == null)
			{
				mAvatarCustomization = new AvatarCustomization();
			}
			return mAvatarCustomization;
		}
	}

	public bool pLockVelocity
	{
		set
		{
			mLockVelocity = value;
			mLockedVelocity = mVelocity;
		}
	}

	public AvAvatarState pPrevState
	{
		get
		{
			return mPrevState;
		}
		set
		{
			mPrevState = value;
		}
	}

	public bool DragSmall
	{
		get
		{
			return mDragSmall;
		}
		set
		{
			mDragSmall = value;
		}
	}

	public PsPet pPetObject
	{
		get
		{
			return mPetObject;
		}
		set
		{
			mPetObject = value;
		}
	}

	public float pFlyingFlapCooldownTimer
	{
		get
		{
			return mFlyingFlapCooldownTimer;
		}
		set
		{
			mFlyingFlapCooldownTimer = value;
		}
	}

	public float pFlyingBoostTimer
	{
		get
		{
			return mFlyingBoostTimer;
		}
		set
		{
			mFlyingBoostTimer = value;
		}
	}

	public float pMaxFlightSpeed => mMaxFlightSpeed;

	public bool pIsBraking => mIsBraking;

	public bool pFlyingGlidingMode
	{
		get
		{
			return mFlyingGlidingMode;
		}
		set
		{
			mFlyingGlidingMode = value;
		}
	}

	public float pFlyingRoll
	{
		get
		{
			return mRoll;
		}
		set
		{
			mRoll = value;
		}
	}

	public float pFlyingPitch
	{
		get
		{
			return mPitch;
		}
		set
		{
			mPitch = value;
		}
	}

	public float pFlightSpeed
	{
		get
		{
			return mFlightSpeed;
		}
		set
		{
			mFlightSpeed = value;
		}
	}

	public float pFlyingPositionBoost
	{
		get
		{
			if (AvAvatar.pLevelState != AvAvatarLevelState.RACING)
			{
				return 0f;
			}
			return mFlyingPositionBoost;
		}
		set
		{
			mFlyingPositionBoost = value;
		}
	}

	public bool pIsTransiting { get; set; }

	public UWSwimmingZone pUWSwimZone { get; set; }

	public float pUWSwimmingBoostCooldownTimer
	{
		get
		{
			return mUWSwimmingBoostCooldownTimer;
		}
		set
		{
			mUWSwimmingBoostCooldownTimer = value;
		}
	}

	public float pUWSwimmingBoostTimer
	{
		get
		{
			return mUWSwimmingBoostTimer;
		}
		set
		{
			mUWSwimmingBoostTimer = value;
		}
	}

	public float pMaxUWSwimmingSpeed => mMaxUWSwimmingSpeed;

	public bool pIsUWSwimBraking => mIsUWSwimBraking;

	public float pUWSwimmingRoll
	{
		get
		{
			return mUWSwimmingRoll;
		}
		set
		{
			mUWSwimmingRoll = value;
		}
	}

	public float pUWSwimmingPitch
	{
		get
		{
			return mUWSwimmingPitch;
		}
		set
		{
			mUWSwimmingPitch = value;
		}
	}

	public float pUWSwimmingSpeed
	{
		get
		{
			return mUWSwimmingSpeed;
		}
		set
		{
			mUWSwimmingSpeed = value;
		}
	}

	public event PetFlyingStateChanged OnPetFlyingStateChanged;

	public void ResetLastPositionOnGround()
	{
		mLastPositionOnGround = DEFAULT_GROUND_POS;
	}

	public bool IsValidLastPositionOnGround()
	{
		return mLastPositionOnGround != DEFAULT_GROUND_POS;
	}

	private void Awake()
	{
		string n = AvatarData.GetParentBone(AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT, 0) + "/" + AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT;
		Transform transform = base.transform.Find(n);
		if (transform != null)
		{
			mFlightArmorWing = transform.GetComponent<SkinnedMeshRenderer>();
			if (mFlightArmorWing != null)
			{
				mFlightArmorWing.enabled = true;
			}
		}
		mLastPositionOnGround = DEFAULT_GROUND_POS;
		if (AvAvatar.pObject != null && !AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if (component != null)
			{
				_ProjectileFiringAreas = component._ProjectileFiringAreas;
				_AvatarShootingData = component._AvatarShootingData;
				_AvatarBounceParams = component._AvatarBounceParams;
			}
		}
		mFlyingData = FlightData.GetFlightData(base.gameObject, FlightDataType.GLIDING);
		mUWSwimmingData = UWSwimmingData.GetSwimmingData(base.gameObject);
		mOriginalBonusMaxSpeedWithBoost = _BonusMaxSpeedWithBoost;
		mOriginalMaxUWSwimSpeedWithBoost = _MaxUWSwimSpeedWithBoost;
		if (_FlyingBone == null)
		{
			Debug.LogError("Flying bone not set.  Adding one");
			_FlyingBone = new GameObject("nullFlyingBone").transform;
		}
		mController = (CharacterController)collider;
		if ((bool)mController)
		{
			mController.detectCollisions = false;
		}
		mProperties = GetComponent<AvAvatarProperties>();
		pState = AvAvatarState.NONE;
		pSubState = AvAvatarSubState.NORMAL;
		mSlopeLimit = mController.slopeLimit;
		mStepOffset = mController.stepOffset;
		AvAvatarStateData stateDataFromSubState = GetStateDataFromSubState(AvAvatarSubState.GLIDING);
		if (stateDataFromSubState != null)
		{
			mMeanGlideSpeed = 0.5f * (stateDataFromSubState._MaxForwardSpeed + stateDataFromSubState._MinForwardSpeed);
		}
		if (_ActivateOnAwake)
		{
			AvAvatar.pObject = base.gameObject;
			AvAvatar.pState = AvAvatarState.IDLE;
		}
		if (AvatarData.pInstance != null)
		{
			mGender = ((AvatarData.GetGender() == Gender.Male) ? "M" : "F");
		}
		GetFlightSuitItems();
		KAInput.pInstance.EnableInputType("Horizontal", InputType.ACCELEROMETER, inEnable: false);
		KAInput.pInstance.EnableInputType("Vertical", InputType.ACCELEROMETER, inEnable: false);
		mAvAvatarAnim = base.gameObject.GetComponentInChildren<AvAvatarAnim>();
		if (AvAvatar.pObject == null || AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			SetupModifiers();
			CreateProjectileCollider();
			CreateCSMCollider();
		}
		SetMounted(mounted: false);
	}

	public void CreateProjectileCollider()
	{
		if (!(mProjCollider == null))
		{
			return;
		}
		GameObject gameObject = new GameObject();
		if (gameObject != null)
		{
			gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
			gameObject.transform.parent = base.gameObject.transform;
			gameObject.name = "ProjCollider";
			mProjCollider = gameObject.AddComponent<SphereCollider>();
			ObTargetable obTargetable = gameObject.AddComponent<ObTargetable>();
			if (obTargetable != null)
			{
				obTargetable._Active = false;
			}
		}
	}

	public void CreateCSMCollider()
	{
		if (mCSMCollider == null)
		{
			GameObject gameObject = new GameObject();
			if (gameObject != null)
			{
				gameObject.transform.parent = _MainRoot;
				gameObject.name = "AvatarCSMCollider";
				gameObject.tag = "AvatarCSMCollider";
				gameObject.layer = LayerMask.NameToLayer("Avatar");
				mCSMCollider = gameObject.AddComponent<CapsuleCollider>();
				mCSMCollider.radius = _CSMColliderRadius;
				mCSMCollider.isTrigger = true;
				gameObject.AddComponent<ObClickableAvatarCSM>()._RollOverCursorName = _CSMRollOverCurserName;
			}
		}
	}

	private void OnActive(bool inActive)
	{
		if (!AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			UtDebug.LogError("OnActive should only be used on the current player!");
		}
		mCurrentAnim = "none";
		if ((bool)_MainRoot)
		{
			_MainRoot.localEulerAngles = Vector3.zero;
		}
		if (inActive)
		{
			base.enabled = true;
			if (mCurrentStateData != null)
			{
				mFidgetTime = mCurrentStateData._FidgetTimeMax;
			}
			mVelocity = Vector3.zero;
			AvAvatar.pInputEnabled = true;
			UpdateDisplayName(pPlayerMounted, (!AvAvatar.IsCurrentPlayer(base.gameObject)) ? (GetInstanceInfo().mInstance._Group != null) : (UserProfile.pProfileData != null && UserProfile.pProfileData.HasGroup()));
			return;
		}
		SetImmune(isImmune: false);
		ClearSpline();
		if (!AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			base.transform.parent = null;
		}
		else
		{
			AvAvatar.SetParentTransform(null);
		}
		Transform transform = base.transform.Find("AvatarEmoticon");
		if ((bool)transform)
		{
			UnityEngine.Object.Destroy(transform.gameObject);
		}
		string n = "PfMyChatBubble";
		if (!AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			n = "PfChatBubble";
		}
		Transform transform2 = base.transform.Find(n);
		if ((bool)transform2)
		{
			UnityEngine.Object.Destroy(transform2.gameObject);
		}
	}

	public void MoveChildrenTo(GameObject newParent)
	{
		Transform displayNameObj = AvatarData.GetDisplayNameObj(base.gameObject);
		if ((bool)displayNameObj)
		{
			displayNameObj.parent = newParent.transform;
			displayNameObj.transform.localEulerAngles = Vector3.zero;
			displayNameObj.transform.localScale = Vector3.one;
		}
		Transform transform = base.transform.Find("AvatarEmoticon");
		if ((bool)transform)
		{
			transform.parent = newParent.transform;
			transform.transform.localScale = Vector3.one;
		}
		string n = "PfMyChatBubble";
		if (!AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			n = "PfChatBubble";
		}
		Transform transform2 = base.transform.Find(n);
		if ((bool)transform2)
		{
			transform2.parent = newParent.transform;
			transform2.transform.localScale = Vector3.one;
		}
	}

	private void ResetVelocity()
	{
		pVelocity = Vector3.zero;
		mFlightSpeed = 0f;
	}

	private void OnSetPosition()
	{
		if (!AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			UtDebug.LogError("OnSetPosition should only be used on the current player!");
		}
		ClearSpline();
		if ((bool)_MainRoot)
		{
			_MainRoot.localEulerAngles = Vector3.zero;
		}
		if (mCurrentStateData != null)
		{
			mFidgetTime = mCurrentStateData._FidgetTimeMax;
		}
		mVelocity = Vector3.zero;
		if ((bool)AvAvatar.pAvatarCam)
		{
			AvAvatar.pAvatarCam.SendMessage("ResetCamera", null, SendMessageOptions.DontRequireReceiver);
		}
		if (MainStreetMMOClient.pInstance != null && AvAvatar.pState != 0)
		{
			MainStreetMMOClient.pInstance.SendUpdate(MMOAvatarFlags.SETPOSITION);
		}
		if (mPetObject != null)
		{
			mPetObject.OnAvatarSetPositionDone(base.transform.position);
		}
	}

	public bool UsingProp(string name)
	{
		if (mCurrentUsedProp == null)
		{
			return false;
		}
		return mCurrentUsedProp._Name == name;
	}

	public void UseProp(string name)
	{
		if (!mProperties || string.IsNullOrEmpty(name))
		{
			return;
		}
		StopUseProp();
		PropStates[] propStates = mProperties._PropStates;
		foreach (PropStates propStates2 in propStates)
		{
			if (propStates2._Name == name)
			{
				UseProp(propStates2);
				break;
			}
		}
	}

	public void UseProp(PropStates prop)
	{
		mCurrentUsedProp = prop;
		if (prop._Objects != null && prop._Objects.Length != 0)
		{
			EquipObject[] objects = prop._Objects;
			foreach (EquipObject equipObject in objects)
			{
				GameObject gameObject = UtUtilities.AttachObject(base.gameObject, equipObject._Object, equipObject._Bone, equipObject._Offset, equipObject._Rotation);
				if (gameObject != null)
				{
					UtUtilities.SetLayerRecursively(gameObject, base.gameObject.layer);
				}
			}
		}
		if (!string.IsNullOrEmpty(prop._AnimState))
		{
			AvAvatarAnim componentInChildren = GetComponentInChildren<AvAvatarAnim>();
			if ((bool)componentInChildren)
			{
				componentInChildren.PlayCannedAnim(prop._AnimState, prop._QuitOnPlayerMove, prop._QuitOnAnimEnd, prop._FreezePlayer);
			}
		}
		if ((bool)prop._SoundClip)
		{
			SnChannel.Play(prop._SoundClip, "SFX_Pool", inForce: true);
		}
	}

	public void StopUseProp()
	{
		if (mCurrentUsedProp == null)
		{
			return;
		}
		if (mCurrentUsedProp._Objects != null && mCurrentUsedProp._Objects.Length != 0)
		{
			EquipObject[] objects = mCurrentUsedProp._Objects;
			foreach (EquipObject equipObject in objects)
			{
				if (equipObject != null && equipObject._Object != null)
				{
					UtUtilities.DetachObject(base.gameObject, equipObject._Object.name);
				}
			}
		}
		if (!string.IsNullOrEmpty(mCurrentUsedProp._AnimState))
		{
			AvAvatarAnim componentInChildren = GetComponentInChildren<AvAvatarAnim>();
			if ((bool)componentInChildren)
			{
				componentInChildren.EndCannedAnim(bUseCallback: false);
			}
		}
		mCurrentUsedProp = null;
	}

	private void OnSetState()
	{
		if (AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			mState = AvAvatar.pState;
			mPrevState = AvAvatar.pPrevState;
			if (OnAvatarStateChange != null)
			{
				OnAvatarStateChange();
			}
		}
		if (mPetObject != null)
		{
			if (mPrevState == AvAvatarState.SPRINGBOARD)
			{
				mPetObject.OnAvatarSpringBoardStateEnded();
			}
			if (mPrevState == AvAvatarState.SLIDING)
			{
				mPetObject.OnAvatarSlidingStateEnded(base.transform.position);
			}
			if (mPrevState == AvAvatarState.ZIPLINE)
			{
				mPetObject.OnAvatarZiplineStateEnded();
			}
		}
		switch (mPrevState)
		{
		case AvAvatarState.NONE:
			if (mState != 0)
			{
				mCurrentAnim = "";
			}
			break;
		case AvAvatarState.ONSPLINE:
			ClearSpline();
			Speed = 0f;
			mEndReached = false;
			CurrentPos = 0f;
			break;
		case AvAvatarState.ZIPLINE:
		case AvAvatarState.SLIDING:
		case AvAvatarState.SPRINGBOARD:
		{
			bool flag = mEndReached;
			ClearSpline();
			base.transform.localEulerAngles = new Vector3(0f, base.transform.localEulerAngles.y, 0f);
			if (AvAvatar.IsCurrentPlayer(base.gameObject))
			{
				AvAvatar.SetUIActive(inActive: true);
				AvAvatar.pInputEnabled = true;
				if ((bool)AvAvatar.pAvatarCam)
				{
					AvAvatar.pAvatarCam.GetComponent<CaAvatarCam>()._IgnoreCollision = false;
				}
			}
			if (mPrevState == AvAvatarState.ZIPLINE)
			{
				Transform parent = base.transform.parent;
				if (parent != null)
				{
					ZiplineController component = parent.GetComponent<ZiplineController>();
					if (component != null)
					{
						component.Stop(flag);
					}
					else
					{
						if (!AvAvatar.IsCurrentPlayer(base.gameObject))
						{
							base.transform.parent = null;
						}
						else
						{
							AvAvatar.SetParentTransform(null);
						}
						UnityEngine.Object.Destroy(parent.gameObject);
					}
				}
			}
			if (mPrevState == AvAvatarState.ZIPLINE || mPrevState == AvAvatarState.SLIDING)
			{
				if (!flag)
				{
					SnChannel.StopPool("Music_Pool");
				}
				break;
			}
			if (AvAvatar.IsCurrentPlayer(base.gameObject) && (bool)AvAvatar.pAvatarCam)
			{
				AvAvatar.pAvatarCam.SendMessage("ResetSpeed", null, SendMessageOptions.DontRequireReceiver);
			}
			SnChannel.StopPool("SFX_Pool");
			break;
		}
		case AvAvatarState.CANNON:
			ClearSpline();
			if (AvAvatar.IsCurrentPlayer(base.gameObject) && (bool)AvAvatar.pAvatarCam)
			{
				AvAvatar.pAvatarCam.SendMessage("ResetSpeed", null, SendMessageOptions.DontRequireReceiver);
			}
			SnChannel.StopPool("SFX_Pool");
			break;
		}
		switch (mState)
		{
		case AvAvatarState.NONE:
		case AvAvatarState.IDLE:
		case AvAvatarState.PAUSED:
			if ((bool)_MainRoot && !pPlayerMounted)
			{
				_MainRoot.localEulerAngles = Vector3.zero;
			}
			if (mCurrentStateData != null)
			{
				mFidgetTime = mCurrentStateData._FidgetTimeMax;
			}
			break;
		case AvAvatarState.SPRINGBOARD:
		case AvAvatarState.CANNON:
			if (AvAvatar.IsCurrentPlayer(base.gameObject) && (bool)AvAvatar.pAvatarCam)
			{
				AvAvatar.pAvatarCam.SendMessage("SetSpeed", 100, SendMessageOptions.DontRequireReceiver);
			}
			break;
		case AvAvatarState.SPIN:
			mVelocity.y = 0f;
			break;
		}
	}

	private void InitializeModifiedStateData()
	{
		if (mModifiedStateData == null)
		{
			mModifiedStateData = new AvAvatarStateData[_StateData.Length];
		}
		for (int i = 0; i < _StateData.Length; i++)
		{
			mModifiedStateData[i] = _StateData[i].Clone();
		}
	}

	public AvAvatarStateData GetStateDataFromSubState(AvAvatarSubState subState)
	{
		if (_StateData == null)
		{
			return null;
		}
		if (mModifiedStateData == null)
		{
			InitializeModifiedStateData();
		}
		AvAvatarStateData[] array = mModifiedStateData;
		foreach (AvAvatarStateData avAvatarStateData in array)
		{
			if (avAvatarStateData._State == subState)
			{
				return avAvatarStateData.Clone();
			}
		}
		return null;
	}

	public void UpdateDisplayName(bool offsetMount, bool hasGroup = false)
	{
		Transform displayNameTransform = AvatarData.GetDisplayNameObj(base.gameObject);
		if (displayNameTransform == null)
		{
			return;
		}
		Transform groupNameObj = AvatarData.GetGroupNameObj(base.gameObject);
		Transform uDTStarsObj = AvatarData.GetUDTStarsObj(base.gameObject);
		AvatarData.SetGroupNameVisible(base.gameObject, hasGroup && AvatarData.pDisplayYourName);
		List<Transform> list = new List<Transform>();
		list.Add(groupNameObj);
		list.Add(uDTStarsObj);
		list.RemoveAll((Transform t) => t == null);
		displayNameTransform.localPosition = new Vector3(0f, _DisplayNameStartHeight + (float)(list.Count - 1) * _DisplayNameSpacing, 0f);
		if (list.Count == 0)
		{
			return;
		}
		Transform parent = displayNameTransform.parent;
		foreach (Transform item in list.FindAll((Transform p) => p != displayNameTransform))
		{
			item.SetParent(parent, worldPositionStays: true);
		}
		if (!hasGroup)
		{
			list.Remove(groupNameObj);
		}
		for (int i = 0; i < list.Count; i++)
		{
			list[i].localPosition = new Vector3(0f, displayNameTransform.localPosition.y - _DisplayNameSpacing * (float)(i + 1), 0f);
		}
		list.Add(groupNameObj);
		foreach (Transform item2 in list)
		{
			item2.SetParent(displayNameTransform, worldPositionStays: true);
		}
		Vector3 offset = new Vector3(_DisplayNameOffset.x, offsetMount ? (_DisplayNameOffset.y * 0.5f) : _DisplayNameOffset.y, _DisplayNameOffset.z);
		AvatarData.SetDisplayNameLocalPosition(base.gameObject, offset);
		ObNGUI_Proxy[] componentsInChildren = displayNameTransform.GetComponentsInChildren<ObNGUI_Proxy>();
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			componentsInChildren[j].UpdateData();
		}
	}

	private void OnSetSubState(AvAvatarSubState oldSubState)
	{
		if (AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			mSubState = AvAvatar.pSubState;
		}
		AvAvatarStateData stateDataFromSubState = GetStateDataFromSubState(mSubState);
		if (stateDataFromSubState == null)
		{
			if (AvAvatar.IsCurrentPlayer(base.gameObject))
			{
				UtDebug.LogError("Missing avatar substate " + mSubState);
			}
			return;
		}
		mCurrentStateData = stateDataFromSubState;
		if (oldSubState == mSubState)
		{
			return;
		}
		if (AvAvatar.IsCurrentPlayer(base.gameObject) && AvAvatar.pAvatarCam != null)
		{
			CaAvatarCam component = AvAvatar.pAvatarCam.GetComponent<CaAvatarCam>();
			if (component != null)
			{
				float a = 2f;
				if (SanctuaryManager.pCurPetInstance != null && pPlayerMounted)
				{
					a = Mathf.Max(a, SanctuaryManager.pCurPetInstance.pTypeInfo._MountedMinCameraDistance);
				}
				component.SetAvatarCamParams(mCurrentStateData._AvatarCameraParams, mSubState);
			}
		}
		if (mCurrentStateData._Gravity == 0f)
		{
			mVelocity.y = 0f;
		}
		if (mAnimator == null)
		{
			mAnimator = base.gameObject.GetComponentInChildren<Animator>();
		}
		switch (oldSubState)
		{
		case AvAvatarSubState.SWIMMING:
			if (AvAvatar.pSubState != AvAvatarSubState.SWIMMING)
			{
				ApplySwimAnim(SwimAnimState.NONE);
			}
			break;
		case AvAvatarSubState.FLYING:
			FlyingCleanup();
			break;
		case AvAvatarSubState.GLIDING:
			FlyingCleanup();
			if (mAnimator != null)
			{
				mAnimator.SetFloat("fVertical", 0f);
			}
			break;
		case AvAvatarSubState.UWSWIMMING:
			ApplySwimAnim(SwimAnimState.NONE);
			if (AvAvatar.IsCurrentPlayer(base.gameObject) && UiAvatarControls.pInstance != null)
			{
				UiAvatarControls.pInstance.ShowUWSwimButtons(isVisible: false);
				if (mAnimator != null && mAnimator.GetBool("bUWSwim"))
				{
					mAnimator.SetBool("bUWSwim", value: false);
				}
			}
			break;
		case AvAvatarSubState.DIVESUIT:
			if (mAnimator != null && mAnimator.GetBool("bUnderwaterWalk"))
			{
				mAnimator.SetBool("bUnderwaterWalk", value: false);
			}
			break;
		}
		if (pSubState != 0 && mPlayerCarrying)
		{
			RemoveCarriedObject();
		}
		switch (pSubState)
		{
		case AvAvatarSubState.UWSWIMMING:
			mUWSwimmingSpeed = 0f;
			mVelocity = Vector3.zero;
			if (AvAvatar.IsCurrentPlayer(base.gameObject))
			{
				if (pUWSwimZone != null)
				{
					pUWSwimZone.Enter();
				}
				UiAvatarControls.pInstance.ShowUWSwimButtons(isVisible: true);
			}
			break;
		case AvAvatarSubState.DIVESUIT:
			if (mAnimator != null && mAnimator.GetBool("bUnderwaterWalk"))
			{
				mAnimator.SetBool("bUnderwaterWalk", value: true);
			}
			break;
		}
	}

	private void FlyingCleanup()
	{
		if (AvAvatar.IsCurrentPlayer(base.gameObject) && AvAvatar.pAvatarCam != null)
		{
			CaAvatarCam component = AvAvatar.pAvatarCam.GetComponent<CaAvatarCam>();
			if (component != null)
			{
				component.SetLayer(CaAvatarCam.CameraLayer.LAYER_AVATAR);
			}
			_FlyingBone.Rotate(Vector3.zero);
			base.transform.Rotate(Vector3.zero);
		}
	}

	private void VelocityUpdate(float targetSpeed)
	{
		if (!IsFlyingOrGliding())
		{
			mCurSpeed = Mathf.Lerp(mCurSpeed, targetSpeed, mCurrentStateData._Acceleration * Time.deltaTime);
			Vector3 forward = base.transform.forward;
			forward.y = 0f;
			forward.Normalize();
			forward *= mCurSpeed;
			mVelocity.x = forward.x;
			mVelocity.z = forward.z;
		}
	}

	private void UpdateWallClimbVelocity(float targetXSpeed, float targetYSpeed)
	{
		mVelocity = base.transform.InverseTransformDirection(mVelocity);
		mVelocity.x = targetXSpeed * _WallClimbParams._LeftRightMoveSpeed;
		mVelocity.y = targetYSpeed * _WallClimbParams._UpDownMoveSpeed;
		mVelocity.z = 0f;
		mVelocity = base.transform.TransformDirection(mVelocity);
		if ((bool)mAvAvatarAnim)
		{
			AvAvatarAnim.WallClimbAnimState state = ((targetXSpeed > 0f) ? AvAvatarAnim.WallClimbAnimState.WallClimbRight : AvAvatarAnim.WallClimbAnimState.WallClimbLeft);
			mAvAvatarAnim.SetWallClimbAnim(state, targetXSpeed, targetYSpeed);
			state = ((targetYSpeed > 0f) ? AvAvatarAnim.WallClimbAnimState.WallClimbUp : AvAvatarAnim.WallClimbAnimState.WallClimbDown);
			mAvAvatarAnim.SetWallClimbAnim(state, targetXSpeed, targetYSpeed);
			if (targetXSpeed < 0.02f && targetXSpeed > -0.02f && targetYSpeed < 0.02f && targetYSpeed > -0.02f)
			{
				mAvAvatarAnim.SetWallClimbAnim(AvAvatarAnim.WallClimbAnimState.WallClimbIdle);
			}
		}
	}

	private float GetVerticalFromInput()
	{
		float num = KAInput.GetAxis("Vertical");
		if (IsFlyingOrGliding() || pSubState == AvAvatarSubState.UWSWIMMING)
		{
			if (UtPlatform.IsMobile() && UiOptions.pIsTiltSteer)
			{
				num = 0f - num;
			}
			num = (UiOptions.pIsFlightInverted ? (0f - num) : num);
		}
		return num;
	}

	private float GetHorizontalFromInput()
	{
		return KAInput.GetAxis("Horizontal");
	}

	private void KeyboardUpdate()
	{
		if (!AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			UtDebug.LogError("KeyboardUpdate should only be used on the current player!");
		}
		mRotation = GetHorizontalFromInput();
		float num = GetVerticalFromInput();
		if ((bool)_MainRoot && (mRotation != 0f || num != 0f) && !pPlayerMounted)
		{
			_MainRoot.localEulerAngles = Vector3.zero;
		}
		if ((num != 0f || mRotation != 0f || OnGround()) && mIsInitialGlideThrustStarted)
		{
			mIsInitialGlideThrustStarted = false;
		}
		if (mSubState == AvAvatarSubState.GLIDING && _MainRoot != null)
		{
			float x = 0f;
			float z = 0f;
			if (AvAvatar.IsCurrentPlayer(base.gameObject))
			{
				z = (0f - mRotation) * mCurrentStateData._MaxSidewaysTilt;
				x = num * mCurrentStateData._MaxForwardBackwardTilt;
			}
			_MainRoot.localEulerAngles = new Vector3(x, 0f, z);
			Transform transform = base.transform.Find("Wing");
			if (transform != null)
			{
				transform.localEulerAngles = _MainRoot.localEulerAngles;
			}
		}
		if (mCurrentStateData != null)
		{
			mRotation *= mCurrentStateData._RotSpeed * Time.deltaTime;
		}
		if (mPlayerDragging && mDragObject != null)
		{
			float num2 = mRotation;
			if (mDraggable != null)
			{
				num2 = mDraggable.RotateAround(base.transform.position, base.transform.up, mRotation);
			}
			Vector3 vector = mDragObject.transform.position - base.transform.position;
			Vector3 forward = base.transform.forward;
			float num3 = Vector3.Angle(vector, forward);
			if (Math.Abs(((Vector3.Dot(Vector3.Cross(vector, forward), base.transform.up) > 0f) ? num3 : (0f - num3)) + num2) < _ObjectDragAngleLimit)
			{
				base.transform.Rotate(0f, num2, 0f);
			}
		}
		else if (!IsFlyingOrGliding() && mSubState != AvAvatarSubState.UWSWIMMING && mSubState != AvAvatarSubState.WALLCLIMB)
		{
			base.transform.Rotate(0f, mRotation, 0f);
		}
		switch (mSubState)
		{
		case AvAvatarSubState.NORMAL:
		case AvAvatarSubState.DIVESUIT:
			if (!IsFlyingOrGliding())
			{
				if (num > 0f)
				{
					num *= mCurrentStateData._MaxForwardSpeed * (pPlayerCarrying ? 0.5f : 1f);
				}
				else if (num < 0f)
				{
					num *= mCurrentStateData._MaxBackwardSpeed;
				}
				if (SanctuaryManager.pCurPetInstance != null && pPlayerMounted)
				{
					num *= SanctuaryManager.pCurPetInstance.GetMountedSpeedModifer();
				}
			}
			else
			{
				num *= mCurrentStateData._MaxAirSpeed;
			}
			break;
		case AvAvatarSubState.SKATING:
		{
			Vector3 vector2 = mVelocity;
			vector2.y = 0f;
			if (num != 0f)
			{
				mSkateSpeed = vector2.magnitude;
				float num4 = mCurrentStateData._Acceleration * Time.deltaTime;
				mSkateSpeed += num4;
				if (num > 0f)
				{
					if (mSkateSpeed > mCurrentStateData._MaxForwardSpeed)
					{
						mSkateSpeed = mCurrentStateData._MaxForwardSpeed;
					}
				}
				else if (num < 0f)
				{
					if (mSkateSpeed > mCurrentStateData._MaxBackwardSpeed)
					{
						mSkateSpeed = mCurrentStateData._MaxBackwardSpeed;
					}
					mSkateSpeed *= -1f;
				}
			}
			else if (mSkateSpeed != 0f)
			{
				bool flag = false;
				if (mSkateSpeed < 0f)
				{
					flag = true;
				}
				mSkateSpeed = vector2.magnitude;
				mSkateSpeed -= mCurrentStateData._Acceleration * Time.deltaTime;
				if (mSkateSpeed <= 0f)
				{
					mSkateSpeed = num;
				}
				if (flag && mSkateSpeed != num)
				{
					mSkateSpeed *= -1f;
				}
			}
			else
			{
				mSkateSpeed = num;
			}
			num = mSkateSpeed;
			break;
		}
		default:
			if (num > 0f)
			{
				num *= mCurrentStateData._MaxForwardSpeed;
			}
			else if (num < 0f)
			{
				num *= mCurrentStateData._MaxBackwardSpeed;
			}
			break;
		case AvAvatarSubState.FLYING:
		case AvAvatarSubState.GLIDING:
			break;
		}
		if (mIsBouncing)
		{
			Vector3 forward2 = base.transform.forward;
			forward2.y = 0f;
			forward2.Normalize();
			forward2 *= num;
			forward2.y = mCurrentStateData._Gravity * mGravityMultiplier;
			forward2 *= Time.deltaTime;
			mBouncingObject.SendMessage("UpdateAvatarVelocity", forward2, SendMessageOptions.DontRequireReceiver);
		}
		else if (mSubState == AvAvatarSubState.WALLCLIMB)
		{
			UpdateWallClimbVelocity(GetHorizontalFromInput(), GetVerticalFromInput());
		}
		else
		{
			VelocityUpdate(num);
		}
		if ((IsFlyingOrGliding() || mState == AvAvatarState.FALLING) && KAInput.GetButtonUp("Soar") && KAInput.pInstance.GetInputTypeState("Soar", InputType.UI_BUTTONS))
		{
			if (!mIsSoarTutorialDone)
			{
				ProductData.AddTutorial(_SoarTutorialName);
				mIsSoarTutorialDone = true;
			}
			if ((bool)SanctuaryManager.pCurPetInstance && pPlayerMounted && !AvatarData.pInstanceInfo.FlightSuitEquipped())
			{
				UserItemData lastUsedFlightSuit = GetLastUsedFlightSuit();
				if (lastUsedFlightSuit != null)
				{
					AvatarData.pInstanceInfo.UpdatePartInventoryId(AvatarData.pPartSettings.AVATAR_PART_WING, lastUsedFlightSuit);
					EquipFlightSuit(lastUsedFlightSuit.Item, OnAllItemsDownloaded);
				}
			}
			else
			{
				InitiateGliding();
			}
		}
		if (mSubState == AvAvatarSubState.NORMAL || mSubState == AvAvatarSubState.DIVESUIT || mSubState == AvAvatarSubState.SKATING || mSubState == AvAvatarSubState.SWIMMING)
		{
			if (!AvAvatar.pInputEnabled || KAInput.GetButtonUp("DragonMount") || (!KAInput.GetButtonDown("WingFlap") && !KAInput.GetButtonDown("Jump")))
			{
				return;
			}
			if ((OnGround() && pCanJump) || (!pPlayerMounted && mSubState == AvAvatarSubState.SWIMMING))
			{
				Jump(null);
			}
			else
			{
				if (mState != AvAvatarState.FALLING && mSubState != AvAvatarSubState.SWIMMING)
				{
					return;
				}
				if (pPlayerMounted)
				{
					if (!SanctuaryManager.pCurPetInstance.pTypeInfo._Flightless)
					{
						BroadcastMessage("StartGliding", SendMessageOptions.DontRequireReceiver);
						pSubState = AvAvatarSubState.FLYING;
					}
					else
					{
						Jump(null);
					}
				}
				string[] avatarAttackingAreas = _AvatarAttackingAreas;
				foreach (string value in avatarAttackingAreas)
				{
					if (RsResourceManager.pCurrentLevel.Equals(value))
					{
						pState = AvAvatarState.SPIN;
						break;
					}
				}
			}
		}
		else if (pSubState == AvAvatarSubState.UWSWIMMING)
		{
			float horizontalFromInput = GetHorizontalFromInput();
			float verticalFromInput = GetVerticalFromInput();
			UpdateUWSwimmingControl(horizontalFromInput, verticalFromInput);
		}
		else if (IsFlyingOrGliding())
		{
			float horizontalFromInput2 = GetHorizontalFromInput();
			float num5 = GetVerticalFromInput();
			if (num5 == 0f && mIsInitialGlideThrustStarted)
			{
				num5 = _InitialGlideThrust;
			}
			if (KAInput.GetButtonUp("DragonMount") && mIsInitialGlideThrustStarted)
			{
				mIsInitialGlideThrustStarted = false;
			}
			UpdateFlying(horizontalFromInput2, num5);
		}
		else if (mSubState == AvAvatarSubState.WALLCLIMB)
		{
			if (AvAvatar.IsCurrentPlayer(base.gameObject) && AvAvatar.pToolbar != null)
			{
				AvAvatar.pToolbar.SendMessage("DisableFireButton", null, SendMessageOptions.DontRequireReceiver);
			}
			if (KAInput.GetButtonDown("Jump"))
			{
				ResetWallClimb(AvAvatarAnim.WallClimbAnimState.WallClimbDownDismount);
				base.transform.Translate(0f, 0f, _WallClimbParams._DetachOffset);
			}
		}
		else if (mState != AvAvatarState.ATTACK && mSubState != AvAvatarSubState.GLIDING)
		{
			if (AvAvatar.IsCurrentPlayer(base.gameObject) && AvAvatar.pToolbar != null)
			{
				AvAvatar.pToolbar.SendMessage("DisableFireButton", null, SendMessageOptions.DontRequireReceiver);
			}
			if (Input.GetButtonDown("Jump") && (bool)_MainRoot)
			{
				_MainRoot.localEulerAngles = Vector3.zero;
			}
		}
		else if (AvAvatar.IsCurrentPlayer(base.gameObject) && AvAvatar.pToolbar != null)
		{
			AvAvatar.pToolbar.SendMessage("DisableFireButton", null, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void Jump(SnISound inSound)
	{
		if (!mTriggerJump)
		{
			mTriggerJump = true;
			mJumpSound = inSound;
		}
	}

	private void OnEnable()
	{
		mIsBouncing = false;
		mTriggerJump = false;
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	public bool IsFlyingOrGliding()
	{
		if (pSubState != AvAvatarSubState.FLYING)
		{
			return pSubState == AvAvatarSubState.GLIDING;
		}
		return true;
	}

	private void MouseUpdate()
	{
		if (!AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			UtDebug.LogError("MouseUpdate should only be used on the current player!");
		}
		if (Input.GetButtonUp("MouseLeft") && !IsFlyingOrGliding() && Input.mousePosition.y >= 0f && Input.mousePosition.y <= (float)Screen.height && Input.mousePosition.x >= 0f && Input.mousePosition.x <= (float)Screen.width)
		{
			bool flag = false;
			if (KAUI.GetGlobalMouseOverItem() != null)
			{
				flag = true;
			}
			Camera component = AvAvatar.pAvatarCam.GetComponent<Camera>();
			Vector3 vector = component.WorldToScreenPoint(AvAvatar.GetPosition());
			if (Input.mousePosition.y > vector.y && !flag)
			{
				int layerMask = component.cullingMask & ~((1 << LayerMask.NameToLayer("Avatar")) | (1 << LayerMask.NameToLayer("Ignore Raycast")));
				if (Physics.Raycast(component.ScreenPointToRay(Input.mousePosition), out var hitInfo, float.PositiveInfinity, layerMask) && hitInfo.collider.gameObject.GetComponent<ObClickable>() == null)
				{
					MoveTo(hitInfo.point);
				}
			}
		}
		if (mSpline == null)
		{
			return;
		}
		base.Update();
		if (mEndReached)
		{
			if (mActivateObject != null)
			{
				mActivateObject.SendMessage("OnActivate", null, SendMessageOptions.DontRequireReceiver);
			}
			ClearSpline();
		}
	}

	private void OnClick(GameObject go)
	{
		if (_ControlMode != AvAvatarControlMode.MOUSE && _ControlMode != AvAvatarControlMode.BOTH)
		{
			return;
		}
		ObClickable component = go.GetComponent<ObClickable>();
		Vector3 vector = go.transform.position + go.transform.TransformDirection(component._Offset);
		if ((base.transform.position - go.transform.position).magnitude > component._Range && component._Range != 0f)
		{
			Vector3 vector2 = base.transform.position - vector;
			vector2.y = 0f;
			if (vector2.magnitude < 0.2f)
			{
				go.SendMessage("OnActivate", null, SendMessageOptions.DontRequireReceiver);
			}
			else if (FindPath(base.transform.position, vector))
			{
				pState = AvAvatarState.IDLE;
				mActivateObject = go;
			}
			else
			{
				go.SendMessage("OnActivate", null, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	private void OnRotate(float inDir)
	{
		ClearSpline();
		if ((bool)_MainRoot)
		{
			_MainRoot.localEulerAngles = Vector3.zero;
		}
		mRotation = inDir * mCurrentStateData._RotSpeed * Time.deltaTime;
		base.transform.Rotate(0f, mRotation, 0f);
	}

	private bool FindPath(Vector3 start, Vector3 end)
	{
		Spline spline = new Spline(2, looping: false, constSpeed: true, alignTangent: true, hasQ: false);
		spline.SetControlPoint(0, start, Quaternion.identity, 0f);
		spline.SetControlPoint(1, end, Quaternion.identity, 0f);
		spline.RecalculateSpline();
		_Draw = true;
		SetSpline(spline);
		Speed = mCurrentStateData._MaxForwardSpeed;
		Input.ResetInputAxes();
		return true;
	}

	public void SetFlyAnimPrefix(string p)
	{
	}

	public void SetFlashInterval(float interval)
	{
		mFlashingInterval = interval;
	}

	public void StartFlashing(float interval)
	{
		mFlashingTimer = interval;
		mFlashingInterval = interval;
		mAvatarFlashing = true;
		if (!_MainRoot)
		{
			return;
		}
		Renderer[] componentsInChildren = _MainRoot.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (renderer != null && renderer.enabled)
			{
				mActiveRenderers.Add(renderer);
			}
		}
	}

	public void EnableRenderer(bool enable)
	{
		if (!_MainRoot)
		{
			return;
		}
		Renderer[] componentsInChildren = _MainRoot.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (renderer != null)
			{
				renderer.enabled = enable;
			}
		}
		AvatarData.InstanceInfo instanceInfo = GetInstanceInfo();
		if (enable && instanceInfo != null)
		{
			instanceInfo.CheckHelmetHair();
			instanceInfo.CheckDisableBlink();
			ShowArmorWing(!instanceInfo.CheckAttributeSet(AvatarData.pPartSettings.AVATAR_PART_HAND, "ToggleWings") || pSubState == AvAvatarSubState.GLIDING);
		}
	}

	private void UpdateFlashing()
	{
		if (!(mFlashingTimer > 0f))
		{
			return;
		}
		mFlashingTimer -= Time.deltaTime;
		if (!(mFlashingTimer <= 0f))
		{
			return;
		}
		if ((bool)_MainRoot && mActiveRenderers != null && mActiveRenderers.Count > 0)
		{
			foreach (Renderer mActiveRenderer in mActiveRenderers)
			{
				if (mActiveRenderer != null)
				{
					mActiveRenderer.enabled = !mActiveRenderer.enabled;
				}
			}
		}
		mFlashingTimer = mFlashingInterval;
	}

	public void StopFlashing()
	{
		mFlashingTimer = 0f;
		mAvatarFlashing = false;
		if (!_MainRoot || mActiveRenderers == null || mActiveRenderers.Count <= 0)
		{
			return;
		}
		foreach (Renderer mActiveRenderer in mActiveRenderers)
		{
			if (mActiveRenderer != null)
			{
				mActiveRenderer.enabled = true;
			}
		}
		mActiveRenderers.Clear();
	}

	public void SetPosition(Vector3 pos)
	{
		if (AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			AvAvatar.SetPosition(pos);
		}
		else
		{
			base.transform.position = pos;
		}
	}

	public void GroundFallThroughCheck(Vector3 oldPos)
	{
		float groundHeight = UtUtilities.GetGroundHeight(mController.transform.position, 10000f);
		if (groundHeight != float.NegativeInfinity)
		{
			return;
		}
		float num = 1f;
		if (GrFPS.pFrameRate < 10f)
		{
			num = 5f;
		}
		UtDebug.LogError("Ground returned NegativeInfinity moved Avatar to previous");
		groundHeight = UtUtilities.GetGroundHeight(oldPos, 10000f);
		UtDebug.LogError("UtUtilities.GetGroundHeight Previous Position returned " + groundHeight);
		if (groundHeight == float.NegativeInfinity)
		{
			Vector3 vector = oldPos;
			vector.y += 10f;
			groundHeight = UtUtilities.GetGroundHeight(vector, 10000f);
			UtDebug.LogError("UtUtilities.GetGroundHeight Previous Position + 10m returned " + groundHeight);
			if (groundHeight == float.NegativeInfinity)
			{
				UtDebug.LogError("UtUtilities.GetGroundHeight retuned NegativeInfinity ");
				GameObject gameObject = GameObject.Find("PfCommonLevel");
				CoCommonLevel coCommonLevel = null;
				if (gameObject != null)
				{
					coCommonLevel = gameObject.GetComponent<CoCommonLevel>();
				}
				if (coCommonLevel != null)
				{
					if (coCommonLevel._AvatarStartMarker != null)
					{
						vector = coCommonLevel._AvatarStartMarker[0].position;
						vector.y += num;
						SetPosition(vector);
						UtDebug.LogError("Avatar fall through reset to StartMarker");
					}
					else
					{
						SetPosition(new Vector3(0f, num, 0f));
						UtDebug.LogError("Avatar fall through reset to origin");
					}
				}
				else
				{
					SetPosition(new Vector3(0f, num, 0f));
					UtDebug.LogError("Avatar fall through reset to 0, " + num + ", 0");
				}
			}
			else
			{
				vector.y = groundHeight + num;
				SetPosition(vector);
				UtDebug.LogError("Avatar fall through reset to 10 feet above previous position");
			}
		}
		else
		{
			Vector3 position = oldPos;
			position.y = groundHeight + num;
			SetPosition(position);
			UtDebug.LogError("Avatar fall through reset to previous position");
		}
	}

	public new void Update()
	{
		if (AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			if (_Stats != null && !mHealthInitialized && UserRankData.pIsReady)
			{
				mHealthInitialized = true;
				_Stats._MaxHealth = UserRankData.GetAttribute(UserRankData.pInstance.RankID, "HEALTH", 1f, orLower: true);
				_Stats._CurrentHealth = _Stats._MaxHealth;
				_Stats._CurrentAir = _Stats._MaxAir;
				mCanRegenHealth = true;
			}
			if (mFlyingData != null && mRefreshFlightData)
			{
				if (!pPlayerMounted)
				{
					ApplyAvatarModifiers(ref mFlyingData);
					mRefreshFlightData = false;
				}
				else if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pIsMounted)
				{
					ApplyAvatarModifiers(ref mFlyingData);
					ApplyPetModifiers(ref mFlyingData);
					mRefreshFlightData = false;
				}
			}
			DoUpdate();
		}
		else if (pSubState == AvAvatarSubState.FLYING && _FlyingBone != null)
		{
			float b = Mathf.Clamp((0f - pVelocity.y) * 5f, -45f, 45f);
			mMMOPitch = Mathf.Lerp(mMMOPitch, b, Time.deltaTime);
			float b2 = Mathf.Clamp(pRotVel * 25f, -45f, 45f);
			mMMORoll = Mathf.Lerp(mMMORoll, b2, Time.deltaTime / 2f);
			_FlyingBone.localEulerAngles = new Vector3(mMMOPitch, 0f, mMMORoll);
		}
		AvatarData.InstanceInfo instanceInfo = GetInstanceInfo();
		if (instanceInfo != null && !AvatarHidden && !mAvatarFlashing)
		{
			ShowArmorWing(!instanceInfo.CheckAttributeSet(AvatarData.pPartSettings.AVATAR_PART_HAND, "ToggleWings") || pSubState == AvAvatarSubState.GLIDING);
		}
	}

	public void UpdateImmunity()
	{
		if (pImmune)
		{
			mImmuneTimer -= Time.deltaTime;
			if (mImmuneTimer < 0f)
			{
				SetImmune(isImmune: false);
			}
		}
	}

	private void OnAllItemsDownloaded()
	{
		pAvatarCustomization.pCustomAvatar.UpdateShaders(AvAvatar.pObject);
		UserItemData lastUsedFlightSuit = GetLastUsedFlightSuit();
		if (lastUsedFlightSuit != null)
		{
			UiAvatarItemCustomization.ApplyCustomizationOnPart(AvAvatar.mTransform.gameObject, AvatarData.pPartSettings.AVATAR_PART_WING, AvatarData.pInstance, lastUsedFlightSuit.UserInventoryID);
			UiAvatarItemCustomization.SaveAvatarPartAttributes(AvatarData.pPartSettings.AVATAR_PART_WING, lastUsedFlightSuit.UserInventoryID);
		}
		pAvatarCustomization.SaveCustomAvatar();
		StartCoroutine(WaitToInitiateGlide());
	}

	private IEnumerator WaitToInitiateGlide()
	{
		yield return new WaitUntil(() => AvatarData.pInstanceInfo.pIsReady);
		InitiateGliding();
	}

	public void ShowArmorWing(bool show)
	{
		if (!(mFlightArmorWing == null) && show != mFlightArmorWing.enabled)
		{
			mFlightArmorWing.enabled = show;
		}
	}

	public void DoUpdate()
	{
		if (mAvatarCustomization != null)
		{
			mAvatarCustomization.DoUpdate();
		}
		if (_MainRoot == null && _MainRootName.Length > 0)
		{
			_MainRoot = base.transform.Find(_MainRootName);
		}
		if (_Stats != null)
		{
			if (AvAvatar.pLevelState != AvAvatarLevelState.RACING && mCanRegenHealth)
			{
				_Stats._CurrentHealth += _Stats._HealthRegenRate * Time.deltaTime;
				if (_Stats._CurrentHealth > _Stats._MaxHealth)
				{
					_Stats._CurrentHealth = _Stats._MaxHealth;
				}
			}
			if (IsAirRefillingAllowed())
			{
				_Stats._CurrentAir += (0f - _Stats._AirUseRate) * Time.deltaTime;
				if (_Stats._CurrentAir > _Stats._MaxAir)
				{
					_Stats._CurrentAir = _Stats._MaxAir;
				}
			}
		}
		if (OnGround())
		{
			mLastPositionOnGround = base.transform.position;
		}
		UpdateImmunity();
		UpdateFlashing();
		if (mState >= AvAvatarState.PAUSED || mState == AvAvatarState.NONE)
		{
			ClearSpline();
			UpdateAnimation();
			return;
		}
		if (mState == AvAvatarState.SLIDING || mState == AvAvatarState.ZIPLINE || mState == AvAvatarState.SPRINGBOARD || mState == AvAvatarState.DIVING || mState == AvAvatarState.CANNON || mState == AvAvatarState.ONSPLINE)
		{
			if (mState == AvAvatarState.SPRINGBOARD && (_ControlMode == AvAvatarControlMode.KEYBOARD || _ControlMode == AvAvatarControlMode.BOTH))
			{
				mRotation = KAInput.GetAxis("Horizontal");
				mRotation *= mCurrentStateData._RotSpeed * Time.deltaTime;
				base.transform.Rotate(0f, mRotation, 0f);
			}
			base.Update();
			if (mEndReached)
			{
				if (mEndSplineMessageObject != null)
				{
					mEndSplineMessageObject.SendMessage("OnSplineEnded", base.gameObject);
					mEndSplineMessageObject = null;
				}
				if (mState == AvAvatarState.CANNON || mState == AvAvatarState.ONSPLINE)
				{
					pState = AvAvatarState.PAUSED;
				}
				else
				{
					pState = AvAvatarState.IDLE;
				}
			}
			UpdateAnimation();
			return;
		}
		float num = mCurrentStateData._Gravity * mGravityMultiplier;
		if (mSubState == AvAvatarSubState.GLIDING && AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			Vector3 vector = mVelocity;
			vector.y = 0f;
			float num2 = vector.magnitude / mMeanGlideSpeed;
			mVelocity.y = pGravity;
			GameObject partObject = AvatarData.GetPartObject(base.transform, AvatarData.pPartSettings.AVATAR_PART_WING, 0);
			if (partObject != null)
			{
				GlideModeProperties component = partObject.GetComponent<GlideModeProperties>();
				if (component != null)
				{
					float num3 = 0.5f * (component._GlideModeMinRateOfDescent + component._GlideModeMaxRateOfDescent);
					mVelocity.y = num3;
					if (num2 > 1.1f)
					{
						mVelocity.y = (1f - num2) * num3 + num2 * component._GlideModeMaxRateOfDescent;
					}
					else if (num2 < 0.9f)
					{
						mVelocity.y = (1f - num2) * num3 + num2 * component._GlideModeMinRateOfDescent;
					}
				}
			}
			mVelocity.y *= pGravityMultiplier;
		}
		else if (mState != AvAvatarState.SPIN && mSubState != AvAvatarSubState.WALLCLIMB)
		{
			mVelocity.y += num * Time.deltaTime;
		}
		if (AvAvatar.IsCurrentPlayer(base.gameObject) && !AvAvatar.pInputEnabled)
		{
			if (mSpline != null)
			{
				base.Update();
				if (mEndReached)
				{
					if (mEndSplineMessageObject != null)
					{
						mEndSplineMessageObject.SendMessage("OnSplineEnded", base.gameObject);
						mEndSplineMessageObject = null;
					}
					ClearSpline();
					AvAvatar.SetUIActive(inActive: true);
					AvAvatar.pInputEnabled = true;
					if ((bool)AvAvatar.pAvatarCam)
					{
						AvAvatar.pAvatarCam.GetComponent<CaAvatarCam>()._IgnoreCollision = false;
					}
				}
			}
			else
			{
				VelocityUpdate(0f);
				if (IsFlyingOrGliding())
				{
					UpdateFlying(0f, 0f);
				}
				else if (pSubState == AvAvatarSubState.UWSWIMMING)
				{
					UpdateUWSwimmingControl(0f, 0f);
				}
			}
		}
		else if (_ControlMode == AvAvatarControlMode.MOUSE)
		{
			MouseUpdate();
		}
		else if (_ControlMode == AvAvatarControlMode.KEYBOARD)
		{
			KeyboardUpdate();
		}
		else if (_ControlMode == AvAvatarControlMode.BOTH)
		{
			MouseUpdate();
			if ((KAInput.GetAxis("Horizontal") != 0f || KAInput.GetAxis("Vertical") != 0f || KAInput.GetButton("Jump")) && mSpline != null)
			{
				ClearSpline();
			}
			KeyboardUpdate();
		}
		else if (AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			VelocityUpdate(0f);
			if (IsFlyingOrGliding())
			{
				UpdateFlying(0f, 0f);
			}
			else if (pSubState == AvAvatarSubState.UWSWIMMING)
			{
				UpdateUWSwimmingControl(0f, 0f);
			}
		}
		if (AvAvatar.pSubState != AvAvatarSubState.WALLCLIMB && AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			if (!IsFlyingOrGliding())
			{
				Vector3 vector2 = mVelocity;
				if (OnGround())
				{
					vector2.y = 0f;
				}
				if (vector2.magnitude > 0.5f && AvAvatar.pAvatarCam != null)
				{
					CaAvatarCam component2 = AvAvatar.pAvatarCam.GetComponent<CaAvatarCam>();
					if (component2 != null && component2.GetCurrentLayer().mode == CaAvatarCam.CameraMode.MODE_RELATIVE)
					{
						Vector3 offset = component2.GetOffset();
						base.transform.Rotate(0f, offset.x, 0f);
						component2.SetYaw(0f);
						component2.ResetLookAt();
					}
				}
			}
			else if (!KAInput.GetMouseButton(1) && AvAvatar.pAvatarCam != null)
			{
				CaAvatarCam component3 = AvAvatar.pAvatarCam.GetComponent<CaAvatarCam>();
				if (component3 != null && component3.GetCurrentLayer().mode == CaAvatarCam.CameraMode.MODE_RELATIVE)
				{
					Vector3 offset2 = component3.GetOffset();
					offset2.x = Mathf.Lerp(offset2.x, 0f, Time.deltaTime * 3f);
					component3.SetYaw(offset2.x);
				}
			}
		}
		if (mTriggerJump)
		{
			if ((OnGround() && pCanJump && !mIsBouncing && !mPlayerDragging) || mSubState == AvAvatarSubState.SWIMMING)
			{
				mCanJump = false;
				if (pPlayerMounted && SanctuaryManager.pCurPetInstance != null)
				{
					SanctuaryManager.pCurPetInstance.TriggerJumpEvent(isJump: true);
				}
				float num4 = (pPlayerCarrying ? 0.25f : 1f);
				if (mSubState == AvAvatarSubState.SWIMMING)
				{
					pSubState = AvAvatarSubState.NORMAL;
				}
				mVi = GetJumpVelocity(mCurrentStateData._JumpValues._MinJumpHeight * num4);
				mVelocity.y = mVi;
				pState = AvAvatarState.FALLING;
				JumpAnimData randomJumpAnimData = GetRandomJumpAnimData();
				if (randomJumpAnimData != null)
				{
					mCurrentStateData._JumpValues._JumpAnim = randomJumpAnimData._JumpAnim;
					mCurrentStateData._JumpValues._FallAnim = randomJumpAnimData._FallAnim;
					mCurrentStateData._JumpValues._LandAnim = randomJumpAnimData._LandAnim;
				}
				if ((bool)_MainRoot)
				{
					_MainRoot.localEulerAngles = Vector3.zero;
				}
				if (MainStreetMMOClient.pInstance != null)
				{
					MainStreetMMOClient.pInstance.SendUpdate(MMOAvatarFlags.JUMP);
				}
				if (mJumpSound != null)
				{
					mJumpSound.Play(inForce: true);
				}
			}
			mTriggerJump = false;
			mJumpSound = null;
		}
		if (mState >= AvAvatarState.PAUSED || mState == AvAvatarState.NONE)
		{
			UpdateAnimation();
			return;
		}
		Vector3 vector3 = ((mLockVelocity ? mLockedVelocity : mVelocity) + mPushVelocity) * Time.deltaTime;
		if (mPushTimer > Mathf.Epsilon)
		{
			mPushVelocity = Vector3.Lerp(mPushVelocity, Vector3.zero, Time.deltaTime * (1f / mPushTimer));
		}
		else
		{
			mPushVelocity = Vector3.zero;
		}
		mForwardCollisionNormal = Vector3.zero;
		mGroundCollisionNormal = Vector3.zero;
		if (!mIsBouncing)
		{
			if (mController.gameObject.activeInHierarchy)
			{
				if (mPlayerDragging && mDragObject != null)
				{
					if (mDraggable != null && vector3 == mDraggable.Move(base.transform.position, vector3))
					{
						return;
					}
					Vector3 vector4 = base.transform.position + vector3;
					Vector3 vector5 = new Vector3(mHandleMarker.position.x, base.transform.position.y, mHandleMarker.position.z);
					float num5 = Vector3.Distance(vector5, vector4);
					vector3 += Vector3.Lerp(vector4, vector5, _ObjectDragSnapPosLimit / num5) - vector4;
					collisionFlags = mController.Move(vector3);
					Vector3 vector6 = mDragObject.transform.position - base.transform.position;
					Vector3 forward = base.transform.forward;
					float num6 = Math.Abs(Vector3.Angle(vector6, forward)) - _ObjectDragAngleLimit;
					if (num6 > 0f)
					{
						float num7 = Vector3.Dot(Vector3.Cross(vector6, forward), base.transform.up);
						if (num7 > 0f)
						{
							base.transform.Rotate(0f, 0f - num6, 0f);
						}
						else if (num7 < 0f)
						{
							base.transform.Rotate(0f, num6, 0f);
						}
					}
				}
				else
				{
					collisionFlags = mController.Move(vector3);
				}
				if (mSubState != AvAvatarSubState.WALLCLIMB && pState != AvAvatarState.FALLING && !IsFlyingOrGliding() && pSubState != AvAvatarSubState.UWSWIMMING && (collisionFlags & CollisionFlags.Below) == 0)
				{
					Vector3 sp = base.transform.position + new Vector3(0f, 1f, 0f);
					float num8 = sp.y - 1f - UtUtilities.GetGroundHeightNT(sp, _GroundSnapThreshold + 2f);
					if (num8 <= _GroundSnapThreshold)
					{
						vector3 = new Vector3(0f, 0f - (num8 + 0.2f), 0f);
						collisionFlags = mController.Move(vector3);
						mVelocity.y = 0f;
						collisionFlags |= CollisionFlags.Below;
					}
				}
			}
			else
			{
				Debug.LogError("ERROR: MOVE ON INACTIVE CONTROLLER: " + mController.gameObject.name);
			}
		}
		if ((collisionFlags & CollisionFlags.Above) != 0 && mVelocity.y > 0f)
		{
			mCurrentAnim = "";
			mVelocity.y = 0f;
		}
		if ((collisionFlags & CollisionFlags.Below) != 0)
		{
			if (Vector3.Dot(mGroundCollisionNormal, Vector3.up) > 0.707f)
			{
				OrientToNormal(mGroundCollisionNormal);
			}
			else
			{
				OrientToNormal(Vector3.up);
			}
			mLastTimeOnGround = Time.time;
			if (mCurrentStateData._JumpValues._JumpAnim.Length > 0 && !pCanJump)
			{
				mCanJump = true;
				if (pPlayerMounted && SanctuaryManager.pCurPetInstance != null)
				{
					SanctuaryManager.pCurPetInstance.TriggerJumpEvent(isJump: false);
				}
			}
			if (mSubState == AvAvatarSubState.SWIMMING)
			{
				if (mWaterObject == null)
				{
					UtDebug.LogWarning("Avatar " + base.gameObject.name + " in swim state, but no swim object");
					pSubState = AvAvatarSubState.NORMAL;
					ApplySwimAnim(SwimAnimState.NONE);
				}
				else if (mWaterObject.transform.position.y - base.transform.position.y <= _Height * base.transform.lossyScale.y)
				{
					pSubState = AvAvatarSubState.NORMAL;
					ApplySwimAnim(SwimAnimState.NONE);
					mWaterObject = null;
					KAInput.pInstance.EnableInputType("Jump", InputType.UI_BUTTONS, inEnable: true);
					KAInput.pInstance.EnableInputType("WingFlap", InputType.UI_BUTTONS, inEnable: false);
				}
			}
			else if (IsFlyingOrGliding() && AvAvatar.IsCurrentPlayer(base.gameObject) && !_NoFlyingLanding && (pFlyingGlidingMode || (SanctuaryManager.pCurPetInstance != null && mFlightSpeed < SanctuaryManager.pCurPetInstance.pCurAgeData._RunSpeed)))
			{
				Vector3 position = base.transform.position;
				position.y += 0.5f;
				if (Physics.Raycast(new Ray(position, new Vector3(0f, -1f, 0f)), out var hitInfo, 1f, UtUtilities.GetGroundRayCheckLayers()) && !hitInfo.collider.gameObject.CompareTag("Water") && !hitInfo.collider.gameObject.CompareTag("NoLanding"))
				{
					Vector3 eulerAngles = _FlyingBone.localRotation.eulerAngles;
					if (pFlyingRotationPivot != null)
					{
						_FlyingBone.RotateAround(pFlyingRotationPivot.position, _FlyingBone.right, 0f - eulerAngles.x);
						_FlyingBone.RotateAround(pFlyingRotationPivot.position, _FlyingBone.forward, 0f - eulerAngles.z);
					}
					ProcessOnLand();
					if (mIsPlayerGliding)
					{
						OnGlideEnd();
					}
				}
			}
		}
		else
		{
			if (base.transform.parent != null)
			{
				if (!AvAvatar.IsCurrentPlayer(base.gameObject))
				{
					base.transform.parent = null;
				}
				else
				{
					AvAvatar.SetParentTransform(null);
				}
			}
			if (!IsFlyingOrGliding() && pSubState != AvAvatarSubState.UWSWIMMING)
			{
				OrientToNormal(Vector3.up);
			}
			if (mVelocity.y < 0f && mSubState == AvAvatarSubState.WALLCLIMB && UtUtilities.GetGroundHeight(mController.transform.position, _WallClimbParams._DetachDistance) != float.NegativeInfinity)
			{
				ResetWallClimb(AvAvatarAnim.WallClimbAnimState.WallClimbDownDismount);
				AvAvatar.pInputEnabled = true;
			}
		}
		if (mSubState == AvAvatarSubState.WALLCLIMB && mVelocity.magnitude > mCurrentStateData._RestThreshold)
		{
			bool flag = false;
			Vector3 forward2 = base.transform.forward;
			forward2.y = 0f;
			forward2.Normalize();
			RaycastHit[] array = Physics.RaycastAll(base.transform.position + Vector3.up * _WallClimbParams._Height + forward2 * (0f - _WallClimbParams._RaycastDistance), forward2, _WallClimbParams._RaycastDistance * 2f, UtUtilities.GetGroundRayCheckLayers() | (1 << LayerMask.NameToLayer("Wall")));
			Vector3 vector7 = Vector3.forward;
			Vector3 vector8 = Vector3.zero;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].collider.CompareTag("WallClimb"))
				{
					flag = true;
					vector7 = array[i].point;
					vector8 = array[i].normal;
				}
			}
			if (!flag)
			{
				if (mVelocity.y > mCurrentStateData._RestThreshold && Physics.Raycast(base.transform.position + Vector3.up * _WallClimbParams._Height + base.transform.forward * _WallClimbParams._PullUpForwardDistance, Vector3.down, mController.height, UtUtilities.GetGroundRayCheckLayers()))
				{
					mVelocity = Vector3.zero;
					base.transform.eulerAngles = new Vector3(0f, base.transform.eulerAngles.y, 0f);
					if (AvAvatar.IsCurrentPlayer(base.gameObject))
					{
						AvAvatar.pInputEnabled = false;
					}
					mCurrentAnim = mCurrentStateData._StateAnims._Idle;
				}
				else
				{
					if (mVelocity.y < 0f)
					{
						ResetWallClimb(AvAvatarAnim.WallClimbAnimState.WallClimbDownDismount);
					}
					else
					{
						ResetWallClimb(AvAvatarAnim.WallClimbAnimState.WallClimbUpDismount);
						Vector3 b = base.transform.position + new Vector3(0f, _WallClimbParams._PullUpSpeed, _WallClimbParams._PullUpForwardDistance);
						StartCoroutine(Translate(base.transform.position, b, _WallClimbParams._JumpSpeed));
					}
					AvAvatar.pInputEnabled = true;
				}
			}
			else if (vector8.magnitude > 0f)
			{
				base.transform.forward = Vector3.Slerp(base.transform.forward, vector8 * -1f, Time.deltaTime);
				base.transform.position = vector7 + base.transform.TransformDirection(0f, 0f - _WallClimbParams._Height, 0f - _WallClimbParams._AttachOffset);
			}
		}
		if (mState == AvAvatarState.ATTACK)
		{
			if (OnGround())
			{
				JumpBack();
			}
			UpdateAnimation();
			return;
		}
		switch (mSubState)
		{
		case AvAvatarSubState.NORMAL:
		case AvAvatarSubState.DIVESUIT:
		case AvAvatarSubState.SKATING:
			if (OnGround())
			{
				if (mVelocity.y < 0f && (collisionFlags & CollisionFlags.Below) != 0 && mForwardCollisionNormal.magnitude == 0f)
				{
					mVelocity.y = 0f;
				}
				if (mVelocity.magnitude > mCurrentStateData._RestThreshold || mRotation != 0f)
				{
					pState = AvAvatarState.MOVING;
				}
				else if (mState != AvAvatarState.IDLE)
				{
					pState = AvAvatarState.IDLE;
				}
			}
			else
			{
				if (mState == AvAvatarState.JUMPBACK || mState == AvAvatarState.TAKEHIT || mState == AvAvatarState.SPIN)
				{
					break;
				}
				if (mState != AvAvatarState.FALLING && mCurrentStateData._JumpValues._JumpAnim != mCurrentAnim)
				{
					JumpAnimData randomNoJumpAnimData = GetRandomNoJumpAnimData();
					if (randomNoJumpAnimData != null)
					{
						mCurrentStateData._JumpValues._FallAnim = randomNoJumpAnimData._FallAnim;
						mCurrentStateData._JumpValues._LandAnim = randomNoJumpAnimData._LandAnim;
					}
				}
				if (pState != AvAvatarState.FALLING)
				{
					pState = AvAvatarState.FALLING;
				}
			}
			break;
		case AvAvatarSubState.SWIMMING:
			if (mVelocity.magnitude > mCurrentStateData._RestThreshold || mRotation != 0f)
			{
				pState = AvAvatarState.MOVING;
			}
			else if (mState != AvAvatarState.IDLE)
			{
				pState = AvAvatarState.IDLE;
			}
			UpdateCurrentWaterDepth();
			break;
		default:
			if (mVelocity.magnitude > mCurrentStateData._RestThreshold || mRotation != 0f)
			{
				pState = AvAvatarState.MOVING;
			}
			else if (mState != AvAvatarState.IDLE)
			{
				pState = AvAvatarState.IDLE;
			}
			break;
		case AvAvatarSubState.FLYING:
			break;
		}
		AvAvatar.mNetworkVelocity = (mLockVelocity ? mLockedVelocity : mVelocity);
		UpdateAnimation();
		UpdateWaterSplash();
		if (AvAvatar.pLevelState != AvAvatarLevelState.RACING && !DisableSoarButtonUpdate)
		{
			bool inputTypeState = KAInput.pInstance.GetInputTypeState("Soar", InputType.UI_BUTTONS);
			if (AvAvatar.pState == AvAvatarState.FALLING || AvAvatar.pSubState == AvAvatarSubState.FLYING)
			{
				if (IsAllowedToGlide() && AboveGlideModeHeight() && !inputTypeState)
				{
					ShowSoarButton(inShow: true);
				}
				else if (!AboveGlideModeHeight() && inputTypeState)
				{
					ShowSoarButton(inShow: false);
				}
			}
			else if (inputTypeState)
			{
				ShowSoarButton(inShow: false);
			}
		}
		ProcessRemoveButton();
	}

	public void UpdateCurrentWaterDepth()
	{
		if (mWaterObject != null && mWaterObject.transform.position.y > (base.transform.position.y + mCurrentStateData._Height) * base.transform.lossyScale.y)
		{
			float num = mWaterObject.transform.position.y - (base.transform.position.y + mCurrentStateData._Height) * base.transform.lossyScale.y;
			mController.Move(new Vector3(0f, num * Time.deltaTime, 0f));
		}
	}

	private IEnumerator Translate(Vector3 a, Vector3 b, float speed)
	{
		float step = speed / (a - b).magnitude * Time.deltaTime;
		float t = 0f;
		while (t <= 1f)
		{
			t += step;
			base.transform.position = Vector3.Lerp(a, b, t);
			yield return new WaitForEndOfFrame();
		}
		base.transform.position = b;
	}

	public void OrientToNormal(Vector3 n)
	{
		if (pPlayerMounted && _FlyingBone != null && n.magnitude > Mathf.Epsilon)
		{
			Vector3 forward = base.transform.forward;
			if (Vector3.Dot(forward, n) < 1f)
			{
				forward = Vector3.Cross(Vector3.Cross(n, forward), n).normalized;
				Quaternion b = Quaternion.LookRotation(forward, n);
				_FlyingBone.rotation = Quaternion.Slerp(_FlyingBone.rotation, b, Time.deltaTime * 5f);
			}
		}
	}

	public void ApplySwimAnim(SwimAnimState newState)
	{
		if (newState == mSwimAnimState)
		{
			return;
		}
		mSwimAnimState = newState;
		StopParticles(mWaterParticles);
		GameObject gameObject = null;
		switch (newState)
		{
		case SwimAnimState.SPLASH:
			gameObject = _WaterSplashParticle;
			break;
		case SwimAnimState.SWIM:
			gameObject = _SwimParticle;
			break;
		case SwimAnimState.IDLE:
			gameObject = _SwimIdleParticle;
			break;
		case SwimAnimState.UWSWIM:
			gameObject = _UWSwimParticle;
			break;
		case SwimAnimState.UWIDLE:
			gameObject = _UWSwimIdleParticle;
			break;
		case SwimAnimState.UWBRAKE:
			gameObject = _UWSwimBrakeParticle;
			break;
		}
		if (_SwimParticleBone != null)
		{
			if (gameObject != null)
			{
				mWaterParticles = PlayParticles(gameObject, new Vector3(_SwimParticleBone.position.x, _SwimParticleBone.position.y + 0.4f, _SwimParticleBone.position.z));
				if (mWaterParticles != null)
				{
					mWaterParticles.transform.parent = _SwimParticleBone;
				}
			}
		}
		else
		{
			UtDebug.LogError("_SwimParticleBone not set to attach water particle.");
		}
	}

	public void UpdateWaterSplash()
	{
		bool flag = pSubState == AvAvatarSubState.SWIMMING && !pPlayerMounted;
		bool flag2 = IsFlyingOrGliding() && mVelocity.magnitude > 5f;
		if (!(flag2 || flag))
		{
			return;
		}
		bool flag3 = mWaterObject != null;
		RaycastHit hitInfo = default(RaycastHit);
		Vector3 origin = base.transform.position + new Vector3(0f, 0.5f, 0f);
		if (flag3 || Physics.Raycast(origin, new Vector3(0f, -1f, 0f), out hitInfo, 5.5f, UtUtilities.GetGroundRayCheckLayers()))
		{
			if (!flag3 && !hitInfo.collider.gameObject.CompareTag("Water"))
			{
				return;
			}
			if (flag2)
			{
				Vector3 pos = (flag3 ? new Vector3(base.transform.position.x, mWaterObject.transform.position.y, base.transform.position.z) : hitInfo.point);
				PlayParticleAtPos(_WaterSkimParticle, pos, base.transform.rotation);
			}
			else if (mSwimAnimState == SwimAnimState.NONE)
			{
				ApplySwimAnim(SwimAnimState.SPLASH);
				mWaterCheckTime = 0f;
			}
			else if (mWaterCheckTime >= mWaterSplashInterval)
			{
				if (mVelocity.magnitude < 0.2f)
				{
					ApplySwimAnim(SwimAnimState.IDLE);
				}
				else
				{
					ApplySwimAnim(SwimAnimState.SWIM);
				}
			}
			mWaterCheckTime += Time.deltaTime;
		}
		else
		{
			mWaterCheckTime = 0f;
		}
	}

	private bool IsAllowedToGlide()
	{
		if (DragonTaxiManager.pInstance != null || !mCanGlide)
		{
			return false;
		}
		if (AvatarData.pInstanceInfo.FlightSuitEquipped())
		{
			return true;
		}
		if (pPlayerMounted)
		{
			return GetLastUsedFlightSuit() != null;
		}
		return false;
	}

	public ItemData GetInventoryItem(int[] categoryID)
	{
		if (CommonInventoryData.pInstance != null)
		{
			UserItemData[] items = CommonInventoryData.pInstance.GetItems(categoryID);
			if (items != null && items.Length != 0 && items[0] != null)
			{
				return items[0].Item;
			}
		}
		return null;
	}

	private bool AboveGlideModeHeight()
	{
		bool result = false;
		float groundHeight = UtUtilities.GetGroundHeight(base.transform.position + new Vector3(0f, GroundCheckStartHeight, 0f), float.PositiveInfinity);
		if (groundHeight == float.NegativeInfinity)
		{
			result = true;
		}
		else if (base.transform.position.y - groundHeight > _GlideModeAutoHeight)
		{
			result = true;
		}
		return result;
	}

	private void FixedUpdate()
	{
		if (mNewParent != null)
		{
			if (!AvAvatar.IsCurrentPlayer(base.gameObject))
			{
				base.transform.parent = mNewParent;
			}
			else
			{
				AvAvatar.SetParentTransform(mNewParent);
			}
			mPlatformHitTime = Time.time;
			mNewParent = null;
		}
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (hit.collider.gameObject.CompareTag("Water") && AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			ObAvatarRespawn component = hit.gameObject.GetComponent<ObAvatarRespawn>();
			if (component != null)
			{
				component.DoRespawn(base.gameObject);
			}
			ShowSoarButton(inShow: false);
		}
		if (OnAvatarCollision != null)
		{
			OnAvatarCollision(base.gameObject, hit.gameObject);
		}
		if (IsFlyingOrGliding() && !pFlyingGlidingMode && _MainRoot != null)
		{
			float num = Mathf.Clamp(0f - Vector3.Dot(_MainRoot.forward, hit.normal), 0f, 1f);
			if (!hit.collider.gameObject.CompareTag("Water") && mFlyingData != null)
			{
				mFlightSpeed = Mathf.Lerp(mFlightSpeed, mFlyingData._Speed.Min, num * Time.deltaTime * 5f);
			}
			if (hit.normal.y > 0.707f && pFlyingPitch > 0f)
			{
				if (AvAvatar.pLevelState == AvAvatarLevelState.NORMAL && !hit.collider.gameObject.CompareTag("Water"))
				{
					mFlightSpeed = Mathf.Lerp(mFlightSpeed, 0f, num * Time.deltaTime * 5f);
				}
				pFlyingPitch = Mathf.Lerp(pFlyingPitch, 0f, Time.deltaTime * 5f);
			}
		}
		if (hit.gameObject.CompareTag("WallClimb"))
		{
			SetWallClimbState(hit.transform);
		}
		else if (hit.gameObject.CompareTag("Platform"))
		{
			if (base.transform.parent != hit.gameObject.transform)
			{
				mNewParent = hit.gameObject.transform;
			}
		}
		else if (base.transform.parent != null && mPlatformHitTime != Time.time)
		{
			if (!AvAvatar.IsCurrentPlayer(base.gameObject))
			{
				base.transform.parent = null;
			}
			else
			{
				AvAvatar.SetParentTransform(null);
			}
		}
		if (hit.normal.y > 0f)
		{
			float num2 = 0.40673664f;
			if (hit.normal.y < num2)
			{
				mForwardCollisionNormal += hit.normal;
				mForwardCollisionNormal.Normalize();
			}
			else
			{
				if (hit.gameObject.CompareTag("NoOrient"))
				{
					mGroundCollisionNormal += Vector3.up;
				}
				else
				{
					mGroundCollisionNormal += hit.normal;
				}
				mGroundCollisionNormal.Normalize();
			}
		}
		Rigidbody rigidbody = hit.rigidbody;
		if ((bool)rigidbody && !rigidbody.isKinematic && hit.moveDirection.y >= -0.3f && !hit.gameObject.CompareTag("Projectile") && !hit.gameObject.tag.StartsWith("PowerBall") && !hit.gameObject.tag.StartsWith("Draggable"))
		{
			Vector3 vector = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
			rigidbody.velocity = vector * mCurrentStateData._PushPower;
			hit.gameObject.SendMessage("OnPushed", base.gameObject, SendMessageOptions.DontRequireReceiver);
		}
		if (hit.collider.gameObject.CompareTag("JumpTrigger") && mState == AvAvatarState.FALLING)
		{
			Jump(_JumpTriggerSound);
		}
		if (hit.collider.gameObject.CompareTag("Bounceable") && IsAllowedToBounce() && !mIsBouncing && Mathf.Abs(Vector3.Dot(mVelocity, hit.normal)) > _AvatarBounceParams._BounceStartThreshold)
		{
			StartBouncing();
			if (MainStreetMMOClient.pInstance != null)
			{
				MainStreetMMOClient.pInstance.SendUpdate(MMOAvatarFlags.STARTBOUNCE);
			}
		}
	}

	public virtual GameObject PlayParticles(GameObject pfx, Vector3 pos)
	{
		GameObject result = null;
		if (pfx != null)
		{
			result = UnityEngine.Object.Instantiate(pfx, pos, Quaternion.identity);
		}
		return result;
	}

	public virtual void StopParticles(GameObject pfx)
	{
		if (pfx != null)
		{
			ParticleSystem[] componentsInChildren = pfx.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Stop();
			}
			UnityEngine.Object.Destroy(pfx);
			pfx = null;
		}
	}

	public virtual void PlayParticleAtPos(GameObject pfx, Vector3 pos, Quaternion rot)
	{
		if (pfx != null)
		{
			if (mParticlePool == null)
			{
				PoolManager.Pools.TryGetValue("Particles", out mParticlePool);
			}
			if (mParticlePool != null)
			{
				mParticlePool.Spawn(pfx, pos, rot);
			}
			else
			{
				UnityEngine.Object.Instantiate(pfx, pos, rot);
			}
		}
	}

	private bool IsAllowedToBounce()
	{
		if (!AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			return false;
		}
		if (!IsMember())
		{
			return false;
		}
		string[] bouncingAreas = _AvatarBounceParams._BouncingAreas;
		foreach (string value in bouncingAreas)
		{
			if (RsResourceManager.pCurrentLevel.Equals(value))
			{
				return true;
			}
		}
		return false;
	}

	public void StartBouncing()
	{
		if (mBouncingObject == null)
		{
			mBouncingObject = UnityEngine.Object.Instantiate(_AvatarBounceParams._BouncingObject);
			mBouncingObject.SendMessage("SetBounceStopThreshold", _AvatarBounceParams._BounceStopThreshold, SendMessageOptions.DontRequireReceiver);
		}
		mIsBouncing = true;
		mBounceVelocity = GetJumpVelocity(mCurrentStateData._JumpValues._MinJumpHeight);
		Rigidbody component = mBouncingObject.GetComponent<Rigidbody>();
		component.MovePosition(base.transform.position);
		component.velocity = new Vector3(mVelocity.x, mBounceVelocity * _AvatarBounceParams._BouncingFactor, mVelocity.z);
		mBouncingObject.SendMessage("StartBouncing", this, SendMessageOptions.DontRequireReceiver);
	}

	public void PlayBounceSound(bool inWithAnimation)
	{
		_AvatarBounceParams._BounceSound.Play(inForce: true);
		if (inWithAnimation)
		{
			JumpAnimData randomJumpAnimData = GetRandomJumpAnimData();
			if (randomJumpAnimData != null)
			{
				mCurrentStateData._JumpValues._JumpAnim = randomJumpAnimData._JumpAnim;
				mCurrentStateData._JumpValues._FallAnim = randomJumpAnimData._FallAnim;
				mCurrentStateData._JumpValues._LandAnim = randomJumpAnimData._LandAnim;
			}
			mCurrentAnim = mCurrentStateData._JumpValues._JumpAnim;
			AvAvatar.PlayAnim(base.gameObject, mCurrentStateData._JumpValues._JumpAnim, WrapMode.ClampForever);
		}
	}

	public void StopBouncing()
	{
		if (AvAvatar.IsCurrentPlayer(base.gameObject) && MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.SendUpdate(MMOAvatarFlags.STOPBOUNCE);
		}
		mIsBouncing = false;
		pState = AvAvatarState.FALLING;
		pSubState = AvAvatarSubState.NORMAL;
		UpdateAnimation();
	}

	public void StopBouncingObject()
	{
		if (mBouncingObject != null)
		{
			mBouncingObject.SendMessage("StopBouncing", null, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void OnCollisionEnter(Collision iCollision)
	{
		if (iCollision.gameObject.CompareTag("Projectile"))
		{
			PrProjectile component = iCollision.gameObject.GetComponent<PrProjectile>();
			if (component != null && component.pProjectileSource != base.gameObject)
			{
				TakeHit(component._Damage, component._Force, iCollision.gameObject.transform);
			}
		}
	}

	private void OnTriggerEnter(Collider c)
	{
		if (mState != 0)
		{
			CheckForSubStateChange(c);
		}
	}

	private void OnTriggerStay(Collider c)
	{
		if (mState != 0)
		{
			CheckForSubStateChange(c);
		}
	}

	private void CheckForSubStateChange(Collider c)
	{
		int num = LayerMask.NameToLayer("Water");
		if (c.gameObject.layer == num)
		{
			bool flag = IsFlyingOrGliding();
			if (!flag && pPlayerMounted && AvAvatar.IsCurrentPlayer(base.gameObject) && SanctuaryManager.pCurPetInstance != null && !SanctuaryManager.pCurPetInstance.IsSwimming())
			{
				SanctuaryManager.pCurPetInstance.CheckSwim(c);
				SetFlyingState(FlyingState.SkimOverWater);
			}
			if (mSubState != AvAvatarSubState.SWIMMING && mSubState != AvAvatarSubState.UWSWIMMING)
			{
				bool flag2 = false;
				if (flag && AvAvatar.IsCurrentPlayer(base.gameObject))
				{
					if (pSubState == AvAvatarSubState.GLIDING && mFlightSpeed < _FlySpeedLimitToSwim)
					{
						flag2 = true;
						OnGlideLanding();
					}
					else
					{
						if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pAge == 2 && mFlightSpeed < _FlySpeedLimitToSwim)
						{
							SanctuaryManager.pCurPetInstance.OnFlyLanding(base.gameObject);
						}
						SetFlyingState(FlyingState.SkimOverWater);
					}
				}
				else if (c.transform.position.y - base.transform.position.y > _Height * base.transform.lossyScale.y)
				{
					flag2 = true;
				}
				if (flag2)
				{
					if (AvAvatar.IsCurrentPlayer(base.gameObject) && (mState == AvAvatarState.JUMPBACK || mState == AvAvatarState.TAKEHIT || mState == AvAvatarState.ATTACK))
					{
						AvAvatar.pInputEnabled = true;
					}
					float y = c.transform.position.y - _Height * base.transform.lossyScale.y;
					base.transform.position = new Vector3(base.transform.position.x, y, base.transform.position.z);
					pState = AvAvatarState.IDLE;
					pSubState = AvAvatarSubState.SWIMMING;
					mCurrentAnim = "none";
					if (pPlayerMounted)
					{
						ApplySwimAnim(SwimAnimState.NONE);
						KAInput.pInstance.EnableInputType("Jump", InputType.UI_BUTTONS, SanctuaryManager.pCurPetInstance.pTypeInfo._Flightless);
						KAInput.pInstance.EnableInputType("WingFlap", InputType.UI_BUTTONS, !SanctuaryManager.pCurPetInstance.pTypeInfo._Flightless);
						KAInput.pInstance.EnableInputType("DragonMount", InputType.ALL, mWasDragonMountEnabled);
					}
				}
			}
			mWaterObject = c.gameObject;
		}
		else if (c.CompareTag("WallClimb"))
		{
			SetWallClimbState(c.transform);
		}
	}

	private void SetWallClimbState(Transform trans)
	{
		if (mSubState == AvAvatarSubState.WALLCLIMB || pPlayerMounted || !(mClimbAnimTime <= Time.time) || base.transform.InverseTransformDirection(mVelocity).z <= mCurrentStateData._RestThreshold || !Physics.Raycast(base.transform.position + Vector3.up * _Height, base.transform.forward, out var hitInfo, float.PositiveInfinity, 1 << LayerMask.NameToLayer("Wall")) || !(hitInfo.transform == trans))
		{
			return;
		}
		float num = Vector3.Angle(base.transform.forward, hitInfo.normal);
		if (180f - num <= _WallClimbParams._AttachAngle)
		{
			if (mIsPlayerGliding)
			{
				OnGlideLanding();
			}
			base.transform.forward = hitInfo.normal * -1f;
			pSubState = AvAvatarSubState.WALLCLIMB;
			mWasDragonMountEnabled = FUEManager.IsInputEnabled("DragonMount");
			KAInput.pInstance.EnableInputType("DragonMount", InputType.ALL, inEnable: false);
			if ((bool)mAvAvatarAnim)
			{
				mAvAvatarAnim.SetWallClimbAnim(AvAvatarAnim.WallClimbAnimState.WallClimbMount);
			}
			SetToolbarButtonsVisible(visible: false);
			if (mToolBar != null && !FUEManager.pIsFUERunning)
			{
				mToolBar.SetBackBtnEnabled(enable: true);
			}
			if (SanctuaryManager.pCurPetInstance != null)
			{
				SanctuaryManager.pCurPetInstance.SetFollowAvatar(follow: false);
				SanctuaryManager.pCurPetInstance.FallToGround();
				SanctuaryManager.pCurPetInstance.SetState(Character_State.idle);
				SanctuaryManager.pCurPetInstance.AIActor.SetState(AISanctuaryPetFSM.IDLE);
			}
		}
	}

	private void ResetWallClimb(AvAvatarAnim.WallClimbAnimState animState)
	{
		if (mAvAvatarAnim != null)
		{
			mAvAvatarAnim.SetWallClimbAnim(animState);
		}
		mClimbAnimTime = Time.time + _WallClimbParams._AttachDelay;
		pSubState = AvAvatarSubState.NORMAL;
		if (SanctuaryManager.pInstance._CreateInstance)
		{
			KAInput.pInstance.EnableInputType("DragonMount", InputType.ALL, mWasDragonMountEnabled);
		}
		SetToolbarButtonsVisible(visible: true);
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pCurPetInstance.SetFollowAvatar(follow: true);
		}
	}

	public void ResetAvatarState()
	{
		if (pIsPlayerGliding)
		{
			OnGlideLanding();
		}
		if (pSubState == AvAvatarSubState.WALLCLIMB)
		{
			ResetWallClimb(AvAvatarAnim.WallClimbAnimState.WallClimbDownDismount);
		}
		if (pPlayerDragging)
		{
			KAInput.pInstance.EnableInputType("Jump", InputType.UI_BUTTONS, inEnable: true);
			StopDrag();
		}
	}

	private void OnTriggerExit(Collider c)
	{
		if (c.gameObject == mWaterObject)
		{
			AvAvatarSubState avAvatarSubState = mSubState;
			if (avAvatarSubState == AvAvatarSubState.FLYING || avAvatarSubState == AvAvatarSubState.GLIDING)
			{
				SetFlyingState(FlyingState.Normal);
			}
			ApplySwimAnim(SwimAnimState.NONE);
			mWaterObject = null;
		}
	}

	public void OnEnemyCollide(GameObject enemy)
	{
		float num = base.transform.position.y - enemy.transform.position.y;
		CharacterController characterController = (CharacterController)enemy.GetComponent<Collider>();
		EnEnemy component = enemy.GetComponent<EnEnemy>();
		mTakeAvatarMoney = component._TakeAvatarMoney;
		if (num > characterController.center.y)
		{
			switch (pState)
			{
			case AvAvatarState.FALLING:
				if (component._IsButtStompVulnerable)
				{
					float f = mCurrentStateData._Gravity * mGravityMultiplier;
					float num2 = Mathf.Sqrt(component._LaunchHeight * Mathf.Abs(f) * 2f);
					mVelocity += Vector3.up * (num2 - mVelocity.y);
					JumpAnimData randomSuperJumpAnimData = GetRandomSuperJumpAnimData();
					if (randomSuperJumpAnimData != null)
					{
						mCurrentStateData._JumpValues._SuperJumpAnim = randomSuperJumpAnimData._JumpAnim;
						mCurrentStateData._JumpValues._FallAnim = randomSuperJumpAnimData._FallAnim;
						mCurrentStateData._JumpValues._LandAnim = randomSuperJumpAnimData._LandAnim;
					}
					mCurrentAnim = mCurrentStateData._JumpValues._SuperJumpAnim;
					AvAvatar.PlayAnim(base.gameObject, mCurrentStateData._JumpValues._SuperJumpAnim, WrapMode.ClampForever);
				}
				else if (component._DoDamage)
				{
					TakeHit(component._Damage, component._AttackForce, (component.pState == EnEnemyState.ATTACK) ? component.transform : null);
				}
				break;
			case AvAvatarState.ATTACK:
				if (component._IsButtStompVulnerable)
				{
					JumpBack();
					enemy.SendMessage("TakeHit", _AttackDamage, SendMessageOptions.DontRequireReceiver);
				}
				else if (component._DoDamage)
				{
					TakeHit(component._Damage, component._AttackForce, (component.pState == EnEnemyState.ATTACK) ? component.transform : null);
				}
				break;
			default:
				if (component._DoDamage)
				{
					TakeHit(component._Damage, component._AttackForce, (component.pState == EnEnemyState.ATTACK) ? component.transform : null);
				}
				break;
			}
		}
		else if (component._DoDamage)
		{
			TakeHit(component._Damage, component._AttackForce, (component.pState == EnEnemyState.ATTACK) ? component.transform : null);
		}
	}

	public void JumpBack()
	{
		pState = AvAvatarState.JUMPBACK;
		mVelocity = Vector3.up * GetJumpVelocity(mCurrentStateData._JumpBack.y) + base.transform.forward * mCurrentStateData._JumpBack.z;
	}

	public void StartDrag(GameObject dragObject)
	{
		mDragObject = dragObject;
		mDraggable = mDragObject.GetComponent<ObDraggable>();
		if (!(mDraggable != null))
		{
			return;
		}
		mDragSmall = mDraggable._DragSmall;
		GameObject gameObject = null;
		float num = -1f;
		GameObject[] avatarMarker = mDraggable._AvatarMarker;
		foreach (GameObject gameObject2 in avatarMarker)
		{
			float num2 = Vector3.Distance(gameObject2.transform.position, base.transform.position);
			if (num2 < num || gameObject == null)
			{
				num = num2;
				gameObject = gameObject2;
			}
		}
		if (gameObject != null)
		{
			mHandleMarker = gameObject.transform;
			base.transform.position = new Vector3(gameObject.transform.position.x, base.transform.position.y, gameObject.transform.position.z);
			Vector3 forward = mDragObject.transform.position - base.transform.position;
			forward.y = 0f;
			base.transform.rotation = Quaternion.LookRotation(forward);
		}
		pPlayerDragging = true;
	}

	public void StopDrag(bool bMMO = true)
	{
		if ((bool)mDragObject)
		{
			mDragObject = null;
		}
		if ((bool)mDraggable)
		{
			mDraggable = null;
		}
		pPlayerDragging = false;
	}

	public void Collect(GameObject collectObject)
	{
		if (collectObject.CompareTag("Health"))
		{
			ObCollectHealth component = collectObject.GetComponent<ObCollectHealth>();
			if (component != null && AvAvatar.pToolbar != null)
			{
				AvAvatar.pToolbar.SendMessage("UpdateHealth", component._Health, SendMessageOptions.DontRequireReceiver);
			}
		}
		else if (collectObject.CompareTag("Carriable"))
		{
			CarryObject(collectObject);
		}
	}

	public void CarryObject(GameObject collectObject, bool duplicateItem = true)
	{
		CarryObject(collectObject, _CarryOffset, duplicateItem);
	}

	public void CarryObject(GameObject collectObject, Vector3 carryOffset, bool duplicateItem = true)
	{
		if (mCarriedObject != null)
		{
			mCarriedObject.Delete();
		}
		mCarriedObject = null;
		ObCollect component = collectObject.GetComponent<ObCollect>();
		if (component != null)
		{
			if (component._CarryObject == null)
			{
				UtDebug.LogError("Carriable object collected but no CarryObject defined!");
			}
			else
			{
				mCarriedObject = new CarriedObject(component._CarryObject, duplicateItem);
			}
		}
		else
		{
			mCarriedObject = new CarriedObject(collectObject, duplicateItem);
		}
		if (mCarriedObject == null || !(mCarriedObject._CarriedObject != null))
		{
			return;
		}
		mCarriedObject._CarriedObject.transform.parent = _HandBone;
		mCarriedObject._CarriedObject.transform.localPosition = carryOffset;
		mCarriedObject._CarriedObject.transform.localRotation = Quaternion.identity;
		AvAvatar.PlayAnimInstant(base.gameObject, _CarryAnim, WrapMode.Loop);
		GameObject partObject = AvatarData.GetPartObject(base.transform, AvatarData.pPartSettings.AVATAR_PART_TOP, 0);
		if (partObject != null)
		{
			AvAvatarAnim component2 = partObject.GetComponent<AvAvatarAnim>();
			if (component2 != null)
			{
				component2.enabled = false;
			}
		}
		mPlayerCarrying = true;
		mCanGlide = false;
	}

	public void RemoveCarriedObject(bool forceDestroy = false)
	{
		if (mCarriedObject != null)
		{
			mCarriedObject.Delete(forceDestroy);
			mCarriedObject = null;
			GameObject partObject = AvatarData.GetPartObject(base.transform, AvatarData.pPartSettings.AVATAR_PART_TOP, 0);
			if (partObject != null)
			{
				AvAvatarAnim component = partObject.GetComponent<AvAvatarAnim>();
				if (component != null)
				{
					component.enabled = true;
				}
			}
		}
		mPlayerCarrying = false;
		mCanGlide = true;
	}

	public void TakeHit(int damage, Vector3 force, Transform transform)
	{
	}

	public void TakeHit(float damage, ObAmmo ammo)
	{
		if (ammo != null && IsAvatarImmune(ammo.pWeapon))
		{
			damage = 0f;
		}
		TakeHit(damage, ammo.gameObject);
	}

	public void TakeHit(float damage, GameObject go = null)
	{
		if (!AvAvatar.IsCurrentPlayer(base.gameObject) || damage == 0f || pImmune || mState == AvAvatarState.NONE || mState == AvAvatarState.PAUSED)
		{
			return;
		}
		if (OnTakeDamage != null)
		{
			OnTakeDamage(go, ref damage);
		}
		if (SanctuaryManager.pCurPetInstance == null && _Stats != null)
		{
			_Stats._CurrentHealth = Mathf.Max(0f, _Stats._CurrentHealth - damage);
		}
		if ((SanctuaryManager.pCurPetInstance == null && _Stats != null && _Stats._CurrentHealth == 0f) || (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.GetMeterValue(SanctuaryPetMeterType.HEALTH) <= SanctuaryManager.pCurPetInstance._MinPetMeterValue))
		{
			GameObject[] array = GameObject.FindGameObjectsWithTag("Respawn");
			GameObject gameObject = array[UnityEngine.Random.Range(0, array.Length)];
			if (gameObject != null)
			{
				AvAvatar.TeleportToObject(gameObject);
			}
		}
		if (OnAvatarHealth != null && _Stats != null)
		{
			OnAvatarHealth(_Stats._CurrentHealth);
		}
		OnEffectSlow(WeaponTuneData.pInstance._SlowEffect._Strength, WeaponTuneData.pInstance._SlowEffect._Time);
		SetImmune(isImmune: true);
	}

	private bool IsAvatarImmune(WeaponTuneData.Weapon inWeapon)
	{
		if (inWeapon != null)
		{
			AvAvatarLevelState[] avatarNonImmuneLevelStates = inWeapon._AvatarNonImmuneLevelStates;
			for (int i = 0; i < avatarNonImmuneLevelStates.Length; i++)
			{
				if (avatarNonImmuneLevelStates[i] == AvAvatar.pLevelState)
				{
					return false;
				}
			}
		}
		return true;
	}

	private void GenerateCoins()
	{
		if (mTakeAvatarMoney)
		{
			ObBouncyCoinEmitter component = GetComponent<ObBouncyCoinEmitter>();
			if (component != null)
			{
				component.GenerateCoins();
			}
		}
	}

	public void UpdateAnimation()
	{
	}

	public bool OnGround()
	{
		if (mCurrentStateData == null)
		{
			return false;
		}
		return (collisionFlags & CollisionFlags.Below) != 0;
	}

	public float GetJumpVelocity(float height)
	{
		return Mathf.Sqrt(height * Mathf.Abs(mGravityMultiplier * mCurrentStateData._Gravity) * 2f);
	}

	public void ClearSpline()
	{
		mEndSplineMessageObject = null;
		if (mSpline != null)
		{
			SetSpline(null);
		}
	}

	public override void SetSpline(Spline sp)
	{
		base.SetSpline(sp);
		if (mProperties._NavTarget != null)
		{
			GameObject gameObject = GameObject.Find(mProperties._NavTarget.name);
			if ((bool)gameObject)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}
		if (mProperties._NavArrow != null)
		{
			GameObject gameObject2 = GameObject.Find(mProperties._NavArrow.name);
			if ((bool)gameObject2)
			{
				UnityEngine.Object.Destroy(gameObject2);
			}
		}
		if (sp != null)
		{
			mController.slopeLimit = 90f;
			mController.stepOffset = 0.65f;
			return;
		}
		mController.slopeLimit = mSlopeLimit;
		mController.stepOffset = mStepOffset;
		if (mCurrentStateData != null && mCurrentStateData._Gravity == 0f)
		{
			mVelocity = Vector3.zero;
		}
		else
		{
			mVelocity = Vector3.up * 0.1f;
		}
		mActivateObject = null;
	}

	public override void SetPosOnSpline(float p)
	{
		if (mState == AvAvatarState.SLIDING || mState == AvAvatarState.SPRINGBOARD || mState == AvAvatarState.DIVING || mState == AvAvatarState.CANNON || mState == AvAvatarState.ONSPLINE)
		{
			base.SetPosOnSpline(p);
		}
		else if (mState == AvAvatarState.ZIPLINE)
		{
			if (base.transform.parent != null)
			{
				CurrentPos = p;
				CurrentPos = mSpline.GetPosQuatByDist(CurrentPos, out var pos, out var quat);
				base.transform.parent.position = pos;
				base.transform.parent.rotation = quat;
			}
			else
			{
				base.SetPosOnSpline(p);
			}
		}
		else
		{
			CurrentPos = p;
			CurrentPos = mSpline.GetPosQuatByDist(CurrentPos, out var pos2, out var quat2);
			Vector3 forward = pos2 - base.transform.position;
			forward.y = 0f;
			quat2 = Quaternion.LookRotation(forward);
			base.transform.rotation = quat2;
			mVelocity.x = forward.x / Time.deltaTime;
			mVelocity.z = forward.z / Time.deltaTime;
		}
	}

	public void OnEmote(int id)
	{
		Transform transform = base.transform.Find("AvatarEmoticon");
		if ((bool)transform)
		{
			UnityEngine.Object.Destroy(transform.gameObject);
		}
		EmoticonActionData.Emoticon emoticonFromId = EmoticonActionData.GetEmoticonFromId(id);
		if (emoticonFromId != null)
		{
			transform = ((GameObject)UnityEngine.Object.Instantiate(EmoticonActionData._Bundle.LoadAsset(emoticonFromId.Emitter))).transform;
			transform.parent = base.transform;
			transform.localPosition = _EmoticonOffset;
			transform.name = "AvatarEmoticon";
			Texture mainTexture = (Texture)EmoticonActionData._Bundle.LoadAsset(emoticonFromId.Texture);
			ParticleSystemRenderer component = transform.GetComponent<ParticleSystemRenderer>();
			if (component != null)
			{
				component.materials[0].mainTexture = mainTexture;
				UnityEngine.Object.DontDestroyOnLoad(component.materials[0]);
			}
		}
	}

	public void OnAction(int id)
	{
	}

	public void OnChat(int id)
	{
		EmoticonActionData.Phrase phraseFromId = EmoticonActionData.GetPhraseFromId(id);
		if (phraseFromId != null)
		{
			OnChat(phraseFromId.Bubble);
		}
	}

	public void OnChat(string[] chat)
	{
		ChatOptions channel = ChatOptions.CHAT_ROOM;
		if (chat.Length > 1 && !string.IsNullOrEmpty(chat[1]))
		{
			if (chat[1] == ChatOptions.CHAT_FRIENDS.ToString())
			{
				channel = ChatOptions.CHAT_FRIENDS;
			}
			else if (chat[1] == ChatOptions.CHAT_CLANS.ToString())
			{
				channel = ChatOptions.CHAT_CLANS;
			}
		}
		OnChat(chat[0], channel);
	}

	public void OnChat(string chat)
	{
		OnChat(chat, ChatOptions.CHAT_ROOM);
	}

	public void OnChat(string chat, ChatOptions channel)
	{
		bool addToHistory = true;
		string text = "PfMyChatBubble";
		string displayName = GetInstanceInfo().mInstance.DisplayName;
		if (!AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			text = "PfChatBubble";
		}
		if (string.IsNullOrEmpty(chat))
		{
			addToHistory = false;
			if (!string.IsNullOrEmpty(_NoOpenChatText._Text))
			{
				chat = StringTable.GetStringData(_NoOpenChatText._ID, _NoOpenChatText._Text);
			}
		}
		string text2 = "";
		if (_ChatSizes != null)
		{
			AvAvatarChatSizes[] chatSizes = _ChatSizes;
			foreach (AvAvatarChatSizes avAvatarChatSizes in chatSizes)
			{
				if (chat.Length >= avAvatarChatSizes._MaxLetters)
				{
					continue;
				}
				string[] array = chat.Split(' ');
				bool flag = false;
				string[] array2 = array;
				for (int j = 0; j < array2.Length; j++)
				{
					if (array2[j].Length > avAvatarChatSizes._MaxWordLength)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					text2 = avAvatarChatSizes._MaxLetters.ToString();
					break;
				}
			}
		}
		Transform transform = _ChatBubbleParent.Find(text);
		if (transform != null && mChatBubbleSize != text2)
		{
			UnityEngine.Object.Destroy(transform.gameObject);
			transform = null;
		}
		if (transform == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources(text + text2));
			gameObject.name = text;
			mChatBubbleSize = text2;
			if (_ChatBubbleParent != null && AvAvatar.IsCurrentPlayer(base.gameObject))
			{
				gameObject.transform.parent = _ChatBubbleParent;
			}
			else
			{
				gameObject.transform.parent = base.transform;
			}
			gameObject.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
			transform = gameObject.transform;
		}
		string groupID = "";
		ChatBubble component = transform.GetComponent<ChatBubble>();
		component.WriteChat(chat, groupID, channel, displayName, base.gameObject, addToHistory);
		if (MainStreetMMOClient.pInstance.IsLevelRacing())
		{
			component.gameObject.SetActive(value: false);
		}
	}

	public void DisableInput(bool cancelInvokeFlag)
	{
		if (cancelInvokeFlag)
		{
			CancelInvoke("EnableInput");
		}
		DisableInput();
	}

	public void DisableInput()
	{
		AvAvatar.pInputEnabled = false;
	}

	public void EnableInput()
	{
		AvAvatar.pInputEnabled = true;
	}

	public void OnSpringBoardUse(ObSpringBoard inSB)
	{
		Spline newSpline = inSB.GetNewSpline(base.transform.position, 0f - pGravity);
		SetSpline(newSpline);
		Speed = inSB._Speed;
		_Draw = true;
		pState = AvAvatarState.SPRINGBOARD;
		AvAvatar.PlayAnim(base.gameObject, pCurrentStateData._JumpValues._JumpAnim, WrapMode.ClampForever);
		if (mPetObject != null)
		{
			mPetObject.OnAvatarSpringBoardUse(inSB);
		}
	}

	public AvatarData.InstanceInfo GetInstanceInfo()
	{
		AvatarData.InstanceInfo result = AvatarData.pInstanceInfo;
		if (!AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			MMOAvatar component = base.gameObject.GetComponent<MMOAvatar>();
			if (component != null)
			{
				result = component.pAvatarData;
			}
		}
		return result;
	}

	public string GetUserID()
	{
		string result = UserInfo.pInstance.UserID;
		if (!AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			MMOAvatar component = GetComponent<MMOAvatar>();
			result = ((!(component != null)) ? "" : component.pUserID);
		}
		return result;
	}

	public bool IsMember()
	{
		bool pIsMember = SubscriptionInfo.pIsMember;
		if (!AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			MMOAvatar component = GetComponent<MMOAvatar>();
			if (component != null)
			{
				pIsMember = component.pIsMember;
			}
		}
		return pIsMember;
	}

	public void SetPet(PsPet pet)
	{
		mPetObject = pet;
	}

	public void OnCoinsReceived()
	{
		if (!(_CoinsReceivedPrtPrefab == null))
		{
			Vector3 position = base.transform.position + _CoinsReceivedPrtOffset;
			GameObject gameObject = UnityEngine.Object.Instantiate(_CoinsReceivedPrtPrefab, position, Quaternion.identity);
			if (gameObject != null)
			{
				gameObject.transform.parent = base.transform;
				gameObject.SetActive(value: true);
			}
		}
	}

	public bool IsGoingUpOnRamp()
	{
		if (mVelocity.y < 0f)
		{
			return false;
		}
		if (!OnGround())
		{
			return false;
		}
		Vector3 vector = mVelocity;
		vector.y = 0f;
		if (vector.magnitude < 4f)
		{
			return false;
		}
		if (Mathf.Atan2(mVelocity.y, vector.magnitude) * 57.29578f < 40f)
		{
			return false;
		}
		return true;
	}

	private JumpAnimData GetRandomSuperJumpAnimData()
	{
		int num = mCurrentStateData._JumpValues._SuperJumpAnimData.Length;
		if (num < 1)
		{
			return null;
		}
		int num2 = UnityEngine.Random.Range(0, num);
		return mCurrentStateData._JumpValues._SuperJumpAnimData[num2];
	}

	private JumpAnimData GetRandomJumpAnimData()
	{
		int num = mCurrentStateData._JumpValues._JumpAnimData.Length;
		if (num < 1)
		{
			return null;
		}
		int num2 = UnityEngine.Random.Range(0, num);
		return mCurrentStateData._JumpValues._JumpAnimData[num2];
	}

	private JumpAnimData GetRandomNoJumpAnimData()
	{
		int num = mCurrentStateData._JumpValues._NoJumpAnimData.Length;
		if (num < 1)
		{
			return null;
		}
		int num2 = UnityEngine.Random.Range(0, num);
		return mCurrentStateData._JumpValues._NoJumpAnimData[num2];
	}

	public void OnEffectPush(Vector3 direction, float strength, float time)
	{
		if (!IsFlyingOrGliding())
		{
			mTriggerJump = true;
			mPushVelocity = Vector3.Reflect(direction, Vector3.up) * strength * 0.5f;
			mPushVelocity.y = 0f;
			mPushTimer = time;
		}
		else
		{
			mPushVelocity = direction * strength;
			mPushTimer = time;
		}
	}

	public void OnCannedAnimBegin(string name)
	{
	}

	public void OnCannedAnimEnd(string name)
	{
		if (mCurrentUsedProp != null && mCurrentUsedProp._AnimState == name)
		{
			StopUseProp();
		}
	}

	public void RequestMMODelete()
	{
		MMOAvatar component = base.gameObject.GetComponent<MMOAvatar>();
		if (component != null && MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.RemoveUser(component.mUserName);
		}
	}

	private void ResetTransform(Transform transform)
	{
		if (transform != null)
		{
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
		}
	}

	public void SoftReset()
	{
		ResetTransform(_MainRoot);
		ResetTransform(_FlyingBone);
		string[] array = new string[2]
		{
			AvatarSettings.pInstance._MainRootName,
			AvatarSettings.pInstance._SpriteName
		};
		for (int i = 0; i < array.Length; i++)
		{
			Transform transform = UtUtilities.FindChildTransform(base.gameObject, array[i]);
			ResetTransform(transform);
		}
		SetMounted(mounted: false);
		UpdateDisplayName(pPlayerMounted, AvAvatar.IsCurrentPlayer(base.gameObject) ? UserProfile.pProfileData.HasGroup() : (GetInstanceInfo().mInstance._Group != null));
		RemoveCarriedObject();
		SetFlyingState(FlyingState.Grounded);
		ClearSpline();
		pState = AvAvatarState.NONE;
		pSubState = AvAvatarSubState.NORMAL;
	}

	private void ProcessOnLand()
	{
		if (pPlayerMounted)
		{
			BroadcastMessage("OnFlyLanding", base.gameObject);
		}
		else
		{
			OnGlideLanding();
		}
	}

	public void MoveTo(Vector3 targetPos)
	{
		AvNavMeshAgent component = GetComponent<AvNavMeshAgent>();
		if (component != null)
		{
			component.pMessageObject = mEndSplineMessageObject;
			component.pTargetPos = targetPos;
			component.enabled = true;
		}
		else
		{
			if (FindPath(base.transform.position, targetPos))
			{
				mSpline.GetPosQuatByDist(mSpline.mLinearLength, out var pos, out var _);
				pos.y += 100f;
				pos.y = UtUtilities.GetGroundHeight(pos, 200f) + 0.01f;
				if (mProperties._NavTarget != null)
				{
					GameObject obj = UnityEngine.Object.Instantiate(mProperties._NavTarget);
					obj.name = mProperties._NavTarget.name;
					obj.transform.position = pos;
				}
				Vector3 vector = pos - base.transform.position;
				vector.y = 0f;
				if (mProperties._NavArrow != null && vector.magnitude > 10f)
				{
					GameObject obj2 = UnityEngine.Object.Instantiate(mProperties._NavArrow);
					obj2.name = mProperties._NavArrow.name;
					obj2.transform.position += pos;
				}
			}
			mActivateObject = null;
		}
		AvAvatar.pState = AvAvatarState.IDLE;
	}

	public static void AddModifier(string inModifier, string inFieldName)
	{
		if (mModifierFieldMap.ContainsKey(inModifier))
		{
			mModifierFieldMap[inModifier] = inFieldName;
		}
		else
		{
			mModifierFieldMap.Add(inModifier, inFieldName);
		}
	}

	public static bool ContainsModifier(string inModifierName)
	{
		return mModifierFieldMap.ContainsKey(inModifierName);
	}

	public static void GetModifierNames(ref List<string> outStatNames)
	{
		foreach (KeyValuePair<string, string> item in mModifierFieldMap)
		{
			outStatNames.Add(item.Key);
		}
	}

	public static bool GetStatValue(string inModifierName, out float outStatValue)
	{
		outStatValue = 0f;
		if (AvAvatar.pObject == null || !mModifierFieldMap.ContainsKey(inModifierName))
		{
			return false;
		}
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (component == null)
		{
			return false;
		}
		FieldInfo fieldInfo = component.mCurrentStateData.GetType().GetFieldInfo(mModifierFieldMap[inModifierName], BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
		outStatValue = (float)fieldInfo.GetValue(component.mCurrentStateData);
		return true;
	}

	public void SetupModifiers()
	{
		foreach (ModifierAttributes modifierAttribute in _ModifierAttributes)
		{
			if (!string.IsNullOrEmpty(modifierAttribute._ItemAttribute) && !string.IsNullOrEmpty(modifierAttribute._FieldToModify))
			{
				AddModifier(modifierAttribute._ItemAttribute, modifierAttribute._FieldToModify);
			}
		}
	}

	public void OnUpdateAvatar()
	{
		if (AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			RefreshAvatarModifiers();
		}
	}

	public void SetImmune(bool isImmune, float inImmuneTime = -1f)
	{
		pImmune = isImmune;
		if (isImmune)
		{
			mImmuneTimer = ((inImmuneTime == -1f) ? _FireballImmuneTime : inImmuneTime);
		}
		else
		{
			mImmuneTimer = -1f;
		}
	}

	public void RefreshAvatarModifiers()
	{
		if (!AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			return;
		}
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (!(component == null))
		{
			component.InitializeModifiedStateData();
			for (int i = 0; i < component.mModifiedStateData.Length; i++)
			{
				ApplyAvatarModifiers(ref component.mModifiedStateData[i]);
			}
		}
	}

	public void ApplyAvatarModifiers(ref AvAvatarStateData inStateData)
	{
		if (AvAvatar.pObject == null || AvAvatar.pObject.GetComponent<AvAvatarController>() == null)
		{
			return;
		}
		Dictionary<string, float> modifiersFound = new Dictionary<string, float>();
		AvatarDataPart[] part = AvatarData.pInstanceInfo.mInstance.Part;
		foreach (AvatarDataPart avatarDataPart in part)
		{
			if (avatarDataPart.Attributes != null && !avatarDataPart.IsDefault())
			{
				AvatarPartAttribute[] attributes = avatarDataPart.Attributes;
				foreach (AvatarPartAttribute avatarPartAttribute in attributes)
				{
					GetModifiers(ref modifiersFound, avatarPartAttribute.Key, avatarPartAttribute.Value);
				}
			}
		}
		foreach (KeyValuePair<string, float> item in modifiersFound)
		{
			ApplyStateDataModifier(item.Key, item.Value, ref inStateData);
		}
	}

	public void ApplyAvatarModifiers(ref AvAvatarFlyingData inFlightData)
	{
		if (inFlightData == null)
		{
			return;
		}
		Dictionary<string, float> modifiersFound = new Dictionary<string, float>();
		AvatarDataPart[] part = AvatarData.pInstanceInfo.mInstance.Part;
		foreach (AvatarDataPart avatarDataPart in part)
		{
			if (avatarDataPart.Attributes != null && !avatarDataPart.IsDefault())
			{
				AvatarPartAttribute[] attributes = avatarDataPart.Attributes;
				foreach (AvatarPartAttribute avatarPartAttribute in attributes)
				{
					GetModifiers(ref modifiersFound, avatarPartAttribute.Key, avatarPartAttribute.Value);
				}
			}
		}
		foreach (KeyValuePair<string, float> item in modifiersFound)
		{
			ApplyAvAvatarFlyingDataModifier(item.Key, item.Value, ref inFlightData);
		}
	}

	public void ApplyPetModifiers(ref AvAvatarFlyingData inFlightData)
	{
		int num = -1;
		UserItemData userItemData = null;
		if (inFlightData == null)
		{
			return;
		}
		if (SanctuaryManager.pCurPetData != null)
		{
			num = SanctuaryManager.pCurPetData.GetAccessoryItemID(RaisedPetAccType.Saddle);
		}
		if (num > 0)
		{
			userItemData = CommonInventoryData.pInstance.FindItem(num);
		}
		if (userItemData == null || userItemData.Item == null || userItemData.Item.Attribute == null)
		{
			return;
		}
		Dictionary<string, float> modifiersFound = new Dictionary<string, float>();
		ItemAttribute[] attribute = userItemData.Item.Attribute;
		foreach (ItemAttribute itemAttribute in attribute)
		{
			GetModifiers(ref modifiersFound, itemAttribute.Key, itemAttribute.Value);
		}
		foreach (KeyValuePair<string, float> item in modifiersFound)
		{
			ApplyAvAvatarFlyingDataModifier(item.Key, item.Value, ref inFlightData);
		}
	}

	public void GetModifiers(ref Dictionary<string, float> modifiersFound, string attributeKey, string attributeValue)
	{
		if (!mModifierFieldMap.ContainsKey(attributeKey))
		{
			return;
		}
		float result = 0f;
		if (float.TryParse(attributeValue, out result))
		{
			string key = mModifierFieldMap[attributeKey];
			if (modifiersFound.ContainsKey(key))
			{
				modifiersFound[key] += result;
			}
			else
			{
				modifiersFound.Add(key, result);
			}
		}
		else
		{
			UtDebug.LogError("Couldn't parse modifier value: " + attributeKey);
		}
	}

	private static void ApplyStateDataModifier(string inModifierName, float inValue, ref AvAvatarStateData inObject)
	{
		FieldInfo fieldInfo = inObject.GetType().GetFieldInfo(inModifierName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
		if (fieldInfo != null)
		{
			float num = (float)fieldInfo.GetValue(inObject);
			fieldInfo.SetValue(inObject, num + num * inValue);
		}
	}

	private static void ApplyAvAvatarFlyingDataModifier(string inModifierName, float inValue, ref AvAvatarFlyingData inObject)
	{
		FieldInfo fieldInfo = inObject.GetType().GetFieldInfo(inModifierName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
		if (fieldInfo != null)
		{
			object value = fieldInfo.GetValue(inObject);
			if (value is float num)
			{
				fieldInfo.SetValue(inObject, num + num * inValue);
			}
			else if (value is MinMax)
			{
				MinMax minMax = (MinMax)value;
				minMax.Min += minMax.Min * inValue;
				minMax.Max += minMax.Max * inValue;
			}
		}
	}

	public void SetUDTStarsVisible(bool inVisible, int inUDTPoints)
	{
		Transform uDTStarsObj = AvatarData.GetUDTStarsObj(base.gameObject);
		if (uDTStarsObj == null)
		{
			return;
		}
		int num = 0;
		Transform transform = uDTStarsObj.Find("UDTStarsIconBkg" + $"{num:d2}");
		Transform transform2 = uDTStarsObj.Find("UDTStarsIconFrameBkg" + $"{num:d2}");
		while (transform != null)
		{
			ObNGUI_Proxy component = transform.GetComponent<ObNGUI_Proxy>();
			component.SetVisible(inVisible);
			if (component._Widget != null && component._Widget.gameObject.name.Contains("UID"))
			{
				string text = component._Widget.gameObject.name;
				text = text.Replace("UID", GetUserID());
				component._Widget.gameObject.name = text;
			}
			if (transform2 != null)
			{
				ObNGUI_Proxy component2 = transform2.GetComponent<ObNGUI_Proxy>();
				component2.SetVisible(inVisible);
				if (component2._Widget != null && component2._Widget.gameObject.name.Contains("UID"))
				{
					string text2 = component2._Widget.gameObject.name;
					text2 = text2.Replace("UID", GetUserID());
					component2._Widget.gameObject.name = text2;
				}
			}
			num++;
			transform = uDTStarsObj.Find("UDTStarsIconBkg" + $"{num:d2}");
			transform2 = uDTStarsObj.Find("UDTStarsIconFrameBkg" + $"{num:d2}");
		}
		if (UserRankData.pIsReady)
		{
			UDTUtilities.UpdateUDTStars(uDTStarsObj, "UDTStarsIconBkg", inUDTPoints, "UDTStarsIconFrameBkg", inUseNGUIProxy: true);
		}
	}

	public void InitiateGliding()
	{
		if ((bool)SanctuaryManager.pCurPetInstance && pPlayerMounted)
		{
			SanctuaryManager.pCurPetInstance.OnFlyDismount(AvAvatar.pObject);
			mWasPlayerMounted = true;
		}
		mIsInitialGlideThrustStarted = true;
		mIsPlayerGliding = true;
		mWasPlayerGliding = false;
		mFlyingData = FlightData.GetFlightData(base.gameObject, FlightDataType.GLIDING);
		mRefreshFlightData = true;
		AvAvatar.pState = AvAvatarState.MOVING;
		pSubState = AvAvatarSubState.GLIDING;
		ShowSoarButton(inShow: false);
		if (_MainRoot != null)
		{
			mGlidingParentObject = new GameObject("GlideRoot");
			mGlidingParentObject.transform.parent = base.transform;
			mGlidingParentObject.transform.localPosition = new Vector3(_MainRoot.localPosition.x, _MainRoot.localPosition.y + 0.7f, _MainRoot.localPosition.z);
			mGlidingParentObject.transform.localRotation = Quaternion.identity;
			_MainRoot.parent = mGlidingParentObject.transform;
		}
		UpdateGliding();
		UiAvatarControls.EnableTiltControls(enable: true);
		KAInput.pInstance.EnableInputType("WingFlap", InputType.UI_BUTTONS, inEnable: false);
		KAInput.pInstance.EnableInputType("DragonBrake", InputType.UI_BUTTONS, inEnable: false);
		mWasPlayerMounted = false;
		if (mToolBar != null)
		{
			mToolBar.GlidingEvent(gliding: true);
		}
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pCurPetInstance.pIsFlying = true;
			SanctuaryManager.pCurPetInstance.SetClickableActive(active: false);
		}
		if (_GlidingFxData != null)
		{
			FxData[] glidingFxData = _GlidingFxData;
			foreach (FxData fxData in glidingFxData)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(fxData._Fx);
				gameObject.transform.parent = fxData._Bone;
				gameObject.transform.localPosition = fxData._Offset;
				if (_ApplyGlideFxOnDive)
				{
					gameObject.SetActive(value: false);
				}
				if (mGlidingFxList == null)
				{
					mGlidingFxList = new List<GameObject>();
				}
				mGlidingFxList.Add(gameObject);
			}
		}
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.SetJoinAllowed(MMOJoinStatus.NOT_ALLOWED);
		}
	}

	public void OnGlideLanding()
	{
		mIsPlayerGliding = false;
		mWasPlayerGliding = true;
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.pSubState = AvAvatarSubState.NORMAL;
		UiAvatarControls.EnableTiltControls(enable: false);
		ShowSoarButton(inShow: false);
		OnGlideEnd();
		SetFlyingState(FlyingState.Grounded);
	}

	public void OnGlideEnd()
	{
		if (_MainRoot != null)
		{
			_MainRoot.parent = base.transform;
			_MainRoot.localPosition = Vector3.zero;
			_MainRoot.localEulerAngles = Vector3.zero;
			if (mGlidingParentObject != null)
			{
				UnityEngine.Object.Destroy(mGlidingParentObject);
			}
		}
		if (mToolBar != null)
		{
			mToolBar.GlidingEvent(gliding: false);
		}
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pCurPetInstance.pIsFlying = false;
			SanctuaryManager.pCurPetInstance.SetClickableActive(active: true);
		}
		if (mGlidingFxList != null)
		{
			foreach (GameObject mGlidingFx in mGlidingFxList)
			{
				UnityEngine.Object.Destroy(mGlidingFx);
			}
			mGlidingFxList.Clear();
		}
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.SetJoinAllowed(MMOJoinStatus.ALLOWED);
		}
		AvatarData.InstanceInfo instanceInfo = GetInstanceInfo();
		instanceInfo.CheckHelmetHair();
		instanceInfo.CheckDisableBlink();
	}

	public void SetToolbarButtonsVisible(bool visible)
	{
		if (mToolBar != null && !FUEManager.pIsFUERunning)
		{
			mToolBar.SetButtonsVisiblity(visible);
		}
	}

	public void ShowSoarButton(bool inShow)
	{
		if (AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			KAInput.pInstance.EnableInputType("Soar", InputType.UI_BUTTONS, inShow);
			if (inShow && UiAvatarControls.pInstance != null && UiAvatarControls.pInstance.pSoarBtn != null)
			{
				UiAvatarControls.pInstance.pSoarBtn.PlayAnim((!mIsSoarTutorialDone) ? "Flash" : "Normal");
			}
			if (inShow && KAInput.pInstance.GetInputTypeState("Remove", InputType.UI_BUTTONS))
			{
				mGlideEndTime = 0f;
			}
		}
	}

	public void ProcessRemoveButton()
	{
		bool inputTypeState = KAInput.pInstance.GetInputTypeState("Remove", InputType.UI_BUTTONS);
		if (mWasPlayerGliding)
		{
			mWasPlayerGliding = false;
			mGlideEndTime = _RemoveButtonTimer;
		}
		if (IsFlyingOrGliding() && mGlideEndTime > 0f)
		{
			mGlideEndTime = 0f;
		}
		if (mGlideEndTime > 0f && mCanGlide)
		{
			mGlideEndTime -= Time.deltaTime;
			if (!inputTypeState)
			{
				EnableRemoveButton(inEnable: true);
			}
		}
		else if (inputTypeState)
		{
			EnableRemoveButton(inEnable: false);
		}
		if (KAInput.GetButtonUp("Remove"))
		{
			mGlideEndTime = 0f;
			EnableRemoveButton(inEnable: false);
			if (!mIsRemoveTutorialDone)
			{
				ProductData.AddTutorial(_RemoveTutorialName);
				mIsRemoveTutorialDone = true;
			}
			pAvatarCustomization.RestoreAvatar();
			pAvatarCustomization.SaveCustomAvatar();
		}
	}

	public void EnableRemoveButton(bool inEnable)
	{
		KAInput.pInstance.EnableInputType("Remove", InputType.UI_BUTTONS, inEnable);
		if (!mIsRemoveTutorialDone && inEnable && UiAvatarControls.pInstance.pRemoveBtn != null)
		{
			UiAvatarControls.pInstance.pRemoveBtn.PlayAnim("Flash");
		}
	}

	public void EquipFlightSuit(ItemData item, AvatarCustomization.OnAllItemsDownloaded onAllItemsDownloaded)
	{
		if (item != null)
		{
			pAvatarCustomization.RestoreAvatar();
			pAvatarCustomization.UpdateGroupPart(AvatarData.pInstanceInfo, item, onAllItemsDownloaded);
		}
	}

	public void SetFlightSuitData()
	{
		mIsSoarTutorialDone = ProductData.TutorialComplete(_SoarTutorialName);
		mIsRemoveTutorialDone = ProductData.TutorialComplete(_RemoveTutorialName);
		if (AvAvatar.pToolbar != null)
		{
			mToolBar = AvAvatar.pToolbar.GetComponent<UiToolbar>();
		}
	}

	public void GetFlightSuitItems()
	{
		if (mFlightSuitStoreID < 0)
		{
			mFlightSuitStoreID = Convert.ToInt32(GameConfig.GetKeyData("FlightSuitStoreID"));
		}
		ItemStoreDataLoader.Load(mFlightSuitStoreID, OnStoreLoaded);
	}

	public void OnStoreLoaded(StoreData sd)
	{
		if (sd == null || sd._ID != mFlightSuitStoreID)
		{
			return;
		}
		StoreCategoryData storeCategoryData = sd.FindCategoryData(_FlightSuitCategoryIDs);
		if (storeCategoryData == null || storeCategoryData._Items == null)
		{
			return;
		}
		foreach (ItemData item in storeCategoryData._Items)
		{
			if (item.IsSubPart() || item.Relationship == null)
			{
				continue;
			}
			ItemDataRelationship[] relationship = item.Relationship;
			for (int i = 0; i < relationship.Length; i++)
			{
				if (relationship[i].Type != "Prereq" && !string.IsNullOrEmpty(mGender) && item.Attribute[0].Value == mGender)
				{
					mMemberFlightSuit = item;
					return;
				}
			}
		}
	}

	public void SetLastEquippedFlightSuit(ItemData inItem = null)
	{
		if (inItem != null)
		{
			SetLastUsedFlightSuitInAttribute(inItem);
		}
	}

	public UserItemData GetLastUsedFlightSuit()
	{
		AvatarDataPart avatarDataPart = AvatarData.pInstanceInfo.FindPart(AvatarData.pPartSettings.AVATAR_PART_WING);
		AvatarPartAttribute avatarPartAttribute = null;
		if (avatarDataPart != null)
		{
			if (CommonInventoryData.pInstance != null && avatarDataPart.UserInventoryId.HasValue && avatarDataPart.UserInventoryId > 0)
			{
				UserItemData userItemData2 = CommonInventoryData.pInstance.FindItemByUserInventoryID(avatarDataPart.UserInventoryId.GetValueOrDefault(-1));
				if (userItemData2 != null)
				{
					if (!userItemData2.Item.HasAttribute("FlightSuit"))
					{
						return null;
					}
					return userItemData2;
				}
			}
			if (avatarDataPart.Attributes != null)
			{
				AvatarPartAttribute[] attributes = avatarDataPart.Attributes;
				foreach (AvatarPartAttribute avatarPartAttribute2 in attributes)
				{
					if (avatarPartAttribute2.Key.Equals(_LastEquippedFlightSuitKey))
					{
						avatarPartAttribute = avatarPartAttribute2;
						break;
					}
				}
			}
		}
		UserItemData[] array = null;
		if (CommonInventoryData.pInstance != null)
		{
			array = CommonInventoryData.pInstance.GetItems(_FlightSuitCategoryIDs);
		}
		if (avatarPartAttribute != null && !string.IsNullOrEmpty(avatarPartAttribute.Value) && array != null && array.Length != 0)
		{
			UserItemData[] array2 = array;
			foreach (UserItemData userItemData3 in array2)
			{
				if (userItemData3.Item != null && userItemData3.Item.ItemID.ToString().Equals(avatarPartAttribute.Value))
				{
					if (!userItemData3.Item.HasAttribute("FlightSuit"))
					{
						return null;
					}
					return userItemData3;
				}
			}
		}
		if (IsMember())
		{
			return new UserItemData
			{
				Item = mMemberFlightSuit
			};
		}
		if (array != null && array.Length != 0)
		{
			return array.FirstOrDefault((UserItemData userItemData) => userItemData.Item.HasAttribute("FlightSuit"));
		}
		return null;
	}

	public ItemData GetMemberFlightSuit()
	{
		if (!IsMember())
		{
			return null;
		}
		return mMemberFlightSuit;
	}

	private void SetLastUsedFlightSuitInAttribute(ItemData inItem)
	{
		AvatarPartAttribute attribute = new AvatarPartAttribute
		{
			Key = _LastEquippedFlightSuitKey,
			Value = inItem.ItemID.ToString()
		};
		AvatarData.SetAttribute(AvatarData.pInstanceInfo, AvatarData.pPartSettings.AVATAR_PART_WING, attribute);
	}

	public void SetMounted(bool mounted)
	{
		mRefreshFlightData = true;
		pPlayerMounted = mounted;
		if ((bool)_Blink)
		{
			_Blink.ResetBlink();
		}
		if (mProjCollider != null)
		{
			mProjCollider.isTrigger = true;
			mProjCollider.center = new Vector3(0f, mounted ? 1.5f : 0.5f, 0f);
			mProjCollider.radius = (mounted ? 1.5f : 1f);
		}
		if (mCSMCollider != null)
		{
			mCSMCollider.transform.localPosition = (mounted ? _CSMColliderMountedPosition : _CSMColliderUnmountedPosition);
			mCSMCollider.height = (mounted ? _CSMColliderMountedHeight : _CSMColliderUnmountedHeight);
		}
		if (mounted && mPlayerCarrying)
		{
			RemoveCarriedObject();
		}
	}

	private bool IsAirRefillingAllowed()
	{
		if (mSubState != AvAvatarSubState.UWSWIMMING && mState != AvAvatarState.PAUSED && mState != 0)
		{
			return !RsResourceManager.pLevelLoadingScreen;
		}
		return false;
	}

	private void OnSceneLoaded(Scene newScene, LoadSceneMode loadSceneMode)
	{
		if (mPlayerCarrying)
		{
			RemoveCarriedObject(forceDestroy: true);
		}
	}

	public void UpdateGliding()
	{
		if (AvAvatar.IsCurrentPlayer(base.gameObject) && AvAvatar.pLevelState != AvAvatarLevelState.FLIGHTSCHOOL)
		{
			if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pAge >= SanctuaryManager.pCurPetInstance.pTypeInfo._MinAgeToFly && pPlayerMounted)
			{
				pFlyingGlidingMode = false;
			}
			else
			{
				pFlyingGlidingMode = true;
			}
		}
		mFlyingFlapCooldownTimer = 0f;
		mFlyingBoostTimer = 0f;
		SetFlyingState(FlyingState.TakeOffGliding);
	}

	private void UpdateFlying(float HorizInput, float VertInput)
	{
		UpdateEffectModifiers();
		switch (mFlyingState)
		{
		case FlyingState.TakeOff:
			mFlyTakeOffTimer -= Time.deltaTime;
			if (mFlyTakeOffTimer <= 0f)
			{
				if (mVelocity.magnitude < 1f)
				{
					SetFlyingState(FlyingState.Hover);
				}
				else
				{
					SetFlyingState(FlyingState.Normal);
				}
			}
			else
			{
				mVelocity.y *= 1f - Time.deltaTime * 1f;
			}
			break;
		case FlyingState.TakeOffGliding:
			mGlideTakeOffTimer -= Time.deltaTime;
			if (mGlideTakeOffTimer < 0f)
			{
				SetFlyingState(FlyingState.Normal);
			}
			break;
		case FlyingState.SkimOverWater:
			UpdateFlyingControl(HorizInput, (VertInput > 0f) ? VertInput : 0f);
			if (mVelocity.y < 0f)
			{
				mVelocity.y = 0f;
			}
			mFlightSpeed -= _WaterDrag * Time.deltaTime;
			if (mFlightSpeed < 0f)
			{
				mFlightSpeed = 0f;
			}
			UpdateCurrentWaterDepth();
			break;
		case FlyingState.Normal:
		case FlyingState.Hover:
			UpdateFlyingControl(HorizInput, VertInput);
			break;
		}
		if (!(_FlyingBone != null))
		{
			return;
		}
		if (mIsPlayerGliding)
		{
			_MainRoot.localEulerAngles = new Vector3(0f, mRoll * 360f, 0f);
			_MainRoot.parent.localEulerAngles = new Vector3(mPitch * 360f + (float)_GlidingAngle, 0f, 0f);
			if (mGlidingFxList != null && _ApplyGlideFxOnDive)
			{
				foreach (GameObject mGlidingFx in mGlidingFxList)
				{
					mGlidingFx.SetActive(mPitch > 0.01f);
				}
			}
		}
		_FlyingBone.localEulerAngles = new Vector3(mPitch * 360f, 0f, mRoll * 360f);
		mTurnFactor = Mathf.Lerp(1f, mFlyingData._YawTurnFactor, mVelocity.magnitude / 120f);
		base.transform.Rotate(new Vector3(0f, mRoll * (0f - mFlyingData._YawTurnRate) * 750f * mTurnFactor * Time.deltaTime, 0f));
	}

	public void OnEffectSlow(float strength, float time)
	{
		mSlowPenaltyStrength = strength;
		mSlowPenaltyStep = strength / time;
	}

	private void UpdateEffectModifiers()
	{
		mSlowPenaltyStrength -= mSlowPenaltyStep * Time.deltaTime;
		if (mSlowPenaltyStrength < 0f)
		{
			mSlowPenaltyStrength = 0f;
		}
	}

	private void UpdateFlyingControl(float HorizInput, float VertInput)
	{
		bool num = AvAvatar.pObject != null && AvAvatar.pLevelState == AvAvatarLevelState.RACING;
		mIsBraking = (AvAvatar.pInputEnabled && KAInput.GetButton("DragonBrake")) || mForceBraking;
		mVelocity.Set(0f, 0f, 0f);
		float num2 = mFlyingData._FlyingMaxUpPitch;
		float num3 = mFlyingData._FlyingMaxDownPitch;
		float maxRoll = mFlyingData._MaxRoll;
		if (UtPlatform.IsMobile())
		{
			if (mAimControlMode)
			{
				mRoll = Mathf.Lerp(mRoll, (0f - HorizInput) * _JoySensitivity * maxRoll / 360f, Time.deltaTime * 5f);
				float num4 = Mathf.Max(num2, num3);
				mPitch = Mathf.Lerp(mPitch, (0f - VertInput) * _JoySensitivity * num4 / 360f, Time.deltaTime * 5f);
			}
			else
			{
				mRoll -= HorizInput * mFlyingData._RollTurnRate * Time.deltaTime * 0.2f;
				mRoll *= 1f - mFlyingData._RollDampRate * Time.deltaTime * 1.2f;
				mPitch -= VertInput * mFlyingData._PitchTurnRate * Mathf.Max(mTurnFactor, 0.5f) * Time.deltaTime * 0.2f;
				mPitch *= 1f - mFlyingData._PitchDampRate * Time.deltaTime * 1.2f;
			}
		}
		else
		{
			mRoll -= HorizInput * mFlyingData._RollTurnRate * Time.deltaTime * 0.2f;
			mRoll *= 1f - mFlyingData._RollDampRate * Time.deltaTime * 1.2f;
			mPitch -= VertInput * mFlyingData._PitchTurnRate * Mathf.Max(mTurnFactor, 0.5f) * Time.deltaTime * 0.2f;
			mPitch *= 1f - mFlyingData._PitchDampRate * Time.deltaTime * 1.2f;
		}
		if (pFlyingGlidingMode)
		{
			num2 = mFlyingData._GlidingMaxUpPitch;
			num3 = mFlyingData._GlidingMaxDownPitch;
		}
		if (mPitch * 360f < 0f - num2)
		{
			mPitch = (0f - num2) / 360f;
		}
		if (mPitch * 360f > num3)
		{
			mPitch = num3 / 360f;
		}
		if (mRoll * 360f < 0f - maxRoll)
		{
			mRoll = (0f - maxRoll) / 360f;
		}
		if (mRoll * 360f > maxRoll)
		{
			mRoll = maxRoll / 360f;
		}
		if (mFlightSpeed > mFlyingData._Speed.Min)
		{
			float num5 = (pFlyingGlidingMode ? (mFlyingData._SpeedDampRate / 4f) : mFlyingData._SpeedDampRate);
			mFlightSpeed *= 1f - num5 * Time.deltaTime * 0.02f;
		}
		bool flag = mPitch < 0f;
		if (!mIsBraking && mFlyingState != FlyingState.Hover)
		{
			if (flag)
			{
				if (mFlightSpeed > mFlyingData._Speed.Min && mFlyingData._ClimbAccelRate > 0f)
				{
					mFlightSpeed += mFlyingData._ClimbAccelRate * mPitch * Time.deltaTime * 100f;
				}
			}
			else if (mFlyingData._DiveAccelRate > 0f)
			{
				mFlightSpeed += mFlyingData._DiveAccelRate * mPitch * Time.deltaTime * 100f;
			}
		}
		if (SanctuaryManager.pCurPetInstance != null && !mForceBraking)
		{
			if (AvAvatar.pInputEnabled && KAInput.GetButton("WingFlap") && !pFlyingGlidingMode && mFlyingData._ManualFlapTimer > mFlyingFlapCooldownTimer)
			{
				mFlyingFlapCooldownTimer = mFlyingData._ManualFlapTimer;
			}
			if (mFlyingFlapCooldownTimer > 0f)
			{
				mFlyingFlapCooldownTimer -= Time.deltaTime;
				mFlyingBoostTimer -= Time.deltaTime;
				float num6;
				float num7;
				float num8;
				if (mFlyingBoostTimer > 0f)
				{
					num6 = mFlyingData._Speed.Max * (1f + _BonusMaxSpeedWithBoost);
					num7 = _AccelValueWithBoost;
					num8 = 20f;
				}
				else
				{
					num6 = mFlyingData._Speed.Max;
					num7 = mFlyingData._ManualFlapAccel;
					num8 = 10f;
				}
				if (mFlightSpeed < num6)
				{
					mFlightSpeed += num7 * Time.deltaTime * num8;
				}
				mMaxFlightSpeed = num6;
			}
		}
		if (!mIsPlayerGliding)
		{
			if (mIsBraking)
			{
				mFlyingFlapCooldownTimer = 0f;
				mFlightSpeed *= 1f - Time.deltaTime * mFlyingData._BrakeDecel * 3f;
			}
			if (mFlightSpeed > 1f && mFlyingState == FlyingState.Hover)
			{
				SetFlyingState(FlyingState.Normal);
			}
			if (mFlightSpeed < 1f && mFlyingState == FlyingState.Normal)
			{
				mFlightSpeed = 0f;
				SetFlyingState(FlyingState.Hover);
			}
		}
		float num9 = (num ? 1f : ((SanctuaryManager.pCurPetInstance != null) ? SanctuaryManager.pCurPetInstance.GetFlightSpeedModifer() : 1f));
		if (pFlyingGlidingMode)
		{
			if (mFlyingState == FlyingState.Hover && VertInput > 0f)
			{
				mFlightSpeed += 500f * Time.deltaTime * VertInput;
			}
			num9 = Mathf.Max(1f, num9);
		}
		Vector3 normalized = (_FlyingBone.forward * 0.5f).normalized;
		mVelocity = normalized * (mFlightSpeed * num9 * (1f - mSlowPenaltyStrength) * (1f + pFlyingPositionBoost * 0.4f));
		if (pFlyingGlidingMode)
		{
			float num10 = ((mPitch > 0f) ? 1f : (1f + mPitch * -2f));
			mVelocity += Vector3.up * Mathf.Max(mFlightSpeed, 10f) * (0f - mFlyingData._GlideDownMultiplier) * num10;
			float num11 = 1f;
			if (mPitch < -0.001f)
			{
				num11 = mFlyingData._GravityClimbMultiplier;
			}
			else if (mPitch > 0.001f)
			{
				num11 = mFlyingData._GravityDiveMultiplier;
			}
			float num12 = mFlyingData._GravityModifier + Mathf.Sin(mPitch * MathF.PI) * mFlyingData._GravityModifier * num11;
			mVelocity += Vector3.up * (0f - num12);
		}
		if (mIsPlayerGliding && mFlightSpeed > mFlyingData._Speed.Max)
		{
			mFlightSpeed = mFlyingData._Speed.Max;
		}
	}

	public void SetFlyingState(FlyingState inFlyingState)
	{
		if (!AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			return;
		}
		if (this.OnPetFlyingStateChanged != null)
		{
			this.OnPetFlyingStateChanged(inFlyingState);
		}
		switch (inFlyingState)
		{
		case FlyingState.TakeOff:
			mFlyTakeOffTimer = 0.5f;
			mVelocity = base.transform.forward * KAInput.GetValue("Vertical") * 15f + new Vector3(0f, 10f, 0f);
			mRoll = 0f;
			mPitch = 0f;
			mIsBraking = false;
			mForceBraking = false;
			mFlightSpeed = new Vector3(mVelocity.x, 0f, mVelocity.z).magnitude * 1.2f;
			SetFlyingAvatarCamParams(AvAvatar.mTransform);
			break;
		case FlyingState.TakeOffGliding:
			mGlideTakeOffTimer = _GlideTakeOffTimer;
			if (mWasPlayerMounted)
			{
				mVelocity += new Vector3(0f, _GlidingTakeOffSpeed, 0f);
			}
			else
			{
				mVelocity += new Vector3(0f, 2f, 0f);
			}
			mFlightSpeed = new Vector3(mVelocity.x, 0f, mVelocity.z).magnitude * 1.2f;
			mRoll = 0f;
			mPitch = 0f;
			mIsBraking = false;
			mForceBraking = false;
			if (AvAvatar.pLevelState == AvAvatarLevelState.RACING || (AvAvatar.pLevelState == AvAvatarLevelState.FLIGHTSCHOOL && !PlayingFlightSchoolFlightSuit))
			{
				SetFlyingAvatarCamParams(AvAvatar.mTransform, _FlyingBone);
			}
			else
			{
				SetFlyingAvatarCamParams(AvAvatar.mTransform);
			}
			break;
		}
		mFlyingState = inFlyingState;
		if (AvAvatar.IsCurrentPlayer(base.gameObject) && SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pFlyCollider != null)
		{
			SanctuaryManager.pCurPetInstance.pFlyCollider.gameObject.SetActive(mFlyingState != FlyingState.Grounded);
		}
	}

	public void SetFlyingAvatarCamParams(Transform inTarget, Transform pitchBone = null)
	{
		if (!AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			return;
		}
		CaAvatarCam caAvatarCam = ((AvAvatar.pAvatarCam != null) ? AvAvatar.pAvatarCam.GetComponent<CaAvatarCam>() : null);
		if (!(caAvatarCam == null))
		{
			if (inTarget == null)
			{
				caAvatarCam.SetLookAt(base.transform, pitchBone, _DefaultFlightPitch);
			}
			else
			{
				caAvatarCam.SetLookAt(inTarget, pitchBone, _DefaultFlightPitch);
			}
			if (pitchBone != null)
			{
				caAvatarCam.SetDistance(0f - _DefaultFlightDistance);
			}
		}
	}

	public void SetFlyingHover(bool inHover)
	{
		if (inHover)
		{
			SetFlyingState(FlyingState.Hover);
		}
		else
		{
			SetFlyingState(FlyingState.Normal);
		}
	}

	public void DisableFlightControls(bool isDisabled)
	{
		if (UiAvatarControls.pInstance != null)
		{
			UiAvatarControls.pInstance.DisableAllDragonControls(isDisabled);
		}
	}

	public bool IsFlyingHover()
	{
		if (mFlyingState == FlyingState.Hover)
		{
			return true;
		}
		return false;
	}

	public void StopFlying()
	{
		if (AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			mFlyingState = FlyingState.Normal;
			mForceBraking = true;
		}
	}

	public void SetPushSpeed(float speed)
	{
		if (pPlayerMounted)
		{
			mFlightSpeed = speed;
		}
	}

	public bool CheckCollision(Collider my, Collider other, bool onCollisionStay = false)
	{
		if (mFlyingState != FlyingState.Normal)
		{
			return false;
		}
		if (!DidCollide(my.transform.position, out var hitPoint, out var hitDirection))
		{
			return false;
		}
		Vector3 vector = other.ClosestPointOnBounds(hitPoint) - base.transform.position;
		float num = Vector3.Angle(new Vector3(vector.x, vector.y, vector.z), new Vector3(base.transform.forward.x, base.transform.forward.y, base.transform.forward.z));
		float num2 = Mathf.Sin(num * MathF.PI / 180f) * mFlyingData._SpeedModifierOnCollision;
		if (onCollisionStay)
		{
			if (num > 30f)
			{
				return false;
			}
			if (Vector3.Dot(mGroundCollisionNormal, Vector3.up) > 0.707f)
			{
				mFlightSpeed -= Mathf.Cos(GetVerticalFromInput());
			}
			else
			{
				mFlightSpeed = Mathf.Lerp(mFlightSpeed, 0f, mCurrentStateData._Acceleration * Time.deltaTime);
			}
			return true;
		}
		mFlightSpeed *= num2;
		mRoll = mFlyingData._BounceOnCollision * hitDirection * Time.deltaTime;
		if (mRoll * 360f < 0f - mFlyingData._MaxRoll)
		{
			mRoll = (0f - mFlyingData._MaxRoll) / 360f;
		}
		if (mRoll * 360f > mFlyingData._MaxRoll)
		{
			mRoll = mFlyingData._MaxRoll / 360f;
		}
		return true;
	}

	public bool DidCollide(Vector3 pos, out Vector3 hitPoint, out float hitDirection)
	{
		float num = 1000f;
		hitDirection = 255f;
		hitPoint = Vector3.zero;
		if (Physics.Raycast(pos, base.transform.forward, out var hitInfo, 10f))
		{
			hitDirection = 0f;
			float num2 = Vector3.Distance(hitInfo.point, pos);
			if (num2 < num)
			{
				num = num2;
				hitPoint = hitInfo.point;
			}
		}
		if (Physics.Raycast(pos, base.transform.right + base.transform.forward, out hitInfo, 10f))
		{
			hitDirection = 1f;
			float num3 = Vector3.Distance(hitInfo.point, pos);
			if (num3 < num)
			{
				num = num3;
				hitPoint = hitInfo.point;
			}
		}
		if (Physics.Raycast(pos, -base.transform.right + base.transform.forward, out hitInfo, 10f))
		{
			hitDirection = -1f;
			if (Vector3.Distance(hitInfo.point, pos) < num)
			{
				hitPoint = hitInfo.point;
			}
		}
		if (!(hitDirection < 255f))
		{
			return false;
		}
		return true;
	}

	public void OnGUI()
	{
		if (mDisplayFlyingData)
		{
			GUI.Label(new Rect(0f, 300f, 300f, 100f), "Current Flight Speed:: " + mFlightSpeed);
			GUI.Label(new Rect(0f, 320f, 300f, 100f), "Max Speed:: " + mFlyingData._Speed.Max);
			GUI.Label(new Rect(0f, 340f, 300f, 100f), "Min Speed:: " + mFlyingData._Speed.Min);
			GUI.Label(new Rect(0f, 360f, 300f, 100f), "Acceleration:: " + mFlyingData._Acceleration);
			GUI.Label(new Rect(0f, 380f, 300f, 100f), "Speed Damp Rate:: " + mFlyingData._SpeedDampRate);
			GUI.Label(new Rect(0f, 400f, 300f, 100f), "Actual Velocity:: " + mVelocity.ToString());
			GUI.Label(new Rect(0f, 420f, 300f, 100f), "Current Pitch:: " + mPitch);
			GUI.Label(new Rect(0f, 440f, 300f, 100f), "Gliding Max Up Pitch:: " + mFlyingData._GlidingMaxUpPitch);
			GUI.Label(new Rect(0f, 460f, 300f, 100f), "Gliding Max Down Pitch:: " + mFlyingData._GlidingMaxDownPitch);
			GUI.Label(new Rect(0f, 480f, 300f, 100f), "Flying Max Up Pitch:: " + mFlyingData._FlyingMaxUpPitch);
			GUI.Label(new Rect(0f, 500f, 300f, 100f), "Flying Max Down Pitch:: " + mFlyingData._FlyingMaxDownPitch);
		}
	}

	public void UpdateUWSwimmingControl(float HorizInput, float VertInput)
	{
		mIsUWSwimBraking = (AvAvatar.pInputEnabled && KAInput.GetButton("UWSwimBrake")) || mForceBrakeUWSwimming;
		mVelocity.Set(0f, 0f, 0f);
		float maxUpPitch = mUWSwimmingData._MaxUpPitch;
		float maxDownPitch = mUWSwimmingData._MaxDownPitch;
		float maxRoll = mUWSwimmingData._MaxRoll;
		if (!pIsTransiting && base.transform.position.y >= GetAllowedYPosForAvatar() && VertInput > 0f)
		{
			VertInput = 0f;
			mUWSwimmingPitch = 0f;
		}
		if (UtPlatform.IsMobile())
		{
			if (mAimControlMode)
			{
				mUWSwimmingRoll = Mathf.Lerp(mUWSwimmingRoll, (0f - HorizInput) * _JoySensitivity * maxRoll / 360f, Time.deltaTime * 5f);
				float num = Mathf.Max(maxUpPitch, maxDownPitch);
				mUWSwimmingPitch = Mathf.Lerp(mUWSwimmingPitch, (0f - VertInput) * _JoySensitivity * num / 360f, Time.deltaTime * 5f);
			}
			else
			{
				mUWSwimmingRoll -= HorizInput * mUWSwimmingData._RollTurnRate * Time.deltaTime * 0.2f;
				mUWSwimmingRoll *= 1f - mUWSwimmingData._RollDampRate * Time.deltaTime * 1.2f;
				mUWSwimmingPitch -= VertInput * mUWSwimmingData._PitchTurnRate * Mathf.Max(mUWSwimmingTurnFactor, 0.5f) * Time.deltaTime * 0.2f;
				mUWSwimmingPitch *= 1f - mUWSwimmingData._PitchDampRate * Time.deltaTime * 1.2f;
			}
		}
		else
		{
			mUWSwimmingRoll -= HorizInput * mUWSwimmingData._RollTurnRate * Time.deltaTime * 0.2f;
			mUWSwimmingRoll *= 1f - mUWSwimmingData._RollDampRate * Time.deltaTime * 1.2f;
			mUWSwimmingPitch -= VertInput * mUWSwimmingData._PitchTurnRate * Mathf.Max(mUWSwimmingTurnFactor, 0.5f) * Time.deltaTime * 0.2f;
			mUWSwimmingPitch *= 1f - mUWSwimmingData._PitchDampRate * Time.deltaTime * 1.2f;
		}
		if (mUWSwimmingPitch * 360f < 0f - maxUpPitch)
		{
			mUWSwimmingPitch = (0f - maxUpPitch) / 360f;
		}
		if (mUWSwimmingPitch * 360f > maxDownPitch)
		{
			mUWSwimmingPitch = maxDownPitch / 360f;
		}
		if (mUWSwimmingRoll * 360f < 0f - maxRoll)
		{
			mUWSwimmingRoll = (0f - maxRoll) / 360f;
		}
		if (mUWSwimmingRoll * 360f > maxRoll)
		{
			mUWSwimmingRoll = maxRoll / 360f;
		}
		if (mUWSwimmingSpeed > mUWSwimmingData._Speed.Min)
		{
			float speedDampRate = mUWSwimmingData._SpeedDampRate;
			mUWSwimmingSpeed *= 1f - speedDampRate * Time.deltaTime * 0.02f;
		}
		bool flag = mUWSwimmingPitch < 0f;
		if (!mIsUWSwimBraking)
		{
			if (flag)
			{
				if (mUWSwimmingSpeed > mUWSwimmingData._Speed.Min && mUWSwimmingData._ClimbAccelRate > 0f)
				{
					mUWSwimmingSpeed += mUWSwimmingData._ClimbAccelRate * mUWSwimmingPitch * Time.deltaTime * 100f;
				}
			}
			else if (mUWSwimmingData._DiveAccelRate > 0f)
			{
				mUWSwimmingSpeed += mUWSwimmingData._DiveAccelRate * mUWSwimmingPitch * Time.deltaTime * 100f;
			}
		}
		if (!mForceBrakeUWSwimming)
		{
			if (AvAvatar.pInputEnabled && KAInput.GetButton("UWSwimBoost") && mUWSwimmingData._ManualBoostTimer > mUWSwimmingBoostCooldownTimer)
			{
				mUWSwimmingBoostCooldownTimer = mUWSwimmingData._ManualBoostTimer;
			}
			if (mUWSwimmingBoostCooldownTimer > 0f)
			{
				mUWSwimmingBoostCooldownTimer -= Time.deltaTime;
				mUWSwimmingBoostTimer -= Time.deltaTime;
				float num2;
				float num3;
				float num4;
				if (mUWSwimmingBoostTimer > 0f)
				{
					num2 = mUWSwimmingData._Speed.Max * (1f + _MaxUWSwimSpeedWithBoost);
					num3 = _UWSwimAccelValueWithBoost;
					num4 = 20f;
				}
				else
				{
					num2 = mUWSwimmingData._Speed.Max;
					num3 = mUWSwimmingData._ManualBoostAccel;
					num4 = 10f;
				}
				if (mUWSwimmingSpeed < num2)
				{
					mUWSwimmingSpeed += num3 * Time.deltaTime * num4;
				}
				mMaxUWSwimmingSpeed = num2;
			}
		}
		if (mIsUWSwimBraking)
		{
			mUWSwimmingBoostCooldownTimer = 0f;
			mUWSwimmingSpeed *= 1f - Time.deltaTime * mUWSwimmingData._BrakeDecel * 3f;
		}
		if (_FlyingBone != null)
		{
			Vector3 normalized = (_FlyingBone.forward * 0.5f).normalized;
			mVelocity = normalized * (mUWSwimmingSpeed * (1f - mSlowPenaltyStrength) * (1f + pFlyingPositionBoost * 0.4f));
			if (mUWSwimmingSpeed > mUWSwimmingData._Speed.Max)
			{
				mUWSwimmingSpeed = mUWSwimmingData._Speed.Max;
			}
			_FlyingBone.localEulerAngles = new Vector3(mUWSwimmingPitch * 360f, 0f, mUWSwimmingRoll * 360f);
			if (mVelocity.magnitude > 0.01f)
			{
				_MainRoot.localEulerAngles = new Vector3(mUWSwimmingPitch * 360f, 0f, mUWSwimmingRoll * 360f);
			}
			mUWSwimmingTurnFactor = Mathf.Lerp(1f, mUWSwimmingData._YawTurnFactor, mVelocity.magnitude / 120f);
			base.transform.Rotate(new Vector3(0f, mUWSwimmingRoll * (0f - mUWSwimmingData._YawTurnRate) * 750f * mUWSwimmingTurnFactor * Time.deltaTime, 0f));
		}
		pUWSwimZone._AirMeter.UpdateMeter(_Stats._AirUseRate * Time.deltaTime);
		if (_Stats._CurrentAir == 0f)
		{
			UpdateHealth(_Stats._NoAirHealthDamageRate * Time.deltaTime);
		}
		if (mIsUWSwimBraking)
		{
			ApplySwimAnim(SwimAnimState.UWBRAKE);
		}
		else if (mVelocity.magnitude > 0.2f)
		{
			ApplySwimAnim(SwimAnimState.UWSWIM);
		}
		else
		{
			ApplySwimAnim(SwimAnimState.UWIDLE);
		}
		if (pUWSwimZone.pLastUsedBreathZone != null && pUWSwimZone.pLastUsedBreathZone.pSubState == UWBreathZoneCSM.BreathZoneSubState.BREATHING)
		{
			mToolBar.SetHP();
		}
	}

	public void SetUWSwimmingAvatarCamParams(Transform inTarget, Transform pitchBone = null)
	{
		if (!AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			return;
		}
		CaAvatarCam caAvatarCam = ((AvAvatar.pAvatarCam != null) ? AvAvatar.pAvatarCam.GetComponent<CaAvatarCam>() : null);
		if (!(caAvatarCam == null))
		{
			if (inTarget == null)
			{
				caAvatarCam.SetLookAt(base.transform, pitchBone, _DefaultUWSwimmingPitch);
			}
			else
			{
				caAvatarCam.SetLookAt(inTarget, pitchBone, _DefaultUWSwimmingPitch);
			}
			if (pitchBone != null)
			{
				caAvatarCam.SetDistance(0f - _DefaultUWSwimDistance);
			}
		}
	}

	public void StopUWSwimming()
	{
		mForceBrakeUWSwimming = true;
	}

	public void StopUWSwimmingImmediate()
	{
		mUWSwimmingBoostCooldownTimer = 0f;
		mUWSwimmingBoostTimer = 0f;
		mUWSwimmingPitch = 0f;
		mUWSwimmingRoll = 0f;
		mUWSwimmingSpeed = 0f;
	}

	public void TransitAvatar(Vector3 endPos)
	{
		pIsTransiting = true;
		AvAvatar.EnableAllInputs(inActive: false);
		AvAvatar.SetUIActive(inActive: false);
		TweenPosition tweenPosition = TweenPosition.Begin(base.gameObject, 1f, endPos);
		tweenPosition.callWhenFinished = "TweenPositionDone";
		tweenPosition.eventReceiver = base.gameObject;
	}

	public void TweenPositionDone()
	{
		pIsTransiting = false;
		AvAvatar.EnableAllInputs(inActive: true);
		AvAvatar.SetUIActive(inActive: true);
		if (pUWSwimZone != null)
		{
			pUWSwimZone.OnTransitAvatarDone();
		}
	}

	public void UpdateHealth(float delta)
	{
		if (!AvAvatar.IsCurrentPlayer(base.gameObject) || delta == 0f || mState == AvAvatarState.NONE || mState == AvAvatarState.PAUSED)
		{
			return;
		}
		if (!pUWSwimZone._AirMeter._HealthMeter.pExpanded)
		{
			pUWSwimZone._AirMeter._HealthMeter.pExpand = true;
		}
		if (!(delta < 0f))
		{
			return;
		}
		delta = 0f - delta;
		if (OnTakeDamage != null)
		{
			OnTakeDamage(null, ref delta);
		}
		if (SanctuaryManager.pCurPetInstance == null && _Stats != null)
		{
			_Stats._CurrentHealth = Mathf.Max(0f, _Stats._CurrentHealth - delta);
			if (_Stats._CurrentHealth == 0f)
			{
				OnPlayerHealthZero();
			}
		}
		else if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.GetMeterValue(SanctuaryPetMeterType.HEALTH) <= SanctuaryManager.pCurPetInstance._MinPetMeterValue)
		{
			OnPlayerHealthZero();
		}
	}

	public void OnPlayerHealthZero()
	{
		if (pUWSwimZone != null)
		{
			SnChannel.Play(_PlayerHealthZeroSFX);
			if (pUWSwimZone.pLastUsedBreathZone != null)
			{
				pUWSwimZone.pLastUsedBreathZone.SetState(UWBreathZoneCSM.BreathZoneSubState.BREATHING);
			}
			else
			{
				pUWSwimZone.Exit();
			}
		}
	}

	public float GetAllowedYPosForCamera()
	{
		return pUWSwimZone._TopYPosition - pUWSwimZone._CameraDistanceFromSurface;
	}

	public float GetAllowedYPosForAvatar()
	{
		return pUWSwimZone._TopYPosition - pUWSwimZone._AvatarDistanceFromSurface;
	}

	public bool IsInTriggerZone()
	{
		return base.transform.position.y > pUWSwimZone._TopYPosition - pUWSwimZone._CSMTriggerHeight;
	}
}
