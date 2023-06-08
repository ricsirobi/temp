using System.Collections.Generic;
using UnityEngine;

public class UiMessageThread : KAUI
{
	public UiMessageBoard _Board;

	public UiMessageThreadMenu _Menu;

	public Color _MaskColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	public string _DateTimeFormat = "MMM dd, yyyy hh:mm tt";

	private KAWidget mCloseBtn;

	private string mUserID;

	private int mMessageID;

	private AvPhotoManager mPhotoManager;

	private bool mLoadingConversation;

	public Dictionary<int, List<Message>> mConversations = new Dictionary<int, List<Message>>();

	public Dictionary<int, ConversationDownloadReadyEvent> mConversationReadyEventList = new Dictionary<int, ConversationDownloadReadyEvent>();

	protected override void Start()
	{
		base.Start();
		mCloseBtn = FindItem("CloseBtn");
		mPhotoManager = AvPhotoManager.Init("PfMessagePhotoMgr");
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mCloseBtn)
		{
			_Menu.ClearItems();
			SetVisibility(show: false);
			Input.ResetInputAxes();
		}
		else if (item.name == "Picture" || item.name == "TxtName")
		{
			MessageUserData obj = (MessageUserData)item.GetParentItem().GetUserData();
			_Board.Close();
			UiProfile.ShowProfile(obj._Data.Creator);
		}
	}

	public void Show(string userID, int messageID)
	{
		if (!mLoadingConversation)
		{
			mUserID = userID;
			mMessageID = messageID;
			SetVisibility(show: true);
			_Menu.ClearItems();
			LoadConversation(mMessageID, OnConversationReady, null);
		}
	}

	public void AddConversation(int messageID, Message message)
	{
		AddConversation(messageID, message, addLast: false);
	}

	public void RemoveConversation(int messageID, Message message)
	{
		if (mConversations.ContainsKey(messageID))
		{
			mConversations[messageID].Remove(message);
		}
	}

	public void AddConversation(int messageID, Message message, bool addLast)
	{
		if (!mConversations.ContainsKey(messageID))
		{
			mConversations[messageID] = new List<Message>();
		}
		if (addLast)
		{
			mConversations[messageID].Add(message);
		}
		else
		{
			mConversations[messageID].Insert(0, message);
		}
	}

	public void ClearConversation()
	{
		mConversations.Clear();
	}

	public void LoadConversation(int messageID, ConversationDownloadReadyEvent callBackEvent, KAWidget parentItem)
	{
		mConversationReadyEventList[messageID] = callBackEvent;
		if (mConversations.ContainsKey(messageID) && mConversationReadyEventList.ContainsKey(messageID))
		{
			mConversationReadyEventList[messageID](mConversations[messageID], parentItem);
		}
	}

	public override void SetVisibility(bool show)
	{
		if (show)
		{
			KAUI.SetExclusive(this, _MaskColor);
		}
		else if (GetVisibility())
		{
			KAUI.RemoveExclusive(this);
		}
		base.SetVisibility(show);
		if (_Menu != null)
		{
			_Menu.SetVisibility(show);
		}
		if (_Board != null)
		{
			_Board.SetInteractive(!show);
		}
	}

	public KAWidget CreateThreadMessage(Message message, KAWidget duplicateItem, bool takePhoto)
	{
		if (takePhoto)
		{
			new UiAvatarItem(message.Creator, mPhotoManager, duplicateItem, _Board).TakePhoto();
		}
		KAWidget kAWidget = duplicateItem.FindChildItem("TxtMessage");
		if (BuddyList.pIsReady && BuddyList.pInstance.GetBuddyStatus(message.Creator) == BuddyStatus.BlockedBySelf && message.Creator != mUserID)
		{
			duplicateItem.FindChildItem("IgnoredIcon").SetVisibility(inVisible: true);
		}
		else
		{
			kAWidget.SetText(message.Content);
		}
		duplicateItem.FindChildItem("TxtDate").SetText(message.CreateTime.ToLocalTime().ToString(_DateTimeFormat));
		KAWidget kAWidget2 = duplicateItem.FindChildItem("ModeratorBtn");
		if (kAWidget2 != null)
		{
			kAWidget2.SetDisabled(message.Creator == UserInfo.pInstance.UserID);
		}
		MessageUserData messageUserData = new MessageUserData();
		messageUserData._Data = message;
		messageUserData.pLoaded = false;
		duplicateItem.SetUserData(messageUserData);
		return duplicateItem;
	}

	private void OnConversationReady(List<Message> messages, KAWidget parentItem)
	{
		SetInteractive(interactive: true);
		if (messages == null)
		{
			return;
		}
		int num = 0;
		foreach (Message message in messages)
		{
			KAWidget kAWidget = DuplicateWidget("Message");
			_Menu.AddWidget(kAWidget);
			kAWidget.SetVisibility(inVisible: true);
			CreateThreadMessage(message, kAWidget, takePhoto: true);
			if (message.MessageID.Value == mMessageID)
			{
				num = _Menu.GetNumItems() - 1;
			}
		}
		if (_Menu.GetPageCount() == 1)
		{
			_Menu.ResetPosition();
			return;
		}
		int num2 = _Menu.GetNumItemsPerPage() / 2;
		num -= num2;
		if (num < 0)
		{
			num = 0;
		}
		else if (num + _Menu.GetNumItemsPerPage() > _Menu.GetNumItems())
		{
			num -= num + _Menu.GetNumItemsPerPage() - _Menu.GetNumItems();
		}
		_Menu.SetTopItemIdx(num);
	}
}
