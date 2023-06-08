using System;
using UnityEngine;

public class StableHatchTutorial : InteractiveTutManager
{
	public GameObject _ObLavaPit;

	public override void Start()
	{
		UiStablesListCardMenu.pOnStablesListUILoaded = (OnStablesListUILoad)Delegate.Combine(UiStablesListCardMenu.pOnStablesListUILoaded, new OnStablesListUILoad(OnStableListUILoad));
		SetUpClickableObjects(enable: false);
		base.Start();
	}

	public void OnStableListUILoad(bool isSuccess)
	{
		UiStablesListCardMenu.pOnStablesListUILoaded = (OnStablesListUILoad)Delegate.Remove(UiStablesListCardMenu.pOnStablesListUILoaded, new OnStablesListUILoad(OnStableListUILoad));
		if (InteractiveTutManager._CurrentActiveTutorialObject != null)
		{
			InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", "StableUILoaded");
		}
	}

	public override void DeleteInstance()
	{
		base.DeleteInstance();
		SetUpClickableObjects(enable: true);
		UserNotifyStableTutorial.pInstance.SetToWaitState(flag: false);
		UserNotifyStableTutorial.pInstance.CheckForNextTut();
	}

	private void SetUpClickableObjects(bool enable)
	{
		ObClickable.pGlobalActive = enable;
		if (_ObLavaPit != null)
		{
			_ObLavaPit.GetComponent<ObClickable>()._UseGlobalActive = enable;
			_ObLavaPit.GetComponent<ObProximityStableHatch>()._UseGlobalActive = enable;
		}
	}
}
