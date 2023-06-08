using KnowledgeAdventure.Multiplayer.Events;
using UnityEngine;

public class GasCloudPowerUp : PowerUp
{
	public float _FieldEffectOffsetDist;

	public float _FieldRadius = 50f;

	public GameObject _HitScreenEffect;

	public float _HitFlyingSlowDownFactor = 0.9f;

	public float _HitFlyingMaxSpeedFactor = 0.6f;

	public float _BuffDurationOnFieldExit = 0.5f;

	public AudioClip _GasHitAudioClip;

	private GameObject mHitScreenEffectObject;

	private float mPrevFlyingMaxSpeed = -1f;

	private float mBuffTimer = -1f;

	private bool mTriggerAccepted;

	public override void Init(MonoBehaviour gameManager, PowerUpManager manager, MMOMessageReceivedEventArgs args)
	{
		base.Init(gameManager, manager, args);
		AvatarRacing component = AvAvatar.pObject.GetComponent<AvatarRacing>();
		if (!(component != null) || RacingManager.pIsSinglePlayer)
		{
			return;
		}
		string[] array = args.MMOMessage.MessageText.Split(':');
		if (!(mParticleSys != null) || array.Length <= 2)
		{
			return;
		}
		Vector3 inDefaultPos = Vector3.zero;
		UtUtilities.Vector3FromString(array[2], ref inDefaultPos);
		base.transform.position = inDefaultPos;
		mParticleSys.transform.parent = base.transform;
		mParticleSys.transform.localPosition = Vector3.zero;
		mParticleSys.transform.localEulerAngles = Vector3.zero;
		mParticleSys.gameObject.SetActive(value: true);
		bool flag = args.MMOMessage.Sender.Username == ProductConfig.pToken || args.MMOMessage.Sender.Username == WsWebService.pUserToken;
		SphereCollider component2 = GetComponent<SphereCollider>();
		if (component2 != null)
		{
			component2.enabled = false;
			if (!flag)
			{
				component2.radius = _FieldRadius;
				component2.enabled = true;
			}
		}
		base.Activate();
		mBuffTimer = -1f;
		mPrevFlyingMaxSpeed = component.GetComponent<AvAvatarController>().pFlyingData._Speed.Max;
	}

	public override void Update()
	{
		base.Update();
		if (mActive && mBuffTimer > -1f)
		{
			mBuffTimer -= Time.deltaTime;
			if (mBuffTimer <= 0f)
			{
				DeActivate();
			}
		}
	}

	private void OnTriggerEnter(Collider inCollider)
	{
		if (!(inCollider.transform.root.gameObject == AvAvatar.pObject))
		{
			return;
		}
		if (PowerUp.pImmune)
		{
			PowerUp.pHitCount++;
		}
		else if (!IsCurrAvatarImmune())
		{
			if (_GasHitAudioClip != null)
			{
				SnChannel.Play(_GasHitAudioClip, "SFX_Pool", inForce: true);
			}
			if (_HitScreenEffect != null && mHitScreenEffectObject == null)
			{
				mHitScreenEffectObject = Object.Instantiate(_HitScreenEffect, Vector3.zero, Quaternion.identity);
			}
			SetCurrAvatarImmune(isImmune: true);
			mTriggerAccepted = true;
		}
	}

	private void OnTriggerStay(Collider inCollider)
	{
		if (inCollider.gameObject == AvAvatar.pObject && !PowerUp.pImmune && (!IsCurrAvatarImmune() || mTriggerAccepted))
		{
			SlowDownFlySpeed();
		}
	}

	private void OnTriggerExit(Collider inCollider)
	{
		if (inCollider.gameObject == AvAvatar.pObject && !PowerUp.pImmune && (!IsCurrAvatarImmune() || mTriggerAccepted))
		{
			mBuffTimer = _BuffDurationOnFieldExit;
		}
	}

	public override void Activate()
	{
		AvatarRacing component = AvAvatar.pObject.GetComponent<AvatarRacing>();
		if (component != null && !RacingManager.pIsSinglePlayer)
		{
			Vector3 vector = component.transform.position - component.transform.forward * _FieldEffectOffsetDist;
			SendMMOMessage("POWERUP:" + _Type + ":" + vector.ToString());
		}
	}

	public override void DeActivate()
	{
		base.DeActivate();
		if (mHitScreenEffectObject != null)
		{
			Object.Destroy(mHitScreenEffectObject);
		}
		SphereCollider component = GetComponent<SphereCollider>();
		if (component != null)
		{
			component.enabled = false;
		}
		AvAvatarController component2 = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (component2 != null)
		{
			component2.pFlyingData._Speed.Max = mPrevFlyingMaxSpeed;
		}
		mTriggerAccepted = false;
		SetCurrAvatarImmune(isImmune: false);
	}

	private void SlowDownFlySpeed()
	{
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (component != null)
		{
			component.pFlightSpeed -= component.pFlightSpeed * (_HitFlyingSlowDownFactor * Time.deltaTime);
			component.pVelocity -= component.pVelocity * (_HitFlyingSlowDownFactor * Time.deltaTime);
			component.pFlyingData._Speed.Max = mPrevFlyingMaxSpeed * _HitFlyingMaxSpeedFactor;
		}
	}
}
