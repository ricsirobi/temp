using UnityEngine;

public class FakeBoxObject : KAMonoBase
{
	public enum CamEffectDoneState
	{
		NONE,
		MOVE_TO_POS,
		MOVE_BACK
	}

	public float _HitFlyingSlowDownFactor = 0.3f;

	public float _HitSpinSpeedFactor = 26f;

	public float _MinFlySpeedForRotOnHit = 8f;

	public float _EffectOnPlayerDur = 20f;

	public Transform _NegativeEffect;

	public Vector3 _NegativeEffectPosOffset;

	public AudioClip _ConsumeAudioClip;

	public float _CamMoveSpeed = 4f;

	public float _TargetCamDistance = 10f;

	public float _TargetReachLerpEpsilon = 0.5f;

	private Transform mNegativeEffect;

	private float mEffectOnPlayerTimer;

	private float mFlySpeedOnHit = -1f;

	private float mCurrFlyAcceleration;

	private float mInitialRotY;

	private float mCurRotY;

	private float mPrevRotY;

	private bool mStartSpin;

	private float mHitSpinSpeed;

	private float mCurrCamDistance;

	private float mTargetCamDistance;

	private CaAvatarCam mAvatarCam;

	private CamEffectDoneState mCurrCamMoveState;

	private FakeBoxPowerUp mPowerUp;

	private AvAvatarController mAvatarController;

	private bool mReachedMinSpeed;

	private DragonFlyCollide mDragonCollide;

	private void Start()
	{
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		mAvatarController = component;
		mAvatarCam = AvAvatar.pAvatarCam.GetComponent<CaAvatarCam>();
		if (SanctuaryManager.pCurPetInstance != null)
		{
			mDragonCollide = SanctuaryManager.pCurPetInstance.GetComponentInChildren<DragonFlyCollide>();
		}
		if (base.transform.parent != null)
		{
			mPowerUp = base.transform.parent.GetComponent<FakeBoxPowerUp>();
		}
		mCurrFlyAcceleration = mAvatarController.pFlyingData._Acceleration;
	}

	private void OnTriggerEnter(Collider inCollider)
	{
		if (!(inCollider.gameObject == AvAvatar.pObject))
		{
			return;
		}
		if (PowerUp.pImmune)
		{
			PowerUp.pHitCount++;
		}
		else if (!mPowerUp.IsCurrAvatarImmune())
		{
			if (MainStreetMMOClient.pIsReady)
			{
				MainStreetMMOClient.pInstance.SendPublicMMOMessage("POWERUP_A:" + mPowerUp.pHostUserId + ":OnFakeBoxObjectHit:" + UserInfo.pInstance.UserID);
			}
			collider.enabled = false;
			Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			mPowerUp.SetCurrAvatarImmune(isImmune: true);
			AvAvatar.pInputEnabled = false;
			if (_NegativeEffect != null)
			{
				mNegativeEffect = ParticleManager.PlayParticle(_NegativeEffect);
			}
			if (_ConsumeAudioClip != null)
			{
				SnChannel.Play(_ConsumeAudioClip, mPowerUp._SndSettings, inForce: true);
			}
			mInitialRotY = mAvatarController._FlyingBone.localEulerAngles.y;
			mPrevRotY = (mCurRotY = mInitialRotY);
			mStartSpin = true;
			mHitSpinSpeed = ((mAvatarController.pFlightSpeed < _MinFlySpeedForRotOnHit) ? (_HitSpinSpeedFactor * _MinFlySpeedForRotOnHit) : (_HitSpinSpeedFactor * mAvatarController.pFlightSpeed));
			mAvatarController.pLockVelocity = true;
			mFlySpeedOnHit = mAvatarController.pFlightSpeed;
			mAvatarController.pFlyingData._Acceleration = 0f;
			mCurrCamDistance = mAvatarController._DefaultFlightDistance;
			mTargetCamDistance = _TargetCamDistance;
			mCurrCamMoveState = CamEffectDoneState.MOVE_TO_POS;
			if (mDragonCollide != null)
			{
				mDragonCollide._OnHitMsgObject = base.gameObject;
			}
			mEffectOnPlayerTimer = _EffectOnPlayerDur;
		}
	}

