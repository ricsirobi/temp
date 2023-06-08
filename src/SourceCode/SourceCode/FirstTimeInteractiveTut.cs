using System;
using UnityEngine;

public class FirstTimeInteractiveTut : InteractiveTutManager
{
	public InteractiveTutManager _NextMobileTut;

	public InteractiveTutManager _NextOnlineTut;

	private Vector3 mCachedPosition = Vector3.zero;

	private bool mStepFourReached;

	private UiJoystick mUiJoyStick;

	private int mPrevPriority;

	public override void Start()
	{
		base.Start();
		_StepEndedEvent = (StepEndedEvent)Delegate.Combine(_StepEndedEvent, new StepEndedEvent(OnStepEnded));
		if (base.gameObject.name == "PfFirstTimeTutMobile")
		{
			_StepStartedEvent = (StepStartedEvent)Delegate.Combine(_StepStartedEvent, new StepStartedEvent(OnStepStarted));
		}
	}

	public void OnStepEnded(int stepIdx, string stepName, bool tutQuit)
	{
		switch (stepName)
		{
		case "AcceptQuest":
			_StepEndedEvent = (StepEndedEvent)Delegate.Remove(_StepEndedEvent, new StepEndedEvent(OnStepEnded));
			if (UtPlatform.IsMobile())
			{
				_NextMobileTut.gameObject.SetActive(value: true);
				_NextMobileTut.ShowTutorial();
				_NextMobileTut._IdleRepeatInterval = 5f;
			}
			else if (UtPlatform.IsWSA())
			{
				if (UtUtilities.IsKeyboardAttached())
				{
					_NextOnlineTut.gameObject.SetActive(value: true);
					_NextOnlineTut.ShowTutorial();
				}
				else
				{
					_NextMobileTut.gameObject.SetActive(value: true);
					_NextMobileTut.ShowTutorial();
					_NextMobileTut._IdleRepeatInterval = 5f;
				}
			}
			else
			{
				_NextOnlineTut.gameObject.SetActive(value: true);
				_NextOnlineTut.ShowTutorial();
			}
			break;
		case "GoodJob":
			UiCustomizeHUD.Load(base.gameObject, UILoadOptions.NEW_SCENE);
			break;
		case "ShowCustomizationScreen":
			KAInput.ShowJoystick(UiJoystick.pInstance.pPos, inShow: true);
			if (MainStreetMMOClient.pInstance != null)
			{
				MainStreetMMOClient.pInstance.ActivateAll(active: true);
			}
			_StepStartedEvent = (StepStartedEvent)Delegate.Remove(_StepStartedEvent, new StepStartedEvent(OnStepStarted));
			if (mUiJoyStick != null)
			{
				mUiJoyStick.SetPriority(mPrevPriority);
				mUiJoyStick.ResetJoystick();
			}
			Exit();
			break;
		}
	}

	public void OnStepStarted(int stepNo, string stepName)
	{
		if (stepName == "JoyStickMobile")
		{
			KAInput.ShowJoystick(UiJoystick.pInstance.pPos, inShow: false);
		}
	}

	public void SetData()
	{
		mCachedPosition = AvAvatar.position;
		mStepFourReached = true;
	}

	public override void SetTutDBButtonStates(bool inBtnNext, bool inBtnBack, bool inBtnYes, bool inBtnNo, bool inBtnDone, bool inBtnClose)
	{
		if (GetStepName() == "JoyStickMobile")
		{
			base.SetTutDBButtonStates(inBtnNext: false, inBtnBack: false, inBtnYes: false, inBtnNo: false, inBtnDone: true, inBtnClose: false);
		}
		else
		{
			base.SetTutDBButtonStates(inBtnNext, inBtnBack, inBtnYes, inBtnNo, inBtnDone, inBtnClose);
		}
	}

	public override void ShowTutorial()
	{
		if (mUiJoyStick == null)
		{
			mUiJoyStick = KAInput.GetJoyStick(JoyStickPos.BOTTOM_LEFT);
		}
		if (mUiJoyStick != null)
		{
			mUiJoyStick.SetVisibility(isVisible: false);
		}
		if (base.gameObject.name == "PfFirstTimeTutOnline" || base.gameObject.name == "PfFirstTimeTutMobile")
		{
			SetData();
		}
		base.ShowTutorial();
	}

	public override void Exit()
	{
		base.Exit();
		UiActionsMenu componentInChildren = AvAvatar.pToolbar.GetComponentInChildren<UiActionsMenu>();
		if (componentInChildren != null)
		{
			componentInChildren.SetVisibility(inVisible: false);
		}
	}

	public override void Update()
	{
		base.Update();
		if (!mStepFourReached)
		{
			return;
		}
		float sqrMagnitude = (mCachedPosition - AvAvatar.position).sqrMagnitude;
		if (sqrMagnitude > 10f || base.gameObject.name == "PfFirstTimeTutOnline")
		{
			mStepFourReached = false;
			if (mTutorialStepHandler != null)
			{
				mTutorialStepHandler.FinishTutorialStep();
			}
			StepProgressCallback(0f, 0f);
			if (base.gameObject.name == "PfFirstTimeTutOnline")
			{
				Exit();
			}
		}
		if (sqrMagnitude > 0.1f)
		{
			KAUI component = base.transform.parent.Find("PfUiMovement").GetComponent<KAUI>();
			if (component != null)
			{
				KAWidget kAWidget = component.FindItem("AniJoystick");
				kAWidget.StopAnim("Flash");
				kAWidget.SetVisibility(inVisible: false);
			}
		}
	}
}
