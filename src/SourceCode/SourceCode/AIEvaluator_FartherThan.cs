using UnityEngine;

public class AIEvaluator_FartherThan : AIEvaluator
{
	public float _EnableWhen_FartherThan = 5f;

	public float _DisableWhen_NearThan = 3f;

	public bool _Only2D;

	public string _BoneName;

	public bool _UseHeadBone;

	public AITarget _Target = new AITarget();

	private Transform mBone;

	public override float OnGetDesirability(AIActor Actor, bool IsActive, AIBehavior Behavior)
	{
		_Target.Actor = Actor;
		if (_Target.Actor.GetAvatar() != null)
		{
			AvAvatarController component = _Target.Actor.GetAvatar().GetComponent<AvAvatarController>();
			if (component != null && component.pIsPlayerGliding)
			{
				return _Priority;
			}
		}
		Vector3 location = _Target.GetLocation();
		float num = (_Only2D ? AIEvaluator.Dist2D(location, Actor.Position) : Vector3.Distance(location, Actor.Position));
		num -= GetExtraDistance();
		bool flag = true;
		if (!(IsActive ? (num >= _DisableWhen_NearThan) : (num >= _EnableWhen_FartherThan)))
		{
			return 0f;
		}
		return _Priority;
	}

	public float GetExtraDistance()
	{
		if (_Target.Actor == null)
		{
			return 0f;
		}
		if (mBone == null)
		{
			if (_UseHeadBone)
			{
				AIActor_Pet aIActor_Pet = _Target.Actor as AIActor_Pet;
				if (aIActor_Pet != null && aIActor_Pet.SanctuaryPet != null)
				{
					mBone = UtUtilities.FindChildTransform(_Target.Actor.gameObject, aIActor_Pet.SanctuaryPet.GetHeadBoneName());
				}
			}
			else if (!string.IsNullOrEmpty(_BoneName))
			{
				mBone = UtUtilities.FindChildTransform(_Target.Actor.gameObject, _BoneName);
			}
		}
		if (mBone != null)
		{
			return Vector3.Distance(_Target.Actor.Position, mBone.position);
		}
		return 0f;
	}
}
