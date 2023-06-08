using UnityEngine;

public class AIEvaluator : MonoBehaviour
{
	public float _Priority = 1f;

	private AIEvaluator[] mChildEvaluators;

	public void CheckForChildEvaluators()
	{
		mChildEvaluators = GetComponents<AIEvaluator>();
	}

	public float GetDesirability(AIActor Actor, bool IsActive, AIBehavior Behavior)
	{
		if (mChildEvaluators == null)
		{
			return _Priority;
		}
		float num = 0f;
		for (int i = 0; i < mChildEvaluators.Length; i++)
		{
			num = Mathf.Max(num, mChildEvaluators[i].OnGetDesirability(Actor, IsActive, Behavior));
		}
		return num;
	}

	public virtual float OnGetDesirability(AIActor Actor, bool IsActive, AIBehavior Behavior)
	{
		return _Priority;
	}

	public static float Dist2D(Transform t1, Transform t2)
	{
		if (t1 == null || t2 == null)
		{
			return 0f;
		}
		return Dist2D(t1.position, t2.position);
	}

	public static float Distance(Transform t1, Transform t2)
	{
		if (t1 == null || t2 == null)
		{
			return 0f;
		}
		return Vector3.Distance(t1.position, t2.position);
	}

	public static float Dist2D(Vector3 v1, Vector3 v2)
	{
		Vector3 vector = v2 - v1;
		vector.y = 0f;
		return vector.magnitude;
	}
}
