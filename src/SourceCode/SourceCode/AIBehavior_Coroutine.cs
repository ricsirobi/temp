using System.Collections;

public class AIBehavior_Coroutine : AIBehavior
{
	private CoroutineWrapper mWrapper;

	public override AIBehaviorState Think(AIActor Actor)
	{
		return State;
	}

	public override void OnStart(AIActor Actor)
	{
		mWrapper = CoroutineWrapper.StartWrapper(Actor, CoroutineFunction(Actor, this));
	}

	public override void OnTerminate(AIActor Actor)
	{
		mWrapper.Stop();
		mWrapper = null;
		SetState(AIBehaviorState.INACTIVE);
	}

	public virtual IEnumerator CoroutineFunction(AIActor Actor, AIBehavior Behavior)
	{
		Behavior.SetState(AIBehaviorState.ACTIVE);
		yield return null;
		Behavior.SetState(AIBehaviorState.COMPLETED);
	}
}
