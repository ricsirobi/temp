using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JSGames.UI.Util;
using SOD.Event;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JSGames.UI.TerrorMail;

public class UITerrorMail : UI
{
	public string _UiChatRegisterNowPath;

	public MessageListInstance pMessageBoard;

	public UITerrorMailMenu _Menu;

	public UITerrorMailMessageThread _ThreadUI;

	public UITerrorMailTabMenu _TabUI;

	public LocaleString _DeleteMessageTitleText = new LocaleString("Delete this message?");

	public LocaleString _DeleteMessageText = new LocaleString("Are you sure you want to delete this message?");

	public LocaleString _IgnorePlayerTitleText = new LocaleString("Ignore this player?");

	public LocaleString _IgnorePlayerText = new LocaleString("Are you sure you'd like to ignore this person?");

	public LocaleString _GenericErrorText = new LocaleString("There was an error connecting to the server. Please try again.");

	public LocaleString _NeedPetText = new LocaleString("You need a dragon to play.");

	public LocaleString _MessageUnavailableText = new LocaleString("This message is not available.");

	public LocaleString _NotInMMORoomText;

	public LocaleString _NoDisplayNameText = new LocaleString("A friend");

	public TagAndDefaultText[] _TagAndDefaultText;

	public NPCFriendData[] _NPCFriends;

	public UIWidget _CloseBtn;

	public UIWidget _PostBtn;

	public UIWidget _NoMessageTxtItem;

	public UIWidget _CalendarButton;

	public string _MyMessageFilterName = "OnlineProfile";

	public string _OthersMessageFilterName = "OnlineBuddyProfile";

	public string _GiftMessageTemplateName = "MessageGift";

	public string _AnnouncementMessageTemplateName = "MessageAnnouncement";

	public string _AchievementMessageTemplateName = "MessageAchievement";

	public string _SocialMessageTemplateName = "MessageBuddy";

	public string _ChallengeMessageTemplateName = "MessageChallenge";

	public UIMessagePopulator _ChallengeForm;

	public UITerrorMailMessagesChallengeWon _ChallengeResultsForm;

	public UIMessagePopulator _ThreadUpdateForm;

	private ChallengeInfo mCurrentChallenge;

	public UIMessageItem _MessageThreadItemTemplate;

	public UIMessageItem _MessageThreadReplyItemTemplate;

	public UIMessageItem _MessageListItemTemplate;

	public UITerrorMailMessageGifts _GiftForm;

	public UIMessagePopulator _AnnouncementForm;

	public UIMenu _AchievementsUIMenu;

	public UISystemMessage _AchievementsUIPopulator;

	public int _AcceptChallengeAchievementID = 133;

	public LocaleString _AllTabNoMessageText = new LocaleString("No Messages To Display");

	public LocaleString _MessageTabNoMessageText = new LocaleString("No Player Messages To Display");

	public LocaleString _ChallengeTabNoMessageText = new LocaleString("No Challenge Messages To Display");

	public LocaleString _GiftTabNoMessageText = new LocaleString("No Gift Messages To Display");

	public LocaleString _SocialTabNoMessageText = new LocaleString("No Social Messages To Display");

	public LocaleString _AchievementTabNoMessageText = new LocaleString("No Achievement Messages To Display");

	protected LocaleString noMessageText;

	public string _PostUIDBPath = "RS_DATA/PfUINewMessageDB.unity3d/PfUINewMessageDB";

	public UITerrorMailPost _PostUI;

	private int[] mMessageQueueTypes = new int[6] { 19, 9, 14, 11, 4, 21 };

	protected bool mLoading = true;

	protected UIWidget mSelectedItem;

	protected MessageInfo mMessageInfo;

	protected Message mSelectedMessage;

	protected List<object> mCombinedListMessages = new List<object>();

	private List<MessageInfo> mRemovedGiftMessages = new List<MessageInfo>();

	[SerializeField]
	private LocaleString m_GiftMessageDeleteConfirmationText = new LocaleString("Are you sure you want to delete this message with unopened gifts?");

	private static string mUserID = "";

	private static CombinedMessageType mCombinedMessageType = CombinedMessageType.NONE;

	private static int mMessageInfoType = -1;

	private static int mMessageIDToShow = -1;

	private static string mLastLevel = "";

	private static bool mIsMyBoard = false;

	private bool mBuddyCheckApplied;

	private Dictionary<int, List<Message>> mConversations = new Dictionary<int, List<Message>>();

	private Dictionary<int, Action<List<Message>, UIWidget>> mConversationReadyEventList = new Dictionary<int, Action<List<Message>, UIWidget>>();

	public static bool pIsMyBoard
	{
		get
		{
			return mIsMyBoard;
		}
		set
		{
			mIsMyBoard = value;
		}
	}

