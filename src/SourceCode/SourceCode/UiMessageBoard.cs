using System;
using System.Collections.Generic;
using JSGames;
using SOD.Event;
using UnityEngine;

public class UiMessageBoard : KAUI
{
	public static string ATTRIBUTE_COLOR = "C";

	protected static string mUserID = "";

	public string _UiChatRegisterNowPath;

	public MessageListInstance pMessageBoard;

	public UiMessageBoardMenu _Menu;

	public UiMessageBoardPost _PostUI;

	public UiMessageThread _ThreadUI;

	public UiMessageBoardTabMenu _TabUI;

	public TextAsset _TutorialTextAsset;

	public string _Intro;

	public string _DateTimeFormat = "MMM dd, yyyy hh:mm tt";

	public GameObject _PhotoTextureObj;

	public GameObject _CameraObj;

	public LocaleString _DeleteMessageText = new LocaleString("Are you sure you want to delete this message?");

	public LocaleString _IgnorePlayerText = new LocaleString("Are you sure you'd like to ignore this person?");

	public LocaleString _GenericErrorText = new LocaleString("There was an error connecting to the server. Please try again.");

	public LocaleString _NeedPetText = new LocaleString("You need a dragon to play.");

	public LocaleString _NotInMMORoomText;

	public LocaleString _NoDisplayNameText = new LocaleString("A friend");

	public TagAndDefaultText[] _TagAndDefaultText;

	public NPCFriendData[] _NPCFriends;

	public KAWidget _CloseBtn;

	public KAWidget _PostBtn;

	public KAWidget _NoMessageTxtItem;

	public string _MyMessageFilterName = "OnlineProfile";

	public string _OthersMessageFilterName = "OnlineBuddyProfile";

	public string _PlayerMessageTemplate = "Message";

	public string _ChallengeMessageTemplate = "MessageChallenge";

	public string _MuttCareMessageTemplate = "MessageMuttCare";

	public string _GiftMessageTemplate = "MessageGift";

	public string _AnnouncementMessageTemplate = "MessageAnnouncement";

	public string _MessageThreadTemplate = "MessageReply";

	public string _AchievementMessageTemplate = "MessageAchievement";

	public string _SocialMessageTemplate = "MessageBuddy";

	public string _PrivateMessageTemplate = "MessagePrivate";

	public int _AcceptChallengeAchievementID = 133;

	public ChallengeInviteMessageData[] _ChallengeMessage;

	public LocaleString _DaysLeftText;

	public LocaleString _HoursLeftText;

	public LocaleString _MinutesLeftText;

	public LocaleString _SecondsLeftText;

	public LocaleString _OneSecondLeftText;

	public LocaleString _AllTabNoMessageText = new LocaleString("No Messages To Display");

	public LocaleString _MessageTabNoMessageText = new LocaleString("No Player Messages To Display");

	public LocaleString _MuttCareTabNoMessageText = new LocaleString("No Mutt Care Messages To Display");

	public LocaleString _ChallengeTabNoMessageText = new LocaleString("No Challenge Messages To Display");

	public LocaleString _GiftTabNoMessageText = new LocaleString("No Gift Messages To Display");

	public LocaleString _SocialTabNoMessageText = new LocaleString("No Social Messages To Display");

	public LocaleString _AchievementTabNoMessageText = new LocaleString("No Achievement Messages To Display");

	public string _UiGiftMessageURL;

	public int _MaxReplyToShow = 2;

	public bool _TakePhoto;

	public Color _MaskColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	private AvPhotoManager mStillPhotoManager;

	private bool mLoadProfileOnDestroy;

	private string mProfileToLoadOnDestroy = "";

	protected KAWidget mPortraitFrame;

	protected KAWidget mPlayerInfo;

	protected bool mLoading = true;

	protected bool mClanReady;

	protected KAUIGenericDB mKAUIGenericDB;

	protected KAWidget mSelectedItem;

	protected LocaleString noMessageText;

	protected List<object> mCombinedListMessages = new List<object>();

	private List<MessageInfo> mRemovedGiftMessages = new List<MessageInfo>();

	private KAUIGenericDB mUiGenericDB;

	[SerializeField]
	private LocaleString m_GiftMessageDeleteConfirmationText = new LocaleString("[Review] Are you sure you want to delete this message with unopened gifts?");

	protected CombinedMessageType mCombinedMessageType;

	protected int mMessageInfoType;

	protected bool mShowBoard;

	protected bool mIsMyBoard;

	private bool mBuddyCheckApplied;

	protected Dictionary<KAWidget, List<KAWidget>> mThreadItems = new Dictionary<KAWidget, List<KAWidget>>();

	public static int pMessageIDToBeShown = -1;

	public static int pMessageInfoType = -1;

	public static CombinedMessageType pMessageType;

	public static string pUserID => mUserID;

	public bool pShowPrivateMessages { get; set; }

	public UiMessageThread pMessageThread => _ThreadUI;

	public static void ShowBoard()
	{
		ShowBoard(UserInfo.pInstance.UserID);
	}

	public virtual void OnOpenUI(string userID)
	{
		mUserID = userID;
		mIsMyBoard = mUserID == UserInfo.pInstance.UserID;
		pMessageBoard = MessageListInstance.Load(userID, mIsMyBoard ? _MyMessageFilterName : _OthersMessageFilterName, OnMessageListReady);
		Init();
	}

	public virtual void OnCloseUI()
	{
		if (pMessageBoard != null)
		{
			pMessageBoard.Unload();
		}
		pMessageBoard = null;
		pMessageIDToBeShown = -1;
	}

	public static void ShowBoard(string userID, int messageIDToShow = -1)
	{
		UiProfile.pUserProfile = UserProfile.LoadUserProfile(userID);
		LoadMessageBoardBundle(GameConfig.GetKeyData("MessageBoardAsset"));
		pMessageIDToBeShown = messageIDToShow;
	}

