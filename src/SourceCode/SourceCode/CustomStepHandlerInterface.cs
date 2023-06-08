using System;

[Serializable]
public class CustomStepHandlerInterface
{
	public virtual TutorialStepProgress GetTutorialStepProgress()
	{
		return new TutorialStepProgress(0f, 0f);
	}
}
