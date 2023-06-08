using JSGames;
using UnityEngine;

public class UiClansMessageBoard : UiMessageBoard
{
	public KAWidget _PostNewsBtn;

	public string _NewsMessageTemplate = "MessageNews";

	public LocaleString _NewsTabNoMessageText = new LocaleString("No News Messages To Display");

	private MessageListType mMessageListType = MessageListType.MessageBoard;

	private Group mClan;

	private bool mWasVisible = true;

	public override void OnOpenUI(string userID)
	{
		UiMessageBoard.mUserID = userID;
		mIsMyBoard = UiMessageBoard.mUserID == UserInfo.pInstance.UserID;
		if (UiProfile.pUserProfile == null || UiProfile.pUserProfile.UserID != UiMessageBoard.mUserID)
		{
			UiProfile.pUserProfile = UserProfile.LoadUserProfile(UiMessageBoard.mUserID);
		}
		Init();
		mWasVisible = GetVisibility();
	}

	public void OnClan(Group inGroup)
	{
		if (inGroup != null)
		{
			mClan = inGroup;
			pMessageBoard = MessageListInstance.Load(mClan.GroupID, _MyMessageFilterName, base.OnMessageListReady);
			mCombinedMessageType = CombinedMessageType.NONE;
			mLoading = true;
			SetNoMessageTextVisible(isVisible: false);
			bool flag = false;
			if (UserProfile.pProfileData.InGroup(mClan.GroupID) && mClan.HasPermission((UserRole)UserProfile.pProfileData.Groups[0].RoleID.Value, "Post News"))
			{
				flag = true;
			}
			_PostNewsBtn.SetDisabled(!flag);
		}
	}

	public override void OnSetVisibility(bool visible)
	{
		mShowBoard = (visible ? mWasVisible : visible);
		base.OnSetVisibility(mShowBoard);
	}

	protected override void InitMessageLists()
	{
		mCombinedListMessages.Clear();
		if (pMessageBoard.mCombinedMessages == null)
		{
			UtDebug.Log("Combined Message list is null");
			return;
		}
		CombinedListMessage[] mCombinedMessages = pMessageBoard.mCombinedMessages;
		foreach (CombinedListMessage combinedListMessage in mCombinedMessages)
		{
			switch ((CombinedMessageType)combinedListMessage.MessageType)
			{
			case CombinedMessageType.MESSAGE_BOARD:
				if (UtUtilities.DeserializeFromXml(combinedListMessage.MessageBody, typeof(Message)) is Message message)
				{
					if (!message.ReplyToMessageID.HasValue)
					{
						mCombinedListMessages.Add(message);
						if ((bool)base.pMessageThread)
						{
							base.pMessageThread.AddConversation(message.MessageID.Value, message);
						}
					}
					else if ((bool)base.pMessageThread)
					{
						base.pMessageThread.AddConversation(message.ReplyToMessageID.Value, message);
					}
				}
				else
				{
					UtDebug.LogError("Message is null (failed to deserialize)");
				}
				break;
			case CombinedMessageType.NEWS:
				if (UtUtilities.DeserializeFromXml(combinedListMessage.MessageBody, typeof(Message)) is Message message2)
				{
					if (!message2.ReplyToMessageID.HasValue)
					{
						mCombinedListMessages.Add(message2);
						if ((bool)base.pMessageThread)
						{
							base.pMessageThread.AddConversation(message2.MessageID.Value, message2);
						}
					}
					else if ((bool)base.pMessageThread)
					{
						base.pMessageThread.AddConversation(message2.ReplyToMessageID.Value, message2);
					}
				}
				else
				{
					UtDebug.LogError("Message is null (failed to deserialize)");
				}
				break;
			case CombinedMessageType.USER_MESSAGE_QUEUE:
				if (UtUtilities.DeserializeFromXml(combinedListMessage.MessageBody, typeof(MessageInfo)) is MessageInfo messageInfo && messageInfo.MessageID.HasValue)
				{
					if (messageInfo.MessageTypeID.Value == 30)
					{
						messageInfo.CreateDate = combinedListMessage.MessageDate;
						mCombinedListMessages.Add(messageInfo);
					}
				}
				else
				{
					Debug.LogError("MessgeInfo is null (failed to deserialize)");
				}
				break;
			}
		}
	}

	public override void ProcessMessageType(CombinedMessageType type, int messageInfoType)
	{
		if (type == CombinedMessageType.USER_MESSAGE_QUEUE && messageInfoType != mMessageInfoType)
		{
			mCombinedMessageType = CombinedMessageType.NONE;
		}
		mMessageInfoType = messageInfoType;
		ProcessMessageType(type);
	}

