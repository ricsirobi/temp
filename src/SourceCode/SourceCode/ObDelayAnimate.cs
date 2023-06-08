using UnityEngine;

public class ObDelayAnimate : KAMonoBase
{
	public float _Delay;

	public MinMax _RandomDelay;

	public float _ReturnFade = 0.3f;

	public AnimationClip _Animation;

	public WrapMode _WrapMode = WrapMode.Loop;

	public SnSound _Sound;

	public AudioClip[] _Sounds;

	private float mDelay;

	private string mDefaultAnimation = "";

	private WrapMode mDefaultWrapMode;

	private SnChannel mChannel;

	public void Awake()
	{
		if (base.animation.clip != null)
		{
			mDefaultAnimation = base.animation.clip.name;
		}
		mDefaultWrapMode = base.animation.wrapMode;
		if (_Delay > 0f)
		{
			mDelay = _Delay;
		}
		else
		{
			mDelay = _RandomDelay.GetRandomValue();
		}
	}

	public void Update()
	{
		if (!string.IsNullOrEmpty(mDefaultAnimation) && !base.animation.IsPlaying(mDefaultAnimation) && !base.animation.IsPlaying(_Animation.name))
		{
			if (_Sound._AudioClip != null && mChannel != null && mChannel.pAudioSource.clip == _Sound._AudioClip)
			{
				mChannel.Stop();
				mChannel = null;
			}
			base.animation.CrossFade(mDefaultAnimation, _ReturnFade);
			base.animation[mDefaultAnimation].wrapMode = mDefaultWrapMode;
		}
		else
		{
			if (base.animation.IsPlaying(_Animation.name))
			{
				return;
			}
			mDelay -= Time.deltaTime;
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
				base.animation.CrossFade(_Animation.name);
				base.animation[_Animation.name].wrapMode = _WrapMode;
				if (_Delay > 0f)
				{
					mDelay = _Delay;
				}
				else
				{
					mDelay = _RandomDelay.GetRandomValue();
				}
			}
		}
	}
}
