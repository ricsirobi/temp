public class AIBehavior_CustomScriptedFunctions : AIBehavior
{
	public AICustomScriptFunctions StartFunction;

	public AICustomScriptFunctions ThinkFunction;

	public AICustomScriptFunctions TerminateFunction;

	public override AIBehaviorState Think(AIActor Actor)
	{
		DoFunction(TerminateFunction);
		return State;
	}

	public override void OnStart(AIActor Actor)
	{
		base.OnStart(Actor);
		DoFunction(StartFunction);
		if (ThinkFunction == AICustomScriptFunctions.NONE)
		{
			SetState(AIBehaviorState.COMPLETED);
		}
		else
		{
			SetState(AIBehaviorState.ACTIVE);
		}
	}

	public override void OnTerminate(AIActor Actor)
	{
		DoFunction(TerminateFunction);
		base.OnTerminate(Actor);
	}

	private void DoFunction(AICustomScriptFunctions Func)
	{
	}
}
