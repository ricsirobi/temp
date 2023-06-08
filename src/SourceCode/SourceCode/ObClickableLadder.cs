using UnityEngine;
using UnityEngine.AI;

public class ObClickableLadder : ObClickable
{
	public enum LadderState
	{
		NONE,
		ASCEND,
		DESCEND,
		BOTTOMMOUNT,
		TOPMOUNT,
		BOTTOMDISMOUNT,
		TOPDISMOUNT
	}

	public int _Rungs = 10;

	public float _AscendSpeedMultiplier = 1f;

	public float _DescendAvatarSpeed = 4f;

	public string _AscendAnimation = "ClimbUp";

	public string _DescendAnimation = "ClimbDown";

	public string _AscendDismountAnimation = "ClimbUpDismount";

	public string _DescendDismountAnimation = "ClimbDownDismount";

	public string _DescendMountAnimation = "ClimbDownMount";

	private string mCurrentRunningAnim;

	public float mRungHeight = 0.24f;

	private float mTopMountDistance = 0.36f;

	private float mBottomMountDistance = 0.3f;

	private float mTopDismountHeight;

	private float mBottomDismountHeight = 0.6f;

	private float mAvatarAscendSpeed = 0.48f;

	private bool mIsOnGroundLevel;

	private Vector3 mMountPosition;

	private NavMeshAgent mAvNavMeshAgent;

	private Animator mAnim;

	private LadderState mState;

	private AvAvatarController mAvatarController;

	public void Start()
	{
		if (AvAvatar.pObject != null)
		{
			mAnim = AvAvatar.pObject.GetComponentInChildren<Animator>();
			mAvatarController = AvAvatar.pObject.GetComponent<AvAvatarController>();
			mAvNavMeshAgent = AvAvatar.pObject.GetComponentInChildren<NavMeshAgent>();
		}
	}

	public override void OnActivate()
	{
		if (mAnim == null)
		{
			UtDebug.Log("No animator component found on Avatar");
		}
		else if (mAvatarController == null)
		{
			UtDebug.Log("No Avatar Controller found");
		}
		else if (mAvatarController.OnGround() && !mAvatarController.pPlayerMounted)
		{
			SanctuaryPet pCurPetInstance = SanctuaryManager.pCurPetInstance;
			if (pCurPetInstance != null)
			{
				pCurPetInstance.SetAvatar(null);
			}
			AvAvatar.SetUIActive(inActive: false);
			if (AvAvatar.pAvatarCam != null)
			{
				AvAvatar.pAvatarCam.GetComponent<CaAvatarCam>().ForceFreeRotate(isForceRotation: true);
			}
			mTopDismountHeight = (float)(_Rungs - 2) * mRungHeight;
			mIsOnGroundLevel = AvAvatar.position.y < base.transform.position.y + mTopDismountHeight;
			mAvatarController.pEndSplineMessageObject = base.gameObject;
			if (mIsOnGroundLevel)
			{
				mMountPosition = base.transform.position + base.transform.forward * mBottomMountDistance;
			}
			else
			{
				mMountPosition = base.transform.position + base.transform.up * _Rungs * mRungHeight - base.transform.forward * mTopMountDistance;
			}
			Vector3 targetPos = ((mAvNavMeshAgent != null) ? new Vector3(mMountPosition.x, mMountPosition.y, base.transform.position.z + (mIsOnGroundLevel ? mAvNavMeshAgent.radius : (0f - mAvNavMeshAgent.radius))) : mMountPosition);
			if (Vector3.Distance(AvAvatar.position, mMountPosition) <= Mathf.Abs(base.transform.position.z - targetPos.z))
			{
				OnPathEndReached();
			}
			else
			{
				mAvatarController.MoveTo(targetPos);
			}
			base.OnActivate();
		}
	}

