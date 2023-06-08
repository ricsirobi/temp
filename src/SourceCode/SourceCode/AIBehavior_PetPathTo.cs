using UnityEngine;

public class AIBehavior_PetPathTo : AIBehavior_PathTo
{
	public bool _UseHeadBone;

	public float _FlyingSpeed = 7f;

	public string _FlyForwardAnim = "FlyForward";

	public string _FlyIdleAnim = "FlyIdle";

	public float _PetDistance = 1f;

	public Vector3 _PetOffsetDistance = Vector3.one;

	private bool mTargetFlying;

	public override void OnStart(AIActor Actor)
	{
		AIActor_Pet aIActor_Pet = Actor as AIActor_Pet;
		if (aIActor_Pet != null && aIActor_Pet.SanctuaryPet != null)
		{
			aIActor_Pet.SanctuaryPet.MoveToHeight(0f);
			aIActor_Pet.SanctuaryPet.FallToGround();
			if (_Target.TargetType == TargetTypes.AVATAR && aIActor_Pet.SanctuaryPet.pAvatarController != null && aIActor_Pet.SanctuaryPet.pAvatarController.IsFlyingOrGliding())
			{
				mTargetFlying = true;
				_Target.Actor = Actor;
				return;
			}
			mTargetFlying = false;
		}
		base.OnStart(Actor);
		if (_UseHeadBone && aIActor_Pet != null && aIActor_Pet.SanctuaryPet != null)
		{
			mBone = UtUtilities.FindChildTransform(Actor.gameObject, aIActor_Pet.SanctuaryPet.GetHeadBoneName());
		}
	}

	public override AIBehaviorState Think(AIActor Actor)
	{
		Vector3 location = _Target.GetLocation();
		AIActor_Pet aIActor_Pet = Actor as AIActor_Pet;
		if (mTargetFlying && _Target.TargetType == TargetTypes.AVATAR && aIActor_Pet != null && aIActor_Pet.SanctuaryPet != null && aIActor_Pet.SanctuaryPet.pAvatarController.IsFlyingOrGliding())
		{
			Vector3 vector = aIActor_Pet.SanctuaryPet.mAvatar.right * _PetOffsetDistance.x + aIActor_Pet.SanctuaryPet.mAvatar.up * _PetOffsetDistance.y + aIActor_Pet.SanctuaryPet.mAvatar.forward * _PetOffsetDistance.z;
			float num = Vector3.Distance(Actor.transform.position, location + vector);
			if (!ApproximatelyEqual(num, 0f))
			{
				Vector3 position = Vector3.MoveTowards(Actor.transform.position, location + vector, _FlyingSpeed * Actor.DeltaTime * num / _PetDistance);
				Actor.transform.position = position;
				Vector3 forward = location + vector - Actor.transform.position;
				if (!ApproximatelyEqual(forward.x, 0f) || !ApproximatelyEqual(forward.z, 0f))
				{
					Actor.transform.rotation = Quaternion.RotateTowards(Actor.transform.rotation, Quaternion.LookRotation(forward), 860f * Actor.DeltaTime);
				}
				if (!Actor.animation.IsPlaying(_FlyForwardAnim) && Actor.animation[_FlyForwardAnim] != null)
				{
					Actor.animation.CrossFade(_FlyForwardAnim, 0.2f, PlayMode.StopAll);
					Actor.animation[_FlyForwardAnim].wrapMode = WrapMode.Loop;
				}
			}
			else if (!Actor.animation.IsPlaying(_FlyIdleAnim) && Actor.animation[_FlyIdleAnim] != null)
			{
				Actor.animation.CrossFade(_FlyIdleAnim, 0.2f, PlayMode.StopAll);
				Actor.animation[_FlyIdleAnim].wrapMode = WrapMode.Loop;
			}
			return SetState(AIBehaviorState.ACTIVE);
		}
		if (mTargetFlying)
		{
			mTargetFlying = false;
			if (Actor == null)
			{
				UtDebug.LogError("Actor is null");
			}
			if (Actor != null && Actor.animation[_IdleStandAnim] != null)
			{
				Actor.animation.CrossFade(_IdleStandAnim, 0.2f, PlayMode.StopAll);
				Actor.animation[_IdleStandAnim].wrapMode = WrapMode.Loop;
			}
			if (aIActor_Pet == null)
			{
				UtDebug.LogError("Pet Actor is null");
			}
			if (aIActor_Pet != null && aIActor_Pet.SanctuaryPet != null)
			{
				aIActor_Pet.SanctuaryPet.SetAvatar(aIActor_Pet.SanctuaryPet.mAvatar, SpawnTeleportEffect: false);
			}
			return SetState(AIBehaviorState.INACTIVE);
		}
		return base.Think(Actor);
	}

	private bool ApproximatelyEqual(float a, float b)
	{
		return !(Mathf.Abs(a - b) > 0.0001f);
	}

	public override void OnTerminate(AIActor Actor)
	{
		if (mTargetFlying)
		{
			SetState(AIBehaviorState.INACTIVE);
			return;
		}
		base.OnTerminate(Actor);
		AIActor_Pet aIActor_Pet = Actor as AIActor_Pet;
		if (aIActor_Pet != null && aIActor_Pet.SanctuaryPet != null)
		{
			aIActor_Pet.SanctuaryPet.MoveToHeight(0f);
			aIActor_Pet.SanctuaryPet.FallToGround();
		}
	}

	public override float GetSpeed(AIActor Actor)
	{
		AIActor_Pet aIActor_Pet = Actor as AIActor_Pet;
		SanctuaryPet sanctuaryPet = ((aIActor_Pet != null) ? aIActor_Pet.SanctuaryPet : null);
		float walkWhenCloserThan = _WalkWhenCloserThan;
		float runWhenFartherThan = _RunWhenFartherThan;
		if (sanctuaryPet != null)
		{
			if (sanctuaryPet.pCurAgeData != null)
			{
				_WalkSpeed = sanctuaryPet.pCurAgeData._WalkSpeed;
				_RunSpeed = sanctuaryPet.pCurAgeData._RunSpeed;
			}
			if (_WalkWithAvatarSpeed)
			{
				_WalkWhenCloserThan = sanctuaryPet._FollowFrontDistance + _WalkWhenCloserThan;
				_RunWhenFartherThan = sanctuaryPet._FollowFrontDistance + _RunWhenFartherThan;
			}
		}
		float speed = base.GetSpeed(Actor);
		if (_WalkWithAvatarSpeed)
		{
			_WalkWhenCloserThan = walkWhenCloserThan;
			_RunWhenFartherThan = runWhenFartherThan;
		}
		return speed;
	}

	public override void SetDestination(Vector3 destination)
	{
		base.SetDestination(destination);
		AIActor_Pet aIActor_Pet = _Target.Actor as AIActor_Pet;
		if (!mAgent.isOnOffMeshLink && aIActor_Pet != null && aIActor_Pet.SanctuaryPet != null)
		{
			aIActor_Pet.SanctuaryPet.FallToGround();
			aIActor_Pet.SanctuaryPet.MoveToHeight(destination.y);
		}
	}
}
