using UnityEngine;

public class TPTutorialManager : InteractiveTutManager
{
	public bool IsTutorialRunning()
	{
		if (mIsShowingTutorial && !TutorialComplete())
		{
			return true;
		}
		return false;
	}

	public void SkipTutorial()
	{
		if (mTutorialStepHandler != null)
		{
			mTutorialStepHandler.FinishTutorialStep();
		}
		mCurrentTutIndex = _TutSteps.Length - 1;
		InteractiveTutStep tutStepDef = _TutSteps[mCurrentTutIndex];
		mTutorialStepHandler = TutorialStepHandler.InitTutorialStepHandler(tutStepDef);
		if (mTutorialStepHandler != null)
		{
			mTutorialStepHandler.FinishTutorialStep();
		}
		StartNextTutorial();
		Exit();
		SnChannel.StopPool("VO_Pool");
	}

	public override void Update()
	{
		base.Update();
		if (Input.GetKeyDown(KeyCode.Space) && IsTutorialRunning())
		{
			SkipTutorial();
		}
	}
}
