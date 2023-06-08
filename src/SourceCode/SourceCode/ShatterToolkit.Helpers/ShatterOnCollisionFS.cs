using UnityEngine;

namespace ShatterToolkit.Helpers;

public class ShatterOnCollisionFS : ShatterOnCollision
{
	public float _ShatterVelocity = 5f;

	public float _FlyingSlowdownSpeedFactor = 0.6f;

	public float _DestructTimer = 5f;

	public AudioClip _ShatterSfx;

	public GameObject _TreeBreakingParticle;

	public int _ScoreOnHit;

	public void LateUpdate()
	{
		if (!UiAvatarControls.pInstance.pEnableFireOnButtonUp)
		{
			base.tag = "IgnoreFlyCollider";
		}
		else
		{
			base.tag = "Untagged";
		}
	}

	protected virtual void OnTriggerEnter(Collider inCollider)
	{
		if (UiAvatarControls.pInstance.pEnableFireOnButtonUp)
		{
			return;
		}
		bool flag = AvAvatar.IsCurrentPlayer(inCollider.gameObject) || (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pFlyCollider == inCollider.transform);
		if (flag || inCollider.gameObject.GetComponent<ObAmmo>() != null)
		{
			Collider component = base.gameObject.GetComponent<Collider>();
			if (component != null)
			{
				component.isTrigger = false;
			}
			ObSelfDestructTimer component2 = GetComponent<ObSelfDestructTimer>();
			if (component2 != null)
			{
				component2._SelfDestructTime = _DestructTimer;
				component2.enabled = true;
			}
			SendMessage("Shatter", base.transform.position, SendMessageOptions.RequireReceiver);
			if (flag)
			{
				SetAvatarFlyingSpeedFactor(_FlyingSlowdownSpeedFactor);
			}
			if (_ShatterSfx != null)
			{
				SnChannel.Play(_ShatterSfx, "SFX_Pool", inForce: true, null);
			}
			if (AvAvatar.pToolbar != null)
			{
				AvAvatar.pToolbar.BroadcastMessage("HitObject", base.gameObject, SendMessageOptions.DontRequireReceiver);
			}
			if (_TreeBreakingParticle != null)
			{
				GameObject obj = Object.Instantiate(_TreeBreakingParticle, base.transform.position, base.transform.rotation);
				obj.GetComponentInChildren<ParticleSystem>().Play();
				obj.AddComponent<ObSelfDestructTimer>()._SelfDestructTime = _DestructTimer;
			}
		}
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
}
