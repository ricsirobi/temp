public class FishingSnapState : FishingState
{
	private static int mTimesSnapped;

	public override void Enter()
	{
		mTimesSnapped++;
		mController.mPlayerAnimState = "strike";
		UtDebug.Log("ENTERING : SNAP_STATE ");
		mController._CurrentFishingRod.GetComponent<FishingRod>().LineSetVisible(visible: false);
		if (null != mController._SndLineSnap)
		{
			SnChannel.Play(mController._SndLineSnap, "DEFAULT_POOL", inForce: true, null);
		}
		FishingZone._FishingZoneUi.RemoveFish();
		if (mTimesSnapped >= mController._LineSnapStoreReminderCount)
		{
			mTimesSnapped = 0;
			mController.ShowResultDB(mController._LineSnappedStoreText.GetLocalizedString(), mController._FishCaughtFailureTitleText.GetLocalizedString(), "lose");
		}
		else
		{
			mController.ShowResultDB(mController._LineSnappedText.GetLocalizedString(), mController._FishCaughtFailureTitleText.GetLocalizedString(), "lose");
		}
		mController.SnapLine();
		if (!mController.pIsTutAvailable)
		{
			mController.LoseBait();
		}
		mController.Reset();
		base.Enter();
	}

	public override void ShowTutorial()
	{
	}

	protected override void HandleOkCancel()
	{
		mController.SetState(2);
	}

	public override void Exit()
	{
		base.Exit();
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
