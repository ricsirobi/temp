using UnityEngine;

public class AIBehavior_PlayAnim : AIBehavior
{
	public string _AnimName;

	public WrapMode _Mode = WrapMode.Once;

	public float _CrossFadeTime = 0.1f;

	public bool _Queue;

	public bool _ResetAnim;

	public bool _CompletedAfterPlay;

	public bool _ClearAnimNameWhenCompleted;

	public int _NumLoops;

	private int mNumLoops;

	public override bool CanBeExecuted(AIActor Actor)
	{
		if (string.IsNullOrEmpty(_AnimName))
		{
			return false;
		}
		return true;
	}

	public override AIBehaviorState Think(AIActor Actor)
	{
		if (_CompletedAfterPlay && State == AIBehaviorState.COMPLETED)
		{
			return State;
		}
		if (State == AIBehaviorState.INACTIVE || State == AIBehaviorState.COMPLETED)
		{
			OnStart(Actor);
			return State;
		}
		if (State == AIBehaviorState.FAILED)
		{
			return State;
		}
		if (Actor.animation.IsPlaying(_AnimName))
		{
			if (_CompletedAfterPlay)
			{
				if (_ClearAnimNameWhenCompleted)
				{
					_AnimName = "";
				}
				return SetState(AIBehaviorState.COMPLETED);
			}
			return SetState(AIBehaviorState.ACTIVE);
		}
		if (ShouldPlayForever())
		{
			PlayAnim(Actor);
			return State;
		}
		if (mNumLoops > 0)
		{
			PlayAnim(Actor);
			mNumLoops--;
			return State;
		}
		if (_ClearAnimNameWhenCompleted)
		{
			_AnimName = "";
		}
		return SetState(AIBehaviorState.COMPLETED);
	}

	public override void OnStart(AIActor Actor)
	{
		if (Actor.animation[_AnimName] == null)
		{
			SetState(AIBehaviorState.FAILED);
			return;
		}
		mNumLoops = _NumLoops;
		PlayAnim(Actor);
		SetState(AIBehaviorState.ACTIVE);
	}

	public override void OnTerminate(AIActor Actor)
	{
		SetState(AIBehaviorState.INACTIVE);
	}

	private bool ShouldPlayForever()
	{
		if (_Mode != WrapMode.ClampForever && _Mode != WrapMode.Loop)
		{
			return _Mode == WrapMode.PingPong;
		}
		return true;
	}

	private void PlayAnim(AIActor Actor)
	{
		bool flag = Actor.animation.IsPlaying(_AnimName);
		if (!flag)
		{
			if (_Queue)
			{
				if (_CrossFadeTime > 0f)
				{
					Actor.animation.CrossFadeQueued(_AnimName, _CrossFadeTime);
				}
				else
				{
					Actor.animation.PlayQueued(_AnimName);
				}
			}
			else if (_CrossFadeTime > 0f)
			{
				Actor.animation.CrossFade(_AnimName, _CrossFadeTime, PlayMode.StopAll);
			}
			else
			{
				Actor.animation.Play(_AnimName, PlayMode.StopAll);
			}
		}
		if (_ResetAnim || !flag)
		{
			Actor.animation.Rewind(_AnimName);
		}
		if (_Mode != 0)
		{
			Actor.animation[_AnimName].wrapMode = _Mode;
		}
	}
}
