public class FishingScareState : FishingState
{
	public override void Enter()
	{
		mController.mPlayerAnimState = "strike";
		UtDebug.Log("ENTERING : SCARE_STATE ");
		mController.ShowResultDB(mController._LostBaitText.GetLocalizedString(), mController._FishCaughtFailureTitleText.GetLocalizedString(), "lose");
		mController.LoseBait();
		mController._CurrentFishingRod.GetComponent<FishingRod>().LineSetVisible(visible: false);
		FishingZone._FishingZoneUi.RemoveFish();
	}

	public override void Exit()
	{
		mController.Reset();
		base.Exit();
	}

	public override void Execute()
	{
		base.Execute();
		UtDebug.Log("You have scared a fish.. start nibbling again");
		mController.SetState(0);
	}

	public override void Initialize(FishingZone controller, int nStateId)
	{
		base.Initialize(controller, nStateId);
	}
}
