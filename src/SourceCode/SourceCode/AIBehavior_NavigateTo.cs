using UnityEngine;

public class AIBehavior_NavigateTo : AIBehavior
{
	private enum NavigateToBehaviorState
	{
		WAITING,
		MOVING_BACKWARDS,
		FOLLOWING_PATH,
		FLYING_TO,
		TELEPORTING
	}

	public AITarget _Target = new AITarget();

	public string _HeadBoneName;

	public float _ArriveDistance = 0.05f;

	public float _TeleportIfFartherThan = 30f;

	private AIBehavior mBehavior;

	private NavigateToBehaviorState mNavigationState;

	private Transform mHeadBone;

	public override AIBehaviorState Think(AIActor Actor)
	{
		if (mBehavior == null)
		{
			return State;
		}
		if (mBehavior.State != AIBehaviorState.ACTIVE)
		{
			SetState(mBehavior.State);
			mBehavior = null;
			return State;
		}
		switch (mNavigationState)
		{
		case NavigateToBehaviorState.MOVING_BACKWARDS:
			if (Vector3.Distance(_Target.GetLocation(), Actor.Position) > _TeleportIfFartherThan)
			{
				StartFlyTo(Actor);
			}
			else
			{
				mBehavior.Think(Actor);
			}
			break;
		case NavigateToBehaviorState.FOLLOWING_PATH:
			if (mBehavior.Think(Actor) == AIBehaviorState.FAILED)
			{
				StartFlyTo(Actor);
			}
			break;
		case NavigateToBehaviorState.FLYING_TO:
			if (mBehavior.Think(Actor) == AIBehaviorState.FAILED)
			{
				StartTeleporting(Actor);
			}
			break;
		case NavigateToBehaviorState.TELEPORTING:
			mBehavior.Think(Actor);
			break;
		default:
			SetState(AIBehaviorState.COMPLETED);
			break;
		}
		return State;
	}

	public void StartFlyTo(AIActor Actor)
	{
		_ForceExecution = true;
		AIBehavior_TeleportByFlying aIBehavior_TeleportByFlying = base.gameObject.GetComponent<AIBehavior_TeleportByFlying>();
		if (aIBehavior_TeleportByFlying == null)
		{
			aIBehavior_TeleportByFlying = base.gameObject.AddComponent<AIBehavior_TeleportByFlying>();
		}
		aIBehavior_TeleportByFlying._Target = _Target;
		mBehavior.OnTerminate(Actor);
		mBehavior = aIBehavior_TeleportByFlying;
		mNavigationState = NavigateToBehaviorState.FLYING_TO;
		mBehavior.OnStart(Actor);
		if (mBehavior.State == AIBehaviorState.FAILED)
		{
			StartTeleporting(Actor);
		}
	}

	public void StartTeleporting(AIActor Actor)
	{
		_ForceExecution = false;
		AIBehavior_TeleportTo aIBehavior_TeleportTo = base.gameObject.GetComponent<AIBehavior_TeleportTo>();
		if (aIBehavior_TeleportTo == null)
		{
			aIBehavior_TeleportTo = base.gameObject.AddComponent<AIBehavior_TeleportTo>();
		}
		aIBehavior_TeleportTo._Target = _Target;
		mBehavior.OnTerminate(Actor);
		mBehavior = aIBehavior_TeleportTo;
		mNavigationState = NavigateToBehaviorState.TELEPORTING;
		mBehavior.OnStart(Actor);
	}

	public override void OnStart(AIActor Actor)
	{
		base.OnStart(Actor);
		_Target.Actor = Actor;
		_ForceExecution = false;
		if (!string.IsNullOrEmpty(_HeadBoneName))
		{
			mHeadBone = UtUtilities.FindChildTransform(Actor.gameObject, _HeadBoneName);
		}
		float num = AIEvaluator.Dist2D(_Target.GetLocation(), Actor.Position);
		float extraDistance = GetExtraDistance(Actor);
		if (num < extraDistance + _ArriveDistance)
		{
			AIBehavior_MoveBackwardFrom aIBehavior_MoveBackwardFrom = base.gameObject.GetComponent<AIBehavior_MoveBackwardFrom>();
			if (aIBehavior_MoveBackwardFrom == null)
			{
				aIBehavior_MoveBackwardFrom = base.gameObject.AddComponent<AIBehavior_MoveBackwardFrom>();
			}
			aIBehavior_MoveBackwardFrom._Target = _Target;
			aIBehavior_MoveBackwardFrom._ClosestDistToReach = _ArriveDistance;
			aIBehavior_MoveBackwardFrom._BoneName = _HeadBoneName;
			mBehavior = aIBehavior_MoveBackwardFrom;
			mNavigationState = NavigateToBehaviorState.MOVING_BACKWARDS;
			mBehavior.OnStart(Actor);
		}
		else
		{
			AIBehavior_PathTo aIBehavior_PathTo = base.gameObject.GetComponent<AIBehavior_PathTo>();
			if (aIBehavior_PathTo == null)
			{
				aIBehavior_PathTo = base.gameObject.AddComponent<AIBehavior_PathTo>();
			}
			aIBehavior_PathTo._Target = _Target;
			aIBehavior_PathTo._DisablePathFindingIfFartherThan = _TeleportIfFartherThan;
			aIBehavior_PathTo._BoneName = _HeadBoneName;
			aIBehavior_PathTo._ArriveWhenCloserThan = _ArriveDistance;
			mBehavior = aIBehavior_PathTo;
			mNavigationState = NavigateToBehaviorState.FOLLOWING_PATH;
			mBehavior.OnStart(Actor);
		}
		SetState(AIBehaviorState.ACTIVE);
	}

	public override void OnTerminate(AIActor Actor)
	{
		base.OnTerminate(Actor);
		if (mBehavior != null)
		{
			mBehavior.OnTerminate(Actor);
		}
		_ForceExecution = false;
	}

	public float GetExtraDistance(AIActor Actor)
	{
		if (string.IsNullOrEmpty(_HeadBoneName))
		{
			return 0f;
		}
		return Vector3.Distance(Actor.Position, mHeadBone.position);
	}
}