	public static void Show(string inUserID, CombinedMessageType inCombinedMessageType, int inMessageInfoType, int inMessageIDToBeShown = -1, string inLastLevel = "")
	{
		mUserID = inUserID;
		mCombinedMessageType = inCombinedMessageType;
		mMessageInfoType = inMessageInfoType;
		mMessageIDToShow = inMessageIDToBeShown;
		if (!string.IsNullOrEmpty(inLastLevel))
		{
			mLastLevel = inLastLevel;
		}
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
		string[] array = GameConfig.GetKeyData("MessageBoardAsset").Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnResourceDownloaded, typeof(GameObject), inDontDestroy: false, inMessageIDToBeShown);
	}

	private static void OnResourceDownloaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			UnityEngine.Object.Instantiate((GameObject)inObject);
			break;
		case RsResourceLoadEvent.ERROR:
			if (!string.IsNullOrEmpty(mLastLevel) && RsResourceManager.pCurrentLevel == GameConfig.GetKeyData("MessageBoardScene"))
			{
				AvAvatar.pStartLocation = AvAvatar.pSpawnAtSetPosition;
				RsResourceManager.LoadLevel(mLastLevel);
				break;
			}
			if (UiProfile.pInstance != null)
			{
				UiProfile.pInstance.CloseUI();
			}
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			break;
		}
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		OnOpenUI();
		pVisible = true;
	}

	public virtual void OnOpenUI()
	{
		if (!BuddyList.pIsReady)
		{
			BuddyList.Init();
		}
		if ((bool)_PostUI)
		{
			_PostUI.pVisible = false;
		}
		_GiftForm.OnClose();
		mIsMyBoard = mUserID == UserInfo.pInstance.UserID;
		if (!mIsMyBoard)
		{
			List<TabType> list = new List<TabType>();
			_TabUI._AchievementTabBtn.pVisible = false;
			list.Add(_TabUI._TabBtnTypes.Find((TabType t) => t._TabMessageType == CombinedMessageType.MESSAGE_BOARD));
			ShowTabs(list);
			UIToggleButton uIToggleButton = (UIToggleButton)_TabUI._TabBtnTypes[_TabUI._TabBtnTypes.IndexOf(list[0])]._Tab;
			if ((bool)uIToggleButton)
			{
				uIToggleButton.pChecked = true;
			}
		}
		_CalendarButton.pVisible = mIsMyBoard;
		pState = WidgetState.NOT_INTERACTIVE;
		pMessageBoard = MessageListInstance.Load(mUserID, mIsMyBoard ? _MyMessageFilterName : _OthersMessageFilterName, null);
		StartCoroutine(CheckMessageListError());
	}

	private void ShowTabs(List<TabType> inTabTypes)
	{
		if (!_TabUI)
		{
			return;
		}
		foreach (TabType tabBtnType in _TabUI._TabBtnTypes)
		{
			tabBtnType._Tab.pVisible = inTabTypes.Contains(tabBtnType);
		}
	}

	private IEnumerator CheckMessageListError()
	{
		while (pMessageBoard == null || !pMessageBoard.pIsReady)
		{
			yield return new WaitForEndOfFrame();
		}
		if (pMessageBoard.pIsError)
		{
			UtDebug.LogError("Messages failed to load");
			UIUtil.DisplayGenericDB("PfUIGenericDB", _GenericErrorText.GetLocalizedString(), _GenericErrorText.GetLocalizedString(), base.gameObject, "Close");
		}
		else
		{
			OnMessageListReady();
		}
	}

	public void OnMessageListReady()
	{
		ClearConversation();
		UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
		if (pMessageBoard == null || pMessageBoard.mCombinedMessages == null)
		{
			UtDebug.LogError("Combined Message list is null");
			UIUtil.DisplayGenericDB("PfUIGenericDB", _GenericErrorText.GetLocalizedString(), _GenericErrorText.GetLocalizedString(), base.gameObject, "Close");
			SetInteractive(interactive: true);
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
				if (!(UtUtilities.DeserializeFromXml(combinedListMessage.MessageBody, typeof(Message)) is Message message))
				{
					UtDebug.LogError("Message is null (failed to deserialize)");
				}
				else if (!message.ReplyToMessageID.HasValue)
				{
					mCombinedListMessages.Add(message);
					if ((bool)_ThreadUI)
					{
						AddConversation(message.ConversationID, message);
					}
				}
				else if ((bool)_ThreadUI)
				{
					AddConversation(message.ConversationID, message);
				}
				break;
			case CombinedMessageType.USER_MESSAGE_QUEUE:
			{
				if (!(UtUtilities.DeserializeFromXml(combinedListMessage.MessageBody, typeof(MessageInfo)) is MessageInfo messageInfo) || !messageInfo.MessageID.HasValue)
				{
					Debug.LogError("MessgeInfo is null (failed to deserialize)");
					break;
				}
				int value = messageInfo.MessageTypeID.Value;
				if (mMessageQueueTypes.Contains(value))
				{
					messageInfo.CreateDate = combinedListMessage.MessageDate;
					mCombinedListMessages.Add(messageInfo);
				}
				break;
			}
			case CombinedMessageType.ANNOUNCEMENT:
				if (!(UtUtilities.DeserializeFromXml(combinedListMessage.MessageBody, typeof(Announcement)) is Announcement announcement))
				{
					UtDebug.LogError("Announcement is null (failed to deserialize)");
				}
				else if (new TaggedAnnouncementHelper(announcement.AnnouncementText).Announcement.ContainsKey("Message"))
				{
					mCombinedListMessages.Add(announcement);
				}
				break;
			}
		}
		pState = WidgetState.INTERACTIVE;
		SetVisible();
	}

	private void SetVisible()
	{
		if (mMessageIDToShow > -1)
		{
			mLoading = false;
			(_TabUI._TabBtnTypes.Find((TabType t) => t._TabMessageType == CombinedMessageType.MESSAGE_BOARD)._Tab as UIToggleButton).pChecked = true;
			ProcessMessageType(CombinedMessageType.MESSAGE_BOARD, mMessageInfoType, force: true);
			mSelectedMessage = GetConversationParent(mMessageIDToShow);
			mSelectedItem = _Menu.pChildWidgets.Find((UIWidget t) => t.pData == mSelectedMessage);
			if ((bool)mSelectedItem)
			{
				(mSelectedItem as UIMessageItem).pChecked = true;
			}
			OnClickThreadParent();
			pVisible = true;
		}
		else
		{
			OnSetVisibility(visible: true);
		}
	}

	private Message GetConversationParent(int messageID)
	{
		foreach (KeyValuePair<int, List<Message>> mConversation in mConversations)
		{
			for (int i = 0; i < mConversation.Value.Count; i++)
			{
				if (mConversation.Value[i].MessageID == messageID)
				{
					return mConversation.Value[0];
				}
			}
		}
		return null;
	}

	protected void OpenRegisterNowUI()
	{
		UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
		SetInteractive(interactive: false);
		RsResourceManager.LoadAssetFromBundle(_UiChatRegisterNowPath, OnRegisterNowBundleLoaded, typeof(GameObject), inDontDestroy: false, typeof(GameObject));
	}

	private void OnRegisterNowBundleLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			UnityEngine.Object.Instantiate((GameObject)inObject).name = ((GameObject)inObject).name;
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			SetInteractive(interactive: true);
			break;
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("Error loading Event Intro Prefab!" + inURL);
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			SetInteractive(interactive: true);
			break;
		}
	}

	public virtual void OnSetVisibility(bool visible)
	{
		if (visible && pMessageBoard != null)
		{
			if (pMessageBoard.pIsReady)
			{
				ProcessMessageType((mCombinedMessageType == CombinedMessageType.NONE) ? CombinedMessageType.ALL : mCombinedMessageType, mMessageInfoType, force: true);
				UiToolbar.pMessageCount = 0;
			}
			else
			{
				UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
			}
		}
		pVisible = visible;
	}

	public virtual void ProcessMessageType(CombinedMessageType type, int messageInfoType = -1, bool force = false)
	{
		if (messageInfoType > -1)
		{
			if (type == CombinedMessageType.USER_MESSAGE_QUEUE && messageInfoType != mMessageInfoType)
			{
				mCombinedMessageType = CombinedMessageType.NONE;
			}
			mMessageInfoType = messageInfoType;
		}
		if (type == mCombinedMessageType && !force)
		{
			return;
		}
		ClearItems();
		SetNoMessageTextVisible(visible: false);
		mCombinedMessageType = type;
		_PostBtn.pVisible = mCombinedMessageType == CombinedMessageType.MESSAGE_BOARD;
		_ThreadUI._Menu.ClearChildren();
		_ThreadUI.pVisible = false;
		_AchievementsUIMenu.pVisible = false;
		_GiftForm?.OnClose();
		_AnnouncementForm.pVisible = false;
		_ChallengeForm.pVisible = false;
		_ThreadUpdateForm.pVisible = false;
		_ChallengeResultsForm.pVisible = false;
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
			if (mMessageInfoType == 19)
			{
				AddGiftMessages();
			}
			else if (mMessageInfoType == 9)
			{
				ShowAchievementMessages();
			}
			break;
		case CombinedMessageType.NEWS:
			AddAnnouncementMessages();
			break;
		}
		RemoveExpiredGiftMessages();
	}

	private void ClearItems()
	{
		foreach (UIWidget item in _Menu.pChildWidgets.FindAll((UIWidget t) => t.name.Contains("(Clone)")))
		{
			_Menu.RemoveWidget(item, destroy: true);
		}
	}

	public void RemoveFromCombinedMessageList(CombinedMessageType type, object inListItem)
	{
		Message selectedMessage = null;
		MessageInfo selectedMessageInfo = null;
		if (inListItem is Message)
		{
			selectedMessage = (Message)inListItem;
		}
		else if (inListItem is MessageInfo)
		{
			selectedMessageInfo = inListItem as MessageInfo;
		}
		if (type == CombinedMessageType.USER_MESSAGE_QUEUE && selectedMessageInfo != null)
		{
			RemoveMessageInfo(selectedMessageInfo);
		}
		List<Message> messages = GetMessages<Message>();
		List<ChallengeInfo> messages2 = GetMessages<ChallengeInfo>();
		if (selectedMessageInfo != null)
		{
			mCombinedListMessages.Remove(messages.Find((Message t) => t.MessageID == selectedMessageInfo.MessageID));
			mCombinedListMessages.Remove(selectedMessageInfo);
		}
		else if (selectedMessage != null)
		{
			mCombinedListMessages.Remove(messages.Find((Message t) => t.MessageID == selectedMessage.MessageID));
		}
		else if (mCurrentChallenge != null)
		{
			mCombinedListMessages.Remove(messages2.Find((ChallengeInfo t) => t.ChallengeID == mCurrentChallenge.ChallengeID));
		}
	}

	private void UpdateMessageListCount()
	{
		if (mCombinedMessageType == CombinedMessageType.ALL)
		{
			return;
		}
		int num = 0;
		switch (mCombinedMessageType)
		{
		case CombinedMessageType.MESSAGE_BOARD:
			num += GetMessages<Message>().Count;
			break;
		case CombinedMessageType.USER_MESSAGE_QUEUE:
			num += GetMessageByInfoType(GetMessages<MessageInfo>(), 19).Count;
			num += GetMessageByInfoType(GetMessages<MessageInfo>(), 9).Count;
			num += GetMessageByInfoType(GetMessages<MessageInfo>(), 11).Count;
			break;
		case CombinedMessageType.CHALLENGE:
			num += GetMessages<ChallengeInfo>().Count;
			num += mCombinedListMessages.FindAll((object t) => t is MessageInfo && (t as MessageInfo).MessageTypeID.Value == 14).Count;
			break;
		}
		if (num == 0)
		{
			SetNoMessageTextVisible(visible: true);
		}
	}

	protected virtual void SetNoMessageTextVisible(bool visible)
	{
		if (_NoMessageTxtItem == null)
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
				if (mMessageInfoType == 19)
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
				_NoMessageTxtItem.pText = noMessageText.GetLocalizedString();
			}
		}
		_NoMessageTxtItem.pVisible = visible;
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
			RemoveFromCombinedMessageList(CombinedMessageType.USER_MESSAGE_QUEUE, mRemovedGiftMessage);
		}
		mRemovedGiftMessages.Clear();
		UpdateMessageListCount();
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
			if (!(mCombinedListMessage is Message))
			{
				if (!(mCombinedListMessage is ChallengeInfo))
				{
					if (!(mCombinedListMessage is MessageInfo))
					{
						if (mCombinedListMessage is Announcement)
						{
							AddAnnouncementItem((Announcement)mCombinedListMessage);
						}
						continue;
					}
					MessageInfo messageInfo = (MessageInfo)mCombinedListMessage;
					switch (messageInfo.MessageTypeID.Value)
					{
					case 19:
						AddGiftItem(messageInfo);
						break;
					case 14:
						((UIMessageItem)_Menu._Template.Duplicate(autoAddToSameParent: true)).Populate(messageInfo);
						break;
					case 21:
						if (!BuddyList.pIsReady || BuddyList.pInstance.GetBuddyStatus(messageInfo.FromUserID) == BuddyStatus.Approved)
						{
							((UIMessageItem)_Menu._Template.Duplicate(autoAddToSameParent: true)).Populate(messageInfo);
						}
						break;
					}
				}
				else
				{
					ChallengeInfo challenge = (ChallengeInfo)mCombinedListMessage;
					AddChallengeItem(challenge);
				}
			}
			else
			{
				Message message = (Message)mCombinedListMessage;
				UIMessageItem uIMessageItem = (UIMessageItem)_Menu._Template.Duplicate(autoAddToSameParent: true);
				uIMessageItem.Populate(message);
				LoadConversation(message.ConversationID, OnConversationReady, uIMessageItem);
			}
		}
	}

	protected virtual void AddMessageBoardMessages(MessageType type = MessageType.Post)
	{
		List<Message> list = GetMessages<Message>().FindAll((Message x) => x.MessageType == type);
		if (list == null || list.Count == 0)
		{
			SetNoMessageTextVisible(visible: true);
			return;
		}
		foreach (Message item in list)
		{
			if (mIsMyBoard || !item.isPrivate || !(item.Creator != UserInfo.pInstance.UserID))
			{
				UIMessageItem parentItem = AddMessageBoardItem(_MessageListItemTemplate, item);
				LoadConversation(item.ConversationID, OnConversationReady, parentItem);
			}
		}
	}

	public void AddConversation(int conversationID, Message message, bool addLast = false)
	{
		if (!mConversations.ContainsKey(conversationID))
		{
			mConversations[conversationID] = new List<Message>();
		}
		if (!mConversations[conversationID].Contains(message))
		{
			if (addLast)
			{
				mConversations[conversationID].Add(message);
			}
			else
			{
				mConversations[conversationID].Insert(0, message);
			}
		}
	}

	public void RemoveConversation(int messageID, Message message)
	{
		if (mConversations.ContainsKey(messageID))
		{
			mConversations[messageID].Remove(message);
		}
	}

	public void ClearConversation()
	{
		mConversations.Clear();
	}

	private void LoadConversation(int conversationID, Action<List<Message>, UIWidget> callBackEvent, UIWidget parentItem)
	{
		mConversationReadyEventList[conversationID] = callBackEvent;
		if (mConversations.ContainsKey(conversationID) && mConversationReadyEventList.ContainsKey(conversationID))
		{
			mConversationReadyEventList[conversationID](mConversations[conversationID], parentItem);
		}
	}

	private void OnConversationReady(List<Message> messages, UIWidget parentItem)
	{
		pState = WidgetState.INTERACTIVE;
		if (messages == null)
		{
			return;
		}
		if (_ThreadUI.pVisible)
		{
			messages = messages.OrderBy((Message x) => x.CreateTime).ToList();
			foreach (Message message in messages)
			{
				_ThreadUI._Menu.AddWidget(message.ReplyToMessageID.HasValue ? _MessageThreadReplyItemTemplate.Duplicate(autoAddToSameParent: true) : _MessageThreadItemTemplate.Duplicate(autoAddToSameParent: true));
				((UIMessageItem)_ThreadUI._Menu.pChildWidgets[_ThreadUI._Menu.pChildWidgets.Count - 1]).Populate(message);
			}
		}
		if (parentItem is UIMessageItem)
		{
			UIMessageItem uIMessageItem = (UIMessageItem)parentItem;
			if (messages.Count > 1)
			{
				uIMessageItem?.SetReplyCountFields(messages.Count);
			}
		}
		if (mIsMyBoard)
		{
			_ThreadUI._DeleteBtn.pVisible = true;
		}
	}

	private void AddChallengeMessages()
	{
		List<ChallengeInfo> messages = GetMessages<ChallengeInfo>();
		List<object> list = mCombinedListMessages.FindAll((object t) => t is MessageInfo && (t as MessageInfo).MessageTypeID.Value == 14);
		foreach (MessageInfo item in list)
		{
			((UIMessageItem)_Menu._Template.Duplicate(autoAddToSameParent: true)).Populate(item);
		}
		if (messages.Count == 0 && list.Count == 0)
		{
			SetNoMessageTextVisible(visible: true);
			return;
		}
		foreach (ChallengeInfo item2 in messages)
		{
			AddChallengeItem(item2);
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

	private void AddGiftItem(MessageInfo inMessageInfo)
	{
		if (!SetGiftMessageExpired(inMessageInfo))
		{
			((UIMessageItem)_Menu._Template.Duplicate(autoAddToSameParent: true)).Populate(inMessageInfo);
		}
	}

	private bool SetGiftMessageExpired(MessageInfo info)
	{
		if (info == null || !GiftManager.pIsReady)
		{
			return false;
		}
		GiftData giftDataByMessageID = GiftManager.pInstance.GetGiftDataByMessageID(info.MessageID.Value);
		if (giftDataByMessageID != null && !giftDataByMessageID.pExpired)
		{
			return false;
		}
		if (mRemovedGiftMessages != null)
		{
			mRemovedGiftMessages.Add(info);
			return true;
		}
		return false;
	}

	public void AddChallengeItem(ChallengeInfo challenge)
	{
		if (challenge != null)
		{
			((UIMessageItem)_Menu._Template.Duplicate(autoAddToSameParent: true)).Populate(challenge);
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

	private UIMessageItem AddAnnouncementItem(Announcement announcement)
	{
		UIMessageItem obj = (UIMessageItem)_Menu._Template.Duplicate(autoAddToSameParent: true);
		obj.Populate(announcement);
		return obj;
	}

	public void ShowAchievementMessages()
	{
		_Menu.pVisible = false;
		SetNoMessageTextVisible(visible: false);
		_PostBtn.pVisible = false;
		_ThreadUI.pVisible = false;
		_GiftForm?.OnClose();
		_AnnouncementForm.pVisible = false;
		_ChallengeForm.pVisible = false;
		_ThreadUpdateForm.pVisible = false;
		_ChallengeResultsForm.pVisible = false;
		_PostBtn.pVisible = false;
		if (_AchievementsUIMenu.pChildWidgets.Count > 1)
		{
			_AchievementsUIMenu.pVisible = true;
			return;
		}
		List<MessageInfo> list = GetMessages<MessageInfo>().FindAll((MessageInfo t) => t.MessageTypeID == 9 || t.MessageTypeID.Value == 4);
		if (list.Count == 0)
		{
			SetNoMessageTextVisible(visible: true);
			return;
		}
		foreach (MessageInfo item in list)
		{
			(_AchievementsUIPopulator.Duplicate(autoAddToSameParent: true) as UISystemMessage)?.InitFields(item, _TagAndDefaultText);
		}
		_AchievementsUIMenu.pVisible = true;
	}

	private void RemoveMessageInfo(MessageInfo mInfo)
	{
		if (mInfo != null)
		{
			WsWebService.SaveMessage(mInfo.UserMessageQueueID.Value, isNew: false, isDeleted: true, OnMessageRemoved, null);
		}
	}

	private void OnMessageRemoved(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			SetInteractive(interactive: true);
			if (mSelectedItem != null)
			{
				if (mSelectedItem.pParentUI != null)
				{
					mSelectedItem.pParentUI.RemoveWidget(mSelectedItem, destroy: true);
				}
				else
				{
					_Menu.RemoveWidget(mSelectedItem, destroy: true);
				}
			}
			_GiftForm.OnClose();
			_ThreadUpdateForm.pVisible = false;
			_ThreadUI.ClearMessageItems();
			_ChallengeResultsForm.pVisible = false;
			if ((bool)mSelectedItem)
			{
				if (mSelectedItem.pData != null)
				{
					mCombinedListMessages.Remove(mSelectedItem.pData);
				}
				UnityEngine.Object.Destroy(mSelectedItem);
			}
			UpdateMessageListCount();
			break;
		case WsServiceEvent.ERROR:
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			SetInteractive(interactive: true);
			UtDebug.LogError("OnMessageRemoved failed!");
			break;
		}
	}

	private void OnDestroy()
	{
		if ((bool)_PostUI)
		{
			UnityEngine.Object.Destroy(_PostUI.gameObject);
		}
		mCombinedMessageType = CombinedMessageType.NONE;
		mMessageInfoType = -1;
		_GiftForm.OnClaim = null;
	}

	public void Close()
	{
		if (!string.IsNullOrEmpty(mLastLevel) && RsResourceManager.pCurrentLevel == GameConfig.GetKeyData("MessageBoardScene"))
		{
			pVisible = false;
			AvAvatar.pStartLocation = AvAvatar.pSpawnAtSetPosition;
			RsResourceManager.LoadLevel(mLastLevel);
			mLastLevel = "";
			return;
		}
		if (UiProfile.pInstance != null)
		{
			UiProfile.pInstance.CloseUI();
		}
		UnityEngine.Object.Destroy(base.gameObject);
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
	}

	protected virtual void UpdateCall()
	{
		if (!mLoading)
		{
			return;
		}
		if (pMessageBoard != null && pMessageBoard.pIsReady)
		{
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			mLoading = false;
			if (RsResourceManager.pCurrentLevel == GameConfig.GetKeyData("MessageBoardScene"))
			{
				RsResourceManager.DestroyLoadScreen();
			}
			if (pVisible)
			{
				CombinedMessageType type = (mIsMyBoard ? CombinedMessageType.ALL : CombinedMessageType.MESSAGE_BOARD);
				ProcessMessageType(type);
				UiToolbar.pMessageCount = 0;
			}
		}
		else if (pMessageBoard != null && pMessageBoard.pIsError)
		{
			mLoading = false;
			UIUtil.DisplayGenericDB("PfUIGenericDB", _GenericErrorText.GetLocalizedString(), _GenericErrorText.GetLocalizedString(), base.gameObject, "OnError");
		}
	}

	protected override void Update()
	{
		base.Update();
		UpdateCall();
		if (_Menu != null && _Menu.pChildWidgets.Count > 0)
		{
			for (int i = 0; i < _Menu.pChildWidgets.Count; i++)
			{
				UIWidget uIWidget = _Menu.pChildWidgets[i];
				if (uIWidget.pData != null && !(uIWidget.pData.GetType() != typeof(ChallengeInfo)))
				{
					ChallengeInfo challengeInfo = (ChallengeInfo)uIWidget.pData;
					if (challengeInfo != null && !EventManager.IsChallengeValid(challengeInfo))
					{
						WsWebService.RejectChallenge(challengeInfo.ChallengeID, -1, null, null);
						_Menu.RemoveWidget(uIWidget, destroy: true);
						RemoveFromCombinedMessageList(CombinedMessageType.CHALLENGE, challengeInfo);
						UpdateMessageListCount();
					}
				}
			}
		}
		if (BuddyList.pIsReady && !mBuddyCheckApplied)
		{
			mBuddyCheckApplied = true;
			if (!mIsMyBoard && BuddyList.pInstance.GetBuddyStatus(mUserID) != BuddyStatus.Approved)
			{
				_PostBtn.pState = WidgetState.DISABLED;
			}
		}
	}

	protected override void OnClick(UIWidget inWidget, PointerEventData eventData)
	{
		base.OnClick(inWidget, eventData);
		_Menu.pVisible = true;
		if (_Menu.pChildWidgets.Contains(inWidget.pParentWidget) || _ThreadUI._Menu.pChildWidgets.Contains(inWidget.pParentWidget) || _AchievementsUIMenu.pChildWidgets.Contains(inWidget.pParentWidget))
		{
			mSelectedItem = inWidget.pParentWidget;
		}
		UIWidget pParentWidget = inWidget.pParentWidget;
		string text = inWidget.name;
		string text2 = ((pParentWidget != null) ? pParentWidget.name : "");
		if (mSelectedItem != null && mSelectedItem is UIMessageItem)
		{
			(mSelectedItem as UIMessageItem).pChecked = true;
		}
		else if (pParentWidget != null && pParentWidget is UIMessageItem)
		{
			(pParentWidget as UIMessageItem).pChecked = true;
		}
		if (inWidget == _CloseBtn)
		{
			Close();
			return;
		}
		if (inWidget == _PostBtn)
		{
			if (UiLogin.pIsGuestUser)
			{
				OpenRegisterNowUI();
			}
			else if (_PostUI != null)
			{
				_PostUI.SetVisibility(mUserID, UserProfile.pProfileData.ID, -1, !mIsMyBoard);
			}
			return;
		}
		if (pParentWidget != null && pParentWidget.pData != null && pParentWidget.pData is Message)
		{
			mSelectedMessage = (Message)pParentWidget.pData;
			OnClickThreadParent();
			return;
		}
		_ThreadUI.pVisible = false;
		_ThreadUI._Menu.ClearChildren();
		if (inWidget == _ThreadUI._ReplyBtn)
		{
			OnClickReply(mSelectedMessage);
		}
		else if (inWidget == _ThreadUI._IgnoreBtn)
		{
			OnClickIgnorePlayer(mSelectedMessage);
		}
		else if (inWidget == _ThreadUI._ReportBtn)
		{
			OnClickReportPlayer(mSelectedMessage.Creator);
		}
		else if ((bool)pParentWidget && pParentWidget.pData != null && pParentWidget.pData is ChallengeInfo)
		{
			ChallengeInfo inData = (mCurrentChallenge = (ChallengeInfo)pParentWidget.pData);
			if ((bool)_AnnouncementForm)
			{
				_AnnouncementForm.pVisible = false;
			}
			_GiftForm?.OnClose();
			if ((bool)_PostBtn)
			{
				_PostBtn.pVisible = false;
			}
			_ChallengeForm.Populate(inData);
			_ChallengeResultsForm.pVisible = false;
		}
		else if (text2 == "ChallengeResponseButtonGrp")
		{
			if (text == "Btn_Accept")
			{
				OnAcceptChallenge();
			}
			else if (text == "Btn_Decline")
			{
				ShowDeleteConfirmation(base.gameObject, "OnDeclineChallengeConfirm");
			}
		}
		else if (inWidget.name == "Btn_Delete")
		{
			if (mSelectedItem.pData is UiMessageInfoUserData)
			{
				mMessageInfo = (mSelectedItem.pData as UiMessageInfoUserData).GetMessageInfo();
			}
			else if (mSelectedItem.pData is MessageInfo)
			{
				mMessageInfo = mSelectedItem.pData as MessageInfo;
			}
			ShowDeleteConfirmation(base.gameObject, "OnDelete");
		}
		else if ((bool)pParentWidget && pParentWidget.pData != null && pParentWidget.pData is MessageInfo)
		{
			if (inWidget == _GiftForm._ClaimBtn)
			{
				_GiftForm.AttemptClaim();
				return;
			}
			if (inWidget == _GiftForm._DeleteBtn)
			{
				UIUtil.DisplayGenericDB("PfUIGenericDB", m_GiftMessageDeleteConfirmationText.GetLocalizedString(), _DeleteMessageTitleText.GetLocalizedString(), base.gameObject, null, "OnGiftMessageDelete", "OnCancelPrompt");
				return;
			}
			mMessageInfo = (MessageInfo)pParentWidget.pData;
			if (mMessageInfo == null)
			{
				return;
			}
			if (inWidget.name == "Btn_ViewMessage")
			{
				Dictionary<string, string> dictionary = TaggedMessageHelper.Match(mMessageInfo.Data);
				if (RsResourceManager.pCurrentLevel == GameConfig.GetKeyData("MessageBoardScene"))
				{
					UnityEngine.Object.Destroy(base.gameObject);
					MessageBoardLoader.Load(dictionary["OwnerID"], int.Parse(dictionary["MessageID"]));
				}
				else
				{
					Close();
					MessageBoardLoader.Load(dictionary["OwnerID"], int.Parse(dictionary["MessageID"]));
				}
				return;
			}
			_PostBtn.pVisible = false;
			_GiftForm.pVisible = false;
			_ChallengeForm.pVisible = false;
			_ChallengeResultsForm.pVisible = false;
			_ThreadUpdateForm.pVisible = false;
			if ((bool)_AnnouncementForm)
			{
				_AnnouncementForm.pVisible = false;
			}
			if (mMessageInfo.MessageTypeID == 19)
			{
				_AnnouncementForm.pVisible = false;
				_GiftForm.Initialize(mMessageInfo);
				UITerrorMailMessageGifts giftForm = _GiftForm;
				giftForm.OnClaim = (Action<MessageInfo>)Delegate.Combine(giftForm.OnClaim, new Action<MessageInfo>(OnGiftClaimed));
			}
			else if (mMessageInfo.MessageTypeID == 14)
			{
				if ((bool)_AnnouncementForm)
				{
					_AnnouncementForm.pVisible = false;
				}
				_GiftForm?.OnClose();
				if ((bool)_PostBtn)
				{
					_PostBtn.pVisible = false;
				}
				_ChallengeForm.pVisible = false;
				_ChallengeResultsForm.Populate(mMessageInfo);
				_ChallengeResultsForm.pVisible = true;
			}
			else if (mMessageInfo.MessageTypeID == 21)
			{
				_ThreadUpdateForm.pVisible = true;
				_ThreadUpdateForm.Populate(mMessageInfo);
			}
		}
		else if ((bool)pParentWidget && pParentWidget.pData != null && pParentWidget.pData is Announcement)
		{
			_AnnouncementForm.Populate(pParentWidget.pData);
		}
		else if (text2 == "ThreadUpdateMessage" && inWidget == _ThreadUI._DeleteBtn)
		{
			ShowDeleteConfirmation(base.gameObject, "OnThreadUpdateMessageDelete");
		}
		else if (inWidget == _TabUI._AchievementTabBtn)
		{
			ShowAchievementMessages();
		}
		else if (inWidget == _TabUI._GiftTabBtn)
		{
			ProcessMessageType(_TabUI._TabBtnTypes.Find((TabType t) => t._Tab == _TabUI._GiftTabBtn)._TabMessageType, 19);
		}
		else if (_TabUI._TabBtnTypes.Find((TabType t) => t._Tab == inWidget) != null)
		{
			ProcessMessageType(_TabUI._TabBtnTypes.Find((TabType t) => t._Tab == inWidget)._TabMessageType, -1, force: true);
		}
	}

	public void ShowDeleteConfirmation(GameObject inMessageObject, string inConfirmMessage)
	{
		UIUtil.DisplayGenericDB("PfUIGenericDB", _DeleteMessageText.GetLocalizedString(), _DeleteMessageTitleText.GetLocalizedString(), inMessageObject, null, inConfirmMessage, "OnClose");
	}

	public void OnClickReply(Message inSelectedMessage = null)
	{
		if (UiLogin.pIsGuestUser)
		{
			OpenRegisterNowUI();
			return;
		}
		if (inSelectedMessage == null)
		{
			inSelectedMessage = mSelectedMessage;
		}
		string text = "";
		if (inSelectedMessage.Creator != mUserID && BuddyList.pInstance != null && BuddyList.pIsReady)
		{
			Buddy buddy = BuddyList.pInstance.GetBuddy(inSelectedMessage.Creator);
			if (buddy != null)
			{
				text = buddy.UserID;
			}
		}
		else
		{
			text = mUserID;
		}
		if (_PostUI != null)
		{
			_PostUI._Board = this;
			_PostUI.SetVisibility(mUserID, (inSelectedMessage == null) ? UserProfile.pProfileData.ID : inSelectedMessage.Creator, inSelectedMessage?.MessageID.Value ?? (-1), inSelectedMessage != null || !mIsMyBoard, inSelectedMessage != null);
			_PostUI.UpdateToggleState(inSelectedMessage?.isPrivate ?? false);
		}
		else
		{
			_PostUI.SetVisibility(mUserID, text, inSelectedMessage.MessageID.Value, inBlockDropdown: true, inBlockToggles: true);
			_PostUI.UpdateToggleInteractivity(inSelectedMessage.isPrivate);
			_PostUI.UpdateToggleState(inSelectedMessage.isPrivate);
		}
	}

	public void OnClickIgnorePlayer(Message inSelectedMessage = null)
	{
		if (inSelectedMessage == null)
		{
			inSelectedMessage = mSelectedMessage;
		}
		mSelectedMessage = inSelectedMessage;
		UIUtil.DisplayGenericDB("PfUIGenericDB", _IgnorePlayerTitleText.GetLocalizedString(), _IgnorePlayerText.GetLocalizedString(), base.gameObject, null, "OnIgnore", "OnClose");
	}

	public void OnClickReportPlayer(string inUserID = null)
	{
		if (string.IsNullOrEmpty(inUserID))
		{
			inUserID = mSelectedMessage.Creator;
		}
		UiModeratorDB component = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfUiModeratorDB")).GetComponent<UiModeratorDB>();
		component.SetVisibility(ModeratorType.REPORT);
		component._UserID = inUserID;
		component._MessageObject = base.gameObject;
		pVisible = false;
	}

	public void OnModeratorClose()
	{
		pVisible = true;
	}

	public void OnClickThreadParent()
	{
		if ((bool)_AnnouncementForm)
		{
			_AnnouncementForm.pVisible = false;
		}
		_GiftForm.OnClose();
		_ChallengeForm.pVisible = false;
		_ThreadUpdateForm.pVisible = false;
		_ChallengeResultsForm.pVisible = false;
		if ((bool)_PostBtn)
		{
			_PostBtn.pVisible = true;
		}
		_ThreadUI.pVisible = true;
		_ThreadUI._Menu.ClearChildren();
		_ThreadUI._DeleteBtn.pVisible = false;
		BuddyStatus buddyStatus = BuddyStatus.Unknown;
		try
		{
			LoadConversation(mSelectedMessage.ConversationID, OnConversationReady, (mSelectedItem.pParentWidget == null) ? mSelectedItem : (mSelectedItem.pParentWidget as UIMessageItem));
		}
		catch (Exception)
		{
			buddyStatus = BuddyList.pInstance.GetBuddyStatus(mUserID);
			_ThreadUI._IgnoreBtn.pState = ((buddyStatus != BuddyStatus.BlockedBySelf && buddyStatus != BuddyStatus.BlockedByBoth && buddyStatus != 0) ? WidgetState.INTERACTIVE : WidgetState.DISABLED);
			_ThreadUI._ReplyBtn.pState = WidgetState.DISABLED;
			_ThreadUI._ReportBtn.pState = WidgetState.DISABLED;
			ShowMessageDB(_MessageUnavailableText);
			return;
		}
		if (BuddyList.pIsReady)
		{
			buddyStatus = BuddyList.pInstance.GetBuddyStatus(mSelectedMessage.Creator);
		}
		_ThreadUI._IgnoreBtn.pState = ((!(mSelectedMessage.Creator == UserInfo.pInstance.UserID) && buddyStatus != BuddyStatus.BlockedBySelf && buddyStatus != BuddyStatus.BlockedByBoth && buddyStatus != 0) ? WidgetState.INTERACTIVE : WidgetState.DISABLED);
		_ThreadUI._ReplyBtn.pState = ((mIsMyBoard || BuddyList.pInstance.GetBuddyStatus(mUserID) == BuddyStatus.Approved) ? WidgetState.INTERACTIVE : WidgetState.DISABLED);
		_ThreadUI._ReportBtn.pState = ((!(mSelectedMessage.Creator == mUserID)) ? WidgetState.INTERACTIVE : WidgetState.DISABLED);
	}

	private void OnCancelPrompt()
	{
		pVisible = true;
	}

	private void OnGiftMessageDelete()
	{
		if (mMessageInfo != null)
		{
			_Menu.RemoveWidget(mSelectedItem, destroy: true);
			RemoveFromCombinedMessageList(CombinedMessageType.USER_MESSAGE_QUEUE, mMessageInfo);
			UpdateMessageListCount();
			_GiftForm?.OnClose();
		}
	}

	private void OnGiftClaimed(MessageInfo inMessageInfo)
	{
		UIMessageItem uIMessageItem = _Menu.pChildWidgets.Find((UIWidget t) => t.pData is MessageInfo && ((MessageInfo)t.pData).MessageID == inMessageInfo.MessageID) as UIMessageItem;
		if ((bool)uIMessageItem)
		{
			uIMessageItem._GiftOpenedWidget.pVisible = true;
			uIMessageItem._GiftUnopenedWidget.pVisible = false;
		}
	}

	public void OnAcceptChallenge()
	{
		if (mCurrentChallenge == null)
		{
			return;
		}
		ChallengeInfo challengeInfo = mCurrentChallenge;
		ChallengePetData petData = ChallengeInfo.GetPetData(challengeInfo.ChallengeGameInfo.GameID);
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
		if (challengeInfo.GetState() == ChallengeState.Initiated)
		{
			UserAchievementTask.Set(_AcceptChallengeAchievementID);
		}
		if (UiJournal.pInstance != null && UiJournal.pIsJournalActive)
		{
			UiJournal.pInstance.CloseJournal();
		}
		UiChatHistory.SystemMessageAccepted(challengeInfo);
		WsWebService.AcceptChallenge(challengeInfo.ChallengeID, -1, null, null);
		ChallengeInfo.pActiveChallenge = challengeInfo;
		PetPlayAreaLoader._ExitToScene = RsResourceManager.pCurrentLevel;
		RsResourceManager.LoadLevel(ChallengeInfo.GetSceneName(challengeInfo.ChallengeGameInfo.GameID));
		mCurrentChallenge = null;
	}

	public void OnDeclineChallengeConfirm()
	{
		if (mCurrentChallenge != null && mCurrentChallenge != null)
		{
			UiChatHistory.SystemMessageAccepted(mCurrentChallenge);
			WsWebService.RejectChallenge(mCurrentChallenge.ChallengeID, -1, null, null);
			_Menu.RemoveWidget(mSelectedItem, destroy: true);
			RemoveFromCombinedMessageList(CombinedMessageType.CHALLENGE, mCurrentChallenge);
			UpdateMessageListCount();
			_ChallengeForm.pVisible = false;
			_ThreadUpdateForm.pVisible = false;
			mCurrentChallenge = null;
		}
	}

	public void SetUIDisabled(bool isDisable)
	{
		pState = ((!isDisable) ? WidgetState.INTERACTIVE : WidgetState.DISABLED);
		if (_Menu != null)
		{
			_Menu.pState = pState;
		}
	}

	private void ShowMessageDB(LocaleString inText)
	{
		SetUIDisabled(isDisable: true);
		UIUtil.DisplayGenericDB("PfUIGenericDB", inText.GetLocalizedString(), inText.GetLocalizedString(), base.gameObject, "OnDBClose");
	}

	private void OnDBClose()
	{
		SetUIDisabled(isDisable: false);
	}

	public UIMessageItem AddMessageBoardItem(UIMessageItem template, object messageData, bool setAsFirst = false)
	{
		Message message = messageData as Message;
		MessageInfo messageInfo = messageData as MessageInfo;
		if (template.pParentUI == null)
		{
			template.Initialize(_Menu, null);
		}
		bool flag = true;
		NPCFriendData[] nPCFriends = _NPCFriends;
		for (int i = 0; i < nPCFriends.Length; i++)
		{
			if (nPCFriends[i]._UserID == ((message == null) ? messageInfo.FromUserID : message.Creator))
			{
				flag = false;
				_ThreadUI.UpdateSocialButtonVisibility(showIgnoreBtn: false, showReplyBtn: false, showReportBtn: false, showDeleteBtn: true);
				break;
			}
		}
		UIMessageItem uIMessageItem = (UIMessageItem)template.Duplicate(autoAddToSameParent: true);
		if (setAsFirst)
		{
			uIMessageItem.transform.SetAsFirstSibling();
		}
		if (!GetMessages<Message>().Contains(message))
		{
			mCombinedListMessages.Add(message);
		}
		BuddyStatus buddyStatus = BuddyStatus.Unknown;
		if (BuddyList.pIsReady)
		{
			buddyStatus = BuddyList.pInstance.GetBuddyStatus((message == null) ? messageInfo.FromUserID : message.Creator);
		}
		if ((buddyStatus == BuddyStatus.BlockedBySelf || buddyStatus == BuddyStatus.BlockedByBoth) && ((message == null) ? messageInfo.FromUserID : message.Creator) != mUserID)
		{
			_ThreadUI.UpdateSocialButtonVisibility(showIgnoreBtn: false, showReplyBtn: false, showReportBtn: true, showDeleteBtn: true);
		}
		uIMessageItem.Populate(messageData);
		if (flag)
		{
			SetMessageItemButtons(uIMessageItem);
		}
		return uIMessageItem;
	}

	public virtual void SetMessageItemButtons(UIWidget item)
	{
		Message message = item.pData as Message;
		MessageInfo messageInfo = item.pData as MessageInfo;
		BuddyStatus buddyStatus = BuddyStatus.Unknown;
		if (BuddyList.pIsReady)
		{
			buddyStatus = BuddyList.pInstance.GetBuddyStatus((message == null) ? messageInfo.FromUserID : message.Creator);
		}
		if (!mIsMyBoard)
		{
			_ThreadUI.UpdateSocialButtonVisibility(showIgnoreBtn: false, showReplyBtn: true, showReportBtn: false, showDeleteBtn: false);
		}
		else if (((message == null) ? messageInfo.FromUserID : message.Creator) == UserInfo.pInstance.UserID || buddyStatus == BuddyStatus.BlockedBySelf || buddyStatus == BuddyStatus.BlockedByBoth || buddyStatus == BuddyStatus.Unknown)
		{
			_ThreadUI._IgnoreBtn.pState = WidgetState.DISABLED;
		}
		buddyStatus = BuddyStatus.Unknown;
		if (BuddyList.pIsReady)
		{
			buddyStatus = BuddyList.pInstance.GetBuddyStatus(mUserID);
		}
		if (!mIsMyBoard && buddyStatus != BuddyStatus.Approved)
		{
			_ThreadUI._ReplyBtn.pState = WidgetState.DISABLED;
		}
		if (((message == null) ? messageInfo.FromUserID : message.Creator) == UserInfo.pInstance.UserID)
		{
			_ThreadUI._ReportBtn.pState = WidgetState.DISABLED;
		}
	}

	public void OnError()
	{
		Close();
	}

	public void OnClose()
	{
	}

	private void OnThreadUpdateMessageDelete()
	{
		pVisible = true;
		MessageInfo messageInfo = (MessageInfo)mSelectedItem.pData;
		if (messageInfo != null)
		{
			_Menu.RemoveWidget(mSelectedItem, destroy: true);
			RemoveFromCombinedMessageList(CombinedMessageType.USER_MESSAGE_QUEUE, messageInfo);
			UpdateMessageListCount();
		}
	}

	public virtual void OnDelete()
	{
		UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
		SetInteractive(interactive: false);
		if (mSelectedMessage != null)
		{
			pMessageBoard.DeleteMessage(mSelectedMessage.MessageID.Value, DeleteEventHandler);
			return;
		}
		if (mMessageInfo != null)
		{
			RemoveMessageInfo(mMessageInfo);
			return;
		}
		UtDebug.LogError("No mSelectedUserData or mMessageInfo found!");
		UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
		pVisible = true;
	}

	public void OnIgnore()
	{
		UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
		SetInteractive(interactive: false);
		BuddyList.pInstance.BlockBuddy(mSelectedMessage.Creator, IgnoreEventHandler);
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

	public virtual void OnPostSuccess(Message message, int replyToMessageID)
	{
		if (mCombinedListMessages.FirstOrDefault((object x) => x is Message && ((Message)x).MessageID.HasValue && ((Message)x).MessageID.Value == replyToMessageID) is Message message2)
		{
			AddConversation(message2.ConversationID, message, addLast: true);
			UIMessageItem uIMessageItem = mSelectedItem as UIMessageItem;
			int.TryParse(uIMessageItem._MessageReplyCountWidget.pText, out var result);
			result++;
			if (result > 1)
			{
				uIMessageItem.SetReplyCountFields(result);
			}
			_ThreadUI.ClearMessageItems();
			message.ReplyToMessageID = replyToMessageID;
			LoadConversation(message2.ConversationID, OnConversationReady, (mSelectedItem != null) ? mSelectedItem : mSelectedItem.pParentWidget);
			_ThreadUI.pVisible = true;
		}
	}

	public virtual void DeleteEventHandler(bool success)
	{
		UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
		SetInteractive(interactive: true);
		pVisible = true;
		if (!success)
		{
			UIUtil.DisplayGenericDB("PfUIGenericDB", _GenericErrorText.GetLocalizedString(), null, base.gameObject, "OnClose");
			return;
		}
		if (mSelectedMessage == null && mSelectedItem == null)
		{
			UtDebug.LogError("No mSelectedUserData or mMessageInfo found!");
			return;
		}
		_Menu.RemoveWidget(mSelectedItem, destroy: true);
		_ThreadUI.ClearMessageItems();
		_ThreadUI.pVisible = false;
		_ThreadUpdateForm.pVisible = false;
		RemoveFromCombinedMessageList(CombinedMessageType.MESSAGE_BOARD, (mSelectedMessage != null) ? ((object)mSelectedMessage) : ((object)mSelectedItem));
		UpdateMessageListCount();
		if (mSelectedMessage != null && mSelectedMessage.ReplyToMessageID.HasValue)
		{
			RemoveConversation(mSelectedMessage.ReplyToMessageID.Value, mSelectedMessage);
		}
	}

	private void IgnoreEventHandler(WsServiceType inType, object inResult)
	{
		UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
		SetInteractive(interactive: true);
		if (inResult == null)
		{
			UIUtil.DisplayGenericDB("PfUIGenericDB", _GenericErrorText.GetLocalizedString(), null, base.gameObject, "OnClose");
			return;
		}
		Message message = (Message)mSelectedItem.pData;
		IgnorePlayer(message.Creator);
	}

	public void IgnorePlayer(string userID)
	{
		for (int i = 0; i < _Menu.pChildWidgets.Count; i++)
		{
			UIMessageItem component = _Menu._Content.transform.GetChild(i).GetComponent<UIMessageItem>();
			if (component.pData != null && !(component.pData.GetType() != typeof(Message)))
			{
				Message message = (Message)component.pData;
				if (!(message.Creator != userID))
				{
					component.Populate(message);
					_ThreadUI._ReplyBtn.pVisible = false;
					SetMessageItemButtons(component);
				}
			}
		}
	}
}
