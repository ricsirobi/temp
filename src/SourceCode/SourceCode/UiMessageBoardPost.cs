using JSGames;
using UnityEngine;

public class UiMessageBoardPost : KAUIDropDownEdit
{
	public UiMessageBoard _Board;

	public UIMessageBoardPostFriendListMenu _DropDownListMenu;

	public int _MaxCharacters = 160;

	public string _Intro;

	public LocaleString _MessageTooLongText = new LocaleString("That phrase doesn't fit.");

	public LocaleString _NoMatchesText = new LocaleString("(No matches found.)");

	public LocaleString _PostSucceededText = new LocaleString("Message posted.");

	public LocaleString _PostFailedText = new LocaleString("Unable to post your message. Please try again later.");

	public LocaleString _PostBlockedText = new LocaleString("This message has been blocked.");

	public LocaleString _PostAuthorizationFailedText = new LocaleString("You must authorize your account before you can post messages.");

	public float _TimeoutDuration = 15f;

	public LocaleString _TimeoutText = new LocaleString("Message cannot be posted at this time.");

	public int _SocialBuddyMessageAchievementID = 106;

	public KAAnim2D _ColorBkg;

	public string _FBAuthDBAssetPath = "RS_DATA/PfUiFBAuthorizeDBDO.unity3d/PfUiFBAuthorizeDBDO";

	private KAWidget mCloseBtn;

	private KAWidget mPostBtn;

	private KAWidget mClearBtn;

	private KAWidget mFriendsMenuBtn;

	private KAWidget mTxtName;

	private KAWidget mTxtCount;

	private KAEditBox mEditOpen;

	private KAToggleButton mPrivateToggle;

	private KAToggleButton mPublicToggle;

	private bool mHasBlockedWord;

	private string mOriginalText = "";

	private string mUserID;

	private int mMessageID;

	private KAEditBox mEditBox;

	private KAUIGenericDB mKAUIGenericDB;

	private string mPostMessage;

	private KAToggleButton mColorButton;

	private string mColor;

	private float mTimeoutTimer;

	private bool mIsPrivateMessage
	{
		get
		{
			if (mPrivateToggle != null && mPublicToggle != null)
			{
				return mPrivateToggle.IsChecked();
			}
			return false;
		}
	}

	protected override void Start()
	{
		base.Start();
		mCloseBtn = FindItem("CloseBtn");
		mPostBtn = FindItem("PostBtn");
		mClearBtn = FindItem("ClearBtn");
		mFriendsMenuBtn = FindItem("FriendsMenuBtn");
		mTxtName = FindItem("TxtName");
		mTxtCount = FindItem("TxtCount");
		mEditOpen = (KAEditBox)FindItem("EditMessageOpen");
		mPrivateToggle = (KAToggleButton)FindItem("PrivateBtn");
		mPublicToggle = (KAToggleButton)FindItem("PublicBtn");
		if (_DropDownListMenu != null)
		{
			_DropDownListMenu.pMessageBoardPost = this;
		}
		mEditOpen.pInput.characterLimit = _MaxCharacters;
		foreach (KAWidget item in mItemInfo)
		{
			if (item.name.StartsWith("Color"))
			{
				mColorButton = (KAToggleButton)item;
				mColorButton.SetChecked(isChecked: false);
				mColorButton.SetInteractive(isInteractive: false);
				mColor = item.name.Substring(5, item.name.Length - 8);
				_ColorBkg.Play(mColor, 1);
				break;
			}
		}
	}

	public void UpdateToggleState(bool isPrivate)
	{
		if (!(mPrivateToggle == null) && !(mPublicToggle == null))
		{
			if (isPrivate)
			{
				mPrivateToggle.SetChecked(isChecked: true);
			}
			else
			{
				mPublicToggle.SetChecked(isChecked: true);
			}
		}
	}

	public void UpdateToggleInteractivity(bool isPrivate)
	{
		if (!(mPrivateToggle == null) && !(mPublicToggle == null))
		{
			mPrivateToggle.SetDisabled(!isPrivate);
			mPublicToggle.SetDisabled(isPrivate);
		}
	}

	public void SetVisibility(string userID, string name, int messageID, bool showFriendsButton = false)
	{
		if (mPrivateToggle != null && mPublicToggle != null)
		{
			mPrivateToggle.SetDisabled(isDisabled: false);
			mPublicToggle.SetDisabled(isDisabled: false);
			UpdateToggleState(_Board.pShowPrivateMessages);
		}
		SetVisibility(show: true);
		mHasBlockedWord = false;
		mUserID = userID;
		mMessageID = messageID;
		mTxtName.SetText(name);
		if (mFriendsMenuBtn != null)
		{
			mFriendsMenuBtn.SetVisibility(showFriendsButton);
		}
		mTxtCount.SetText("0/" + _MaxCharacters);
		mEditOpen.SetVisibility(inVisible: true);
		mEditOpen.pInput.characterLimit = _MaxCharacters;
		mEditOpen.pInput.label.supportEncoding = false;
		mPostBtn.SetState(KAUIState.DISABLED);
		mEditBox = mEditOpen;
		mEditBox.SetText("");
		KAInput.ResetInputAxes();
		if (!(_DropDownListMenu != null) || !BuddyList.pIsReady || BuddyList.pList == null)
		{
			return;
		}
		_DropDownListMenu.ClearItems();
		AddFriendToDropDown(UserInfo.pInstance.UserID, UserInfo.pInstance.Username);
		Buddy[] pList = BuddyList.pList;
		foreach (Buddy buddy in pList)
		{
			if (buddy.Status == BuddyStatus.Approved)
			{
				AddFriendToDropDown(buddy.UserID, buddy.DisplayName);
			}
		}
	}

