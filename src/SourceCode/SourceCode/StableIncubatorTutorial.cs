public class StableIncubatorTutorial : InteractiveTutManager
{
	public ObTrigger _HatcheryPortal;

	public override void Start()
	{
		base.Start();
		ObClickable.pGlobalActive = false;
		if (_HatcheryPortal != null)
		{
			_HatcheryPortal._UseGlobalActive = false;
		}
	}

	public override void Exit()
	{
		AvAvatar.EnableAllInputs(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		UserNotifyStableTutorial.pInstance.SetToWaitState(flag: false);
		UserNotifyStableTutorial.pInstance.CheckForNextTut();
		if (_HatcheryPortal != null)
		{
			_HatcheryPortal._UseGlobalActive = true;
		}
		base.Exit();
	}
}
