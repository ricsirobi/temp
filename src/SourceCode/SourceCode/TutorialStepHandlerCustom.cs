using UnityEngine;

public class TutorialStepHandlerCustom : TutorialStepHandler
{
	public override void SetupTutorialStep()
	{
	}

	public override void StepUpdate()
	{
		if (mTutStep._StepDetails._CustomHandler == null)
		{
			return;
		}
		TutorialStepProgress tutorialStepProgress = mTutStep._StepDetails._CustomHandler.GetTutorialStepProgress();
		if (_StepProgressCallback != null)
		{
			_StepProgressCallback(tutorialStepProgress._Completed, tutorialStepProgress._Total);
		}
		if (!string.IsNullOrEmpty(mTutStep._StepDetails._SpawnObject) && !string.IsNullOrEmpty(mTutStep._StepDetails._SpawnAt))
		{
			GameObject gameObject = GameObject.Find(mTutStep._StepDetails._SpawnObject);
			GameObject gameObject2 = GameObject.Find(mTutStep._StepDetails._SpawnAt);
			if (gameObject != null && gameObject2 != null)
			{
				gameObject.transform.position = gameObject2.transform.position;
				gameObject.transform.rotation = gameObject2.transform.rotation;
			}
			else
			{
				UtDebug.LogError("Either spawnObject or spawnTo is null");
			}
		}
	}

	public override void FinishTutorialStep()
	{
	}
}
