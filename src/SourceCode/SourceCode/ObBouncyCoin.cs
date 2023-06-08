using UnityEngine;

public class ObBouncyCoin : KAMonoBase
{
	public GameObject _Coin;

	public float _ExplosionForce = 10f;

	public Vector3 _ExplosionPositionOffset;

	public float _ExplosionRadius = 5f;

	public float _UpwardsModifier = 1f;

	public float _ExplodeUpTimerDuration = 0.5f;

	public float _CoinDuration = 6.5f;

	public float _WarningTime = 3f;

	public float _FlashInterval = 0.2f;

	public float _SleepVelocity = 0.15f;

	public bool _EnableTimerAndDestroy = true;

	private bool mExplode;

	private bool mIdle;

	private float mExplodeUpTimer = 0.5f;

	private bool mExplodeUpTimerActive;

	private float mLifeTimeTimer = 3f;

	private bool mLifeTimeTimerActive;

	private float mFlashTimer = 3f;

	private bool mFlashing;

	private bool mVisible = true;

	private bool mMoving;

	private Vector3 mDest;

	private bool mStartFloatNextCycle;

	private bool mForceIdleTimerActive;

	private float mForceIdleTimer = 1f;

	private void Start()
	{
		if (!mExplodeUpTimerActive)
		{
			base.rigidbody.Sleep();
		}
	}

	private void Update()
	{
		if (AvAvatar.pState == AvAvatarState.PAUSED)
		{
			return;
		}
		if (mExplodeUpTimerActive)
		{
			mExplodeUpTimer -= Time.deltaTime;
			if (mExplodeUpTimer <= 0f)
			{
				mExplodeUpTimerActive = false;
				mExplode = false;
				mForceIdleTimerActive = true;
				mForceIdleTimer += Time.deltaTime;
			}
		}
		if (!mIdle && base.rigidbody.IsSleeping())
		{
			mForceIdleTimerActive = false;
			SphereCollider component = GetComponent<SphereCollider>();
			if (component != null)
			{
				component.isTrigger = true;
			}
			base.rigidbody.isKinematic = true;
			StartRotate();
			MoveToFloatPosition();
		}
		if (mForceIdleTimerActive && !base.rigidbody.isKinematic)
		{
			mForceIdleTimer -= Time.deltaTime;
			if (mForceIdleTimer <= 0f && Mathf.Pow(base.rigidbody.velocity.magnitude, 2f) * 0.5f < base.rigidbody.sleepThreshold)
			{
				mForceIdleTimerActive = false;
				base.rigidbody.Sleep();
			}
		}
		if (mLifeTimeTimerActive)
		{
			mLifeTimeTimer -= Time.deltaTime;
			if (mLifeTimeTimer <= 0f)
			{
				if (mFlashing)
				{
					mExplodeUpTimerActive = false;
					Object.Destroy(base.gameObject);
					return;
				}
				mFlashing = true;
				mFlashTimer = _FlashInterval;
				mLifeTimeTimer = _WarningTime;
			}
		}
		if (mFlashing && _Coin != null)
		{
			mFlashTimer -= Time.deltaTime;
			if (mFlashTimer <= 0f)
			{
				mFlashTimer = _FlashInterval;
				mVisible = !mVisible;
				UtUtilities.SetObjectVisibility(_Coin, mVisible);
			}
		}
		if (mStartFloatNextCycle)
		{
			mStartFloatNextCycle = false;
			StartFloat();
		}
		if (!mMoving)
		{
			return;
		}
		float num = 0.05f;
		float x = mDest.x;
		float x2 = base.transform.position.x;
		float y = mDest.y;
		float y2 = base.transform.position.y;
		float z = mDest.z;
		float z2 = base.transform.position.z;
		if (Mathf.Abs(x - x2) <= num && Mathf.Abs(y - y2) <= num && Mathf.Abs(z - z2) <= num)
		{
			base.transform.position = mDest;
			mMoving = false;
			TurnOffShadowMove();
			mStartFloatNextCycle = true;
			if (_EnableTimerAndDestroy)
			{
				StartLifeTimeTimer();
			}
		}
		else
		{
			base.transform.position = Vector3.Lerp(base.transform.position, mDest, Time.deltaTime * 1f);
		}
	}

	public void StartExplosion()
	{
		base.rigidbody.sleepThreshold = _SleepVelocity;
		mExplode = true;
		mExplodeUpTimerActive = true;
		mExplodeUpTimer = _ExplodeUpTimerDuration;
		Physics.IgnoreCollision(collider, AvAvatar.pObject.GetComponent<Collider>());
	}

	public void FixedUpdate()
	{
		if (mExplode)
		{
			Vector3 explosionPosition = base.transform.position + _ExplosionPositionOffset;
			base.rigidbody.AddExplosionForce(_ExplosionForce, explosionPosition, _ExplosionRadius, _UpwardsModifier, ForceMode.Force);
		}
	}

	private void StartRotate()
	{
		mIdle = true;
		if (!(_Coin == null))
		{
			ObRotate component = _Coin.GetComponent<ObRotate>();
			if (component != null)
			{
				component.enabled = true;
			}
		}
	}

	private void MoveToFloatPosition()
	{
		mMoving = true;
		mDest = base.transform.position + new Vector3(0f, 0.2f, 0f);
	}

	private void StartFloat()
	{
		mIdle = true;
		if (!(_Coin == null))
		{
			ObFloat component = _Coin.GetComponent<ObFloat>();
			if (component != null)
			{
				component.enabled = true;
			}
		}
	}

	private void TurnOffShadowMove()
	{
		CreateShadows component = GetComponent<CreateShadows>();
		if (component != null)
		{
			component._UpdateShadowPosition = false;
		}
	}

	private void StartLifeTimeTimer()
	{
		mFlashing = false;
		mLifeTimeTimer = _CoinDuration - _WarningTime;
		mLifeTimeTimerActive = true;
	}

	public void Collected()
	{
		mFlashing = false;
		mLifeTimeTimerActive = false;
	}

	public void StopLifeTimer()
	{
		mLifeTimeTimerActive = false;
	}

	public void StopBouncing()
	{
		base.enabled = false;
	}
}
