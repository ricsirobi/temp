using System.Collections;
using UnityEngine;

public class AIBehavior_LookAt : AIBehavior
{
	public AITarget _Target = new AITarget();

	public float _LookAtSpeed = 4f;

	private ObLookAt[] mLookAtModifiers;

	public override AIBehaviorState Think(AIActor Actor)
	{
		if (mLookAtModifiers == null)
		{
			PopulateModifiers(Actor.gameObject, out mLookAtModifiers);
		}
		DoLookAt(_Target, mLookAtModifiers, _LookAtSpeed);
		return SetState(AIBehaviorState.ACTIVE);
	}

	public override void OnStart(AIActor Actor)
	{
		_Target.Actor = Actor;
		PopulateModifiers(Actor.gameObject, out mLookAtModifiers);
		SetState(AIBehaviorState.ACTIVE);
	}

	public override void OnTerminate(AIActor Actor)
	{
		base.OnTerminate(Actor);
		StopLookAt(mLookAtModifiers);
	}

	public static IEnumerator LookAt(GameObject gameObject, AITarget Target, float Speed)
	{
		ObLookAt[] LookAtModifiers = null;
		if (!PopulateModifiers(gameObject, out LookAtModifiers))
		{
			yield break;
		}
		while (true)
		{
			DoLookAt(Target, LookAtModifiers, Speed);
			yield return null;
		}
	}

	private static void DoLookAt(AITarget Target, ObLookAt[] LookAtModifiers, float Speed)
	{
		if (LookAtModifiers != null)
		{
			Vector3 location = Target.GetLocation();
			foreach (ObLookAt obj in LookAtModifiers)
			{
				obj._LookAtSpeed = Speed;
				obj.LookAt(location);
			}
		}
	}

	public static void StopLookAt(ObLookAt[] LookAtModifiers)
	{
		if (LookAtModifiers != null)
		{
			for (int i = 0; i < LookAtModifiers.Length; i++)
			{
				LookAtModifiers[i].DisableLookAt();
			}
		}
	}

	private static bool PopulateModifiers(GameObject gameObject, out ObLookAt[] LookAtModifiers)
	{
		LookAtModifiers = gameObject.GetComponentsInChildren<ObLookAt>();
		if (LookAtModifiers != null)
		{
			return LookAtModifiers.Length != 0;
		}
		return false;
	}
}
