using System.Collections.Generic;

public class UiMessageBoardTabMenu : KAUIMenu
{
	public UiMessageBoard _MessageBoard;

	public UiClansMessageBoard _ClanMessageBoard;

	public UiMessageBoardPost _PostUI;

	public KAToggleButton _AllTabBtn;

	public KAToggleButton _MessageTabBtn;

	public KAToggleButton _PrivateMessageTabBtn;

	public KAToggleButton _MuttCareTabBtn;

	public KAToggleButton _ChallengeTabBtn;

	public KAToggleButton _GiftTabBtn;

	public KAToggleButton _ClanMessagesTabBtn;

	public KAToggleButton _ClanNewsTabBtn;

	protected Dictionary<KAWidget, CombinedMessageType> mTabBtnType = new Dictionary<KAWidget, CombinedMessageType>();

	private bool mShowClanWidgets;

	private bool mIsMyBoard;

	private bool mIsClanReady;

	private bool mIsProfileReady;

	protected override void Start()
	{
		base.Start();
		if (_AllTabBtn != null)
		{
			mTabBtnType[_AllTabBtn] = CombinedMessageType.ALL;
		}
		if (_MessageTabBtn != null)
		{
			mTabBtnType[_MessageTabBtn] = CombinedMessageType.MESSAGE_BOARD;
		}
		if (_PrivateMessageTabBtn != null)
		{
			mTabBtnType[_PrivateMessageTabBtn] = CombinedMessageType.MESSAGE_BOARD;
		}
		if (_MuttCareTabBtn != null)
		{
			mTabBtnType[_MuttCareTabBtn] = CombinedMessageType.USER_MESSAGE_QUEUE;
		}
		if (_ChallengeTabBtn != null)
		{
			mTabBtnType[_ChallengeTabBtn] = CombinedMessageType.CHALLENGE;
		}
		if (_GiftTabBtn != null)
		{
			mTabBtnType[_GiftTabBtn] = CombinedMessageType.USER_MESSAGE_QUEUE;
		}
		if (_ClanMessagesTabBtn != null)
		{
			mTabBtnType[_ClanMessagesTabBtn] = CombinedMessageType.ALL;
		}
		if (_ClanNewsTabBtn != null)
		{
			mTabBtnType[_ClanNewsTabBtn] = CombinedMessageType.NEWS;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mIsClanReady && mIsProfileReady)
		{
			UpdateTabVisibility();
			mIsClanReady = (mIsProfileReady = false);
		}
	}

	private void MessageBoardInitialized(string userID)
	{
		mIsMyBoard = userID == UserInfo.pInstance.UserID;
		ResetToggleState();
		mIsProfileReady = true;
	}

	private void ResetToggleState()
	{
		if (mIsMyBoard)
		{
			if (_AllTabBtn != null)
			{
				SetSelectedItem(_AllTabBtn);
				_AllTabBtn.SetInteractive(isInteractive: false);
				_AllTabBtn.SetChecked(isChecked: true);
			}
		}
		else if (_MessageTabBtn != null)
		{
			SetSelectedItem(_MessageTabBtn);
			_MessageTabBtn.SetInteractive(isInteractive: false);
			_MessageTabBtn.SetChecked(isChecked: true);
		}
	}

	private void UpdateTabVisibility()
	{
		if (_AllTabBtn != null && mIsMyBoard)
		{
			AddWidget(_AllTabBtn);
			_AllTabBtn.SetVisibility(inVisible: true);
		}
		if (_MessageTabBtn != null)
		{
			AddWidget(_MessageTabBtn);
			_MessageTabBtn.SetVisibility(inVisible: true);
		}
		if (_PrivateMessageTabBtn != null)
		{
			AddWidget(_PrivateMessageTabBtn);
			_PrivateMessageTabBtn.SetVisibility(inVisible: true);
		}
		if (mIsMyBoard && _ChallengeTabBtn != null)
		{
			AddWidget(_ChallengeTabBtn);
			_ChallengeTabBtn.SetVisibility(mIsMyBoard);
		}
		if (mShowClanWidgets)
		{
			if (_ClanMessagesTabBtn != null)
			{
				AddWidget(_ClanMessagesTabBtn);
				_ClanMessagesTabBtn.SetVisibility(inVisible: true);
			}
			if (_ClanNewsTabBtn != null)
			{
				AddWidget(_ClanNewsTabBtn);
				_ClanNewsTabBtn.SetVisibility(inVisible: true);
			}
		}
		if (mIsMyBoard && _GiftTabBtn != null)
		{
			AddWidget(_GiftTabBtn);
			_GiftTabBtn.SetVisibility(inVisible: true);
		}
		SetVisibility(inVisible: true);
	}

	private void OnDisable()
	{
		ResetToggleState();
	}

	public void OnClan(Group inGroup)
	{
		mShowClanWidgets = inGroup != null && UserProfile.pProfileData.InGroup(inGroup.GroupID);
		mIsClanReady = true;
	}

	private void OnClanFailed()
	{
		mShowClanWidgets = false;
		mIsClanReady = true;
	}

	public override void SetSelectedItem(KAWidget inWidget)
	{
		if (mSelectedItem != null)
		{
			mSelectedItem.SetInteractive(isInteractive: true);
		}
		base.SetSelectedItem(inWidget);
	}

	public void ProcessClick(KAWidget item)
	{
		_ClanMessageBoard.ProcessMessageType(mTabBtnType[item], force: true);
		_ClanMessageBoard.SetVisibility(inVisible: true);
		if (_MessageBoard != null)
		{
			_MessageBoard.SetVisibility(inVisible: false);
		}
	}

	public void SelectGiftTab()
	{
		if (_GiftTabBtn != null)
		{
			SetSelectedItem(_GiftTabBtn);
			_GiftTabBtn.SetInteractive(isInteractive: false);
			_GiftTabBtn.SetChecked(isChecked: true);
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if ((!_MessageBoard && !_ClanMessageBoard) || !mTabBtnType.ContainsKey(item))
		{
			return;
		}
		if ((bool)_MessageBoard)
		{
			_MessageBoard.pShowPrivateMessages = false;
		}
		string text = item.name;
		if (text == _ClanMessagesTabBtn.name || text == _ClanNewsTabBtn.name)
		{
			ProcessClick(item);
		}
		else
		{
			_MessageBoard.pShowPrivateMessages = text == _PrivateMessageTabBtn.name;
			if (text == _GiftTabBtn.name)
			{
				_MessageBoard.ProcessMessageType(mTabBtnType[item], 19);
			}
			else
			{
				_MessageBoard.ProcessMessageType(mTabBtnType[item], force: true);
			}
			_ClanMessageBoard.SetVisibility(inVisible: false);
			_MessageBoard.SetVisibility(inVisible: true);
		}
		if (mSelectedItem != null)
		{
			mSelectedItem.SetInteractive(isInteractive: false);
		}
	}
}
