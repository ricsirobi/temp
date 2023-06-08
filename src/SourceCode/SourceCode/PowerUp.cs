using KnowledgeAdventure.Multiplayer.Events;
using UnityEngine;

public abstract class PowerUp : MonoBehaviour
{
	public string _Type;

	public float _Duration;

	public float _InvulnerableDuration = -1f;

	public AudioClip _SndActivate;

	public SnSettings _SndSettings;

	public GameObject _Particle;

	public Vector3 _ParticlePos = new Vector3(0f, 0f, 0f);

	public Vector3 _ParticleRot = new Vector3(0f, 0f, 0f);

	public bool _IsSingleInstance = true;

	protected MonoBehaviour mGameManager;

	protected float mPowerUpDuration;

	protected bool mActive;

	protected bool mIsGamePaused;

	public static bool pImmune;

	public static int pHitCount;

	private string mHostUserId = "";

	protected ParticleSystem mParticleSys;

	protected PowerUpManager mPowerUpManager;

	private SnChannel mChannel;

	public string pHostUserId
	{
		get
		{
			return mHostUserId;
		}
		set
		{
			mHostUserId = value;
		}
	}

	public virtual void Activate()
	{
		mActive = true;
		mIsGamePaused = false;
		mPowerUpDuration = _Duration;
		if (_SndActivate != null)
		{
			mChannel = SnChannel.Play(_SndActivate, _SndSettings, inForce: true);
		}
	}

	public virtual void DeActivate()
	{
		mActive = false;
		if (mChannel != null)
		{
			mChannel.pLoop = false;
			mChannel = null;
		}
		if (_SndActivate != null)
		{
			SnChannel.StopPool("SFX_Pool");
		}
		if (mParticleSys != null)
		{
			ParticleManager.Despawn(mParticleSys.transform);
		}
	}

	public bool IsActive()
	{
		return mActive;
	}

	public virtual void SendMMOMessage(string message)
	{
		message = message + ":" + (MMOTimeManager.pInstance.GetServerTime() + 0.0);
		MainStreetMMOClient.pInstance.SendPublicMMOMessage(message);
	}

	public virtual void Init(MonoBehaviour gameManager, PowerUpManager powerUpManager)
	{
		mGameManager = gameManager;
		mPowerUpManager = powerUpManager;
		InitParticle();
	}

	public virtual void Init(MonoBehaviour gameManager, PowerUpManager powerUpManager, MMOMessageReceivedEventArgs args)
	{
		Init(gameManager, powerUpManager);
		mPowerUpDuration = _Duration;
	}

	private void InitParticle()
	{
		if (_Particle != null)
		{
			ParticleSystem component = _Particle.GetComponent<ParticleSystem>();
			mParticleSys = ParticleManager.PlayParticle(component, _ParticlePos, Quaternion.identity);
			mParticleSys.gameObject.SetActive(value: false);
		}
	}

	public virtual void Update()
	{
		if (mActive && !mIsGamePaused && mPowerUpDuration > 0f)
		{
			mPowerUpDuration -= Time.deltaTime;
			if (mPowerUpDuration <= 0f)
			{
				mActive = false;
				DeActivate();
			}
		}
	}

	public virtual void GamePause(bool pause)
	{
		mIsGamePaused = pause;
	}

	public virtual void Action(string inMessage, string inMsgSourceUserID)
	{
	}

	public virtual void ResetDuration()
	{
		mPowerUpDuration = _Duration;
	}

	public void SetCurrAvatarImmune(bool isImmune)
	{
		if (isImmune || !pImmune)
		{
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if (component != null)
			{
				component.SetImmune(isImmune, _InvulnerableDuration);
			}
		}
	}

	public bool IsCurrAvatarImmune()
	{
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (component != null)
		{
			return component.pImmune;
		}
		return false;
	}
}
