using System;
using System.Collections;
using System.Collections.Generic;
using KA.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SanctuaryPet : Pet
{
	public delegate void PetMount(bool mount, PetSpecialSkillType skill);

	public delegate void PetJump(bool isJump);

	private struct FlightAnimState
	{
		public string[] FlapAnim;

		public string[] GlideAnim;
	}

	private const string PRICOLOR = "_PrimaryColor";

	private const string SECCOLOR = "_SecondaryColor";

	private const string TERCOLOR = "_TertiaryColor";

	private const string SADDLE = "Saddle";

	public const uint LOG_MASK = 32u;

	public static bool pSkillNoDateCheck = true;

	public static SanctuaryPet pLastClickedPet = null;

	[NonSerialized]
	public Transform pFlyCollider;

	[NonSerialized]
	public CapsuleCollider pClickCollider;

	[HideInInspector]
	public RaisedPetData pData;

	public PetMeterActionData[] _ActionMeterData;

	public GameObject _ActionDoneMessageObject;

	private GameObject mOldActionDoneMessageObject;

	[HideInInspector]
	public List<AssetBundle> _AnimBundles;

	public Texture2D[] _ColorMasks;

	public Texture2D _ShadowTex;

	public Texture2D _HighLightTex;

	public bool _PlayMoodParticleInLab;

	public string[] _FidgetAnim;

	public string _PettingLieDownAnim = "LieDownPetting01";

	public string[] _PettingPartAnim;

	public float _PettingHeadSize = 0.09f;

	public float _PettingBodySize = 0.18f;

	public string _PounceAnim = "Pounce";

	public string _RunSadAnim = "RunSad";

	public string _WalkSadAnim = "WalkSad";

	public string _RunAngryAnim = "RunAngry";

	public string _WalkAngryAnim = "WalkAngry";

	public string _MountIdleAnim = "IdleStand";

	public string _MountRunAnim = "Run";

	public string _AttackAnim = "Attack01";

	public string _FlyAttackAnim = "FlyAttackAdd";

	public AudioClip _BreatheFireSound;

	[NonSerialized]
	public int pAge;

	public AIActor_Pet AIActor;

	public List<GameObject> _DisabledWhenMounted;

	private PetWeaponManager mWeaponManager;

	private MMOAvatar mMMOAvatar;

	private float mTOWPlayTime;

	private float mTOWTimer;

	private Transform mTOWObj;

	private Transform mBrushObj;

	private SanctuaryPetMeterInstance[] mMeterInstances;

	private bool mChewToy;

	public PetAgeData pCurAgeData;

	private SanctuaryPetTypeInfo mTypeInfo;

	private SanctuaryPetTypeSettings mTypeSettings;

	public Transform _HatObj;

	public Vector3 _HatOffset = new Vector3(0f, 0f, 0f);

	public Vector3 _FlyScale = new Vector3(1.4f, 1.4f, 1.4f);

	private float mSkillLevel;

	public bool pMeterPaused;

	private float mUpdateTimer;

	protected int mFlySkillLevel;

	public bool _NoWalk;

	private Transform mPillionRider;

	private PetSpecialSkillType mMountSkill;

	private bool mMountPending;

	private bool mEventsTriggered;

	public float _MinPetMeterValue = 0.01f;

	private Vector3 mPrevPosition;

	private Vector3 mPrevFacing;

	private float mPrevVelocity;

	private bool mIsAccelerating;

	private float mCurFXAlpha;

	private List<GameObject> mTrailFX;

	private bool mIsGlowing;

	[HideInInspector]
	public bool pIsGlowDisabled;

	private Coroutine mRemoveGlowCoroutine;

	[HideInInspector]
	public bool mMountDisabled;

	[HideInInspector]
	public bool pIsFlying;

	[HideInInspector]
	public bool mNPC;

	private Transform mSeatBone;

	private Transform mAvatarFlyBone;

	private Transform mAvatarRootBone;

	private Vector3 mAvatarMountOffset = Vector3.zero;

	private Transform mDragonRootBone;

	private Vector3 mFlyPivot;

	private Transform mDragonTransform;

	private string mAnimationWithSpeedDt;

	private float mAnimationOldSpeed;

	private float mAnimationSpeedCorrectionTimeLeft;

	private string mSkinApplied = string.Empty;

	private string mCustomSkinName = string.Empty;

	private bool mApplyGlowSkinParameters;

	private Dictionary<string, SkinnedMeshRenderer> mRendererMap = new Dictionary<string, SkinnedMeshRenderer>();

	private List<AssetBundleRequest> mAssetBundleRequests = new List<AssetBundleRequest>();

	private bool mPetLoadDone;

	public CountDownTimer mGlowTimer = new CountDownTimer();

	private static AvAvatarController mLocalController = null;

	[NonSerialized]
	public bool _UseMeterAutoSaving;

	[NonSerialized]
	public float _MeterAutoSaveFrequency = 60f;

	private float mMeterAutoSaveTimer;

	private bool mIsMeterDirty;

	[NonSerialized]
	public bool pIsMounted;

	public GameObject _BreathAttackBall;

	[NonSerialized]
	public AssetBundle mAvatarAnimBundle;

	public string _RideAssetName;

	public bool _NoFlyingLanding;

	private Dictionary<RaisedPetAccType, object> mAccObjects = new Dictionary<RaisedPetAccType, object>();

	[NonSerialized]
	private AvAvatarAnimation mInSettings;

	private int mSyncFrame;

	public int _SyncFrameOffset;

	private bool mPettingDown;

	[NonSerialized]
	public string _CustomPettingAnim;

	[NonSerialized]
	public bool pCanBreatheAttackNow;

	public string _RootBone = "Main_Root";

	public string _Bone_Nose = "Head_J";

	public string[] _BoneHeads = new string[1] { "Head_J" };

	public string _Bone_Seat = "AvatarConstraint_J";

	public string _Bone_Hip = "Hip_J";

	public Vector3 _Head_LookAt_Offset = Vector3.zero;

	public Vector3 _PillionOffset = Vector3.zero;

	public string _PictureTargetTransformName = "";

	public string[] _DirtTextures = new string[4] { "BlasterPetDirt01", "BlasterPetDirt02", "BlasterPetDirt03", "BlasterPetDirt04" };

	public GameObject _MessageObject;

	private List<SanctuaryPetAccData> mAccLoaders;

	public SanctuaryPowerUpData[] _PowerUpData;

	public float _MaxJumpInterval = 1.5f;

	public int _JumpCountToFly = 2;

	public float _FireDelay = 0.1f;

	private float mRemainingFireDelay;

	private Transform mFiringTarget;

	private bool mFiringUseDirection;

	private Vector3 mFiringDirection;

	private SanctuaryPowerUpType mPowerUpType;

	private GameObject mPowerUpPrtEffect;

	private float mPowerUpTimer;

	private Texture mOriginalTexture;

	private Texture mOriginalBumpMapTexture;

	private ItemData mFoodItemData;

	private PetSpecialSkillType mCurrentSkillType = PetSpecialSkillType.FLY;

	public List<GameObject> pEquippedItems = new List<GameObject>();

	private FlightAnimState mFlightRollAnim;

	private FlightAnimState mFlightPitchAnim;

	private FlightAnimState mFlightForwardAnim;

	private float mFlyGlideTime;

	public float mFlyGlideTimer = 4f;

	private float mFlyGlideWeight;

	private float mFlyAccelWeight;

	private float mFlyBrakeWeight;

	[HideInInspector]
	public int mPrevAnimState = -1;

	private float mMoveToTimer;

	private float mMoveToHeight;

	private float mMoveFromHeight;

	private bool mEnablePetAnim = true;

	public float _FlightClubHandicap;

	private List<GameObject> mMoodPrtEffect;

	private bool mDisableMoodParticle;

	private PetMoodParticleData mCurrMoodPrtData;

	private bool mPrevWaterSplash;

	private float mWaterCheckInterval = 0.2f;

	public float _WaterSplashSpawnInterval = 0.2f;

	private float mWaterSplashCheckTime;

	private GameObject mWaterParticleObj;

	private float mWaterCheckTime;

	private List<string> mIgnoreAccesoryList = new List<string> { "Saddle" };

	public ParticleSystem _SleepingParticleSystem;

	private string mAnimToPlay = string.Empty;

	public UiPetMeter pUiPetMeter { get; set; }

	public PetWeaponManager pWeaponManager => mWeaponManager;

	public float pSkillLevel => mSkillLevel;

	public bool pMountPending => mMountPending;

	public bool pCustomSkinAvailable => !string.IsNullOrEmpty(mCustomSkinName);

	public static AvAvatarController pLocalController
	{
		get
		{
			if (!mLocalController && (bool)AvAvatar.pObject)
			{
				mLocalController = AvAvatar.pObject.GetComponentInChildren<AvAvatarController>();
			}
			return mLocalController;
		}
	}

	public bool pCanShoot
	{
		get
		{
			if (SanctuaryManager.pCurPetInstance == this && (pWeaponShotsAvailable <= 0 || mWeaponManager.pCooldownTimer > 0f || (_SleepingParticleSystem != null && _SleepingParticleSystem.isEmitting)))
			{
				return false;
			}
			return true;
		}
	}

	public SanctuaryPetTypeInfo pTypeInfo => mTypeInfo;

	public Dictionary<RaisedPetAccType, object> pAccObjects
	{
		get
		{
			return mAccObjects;
		}
		set
		{
			mAccObjects = value;
		}
	}

	public SanctuaryPowerUpType pPowerUpType => mPowerUpType;

	public PetSpecialSkillType pCurrentSkillType => mCurrentSkillType;

	public bool pEnablePetAnim
	{
		get
		{
			return mEnablePetAnim;
		}
		set
		{
			mEnablePetAnim = value;
		}
	}

	public List<GameObject> pMoodPrtEffect => mMoodPrtEffect;

	public AvAvatarController pAvatarController => mAvatarController;

	public int pWeaponShotsAvailable
	{
		get
		{
			if (mWeaponManager != null)
			{
				return mWeaponManager.GetWeaponShotsAvailable();
			}
			return 0;
		}
		set
		{
			if (mWeaponManager != null)
			{
				mWeaponManager.UpdateShotsAvailable(value);
			}
		}
	}

	public List<GameObject> pTrailFX
	{
		get
		{
			return mTrailFX;
		}
		set
		{
			mTrailFX = value;
		}
	}

	public string pAnimToPlay
	{
		get
		{
			return mAnimToPlay;
		}
		set
		{
			mAnimToPlay = value;
		}
	}

	public event PetMount OnPetMount;

	public event PetJump OnPetJump;

	public override void SetFollowAvatar(bool follow)
	{
		base.SetFollowAvatar(follow);
		if (!follow)
		{
			DestroyPetCSM();
		}
		if (AIActor != null)
		{
			if (follow && !pIsMounted)
			{
				if (IsSwimming())
				{
					AIActor.SetState(AISanctuaryPetFSM.SWIMMING);
				}
				else
				{
					AIActor.SetState(AISanctuaryPetFSM.NORMAL);
				}
			}
			else if (!follow && AIActor._State == AISanctuaryPetFSM.NORMAL)
			{
				AIActor.SetState(AISanctuaryPetFSM.CUSTOM);
			}
		}
		UpdateAvatarTargetPosition();
	}

	public float GetWeaponCooldown()
	{
		if (mWeaponManager != null)
		{
			float num = mWeaponManager.GetCooldown();
			if (AvAvatar.pLevelState == AvAvatarLevelState.RACING || AvAvatar.pLevelState == AvAvatarLevelState.FLIGHTSCHOOL)
			{
				num *= 0.5f;
			}
			return num;
		}
		return 1f;
	}

	public MinMax GetWeaponRechargeRange()
	{
		if (mWeaponManager != null)
		{
			return mWeaponManager.GetWeaponRechargeRange();
		}
		return new MinMax(1f, 4f);
	}

	public int GetWeaponTotalShots()
	{
		if (mWeaponManager != null)
		{
			return mWeaponManager.GetWeaponTotalShots();
		}
		return 1;
	}

	public void Awake()
	{
		AIActor = GetComponent<AIActor_Pet>();
		mWaterSplashCheckTime = _WaterSplashSpawnInterval;
		mWeaponManager = GetComponent<PetWeaponManager>();
		mDragonTransform = base.transform;
		SkinnedMeshRenderer[] componentsInChildren = GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
		{
			mRendererMap[skinnedMeshRenderer.name] = skinnedMeshRenderer;
		}
		pIsFlying = false;
	}

	public void OnSceneChanged(Scene activeScene, Scene newScene)
	{
		try
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		catch
		{
		}
	}

	public virtual void BreathAttack(bool t)
	{
		SetEmitter("PfPrtFireDragon", t);
	}

	public virtual void SetBreathAttackBall(GameObject ball, string boneName, Vector3 offset)
	{
		if (ball != null)
		{
			_BreathAttackBall = ball;
			_BreathAttackBall.transform.parent = FindBoneTransform(boneName);
			_BreathAttackBall.transform.localPosition = offset;
			_BreathAttackBall.transform.localRotation = Quaternion.identity;
			_BreathAttackBall.SetActive(value: true);
		}
		else if (_BreathAttackBall != null)
		{
			_BreathAttackBall.SetActive(value: false);
			_BreathAttackBall.transform.parent = null;
			_BreathAttackBall = null;
		}
	}

	public static void AddMountEvent(SanctuaryPet petInstance, PetMount petMount)
	{
		if (petInstance != null)
		{
			petInstance.OnPetMount -= petMount;
			petInstance.OnPetMount += petMount;
		}
	}

	public static void RemoveMountEvent(SanctuaryPet petInstance, PetMount petMount)
	{
		if (petInstance != null)
		{
			petInstance.OnPetMount -= petMount;
		}
	}

	public static void AddJumpEvent(SanctuaryPet petInstance, PetJump petJump)
	{
		if (petInstance != null)
		{
			petInstance.OnPetJump -= petJump;
			petInstance.OnPetJump += petJump;
		}
	}

	public static void RemoveJumpEvent(SanctuaryPet petInstance, PetJump petJump)
	{
		if (petInstance != null)
		{
			petInstance.OnPetJump -= petJump;
		}
	}

	public virtual void SetBreathAttackBall(GameObject ball)
	{
		SetBreathAttackBall(ball, _Bone_Nose, new Vector3(0f, 0f, 1.5f));
		if (!(ball != null))
		{
			return;
		}
		Transform transform = FindBoneTransform("PfPrtFireDragon");
		if (transform != null)
		{
			ParticleSystem component = transform.GetComponent<ParticleSystem>();
			if (component != null)
			{
				ParticleSystem.MainModule main = component.main;
				ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = component.velocityOverLifetime;
				ParticleSystem.MinMaxCurve startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 2f);
				main.startLifetime = startLifetime;
				main.maxParticles = 300;
				velocityOverLifetime.z = 19f;
			}
		}
	}

	public void OnActivate()
	{
		pLastClickedPet = this;
		ObClickable component = base.gameObject.GetComponent<ObClickable>();
		if (component != null && component._ActivateObject != null && component._ActivateObject.GetComponent<ObContextSensitive>() != null)
		{
			component._ActivateObject.SendMessage("OnActivate");
		}
	}

	public void OnClick()
	{
		pLastClickedPet = this;
	}

	public override string GetIdleAnimationName()
	{
		if (pIsMounted)
		{
			return _MountIdleAnim;
		}
		if (_IdleAnimName == "SitIdle")
		{
			if (HasMood(Character_Mood.happy))
			{
				return "SitSad";
			}
			if (HasMood(Character_Mood.angry))
			{
				return "SitAngry";
			}
			return "SitIdle";
		}
		if (_IdleAnimName == "Idle")
		{
			if (HasMood(Character_Mood.happy))
			{
				return "IdleSad";
			}
			if (HasMood(Character_Mood.angry))
			{
				return "IdleAngry";
			}
			return "Idle";
		}
		return _IdleAnimName;
	}

	public string GetAnimationName(Character_Mood mood)
	{
		if (_IdleAnimName == "SitIdle")
		{
			return mood switch
			{
				Character_Mood.happy => "SitSad", 
				Character_Mood.angry => "SitAngry", 
				_ => "SitIdle", 
			};
		}
		if (_IdleAnimName == "Idle" || _IdleAnimName == _MountIdleAnim)
		{
			return mood switch
			{
				Character_Mood.happy => "IdleSad", 
				Character_Mood.angry => "IdleAngry", 
				_ => "Idle", 
			};
		}
		return _IdleAnimName;
	}

	public override bool IsAnimIdle(string aname, out bool lookatcam)
	{
		lookatcam = false;
		switch (aname)
		{
		case "SitIdle":
		case "SitSad":
		case "SitAngry":
		case "Idle":
		case "IdleSad":
		case "IdleAngry":
			return true;
		default:
			return false;
		}
	}

	public override void PlayAnim(string aname)
	{
	}

	public override void PlayAnim(string aname, int numLoops, float aspeed, int alayer)
	{
	}

	public override void PlayMoveAnimation()
	{
	}

	public virtual void ApplyMeter()
	{
		if (UserRankData.pIsReady && SanctuaryManager.pCurPetData != null)
		{
			float num = GetMeterValue(SanctuaryPetMeterType.HAPPINESS) / SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.HAPPINESS, SanctuaryManager.pCurPetData);
			float num2 = GetMeterValue(SanctuaryPetMeterType.ENERGY) / SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.ENERGY, SanctuaryManager.pCurPetData);
			if (num >= mTypeSettings._FiredUpThreshold)
			{
				SetMood(Character_Mood.firedup, t: true);
			}
			else if (num >= mTypeSettings._HappyThreshold)
			{
				SetMood(Character_Mood.happy, t: true);
			}
			else
			{
				SetMood(Character_Mood.angry, t: true);
			}
			SetMood(Character_Mood.tired, num2 <= mTypeSettings._TiredThreshold);
		}
	}

	public void InitTextureFromData()
	{
		if (pData.Colors != null && pData.Colors.Length != 0)
		{
			pData.pTexture = GenerateTexture(pData.Colors);
			pData.pTextureBMP = null;
		}
		if (!(pData.pTexture != null))
		{
			return;
		}
		foreach (SkinnedMeshRenderer value in mRendererMap.Values)
		{
			Material[] materials = value.materials;
			foreach (Material material in materials)
			{
				if (material.HasProperty("_Skin"))
				{
					material.SetTexture("_Skin", pData.pTexture);
				}
				else
				{
					material.mainTexture = pData.pTexture;
				}
			}
		}
	}

	public string GetHeadBoneName()
	{
		if (_BoneHeads != null && _BoneHeads.Length != 0)
		{
			return _BoneHeads[0];
		}
		return string.Empty;
	}

	public virtual void InitBones()
	{
		mNumPettingBones = 0;
		string[] boneHeads = _BoneHeads;
		foreach (string headBoneFind in boneHeads)
		{
			SetHeadBoneFind(headBoneFind);
		}
		if (pData.pStage == RaisedPetStage.FIND || pData.pStage == RaisedPetStage.EGGINHAND)
		{
			SetPettingBone(GetHeadBoneName(), "PettingHead", new Vector3(0f, 0f, 0f), _PettingHeadSize);
			SetPettingBone(_Bone_Hip, "PettingHip", new Vector3(0f, 0f, 0f), _PettingBodySize);
		}
		else
		{
			SetPettingBone(GetHeadBoneName(), "PettingHead", new Vector3(0f, 0f, 0f), _PettingHeadSize);
			SetPettingBone(_Bone_Hip, "PettingHip", new Vector3(0f, 0f, 0f), _PettingBodySize);
		}
		mPettingBones[2] = null;
	}

	public virtual string[] GetBathBoneArray()
	{
		return new string[11]
		{
			"LWing04", "FaceL", "HeadJ", "LElb", "LAnkl", "Chst", "Hip", "Tail02", "Tail03", "Tail04",
			"Tail05"
		};
	}

	public void Init(RaisedPetData pdata, bool noHat, string customSkinName = null)
	{
		pClickCollider = (CapsuleCollider)collider;
		if (pFlyCollider == null)
		{
			pFlyCollider = base.transform.Find("FlyCollider");
		}
		if (pFlyCollider != null)
		{
			pFlyCollider.gameObject.SetActive(value: false);
		}
		pData = pdata;
		pData.pObject = base.gameObject;
		SetTypeInfo(SanctuaryData.FindSanctuaryPetTypeInfo(pData.PetTypeID));
		InitTextureFromData();
		Initialize(Camera.main, 3);
		InitBones();
		CacheMountBones();
		mCustomSkinName = customSkinName;
		UpdateAccessories(noHat);
		mFlySkillLevel = (int)GetSkillLevel(PetSkills.FLY);
		mCurrentSkillType = PetSpecialSkillType.RUN;
		_CanFly = mFlySkillLevel > 2;
		TrailRenderer[] componentsInChildren = GetComponentsInChildren<TrailRenderer>(includeInactive: true);
		mTrailFX = new List<GameObject>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			mTrailFX.Add(componentsInChildren[i].gameObject);
		}
		if ((bool)collider)
		{
			mFlyPivot = (collider.bounds.max - collider.bounds.min) / 2f;
			mFlyPivot = Vector3.Scale(mFlyPivot, new Vector3(0f, -1f, 0f));
		}
		UpdateShaders();
		LoadAnimBundles();
	}

	public void LoadAnimBundles()
	{
		if (_AnimBundles == null || _AnimBundles.Count <= 0)
		{
			return;
		}
		foreach (AssetBundle animBundle in _AnimBundles)
		{
			if (!(animBundle == null))
			{
				AssetBundleRequest item = animBundle.LoadAllAssetsAsync();
				mAssetBundleRequests.Add(item);
			}
		}
	}

	public void ApplyAnimation()
	{
		foreach (AssetBundleRequest mAssetBundleRequest in mAssetBundleRequests)
		{
			UnityEngine.Object[] allAssets = mAssetBundleRequest.allAssets;
			if (allAssets == null || allAssets.Length == 0)
			{
				continue;
			}
			for (int i = 0; i < allAssets.Length; i++)
			{
				GameObject gameObject = allAssets[i] as GameObject;
				if (gameObject != null)
				{
					Animation component = gameObject.GetComponent<Animation>();
					if (component != null)
					{
						foreach (AnimationState item in component)
						{
							base.animation.AddClip(item.clip, item.clip.name);
						}
					}
					else
					{
						UtDebug.LogError("Animation component was expected in " + gameObject.name);
					}
				}
				else
				{
					UtDebug.LogError("Animation object is null !!!");
				}
			}
		}
		mAssetBundleRequests = null;
		_AnimBundles = null;
		if (!string.IsNullOrEmpty(_IdleAnimName))
		{
			base.animation.Play(_IdleAnimName);
		}
		else
		{
			_IdleAnimName = _AnimNameIdle;
		}
		if (_PettingPartAnim == null || _PettingPartAnim.Length == 0)
		{
			_PettingPartAnim = new string[2];
			_PettingPartAnim[0] = "GlassDownPetHead";
			_PettingPartAnim[1] = "GlassDownPetBody";
		}
		mFlightRollAnim.FlapAnim = new string[2];
		mFlightRollAnim.GlideAnim = new string[2];
		mFlightPitchAnim.FlapAnim = new string[2];
		mFlightPitchAnim.GlideAnim = new string[2];
		mFlightForwardAnim.FlapAnim = new string[2];
		mFlightForwardAnim.GlideAnim = new string[2];
		mFlightForwardAnim.FlapAnim[0] = "FlyForward";
		mFlightForwardAnim.GlideAnim[0] = "FlyForwardGlide";
		mFlightForwardAnim.FlapAnim[1] = "FlyForwardBoostFlap";
		mFlightForwardAnim.GlideAnim[1] = "FlyBrake";
		mFlightRollAnim.FlapAnim[0] = "FlyTurnLeft";
		mFlightRollAnim.GlideAnim[0] = "BankingLeft";
		mFlightRollAnim.FlapAnim[1] = "FlyTurnRight";
		mFlightRollAnim.GlideAnim[1] = "BankingRight";
		mFlightPitchAnim.FlapAnim[0] = "GainingAltitudeFlap";
		mFlightPitchAnim.GlideAnim[0] = "";
		mFlightPitchAnim.FlapAnim[1] = "Dive";
		mFlightPitchAnim.GlideAnim[1] = "";
		int layer = 1;
		for (int j = 0; j < 2; j++)
		{
			if (base.animation[mFlightForwardAnim.FlapAnim[j]] != null)
			{
				base.animation[mFlightForwardAnim.FlapAnim[j]].layer = layer;
			}
			if (base.animation[mFlightForwardAnim.GlideAnim[j]] != null)
			{
				base.animation[mFlightForwardAnim.GlideAnim[j]].layer = layer;
			}
			if (base.animation[mFlightRollAnim.FlapAnim[j]] != null)
			{
				base.animation[mFlightRollAnim.FlapAnim[j]].layer = layer;
			}
			if (base.animation[mFlightRollAnim.GlideAnim[j]] != null)
			{
				base.animation[mFlightRollAnim.GlideAnim[j]].layer = layer;
			}
			if (base.animation[mFlightPitchAnim.FlapAnim[j]] != null)
			{
				base.animation[mFlightPitchAnim.FlapAnim[j]].layer = layer;
			}
			if (base.animation[mFlightPitchAnim.GlideAnim[j]] != null)
			{
				base.animation[mFlightPitchAnim.GlideAnim[j]].layer = layer;
			}
		}
		UpdateAttackAnimState();
	}

	public void SendPetReady()
	{
		if (_MessageObject != null)
		{
			if (!_MessageObject.activeInHierarchy)
			{
				_MessageObject.SetActive(value: true);
				_MessageObject.SendMessage("OnPetReady", this);
				_MessageObject.SetActive(value: false);
			}
			else
			{
				_MessageObject.SendMessage("OnPetReady", this);
			}
			_MessageObject = null;
		}
	}

	public void RegisterAvatarDamage()
	{
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (component != null)
		{
			component.OnTakeDamage = (AvAvatarController.OnTakeDamageDelegate)Delegate.Combine(component.OnTakeDamage, new AvAvatarController.OnTakeDamageDelegate(OnAvatarTakeDamage));
		}
	}

	public void UnregisterAvatarDamage()
	{
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (component != null && component.OnTakeDamage != null)
		{
			component.OnTakeDamage = (AvAvatarController.OnTakeDamageDelegate)Delegate.Remove(component.OnTakeDamage, new AvAvatarController.OnTakeDamageDelegate(OnAvatarTakeDamage));
		}
	}

	private void OnAvatarTakeDamage(GameObject go, ref float damage)
	{
		UpdateMeter(SanctuaryPetMeterType.HEALTH, 0f - damage);
	}

	public void UpdateData(RaisedPetData pdata, bool noHat)
	{
		pData = pdata;
		pData.pObject = base.gameObject;
		UpdateAccessories(noHat);
	}

	protected override void PettingStarted(string partname)
	{
		base.PettingStarted(partname);
		UICursorManager.SetCursor("Activate", showHideSystemCursor: true);
		if (mBrushObj != null)
		{
			mBrushObj.SendMessage("StartBrushing", null, SendMessageOptions.DontRequireReceiver);
		}
		if (_ActionDoneMessageObject != null)
		{
			_ActionDoneMessageObject.SendMessage("OnPettingStart");
		}
	}

	protected override void ProcessPettingEnded(string partname)
	{
		base.ProcessPettingEnded(partname);
		UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
		if (mBrushObj != null)
		{
			mBrushObj.SendMessage("StopBrushing", false, SendMessageOptions.DontRequireReceiver);
		}
		else if (!(mTOWObj != null))
		{
			if (mPettingTimer <= 0f)
			{
				UpdateActionMeters(PetActions.BRUSH, 1f, doUpdateSkill: true);
				CheckForTaskCompletion(PetActions.BRUSH);
			}
			if (_ActionDoneMessageObject != null)
			{
				_ActionDoneMessageObject.SendMessage("OnPettingEnd");
			}
			mPettingDown = false;
		}
	}

	public int FindAccCatID(RaisedPetAccType at)
	{
		return mTypeInfo.FindAccCatID(pData, at);
	}

	public SanctuaryPetAccInfo[] GetAccessoryInfo()
	{
		return mTypeInfo._AccTypeInfo;
	}

	public bool HasAccessory(RaisedPetAccType at)
	{
		return mTypeInfo.HasAccessory(at);
	}

	public string GetTextureType()
	{
		return mTypeInfo.FindTextureTypeName(pData);
	}

	public int GetMaterialIndex()
	{
		if (mTypeInfo != null)
		{
			return mTypeInfo.FindMaterialIndex(pData);
		}
		return 0;
	}

	public override void EnablePetting(bool t)
	{
		if (mCanBePetted)
		{
			base.EnablePetting(t);
			EnablePettingCollider(t);
		}
	}

	public void EnablePettingCollider(bool t)
	{
		if (t)
		{
			pClickCollider.radius = 0.001f;
			return;
		}
		pClickCollider.radius = pCurAgeData._ClickRadius;
		pClickCollider.height = pCurAgeData._ClickHeight;
		pClickCollider.center = pCurAgeData._ClickCenter;
	}

	public void SetClickActivateObject(GameObject obj)
	{
		ObClickable component = base.gameObject.GetComponent<ObClickable>();
		if (component != null)
		{
			component.enabled = obj != null;
			component._ActivateObject = obj;
		}
	}

	public void SetClickableActive(bool active)
	{
		ObClickable component = base.gameObject.GetComponent<ObClickable>();
		if (component != null)
		{
			component.enabled = active;
		}
	}

	public void DestroyPetCSM()
	{
		ObClickable component = base.gameObject.GetComponent<ObClickable>();
		if (component != null && component._ActivateObject != null)
		{
			ObContextSensitive component2 = component._ActivateObject.GetComponent<ObContextSensitive>();
			if (component2 != null)
			{
				component2.CloseMenu();
			}
		}
	}

	public void SetFlyUIObject(GameObject obj)
	{
	}

	public override void DoFidget()
	{
	}

	public void RestoreScale()
	{
		if (mPowerUpType != SanctuaryPowerUpType.SHRIMP)
		{
			base.transform.localScale = Vector3.one;
			return;
		}
		ObScale component = base.gameObject.GetComponent<ObScale>();
		if (component != null)
		{
			component.pOriginalScale = Vector3.one;
		}
	}

	public virtual void SetBirthScale()
	{
	}

	public bool RemovePowerUpScale()
	{
		if (mPowerUpType == SanctuaryPowerUpType.SHRIMP)
		{
			mPowerUpTimer = 0f;
			mPowerUpType = SanctuaryPowerUpType.NONE;
			ObScale component = GetComponent<ObScale>();
			if (component != null)
			{
				component.OnRemoveScaleImmediate();
			}
			return true;
		}
		return false;
	}

	public void SetPlayScale()
	{
		RemovePowerUpScale();
		base.transform.localScale = Vector3.one * pCurAgeData._PlayScale;
	}

	public void SetEelBlastScale()
	{
		RemovePowerUpScale();
		base.transform.localScale = Vector3.one * pCurAgeData._EelBlastScale;
	}

	public void SetBathScale()
	{
	}

	public virtual bool SetAge(int age, bool save, bool resetSkills, GameObject inPictureDoneReceiver = null)
	{
		if (age < 0 || mTypeInfo == null)
		{
			return false;
		}
		pAge = age;
		pCurAgeData = mTypeInfo._AgeData[age];
		if (RaisedPetData.GetGrowthStage(age) != pData.pStage && SanctuaryManager.pInstance != null)
		{
			SanctuaryManager.pInstance.CreateCurrentPet(pData, age, inPictureDoneReceiver, save);
			return true;
		}
		PetAgeBoneData[] boneInfo = pCurAgeData._BoneInfo;
		foreach (PetAgeBoneData petAgeBoneData in boneInfo)
		{
			SetBoneScale0(petAgeBoneData._BoneName, petAgeBoneData._Scale);
		}
		_WalkSpeed = pCurAgeData._WalkSpeed;
		_RunSpeed = pCurAgeData._RunSpeed;
		if (resetSkills)
		{
			ResetSkills();
		}
		pClickCollider.radius = pCurAgeData._ClickRadius;
		pClickCollider.height = pCurAgeData._ClickHeight;
		pClickCollider.center = pCurAgeData._ClickCenter;
		pData.SetState(RaisedPetData.GetGrowthStage(age), save);
		if (save && SanctuaryManager.pInstance != null)
		{
			SanctuaryManager.pInstance.TakePicture(base.gameObject);
		}
		return true;
	}

	public void ResetSkills()
	{
		PetSkillRequirements[] skillsRequired = pCurAgeData._SkillsRequired;
		foreach (PetSkillRequirements petSkillRequirements in skillsRequired)
		{
			pData.UpdateSkillData(petSkillRequirements._Skill.ToString(), 0f, save: false);
			UtDebug.Log("Age Change Skill reset " + petSkillRequirements._Skill, 32u);
		}
	}

	public override void MoveToDone(Transform target)
	{
	}

	public override void GenerateMoveToPath()
	{
		UtDebug.LogWarning("Pet Path not found");
	}

	private Texture2D GenerateTexture(RaisedPetColor[] inColor)
	{
		if (_ColorMasks == null || _ColorMasks.Length == 0)
		{
			return null;
		}
		Color[] array = new Color[inColor.Length];
		foreach (RaisedPetColor raisedPetColor in inColor)
		{
			array[raisedPetColor.Order] = new Color(raisedPetColor.Red, raisedPetColor.Green, raisedPetColor.Blue);
		}
		Texture2D obj = (Texture2D)UtUtilities.GetObjectTexture(base.gameObject, 0);
		int width = obj.width;
		int height = obj.height;
		Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, mipChain: true);
		GrBitmap grBitmap = new GrBitmap(obj);
		GrBitmap grBitmap2 = new GrBitmap(texture2D);
		grBitmap.SetBlendMode(BlendOp.REPLACE, BlendOp.REPLACE, RepeatMode.STAMP);
		grBitmap.BlitTo(grBitmap2, 0, 0);
		GrBitmap grBitmap3 = new GrBitmap("color", width, height, c: false, Color.white);
		for (int j = 0; j < _ColorMasks.Length; j++)
		{
			grBitmap3.Clear(array[j]);
			GrBitmap grBitmap4 = new GrBitmap(_ColorMasks[j]);
			grBitmap4.SetBlendMode(BlendOp.NONE, BlendOp.REPLACE, RepeatMode.STAMP);
			grBitmap4.BlitTo(grBitmap3, 0, 0);
			grBitmap3.SetBlendMode(BlendOp.USEALPHA, BlendOp.NONE, RepeatMode.STAMP);
			grBitmap3.BlitTo(grBitmap2, 0, 0);
		}
		if (_HighLightTex != null)
		{
			GrBitmap grBitmap5 = new GrBitmap(_HighLightTex);
			grBitmap5.SetBlendMode(BlendOp.OVERLAY, BlendOp.NONE, RepeatMode.STAMP);
			grBitmap5.BlitTo(grBitmap2, 0, 0);
		}
		if (_ShadowTex != null)
		{
			GrBitmap grBitmap6 = new GrBitmap(_ShadowTex);
			grBitmap6.SetBlendMode(BlendOp.MULTIPLY, BlendOp.NONE, RepeatMode.STAMP);
			grBitmap6.BlitTo(grBitmap2, 0, 0);
		}
		grBitmap2.Apply(t: true);
		return texture2D;
	}

	public void OnAvatarMountAnimationLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			mAvatarAnimBundle = (AssetBundle)inObject;
		}
	}

	public void LoadAvatarMountAnimation()
	{
	}

	public void SetScale(string bname, float s)
	{
	}

	public void SaveData()
	{
		pData.SaveData();
	}

	public float GetMeterValue(SanctuaryPetMeterType meterType)
	{
		if (mMeterInstances == null || mMeterInstances.Length == 0)
		{
			return 0f;
		}
		SanctuaryPetMeterInstance[] array = mMeterInstances;
		foreach (SanctuaryPetMeterInstance sanctuaryPetMeterInstance in array)
		{
			if (sanctuaryPetMeterInstance.mMeterTypeInfo._Type == meterType)
			{
				return sanctuaryPetMeterInstance.mMeterValData.Value;
			}
		}
		UtDebug.LogError("Meter not found " + meterType);
		return 0f;
	}

	private void UpdateMeter(SanctuaryPetMeterInstance ins, float deltaVal)
	{
		RaisedPetState mMeterValData = ins.mMeterValData;
		if (mMeterValData != null && deltaVal != 0f)
		{
			SetMeter(ins, mMeterValData.Value + deltaVal);
		}
	}

	public SanctuaryPetMeterInstance GetPetMeter(SanctuaryPetMeterType meterType)
	{
		SanctuaryPetMeterInstance[] array = mMeterInstances;
		foreach (SanctuaryPetMeterInstance sanctuaryPetMeterInstance in array)
		{
			if (sanctuaryPetMeterInstance.mMeterTypeInfo._Type == meterType)
			{
				return sanctuaryPetMeterInstance;
			}
		}
		return null;
	}

	public void UpdateMeter(SanctuaryPetMeterType meterType, float deltaVal)
	{
		if (mMeterInstances == null)
		{
			return;
		}
		SanctuaryPetMeterInstance[] array = mMeterInstances;
		foreach (SanctuaryPetMeterInstance sanctuaryPetMeterInstance in array)
		{
			if (sanctuaryPetMeterInstance.mMeterTypeInfo._Type == meterType)
			{
				UpdateMeter(sanctuaryPetMeterInstance, deltaVal);
			}
		}
	}

	public void UpdateAllMeters()
	{
		if (SanctuaryManager.pCurPetInstance == this && mMeterInstances != null)
		{
			SanctuaryPetMeterInstance[] array = mMeterInstances;
			foreach (SanctuaryPetMeterInstance sanctuaryPetMeterInstance in array)
			{
				RaisedPetState mMeterValData = sanctuaryPetMeterInstance.mMeterValData;
				SetMeter(sanctuaryPetMeterInstance, mMeterValData.Value);
			}
		}
	}

	private void SetMeter(SanctuaryPetMeterInstance ins, float val, bool forceUpdate = false)
	{
		if ((pMeterPaused && !forceUpdate) || (MissionManager.pInstance as MissionManagerDO).OverridePetMeter(ins.mMeterTypeInfo._Type))
		{
			return;
		}
		RaisedPetState mMeterValData = ins.mMeterValData;
		if (!ins.mMeterTypeInfo._LocalOnly)
		{
			float num = 1f;
			if (UserRankData.pIsReady && SanctuaryManager.pCurPetData != null)
			{
				num = SanctuaryData.GetMaxMeter(ins.mMeterTypeInfo._Type, pData);
				mMeterValData.Value = Mathf.Clamp(val, _MinPetMeterValue, num);
			}
			else
			{
				mMeterValData.Value = val;
			}
		}
		else
		{
			mMeterValData.Value = val;
		}
		if (pUiPetMeter != null && ins.mMeterTypeInfo._MeterIdx >= 0)
		{
			pUiPetMeter.SetMeter(ins.mMeterTypeInfo._Type, mMeterValData.Value);
		}
		ApplyMeter();
	}

	public bool IsMeterFull(SanctuaryPetMeterType meterType)
	{
		float maxMeter = SanctuaryData.GetMaxMeter(meterType, pData);
		return GetMeterValue(meterType) >= maxMeter;
	}

	public void SetMeter(SanctuaryPetMeterType meterType, float val, bool forceUpdate = false)
	{
		if (mMeterInstances == null)
		{
			return;
		}
		SanctuaryPetMeterInstance[] array = mMeterInstances;
		foreach (SanctuaryPetMeterInstance sanctuaryPetMeterInstance in array)
		{
			if (sanctuaryPetMeterInstance.mMeterTypeInfo._Type == meterType)
			{
				SetMeter(sanctuaryPetMeterInstance, val, forceUpdate);
			}
		}
	}

	public void PlayAnimation(string AnimName, WrapMode wrapMode, float speed = 1f, float CrossFadeTime = 0.2f)
	{
		if (base.animation[AnimName] != null)
		{
			if (speed != base.animation[AnimName].speed)
			{
				if (mAnimationWithSpeedDt != null)
				{
					base.animation.Stop(mAnimationWithSpeedDt);
					mAnimationSpeedCorrectionTimeLeft = 0f;
					base.animation[mAnimationWithSpeedDt].speed = mAnimationOldSpeed;
					mAnimationWithSpeedDt = null;
				}
				mAnimationOldSpeed = base.animation[AnimName].speed;
				mAnimationWithSpeedDt = AnimName;
				mAnimationSpeedCorrectionTimeLeft = base.animation[AnimName].length / base.animation[AnimName].speed;
			}
			base.animation[AnimName].wrapMode = wrapMode;
			base.animation[AnimName].speed = speed;
			if (CrossFadeTime > 0f)
			{
				base.animation.CrossFade(AnimName, CrossFadeTime);
			}
			else
			{
				base.animation.Stop(AnimName);
				base.animation.Play(AnimName);
			}
			if (_SoundMapper != null)
			{
				PlayAnimSFX(AnimName, wrapMode == WrapMode.Loop);
			}
		}
		else
		{
			UtDebug.LogError("Animation failed to play :" + AnimName);
		}
	}

	public void Animate(AvAvatarAnimation inSettings)
	{
		if (inSettings.mName == null || inSettings.mName.Length == 0)
		{
			Debug.LogError(base.name + " invalid Animation");
			return;
		}
		if (base.animation[inSettings.mName] == null)
		{
			UtDebug.LogWarning("Anim not exist = " + inSettings.mName);
			return;
		}
		mInSettings = inSettings;
		mSyncFrame = Time.frameCount + _SyncFrameOffset;
	}

	public void UpdateAnimationSettings(AvAvatarAnimation inSettings)
	{
		if (!(base.animation[inSettings.mName] == null))
		{
			if (inSettings.IsEnabled(4u))
			{
				base.animation[inSettings.mName].wrapMode = inSettings.mWrapMode;
			}
			if (inSettings.IsEnabled(8u))
			{
				base.animation[inSettings.mName].speed = inSettings.mSpeed;
			}
			if (inSettings.IsEnabled(1u))
			{
				base.animation[inSettings.mName].time = inSettings.mOffset * base.animation[inSettings.mName].length;
			}
		}
	}

	public override void SetAvatar(Transform av0, bool SpawnTeleportEffect = true, bool teleportPet = true)
	{
		base.SetAvatar(av0, SpawnTeleportEffect);
		_FlySpeed = 6f;
		_RunSpeed = 6f;
		if (teleportPet)
		{
			TeleportToAvatar(SpawnTeleportEffect);
		}
	}

	public void OnFlyDismount(GameObject avatar, bool falltoGround = true)
	{
		OnFlyDismountImmediate(avatar, falltoGround);
		_ = mCurrentSkillType;
		_ = 1;
		if (AvAvatar.IsCurrentPlayer(avatar))
		{
			TriggerMountEvent(isMount: false, mCurrentSkillType);
		}
	}

	public void OnFlyLanding(GameObject avatar)
	{
		if (mAvatarController == null)
		{
			return;
		}
		mCurrentSkillType = PetSpecialSkillType.RUN;
		pIsFlying = false;
		SetFollowAvatar(follow: false);
		SetFidgetOnOff(isOn: false);
		SetState(Character_State.attached);
		collider.enabled = false;
		ClearRoot();
		AIActor.SetState(AISanctuaryPetFSM.MOUNTED);
		mAvatar = avatar.transform;
		pIsMounted = true;
		CacheMountBones();
		base.transform.parent = mAvatarFlyBone;
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = Quaternion.identity;
		if (mAvatarFlyBone != null)
		{
			mAvatarFlyBone.localPosition = Vector3.zero;
			mAvatarFlyBone.localRotation = Quaternion.identity;
		}
		if (avatar == AvAvatar.pObject)
		{
			if (MainStreetMMOClient.pInstance != null)
			{
				MainStreetMMOClient.pInstance.SetJoinAllowed(MMOJoinStatus.ALLOWED);
			}
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.pSubState = AvAvatarSubState.NORMAL;
			if (MainStreetMMOClient.pInstance != null && MainStreetMMOClient.pInstance.IsLevelMMO(RsResourceManager.pCurrentLevel))
			{
				MainStreetMMOClient.pInstance.SetRaisedPet(pData, 0);
			}
			if (mAvatarController != null)
			{
				if (AvAvatar.pObject == mAvatar.gameObject)
				{
					mAvatarController.UpdateDisplayName(offsetMount: true, UserProfile.pProfileData.HasGroup());
				}
				else
				{
					mAvatarController.UpdateDisplayName(offsetMount: true, mMMOAvatar.pAvatarData?.mInstance?._Group != null);
				}
				mAvatarController.SetFlyingState(FlyingState.Grounded);
			}
		}
		else if (mAvatarController != null)
		{
			mAvatarController.pState = AvAvatarState.IDLE;
			mAvatarController.pSubState = AvAvatarSubState.NORMAL;
		}
	}

	public void OnFlyDismountImmediate(GameObject avatar, bool falltoGround = true)
	{
		if (_NoFlyingLanding && mAvatarController.pSubState == AvAvatarSubState.FLYING)
		{
			mAvatarController.pFlyTakeOffTimer = 0.8f;
			return;
		}
		EndPowerUp(isMounted: false);
		if (mAvatarController != null)
		{
			if (AvAvatar.pObject == mAvatar.gameObject)
			{
				mAvatarController.UpdateDisplayName(offsetMount: false, UserProfile.pProfileData.HasGroup());
			}
			else
			{
				mAvatarController.UpdateDisplayName(offsetMount: false, mMMOAvatar.pAvatarData?.mInstance?._Group != null);
			}
			mAvatarController.SetFlyingState(FlyingState.Grounded);
		}
		pIsMounted = false;
		if (IsSwimming())
		{
			AIActor.SetState(AISanctuaryPetFSM.SWIMMING);
		}
		else if (AIActor._State != AISanctuaryPetFSM.CUSTOM)
		{
			AIActor.SetState(AISanctuaryPetFSM.NORMAL);
		}
		mMountPending = false;
		mEventsTriggered = false;
		SetFollowAvatar(follow: true);
		SetFidgetOnOff(isOn: true);
		SetAvatar(avatar.transform, SpawnTeleportEffect: false);
		_FollowAvatar = true;
		collider.enabled = true;
		if (mAvatarRootBone != null)
		{
			mAvatarRootBone.localPosition = Vector3.zero;
			mAvatarRootBone.localRotation = Quaternion.identity;
		}
		if (mAvatarFlyBone != null)
		{
			mAvatarFlyBone.localPosition = Vector3.zero;
			mAvatarFlyBone.localRotation = Quaternion.identity;
		}
		if (mDragonRootBone != null)
		{
			mDragonRootBone.localPosition = Vector3.zero;
			mDragonRootBone.localRotation = Quaternion.identity;
		}
		if (avatar == AvAvatar.pObject)
		{
			if (MainStreetMMOClient.pInstance != null)
			{
				MainStreetMMOClient.pInstance.SetJoinAllowed(MMOJoinStatus.ALLOWED);
			}
			if (mAvatarController != null)
			{
				mAvatarController.SetMounted(mounted: false);
			}
			if (AvAvatar.pState != AvAvatarState.PAUSED && AvAvatar.pSubState != AvAvatarSubState.UWSWIMMING)
			{
				AvAvatar.pState = AvAvatarState.IDLE;
				AvAvatar.pSubState = AvAvatarSubState.NORMAL;
			}
			if (MainStreetMMOClient.pInstance != null && MainStreetMMOClient.pInstance.IsLevelMMO(RsResourceManager.pCurrentLevel))
			{
				MainStreetMMOClient.pInstance.SetRaisedPet(pData, -1);
			}
			if (_ActionDoneMessageObject != null)
			{
				_ActionDoneMessageObject.SendMessage("OnDismount");
			}
			_ActionDoneMessageObject = mOldActionDoneMessageObject;
		}
		else if (mAvatarController != null)
		{
			mAvatarController.pState = AvAvatarState.IDLE;
			mAvatarController.pSubState = AvAvatarSubState.NORMAL;
			mAvatarController.pVelocity = Vector3.zero;
			mAvatarController.SetMounted(mounted: false);
		}
		SetState(Character_State.idle);
		if (collider != null)
		{
			collider.enabled = false;
			collider.enabled = true;
		}
		Vector3 position = mDragonTransform.position;
		base.transform.parent = null;
		base.transform.position = position + Vector3.up * 3f;
		ClearRoot();
		if (falltoGround)
		{
			FallToGround();
		}
	}

	public void MountPillion(GameObject inRiderObj)
	{
		mPillionRider = inRiderObj.transform;
	}

	public void UnMountPillion()
	{
		mPillionRider = null;
	}

	public void Mount(GameObject avatar, PetSpecialSkillType inSkill)
	{
		if (!(avatar == null) && !mMountPending)
		{
			if (inSkill == PetSpecialSkillType.FLY && mTypeInfo._Flightless)
			{
				inSkill = PetSpecialSkillType.RUN;
			}
			if (AvAvatar.IsCurrentPlayer(avatar) && inSkill == PetSpecialSkillType.FLY)
			{
				mFlyGlideTime = 0f;
			}
			SetAvatar(avatar.transform, SpawnTeleportEffect: false);
			if (mAvatarController != null)
			{
				mAvatarController.SetMounted(mounted: true);
			}
			mMountSkill = inSkill;
			mMountPending = true;
			_FollowAvatar = false;
			if (AvAvatar.IsCurrentPlayer(avatar))
			{
				KAUICursorManager.SetDefaultCursor("Loading");
			}
		}
	}

	public bool IsMountAllowed()
	{
		bool flag = !mMountDisabled && mTypeInfo != null && pAge >= mTypeInfo._MinAgeToMount;
		if (!(SanctuaryManager.pCurPetInstance == this))
		{
			return flag;
		}
		if (flag)
		{
			if (AvAvatar.IsFlying())
			{
				return !mTypeInfo._Flightless;
			}
			return true;
		}
		return false;
	}

	private CustomSkinData GetCustomSkinData(string skinName)
	{
		if (mTypeInfo?._CustomSkinData == null || mTypeInfo._CustomSkinData.Length == 0)
		{
			return null;
		}
		RaisedPetAccessory accessory = pData.GetAccessory(RaisedPetAccType.Materials);
		if (accessory != null)
		{
			int accessoryItemID = pData.GetAccessoryItemID(accessory);
			UserItemData userItemData = null;
			if (CommonInventoryData.pIsReady)
			{
				userItemData = CommonInventoryData.pInstance.FindItem(accessoryItemID);
			}
			if (userItemData == null && ParentData.pInstance.pInventory.pIsReady)
			{
				userItemData = ParentData.pInstance.pInventory.pData.FindItem(accessoryItemID);
			}
			if (userItemData != null)
			{
				string tempSkinName = userItemData.Item.GetAttribute(skinName, skinName);
				CustomSkinData customSkinData = Array.Find(mTypeInfo._CustomSkinData, (CustomSkinData data) => data._SkinName == tempSkinName);
				if (customSkinData != null)
				{
					return customSkinData;
				}
			}
		}
		return Array.Find(mTypeInfo._CustomSkinData, (CustomSkinData data) => data._SkinName == skinName);
	}

	private void StartMount(GameObject avatar, PetSpecialSkillType inSkill)
	{
		if (mAvatarController == null)
		{
			Debug.LogError("Cannot mount!!. Unable to find the controller script.");
			return;
		}
		if (AvAvatar.pLevelState != AvAvatarLevelState.FLIGHTSCHOOL)
		{
			if (AvAvatar.pLevelState == AvAvatarLevelState.RACING)
			{
				mAvatarController.pFlyingData = FlightData.GetFlightData(base.gameObject, FlightDataType.RACING);
			}
			else if (pAge == 2)
			{
				mAvatarController.pFlyingData = FlightData.GetFlightData(base.gameObject, FlightDataType.GLIDING);
			}
			else
			{
				mAvatarController.pFlyingData = FlightData.GetFlightData(base.gameObject, FlightDataType.FLYING);
			}
		}
		if (mAvatarController.pIsPlayerGliding)
		{
			mAvatarController.pIsPlayerGliding = false;
			mAvatarController.OnGlideEnd();
			mAvatarController.UpdateGliding();
			inSkill = PetSpecialSkillType.FLY;
			UiAvatarControls.pInstance.OnFlyingStateChanged(FlyingState.Hover);
		}
		if (inSkill == PetSpecialSkillType.FLY)
		{
			pIsFlying = true;
		}
		mCurrentSkillType = inSkill;
		mOldActionDoneMessageObject = _ActionDoneMessageObject;
		_ActionDoneMessageObject = null;
		SetFollowAvatar(follow: false);
		SetFidgetOnOff(isOn: false);
		SetState(Character_State.attached);
		collider.enabled = false;
		ClearRoot();
		if (avatar == AvAvatar.pObject)
		{
			AvatarData.RemovePartScale(AvatarData.pInstanceInfo);
		}
		else
		{
			MMOAvatar component = avatar.GetComponent<MMOAvatar>();
			if (component != null)
			{
				AvatarData.RemovePartScale(component.pAvatarData);
			}
		}
		AIActor.SetState(AISanctuaryPetFSM.MOUNTED);
		mAvatar = avatar.transform;
		pIsMounted = true;
		CacheMountBones();
		base.transform.parent = mAvatarFlyBone;
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = Quaternion.identity;
		if (avatar == AvAvatar.pObject)
		{
			if (inSkill == PetSpecialSkillType.RUN)
			{
				if (AvAvatar.pState != AvAvatarState.PAUSED && AvAvatar.pState != 0)
				{
					AvAvatar.pState = AvAvatarState.IDLE;
				}
				AvAvatar.pSubState = AvAvatarSubState.NORMAL;
			}
			else
			{
				if (AvAvatar.pState != AvAvatarState.PAUSED && AvAvatar.pState != 0)
				{
					AvAvatar.pState = AvAvatarState.MOVING;
				}
				AvAvatar.pSubState = AvAvatarSubState.FLYING;
				if (MainStreetMMOClient.pInstance != null)
				{
					MainStreetMMOClient.pInstance.SetJoinAllowed(MMOJoinStatus.NOT_ALLOWED);
				}
			}
			if (pLocalController != null)
			{
				pLocalController.pFlyingRotationPivot = mDragonTransform.Find("RotationPivot");
				if (pLocalController.pFlyingRotationPivot == null)
				{
					pLocalController.pFlyingRotationPivot = mDragonTransform;
				}
			}
			if (MainStreetMMOClient.pInstance != null && MainStreetMMOClient.pInstance.IsLevelMMO(RsResourceManager.pCurrentLevel))
			{
				MainStreetMMOClient.pInstance.SetRaisedPet(pData, (int)inSkill);
			}
			mAvatarController.UpdateDisplayName(offsetMount: true, UserProfile.pProfileData.HasGroup());
		}
		else
		{
			if (inSkill == PetSpecialSkillType.RUN)
			{
				mAvatarController.pState = AvAvatarState.IDLE;
				mAvatarController.pSubState = AvAvatarSubState.NORMAL;
			}
			else
			{
				mAvatarController.pState = AvAvatarState.MOVING;
				mAvatarController.pSubState = AvAvatarSubState.FLYING;
			}
			mAvatarController.UpdateDisplayName(offsetMount: true, mMMOAvatar.pAvatarData?.mInstance?._Group != null);
		}
	}

	private void StartGliding()
	{
		if ((bool)mAvatar && AvAvatar.pObject == mAvatar.gameObject && (bool)pLocalController)
		{
			Mount(AvAvatar.pObject, PetSpecialSkillType.FLY);
			pLocalController.UpdateGliding();
		}
	}

	private void CacheMountBones()
	{
		mSeatBone = UtUtilities.FindChildTransform(base.gameObject, _Bone_Seat);
		mDragonRootBone = mDragonTransform.Find(_RootBone);
		if (mAvatarController != null)
		{
			mAvatarFlyBone = mAvatarController._FlyingBone;
			mAvatarRootBone = UtUtilities.FindChildTransform(mAvatarController.gameObject, AvatarSettings.pInstance._AvatarName);
			if (mAvatarRootBone == null)
			{
				mAvatarMountOffset = Vector3.zero;
				mAvatarRootBone = UtUtilities.FindChildTransform(mAvatarController.gameObject, AvatarSettings.pInstance._SpriteName);
			}
			else
			{
				mAvatarMountOffset = Vector3.zero;
			}
		}
	}

	public override void LateUpdate()
	{
		base.LateUpdate();
		if (mMoveToTimer > 0f)
		{
			mMoveToTimer -= Time.deltaTime;
			mMoveFromHeight = Mathf.MoveTowards(mMoveFromHeight, mMoveToHeight, 0.5f * Time.deltaTime);
			Vector3 position = mDragonTransform.position;
			position.y = mMoveFromHeight;
			base.transform.position = position;
		}
		mFlyGlideTime += Time.deltaTime;
		float num = (Mathf.Sin(mFlyGlideTime / mFlyGlideTimer) + 1f) / 2f;
		for (int i = 0; i < 3; i++)
		{
			num = num * num * (3f - 2f * num);
		}
		mFlyGlideWeight = num;
		if (!(mAvatar != null))
		{
			return;
		}
		if (mMMOAvatar == null)
		{
			mMMOAvatar = mAvatar.GetComponent<MMOAvatar>();
		}
		if (mMMOAvatar != null && mMMOAvatar.mLimbo && mMMOAvatar.pRaisedPetData.pObject != null && mMMOAvatar.pRaisedPetData.pObject == base.gameObject)
		{
			base.gameObject.SetActive(value: false);
		}
		if (mAvatarController != null && (mAvatarRootBone == null || !mAvatarRootBone.gameObject.activeInHierarchy))
		{
			CacheMountBones();
		}
		if (pIsMounted && (bool)mSeatBone && (bool)mAvatarFlyBone && (bool)mDragonRootBone)
		{
			if (mAvatarController != null && mAvatarController.pPlayerMounted)
			{
				base.transform.parent = mAvatarFlyBone;
				if ((bool)mAvatarRootBone)
				{
					mAvatarRootBone.position = mSeatBone.position + mAvatarMountOffset;
					mAvatarRootBone.rotation = mSeatBone.rotation;
				}
			}
			else if ((bool)mAvatarRootBone)
			{
				mAvatarRootBone.localPosition = Vector3.zero;
				mAvatarRootBone.localRotation = Quaternion.identity;
			}
			if (mPillionRider != null)
			{
				mPillionRider.position = mSeatBone.position;
				mPillionRider.rotation = mAvatarFlyBone.rotation;
				Vector3 position2 = mPillionRider.TransformPoint(_PillionOffset);
				mPillionRider.position = position2;
			}
			if (mCurrentSkillType == PetSpecialSkillType.FLY)
			{
				mDragonRootBone.localPosition = mFlyPivot;
				mAvatarFlyBone.localPosition = -mFlyPivot;
			}
			else
			{
				mDragonRootBone.localPosition = Vector3.zero;
				mAvatarFlyBone.localPosition = Vector3.zero;
			}
			if (!mEventsTriggered && (bool)mAvatar && AvAvatar.IsCurrentPlayer(mAvatar.gameObject))
			{
				TriggerMountEvent(isMount: true, mCurrentSkillType);
				mEventsTriggered = true;
			}
		}
		else if ((bool)mAvatarRootBone && !mAvatarController.pIsPlayerGliding && mAvatarController.pSubState != AvAvatarSubState.UWSWIMMING)
		{
			mAvatarRootBone.localPosition = Vector3.zero;
			mAvatarRootBone.localRotation = Quaternion.identity;
		}
	}

	public override void Update()
	{
		if (!mPetLoadDone)
		{
			foreach (AssetBundleRequest mAssetBundleRequest in mAssetBundleRequests)
			{
				if (!mAssetBundleRequest.isDone)
				{
					return;
				}
			}
			if (mAccLoaders != null && mAccLoaders.Count > 0)
			{
				foreach (SanctuaryPetAccData mAccLoader in mAccLoaders)
				{
					if (!mAccLoader.pIsReady)
					{
						return;
					}
				}
			}
			ApplyAnimation();
			SendPetReady();
			mPetLoadDone = true;
			return;
		}
		base.Update();
		if (!pIsMounted)
		{
			if (mCurrentGroundNormal.y > 0.707f)
			{
				OrientToNormal(mCurrentGroundNormal);
			}
			else
			{
				OrientToNormal(Vector3.up);
			}
		}
		else
		{
			ClearRoot(bClearPos: false);
		}
		if (mTypeInfo == null)
		{
			return;
		}
		if (!pMeterPaused)
		{
			mUpdateTimer += Time.deltaTime;
			if (mUpdateTimer >= mTypeSettings._UpdateFrequency && mMeterInstances != null)
			{
				bool flag = false;
				SanctuaryPetMeterInstance[] array = mMeterInstances;
				foreach (SanctuaryPetMeterInstance sanctuaryPetMeterInstance in array)
				{
					if (pData.pFoodEffect.ContainsKey((int)sanctuaryPetMeterInstance.mMeterTypeInfo._Type))
					{
						if (pData.pFoodEffect[(int)sanctuaryPetMeterInstance.mMeterTypeInfo._Type] > ServerTime.pCurrentTime)
						{
							continue;
						}
						flag = true;
						pData.pFoodEffect.Remove((int)sanctuaryPetMeterInstance.mMeterTypeInfo._Type);
					}
					float num = GetDecreaseRate(sanctuaryPetMeterInstance);
					if (sanctuaryPetMeterInstance.mMeterTypeInfo._DecreaseRateInPercent)
					{
						num = num * 0.01f * SanctuaryData.GetMaxMeter(sanctuaryPetMeterInstance.mMeterTypeInfo._Type, pData);
					}
					if (SanctuaryManager.pCurPetInstance == this)
					{
						UpdateMeter(sanctuaryPetMeterInstance, (0f - mUpdateTimer) * num);
					}
				}
				if (flag)
				{
					pData.SaveData();
					mIsMeterDirty = false;
				}
				else if (_UseMeterAutoSaving)
				{
					mIsMeterDirty = true;
				}
				mUpdateTimer = 0f;
			}
			if (_UseMeterAutoSaving)
			{
				mMeterAutoSaveTimer += Time.deltaTime;
				if (mMeterAutoSaveTimer >= _MeterAutoSaveFrequency)
				{
					mMeterAutoSaveTimer = 0f;
					if (mIsMeterDirty)
					{
						pData.SaveDataReal(null, null, savePetMeterAlone: true);
						mIsMeterDirty = false;
					}
				}
			}
		}
		if (mTOWTimer > 0f && mTOWObj != null)
		{
			mTOWTimer -= Time.deltaTime;
			if (mTOWTimer <= 0f)
			{
				mTOWTimer = 0f;
				SetTOW(null);
			}
		}
		if (pIsMounted)
		{
			base.transform.localPosition = Vector3.zero;
			base.transform.localRotation = Quaternion.identity;
			if (mInSettings != null && Time.frameCount >= mSyncFrame)
			{
				if (mInSettings.mFadeLength > 0f && mInSettings.IsEnabled(2u))
				{
					base.animation.CrossFade(mInSettings.mName, mInSettings.mFadeLength);
				}
				else
				{
					base.animation.Play(mInSettings.mName);
				}
				if (mInSettings.IsEnabled(4u))
				{
					base.animation[mInSettings.mName].wrapMode = mInSettings.mWrapMode;
				}
				if (mInSettings.IsEnabled(8u))
				{
					base.animation[mInSettings.mName].speed = mInSettings.mSpeed;
				}
				if (mInSettings.IsEnabled(1u))
				{
					base.animation[mInSettings.mName].time = mInSettings.mOffset * base.animation[mInSettings.mName].length;
				}
				mInSettings = null;
			}
			if (AvAvatar.pInputEnabled && AvAvatar.pState != 0 && AvAvatar.pState != AvAvatarState.PAUSED && SanctuaryManager.pCurPetInstance == this && mCurrentSkillType == PetSpecialSkillType.RUN && KAInput.GetButtonUp("DragonMount") && FUEManager.IsInputEnabled("DragonMount"))
			{
				OnFlyDismount(AvAvatar.pObject);
			}
			if (mMountPending && (bool)mAvatar)
			{
				RemovePowerUpScale();
				if (AvAvatar.IsCurrentPlayer(mAvatar.gameObject))
				{
					KAUICursorManager.SetDefaultCursor("Arrow");
				}
				StartMount(mAvatar.gameObject, mMountSkill);
				mMountPending = false;
			}
		}
		else if (!mMountPending && IsMountAllowed() && SanctuaryManager.pCurPetInstance == this && AvAvatar.pInputEnabled && KAInput.GetButtonUp("DragonMount") && FUEManager.IsInputEnabled("DragonMount"))
		{
			if (AvAvatar.pObject.activeInHierarchy && AvAvatar.pInputEnabled && AvAvatar.pState != 0 && AvAvatar.pState != AvAvatarState.PAUSED)
			{
				Mount(AvAvatar.pObject, PetSpecialSkillType.RUN);
				if (MissionManager.pInstance != null)
				{
					MissionManager.pInstance.CheckForTaskCompletion("Action", "MountDragon");
				}
			}
		}
		else if (mMountPending && (bool)mAvatar)
		{
			if (AvAvatar.IsCurrentPlayer(mAvatar.gameObject))
			{
				KAUICursorManager.SetDefaultCursor("Arrow");
			}
			if (IsMountAllowed())
			{
				RemovePowerUpScale();
				StartMount(mAvatar.gameObject, mMountSkill);
				mMountPending = false;
			}
			else
			{
				OnFlyDismount(AvAvatar.pObject);
			}
		}
		if (mPowerUpTimer > 0f)
		{
			mPowerUpTimer -= Time.deltaTime;
			if (mPowerUpTimer <= 0f)
			{
				EndPowerUp();
			}
		}
		if (pData.pIsSleeping && pData.pStage != RaisedPetStage.HATCHING && _SoundMapper != null && !base.audio.isPlaying)
		{
			PlayAnimSFX("Sleep", looping: true);
		}
		if (mEnablePetAnim)
		{
			PetAnimUpdate();
		}
		PetFXUpdate();
		if (mRemainingFireDelay > 0f)
		{
			mRemainingFireDelay = Mathf.Max(0f, mRemainingFireDelay - Time.deltaTime);
			if (mRemainingFireDelay <= 0f)
			{
				DoFire(mFiringTarget, mFiringUseDirection, mFiringDirection);
			}
		}
		if (mAnimationWithSpeedDt != null && base.animation[mAnimationWithSpeedDt] != null)
		{
			mAnimationSpeedCorrectionTimeLeft -= Time.deltaTime;
			if (mAnimationSpeedCorrectionTimeLeft <= 0f)
			{
				mAnimationSpeedCorrectionTimeLeft = 0f;
				base.animation[mAnimationWithSpeedDt].speed = mAnimationOldSpeed;
				mAnimationWithSpeedDt = null;
			}
		}
		UpdateCountDown();
	}

	private float GetDecreaseRate(SanctuaryPetMeterInstance ins)
	{
		float num = ins.mMeterTypeInfo._DecreaseRate;
		if (ins.mMeterTypeInfo.DecreaseRateModifierAtAge != null && ins.mMeterTypeInfo.DecreaseRateModifierAtAge.Length != 0)
		{
			DecreaseRateAtAge[] decreaseRateModifierAtAge = ins.mMeterTypeInfo.DecreaseRateModifierAtAge;
			foreach (DecreaseRateAtAge decreaseRateAtAge in decreaseRateModifierAtAge)
			{
				if (decreaseRateAtAge._PetStage == pData.pStage)
				{
					num = decreaseRateAtAge._Rate;
				}
			}
		}
		if (ins.mMeterTypeInfo._RelatedMeters != null && ins.mMeterTypeInfo._RelatedMeters.Length != 0)
		{
			SanctuaryPetMeterDecreaseRate[] relatedMeters = ins.mMeterTypeInfo._RelatedMeters;
			foreach (SanctuaryPetMeterDecreaseRate sanctuaryPetMeterDecreaseRate in relatedMeters)
			{
				if (GetMeterValue(sanctuaryPetMeterDecreaseRate._Type) < sanctuaryPetMeterDecreaseRate._Value)
				{
					num = sanctuaryPetMeterDecreaseRate._DecreaseRate;
				}
			}
		}
		return num * (1f - ins.mMeterTypeInfo._DecreaseRateAttributeMultiplier - ins.mMeterTypeInfo._DecreaseRateMultiplier);
	}

	public void SetDecreaseRateMultiplier(SanctuaryPetMeterType inMeterType, float multiplier)
	{
		if (mMeterInstances == null)
		{
			return;
		}
		for (int i = 0; i < mMeterInstances.Length; i++)
		{
			if (mMeterInstances[i].mMeterTypeInfo != null && mMeterInstances[i].mMeterTypeInfo._Type == inMeterType)
			{
				mMeterInstances[i].mMeterTypeInfo._DecreaseRateMultiplier = multiplier;
				mUpdateTimer = 0f;
			}
		}
	}

	public float GetDecreaseRateMultiplier(SanctuaryPetMeterType inMeterType)
	{
		if (mMeterInstances != null)
		{
			for (int i = 0; i < mMeterInstances.Length; i++)
			{
				if (mMeterInstances[i].mMeterTypeInfo != null && mMeterInstances[i].mMeterTypeInfo._Type == inMeterType)
				{
					return mMeterInstances[i].mMeterTypeInfo._DecreaseRateMultiplier;
				}
			}
		}
		return 0f;
	}

	public void OnModifersUpdated()
	{
		if (mMeterInstances != null)
		{
			SanctuaryPetMeterInstance[] array = mMeterInstances;
			foreach (SanctuaryPetMeterInstance sanctuaryPetMeterInstance in array)
			{
				sanctuaryPetMeterInstance.mMeterTypeInfo._DecreaseRateAttributeMultiplier = GetDecreaseRateAttributeMultiplier(sanctuaryPetMeterInstance);
			}
		}
	}

	private float GetDecreaseRateAttributeMultiplier(SanctuaryPetMeterInstance ins)
	{
		float num = 0f;
		string text = ins.mMeterTypeInfo._Type.ToString().ToLower();
		if (SanctuaryManager.pCurPetInstance == this && AvAvatar.pObject != null && AvAvatar.pObject.GetComponent<AvAvatarController>() != null)
		{
			AvatarDataPart[] part = AvatarData.pInstanceInfo.mInstance.Part;
			foreach (AvatarDataPart avatarDataPart in part)
			{
				if (avatarDataPart.Attributes == null)
				{
					continue;
				}
				AvatarPartAttribute[] attributes = avatarDataPart.Attributes;
				foreach (AvatarPartAttribute avatarPartAttribute in attributes)
				{
					if (avatarPartAttribute.Key.ToLower() == text)
					{
						float result = 0f;
						if (float.TryParse(avatarPartAttribute.Value, out result))
						{
							num += result;
						}
					}
				}
			}
		}
		return num;
	}

	public void PetFXUpdate()
	{
		bool flag = ((base.transform.forward.y > Mathf.Sin(0.17453292f)) ? true : false);
		float num = ((mIsAccelerating && !flag && mCurrentSkillType == PetSpecialSkillType.FLY) ? 1f : (-1f));
		mCurFXAlpha = Mathf.Clamp(mCurFXAlpha + num * Time.deltaTime, 0f, 1f);
		if (mTrailFX != null)
		{
			int count = mTrailFX.Count;
			for (int i = 0; i < count; i++)
			{
				mTrailFX[i].SetActive(mCurFXAlpha > Mathf.Epsilon);
				TrailRenderer component = mTrailFX[i].GetComponent<TrailRenderer>();
				if (!(component != null))
				{
					continue;
				}
				Material[] materials = component.materials;
				int num2 = materials.Length;
				for (int j = 0; j < num2; j++)
				{
					Material material = materials[j];
					if (material != null && material.HasProperty("_TintColor"))
					{
						Color color = material.GetColor("_TintColor");
						color.a = mCurFXAlpha;
						material.SetColor("_TintColor", color);
					}
				}
			}
		}
		UpdateWaterSplash();
	}

	public override void UpdateHeadBone(LookAtHeadBoneData headBoneData)
	{
		if (!(AIActor != null) || AIActor._State == AISanctuaryPetFSM.MOUNTED || AIActor._State == AISanctuaryPetFSM.SCIENCE_LAB)
		{
			base.UpdateHeadBone(headBoneData);
		}
	}

	private bool IsPetSwimIdle()
	{
		float num = (base.transform.root.position - mPrevPosition).magnitude / Time.deltaTime;
		mPrevPosition = mDragonTransform.root.position;
		mIsAccelerating = num > mPrevVelocity;
		mPrevVelocity = num;
		return mPrevVelocity < 0.2f;
	}

	public void PetAnimUpdate()
	{
		if (AIActor != null && AIActor._State != AISanctuaryPetFSM.MOUNTED && AIActor._State != AISanctuaryPetFSM.SCIENCE_LAB && AIActor._State != AISanctuaryPetFSM.IDLE)
		{
			StopLookAtObject();
			return;
		}
		Animation animation = base.animation;
		string text = GetIdleAnimationName();
		if (!string.IsNullOrEmpty(mAnimToPlay) && animation[mAnimToPlay] != null)
		{
			text = mAnimToPlay;
			if (!animation.IsPlaying(text) || mPrevAnimState == 0)
			{
				animation.CrossFade(text, _CrossFadeTime);
				if (_SoundMapper != null)
				{
					PlayAnimSFX(text, looping: false);
				}
			}
		}
		else
		{
			float num = (base.transform.root.position - mPrevPosition).magnitude / Time.deltaTime;
			bool flag = base.transform.root.position.y > mPrevPosition.y;
			mPrevPosition = mDragonTransform.root.position;
			mIsAccelerating = num > mPrevVelocity;
			mPrevVelocity = num;
			float num2 = Vector3.Dot(base.transform.root.forward, mPrevFacing);
			num2 = ((num2 >= 1f - Mathf.Epsilon) ? 0f : Mathf.Acos(num2));
			mPrevFacing = mDragonTransform.root.forward;
			if (mCurrentSkillType == PetSpecialSkillType.FLY && mAvatarController != null && num > Mathf.Epsilon)
			{
				float num3 = mFlyGlideWeight;
				float num4 = 1f - mFlyGlideWeight;
				float b = ((mAvatarController.pFlyingFlapCooldownTimer > Mathf.Epsilon) ? 1f : 0f);
				mFlyAccelWeight = Mathf.Lerp(mFlyAccelWeight, b, Time.deltaTime * 4f);
				float b2 = ((mAvatarController.pIsBraking || AvAvatarController.mForceBraking) ? 1f : 0f);
				mFlyBrakeWeight = Mathf.Lerp(mFlyBrakeWeight, b2, Time.deltaTime * 4f);
				float value = mAvatarController.pFlyingRoll * 360f / 45f;
				float value2 = mAvatarController.pFlyingPitch * 360f / 35f;
				value = Mathf.Clamp(value, -1f, 1f);
				value2 = Mathf.Clamp(value2, -1f, 1f);
				Vector3 vector = new Vector3(value, value2, 0f);
				float num5 = 1f - Mathf.Max(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
				if (mAvatarController.pIsBraking)
				{
					num5 = Mathf.Max(0.5f * mFlyBrakeWeight, num5);
				}
				float num6 = Mathf.Abs(vector.normalized.x) * (1f - num5);
				float num7 = Mathf.Abs(vector.normalized.y) * (1f - num5);
				float num8 = ((vector.x < 0f) ? 0f : 1f);
				float num9 = ((vector.y < 0f) ? 0f : 1f);
				if (animation[mFlightForwardAnim.FlapAnim[0]] != null)
				{
					animation.Blend(mFlightForwardAnim.FlapAnim[0], num5 * num3 * (1f - mFlyAccelWeight) * (1f - mFlyBrakeWeight), 0.01f);
				}
				if (animation[mFlightForwardAnim.GlideAnim[0]] != null)
				{
					animation.Blend(mFlightForwardAnim.GlideAnim[0], num5 * num4 * (1f - mFlyAccelWeight) * (1f - mFlyBrakeWeight), 0.01f);
				}
				if (animation[mFlightForwardAnim.FlapAnim[1]] != null)
				{
					animation.Blend(mFlightForwardAnim.FlapAnim[1], num5 * mFlyAccelWeight * (1f - mFlyBrakeWeight), 0.01f);
				}
				if (animation[mFlightForwardAnim.GlideAnim[1]] != null)
				{
					animation.Blend(mFlightForwardAnim.GlideAnim[1], num5 * mFlyBrakeWeight);
				}
				if (animation[mFlightRollAnim.FlapAnim[0]] != null)
				{
					animation.Blend(mFlightRollAnim.FlapAnim[0], vector.x * num6 * num8 * num3, 0.01f);
				}
				if (animation[mFlightRollAnim.GlideAnim[0]] != null)
				{
					animation.Blend(mFlightRollAnim.GlideAnim[0], vector.x * num6 * num8 * num4, 0.01f);
				}
				if (animation[mFlightRollAnim.FlapAnim[1]] != null)
				{
					animation.Blend(mFlightRollAnim.FlapAnim[1], (0f - vector.x) * num6 * (1f - num8) * num3, 0.01f);
				}
				if (animation[mFlightRollAnim.GlideAnim[1]] != null)
				{
					animation.Blend(mFlightRollAnim.GlideAnim[1], (0f - vector.x) * num6 * (1f - num8) * num4, 0.01f);
				}
				if (animation[mFlightPitchAnim.FlapAnim[0]] != null)
				{
					animation.Blend(mFlightPitchAnim.FlapAnim[0], (0f - vector.y) * num7 * (1f - num9), 0.01f);
				}
				if (animation[mFlightPitchAnim.GlideAnim[0]] != null)
				{
					animation.Blend(mFlightPitchAnim.GlideAnim[0], 0f);
				}
				if (animation[mFlightPitchAnim.FlapAnim[1]] != null)
				{
					animation.Blend(mFlightPitchAnim.FlapAnim[1], vector.y * num7 * num9, 0.01f);
				}
				if (animation[mFlightPitchAnim.GlideAnim[1]] != null)
				{
					animation.Blend(mFlightPitchAnim.GlideAnim[1], 0f);
				}
				if (mPrevAnimState != 0)
				{
					foreach (AnimationState item in animation)
					{
						if (item.layer == 0)
						{
							base.animation.Blend(item.name, 0f, _CrossFadeTime);
						}
						if (item.layer == 1)
						{
							item.enabled = true;
							item.wrapMode = WrapMode.Loop;
						}
						if ((bool)animation[_AttackAnim])
						{
							animation[_AttackAnim].wrapMode = WrapMode.Once;
						}
						if ((bool)animation[_FlyAttackAnim])
						{
							animation[_FlyAttackAnim].wrapMode = WrapMode.Once;
						}
					}
					mPrevAnimState = 0;
				}
			}
			else
			{
				if (mPrevAnimState != 1)
				{
					foreach (AnimationState item2 in animation)
					{
						if (item2.layer == 1)
						{
							animation.Blend(item2.name, 0f, _CrossFadeTime);
						}
					}
				}
				if (pIsMounted && mAvatarController != null)
				{
					if (mCurrentSkillType == PetSpecialSkillType.FLY || mAvatarController.pState == AvAvatarState.FALLING)
					{
						text = (flag ? "Jump01" : "FlyIdle");
					}
					else if (num > 3f)
					{
						text = _MountRunAnim;
					}
					else if (num > 0.1f)
					{
						text = "Walk";
					}
				}
				else if (num > 4f)
				{
					text = _MountRunAnim;
				}
				else if (num > 0.025f)
				{
					text = "Walk";
				}
				if (text == GetIdleAnimationName() && num2 > Mathf.Sin(0.0017453292f))
				{
					text = "Walk";
				}
				if (IsSwimming())
				{
					text = ((num > 0.1f) ? _AnimNameSwim : (_AnimNameSwim + "Idle"));
				}
				if ((bool)animation[text] && (!animation.IsPlaying(text) || mPrevAnimState != 1))
				{
					animation.CrossFade(text, _CrossFadeTime);
					animation[text].wrapMode = WrapMode.Loop;
					if (_SoundMapper != null)
					{
						PlayAnimSFX(text, looping: false);
					}
				}
				mPrevAnimState = 1;
			}
		}
		if (IsSwimming() && pIsMounted && mAvatarController != null && mAvatarController.pSubState != AvAvatarSubState.SWIMMING)
		{
			mWaterObject = null;
			ApplySwim(apply: false);
		}
	}

	private void UpdateAttackAnimState()
	{
		UpdateAnimStateToAdditive(_AttackAnim);
		UpdateAnimStateToAdditive(_FlyAttackAnim);
	}

	private void UpdateAnimStateToAdditive(string inAnim)
	{
		AnimationState animationState = base.animation[inAnim];
		if (animationState != null)
		{
			animationState.blendMode = AnimationBlendMode.Additive;
			animationState.wrapMode = WrapMode.Once;
			animationState.layer = 1;
			animationState.time = 0f;
		}
	}

	public void PlayAdditiveAnimation(string inAnim)
	{
		AnimationState animationState = base.animation[inAnim];
		if (animationState != null)
		{
			animationState.blendMode = AnimationBlendMode.Additive;
			animationState.wrapMode = WrapMode.Once;
			animationState.layer = 1;
			animationState.enabled = true;
			animationState.weight = 1f;
			animationState.time = 0f;
		}
	}

	public SanctuaryPetTypeInfo GetTypeInfo()
	{
		return mTypeInfo;
	}

	public SanctuaryPetTypeSettings GetTypeSettings()
	{
		return mTypeSettings;
	}

	public void SetTypeInfo(SanctuaryPetTypeInfo pti, bool updateMeter = true)
	{
		mTypeInfo = pti;
		if (pti == null)
		{
			return;
		}
		mTypeSettings = SanctuaryData.GetSanctuaryPetSettings(mTypeInfo._Settings);
		SetAge(RaisedPetData.GetAgeIndex(pData.pStage), save: false, resetSkills: false);
		if (_ActionMeterData == null || _ActionMeterData.Length == 0)
		{
			_ActionMeterData = mTypeSettings._ActionMeterData;
		}
		mMeterInstances = new SanctuaryPetMeterInstance[mTypeSettings._Meters.Length];
		for (int i = 0; i < mTypeSettings._Meters.Length; i++)
		{
			mMeterInstances[i] = new SanctuaryPetMeterInstance();
			mMeterInstances[i].mMeterTypeInfo = mTypeSettings._Meters[i];
			mMeterInstances[i].mMeterValData = pData.FindStateData(mTypeSettings._Meters[i]._Type.ToString());
			if (mMeterInstances[i].mMeterValData == null)
			{
				float maxMeter = SanctuaryData.GetMaxMeter(mTypeSettings._Meters[i]._Type, pData);
				mMeterInstances[i].mMeterValData = pData.SetStateData(mTypeSettings._Meters[i]._Type.ToString(), maxMeter);
			}
		}
		if (updateMeter)
		{
			UpdateAllMeters();
		}
		if (_SoundMapper == null || _SoundMapper.Length == 0)
		{
			_SoundMapper = pti._SoundMapper;
		}
	}

	public void SetMeterUI(UiPetMeter ui)
	{
		pUiPetMeter = ui;
	}

	public bool GrowToNextAge()
	{
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(pData.PetTypeID);
		if (pAge == sanctuaryPetTypeInfo._AgeData.Length - 1)
		{
			UtDebug.LogError("Pet growing :: Already reached max age");
			return false;
		}
		int ageIndex = RaisedPetData.GetAgeIndex(pData.pStage);
		ageIndex++;
		if (pData.pStage == RaisedPetStage.BABY)
		{
			ageIndex++;
		}
		UtDebug.Log("Pet growing to " + ageIndex, 32u);
		SetAge(ageIndex, save: true, resetSkills: true);
		return true;
	}

	public void DoWakeUp()
	{
		pData.SetSleepMode(issleep: false, savedata: true);
		if (pUiPetMeter != null)
		{
			pUiPetMeter.SetVisibility(inVisible: true);
		}
	}

	public void DoSleep()
	{
		pData.SetSleepMode(issleep: true, savedata: true);
	}

	public string GetWakeMsg(out AudioClip vo)
	{
		vo = mTypeInfo._WakeMsgVO;
		return StringTable.GetStringData(mTypeInfo._WakeupText._ID, mTypeInfo._WakeupText._Text);
	}

	public string GetSleepMsg(out AudioClip vo)
	{
		int num = UnityEngine.Random.Range(0, mTypeInfo._SleepText.Length);
		if (mTypeInfo._SleepMsgVO.Length > num)
		{
			vo = mTypeInfo._SleepMsgVO[num];
		}
		else
		{
			vo = null;
		}
		return StringTable.GetStringData(mTypeInfo._SleepText[num]._ID, mTypeInfo._SleepText[num]._Text);
	}

	public string GetGrowthMsg(out AudioClip vo)
	{
		vo = pCurAgeData._GrowVO;
		return StringTable.GetStringData(pCurAgeData._GrowthText._ID, pCurAgeData._GrowthText._Text);
	}

	public void CheckForTaskCompletion(PetActions pa, string inItemName = "")
	{
		string skillName = GetSkillName(pa);
		if (MissionManager.pInstance != null && (MyRoomsIntLevel.pInstance == null || !MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt()))
		{
			MissionManager.pInstance.CheckForTaskCompletion("Action", "Pet" + skillName.ToLower(), inItemName);
		}
	}

	public bool CheckForSkillCompleted()
	{
		if (pCurAgeData == null || mTypeInfo == null)
		{
			return false;
		}
		UtDebug.Log("Check pet skills, needed = " + pCurAgeData._MinTotalSkillLevelToNextAge, 32u);
		if (pData.pStage == RaisedPetStage.ADULT)
		{
			return false;
		}
		mSkillLevel = 0f;
		string text = "";
		bool flag = true;
		float num = 0f;
		float num2 = 0f;
		PetSkillRequirements[] skillsRequired = pCurAgeData._SkillsRequired;
		foreach (PetSkillRequirements petSkillRequirements in skillsRequired)
		{
			float skillLevel = GetSkillLevel(petSkillRequirements._Skill);
			text = text + "---- Checking " + petSkillRequirements._Skill.ToString() + " = " + skillLevel + "\n";
			if (skillLevel >= petSkillRequirements._Level)
			{
				mSkillLevel += skillLevel;
				continue;
			}
			flag = false;
			num += skillLevel;
			num2 += petSkillRequirements._Level;
		}
		UtDebug.Log(text, 32u);
		float minTotalSkillLevelToNextAge = pCurAgeData._MinTotalSkillLevelToNextAge;
		if (!flag)
		{
			if (mSkillLevel >= minTotalSkillLevelToNextAge - num2)
			{
				mSkillLevel = minTotalSkillLevelToNextAge - num2 + num;
			}
			return false;
		}
		if (mSkillLevel >= minTotalSkillLevelToNextAge)
		{
			UtDebug.Log("Check pet skills Met total = " + mSkillLevel, 32u);
			return true;
		}
		UtDebug.Log("Check pet skills NOT MET total = " + mSkillLevel, 32u);
		return false;
	}

	public float GetSkillLevel(PetSkills skill)
	{
		return pData.GetSkillLevel(skill.ToString());
	}

	public string GetSkillName(PetActions actionID)
	{
		return actionID switch
		{
			PetActions.CHEWTOY => PetSkills.PLAY.ToString(), 
			PetActions.FETCHBALL => PetSkills.PLAY.ToString(), 
			PetActions.TOW => PetSkills.PLAY.ToString(), 
			PetActions.BRUSH => PetSkills.PLAY.ToString(), 
			PetActions.BATH => PetSkills.BATHE.ToString(), 
			PetActions.SLEEP => PetSkills.SLEEP.ToString(), 
			PetActions.FOLLOWLASER => PetSkills.PLAY.ToString(), 
			PetActions.SHOOTFIRE => PetActions.SHOOTFIRE.ToString(), 
			PetActions.EAT => PetSkills.EAT.ToString(), 
			PetActions.FLIGHTSCHOOL => PetActions.FLIGHTSCHOOL.ToString(), 
			PetActions.TARGETPRACTICE => PetActions.TARGETPRACTICE.ToString(), 
			PetActions.RACING => PetActions.RACING.ToString(), 
			PetActions.LAB => PetActions.LAB.ToString(), 
			PetActions.EELBLAST => PetSkills.PLAY.ToString(), 
			PetActions.ARENAFRENZY => PetActions.ARENAFRENZY.ToString(), 
			_ => "", 
		};
	}

	private SanctuaryPetMeterType GetMeterTypeByKey(string inKey)
	{
		return (SanctuaryPetMeterType)Enum.Parse(typeof(SanctuaryPetMeterType), inKey, ignoreCase: true);
	}

	public void UpdateActionMeters(PetActions actionID, float percentage, bool doUpdateSkill, bool doSave = true)
	{
		bool flag = true;
		bool flag2 = false;
		if (doUpdateSkill)
		{
			string skillName = GetSkillName(actionID);
			if (skillName != "")
			{
				RaisedPetSkill skillData = pData.GetSkillData(skillName);
				float newval = 1f;
				if (skillData != null)
				{
					DateTime updateDate = skillData.UpdateDate;
					DateTime dateTime = new DateTime(updateDate.Year, updateDate.Month, updateDate.Day, updateDate.Hour, updateDate.Minute, updateDate.Second, DateTimeKind.Utc);
					if (ServerTime.pCurrentTime.Date > dateTime.Date || pSkillNoDateCheck || skillData.Value == 0f)
					{
						newval = skillData.Value + 1f;
					}
					else
					{
						UtDebug.Log(skillName + " already updated today :" + skillData.UpdateDate.Date, 32u);
						flag = false;
					}
				}
				if (flag)
				{
					pData.UpdateSkillData(skillName, newval, save: false);
				}
			}
		}
		if (mFoodItemData != null)
		{
			string[] obj = new string[3] { "Hunger", "Energy", "Happiness" };
			string text = SanctuaryData.FindSanctuaryPetTypeInfo(SanctuaryManager.pCurPetData.PetTypeID)._Name;
			string[] array = obj;
			foreach (string text2 in array)
			{
				string attribute = text + text2;
				if (mFoodItemData.HasAttribute(attribute) || mFoodItemData.HasAttribute(text2))
				{
					float attribute2 = mFoodItemData.GetAttribute(text2, 0f);
					attribute2 = mFoodItemData.GetAttribute(attribute, attribute2);
					SanctuaryPetMeterType meterTypeByKey = GetMeterTypeByKey(text2);
					UpdateMeter(meterTypeByKey, attribute2);
					flag2 = true;
				}
			}
		}
		mFoodItemData = null;
		if (!flag2)
		{
			PetMeterActionData[] actionMeterData = _ActionMeterData;
			foreach (PetMeterActionData petMeterActionData in actionMeterData)
			{
				if (petMeterActionData._ID == actionID && (!pData.pFoodEffect.ContainsKey((int)petMeterActionData._MeterType) || (pData.pFoodEffect.ContainsKey((int)petMeterActionData._MeterType) && petMeterActionData._Delta > 0f)))
				{
					UpdateMeter(petMeterActionData._MeterType, petMeterActionData._Delta * percentage);
					flag2 = true;
				}
			}
		}
		if (pData.IsSelected)
		{
			PetActionAchievement[] petActionAchievement = SanctuaryData.pInstance._PetActionAchievement;
			foreach (PetActionAchievement petActionAchievement2 in petActionAchievement)
			{
				if (petActionAchievement2._Action == actionID && (!petActionAchievement2._PetMeterCap || !IsMeterFull(petActionAchievement2._MeterType)))
				{
					WsWebService.SetUserAchievementAndGetReward(petActionAchievement2._AchievementID, AchievementEventHandler, null);
				}
			}
		}
		if (_MessageObject != null)
		{
			_MessageObject.SendMessage("OnUpdateActionMeters", actionID, SendMessageOptions.DontRequireReceiver);
		}
		if (doSave && ((doUpdateSkill && flag) || flag2))
		{
			SaveData();
		}
	}

	public void ItemCollected()
	{
		if (SanctuaryManager.pInstance._PetCollectItemAchievementID > 0)
		{
			WsWebService.SetUserAchievementAndGetReward(SanctuaryManager.pInstance._PetCollectItemAchievementID, AchievementEventHandler, null);
		}
	}

	private void AchievementEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.ERROR:
			UtDebug.LogError("!!!" + inType.ToString() + " failed!!!!");
			break;
		case WsServiceEvent.COMPLETE:
			if (inObject != null)
			{
				GameUtilities.AddRewards((AchievementReward[])inObject, inUseRewardManager: false, inImmediateShow: false);
			}
			else
			{
				UtDebug.LogError("!!!" + inType.ToString() + " did not return valid object!!!!");
			}
			break;
		}
	}

	public override string GetFlutterAnim()
	{
		return mFlySkillLevel switch
		{
			0 => "Flutter01", 
			1 => "Flutter01", 
			_ => "Flutter02", 
		};
	}

	public override string GetFlyAnim()
	{
		return mFlySkillLevel switch
		{
			0 => "FlyIdle", 
			1 => "FlyIdle", 
			_ => "FlyIdle", 
		};
	}

	public virtual string GetBreathAttackAnimName()
	{
		float skillLevel = GetSkillLevel(PetSkills.FIRE);
		if (skillLevel == 0f)
		{
			return "BreatheSmokeClip";
		}
		if (skillLevel == 1f)
		{
			return "BreatheSmokeFire02Clip";
		}
		if (skillLevel == 2f && !pCanBreatheAttackNow)
		{
			return "BreatheSmokeFire02Clip";
		}
		return "BreatheFireClip";
	}

	public virtual PetSkills GetBreathAttackSkill()
	{
		return PetSkills.FIRE;
	}

	public int GetBreathAttackSkillLevel()
	{
		return (int)GetSkillLevel(GetBreathAttackSkill());
	}

	public virtual void SetBreathAttackParticleColor(Color c)
	{
		SetParticleColor("PfPrtFireDragon", c, 3);
		SetParticleColor("PfPrtSmokeDragon", c, 3);
	}

	public override void TweenOutDone()
	{
		base.TweenOutDone();
		foreach (LookAtHeadBoneData mHeadBonesLookAtDatum in mHeadBonesLookAtData)
		{
			if (mHeadBonesLookAtDatum != null && mHeadBonesLookAtDatum.mHeadBone != null)
			{
				mHeadBonesLookAtDatum.mHeadBone.localRotation = mHeadBonesLookAtDatum.mOriginalRotation;
			}
		}
	}

	public override void SetHeadBoneLookAt(LookAtHeadBoneData headBoneData, Vector3 pos, Vector3 upv)
	{
		pos += _Head_LookAt_Offset;
		headBoneData.mHeadBone.localRotation = headBoneData.mOldHeadBoneRotation;
		headBoneData.mHeadBone.LookAt(pos, upv);
	}

	public override void ActionDone(Character_Action actionid, bool ended, Transform actObj)
	{
		switch (actionid)
		{
		case Character_Action.userAction8:
			if (actObj != null)
			{
				UnityEngine.Object.Destroy(actObj.gameObject);
			}
			break;
		case Character_Action.eat:
			if (mChewToy)
			{
				if (_ActionDoneMessageObject != null)
				{
					_ActionDoneMessageObject.SendMessage("OnActionDone", PetActions.CHEWTOY);
				}
				if (actObj != null)
				{
					UnityEngine.Object.Destroy(actObj.gameObject);
				}
			}
			else
			{
				if (mFoodItemData != null)
				{
					StartPowerUp(GetPowerUpType(mFoodItemData), GetAttributes(mFoodItemData));
				}
				if (actObj != null)
				{
					UnityEngine.Object.Destroy(actObj.gameObject);
				}
			}
			break;
		}
	}

	protected override bool CheckPetting()
	{
		bool num = base.CheckPetting();
		if (!num)
		{
			if (mMouseOverPettingParts)
			{
				UICursorManager.SetCursor("Activate", showHideSystemCursor: true);
				return num;
			}
			if (ObClickable.pMouseOverObject == null)
			{
				UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			}
		}
		return num;
	}

	public override void RepeatAction()
	{
		base.RepeatAction();
		if (mAction == Character_Action.userAction1)
		{
			if (!Input.GetMouseButton(0))
			{
				SetState(Character_State.idle);
				StopLookAtObject();
				return;
			}
			mTOWTimer = 2f;
			mTOWPlayTime += Time.deltaTime;
			Vector3 position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1.2f);
			position = mCamera.GetComponent<Camera>().ScreenToWorldPoint(position);
			position.y += 0.2f;
			SetLookAt(position, tween: true);
		}
	}

	private string GetPettingAnimName(string partname)
	{
		if (_CustomPettingAnim != null)
		{
			return _CustomPettingAnim;
		}
		if (mPettingDown)
		{
			return _PettingLieDownAnim;
		}
		int num = 0;
		for (num = 0; num < mPettingBones.Length; num++)
		{
			if (partname == mPettingBones[num].name)
			{
				return _PettingPartAnim[num];
			}
		}
		return "";
	}

	public bool DoBrushPetting(string partname)
	{
		string pettingAnimName = GetPettingAnimName(partname);
		if (pettingAnimName.Length == 0)
		{
			return false;
		}
		if (!base.animation.IsPlaying(pettingAnimName))
		{
			StopLookAtObject();
			PlayAnimation(pettingAnimName, WrapMode.Loop, 1f, _CrossFadeTime);
		}
		return true;
	}

	public void DoTOW(Transform obj)
	{
		DoAction(obj, Character_Action.userAction1);
		PlayAnim("TugOWar", -1, 1f, 0);
	}

	protected override bool DoPettingAction(string partname)
	{
		if (mBrushObj != null)
		{
			return DoBrushPetting(partname);
		}
		if (mTOWObj != null)
		{
			DoTOW(mTOWObj);
			return true;
		}
		return DoBrushPetting(partname);
	}

	public void SetBrush(Transform obj)
	{
		mBrushObj = obj;
	}

	public virtual string GetTargetAttackPrtName()
	{
		return "PfPrtDragonTargetFire";
	}

	public virtual void AttachTOW(GameObject obj)
	{
		obj.transform.parent = FindBoneTransform(_Bone_Nose);
		ApplyPetToyOffset(obj.transform, PetToyType.TOW);
	}

	public bool SetTOW(GameObject obj)
	{
		if (obj != null && HasMood(Character_Mood.angry))
		{
			obj.SetActive(value: false);
			DoAction(obj.transform, Character_Action.userAction8);
			PlayAnim("Refuse", 0, 1f, 1);
			return false;
		}
		if (obj != null)
		{
			obj.layer = LayerMask.NameToLayer("IgnoreGroundRay");
			AttachTOW(obj);
			mTOWTimer = 10f;
			mTOWObj = obj.transform;
			mPettingBones[2] = mTOWObj.gameObject;
		}
		else if (mTOWObj != null)
		{
			mTOWObj.parent = null;
			UnityEngine.Object.Destroy(mTOWObj.gameObject);
			mTOWTimer = 0f;
			if (mTOWPlayTime >= 3f)
			{
				UpdateActionMeters(PetActions.TOW, 1f, doUpdateSkill: true);
			}
			mTOWPlayTime = 0f;
			mTOWObj = null;
			mPettingBones[2] = null;
			if (_ActionDoneMessageObject != null)
			{
				_ActionDoneMessageObject.SendMessage("OnActionDone", PetActions.TOW);
			}
		}
		return true;
	}

	private void ResetEatState()
	{
		mChewToy = false;
	}

	public virtual void AttachChewToy(GameObject obj)
	{
		obj.transform.parent = FindBoneTransform(_Bone_Nose);
		ApplyPetToyOffset(obj.transform, PetToyType.CHEWTOY);
	}

	public bool DoChewToy(GameObject obj)
	{
		ResetEatState();
		if (HasMood(Character_Mood.angry))
		{
			obj.SetActive(value: false);
			DoAction(obj.transform, Character_Action.userAction8);
			PlayAnim("Refuse", 0, 1f, 1);
			return false;
		}
		mChewToy = true;
		AttachChewToy(obj);
		DoEat(obj.transform);
		if (UnityEngine.Random.value > 0.5f)
		{
			PlayAnim("ShakeHeadChewToy01", 1, 1f, 1);
		}
		else
		{
			PlayAnim("ShakeHeadChewToy02", 1, 1f, 1);
		}
		return true;
	}

	public bool DoEat(ItemData inItemData)
	{
		CheckForTaskCompletion(PetActions.EAT, inItemData.ItemID.ToString());
		mFoodItemData = inItemData;
		UpdateActionMeters(PetActions.EAT, 1f, doUpdateSkill: true);
		return true;
	}

	private void SetFoodOffset(GameObject inObject, PetToyType inType)
	{
		SanctuaryPetToyOffset petToyOffset = GetPetToyOffset(inType);
		if (petToyOffset != null)
		{
			inObject.transform.localPosition = mDragonTransform.TransformPoint(petToyOffset._PositionOffset);
			inObject.transform.localScale = petToyOffset._Scale;
			Quaternion localRotation = Quaternion.Euler(petToyOffset._Rotation + base.transform.localRotation.eulerAngles);
			inObject.transform.localRotation = localRotation;
		}
	}

	public bool IsActionAllowed(PetActions inActionID)
	{
		PetMeterActionData[] actionMeterData = _ActionMeterData;
		foreach (PetMeterActionData petMeterActionData in actionMeterData)
		{
			if (petMeterActionData._ID == inActionID && petMeterActionData._Delta < 0f && GetMeterValue(petMeterActionData._MeterType) < 0f - petMeterActionData._Delta)
			{
				return false;
			}
		}
		return true;
	}

	public virtual string GetEatBerryAnim()
	{
		if (UnityEngine.Random.Range(0, 2) == 0)
		{
			return "EatFireflyStanding01";
		}
		return "EatFireflyStanding02";
	}

	public bool DoEatFood(GameObject obj, ItemData inItemData)
	{
		ResetEatState();
		SetFoodOffset(obj, PetToyType.FOOD);
		if (HasMood(Character_Mood.full))
		{
			DoAction(obj.transform, Character_Action.userAction8);
			PlayAnim("Refuse", 0, 1f, 1);
			return false;
		}
		if (RemovePowerUpScale())
		{
			SetFoodOffset(obj, PetToyType.FOOD);
		}
		DoEat(obj.transform);
		mFoodItemData = inItemData;
		if (pData.pStage == RaisedPetStage.EGGINHAND)
		{
			PlayAnim("Feed");
		}
		else
		{
			PlayAnim("EatFoodBowl", 2, 1f, 1);
		}
		return true;
	}

	private SanctuaryPowerUpType GetPowerUpType(ItemData inItemData)
	{
		SanctuaryPowerUpType result = SanctuaryPowerUpType.NONE;
		if (inItemData != null)
		{
			result = (SanctuaryPowerUpType)inItemData.GetAttribute("Effect", 0);
		}
		return result;
	}

	private string GetAttributes(ItemData inItemData)
	{
		if (inItemData != null)
		{
			return string.Concat(inItemData.GetAttribute("Duration", "-1") + "/", inItemData.GetAttribute("Scale", "-1"));
		}
		return null;
	}

	public void DoDrop(GameObject obj)
	{
		obj.transform.parent = null;
		Rigidbody component = obj.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = false;
		}
	}

	public virtual void AttachPickup(GameObject obj, PetToyType toyType)
	{
		obj.transform.parent = FindBoneTransform(_Bone_Nose);
		ApplyPetToyOffset(obj.transform, toyType);
	}

	public void DoPickUp(GameObject obj)
	{
		DoPickUp(obj, PetToyType.BALL);
	}

	public void DoPickUp(GameObject obj, PetToyType toyType)
	{
		AttachPickup(obj, toyType);
		Rigidbody component = obj.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = true;
		}
	}

	public virtual void AttachAccessory(GameObject obj, string bname, Vector3 pos, Vector3 scale, Quaternion q)
	{
		obj.transform.parent = FindBoneTransform(bname);
		obj.transform.localPosition = pos;
		obj.transform.localScale = scale;
		obj.transform.localRotation = q;
	}

	public virtual void SetAccessory(RaisedPetAccType atype, GameObject obj, Texture2D newTexture)
	{
		switch (atype)
		{
		case RaisedPetAccType.Hat:
		{
			GameObject accessoryObject2 = GetAccessoryObject(atype);
			if (accessoryObject2 != null)
			{
				accessoryObject2.transform.parent = null;
				accessoryObject2.transform.position = new Vector3(0f, -5000f, 0f);
				mAccObjects.Remove(atype);
			}
			accessoryObject2 = obj;
			if (accessoryObject2 != null)
			{
				mAccObjects.Add(atype, accessoryObject2);
				AttachAccessory(accessoryObject2, GetHeadBoneName(), Vector3.zero, Vector3.one, Quaternion.identity);
				UtDebug.Log("Applying hat " + accessoryObject2.name, 32u);
			}
			else
			{
				UtDebug.Log("Applying hat = null", 32u);
			}
			break;
		}
		case RaisedPetAccType.Saddle:
		{
			Transform transform = FindBoneTransform("Saddle");
			SkinnedMeshRenderer skinnedMeshRenderer = null;
			if (transform != null)
			{
				skinnedMeshRenderer = transform.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
			}
			if (obj != null)
			{
				DisableGameObjectsOnMountOrSaddle(disable: true);
				if (skinnedMeshRenderer != null)
				{
					SkinnedMeshRenderer componentInChildren2 = obj.GetComponentInChildren<SkinnedMeshRenderer>();
					if (componentInChildren2 != null && (bool)componentInChildren2.sharedMesh)
					{
						skinnedMeshRenderer.sharedMesh = componentInChildren2.sharedMesh;
					}
				}
				if (newTexture != null && transform != null)
				{
					UtUtilities.SetObjectTexture(transform.gameObject, 0, newTexture);
				}
				UtDebug.Log("Applying Saddle " + obj.name, 32u);
				obj.SetActive(value: false);
				UnityEngine.Object.Destroy(obj);
			}
			else
			{
				if (skinnedMeshRenderer != null)
				{
					skinnedMeshRenderer.sharedMesh = null;
				}
				DisableGameObjectsOnMountOrSaddle(SanctuaryManager.pMountedState);
				UtDebug.Log("Applying Saddle = null", 32u);
			}
			break;
		}
		case RaisedPetAccType.Texture:
			if (newTexture != null)
			{
				mAccObjects.Remove(atype);
				mAccObjects.Add(atype, newTexture);
				UtDebug.Log("Applying texture " + newTexture.name, 32u);
				if (mOriginalTexture == null)
				{
					if (pData.pTexture != null)
					{
						mOriginalTexture = pData.pTexture;
					}
					else
					{
						int materialIndex = GetMaterialIndex();
						Renderer componentInChildren = base.gameObject.GetComponentInChildren<Renderer>();
						if (componentInChildren.materials[materialIndex].HasProperty("_Skin"))
						{
							mOriginalTexture = componentInChildren.materials[materialIndex].GetTexture("_Skin");
						}
						else
						{
							mOriginalTexture = componentInChildren.materials[materialIndex].mainTexture;
						}
					}
				}
				pData.pTexture = newTexture;
				pData.pTextureBMP = null;
				ApplyAccessoryTexture(base.gameObject, pData.pTexture);
			}
			else
			{
				UtDebug.Log("Removing texture", 32u);
				pData.pTexture = null;
				pData.pTextureBMP = null;
				InitTextureFromData();
				if (mOriginalTexture != null)
				{
					ApplyAccessoryTexture(base.gameObject, (Texture2D)mOriginalTexture);
				}
			}
			break;
		case RaisedPetAccType.BumpMap:
			if (newTexture != null)
			{
				mAccObjects.Remove(atype);
				mAccObjects.Add(atype, newTexture);
				UtDebug.Log("Applying Bump Map texture " + newTexture.name, 32u);
				ApplyAccessoryBumpMapTexture(base.gameObject, newTexture);
			}
			else if (mOriginalBumpMapTexture != null)
			{
				ApplyAccessoryBumpMapTexture(base.gameObject, mOriginalBumpMapTexture);
			}
			break;
		case RaisedPetAccType.PatternTexture:
			if (newTexture != null)
			{
				UtDebug.Log("Applying Pattern texture " + newTexture.name, 32u);
				pData.pTexture = newTexture;
				pData.pTextureBMP = null;
				ApplyAccessoryPattern(base.gameObject, pData.pTexture);
				break;
			}
			UtDebug.Log("Removing texture", 32u);
			pData.pTexture = null;
			pData.pTextureBMP = null;
			InitTextureFromData();
			if (mOriginalTexture != null)
			{
				ApplyAccessoryPattern(base.gameObject, (Texture2D)mOriginalTexture);
			}
			break;
		case RaisedPetAccType.Materials:
		{
			GameObject accessoryObject = GetAccessoryObject(atype);
			if (accessoryObject != null)
			{
				UnityEngine.Object.Destroy(accessoryObject);
			}
			mAccObjects.Remove(atype);
			accessoryObject = obj;
			if (accessoryObject != null)
			{
				mAccObjects.Add(atype, accessoryObject);
			}
			else
			{
				UtDebug.Log("Accessory materials is null", 32u);
			}
			UpdateMaterials(accessoryObject);
			break;
		}
		}
		if (!pIsGlowDisabled)
		{
			RefreshGlowEffect();
		}
	}

	private void ApplyAccessoryTexture(GameObject obj, Texture t)
	{
		Material[] materials = obj.GetComponentInChildren<Renderer>().materials;
		foreach (Material material in materials)
		{
			if (material.HasProperty("_Skin"))
			{
				material.SetTexture("_Skin", t);
			}
			else
			{
				material.mainTexture = t;
			}
		}
	}

	public void ApplyAccessoryPattern(GameObject obj, Texture t)
	{
		if (!UtPlatform.IsMobile())
		{
			int materialIndex = GetMaterialIndex();
			Renderer componentInChildren = obj.GetComponentInChildren<Renderer>();
			if (componentInChildren.materials[materialIndex].HasProperty("_PatternTexture"))
			{
				componentInChildren.materials[materialIndex].SetTexture("_PatternTexture", t);
			}
			else
			{
				componentInChildren.materials[materialIndex].mainTexture = t;
			}
		}
	}

	private void ApplyAccessoryBumpMapTexture(GameObject obj, Texture t)
	{
		Material[] materials = obj.GetComponentInChildren<Renderer>().materials;
		foreach (Material material in materials)
		{
			if (material.HasProperty("_BumpMap"))
			{
				if (mOriginalBumpMapTexture == null)
				{
					mOriginalBumpMapTexture = material.GetTexture("_BumpMap");
				}
				material.SetTexture("_BumpMap", t);
			}
		}
	}

	public virtual GameObject GetAccessoryObject(RaisedPetAccType atype)
	{
		if ((atype == RaisedPetAccType.Hat || atype == RaisedPetAccType.Saddle || atype == RaisedPetAccType.Materials) && mAccObjects.ContainsKey(atype))
		{
			return (GameObject)mAccObjects[atype];
		}
		return null;
	}

	public Texture2D GetAccessoryTexture(RaisedPetAccType atype)
	{
		if ((atype == RaisedPetAccType.Texture || atype == RaisedPetAccType.BumpMap) && mAccObjects.ContainsKey(atype))
		{
			return (Texture2D)mAccObjects[atype];
		}
		return null;
	}

	public void OnAccReady(SanctuaryPetAccData adata)
	{
		if (this != null)
		{
			SetAccessory(RaisedPetData.GetAccessoryType(adata.mAccData.Type), adata.mObj, adata.mTex);
		}
	}

	private SanctuaryPetToyOffset GetPetToyOffset(PetToyType toyType)
	{
		for (int i = 0; i < pCurAgeData._PetToyOffset.Length; i++)
		{
			if (pCurAgeData._PetToyOffset[i]._PetToyType == toyType)
			{
				return pCurAgeData._PetToyOffset[i];
			}
		}
		Debug.LogError("Could not find PetToy offset : " + mTypeInfo._Name + ", " + pCurAgeData._Name + ", " + toyType);
		return null;
	}

	private void ApplyPetToyOffset(Transform trans, PetToyType toyType)
	{
		if (!(trans == null))
		{
			SanctuaryPetToyOffset petToyOffset = GetPetToyOffset(toyType);
			if (petToyOffset == null)
			{
				trans.localPosition = Vector3.zero;
				trans.localRotation = Quaternion.identity;
				trans.localScale = Vector3.one;
			}
			else
			{
				trans.localPosition = petToyOffset._PositionOffset;
				trans.localScale = petToyOffset._Scale;
				Quaternion localRotation = Quaternion.Euler(petToyOffset._Rotation);
				trans.localRotation = localRotation;
			}
		}
	}

	public void ProcessPowerUpData(string inData)
	{
		if (inData.Length > 0)
		{
			string[] array = inData.Split('_');
			if (array.Length > 2 && int.TryParse(array[1], out var result))
			{
				StartPowerUp((SanctuaryPowerUpType)result, array[2]);
			}
		}
	}

	public void StartPowerUp(SanctuaryPowerUpType inType, string inData)
	{
		if (mPowerUpType == inType)
		{
			return;
		}
		EndPowerUp();
		mPowerUpType = inType;
		GameObject rideObject = GetRideObject();
		if (rideObject != null)
		{
			bool flag = rideObject != AvAvatar.pObject;
			if (!flag && MainStreetMMOClient.pInstance != null)
			{
				string text = (pIsMounted ? "Y" : "N");
				MainStreetMMOClient pInstance = MainStreetMMOClient.pInstance;
				RaisedPetData raisedPetData = pData;
				string[] obj = new string[5] { text, "_", null, null, null };
				int num = (int)mPowerUpType;
				obj[2] = num.ToString();
				obj[3] = "_";
				obj[4] = inData;
				pInstance.SetRaisedPetString(raisedPetData.SaveToResStringEx(string.Concat(obj)));
			}
			SanctuaryPowerUpData powerUp = GetPowerUp(inType);
			if (powerUp != null && powerUp._Duration > 0)
			{
				mPowerUpTimer = powerUp._Duration;
				switch (mPowerUpType)
				{
				case SanctuaryPowerUpType.SHRINK:
					ObScale.Set(rideObject, powerUp._Value, 2f, inStartScale: true, inRequireOnScaleUpdate: true)._RequireOnScaleUpdate = false;
					break;
				case SanctuaryPowerUpType.SPARKLE:
					if (powerUp._PrtEffectURL.Length > 0)
					{
						string[] array2 = powerUp._PrtEffectURL.Split('/');
						RsResourceManager.LoadAssetFromBundle(array2[0] + "/" + array2[1], array2[2], OnPowerUpPrtEffectEventHandler, typeof(GameObject));
					}
					break;
				case SanctuaryPowerUpType.SPEED:
					if (!flag)
					{
						AvAvatarStateData[] stateData = rideObject.GetComponent<AvAvatarController>()._StateData;
						foreach (AvAvatarStateData avAvatarStateData in stateData)
						{
							avAvatarStateData._MaxForwardSpeed += avAvatarStateData._MaxForwardSpeed * powerUp._Value;
							avAvatarStateData._MaxBackwardSpeed += avAvatarStateData._MaxBackwardSpeed * powerUp._Value;
							avAvatarStateData._MaxAirSpeed += avAvatarStateData._MaxAirSpeed * powerUp._Value;
						}
					}
					if (powerUp._PrtEffectURL.Length > 0)
					{
						string[] array = powerUp._PrtEffectURL.Split('/');
						RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnPowerUpPrtEffectEventHandler, typeof(GameObject));
					}
					break;
				}
			}
		}
		if (mPowerUpType != SanctuaryPowerUpType.SHRIMP)
		{
			return;
		}
		string[] array3 = null;
		if (!string.IsNullOrEmpty(inData))
		{
			array3 = inData.Split('/');
		}
		if (array3 != null && array3.Length > 1)
		{
			if (int.TryParse(array3[0], out var result) && result != -1)
			{
				mPowerUpTimer = result;
			}
			if (float.TryParse(array3[1], out var result2) && result2 != -1f)
			{
				ObScale obScale = ObScale.Set(base.gameObject, result2 * base.transform.localScale, 1f, inStartScale: true, inRequireOnScaleUpdate: true);
				obScale._RequireOnScaleUpdate = false;
				obScale.Awake();
			}
		}
	}

	public void EndPowerUp()
	{
		EndPowerUp(pIsMounted);
	}

	public void EndPowerUp(bool isMounted)
	{
		if (mPowerUpType == SanctuaryPowerUpType.NONE)
		{
			return;
		}
		GameObject rideObject = GetRideObject();
		if (rideObject != null)
		{
			bool flag = rideObject != AvAvatar.pObject;
			if (!flag && MainStreetMMOClient.pInstance != null)
			{
				string text = (isMounted ? "Y" : "N");
				MainStreetMMOClient.pInstance.SetRaisedPetString(pData.SaveToResStringEx(text + "_0"));
			}
			SanctuaryPowerUpData powerUp = GetPowerUp(mPowerUpType);
			if (powerUp != null)
			{
				switch (mPowerUpType)
				{
				case SanctuaryPowerUpType.SHRINK:
				{
					ObScale component = rideObject.GetComponent<ObScale>();
					if (component != null)
					{
						component._RequireOnScaleUpdate = true;
					}
					break;
				}
				case SanctuaryPowerUpType.SPARKLE:
					if (mPowerUpPrtEffect != null)
					{
						UnityEngine.Object.Destroy(mPowerUpPrtEffect);
					}
					break;
				case SanctuaryPowerUpType.SPEED:
					if (!flag)
					{
						AvAvatarStateData[] stateData = rideObject.GetComponent<AvAvatarController>()._StateData;
						foreach (AvAvatarStateData obj in stateData)
						{
							obj._MaxForwardSpeed /= 1f + powerUp._Value;
							obj._MaxBackwardSpeed /= 1f + powerUp._Value;
							obj._MaxAirSpeed /= 1f + powerUp._Value;
						}
					}
					if (mPowerUpPrtEffect != null)
					{
						UnityEngine.Object.Destroy(mPowerUpPrtEffect);
					}
					break;
				}
				if (_ActionDoneMessageObject != null)
				{
					_ActionDoneMessageObject.SendMessage("OnEndPowerUp", mPowerUpType, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		if (mPowerUpType == SanctuaryPowerUpType.SHRIMP)
		{
			ObScale component2 = base.gameObject.GetComponent<ObScale>();
			if (component2 != null)
			{
				component2._RequireOnScaleUpdate = true;
			}
		}
		mPowerUpType = SanctuaryPowerUpType.NONE;
	}

	private GameObject GetRideObject()
	{
		GameObject gameObject = null;
		if (mAvatar != null)
		{
			gameObject = mAvatar.gameObject;
			if (mTypeInfo._SpecialSkill == PetSpecialSkillType.RUN)
			{
				MMOAvatar component = gameObject.GetComponent<MMOAvatar>();
				gameObject = ((!(component != null)) ? AvAvatar.pObject : component.pObject);
			}
		}
		return gameObject;
	}

	private SanctuaryPowerUpData GetPowerUp(SanctuaryPowerUpType inType)
	{
		if (inType == SanctuaryPowerUpType.NONE)
		{
			return null;
		}
		SanctuaryPowerUpData[] powerUpData = _PowerUpData;
		foreach (SanctuaryPowerUpData sanctuaryPowerUpData in powerUpData)
		{
			if (inType == sanctuaryPowerUpData._Type)
			{
				return sanctuaryPowerUpData;
			}
		}
		return null;
	}

	private void OnPowerUpPrtEffectEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject rideObject = GetRideObject();
			SanctuaryPowerUpData powerUp = GetPowerUp(mPowerUpType);
			if (rideObject != null && powerUp != null)
			{
				if (mPowerUpPrtEffect != null)
				{
					UnityEngine.Object.Destroy(mPowerUpPrtEffect);
				}
				mPowerUpPrtEffect = UnityEngine.Object.Instantiate((GameObject)inObject);
				if (mDragonRootBone != null)
				{
					mPowerUpPrtEffect.transform.parent = mDragonRootBone;
				}
				else
				{
					mPowerUpPrtEffect.transform.parent = mDragonTransform;
				}
				mPowerUpPrtEffect.transform.localPosition = powerUp._PrtOffset;
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Error !!! downloading powerup particle effect");
			break;
		}
	}

	public override void PlayAnimSFX(string aname, bool looping)
	{
		PlayAnimSFX(_SoundMapper, aname, looping);
	}

	public bool PlaySFX(AudioClip clip, bool looping)
	{
		if (base.audio == null || clip == null)
		{
			return false;
		}
		base.audio.clip = clip;
		base.audio.loop = looping;
		base.audio.Play();
		return true;
	}

	public void LoadAnimSfx(string aname)
	{
		SFXMap[] soundMapper = _SoundMapper;
		foreach (SFXMap sFXMap in soundMapper)
		{
			if (!(aname == sFXMap._AnimName) || !(sFXMap._ClipRes == null) || sFXMap._ClipResName == null || sFXMap._ClipResName.Length <= 0)
			{
				continue;
			}
			sFXMap._ClipRes = (AudioClip)RsResourceManager.LoadAssetFromBundle(sFXMap._ClipResName);
			if (sFXMap._ClipRes == null)
			{
				string[] array = sFXMap._ClipResName.Split('/');
				if (array.Length == 3)
				{
					RsResourceManager.Load(array[0] + "/" + array[1], base.OnSFXResLoadingEvent, RsResourceType.NONE, inDontDestroy: true);
				}
			}
		}
	}

	private bool PlayAnimSFX(SFXMap[] soundMapper, string aname, bool looping)
	{
		if (base.audio == null)
		{
			return false;
		}
		foreach (SFXMap sFXMap in soundMapper)
		{
			if (!(aname == sFXMap._AnimName))
			{
				continue;
			}
			if (sFXMap._ClipRes == null && sFXMap._ClipResName != null && sFXMap._ClipResName.Length > 0)
			{
				sFXMap._ClipRes = (AudioClip)RsResourceManager.LoadAssetFromBundle(sFXMap._ClipResName);
				if (sFXMap._ClipRes == null)
				{
					string[] array = sFXMap._ClipResName.Split('/');
					if (array.Length == 3)
					{
						RsResourceManager.Load(array[0] + "/" + array[1], base.OnSFXResLoadingEvent, RsResourceType.NONE, inDontDestroy: true);
					}
				}
			}
			if (sFXMap._ClipRes != null)
			{
				base.audio.clip = sFXMap._ClipRes;
				base.audio.loop = looping;
				base.audio.spatialBlend = sFXMap._SpatialBlend;
				base.audio.Play();
				return true;
			}
			base.audio.Stop();
			return false;
		}
		base.audio.Stop();
		return false;
	}

	public void TriggerJumpEvent(bool isJump)
	{
		if (this.OnPetJump != null)
		{
			this.OnPetJump(isJump);
		}
	}

	private void TriggerMountEvent(bool isMount, PetSpecialSkillType skillType)
	{
		if (this.OnPetMount != null)
		{
			this.OnPetMount(isMount, skillType);
		}
		SanctuaryManager.pMountedState = isMount;
		if (pData != null)
		{
			string accessoryGeometry = pData.GetAccessoryGeometry(RaisedPetAccType.Saddle);
			bool disable = (accessoryGeometry != "NULL" && accessoryGeometry != "") || SanctuaryManager.pMountedState;
			DisableGameObjectsOnMountOrSaddle(disable);
		}
	}

	private void DisableGameObjectsOnMountOrSaddle(bool disable)
	{
		foreach (GameObject item in _DisabledWhenMounted)
		{
			if (item != null)
			{
				item.SetActive(!disable);
			}
		}
	}

	public void Fire(Transform target, bool useDirection, Vector3 direction, bool ignoreCoolDown = false, bool playAnimation = true, bool FireParticles = true)
	{
		if (ignoreCoolDown && mWeaponManager != null)
		{
			mWeaponManager.ResetCooldown();
		}
		string inAnim = _AttackAnim;
		if (mAvatarController.pSubState == AvAvatarSubState.FLYING || mAvatarController.pSubState == AvAvatarSubState.GLIDING)
		{
			inAnim = _FlyAttackAnim;
		}
		AnimationState animationState = base.animation[inAnim];
		float time;
		if (playAnimation)
		{
			if (animationState != null)
			{
				if (!pIsMounted)
				{
					string currentPlayingAnimation = GetCurrentPlayingAnimation();
					if (base.animation.Play(inAnim, PlayMode.StopAll))
					{
						animationState.blendMode = AnimationBlendMode.Blend;
						animationState.wrapMode = WrapMode.Once;
						animationState.weight = 1f;
						if (currentPlayingAnimation != "")
						{
							base.animation.CrossFadeQueued(currentPlayingAnimation, 0.2f);
						}
					}
				}
				else
				{
					PlayAdditiveAnimation(inAnim);
				}
				time = animationState.length;
			}
			else
			{
				time = _FireDelay + 0.001f;
			}
			mRemainingFireDelay = _FireDelay;
		}
		else
		{
			mRemainingFireDelay = 0.0001f;
			time = mRemainingFireDelay;
		}
		ObLookAt[] componentsInChildren = GetComponentsInChildren<ObLookAt>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].TimedDisable(time);
		}
		mFiringTarget = target;
		mFiringUseDirection = useDirection;
		mFiringDirection = direction;
		if (!FireParticles)
		{
			mRemainingFireDelay = 0f;
		}
	}

	public void DoFire(Transform target, bool useDirection, Vector3 direction)
	{
		float parentSpeed = 0f;
		if (mAvatarController != null)
		{
			parentSpeed = mAvatarController.pVelocity.magnitude;
		}
		if (mWeaponManager != null)
		{
			mWeaponManager.Fire(target, useDirection, direction, parentSpeed);
		}
	}

	public void Fire(GameObject obj)
	{
		Transform transform = obj.transform;
		Vector3 vector = ((transform.childCount > 0) ? transform.GetChild(UnityEngine.Random.Range(0, transform.childCount)).position : transform.position);
		DoFire(null, useDirection: true, (vector - base.transform.position).normalized);
	}

	public void PlayAnimLoop(string anim)
	{
		PlayAnimation(anim, WrapMode.Loop);
	}

	public void PlayAnimOnce(string anim)
	{
		PlayAnimation(anim, WrapMode.Once);
	}

	private string GetCurrentPlayingAnimation()
	{
		foreach (AnimationState item in base.animation)
		{
			if (item.weight > 0f)
			{
				return item.name;
			}
		}
		return "";
	}

	public float GetMountedSpeedModifer()
	{
		SanctuaryPetSpeedModifier[] speedModifers = mTypeSettings._SpeedModifers;
		foreach (SanctuaryPetSpeedModifier sanctuaryPetSpeedModifier in speedModifers)
		{
			if (HasMood(sanctuaryPetSpeedModifier._Mood))
			{
				return sanctuaryPetSpeedModifier._Mounted;
			}
		}
		return 1f;
	}

	public float GetFlightSpeedModifer()
	{
		SanctuaryPetSpeedModifier[] speedModifers = mTypeSettings._SpeedModifers;
		foreach (SanctuaryPetSpeedModifier sanctuaryPetSpeedModifier in speedModifers)
		{
			if (HasMood(sanctuaryPetSpeedModifier._Mood))
			{
				return sanctuaryPetSpeedModifier._Flight;
			}
		}
		return 1f;
	}

	public void OrientToNormal(Vector3 n, bool bLerp = true)
	{
		if (mDragonRootBone == null)
		{
			CacheMountBones();
		}
		if (!(mDragonRootBone != null) || !(n.sqrMagnitude > Mathf.Epsilon))
		{
			return;
		}
		Vector3 forward = mDragonTransform.forward;
		if (!(forward != n))
		{
			return;
		}
		forward = Vector3.Cross(Vector3.Cross(n, forward), n);
		if (forward != n)
		{
			Quaternion quaternion = Quaternion.LookRotation(forward, n);
			if (bLerp)
			{
				mDragonRootBone.rotation = Quaternion.Lerp(mDragonRootBone.rotation, quaternion, Time.deltaTime * 5f);
			}
			else
			{
				mDragonRootBone.rotation = quaternion;
			}
		}
	}

	public void ClearRoot(bool bClearPos = true)
	{
		if ((bool)mDragonRootBone)
		{
			mDragonRootBone.localEulerAngles = Vector3.zero;
			if (bClearPos)
			{
				mDragonRootBone.localPosition = Vector3.zero;
			}
		}
	}

	public void SetColors(Color pri, Color sec, Color ter, bool bSaveData)
	{
		if (pData != null)
		{
			Color[] colors = new Color[3] { pri, sec, ter };
			pData.SetColors(colors);
			UpdateShaders();
			if (bSaveData)
			{
				SaveData();
			}
		}
	}

	public void UpdateAccessories(bool noHat = false)
	{
		List<string> list = new List<string>();
		mApplyGlowSkinParameters = false;
		if (noHat)
		{
			list.Add(RaisedPetAccType.Hat.ToString());
		}
		if (!string.IsNullOrEmpty(mCustomSkinName) && (pData.pGlowEffect == null || (!MMOPet() && (pIsGlowDisabled || !pData.IsGlowAvailable() || !pData.IsGlowRunning()))))
		{
			list.Add(RaisedPetAccType.Materials.ToString());
			UpdateMaterials(null);
			UpdateRaisedPetAccessories(list);
			ApplyCustomSkin(mCustomSkinName);
		}
		else
		{
			UpdateRaisedPetAccessories(list);
		}
	}

	public void UpdateRaisedPetAccessories(List<string> skipAccessories = null)
	{
		UpdateRaisedPetAccessories(pData, skipAccessories);
	}

	public void UpdateRaisedPetAccessories(RaisedPetData data, List<string> skipAccessories = null)
	{
		if (data.Accessories != null && data.Accessories.Length != 0)
		{
			if (mAccLoaders != null && mAccLoaders.Count > 0)
			{
				for (int i = 0; i < mAccLoaders.Count; i++)
				{
					UnityEngine.Object.Destroy(mAccLoaders[i].mObj);
					mAccLoaders[i] = null;
				}
			}
			mAccLoaders = new List<SanctuaryPetAccData>();
			RaisedPetAccessory[] accessories = data.Accessories;
			foreach (RaisedPetAccessory raisedPetAccessory in accessories)
			{
				if (skipAccessories == null || skipAccessories.Count <= 0 || !skipAccessories.Contains(raisedPetAccessory.Type))
				{
					SanctuaryPetAccData sanctuaryPetAccData = new SanctuaryPetAccData(raisedPetAccessory, OnAccReady);
					mAccLoaders.Add(sanctuaryPetAccData);
					sanctuaryPetAccData.LoadResource();
				}
			}
		}
		if (data.GetAccessory(RaisedPetAccType.Materials) == null)
		{
			UpdateMaterials(null);
		}
		if (data.GetAccessory(RaisedPetAccType.Saddle) == null)
		{
			SetAccessory(RaisedPetAccType.Saddle, null, null);
		}
	}

	public void UpdateShaders()
	{
		if (pData == null)
		{
			return;
		}
		if (pData.Colors == null || pData.Colors.Length < 3)
		{
			SetColorsFromMaterial();
		}
		else
		{
			Color color = pData.GetColor(0);
			Color color2 = pData.GetColor(1);
			Color color3 = pData.GetColor(2);
			foreach (SkinnedMeshRenderer value in mRendererMap.Values)
			{
				Material[] materials = value.materials;
				foreach (Material material in materials)
				{
					if (material.HasProperty("_PrimaryColor"))
					{
						material.SetColor("_PrimaryColor", color);
					}
					if (material.HasProperty("_SecondaryColor"))
					{
						material.SetColor("_SecondaryColor", color2);
					}
					if (material.HasProperty("_TertiaryColor"))
					{
						material.SetColor("_TertiaryColor", color3);
					}
				}
			}
		}
		if (!pIsGlowDisabled)
		{
			RefreshGlowEffect();
		}
	}

	public void RemoveAllEffects()
	{
		if (pData.pGlowEffect != null)
		{
			RemoveGlowEffect();
		}
		RemoveCustomSkin();
	}

	private void RemoveCustomSkin()
	{
		if (!string.IsNullOrEmpty(mCustomSkinName))
		{
			mCustomSkinName = null;
			UpdateAccessories();
		}
	}

	private void ApplyCustomSkin(string skinName)
	{
		CustomSkinData customSkinData = GetCustomSkinData(skinName);
		if (customSkinData != null)
		{
			RaisedPetAccessory adata = new RaisedPetAccessory
			{
				Type = RaisedPetAccType.Materials.ToString(),
				Geometry = customSkinData._ResourcePath
			};
			mApplyGlowSkinParameters = customSkinData._ApplyGlowParameters;
			SanctuaryPetAccData sanctuaryPetAccData = new SanctuaryPetAccData(adata, OnAccReady);
			if (mAccLoaders == null)
			{
				mAccLoaders = new List<SanctuaryPetAccData>();
			}
			mAccLoaders.Add(sanctuaryPetAccData);
			sanctuaryPetAccData.LoadResource();
		}
	}

	private void UpdateMaterials(GameObject materialsObject)
	{
		if (materialsObject != null)
		{
			DragonSkin component = materialsObject.GetComponent<DragonSkin>();
			if (!(component == null) && !(component.name == mSkinApplied))
			{
				if (!string.IsNullOrEmpty(mSkinApplied))
				{
					ResetSkinData();
				}
				SetSkinData(component);
				UpdateShaders();
				if (mApplyGlowSkinParameters && GlowManager.pInstance != null)
				{
					GlowManager.pInstance.ApplyMaterialParameters(base.gameObject, mIgnoreAccesoryList);
				}
			}
		}
		else if (!string.IsNullOrEmpty(mSkinApplied))
		{
			ResetSkinData();
			UpdateShaders();
		}
	}

	private void ResetSkinData()
	{
		if (pCurAgeData._PetResList == null || pCurAgeData._PetResList.Length < 2)
		{
			UtDebug.LogError("pCurAgeData._PetResList empty for pet " + base.gameObject.name);
			return;
		}
		if (pWeaponManager != null)
		{
			pWeaponManager.ModifyWeapon(null);
		}
		string text = "";
		text = ((pCurAgeData._PetResList[0]._Gender != pData.Gender) ? pCurAgeData._PetResList[1]._Prefab : pCurAgeData._PetResList[0]._Prefab);
		List<string> list = new List<string>();
		if (mIgnoreAccesoryList != null)
		{
			Transform transform = FindBoneTransform("Saddle");
			SkinnedMeshRenderer skinnedMeshRenderer = null;
			if (transform != null)
			{
				skinnedMeshRenderer = transform.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
			}
			if (skinnedMeshRenderer != null)
			{
				list.Add(skinnedMeshRenderer.name);
			}
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(RsResourceManager.LoadAssetFromBundle(text, typeof(GameObject)) as GameObject);
		SkinnedMeshRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (SkinnedMeshRenderer skinnedMeshRenderer2 in componentsInChildren)
		{
			bool flag = false;
			foreach (string item in list)
			{
				if (skinnedMeshRenderer2.name == item)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				mRendererMap[skinnedMeshRenderer2.name].materials = skinnedMeshRenderer2.materials;
				mRendererMap[skinnedMeshRenderer2.name].sharedMesh = skinnedMeshRenderer2.sharedMesh;
			}
		}
		UnityEngine.Object.Destroy(gameObject);
		mSkinApplied = string.Empty;
	}

	private void SetSkinData(DragonSkin dragonSkin)
	{
		if (pWeaponManager != null && dragonSkin._Weapon != null && !string.IsNullOrEmpty(dragonSkin._Weapon.name))
		{
			pWeaponManager.ModifyWeapon(dragonSkin._Weapon.name);
		}
		Material[] array;
		Material[] array2;
		switch (pData.pStage)
		{
		case RaisedPetStage.BABY:
			array = dragonSkin._BabyMaterials;
			array2 = dragonSkin._BabyMaterials;
			break;
		case RaisedPetStage.TEEN:
			array = ((dragonSkin._TeenMaterials.Length == 0) ? dragonSkin._Materials : dragonSkin._TeenMaterials);
			array2 = ((dragonSkin._TeenLODMaterials.Length == 0) ? dragonSkin._LODMaterials : dragonSkin._TeenLODMaterials);
			break;
		case RaisedPetStage.TITAN:
			array = dragonSkin._TitanMaterials;
			array2 = dragonSkin._TitanLODMaterials;
			break;
		default:
			array = dragonSkin._Materials;
			array2 = dragonSkin._LODMaterials;
			break;
		}
		Mesh mesh = ((pData.pStage == RaisedPetStage.BABY) ? dragonSkin._BabyMesh : ((pData.pStage == RaisedPetStage.TEEN) ? ((!(dragonSkin._TeenMesh != null)) ? dragonSkin._Mesh : dragonSkin._TeenMesh) : ((pData.pStage != RaisedPetStage.TITAN) ? dragonSkin._Mesh : dragonSkin._TitanMesh)));
		foreach (string key in mRendererMap.Keys)
		{
			if (!dragonSkin.IsRendererAllowedToChange(key))
			{
				continue;
			}
			if (array != null && array.Length != 0)
			{
				if (key.Contains("LOD") && array2 != null && array2.Length != 0)
				{
					array = array2;
				}
				if (mRendererMap[key].materials.Length < array.Length)
				{
					Material[] array3 = new Material[mRendererMap[key].materials.Length];
					Array.Copy(array, array3, mRendererMap[key].materials.Length);
					mRendererMap[key].materials = array3;
				}
				else
				{
					mRendererMap[key].materials = array;
				}
			}
			if (mesh != null)
			{
				mRendererMap[key].sharedMesh = mesh;
			}
		}
		mSkinApplied = dragonSkin.name;
		if (UtPlatform.IsEditor())
		{
			UtUtilities.ReAssignShader(base.gameObject);
		}
	}

	public void SetColorsFromMaterial()
	{
		if (pData == null)
		{
			return;
		}
		Color color = Color.white;
		Color sec = Color.white;
		Color ter = Color.white;
		foreach (SkinnedMeshRenderer value in mRendererMap.Values)
		{
			Material[] materials = value.materials;
			foreach (Material material in materials)
			{
				if (material.HasProperty("_PrimaryColor"))
				{
					color = material.GetColor("_PrimaryColor");
				}
				if (material.HasProperty("_SecondaryColor"))
				{
					sec = material.GetColor("_SecondaryColor");
				}
				if (material.HasProperty("_TertiaryColor"))
				{
					ter = material.GetColor("_TertiaryColor");
				}
			}
			if (color != Color.white)
			{
				break;
			}
		}
		SetColors(color, sec, ter, bSaveData: true);
	}

	private void OnDestroy()
	{
		SceneManager.activeSceneChanged -= OnSceneChanged;
		if (pData != null && !string.IsNullOrEmpty(pData.Texture))
		{
			string inURL = pData.Texture;
			if (pData != SanctuaryManager.pCurPetData)
			{
				inURL = UtUtilities.GetImageURL(inURL);
			}
			RsResourceManager.Unload(inURL, splitURL: false);
		}
		StopGlowCouroutine();
		if (AvAvatar.pObject != null)
		{
			UnregisterAvatarDamage();
		}
		if (GlowManager.pInstance != null)
		{
			GlowManager.pInstance.AddRemoveGlowObject(base.gameObject, remove: true);
		}
	}

	public void StopGlowCouroutine()
	{
		if (mRemoveGlowCoroutine != null)
		{
			StopCoroutine(mRemoveGlowCoroutine);
			mRemoveGlowCoroutine = null;
		}
	}

	public override void OnDisable()
	{
		if (base.animation != null)
		{
			base.animation.Stop();
		}
		base.OnDisable();
	}

	private void PatchUpdateDragonFromPairData()
	{
		PairData.Load(2014, PatchPairDataHandler, null);
	}

	private void PatchPairDataHandler(bool success, PairData inData, object inUserData)
	{
		if (inData != null)
		{
			Color pri = PatchGetPairDataColor(inData, "Color_PrimaryDragonC");
			Color sec = PatchGetPairDataColor(inData, "Color_SecondaryDragonC");
			Color ter = PatchGetPairDataColor(inData, "Color_TertiaryDragonC");
			if (!inData.GetValue("Color_PrimaryDragonC").Equals("___VALUE_NOT_FOUND___"))
			{
				SetColors(pri, sec, ter, bSaveData: true);
			}
		}
	}

	private Color PatchGetPairDataColor(PairData inData, string key)
	{
		Color color = Color.white;
		string value = inData.GetValue(key);
		if (!value.Equals("LIST_NOT_VALID") && !value.Equals("___VALUE_NOT_FOUND___"))
		{
			HexUtil.HexToColor(value, out color);
		}
		return color;
	}

	public void OnTelportedToPlayer()
	{
		if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance == this && mCurrentSkillType == PetSpecialSkillType.FLY)
		{
			SanctuaryManager.pCurPetInstance.OnFlyDismount(AvAvatar.pObject);
		}
	}

	public void MoveToHeight(float to)
	{
		if (to == 0f)
		{
			mMoveToTimer = 0f;
			return;
		}
		mMoveToTimer = 0.5f;
		mMoveToHeight = to;
		mMoveFromHeight = mDragonTransform.position.y;
	}

	public bool IsMMOPlayer()
	{
		return mMMOAvatar != null;
	}

	public virtual bool IsSwimming()
	{
		return _IdleAnimName == _AnimNameSwim;
	}

	public override void CheckSwim(Collider c)
	{
		if ((!(mAvatar != null) || !(c.gameObject == mAvatar.gameObject)) && !(c.transform.root == base.transform.root))
		{
			base.CheckSwim(c);
		}
	}

	public override void ApplySwim(bool apply)
	{
		if (!(AIActor != null) || !(base.animation[_AnimNameSwim] != null))
		{
			return;
		}
		if (apply)
		{
			if (!pIsMounted)
			{
				AIActor.SetState(AISanctuaryPetFSM.SWIMMING);
			}
		}
		else if (AIActor._State == AISanctuaryPetFSM.SWIMMING)
		{
			AIActor.SetState(AISanctuaryPetFSM.NORMAL);
		}
		base.ApplySwim(apply);
	}

	public void UpdateWaterSplash()
	{
		if (IsSwimming())
		{
			mWaterCheckTime -= Time.deltaTime;
			if (!(mWaterCheckTime <= 0f) && !mPrevWaterSplash)
			{
				return;
			}
			mWaterCheckTime = mWaterCheckInterval;
			bool flag = mWaterObject != null;
			RaycastHit hitInfo = default(RaycastHit);
			Vector3 origin = base.transform.position + new Vector3(0f, 0.5f, 0f);
			if (flag || Physics.Raycast(origin, new Vector3(0f, -1f, 0f), out hitInfo, 5.5f, UtUtilities.GetGroundRayCheckLayers()))
			{
				Vector3 vector = Vector3.zero;
				bool flag2 = true;
				if (flag)
				{
					vector = new Vector3(base.transform.position.x, mWaterObject.transform.position.y, base.transform.position.z);
				}
				else if (hitInfo.collider.gameObject.CompareTag("Water"))
				{
					vector = hitInfo.point;
				}
				else
				{
					flag2 = false;
				}
				if (!flag2)
				{
					return;
				}
				if (IsPetSwimIdle())
				{
					if (mWaterParticleObj == null && SanctuaryManager.pInstance != null && SanctuaryManager.pInstance._PetSwimIdleFx != null)
					{
						mWaterParticleObj = UnityEngine.Object.Instantiate(SanctuaryManager.pInstance._PetSwimIdleFx, vector, base.transform.rotation);
						mWaterParticleObj.transform.parent = base.transform;
					}
				}
				else
				{
					StopParticles(mWaterParticleObj);
					mWaterSplashCheckTime += Time.deltaTime;
					if (mWaterSplashCheckTime >= _WaterSplashSpawnInterval)
					{
						mWaterSplashCheckTime = 0f;
						if (SanctuaryManager.pInstance != null && SanctuaryManager.pInstance._PetSwimFx != null)
						{
							PlayParticleAtPos(SanctuaryManager.pInstance._PetSwimFx, vector, base.transform.rotation);
						}
					}
				}
				mPrevWaterSplash = true;
			}
			else
			{
				mPrevWaterSplash = false;
			}
		}
		else
		{
			StopParticles(mWaterParticleObj);
		}
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

	public virtual void PlayParticleAtPos(Transform pfx, Vector3 pos, Quaternion rot)
	{
		if (!(pfx != null))
		{
			return;
		}
		Transform transform = null;
		SpawnPool spawnPool = null;
		if (PoolManager.Pools.TryGetValue("Particles", out spawnPool))
		{
			Transform transform2 = spawnPool.Spawn(pfx, pos, rot);
			transform = ((transform2 != null) ? transform2 : null);
			if (transform != null)
			{
				ParticleSystem[] componentsInChildren = transform.GetComponentsInChildren<ParticleSystem>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].Play();
				}
			}
		}
		else
		{
			transform = UnityEngine.Object.Instantiate(pfx, pos, rot);
		}
		if (transform != null)
		{
			ObDespawnEmitter component = transform.GetComponent<ObDespawnEmitter>();
			if (component != null)
			{
				component._Pool = spawnPool;
			}
			else if (transform.GetComponent<ObSelfDestructTimer>() == null)
			{
				ObSelfDestructTimer obSelfDestructTimer = transform.gameObject.AddComponent<ObSelfDestructTimer>();
				obSelfDestructTimer._Pool = spawnPool;
				obSelfDestructTimer._SelfDestructTime = 1f;
			}
		}
	}

	public void PlayPetMoodParticle(string inMeter, bool isForcePlay)
	{
		if (!string.IsNullOrEmpty(inMeter))
		{
			SanctuaryPetMeterType meterTypeByKey = GetMeterTypeByKey(inMeter);
			PlayPetMoodParticle(meterTypeByKey, isForcePlay);
		}
		else
		{
			StopPetMoodParticles();
		}
	}

	public void PlayPetMoodParticle(SanctuaryPetMeterType inMeter, bool isForcePlay)
	{
		if (pCurAgeData == null || pCurAgeData._MoodParticlesOnPet == null || pCurAgeData._MoodParticlesOnPet.Length == 0)
		{
			return;
		}
		if (mDisableMoodParticle)
		{
			StopPetMoodParticles();
			return;
		}
		PetMoodParticleData petMoodParticleData = GetPetMoodParticleData(inMeter);
		if (petMoodParticleData == null)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		if (mCurrMoodPrtData != null && petMoodParticleData._Type == mCurrMoodPrtData._Type)
		{
			if (isForcePlay || PetMoodParticleAllowed(inMeter, petMoodParticleData._ThresholdPercentage))
			{
				if (mMoodPrtEffect == null)
				{
					flag = true;
				}
				else if (MainStreetMMOClient.pInstance != null && mAvatar != null && AvAvatar.IsCurrentPlayer(mAvatar.gameObject))
				{
					MainStreetMMOClient.pInstance.SendPetMoodParticle(mCurrMoodPrtData._Type.ToString());
				}
			}
			else if (mMoodPrtEffect != null)
			{
				flag2 = true;
			}
		}
		else if (isForcePlay || PetMoodParticleAllowed(inMeter, petMoodParticleData._ThresholdPercentage))
		{
			if (mMoodPrtEffect != null)
			{
				flag2 = true;
			}
			flag = true;
		}
		if (flag2)
		{
			StopPetMoodParticles();
		}
		if (flag)
		{
			mCurrMoodPrtData = petMoodParticleData;
			string[] array = mCurrMoodPrtData._ParticleObject.Split('/');
			DestroyMoodParticles();
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnMoodPrtEffectEventHandler, typeof(GameObject));
			for (int i = 0; i < petMoodParticleData._AdditionalParticleData.Length; i++)
			{
				array = petMoodParticleData._AdditionalParticleData[i]._ParticleObject.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnMoodPrtEffectEventHandler, typeof(GameObject), inDontDestroy: false, petMoodParticleData._AdditionalParticleData[i]);
			}
			if (MainStreetMMOClient.pInstance != null && mAvatar != null && AvAvatar.IsCurrentPlayer(mAvatar.gameObject))
			{
				MainStreetMMOClient.pInstance.SendPetMoodParticle(mCurrMoodPrtData._Type.ToString());
			}
		}
	}

	private bool PetMoodParticleAllowed(SanctuaryPetMeterType inMeter, int inThreshold)
	{
		float maxMeter = SanctuaryData.GetMaxMeter(inMeter, pData);
		return Mathf.CeilToInt(GetMeterValue(inMeter) / maxMeter * 100f) >= inThreshold;
	}

	private PetMoodParticleData GetPetMoodParticleData(SanctuaryPetMeterType inMeter)
	{
		for (int i = 0; i < pCurAgeData._MoodParticlesOnPet.Length; i++)
		{
			if (pCurAgeData._MoodParticlesOnPet[i]._Type == inMeter)
			{
				return pCurAgeData._MoodParticlesOnPet[i];
			}
		}
		return null;
	}

	private void StopPetMoodParticles()
	{
		if (mMoodPrtEffect != null)
		{
			DestroyMoodParticles();
			mMoodPrtEffect = null;
			mCurrMoodPrtData = null;
			if (MainStreetMMOClient.pInstance != null && mAvatar != null && AvAvatar.IsCurrentPlayer(mAvatar.gameObject))
			{
				MainStreetMMOClient.pInstance.SendPetMoodParticle(string.Empty);
			}
		}
	}

	public void DisableAllMoodParticles()
	{
		mMoodPrtEffect = null;
		mCurrMoodPrtData = null;
		foreach (SanctuaryPetMeterType value in Enum.GetValues(typeof(SanctuaryPetMeterType)))
		{
			PetMoodParticleData petMoodParticleData = GetPetMoodParticleData(value);
			if (petMoodParticleData == null)
			{
				continue;
			}
			Transform transform = FindBoneTransform(petMoodParticleData._ParentBone);
			if (transform != null)
			{
				string n = petMoodParticleData._ParticleObject.Substring(petMoodParticleData._ParticleObject.LastIndexOf("/") + 1);
				Transform transform2 = transform.Find(n);
				if (transform2 != null)
				{
					UnityEngine.Object.Destroy(transform2.gameObject);
				}
			}
		}
	}

	public void DestroyMoodParticles()
	{
		if (mMoodPrtEffect == null)
		{
			return;
		}
		for (int i = 0; i < mMoodPrtEffect.Count; i++)
		{
			if (mMoodPrtEffect[i] != null)
			{
				UnityEngine.Object.Destroy(mMoodPrtEffect[i]);
			}
		}
		mMoodPrtEffect.Clear();
		mMoodPrtEffect = null;
	}

	public void CreateMoodParticles(GameObject inObject, PetMoodParticleData prtData)
	{
		CreateMoodParticles(inObject, prtData._ParentBone, prtData._ParticleOffset, prtData._ParticleObject);
	}

	public void CreateMoodParticles(GameObject inObject, AdditionalParticleData additionalParticleData)
	{
		CreateMoodParticles(inObject, additionalParticleData._ParentBone, additionalParticleData._ParticleOffset, additionalParticleData._ParticleObject);
	}

	public void CreateMoodParticles(GameObject inObject, string bone, Vector3 particleOffset, string particleObjectName)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(inObject);
		Transform transform = FindBoneTransform(bone);
		if (transform == null)
		{
			transform = mDragonTransform;
		}
		gameObject.name = particleObjectName.Substring(particleObjectName.LastIndexOf("/") + 1);
		gameObject.transform.parent = transform;
		gameObject.transform.localPosition = particleOffset;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		if (UtPlatform.IsWSA())
		{
			UtUtilities.ReAssignShader(gameObject);
		}
		if (mMoodPrtEffect == null)
		{
			mMoodPrtEffect = new List<GameObject>();
		}
		mMoodPrtEffect.Add(gameObject);
	}

	private void OnMoodPrtEffectEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (inUserData == null)
			{
				CreateMoodParticles((GameObject)inObject, mCurrMoodPrtData);
			}
			else
			{
				CreateMoodParticles((GameObject)inObject, (AdditionalParticleData)inUserData);
			}
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Error !!! downloading mood particle effect");
			break;
		}
	}

	public void SetMoodParticleIgnore(bool isIgnore)
	{
		mDisableMoodParticle = isIgnore;
		if (mDisableMoodParticle)
		{
			StopPetMoodParticles();
		}
	}

	public void SetSleepParticle(bool active, Transform defaultParticle)
	{
		if (_SleepingParticleSystem == null)
		{
			Transform transform = UtUtilities.FindChildTransform(base.gameObject, GetHeadBoneName());
			if (transform == null)
			{
				transform = base.transform;
			}
			Transform transform2 = UnityEngine.Object.Instantiate(defaultParticle, transform.position, Quaternion.identity, transform);
			_SleepingParticleSystem = transform2.GetComponent<ParticleSystem>();
		}
		if (active)
		{
			_SleepingParticleSystem.Play();
		}
		else
		{
			_SleepingParticleSystem.Stop();
		}
	}

	private void OnEnable()
	{
		UpdatePetEffects();
	}

	public void RefreshGlowEffect(bool runCoroutineOnRemove = true)
	{
		if (mMMOAvatar != null || mRemoveGlowCoroutine != null)
		{
			return;
		}
		bool flag = true;
		bool removeAndSaveData = false;
		if (pData.IsGlowAvailable() && !pIsGlowDisabled)
		{
			flag = false;
			if (pData.IsGlowRunning())
			{
				ApplyGlowEffect(pData.pGlowEffect.GlowColor);
			}
			else
			{
				flag = true;
				mIsGlowing = true;
				removeAndSaveData = true;
			}
		}
		if (!(mIsGlowing && flag))
		{
			return;
		}
		if (runCoroutineOnRemove)
		{
			if (mRemoveGlowCoroutine == null)
			{
				mRemoveGlowCoroutine = StartCoroutine(RemoveGlowOnCheck(removeAndSaveData));
			}
		}
		else
		{
			RemoveGlowEffect();
		}
	}

	private IEnumerator RemoveGlowOnCheck(bool removeAndSaveData = false)
	{
		if (GlowManager.pInstance == null)
		{
			mRemoveGlowCoroutine = null;
			yield break;
		}
		while (!GlowManager.pInstance.CanUpdateGlow(this))
		{
			yield return null;
		}
		RemoveGlowEffect(this == SanctuaryManager.pCurPetInstance);
		if (pData.pGlowEffect != null && removeAndSaveData)
		{
			pData.RemoveGlowEffect();
			pData.SaveDataReal();
		}
		mRemoveGlowCoroutine = null;
	}

	private void UpdatePetEffects()
	{
		if (!string.IsNullOrEmpty(mCustomSkinName))
		{
			UpdateAccessories();
		}
		else
		{
			RefreshGlowEffect();
		}
	}

	public void ApplyGlowEffect(string color)
	{
		if (GlowManager.pInstance != null)
		{
			mIsGlowing = true;
			GlowManager.pInstance.ApplyGlow(color, pData.PetTypeID, base.gameObject, mIgnoreAccesoryList);
			StartGlowTimer();
		}
	}

	public void RemoveGlowEffect(bool playFx = false)
	{
		if (GlowManager.pInstance != null)
		{
			if (string.IsNullOrEmpty(mCustomSkinName))
			{
				GlowManager.pInstance.RemoveGlow(base.gameObject, mIgnoreAccesoryList, playFx, SanctuaryManager.pCurPetInstance == this);
			}
			else
			{
				GlowManager.pInstance.UpdateUser(base.gameObject.transform.position, playFx, SanctuaryManager.pCurPetInstance == this);
			}
			mIsGlowing = false;
		}
	}

	public void StartGlowTimer()
	{
		if (pData.IsGlowAvailable())
		{
			mGlowTimer.StartTimer(pData.pGlowEffect.Duration, pData.pGlowEffect.EndTime);
			mGlowTimer.SetCountDownEndCallback(ResetGlowTimerCallback);
			mGlowTimer.SetTickInterval(0.9f);
		}
	}

	public void ForceUpdateGlowTimer(int seconds)
	{
		if (pData.IsGlowAvailable())
		{
			mGlowTimer.pEndTime = ServerTime.pCurrentTime.AddSeconds(seconds);
			if (UiDragonCustomization.pInstance != null)
			{
				UiDragonCustomization.pInstance.mGlowCountDown.pEndTime = mGlowTimer.pEndTime;
			}
			pData.SetAttrData("GlowEffect", pData.pGlowEffect.Save(seconds.ToString(), pData.pGlowEffect.GlowColor, mGlowTimer.pEndTime), DataType.STRING);
			pData.SaveDataReal();
		}
	}

	public void ResetGlowTimerCallback()
	{
		pData.pGlowEffect.EndTime = DateTime.MinValue;
		pData.SaveGlowData();
		pData.SaveDataReal();
		mGlowTimer.Reset();
		UpdatePetEffects();
		if (MainStreetMMOClient.pInstance != null && MainStreetMMOClient.pInstance.IsLevelMMO(RsResourceManager.pCurrentLevel))
		{
			SanctuaryPet pCurPetInstance = SanctuaryManager.pCurPetInstance;
			int mount = (int)(pCurPetInstance.pIsMounted ? pCurPetInstance.pCurrentSkillType : ((PetSpecialSkillType)(-1)));
			MainStreetMMOClient.pInstance.SetRaisedPet(SanctuaryManager.pCurPetInstance.pData, mount);
		}
	}

	public void UpdateCountDown()
	{
		mGlowTimer.UpdateCountDown();
	}

	public void VerifyPetFollow()
	{
		if (!(AvAvatar.pObject == null) && !SanctuaryManager.pInstance._AttachPetToBed)
		{
			if (SanctuaryManager.pCurPetInstance.mAvatar != AvAvatar.pObject.transform)
			{
				SanctuaryManager.pCurPetInstance.SetAvatar(AvAvatar.pObject.transform, SpawnTeleportEffect: false);
			}
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if (component != null && component.pPlayerMounted)
			{
				SanctuaryManager.pCurPetInstance.Mount(AvAvatar.pObject, PetSpecialSkillType.RUN);
			}
		}
	}

	private bool MMOPet()
	{
		if (!(mMMOAvatar != null))
		{
			return pData.UserID == Guid.Empty;
		}
		return true;
	}
}
