using System.Collections.Generic;
using UnityEngine;

public class UiSelectFriend : KAUI
{
	public GameObject _MessageObject;

	public string _YesMessage;

	public string _NoMessage;

	public string _CloseMessage;

	public bool _AllowMultipleSelection = true;

	public bool _AutoRemoveProcessedRequestItems = true;

	public bool _UseMask;

	public GameObject _SelectFriendMenuObject;

	public GameObject _FriendRequestMenuObject;

	public UiSelectFriendMenu _SelectFriendMenu;

	public UiSelectFriendMenu _FriendRequestMenu;

	public KAWidget _TitleTxt;

	public KAWidget _YesBtn;

	public KAWidget _NoBtn;

	public KAWidget _CloseBtn;

	public KAWidget _TxtDialog;

	private bool mInitialized;

	private AvPhotoManager mStillPhotoManager;

	private string mSelectedFriend;

	private List<string> mSelectedFriendsList = new List<string>();

	private List<string> mPrevChallengedFriends = new List<string>();

	private string[] mCustomFriendList;

	private string[] mFriendRequests;

	private string[] mFriendDescription;

	private bool mIsFriendSelectionTab = true;

	private KAWidget mSelectedCheckBox;

	protected override void Start()
	{
		base.Start();
		if (!BuddyList.pIsReady)
		{
			BuddyList.Init();
		}
		SetInteractive(interactive: false);
		mStillPhotoManager = AvPhotoManager.Init("PfMessagePhotoMgr");
		if (_YesBtn != null)
		{
			_YesBtn.SetDisabled(isDisabled: true);
		}
		if (_UseMask)
		{
			KAUI.SetExclusive(this);
		}
		if (_YesBtn != null)
		{
			_YesBtn.SetVisibility(mIsFriendSelectionTab);
		}
		if (_NoBtn != null)
		{
			_NoBtn.SetVisibility(mIsFriendSelectionTab);
		}
		if (_CloseBtn != null)
		{
			_CloseBtn.SetVisibility(!mIsFriendSelectionTab);
		}
		if (_SelectFriendMenuObject != null)
		{
			_SelectFriendMenuObject.SetActive(mIsFriendSelectionTab);
		}
		if (_FriendRequestMenuObject != null)
		{
			_FriendRequestMenuObject.SetActive(!mIsFriendSelectionTab);
		}
		if (UtPlatform.IsiOS() && _YesBtn != null && _NoBtn != null)
		{
			Vector3 localPosition = _NoBtn.transform.localPosition;
			_NoBtn.transform.localPosition = _YesBtn.transform.localPosition;
			_YesBtn.transform.localPosition = localPosition;
		}
	}

	public void ShowFriendSelection(string inTitle, string inNoFriendsText, bool allowMultipleSelection, string[] inCustomFriendList = null)
	{
		if (inTitle != null && _TitleTxt != null)
		{
			_TitleTxt.SetText(inTitle);
		}
		if (inNoFriendsText != null && _TxtDialog != null)
		{
			_TxtDialog.SetText(inNoFriendsText);
		}
		_AllowMultipleSelection = allowMultipleSelection;
		mCustomFriendList = inCustomFriendList;
		mIsFriendSelectionTab = true;
	}

	public void ShowFriendRequest(string inTitle, string[] inRequestUserIds, string[] inDescription = null)
	{
		if (inTitle != null && _TitleTxt != null)
		{
			_TitleTxt.SetText(inTitle);
		}
		mFriendRequests = inRequestUserIds;
		mFriendDescription = inDescription;
		mIsFriendSelectionTab = false;
	}

	public void SetMessage(GameObject inMessageObject, string inYesMessage, string inNoMessage, string inCloseMessage)
	{
		_MessageObject = inMessageObject;
		_YesMessage = inYesMessage;
		_NoMessage = inNoMessage;
		_CloseMessage = inCloseMessage;
	}

