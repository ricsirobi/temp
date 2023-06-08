using System.Collections;
using UnityEngine;

public class ObAnimate : KAMonoBase
{
	public float _Delay;

	public float _ReturnFade = 0.3f;

	public AnimationClip _Animation;

	public WrapMode _WrapMode = WrapMode.Loop;

	public bool _Reverse;

	public bool _ForceStopCurrentCycle = true;

	public bool _PlayOnce;

	public SnSound _Sound;

	public AudioClip[] _Sounds;

	public bool _SoundFinish;

	public bool _NoRides;

	public AudioClip _NoRidesVO;

	public bool _DisablePlayerMovement;

	protected bool mActivated;

	private bool mInitAnimStateValue;

	private bool mAnimPlayed;

	private float mRemainingAnimTime;

	private float mDelay;

	private string mDefaultAnimation = "";

	private string mAnimToPlay = "";

	private WrapMode mDefaultWrapMode;

	private SnChannel mChannel;

	public virtual void Awake()
	{
		if (base.animation != null && base.animation.clip != null)
		{
			mDefaultAnimation = base.animation.clip.name;
			mDefaultWrapMode = base.animation.wrapMode;
		}
		if (_Animation != null)
		{
			mAnimToPlay = _Animation.name;
		}
		mDelay = _Delay;
	}

	public void OnStateChange(bool switchOn)
	{
		mActivated = switchOn;
		if (_DisablePlayerMovement)
		{
			StartCoroutine(DisableMovement());
		}
	}

	private IEnumerator DisableMovement()
	{
		if ((_PlayOnce && mAnimPlayed) || !base.animation || !_Animation || _Animation.length <= 0f)
		{
			yield return null;
		}
		while (!base.animation.isPlaying)
		{
			yield return new WaitForEndOfFrame();
		}
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.pInputEnabled = false;
		yield return new WaitForSeconds(_Animation.length);
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pInputEnabled = true;
		yield return null;
	}

	public virtual void Update()
	{
		if (base.animation == null || _Animation == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(mDefaultAnimation) && !base.animation.IsPlaying(mDefaultAnimation) && !base.animation.IsPlaying(mAnimToPlay))
		{
			if (_Sound._AudioClip != null && mChannel != null && SnUtility.SafeClipCompare(mChannel.pAudioSource.clip, _Sound._AudioClip))
			{
				if (_SoundFinish)
				{
					mChannel.pAudioSource.loop = false;
				}
				else
				{
					mChannel.Stop();
				}
				mChannel = null;
			}
			base.animation.CrossFade(mDefaultAnimation, _ReturnFade);
			base.animation[mDefaultAnimation].wrapMode = mDefaultWrapMode;
		}
		else if (!IsTriggered())
		{
			if (!_ForceStopCurrentCycle && base.animation.IsPlaying(mAnimToPlay))
			{
				float normalizedTime = base.animation[mAnimToPlay].normalizedTime;
				if (mInitAnimStateValue)
				{
					mRemainingAnimTime = (int)normalizedTime + 1;
					mInitAnimStateValue = false;
				}
				if (normalizedTime < mRemainingAnimTime)
				{
					return;
				}
			}
			if (_Sound._AudioClip != null && mChannel != null && SnUtility.SafeClipCompare(mChannel.pAudioSource.clip, _Sound._AudioClip))
			{
				if (_SoundFinish)
				{
					mChannel.pAudioSource.loop = false;
				}
				else
				{
					mChannel.Stop();
				}
				mChannel = null;
			}
			if (_Reverse && base.animation.IsPlaying(mAnimToPlay))
			{
				AnimationState animationState = base.animation[mAnimToPlay];
				animationState.speed = -1f;
				animationState.wrapMode = WrapMode.ClampForever;
				if (animationState.normalizedTime > 1f)
				{
					animationState.normalizedTime = 1f;
				}
				if (animationState.time > 0f)
				{
					return;
				}
			}
			if (!string.IsNullOrEmpty(mDefaultAnimation) && !base.animation.IsPlaying(mDefaultAnimation))
			{
				mDelay = 0f;
				base.animation.CrossFade(mDefaultAnimation, _ReturnFade);
				base.animation[mDefaultAnimation].wrapMode = mDefaultWrapMode;
			}
		}
		else if (_NoRides && AvAvatar.IsPlayerOnRide())
		{
			mChannel = SnChannel.Play(_NoRidesVO, "VO_Pool", 0, inForce: true);
		}
		else if (!base.animation.IsPlaying(mAnimToPlay))
		{
			if (_PlayOnce && mAnimPlayed)
			{
				return;
			}
			AnimationState animationState2 = base.animation[mAnimToPlay];
			if (mDelay <= 0f)
			{
				if (_Sounds != null && _Sounds.Length != 0)
				{
					_Sound._AudioClip = _Sounds[Random.Range(0, _Sounds.Length)];
				}
				if (_Sound._AudioClip != null)
				{
					mChannel = _Sound.Play();
				}
				base.animation.CrossFade(mAnimToPlay);
				animationState2.speed = 1f;
				animationState2.wrapMode = _WrapMode;
				mInitAnimStateValue = true;
				mAnimPlayed = true;
				mDelay = _Delay;
			}
			else if (_Delay > 0f)
			{
				mDelay -= Time.deltaTime;
			}
		}
		else if (base.animation[mAnimToPlay].speed < 0f)
		{
			base.animation[mAnimToPlay].speed = 1f;
			AnimationState animationState3 = base.animation[mAnimToPlay];
			if (animationState3.normalizedTime < 0f)
			{
				animationState3.normalizedTime = 0f;
			}
		}
	}

	protected virtual bool IsTriggered()
	{
		return mActivated;
	}

	public void OnSetDefaultAnimation(string name)
	{
		mDefaultAnimation = name;
	}
}
