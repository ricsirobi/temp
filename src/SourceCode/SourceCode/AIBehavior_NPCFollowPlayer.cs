using System.Collections.Generic;
using UnityEngine;

public class AIBehavior_NPCFollowPlayer : AIBehavior_Mission
{
	private AvAvatarController mAvController;

	private AIActor_NPCCarried mAIActorNPCCarried;

	private Transform mParent;

	public override void OnStart(AIActor Actor)
	{
		if (mMissionData != null && !string.IsNullOrEmpty(mMissionData.SplineObjectName))
		{
			mMissionData.SplineObjectName = string.Empty;
		}
		mAIActorNPCCarried = Actor as AIActor_NPCCarried;
		if (mAIActorNPCCarried != null)
		{
			CarryNPC(Actor);
		}
		else
		{
			base.OnStart(Actor);
		}
	}

	private void CarryNPC(AIActor Actor)
	{
		mActor = Actor;
		mStartPoint = mActor.transform.position;
		mStartRotation = mActor.transform.rotation;
		mParent = Actor.transform.parent;
		SetState(AIBehaviorState.ACTIVE);
		MissionManager.AddMissionEventHandler(OnMissionEvent);
		if (SanctuaryManager.pCurPetInstance.pIsMounted)
		{
			SanctuaryManager.pCurPetInstance.OnFlyDismount(AvAvatar.pObject);
		}
		if (AvAvatar.pObject != null)
		{
			mAvController = AvAvatar.pObject.GetComponent<AvAvatarController>();
			mAvController.CarryObject(Actor.gameObject, mAIActorNPCCarried._CarryOffset, duplicateItem: false);
		}
		Actor.PlayAnimation(mAIActorNPCCarried._CarryAnimation);
		Actor.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
		if (SanctuaryManager.pCurPetInstance != null && AvAvatar.pSubState != AvAvatarSubState.FLYING)
		{
			KAInput.pInstance.EnableInputType("DragonMount", InputType.ALL, inEnable: false);
		}
	}

	protected override bool CanMove(AIActor Actor)
	{
		if (IsCloseBy(mTarget, Actor.transform, Actor._StoppingDistance))
		{
			return false;
		}
		if (base.CanMove(Actor))
		{
			return GetMoveData(Actor) != null;
		}
		return false;
	}

	public override void SetMoveStateSpeed(AIActor actor)
	{
		actor.pMoveState = GetMoveData(actor);
		if (actor.pMoveState != null)
		{
			actor._Speed = actor.pMoveState._Speed;
		}
		else
		{
			actor._Speed = 0f;
		}
	}

	private MoveStateData GetMoveData(AIActor actor)
	{
		Vector3 position = actor.Position;
		position.y += _GroundCheckStartHeight;
		float groundHeight;
		Vector3 normal;
		Collider groundHeight2 = UtUtilities.GetGroundHeight(position, _GroundCheckDist, out groundHeight, out normal);
		if (actor.GetMoveState(MoveStates.Air) != null && (AvAvatar.IsFlying() || actor.CanOnlyFly() || groundHeight2 == null || GetPathStatus(actor) != 0))
		{
			return actor.GetMoveState(MoveStates.Air);
		}
		if (actor.GetMoveState(MoveStates.Water) != null && groundHeight2 != null && groundHeight2.gameObject.layer == LayerMask.NameToLayer("Water"))
		{
			return actor.GetMoveState(MoveStates.Water);
		}
		if (actor.GetMoveState(MoveStates.Ground) != null && groundHeight2 != null && groundHeight2.gameObject.layer != LayerMask.NameToLayer("Water"))
		{
			return actor.GetMoveState(MoveStates.Ground);
		}
		return null;
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

	public override AIBehaviorState Think(AIActor Actor)
	{
		if (mAIActorNPCCarried != null)
		{
			if (AvAvatar.pSubState == AvAvatarSubState.SWIMMING)
			{
				mTask.Failed = true;
			}
			return State;
		}
		return base.Think(Actor);
	}

	public override void OnMissionEvent(MissionEvent inEvent, object inObject)
	{
		if ((inEvent != MissionEvent.TASK_FAIL && inEvent != MissionEvent.TASK_COMPLETE) || mTask != (Task)inObject)
		{
			return;
		}
		if (mAIActorNPCCarried != null)
		{
			if (SanctuaryManager.pCurPetInstance != null)
			{
				KAInput.pInstance.EnableInputType("DragonMount", InputType.ALL, inEnable: true);
			}
			mAvController.RemoveCarriedObject();
			mActor.transform.SetParent(mParent);
		}
		EndBehavior();
		if (inEvent != MissionEvent.TASK_FAIL || !mActor.pHasController)
		{
			return;
		}
		List<TaskSetup> setups = mTask.GetSetups();
		if (mTask.pData == null || setups == null)
		{
			return;
		}
		AIActor rootController = GetRootController(mActor);
		if (rootController == null)
		{
			return;
		}
		foreach (TaskSetup item in setups)
		{
			if (!string.IsNullOrEmpty(item.Asset) && item.Asset.Contains(rootController.name) && !string.IsNullOrEmpty(item.Location))
			{
				TeleportTo(mStartPoint, mStartRotation);
				break;
			}
		}
	}

	private AIActor GetRootController(AIActor actor)
	{
		if (!(actor.pController != null))
		{
			return actor;
		}
		return GetRootController(actor.pController);
	}

	public override void OnTerminate(AIActor Actor)
	{
		PlayIdle();
		base.OnTerminate(Actor);
	}

	public override void PlayIdle()
	{
		if (mActor.pMoveState != null)
		{
			PlayAnim(mActor.pMoveState._IdleAnimationName);
		}
		else
		{
			base.PlayIdle();
		}
	}
}
