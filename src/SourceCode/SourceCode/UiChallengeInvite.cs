using System;
using System.Collections.Generic;
using UnityEngine;

public class UiChallengeInvite : KAUI
{
	public UiChallengeInviteMenu _Menu;

	public UiClansDetails _UiClansDetails;

	public UiClansSearch _UiClansJoin;

	public UiBuddyListMenu _UiBuddyListMenu;

	public KAWidget _BuddyListTemplate;

	public int _MaxInviteeCount = 25;

	public LocaleString _InviteListFullText = new LocaleString("You cannot challenge anymore friends!");

	public LocaleString _InviteSentText = new LocaleString("Challenge sent successfully!");

	public LocaleString _InviteFailedText = new LocaleString("Failed sending the challenge!");

	public LocaleString _InviteAlreadySentText = new LocaleString("You have already challenged this player! Do you want to send it again!");

	public int _ChallengeExpiryDuration = 7;

	public int _InitiateChallengeAchievementID;

	private AvPhotoManager mStillPhotoManager;

	private KAToggleButton mBtnClanChallenge;

	private KAToggleButton mBtnFriendChallenge;

	private KAWidget mBtnClearAll;

	private KAWidget mBtnChallenge;

	private KAWidget mTxtInviteCount;

	private KAWidget mBtnClose;

	private KAWidget mBtnBack;

	private KAWidget mTxtNoBuddies;

	private bool mLoadBuddyList;

	private bool mLoadClanList;

	private KAUIGenericDB mKAUIGenericDB;

	private int mModuleID;

	private int mLevelID;

	private int mDifficultyID;

	private int mScore;

	private GameObject mMessageObject;

	private string mMessageFunctionName;

	private List<string> mInvitedFriends = new List<string>();

	private string mResendInviteUserID;

	private string mResendInviteUserName;

	private static bool mIsChallengeSent;

	private static UiChallengeInvite mInstance;

	public static bool pIsChallengeSent => mIsChallengeSent;

	public static UiChallengeInvite pInstance => mInstance;

	protected override void Start()
	{
		base.Start();
		mInstance = this;
		mStillPhotoManager = AvPhotoManager.Init("PfMessagePhotoMgr");
		mBtnClanChallenge = (KAToggleButton)FindItem("ClanChallenge");
		mBtnFriendChallenge = (KAToggleButton)FindItem("FriendChallenge");
		mBtnClearAll = FindItem("BtnClearAll");
		mBtnChallenge = FindItem("BtnChallenge");
		mTxtInviteCount = FindItem("TxtInviteCount");
		mBtnClose = FindItem("CloseBtn", recursive: false);
		mBtnBack = FindItem("BackBtn");
		mTxtNoBuddies = FindItem("TxtNoBuddies");
	}

	public static void SetData(int inModuleID, int inLevelID, int inDifficulty, int inScore)
	{
		if (mInstance != null)
		{
			mIsChallengeSent = false;
			mInstance.mInvitedFriends.Clear();
			mInstance.mModuleID = inModuleID;
			mInstance.mLevelID = inLevelID;
			mInstance.mDifficultyID = inDifficulty;
			mInstance.mScore = inScore;
		}
	}

	public static void Show(GameObject inMessageObject = null, string inMessageFunction = null)
	{
		if (mInstance != null)
		{
			mInstance.mMessageObject = inMessageObject;
			mInstance.mMessageFunctionName = inMessageFunction;
			mInstance.SetVisibility(inVisible: true);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnClose)
		{
			OnExit();
		}
		if (inWidget == mBtnFriendChallenge)
		{
			OnClick(mBtnClearAll);
			_UiClansJoin.gameObject.SetActive(value: false);
			_UiClansDetails.Show(show: false, null);
			mBtnFriendChallenge.SetInteractive(isInteractive: false);
			mBtnClanChallenge.SetInteractive(isInteractive: true);
			mBtnBack.SetVisibility(inVisible: false);
			if (!BuddyList.pIsReady)
			{
				BuddyList.Init();
			}
			SetInteractive(interactive: false);
			mLoadBuddyList = true;
		}
		else if (inWidget == mBtnClanChallenge)
		{
			OnClick(mBtnClearAll);
			_UiBuddyListMenu.SetVisibility(inVisible: false);
			_UiClansDetails.Show(show: false, null);
			mTxtNoBuddies.SetVisibility(inVisible: false);
			mBtnFriendChallenge.SetInteractive(isInteractive: true);
			mBtnClanChallenge.SetInteractive(isInteractive: false);
			if (!Group.pIsAllGroupsReady)
			{
				Group.Reset();
				Group.Init(includeMemberCount: true);
			}
			SetInteractive(interactive: false);
			mLoadClanList = true;
		}
		else if (inWidget == mBtnClearAll)
		{
			DisableInvitee(null, disable: false);
			_Menu.ClearItems();
			UpdateInviteCount();
		}
		else if (inWidget == mBtnChallenge)
		{
			SetInteractive(interactive: false);
			List<Guid> list = new List<Guid>();
			foreach (KAWidget item in _Menu.GetItems())
			{
				list.Add(new Guid(item.name));
			}
			UserAchievementTask.Set(_InitiateChallengeAchievementID);
			WsWebService.InitiateChallenge(list.ToArray(), mModuleID, mLevelID, mDifficultyID, 604, mScore, _ChallengeExpiryDuration, ServiceEventHandler, null);
		}
		else if (inWidget == mBtnBack)
		{
			_UiClansJoin.gameObject.SetActive(value: true);
			_UiClansDetails.Show(show: false, null);
			mBtnBack.SetVisibility(inVisible: false);
		}
		else if (inWidget.name == "CloseBtn")
		{
			KAWidget parentItem = inWidget.GetParentItem();
			DisableInvitee(parentItem, disable: false);
			if (parentItem != null)
			{
				_Menu.RemoveWidget(parentItem);
			}
			UpdateInviteCount();
		}
		BuddyListData buddyListData = (BuddyListData)inWidget.GetUserData();
		if (buddyListData != null)
		{
			AddInvite(buddyListData._UserID, buddyListData._Item.FindChildItem("TxtBuddyName").GetText());
		}
	}

