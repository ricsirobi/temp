public class FarmHarvestTutorial : FarmingTutorialBase
{
	public void StartTutorial()
	{
		ShowTutorial();
	}

	public override void LateUpdate()
	{
		if (mIsWaitListCompleted && !MissionManager.MissionActionPending() && !mWaitingForMissionProcessing)
		{
			base.LateUpdate();
			if (mCurrentTutIndex == 0 && mTutorialStepHandler != null && AvAvatar.pToolbar.activeSelf)
			{
				mTutorialStepHandler.SetupTutorialStep();
			}
			if (mCurrentTutIndex == 0)
			{
				HighlightHarvestSlot(isHighlight: true);
			}
			if (mCurrentTutIndex == 2)
			{
				HighlightHarvestSlot(isHighlight: false);
			}
		}
	}

	protected override void OnStepStarted(int stepIdx, string stepName)
	{
		base.OnStepStarted(stepIdx, stepName);
		switch (stepIdx)
		{
		case 0:
			HighlightHarvestSlot(isHighlight: true);
			break;
		case 2:
			if (AvAvatar.pToolbar != null)
			{
				UiToolbar component = AvAvatar.pToolbar.GetComponent<UiToolbar>();
				if (component != null && component._UiAvatarCSM != null)
				{
					component._UiAvatarCSM.OpenCSM();
					KAWidget kAWidget = component._UiAvatarCSM.FindItem("BtnCSMBackpack");
					if (kAWidget != null)
					{
						mStartBlinking = true;
						mBlinkObj = kAWidget.gameObject;
					}
				}
			}
			HighlightHarvestSlot(isHighlight: false);
			break;
		}
	}

	protected override void OnStepEnded(int stepIdx, string stepName, bool tutQuit)
	{
		base.OnStepEnded(stepIdx, stepName, tutQuit);
		if (stepIdx == 2)
		{
			mEndBlinking = true;
		}
	}

	private void HighlightHarvestSlot(bool isHighlight)
	{
		FarmManager farmManager = MyRoomsIntMain.pInstance as FarmManager;
		if (!(farmManager != null))
		{
			return;
		}
		foreach (FarmItem pFarmItem in farmManager.pFarmItems)
		{
			CropFarmItem cropFarmItem = pFarmItem as CropFarmItem;
			if (cropFarmItem != null && cropFarmItem.pCurrentStage._Name.Equals("Harvest"))
			{
				pFarmItem.HighlightObject(isHighlight);
				pFarmItem.SetCSMVisible(!isHighlight);
			}
		}
	}

	public override void SetTutDBButtonStates(bool inBtnNext, bool inBtnBack, bool inBtnYes, bool inBtnNo, bool inBtnDone, bool inBtnClose)
	{
		switch (mCurrentTutIndex)
		{
		case 0:
			base.SetTutDBButtonStates(inBtnNext: false, inBtnBack: false, inBtnYes: false, inBtnNo: false, inBtnDone: false, inBtnClose);
			break;
		case 1:
			base.SetTutDBButtonStates(inBtnNext: false, inBtnBack: false, inBtnYes: false, inBtnNo: false, inBtnDone: false, inBtnClose);
			break;
		case 2:
			base.SetTutDBButtonStates(inBtnNext: false, inBtnBack: false, inBtnYes: false, inBtnNo: false, inBtnDone: false, inBtnClose);
			break;
		case 3:
			base.SetTutDBButtonStates(inBtnNext: false, inBtnBack: false, inBtnYes: false, inBtnNo: false, inBtnDone: false, inBtnClose);
			break;
		}
	}
}
