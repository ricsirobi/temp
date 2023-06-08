using UnityEngine;

public class AIBehavior_MoveTo : AIBehavior
{
	public AITarget _Target = new AITarget();

	public float _Speed = 3f;

	public float _SpeedToRotateTowardTarget = 360f;

	public float _ClosestDistToReach = 0.05f;

	public float _ClosestDistToReachY = 1f;

	protected float mMaxStuckTime = 1f;

	protected float mStuckTime;

	public static bool HasArrived(AIActor Actor, Vector3 Target, float Dist2D, float DistY)
	{
		Vector3 vector = Actor.Position - Target;
		Vector3 vector2 = vector;
		vector2.y = 0f;
		if (vector2.magnitude < Dist2D)
		{
			return Mathf.Abs(vector.y) < DistY;
		}
		return false;
	}

	public virtual float GetMoveSpeed(AIActor Actor)
	{
		return _Speed;
	}

	public override AIBehaviorState Think(AIActor Actor)
	{
		Vector3 targetLocation = GetTargetLocation(Actor);
		UpdateLocation(Actor);
		UpdateRotation(Actor, targetLocation);
		if (HasArrived(Actor, GetTargetLocation(Actor), _ClosestDistToReach, _ClosestDistToReachY))
		{
			return SetState(AIBehaviorState.COMPLETED);
		}
		if (mStuckTime > mMaxStuckTime)
		{
			return SetState(AIBehaviorState.FAILED);
		}
		return SetState(AIBehaviorState.ACTIVE);
	}

	private void UpdateLocation(AIActor Actor)
	{
		Vector3 targetLocation = GetTargetLocation(Actor);
		float num = GetMoveSpeed(Actor) * Actor.DeltaTime;
		Vector3 vector = MoveToward_Clamp2D(Actor.Position, targetLocation, num);
		Vector3 position = Actor.Position;
		Vector3 vector2 = vector - position;
		vector2.y = 0f;
		if (vector2.magnitude < num * 0.2f)
		{
			mStuckTime += Actor.DeltaTime;
		}
		vector.y = position.y;
		float y = vector.y;
		Actor.transform.position = vector;
		Actor.FallToGround();
		float num2 = (vector.y = Actor.transform.position.y);
		float num3 = 4f;
		if (num2 < y)
		{
			vector.y = Mathf.MoveTowards(y, num2, Actor.DeltaTime * num3);
		}
		Actor.transform.position = vector;
	}

	private Vector3 MoveToward_Clamp2D(Vector3 v1, Vector3 v2, float MaxDistance2D)
	{
		Vector3 vector = v1;
		vector.y = 0f;
		Vector3 vector2 = v2;
		vector2.y = 0f;
		float magnitude = (vector2 - vector).magnitude;
		float t = Mathf.Clamp01(MaxDistance2D / magnitude);
		return Vector3.Lerp(v1, v2, t);
	}

	private void UpdateRotation(AIActor Actor, Vector3 vTarget)
	{
		if (!(_SpeedToRotateTowardTarget <= 0f))
		{
			Quaternion targetRotation = GetTargetRotation(Actor);
			Actor.transform.rotation = Quaternion.RotateTowards(Actor.transform.rotation, targetRotation, _SpeedToRotateTowardTarget * Actor.DeltaTime);
		}
	}

	public override void OnStart(AIActor Actor)
	{
		_Target.Actor = Actor;
		mStuckTime = 0f;
		SetState(AIBehaviorState.ACTIVE);
	}

	public override void OnTerminate(AIActor Actor)
	{
		SetState(AIBehaviorState.INACTIVE);
	}

	public virtual Vector3 GetTargetLocation(AIActor Actor)
	{
		_Target.Actor = Actor;
		return _Target.GetLocation();
	}

	public virtual Quaternion GetTargetRotation(AIActor Actor)
	{
		Vector3 forward = GetTargetLocation(Actor) - Actor.Position;
		if (forward.x == 0f && forward.z == 0f)
		{
			forward = Actor.transform.forward;
		}
		forward.y = 0f;
		return Quaternion.LookRotation(forward);
	}
}
