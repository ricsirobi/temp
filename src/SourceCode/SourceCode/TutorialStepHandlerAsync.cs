public class TutorialStepHandlerAsync : TutorialStepHandler
{
	public override void SetupTutorialStep()
	{
		base.SetupTutorialStep();
	}

	public override void StepUpdate()
	{
		base.StepUpdate();
	}

	public override void FinishTutorialStep()
	{
		base.FinishTutorialStep();
	}

	public void AsyncMessageRecieved(string message)
	{
		if (mTutStep != null && message == mTutStep._StepDetails._AsyncMessageString && _StepProgressCallback != null)
		{
			_StepProgressCallback(0f, 0f);
		}
	}
}
