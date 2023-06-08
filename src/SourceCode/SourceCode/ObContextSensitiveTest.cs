using UnityEngine;

public class ObContextSensitiveTest : ObContextSensitive
{
	public ContextSensitiveState[] _States;

	protected override void UpdateData(ref ContextSensitiveState[] inStatesArrData)
	{
		inStatesArrData = _States;
	}

	protected override void Update()
	{
		base.Update();
		if (KAInput.GetKeyUp(KeyCode.I))
		{
			Debug.Log("Setting names forcefully");
			ContextSensitiveState state = GetState(ContextSensitiveStateType.OBJECT_STATE);
			if (state != null)
			{
				state._CurrentContextNamesList = new string[1] { "ObjectState" };
			}
		}
		else if (KAInput.GetKeyUp(KeyCode.J))
		{
			Debug.Log("Resetting names forcefully");
			ContextSensitiveState state2 = GetState(ContextSensitiveStateType.OBJECT_STATE);
			if (state2 != null)
			{
				state2._CurrentContextNamesList = new string[0];
			}
			CloseMenu();
		}
		else if (KAInput.GetKeyUp(KeyCode.K))
		{
			Debug.Log("Destroy all ....");
			CloseMenu(checkProximity: true);
		}
	}

	public void OnContextAction(string inName)
	{
		if (!string.IsNullOrEmpty(inName))
		{
			Debug.Log("Clicked!!!!!!!!!!! " + inName);
		}
	}

	public ContextSensitiveState GetState(ContextSensitiveStateType inType)
	{
		ContextSensitiveState[] states = _States;
		foreach (ContextSensitiveState contextSensitiveState in states)
		{
			if (contextSensitiveState._MenuType == inType)
			{
				return contextSensitiveState;
			}
		}
		return null;
	}
}
