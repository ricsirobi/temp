using UnityEngine;

public class ObCatapultAmmo : ObAmmo
{
	public float _ExplodingRadius;

	protected override void Awake()
	{
	}

	public override void Activate(bool inLocal, Transform inCreator, WeaponManager inManager, WeaponTuneData.Weapon inWeapon, Transform target, Vector3 direction, float parentSpeed)
	{
		base.Activate(inLocal, inCreator, inManager, inWeapon, target, direction, parentSpeed);
		Rigidbody component = GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = false;
			component.useGravity = true;
			component.velocity = Vector3.zero;
			component.AddForce(mMinSpeed * direction, ForceMode.Impulse);
		}
	}

	protected override void FixedUpdate()
	{
		mElapsedTime += Time.deltaTime;
		if (mState == State.TARGETING)
		{
			if (mElapsedTime >= mWeapon._Lifetime)
			{
				Explode();
			}
		}
		else if (mState == State.EXPLODING && mElapsedTime / 1f > 1f)
		{
			DeActivate();
		}
	}

	private void Explode()
	{
		mElapsedTime = 0f;
		mState = State.EXPLODING;
		Invoke("HideAmmo", 0.5f);
		if (mHitPrefab != null)
		{
			PlayHitParticle(base.transform.position, mHitPrefab);
		}
		if (Vector3.Distance(base.transform.position, AvAvatar.position) <= _ExplodingRadius)
		{
			AvAvatarController avAvatarController = AvAvatar.pObject.GetComponent(typeof(AvAvatarController)) as AvAvatarController;
			if (avAvatarController != null)
			{
				avAvatarController.TakeHit(mWeapon._Damage, this);
			}
		}
	}

	protected override void Update()
	{
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!(mCreator != null) || !(other.transform.root == mCreator.root))
		{
			PlayHitSoundAt(other.transform.position);
			Explode();
		}
	}

	public override void PlayHitParticle(Vector3 pos, GameObject _hitPrefab)
	{
		if (_hitPrefab != null)
		{
			if (mParticlePool == null)
			{
				PoolManager.Pools.TryGetValue(_ParticlePoolName, out mParticlePool);
			}
			GameObject gameObject = ((mParticlePool != null) ? mParticlePool.Spawn(_hitPrefab, pos, Quaternion.identity) : Object.Instantiate(_hitPrefab, pos, Quaternion.identity));
			if (gameObject != null && gameObject.GetComponent<ObSelfDestructTimer>() == null)
			{
				gameObject.AddComponent<ObSelfDestructTimer>()._SelfDestructTime = 1f;
			}
		}
	}
}
