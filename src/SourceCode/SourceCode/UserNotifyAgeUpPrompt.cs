public class UserNotifyAgeUpPrompt : UserNotify
{
	public override void OnWaitBeginImpl()
	{
		if (!DragonAgeUpConfig.Trigger(RsResourceManager.pLastLevel, OnDragonAgeUpDone))
		{
			OnDragonAgeUpDone();
			return;
		}
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.pState = AvAvatarState.PAUSED;
	}

	private void OnDragonAgeUpDone()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		OnWaitEnd();
	}
}
