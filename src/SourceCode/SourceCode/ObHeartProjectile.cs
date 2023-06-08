using UnityEngine;

public class ObHeartProjectile : PrProjectile
{
	public float _MinSpeed;

	public GameObject _CardPrefab;

	public override void Start()
	{
		AvAvatarController component = base.pProjectileSource.GetComponent<AvAvatarController>();
		if (component.pVelocity.magnitude < 1f)
		{
			base.rigidbody.velocity = base.pProjectileSource.transform.forward * _MinSpeed + Vector3.up * 0.2f * _MinSpeed;
		}
		else
		{
			base.rigidbody.velocity = component.pVelocity.magnitude * _ProjectileSpeed * base.pProjectileSource.transform.forward;
		}
		mDestroyTimer = _LifeTime;
		base.gameObject.transform.LookAt(base.pTargetPosition);
		if ((bool)_AttackEffect)
		{
			Object.Instantiate(_AttackEffect, base.transform.position, base.transform.rotation);
		}
	}

	protected override void OnCollisionEnter(Collision iCollision)
	{
		if (HasHitOtherPlayer(iCollision))
		{
			GameObject gameObject = iCollision.gameObject;
			if (AvAvatar.IsCurrentPlayer(gameObject))
			{
				SpawnCardForPlayer(gameObject);
			}
		}
		base.OnCollisionEnter(iCollision);
	}

	private void SpawnCardForPlayer(GameObject player)
	{
		if (_CardPrefab != null)
		{
			GameObject gameObject = Object.Instantiate(_CardPrefab, player.transform.position, player.transform.rotation);
			if (gameObject == null)
			{
				UtDebug.LogError("Unable to spawn card");
				return;
			}
			gameObject.SetActive(value: true);
			gameObject.transform.parent = player.transform;
		}
	}
}
