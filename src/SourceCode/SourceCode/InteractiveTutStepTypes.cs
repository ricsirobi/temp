using System;

[Serializable]
public enum InteractiveTutStepTypes
{
	STEP_TYPE_NON_INTERACTIVE,
	STEP_TYPE_UI_CLICK,
	STEP_TYPE_GAMEOBJECT_CLICK,
	STEP_TYPE_PROXIMITY_CHECK,
	STEP_TYPE_TIMED_STEP,
	STEP_TYPE_ASYNC,
	STEP_TYPE_CUSTOM
}
