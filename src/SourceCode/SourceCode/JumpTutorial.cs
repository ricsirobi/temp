using System;

public class JumpTutorial : InteractiveTutManager
{
	private KAWidget mPointerArrow;

	public override void Update()
	{
		base.Update();
		if (IsShowingTutorial())
		{
			AvAvatar.pInputEnabled = false;
		}
	}

	public override void ShowTutorial()
	{
		_StepStartedEvent = (StepStartedEvent)Delegate.Combine(_StepStartedEvent, new StepStartedEvent(OnStepStart));
		_StepEndedEvent = (StepEndedEvent)Delegate.Combine(_StepEndedEvent, new StepEndedEvent(OnStepEnd));
		AvAvatar.pInputEnabled = false;
		base.ShowTutorial();
	}

	public void OnStepStart(int stepIdx, string stepName)
	{
		if (stepName == "ShowJump")
		{
			mPointerArrow = UiAvatarControls.pInstance.FindItem("AniWingFlapPointer");
			if (mPointerArrow != null)
			{
				mPointerArrow.SetVisibility(inVisible: true);
				mPointerArrow.PlayAnim("Play");
			}
		}
	}

	public void OnStepEnd(int stepIdx, string stepName, bool tutQuit)
	{
		if (mPointerArrow != null)
		{
			mPointerArrow.SetVisibility(inVisible: false);
		}
		if (stepName == "ShowJump")
		{
			AvAvatar.pInputEnabled = true;
		}
	}
}
