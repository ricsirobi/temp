using UnityEngine;

public class AvAvatarAnim : KAMonoBase
{
	public enum WallClimbAnimState
	{
		WallClimbNone,
		WallClimbMount,
		WallClimbIdle,
		WallClimbUp,
		WallClimbDown,
		WallClimbLeft,
		WallClimbRight,
		WallClimbUpDismount,
		WallClimbDownDismount
	}

	public delegate void OnCannedAnimEnd(string cannedAnimName);

	public static float mLocalIdleTime;

	private Animator mAnim;

	private MMOAvatar mMav;

	private AvAvatarController mAC;

	private float mMaxSpeed;

	private bool mTimedOut;

	private float mCurVelocity;

	private float mCurReelBlend;

	private float mCurReelSpeed;

	private bool mUseMMO;

	private string mCannedAnimName;

	private bool mCannedMoveAbort;

	private bool mCannedEndAbort;

	private bool mCannedFreezePlayer;

	private OnCannedAnimEnd mOnCannedAnimEnd;

	[HideInInspector]
	public int mPackState;

	public readonly string[] mParamList = new string[20]
	{
		"bJump", "bFall", "bLand", "bMounted", "bCarry", "bIsFishing", "bFishingCast", "bFishingCastIdle", "bFishingIdle", "bFishingReelIn",
		"bFishingStrike", "ChopTree", "FeedAnimal", "Harvest", "Water", "Milking", "bGliding", "bSwim", "bUWSwim", "bUWSwimBrake"
	};

	public float _FallThreshold = 0.1f;

	private float mFallTimer;

	public float _MaxLandTime = 0.5f;

	private float mLandTimer;

	public float _MMOAnimTimeout = 30f;

	private void Start()
	{
		mAnim = GetComponent<Animator>();
		mMav = base.transform.parent.gameObject.GetComponent<MMOAvatarLite>();
		mAC = base.transform.parent.gameObject.GetComponent<AvAvatarController>();
		if (mAnim != null && mAnim.avatar != null && mAnim.avatar.isValid)
		{
			mAnim.logWarnings = false;
			mAnim.SetLayerWeight(0, 1f);
			if (mAnim.layerCount > 1)
			{
				mAnim.SetLayerWeight(1, 0f);
			}
			if (mAnim.layerCount > 2)
			{
				mAnim.SetLayerWeight(2, 0f);
			}
		}
	}

	public int PackState()
	{
		int num = 0;
		if (mAnim != null && mAnim.avatar != null && mAnim.avatar.isValid)
		{
			for (int i = 0; i < mParamList.Length; i++)
			{
				num |= (mAnim.GetBool(mParamList[i]) ? (1 << i) : 0);
			}
		}
		return num;
	}

	public void UnpackState(int bitstate)
	{
		mUseMMO = true;
		mTimedOut = false;
		if (mAnim != null && mAnim.avatar != null && mAnim.avatar.isValid)
		{
			for (int i = 0; i < mParamList.Length; i++)
			{
				mAnim.SetBool(mParamList[i], (bitstate & (1 << i)) == 1 << i);
			}
		}
		if (mAnim != null && mAC != null && !mAC.pPlayerMounted)
		{
			mAnim.SetBool("bMounted", value: false);
		}
	}

	public void ClearState()
	{
		if (mAnim != null && mAnim.avatar != null && mAnim.avatar.isValid)
		{
			for (int i = 0; i < mParamList.Length; i++)
			{
				mAnim.SetBool(mParamList[i], value: false);
			}
			mTimedOut = true;
		}
	}

