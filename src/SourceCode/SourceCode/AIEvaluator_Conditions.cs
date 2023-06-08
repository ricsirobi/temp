using UnityEngine;

public class AIEvaluator_Conditions : AIEvaluator
{
	public AICondition[] _Conditions;

	public bool _ApplyWhenActive = true;

	public bool _ApplyWhenInactive = true;

	public override float OnGetDesirability(AIActor Actor, bool IsActive, AIBehavior Behavior)
	{
		if (IsActive && !_ApplyWhenActive)
		{
			return 0f;
		}
		if (!IsActive && !_ApplyWhenInactive)
		{
			return 0f;
		}
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < _Conditions.Length; i++)
		{
			num2 += (float)_Conditions[i].Priority;
			num += EvaluateCondition(i, Actor) * (float)_Conditions[i].Priority;
		}
		if (num2 == 0f && _Conditions.Length == 0)
		{
			return _Priority;
		}
		return _Priority * Mathf.Clamp01(num / num2);
	}

	public float EvaluateCondition(int Index, AIActor Actor)
	{
		Transform avatar = Actor.GetAvatar();
		switch (_Conditions[Index].Type)
		{
		case AIEvaluationType.FAR_FROM_OWNER:
			return (AIEvaluator.Distance(avatar, Actor.transform) >= _Conditions[Index].Value) ? 1 : 0;
		case AIEvaluationType.NEAR_OWNER:
			if (avatar == null)
			{
				return 0f;
			}
			return (AIEvaluator.Distance(avatar, Actor.transform) <= _Conditions[Index].Value) ? 1 : 0;
		case AIEvaluationType.PETPLAY_FAR_FROM_HOME:
			if (KAUIPetPlaySelect.Instance == null)
			{
				return 0f;
			}
			return (AIEvaluator.Distance(KAUIPetPlaySelect.Instance.GetHomeObject(), Actor.transform) >= _Conditions[Index].Value) ? 1 : 0;
		case AIEvaluationType.PET_ISNOT_ANGRY:
			return (!((AIActor_Pet)Actor).SanctuaryPet.HasMood(Character_Mood.angry)) ? 1 : 0;
		default:
			return 0f;
		}
	}
}
