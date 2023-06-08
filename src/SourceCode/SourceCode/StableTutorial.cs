using System;
using UnityEngine;

public class StableTutorial : InteractiveTutManager
{
	private bool mIsStableUIClosed = true;

	public override void Start()
	{
		UiDragonsStable.pOnStablesUILoadHandler = (OnStablesUILoad)Delegate.Combine(UiDragonsStable.pOnStablesUILoadHandler, new OnStablesUILoad(OnStableUILoad));
		UiDragonsStable.pOnStablesUIClosed = (OnStablesUIClosed)Delegate.Combine(UiDragonsStable.pOnStablesUIClosed, new OnStablesUIClosed(OnStableUIClose));
		ObClickable.pGlobalActive = false;
		base.Start();
	}

	public void OnStableUILoad(bool isSuccess)
	{
		mIsStableUIClosed = false;
		UiDragonsStable.pOnStablesUILoadHandler = (OnStablesUILoad)Delegate.Remove(UiDragonsStable.pOnStablesUILoadHandler, new OnStablesUILoad(OnStableUILoad));
		mTutorialStepHandler.FinishTutorialStep();
		StartNextTutorial();
	}

	public void OnStableUIClose()
	{
		UiDragonsStable.pOnStablesUIClosed = (OnStablesUIClosed)Delegate.Remove(UiDragonsStable.pOnStablesUIClosed, new OnStablesUIClosed(OnStableUIClose));
		mTutorialStepHandler.FinishTutorialStep();
		ObClickable.pGlobalActive = true;
		mIsStableUIClosed = true;
		UserNotifyStableTutorial.pInstance.SetToWaitState(flag: false);
		DeleteInstance();
	}

	public override void DeleteInstance()
	{
		if (mIsStableUIClosed)
		{
			if (this != null)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		else if (UserNotifyStableTutorial.pInstance != null)
		{
			UserNotifyStableTutorial.pInstance.SetToWaitState(flag: true);
			AvAvatar.EnableAllInputs(inActive: false);
			AvAvatar.pState = AvAvatarState.PAUSED;
		}
	}
}
