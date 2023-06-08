using UnityEngine;

public class ObSnowball : PrProjectile
{
	public float _MinSnowballSpeed;

	public override void Start()
	{
		AvAvatarController component = base.pProjectileSource.GetComponent<AvAvatarController>();
		if (component.pVelocity.magnitude < 1f)
		{
			base.rigidbody.velocity = base.pProjectileSource.transform.forward * _MinSnowballSpeed + Vector3.up * 0.2f * _MinSnowballSpeed;
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
}
