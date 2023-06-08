public class FishingInitialState : FishingState
{
	public override void ShowTutorial()
	{
		if (mController.IsMessageDBShown() && mController.pEndDBResult == "win")
		{
			mController.StartTutorial();
			mController.pFishingTutDB.SetOk("", mController._TutMessages[9]._LocaleText.GetLocalizedString());
			mController.pFishingTutDB.SetPosition(mController._TutMessages[9]._Position.x, mController._TutMessages[9]._Position.y);
		}
	}

	protected override void HandleOkCancel()
	{
		mController.OnTutorialDone();
		mController.SetState(1);
	}

	public override void Enter()
	{
		UtDebug.Log("ENTERING: FISHING INITIAL_STATE");
		mController.mPlayerAnimState = "";
		if (null != FishingZone._FishingZoneUi)
		{
			FishingZone._FishingZoneUi.SetStateText("");
		}
		if (null != mController._CurrentFishingRod)
		{
			mController._CurrentFishingRod.GetComponent<FishingRod>().LineSetVisible(visible: false);
		}
		base.Enter();
	}

	public override void Exit()
	{
		base.Exit();
	}

	public override void Execute()
	{
		base.Execute();
		if (!mController.IsFishingRodEquipped())
		{
			mController.ShowFishingRodButton(show: true);
			return;
		}
		mController.ShowFishingRodButton(show: false);
		if (!mController.pIsTutorialRunning || mController.pFishingTutDB == null || !mController.pFishingTutDB.GetVisibility())
		{
			mController.SetState(1);
		}
	}

	public override void Initialize(FishingZone controller, int nStateId)
	{
		base.Initialize(controller, nStateId);
	}
}
