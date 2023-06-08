using UnityEngine;

public class ObBlastAmmo : ObAmmo
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
			component.AddForce(mMinSpeed * direction.normalized, ForceMode.VelocityChange);
		}
	}

	protected override void FixedUpdate()
	{
		mElapsedTime += Time.deltaTime;
		if (mState == State.TARGETING && mElapsedTime >= mWeapon._Lifetime)
		{
			Explode();
		}
		base.FixedUpdate();
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
		float num = Vector3.Distance(base.transform.position, AvAvatar.position);
		if (num <= _ExplodingRadius)
		{
			AvAvatarController avAvatarController = AvAvatar.pObject.GetComponent(typeof(AvAvatarController)) as AvAvatarController;
			float damage = Mathf.Lerp(mWeapon._Damage, 0f, num / _ExplodingRadius);
			if (avAvatarController != null)
			{
				PlayHitSoundAt(avAvatarController.transform.position);
				avAvatarController.TakeHit(damage, this);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!(mCreator != null) || !(other.transform.root == mCreator.root))
		{
			Explode();
		}
	}

	public override void PlayHitParticle(Vector3 pos, GameObject _hitPrefab)
	{
		if (!(_hitPrefab != null))
		{
			return;
		}
		GameObject gameObject = Object.Instantiate(_hitPrefab, pos, Quaternion.identity);
		if (gameObject != null)
		{
			gameObject.transform.position = pos;
			if (gameObject.GetComponent<ObSelfDestructTimer>() == null)
			{
				gameObject.AddComponent<ObSelfDestructTimer>()._SelfDestructTime = 1f;
			}
		}
	}
}
