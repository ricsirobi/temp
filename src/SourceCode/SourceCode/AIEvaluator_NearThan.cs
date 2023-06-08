using UnityEngine;

public class AIEvaluator_NearThan : AIEvaluator
{
	public float _EnableWhen_NearThan = 3f;

	public float _DisableWhen_FartherThan = 5f;

	public bool _Only2D;

	public string _BoneName;

	public bool _UseHeadBone;

	public AITarget _Target = new AITarget();

	private Transform mBone;

	public override float OnGetDesirability(AIActor Actor, bool IsActive, AIBehavior Behavior)
	{
		_Target.Actor = Actor;
		Vector3 location = _Target.GetLocation();
		float num = (_Only2D ? AIEvaluator.Dist2D(location, Actor.Position) : Vector3.Distance(location, Actor.Position));
		num -= GetExtraDistance();
		bool flag = true;
		if (!(IsActive ? (num <= _DisableWhen_FartherThan) : (num <= _EnableWhen_NearThan)))
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
