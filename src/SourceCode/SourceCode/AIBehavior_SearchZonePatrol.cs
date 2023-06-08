using DG.Tweening;
using SWS;

public class AIBehavior_SearchZonePatrol : AIBehavior
{
	private splineMove mSplineMove;

	private AIActor mActor;

	private AIActor_NPC.SearchZoneData.PatrolInfo mPatrolInfo;

	public override void OnStart(AIActor actor)
	{
		base.OnStart(actor);
		mActor = actor;
		mPatrolInfo = ((AIActor_NPC)actor)._SearchZoneData._PatrolInfo;
		mSplineMove = actor.GetComponent<splineMove>();
		if (mSplineMove == null && (bool)mPatrolInfo._Path)
		{
			mSplineMove = actor.gameObject.AddComponent<splineMove>();
			mSplineMove.loopType = splineMove.LoopType.pingPong;
			mSplineMove.lockRotation = AxisConstraint.X;
		}
		mSplineMove?.ChangeSpeed(actor._Character.Speed);
		if ((bool)mPatrolInfo._Path)
		{
			mSplineMove?.SetPath(mPatrolInfo._Path);
		}
		else
		{
			mSplineMove?.StartMove();
		}
		actor.PlayAnimation(mPatrolInfo._PatrolAnimation);
	}

	public void Patrol(bool run)
	{
		if (!(mSplineMove == null))
		{
			string empty = string.Empty;
			if (run)
			{
				mSplineMove.Resume();
				empty = mPatrolInfo._PatrolAnimation;
			}
			else
			{
				mSplineMove.Pause();
				empty = mPatrolInfo._IdleAnimation;
			}
			mActor.PlayAnimation(empty);
		}
	}

	public override AIBehaviorState Think(AIActor actor)
	{
		return AIBehaviorState.ACTIVE;
	}

	public override void OnTerminate(AIActor actor)
	{
		base.OnTerminate(actor);
		if (mSplineMove != null)
		{
			mSplineMove.Stop();
		}
	}
}
