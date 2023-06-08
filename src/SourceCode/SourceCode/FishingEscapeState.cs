public class FishingEscapeState : FishingState
{
	public override void ShowTutorial()
	{
	}

	protected override void HandleOkCancel()
	{
		Reset();
	}

	private void Reset()
	{
		string localizedString = mController._LostBaitText.GetLocalizedString();
		if (!mController.pIsTutAvailable)
		{
			mController.LoseBait();
		}
		else
		{
			localizedString = mController._LostBaitTutorialText.GetLocalizedString();
		}
		mController.ShowResultDB(localizedString, mController._FishCaughtFailureTitleText.GetLocalizedString(), "lose");
		mController.Reset();
		FishingZone._FishingZoneUi.SetStateText("");
	}

	public override void Enter()
	{
		mController.mPlayerAnimState = "strike";
		UtDebug.Log("ENTERING : Escape ");
		if (!mController.pIsTutAvailable || mController._StrikeFailed)
		{
			Reset();
		}
		else
		{
			base.Enter();
		}
		if (null != mController._SndBaitLost)
		{
			SnChannel.Play(mController._SndBaitLost, "DEFAULT_POOL", inForce: true, null);
		}
		FishingZone._FishingZoneUi.RemoveFish();
	}

	public override void Exit()
	{
		if (mController.pIsTutAvailable)
		{
			base.Exit();
		}
	}

	public override void Execute()
	{
		base.Execute();
	}

	public override void Initialize(FishingZone controller, int nStateId)
	{
		base.Initialize(controller, nStateId);
	}
}