	private void LateUpdate()
	{
		if (mAnim == null || mAC == null || mAC.pCurrentStateData == null || mAC.pState == AvAvatarState.NONE || mAnim.avatar == null || !mAnim.avatar.isValid || mAC.pSubState == AvAvatarSubState.WALLCLIMB)
		{
			return;
		}
		UpdateCannedAnim();
		mMaxSpeed = mAC.pMaxForwardSpeed;
		if (mUseMMO && !mTimedOut && mMav != null && Time.fixedTime - mMav.pLastMMOUpdate > _MMOAnimTimeout)
		{
			ClearState();
		}
		Vector3 vector = (mTimedOut ? Vector3.zero : mAC.pVelocity);
		float y = vector.y;
		vector.y = 0f;
		float magnitude = vector.magnitude;
		bool flag = mAC.pState == AvAvatarState.FALLING;
		if (!mUseMMO)
		{
			if (!flag)
			{
				if (mAnim.GetBool("bFall") || mAnim.GetBool("bJump"))
				{
					mLandTimer = _MaxLandTime;
				}
				if (mLandTimer > 0f)
				{
					mAnim.SetBool("bLand", value: true);
				}
				else
				{
					mAnim.SetBool("bLand", value: false);
					mFallTimer = 0f;
				}
				mAnim.SetBool("bJump", value: false);
				mAnim.SetBool("bFall", value: false);
				mLandTimer -= Time.deltaTime;
			}
			else
			{
				mLandTimer = 0f;
				mFallTimer += Time.deltaTime;
				mAnim.SetBool("bLand", value: false);
				if (y >= 0f)
				{
					mAnim.SetBool("bJump", value: true);
					mAnim.SetBool("bFall", value: false);
				}
				else
				{
					mAnim.SetBool("bFall", value: true);
				}
			}
		}
		if (mUseMMO && mMav != null)
		{
			mAnim.SetBool("bMounted", mAC.pPlayerMounted && mMav.pSanctuaryPet != null);
		}
		else
		{
			mAnim.SetBool("bMounted", mAC.pPlayerMounted);
			mAnim.SetBool("bGliding", mAC.pIsPlayerGliding);
			mAnim.SetBool("bGrounded", mAC.OnGround());
		}
		if (mUseMMO ? mAnim.GetBool("bCarry") : mAC.pPlayerCarrying)
		{
			mAnim.SetBool("bCarry", value: true);
			mAnim.SetLayerWeight(1, 1f);
		}
		else
		{
			mAnim.SetBool("bCarry", value: false);
		}
		string text = (mAC.DragSmall ? "bDragSmall" : "bDrag");
		int layerIndex = (mAC.DragSmall ? 5 : 3);
		if (mUseMMO ? mAnim.GetBool(text) : mAC.pPlayerDragging)
		{
			mAnim.SetBool(text, value: true);
			mAnim.SetLayerWeight(layerIndex, 1f);
		}
		else
		{
			mAnim.SetBool(text, value: false);
			mAnim.SetLayerWeight(layerIndex, 0f);
		}
		if (mAC.pSubState == AvAvatarSubState.DIVESUIT)
		{
			mAnim.SetBool("bUnderwaterWalk", value: true);
			mAnim.SetLayerWeight(4, 1f);
		}
		else
		{
			mAnim.SetBool("bUnderwaterWalk", value: false);
			mAnim.SetLayerWeight(4, 0f);
		}
		if (magnitude > mMaxSpeed)
		{
			magnitude = mMaxSpeed;
		}
		float b = (flag ? 1f : (magnitude /= mMaxSpeed));
		mCurVelocity = Mathf.Lerp(mCurVelocity, b, Mathf.Min(10f * Time.deltaTime, 1f));
		mAnim.SetFloat("fSpeed", mCurVelocity);
		if (base.transform.root.gameObject == AvAvatar.pObject)
		{
			mAnim.SetFloat("fHorizontal", KAInput.GetAxis("Horizontal"));
			float value = KAInput.GetAxis("Vertical") * (float)((!UiOptions.pIsFlightInverted) ? 1 : (-1)) * (float)((!UiOptions.pIsTiltSteer) ? 1 : (-1));
			mAnim.SetFloat("fVertical", value);
		}
		mAnim.SetBool("bSwim", mAC.pSubState == AvAvatarSubState.SWIMMING);
		mAnim.SetBool("bUWSwim", mAC.pSubState == AvAvatarSubState.UWSWIMMING);
		mAnim.SetBool("bUWSwimBrake", mAC.pSubState == AvAvatarSubState.UWSWIMMING && mAC.pIsUWSwimBraking);
		FishingZone pActiveFishingZone;
		if (mUseMMO ? mAnim.GetBool("bIsFishing") : mAC.pIsFishing)
		{
			mAnim.SetLayerWeight(0, 0f);
			if (mAnim.layerCount > 2)
			{
				mAnim.SetLayerWeight(2, 1f);
			}
			if (!mUseMMO)
			{
				mAnim.SetBool("bIsFishing", value: true);
				mAnim.SetBool("bFishingIdle", value: false);
				mAnim.SetBool("bFishingCast", value: false);
				mAnim.SetBool("bFishingCastIdle", value: false);
				mAnim.SetBool("bFishingStrike", value: false);
				mAnim.SetBool("bFishingReelIn", value: false);
				pActiveFishingZone = mAC.pActiveFishingZone;
				string mPlayerAnimState = pActiveFishingZone.mPlayerAnimState;
				if (mPlayerAnimState == null || mPlayerAnimState.Length != 0)
				{
					switch (mPlayerAnimState)
					{
					case "idle":
						break;
					case "cast":
						mAnim.SetBool("bFishingCast", value: true);
						goto IL_06ce;
					case "castidle":
						mAnim.SetBool("bFishingCastIdle", value: true);
						goto IL_06ce;
					case "reelin":
						mAnim.SetBool("bFishingReelIn", value: true);
						goto IL_06ce;
					case "strike":
						mAnim.SetBool("bFishingStrike", value: true);
						goto IL_06ce;
					default:
						goto IL_06ce;
					}
				}
				mAnim.SetBool("bFishingIdle", value: true);
				goto IL_06ce;
			}
			if (!mAC.UsingProp("FishingRod"))
			{
				mAC.UseProp("FishingRod");
			}
			mAnim.SetFloat("fFishingReelBlend", 0.5f);
			mAnim.SetFloat("fFishingReelSpeed", 1f);
		}
		else
		{
			if (mUseMMO && mAC.UsingProp("FishingRod"))
			{
				mAC.StopUseProp();
			}
			mAnim.SetBool("bIsFishing", value: false);
			mAnim.SetLayerWeight(0, 1f);
			if (mAnim.layerCount > 2)
			{
				mAnim.SetLayerWeight(2, 0f);
			}
		}
		goto IL_0800;
		IL_0800:
		if (AvAvatar.pObject == base.transform.root.gameObject)
		{
			int num = PackState();
			if (num == mPackState)
			{
				mLocalIdleTime += Time.deltaTime;
			}
			else
			{
				mLocalIdleTime = 0f;
			}
			mPackState = num;
		}
		return;
		IL_06ce:
		float b2 = (pActiveFishingZone.mReelAnimHigh ? 1f : 0f);
		mCurReelBlend = Mathf.Lerp(mCurReelBlend, b2, Time.deltaTime);
		mAnim.SetFloat("fFishingReelBlend", mCurReelBlend);
		mCurReelSpeed = Mathf.Lerp(mCurReelSpeed, b2, Time.deltaTime * 5f);
		mAnim.SetFloat("fFishingReelSpeed", mCurReelSpeed);
		goto IL_0800;
	}

