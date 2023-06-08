using UnityEngine;

public class ObTriggerAnimate : ObTrigger
{
	public GameObject _Object;

	public float _Delay;

	public float _ReturnFade = 0.3f;

	public AnimationClip _Animation;

	public WrapMode _WrapMode = WrapMode.Loop;

	protected Animation mAnimation;

	protected float mDelay;

	private string mDefaultAnimation;

	private WrapMode mDefaultWrapMode;

	public virtual void Awake()
	{
		mAnimation = base.animation;
		if (_Object != null)
		{
			mAnimation = _Object.GetComponent<Animation>();
		}
		if (mAnimation != null)
		{
			if (mAnimation.clip != null)
			{
				mDefaultAnimation = mAnimation.clip.name;
			}
			mDefaultWrapMode = mAnimation.wrapMode;
		}
	}

	public override void DoTriggerAction(GameObject other)
	{
		base.DoTriggerAction(other);
		if (mAnimation != null && _Animation != null && !mAnimation.IsPlaying(_Animation.name))
		{
			if (mDelay <= 0f)
			{
				mAnimation.CrossFade(_Animation.name);
				mAnimation[_Animation.name].wrapMode = _WrapMode;
				mDelay = _Delay;
			}
			else if (_Delay > 0f)
			{
				mDelay -= Time.deltaTime;
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (mAnimation != null && _Animation != null && !string.IsNullOrEmpty(mDefaultAnimation) && !mAnimation.IsPlaying(mDefaultAnimation) && !mAnimation.IsPlaying(_Animation.name))
		{
			mAnimation.CrossFade(mDefaultAnimation, _ReturnFade);
			mAnimation[mDefaultAnimation].wrapMode = mDefaultWrapMode;
		}
	}
}
