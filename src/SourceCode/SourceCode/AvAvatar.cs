using System;
using KA.Framework;
using UnityEngine;

public class AvAvatar
{
	public const uint LOG_MASK = 2u;

	private static GameObject mObject;

	private static GameObject mCacheObject;

	private static AvAvatarState mState = AvAvatarState.NONE;

	private static AvAvatarSubState mSubState = AvAvatarSubState.NORMAL;

	private static AvAvatarLevelState mLevelState = AvAvatarLevelState.NORMAL;

	private static AvAvatarState mPrevState = mState;

	private static GameObject mAvatarCam;

	public static Transform pAvatarCamTransform;

	private static GameObject mToolbar;

	private static bool mInputEnabled = true;

	private static Vector3 mStartPosition = Vector3.zero;

	private static Quaternion mStartRotation = Quaternion.identity;

	private static string mStartLocation = null;

	public static Vector3 mNetworkVelocity = Vector3.zero;

	private static GameObject mDelayObject = null;

	public static Transform mTransform;

	private static Vector3 mAvatarCamPosition;

	private static Vector3 mAvatarCamForward;

	private static int mLastAvatarCamPositionUpdateFrame = 0;

	private static Vector3 mPosition;

	private static Vector3 mForward;

	private static int mLastPositionUpdateFrame = 0;

	private static string UserEventMsg = "";

	private static int UserEventSeqID = 0;

	public static GameObject pObject
	{
		get
		{
			return mObject;
		}
		set
		{
			mObject = value;
			if (mObject == null)
			{
				mTransform = null;
			}
			else
			{
				mTransform = mObject.transform;
			}
		}
	}

	public static GameObject OriginalObject
	{
		get
		{
			if (!(mCacheObject != null) || !(mCacheObject != mObject))
			{
				return mObject;
			}
			return mCacheObject;
		}
	}