	public override void ProcessMessageType(CombinedMessageType type, bool force = false)
	{
		base.ProcessMessageType(type, force);
		switch (mCombinedMessageType)
		{
		case CombinedMessageType.ALL:
			_PostBtn.SetVisibility(inVisible: true);
			_PostNewsBtn.SetVisibility(inVisible: false);
			mMessageListType = MessageListType.MessageBoard;
			break;
		case CombinedMessageType.NEWS:
			_PostBtn.SetVisibility(inVisible: false);
			_PostNewsBtn.SetVisibility(inVisible: true);
			mMessageListType = MessageListType.News;
			AddMessageBoardMessages(MessageType.News);
			break;
		}
	}

	public override void Init()
	{
		if (_Menu != null)
		{
			_Menu.ClearItems();
		}
	}

	public override void Click(KAWidget item)
	{
		KAWidget parentItem = item.GetParentItem();
		string text = ((parentItem != null) ? parentItem.name : "");
		mSelectedItem = ((parentItem != null) ? parentItem : item);
		if (item == _PostBtn || item == _PostNewsBtn)
		{
			if (UiLogin.pIsGuestUser)
			{
				OpenRegisterNowUI();
			}
			else
			{
				_PostUI.SetVisibility(UiMessageBoard.mUserID, mClan.Name, -1);
			}
		}
		else if (text == _NewsMessageTemplate)
		{
			ClickPlayerMessage(item);
		}
		else
		{
			base.Click(item);
		}
	}

	public override KAWidget AddMessage(Message message)
	{
		string text = "";
		text = ((message.MessageType != MessageType.News) ? _PlayerMessageTemplate : _NewsMessageTemplate);
		return AddMessage(text, message);
	}

	public override void SetMessageItemButtons(KAWidget item)
	{
		MessageUserData messageUserData = (MessageUserData)item.GetUserData();
		if (item.FindChildItem("DeleteBtn") != null)
		{
			if (messageUserData._Data.Creator == UserInfo.pInstance.UserID || (UserProfile.pProfileData.InGroup(mClan.GroupID) && ((mMessageListType == MessageListType.MessageBoard && mClan.HasPermission((UserRole)UserProfile.pProfileData.Groups[0].RoleID.Value, "Delete Any Msg")) || (mMessageListType == MessageListType.News && mClan.HasPermission((UserRole)UserProfile.pProfileData.Groups[0].RoleID.Value, "Delete News")))))
			{
				item.FindChildItem("DeleteBtn").SetVisibility(inVisible: true);
			}
			else
			{
				item.FindChildItem("DeleteBtn").SetVisibility(inVisible: false);
			}
		}
		BuddyStatus buddyStatus = BuddyStatus.Unknown;
		if (BuddyList.pIsReady)
		{
			buddyStatus = BuddyList.pInstance.GetBuddyStatus(messageUserData._Data.Creator);
		}
		if (item.FindChildItem("IgnoreBtn") != null && (messageUserData._Data.Creator == UserInfo.pInstance.UserID || buddyStatus == BuddyStatus.BlockedBySelf || buddyStatus == BuddyStatus.BlockedByBoth))
		{
			item.FindChildItem("IgnoreBtn").SetState(KAUIState.DISABLED);
		}
		if (messageUserData._Data.Creator == UserInfo.pInstance.UserID)
		{
			item.FindChildItem("ModeratorBtn").SetState(KAUIState.DISABLED);
		}
	}

	protected override bool CanShowReplyDeleteButton(Message inMessage)
	{
		if (!(inMessage.Creator == UserInfo.pInstance.UserID))
		{
			if (UserProfile.pProfileData.InGroup(mClan.GroupID))
			{
				return mClan.HasPermission((UserRole)UserProfile.pProfileData.Groups[0].RoleID.Value, "Delete Any Msg");
			}
			return false;
		}
		return true;
	}

	public override void PostMessage(string userID, string message, MessageLevel level, string attribute, bool isPrivate = false)
	{
		WsWebService.PostNewGroupMessageToList(mClan.GroupID, message, level, (int)mMessageListType, attribute, base.EventHandler, null);
	}

	public override void ReplyToMessage(string userID, int messageID, string message, MessageLevel level, string attribute, bool isPrivate = false)
	{
		WsWebService.PostGroupMessageReply(mClan.GroupID, message, level, messageID, attribute, base.EventHandler, null);
	}

	public override void OnPost(string userID, Message message, int replyToMessageID)
	{
		if (mMessageListType == MessageListType.News)
		{
			message.MessageType = MessageType.News;
		}
		base.OnPost(UiMessageBoard.mUserID, message, replyToMessageID);
	}

	protected override void SetNoMessageTextVisible(bool isVisible)
	{
		if (!(_NoMessageTxtItem != null))
		{
			return;
		}
		if (isVisible)
		{
			switch (mCombinedMessageType)
			{
			case CombinedMessageType.NEWS:
				noMessageText = _NewsTabNoMessageText;
				break;
			case CombinedMessageType.ALL:
				noMessageText = _AllTabNoMessageText;
				break;
			}
			if (noMessageText != null)
			{
				_NoMessageTxtItem.SetText(noMessageText.GetLocalizedString());
			}
		}
		_NoMessageTxtItem.SetVisibility(isVisible);
	}
}