	public void PlayCannedAnim(string animName, OnCannedAnimEnd cannedAnimEndCallback = null)
	{
		PlayCannedAnim(animName, bQuitOnMovement: true, bQuitOnEnd: true, bFreezePlayer: false, cannedAnimEndCallback);
	}

	public void PlayCannedAnim(string animName, bool bQuitOnMovement, bool bQuitOnEnd, bool bFreezePlayer, OnCannedAnimEnd cannedAnimEndCallback = null)
	{
		if (!mUseMMO && !(mAnim == null) && !string.IsNullOrEmpty(animName) && !(mAnim.avatar == null) && mAnim.avatar.isValid)
		{
			EndCannedAnim();
			mAnim.SetBool(animName, value: true);
			mCannedAnimName = animName;
			mCannedMoveAbort = bQuitOnMovement;
			mCannedEndAbort = bQuitOnEnd;
			mCannedFreezePlayer = bFreezePlayer;
			mOnCannedAnimEnd = cannedAnimEndCallback;
			if ((bool)mAC)
			{
				mAC.OnCannedAnimBegin(mCannedAnimName);
			}
			mAnim.Play(mCannedAnimName, -1, 0f);
			if (mCannedFreezePlayer)
			{
				AvAvatar.pState = AvAvatarState.NONE;
				AvAvatar.SetUIActive(inActive: false);
			}
		}
	}

