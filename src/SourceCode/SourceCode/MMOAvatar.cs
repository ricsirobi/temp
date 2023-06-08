using System.Collections.Generic;
using KA.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class MMOAvatar : MMOPrediction
{
	private const char MESSAGE_SEPARATOR = ':';

	private const string WEAPON_HIT = "WH";

	private const string WEAPON_FIRED = "WF";

	private const string WEAPON_FIRED_WITH_TARGET = "WFWT";

	[HideInInspector]
	public bool mLimbo = true;

	public static bool _IsRideAllowed = true;

	public bool pMMOInit;

	[HideInInspector]
	public string mUserID = "";

	[HideInInspector]
	public string mUserName = "";

	protected AvatarData.InstanceInfo mAvatarData;

	private UserAchievementInfo mRankData;

	private UserProfileData mProfileData;

	private Texture mRankTexture;

	private Transform mDisplayName;

	protected string mLevel = "";

	private float mTime;

	private bool mActive;

	private float mInactiveTimer;

	private float mInactiveTimeout = 15f;

	protected bool mLevelChanged;

	private bool mMember;

	private bool mReloading;

	private bool mFirstPosition;

	protected bool mTeleportAvatar;

	private MMOJoinStatus mJoinAllowed = MMOJoinStatus.ALLOWED;

	private bool mBusy;

	private Group mGroup;

	private PetDataPet mPetDataPet;

	private MMOAvatarUserVarData mEmoteID;

	private MMOAvatarUserVarData mActionID;

	private MMOAvatarUserVarData mChatID;

	private ChatOptions mChannel;

	public Dictionary<string, string> mKeys = new Dictionary<string, string>();

	public List<MMOAvatarUserVarData> mUserVarData = new List<MMOAvatarUserVarData>();

	private GameObject mObject;

	private GameObject mBusyProp;

	protected bool mIsVisible;

	protected float mLastSwitchTime;

	[HideInInspector]
	public float pLastMMOUpdate;

	private int mTimeReceived = -1;

	private int mRideItemID = -1;

	private SanctuaryPet mSanctuaryPet;

	protected RaisedPetData mRaisedPetData;

	private string mRaisedPetDataString = "";

	private bool mRaisedPetSetPending;

	private bool mSanctuaryPetSynced;

	protected bool mPendingLoadRaisedPet;

	protected bool mMountRaisedPet;

	protected PetSpecialSkillType mMountTypeRaisedPet;

	private bool mDismountRaisedPet;

	private bool mRaisedPetLoading;

	protected OptimizationData.LevelData mOptimizationData;

	protected float mOptimizationCheckTimer = 1f;

	private SkinnedMeshRenderer[] mSkinnedMeshRenderers;

	private Collider[] mColliders;

	protected MMOAvatarType mMMOAvatarType = MMOAvatarType.LITE;

	private int mOptimizedFlags;

	protected Transform mSprite;

	protected Transform mMeshObject;

	private const int MOOD_NAME_SHADOW_FLAG = 1;

	private const int MUTT_FLAG = 2;

	protected Animator mAnimator;

	public bool mLoaded;

	public string pUserID => mUserID;

	public AvatarData.InstanceInfo pAvatarData => mAvatarData;

	public GameObject pObject
	{
		get
		{
			if (mObject == null)
			{
				mObject = mAvatarData.mAvatar;
			}
			return mObject;
		}
		set
		{
			mObject = value;
		}
	}

	public int pTimeReceived
	{
		get
		{
			return mTimeReceived;
		}
		set
		{
			mTimeReceived = value;
		}
	}

	public Transform pDisplayName => mDisplayName;

	public bool pIsReady
	{
		get
		{
			if (mMMOAvatarType == MMOAvatarType.LITE)
			{
				return true;
			}
			if (!mAvatarData.pIsReady)
			{
				return mReloading;
			}
			return true;
		}
	}

	public UserAchievementInfo pRankData
	{
		get
		{
			return mRankData;
		}
		set
		{
			mRankData = value;
		}
	}

	public UserProfileData pProfileData => mProfileData;

	public bool pIsSanctuaryPetReady
	{
		get
		{
			if (mSanctuaryPet != null)
			{
				return true;
			}
			return false;
		}
	}

	public bool pCanSwitch => Time.realtimeSinceStartup - mLastSwitchTime >= 10f;

	public bool pIsMember
	{
		get
		{
			if (!pMMOInit)
			{
				return false;
			}
			return mMember;
		}
	}

	public bool pReloading => mReloading;

	public MMOJoinStatus pJoinAllowed => mJoinAllowed;

	public string pLevel => mLevel;

	public float pTime => mTime;

	public bool pActive => mActive;

	public AvAvatarController pController
	{
		get
		{
			return mController;
		}
		set
		{
			mController = value;
		}
	}

	public SanctuaryPet pSanctuaryPet => mSanctuaryPet;

	public RaisedPetData pRaisedPetData => mRaisedPetData;

	public bool pCanAvatarShow
	{
		get
		{
			if (!string.IsNullOrEmpty(mRaisedPetDataString))
			{
				if (mSanctuaryPet == null)
				{
					return !RaisedPetData.IsMounted(mRaisedPetDataString);
				}
				return true;
			}
			return true;
		}
	}

	public MMOAvatarType pMMOAvatarType
	{
		get
		{
			return mMMOAvatarType;
		}
		set
		{
			if (mMMOAvatarType != value)
			{
				mMMOAvatarType = value;
				mLastSwitchTime = Time.realtimeSinceStartup;
				OnAvatarTypeChanged();
			}
		}
	}

	public Transform pTransform => mTransform;

	public static MMOAvatar CreateAvatar(string userID, string userName, Gender gender)
	{
		GameObject avatar = MainStreetMMOClient.GetAvatar();
		if (avatar != null)
		{
			MMOAvatarLite mMOAvatarLite = avatar.GetComponent<MMOAvatarLite>();
			if (mMOAvatarLite == null)
			{
				mMOAvatarLite = avatar.AddComponent<MMOAvatarLite>();
			}
			mMOAvatarLite.Init(userID, userName);
			return mMOAvatarLite;
		}
		return null;
	}

	public void Init(string userID, string userName)
	{
		mLimbo = false;
		mHasReceivedPackage = false;
		pMMOInit = false;
		mRankData = null;
		mProfileData = null;
		mRankTexture = null;
		mDisplayName = null;
		mLevel = "";
		mTime = 0f;
		mActive = false;
		mInactiveTimer = 0f;
		mLevelChanged = false;
		mMember = false;
		mReloading = false;
		mFirstPosition = false;
		mTeleportAvatar = false;
		mJoinAllowed = MMOJoinStatus.ALLOWED;
		mBusy = false;
		mRaisedPetData = null;
		mPetDataPet = null;
		mEmoteID = null;
		mActionID = null;
		mChatID = null;
		mUserVarData = new List<MMOAvatarUserVarData>();
		mObject = null;
		mBusyProp = null;
		mIsVisible = false;
		mLastSwitchTime = 0f;
		mTimeReceived = -1;
		mRideItemID = -1;
		mSanctuaryPet = null;
		mRaisedPetDataString = "";
		mRaisedPetSetPending = false;
		mSanctuaryPetSynced = false;
		mPendingLoadRaisedPet = false;
		mMountRaisedPet = false;
		mDismountRaisedPet = false;
		mRaisedPetLoading = false;
		mTransform = base.transform;
		mUserID = userID;
		mUserName = userName;
		mAvatarData = new AvatarData.InstanceInfo();
		mAvatarData.mAvatar = base.gameObject;
		mAvatarData.mAvatar.name = "Avatar " + userID;
		mAvatarData.mAvatar.tag = "Untagged";
		mAvatarData.mAvatar.transform.position = new Vector3(0f, -5000f, 0f);
		mAvatarData.mMergeWithDefault = true;
		mDisplayName = AvatarData.GetDisplayNameObj(mAvatarData.mAvatar);
		ObClickable obClickable = base.gameObject.GetComponent<ObClickable>();
		if (obClickable == null)
		{
			obClickable = base.gameObject.AddComponent<ObClickable>();
		}
		obClickable._AvatarWalkTo = false;
		obClickable._RollOverCursorName = "Activate";
		mController = mAvatarData.mAvatar.GetComponent<AvAvatarController>();
		mController._ControlMode = AvAvatarControlMode.NONE;
		mController.pState = AvAvatarState.NONE;
		mOptimizationData = GameDataConfig.GetLevelData(RsResourceManager.pCurrentLevel);
		mOptimizationCheckTimer = GameDataConfig.pInstance.OptimizationData.UpdateFrequency;
		AvatarData.SetDisplayNameVisible(mAvatarData.mAvatar, AvatarData.pDisplayOtherName, isMember: false);
		mSprite = mTransform.Find(AvatarSettings.pInstance._SpriteName);
		mSprite.gameObject.SetActive(mMMOAvatarType == MMOAvatarType.LITE);
		if (mMeshObject == null)
		{
			mMeshObject = mController._MainRoot;
		}
		mAnimator = mController.GetComponentInChildren<Animator>();
	}

	public virtual void OnCreated()
	{
		if (mAvatarData != null && mAvatarData.mInstance != null)
		{
			AvatarData.SetDisplayName(base.gameObject, mAvatarData.mInstance.DisplayName);
		}
	}

	public void SetIgnoreCollisionToAllAvatars()
	{
	}

	public virtual void Update()
	{
		mTime = Time.realtimeSinceStartup;
		if (mLevel == RsResourceManager.pCurrentLevel && MainStreetMMOClient.pInstance != null && !MainStreetMMOClient.pInstance.pAllDeactivated && mActive && mPendingLoadRaisedPet && SanctuaryManager.pInstance != null && RsResourceManager.pCurrentLevel == SanctuaryManager.pLevelName && mRaisedPetDataString.Length > 0)
		{
			mPendingLoadRaisedPet = false;
			string userdata;
			RaisedPetData raisedPetData = RaisedPetData.LoadFromResString(mRaisedPetDataString, out userdata);
			UtDebug.Log("MMO RP String :::: " + mRaisedPetDataString);
			bool flag = false;
			if (mSanctuaryPet == null || mRaisedPetData == null || raisedPetData.Geometry != mRaisedPetData.Geometry || raisedPetData.pStage != mRaisedPetData.pStage)
			{
				flag = true;
			}
			if (!flag && (mRaisedPetData.GetAccessoryItemID(RaisedPetAccType.Saddle) != raisedPetData.GetAccessoryItemID(RaisedPetAccType.Saddle) || mRaisedPetData.GetAccessoryItemID(RaisedPetAccType.Materials) != raisedPetData.GetAccessoryItemID(RaisedPetAccType.Materials)))
			{
				flag = true;
			}
			int result = -1;
			int.TryParse(userdata, out result);
			OnRaisedPetDataReady(raisedPetData, flag, result);
		}
		if (mSanctuaryPet != null)
		{
			if (mMountRaisedPet && !mSanctuaryPet.pMountPending)
			{
				mMountRaisedPet = false;
				if (mMountTypeRaisedPet == PetSpecialSkillType.RUN && mSanctuaryPet.pCurrentSkillType == PetSpecialSkillType.FLY)
				{
					mSanctuaryPet.OnFlyLanding(base.gameObject);
				}
				else
				{
					mSanctuaryPet.Mount(base.gameObject, mMountTypeRaisedPet);
				}
				RaisedPetData.LoadFromResString(mRaisedPetDataString, out var userdata2);
				mSanctuaryPet.ProcessPowerUpData(userdata2);
			}
			else if (mDismountRaisedPet)
			{
				mDismountRaisedPet = false;
				mSanctuaryPet.OnFlyDismount(base.gameObject);
			}
		}
		if (!pCanAvatarShow)
		{
			SetVisible(Visible: false);
		}
		if (mRaisedPetSetPending && !mRaisedPetLoading)
		{
			mRaisedPetSetPending = false;
			mPendingLoadRaisedPet = true;
		}
		if (mLevelChanged)
		{
			mLevelChanged = false;
			Activate(mLevel == RsResourceManager.pCurrentLevel && !MainStreetMMOClient.pInstance.pAllDeactivated);
		}
		if (mReloading && mAvatarData.pIsReady)
		{
			mReloading = false;
			if (mMeshObject == null)
			{
				mMeshObject = mController._MainRoot;
			}
		}
		if (mController.pSubState == AvAvatarSubState.GLIDING && mAnimator != null)
		{
			float @float = mAnimator.GetFloat("fVertical");
			@float = ((@float == 0f) ? (mController.pFlyingPitch * -3f) : Mathf.Lerp(@float, mController.pFlyingPitch * -3f, Time.deltaTime * 3f));
			mAnimator.SetFloat("fVertical", @float);
		}
		UpdateMMOData();
		mController.UpdateWaterSplash();
		if (pMMOInit)
		{
			if (mBusy && mController.pSubState == AvAvatarSubState.NORMAL && mController.pProperties != null && mController.pProperties._BusyProp != null)
			{
				if (mBusyProp == null)
				{
					mBusyProp = Object.Instantiate(mController.pProperties._BusyProp);
					Transform parent = mTransform.Find(AvatarSettings.pInstance._BoneSettings.BUSY_PROP_BONE);
					mBusyProp.transform.parent = parent;
					mBusyProp.transform.localPosition = Vector3.zero;
					mBusyProp.transform.localRotation = Quaternion.identity;
				}
			}
			else if (!mBusy && mBusyProp != null)
			{
				mBusyProp.transform.parent = null;
				mBusyProp.SetActive(value: false);
				Object.Destroy(mBusyProp);
				mController.pCurrentAnim = "";
			}
		}
		UpdateOptimization();
	}

	private bool ContainsFlag(int values, int flag)
	{
		return (values & flag) > 0;
	}

	private void OccupyCouch(int inId)
	{
		if (inId == -1)
		{
			mController.pState = AvAvatarState.IDLE;
			return;
		}
		foreach (ObCouch pCouch in ObCouch.pCouchList)
		{
			ObCouchAttributes[] couchAttributes = pCouch._CouchAttributes;
			foreach (ObCouchAttributes obCouchAttributes in couchAttributes)
			{
				if (obCouchAttributes._ID == inId)
				{
					pCouch.OccupyCouch(obCouchAttributes, mController.gameObject);
					return;
				}
			}
		}
	}

	private void LeaveCouch(GameObject go)
	{
		if (ObCouch.pCouchList == null)
		{
			return;
		}
		foreach (ObCouch pCouch in ObCouch.pCouchList)
		{
			ObCouchAttributes[] couchAttributes = pCouch._CouchAttributes;
			foreach (ObCouchAttributes obCouchAttributes in couchAttributes)
			{
				if (obCouchAttributes.pOccupiedAvatar == go)
				{
					pCouch.LeaveCouch(obCouchAttributes);
					return;
				}
			}
		}
	}

	private void RestoreIdleState()
	{
		GameObject.Find("PfGemMineManager").SendMessage("RemoveAxe", mAvatarData.mAvatar, SendMessageOptions.DontRequireReceiver);
		mController.pState = AvAvatarState.IDLE;
	}

	public virtual void PrepareForLoading(AvatarData avatarData, bool reload)
	{
		mLoaded = false;
		mReloading = reload;
		if (mAvatarData.mInstance == null || mAvatarData.mInstance != avatarData || (!UtPlatform.IsMobile() && (mAvatarData.IsDefaultSaved() || AvatarData.InstanceInfo.IsDefaultSaved(avatarData))))
		{
			mAvatarData.mInstance = avatarData;
			OnCreated();
			AvAvatarController component = base.gameObject.GetComponent<AvAvatarController>();
			component?.UpdateDisplayName(component.pPlayerMounted, mAvatarData.mInstance._Group != null);
		}
	}

	public bool DoLoad()
	{
		if (mLoaded)
		{
			return true;
		}
		mLoaded = true;
		Load(mAvatarData.mInstance, mReloading);
		return false;
	}

	public virtual void Load(AvatarData avatarData, bool reload)
	{
		if (mAvatarData.mInstance == null)
		{
			mAvatarData.mInstance = avatarData;
			OnCreated();
		}
		mAvatarData.mInstance._Group = mGroup;
		mReloading = reload;
		AvatarData.Load(mAvatarData, avatarData);
		DataCache.Set(mUserID + "_AvatarData", avatarData);
	}

	public void Load(bool reload)
	{
		mReloading = reload;
		mAvatarData.mLoading = true;
		WsWebService.GetUserProfileByUserID(mUserID, GetProfileEventHandler, null);
	}

	public void UpdateProfileData()
	{
		WsWebService.GetUserProfileByUserID(mUserID, GetProfileEventHandler, null);
	}

	public void RemoveRaisedPet(bool unload = true)
	{
		if (mRaisedPetData != null && unload)
		{
			mRaisedPetData.pAbortCreation = true;
			mRaisedPetLoading = false;
		}
		mPendingLoadRaisedPet = false;
		mMountRaisedPet = false;
		mDismountRaisedPet = false;
		if (mSanctuaryPet != null)
		{
			if (SanctuaryManager.pInstance == null || !SanctuaryManager.pInstance._PoolingEnabled)
			{
				if (unload)
				{
					SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(mSanctuaryPet.pData.PetTypeID);
					if (sanctuaryPetTypeInfo != null)
					{
						RsResourceManager.Unload(sanctuaryPetTypeInfo._PetTextureRes);
						if (mSanctuaryPet.pData.Accessories != null && mSanctuaryPet.pData.Accessories.Length != 0)
						{
							RaisedPetAccessory[] accessories = mSanctuaryPet.pData.Accessories;
							foreach (RaisedPetAccessory raisedPetAccessory in accessories)
							{
								RsResourceManager.Unload(mSanctuaryPet.pData.GetAccessoryGeometry(raisedPetAccessory.Type));
								RsResourceManager.Unload(mSanctuaryPet.pData.GetAccessoryTexture(raisedPetAccessory.Type));
							}
						}
						if (mSanctuaryPet.pData.Geometry != null)
						{
							RsResourceManager.Unload(mSanctuaryPet.pData.Geometry);
						}
						RsResourceManager.Unload("RS_SHARED/DW" + sanctuaryPetTypeInfo._Name + "AnimBasic");
					}
					else
					{
						UtDebug.LogError("Couldn't find pet of type " + mSanctuaryPet.pData.PetTypeID);
					}
				}
				Object.Destroy(mSanctuaryPet.gameObject);
			}
			else
			{
				if (pController.pPlayerMounted)
				{
					mSanctuaryPet.OnFlyDismount(base.gameObject);
				}
				if (PoolManager.Pools.TryGetValue("_Dragons", out var spawnPool))
				{
					mSanctuaryPet.transform.parent = spawnPool.transform;
					spawnPool.Despawn(mSanctuaryPet.transform);
					if (!MainStreetMMOClient.pIsMMOEnabled)
					{
						mSanctuaryPet.transform.parent = null;
						spawnPool.RemoveFromPool(mSanctuaryPet.transform);
						Object.Destroy(mSanctuaryPet.gameObject);
					}
					else
					{
						NavMeshAgent component = mSanctuaryPet.gameObject.GetComponent<NavMeshAgent>();
						if (component != null)
						{
							component.enabled = false;
						}
					}
				}
				else
				{
					Object.Destroy(mSanctuaryPet.gameObject);
				}
			}
		}
		mSanctuaryPet = null;
	}

	public void Unload()
	{
		mLimbo = true;
		if (mController != null && mController.pPetObject != null)
		{
			mController.pPetObject.OnAvatarDetachPets();
		}
		RemoveRaisedPet();
		LeaveCouch(mAvatarData.mAvatar);
		mAvatarData.Release();
	}

	public void Activate(bool active)
	{
		if (active)
		{
			if (!pIsReady || !(mLevel == RsResourceManager.pCurrentLevel) || !MainStreetMMOClient.pInstance.IsLevelMMO(mLevel))
			{
				return;
			}
			UtDebug.Log("MMO: Active true for " + ((mAvatarData.mInstance != null) ? mAvatarData.mInstance.DisplayName : "") + "(" + mUserID + ")", MainStreetMMOClient.LOG_MASK);
			mFirstPosition = true;
			if (mUserVarData.Count > 0)
			{
				SetToUserVarData(mUserVarData[0], useY: true);
			}
			if (!mActive)
			{
				if (pMMOInit && mPetDataPet != null)
				{
					PetBundleLoader.LoadPetsForAvatar(base.gameObject, mPetDataPet);
				}
				if (!mPendingLoadRaisedPet && mSanctuaryPet == null && mRaisedPetDataString != "" && !mRaisedPetLoading)
				{
					mRaisedPetSetPending = false;
					mPendingLoadRaisedPet = true;
				}
			}
			if (pController != null)
			{
				pController.CreateProjectileCollider();
			}
			mActive = true;
			return;
		}
		UtDebug.Log("MMO: Active false for " + ((mAvatarData.mInstance != null) ? mAvatarData.mInstance.DisplayName : "") + "(" + mUserID + ")", MainStreetMMOClient.LOG_MASK);
		RemoveRaisedPet();
		mPendingLoadRaisedPet = false;
		SetVisible(Visible: false);
		if (mController != null)
		{
			if (mController.pPetObject != null)
			{
				mController.pPetObject.OnAvatarDetachPets();
			}
			mActive = false;
			mController.transform.position = new Vector3(0f, -5000f, 0f);
			mController.pState = AvAvatarState.NONE;
			Transform transform = mTransform.Find("AvatarEmoticon");
			if ((bool)transform)
			{
				Object.Destroy(transform.gameObject);
			}
			Transform transform2 = pObject.transform.Find("PfChatBubble");
			if ((bool)transform2)
			{
				Object.Destroy(transform2.gameObject);
			}
		}
		else
		{
			Debug.LogError("ERROR: MMO AVATAR CONTROLLER IS NULL WHEN CALLING ACTIVATE -> FALSE!!");
		}
	}

	public void SetKey(string inKey, string inValue)
	{
		mKeys[inKey] = inValue;
	}

	public string GetLevel()
	{
		return mLevel;
	}

	public void SetLevel(string level)
	{
		UtDebug.Log("MMO: SetLevel (" + level + ") for " + ((mAvatarData.mInstance != null) ? mAvatarData.mInstance.DisplayName : "") + "(" + mUserID + ")", MainStreetMMOClient.LOG_MASK);
		mUserVarData.Clear();
		mLevelChanged = true;
		mLevel = level;
	}

	public void SetMember(bool isMember)
	{
		UtDebug.Log("MMO: SetMember for " + ((mAvatarData.mInstance != null) ? mAvatarData.mInstance.DisplayName : "") + "(" + mUserID + ")", MainStreetMMOClient.LOG_MASK2);
		mMember = isMember;
		AvatarData.AddMemberToDisplayName(mAvatarData.mAvatar, AvatarData.pDisplayOtherName, isMember);
	}

	public void SetCountry(string url)
	{
		pAvatarData.LoadCountry(url);
	}

	public void SetMood(string url)
	{
		pAvatarData.LoadMood(url);
	}

	public void SetGroup(Group inGroup)
	{
		AvatarData.SetGroupName(pAvatarData.mAvatar, inGroup);
		mGroup = inGroup;
		AvAvatarController component = base.gameObject.GetComponent<AvAvatarController>();
		component?.UpdateDisplayName(component.pPlayerMounted, mGroup != null);
	}

	private void GroupsEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE)
		{
			GetGroupsResult getGroupsResult = (GetGroupsResult)inObject;
			if (getGroupsResult != null && getGroupsResult.Success)
			{
				Group.AddGroup(getGroupsResult.Groups[0]);
				AvatarData.SetGroupName(pAvatarData.mAvatar, getGroupsResult.Groups[0]);
				AvAvatarController component = base.gameObject.GetComponent<AvAvatarController>();
				component?.UpdateDisplayName(component.pPlayerMounted, mGroup != null);
			}
		}
	}

	public void SetJoinAllowed(MMOJoinStatus canBeJoined)
	{
		UtDebug.Log("MMO: SetJoinAllowed for " + ((mAvatarData.mInstance != null) ? mAvatarData.mInstance.DisplayName : "") + "(" + mUserID + ")", MainStreetMMOClient.LOG_MASK2);
		mJoinAllowed = canBeJoined;
	}

	public void SetPet(PetDataPet petData)
	{
		mPetDataPet = petData;
		if (mActive)
		{
			if (mPetDataPet != null)
			{
				PetBundleLoader.LoadPetsForAvatar(base.gameObject, mPetDataPet);
			}
			else
			{
				PetBundleLoader.DetachPetsForAvatar(base.gameObject);
			}
		}
	}

	public virtual void SetRaisedPet(string pdata)
	{
		if (MainStreetMMOClient.pInstance.IsLevelMMO(RsResourceManager.pCurrentLevel))
		{
			mRaisedPetDataString = pdata;
			if (!string.IsNullOrEmpty(pdata))
			{
				mRaisedPetSetPending = true;
				return;
			}
			RemoveRaisedPet();
			mRaisedPetSetPending = false;
		}
	}

	public void SetRaisedPetPosition(Vector3 petPosition)
	{
		mSanctuaryPet.SetPosition(petPosition);
	}

	public void SetRaisedPetState(AISanctuaryPetFSM petState)
	{
		mSanctuaryPet.AIActor.SetState(petState);
	}

	public void SetBusy(bool busy)
	{
		mBusy = busy;
	}

	public void DisplayMember()
	{
		if (pMMOInit)
		{
			AvatarData.AddMemberToDisplayName(mAvatarData.mAvatar, AvatarData.pDisplayOtherName, mMember);
		}
	}

	public void DisplayRank()
	{
		if (mRankData != null)
		{
			AvatarData.AddRankToDisplayName(mAvatarData.mAvatar, null, AvatarData.pDisplayOtherName);
		}
	}

	public void Emote(int id)
	{
		MMOAvatarUserVarData mMOAvatarUserVarData = new MMOAvatarUserVarData();
		mMOAvatarUserVarData._ID = id;
		mMOAvatarUserVarData._TimeStamp = mTime;
		mEmoteID = mMOAvatarUserVarData;
	}

	public void Action(int id)
	{
		MMOAvatarUserVarData mMOAvatarUserVarData = new MMOAvatarUserVarData();
		mMOAvatarUserVarData._ID = id;
		mMOAvatarUserVarData._TimeStamp = mTime;
		mActionID = mMOAvatarUserVarData;
	}

	public void Chat(int id, ChatOptions channel)
	{
		MMOAvatarUserVarData mMOAvatarUserVarData = new MMOAvatarUserVarData();
		mMOAvatarUserVarData._ID = id;
		mMOAvatarUserVarData._TimeStamp = mTime;
		mChatID = mMOAvatarUserVarData;
		mChannel = channel;
	}

	public void Chat(string chat, ChatOptions channel)
	{
		MMOAvatarUserVarData mMOAvatarUserVarData = new MMOAvatarUserVarData();
		mMOAvatarUserVarData._Data = chat;
		mMOAvatarUserVarData._TimeStamp = mTime;
		mChatID = mMOAvatarUserVarData;
		mChannel = channel;
	}

	public void Add(MMOAvatarUserVarData uvd)
	{
		uvd._TimeStamp = mTime;
		mUserVarData.Add(uvd);
		ProcessNetworkData(uvd._Position, uvd._Rotation, uvd._Velocity, uvd._MaxSpeed, uvd._ServerTimeStamp);
		pLastMMOUpdate = Time.fixedTime;
	}

	private void SetToUserVarData(MMOAvatarUserVarData uvd, bool useY)
	{
		if (mFirstPosition)
		{
			mController.transform.position = uvd._Position;
			mController.transform.rotation = uvd._Rotation;
		}
		if (mSanctuaryPet != null && !mSanctuaryPetSynced)
		{
			mSanctuaryPetSynced = true;
			mSanctuaryPet.SetAvatar(base.transform);
			if (!MainStreetMMOClient.pInstance.IsLevelRacing())
			{
				mSanctuaryPet.SetFollowAvatar(follow: true);
				mSanctuaryPet.MoveToAvatar(postponed: true);
			}
		}
		if (mFirstPosition && mTeleportAvatar)
		{
			mTeleportAvatar = false;
			MainStreetMMOClient.pInstance.TeleportToBuddy(mUserID);
		}
		if (mFirstPosition && mController.pState == AvAvatarState.NONE)
		{
			mController.pState = AvAvatarState.IDLE;
		}
		mFirstPosition = false;
		mController.pVelocity = Vector3.zero;
	}

	private void GetRankEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE)
		{
			mRankData = (UserAchievementInfo)inObject;
		}
	}

	private void RankImageLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			mRankTexture = (Texture)inObject;
			AvatarData.AddRankToDisplayName(mAvatarData.mAvatar, mRankTexture, AvatarData.pDisplayOtherName);
		}
	}

	private void GetProfileEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE)
		{
			if (mAvatarData.mAvatar == null)
			{
				UtDebug.LogError("Avatar destroyed before GetUserProfile returned!");
				return;
			}
			mProfileData = (UserProfileData)inObject;
			AvatarData.Load(mAvatarData, mProfileData.AvatarInfo.AvatarData);
			DataCache.Set(mUserID + "_AvatarData", mProfileData.AvatarInfo.AvatarData);
			mAvatarData.LoadCountry(mProfileData.GetCountryURL(mAvatarData.mAvatar));
			SetMember(mProfileData.AvatarInfo.IsMember());
			GetRankEventHandler(WsServiceType.GET_USER_ACHIEVEMENT_INFO, WsServiceEvent.COMPLETE, 1f, UserRankData.GetUserAchievementInfoByType(mProfileData.AvatarInfo.Achievements, 1), null);
		}
	}

	public void OnActivate()
	{
		UiPlayerInfoDB.ShowPlayerInfo(mUserID);
	}

	public void TeleportAvatar()
	{
		mTeleportAvatar = true;
	}

	public int GetRideItemID()
	{
		return mRideItemID;
	}

	public void SetRide(int rideItemID)
	{
		if (rideItemID != mRideItemID)
		{
			mRideItemID = rideItemID;
			UtDebug.Log("MMO: SetRide for " + ((mAvatarData.mInstance != null) ? mAvatarData.mInstance.DisplayName : "") + "(" + mUserID + ") ride = " + rideItemID, MainStreetMMOClient.LOG_MASK);
		}
	}

	public virtual void OnPetReady(SanctuaryPet pet)
	{
		mRaisedPetLoading = false;
		if (!(pet == null))
		{
			mSanctuaryPetSynced = false;
			mSanctuaryPet = pet;
			pet.SetAvatar(base.transform);
			if (!MainStreetMMOClient.pInstance.IsLevelRacing())
			{
				pet.SetFollowAvatar(follow: true);
				pet.MoveToAvatar(postponed: true);
			}
			pet.LoadAvatarMountAnimation();
			pet.SetClickActivateObject(null);
			SetVisible(Visible: true, bForced: true);
			if (mRaisedPetData != null && mRaisedPetData.pGlowEffect != null && !string.IsNullOrEmpty(mRaisedPetData.pGlowEffect.GlowColor))
			{
				mSanctuaryPet.ApplyGlowEffect(mRaisedPetData.pGlowEffect.GlowColor);
			}
		}
	}

	public void OnRaisedPetDataReady(RaisedPetData rd, bool bRespawn, int mountType)
	{
		if (rd == null)
		{
			Debug.LogError("Pet data is null");
		}
		else
		{
			if (rd.pIsSleeping || rd.pAbortCreation)
			{
				return;
			}
			string text = ((rd.pGlowEffect != null) ? rd.pGlowEffect.GlowColor : "");
			string text2 = ((mRaisedPetData != null && mRaisedPetData.pGlowEffect != null) ? mRaisedPetData.pGlowEffect.GlowColor : "");
			mRaisedPetData = rd;
			if (bRespawn || mSanctuaryPet == null)
			{
				RemoveRaisedPet(unload: false);
				mRaisedPetLoading = true;
				if (SanctuaryManager.pInstance != null && SanctuaryManager.IsPetInstanceAllowed(rd))
				{
					SanctuaryManager.CreatePet(mUserID, rd, SanctuaryManager.pInstance._PetOffScreenPosition, Quaternion.identity, base.gameObject, "Basic", applyCustomSkin: true);
				}
			}
			if (mountType != -1)
			{
				mMountRaisedPet = true;
				mMountRaisedPet = true;
				mMountTypeRaisedPet = (PetSpecialSkillType)mountType;
			}
			else
			{
				mDismountRaisedPet = true;
			}
			if (!(mSanctuaryPet != null))
			{
				return;
			}
			mSanctuaryPet.pData = mRaisedPetData;
			mSanctuaryPet.UpdateShaders();
			if (text != text2)
			{
				if (mSanctuaryPet.pCustomSkinAvailable)
				{
					mSanctuaryPet.UpdateAccessories();
				}
				if (string.IsNullOrEmpty(text))
				{
					mSanctuaryPet.RemoveGlowEffect(playFx: true);
				}
				else
				{
					mSanctuaryPet.ApplyGlowEffect(text);
				}
			}
		}
	}

	public virtual void OnAvatarTypeChanged()
	{
	}

	public virtual void UpdateMMOData()
	{
		if (mAvatarData == null || mAvatarData.mInstance == null)
		{
			return;
		}
		mTime = Time.realtimeSinceStartup;
		float num = mTime;
		int i = 0;
		if (!mActive)
		{
			mInactiveTimer += Time.deltaTime;
			if (mInactiveTimer > mInactiveTimeout && !MainStreetMMOClient.pInstance.pAllDeactivated)
			{
				mActive = true;
				return;
			}
			for (; i < mUserVarData.Count && !((double)num < mUserVarData[i]._TimeStamp); i++)
			{
			}
			if (mUserVarData.Count > 0 && i <= mUserVarData.Count)
			{
				while (i > 1)
				{
					mUserVarData.RemoveAt(0);
					i--;
				}
				if (mTeleportAvatar)
				{
					mTeleportAvatar = false;
					MainStreetMMOClient.pInstance.TeleportToBuddy(mUserID);
				}
			}
			if (mEmoteID != null && (double)num > mEmoteID._TimeStamp)
			{
				mEmoteID = null;
			}
			if (mActionID != null && (double)num > mActionID._TimeStamp)
			{
				mActionID = null;
			}
			if (mChatID != null && (double)num > mChatID._TimeStamp)
			{
				mChatID = null;
			}
			return;
		}
		for (i = 0; i < mUserVarData.Count; i++)
		{
			if (!((double)num >= mUserVarData[i]._TimeStamp))
			{
				continue;
			}
			MMOAvatarFlags flags = mUserVarData[i]._Flags;
			if (flags <= MMOAvatarFlags.UWSWIMMING)
			{
				if (flags != 0)
				{
					UtDebug.LogFastM(MainStreetMMOClient.LOG_MASK2, "MMO: Flags for ", mAvatarData.mInstance.DisplayName, "(", mUserID, ") = ", flags);
				}
				if (mController.pSubState != (AvAvatarSubState)flags && mController.pSubState != AvAvatarSubState.FLYING && flags != MMOAvatarFlags.FLYING)
				{
					mController.pSubState = (AvAvatarSubState)flags;
				}
			}
			else
			{
				switch (flags)
				{
				case MMOAvatarFlags.SETPOSITION:
					UtDebug.LogFastM(MainStreetMMOClient.LOG_MASK2, "MMO: SetPosition for ", mAvatarData.mInstance.DisplayName, "(", mUserID, ")");
					SetToUserVarData(mUserVarData[i], useY: true);
					if (mController.pPetObject != null)
					{
						mController.pPetObject.OnAvatarSetPositionDone(mUserVarData[i]._Position);
					}
					break;
				case MMOAvatarFlags.SPRINGBOARD:
					UtDebug.LogFastM(MainStreetMMOClient.LOG_MASK2, "MMO: Springboard for ", mAvatarData.mInstance.DisplayName, "(", mUserID, ")");
					if (mUserVarData[i]._GameObject != null)
					{
						mUserVarData[i]._GameObject.GetComponent<ObSpringBoard>().Use(pObject);
					}
					break;
				case MMOAvatarFlags.JUMP:
					UtDebug.LogFastM(MainStreetMMOClient.LOG_MASK2, "MMO: Jump for ", mAvatarData.mInstance.DisplayName, "(", mUserID, ")");
					mController.pVelocity = new Vector3(mController.pVelocity.x, mController.GetJumpVelocity(mController.pCurrentStateData._JumpValues._MinJumpHeight), mController.pVelocity.z);
					mController.pCurrentAnim = mController.pCurrentStateData._JumpValues._JumpAnim;
					break;
				case MMOAvatarFlags.RELEASERAISEDPET:
					RemoveRaisedPet();
					break;
				case MMOAvatarFlags.COINREWARDRECEIVED:
					mController.OnCoinsReceived();
					break;
				case MMOAvatarFlags.GEMDIG:
					GameObject.Find("PfGemMineManager").SendMessage("AddAxe", mAvatarData.mAvatar, SendMessageOptions.DontRequireReceiver);
					mController.pState = AvAvatarState.NONE;
					AvAvatar.PlayAnim(mAvatarData.mAvatar, "GemDig", WrapMode.Loop);
					Invoke("RestoreIdleState", 1f);
					break;
				case MMOAvatarFlags.STARTBOUNCE:
					mController.StartBouncing();
					break;
				case MMOAvatarFlags.STOPBOUNCE:
					mController.StopBouncingObject();
					break;
				case MMOAvatarFlags.COUCHSIT:
					OccupyCouch(mUserVarData[i]._ID);
					break;
				case MMOAvatarFlags.CARRYOBJECT:
					if (mUserVarData[i]._GameObject != null)
					{
						mController.Collect(mUserVarData[i]._GameObject);
					}
					break;
				case MMOAvatarFlags.DROPOBJECT:
					mController.RemoveCarriedObject();
					break;
				case MMOAvatarFlags.SENDOBJECTMESSAGE:
					if (mUserVarData[i]._GameObject != null)
					{
						mUserVarData[i]._GameObject.SendMessage(mUserVarData[i]._Data, pObject);
					}
					break;
				}
			}
			if (mUserVarData[i]._Flags > MMOAvatarFlags.UWSWIMMING)
			{
				MMOAvatarUserVarData mMOAvatarUserVarData = mUserVarData[i];
				mMOAvatarUserVarData._Flags = MMOAvatarFlags.NONE;
				mUserVarData[i] = mMOAvatarUserVarData;
			}
		}
		UpdatePlayer();
		if (i > 1)
		{
			if (i == mUserVarData.Count && (mFirstPosition || (mController.pState != 0 && mController.pState != AvAvatarState.ONSPLINE)))
			{
				SetToUserVarData(mUserVarData[i - 1], useY: false);
			}
			while (i > 1)
			{
				mUserVarData.RemoveAt(0);
				i--;
			}
		}
		if (mEmoteID != null && (double)num > mEmoteID._TimeStamp)
		{
			UtDebug.Log("MMO: Emote " + mEmoteID._ID + " for " + mAvatarData.mInstance.DisplayName + "(" + mUserID + ")", MainStreetMMOClient.LOG_MASK2);
			mController.OnEmote(mEmoteID._ID);
			mEmoteID = null;
		}
		if (mActionID != null && (double)num > mActionID._TimeStamp)
		{
			UtDebug.Log("MMO: Action " + mActionID._ID + " for " + mAvatarData.mInstance.DisplayName + "(" + mUserID + ")", MainStreetMMOClient.LOG_MASK2);
			mController.OnAction(mActionID._ID);
			mActionID = null;
		}
		if (mChatID != null && (double)num > mChatID._TimeStamp)
		{
			if (mChatID._ID > 0)
			{
				UtDebug.Log("MMO: Chat " + mChatID._ID + " for " + mAvatarData.mInstance.DisplayName + "(" + mUserID + ")", MainStreetMMOClient.LOG_MASK2);
				mController.OnChat(mChatID._ID);
			}
			else
			{
				UtDebug.Log("MMO: Chat " + mChatID._Data + " for " + mAvatarData.mInstance.DisplayName + "(" + mUserID + ")", MainStreetMMOClient.LOG_MASK2);
				mController.OnChat(mChatID._Data, mChannel);
			}
			Transform transform = pObject.transform.Find("PfChatBubble");
			if (transform != null && transform.gameObject.GetComponent<ObFaceAvatarCamera>() == null)
			{
				transform.gameObject.AddComponent<ObFaceAvatarCamera>();
			}
			mChatID = null;
		}
	}

	protected virtual bool UpdateOptimization()
	{
		mOptimizationCheckTimer -= Time.deltaTime;
		if (!pIsReady || MainStreetMMOClient.pInstance == null || MainStreetMMOClient.pInstance.pState != MMOClientState.IN_ROOM || mOptimizationCheckTimer > 0f || mOptimizationData == null)
		{
			return false;
		}
		float sqrMagnitude = (mTransform.position - AvAvatar.position).sqrMagnitude;
		Vector3 rhs = mTransform.position - AvAvatar.position;
		if (Vector3.Dot(AvAvatar.forward, rhs) < 0f)
		{
			mIsVisible = false;
		}
		else
		{
			mIsVisible = true;
		}
		if ((bool)mSanctuaryPet)
		{
			bool flag = pController.pPlayerMounted && !mSanctuaryPet.gameObject.activeSelf;
			if (!pController.pPlayerMounted && sqrMagnitude > mOptimizationData.RaisedPetRemoveDistance * mOptimizationData.RaisedPetRemoveDistance)
			{
				if (!ContainsFlag(mOptimizedFlags, 2))
				{
					if ((bool)mSanctuaryPet)
					{
						mSanctuaryPet.gameObject.SetActive(value: false);
					}
					mOptimizedFlags |= 2;
				}
			}
			else if (ContainsFlag(mOptimizedFlags, 2))
			{
				flag = true;
				if (mColliders == null)
				{
					mColliders = mSanctuaryPet.GetComponentsInChildren<Collider>();
				}
				Collider[] array = mColliders;
				foreach (Collider collider in array)
				{
					if (collider != null && !collider.isTrigger)
					{
						collider.enabled = false;
					}
				}
				mOptimizedFlags ^= 2;
			}
			if (flag)
			{
				mSanctuaryPet.gameObject.SetActive(value: true);
				mSanctuaryPet.MoveToAvatar();
			}
		}
		if (sqrMagnitude > mOptimizationData.DisplayRemoveDistance * mOptimizationData.DisplayRemoveDistance)
		{
			if (!ContainsFlag(mOptimizedFlags, 1))
			{
				if (mSkinnedMeshRenderers == null)
				{
					mSkinnedMeshRenderers = pObject.GetComponentsInChildren<SkinnedMeshRenderer>();
				}
				SkinnedMeshRenderer[] array2 = mSkinnedMeshRenderers;
				foreach (SkinnedMeshRenderer skinnedMeshRenderer in array2)
				{
					if (skinnedMeshRenderer != null)
					{
						skinnedMeshRenderer.receiveShadows = false;
						skinnedMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
					}
				}
				AvatarData.SetDisplayNameVisible(pObject, inVisible: false, pIsMember);
				mOptimizedFlags |= 1;
			}
		}
		else if (ContainsFlag(mOptimizedFlags, 1))
		{
			AvatarData.SetDisplayNameVisible(pObject, AvatarData.pDisplayOtherName, pIsMember);
			if (mSkinnedMeshRenderers != null)
			{
				SkinnedMeshRenderer[] array2 = mSkinnedMeshRenderers;
				foreach (SkinnedMeshRenderer skinnedMeshRenderer2 in array2)
				{
					if (skinnedMeshRenderer2 != null)
					{
						skinnedMeshRenderer2.receiveShadows = true;
						skinnedMeshRenderer2.shadowCastingMode = ShadowCastingMode.On;
					}
				}
			}
			mOptimizedFlags ^= 1;
		}
		mOptimizationCheckTimer = GameDataConfig.pInstance.OptimizationData.UpdateFrequency;
		return true;
	}

	public float TimeSinceLastSwitch()
	{
		return Time.realtimeSinceStartup - mLastSwitchTime;
	}

	public virtual void SetVisible(bool Visible, bool bForced = false)
	{
	}

	public void SetUDTPoints(int inPoints)
	{
		if (mController != null)
		{
			mController.SetUDTStarsVisible(AvatarData.pDisplayOtherName, inPoints);
		}
		UtDebug.Log("MMO: SetUDTPoints for " + ((mAvatarData.mInstance != null) ? mAvatarData.mInstance.DisplayName : "") + "(" + mUserID + ") UDT Points = " + inPoints, MainStreetMMOClient.LOG_MASK);
	}

	public void HandleUserEvent(string userEvent)
	{
		string[] array = userEvent.Split(':');
		int num = 1;
		if (!(array[num] == "WF") && !(array[num] == "WFWT"))
		{
			return;
		}
		Transform transform = null;
		if (array[num] == "WFWT")
		{
			string text = array[num + 2];
			if (text == UserInfo.pInstance.UserID)
			{
				if (AvAvatar.pObject != null)
				{
					transform = AvAvatar.mTransform;
				}
			}
			else if (MainStreetMMOClient.pInstance.pPlayerList.ContainsKey(text))
			{
				if (MainStreetMMOClient.pInstance.pPlayerList[text] != null)
				{
					transform = MainStreetMMOClient.pInstance.pPlayerList[text].transform;
				}
			}
			else
			{
				GameObject gameObject = GameObject.Find(text);
				if (gameObject != null)
				{
					transform = gameObject.transform;
				}
			}
			SanctuaryPet sanctuaryPet = pSanctuaryPet;
			if (sanctuaryPet != null && sanctuaryPet.gameObject.activeSelf)
			{
				sanctuaryPet.Fire(transform, transform == null, (transform == null) ? mTransform.forward : Vector3.zero);
			}
			return;
		}
		Vector3 zero = Vector3.zero;
		float.TryParse(array[num + 2], out zero.x);
		float.TryParse(array[num + 3], out zero.y);
		float.TryParse(array[num + 4], out zero.z);
		if (zero.magnitude > float.Epsilon)
		{
			SanctuaryPet sanctuaryPet2 = pSanctuaryPet;
			if (sanctuaryPet2 != null && sanctuaryPet2.gameObject.activeSelf)
			{
				sanctuaryPet2.Fire(null, useDirection: true, zero, ignoreCoolDown: true);
			}
		}
	}
}
