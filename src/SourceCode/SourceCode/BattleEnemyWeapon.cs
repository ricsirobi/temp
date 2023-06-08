using System.Collections;
using UnityEngine;

public class BattleEnemyWeapon : WeaponManager
{
	public enum State
	{
		None,
		Reload,
		Patrol,
		Fire
	}

	public enum WeaponType
	{
		Projectile,
		Linear
	}

	public WeaponType _TypeWeapon;

	protected const string WEAPON_PREFIX = "Weapon_";

	public AudioClip _FireSFX;

	public string SFXPool = "";

	public float _AttackAngle = 20f;

	public float _AttackRange = 100f;

	public bool _DrawAttackCone;

	public float _RotationDegrees;

	public float _RotationTime;

	public GameObject _FiringParticleEffect;

	public string _FiringParticlePoolName;

	public string _FireAnimName;

	protected State mState;

	protected Vector3 mInitialDirection = Vector3.zero;

	protected Transform mTransform;

	private Vector3 mTargetPosition;

	private Animation mWeaponAnim;

	private bool mRotating;

	private bool mRotatedRight = true;

	private Vector3 mLeftRotation;

	private Vector3 mRightRotation;

	private SpawnPool mParticlePool;

	private void Start()
	{
		mTransform = base.transform;
		mInitialDirection = mTransform.forward;
		mWeaponAnim = GetComponent<Animation>();
		mLeftRotation = mTransform.localEulerAngles + new Vector3(0f, 0f - _RotationDegrees, 0f);
		mRightRotation = mTransform.localEulerAngles + new Vector3(0f, _RotationDegrees, 0f);
		if (mWeaponAnim != null)
		{
			mWeaponAnim.playAutomatically = false;
		}
		SetState(State.Reload);
	}

	private void SetState(State inState)
	{
		if (mState == inState)
		{
			return;
		}
		switch (inState)
		{
		case State.Reload:
			mState = inState;
			StartCoroutine(Reload());
			break;
		case State.Patrol:
			mState = inState;
			StartCoroutine(Patrol());
			break;
		case State.Fire:
		{
			mState = inState;
			Vector3 zero = Vector3.zero;
			SnChannel.Play(_FireSFX, SFXPool, inForce: true);
			if (_TypeWeapon == WeaponType.Projectile)
			{
				Rigidbody component = GetCurrentWeapon()._ProjectilePrefab.GetComponent<Rigidbody>();
				zero = GetThrowAngleForProjectile(GetCurrentWeapon()._MinSpeed, component.mass, base.pShootPoint, mTargetPosition);
				Fire(null, useDirection: true, zero, 30f);
			}
			else if (_TypeWeapon == WeaponType.Linear)
			{
				Quaternion rotation = mTransform.rotation;
				mTransform.LookAt(mTargetPosition);
				Fire(null, useDirection: false, Vector3.zero, 30f);
				mTransform.rotation = rotation;
			}
			if (mWeaponAnim != null && !string.IsNullOrEmpty(_FireAnimName) && mWeaponAnim.GetClip(_FireAnimName) != null)
			{
				mWeaponAnim.Play(_FireAnimName, PlayMode.StopAll);
			}
			if (_FiringParticleEffect != null)
			{
				if (mParticlePool == null)
				{
					PoolManager.Pools.TryGetValue(_FiringParticlePoolName, out mParticlePool);
				}
				if (mParticlePool != null)
				{
					mParticlePool.Spawn(_FiringParticleEffect, mTransform.position, mTransform.rotation);
				}
				else
				{
					Object.Instantiate(_FiringParticleEffect, mTransform.position, mTransform.rotation);
				}
			}
			SetState(State.Reload);
			break;
		}
		}
	}

	private Vector3 GetTargetDirection(Vector3 targetPos)
	{
		return (targetPos - mTransform.position).normalized;
	}

