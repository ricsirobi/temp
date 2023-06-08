using System;

public class DragonWeaponTutorial : InteractiveTutManager
{
	private KAWidget mPointerArrow;

	public override void ShowTutorial()
	{
		_StepStartedEvent = (StepStartedEvent)Delegate.Combine(_StepStartedEvent, new StepStartedEvent(OnStepStart));
		_StepEndedEvent = (StepEndedEvent)Delegate.Combine(_StepEndedEvent, new StepEndedEvent(OnStepEnd));
		AvAvatar.pInputEnabled = false;
		base.ShowTutorial();
	}

	public void OnStepStart(int stepIdx, string stepName)
	{
		if (stepName == "ShowFire")
		{
			mPointerArrow = UiAvatarControls.pInstance.FindItem("AniFirePointer");
		}
		else if (stepName == "ShowHealth" && AvAvatar.pToolbar != null)
		{
			KAUI component = AvAvatar.pToolbar.GetComponent<KAUI>();
			if (component != null)
			{
				mPointerArrow = component.FindItem("AniHealthMeterPointer");
			}
		}
		if (mPointerArrow != null)
		{
			mPointerArrow.SetVisibility(inVisible: true);
			mPointerArrow.PlayAnim("Play");
		}
	}

	public void OnStepEnd(int stepIdx, string stepName, bool tutQuit)
	{
		if (mPointerArrow != null)
		{
			mPointerArrow.SetVisibility(inVisible: false);
		}
		if (stepName == "ShowHealth")
		{
			AvAvatar.pInputEnabled = true;
		}
	}
}
