namespace JSGames.UI.TerrorMail;

public class MessageBoardFriendUserData
{
	public string pUserID { get; private set; }

	public string pUserName { get; private set; }

	public MessageBoardFriendUserData(string userID, string userName)
	{
		pUserID = userID;
		pUserName = userName;
	}
}