	private Vector3 GetThrowAngleForProjectile(float force, float mass, Vector3 source, Vector3 target, bool highThrow = false)
	{
		Vector3 vector = target - source;
		vector.Normalize();
		float num = Vector3.Distance(new Vector3(target.x, source.y, target.z), source);
		float num2 = Vector3.Distance(new Vector3(source.x, target.y, source.z), source);
		if (source.y > target.y)
		{
			num2 *= -1f;
		}
		float num3 = force / mass;
		float num4 = 0f - Physics.gravity.y;
		float num5 = -1f * num4 * num * num / (2f * num3 * num3);
		float num6 = Mathf.Sqrt(num * num - 4f * num5 * (num5 - num2));
		float num7 = 0f;
		num7 = (highThrow ? (Mathf.Atan((0f - num - num6) / (2f * num5)) * 57.29578f) : (Mathf.Atan((0f - num + num6) / (2f * num5)) * 57.29578f));
		Vector3 vector2 = new Vector3(vector.x, 0f, vector.z);
		Vector3 axis = Vector3.Cross(vector2, Vector3.up);
		Vector3 result = Quaternion.AngleAxis(num7, axis) * vector2;
		result.Normalize();
		return result;
	}

	protected override void Update()
	{
		base.Update();
		if (_DrawAttackCone && Application.isEditor)
		{
			DrawAttackCone();
		}
		if (_RotationDegrees > 0f && !mRotating)
		{
			if (mRotatedRight)
			{
				StartCoroutine(RotateWeapon(mRightRotation, mLeftRotation, _RotationTime));
			}
			else
			{
				StartCoroutine(RotateWeapon(mLeftRotation, mRightRotation, _RotationTime));
			}
		}
	}

	private IEnumerator Reload()
	{
		while (mState == State.Reload)
		{
			if (mCooldownTimer <= 0f)
			{
				SetState(State.Patrol);
			}
			yield return null;
		}
	}

	private IEnumerator Patrol()
	{
		while (mState == State.Patrol)
		{
			if (CanAttack(AvAvatar.position))
			{
				mTargetPosition = AvAvatar.position;
				if (mTargetPosition != Vector3.zero)
				{
					SetState(State.Fire);
				}
			}
			yield return null;
		}
	}

	protected virtual bool CanAttack(Vector3 position)
	{
		if (!BattleEnemy.pInstance.IsPlayerInSafeZone(position) && !BattleEnemy.pInstance._BattleEnemyTarget.pDying)
		{
			return Vector3.Angle(position - ShootPointTrans.position, ShootPointTrans.forward) <= _AttackAngle;
		}
		return false;
	}

	private IEnumerator RotateWeapon(Vector2 startRotation, Vector3 targetRotation, float seconds)
	{
		mRotating = true;
		if (mRotatedRight)
		{
			mRotatedRight = false;
		}
		else
		{
			mRotatedRight = true;
		}
		float t = 0f;
		float rate = 1f / seconds;
		while (t < 1f)
		{
			t += Time.deltaTime * rate;
			mTransform.localEulerAngles = Vector3.Lerp(startRotation, targetRotation, t);
			yield return null;
		}
		mRotating = false;
	}

	private void DrawAttackCone()
	{
		Vector3 vector = Quaternion.AngleAxis(_AttackAngle, ShootPointTrans.right) * ShootPointTrans.forward;
		Vector3 vector2 = Quaternion.AngleAxis(0f - _AttackAngle, ShootPointTrans.right) * ShootPointTrans.forward;
		Vector3 vector3 = Quaternion.AngleAxis(_AttackAngle, ShootPointTrans.up) * ShootPointTrans.forward;
		Vector3 vector4 = Quaternion.AngleAxis(0f - _AttackAngle, ShootPointTrans.up) * ShootPointTrans.forward;
		Debug.DrawRay(ShootPointTrans.position, vector * _AttackRange, Color.green);
		Debug.DrawRay(ShootPointTrans.position, vector2 * _AttackRange, Color.green);
		Debug.DrawRay(ShootPointTrans.position, vector3 * _AttackRange, Color.green);
		Debug.DrawRay(ShootPointTrans.position, vector4 * _AttackRange, Color.green);
	}
}
