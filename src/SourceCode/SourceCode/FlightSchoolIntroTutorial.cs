using System;
using UnityEngine;

public class FlightSchoolIntroTutorial : InteractiveTutManager
{
	public UiSelectHeroDragonsMenu _UiSelectHeroDragonsMenu;

	public UiObstacleCourseMenu _UiObstacleCourseMenu;

	public UiObstacleCourseSpecialMenu _UiObstacleCourseSpecialMenu;

	public UiObstacleCourseMain _UiObstacleCourseMain;

	public UiGameResults _UiGameResults;

	public UiUpsellDropDown _UiUpsellDropDown;

	public Vector2 _DragonPointerPosition = new Vector2(120f, -300f);

	public Vector2 _LevelPointerPosition = new Vector2(370f, -180f);

	public int _HeroDragonID;

	public int _ActiveLevelID;

	private bool mBlinking;

	private GameObject mBlinkObj;

	private bool mStartBlinking;

	private float mBlinkTime;

	private bool mEndBlinking;

	public Vector3 _MaxBlinkTimeScale = new Vector3(1.1f, 1.1f, 1.1f);

	public Vector3 _MinBlinkTimeScale = new Vector3(0.8f, 0.8f, 0.8f);

	public float _BlinkDuration = 0.5f;

	public KAWidget _AnimPointer;

	public override void Start()
	{
		base.Start();
		_StepStartedEvent = (StepStartedEvent)Delegate.Combine(_StepStartedEvent, new StepStartedEvent(OnStepStarted));
		_StepEndedEvent = (StepEndedEvent)Delegate.Combine(_StepEndedEvent, new StepEndedEvent(OnStepEnded));
	}

	private void OnStepStarted(int stepIdx, string stepName)
	{
		switch (stepIdx)
		{
		case 1:
			DisableDragonCards();
			break;
		case 2:
			DisableLevels();
			break;
		case 3:
		{
			KAWidget kAWidget = _UiGameResults.FindItem("DeclineBtn");
			mBlinkObj = kAWidget.gameObject;
			kAWidget = _UiGameResults.FindItem("ReplayBtn");
			kAWidget.SetDisabled(isDisabled: true);
			kAWidget = _UiUpsellDropDown.FindItem("StoreBtn");
			kAWidget.SetDisabled(isDisabled: true);
			break;
		}
		case 4:
			base.SetTutDBButtonStates(inBtnNext: true, inBtnBack: false, inBtnYes: false, inBtnNo: false, inBtnDone: false, inBtnClose: false);
			break;
		case 5:
			base.SetTutDBButtonStates(inBtnNext: false, inBtnBack: false, inBtnYes: false, inBtnNo: false, inBtnDone: true, inBtnClose: false);
			break;
		}
	}

	private void OnStepEnded(int stepIdx, string stepName, bool tutQuit)
	{
		switch (stepIdx)
		{
		case 0:
			if (_AnimPointer != null)
			{
				_AnimPointer.SetVisibility(inVisible: false);
			}
			break;
		case 1:
			if (_AnimPointer != null)
			{
				_AnimPointer.SetVisibility(inVisible: false);
			}
			break;
		case 2:
			_UiGameResults.FindItem("ReplayBtn").SetDisabled(isDisabled: false);
			_UiUpsellDropDown.FindItem("StoreBtn").SetDisabled(isDisabled: false);
			mBlinkObj = null;
			break;
		}
	}

	private void DisableDragonCards()
	{
		KAUI component = _UiSelectHeroDragonsMenu.GetComponent<UiSelectHeroDragonsMenu>();
		string text = "Hero_" + _HeroDragonID;
		if (!(component != null) || component.GetItemCount() <= 0)
		{
			return;
		}
		for (int i = 0; i < component.GetItemCount(); i++)
		{
			KAWidget kAWidget = component.FindItemAt(i);
			if (kAWidget.name != text)
			{
				kAWidget.SetDisabled(isDisabled: true);
				continue;
			}
			mBlinkObj = kAWidget.gameObject;
			if (_AnimPointer != null)
			{
				_AnimPointer.SetVisibility(inVisible: true);
				_AnimPointer.SetPosition(_DragonPointerPosition.x, _DragonPointerPosition.y);
			}
			mStartBlinking = true;
		}
	}

	private void DisableLevels()
	{
		KAUI component = _UiObstacleCourseMenu.GetComponent<UiObstacleCourseMenu>();
		if (component != null && component.GetItemCount() > 0)
		{
			for (int i = 0; i < component.GetItemCount(); i++)
			{
				KAWidget kAWidget = component.FindItemAt(i);
				UiLevelGroupSelectMenuDragons.KAUIFlightSchoolItemData kAUIFlightSchoolItemData = (UiLevelGroupSelectMenuDragons.KAUIFlightSchoolItemData)kAWidget.GetUserData();
				if (kAUIFlightSchoolItemData == null)
				{
					continue;
				}
				if (kAUIFlightSchoolItemData._LevelID != _ActiveLevelID)
				{
					kAWidget.SetDisabled(isDisabled: true);
					continue;
				}
				mBlinkObj = kAWidget.gameObject;
				mStartBlinking = true;
				if (_AnimPointer != null)
				{
					_AnimPointer.SetVisibility(inVisible: true);
					_AnimPointer.SetPosition(_LevelPointerPosition.x, _LevelPointerPosition.y);
				}
			}
		}
		KAUI component2 = _UiObstacleCourseSpecialMenu.GetComponent<UiObstacleCourseSpecialMenu>();
		if (component2 != null && component2.GetItemCount() > 0)
		{
			for (int j = 0; j < component2.GetItemCount(); j++)
			{
				component2.FindItemAt(j).SetDisabled(isDisabled: true);
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (Time.realtimeSinceStartup - mBlinkTime > _BlinkDuration && mStartBlinking && mBlinkObj != null)
		{
			mBlinkTime = Time.realtimeSinceStartup;
			TweenScale tweenScale = (mBlinking ? TweenScale.Begin(mBlinkObj, _BlinkDuration, _MinBlinkTimeScale) : TweenScale.Begin(mBlinkObj, _BlinkDuration, _MaxBlinkTimeScale));
			mBlinking = !mBlinking;
			tweenScale.method = UITweener.Method.Linear;
		}
		if (mEndBlinking && mBlinkObj != null)
		{
			mStartBlinking = false;
			float num = 0.1f;
			if (Mathf.Abs(mBlinkObj.transform.localScale.magnitude - 1f) > num)
			{
				TweenScale.Begin(mBlinkObj, _BlinkDuration, Vector3.one).method = UITweener.Method.Linear;
				return;
			}
			mBlinkObj = null;
			mEndBlinking = false;
		}
	}

	public bool CanShowSelectLevelTutorial()
	{
		if (mCurrentTutIndex == 1 || mCurrentTutIndex == 3)
		{
			return true;
		}
		return false;
	}

	public bool CanShowExitTutorial()
	{
		if (mCurrentTutIndex == 2)
		{
			return true;
		}
		return false;
	}

	public override void Exit()
	{
		base.Exit();
		AvAvatar.pState = AvAvatarState.PAUSED;
	}
}
