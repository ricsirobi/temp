public class ChatData
{
	private static ChatOptions mCurrentChatOption;

	public static bool pIsPublicChat => mCurrentChatOption == ChatOptions.CHAT_ROOM;

	public static ChatOptions pCurrentChatOption
	{
		get
		{
			return mCurrentChatOption;
		}
		set
		{
			mCurrentChatOption = value;
		}
	}
}
