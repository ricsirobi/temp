using UnityEngine;

public class AIBehavior_PlayRandomAnim : AIBehavior
{
	public string[] _Animations;

	public float _InitialFade = 0.4f;

	public float _InLoopFadeTime = 0.2f;

	public bool _Looped = true;

	public bool _ForceAlwaysStartByFirstAnim = true;

	private int mCurrentAnim = -1;

	private void PlayAnim(AIActor Actor)
	{
		if (_Animations.Length != 0)
		{
			mCurrentAnim = ((!_ForceAlwaysStartByFirstAnim || mCurrentAnim != -1) ? Random.Range(0, _Animations.Length - 1) : 0);
			float num = ((mCurrentAnim == -1) ? _InitialFade : _InLoopFadeTime);
			if (num > 0f)
			{
				Actor.animation.CrossFade(_Animations[mCurrentAnim], num);
			}
			else
			{
				Actor.animation.Play(_Animations[mCurrentAnim]);
			}
			Actor.animation[_Animations[mCurrentAnim]].wrapMode = WrapMode.Once;
		}
	}

	public override AIBehaviorState Think(AIActor Actor)
	{
		if (mCurrentAnim == -1 || State != AIBehaviorState.ACTIVE)
		{
			PlayAnim(Actor);
			if (mCurrentAnim == -1)
			{
				return SetState(AIBehaviorState.COMPLETED);
			}
			return SetState(AIBehaviorState.ACTIVE);
		}
		if (!IsPlayingAnim(Actor))
		{
			if (!_Looped)
			{
				return SetState(AIBehaviorState.COMPLETED);
			}
			PlayAnim(Actor);
		}
		return SetState(AIBehaviorState.ACTIVE);
	}

	public bool IsPlayingAnim(AIActor Actor)
	{
		if (mCurrentAnim == -1)
		{
			return false;
		}
		if (!Actor.animation.IsPlaying(_Animations[mCurrentAnim]))
		{
			return false;
		}
		AnimationState animationState = Actor.animation[_Animations[mCurrentAnim]];
		if (animationState.length - animationState.time <= _InLoopFadeTime)
		{
			return false;
		}
		return true;
	}

	public override void OnStart(AIActor Actor)
	{
		PlayAnim(Actor);
		if (mCurrentAnim != -1)
		{
			SetState(AIBehaviorState.ACTIVE);
		}
		else
		{
			SetState(AIBehaviorState.COMPLETED);
		}
	}

	public override void OnTerminate(AIActor Actor)
	{
		SetState(AIBehaviorState.INACTIVE);
		mCurrentAnim = -1;
	}
}
