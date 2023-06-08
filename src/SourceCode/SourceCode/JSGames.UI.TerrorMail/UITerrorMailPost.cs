using JSGames.UI.Util;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JSGames.UI.TerrorMail;

public class UITerrorMailPost : UI
{
	public UITerrorMail _Board;

	public UIDropDown _DropDown;

	public int _MaxCharacters = 160;

	public LocaleString _MessageTooLongText = new LocaleString("That phrase doesn't fit.");

	public LocaleString _NoMatchesText = new LocaleString("(No matches found.)");

	public LocaleString _PostSucceededText = new LocaleString("Message posted.");

	public LocaleString _PostFailedText = new LocaleString("Unable to post your message. Please try again later.");

	public LocaleString _PostBlockedText = new LocaleString("This message has been blocked.");

	public LocaleString _PostAuthorizationFailedText = new LocaleString("You must authorize your account before you can post messages.");

	public float _TimeoutDuration = 15f;

	public LocaleString _TimeoutText = new LocaleString("Message cannot be posted at this time.");

	public int _SocialBuddyMessageAchievementID = 106;

	public UIAnim2D _ColorBkg;

	public UIWidget _CloseBtn;

	public UIWidget _PostBtn;

	public UIWidget _ClearBtn;

	public UIWidget _TxtCount;

	public UIEditBox _EditOpen;

	public UIToggleButton _PrivateToggle;

	public UIToggleButton _PublicToggle;

	private bool mDropdownInitialized;

	private bool mHasBlockedWord;

	private string mOriginalText = "";

	private string mUserID;

	private int mReplyID;

	private bool mBlockDropdown;

	private bool mBlockToggles;

	private UIEditBox mEditBox;

	private string mPostMessage;

	private UIToggleButton mColorButton;

	private string mColor;

	private float mTimeoutTimer;

	private bool mIsPrivateMessage
	{
		get
		{
			if (_PrivateToggle != null && _PublicToggle != null)
			{
				return _PrivateToggle.pChecked;
			}
			return false;
		}
	}

	protected override void Start()
	{
		base.Start();
		_EditOpen.pInputField.characterLimit = _MaxCharacters;
	}

	public void UpdateToggleState(bool isPrivate)
	{
		if (!(_PrivateToggle == null) && !(_PublicToggle == null))
		{
			if (isPrivate)
			{
				_PrivateToggle.pChecked = true;
			}
			else
			{
				_PublicToggle.pChecked = true;
			}
		}
	}

	public void UpdateToggleInteractivity(bool isPrivate)
	{
		if (!(_PrivateToggle == null) && !(_PublicToggle == null))
		{
			if (!isPrivate)
			{
				_PrivateToggle.pState = WidgetState.DISABLED;
			}
			else
			{
				_PublicToggle.pState = WidgetState.DISABLED;
			}
		}
	}

	public void SetVisibility(string userID, string name, int messageID, bool inBlockDropdown = false, bool inBlockToggles = false)
	{
		SetExclusive();
		if (_PrivateToggle != null && _PublicToggle != null)
		{
			_PrivateToggle.pState = WidgetState.INTERACTIVE;
			_PublicToggle.pState = WidgetState.INTERACTIVE;
			UpdateToggleState(isPrivate: false);
		}
		pVisible = true;
		mHasBlockedWord = false;
		mUserID = userID;
		mReplyID = messageID;
		mBlockDropdown = inBlockDropdown;
		mBlockToggles = inBlockToggles;
		_TxtCount.pText = "0/" + _MaxCharacters;
		_EditOpen.pVisible = true;
		_EditOpen.pInputField.characterLimit = _MaxCharacters;
		_PostBtn.pState = WidgetState.DISABLED;
		mEditBox = _EditOpen;
		mEditBox.pText = "";
		KAInput.ResetInputAxes();
		if (mDropdownInitialized)
		{
			SetDropdown((UserProfile.pProfileData.ID == name) ? userID : name);
			return;
		}
		if (BuddyList.pIsReady && BuddyList.pList != null && (bool)_DropDown)
		{
			AddFriendToDropDown(UserInfo.pInstance.UserID, GetPlayerNameByUserID(UserInfo.pInstance.UserID));
			Buddy[] pList = BuddyList.pList;
			foreach (Buddy buddy in pList)
			{
				if (buddy.Status == BuddyStatus.Approved)
				{
					AddFriendToDropDown(buddy.UserID, buddy.DisplayName);
				}
			}
		}
		SetDropdown((UserProfile.pProfileData.ID == name) ? userID : name);
		_DropDown._Menu._Template.gameObject.SetActive(value: false);
		mDropdownInitialized = true;
	}

