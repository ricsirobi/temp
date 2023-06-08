public class FishingReadyToCastState : FishingState
{
	public override void Enter()
	{
		mController.mPlayerAnimState = "idle";
		UtDebug.Log("ENTERING : ReadyToCast state ");
		UtDebug.Log("cast fishing line and wait for fish to nibble...");
		base.Enter();
	}

	public override void Execute()
	{
		base.Execute();
		if (KAInput.GetKeyUp("CastRod") || FishingZone._FishingZoneUi.IsReelClicked())
		{
			FishingZone._FishingZoneUi.SetReelClicked(isClicked: false);
			mController.SetState(4);
		}
	}

	public override void Initialize(FishingZone controller, int nStateId)
	{
		base.Initialize(controller, nStateId);
	}
}