	public static void LoadMessageBoardBundle(string bResName)
	{
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.pState = AvAvatarState.PAUSED;
		KAUICursorManager.SetDefaultCursor("Loading");
		string[] array = bResName.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnAssetLoadingEvent, typeof(GameObject));
	}

	public static void OnAssetLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			UnityEngine.Object.Instantiate((GameObject)inObject).name = "PfUiMessageBoard";
			RsResourceManager.ReleaseBundleData(inURL);
			break;
		case RsResourceLoadEvent.ERROR:
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	protected void OpenRegisterNowUI()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		SetInteractive(interactive: false);
		RsResourceManager.LoadAssetFromBundle(_UiChatRegisterNowPath, OnRegisterNowBundleLoaded, typeof(GameObject), inDontDestroy: false, typeof(GameObject));
	}

	private void OnRegisterNowBundleLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			UnityEngine.Object.Instantiate((GameObject)inObject).name = ((GameObject)inObject).name;
			KAUICursorManager.SetDefaultCursor("Arrow");
			SetInteractive(interactive: true);
			break;
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("Error loading Event Intro Prefab!" + inURL);
			KAUICursorManager.SetDefaultCursor("Arrow");
			SetInteractive(interactive: true);
			break;
		}
	}

	public void OnClan()
	{
		mClanReady = true;
	}

	public void OnClanFailed()
	{
		mClanReady = true;
	}

	public virtual void OnSetVisibility(bool visible)
	{
		if (visible && pMessageBoard != null)
		{
			base.gameObject.BroadcastMessage("MessageBoardInitialized", mUserID, SendMessageOptions.DontRequireReceiver);
			if (pMessageBoard.pIsReady)
			{
				ProcessMessageType(CombinedMessageType.ALL);
				UiToolbar.pMessageCount = 0;
				if (pMessageIDToBeShown != -1)
				{
					SetTopMessage(pMessageIDToBeShown);
				}
			}
			else
			{
				KAUICursorManager.SetDefaultCursor("Loading");
			}
		}
		SetVisibility(visible);
		mShowBoard = visible;
	}

	public void OnMessageListReady()
	{
		InitMessageLists();
	}

	protected virtual void SetNoMessageTextVisible(bool visible)
	{
		if (!(_NoMessageTxtItem != null))
		{
			return;
		}
		if (visible)
		{
			noMessageText = _AllTabNoMessageText;
			switch (mCombinedMessageType)
			{
			case CombinedMessageType.MESSAGE_BOARD:
				noMessageText = _MessageTabNoMessageText;
				break;
			case CombinedMessageType.USER_MESSAGE_QUEUE:
				if (mMessageInfoType == 12)
				{
					noMessageText = _MuttCareTabNoMessageText;
				}
				else if (mMessageInfoType == 19)
				{
					noMessageText = _GiftTabNoMessageText;
				}
				else if (mMessageInfoType == 9)
				{
					noMessageText = _AchievementTabNoMessageText;
				}
				else if (mMessageInfoType == 11)
				{
					noMessageText = _SocialTabNoMessageText;
				}
				break;
			case CombinedMessageType.CHALLENGE:
				noMessageText = _ChallengeTabNoMessageText;
				break;
			}
			if (noMessageText != null)
			{
				_NoMessageTxtItem.SetText(noMessageText.GetLocalizedString());
			}
		}
		_NoMessageTxtItem.SetVisibility(visible);
	}

	protected virtual void InitMessageLists()
	{
		mCombinedListMessages.Clear();
		pMessageThread.ClearConversation();
		if (pMessageBoard == null || pMessageBoard.mCombinedMessages == null)
		{
			UtDebug.Log("Combined Message list is null");
			return;
		}
		CombinedListMessage[] mCombinedMessages = pMessageBoard.mCombinedMessages;
		foreach (CombinedListMessage combinedListMessage in mCombinedMessages)
		{
			switch ((CombinedMessageType)combinedListMessage.MessageType)
			{
			case CombinedMessageType.CHALLENGE:
				if (UtUtilities.DeserializeFromXml(combinedListMessage.MessageBody, typeof(ChallengeInfo)) is ChallengeInfo item)
				{
					mCombinedListMessages.Add(item);
				}
				else
				{
					UtDebug.LogError("ChallengeInfo is null (failed to deserialize)");
				}
				break;
			case CombinedMessageType.MESSAGE_BOARD:
				if (UtUtilities.DeserializeFromXml(combinedListMessage.MessageBody, typeof(Message)) is Message message)
				{
					if (!message.ReplyToMessageID.HasValue)
					{
						mCombinedListMessages.Add(message);
						if ((bool)pMessageThread)
						{
							pMessageThread.AddConversation(message.MessageID.Value, message);
						}
					}
					else if ((bool)pMessageThread)
					{
						pMessageThread.AddConversation(message.ReplyToMessageID.Value, message);
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
					int value = messageInfo.MessageTypeID.Value;
					if (value == 12 || value == 19 || value == 9 || value == 11 || value == 4 || value == 21)
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
			case CombinedMessageType.ANNOUNCEMENT:
				if (UtUtilities.DeserializeFromXml(combinedListMessage.MessageBody, typeof(Announcement)) is Announcement announcement)
				{
					TaggedAnnouncementHelper taggedAnnouncementHelper = new TaggedAnnouncementHelper(announcement.AnnouncementText);
					string key = "Message";
					if (taggedAnnouncementHelper.Announcement.ContainsKey(key))
					{
						mCombinedListMessages.Add(announcement);
					}
				}
				else
				{
					UtDebug.LogError("Announcement is null (failed to deserialize)");
				}
				break;
			}
		}
	}

	public void RemoveFromList(CombinedMessageType type, object listItem)
	{
		if (type == CombinedMessageType.USER_MESSAGE_QUEUE)
		{
			MessageInfo mInfo = (MessageInfo)listItem;
			RemoveMessage(mInfo);
		}
		mCombinedListMessages.Remove(listItem);
		int count = mCombinedListMessages.Count;
		if (mCombinedMessageType != CombinedMessageType.ALL)
		{
			switch (mCombinedMessageType)
			{
			case CombinedMessageType.MESSAGE_BOARD:
				count = GetMessages<Message>().Count;
				break;
			case CombinedMessageType.USER_MESSAGE_QUEUE:
				count = GetMessageByInfoType(GetMessages<MessageInfo>(), mMessageInfoType).Count;
				break;
			case CombinedMessageType.CHALLENGE:
				count = GetMessages<ChallengeInfo>().Count;
				break;
			}
		}
		if (count == 0)
		{
			SetNoMessageTextVisible(visible: true);
		}
	}

	public virtual void ProcessMessageType(CombinedMessageType type, int messageInfoType)
	{
		if (type == CombinedMessageType.USER_MESSAGE_QUEUE && messageInfoType != mMessageInfoType)
		{
			mCombinedMessageType = CombinedMessageType.NONE;
		}
		mMessageInfoType = messageInfoType;
		ProcessMessageType(type);
	}

	public virtual void ProcessMessageType(CombinedMessageType type, bool force = false)
	{
		if (type == mCombinedMessageType && !force)
		{
			return;
		}
		SetNoMessageTextVisible(visible: false);
		_Menu.ClearItems();
		mCombinedMessageType = type;
		_PostBtn.SetVisibility(mCombinedMessageType != CombinedMessageType.CHALLENGE && (mCombinedMessageType != CombinedMessageType.USER_MESSAGE_QUEUE || mMessageInfoType != 19));
		switch (mCombinedMessageType)
		{
		case CombinedMessageType.ALL:
			AddAllMessages();
			break;
		case CombinedMessageType.MESSAGE_BOARD:
			AddMessageBoardMessages();
			break;
		case CombinedMessageType.CHALLENGE:
			AddChallengeMessages();
			break;
		case CombinedMessageType.USER_MESSAGE_QUEUE:
			if (mMessageInfoType == 12)
			{
				AddMuttMessages();
			}
			else if (mMessageInfoType == 19)
			{
				AddGiftMessages();
			}
			else if (mMessageInfoType == 9)
			{
				AddAchievementMessages();
			}
			else if (mMessageInfoType == 11)
			{
				AddSocialMessages();
			}
			break;
		}
		RemoveExpiredGiftMessages();
	}

	protected List<TYPE> GetMessages<TYPE>()
	{
		List<TYPE> list = new List<TYPE>();
		foreach (object mCombinedListMessage in mCombinedListMessages)
		{
			if (mCombinedListMessage.GetType() == typeof(TYPE))
			{
				list.Add((TYPE)mCombinedListMessage);
			}
		}
		return list;
	}

	private List<MessageInfo> GetMessageByInfoType(List<MessageInfo> messages, int messageInfoType)
	{
		List<MessageInfo> list = new List<MessageInfo>();
		foreach (MessageInfo message in messages)
		{
			if (message.MessageTypeID.Value == messageInfoType)
			{
				list.Add(message);
			}
		}
		return list;
	}

	private void RemoveExpiredGiftMessages()
	{
		if (mRemovedGiftMessages == null || mRemovedGiftMessages.Count == 0)
		{
			return;
		}
		foreach (MessageInfo mRemovedGiftMessage in mRemovedGiftMessages)
		{
			RemoveFromList(CombinedMessageType.USER_MESSAGE_QUEUE, mRemovedGiftMessage);
		}
		mRemovedGiftMessages.Clear();
	}

	protected virtual void AddAllMessages()
	{
		if (mCombinedListMessages.Count == 0)
		{
			SetNoMessageTextVisible(visible: true);
			return;
		}
		foreach (object mCombinedListMessage in mCombinedListMessages)
		{
			System.Type type = mCombinedListMessage.GetType();
			if (type == typeof(Message))
			{
				Message message = (Message)mCombinedListMessage;
				KAWidget item = AddMessage(message);
				LoadConversation(message.MessageID.Value, item);
			}
			else if (type == typeof(ChallengeInfo))
			{
				ChallengeInfo challenge = (ChallengeInfo)mCombinedListMessage;
				AddChallengeItem(challenge);
			}
			else if (type == typeof(MessageInfo))
			{
				MessageInfo messageInfo = (MessageInfo)mCombinedListMessage;
				TaggedMessageHelper taggedMessageHelper = new TaggedMessageHelper(messageInfo);
				int value = messageInfo.MessageTypeID.Value;
				switch (value)
				{
				case 19:
					AddGiftItem(messageInfo);
					break;
				case 12:
					if (taggedMessageHelper.SubType == "ThankYou")
					{
						AddMuttCareItem(messageInfo, yesBtn: false, noBtn: true, giftBtn: false, giftAndGoBtn: false, okBtn: false);
					}
					else
					{
						AddMuttCareItem(messageInfo, yesBtn: true, noBtn: true, giftBtn: true, giftAndGoBtn: true, okBtn: false);
					}
					break;
				case 4:
				case 9:
				case 11:
				case 30:
				{
					string templateName = _AchievementMessageTemplate;
					if (value == 11 || taggedMessageHelper.SubType == "Social")
					{
						templateName = _SocialMessageTemplate;
					}
					AddMessageItem(messageInfo, templateName);
					break;
				}
				case 21:
					AddMessageItem(messageInfo, _PlayerMessageTemplate).name = "ThreadUpdateMessage";
					break;
				}
			}
			else if (type == typeof(Announcement))
			{
				Announcement announcement = (Announcement)mCombinedListMessage;
				AddAnnouncementItem(announcement);
			}
		}
	}

	protected virtual void AddMessageBoardMessages(MessageType type = MessageType.Post)
	{
		List<Message> list = (pShowPrivateMessages ? GetMessages<Message>().FindAll((Message x) => x.MessageType == type && x.isPrivate) : GetMessages<Message>().FindAll((Message x) => x.MessageType == type && !x.isPrivate));
		if (list == null || list.Count == 0)
		{
			SetNoMessageTextVisible(visible: true);
			return;
		}
		foreach (Message item2 in list)
		{
			if (item2.MessageType == type)
			{
				KAWidget item = AddMessage(item2);
				LoadConversation(item2.MessageID.Value, item);
			}
		}
	}

	private void AddChallengeMessages()
	{
		List<ChallengeInfo> messages = GetMessages<ChallengeInfo>();
		if (messages.Count == 0)
		{
			SetNoMessageTextVisible(visible: true);
			return;
		}
		foreach (ChallengeInfo item in messages)
		{
			AddChallengeItem(item);
		}
	}

	private void AddGiftMessages()
	{
		List<MessageInfo> messageByInfoType = GetMessageByInfoType(GetMessages<MessageInfo>(), 19);
		if (messageByInfoType.Count == 0)
		{
			SetNoMessageTextVisible(visible: true);
			return;
		}
		foreach (MessageInfo item in messageByInfoType)
		{
			AddGiftItem(item);
		}
	}

	private KAWidget AddGiftItem(MessageInfo info)
	{
		if (SetGiftMessageExpired(info))
		{
			return null;
		}
		KAWidget kAWidget = DuplicateWidget(_GiftMessageTemplate);
		kAWidget.name = _GiftMessageTemplate;
		kAWidget.SetVisibility(inVisible: true);
		AddDateTime(kAWidget, info.CreateDate);
		_Menu.AddWidget(kAWidget);
		UiMessageInfoUserData userData = new UiMessageInfoUserData(info, _TakePhoto ? mStillPhotoManager : null, _TagAndDefaultText);
		kAWidget.SetUserData(userData);
		Dictionary<string, string> dictionary = TaggedMessageHelper.Match(info.Data);
		string text = string.Empty;
		if (dictionary.ContainsKey("name"))
		{
			text = dictionary["name"];
		}
		if (!string.IsNullOrEmpty(text) && GiftManager.pIsReady)
		{
			KAWidget kAWidget2 = kAWidget.FindChildItem("OpenBtn");
			GiftData giftDataByName = GiftManager.pInstance.GetGiftDataByName(text);
			kAWidget2.SetVisibility(!GiftManager.pInstance.GetMessageTag(giftDataByName, "Claim"));
		}
		return kAWidget;
	}

	private bool SetGiftMessageExpired(MessageInfo info)
	{
		Dictionary<string, string> dictionary = TaggedMessageHelper.Match(info.Data);
		string text = string.Empty;
		if (dictionary.ContainsKey("name"))
		{
			text = dictionary["name"];
		}
		if (!string.IsNullOrEmpty(text) && GiftManager.pIsReady)
		{
			GiftData giftDataByName = GiftManager.pInstance.GetGiftDataByName(text);
			if ((giftDataByName == null || giftDataByName.pExpired) && mRemovedGiftMessages != null)
			{
				mRemovedGiftMessages.Add(info);
				return true;
			}
		}
		return false;
	}

	private void AddMuttMessages()
	{
		List<MessageInfo> messageByInfoType = GetMessageByInfoType(GetMessages<MessageInfo>(), 12);
		if (messageByInfoType.Count == 0)
		{
			SetNoMessageTextVisible(visible: true);
			return;
		}
		foreach (MessageInfo item in messageByInfoType)
		{
			if (new TaggedMessageHelper(item).SubType == "ThankYou")
			{
				AddMuttCareItem(item, yesBtn: false, noBtn: true, giftBtn: false, giftAndGoBtn: false, okBtn: false);
			}
			else
			{
				AddMuttCareItem(item, yesBtn: true, noBtn: true, giftBtn: true, giftAndGoBtn: true, okBtn: false);
			}
		}
	}

	private KAWidget AddMuttCareItem(MessageInfo info, bool yesBtn, bool noBtn, bool giftBtn, bool giftAndGoBtn, bool okBtn)
	{
		KAWidget kAWidget = DuplicateWidget(_MuttCareMessageTemplate);
		kAWidget.name = _MuttCareMessageTemplate;
		kAWidget.SetVisibility(inVisible: true);
		kAWidget.FindChildItem("YesBtn").SetVisibility(yesBtn);
		kAWidget.FindChildItem("NoBtn").SetVisibility(noBtn);
		kAWidget.FindChildItem("GiftBtn").SetVisibility(giftBtn);
		kAWidget.FindChildItem("GiftandGoBtn").SetVisibility(giftAndGoBtn);
		kAWidget.FindChildItem("OkBtn").SetVisibility(okBtn);
		AddDateTime(kAWidget, info.CreateDate);
		_Menu.AddWidget(kAWidget);
		UiMessageInfoUserData userData = new UiMessageInfoUserData(info, _TakePhoto ? mStillPhotoManager : null, _TagAndDefaultText);
		kAWidget.SetUserData(userData);
		return kAWidget;
	}

	public KAWidget AddChallengeItem(ChallengeInfo challenge)
	{
		KAWidget kAWidget = DuplicateWidget(_ChallengeMessageTemplate);
		kAWidget.name = _ChallengeMessageTemplate;
		kAWidget.SetVisibility(inVisible: true);
		_Menu.AddWidget(kAWidget);
		string challengeMessage = GetChallengeMessage(challenge.ChallengeGameInfo.GameID, challenge.Points);
		ChallengeUserData userData = new ChallengeUserData(challenge, challengeMessage, _TakePhoto ? mStillPhotoManager : null, _NoDisplayNameText);
		kAWidget.SetUserData(userData);
		return kAWidget;
	}

	public string GetChallengeMessage(int gameID, int points)
	{
		string result = null;
		ChallengeInviteMessageData challengeInviteMessageData = null;
		ChallengeInviteMessageData[] challengeMessage = _ChallengeMessage;
		foreach (ChallengeInviteMessageData challengeInviteMessageData2 in challengeMessage)
		{
			if (challengeInviteMessageData2._GameID == gameID || (challengeInviteMessageData == null && challengeInviteMessageData2._GameID == -1))
			{
				challengeInviteMessageData = challengeInviteMessageData2;
			}
		}
		if (challengeInviteMessageData != null)
		{
			result = challengeInviteMessageData._MessageText.GetLocalizedString();
			result = result.Replace("[Time]", UtUtilities.GetTimerString(points));
			result = result.Replace("[Points]", points.ToString());
			result = result.Replace("[GameName]", ChallengeInfo.GetGameTitle(gameID));
		}
		return result;
	}

	public void LoadConversation(int messageID, KAWidget item)
	{
		if ((bool)pMessageThread)
		{
			pMessageThread.LoadConversation(messageID, OnConversationReady, item);
		}
	}

	public void OnConversationReady(List<Message> messages, KAWidget parentItem)
	{
		if (parentItem == null)
		{
			return;
		}
		if (messages != null && messages.Count > 1)
		{
			int num = _Menu.FindItemIndex(parentItem) + 1;
			List<Message> list = new List<Message>(messages);
			list.RemoveAt(0);
			list.Reverse();
			DisplayReplyCount(parentItem, list.Count);
			int num2 = ((list.Count < _MaxReplyToShow) ? list.Count : _MaxReplyToShow);
			for (int i = 0; i < num2; i++)
			{
				if ((bool)pMessageThread && list[i] != null)
				{
					KAWidget kAWidget = pMessageThread.CreateThreadMessage(list[i], DuplicateWidget(_MessageThreadTemplate), _TakePhoto);
					kAWidget.FindChildItem("DeleteBtn").SetVisibility(CanShowReplyDeleteButton(list[i]));
					kAWidget.SetVisibility(inVisible: true);
					_Menu.AddWidgetAt(num, kAWidget);
					if (!mThreadItems.ContainsKey(parentItem))
					{
						mThreadItems[parentItem] = new List<KAWidget>();
					}
					mThreadItems[parentItem].Add(kAWidget);
					num++;
				}
			}
		}
		else
		{
			DisplayReplyCount(parentItem, 0);
		}
	}

	protected virtual bool CanShowReplyDeleteButton(Message inMessage)
	{
		return mIsMyBoard;
	}

	private void DisplayReplyCount(KAWidget parentItem, int count)
	{
		if (parentItem.FindChildItem("CountBkg") != null && parentItem.FindChildItem("TxtMessageCount") != null)
		{
			if (count <= 0)
			{
				parentItem.FindChildItem("CountBkg").SetVisibility(inVisible: false);
				parentItem.FindChildItem("TxtMessageCount").SetVisibility(inVisible: false);
				return;
			}
			parentItem.FindChildItem("CountBkg").SetVisibility(inVisible: true);
			KAWidget kAWidget = parentItem.FindChildItem("TxtMessageCount");
			kAWidget.SetVisibility(inVisible: true);
			kAWidget.SetText(count.ToString());
		}
	}

	private void AddAnnouncementMessages()
	{
		List<Announcement> messages = GetMessages<Announcement>();
		if (messages.Count == 0)
		{
			return;
		}
		foreach (Announcement item in messages)
		{
			AddAnnouncementItem(item);
		}
	}

	private KAWidget AddAnnouncementItem(Announcement announcement)
	{
		KAWidget kAWidget = DuplicateWidget(_AnnouncementMessageTemplate);
		kAWidget.name = _AnnouncementMessageTemplate;
		kAWidget.SetVisibility(inVisible: true);
		kAWidget.SetUserData(new AnnouncementUserData(announcement, "Message"));
		KAWidget kAWidget2 = kAWidget.FindChildItem("TxtMessage");
		if (kAWidget2 != null)
		{
			kAWidget2.SetText(announcement.Description);
		}
		_Menu.AddWidget(kAWidget);
		return kAWidget;
	}

	private void AddAchievementMessages()
	{
		List<MessageInfo> messages = GetMessages<MessageInfo>();
		bool flag = true;
		foreach (MessageInfo item in messages)
		{
			if (item.MessageTypeID.Value == 9 || item.MessageTypeID.Value == 4)
			{
				AddMessageItem(item, _AchievementMessageTemplate);
				flag = false;
			}
		}
		if (flag)
		{
			SetNoMessageTextVisible(visible: true);
		}
	}

	private void AddSocialMessages()
	{
		List<MessageInfo> messageByInfoType = GetMessageByInfoType(GetMessages<MessageInfo>(), 11);
		if (messageByInfoType.Count == 0)
		{
			SetNoMessageTextVisible(visible: true);
			return;
		}
		foreach (MessageInfo item in messageByInfoType)
		{
			AddMessageItem(item, _SocialMessageTemplate);
		}
	}

	private KAWidget AddMessageItem(MessageInfo messageInfo, string templateName)
	{
		KAWidget kAWidget = DuplicateWidget(templateName);
		kAWidget.name = templateName;
		for (int i = 0; i < kAWidget.GetNumChildren(); i++)
		{
			KAWidget kAWidget2 = kAWidget.FindChildItemAt(i);
			if (!(kAWidget2 != null))
			{
				continue;
			}
			string text = kAWidget2.name;
			switch (text)
			{
			case "DeleteBtn":
				if (mIsMyBoard)
				{
					continue;
				}
				break;
			case "TxtMessage":
			case "BkgGrp":
			case "TxtDate":
				continue;
			}
			if (!(text == "ViewBtn") || !mIsMyBoard || messageInfo.MessageTypeID.Value != 21)
			{
				kAWidget2.SetVisibility(inVisible: false);
			}
		}
		UiMessageInfoUserData uiMessageInfoUserData = new UiMessageInfoUserData(messageInfo, _TakePhoto ? mStillPhotoManager : null, _TagAndDefaultText);
		if (new TaggedMessageHelper(messageInfo).MemberMessage.ContainsKey("Line2"))
		{
			uiMessageInfoUserData.Keys = new string[1] { "Line2" };
		}
		kAWidget.SetUserData(uiMessageInfoUserData);
		AddDateTime(kAWidget, messageInfo.CreateDate);
		kAWidget.SetVisibility(inVisible: true);
		_Menu.AddWidget(kAWidget);
		return kAWidget;
	}

	private void ShowAnnouncement(AnnouncementUserData data)
	{
		if (data != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBLg"));
			mKAUIGenericDB = gameObject.GetComponent<KAUIGenericDB>();
			mKAUIGenericDB._MessageObject = base.gameObject;
			mKAUIGenericDB._CloseMessage = "OnAnnouncementDBClose";
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: true);
			KAUI.SetExclusive(mKAUIGenericDB, _MaskColor);
			mKAUIGenericDB.SetText(data._Message, !string.IsNullOrEmpty(data._MessageVOURL));
		}
	}

	public void OnAnnouncementDBText()
	{
		((AnnouncementUserData)mSelectedItem.GetUserData()).PlayVO();
	}

	public void OnAnnouncementDBClose()
	{
		KAUI.RemoveExclusive(mKAUIGenericDB);
		UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
		SetUIDisabled(isDisable: false);
		SnChannel.StopPool("VO_Pool");
	}

	protected void RemoveFromItem(KAWidget item)
	{
		if (!(item != null))
		{
			return;
		}
		int num = 0;
		if (_Menu.GetNumItems() > 0)
		{
			num = _Menu.GetTopItemIdx();
		}
		_Menu.RemoveWidget(item);
		int numItems = _Menu.GetNumItems();
		if (numItems > 0)
		{
			if (num + _Menu.GetNumItemsPerPage() > numItems)
			{
				num = numItems - _Menu.GetNumItemsPerPage();
				num = ((num >= 0) ? num : 0);
			}
			_Menu.SetTopItemIdx((num < numItems) ? num : (numItems - 1));
		}
	}

	private void RemoveMessage(MessageInfo mInfo)
	{
		if (mInfo == null)
		{
			return;
		}
		WsWebService.SaveMessage(mInfo.UserMessageQueueID.Value, isNew: false, isDeleted: true, null, null);
		if (WsUserMessage.pInstance != null)
		{
			List<MessageInfo> list = (List<MessageInfo>)WsUserMessage.pInstance.GetStoredMessages(12);
			if (list != null)
			{
				list.Remove(mInfo);
				WsUserMessage.pInstance.SetStoredMessage(12, list);
			}
		}
	}

	public void OnTutorialDone(string tid)
	{
		KAUI.SetExclusive(null, null);
		SetInteractive(interactive: true);
		_Menu.SetInteractive(interactive: true);
	}

	protected virtual void StartUp()
	{
		if (_PostBtn != null)
		{
			_PostBtn.SetVisibility(inVisible: true);
		}
		mStillPhotoManager = AvPhotoManager.Init("PfMessagePhotoMgr");
		OnOpenUI(UiProfile.pUserProfile.UserID);
		OnSetVisibility(visible: true);
		mClanReady = true;
		SetVisibility(mShowBoard);
		if (_CameraObj != null)
		{
			_CameraObj.SetActive(value: false);
		}
	}

	protected override void Start()
	{
		base.Start();
		pShowPrivateMessages = false;
		StartUp();
		_PostUI.SetVisibility(show: false);
		UiProfile._OnProfileNameChanged += RefreshUI;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		UiProfile._OnProfileNameChanged -= RefreshUI;
		if (mLoadProfileOnDestroy)
		{
			mLoadProfileOnDestroy = false;
			MessageBoardLoader.Load(mProfileToLoadOnDestroy);
		}
	}

	private void RefreshUI()
	{
		for (int i = 0; i < _Menu.GetNumItems(); i++)
		{
			KAWidget kAWidget = _Menu.FindItemAt(i);
			KAWidgetUserData userData = kAWidget.GetUserData();
			if (userData != null && userData.GetType() == typeof(MessageUserData))
			{
				MessageUserData messageUserData = (MessageUserData)userData;
				if (messageUserData._Data.Creator == UserInfo.pInstance.UserID)
				{
					GetPlayerNameByUserID(messageUserData._Data.Creator, kAWidget.FindChildItem("TxtName"));
				}
			}
		}
	}

	public virtual void Init()
	{
		if (_Menu != null)
		{
			_Menu.ClearItems();
		}
		if (mPlayerInfo != null)
		{
			RemoveWidget(mPlayerInfo);
		}
		mPlayerInfo = DuplicateWidget("PlayerInfo");
		AddWidget(mPlayerInfo, UIAnchor.Side.Center);
		mPlayerInfo.SetVisibility(inVisible: true);
		mPlayerInfo.FindChildItem("Picture").SetVisibility(inVisible: false);
		mPlayerInfo.FindChildItem("TxtName").SetText(AvatarData.pInstance.DisplayName);
		if (!BuddyList.pIsReady)
		{
			BuddyList.Init();
		}
	}

	public void Close()
	{
		if (UiProfile.pInstance != null)
		{
			UiProfile.pInstance.CloseUI();
		}
	}

	protected void SetTopMessage(int messageID)
	{
		foreach (KeyValuePair<KAWidget, List<KAWidget>> mThreadItem in mThreadItems)
		{
			MessageUserData messageUserData = (MessageUserData)mThreadItem.Key.GetUserData();
			if (messageUserData != null && messageUserData._Data.MessageID == messageID)
			{
				_Menu.SetTopItemIdx(_Menu.FindItemIndex(mThreadItem.Key));
				break;
			}
			foreach (KAWidget item in mThreadItem.Value)
			{
				messageUserData = (MessageUserData)item.GetUserData();
				if (messageUserData != null && messageUserData._Data.MessageID == messageID)
				{
					_Menu.SetTopItemIdx(_Menu.FindItemIndex(mThreadItem.Key));
					return;
				}
			}
		}
	}

	protected virtual void UpdateCall()
	{
		if (!mLoading)
		{
			return;
		}
		if (pMessageBoard != null && pMessageBoard.pIsReady && UiProfile.pUserProfile.pIsReady && mClanReady)
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			mLoading = false;
			RsResourceManager.DestroyLoadScreen();
			base.gameObject.BroadcastMessage("ProfileDataReady", UiProfile.pUserProfile, SendMessageOptions.DontRequireReceiver);
			if (!mShowBoard)
			{
				return;
			}
			CombinedMessageType type = (mIsMyBoard ? CombinedMessageType.ALL : CombinedMessageType.MESSAGE_BOARD);
			if (mIsMyBoard && pMessageInfoType != -1)
			{
				type = pMessageType;
				ProcessMessageType(type, pMessageInfoType);
				if (_TabUI != null)
				{
					_TabUI.SelectGiftTab();
				}
				pMessageInfoType = -1;
			}
			else
			{
				ProcessMessageType(type);
			}
			UiToolbar.pMessageCount = 0;
			if (pMessageIDToBeShown != -1)
			{
				SetTopMessage(pMessageIDToBeShown);
			}
		}
		else if (pMessageBoard != null && pMessageBoard.pIsError)
		{
			mLoading = false;
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSmSocial"));
			mKAUIGenericDB = gameObject.GetComponent<KAUIGenericDB>();
			mKAUIGenericDB._MessageObject = base.gameObject;
			mKAUIGenericDB._OKMessage = "OnError";
			mKAUIGenericDB.SetTextByID(_GenericErrorText._ID, _GenericErrorText._Text, interactive: false);
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			KAUI.SetExclusive(mKAUIGenericDB, _MaskColor);
		}
	}

	protected override void Update()
	{
		base.Update();
		UpdateCall();
		if (_Menu != null && _Menu.GetNumItems() > 0)
		{
			int topItemIdx = _Menu.GetTopItemIdx();
			for (int i = topItemIdx; i >= 0 && i < topItemIdx + _Menu.GetNumItemsPerPage() && i < _Menu.GetNumItems(); i++)
			{
				KAWidget kAWidget = _Menu.FindItemAt(i);
				KAWidgetUserData userData = kAWidget.GetUserData();
				if (userData == null || !(userData.GetType() == typeof(ChallengeUserData)))
				{
					continue;
				}
				ChallengeUserData challengeUserData = (ChallengeUserData)userData;
				if (challengeUserData == null)
				{
					continue;
				}
				if (!EventManager.IsChallengeValid(challengeUserData._Challenge) || !challengeUserData.UpdateChallengeTimer())
				{
					WsWebService.RejectChallenge(challengeUserData._Challenge.ChallengeID, -1, null, null);
					RemoveFromItem(kAWidget);
					RemoveFromList(CombinedMessageType.CHALLENGE, challengeUserData._Challenge);
				}
				else if (challengeUserData._SecondsLeft <= 60)
				{
					if (challengeUserData._SecondsLeft == 1)
					{
						challengeUserData.DisplayTime(challengeUserData._SecondsLeft + " " + _OneSecondLeftText.GetLocalizedString());
					}
					else
					{
						challengeUserData.DisplayTime(challengeUserData._SecondsLeft + " " + _SecondsLeftText.GetLocalizedString());
					}
				}
				else if (challengeUserData._MinutesLeft <= 60)
				{
					challengeUserData.DisplayTime(challengeUserData._MinutesLeft + " " + _MinutesLeftText.GetLocalizedString());
				}
				else if (challengeUserData._HoursLeft <= 24)
				{
					challengeUserData.DisplayTime(challengeUserData._HoursLeft + " " + _HoursLeftText.GetLocalizedString());
				}
				else
				{
					challengeUserData.DisplayTime(challengeUserData._DaysLeft + " " + _DaysLeftText.GetLocalizedString());
				}
			}
		}
		if (BuddyList.pIsReady && !mBuddyCheckApplied)
		{
			mBuddyCheckApplied = true;
			if (BuddyList.pInstance.GetBuddyStatus(mUserID) == BuddyStatus.Approved)
			{
				_PostBtn.SetDisabled(isDisabled: false);
			}
		}
	}

	public virtual void Click(KAWidget item)
	{
		KAWidget parentItem = item.GetParentItem();
		string text = item.name;
		string text2 = ((parentItem != null) ? parentItem.name : "");
		mSelectedItem = ((parentItem != null) ? parentItem : item);
		if (item == _CloseBtn)
		{
			Close();
		}
		else if (item == _PostBtn)
		{
			if (UiLogin.pIsGuestUser)
			{
				OpenRegisterNowUI();
			}
			else
			{
				_PostUI.SetVisibility(mUserID, UiProfile.pUserProfile.GetDisplayName(), -1, mIsMyBoard);
			}
		}
		else if (text2 == _PlayerMessageTemplate || text2 == _PrivateMessageTemplate || text2 == _MessageThreadTemplate)
		{
			ClickPlayerMessage(item);
		}
		else if (text2 == _ChallengeMessageTemplate)
		{
			if (text == "YesBtn")
			{
				if (!(parentItem != null))
				{
					return;
				}
				ChallengeUserData challengeUserData = (ChallengeUserData)parentItem.GetUserData();
				if (challengeUserData == null || challengeUserData._Challenge == null)
				{
					return;
				}
				ChallengeInfo challenge = challengeUserData._Challenge;
				ChallengePetData petData = ChallengeInfo.GetPetData(challenge.ChallengeGameInfo.GameID);
				if (petData != null && petData._IsPetRequired)
				{
					if (SanctuaryManager.pCurPetData == null)
					{
						ShowMessageDB(_NeedPetText);
						return;
					}
					if (petData._BlockedStage != null)
					{
						ChallengePetBlockedStages[] blockedStage = petData._BlockedStage;
						foreach (ChallengePetBlockedStages challengePetBlockedStages in blockedStage)
						{
							if (SanctuaryManager.pCurPetData.pStage == challengePetBlockedStages._PetStage)
							{
								ShowMessageDB(challengePetBlockedStages._StageText);
								return;
							}
						}
					}
					if (petData._PetAction != null)
					{
						ChallengeRequiredPetAction[] petAction = petData._PetAction;
						foreach (ChallengeRequiredPetAction challengeRequiredPetAction in petAction)
						{
							if (!SanctuaryManager.IsActionAllowed(SanctuaryManager.pCurPetData, (PetActions)Enum.Parse(typeof(PetActions), challengeRequiredPetAction._RequiredPetAction)))
							{
								ShowMessageDB(challengeRequiredPetAction._Text);
								return;
							}
						}
					}
				}
				if (challenge.GetState() == ChallengeState.Initiated)
				{
					UserAchievementTask.Set(_AcceptChallengeAchievementID);
				}
				if (UiJournal.pInstance != null && UiJournal.pIsJournalActive)
				{
					UiJournal.pInstance.CloseJournal();
				}
				UiChatHistory.SystemMessageAccepted(challenge);
				WsWebService.AcceptChallenge(challenge.ChallengeID, -1, null, null);
				ChallengeInfo.pActiveChallenge = challenge;
				PetPlayAreaLoader._ExitToScene = RsResourceManager.pCurrentLevel;
				RsResourceManager.LoadLevel(ChallengeInfo.GetSceneName(challenge.ChallengeGameInfo.GameID));
			}
			else if (text == "NoBtn" && parentItem != null)
			{
				ChallengeUserData challengeUserData2 = (ChallengeUserData)parentItem.GetUserData();
				if (challengeUserData2 != null && challengeUserData2._Challenge != null)
				{
					UiChatHistory.SystemMessageAccepted(challengeUserData2._Challenge);
					WsWebService.RejectChallenge(challengeUserData2._Challenge.ChallengeID, -1, null, null);
					RemoveFromItem(parentItem);
					RemoveFromList(CombinedMessageType.CHALLENGE, challengeUserData2._Challenge);
				}
			}
		}
		else if (text2 == _GiftMessageTemplate)
		{
			switch (text)
			{
			case "ViewBtn":
			{
				UiMessageInfoUserData uiMessageInfoUserData2 = (UiMessageInfoUserData)parentItem.GetUserData();
				if (uiMessageInfoUserData2 != null && uiMessageInfoUserData2.GetMessageInfo() != null && _UiGiftMessageURL.Length > 0)
				{
					if (UiProfile.pInstance != null)
					{
						UiProfile.pInstance.gameObject.BroadcastMessage("SetDisabled", true, SendMessageOptions.DontRequireReceiver);
					}
					string[] array = _UiGiftMessageURL.Split('/');
					KAUICursorManager.SetDefaultCursor("Loading");
					RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], LoadGiftMessageDB, typeof(GameObject));
				}
				break;
			}
			case "OpenBtn":
			{
				UiMessageInfoUserData uiMessageInfoUserData3 = (UiMessageInfoUserData)parentItem.GetUserData();
				if (uiMessageInfoUserData3 != null && UiMessageGifts.pInstance != null)
				{
					UiMessageGifts.pInstance.Initialize(uiMessageInfoUserData3, parentItem);
				}
				break;
			}
			case "DeleteBtn":
			{
				if (!(parentItem != null))
				{
					break;
				}
				if (parentItem.FindChildItem("OpenBtn").GetVisibility())
				{
					ShowGiftMessageDeleteConfirmation(m_GiftMessageDeleteConfirmationText.GetLocalizedString());
					break;
				}
				UiMessageInfoUserData uiMessageInfoUserData = (UiMessageInfoUserData)parentItem.GetUserData();
				if (uiMessageInfoUserData != null && uiMessageInfoUserData.GetMessageInfo() != null)
				{
					RemoveGiftMessage(uiMessageInfoUserData.GetMessageInfo());
				}
				break;
			}
			}
		}
		else if (text2 == _AnnouncementMessageTemplate)
		{
			if (text == "ViewBtn")
			{
				ShowAnnouncement((AnnouncementUserData)parentItem.GetUserData());
			}
		}
		else if (text2 == "ThreadUpdateMessage")
		{
			if (text == "ViewBtn")
			{
				UiMessageInfoUserData uiMessageInfoUserData4 = (UiMessageInfoUserData)parentItem.GetUserData();
				if (uiMessageInfoUserData4 != null && uiMessageInfoUserData4.GetMessageInfo() != null)
				{
					if (UiMessages.pInstance != null)
					{
						UiMessages.pInstance.Close();
					}
					mLoadProfileOnDestroy = true;
					mProfileToLoadOnDestroy = uiMessageInfoUserData4.GetMessageInfo().FromUserID;
				}
			}
			if (text == "DeleteBtn")
			{
				GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSmSocial"));
				mKAUIGenericDB = gameObject.GetComponent<KAUIGenericDB>();
				mKAUIGenericDB._MessageObject = base.gameObject;
				mKAUIGenericDB._YesMessage = "OnThreadUpdateMessageDelete";
				mKAUIGenericDB._NoMessage = "OnClose";
				mKAUIGenericDB.SetTextByID(_DeleteMessageText._ID, _DeleteMessageText._Text, interactive: false);
				mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
				KAUI.SetExclusive(mKAUIGenericDB, _MaskColor);
			}
		}
		else if ((text2 == _AchievementMessageTemplate || text2 == _SocialMessageTemplate) && text == "DeleteBtn")
		{
			UiMessageInfoUserData uiMessageInfoUserData5 = (UiMessageInfoUserData)parentItem.GetUserData();
			if (uiMessageInfoUserData5 != null)
			{
				RemoveFromItem(parentItem);
				RemoveFromList(CombinedMessageType.USER_MESSAGE_QUEUE, uiMessageInfoUserData5.GetMessageInfo());
			}
		}
	}

	private void ShowGiftMessageDeleteConfirmation(string inText)
	{
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message Delete Confirmation");
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB.SetDestroyOnClick(isDestroy: true);
		mKAUIGenericDB._YesMessage = "OnGiftMessageDelete";
		mKAUIGenericDB.SetText(inText, interactive: false);
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
		KAUI.SetExclusive(mUiGenericDB);
	}

	private void OnGiftMessageDelete()
	{
		UiMessageInfoUserData uiMessageInfoUserData = (UiMessageInfoUserData)mSelectedItem.GetUserData();
		if (uiMessageInfoUserData != null && uiMessageInfoUserData.GetMessageInfo() != null)
		{
			RemoveGiftMessage(uiMessageInfoUserData.GetMessageInfo());
		}
	}

	public void ClickPlayerMessage(KAWidget item)
	{
		KAWidget parentItem = item.GetParentItem();
		switch (item.name)
		{
		case "ReplyBtn":
		{
			if (UiLogin.pIsGuestUser)
			{
				OpenRegisterNowUI();
				break;
			}
			MessageUserData messageUserData3 = (MessageUserData)parentItem.GetUserData();
			_PostUI.SetVisibility(messageUserData3._Data.Creator, parentItem.FindChildItem("TxtName").GetText(), messageUserData3._Data.MessageID.Value);
			_PostUI.UpdateToggleInteractivity(messageUserData3._Data.isPrivate);
			_PostUI.UpdateToggleState(messageUserData3._Data.isPrivate);
			break;
		}
		case "ViewBtn":
		{
			MessageUserData messageUserData2 = (MessageUserData)parentItem.GetUserData();
			_ThreadUI.Show(mUserID, messageUserData2._Data.MessageID.Value);
			break;
		}
		case "DeleteBtn":
		{
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSmSocial"));
			mKAUIGenericDB = gameObject.GetComponent<KAUIGenericDB>();
			mKAUIGenericDB._MessageObject = base.gameObject;
			mKAUIGenericDB._YesMessage = "OnDelete";
			mKAUIGenericDB._NoMessage = "OnClose";
			mKAUIGenericDB.SetTextByID(_DeleteMessageText._ID, _DeleteMessageText._Text, interactive: false);
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
			KAUI.SetExclusive(mKAUIGenericDB, _MaskColor);
			break;
		}
		case "IgnoreBtn":
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSmSocial"));
			mKAUIGenericDB = gameObject2.GetComponent<KAUIGenericDB>();
			mKAUIGenericDB._MessageObject = base.gameObject;
			mKAUIGenericDB._YesMessage = "OnIgnore";
			mKAUIGenericDB._NoMessage = "OnClose";
			mKAUIGenericDB.SetTextByID(_IgnorePlayerText._ID, _IgnorePlayerText._Text, interactive: false);
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
			KAUI.SetExclusive(mKAUIGenericDB, _MaskColor);
			break;
		}
		case "ModeratorBtn":
		{
			MessageUserData messageUserData4 = (MessageUserData)parentItem.GetUserData();
			UiModeratorDB component = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfUiModeratorDB")).GetComponent<UiModeratorDB>();
			component.SetVisibility(ModeratorType.REPORT);
			component._UserID = messageUserData4._Data.Creator;
			break;
		}
		default:
			if (!(item == mPortraitFrame))
			{
				break;
			}
			goto case "Picture";
		case "Picture":
		case "TxtName":
		{
			MessageUserData messageUserData = (MessageUserData)parentItem.GetUserData();
			Close();
			UiProfile.ShowProfile((messageUserData != null) ? messageUserData._Data.Creator : mUserID);
			break;
		}
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		Click(item);
	}

	public void SetUIDisabled(bool isDisable)
	{
		SetState(isDisable ? KAUIState.DISABLED : KAUIState.INTERACTIVE);
		if (_Menu != null)
		{
			_Menu.SetState(isDisable ? KAUIState.DISABLED : KAUIState.INTERACTIVE);
		}
	}

	private void ShowMessageDB(LocaleString inText)
	{
		SetUIDisabled(isDisable: true);
		if (mKAUIGenericDB != null)
		{
			UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
		}
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSm"));
		mKAUIGenericDB = gameObject.GetComponent<KAUIGenericDB>();
		mKAUIGenericDB.SetTextByID(inText._ID, inText._Text, interactive: false);
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mKAUIGenericDB.SetExclusive();
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._OKMessage = "OnDBClose";
	}

	private void OnDBClose()
	{
		SetUIDisabled(isDisable: false);
		if (mKAUIGenericDB != null)
		{
			UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
		}
	}

	public virtual KAWidget AddMessage(Message message)
	{
		return AddMessage(message.isPrivate ? _PrivateMessageTemplate : _PlayerMessageTemplate, message);
	}

	protected KAWidget AddMessage(string template, Message message)
	{
		KAWidget kAWidget = DuplicateWidget(template);
		kAWidget.name = template;
		kAWidget.SetVisibility(inVisible: true);
		MessageUserData messageUserData = new MessageUserData();
		messageUserData._Data = message;
		messageUserData.pLoaded = false;
		kAWidget.SetUserData(messageUserData);
		bool flag = true;
		NPCFriendData[] nPCFriends = _NPCFriends;
		foreach (NPCFriendData nPCFriendData in nPCFriends)
		{
			if (nPCFriendData._UserID == messageUserData._Data.Creator)
			{
				flag = false;
				kAWidget.FindChildItem("Picture").SetTexture(nPCFriendData._Picture);
				kAWidget.FindChildItem("Picture").SetInteractive(isInteractive: false);
				kAWidget.FindChildItem("TxtName").SetText(nPCFriendData._NPCName);
				kAWidget.FindChildItem("TxtName").SetInteractive(isInteractive: false);
				mPortraitFrame.SetInteractive(isInteractive: false);
				kAWidget.FindChildItem("IgnoreBtn").SetVisibility(inVisible: false);
				kAWidget.FindChildItem("ReplyBtn").SetVisibility(inVisible: false);
				kAWidget.FindChildItem("ModeratorBtn").SetVisibility(inVisible: false);
				kAWidget.FindChildItem("ViewBtn").SetVisibility(inVisible: false);
				break;
			}
		}
		if (flag)
		{
			_ = _TakePhoto;
			SetMessageItemButtons(kAWidget);
		}
		BuddyStatus buddyStatus = BuddyStatus.Unknown;
		if (BuddyList.pIsReady)
		{
			buddyStatus = BuddyList.pInstance.GetBuddyStatus(message.Creator);
		}
		KAWidget kAWidget2 = kAWidget.FindChildItem("TxtMessage");
		if ((buddyStatus == BuddyStatus.BlockedBySelf || buddyStatus == BuddyStatus.BlockedByBoth) && message.Creator != mUserID)
		{
			kAWidget.FindChildItem("IgnoredIcon").SetVisibility(inVisible: true);
			kAWidget.FindChildItem("ReplyBtn").SetVisibility(inVisible: false);
		}
		else
		{
			kAWidget2.SetText(message.Content);
		}
		AddDateTime(kAWidget, message.CreateTime);
		_Menu.AddWidget(kAWidget);
		return kAWidget;
	}

	protected void AddDateTime(KAWidget item, DateTime dateTime)
	{
		KAWidget kAWidget = item.FindChildItem("TxtDate");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inVisible: true);
			kAWidget.SetText(dateTime.ToLocalTime().ToString(_DateTimeFormat));
		}
	}

	public virtual void SetMessageItemButtons(KAWidget item)
	{
		MessageUserData messageUserData = (MessageUserData)item.GetUserData();
		BuddyStatus buddyStatus = BuddyStatus.Unknown;
		if (BuddyList.pIsReady)
		{
			buddyStatus = BuddyList.pInstance.GetBuddyStatus(messageUserData._Data.Creator);
		}
		if (!mIsMyBoard)
		{
			item.FindChildItem("IgnoreBtn").SetVisibility(inVisible: false);
			item.FindChildItem("DeleteBtn").SetVisibility(inVisible: false);
		}
		else if (messageUserData._Data.Creator == UserInfo.pInstance.UserID || buddyStatus == BuddyStatus.BlockedBySelf || buddyStatus == BuddyStatus.BlockedByBoth || buddyStatus == BuddyStatus.Unknown)
		{
			item.FindChildItem("IgnoreBtn").SetState(KAUIState.DISABLED);
		}
		buddyStatus = BuddyStatus.Unknown;
		if (BuddyList.pIsReady)
		{
			buddyStatus = BuddyList.pInstance.GetBuddyStatus(mUserID);
		}
		if (!mIsMyBoard && buddyStatus != BuddyStatus.Approved)
		{
			item.FindChildItem("ReplyBtn").SetState(KAUIState.DISABLED);
		}
		if (messageUserData._Data.Creator == UserInfo.pInstance.UserID)
		{
			item.FindChildItem("ModeratorBtn").SetState(KAUIState.DISABLED);
		}
	}

	public void OnError()
	{
		KAUI.RemoveExclusive(mKAUIGenericDB);
		UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
		Close();
	}

	public void OnClose()
	{
		KAUI.RemoveExclusive(mKAUIGenericDB);
		UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
	}

	private void OnThreadUpdateMessageDelete()
	{
		UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
		KAUI.RemoveExclusive(mKAUIGenericDB);
		UiMessageInfoUserData uiMessageInfoUserData = (UiMessageInfoUserData)mSelectedItem.GetUserData();
		if (uiMessageInfoUserData != null)
		{
			RemoveFromItem(mSelectedItem);
			RemoveFromList(CombinedMessageType.USER_MESSAGE_QUEUE, uiMessageInfoUserData.GetMessageInfo());
		}
	}

	public virtual void OnDelete()
	{
		KAUI.RemoveExclusive(mKAUIGenericDB);
		UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
		KAUICursorManager.SetDefaultCursor("Loading");
		SetInteractive(interactive: false);
		MessageUserData messageUserData = (MessageUserData)mSelectedItem.GetUserData();
		pMessageBoard.DeleteMessage(messageUserData._Data.MessageID.Value, DeleteEventHandler);
	}

	public void OnIgnore()
	{
		KAUI.RemoveExclusive(mKAUIGenericDB);
		UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
		KAUICursorManager.SetDefaultCursor("Loading");
		SetInteractive(interactive: false);
		MessageUserData messageUserData = (MessageUserData)mSelectedItem.GetUserData();
		BuddyList.pInstance.BlockBuddy(messageUserData._Data.Creator, IgnoreEventHandler);
	}

	public virtual void PostMessage(string userID, string message, MessageLevel level, string attribute, bool isPrivate = false)
	{
		WsWebService.PostNewMessageToBoard(new MessageRequest
		{
			targetUser = userID,
			displayAttribute = attribute,
			content = message,
			level = level,
			isPrivate = isPrivate,
			replyTo = null
		}, EventHandler, null);
	}

	public virtual void ReplyToMessage(string userID, int messageID, string message, MessageLevel level, string attribute, bool isPrivate = false)
	{
		WsWebService.PostMessageReply(new MessageRequest
		{
			targetUser = userID,
			displayAttribute = attribute,
			content = message,
			level = level,
			replyTo = messageID,
			isPrivate = isPrivate
		}, EventHandler, null);
	}

	public void EventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE || inEvent == WsServiceEvent.ERROR)
		{
			RMFMessageBoardPostResult result = (RMFMessageBoardPostResult)inObject;
			_PostUI.PostEventHandler(result);
		}
	}

	public virtual void OnPost(string userID, Message message, int replyToMessageID)
	{
		if (message == null)
		{
			return;
		}
		if (userID == mUserID && replyToMessageID == -1)
		{
			mCombinedListMessages.Insert(0, message);
			CombinedMessageType type = mCombinedMessageType;
			mCombinedMessageType = CombinedMessageType.NONE;
			ProcessMessageType(type);
			if ((bool)pMessageThread)
			{
				pMessageThread.AddConversation(message.MessageID.Value, message, addLast: true);
			}
		}
		else
		{
			if (!pMessageThread || replyToMessageID == -1)
			{
				return;
			}
			pMessageThread.AddConversation(replyToMessageID, message, addLast: true);
			if (mThreadItems.ContainsKey(mSelectedItem))
			{
				foreach (KAWidget item2 in mThreadItems[mSelectedItem])
				{
					_Menu.RemoveWidget(item2);
				}
				mThreadItems[mSelectedItem].Clear();
			}
			message.ReplyToMessageID = replyToMessageID;
			LoadConversation(replyToMessageID, mSelectedItem);
			if (mThreadItems != null && mThreadItems.ContainsKey(mSelectedItem) && mThreadItems[mSelectedItem].Count > 0)
			{
				KAWidget item = mThreadItems[mSelectedItem][mThreadItems[mSelectedItem].Count - 1];
				int currentPage = _Menu.GetCurrentPage();
				int num = _Menu.FindItemIndex(item) / _Menu.GetNumItemsPerPage();
				if (currentPage != num)
				{
					_Menu.SetTopItemIdx(_Menu.FindItemIndex(mSelectedItem));
				}
			}
		}
	}

	public virtual void DeleteEventHandler(bool success)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		SetInteractive(interactive: true);
		if (success)
		{
			RemoveFromItem(mSelectedItem);
			if (mThreadItems.ContainsKey(mSelectedItem))
			{
				foreach (KAWidget item in mThreadItems[mSelectedItem])
				{
					RemoveFromItem(item);
				}
			}
			MessageUserData messageUserData = (MessageUserData)mSelectedItem.GetUserData();
			if (messageUserData == null)
			{
				return;
			}
			RemoveFromList(CombinedMessageType.MESSAGE_BOARD, messageUserData._Data);
			if (messageUserData._Data.ReplyToMessageID.HasValue)
			{
				pMessageThread.RemoveConversation(messageUserData._Data.ReplyToMessageID.Value, messageUserData._Data);
			}
			{
				foreach (KeyValuePair<KAWidget, List<KAWidget>> mThreadItem in mThreadItems)
				{
					foreach (KAWidget item2 in mThreadItem.Value)
					{
						if (!(item2 == mSelectedItem))
						{
							continue;
						}
						KAWidget key = mThreadItem.Key;
						if (!(key != null))
						{
							continue;
						}
						foreach (KAWidget item3 in mThreadItems[key])
						{
							RemoveFromItem(item3);
						}
						mThreadItems[key].Clear();
						OnConversationReady(pMessageThread.mConversations[messageUserData._Data.ReplyToMessageID.Value], key);
						return;
					}
				}
				return;
			}
		}
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSmSocial"));
		mKAUIGenericDB = gameObject.GetComponent<KAUIGenericDB>();
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._OKMessage = "OnClose";
		mKAUIGenericDB.SetTextByID(_GenericErrorText._ID, _GenericErrorText._Text, interactive: false);
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		KAUI.SetExclusive(mKAUIGenericDB, _MaskColor);
	}

	private void IgnoreEventHandler(WsServiceType inType, object inResult)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		SetInteractive(interactive: true);
		if (inResult == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSmSocial"));
			mKAUIGenericDB = gameObject.GetComponent<KAUIGenericDB>();
			mKAUIGenericDB._MessageObject = base.gameObject;
			mKAUIGenericDB._OKMessage = "OnClose";
			mKAUIGenericDB.SetTextByID(_GenericErrorText._ID, _GenericErrorText._Text, interactive: false);
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			KAUI.SetExclusive(mKAUIGenericDB, _MaskColor);
		}
		else
		{
			MessageUserData messageUserData = (MessageUserData)mSelectedItem.GetUserData();
			IgnorePlayer(messageUserData._Data.Creator);
		}
	}

	public void IgnorePlayer(string userID)
	{
		for (int i = 0; i < _Menu.GetNumItems(); i++)
		{
			KAWidget kAWidget = _Menu.FindItemAt(i);
			KAWidgetUserData userData = kAWidget.GetUserData();
			if (userData != null && userData.GetType() == typeof(MessageUserData) && ((MessageUserData)userData)._Data.Creator == userID)
			{
				kAWidget.FindChildItem("TxtMessage").SetText("");
				kAWidget.FindChildItem("IgnoredIcon").SetVisibility(inVisible: true);
				kAWidget.FindChildItem("ReplyBtn").SetVisibility(inVisible: false);
				SetMessageItemButtons(kAWidget);
			}
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

	public void RemoveGiftMessage(MessageInfo messageInfo)
	{
		RemoveFromItem(mSelectedItem);
		RemoveFromList(CombinedMessageType.USER_MESSAGE_QUEUE, messageInfo);
	}

	public void RestoreUI()
	{
		if ((bool)UiProfile.pInstance)
		{
			UiProfile.pInstance.gameObject.BroadcastMessage("OnSetVisibility", true);
		}
		if (mSelectedItem != null)
		{
			UiMessageInfoUserData uiMessageInfoUserData = (UiMessageInfoUserData)mSelectedItem.GetUserData();
			if (uiMessageInfoUserData != null)
			{
				_Menu.RemoveWidget(mSelectedItem);
				RemoveFromList(CombinedMessageType.USER_MESSAGE_QUEUE, uiMessageInfoUserData.GetMessageInfo());
			}
		}
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
	}

	public void GetPlayerNameByUserID(string userID, KAWidget item)
	{
		if (UserProfile.pProfileData != null && userID == UserProfile.pProfileData.ID && UserProfile.pProfileData.AvatarInfo != null && UserProfile.pProfileData.AvatarInfo.AvatarData != null)
		{
			SetPlayerName(item, UserProfile.pProfileData.AvatarInfo.AvatarData.DisplayName);
			return;
		}
		if (UiProfile.pUserProfile != null && UiProfile.pUserProfile.UserID == userID)
		{
			SetPlayerName(item, UiProfile.pUserProfile.GetDisplayName());
			return;
		}
		if (BuddyList.pIsReady)
		{
			Buddy buddy = BuddyList.pInstance.GetBuddy(userID);
			if (buddy != null && !string.IsNullOrEmpty(buddy.DisplayName))
			{
				SetPlayerName(item, buddy.DisplayName);
				return;
			}
		}
		WsWebService.GetDisplayNameByUserID(userID, ServiceEventHandler, item);
	}

	private void SetPlayerName(KAWidget item, string name)
	{
		if (item != null)
		{
			item.SetText(name);
			item.SetInteractive(isInteractive: true);
		}
	}

	protected void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE && inUserData != null && !string.IsNullOrEmpty((string)inObject))
		{
			KAWidget kAWidget = (KAWidget)inUserData;
			if (kAWidget != null)
			{
				SetPlayerName(kAWidget, (string)inObject);
			}
		}
	}

	private void OnMuttPodVisitError()
	{
		Close();
	}

	private void SetUserId(string userID)
	{
		mUserID = userID;
	}
}
