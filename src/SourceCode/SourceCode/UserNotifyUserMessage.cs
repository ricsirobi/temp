public class UserNotifyUserMessage : UserNotify
{
	public override void OnWaitBeginImpl()
	{
		WsUserMessage.ShowMessage(base.gameObject);
	}

	private void OnNotifyUIClosed()
	{
		OnWaitEnd();
	}
}
