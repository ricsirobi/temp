using UnityEngine;

public class TutorialStepHandlerTimed : TutorialStepHandler
{
	private float mStartTime = -1f;

	public override void SetupTutorialStep()
	{
		base.SetupTutorialStep();
		mStartTime = Time.time;
	}

	public override void StepUpdate()
	{
		base.StepUpdate();
		if (mTutStep != null && Time.time - mStartTime > mTutStep._StepDetails._StepTimeSeconds && _StepProgressCallback != null)
		{
			_StepProgressCallback(0f, 0f);
		}
	}

	public override void FinishTutorialStep()
	{
		base.FinishTutorialStep();
	}
}
