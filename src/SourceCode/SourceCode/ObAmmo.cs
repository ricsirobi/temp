using System.Collections.Generic;
using UnityEngine;

public class ObAmmo : ObProjectile
{
	public enum State
	{
		NONE,
		TARGETING,
		EXPLODING
	}

	public delegate void AmmoMissed();

	public static List<GameObject> _DisableHitSoundObjectList = new List<GameObject>();

	public static AmmoMissed pOnAmmoMissed = null;

	public string _SoundPoolName = "SoundChannel";

	public string _AmmoPoolName;

	public string _ParticlePoolName;

	[HideInInspector]
	public bool IsLocal;

	protected Transform mCreator;

	protected WeaponTuneData.Weapon mWeapon;

	protected float mSpeed;

	protected float mMinSpeed;

	protected float mElapsedTime;

	protected Vector3 mForward = Vector3.forward;

	protected GameObject mHitPrefab;

	protected Transform mTarget;

	protected bool mIsWarningSndPlayed;

	protected State mState;

	protected bool mTargetHit;

	protected SpawnPool mSoundPool;

	protected SpawnPool mAmmoPool;

	protected SpawnPool mParticlePool;

	private Vector3 mHitPos;

	public Transform pCreator => mCreator;

	public WeaponTuneData.Weapon pWeapon => mWeapon;

	public Vector3 pHitPos
	{
		get
		{
			return mHitPos;
		}
		set
		{
			mHitPos = value;
		}
	}

	protected virtual void Awake()
	{
		if (base.rigidbody != null)
		{
			base.rigidbody.isKinematic = true;
			base.rigidbody.detectCollisions = false;
		}
		if (collider != null)
		{
			collider.enabled = false;
		}
	}

	public virtual void Activate(bool inLocal, Transform inCreator, WeaponManager inManager, WeaponTuneData.Weapon inWeapon, Transform target, Vector3 direction, float parentSpeed)
	{
		mWeapon = inWeapon;
		mHitPrefab = inManager.GetWeaponHitPrefab(mWeapon);
		if (mWeapon == null)
		{
			Despawn();
			return;
		}
		Alive = true;
		UtUtilities.HideObject(base.gameObject, t: false);
		base.enabled = true;
		mElapsedTime = 0f;
		IsLocal = inLocal;
		mCreator = inCreator;
		mState = State.TARGETING;
		mTarget = target;
		Radius = inWeapon._TouchRadius;
		mMinSpeed = mWeapon._MinSpeed;
		if (mTarget != null)
		{
			mMinSpeed += parentSpeed;
		}
		UpdateSpeed();
		if (target != null)
		{
			direction = (GetTargetPosition() - base.transform.position).normalized;
		}
		if (direction.magnitude > 1.1f)
		{
			base.transform.LookAt(direction, Vector3.up);
		}
		else
		{
			base.transform.LookAt(base.transform.position + direction, Vector3.up);
		}
		mForward = base.transform.forward;
	}

	protected Vector3 GetTargetPosition()
	{
		if (mTarget != null)
		{
			Collider component = mTarget.GetComponent<Collider>();
			if (!(component == null))
			{
				return component.bounds.center;
			}
			return mTarget.position;
		}
		return Vector3.zero;
	}

	public void DespawnDelay(float delay)
	{
		Invoke("DisableProjectile", delay);
	}

	public void DisableProjectile()
	{
		base.gameObject.SetActive(value: false);
	}

	protected void UpdateSpeed()
	{
		Vector3 targetPosition = GetTargetPosition();
		if (targetPosition != Vector3.zero && mWeapon._MaxTimeToTarget > 0f)
		{
			if (mWeapon._MaxTimeToTarget - mElapsedTime > 0f)
			{
				float num = (targetPosition - base.transform.position).magnitude / (mWeapon._MaxTimeToTarget - mElapsedTime);
				if (num < mMinSpeed)
				{
					num = mMinSpeed;
				}
				mSpeed = num;
			}
			else
			{
				mSpeed = mMinSpeed;
			}
		}
		else
		{
			mSpeed = mMinSpeed;
		}
	}

