using UnityEngine;

public class AIBehavior_FuncCall : AIBehavior
{
	public GameObject _ObjectWithScript;

	public string _FunctionName = "";

	public bool _UseParent;

	public override AIBehaviorState Think(AIActor Actor)
	{
		GameObject objectWithScript = _ObjectWithScript;
		if (_UseParent && objectWithScript != null && objectWithScript.transform.parent != null)
		{
			objectWithScript = objectWithScript.transform.parent.gameObject;
		}
		if (Actor == null || objectWithScript == null || string.IsNullOrEmpty(_FunctionName))
		{
			return SetState(AIBehaviorState.FAILED);
		}
		Actor.BehaviorFunctionCallResult = AIBehaviorState.COMPLETED;
		objectWithScript.SendMessage(_FunctionName, Actor, SendMessageOptions.DontRequireReceiver);
		return Actor.BehaviorFunctionCallResult;
	}

	public override void OnStart(AIActor Actor)
	{
		base.OnStart(Actor);
		SetState(AIBehaviorState.ACTIVE);
	}
}
