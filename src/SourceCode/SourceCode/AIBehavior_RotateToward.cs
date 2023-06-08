using UnityEngine;

public class AIBehavior_RotateToward : AIBehavior
{
	public AITarget _Target = new AITarget();

	public float _SpeedToRotateTowardTarget = 360f;

	public bool _ReturnCompleteAfterFinishing = true;

	public float _MinRotation = 0.5f;

	public float _MaxRotation = 1f;

	private bool mRotating;

	public override AIBehaviorState Think(AIActor Actor)
	{
		Quaternion targetRotation = GetTargetRotation(Actor);
		float num = (mRotating ? _MinRotation : _MaxRotation);
		if (Angle2D(Actor.transform.rotation, targetRotation) <= num)
		{
			mRotating = false;
			if (_ReturnCompleteAfterFinishing)
			{
				return SetState(AIBehaviorState.COMPLETED);
			}
			return SetState(AIBehaviorState.ACTIVE);
		}
		Actor.transform.rotation = Quaternion.RotateTowards(Actor.transform.rotation, targetRotation, _SpeedToRotateTowardTarget * Actor.DeltaTime);
		mRotating = true;
		if (_ReturnCompleteAfterFinishing && Angle2D(Actor.transform.rotation, targetRotation) <= _MinRotation)
		{
			mRotating = false;
			return SetState(AIBehaviorState.COMPLETED);
		}
		return SetState(AIBehaviorState.ACTIVE);
	}

	private float Angle2D(Quaternion Rot1, Quaternion Rot2)
	{
		Vector3 forward = Rot1 * Vector3.forward;
		forward.y = 0f;
		Rot1 = Quaternion.LookRotation(forward);
		Vector3 forward2 = Rot2 * Vector3.forward;
		forward2.y = 0f;
		Rot2 = Quaternion.LookRotation(forward2);
		return Quaternion.Angle(Rot1, Rot2);
	}

	public override void OnStart(AIActor Actor)
	{
		SetState(AIBehaviorState.ACTIVE);
	}

	public override void OnTerminate(AIActor Actor)
	{
		SetState(AIBehaviorState.INACTIVE);
	}

	public Quaternion GetTargetRotation(AIActor Actor)
	{
		_Target.Actor = Actor;
		Vector3 forward = _Target.GetLocation() - Actor.Position;
		forward.y = 0f;
		if (forward.x != 0f || forward.z != 0f)
		{
			return Quaternion.LookRotation(forward);
		}
		return Actor.transform.rotation;
	}
}
