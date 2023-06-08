using System;
using System.Collections.Generic;
using UnityEngine;

public class UiBuddyMessage : KAUI
{
	public BuddyMessageStrings _Strings;

	public int _NumberOfCoins;

	public int _ApproveBuddyAchievementID = 60;

	public ClansJoinGroupResultInfo[] _JoinGroupResultInfo;

	public UpdateInviteResultInfo[] _UpdateInviteResultInfo;

	public int _JoinClanAchievementTaskID = 164;

	public int _AddMembersToClanAchievementTaskID = 181;

	private GameObject mUiGenericDB;

	private KAUIGenericDB mKAUIGenericDB;

	private bool mRecheckMessages;

	private Group mInvitedGroup;

	private static MessageInfo mMessageInfo;

	private KAWidget mYesBtn;

	private KAWidget mNoBtn;

	private KAWidget mOKBtn;

	private KAWidget mIgnoreBtn;

	private KAWidget mScrollLtBtn;

	private KAWidget mScrollRtBtn;

	private KAWidget mTxtDialog;

	private KAWidget mTxtTitle;

	public int _SocialAddBuddyAchievementID = 103;

	public int _SocialOtherPlayerAddBuddyAchievementID = 124;

	protected override void Start()
	{
		base.Start();
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.pState = AvAvatarState.PAUSED;
		mYesBtn = FindItem("YesBtn");
		mNoBtn = FindItem("NoBtn");
		mOKBtn = FindItem("OKBtn");
		mIgnoreBtn = FindItem("IgnoreBtn");
		mScrollLtBtn = FindItem("ScrollLtBtn");
		mScrollRtBtn = FindItem("ScrollRtBtn");
		mTxtDialog = FindItem("TxtDialog");
		mTxtTitle = FindItem("TxtTitle");
		mScrollLtBtn.SetVisibility(inVisible: false);
		mScrollRtBtn.SetVisibility(inVisible: false);
		if (mYesBtn != null && mNoBtn != null && UtPlatform.IsiOS())
		{
			Vector3 localPosition = mNoBtn.transform.localPosition;
			mNoBtn.transform.localPosition = mYesBtn.transform.localPosition;
			mYesBtn.transform.localPosition = localPosition;
		}
		ShowMessage();
	}

