public class UserNotifyPushNotifications : UserNotify
{
	public override void OnWaitBeginImpl()
	{
		_ = FUEManager.pIsFUERunning;
		TriggerOnWaitEnd();
	}

	private void TriggerOnWaitEnd()
	{
		base.OnWaitEnd();
	}
}
