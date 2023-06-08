using UnityEngine;

public class ObForceAvatar : KAMonoBase
{
	public MinMax _Force;

	public Vector3 _ForceDirection;

	public float _ForceGrowtime;

	public float _ExitForce;

	public float _ExitForcetime;

	public GameObject _ParticleEffect;

	public float _Acceleration;

	public float _TriggerDelay;

	public string _IgnoreCollisionLayer = "Ignore Raycast";

	public string _VentCollisionLayer = "Vent";

	private Transform mTarget;

	private float mCurDelayTime = -1f;

	private Vector3 mForceDirection;

	private float mTotalForceTime;

	private float mCurrForceTime;

	private float mCurrForceMagnitude;

	private bool mApplyForce;

	private bool mApplyExitForce;

	private void Start()
	{
		Physics.IgnoreLayerCollision(LayerMask.NameToLayer(_IgnoreCollisionLayer), LayerMask.NameToLayer(_VentCollisionLayer));
		mForceDirection = base.transform.TransformDirection(_ForceDirection).normalized;
	}

	private void OnTriggerEnter(Collider inCollider)
	{
		AvAvatarController component = inCollider.GetComponent<AvAvatarController>();
		if (AvAvatar.pObject != null && inCollider.gameObject == AvAvatar.pObject && mTarget != inCollider.transform && mCurDelayTime < 0f)
		{
			mTarget = inCollider.transform;
			if (component != null)
			{
				ShowParticleEffect(show: true, inCollider.transform.position);
				ApplyEntryForce();
			}
		}
	}

	private void OnTriggerStay(Collider inCollider)
	{
		AvAvatarController component = inCollider.GetComponent<AvAvatarController>();
		if (AvAvatar.pObject != null && inCollider.gameObject == AvAvatar.pObject && mCurDelayTime < 0f && component != null)
		{
			if (mTarget == null)
			{
				ShowParticleEffect(show: true, inCollider.transform.position);
				mTarget = inCollider.transform;
			}
			else
			{
				ApplyEntryForce();
			}
		}
	}

	private void OnTriggerExit(Collider inCollider)
	{
		AvAvatarController component = inCollider.GetComponent<AvAvatarController>();
		if (AvAvatar.pObject != null && inCollider.gameObject == AvAvatar.pObject && mTarget == inCollider.transform)
		{
			mTarget = null;
			if (component != null)
			{
				ShowParticleEffect(show: false, Vector3.zero);
				ResetForceParameters();
				ApplyExitForceOnAvatar();
			}
			mCurDelayTime = _TriggerDelay;
		}
	}

	private void Update()
	{
		if (mCurDelayTime >= 0f)
		{
			mCurDelayTime -= Time.deltaTime;
		}
		if ((mApplyForce || mApplyExitForce) && AvAvatar.pObject.activeInHierarchy)
		{
			if (AvAvatar.IsFlying())
			{
				ApplyForceOnAvatar();
			}
			else
			{
				ResetForceParameters();
			}
		}
	}

	private void ApplyEntryForce()
	{
		if (AvAvatar.IsFlying() && mTarget != null && AvAvatar.IsCurrentPlayer(mTarget.gameObject) && !mApplyForce)
		{
			ResetForceParameters();
			mApplyForce = true;
			mTotalForceTime = _ForceGrowtime;
			mCurrForceTime = 0f;
			ApplyForceOnAvatar();
		}
	}

	private void ResetForceParameters()
	{
		mApplyForce = false;
		mTotalForceTime = 0f;
		mCurrForceTime = 0f - Mathf.Epsilon;
		mCurrForceMagnitude = 0f;
		mApplyExitForce = false;
	}

	private void ApplyForceOnAvatar()
	{
		if (mTotalForceTime > mCurrForceTime)
		{
			mCurrForceTime += Time.deltaTime;
			if (mApplyExitForce)
			{
				if (Mathf.Approximately(mTotalForceTime, 0f))
				{
					mCurrForceMagnitude = _ExitForce;
				}
				else
				{
					float num = mCurrForceTime / mTotalForceTime;
					if (num > 0f)
					{
						mCurrForceMagnitude = Mathf.Lerp(_ExitForce, 0f, num);
					}
					else
					{
						mCurrForceMagnitude = 0f;
					}
				}
			}
			else if (mCurrForceMagnitude < _Force.Min)
			{
				mCurrForceMagnitude = _Force.Min;
			}
			else if (mCurrForceMagnitude > _Force.Max)
			{
				mCurrForceMagnitude = _Force.Max;
			}
			else
			{
				mCurrForceMagnitude += _Acceleration;
			}
		}
		else if (mApplyExitForce)
		{
			ResetForceParameters();
		}
		else
		{
			mCurrForceMagnitude = _Force.Max;
		}
		AvAvatar.mTransform.Translate(mForceDirection * mCurrForceMagnitude, Space.World);
	}

	private void ApplyExitForceOnAvatar()
	{
		if (AvAvatar.IsFlying())
		{
			mApplyExitForce = true;
			mTotalForceTime = _ExitForcetime;
			mCurrForceTime = 0f - Mathf.Epsilon;
			mCurrForceMagnitude = _ExitForce;
			ApplyForceOnAvatar();
		}
	}

	private void ShowParticleEffect(bool show, Vector3 pos)
	{
		if (_ParticleEffect == null || !AvAvatar.IsFlying())
		{
			return;
		}
		if (show)
		{
			_ParticleEffect.transform.position = pos;
			_ParticleEffect.SetActive(value: true);
			ParticleSystem component = _ParticleEffect.GetComponent<ParticleSystem>();
			if (component != null)
			{
				component.Simulate(0f, withChildren: true, restart: true);
				component.Play();
			}
		}
		else
		{
			_ParticleEffect.SetActive(value: false);
		}
	}
}