	public void EndCannedAnim(bool bUseCallback = true)
	{
		if (!mUseMMO && !(mAnim == null) && !string.IsNullOrEmpty(mCannedAnimName) && !(mAnim.avatar == null) && mAnim.avatar.isValid)
		{
			string cannedAnimName = mCannedAnimName;
			mAnim.SetBool(mCannedAnimName, value: false);
			mCannedAnimName = "";
			if (mCannedFreezePlayer)
			{
				AvAvatar.pState = AvAvatarState.IDLE;
				AvAvatar.SetUIActive(inActive: true);
			}
			if ((bool)mAC && bUseCallback)
			{
				mAC.OnCannedAnimEnd(cannedAnimName);
			}
			if (mOnCannedAnimEnd != null)
			{
				mOnCannedAnimEnd(cannedAnimName);
			}
		}
	}

	public void UpdateCannedAnim()
	{
		if (!mUseMMO && !(mAnim == null) && !string.IsNullOrEmpty(mCannedAnimName) && !(mAnim.avatar == null) && mAnim.avatar.isValid)
		{
			if (mCannedEndAbort && mAnim.GetCurrentAnimatorStateInfo(0).IsName("Main." + mCannedAnimName) && mAnim.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f >= 0.95f)
			{
				EndCannedAnim();
			}
			if (mCannedMoveAbort && Mathf.Abs(KAInput.GetValue("Vertical")) + Mathf.Abs(KAInput.GetValue("Horizontal")) > 0f)
			{
				EndCannedAnim();
			}
		}
	}

	public void Message(string message)
	{
		BroadcastMessage(message);
	}

	public void SetWallClimbAnim(WallClimbAnimState state, float horizontalMovement = 0f, float verticalMovement = 0f)
	{
		if (mAnim == null)
		{
			return;
		}
		switch (state)
		{
		case WallClimbAnimState.WallClimbIdle:
			mAnim.SetBool("bWallClimbing", value: true);
			break;
		case WallClimbAnimState.WallClimbMount:
			if (mAnim.GetBool("bMounted"))
			{
				mAnim.SetBool("bMounted", value: false);
			}
			mAnim.SetBool("bWallClimbMount", value: true);
			mAnim.SetBool("bWallClimbUpDismount", value: false);
			mAnim.SetBool("bWallClimbDownDismount", value: false);
			if (mAnim.GetBool("bGliding"))
			{
				mAnim.SetBool("bGliding", value: false);
				mAnim.SetBool("bGrounded", value: true);
			}
			break;
		case WallClimbAnimState.WallClimbUp:
		case WallClimbAnimState.WallClimbDown:
		case WallClimbAnimState.WallClimbLeft:
		case WallClimbAnimState.WallClimbRight:
			mAnim.SetFloat("fWallClimpHorizontal", horizontalMovement);
			mAnim.SetFloat("fWallClimpVertical", verticalMovement);
			mAnim.SetBool("bWallClimbing", value: false);
			break;
		case WallClimbAnimState.WallClimbUpDismount:
			mAnim.SetBool("bWallClimbMount", value: false);
			mAnim.SetBool("bWallClimbing", value: false);
			mAnim.SetFloat("fWallClimpHorizontal", horizontalMovement);
			mAnim.SetFloat("fWallClimpVertical", verticalMovement);
			mAnim.SetBool("bWallClimbUpDismount", value: true);
			break;
		case WallClimbAnimState.WallClimbDownDismount:
			mAnim.SetBool("bWallClimbMount", value: false);
			mAnim.SetBool("bWallClimbing", value: false);
			mAnim.SetFloat("fWallClimpHorizontal", horizontalMovement);
			mAnim.SetFloat("fWallClimpVertical", verticalMovement);
			mAnim.SetBool("bWallClimbDownDismount", value: true);
			break;
		default:
			mAnim.SetBool("bWallClimbing", value: false);
			mAnim.SetBool("bWallClimbMount", value: false);
			mAnim.SetFloat("fWallClimpVertical", verticalMovement);
			mAnim.SetFloat("fWallClimpHorizontal", horizontalMovement);
			mAnim.SetBool("bWallClimbUpDismount", value: false);
			mAnim.SetBool("bWallClimbDownDismount", value: false);
			break;
		}
	}
}
