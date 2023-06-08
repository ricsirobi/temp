public class AIEvaluator_ValidBehavior : AIEvaluator
{
	public float _PriorityFactorWhenValid = 1f;

	public float _PriorityFactorWhenInvalid;

	private AIBehavior mBehavior;

	public override float OnGetDesirability(AIActor Actor, bool IsActive, AIBehavior Behavior)
	{
		if (Behavior == null)
		{
			if (mBehavior != null)
			{
				Behavior = mBehavior;
			}
			else
			{
				mBehavior = GetComponent<AIBehavior>();
				Behavior = mBehavior;
			}
		}
		if (Behavior != null && Behavior.CanBeExecuted(Actor))
		{
			return _PriorityFactorWhenValid * _Priority;
		}
		return _PriorityFactorWhenInvalid * _Priority;
	}
}
