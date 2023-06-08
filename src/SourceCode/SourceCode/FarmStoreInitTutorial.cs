using System;
using UnityEngine;

public class FarmStoreInitTutorial : InteractiveTutManager
{
	public int _FreeCoins = 50;

	public GameObject _StoreTutorial;

	private bool mDeleteInstance;

	private bool mCanShowTutorial;

	public override void Start()
	{
		base.Start();
		if (!mDeleteInstance)
		{
			_StepStartedEvent = (StepStartedEvent)Delegate.Combine(_StepStartedEvent, new StepStartedEvent(OnStepStarted));
			_StepEndedEvent = (StepEndedEvent)Delegate.Combine(_StepEndedEvent, new StepEndedEvent(OnStepEnded));
		}
	}

	protected override void EnableAllItems()
	{
	}

	public void OnStepStarted(int stepIdx, string stepName)
	{
	}

	public void OnStepEnded(int stepIdx, string stepName, bool tutQuit)
	{
		if (stepName == "ClickStore" && _StoreTutorial != null)
		{
			_StoreTutorial.SendMessage("ShowTutorial");
		}
	}

	public override void SetTutDBButtonStates(bool inBtnNext, bool inBtnBack, bool inBtnYes, bool inBtnNo, bool inBtnDone, bool inBtnClose)
	{
		if (mCurrentTutIndex == 0)
		{
			base.SetTutDBButtonStates(inBtnNext: false, inBtnBack: false, inBtnYes: false, inBtnNo: false, inBtnDone: false, inBtnClose);
		}
	}

	public override void Update()
	{
		base.Update();
		if (MyRoomsIntLevel.pInstance != null && !MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt() && AvAvatar.pState != AvAvatarState.PAUSED && mCanShowTutorial && AvAvatar.pToolbar.activeSelf)
		{
			mCanShowTutorial = false;
			if (!RsResourceManager.pLastLevel.Equals(GameConfig.GetKeyData("StoreScene")) && _StoreTutorial != null)
			{
				_StoreTutorial.SendMessage("ShowTutorial");
			}
		}
	}

	public override void DeleteInstance()
	{
		mDeleteInstance = true;
	}

	private void OnWaitListCompleted()
	{
		if (!mDeleteInstance)
		{
			mCanShowTutorial = true;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
