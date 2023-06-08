using System.Collections;
using UnityEngine;

public class AIBehavior_LookAtMouse : AIBehavior
{
	public float _LookAtSpeed = 4f;

	private ObLookAt[] mLookAtModifiers;

	public override AIBehaviorState Think(AIActor Actor)
	{
		PopulateModifiers(Actor.transform, ref mLookAtModifiers);
		DoLookAtMouse(Actor.transform, Actor.GetCamera(), ref mLookAtModifiers, _LookAtSpeed);
		return SetState(AIBehaviorState.ACTIVE);
	}

	public override void OnStart(AIActor Actor)
	{
		base.OnStart(Actor);
		PopulateModifiers(Actor.transform, ref mLookAtModifiers);
		SetState(AIBehaviorState.ACTIVE);
		base.enabled = true;
	}

	public override void OnTerminate(AIActor Actor)
	{
		base.OnTerminate(Actor);
		StopLookAt(mLookAtModifiers);
	}

	public static IEnumerator LookAtMouse(Transform Location, Camera pCamera, float Speed)
	{
		ObLookAt[] LookAtModifiers = Location.GetComponentsInChildren<ObLookAt>();
		while (true)
		{
			DoLookAtMouse(Location, pCamera, ref LookAtModifiers, Speed);
			yield return null;
		}
	}

	private static void DoLookAtMouse(Transform location, Camera pCamera, ref ObLookAt[] LookAtModifiers, float Speed)
	{
		if (LookAtModifiers != null && !(pCamera == null))
		{
			float num = Vector3.Distance(pCamera.transform.position, location.position);
			num = Mathf.Min(num * 0.3f, 2f);
			Vector3 point = pCamera.ScreenPointToRay(Input.mousePosition).GetPoint(num);
			ObLookAt[] array = LookAtModifiers;
			foreach (ObLookAt obj in array)
			{
				obj._LookAtSpeed = Speed;
				obj.LookAt(point);
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

	public static void StopLookAt(GameObject Obj)
	{
		StopLookAt(Obj.GetComponentsInChildren<ObLookAt>());
	}

	private static bool PopulateModifiers(Transform location, ref ObLookAt[] LookAtModifiers)
	{
		if (LookAtModifiers != null)
		{
			LookAtModifiers = location.GetComponentsInChildren<ObLookAt>();
		}
		if (LookAtModifiers != null)
		{
			return LookAtModifiers.Length != 0;
		}
		return false;
	}
}
