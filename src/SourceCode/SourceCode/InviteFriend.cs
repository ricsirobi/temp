using UnityEngine;

public class InviteFriend
{
	private static InviteFriendEventHandler mEventDelegate;

	private static GameObject mMessageObject;

	public static bool pUpdateUserMessageObj;

	public static void SendInvite(string inviter, string[] emailIDs, InviteFriendEventHandler inCallBack, Object inUserData)
	{
		mEventDelegate = inCallBack;
		WsWebService.SendFriendInvite(inviter, emailIDs, InviteFriendResponse, inUserData);
	}

	public static void InviteFriendResponse(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType == WsServiceType.SEND_FRIEND_INVITE && mEventDelegate != null)
		{
			mEventDelegate(inType, inEvent, inObject);
		}
	}

	public static void PopUpInviteFriend(GameObject inMessageObject)
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		mMessageObject = inMessageObject;
		string[] array = GameConfig.GetKeyData("InviteFriendAsset").Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnInviteFriendBundleReady, typeof(GameObject));
	}

	private static void OnInviteFriendBundleReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			Object.Instantiate(inObject as GameObject);
			OnDBProcessDone();
			break;
		case RsResourceLoadEvent.ERROR:
			OnDBProcessDone();
			break;
		}
	}

	private static void OnDBProcessDone()
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (mMessageObject != null)
		{
			mMessageObject.SendMessage("OnInviteFriendLoaded", SendMessageOptions.DontRequireReceiver);
		}
	}

	public static void OnInviteFriendClosed()
	{
		if (mMessageObject != null)
		{
			mMessageObject.SendMessage("OnInviteFriendClosed", SendMessageOptions.DontRequireReceiver);
		}
	}
}