	public string GetPlayerNameByUserID(string userID)
	{
		if (UserProfile.pProfileData != null && userID == UserProfile.pProfileData.ID && UserProfile.pProfileData.AvatarInfo != null && UserProfile.pProfileData.AvatarInfo.AvatarData != null)
		{
			return UserProfile.pProfileData.AvatarInfo.AvatarData.DisplayName;
		}
		if (UiProfile.pUserProfile != null && UiProfile.pUserProfile.UserID == userID)
		{
			return UiProfile.pUserProfile.GetDisplayName();
		}
		if (BuddyList.pIsReady)
		{
			Buddy buddy = BuddyList.pInstance.GetBuddy(userID);
			if (buddy != null && !string.IsNullOrEmpty(buddy.DisplayName))
			{
				return buddy.DisplayName;
			}
		}
		return null;
	}

	private void SetDropdown(string name)
	{
		if (mBlockDropdown)
		{
			_DropDown._Menu.pSelectedWidget = _DropDown._Menu.FindWidget(name);
		}
		else
		{
			_DropDown._Menu.pSelectedWidget = _DropDown._Menu.pChildWidgets[1];
		}
		_DropDown.pState = ((!mBlockDropdown) ? WidgetState.INTERACTIVE : WidgetState.DISABLED);
		_PrivateToggle.pState = ((!mBlockToggles) ? WidgetState.INTERACTIVE : WidgetState.DISABLED);
		_PublicToggle.pState = ((!mBlockToggles) ? WidgetState.INTERACTIVE : WidgetState.DISABLED);
	}

	private void AddFriendToDropDown(string userID, string displayName)
	{
		UIWidget uIWidget = _DropDown._Menu._Template.Duplicate(autoAddToSameParent: true);
		uIWidget.gameObject.SetActive(value: true);
		uIWidget.transform.SetParent(_DropDown._Menu._Template.transform.parent);
		uIWidget.transform.localScale = Vector3.one;
		uIWidget.pText = displayName;
		uIWidget.name = userID;
		MessageBoardFriendUserData pData = new MessageBoardFriendUserData(userID, displayName);
		uIWidget.pData = pData;
	}

	public void OnFriendNameSelected(MessageBoardFriendUserData data)
	{
		mUserID = data.pUserID;
	}

	protected override void OnClick(UIWidget item, PointerEventData eventData)
	{
		base.OnClick(item, eventData);
		if (item == _CloseBtn)
		{
			KAInput.ResetInputAxes();
			OnClose();
		}
		else if (item == mEditBox)
		{
			if (mHasBlockedWord)
			{
				mHasBlockedWord = false;
				_EditOpen.pInputField.characterLimit = _MaxCharacters;
				item.pText = mOriginalText;
			}
		}
		else if (item == _PostBtn)
		{
			mTimeoutTimer = _TimeoutDuration;
			mPostMessage = "";
			mPostMessage = mEditBox.pText;
			KAUICursorManager.SetDefaultCursor("Loading");
			pState = WidgetState.NOT_INTERACTIVE;
			string attribute = UiMessageBoard.ATTRIBUTE_COLOR + "=" + mColor;
			if (mReplyID == -1)
			{
				_Board.PostMessage(mUserID, mPostMessage, MessageLevel.WhiteList, attribute, mIsPrivateMessage);
			}
			else
			{
				_Board.ReplyToMessage(mUserID, mReplyID, mPostMessage, MessageLevel.WhiteList, attribute, mIsPrivateMessage);
			}
		}
		else if (item == _ClearBtn)
		{
			mOriginalText = "";
			mEditBox.pText = "";
			_TxtCount.pText = "0/" + _MaxCharacters;
			_PostBtn.pState = WidgetState.DISABLED;
		}
		else if (item == _DropDown)
		{
			_DropDown.pVisible = !_DropDown.pVisible;
		}
		else if (item.name.StartsWith("Color"))
		{
			mColorButton.pChecked = true;
			mColorButton.pState = WidgetState.INTERACTIVE;
			mColorButton = (UIToggleButton)item;
			mColorButton.pChecked = false;
			mColorButton.pState = WidgetState.NOT_INTERACTIVE;
			mColor = item.name.Substring(5, item.name.Length - 8);
			_ColorBkg.Play(mColor, 1);
		}
		else if (item == _PrivateToggle)
		{
			UpdateToggleState(isPrivate: true);
		}
		else if (item == _PublicToggle)
		{
			UpdateToggleState(isPrivate: false);
		}
		MessageBoardFriendUserData messageBoardFriendUserData = (MessageBoardFriendUserData)item.pData;
		if (messageBoardFriendUserData != null)
		{
			OnFriendNameSelected(messageBoardFriendUserData);
		}
	}

