using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UiChatHistory : KAUI
{
	[Serializable]
	public class ChatTabFlashInfo
	{
		public ChatOptions _ChatOption;

		public KAWidget _ChatTabWidget;

		public float _FlashDuration = 2f;

		[HideInInspector]
		public bool _FlashPending;

		[HideInInspector]
		public float _CurrentFlashTime;
	}

	[Serializable]
	public class MessageAction
	{
		public string _MessageType;

		public string _MessageSubType;

		public int _MessageTypeID;

		public LocaleString _Action;

		public bool _IsChallenge;
	}

	public class ChatMessage
	{
		public string _Message;

		public ChatOptions _Channel;

		public object _MessageInfoObject;

		public Action<object> _Callback;

		public KAWidget _Widget;

		public ChatMessage(string message, ChatOptions channel)
		{
			_Message = message;
			_Channel = channel;
		}
	}

	public class AnalyticsChatEventParams
	{
		public const string CloseRegisterPrompt = "Closed Register Prompt";

		public const string AcceptRegisterPrompt = "Accepted Register Prompt";

		public const string RegisterAfterPrompt = "Registered After Prompt";
	}

	public delegate void OnChatUIClosed();

	public static bool _IsVisible = true;

	public static GameObject _Listener;

	private static UiChatHistory mInstance = null;

	private static List<ChatMessage> mDisplayHistory = new List<ChatMessage>();

	private static List<int> mFirstSessionSystemMessages = new List<int>();

	private static int mMaxHistorySize = 100;

	private static string mNameSeparator = ">";

	public const string DEFAULT_BLOCKED_TEXT_COLOR = "FF0000";

	private static Vector3 mChatWindowPosition;

	private static Vector3 mChatExpandBtnPosition;

	private static bool mUsedChat = false;

	private static bool mChanged = false;

	public static bool mFade = false;

	public LocaleString _SystemText = new LocaleString("[c]System");

	public Vector3 _ChatWindowPos;

	public Vector3 _ChatExpandBtnPos;

	public Vector2 _ChatWindowMaxSize = new Vector2(500f, 300f);

	public float _FlashDuration = 2f;

	public List<KAWidget> _WidgetsToFlash;

	public List<ChatTabFlashInfo> _ChatTabFlashInfo;

	public UiMessageLog _UiMessageLog;

	public KAUIMenu _ChatMenu;

	public KAScrollBar _ScrollBar;

	public KAWidget _ChatBackground;

	public KAWidget _DragBtn;

	public KAEditBox _ChatBox;

	public int _ChatEntryEndPadding = 3;

	public int _ID = 1000;

	public string _NameSeparator = ">";

	public string _SystemMessagesExcludedScene = "DragonRacingDO";

	public string _RoomChatOptionSceneName = "DragonRacingDO";

	public LocaleString _TitleText;

	public UITextList _TextList;

	public bool _ExcludeSystemMessages;

	public bool _ClearChatOnSceneLoad = true;

	public int _MaxChatCharacters = 80;

	public LocaleString _TouchInputText = new LocaleString("Tap");

	public LocaleString _NonTouchInputText = new LocaleString("Click");

	public LocaleString _SystemMessageActionText = new LocaleString("[[input]] [-]HERE[c] to [[action]]");

	public LocaleString _BlockedWordWarningText = new LocaleString("Stoick > We do not allow that type of language in School of Dragons. Please do not use iswords like that!");

	public LocaleString _ServerErrorText = new LocaleString("Server error.");

	public LocaleString _ServerErrorTitleText = new LocaleString("Error");

	public LocaleString _MultiplayerDisabledOnDeviceText = new LocaleString("You need to turn multiplayer on. You can do that in your settings.");

	public LocaleString _MultiplayerDisabledOnServerText = new LocaleString("You need to turn multiplayer on. You can do that from your account at www.schoolofdragons.com.");

	public LocaleString _SafeChatDisabledOnServerText = new LocaleString("[REVIEW] You need to turn Safe Chat on. You can do that from your account at www.schoolofdragons.com.");

	public Color _SystemMessageTextColor;

	public Color _ActiveLinkTextColor;

	private KAToggleButton mBtnChatHistoryTabRoom;

	private KAToggleButton mBtnChatHistoryTabSystem;

	private KAToggleButton mBtnChatHistoryTabFriends;

	private KAToggleButton mBtnChatHistoryTabClans;

	private KAToggleButton mBtnChatEmoticons;

	private KAButton mCloseButton;

	private KAWidget mChatTextBkg;

	private UiToolbar mUiToolbar;

	private KAEditBox mEditChat;

	private KAWidget mChatEnterBtn;

	private KAUIGenericDB mUiGenericDB;

	private UISprite mChatBackgroundSprite;

	private Vector3 mOriginalDistance = Vector3.zero;

	private Vector2 mOriginalOffset = Vector2.zero;

	private Vector4 mClipRangeRef;

	private UIPanel mPanel;

	private bool mIsJoystickVisibilityChanged;

	private int mOriginalBarHeight;

	private int mOriginalChatTextBkgWidth;

	private bool mPendingChatButtonFlash;

	private bool mPendingChatTabFlash;

	private bool mSetPreviousPosition;

	private float mFlashButtonDuration;

	private float mFlashTabCount;

	private ChatOptions mLastUsedChatOption;

	public static OnChatUIClosed pOnChatUIClosed = null;

	public LocaleString _MobileChatBoxDefaultText = new LocaleString("Tap here to chat");

	public string _UiChatRegisterNowPath;

	public static void Init()
	{
		mFirstSessionSystemMessages.Clear();
		mDisplayHistory.Clear();
	}

	public static void ShowChatHistory()
	{
		mChanged = true;
		_IsVisible = !_IsVisible;
		if (UtPlatform.IsWSA())
		{
			mInstance.mEditChat.pInput.defaultText = (UtUtilities.IsKeyboardAttached() ? mInstance.mEditChat._DefaultText.GetLocalizedString() : mInstance._MobileChatBoxDefaultText.GetLocalizedString());
		}
	}

	public static void HideChatHistory()
	{
		_IsVisible = false;
		if (pOnChatUIClosed != null)
		{
			pOnChatUIClosed();
		}
	}

	public static void WriteLine(string line, string from)
	{
		if (mInstance != null)
		{
			WriteLine(line, from, mInstance.mLastUsedChatOption);
		}
	}

	public static void AddSystemNotification(string line, object msgData = null, Action<object> callback = null, bool ignoreDuplicateMessage = false, string actionText = "")
	{
		if (mInstance == null || FUEManager.pIsFUERunning || RsResourceManager.pCurrentLevel.Equals(mInstance._SystemMessagesExcludedScene) || mInstance._ExcludeSystemMessages)
		{
			return;
		}
		string text = "";
		if (msgData != null)
		{
			if (ignoreDuplicateMessage)
			{
				int? displayedMessageID = ((MessageInfo)msgData).UserMessageQueueID;
				if (displayedMessageID.HasValue && mDisplayHistory.Find((ChatMessage x) => x._MessageInfoObject != null && x._MessageInfoObject.GetType() == typeof(MessageInfo) && ((MessageInfo)x._MessageInfoObject).UserMessageQueueID.Value == displayedMessageID.Value) != null)
				{
					return;
				}
			}
			string text2 = ((!string.IsNullOrEmpty(actionText)) ? actionText : mInstance.GetAction(msgData));
			if (!string.IsNullOrEmpty(text2))
			{
				text = mInstance._SystemMessageActionText.GetLocalizedString();
				text = " " + text.Replace("[[action]]", text2);
			}
		}
		if (mDisplayHistory.Count == mMaxHistorySize)
		{
			mDisplayHistory.RemoveAt(mDisplayHistory.Count - 1);
		}
		ChatMessage chatMessage = new ChatMessage((mInstance._SystemText.GetLocalizedString() + mNameSeparator + line + text).Replace("[[input]]", KAInput.pInstance.IsTouchInput() ? mInstance._TouchInputText.GetLocalizedString() : mInstance._NonTouchInputText.GetLocalizedString()), ChatOptions.CHAT_SYSTEM);
		chatMessage._MessageInfoObject = msgData;
		chatMessage._Callback = callback;
		mDisplayHistory.Add(chatMessage);
		mInstance.ShowMessage(chatMessage);
		mInstance.UpdateMessageLog();
		if (InteractiveTutManager._CurrentActiveTutorialObject == null)
		{
			mInstance.FlashChatButton(flash: true);
			mInstance.FlashChatTab(ChatOptions.CHAT_SYSTEM);
		}
	}

	public static bool IsSystemMessageShown(MessageInfo info)
	{
		if (info.UserMessageQueueID.HasValue)
		{
			return mFirstSessionSystemMessages.Contains(info.UserMessageQueueID.Value);
		}
		return false;
	}

	public static void AddAsFirstSessionSystemMessage(MessageInfo messageInfo)
	{
		if (messageInfo.UserMessageQueueID.HasValue)
		{
			mFirstSessionSystemMessages.Add(messageInfo.UserMessageQueueID.Value);
		}
	}

	public static void SystemMessageAccepted(object messageInfoObject, bool removeAll = false)
	{
		if (mInstance == null)
		{
			return;
		}
		foreach (ChatMessage item in new List<ChatMessage>(mDisplayHistory))
		{
			ChatMessage chatMessage = null;
			int num = -1;
			if (messageInfoObject.GetType() == typeof(MessageInfo))
			{
				if (((MessageInfo)messageInfoObject).UserMessageQueueID.HasValue)
				{
					num = ((MessageInfo)messageInfoObject).UserMessageQueueID.Value;
				}
			}
			else
			{
				num = ((ChallengeInfo)messageInfoObject).ChallengeID;
			}
			if (item._MessageInfoObject != null)
			{
				if (num > 0)
				{
					if (mFirstSessionSystemMessages.Count > 0)
					{
						mFirstSessionSystemMessages.Remove(num);
					}
					if (item._MessageInfoObject.GetType() == typeof(MessageInfo) && ((MessageInfo)item._MessageInfoObject).UserMessageQueueID.GetValueOrDefault() == num)
					{
						chatMessage = item;
					}
					else if (item._MessageInfoObject.GetType() == typeof(ChallengeInfo) && ((ChallengeInfo)item._MessageInfoObject).ChallengeID == num)
					{
						chatMessage = item;
					}
				}
				else if (item._MessageInfoObject == messageInfoObject)
				{
					chatMessage = item;
				}
			}
			if (chatMessage != null)
			{
				if ((ChatData.pCurrentChatOption == chatMessage._Channel || ChatData.pCurrentChatOption == ChatOptions.CHAT_ROOM) && chatMessage._Widget != null)
				{
					mInstance._ChatMenu.RemoveWidget(chatMessage._Widget);
				}
				mDisplayHistory.Remove(chatMessage);
				if (!removeAll)
				{
					break;
				}
			}
		}
		mInstance.UpdateMenu();
		mInstance.UpdateMessageLog();
	}

	public static void WriteLine(string line, string from, ChatOptions channel)
	{
		if (!FUEManager.pIsFUERunning && !(mInstance == null))
		{
			if (mDisplayHistory.Count == mMaxHistorySize)
			{
				mDisplayHistory.RemoveAt(mDisplayHistory.Count - 1);
			}
			if (!string.IsNullOrEmpty(from))
			{
				line = from + mNameSeparator + line;
			}
			ChatMessage chatMessage = new ChatMessage(line, channel);
			mDisplayHistory.Add(chatMessage);
			mInstance.ShowMessage(chatMessage);
			mInstance.UpdateMessageLog();
			if (AvatarData.pInstance.DisplayName.Equals(from))
			{
				mInstance._ChatMenu.pDragPanel.ResetPosition();
			}
			else if (InteractiveTutManager._CurrentActiveTutorialObject == null)
			{
				mInstance.FlashChatTab(channel);
			}
			if (_Listener != null)
			{
				_Listener.SendMessage("AddToChatHistory", line);
			}
		}
	}

	public static void DisplayBlockedTextWarningMessage()
	{
		if (mInstance != null)
		{
			WriteLine("[FF0000]" + mInstance._BlockedWordWarningText.GetLocalizedString() + "[-]", "", mInstance.mLastUsedChatOption);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (mInstance == null)
		{
			mInstance = this;
		}
	}

	protected override void Start()
	{
		if (AvAvatar.pToolbar != null)
		{
			mUiToolbar = AvAvatar.pToolbar.GetComponent<UiToolbar>();
		}
		mNameSeparator = _NameSeparator;
		mBtnChatHistoryTabRoom = (KAToggleButton)FindItem("ChatTabRoom");
		mBtnChatHistoryTabSystem = (KAToggleButton)FindItem("ChatTabSystem");
		mBtnChatHistoryTabFriends = (KAToggleButton)FindItem("ChatTabFriends");
		mBtnChatHistoryTabClans = (KAToggleButton)FindItem("ChatTabClans");
		mBtnChatEmoticons = (KAToggleButton)FindItem("ChatTabEmotes");
		mCloseButton = (KAButton)FindItem("ChatCloseBtn");
		if (_ChatBox != null)
		{
			mChatTextBkg = _ChatBox.FindChildItem("ChatTextBkg");
		}
		if (null != mBtnChatHistoryTabRoom)
		{
			mBtnChatHistoryTabRoom._StartChecked = false;
		}
		if (null != mBtnChatHistoryTabFriends)
		{
			mBtnChatHistoryTabFriends._StartChecked = false;
		}
		if (null != mBtnChatHistoryTabClans)
		{
			mBtnChatHistoryTabClans._StartChecked = false;
		}
		if (null != mBtnChatHistoryTabSystem)
		{
			mBtnChatHistoryTabSystem._StartChecked = false;
		}
		if (null != mBtnChatEmoticons)
		{
			mBtnChatEmoticons._StartChecked = false;
		}
		mEditChat = (KAEditBox)FindItem("TxtEditChat");
		mEditChat.pInput.onReturnKey = UIInput.OnReturnKey.Submit;
		if (UtPlatform.IsMobile() || (UtPlatform.IsWSA() && !UtUtilities.IsKeyboardAttached()))
		{
			mEditChat.pInput.defaultText = _MobileChatBoxDefaultText.GetLocalizedString();
			mEditChat.pInput.UpdateLabel();
		}
		mChatEnterBtn = FindItem("ChatEnterBtn");
		if (_ChatBackground != null)
		{
			mChatBackgroundSprite = _ChatBackground.GetComponentInChildren<UISprite>();
			SetChatBackgroundColorDark(isDark: false);
		}
		mPanel = _ChatMenu.GetComponent<UIPanel>();
		mClipRangeRef = mPanel.baseClipRegion;
		if (_ScrollBar == null)
		{
			_ScrollBar = UnityEngine.Object.FindObjectOfType<KAScrollBar>();
		}
		if (ChatData.pCurrentChatOption != 0 && _RoomChatOptionSceneName.Equals(RsResourceManager.pCurrentLevel))
		{
			ChatData.pCurrentChatOption = ChatOptions.CHAT_ROOM;
		}
		ActivateTab();
		UpdateChatInputVisibility();
		base.Start();
	}

	private void UpdateMessageLog()
	{
		if (!(_UiMessageLog != null))
		{
			return;
		}
		List<string> list = new List<string>();
		for (int i = 0; i < mDisplayHistory.Count; i++)
		{
			string text = ShowMessage(mDisplayHistory[i], updateChatMenu: false);
			if (!string.IsNullOrEmpty(text))
			{
				list.Add(text);
			}
		}
		_UiMessageLog.ShowMessages(list);
	}

	private void SetChatBackgroundColorDark(bool isDark)
	{
		UISprite[] componentsInChildren = _ChatBackground.GetComponentsInChildren<UISprite>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Color color = componentsInChildren[i].color;
			if (isDark)
			{
				color.a = 1f;
			}
			else
			{
				color.a = 0.5f;
			}
			componentsInChildren[i].color = color;
		}
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
		if (mInstance != this)
		{
			mInstance = this;
		}
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	public static void SetListener(GameObject inObj)
	{
		_Listener = inObj;
	}

	private void OnSceneLoaded(Scene newScene, LoadSceneMode loadSceneMode)
	{
		if (mInstance._ClearChatOnSceneLoad)
		{
			mDisplayHistory.Clear();
			mChanged = false;
		}
	}

	private string GetAction(object messageObject)
	{
		if (mUiToolbar == null || mUiToolbar._MessageActions == null || mUiToolbar._MessageActions.Count == 0)
		{
			return "";
		}
		if (messageObject.GetType() == typeof(MessageInfo))
		{
			MessageInfo messageInfo = (MessageInfo)messageObject;
			foreach (MessageAction messageAction in mUiToolbar._MessageActions)
			{
				if (!messageAction._IsChallenge && messageInfo.MessageTypeID.HasValue && messageAction._MessageTypeID == messageInfo.MessageTypeID.Value)
				{
					if (string.IsNullOrEmpty(messageAction._MessageSubType))
					{
						return messageAction._Action.GetLocalizedString();
					}
					if (new TaggedMessageHelper(messageInfo).SubType.Equals(messageAction._MessageSubType))
					{
						return messageAction._Action.GetLocalizedString();
					}
				}
			}
		}
		else if (messageObject.GetType() == typeof(ChallengeInfo))
		{
			foreach (MessageAction messageAction2 in mUiToolbar._MessageActions)
			{
				if (messageAction2._IsChallenge)
				{
					return messageAction2._Action.GetLocalizedString();
				}
			}
		}
		return "";
	}

	private void SetChatOption(ChatOptions option)
	{
		if (ChatData.pCurrentChatOption != option)
		{
			ChatData.pCurrentChatOption = option;
			mChanged = true;
			UpdateChatInputVisibility();
		}
		else
		{
			_ChatMenu.pDragPanel.ResetPosition();
		}
	}

	public void LoadEmoticons()
	{
		mUiToolbar._UiAvatarCSM._UiActions.ShowEmoticons();
	}

	private void ActivateTab()
	{
		if (_IsVisible)
		{
			if (ChatData.pCurrentChatOption == ChatOptions.CHAT_FRIENDS && null != mBtnChatHistoryTabFriends)
			{
				mBtnChatHistoryTabFriends.OnClick();
			}
			else if (ChatData.pCurrentChatOption == ChatOptions.CHAT_CLANS && null != mBtnChatHistoryTabClans)
			{
				mBtnChatHistoryTabClans.OnClick();
			}
			else if (ChatData.pCurrentChatOption == ChatOptions.CHAT_SYSTEM && null != mBtnChatHistoryTabSystem)
			{
				mBtnChatHistoryTabSystem.OnClick();
			}
			else if (mBtnChatHistoryTabRoom != null)
			{
				ChatData.pCurrentChatOption = ChatOptions.CHAT_ROOM;
				mBtnChatHistoryTabRoom.OnClick();
			}
			else if (ChatData.pCurrentChatOption == ChatOptions.CHAT_EMOTICON && null != mBtnChatEmoticons)
			{
				ChatData.pCurrentChatOption = ChatOptions.CHAT_EMOTICON;
				mBtnChatEmoticons.OnClick();
			}
		}
	}

	private void UpdateChatInputVisibility()
	{
		mChatEnterBtn.SetVisibility(ChatData.pCurrentChatOption != ChatOptions.CHAT_SYSTEM);
		mEditChat.SetVisibility(ChatData.pCurrentChatOption != ChatOptions.CHAT_SYSTEM);
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (UtPlatform.IsMobile())
		{
			if (!inVisible)
			{
				AvAvatar.pState = AvAvatarState.IDLE;
				if (mIsJoystickVisibilityChanged)
				{
					KAInput.ShowJoystick(UiJoystick.pInstance.pPos, inShow: true);
					mIsJoystickVisibilityChanged = false;
				}
			}
		}
		else if (inVisible && mChanged && mEditChat != null && mEditChat.GetVisibility())
		{
			mEditChat.SetFocus(focus: true);
			OnSelect(mEditChat, inSelected: true);
		}
		if (_ChatBackground != null && _ChatBackground.transform.parent != null)
		{
			_ChatBackground.transform.parent.gameObject.SetActive(inVisible);
		}
		if (inVisible)
		{
			if (mFlashButtonDuration > 0f)
			{
				FlashChatButton(flash: false);
			}
			ActivateTab();
			return;
		}
		FlashChatTabs(flash: false);
		if (mBtnChatHistoryTabRoom != null)
		{
			mBtnChatHistoryTabRoom.SetChecked(isChecked: false);
		}
		if (mBtnChatHistoryTabFriends != null)
		{
			mBtnChatHistoryTabFriends.SetChecked(isChecked: false);
		}
		if (mBtnChatHistoryTabClans != null)
		{
			mBtnChatHistoryTabClans.SetChecked(isChecked: false);
		}
		if (mBtnChatHistoryTabSystem != null)
		{
			mBtnChatHistoryTabSystem.SetChecked(isChecked: false);
		}
		if (mBtnChatEmoticons != null)
		{
			mBtnChatEmoticons.SetChecked(isChecked: false);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (InteractiveTutManager._CurrentActiveTutorialObject != null)
		{
			if (GetVisibility())
			{
				SetVisibility(inVisible: false);
			}
			return;
		}
		if (GetVisibility() != _IsVisible)
		{
			SetVisibility(_IsVisible);
		}
		if (_IsVisible)
		{
			if (mPendingChatTabFlash)
			{
				mPendingChatTabFlash = false;
				FlashChatTabs(flash: true);
			}
			if (mFlashTabCount > 0f && _ChatTabFlashInfo != null && _ChatTabFlashInfo.Count > 0)
			{
				foreach (ChatTabFlashInfo item in _ChatTabFlashInfo)
				{
					if (!(item._ChatTabWidget != null) || !(item._CurrentFlashTime > 0f))
					{
						continue;
					}
					item._CurrentFlashTime -= Time.deltaTime;
					if (item._CurrentFlashTime <= 0f)
					{
						item._ChatTabWidget.PlayAnim("Normal");
						item._CurrentFlashTime = 0f;
						KAToggleButton kAToggleButton = (KAToggleButton)item._ChatTabWidget;
						if (kAToggleButton != null && kAToggleButton.IsChecked())
						{
							kAToggleButton.SetChecked(isChecked: true);
						}
						mFlashTabCount -= 1f;
					}
				}
			}
			if (UtPlatform.IsMobile())
			{
				if (AvAvatar.pState != AvAvatarState.PAUSED)
				{
					AvAvatar.pState = AvAvatarState.PAUSED;
				}
				if (UiJoystick.pInstance != null && UiJoystick.pInstance.GetVisibility())
				{
					KAInput.ShowJoystick(UiJoystick.pInstance.pPos, inShow: false);
					mIsJoystickVisibilityChanged = true;
				}
			}
		}
		else
		{
			if (mPendingChatButtonFlash)
			{
				mPendingChatButtonFlash = false;
				FlashChatButton(flash: true);
			}
			if (mFlashButtonDuration > 0f)
			{
				mFlashButtonDuration -= Time.deltaTime;
				if (mFlashButtonDuration <= 0f)
				{
					FlashChatButton(flash: false);
				}
			}
		}
		if (base.pUIManager == null)
		{
			return;
		}
		if (mEditChat != null && mChatEnterBtn != null && UICamera.hoveredObject != mChatEnterBtn.gameObject)
		{
			mChatEnterBtn.SetDisabled(mEditChat.GetText().Trim().Length == 0 || mEditChat.GetText() == mEditChat._DefaultText.GetLocalizedString());
		}
		DoConsoleWindow();
		LimitChatWindow();
		if (!(mUiGenericDB == null) || !(KAUI._GlobalExclusiveUI == null) || !KAInput.GetKeyDown("StartChat") || !FUEManager.IsInputEnabled("StartChat"))
		{
			return;
		}
		if (!_IsVisible)
		{
			_IsVisible = true;
			ResetChatText();
			mChanged = true;
			if (UtPlatform.IsWSA())
			{
				mEditChat.pInput.defaultText = mEditChat._DefaultText.GetLocalizedString();
			}
		}
		else if (mEditChat.GetVisibility() && !mEditChat.HasFocus())
		{
			mEditChat.SetFocus(focus: true);
			OnSelect(mEditChat, inSelected: true);
		}
	}

	private void FlashChatButton(bool flash)
	{
		if (flash)
		{
			if (_IsVisible)
			{
				return;
			}
			if (AvAvatar.pToolbar != null && !AvAvatar.pToolbar.activeInHierarchy)
			{
				mPendingChatButtonFlash = true;
			}
			else
			{
				if (!(mFlashButtonDuration <= 0f))
				{
					return;
				}
				foreach (KAWidget item in _WidgetsToFlash)
				{
					if (item != null)
					{
						item.PlayAnim("Flash");
					}
				}
				mFlashButtonDuration = _FlashDuration;
			}
			return;
		}
		mFlashButtonDuration = 0f;
		foreach (KAWidget item2 in _WidgetsToFlash)
		{
			if (item2 != null)
			{
				item2.PlayAnim("Normal");
			}
		}
	}

	public void FlashChatTab(ChatOptions chatOption)
	{
		if (_ChatTabFlashInfo == null || _ChatTabFlashInfo.Count == 0)
		{
			return;
		}
		ChatTabFlashInfo chatTabFlashInfo = _ChatTabFlashInfo.Find((ChatTabFlashInfo x) => x._ChatOption == chatOption);
		if (chatTabFlashInfo != null && !(chatTabFlashInfo._ChatTabWidget == null) && _IsVisible)
		{
			if (AvAvatar.pToolbar != null && !AvAvatar.pToolbar.activeInHierarchy)
			{
				mPendingChatTabFlash = true;
				chatTabFlashInfo._FlashPending = true;
			}
			else if (chatTabFlashInfo._CurrentFlashTime <= 0f && chatTabFlashInfo._ChatTabWidget != null)
			{
				chatTabFlashInfo._ChatTabWidget.PlayAnim("Flash");
				chatTabFlashInfo._CurrentFlashTime = chatTabFlashInfo._FlashDuration;
				mFlashTabCount += 1f;
			}
		}
	}

	private void FlashChatTabs(bool flash)
	{
		if (_ChatTabFlashInfo == null || _ChatTabFlashInfo.Count == 0)
		{
			return;
		}
		if (flash)
		{
			foreach (ChatTabFlashInfo item in _ChatTabFlashInfo)
			{
				if (item._FlashPending && item._ChatTabWidget != null)
				{
					item._FlashPending = false;
					item._CurrentFlashTime = item._FlashDuration;
					mFlashTabCount += 1f;
					item._ChatTabWidget.PlayAnim("Flash");
				}
			}
			return;
		}
		foreach (ChatTabFlashInfo item2 in _ChatTabFlashInfo)
		{
			if (item2._ChatTabWidget != null)
			{
				item2._CurrentFlashTime = 0f;
				item2._FlashPending = false;
				item2._ChatTabWidget.PlayAnim("Normal");
			}
		}
		mFlashTabCount = 0f;
	}

	private void LateUpdate()
	{
		if (!mSetPreviousPosition && _DragBtn != null && _ChatBackground != null)
		{
			mOriginalDistance = _DragBtn.transform.localPosition - _ChatBackground.transform.localPosition;
			mOriginalOffset = new Vector2(_ChatBackground.transform.localPosition.x + _ChatBackground.pBackground.localSize.x - _DragBtn.transform.localPosition.x, _ChatBackground.pBackground.localSize.y - (_ChatBackground.transform.localPosition.y - _DragBtn.transform.localPosition.y));
			mOriginalBarHeight = _ScrollBar.backgroundWidget.height;
			mOriginalChatTextBkgWidth = mChatTextBkg.pBackground.width;
			if (mChatWindowPosition != Vector3.zero)
			{
				base.transform.position = mChatWindowPosition;
			}
			if (mChatExpandBtnPosition != Vector3.zero)
			{
				ResizeChatWindow(forceUpdate: true);
			}
			mSetPreviousPosition = true;
		}
	}

	private void LimitChatWindow()
	{
		if (!(_ChatBackground != null))
		{
			return;
		}
		Vector3 position = KAUIManager.pInstance.camera.WorldToScreenPoint(base.transform.position);
		Vector3 vector = KAUIManager.pInstance.camera.WorldToScreenPoint(_ChatBackground.transform.position);
		Vector3 vector2 = KAUIManager.pInstance.camera.WorldToScreenPoint(mCloseButton.transform.position + new Vector3(mCloseButton.pBackground.width, mCloseButton.pBackground.height / 2, 0f));
		if (_DragBtn != null)
		{
			Vector3 position2 = _ChatBackground.transform.position + new Vector3(_ChatBackground.pBackground.width, -_ChatBackground.pBackground.height, 0f);
			Vector2 vector3 = new Vector2(vector.x, vector2.y);
			Vector2 vector4 = KAUIManager.pInstance.camera.WorldToScreenPoint(position2);
			if (vector3.x < 0f)
			{
				position.x -= vector3.x;
			}
			if (vector4.x > (float)Screen.width)
			{
				position.x -= vector4.x - (float)Screen.width;
			}
			if (vector4.y < 0f)
			{
				position.y -= vector4.y;
			}
			if (vector3.y > (float)Screen.height)
			{
				position.y -= vector3.y - (float)Screen.height;
			}
		}
		position = KAUIManager.pInstance.camera.ScreenToWorldPoint(position);
		base.transform.position = position;
		if (mSetPreviousPosition)
		{
			mChatWindowPosition = base.transform.position;
		}
	}

	private void ResetChatText()
	{
		if (mEditChat != null && mEditChat.GetVisibility())
		{
			mEditChat.SetText("");
			mEditChat.SetFocus(focus: true);
			mEditChat.OnSelect(inSelected: true);
			AvAvatar.pInputEnabled = false;
		}
	}

	private void UpdateMenu()
	{
		_ChatMenu.pMenuGrid.Reposition();
		_ChatMenu.pDragPanel.UpdateScrollbars(recalculateBounds: true);
		if (!_ChatMenu.pDragPanel.shouldMoveVertically)
		{
			_ChatMenu.pDragPanel.ResetPosition();
		}
	}

	public void DoConsoleWindow()
	{
		if (mChanged)
		{
			_ChatMenu.ClearItems();
			for (int i = 0; i < mDisplayHistory.Count; i++)
			{
				ShowMessage(mDisplayHistory[i]);
			}
			UpdateMessageLog();
			mChanged = false;
		}
	}

	public string ShowMessage(ChatMessage chatMessageData, bool updateChatMenu = true)
	{
		if (ChatData.pCurrentChatOption == chatMessageData._Channel || ChatData.pCurrentChatOption == ChatOptions.CHAT_ROOM)
		{
			string text = chatMessageData._Message;
			if (chatMessageData._Channel == ChatOptions.CHAT_SYSTEM)
			{
				string newValue = "[" + HexUtil.ColorToHexNoAlpha(_SystemMessageTextColor) + "]";
				text = chatMessageData._Message.Replace("[c]", newValue);
				newValue = "[" + HexUtil.ColorToHexNoAlpha(_ActiveLinkTextColor) + "]";
				text = text.Replace("[-]", newValue);
			}
			if (_IsVisible && updateChatMenu)
			{
				KAWidget kAWidget = null;
				kAWidget = _ChatMenu.AddWidget("ChatEntry");
				kAWidget.name = "ChatEntry";
				kAWidget.GetLabel().width = Mathf.FloorToInt(mPanel.baseClipRegion.z - (float)_ChatEntryEndPadding);
				kAWidget.SetText(text);
				kAWidget.SetInteractive(chatMessageData._Channel == ChatOptions.CHAT_SYSTEM);
				kAWidget.SetVisibility(inVisible: true);
				if (chatMessageData._Channel == ChatOptions.CHAT_SYSTEM)
				{
					chatMessageData._Widget = kAWidget;
				}
				BoxCollider component = kAWidget.GetComponent<BoxCollider>();
				if (component != null)
				{
					Vector3 size = component.size;
					size.x = kAWidget.GetLabel().width;
					size.y = kAWidget.GetLabel().height;
					component.center = new Vector3(size.x * 0.5f, size.y * -0.5f, component.center.z);
					component.size = size;
				}
			}
			return text;
		}
		return string.Empty;
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mBtnChatHistoryTabRoom)
		{
			SetChatOption(ChatOptions.CHAT_ROOM);
		}
		else if (item == mBtnChatHistoryTabSystem)
		{
			SetChatOption(ChatOptions.CHAT_SYSTEM);
		}
		else if (item == mBtnChatHistoryTabFriends)
		{
			SetChatOption(ChatOptions.CHAT_FRIENDS);
		}
		else if (item == mBtnChatHistoryTabClans)
		{
			SetChatOption(ChatOptions.CHAT_CLANS);
		}
		else if (item == mBtnChatEmoticons)
		{
			SetChatOption(ChatOptions.CHAT_EMOTICON);
			CloseChatHistory();
			LoadEmoticons();
		}
		else if (item == mChatEnterBtn)
		{
			ProcessFocusLost(mEditChat, enterHit: true);
		}
		else if (item == mCloseButton)
		{
			CloseChatHistory();
		}
	}

	public void CloseChatHistory()
	{
		HideChatHistory();
		mEditChat.pInput.isSelected = false;
	}

	public void Click(KAWidget inWidget)
	{
		if (inWidget != null && mDisplayHistory.Count > 0)
		{
			if (UtPlatform.IsMobile())
			{
				HideChatHistory();
				mEditChat.pInput.isSelected = false;
				SetVisibility(inVisible: false);
			}
			ChatMessage chatMessage = mDisplayHistory.Find((ChatMessage message) => message._Widget == inWidget);
			if (chatMessage != null && chatMessage._Callback != null)
			{
				chatMessage._Callback(chatMessage._MessageInfoObject);
			}
		}
	}

	public override void OnSelect(KAWidget inWidget, bool inSelected)
	{
		base.OnSelect(inWidget, inSelected);
		if (inWidget == mEditChat)
		{
			AvAvatar.pInputEnabled = !inSelected;
			if (_ChatBackground != null)
			{
				SetChatBackgroundColorDark(inSelected);
			}
		}
	}

	public static string GetChatDataString(ChatOptions filterInclude)
	{
		string text = "";
		int num = 0;
		for (int i = 0; i < mDisplayHistory.Count; i++)
		{
			text = text + ((num == 0) ? "" : "\n") + mDisplayHistory[i]._Message;
			num++;
		}
		return text;
	}

	public void ProcessFocusLost(KAWidget inWidget, bool enterHit)
	{
		if (!enterHit || inWidget.GetText().Trim().Length <= 0 || !(mEditChat.GetText() != mEditChat._DefaultText.GetLocalizedString()))
		{
			return;
		}
		if (UiLogin.pIsGuestUser)
		{
			AvAvatar.SetUIActive(inActive: false);
			AvAvatar.pState = AvAvatarState.PAUSED;
			KAUICursorManager.SetDefaultCursor("Loading");
			if ((bool)UiRacingMultiplayer.pInstance)
			{
				UiRacingMultiplayer.pInstance.SetInteractive(interactive: false);
			}
			RsResourceManager.LoadAssetFromBundle(_UiChatRegisterNowPath, OnBundleLoaded, typeof(GameObject), inDontDestroy: false, typeof(GameObject));
		}
		else
		{
			SendChat(inWidget.GetText());
		}
	}

	private void OnBundleLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.name = ((GameObject)inObject).name;
			UiChatRegisterNow component = obj.GetComponent<UiChatRegisterNow>();
			if ((bool)component)
			{
				component.Init(OnUIChatRegisterClosed);
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("Error loading Event Intro Prefab!" + inURL);
			KAUICursorManager.SetDefaultCursor("Arrow");
			OnUIChatRegisterClosed();
			break;
		}
	}

	private void OnUIChatRegisterClosed()
	{
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
	}

	public override void OnSubmit(KAWidget inWidget)
	{
		base.OnSubmit(inWidget);
		OnSelect(mEditChat, inSelected: false);
		ProcessFocusLost(inWidget, enterHit: true);
		KAInput.ResetInputAxes();
		UICamera.selectedObject = null;
	}

	public void OnChatParentRequestLoadEvent(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			KAUICursorManager.SetDefaultCursor("Arrow");
			UnityEngine.Object.Instantiate((GameObject)inObject).SendMessage("SetURL", inURL);
			SetVisibility(_IsVisible);
			break;
		case RsResourceLoadEvent.ERROR:
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	public void SendChat(string text)
	{
		mLastUsedChatOption = ChatData.pCurrentChatOption;
		if (!mUsedChat)
		{
			mUsedChat = true;
		}
		text = ParseChatCommands(text);
		if (!UserInfo.pInstance.MultiplayerEnabled)
		{
			ShowDialog(_MultiplayerDisabledOnServerText, _ServerErrorTitleText);
		}
		else if (!MainStreetMMOClient.pIsMMOEnabled)
		{
			ShowDialog(_MultiplayerDisabledOnDeviceText, _ServerErrorTitleText);
		}
		else if (!UserInfo.pInstance.OpenChatEnabled)
		{
			ShowDialog(_SafeChatDisabledOnServerText, _ServerErrorTitleText);
		}
		else if (MainStreetMMOClient.pInstance != null)
		{
			int chatType = ((ChatData.pCurrentChatOption != ChatOptions.CHAT_FRIENDS) ? 1 : 2);
			string groupID = "";
			if (ChatData.pCurrentChatOption == ChatOptions.CHAT_CLANS)
			{
				groupID = UserProfile.pProfileData.GetGroupID();
			}
			if (!MainStreetMMOClient.pInstance.SendChat(text, groupID, chatType))
			{
				ShowDialog(_ServerErrorText, _ServerErrorTitleText);
			}
		}
		if (UtPlatform.IsMobile() && mEditChat != null)
		{
			mEditChat.SetText("");
		}
		else if (mUiGenericDB == null)
		{
			ResetChatText();
		}
		else if (mEditChat != null)
		{
			mEditChat.SetText(mEditChat._DefaultText.GetLocalizedString());
		}
	}

	public static string ParseChatCommands(string chatString)
	{
		if (chatString.StartsWith("/"))
		{
			int num = chatString.IndexOf(" ") - 1;
			string text = "";
			if (num <= 0 || num > chatString.Trim().Length - 1)
			{
				text = chatString.Substring(1);
				chatString = "";
			}
			else
			{
				text = chatString.Substring(1, chatString.IndexOf(" ") - 1);
				chatString = chatString.Substring(chatString.IndexOf(" ") + 1);
			}
			text = text.ToLower();
		}
		return chatString;
	}

	private void ShowDialog(LocaleString text, LocaleString inTitle)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		SetInteractive(interactive: true);
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDB"));
		mUiGenericDB = gameObject.GetComponent<KAUIGenericDB>();
		mUiGenericDB._MessageObject = base.gameObject;
		mUiGenericDB._OKMessage = "OnClose";
		mUiGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mUiGenericDB.SetTextByID(text._ID, text._Text, interactive: false);
		mUiGenericDB.SetTitle(inTitle.GetLocalizedString());
		KAUI.SetExclusive(mUiGenericDB);
		AvAvatar.pInputEnabled = false;
		_IsVisible = false;
	}

	public void OnClose()
	{
		if (mUiGenericDB != null)
		{
			KAUI.RemoveExclusive(mUiGenericDB);
			UnityEngine.Object.Destroy(mUiGenericDB.gameObject);
			mUiGenericDB = null;
		}
		AvAvatar.pInputEnabled = true;
		_IsVisible = true;
	}

	public override void OnDrag(KAWidget inWidget, Vector2 inDelta)
	{
		base.OnDrag(inWidget, inDelta);
		if (inWidget == _DragBtn)
		{
			ResizeChatWindow();
		}
	}

	public override void OnPress(KAWidget inWidget, bool inPressed)
	{
		base.OnPress(inWidget, inPressed);
		if (inWidget == _DragBtn)
		{
			_DragBtn.collider.enabled = !inPressed;
			mCloseButton.collider.enabled = !inPressed;
			_ScrollBar.SetInteractive(!inPressed);
		}
	}

	public void ResizeChatWindow(bool forceUpdate = false)
	{
		Vector3 vector = mChatEnterBtn.transform.localPosition - _DragBtn.transform.localPosition;
		Vector2 vector2 = mChatExpandBtnPosition;
		if (!forceUpdate)
		{
			Vector2 pos = KAUIManager.pInstance.camera.ScreenToWorldPoint(Input.mousePosition);
			vector2 = _DragBtn.GetLocalPosition(pos);
		}
		Vector3 vector3 = new Vector3(vector2.x, vector2.y, _DragBtn.transform.localScale.z);
		Vector3 vector4 = vector3 - _ChatBackground.transform.localPosition;
		if (vector4.x < mOriginalDistance.x)
		{
			vector4.x = mOriginalDistance.x;
		}
		if (vector4.y > mOriginalDistance.y)
		{
			vector4.y = mOriginalDistance.y;
		}
		if (vector4.x > _ChatWindowMaxSize.x)
		{
			vector4.x = _ChatWindowMaxSize.x;
		}
		if (vector4.y < 0f - _ChatWindowMaxSize.y)
		{
			vector4.y = 0f - _ChatWindowMaxSize.y;
		}
		Vector3 vector5 = (mChatExpandBtnPosition = vector4 + _ChatBackground.transform.localPosition);
		_DragBtn.SetPosition(vector5.x, vector5.y);
		mChatBackgroundSprite.width = (int)(vector4.x + mOriginalOffset.x);
		mChatBackgroundSprite.height = -(int)(vector4.y - mOriginalOffset.y);
		vector3 = vector5 + vector;
		mChatEnterBtn.SetPosition(vector3.x, vector3.y);
		if (mChatEnterBtn.GetState() == KAUIState.INTERACTIVE)
		{
			mChatEnterBtn.SetDisabled(isDisabled: true);
			mChatEnterBtn.SetDisabled(isDisabled: false);
		}
		mCloseButton.SetPosition(vector5.x + (mCloseButton.pOrgPosition.x - _DragBtn.pOrgPosition.x), mCloseButton.GetPosition().y);
		_ChatBox.SetPosition(_ChatBox.transform.localPosition.x, _DragBtn.transform.localPosition.y + (_ChatBox.pOrgPosition.y - _DragBtn.pOrgPosition.y));
		float num = Mathf.Abs(vector5.x - _DragBtn.pOrgPosition.x);
		float num2 = Mathf.Abs(vector5.y - _DragBtn.pOrgPosition.y);
		mChatTextBkg.pBackground.width = mOriginalChatTextBkgWidth + (int)num;
		_ChatBox.GetLabel().width = (int)Mathf.Abs(_ChatBox.transform.localPosition.x - (mChatEnterBtn.transform.localPosition.x - (float)mChatEnterBtn.pBackground.width * 0.5f));
		BoxCollider component = _ChatBox.GetComponent<BoxCollider>();
		Vector3 size = component.size;
		size.x = _ChatBox.GetLabel().width;
		component.center = new Vector3(size.x * 0.5f, component.center.y, component.center.z);
		component.size = size;
		Vector4 baseClipRegion = mPanel.baseClipRegion;
		baseClipRegion.x = mClipRangeRef.x + num * 0.5f;
		baseClipRegion.y = mClipRangeRef.y - num2 * 0.5f;
		baseClipRegion.z = mClipRangeRef.z + num;
		baseClipRegion.w = mClipRangeRef.w + num2;
		mPanel.baseClipRegion = baseClipRegion;
		foreach (KAWidget item in _ChatMenu.GetItems())
		{
			item.GetLabel().width = Mathf.FloorToInt(baseClipRegion.z - (float)_ChatEntryEndPadding);
			component = item.GetComponent<BoxCollider>();
			size = component.size;
			size.x = item.GetLabel().width;
			size.y = item.GetLabel().height;
			component.center = new Vector3(size.x * 0.5f, size.y * -0.5f, component.center.z);
			component.size = size;
		}
		Vector3 localPosition = _ScrollBar.transform.localPosition;
		localPosition.x = baseClipRegion.z - (mClipRangeRef.z - _ScrollBar.pOrgPosition.x);
		_ScrollBar.transform.localPosition = localPosition;
		int height = mOriginalBarHeight + (int)num2;
		_ScrollBar.backgroundWidget.height = height;
		_ScrollBar.foregroundWidget.height = height;
		Vector3 localPosition2 = _ScrollBar._DownArrow.transform.localPosition;
		localPosition2.y = _ScrollBar._DownArrow.pOrgPosition.y - num2;
		_ScrollBar._DownArrow.transform.localPosition = localPosition2;
		component = _ChatBackground.transform.parent.GetComponent<BoxCollider>();
		component.center = new Vector3(_ChatBackground.GetPosition().x + (float)(mChatBackgroundSprite.width / 2), _ChatBackground.GetPosition().y - (float)(mChatBackgroundSprite.height / 2), component.center.z);
		component.size = new Vector3(mChatBackgroundSprite.width, mChatBackgroundSprite.height, component.size.z);
		UpdateMenu();
	}
}