	public void Init(List<string> inChallengedFriends, GameObject msgObject)
	{
		if (inChallengedFriends != null)
		{
			mPrevChallengedFriends = inChallengedFriends;
		}
		_MessageObject = msgObject;
		if (!BuddyList.pIsReady)
		{
			BuddyList.Init();
		}
	}

	public void SetOkButtonText(LocaleString okBtnText)
	{
		if (_YesBtn != null)
		{
			_YesBtn.SetTextByID(okBtnText._ID, okBtnText._Text);
		}
	}

	public void SetTitleText(LocaleString titleText)
	{
		if (_TitleTxt != null)
		{
			_TitleTxt.SetTextByID(titleText._ID, titleText._Text);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (UtPlatform.IsAndroid() && Input.GetKeyUp(KeyCode.Escape) && GetVisibility() && GetState() == KAUIState.INTERACTIVE && _NoBtn != null && _NoBtn.GetVisibility())
		{
			OnClick(_NoBtn);
		}
		if (mInitialized || !BuddyList.pIsReady)
		{
			return;
		}
		mInitialized = true;
		SetInteractive(interactive: true);
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (mIsFriendSelectionTab)
		{
			if (mCustomFriendList != null && mCustomFriendList.Length != 0)
			{
				string[] array = mCustomFriendList;
				for (int i = 0; i < array.Length; i++)
				{
					string[] array2 = array[i].Split('|');
					AddFriend(array2[0], (array2.Length > 1) ? array2[1] : "");
				}
			}
			else
			{
				Buddy[] pList = BuddyList.pList;
				foreach (Buddy buddy in pList)
				{
					if (buddy.Status == BuddyStatus.Approved)
					{
						AddFriend(buddy.UserID, buddy.DisplayName);
					}
				}
			}
			_SelectFriendMenu.pViewChanged = true;
			if (_TxtDialog != null)
			{
				_TxtDialog.SetVisibility(_SelectFriendMenu.GetNumItems() == 0);
			}
			return;
		}
		for (int j = 0; j < mFriendRequests.Length; j++)
		{
			string userID = mFriendRequests[j];
			KAWidget kAWidget = DuplicateWidget(_FriendRequestMenu._Template);
			kAWidget.name = userID;
			kAWidget.SetVisibility(inVisible: true);
			Buddy buddy2 = BuddyList.pInstance.GetBuddy(userID);
			if (buddy2 != null)
			{
				kAWidget.FindChildItem("TxtName").SetText(buddy2.DisplayName);
			}
			else
			{
				kAWidget.FindChildItem("TxtName").SetText("");
			}
			if (mFriendDescription != null && j < mFriendDescription.Length)
			{
				KAWidget kAWidget2 = kAWidget.FindChildItem("TxtMessage");
				if (kAWidget2 != null)
				{
					kAWidget2.SetText(mFriendDescription[j]);
					kAWidget2.SetVisibility(inVisible: true);
				}
			}
			_FriendRequestMenu.AddWidget(kAWidget);
			UiSelectFriendItemData userData = new UiSelectFriendItemData(mStillPhotoManager);
			kAWidget.SetUserData(userData);
		}
		_FriendRequestMenu.pViewChanged = true;
		if (_TxtDialog != null)
		{
			_TxtDialog.SetVisibility(_FriendRequestMenu.GetNumItems() == 0);
		}
	}

	private void AddFriend(string inUserID, string displayName)
	{
		KAWidget kAWidget = DuplicateWidget(_SelectFriendMenu._Template);
		kAWidget.name = inUserID;
		kAWidget.SetVisibility(inVisible: true);
		kAWidget.FindChildItem("TxtName").SetText(displayName);
		_SelectFriendMenu.AddWidget(kAWidget);
		UiSelectFriendItemData userData = new UiSelectFriendItemData(mStillPhotoManager);
		kAWidget.SetUserData(userData);
		if (mPrevChallengedFriends.Contains(inUserID))
		{
			((KACheckBox)kAWidget.FindChildItem("CheckBtn")).SetChecked(isChecked: true);
			for (int i = 0; i < kAWidget.GetNumChildren(); i++)
			{
				kAWidget.FindChildItemAt(i).SetDisabled(isDisabled: true);
			}
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (inWidget.name != "CheckBtn")
		{
			base.OnClick(inWidget);
		}
		if (inWidget == _YesBtn)
		{
			if (_UseMask)
			{
				KAUI.RemoveExclusive(this);
			}
			Object.Destroy(base.gameObject);
			if (_MessageObject != null && !string.IsNullOrEmpty(_YesMessage))
			{
				if (_AllowMultipleSelection)
				{
					_MessageObject.SendMessage(_YesMessage, mSelectedFriendsList.ToArray(), SendMessageOptions.DontRequireReceiver);
				}
				else
				{
					_MessageObject.SendMessage(_YesMessage, mSelectedFriend, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		else if (inWidget == _CloseBtn || inWidget == _NoBtn)
		{
			if (_UseMask)
			{
				KAUI.RemoveExclusive(this);
			}
			Object.Destroy(base.gameObject);
			if (!(_MessageObject != null))
			{
				return;
			}
			if (inWidget == _CloseBtn)
			{
				if (!string.IsNullOrEmpty(_CloseMessage))
				{
					_MessageObject.SendMessage(_CloseMessage, null, SendMessageOptions.DontRequireReceiver);
				}
			}
			else if (!string.IsNullOrEmpty(_NoMessage))
			{
				_MessageObject.SendMessage(_NoMessage, null, SendMessageOptions.DontRequireReceiver);
			}
		}
		else if (inWidget.name == "CheckBtn")
		{
			string text = inWidget.GetParentItem().name;
			if (_AllowMultipleSelection)
			{
				if (mSelectedFriendsList.Contains(text))
				{
					mSelectedFriendsList.Remove(text);
				}
				else
				{
					mSelectedFriendsList.Add(text);
				}
				if (_YesBtn != null)
				{
					_YesBtn.SetDisabled(mSelectedFriendsList.Count == 0);
				}
				return;
			}
			if (mSelectedCheckBox != null)
			{
				((KACheckBox)mSelectedCheckBox).SetChecked(isChecked: false);
			}
			bool flag = mSelectedCheckBox != inWidget;
			mSelectedFriend = (flag ? text : null);
			mSelectedCheckBox = (flag ? inWidget : null);
			if (_YesBtn != null)
			{
				_YesBtn.SetDisabled(string.IsNullOrEmpty(mSelectedFriend));
			}
		}
		else if (inWidget.name == "YesBtn")
		{
			KAWidget parentItem = inWidget.GetParentItem();
			string value = parentItem.name;
			if (_MessageObject != null && !string.IsNullOrEmpty(_YesMessage))
			{
				_MessageObject.SendMessage(_YesMessage, value, SendMessageOptions.DontRequireReceiver);
			}
			if (_AutoRemoveProcessedRequestItems)
			{
				_FriendRequestMenu.RemoveWidget(parentItem);
			}
			if (_TxtDialog != null)
			{
				_TxtDialog.SetVisibility(_FriendRequestMenu.GetNumItems() == 0);
			}
		}
		else if (inWidget.name == "NoBtn")
		{
			KAWidget parentItem2 = inWidget.GetParentItem();
			string value2 = inWidget.GetParentItem().name;
			if (_MessageObject != null && !string.IsNullOrEmpty(_NoMessage))
			{
				_MessageObject.SendMessage(_NoMessage, value2, SendMessageOptions.DontRequireReceiver);
			}
			if (_AutoRemoveProcessedRequestItems)
			{
				_FriendRequestMenu.RemoveWidget(parentItem2);
			}
			if (_TxtDialog != null)
			{
				_TxtDialog.SetVisibility(_FriendRequestMenu.GetNumItems() == 0);
			}
		}
	}
}
