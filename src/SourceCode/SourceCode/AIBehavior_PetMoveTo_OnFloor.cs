using UnityEngine;

public class AIBehavior_PetMoveTo_OnFloor : AIBehavior_MoveTo
{
	public float _WalkWhenCloserThan = 7f;

	public float _RunWhenFartherThan = 10f;

	public string _WalkAnim = "Walk";

	public float _WalkSpeed = 3f;

	public string _RunAnim = "Run";

	public float _RunSpeed = 5f;

	public override AIBehaviorState Think(AIActor Actor)
	{
		float walkSpeed = _WalkSpeed;
		float runSpeed = _RunSpeed;
		AIActor_Pet aIActor_Pet = Actor as AIActor_Pet;
		if (aIActor_Pet != null && aIActor_Pet.SanctuaryPet != null && aIActor_Pet.SanctuaryPet.pCurAgeData != null)
		{
			walkSpeed = aIActor_Pet.SanctuaryPet.pCurAgeData._WalkSpeed;
			runSpeed = aIActor_Pet.SanctuaryPet.pCurAgeData._RunSpeed;
		}
		float num = Vector3.Distance(GetTargetLocation(Actor), Actor.Position);
		float value = Mathf.Max(0f, num - _WalkWhenCloserThan) / Mathf.Max(0f, _RunWhenFartherThan - _WalkWhenCloserThan);
		value = Mathf.Clamp01(value);
		bool flag = Actor.animation.IsPlaying(_RunAnim);
		bool flag2 = Actor.animation.IsPlaying(_WalkAnim);
		if ((flag && value < 0.4f) || (!flag2 && value < 0.6f))
		{
			Actor.animation.CrossFade(_WalkAnim, 0.3f);
			Actor.animation[_WalkAnim].wrapMode = WrapMode.Loop;
		}
		if ((flag2 && value > 0.6f) || (!flag && value > 0.4f))
		{
			Actor.animation.CrossFade(_RunAnim, 0.3f);
			Actor.animation[_RunAnim].wrapMode = WrapMode.Loop;
		}
		value = ((!Actor.animation.IsPlaying(_WalkAnim)) ? 1 : 0);
		_Speed = Mathf.Lerp(walkSpeed, runSpeed, value);
		return base.Think(Actor);
	}
}
