using UnityEngine;

public class AIBehavior_Idle : AIBehavior
{
	public string[] _StandIdleAnims = new string[4] { "IdleStand", "IdleStand", "IdleStand", "PersonalityFidget01" };

	public string[] _SitIdleAnims = new string[1] { "IdleSit" };

	public string[] _SleepIdleAnims = new string[1] { "IdleSleep" };

	public string[] _SadIdleAnims = new string[1] { "IdleSad" };

	public string[] _FiredUpIdleAnims = new string[1] { "IdleHappy" };

	public AIBehavior_LookAtMouse _LookAtToDisable;

	protected AnimationState mCurrentAnim;

	protected float mAnimRemainingTime;

	protected int mIdleState = -1;

	protected int mNumAnimsRemaining;

	public override AIBehaviorState Think(AIActor Actor)
	{
		mAnimRemainingTime -= Actor.DeltaTime;
		if (mAnimRemainingTime > 0f)
		{
			return SetState(AIBehaviorState.ACTIVE);
		}
		if (mNumAnimsRemaining <= 0)
		{
			GotoNextIdleState(Actor);
		}
		else
		{
			PlayNextAnimation(Actor);
		}
		return SetState(AIBehaviorState.ACTIVE);
	}

	public override void OnStart(AIActor Actor)
	{
		mIdleState = -1;
		GotoNextIdleState(Actor);
		SetState(AIBehaviorState.ACTIVE);
	}

	public override void OnTerminate(AIActor Actor)
	{
		base.OnTerminate(Actor);
		if (_LookAtToDisable != null)
		{
			_LookAtToDisable.enabled = true;
		}
		mCurrentAnim = null;
	}

	public virtual void GotoNextIdleState(AIActor Actor)
	{
		mIdleState = Mathf.Min(2, mIdleState + 1);
	}

	public void PlayNextAnimation(AIActor Actor)
	{
		int num = 0;
		string[] array = null;
		switch (mIdleState)
		{
		case 0:
		{
			AIActor_Pet aIActor_Pet = Actor as AIActor_Pet;
			if (aIActor_Pet != null)
			{
				if (aIActor_Pet.SanctuaryPet.HasMood(Character_Mood.angry))
				{
					array = _SadIdleAnims;
					break;
				}
				if (aIActor_Pet.SanctuaryPet.HasMood(Character_Mood.firedup))
				{
					array = _FiredUpIdleAnims;
					break;
				}
			}
			array = _StandIdleAnims;
			break;
		}
		case 1:
			array = _SitIdleAnims;
			break;
		case 2:
			array = _SleepIdleAnims;
			break;
		}
		int num2 = Random.Range(0, array.Length);
		while (Actor.animation[array[num2]] == null && num < array.Length)
		{
			num2 = (num2 + 1) % array.Length;
			num++;
		}
		PlayAnim(Actor, array[num2], Queued: true);
	}

	public void PlayAnim(AIActor Actor, string AnimName, bool Queued)
	{
		if (string.IsNullOrEmpty(AnimName))
		{
			return;
		}
		if (Actor.animation[AnimName] == null)
		{
			UtDebug.LogWarning("Could not find : " + AnimName + " for " + Actor.name);
			return;
		}
		if (mCurrentAnim == null || AnimName != mCurrentAnim.name)
		{
			if (Queued)
			{
				Actor.animation.CrossFade(AnimName, 0.5f);
			}
			else
			{
				Actor.animation.CrossFade(AnimName, 0.5f, PlayMode.StopAll);
			}
			mCurrentAnim = Actor.animation[AnimName];
			if (mCurrentAnim != null)
			{
				mCurrentAnim.time = 0f;
				mCurrentAnim.wrapMode = WrapMode.Loop;
			}
		}
		if (mCurrentAnim != null)
		{
			mAnimRemainingTime = mCurrentAnim.length - 0.2f;
			mNumAnimsRemaining--;
		}
	}
}
