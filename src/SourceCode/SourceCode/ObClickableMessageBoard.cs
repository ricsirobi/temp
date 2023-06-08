public class ObClickableMessageBoard : ObClickable
{
	public override void OnActivate()
	{
		base.OnActivate();
		MessageBoardLoader.Load(UserInfo.pInstance.UserID, -1, CombinedMessageType.NONE, -1, UILoadOptions.AUTO, isMessageBoardUI: true);
	}
}
