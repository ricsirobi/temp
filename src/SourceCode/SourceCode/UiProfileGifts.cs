using System.Collections.Generic;
using UnityEngine;

public class UiProfileGifts : KAUI
{
	public string _UiGiftMessageURL;

	public TagAndDefaultText[] _NoNameTagDefaultText;

	private List<MessageInfo> mGiftMessages = new List<MessageInfo>();

	private UiProfileGiftsMenu mMenu;

	private KAWidget mTxtGiftsCount;

	private AvPhotoManager mPhotoManager;

	protected override void Start()
	{
		base.Start();
		mMenu = base.gameObject.GetComponent<UiProfileGiftsMenu>();
		mTxtGiftsCount = FindItem("TxtGiftsCount");
		mTxtGiftsCount.SetText("0");
	}

	private void UpdateGiftsCount()
	{
		mTxtGiftsCount.SetText(mGiftMessages.Count.ToString());
	}

	public void RemoveGiftMessage(MessageInfo messageInfo)
	{
		if (mGiftMessages.Contains(messageInfo))
		{
			mGiftMessages.Remove(messageInfo);
			PopulateGiftMessage();
		}
	}

	public void OnOpenUI(string uid)
	{
		base.enabled = UserInfo.pInstance.UserID.Equals(uid);
		if (base.enabled)
		{
			mPhotoManager = AvPhotoManager.Init("PfMessagePhotoMgr");
			WsWebService.GetUserMessageQueue(showOld: true, showDeleted: false, GetMessageQueueServiceEventHandler, null);
		}
	}

	public void OnSetVisibility(bool t)
	{
		SetVisibility(t);
	}

	public virtual void EnableEdit(bool t)
	{
		SetInteractive(t);
	}

	private void GetMessageQueueServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType != WsServiceType.GET_USER_MESSAGE_QUEUE)
		{
			return;
		}
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			ArrayOfMessageInfo arrayOfMessageInfo = (ArrayOfMessageInfo)inObject;
			if (arrayOfMessageInfo == null || arrayOfMessageInfo.MessageInfo == null || arrayOfMessageInfo.MessageInfo.Length == 0)
			{
				break;
			}
			MessageInfo[] messageInfo = arrayOfMessageInfo.MessageInfo;
			foreach (MessageInfo messageInfo2 in messageInfo)
			{
				if (messageInfo2.MessageTypeID == 19)
				{
					mGiftMessages.Add(messageInfo2);
				}
			}
			PopulateGiftMessage();
			break;
		}
		case WsServiceEvent.ERROR:
			UtDebug.Log("Error!!! downloading user message");
			break;
		}
	}

	private void PopulateGiftMessage()
	{
		mMenu.ClearItems();
		foreach (MessageInfo mGiftMessage in mGiftMessages)
		{
			KAWidget kAWidget = DuplicateWidget("MenuTemplate");
			kAWidget.SetVisibility(inVisible: true);
			mMenu.AddWidget(kAWidget);
			UiMessageInfoUserData userData = new UiMessageInfoUserData(mGiftMessage, mPhotoManager, _NoNameTagDefaultText);
			kAWidget.SetUserData(userData);
		}
		UpdateGiftsCount();
	}

	public void ProfileAvPhotoCallback(Texture tex, object inUserData)
	{
		((KAWidget)inUserData).FindChildItem("FriendsIcon").SetTexture((Texture2D)tex);
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		UiMessageInfoUserData uiMessageInfoUserData = (UiMessageInfoUserData)item.GetUserData();
		if (uiMessageInfoUserData != null && uiMessageInfoUserData.GetMessageInfo() != null)
		{
			if (UiProfile.pInstance != null)
			{
				UiProfile.pInstance.gameObject.BroadcastMessage("SetDisabled", true, SendMessageOptions.DontRequireReceiver);
			}
			string[] array = _UiGiftMessageURL.Split('/');
			KAUICursorManager.SetDefaultCursor("Loading");
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], LoadGiftMessageDB, typeof(GameObject));
		}
	}

	public void LoadGiftMessageDB(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (inObject == null && UiProfile.pInstance != null)
			{
				UiProfile.pInstance.gameObject.BroadcastMessage("SetDisabled", false, SendMessageOptions.DontRequireReceiver);
			}
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		case RsResourceLoadEvent.ERROR:
			if (UiProfile.pInstance != null)
			{
				UiProfile.pInstance.gameObject.BroadcastMessage("SetDisabled", false, SendMessageOptions.DontRequireReceiver);
			}
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}
}