	protected virtual void FixedUpdate()
	{
		mElapsedTime += Time.deltaTime;
		UpdateSpeed();
		if (mState == State.TARGETING)
		{
			if (mElapsedTime >= mWeapon._Lifetime)
			{
				DeActivate();
				return;
			}
			Vector3 forward = base.transform.forward;
			Vector3 position = base.transform.position;
			if (mTarget != null)
			{
				Vector3 targetPosition = GetTargetPosition();
				base.transform.LookAt(targetPosition);
				forward = (targetPosition - position).normalized;
				Velocity = forward * mSpeed;
				if (Vector3.Distance(targetPosition, base.transform.position) < mWeapon._TouchRadius)
				{
					OnHit(mTarget, targetPosition);
				}
			}
			else
			{
				Velocity = mForward * mSpeed;
			}
		}
		else if (mState == State.EXPLODING && mElapsedTime / 1f > 1f)
		{
			DeActivate();
		}
	}

	protected void Despawn()
	{
		if (mAmmoPool == null)
		{
			PoolManager.Pools.TryGetValue(_AmmoPoolName, out mAmmoPool);
		}
		if (mAmmoPool != null)
		{
			mState = State.NONE;
			mAmmoPool.Despawn(base.transform);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	public virtual void DeActivate()
	{
		Despawn();
		if (!mTargetHit && pOnAmmoMissed != null)
		{
			pOnAmmoMissed();
		}
	}

	protected override bool OnCollision(RaycastHit hit)
	{
		return OnHit(hit.transform, hit.point);
	}

	protected bool OnHit(Transform target, Vector3 hitPos)
	{
		mHitPos = hitPos;
		if (mCreator == null || target.IsChildOf(mCreator))
		{
			return false;
		}
		mElapsedTime = 0f;
		mState = State.EXPLODING;
		Invoke("HideAmmo", 0.5f);
		if (mHitPrefab != null)
		{
			PlayHitParticle(base.transform.position, mHitPrefab);
		}
		ObTargetable component = target.GetComponent<ObTargetable>();
		if (component != null)
		{
			if (!_DisableHitSoundObjectList.Contains(component.gameObject))
			{
				PlayHitSoundAt(hitPos);
			}
			int damage = mWeapon._Damage;
			int num = Random.Range(0, 100);
			bool isCritical = false;
			if (num < mWeapon._CriticalDamageChance)
			{
				damage = (int)(mWeapon._CriticalDamageMultiplier * (float)mWeapon._Damage + (float)mWeapon._Damage);
				isCritical = true;
			}
			component.OnDamage(damage, IsLocal, isCritical);
			mTargetHit = true;
			AvAvatarController avAvatarController = target.GetComponent(typeof(AvAvatarController)) as AvAvatarController;
			if (avAvatarController != null && AvAvatar.pObject == target.gameObject)
			{
				avAvatarController.TakeHit(mWeapon._Damage, this);
			}
		}
		else
		{
			PlayHitSoundAt(hitPos);
		}
		if (IsLocal)
		{
			target.gameObject.SendMessage("OnAmmoHit", this, SendMessageOptions.DontRequireReceiver);
		}
		return true;
	}

	public void PlayHitSoundAt(Vector3 position)
	{
		if (mWeapon != null && mWeapon._HitSound != null && mWeapon._HitSoundSourcePrefab != null)
		{
			if (mSoundPool == null)
			{
				PoolManager.Pools.TryGetValue(_SoundPoolName, out mSoundPool);
			}
			GameObject gameObject = ((mSoundPool != null) ? mSoundPool.Spawn(mWeapon._HitSoundSourcePrefab, position, Quaternion.identity) : Object.Instantiate(mWeapon._HitSoundSourcePrefab, position, Quaternion.identity));
			SnChannel component = gameObject.GetComponent<SnChannel>();
			component.pClip = mWeapon._HitSound;
			component.pEventTarget = gameObject;
			component.Play();
		}
	}

	private void HideAmmo()
	{
		UtUtilities.HideObject(base.gameObject, t: true);
	}

	public virtual void PlayHitParticle(Vector3 pos, GameObject _hitPrefab)
	{
		if (!(_hitPrefab != null))
		{
			return;
		}
		if (mParticlePool == null)
		{
			PoolManager.Pools.TryGetValue(_ParticlePoolName, out mParticlePool);
		}
		GameObject gameObject = ((mParticlePool != null) ? mParticlePool.Spawn(_hitPrefab, pos, Quaternion.identity) : Object.Instantiate(_hitPrefab, pos, Quaternion.identity));
		if (gameObject != null)
		{
			gameObject.transform.forward = -base.transform.forward;
			if (UtPlatform.IsWSA())
			{
				UtUtilities.ReAssignShader(gameObject);
			}
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere(base.transform.position, Radius);
	}
}
