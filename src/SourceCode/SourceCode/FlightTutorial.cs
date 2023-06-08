using System;
using UnityEngine;

public class FlightTutorial : InteractiveTutManager
{
	private KAWidget mPointerArrow;

	private AvAvatarController mAvatarController;

	public override void ShowTutorial()
	{
		mAvatarController = AvAvatar.pObject?.GetComponent<AvAvatarController>();
		_StepStartedEvent = (StepStartedEvent)Delegate.Combine(_StepStartedEvent, new StepStartedEvent(OnStepStart));
		_StepEndedEvent = (StepEndedEvent)Delegate.Combine(_StepEndedEvent, new StepEndedEvent(OnStepEnd));
		AvAvatar.pInputEnabled = false;
		base.ShowTutorial();
	}

	public override void LateUpdate()
	{
		base.LateUpdate();
		StopFlight();
	}

	private void StopFlight()
	{
		if ((bool)mAvatarController)
		{
			mAvatarController.pVelocity = Vector3.zero;
			mAvatarController.pFlightSpeed = 0f;
		}
	}

	public void OnStepStart(int stepIdx, string stepName)
	{
		if (stepName == "ShowBoost")
		{
			mPointerArrow = UiAvatarControls.pInstance.FindItem("AniWingFlapPointer");
			if (mPointerArrow != null)
			{
				mPointerArrow.SetVisibility(inVisible: true);
				mPointerArrow.PlayAnim("Play");
			}
		}
		else if (stepName == "ShowBrake")
		{
			mPointerArrow = UiAvatarControls.pInstance.FindItem("AniBrakePointer");
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
		if (stepName == "ShowBrake")
		{
			AvAvatar.pInputEnabled = true;
		}
	}

	public override void Exit()
	{
		base.Exit();
		StopFlight();
		AnalyticAgent.LogFTUEEvent(FTUEEvent.FLIGHT_TUTORIAL_COMPLETE);
	}
}
