using UnityEngine;

public class AIBehavior_MoveBackwardFrom : AIBehavior_MoveTo
{
	public float _HowFarToMove = 1f;

	public string _BoneName;

	public bool _UseHeadBone;

	public string _WalkAnim = "Walk";

	private Transform mBone;

	public override void OnStart(AIActor Actor)
	{
		base.OnStart(Actor);
		AIActor_Pet aIActor_Pet = Actor as AIActor_Pet;
		if (_UseHeadBone && aIActor_Pet != null && aIActor_Pet.SanctuaryPet != null)
		{
			mBone = UtUtilities.FindChildTransform(Actor.gameObject, aIActor_Pet.SanctuaryPet.GetHeadBoneName());
		}
		else if (!string.IsNullOrEmpty(_BoneName))
		{
			mBone = UtUtilities.FindChildTransform(Actor.gameObject, _BoneName);
		}
	}

	public override AIBehaviorState Think(AIActor Actor)
	{
		if (Actor != null && Actor.animation != null && !Actor.animation.IsPlaying(_WalkAnim))
		{
			Actor.animation.CrossFade(_WalkAnim, 0.3f);
			AnimationState animationState = Actor.animation[_WalkAnim];
			animationState.wrapMode = WrapMode.Loop;
			animationState.speed = -1f;
		}
		return base.Think(Actor);
	}

	public override float GetMoveSpeed(AIActor Actor)
	{
		AIActor_Pet aIActor_Pet = Actor as AIActor_Pet;
		if (aIActor_Pet != null && aIActor_Pet.SanctuaryPet != null && aIActor_Pet.SanctuaryPet.pCurAgeData != null)
		{
			return aIActor_Pet.SanctuaryPet.pCurAgeData._WalkSpeed;
		}
		return _Speed;
	}

	public override Vector3 GetTargetLocation(AIActor Actor)
	{
		Vector3 targetLocation = base.GetTargetLocation(Actor);
		Vector3 vector = Actor.Position - targetLocation;
		vector.y = 0f;
		float num = _HowFarToMove;
		if (mBone != null)
		{
			num += Vector3.Distance(Actor.Position, mBone.position);
		}
		return targetLocation + vector.normalized * num;
	}

	public override Quaternion GetTargetRotation(AIActor Actor)
	{
		Vector3 forward = base.GetTargetLocation(Actor) - Actor.Position;
		if (forward.x == 0f && forward.z == 0f)
		{
			forward = Actor.transform.forward;
		}
		forward.y = 0f;
		return Quaternion.LookRotation(forward);
	}
}
