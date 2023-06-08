using System;
using System.Collections.Generic;
using UnityEngine;

public class FarmingTutorialBase : InteractiveTutManager
{
	public ObClickable _JobBoardClickable;

	protected ContextSensitiveManager mCSMManager;

	public string _FarmingSceneName = "FarmingDO";

	protected bool mBlinking;

	protected GameObject mBlinkObj;

	protected bool mStartBlinking;

	protected float mBlinkTime;

	public Vector3 _MaxBlinkTimeScale = new Vector3(1.1f, 1.1f, 1.1f);

	public Vector3 _MinBlinkTimeScale = new Vector3(0.8f, 0.8f, 0.8f);

	public float _BlinkDuration = 0.5f;

	protected bool mEndBlinking;

	protected bool mIsWaitListCompleted;

	protected bool mDeleteInstance;

	protected bool mWaitingForMissionProcessing;

	public override void Start()
	{
		base.Start();
		_StepStartedEvent = (StepStartedEvent)Delegate.Combine(_StepStartedEvent, new StepStartedEvent(OnStepStarted));
		_StepEndedEvent = (StepEndedEvent)Delegate.Combine(_StepEndedEvent, new StepEndedEvent(OnStepEnded));
		if (MissionManager.IsTaskActive("Visit", "Scene", RsResourceManager.pCurrentLevel))
		{
			MissionManager.AddMissionEventHandler(OnMissionEvent);
			mWaitingForMissionProcessing = true;
		}
	}

	private void OnDestroy()
	{
		MissionManager.RemoveMissionEventHandler(OnMissionEvent);
	}

	public void OnMissionEvent(MissionEvent inEvent, object inObject)
	{
		if ((inEvent == MissionEvent.TASK_END_COMPLETE || inEvent == MissionEvent.TASK_COMPLETE) && inObject is Task task)
		{
			List<Task> tasks = new List<Task>();
			MissionManager.pInstance.GetNextTask(task._Mission, ref tasks);
			if (tasks.Count > 0 || task._Mission.Rewards.Count <= 0)
			{
				mWaitingForMissionProcessing = false;
			}
		}
		if (inEvent == MissionEvent.MISSION_REWARDS_COMPLETE)
		{
			mWaitingForMissionProcessing = false;
		}
	}

	public override void Update()
	{
		if (!mIsWaitListCompleted || MissionManager.MissionActionPending() || mWaitingForMissionProcessing)
		{
			return;
		}
		base.Update();
		if (mInitShowTutorial)
		{
			base.ShowTutorial();
			if (mIsShowingTutorial && _JobBoardClickable != null)
			{
				_JobBoardClickable._Active = false;
			}
		}
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
			}
			else
			{
				mBlinkObj = null;
				mEndBlinking = false;
			}
		}
		ProcessDeleteInstance();
	}

	public override void DeleteInstance()
	{
		mDeleteInstance = true;
		MissionManager.RemoveMissionEventHandler(OnMissionEvent);
	}

	public virtual void ProcessDeleteInstance()
	{
		if (mDeleteInstance)
		{
			_StepStartedEvent = (StepStartedEvent)Delegate.Remove(_StepStartedEvent, new StepStartedEvent(OnStepStarted));
			_StepEndedEvent = (StepEndedEvent)Delegate.Remove(_StepEndedEvent, new StepEndedEvent(OnStepEnded));
			KAInput.pInstance.EnableInputType("CameraZoom", InputType.ALL, inEnable: true);
			base.DeleteInstance();
		}
	}

	protected virtual void OnStepStarted(int stepIdx, string stepName)
	{
		KAInput.pInstance.EnableInputType("CameraZoom", InputType.ALL, inEnable: false);
		if (mCSMManager == null)
		{
			GameObject gameObject = GameObject.Find("ContextSensitiveManager");
			if (gameObject != null)
			{
				mCSMManager = gameObject.GetComponent<ContextSensitiveManager>();
			}
		}
	}

	protected virtual void OnStepEnded(int stepIdx, string stepName, bool tutQuit)
	{
		KAInput.pInstance.EnableInputType("CameraZoom", InputType.ALL, inEnable: true);
	}

	protected void SetBackgroundActive(bool isGlobalClickActive, bool isCSMVisible)
	{
		ObClickable.pGlobalActive = isGlobalClickActive;
		if (mCSMManager != null)
		{
			mCSMManager.SetVisibility(isCSMVisible);
		}
	}

	public override void ShowTutorial()
	{
		if (mIsShowingTutorial)
		{
			return;
		}
		if (MyRoomsIntLevel.pInstance != null && !MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt())
		{
			mInitShowTutorial = true;
			return;
		}
		string ownerIDForLevel = MainStreetMMOClient.pInstance.GetOwnerIDForLevel(_FarmingSceneName);
		if (string.IsNullOrEmpty(ownerIDForLevel) || ownerIDForLevel == UserInfo.pInstance.UserID)
		{
			mInitShowTutorial = true;
		}
	}

	public virtual void OnWaitListCompleted()
	{
		mIsWaitListCompleted = true;
	}

	public override void Exit()
	{
		if (_JobBoardClickable != null)
		{
			_JobBoardClickable._Active = true;
		}
		base.Exit();
	}
}
