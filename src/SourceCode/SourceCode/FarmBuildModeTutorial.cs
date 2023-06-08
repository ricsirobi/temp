using UnityEngine;

public class FarmBuildModeTutorial : FarmingTutorialBase
{
	public string _CreativePointTutorialId = "FarmCreativePointTutorial";

	public int _InitialBuildModeAwardItemId = 8231;

	public KAWidget[] _DisableWidget;

	protected override void OnStepStarted(int stepIdx, string stepName)
	{
		base.OnStepStarted(stepIdx, stepName);
		if (stepIdx != 1)
		{
			return;
		}
		if (CommonInventoryData.pInstance.FindItem(_InitialBuildModeAwardItemId) == null)
		{
			CommonInventoryData.pInstance.AddItem(_InitialBuildModeAwardItemId, updateServer: true);
		}
		SetBackgroundActive(isGlobalClickActive: false, isCSMVisible: true);
		GameObject gameObject = GameObject.Find("RoomBuilderCategoryMenu");
		if (!(gameObject != null))
		{
			return;
		}
		UiMyRoomBuilderCategoryMenu component = gameObject.GetComponent<UiMyRoomBuilderCategoryMenu>();
		if (component != null)
		{
			KAWidget kAWidget = component.FindItem("394");
			if (kAWidget != null)
			{
				mStartBlinking = true;
				mBlinkObj = kAWidget.gameObject;
			}
		}
	}

	protected override void OnStepEnded(int stepIdx, string stepName, bool tutQuit)
	{
		base.OnStepEnded(stepIdx, stepName, tutQuit);
		switch (stepIdx)
		{
		case 1:
			mEndBlinking = true;
			break;
		case 2:
			SetBackgroundActive(isGlobalClickActive: true, isCSMVisible: true);
			break;
		case 3:
			ProductData.AddTutorial(_CreativePointTutorialId);
			break;
		}
	}

	public override void SetTutDBButtonStates(bool inBtnNext, bool inBtnBack, bool inBtnYes, bool inBtnNo, bool inBtnDone, bool inBtnClose)
	{
		if (mCurrentTutIndex == 5)
		{
			base.SetTutDBButtonStates(inBtnNext: false, inBtnBack: false, inBtnYes: false, inBtnNo: false, inBtnDone: true, inBtnClose: false);
		}
	}

	protected override void RestoreUI()
	{
		base.RestoreUI();
		KAWidget[] disableWidget = _DisableWidget;
		foreach (KAWidget kAWidget in disableWidget)
		{
			if (kAWidget != null)
			{
				kAWidget.SetDisabled(isDisabled: true);
			}
		}
	}
}