	public static AvAvatarState pState
	{
		get
		{
			return mState;
		}
		set
		{
			mPrevState = mState;
			mState = value;
			if (mObject != null)
			{
				mObject.SendMessage("OnSetState", null, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public static AvAvatarSubState pSubState
	{
		get
		{
			return mSubState;
		}
		set
		{
			AvAvatarSubState avAvatarSubState = mSubState;
			mSubState = value;
			if (mObject != null)
			{
				mObject.SendMessage("OnSetSubState", avAvatarSubState, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public static AvAvatarLevelState pLevelState
	{
		get
		{
			return mLevelState;
		}
		set
		{
			mLevelState = value;
		}
	}

	public static AvAvatarState pPrevState => mPrevState;

	public static GameObject pAvatarCam
	{
		get
		{
			return mAvatarCam;
		}
		set
		{
			mAvatarCam = value;
			pAvatarCamTransform = ((value == null) ? null : value.transform);
		}
	}

	public static GameObject pToolbar
	{
		get
		{
			return mToolbar;
		}
		set
		{
			mToolbar = value;
		}
	}

	public static bool pInputEnabled
	{
		get
		{
			return mInputEnabled;
		}
		set
		{
			mInputEnabled = value;
		}
	}

	public static Animation pAnimation
	{
		get
		{
			if (mObject == null)
			{
				return null;
			}
			return mObject.GetComponentInChildren<Animation>();
		}
	}

	public static Vector3 pStartPosition
	{
		get
		{
			return mStartPosition;
		}
		set
		{
			mStartPosition = value;
		}
	}

	public static Quaternion pStartRotation
	{
		get
		{
			return mStartRotation;
		}
		set
		{
			mStartRotation = value;
		}
	}

	public static string pStartLocation
	{
		get
		{
			return mStartLocation;
		}
		set
		{
			mStartLocation = value;
		}
	}

	public static string pSpawnAtSetPosition => "Position";

	public static Vector3 AvatarCamPosition
	{
		get
		{
			if (Time.frameCount > mLastAvatarCamPositionUpdateFrame)
			{
				UpdateCameraCache();
			}
			return mAvatarCamPosition;
		}
		set
		{
			pAvatarCamTransform.position = value;
			mAvatarCamPosition = value;
			mLastAvatarCamPositionUpdateFrame = Time.frameCount;
		}
	}

	public static Vector3 pAvatarCamForward
	{
		get
		{
			if (Time.frameCount > mLastAvatarCamPositionUpdateFrame)
			{
				UpdateCameraCache();
			}
			return mAvatarCamForward;
		}
	}

	public static Vector3 position
	{
		get
		{
			if (Time.frameCount > mLastPositionUpdateFrame)
			{
				UpdateAvatarCache();
			}
			return mPosition;
		}
		set
		{
			mTransform.position = value;
			mPosition = value;
			mLastPositionUpdateFrame = Time.frameCount;
		}
	}

	public static Vector3 forward
	{
		get
		{
			if (Time.frameCount > mLastPositionUpdateFrame)
			{
				UpdateAvatarCache();
			}
			return mForward;
		}
	}

	public static void CacheAvatar()
	{
		mCacheObject = mObject;
	}

	public static void RestoreAvatar()
	{
		if ((bool)mCacheObject)
		{
			pObject = mCacheObject;
		}
	}

	private static void UpdateCameraCache()
	{
		mAvatarCamPosition = pAvatarCamTransform.position;
		mAvatarCamForward = pAvatarCamTransform.forward;
		mLastAvatarCamPositionUpdateFrame = Time.frameCount;
	}

	public static void AddUserEvent(string inMsg)
	{
		UserEventSeqID++;
		UserEventMsg = UserEventMsg + inMsg + ":";
	}

	public static bool HasUserEventMsg()
	{
		return !string.IsNullOrEmpty(UserEventMsg);
	}

	public static string GetUserEventMsg(bool bClear)
	{
		string result = UserEventSeqID + ":" + UserEventMsg + "::";
		if (bClear)
		{
			UserEventSeqID++;
			UserEventMsg = "";
		}
		return result;
	}

	private static void UpdateAvatarCache()
	{
		if (mTransform != null)
		{
			mPosition = mTransform.position;
			mForward = mTransform.forward;
		}
		else
		{
			UtDebug.LogError("Avatar transform is null");
		}
	}

	public static void Init()
	{
		if (mObject == null)
		{
			UserProfile.Init();
		}
	}

	public static void CreateDefault(AvatarData inAvatarData)
	{
		if (inAvatarData != null)
		{
			AvatarData.pInstanceInfo.mInstance = inAvatarData;
		}
		else
		{
			AvatarData.pInstanceInfo.mInstance = AvatarData.CreateDefault();
		}
		AvatarData.pInitialized = true;
		if (mObject == null)
		{
			CreateAvatar();
		}
		AvatarData.pInstanceInfo.mAvatar = mObject;
		AvatarData.pInstanceInfo.LoadBundlesAndUpdateAvatar();
	}

	public static void SpawnAvatar()
	{
		if (AvatarData.pInstance != null)
		{
			CreateAvatar();
		}
	}

	private static void CreateAvatar()
	{
		if (AvatarSettings.pInstance != null)
		{
			mObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources(GetAvatarPrefabName()));
			mObject.name = GetAvatarPrefabName();
			UnityEngine.Object.DontDestroyOnLoad(mObject);
			mTransform = mObject.transform;
		}
		else
		{
			UtDebug.Log("AvatarSettings is failed to load...");
		}
	}

	public static string GetAvatarPrefabName()
	{
		return "PfAvatar";
	}

	public static bool IsPlayerOnRide()
	{
		return false;
	}

	public static bool IsCurrentPlayer(GameObject avatar)
	{
		if (avatar == pObject)
		{
			return true;
		}
		return false;
	}

	public static void SetScale(Vector3 s)
	{
		if (mTransform != null)
		{
			mTransform.localScale = s;
		}
	}

	public static void SetPosition(Transform t)
	{
		if (mTransform != null)
		{
			mTransform.rotation = t.rotation;
			SetPosition(t.position);
		}
	}

	public static void SetPosition(Vector3 position)
	{
		if (mObject != null)
		{
			mObject.transform.position = position;
			mObject.SendMessage("OnSetPosition", position, SendMessageOptions.DontRequireReceiver);
		}
	}

	public static Vector3 GetPosition()
	{
		if (mTransform != null)
		{
			return mTransform.position;
		}
		return Vector3.zero;
	}

	public static Quaternion GetRotation()
	{
		if (mTransform != null)
		{
			return mTransform.rotation;
		}
		return Quaternion.identity;
	}

	public static bool TeleportToNPC(GameObject inNPC, bool useEffect)
	{
		Vector3 outPos = Vector3.zero;
		if (UtUtilities.FindPosNextToObject(out outPos, inNPC, 3f))
		{
			if (useEffect)
			{
				TeleportTo(outPos);
			}
			else
			{
				SetPosition(outPos);
			}
			if (mTransform != null)
			{
				mTransform.LookAt(inNPC.transform);
			}
			return true;
		}
		return false;
	}

	public static bool TeleportToObject(string inObjectName, float randomOffset = 0f, bool doTeleportFx = true, float ignoreTeleportOffset = 0f)
	{
		GameObject gameObject = GameObject.Find(inObjectName);
		if (gameObject != null)
		{
			return TeleportToObject(gameObject, randomOffset, doTeleportFx, ignoreTeleportOffset);
		}
		return false;
	}

	public static bool TeleportToObject(GameObject inObject, float randomOffset = 0f, bool doTeleportFx = true, float ignoreTeleportOffset = 0f)
	{
		TeleportTo(inObject.transform.position, inObject.transform.forward, randomOffset, doTeleportFx, ignoreTeleportOffset);
		return true;
	}

	public static void TeleportTo(Vector3 inPosition)
	{
		TeleportTo(inPosition, Vector3.zero);
	}

	public static void TeleportTo(Vector3 inPosition, Vector3 inDirection, float randomOffset = 0f, bool doTeleportFx = true, float ignoreTeleportOffset = 0f)
	{
		if (mTransform == null || ignoreTeleportOffset > Vector3.Distance(inPosition, mTransform.position))
		{
			return;
		}
		Vector3 vector = mTransform.position;
		if (randomOffset > 0f)
		{
			float num = UnityEngine.Random.Range(0, 24) * 15;
			float num2 = Mathf.Cos(num * (MathF.PI / 180f)) * randomOffset;
			float num3 = Mathf.Sin(num * (MathF.PI / 180f)) * randomOffset;
			inPosition.x += num2;
			inPosition.z += num3;
		}
		SetPosition(inPosition);
		if (inDirection != Vector3.zero)
		{
			mTransform.forward = inDirection;
		}
		if (doTeleportFx)
		{
			if ((double)(vector - inPosition).sqrMagnitude > 0.2)
			{
				TeleportFx.PlayAt(vector);
			}
			TeleportFx.PlayAt(inPosition);
		}
	}

	public static void SetActive(bool inActive)
	{
		SetOnlyAvatarActive(inActive);
		SetUIActive(inActive);
		if (pAvatarCam != null)
		{
			pAvatarCam.SetActive(inActive);
		}
	}

	public static void SetOnlyAvatarActive(bool active)
	{
		if (mObject == null)
		{
			return;
		}
		if (active)
		{
			CharacterController characterController = (CharacterController)mObject.GetComponent<Collider>();
			if ((bool)characterController)
			{
				characterController.detectCollisions = true;
			}
			mObject.SetActive(value: true);
			SetDisplayNameVisible(AvatarData.pDisplayYourName);
			mObject.SendMessage("OnActive", true, SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			CharacterController characterController2 = (CharacterController)mObject.GetComponent<Collider>();
			if ((bool)characterController2)
			{
				characterController2.detectCollisions = false;
			}
			mObject.SendMessage("OnActive", false, SendMessageOptions.DontRequireReceiver);
			mObject.SetActive(value: false);
		}
	}

	public static bool IsFlying()
	{
		if (pSubState != AvAvatarSubState.FLYING)
		{
			return pSubState == AvAvatarSubState.GLIDING;
		}
		return true;
	}

	public static void EnableAllInputs(bool inActive)
	{
		if (KAInput.pInstance != null)
		{
			KAInput.pInstance.ShowInputs(inActive);
		}
		if (IsFlying() && mObject != null)
		{
			mObject.SendMessage("DisableFlightControls", !inActive, SendMessageOptions.DontRequireReceiver);
		}
		ObClickable.pGlobalActive = inActive;
		if (mObject != null && !inActive)
		{
			mObject.SendMessage("ResetVelocity", SendMessageOptions.DontRequireReceiver);
		}
	}

	public static bool GetUIActive()
	{
		if (pToolbar != null)
		{
			return pToolbar.activeSelf;
		}
		return false;
	}

	public static void SetUIActive(bool inActive)
	{
		if (pToolbar != null)
		{
			if (inActive)
			{
				if (mDelayObject != null)
				{
					UnityEngine.Object.Destroy(mDelayObject);
				}
				pToolbar.SetActive(value: true);
			}
			else
			{
				pToolbar.SetActive(value: false);
			}
		}
		EnableAllInputs(inActive);
	}

	public static void SetDisplayNameVisible(bool inVisible)
	{
		if (!AvatarData.pDisplayYourName)
		{
			inVisible = false;
		}
		AvatarData.SetDisplayNameVisible(mObject, inVisible, SubscriptionInfo.pIsMember);
	}

	public static void UseProp(string name)
	{
		if ((bool)mObject)
		{
			mObject.SendMessage("UseProp", name, SendMessageOptions.DontRequireReceiver);
		}
	}

	public static void UseProp(PropStates prop)
	{
		if ((bool)mObject)
		{
			mObject.SendMessage("UseProp", prop, SendMessageOptions.DontRequireReceiver);
		}
	}

	public static void StopUseProp()
	{
		if ((bool)mObject)
		{
			mObject.SendMessage("StopUseProp", SendMessageOptions.DontRequireReceiver);
		}
	}

	public static void PlayCannedAnim(string animName)
	{
		if ((bool)mTransform)
		{
			Transform transform = mTransform.Find(AvatarSettings.pInstance._AvatarName);
			if ((bool)transform)
			{
				transform.SendMessage("PlayCannedAnim", animName, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public static void PlayAnim(string inAnimName)
	{
		PlayAnim(mObject, inAnimName);
	}

	public static void PlayAnim(string inAnimName, WrapMode inWrapMode)
	{
		PlayAnim(mObject, inAnimName, inWrapMode);
	}

	public static void PlayAnim(string inAnimName, float offset, float fadelength, WrapMode wrapmode, float speed)
	{
		PlayAnim(mObject, inAnimName, offset, fadelength, wrapmode, speed);
	}

	public static void PlayAnim(GameObject avatar, string inAnimName)
	{
		if (avatar != null)
		{
			AvAvatarAnimation parameter = new AvAvatarAnimation(inAnimName);
			avatar.BroadcastMessage("Animate", parameter, SendMessageOptions.DontRequireReceiver);
			UtDebug.Log("Playing Animation " + inAnimName + " of gameobject " + avatar.name, 2u);
		}
	}

	public static void PlayAnim(GameObject avatar, string inAnimName, WrapMode inWrapMode)
	{
		if (avatar != null)
		{
			AvAvatarAnimation avAvatarAnimation = new AvAvatarAnimation(inAnimName);
			avAvatarAnimation.mWrapMode = inWrapMode;
			avatar.BroadcastMessage("Animate", avAvatarAnimation, SendMessageOptions.DontRequireReceiver);
			UtDebug.Log("Playing Animation " + inAnimName + " of gameobject " + avatar.name, 2u);
		}
	}

	public static void PlayAnim(GameObject avatar, string inAnimName, float offset, float fadelength, WrapMode wrapmode, float speed)
	{
		if (avatar != null)
		{
			AvAvatarAnimation parameter = new AvAvatarAnimation(inAnimName, offset, fadelength, wrapmode, speed);
			avatar.BroadcastMessage("Animate", parameter, SendMessageOptions.DontRequireReceiver);
			UtDebug.Log("Playing Animation " + inAnimName + " of gameobject " + avatar.name, 2u);
		}
	}

	public static void PlayAnimInstant(string inAnimName)
	{
		PlayAnimInstant(mObject, inAnimName);
	}

	public static void PlayAnimInstant(string inAnimName, WrapMode inWrapMode)
	{
		PlayAnimInstant(mObject, inAnimName, inWrapMode);
	}

	public static void PlayAnimInstant(string inAnimName, float offset, float fadelength, WrapMode wrapmode, float speed)
	{
		PlayAnimInstant(mObject, inAnimName, offset, fadelength, wrapmode, speed);
	}

	public static void PlayAnimInstant(GameObject avatar, string inAnimName)
	{
		if (avatar != null)
		{
			AvAvatarAnimation parameter = new AvAvatarAnimation(inAnimName);
			avatar.BroadcastMessage("ApplyAnim", parameter, SendMessageOptions.DontRequireReceiver);
			UtDebug.Log("Playing Animation " + inAnimName + " of gameobject " + avatar.name, 2u);
		}
	}

	public static void PlayAnimInstant(GameObject avatar, string inAnimName, WrapMode inWrapMode)
	{
		if (avatar != null)
		{
			AvAvatarAnimation avAvatarAnimation = new AvAvatarAnimation(inAnimName);
			avAvatarAnimation.mWrapMode = inWrapMode;
			avatar.BroadcastMessage("ApplyAnim", avAvatarAnimation, SendMessageOptions.DontRequireReceiver);
			UtDebug.Log("Playing Animation " + inAnimName + " of gameobject " + avatar.name, 2u);
		}
	}

	public static void PlayAnimInstant(GameObject avatar, string inAnimName, float offset, float fadelength, WrapMode wrapmode, float speed)
	{
		if (avatar != null)
		{
			AvAvatarAnimation parameter = new AvAvatarAnimation(inAnimName, offset, fadelength, wrapmode, speed);
			avatar.BroadcastMessage("ApplyAnim", parameter, SendMessageOptions.DontRequireReceiver);
			UtDebug.Log("Playing Animation " + inAnimName + " of gameobject " + avatar.name, 2u);
		}
	}

	public static void SetParentTransform(Transform inTransform)
	{
		mTransform.parent = inTransform;
		if (inTransform == null)
		{
			UnityEngine.Object.DontDestroyOnLoad(mObject);
		}
	}

	public static void Destroy()
	{
		UnityEngine.Object.Destroy(mObject);
		mObject = null;
		mTransform = null;
	}

	public static void SetStartPositionAndRotation()
	{
		if (!(pObject != null))
		{
			return;
		}
		AvAvatarController component = pObject.GetComponent<AvAvatarController>();
		if (component != null)
		{
			if (component.pSubState == AvAvatarSubState.UWSWIMMING)
			{
				pStartPosition = GetPosition();
			}
			else if (component.IsValidLastPositionOnGround())
			{
				pStartPosition = component.pLastPositionOnGround;
			}
			else
			{
				pStartLocation = null;
			}
			pStartRotation = mTransform.rotation;
		}
	}
}
