using UnityEngine;

public class ObTriggerParticleFS : ObTrigger
{
	public Transform _ExplosionParticleTransform;

	public Vector3 _ExplosionPosOffset;

	public SnSound _Sound;

	public float _FlyingSlowdownSpeedFactor = 0.6f;

	public float _DestructTimer = 5f;

	private GameObject mExplosionParticle;

	public override void OnTriggerEnter(Collider inCollider)
	{
		if (mInTrigger || (_UseGlobalActive && !ObClickable.pGlobalActive) || !_Active || (!AvAvatar.IsCurrentPlayer(inCollider.gameObject) && inCollider.GetComponent<ObAmmo>() == null))
		{
			return;
		}
		mInTrigger = true;
		if (CheckMemberStatus() && CheckRideStatus())
		{
			if (_Confirm)
			{
				ConfirmTrigger();
			}
			else
			{
				DoTriggerAction(inCollider.gameObject);
			}
		}
	}

	public override void DoTriggerAction(GameObject other)
	{
		if (_Sound != null && _Sound._AudioClip != null)
		{
			_Sound.Play();
		}
		if (AvAvatar.IsCurrentPlayer(other))
		{
			SetAvatarFlyingSpeedFactor(_FlyingSlowdownSpeedFactor);
		}
		else if (other.GetComponent<ObAmmo>() != null)
		{
			base.renderer.enabled = false;
			if (mExplosionParticle == null)
			{
				mExplosionParticle = Object.Instantiate(_ExplosionParticleTransform.gameObject, base.transform.position + _ExplosionPosOffset, base.transform.rotation);
				AddDestructTimer(base.gameObject, _DestructTimer);
			}
			collider.enabled = false;
		}
		base.DoTriggerAction(other);
	}

	private void SetAvatarFlyingSpeedFactor(float inSpeedFactor)
	{
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (component != null)
		{
			component.pFlightSpeed *= inSpeedFactor;
			component.pVelocity *= inSpeedFactor;
		}
	}

	private void AddDestructTimer(GameObject inObject, float inTime)
	{
		ObSelfDestructTimer obSelfDestructTimer = inObject.AddComponent<ObSelfDestructTimer>();
		if (obSelfDestructTimer != null)
		{
			obSelfDestructTimer._SelfDestructTime = inTime;
			obSelfDestructTimer.enabled = true;
		}
	}

	private void OnDestroy()
	{
		if (mExplosionParticle != null)
		{
			Object.Destroy(mExplosionParticle);
		}
	}
}