	private void AddFriendToDropDown(string userID, string displayName)
	{
		KAWidget kAWidget = _DropDownListMenu.AddWidget(userID, null);
		kAWidget.SetText(displayName);
		kAWidget.name = _DropDownListMenu._Template.name;
		MessageBoardFriendUserData userData = new MessageBoardFriendUserData(userID, displayName);
		kAWidget.SetUserData(userData);
	}

	public void OnFriendNameSelected(MessageBoardFriendUserData data)
	{
		mUserID = data.pUserID;
		mTxtName.SetText(data.pUserName);
		KAUI.RemoveExclusive(_DropDownListMenu);
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
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mCloseBtn)
		{
			SetVisibility(show: false);
			KAInput.ResetInputAxes();
		}
		else if (item == mEditBox)
		{
			if (mHasBlockedWord)
			{
				mHasBlockedWord = false;
				mEditOpen.pInput.characterLimit = _MaxCharacters;
				mEditOpen.pInput.label.supportEncoding = false;
				item.SetText(mOriginalText);
			}
		}
		else if (item == mPostBtn)
		{
			mTimeoutTimer = _TimeoutDuration;
			mPostMessage = "";
			mPostMessage = mEditBox.GetText();
			KAUICursorManager.SetDefaultCursor("Loading");
			SetInteractive(interactive: false);
			string attribute = UiMessageBoard.ATTRIBUTE_COLOR + "=" + mColor;
			if (mMessageID == -1)
			{
				_Board.PostMessage(mUserID, mPostMessage, MessageLevel.WhiteList, attribute, mIsPrivateMessage);
			}
			else
			{
				_Board.ReplyToMessage(mUserID, mMessageID, mPostMessage, MessageLevel.WhiteList, attribute, mIsPrivateMessage);
			}
		}
		else if (item == mClearBtn)
		{
			mOriginalText = "";
			mEditBox.SetText("");
			mTxtCount.SetText("0/" + _MaxCharacters);
			mPostBtn.SetState(KAUIState.DISABLED);
		}
		else if (item == mFriendsMenuBtn)
		{
			_DropDownListMenu.SetVisibility(!_DropDownListMenu.GetVisibility());
			KAUI.SetExclusive(_DropDownListMenu);
		}
		else if (item.name.StartsWith("Color"))
		{
			mColorButton.SetChecked(isChecked: true);
			mColorButton.SetInteractive(isInteractive: true);
			mColorButton = (KAToggleButton)item;
			mColorButton.SetChecked(isChecked: false);
			mColorButton.SetInteractive(isInteractive: false);
			mColor = item.name.Substring(5, item.name.Length - 8);
			_ColorBkg.Play(mColor, 1);
		}
	}

	public override void OnSelect(KAWidget inWidget, bool inSelected)
	{
		base.OnSelect(inWidget, inSelected);
		if (UtPlatform.IsMobile() && inWidget == mEditOpen && !inSelected && mPostBtn != null && mPostBtn.IsActive())
		{
			mPostBtn.collider.enabled = true;
		}
	}

	public void OnFBAuthDBLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		if (inLoadEvent == RsResourceLoadEvent.COMPLETE)
		{
			SetInteractive(interactive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			Object.Instantiate((GameObject)inObject).name = "PfUiFBAuthorizeDBDO";
		}
	}

	public void PostEventHandler(RMFMessageBoardPostResult result)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		SetInteractive(interactive: true);
		mTimeoutTimer = 0f;
		if (result == null)
		{
			return;
		}
		if (result.ErrorID == 0)
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
			SetVisibility(show: false);
			_Board.OnPost(mUserID, message, mMessageID);
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
		if (!GetVisibility())
		{
			return;
		}
		if (mTxtCount != null && mEditBox != null && !mHasBlockedWord)
		{
			mTxtCount.SetText(mEditBox.GetText().Length + "/" + _MaxCharacters);
			string text = mEditBox.GetText().Trim();
			if (text.Length == 0 || text == mEditBox._DefaultText.GetLocalizedString())
			{
				if (mPostBtn.GetState() != KAUIState.DISABLED)
				{
					mPostBtn.SetState(KAUIState.DISABLED);
				}
			}
			else if (mPostBtn.GetState() != 0)
			{
				mPostBtn.SetState(KAUIState.INTERACTIVE);
			}
		}
		if (mTimeoutTimer > 0f)
		{
			mTimeoutTimer -= Time.deltaTime;
			if (mTimeoutTimer <= 0f)
			{
				SetInteractive(interactive: true);
				ShowDialog(_TimeoutText._ID, _TimeoutText._Text);
			}
		}
	}

	private void ShowDialog(int id, string text)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		GameObject gameObject = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSmSocial"));
		mKAUIGenericDB = gameObject.GetComponent<KAUIGenericDB>();
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._OKMessage = "OnOK";
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mKAUIGenericDB.SetTextByID(id, text, interactive: false);
		KAUI.SetExclusive(mKAUIGenericDB, _MaskColor);
	}

	public void OnOK()
	{
		if (mKAUIGenericDB != null)
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			Object.Destroy(mKAUIGenericDB.gameObject);
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
				array2[i] = "[c][FF0000]" + array[i] + "[" + HexUtil.ColorToHexNoAlpha(mEditOpen.pInput.activeTextColor) + "]";
			}
			text = text + " " + array2[i];
		}
		mEditOpen.pInput.characterLimit = -1;
		mEditOpen.pInput.label.supportEncoding = true;
		mTxtCount.SetText(inOriginalText.Length + "/" + _MaxCharacters);
		mEditBox.SetText(text);
		mHasBlockedWord = true;
	}
}