	private void Update()
	{
		if (mStartSpin && mAvatarController != null)
		{
			float num = mHitSpinSpeed * Time.deltaTime;
			mCurRotY += num;
			if (mCurRotY >= 360f)
			{
				mCurRotY = 360f - mCurRotY;
			}
			if (mPrevRotY < mInitialRotY && mCurRotY >= mInitialRotY && mReachedMinSpeed)
			{
				mCurRotY = mInitialRotY;
				SetDragonRotation(mCurRotY);
				mStartSpin = false;
				mTargetCamDistance = mAvatarController._DefaultFlightDistance;
				mCurrCamMoveState = CamEffectDoneState.MOVE_BACK;
				return;
			}
			SetDragonRotation(mCurRotY);
			mPrevRotY = mCurRotY;
			if (!mReachedMinSpeed && mEffectOnPlayerTimer > 0f)
			{
				mEffectOnPlayerTimer -= Time.deltaTime;
				if (mEffectOnPlayerTimer <= 0f)
				{
					mReachedMinSpeed = true;
				}
			}
			if (mAvatarController.pFlightSpeed > mFlySpeedOnHit * _HitFlyingSlowDownFactor && !mReachedMinSpeed)
			{
				mAvatarController.pFlightSpeed -= mFlySpeedOnHit * _HitFlyingSlowDownFactor * Time.deltaTime;
				if (mAvatarController.pFlightSpeed < 1f)
				{
					mAvatarController.pFlightSpeed = 0f;
					mReachedMinSpeed = true;
				}
				else if (mAvatarController.pFlightSpeed < mFlySpeedOnHit * _HitFlyingSlowDownFactor)
				{
					mAvatarController.pFlightSpeed = mFlySpeedOnHit * _HitFlyingSlowDownFactor;
					mReachedMinSpeed = true;
				}
			}
		}
		UpdateCamera();
	}

	private void UpdateCamera()
	{
		if (mCurrCamMoveState == CamEffectDoneState.NONE)
		{
			return;
		}
		mCurrCamDistance = Mathf.Lerp(mCurrCamDistance, mTargetCamDistance, Time.deltaTime * _CamMoveSpeed);
		mAvatarCam.SetDistance(0f - mCurrCamDistance);
		if (Mathf.Abs(mTargetCamDistance - mCurrCamDistance) < _TargetReachLerpEpsilon)
		{
			mCurrCamDistance = mTargetCamDistance;
			if (mCurrCamMoveState == CamEffectDoneState.MOVE_BACK)
			{
				DeactivatePowerup();
				mCurrCamMoveState = CamEffectDoneState.NONE;
			}
		}
	}

	private void SetDragonRotation(float inCurrRotY)
	{
		Vector3 localEulerAngles = mAvatarController._FlyingBone.localEulerAngles;
		localEulerAngles.y = inCurrRotY;
		mAvatarController._FlyingBone.localEulerAngles = localEulerAngles;
	}

	private void DeactivatePowerup()
	{
		mPowerUp.SetCurrAvatarImmune(isImmune: false);
		if (mPowerUp != null)
		{
			mPowerUp.DeActivate();
		}
	}

	private void OnDragonHit()
	{
		AvAvatar.pObject.SendMessage("ResetVelocity", SendMessageOptions.DontRequireReceiver);
		AvAvatar.pState = AvAvatarState.PAUSED;
		mReachedMinSpeed = true;
	}

	private void OnDestroy()
	{
		if (mNegativeEffect != null)
		{
			ParticleManager.Despawn(mNegativeEffect);
		}
		mAvatarController.pLockVelocity = false;
		mAvatarController.pFlyingData._Acceleration = mCurrFlyAcceleration;
		AvAvatar.pInputEnabled = true;
		AvAvatar.pState = AvAvatarState.IDLE;
		if (mDragonCollide != null)
		{
			mDragonCollide._OnHitMsgObject = null;
		}
	}
}
