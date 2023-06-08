using System.Collections;
using JSGames.UI.TerrorMail;
using UnityEngine;

public class MessageBoardLoader : KAMonoBase
{
	private static string mUserID;

	private static CombinedMessageType mMessageType;

	private static int mMessageInfoType;

	private static int mMessageID;

	private static string mPreviousLevel;

	public static void Load(string inUserID, int messageID = -1, CombinedMessageType messageType = CombinedMessageType.NONE, int messageInfoType = -1, UILoadOptions inLoadOption = UILoadOptions.AUTO, bool isMessageBoardUI = false)
	{
		if (UtMobileUtilities.CanLoadInCurrentScene(UiType.MessageBoard, inLoadOption) || RsResourceManager.pCurrentLevel == GameConfig.GetKeyData("MessageBoardScene"))
		{
			UITerrorMail.Show(inUserID, messageType, messageInfoType, messageID);
			return;
		}
		AvAvatar.SetActive(inActive: false);
		AvAvatar.SetStartPositionAndRotation();
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.Disconnect();
		}
		mUserID = inUserID;
		mMessageType = messageType;
		mMessageInfoType = messageInfoType;
		mMessageID = messageID;
		mPreviousLevel = RsResourceManager.pCurrentLevel;
		RsResourceManager.LoadLevel(GameConfig.GetKeyData("MessageBoardScene"));
	}

	private IEnumerator Start()
	{
		while (RsResourceManager.pLevelLoading)
		{
			yield return new WaitForEndOfFrame();
		}
		RsResourceManager.DestroyLoadScreen();
		UITerrorMail.Show(mUserID, mMessageType, mMessageInfoType, mMessageID, mPreviousLevel);
		yield return null;
	}
}
