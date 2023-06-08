using UnityEngine;

public class TriggerAction : KAMonoBase
{
	public GameObject _GameObject;

	private Animation mAnimation;

	private string mDefaultAnimation;

	private WrapMode mDefaultWrapMode;

	private void Awake()
	{
		if (_GameObject != null)
		{
			mAnimation = _GameObject.GetComponent<Animation>();
		}
		if (mAnimation != null)
		{
			mDefaultAnimation = mAnimation.clip.name;
			mDefaultWrapMode = mAnimation.wrapMode;
		}
	}

	public void PlayAnim(string anim)
	{
		if (mAnimation != null && mAnimation[anim] != null)
		{
			mAnimation[anim].wrapMode = WrapMode.ClampForever;
			mAnimation.Play(anim);
		}
	}

	public void PlayAnimAdditive(string inAnim)
	{
		AnimationState animationState = base.animation[inAnim];
		if (animationState != null)
		{
			animationState.enabled = true;
			animationState.weight = 1f;
			animationState.time = 0f;
		}
	}

	public void PlayAnimLoop(string anim)
	{
		if (mAnimation != null && mAnimation[anim] != null)
		{
			mAnimation[anim].wrapMode = WrapMode.Loop;
			mAnimation.Play(anim);
		}
	}

	private void SetupForTask(Task task)
	{
		mAnimation[mDefaultAnimation].wrapMode = mDefaultWrapMode;
		mAnimation.Play(mDefaultAnimation);
	}
}
