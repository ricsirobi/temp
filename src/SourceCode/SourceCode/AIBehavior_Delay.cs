using UnityEngine;

public class AIBehavior_Delay : AIBehavior
{
	public float _MinTime = 1f;

	public float _MaxTime = 1f;

	private float mRemainingTime;

	public override void OnStart(AIActor Actor)
	{
		mRemainingTime = Random.Range(_MinTime, _MaxTime);
		SetState(AIBehaviorState.ACTIVE);
	}

	public override AIBehaviorState Think(AIActor Actor)
	{
		mRemainingTime -= Actor.DeltaTime;
		if (!(mRemainingTime > 0f))
		{
			return AIBehaviorState.COMPLETED;
		}
		return AIBehaviorState.ACTIVE;
	}
}
