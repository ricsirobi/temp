using UnityEngine;

public class AIBehavior_EscortPlayerMission : AIBehavior_Mission
{
	public override void OnStart(AIActor Actor)
	{
		base.OnStart(Actor);
		if (Actor.pHasController && Actor.HasSplineMovement())
		{
			SetMoveState(MoveState.SWS);
			Actor.RunSplineMovement(run: true);
		}
	}

	public override void ProcessMove(AIActor Actor)
	{
		if (mInProximity && HasArrived(Actor) && IsCloseBy(Actor.transform, GetTargetForProximity(), Actor._StoppingDistance))
		{
			EndBehavior();
		}
		else
		{
			base.ProcessMove(Actor);
		}
	}

	public override bool IsMoving(AIActor Actor)
	{
		if (HasArrived(Actor))
		{
			return false;
		}
		return base.IsMoving(Actor);
	}

	protected override Transform GetTargetForProximity()
	{
		return AvAvatar.mTransform;
	}

	protected override void ProcessLookAt(AIActor Actor)
	{
	}

	protected override bool CanMove(AIActor Actor)
	{
		return true;
	}

	protected bool HasArrived(AIActor Actor)
	{
		if (mSpline != null && Actor._Character != null && Actor._Character.mSpline != null && Actor._Character.mEndReached)
		{
			return true;
		}
		if (mTarget != null)
		{
			return Vector3.Distance(mTarget.transform.position, Actor.transform.position) < 0.5f;
		}
		return false;
	}

	protected override Transform GetTarget()
	{
		string data = GetData<string>("Name");
		if (!string.IsNullOrEmpty(data))
		{
			GameObject gameObject = GameObject.Find(data);
			if (!(gameObject != null))
			{
				return null;
			}
			return gameObject.transform;
		}
		return null;
	}

	public override void OnMissionEvent(MissionEvent inEvent, object inObject)
	{
		if (inEvent == MissionEvent.TASK_COMPLETE && mTask == (Task)inObject && mSpline != null && mActor._Character != null)
		{
			mActor._Character.SetPosOnSpline(mSpline.mLinearLength);
		}
		base.OnMissionEvent(inEvent, inObject);
	}

	public override void PlayAnimation(AIActor Actor)
	{
		if (Actor.pMoveState != null)
		{
			PlayAnim(IsMoving(Actor) ? Actor.pMoveState._ActionAnimationName : Actor.pMoveState._IdleAnimationName);
		}
		else
		{
			base.PlayAnimation(Actor);
		}
	}
}
