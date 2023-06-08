using UnityEngine;

public class PrProjectile : KAMonoBase
{
	public float _ProjectileSpeed = 40f;

	public float _LifeTime = 2f;

	public GameObject _AttackEffect;

	public GameObject _CollideEffect;

	public Vector3 _Force;

	public int _Damage = 1;

	public bool _FollowsTerrain = true;

	public Vector2 _DistanceVector = Vector2.zero;

	private GameObject mProjectileSource;

	protected float mDestroyTimer;

	private Vector3 mTargetPosition = Vector3.zero;

	private Vector3 mProjectileVelocity = Vector3.zero;

	private bool mProjectilePaused;

	public GameObject pProjectileSource
	{
		get
		{
			return mProjectileSource;
		}
		set
		{
			mProjectileSource = value;
		}
	}

	public Vector3 pTargetPosition
	{
		get
		{
			return mTargetPosition;
		}
		set
		{
			mTargetPosition = value;
		}
	}

	public void PauseProjectile(bool pause)
	{
		mProjectilePaused = pause;
		if (pause)
		{
			base.rigidbody.velocity = Vector3.zero;
		}
		else
		{
			base.rigidbody.velocity = mProjectileVelocity;
		}
	}

	public virtual void Start()
	{
		Vector3 vector = pTargetPosition - base.transform.position;
		vector.Normalize();
		mDestroyTimer = _LifeTime;
		base.rigidbody.velocity = vector * _ProjectileSpeed;
		mProjectileVelocity = base.rigidbody.velocity;
		base.gameObject.transform.LookAt(pTargetPosition);
		if ((bool)_AttackEffect)
		{
			Object.Instantiate(_AttackEffect, base.transform.position, base.transform.rotation);
		}
	}

	private void Update()
	{
		if (mProjectilePaused)
		{
			return;
		}
		if (_FollowsTerrain)
		{
			float groundHeight = UtUtilities.GetGroundHeight(base.transform.position, float.PositiveInfinity);
			float num = base.transform.position.y - groundHeight;
			if (!(num < _DistanceVector.x) && !(num > _DistanceVector.y))
			{
				base.transform.position = new Vector3(base.transform.position.x, groundHeight + _DistanceVector.y, base.transform.position.z);
			}
		}
		mDestroyTimer -= Time.deltaTime;
		if (mDestroyTimer <= 0f)
		{
			DestroyMe();
		}
	}

	public virtual void DestroyMe()
	{
		PrProjectileManager.DestroyProjectile(base.gameObject);
		if ((bool)_CollideEffect)
		{
			Object.Instantiate(_CollideEffect, base.transform.position, base.transform.rotation);
		}
		Object.Destroy(base.gameObject);
	}

	protected bool HasHitOtherPlayer(Collision iCollision)
	{
		if (pProjectileSource == iCollision.gameObject)
		{
			return false;
		}
		if ((bool)pProjectileSource && iCollision.gameObject.CompareTag("Player"))
		{
			return true;
		}
		return false;
	}

	protected virtual void OnCollisionEnter(Collision iCollision)
	{
		if (HasHitOtherPlayer(iCollision))
		{
			pProjectileSource.gameObject.SendMessage("ProjectileHitAvatar", null, SendMessageOptions.DontRequireReceiver);
		}
		DestroyMe();
	}
}