	private void OnExit()
	{
		KillGenericDB();
		OnClick(mBtnClearAll);
		SetVisibility(inVisible: false);
		_UiClansDetails.Show(show: false, null);
		_UiClansJoin.gameObject.SetActive(value: false);
		if (mMessageObject != null && !string.IsNullOrEmpty(mMessageFunctionName))
		{
			mMessageObject.SendMessage(mMessageFunctionName, null, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void OnClanSelected(ClanData inClanData)
	{
		_UiClansJoin.gameObject.SetActive(value: false);
		_UiClansDetails.Show(show: true, inClanData._Group);
		mBtnBack.SetVisibility(inVisible: true);
	}

	public void OnClanMemberSelected(ClanMemberData inMemberData)
	{
		AddInvite(inMemberData._Member.UserID, inMemberData._Member.DisplayName);
	}

	private void UpdateInviteCount()
	{
		int itemCount = _Menu.GetItemCount();
		mTxtInviteCount.SetText(itemCount + "/" + _MaxInviteeCount);
		mBtnChallenge.SetDisabled(itemCount == 0);
		mBtnClearAll.SetDisabled(itemCount == 0);
	}

	public override void SetInteractive(bool interactive)
	{
		base.SetInteractive(interactive);
		KAUICursorManager.SetDefaultCursor(interactive ? "Arrow" : "Loading");
		_Menu.SetInteractive(interactive);
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (inVisible)
		{
			mBtnFriendChallenge.OnClick();
			OnClick(mBtnFriendChallenge);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mLoadClanList && Group.pIsReady)
		{
			if (Group.pGroups != null && Group.pGroups.Count > 0 && !Group.pGroups[0].TotalMemberCount.HasValue)
			{
				Group.Reset();
				Group.Init(includeMemberCount: true);
			}
			else
			{
				mLoadClanList = false;
				SetInteractive(interactive: true);
				_UiClansJoin.gameObject.SetActive(value: true);
			}
		}
		if (!mLoadBuddyList || !BuddyList.pIsReady)
		{
			return;
		}
		mLoadBuddyList = false;
		SetInteractive(interactive: true);
		_UiBuddyListMenu.SetVisibility(inVisible: true);
		if (_UiBuddyListMenu.GetNumItems() == 0)
		{
			Buddy[] pList = BuddyList.pList;
			foreach (Buddy buddy in pList)
			{
				if (buddy.Status == BuddyStatus.Approved)
				{
					AddBuddy(buddy.UserID, buddy.DisplayName, buddy.BestBuddy, buddy.OnMobile);
				}
			}
		}
		if (_UiBuddyListMenu.GetNumItems() == 0)
		{
			mTxtNoBuddies.SetVisibility(inVisible: true);
		}
		DisableInvitee();
	}

	private void OnMembersDetailsReady(GroupMember[] inMembers)
	{
		DisableInvitee();
	}

	public void AddBuddy(string inUserID, string inName, bool bestBuddy, bool onMobile)
	{
		KAWidget kAWidget = DuplicateWidget(_BuddyListTemplate);
		kAWidget.SetText(inUserID);
		kAWidget.SetVisibility(inVisible: true);
		kAWidget.SetInteractive(isInteractive: true);
		KAWidget kAWidget2 = kAWidget.FindChildItem("TxtBuddyName");
		kAWidget2.SetText(inName);
		kAWidget2.SetInteractive(isInteractive: true);
		if (string.IsNullOrEmpty(inName))
		{
			WsWebService.GetDisplayNameByUserID(inUserID, ServiceEventHandler, kAWidget2);
		}
		KAWidget kAWidget3 = kAWidget.FindChildItem("BuddyPicture");
		mStillPhotoManager.TakePhotoUI(inUserID, (Texture2D)kAWidget3.GetTexture(), ProfileAvPhotoCallback, kAWidget3);
		kAWidget2 = kAWidget.FindChildItem("MobilePic");
		if (kAWidget2 != null)
		{
			kAWidget2.SetVisibility(onMobile);
		}
		kAWidget2 = kAWidget.FindChildItem("ClanBtn");
		if (kAWidget2 != null)
		{
			kAWidget2.SetDisabled(isDisabled: true);
		}
		_UiBuddyListMenu.AddWidget(kAWidget);
		kAWidget.SetUserData(new BuddyListData(inUserID, inShowAchievements: true));
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.GET_DISPLAYNAME_BY_USER_ID:
			if (inEvent == WsServiceEvent.COMPLETE)
			{
				((KAWidget)inUserData).SetText((string)inObject);
			}
			break;
		case WsServiceType.INITIATE_CHALLENGE:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				if ((ChallengeInfo)inObject != null)
				{
					foreach (KAWidget item in _Menu.GetItems())
					{
						if (!mInvitedFriends.Contains(item.name))
						{
							mInvitedFriends.Add(item.name);
						}
					}
				}
				SetInteractive(interactive: true);
				mIsChallengeSent = true;
				ShowGenericDB("PfKAUIGenericDB", _InviteSentText.GetLocalizedString(), null, null, null, "OnExit", null);
				break;
			case WsServiceEvent.ERROR:
				SetInteractive(interactive: true);
				ShowGenericDB("PfKAUIGenericDB", _InviteFailedText.GetLocalizedString(), null, null, null, "OnExit", null);
				break;
			}
			break;
		}
	}

	public void ProfileAvPhotoCallback(Texture tex, object inUserData)
	{
		KAWidget kAWidget = (KAWidget)inUserData;
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inVisible: false);
			kAWidget.SetTexture((Texture2D)tex);
			kAWidget.SetVisibility(inVisible: true);
		}
	}

	private void AddInvite(string inUserID, string inDisplayName, bool forceAdd = false)
	{
		if (_Menu.GetItemCount() >= _MaxInviteeCount)
		{
			ShowGenericDB("PfKAUIGenericDB", _InviteListFullText.GetLocalizedString(), null, null, null, "KillGenericDB", null);
			return;
		}
		if (!forceAdd && mInvitedFriends.Contains(inUserID))
		{
			mResendInviteUserID = inUserID;
			mResendInviteUserName = inDisplayName;
			ShowGenericDB("PfKAUIGenericDB", _InviteAlreadySentText.GetLocalizedString(), null, "OnResendInvite", "KillGenericDB", null, null);
			return;
		}
		KAWidget kAWidget = DuplicateWidget(_Menu._Template);
		kAWidget.SetVisibility(inVisible: true);
		kAWidget.name = inUserID;
		kAWidget.FindChildItem("TxtBuddyName").SetText(inDisplayName);
		_Menu.AddWidget(kAWidget);
		UpdateInviteCount();
		DisableInvitee(kAWidget);
	}

	private void OnResendInvite()
	{
		KillGenericDB();
		AddInvite(mResendInviteUserID, mResendInviteUserName, forceAdd: true);
	}

	private void ShowGenericDB(string inGenericDBName, string inString, string inTitle, string inYesMessage, string inNoMessage, string inOkMessage, string inCloseMessage)
	{
		mKAUIGenericDB = GameUtilities.DisplayGenericDB(inGenericDBName, inString, inTitle, base.gameObject, inYesMessage, inNoMessage, inOkMessage, inCloseMessage);
	}

	private void KillGenericDB()
	{
		if (mKAUIGenericDB != null)
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}

	private void DisableInvitee(KAWidget inInvitee = null, bool disable = true)
	{
		List<KAWidget> list = new List<KAWidget>();
		if (inInvitee != null)
		{
			list.Add(inInvitee);
		}
		else
		{
			list = new List<KAWidget>(_Menu.GetItems());
		}
		foreach (KAWidget item in list)
		{
			if (_UiClansDetails.gameObject.activeInHierarchy && _UiClansDetails.GetVisibility())
			{
				foreach (KAWidget item2 in _UiClansDetails._Menu.GetItems())
				{
					ClanMemberData clanMemberData = (ClanMemberData)item2.GetUserData();
					if (clanMemberData != null && clanMemberData._Member != null && clanMemberData._Member.UserID == item.name)
					{
						item2.SetDisabled(disable);
					}
				}
			}
			else
			{
				if (!_UiBuddyListMenu.gameObject.activeInHierarchy || !_UiBuddyListMenu.GetVisibility())
				{
					continue;
				}
				foreach (KAWidget item3 in _UiBuddyListMenu.GetItems())
				{
					BuddyListData buddyListData = (BuddyListData)item3.GetUserData();
					if (buddyListData != null && buddyListData._UserID == item.name)
					{
						item3.SetDisabled(disable);
					}
				}
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		mInstance = null;
	}
}
