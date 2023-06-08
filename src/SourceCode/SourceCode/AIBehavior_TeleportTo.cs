using UnityEngine;

public class AIBehavior_TeleportTo : AIBehavior
{
	public AITarget _Target = new AITarget();

	public override AIBehaviorState Think(AIActor Actor)
	{
		Quaternion Rot;
		Vector3 location = _Target.GetLocation(out Rot);
		Actor.TeleportTo(location, Rot);
		return SetState(AIBehaviorState.COMPLETED);
	}

	public override void OnStart(AIActor Actor)
	{
		_Target.Actor = Actor;
		SetState(AIBehaviorState.ACTIVE);
	}

	public override void OnTerminate(AIActor Actor)
	{
		SetState(AIBehaviorState.INACTIVE);
	}
}