	public void PostEventHandler(RMFMessageBoardPostResult result)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		pState = WidgetState.INTERACTIVE;
		mTimeoutTimer = 0f;
		if (result == null || result.resultStatus == MessageResultStatus.FAILED)
		{
			ShowDialog(_PostFailedText._ID, _PostFailedText._Text);
		}
		else if (result.ErrorID == 0)
		{
			Message message = new Message();
			message.Creator = UserInfo.pInstance.UserID;
			message.MessageID = result.MessageID.Value;
			message.Content = mPostMessage;
			message.MessageLevel = MessageLevel.WhiteList;
			message.CreateTime = result.CreateTime;
			message.DisplayAttribute = UiMessageBoard.ATTRIBUTE_COLOR + "=" + mColor;
			message.MessageType = MessageType.Post;
			message.isPrivate = mIsPrivateMessage;
			pState = WidgetState.INTERACTIVE;
			_Board.OnPostSuccess(message, mReplyID);
			if (mReplyID == -1)
			{
				_Board.AddConversation(message.ConversationID, message);
				_Board.AddMessageBoardItem(_Board._MessageListItemTemplate, message, setAsFirst: true);
			}
			ShowDialog(_PostSucceededText._ID, _PostSucceededText._Text);
			if (BuddyList.pInstance != null)
			{
				if (BuddyList.pInstance.GetBuddyStatus(mUserID) == BuddyStatus.Approved)
				{
					UserAchievementTask.Set(_SocialBuddyMessageAchievementID, mUserID, displayRewards: true);
				}
			}
			else
			{
				UtDebug.LogError("BuddyList.pInstance is NULL");
			}
		}
		else if (result.ErrorID == 3)
		{
			ProcessBlockedText(result.originalMessage, result.filteredMessage);
			ShowDialog(_PostBlockedText._ID, _PostBlockedText._Text);
		}
		else
		{
			ShowDialog(_PostFailedText._ID, _PostFailedText._Text);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!pVisible)
		{
			return;
		}
		if (_TxtCount != null && mEditBox != null && !mHasBlockedWord)
		{
			_TxtCount.pText = mEditBox.pText.Length + "/" + _MaxCharacters;
			if (mEditBox.pText.Trim().Length == 0)
			{
				if (_PostBtn.pState != 0)
				{
					_PostBtn.pState = WidgetState.DISABLED;
				}
			}
			else if (_PostBtn.pState != WidgetState.INTERACTIVE)
			{
				_PostBtn.pState = WidgetState.INTERACTIVE;
			}
		}
		if (mTimeoutTimer > 0f)
		{
			mTimeoutTimer -= Time.deltaTime;
			if (mTimeoutTimer <= 0f)
			{
				pState = WidgetState.INTERACTIVE;
				ShowDialog(_TimeoutText._ID, _TimeoutText._Text);
			}
		}
	}

	private void ShowDialog(int id, string text)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		pVisible = false;
		UIUtil.DisplayGenericDB("PfUiGenericSmallDB", text, text, base.gameObject, "OnClose");
	}

	private void OnClose()
	{
		_Board.pState = WidgetState.INTERACTIVE;
		pVisible = false;
		if (mHasBlockedWord)
		{
			pVisible = true;
			mHasBlockedWord = false;
			SetExclusive();
		}
		else
		{
			RemoveExclusive();
		}
	}

	public void ProcessBlockedText(string inOriginalText, string inFilteredText)
	{
		string[] array = inOriginalText.Split(' ');
		string[] array2 = inFilteredText.Split(' ');
		string text = "";
		mOriginalText = inOriginalText;
		for (int i = 0; i < array2.Length; i++)
		{
			if (array2[i].Contains("#"))
			{
				array2[i] = "[c][FF0000]" + array[i] + "[" + HexUtil.ColorToHexNoAlpha(_EditOpen.pInputField.selectionColor) + "]";
			}
			text = text + " " + array2[i];
		}
		_EditOpen.pInputField.characterLimit = -1;
		_TxtCount.pText = inOriginalText.Length + "/" + _MaxCharacters;
		mEditBox.pText = text;
		mHasBlockedWord = true;
	}
}
