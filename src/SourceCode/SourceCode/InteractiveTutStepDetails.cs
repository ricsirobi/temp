using System;

[Serializable]
public class InteractiveTutStepDetails
{
	public InteractiveTutStepTypes _StepType;

	public ClickableUiProperties[] _WaitForClickableUi;

	public string[] _WaitForClickableObject;

	public string _SpawnAt;

	public string _SpawnObject;

	public string _ProximityItem1;

	public string _ProximityItem2;

	public float _ProximityDistance;

	public float _StepTimeSeconds;

	public string _AsyncMessageString;

	public string _RollOverCursorName;

	public CustomStepHandlerInterface _CustomHandler;
}
