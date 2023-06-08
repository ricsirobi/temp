using UnityEngine;

public class ObTriggerAnimateFS : ObTriggerAnimate
{
	public enum AfterAnimateState
	{
		NONE,
		AVATAR_SPEED_SLOW,
		START_DESTRUCT_TIMER
	}

	public float _AnimationSpeed = 1f;

	public float _FlyingSlowdownSpeedFactor = 0.6f;

	public float _DestructTimer = 5f;

	public SnSound _Sound;

	private AfterAnimateState mAfterAnimState;

	public override void Awake()
	{
		mAnimation = base.animation;
		if (_Object != null)
		{
			mAnimation = _Object.GetComponent<Animation>();
		}
	}

	public override void Update()
	{
		base.Update();
		if (mAnimation.IsPlaying(_Animation.name) && mAfterAnimState > AfterAnimateState.NONE)
		{
			ProcessState();
		}
	}

	private void ProcessState()
	{
		if (mAfterAnimState == AfterAnimateState.AVATAR_SPEED_SLOW)
		{
			SetAvatarFlyingSpeedFactor(_FlyingSlowdownSpeedFactor);
		}
		else if (mAfterAnimState == AfterAnimateState.START_DESTRUCT_TIMER)
		{
			ObSelfDestructTimer component = GetComponent<ObSelfDestructTimer>();
			if (component != null)
			{
				component._SelfDestructTime = _DestructTimer;
				component.enabled = true;
			}
		}
		mAfterAnimState = AfterAnimateState.NONE;
	}

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
		if (!mAnimation.IsPlaying(_Animation.name) && mDelay <= 0f)
		{
			if (_Sound != null && _Sound._AudioClip != null)
			{
				_Sound.Play();
			}
			if (collider != null)
			{
				collider.enabled = false;
			}
			mAnimation[_Animation.name].speed = _AnimationSpeed;
			if (AvAvatar.IsCurrentPlayer(other))
			{
				mAfterAnimState = AfterAnimateState.AVATAR_SPEED_SLOW;
			}
			else if (other.GetComponent<ObAmmo>() != null)
			{
				mAfterAnimState = AfterAnimateState.START_DESTRUCT_TIMER;
			}
		}
		base.DoTriggerAction(other);
	}

	private void OnAmmoHit(ObAmmo projectile)
	{
		DoTriggerAction(projectile.gameObject);
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