	private void SetState(LadderState state)
	{
		if (mState != state)
		{
			mState = state;
			switch (mState)
			{
			case LadderState.ASCEND:
				mAnim.SetBool(_AscendAnimation, value: true);
				mAnim.speed *= _AscendSpeedMultiplier;
				break;
			case LadderState.DESCEND:
				mAnim.SetBool(_DescendAnimation, value: true);
				break;
			case LadderState.TOPMOUNT:
				PlayAnimation(_DescendMountAnimation);
				break;
			case LadderState.BOTTOMMOUNT:
				SetState(LadderState.ASCEND);
				break;
			case LadderState.TOPDISMOUNT:
				mAnim.SetBool(_AscendAnimation, value: false);
				mAnim.speed = 1f;
				PlayAnimation(_AscendDismountAnimation);
				break;
			case LadderState.BOTTOMDISMOUNT:
				mAnim.SetBool(_DescendAnimation, value: false);
				PlayAnimation(_DescendDismountAnimation);
				break;
			}
		}
	}

	public override void Update()
	{
		if (mState == LadderState.ASCEND)
		{
			float num = Time.deltaTime * mAvatarAscendSpeed * _AscendSpeedMultiplier;
			if (AvAvatar.position.y + num > base.transform.position.y + mTopDismountHeight)
			{
				SetState(LadderState.TOPDISMOUNT);
				AvAvatar.mTransform.Translate(0f, base.transform.position.y + mTopDismountHeight - AvAvatar.position.y, 0f);
			}
			else
			{
				AvAvatar.mTransform.Translate(0f, num, 0f);
			}
		}
		else if (mState == LadderState.DESCEND)
		{
			float num2 = Time.deltaTime * _DescendAvatarSpeed;
			if (AvAvatar.position.y - num2 < base.transform.position.y + mBottomDismountHeight)
			{
				SetState(LadderState.BOTTOMDISMOUNT);
				AvAvatar.mTransform.Translate(0f, base.transform.position.y + mBottomDismountHeight - AvAvatar.position.y, 0f);
			}
			else
			{
				AvAvatar.mTransform.Translate(0f, (0f - Time.deltaTime) * _DescendAvatarSpeed, 0f);
			}
		}
		if (!string.IsNullOrEmpty(mCurrentRunningAnim) && mAnim.GetCurrentAnimatorStateInfo(0).IsName("Main." + mCurrentRunningAnim))
		{
			SyncAvatarRoot();
			if (mAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
			{
				StopAnimation(mCurrentRunningAnim);
			}
		}
		base.Update();
	}

	private void SyncAvatarRoot()
	{
		AvAvatar.position = mAvatarController._MainRoot.position;
		AvAvatar.mTransform.rotation = mAvatarController._MainRoot.rotation;
		mAvatarController._MainRoot.localPosition = Vector3.zero;
		mAvatarController._MainRoot.localRotation = Quaternion.identity;
	}

	private void OnPathEndReached(GameObject go = null)
	{
		AvAvatar.position = mMountPosition;
		Vector3 forward = base.transform.position - AvAvatar.position;
		forward.y = 0f;
		AvAvatar.mTransform.rotation = Quaternion.LookRotation(forward, base.transform.up);
		AvAvatar.pState = AvAvatarState.NONE;
		SetState(mIsOnGroundLevel ? LadderState.BOTTOMMOUNT : LadderState.TOPMOUNT);
	}

	private void PlayAnimation(string anim)
	{
		mAnim.SetBool(anim, value: true);
		mCurrentRunningAnim = anim;
	}

	private void StopAnimation(string anim)
	{
		mCurrentRunningAnim = string.Empty;
		mAnim.SetBool(anim, value: false);
		SyncAvatarRoot();
		OnAnimationEnd(anim);
	}

	private void OnAnimationEnd(string anim)
	{
		if (anim.Equals(_DescendMountAnimation))
		{
			SetState(LadderState.DESCEND);
			return;
		}
		if (AvAvatar.pAvatarCam != null)
		{
			AvAvatar.pAvatarCam.GetComponent<CaAvatarCam>().ForceFreeRotate(isForceRotation: false);
		}
		SetState(LadderState.NONE);
		AvAvatar.pState = AvAvatarState.IDLE;
		SanctuaryPet pCurPetInstance = SanctuaryManager.pCurPetInstance;
		if (pCurPetInstance != null)
		{
			pCurPetInstance.SetAvatar(AvAvatar.mTransform);
		}
		AvAvatar.SetUIActive(inActive: true);
	}
}