	public static void LoadBuddyMessageDB(MessageInfo info)
	{
		mMessageInfo = info;
		UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfUiBuddyMessage"));
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "CloseBtn")
		{
			OnClose();
		}
		else if (inWidget == mYesBtn)
		{
			if (mMessageInfo.MessageTypeID == 27)
			{
				if (UserProfile.pProfileData.HasGroup() && !UserProfile.pProfileData.InGroup(mInvitedGroup.GroupID))
				{
					SetVisibility(inVisible: false);
					ShowGenericDB("PfKAUIGenericDB", _Strings._JoinGroupConfirmText.GetLocalizedString(), null, "OnJoinGroupConfirmYes", "OnJoinClanError", null, null);
				}
				else
				{
					OnJoinGroupConfirmYes();
				}
			}
			else if (mMessageInfo.MessageTypeID == 5)
			{
				SetInteractive(interactive: false);
				List<AchievementTask> obj = new List<AchievementTask>
				{
					new AchievementTask(_SocialAddBuddyAchievementID, mMessageInfo.FromUserID),
					new AchievementTask(mMessageInfo.FromUserID, _SocialOtherPlayerAddBuddyAchievementID, UserInfo.pInstance.UserID)
				};
				bool addFromFriendCode = ShouldAddThroughFriendCode(mMessageInfo);
				BuddyList.pInstance.ApproveBuddy(mMessageInfo.FromUserID, BuddyListEventHandler, addFromFriendCode);
				obj.Add(new AchievementTask(_ApproveBuddyAchievementID, mMessageInfo.FromUserID));
				obj.Add(new AchievementTask(mMessageInfo.FromUserID, _ApproveBuddyAchievementID, UserInfo.pInstance.UserID));
				UserAchievementTask.Set(obj.ToArray());
			}
			else if (mMessageInfo.MessageTypeID == -1)
			{
				SetInteractive(interactive: false);
				BuddyList.pInstance.JoinBuddy(mMessageInfo.FromUserID, BuddyListEventHandler);
			}
		}
		else if (inWidget == mNoBtn)
		{
			if (mMessageInfo.MessageTypeID == 27)
			{
				SetInteractive(interactive: false);
				WsWebService.UpdateInvite(new UpdateInviteRequest
				{
					FromUserID = mMessageInfo.FromUserID,
					GroupID = mInvitedGroup.GroupID,
					StatusID = GroupJoinRequestStatus.Rejected
				}, ServiceEventHandler, null);
			}
			else if (mMessageInfo.MessageTypeID == 5)
			{
				SetInteractive(interactive: false);
				KAUICursorManager.SetDefaultCursor("Loading");
				BuddyList.pInstance.RemoveBuddy(mMessageInfo.FromUserID, BuddyListEventHandler);
			}
			else if (mMessageInfo.MessageTypeID == -1)
			{
				RemoveCurrentMessage();
			}
		}
		else if (inWidget == mOKBtn)
		{
			if (mMessageInfo.MessageTypeID == 27 && mMessageInfo.UserMessageQueueID.HasValue)
			{
				WsWebService.SaveMessage(mMessageInfo.UserMessageQueueID.Value, isNew: false, isDeleted: true, null, null);
			}
			RemoveCurrentMessage();
		}
		else if (inWidget == mIgnoreBtn)
		{
			SetInteractive(interactive: false);
			BuddyList.pInstance.BlockBuddy(mMessageInfo.FromUserID, BuddyListEventHandler);
		}
	}

	public void ShowMessage()
	{
		SetVisibility(inVisible: true);
		TaggedMessageHelper taggedMessageHelper = new TaggedMessageHelper(mMessageInfo);
		Dictionary<string, string> dictionary = TaggedMessageHelper.Match(mMessageInfo.Data);
		if (dictionary.ContainsKey("GroupID"))
		{
			mInvitedGroup = Group.GetGroup(dictionary["GroupID"]);
		}
		string key = "Line1";
		if (mMessageInfo.MessageTypeID == 27)
		{
			key = "None";
			if (UserProfile.pProfileData.HasGroup())
			{
				key = ((UserRole)UserProfile.pProfileData.Groups[0].RoleID.Value).ToString();
			}
			mTxtTitle.SetText(_Strings._JoinClanRequestTitleText.GetLocalizedString());
			if (mInvitedGroup != null)
			{
				mTxtDialog.SetText(taggedMessageHelper.MemberMessage[key]);
				mYesBtn.SetVisibility(inVisible: true);
				mNoBtn.SetVisibility(inVisible: true);
				mOKBtn.SetVisibility(inVisible: false);
				mIgnoreBtn.SetVisibility(inVisible: false);
			}
			else
			{
				mYesBtn.SetVisibility(inVisible: false);
				mNoBtn.SetVisibility(inVisible: false);
				mOKBtn.SetVisibility(inVisible: true);
				mTxtDialog.SetText(_Strings._InviteNotValidText.GetLocalizedString());
			}
		}
		else
		{
			mTxtDialog.SetText(taggedMessageHelper.MemberMessage[key]);
			mTxtTitle.SetText(_Strings._FriendRequestTitleText.GetLocalizedString());
			mYesBtn.SetVisibility(mMessageInfo.MessageTypeID != -2);
			mNoBtn.SetVisibility(mMessageInfo.MessageTypeID != -2);
			mOKBtn.SetVisibility(mMessageInfo.MessageTypeID == -2);
			mIgnoreBtn.SetVisibility(mMessageInfo.MessageTypeID == 5);
		}
		mTxtDialog.SetVisibility(inVisible: true);
		SetInteractive(interactive: true);
	}

	public void RemoveCurrentMessage()
	{
		UiChatHistory.SystemMessageAccepted(mMessageInfo);
		BuddyList.pMessageList.Remove(mMessageInfo);
		BuddyList.UpdateBuddyBtnCount();
		OnClose();
	}

	public void OnClose()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		if (mRecheckMessages)
		{
			GameObject gameObject = GameObject.Find("PfCheckUserMessages");
			if (gameObject != null)
			{
				gameObject.SendMessage("ReCheckUserMessage");
			}
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.JOIN_GROUP:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				SetInteractive(interactive: true);
				GroupJoinResult groupJoinResult = (GroupJoinResult)inObject;
				if (groupJoinResult == null)
				{
					break;
				}
				bool flag2 = false;
				ClansJoinGroupResultInfo[] joinGroupResultInfo = _JoinGroupResultInfo;
				foreach (ClansJoinGroupResultInfo clansJoinGroupResultInfo in joinGroupResultInfo)
				{
					if (clansJoinGroupResultInfo._Status != groupJoinResult.Status)
					{
						continue;
					}
					flag2 = true;
					SetVisibility(inVisible: false);
					if (groupJoinResult.Success)
					{
						if (mInvitedGroup != null)
						{
							UserProfile.pProfileData.ReplaceGroup(0, mInvitedGroup.GroupID, UserRole.Member);
							AvatarData.SetGroupName(mInvitedGroup);
						}
						if (mMessageInfo.UserMessageQueueID.HasValue)
						{
							WsWebService.SaveMessage(mMessageInfo.UserMessageQueueID.Value, isNew: false, isDeleted: true, null, null);
						}
						ShowGenericDB("PfKAUIGenericDB", clansJoinGroupResultInfo._StatusText.GetLocalizedString(), null, "OnVisitClanPageYes", "OnVisitClanPageNo", null, null);
					}
					else if (groupJoinResult.Status == JoinGroupStatus.Error)
					{
						ShowGenericDB("PfKAUIGenericDB", clansJoinGroupResultInfo._StatusText.GetLocalizedString(), null, null, null, "OnJoinClanError", null);
					}
					else
					{
						if (mMessageInfo.UserMessageQueueID.HasValue)
						{
							WsWebService.SaveMessage(mMessageInfo.UserMessageQueueID.Value, isNew: false, isDeleted: true, null, null);
						}
						ShowGenericDB("PfKAUIGenericDB", clansJoinGroupResultInfo._StatusText.GetLocalizedString(), null, null, null, "OnJoinClanErrorRemoveCurrentMsg", null);
					}
					break;
				}
				if (!flag2 && groupJoinResult.Success)
				{
					OnVisitClanPageYes();
				}
				break;
			}
			case WsServiceEvent.ERROR:
				SetInteractive(interactive: true);
				break;
			}
			break;
		case WsServiceType.UPDATE_INVITE:
		{
			if ((uint)(inEvent - 2) > 1u)
			{
				break;
			}
			SetInteractive(interactive: true);
			if (mMessageInfo.UserMessageQueueID.HasValue)
			{
				WsWebService.SaveMessage(mMessageInfo.UserMessageQueueID.Value, isNew: false, isDeleted: true, null, null);
			}
			bool flag = false;
			UpdateInviteResult updateInviteResult = (UpdateInviteResult)inObject;
			if (updateInviteResult != null)
			{
				UpdateInviteResultInfo[] updateInviteResultInfo = _UpdateInviteResultInfo;
				foreach (UpdateInviteResultInfo updateInviteResultInfo2 in updateInviteResultInfo)
				{
					if (updateInviteResultInfo2._Status == updateInviteResult.Status)
					{
						flag = true;
						SetVisibility(inVisible: false);
						ShowGenericDB("PfKAUIGenericDB", updateInviteResultInfo2._StatusText.GetLocalizedString(), null, null, null, "OnJoinClanErrorRemoveCurrentMsg", null);
						break;
					}
				}
			}
			if (!flag)
			{
				RemoveCurrentMessage();
			}
			break;
		}
		}
	}

	private void BuddyListEventHandler(WsServiceType inType, object inResult)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (inType == WsServiceType.GET_BUDDY_LOCATION)
		{
			if (inResult == null || (JoinBuddyResultType)inResult == JoinBuddyResultType.JoinFailedCommon)
			{
				ShowDialog(_Strings._BuddyLocationErrorText._ID, _Strings._BuddyLocationErrorText._Text, joinError: true);
				return;
			}
		}
		else if (inResult == null)
		{
			ShowDialog(_Strings._GenericErrorText._ID, _Strings._GenericErrorText._Text, joinError: false);
			return;
		}
		SetInteractive(interactive: true);
		if (mMessageInfo.UserMessageQueueID.HasValue)
		{
			WsWebService.SaveMessage(mMessageInfo.UserMessageQueueID.Value, isNew: false, isDeleted: true, null, null);
		}
		bool flag = false;
		if (mMessageInfo.MessageTypeID == -1 && BuddyList.pMessageList.Count > 1)
		{
			flag = true;
		}
		RemoveCurrentMessage();
		if (flag)
		{
			OnClose();
		}
	}

	private void ShowGenericDB(string inGenericDBName, string inString, string inTitle, string inYesMessage, string inNoMessage, string inOkMessage, string inCloseMessage)
	{
		KillGenericDB();
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

	private void OnVisitClanPageYes()
	{
		AchievementTask achievementTask = new AchievementTask(_JoinClanAchievementTaskID, mInvitedGroup.GroupID);
		AchievementTask achievementTask2 = new AchievementTask(mInvitedGroup.GroupID, _AddMembersToClanAchievementTaskID, UserInfo.pInstance.UserID, 0, 2);
		UserAchievementTask.Set(achievementTask, achievementTask2);
		KillGenericDB();
		BuddyList.pMessageList.Remove(mMessageInfo);
		UiChatHistory.SystemMessageAccepted(mMessageInfo);
		OnClose();
		UiClans.ShowClan(UserInfo.pInstance.UserID, mInvitedGroup);
	}

	private void OnVisitClanPageNo()
	{
		AchievementTask achievementTask = new AchievementTask(_JoinClanAchievementTaskID, mInvitedGroup.GroupID);
		AchievementTask achievementTask2 = new AchievementTask(mInvitedGroup.GroupID, _AddMembersToClanAchievementTaskID, UserInfo.pInstance.UserID, 0, 2);
		UserAchievementTask.Set(achievementTask, achievementTask2);
		OnJoinClanError();
		RemoveCurrentMessage();
	}

	private void OnJoinClanErrorRemoveCurrentMsg()
	{
		OnJoinClanError();
		RemoveCurrentMessage();
	}

	private void OnJoinClanError()
	{
		SetVisibility(inVisible: true);
		KillGenericDB();
	}

	private void ShowDialog(int id, string text, bool joinError)
	{
		mUiGenericDB = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDB"));
		KAUIGenericDB kAUIGenericDB = (KAUIGenericDB)mUiGenericDB.GetComponent("KAUIGenericDB");
		kAUIGenericDB._MessageObject = base.gameObject;
		if (joinError)
		{
			kAUIGenericDB._CloseMessage = "OnJoinErrorClose";
		}
		else
		{
			kAUIGenericDB._CloseMessage = "OnErrorClose";
		}
		kAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: true);
		kAUIGenericDB.SetTextByID(id, text, interactive: false);
	}

	public void OnErrorClose()
	{
		if (mUiGenericDB != null)
		{
			UnityEngine.Object.Destroy(mUiGenericDB);
			mUiGenericDB = null;
		}
		SetInteractive(interactive: true);
		Input.ResetInputAxes();
	}

	public void OnJoinErrorClose()
	{
		OnErrorClose();
		RemoveCurrentMessage();
	}

	public void ReCheckUserMessage()
	{
		mRecheckMessages = true;
	}

	public static bool ShouldAddThroughFriendCode(MessageInfo mi)
	{
		bool result = false;
		string key = "AddByFriendCode";
		Dictionary<string, string> dictionary = TaggedMessageHelper.Match(mi.Data);
		if (dictionary.ContainsKey(key))
		{
			string value = dictionary[key];
			if (!string.IsNullOrEmpty(value))
			{
				result = Convert.ToBoolean(value);
			}
		}
		return result;
	}

	public override void SetInteractive(bool interactive)
	{
		base.SetInteractive(interactive);
		KAUICursorManager.SetDefaultCursor(interactive ? "Arrow" : "Loading");
	}

	private void OnJoinGroupConfirmYes()
	{
		KillGenericDB();
		SetVisibility(inVisible: true);
		SetInteractive(interactive: false);
		WsWebService.JoinGroup(new JoinGroupRequest
		{
			GroupID = mInvitedGroup.GroupID
		}, ServiceEventHandler, null);
	}
}
