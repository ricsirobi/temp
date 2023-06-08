public class UIMessageBoardPostFriendListMenu : KAUIDropDownMenu
{
	public UiMessageBoardPost pMessageBoardPost { get; set; }

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == _Template.name)
		{
			MessageBoardFriendUserData messageBoardFriendUserData = (MessageBoardFriendUserData)inWidget.GetUserData();
			if (messageBoardFriendUserData != null)
			{
				pMessageBoardPost.OnFriendNameSelected(messageBoardFriendUserData);
			}
		}
	}
}
