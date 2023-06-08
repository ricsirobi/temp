using System;
using UnityEngine;

public class FlightSuitTutorial : InteractiveTutManager
{
	private AvAvatarState mAvatarLastState;

	private bool mCachedInputEnabled;

	private bool mCachedIsInputHidden;

	private KAWidget mPointerArrow;

	public override void Start()
	{
		base.Start();
		_StepStartedEvent = (StepStartedEvent)Delegate.Combine(_StepStartedEvent, new StepStartedEvent(OnStepStarted));
		_StepEndedEvent = (StepEndedEvent)Delegate.Combine(_StepEndedEvent, new StepEndedEvent(OnStepEnded));
	}

	private void OnStepStarted(int stepIdx, string stepName)
	{
		if (!(stepName == "Step1"))
		{
			if (stepName == "Step2")
			{
				mCachedInputEnabled = AvAvatar.pInputEnabled;
				UiAvatarControls.pInstance.pAVController.DisableSoarButtonUpdate = true;
				AvAvatar.pInputEnabled = false;
				mCachedIsInputHidden = KAInput.pInstance.pAreInputsHidden;
				KAInput.pInstance.ShowInputs(inShow: true);
				AvAvatar.pState = AvAvatarState.IDLE;
				ShowInputButtons(show: true);
			}
		}
		else
		{
			AvAvatar.pState = AvAvatarState.PAUSED;
		}
	}

	private void ShowInputButtons(bool show)
	{
		if (!(UiAvatarControls.pInstance != null))
		{
			return;
		}
		UiAvatarControls.pInstance.SetVisibility(show);
		if (show)
		{
			UiAvatarControls.pInstance.DisableAllDragonControls();
		}
		UiAvatarControls.pInstance.pAVController.ShowSoarButton(show);
		KAInput.pInstance.EnableInputType("Jump", InputType.ALL, show);
		mPointerArrow = UiAvatarControls.pInstance.FindItem("AniSoarPointer");
		if (mPointerArrow != null)
		{
			mPointerArrow.SetVisibility(show);
			if (show)
			{
				mPointerArrow.PlayAnim("Play");
			}
		}
	}

	private void OnStepEnded(int stepIdx, string stepName, bool tutQuit)
	{
		if (stepName == "Step2")
		{
			Debug.Log("Step3 is ended");
			KAInput.pInstance.ShowInputs(!mCachedInputEnabled);
			AvAvatar.pInputEnabled = mCachedInputEnabled;
			UiAvatarControls.pInstance.pAVController.DisableSoarButtonUpdate = false;
			ShowInputButtons(show: false);
		}
	}
}
