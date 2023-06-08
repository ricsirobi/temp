using System.Collections.Generic;
using UnityEngine;

public class UiClansDetails : KAUI
{
	public UiClansCreate _UiClansCreate;

	public UiClansDetailsMenu _Menu;

	public UiClansMemberDB _UiClanMemberDB;

	public string _ApproveJoinRequestDBUrl = "RS_DATA/PfUiClansJoinRequestsDBDO.unity3d/PfUiClansJoinRequestsDBDO";

	public string _SelectFriendDBUrl = "RS_DATA/PfUiFriendsGenericDB.unity3d/PfUiFriendsGenericDB";

	public string _JoinRequestDBUrl = "RS_DATA/PfUiClansRequestDO.unity3d/PfUiClansRequestDO";

	public string _InviteFriendDBUrl = "RS_DATA/PfUiClansInviteDO.unity3d/PfUiClansInviteDO";

	public GameObject _MessageObject;

	public LocaleString _JoinedClanText = new LocaleString("You are now a member of this Clan.");

	public LocaleString _JoinAnotherClanText = new LocaleString("Are you sure you want to switch Clans?");

	public LocaleString _JoinErrorText = new LocaleString("Sorry! join Clans failed?");

	public LocaleString _PendingJoinRequestText = new LocaleString("Join Requests");

	public LocaleString _PendingJoinRequestEmptyText = new LocaleString("No join requests available.");

	public LocaleString _JoinRequestDefaultMessageText = new LocaleString("Hello, Please add me to your Clan.");

	public LocaleString _SelectNewLeaderText = new LocaleString("Select a New Leader");

	public LocaleString _LeaveClanConfirmText = new LocaleString("Do you want to leave this clan?");

	public ClansLeaveGroupResultInfo[] _LeaveGroupResultInfo;

	public ClansAuthorizeJoinResultInfo[] _AuthorizeJoinResultInfo;

	public ClansJoinGroupResultInfo[] _JoinGroupResultInfo;

	public ClansAssignRoleResultInfo[] _AssignRoleInfo;

	public int _JoinClanAchievementTaskID = 164;

	public int _AddMembersToClanAchievementTaskID = 181;

	public LocaleString[] _ClanTypeText;

	public LocaleString[] _UserRoleText;

	private KAWidget mTxtClanName;

	private KAWidget mTxtClanType;

	private KAWidget mTxtDescription;

	private KAWidget mTxtMemberCount;

	private KAWidget mTxtSelectDescription;

	private KAWidget mTxtTotalPoints;

	private KAWidget mBtnInvite;

	private KAWidget mBtnJoinRequests;

	private KAWidget mBtnLeaveClan;

	private KAWidget mBtnEdit;

	private KAWidget mBtnJoinClan;

	private KAWidget mAniRequestCount;

	private Group mClan;

	private ClanData mClanData;

	private KAUIGenericDB mKAUIGenericDB;

	private GroupMember mMyMemberInfo;

	private List<string> mPendingJoinRequests;

	private List<string> mPendingJoinRequestsDescription;

	private bool mMemberInfoDirty;

	private UiClansRequestDB mUiClansRequestDB;

	private UiClansInviteFriendDB mUiInviteFriendDB;

	private List<GroupMember> mGroupMembers;

	private string mNewLeaderID;

	private UiSelectFriend mUiSelectFriendDB;

	private bool mIsGetRequestCount;

	private int mJoinRequestCount;

	protected override void Start()
	{
		base.Start();
		mTxtClanName = FindItem("TxtClanName");
		mTxtClanType = FindItem("TxtClanType");
		mTxtDescription = FindItem("TxtDescription");
		mTxtMemberCount = FindItem("TxtMemberCount");
		mTxtSelectDescription = FindItem("TxtSelect");
		mAniRequestCount = FindItem("AniRequestCount");
		mTxtTotalPoints = FindItem("TxtTotalPoints");
		mBtnInvite = FindItem("BtnInvite");
		mBtnJoinRequests = FindItem("BtnJoinRequests");
		mBtnLeaveClan = FindItem("BtnLeaveClan");
		mBtnEdit = FindItem("BtnEdit");
		mBtnJoinClan = FindItem("BtnJoinClan");
	}

	private void OnDisable()
	{
		mMyMemberInfo = null;
		if (mBtnInvite != null)
		{
			mBtnInvite.SetVisibility(inVisible: false);
		}
		if (mBtnJoinRequests != null)
		{
			mBtnJoinRequests.SetVisibility(inVisible: false);
		}
		if (mBtnEdit != null)
		{
			mBtnEdit.SetVisibility(inVisible: false);
		}
		if (mBtnLeaveClan != null)
		{
			mBtnLeaveClan.SetVisibility(inVisible: false);
		}
		if (mBtnJoinClan != null)
		{
			mBtnJoinClan.SetVisibility(inVisible: false);
		}
		_Menu.ClearItems();
		_Menu.SetVisibility(inVisible: true);
		if (_UiClanMemberDB != null)
		{
			_UiClanMemberDB.gameObject.SetActive(value: false);
		}
	}

	public void Show(bool show, Group inGroup)
	{
		base.gameObject.SetActive(show);
		if (show)
		{
			SetVisibility(inVisible: false);
			mClan = inGroup;
			GetClanMembers();
			mClanData = new ClanData(mClan, FindItem("ClanCrestTemplate"));
			mClanData.Load();
		}
	}

	private void GetClanMembers()
	{
		_Menu.ClearItems();
		WsWebService.GetMembersByGroupID(mClan.GroupID.ToString(), GroupsEventHandler, null);
	}

	private void GroupsEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.GET_MEMBERS_BY_GROUP_ID:
			if (inEvent == WsServiceEvent.COMPLETE)
			{
				SetVisibility(inVisible: true);
				PopulateMenu((GroupMember[])inObject);
				DetermineAuthority();
			}
			break;
		case WsServiceType.JOIN_GROUP:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				GroupJoinResult groupJoinResult = (GroupJoinResult)inObject;
				if (groupJoinResult != null)
				{
					SetInteractive(interactive: true);
					string inOkMessage2 = ((groupJoinResult.Status == JoinGroupStatus.Success) ? "OnJoinClanSuccess" : "KillGenericDB");
					bool flag4 = false;
					ClansJoinGroupResultInfo[] joinGroupResultInfo = _JoinGroupResultInfo;
					foreach (ClansJoinGroupResultInfo clansJoinGroupResultInfo in joinGroupResultInfo)
					{
						if (clansJoinGroupResultInfo._Status == groupJoinResult.Status)
						{
							flag4 = true;
							ShowGenericDB("PfKAUIGenericDB", clansJoinGroupResultInfo._StatusText.GetLocalizedString(), null, null, null, inOkMessage2, null);
							break;
						}
					}
					if (!flag4 && groupJoinResult.Success)
					{
						OnJoinClanSuccess();
					}
					break;
				}
				goto case WsServiceEvent.ERROR;
			}
			case WsServiceEvent.ERROR:
				SetInteractive(interactive: true);
				UtDebug.Log("Error : " + inType);
				break;
			}
			break;
		case WsServiceType.LEAVE_GROUP:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				LeaveGroupResult leaveGroupResult = (LeaveGroupResult)inObject;
				if (leaveGroupResult != null)
				{
					SetInteractive(interactive: true);
					string inOkMessage = (leaveGroupResult.Success ? "OnLeaveClan" : "KillGenericDB");
					bool flag3 = false;
					ClansLeaveGroupResultInfo[] leaveGroupResultInfo = _LeaveGroupResultInfo;
					foreach (ClansLeaveGroupResultInfo clansLeaveGroupResultInfo in leaveGroupResultInfo)
					{
						if (clansLeaveGroupResultInfo._Status == leaveGroupResult.Status)
						{
							flag3 = true;
							ShowGenericDB("PfKAUIGenericDB", clansLeaveGroupResultInfo._StatusText.GetLocalizedString(), null, null, null, inOkMessage, null);
							break;
						}
					}
					if (!flag3 && leaveGroupResult.Success)
					{
						OnLeaveClan();
					}
					break;
				}
				goto case WsServiceEvent.ERROR;
			}
			case WsServiceEvent.ERROR:
				SetInteractive(interactive: true);
				break;
			}
			break;
		case WsServiceType.GET_PENDING_JOIN_REQUEST:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				GetPendingJoinResult getPendingJoinResult = (GetPendingJoinResult)inObject;
				if (mIsGetRequestCount)
				{
					if (getPendingJoinResult != null && getPendingJoinResult.Success && getPendingJoinResult.Requests != null)
					{
						SetJoinRequestCount(getPendingJoinResult.Requests.Length);
					}
					mIsGetRequestCount = false;
				}
				else if (getPendingJoinResult != null && getPendingJoinResult.Success && getPendingJoinResult.Requests != null)
				{
					SetJoinRequestCount(getPendingJoinResult.Requests.Length);
					mPendingJoinRequests = new List<string>();
					mPendingJoinRequestsDescription = new List<string>();
					PendingJoinRequest[] requests = getPendingJoinResult.Requests;
					foreach (PendingJoinRequest pendingJoinRequest in requests)
					{
						mPendingJoinRequests.Add(pendingJoinRequest.FromUserID);
						mPendingJoinRequestsDescription.Add(pendingJoinRequest.Message);
					}
					string[] array = _ApproveJoinRequestDBUrl.Split('/');
					RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], AssetEventHandler, typeof(GameObject));
				}
				else
				{
					SetInteractive(interactive: true);
					SetJoinRequestCount(0);
					ShowGenericDB("PfKAUIGenericDB", _PendingJoinRequestEmptyText.GetLocalizedString(), _PendingJoinRequestText.GetLocalizedString(), null, null, "KillGenericDB", null);
				}
				break;
			}
			case WsServiceEvent.ERROR:
				SetInteractive(interactive: true);
				break;
			}
			break;
		case WsServiceType.AUTHORIZE_JOIN_REQUEST:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				SetJoinRequestCount(mJoinRequestCount - 1);
				AuthorizeJoinResult authorizeJoinResult = (AuthorizeJoinResult)inObject;
				if (authorizeJoinResult != null)
				{
					mUiSelectFriendDB.SetInteractive(interactive: true);
					KAUICursorManager.SetDefaultCursor("Arrow");
					AuthorizeJoinRequest authorizeJoinRequest = (AuthorizeJoinRequest)inUserData;
					if (authorizeJoinResult.Success && authorizeJoinRequest.Approved)
					{
						mMemberInfoDirty = true;
					}
					bool flag2 = false;
					ClansAuthorizeJoinResultInfo[] authorizeJoinResultInfo = _AuthorizeJoinResultInfo;
					foreach (ClansAuthorizeJoinResultInfo clansAuthorizeJoinResultInfo in authorizeJoinResultInfo)
					{
						if (clansAuthorizeJoinResultInfo._Status == authorizeJoinResult.Status)
						{
							flag2 = true;
							mUiSelectFriendDB.SetVisibility(inVisible: false);
							if (authorizeJoinRequest.Approved)
							{
								AchievementTask achievementTask = new AchievementTask(authorizeJoinRequest.JoineeID, _JoinClanAchievementTaskID, authorizeJoinRequest.GroupID);
								AchievementTask achievementTask2 = new AchievementTask(mClan.GroupID, _AddMembersToClanAchievementTaskID, authorizeJoinRequest.JoineeID, 0, 0, 2);
								UserAchievementTask.Set(achievementTask, achievementTask2);
								ShowGenericDB("PfKAUIGenericDB", clansAuthorizeJoinResultInfo._ApprovedStatusText.GetLocalizedString(), null, null, null, "KillJoinRequestDB", null);
							}
							else
							{
								ShowGenericDB("PfKAUIGenericDB", clansAuthorizeJoinResultInfo._RejectedStatusText.GetLocalizedString(), null, null, null, "KillJoinRequestDB", null);
							}
							break;
						}
					}
					if (!flag2)
					{
						KillJoinRequestDB();
					}
					break;
				}
				goto case WsServiceEvent.ERROR;
			}
			case WsServiceEvent.ERROR:
				mUiSelectFriendDB.SetInteractive(interactive: true);
				KAUICursorManager.SetDefaultCursor("Arrow");
				KillJoinRequestDB();
				break;
			}
			break;
		case WsServiceType.ASSIGN_ROLE:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				AssignRoleResult assignRoleResult = (AssignRoleResult)inObject;
				if (assignRoleResult != null)
				{
					if (assignRoleResult.Success)
					{
						if ((UserRole)inUserData == UserRole.Leader)
						{
							if (!string.IsNullOrEmpty(mNewLeaderID))
							{
								Group.SetNewOwner(mClan, mNewLeaderID);
								mNewLeaderID = null;
							}
							LeaveClan();
						}
						break;
					}
					bool flag = false;
					ClansAssignRoleResultInfo[] assignRoleInfo = _AssignRoleInfo;
					foreach (ClansAssignRoleResultInfo clansAssignRoleResultInfo in assignRoleInfo)
					{
						if (clansAssignRoleResultInfo._Status == assignRoleResult.Status)
						{
							flag = true;
							ShowGenericDB("PfKAUIGenericDB", clansAssignRoleResultInfo._StatusText.GetLocalizedString(), null, null, null, "KillGenericDB", null);
							break;
						}
					}
					if (!flag)
					{
						SetInteractive(interactive: true);
					}
					break;
				}
				goto case WsServiceEvent.ERROR;
			}
			case WsServiceEvent.ERROR:
				SetInteractive(interactive: true);
				break;
			}
			break;
		}
	}

	private void AssetEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (inObject != null)
			{
				SetInteractive(interactive: true);
				GameObject gameObject = Object.Instantiate((GameObject)inObject);
				if (RsResourceManager.Compare(inURL, _SelectFriendDBUrl) || RsResourceManager.Compare(inURL, _ApproveJoinRequestDBUrl))
				{
					mUiSelectFriendDB = gameObject.GetComponent<UiSelectFriend>();
					if (!(mUiSelectFriendDB != null))
					{
						break;
					}
					if (mPendingJoinRequests != null)
					{
						mUiSelectFriendDB.SetMessage(base.gameObject, "OnAcceptJoinRequest", "OnRejectJoinRequest", null);
						mUiSelectFriendDB.ShowFriendRequest(_PendingJoinRequestText.GetLocalizedString(), mPendingJoinRequests.ToArray(), mPendingJoinRequestsDescription.ToArray());
					}
					else
					{
						List<string> list = new List<string>();
						foreach (GroupMember mGroupMember in mGroupMembers)
						{
							if (mGroupMember.UserID != UserInfo.pInstance.UserID)
							{
								list.Add(mGroupMember.UserID + "|" + mGroupMember.DisplayName);
							}
						}
						mUiSelectFriendDB.SetMessage(base.gameObject, "OnSelectNewLeaderYes", null, null);
						mUiSelectFriendDB.ShowFriendSelection(_SelectNewLeaderText.GetLocalizedString(), "", allowMultipleSelection: false, list.ToArray());
					}
					mUiSelectFriendDB._UseMask = true;
				}
				else if (RsResourceManager.Compare(inURL, _JoinRequestDBUrl))
				{
					mUiClansRequestDB = gameObject.GetComponent<UiClansRequestDB>();
					if (mUiClansRequestDB != null)
					{
						KAUI.SetExclusive(mUiClansRequestDB);
						mUiClansRequestDB.SetMessage(base.gameObject, "OnJoinClanRequestDBYes", "OnJoinClanRequestDBNo", null, null);
						mUiClansRequestDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
						mUiClansRequestDB.SetClanName(mClan.Name);
						mUiClansRequestDB.SetDefaultMessage(_JoinRequestDefaultMessageText);
					}
				}
				else if (RsResourceManager.Compare(inURL, _InviteFriendDBUrl))
				{
					mUiInviteFriendDB = gameObject.GetComponent<UiClansInviteFriendDB>();
					if (mUiInviteFriendDB != null)
					{
						mUiInviteFriendDB.SetClanInfo(mClan.GroupID, mClan.Name);
					}
				}
				break;
			}
			goto case RsResourceLoadEvent.ERROR;
		case RsResourceLoadEvent.ERROR:
			SetInteractive(interactive: true);
			Debug.Log("Error downloading : " + inURL);
			break;
		}
	}

	private void OnSelectNewLeaderYes(string inNewLeaderID)
	{
		mNewLeaderID = inNewLeaderID;
		WsWebService.AssignRole(new AssignRoleRequest
		{
			GroupID = mClan.GroupID,
			MemberID = inNewLeaderID,
			NewRole = UserRole.Leader
		}, GroupsEventHandler, UserRole.Leader);
	}

	private void OnJoinClanSuccess()
	{
		KillGenericDB();
		UiClans.pInstance._UiAchievements.Reset();
		UserProfile.pProfileData.ReplaceGroup(0, mClan.GroupID, UserRole.Member);
		AchievementTask achievementTask = new AchievementTask(_JoinClanAchievementTaskID, mClan.GroupID);
		AchievementTask achievementTask2 = new AchievementTask(mClan.GroupID, _AddMembersToClanAchievementTaskID, UserInfo.pInstance.UserID, 0, 0, 2);
		UserAchievementTask.Set(achievementTask, achievementTask2);
		Group.UpdateMemberCount(mClan, 1);
		UiClans.pInstance.SetClan(mClan);
		AvatarData.SetGroupName(mClan);
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		component?.UpdateDisplayName(component.pPlayerMounted, UserProfile.pProfileData.HasGroup());
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.CheckForTaskCompletion("Action", "JoinClan");
		}
	}

	private void OnAcceptJoinRequest(string userID)
	{
		mUiSelectFriendDB.SetInteractive(interactive: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		AuthorizeJoinRequest authorizeJoinRequest = new AuthorizeJoinRequest();
		authorizeJoinRequest.GroupID = mClan.GroupID;
		authorizeJoinRequest.JoineeID = userID;
		authorizeJoinRequest.Approved = true;
		authorizeJoinRequest.UserID = UserInfo.pInstance.UserID;
		WsWebService.AuthorizeJoinRequest(authorizeJoinRequest, GroupsEventHandler, authorizeJoinRequest);
	}

	private void OnRejectJoinRequest(string userID)
	{
		mUiSelectFriendDB.SetInteractive(interactive: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		AuthorizeJoinRequest authorizeJoinRequest = new AuthorizeJoinRequest();
		authorizeJoinRequest.GroupID = mClan.GroupID;
		authorizeJoinRequest.JoineeID = userID;
		authorizeJoinRequest.Approved = false;
		authorizeJoinRequest.UserID = UserInfo.pInstance.UserID;
		WsWebService.AuthorizeJoinRequest(authorizeJoinRequest, GroupsEventHandler, authorizeJoinRequest);
	}

	public override void SetInteractive(bool interactive)
	{
		base.SetInteractive(interactive);
		UiClans.pInstance.SetInteractive(interactive);
		KAUICursorManager.SetDefaultCursor(interactive ? "Arrow" : "Loading");
	}

	private void OnLeaveClan()
	{
		KillGenericDB();
		UiClans.pInstance._UiAchievements.Reset();
		AvatarData.SetGroupName(null);
		UserProfile.pProfileData.RemoveGroup(UiClans.pClan.GroupID);
		UiClans.pInstance.SetClan(null);
		Group.UpdateMemberCount(mClan, -1);
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		component?.UpdateDisplayName(component.pPlayerMounted);
	}

	private void OnJoinGroupDone()
	{
		KAUI.RemoveExclusive(mKAUIGenericDB);
		Object.Destroy(mKAUIGenericDB.gameObject);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnInvite)
		{
			_UiClanMemberDB.gameObject.SetActive(value: false);
			SetInteractive(interactive: false);
			string[] array = _InviteFriendDBUrl.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], AssetEventHandler, typeof(GameObject));
		}
		else if (inWidget == mBtnJoinRequests)
		{
			_UiClanMemberDB.gameObject.SetActive(value: false);
			SetInteractive(interactive: false);
			WsWebService.GetPendingJoinRequest(new GetPendingJoinRequest
			{
				GroupID = mClan.GroupID
			}, GroupsEventHandler, null);
		}
		else if (inWidget == mBtnLeaveClan)
		{
			_UiClanMemberDB.gameObject.SetActive(value: false);
			ShowGenericDB("PfKAUIGenericDB", _LeaveClanConfirmText.GetLocalizedString(), "", "OnLeaveClanYes", "KillGenericDB", null, null);
		}
		else if (inWidget == mBtnEdit)
		{
			_UiClanMemberDB.gameObject.SetActive(value: false);
			SetVisibility(inVisible: false);
			_Menu.SetVisibility(inVisible: false);
			_UiClansCreate.EditClan(mClan, mClanData._Texture);
			UiClans.pInstance.ShowBackBtn(isVisible: true, base.gameObject);
		}
		else if (inWidget == mBtnJoinClan)
		{
			_UiClanMemberDB.gameObject.SetActive(value: false);
			if (UserProfile.pProfileData.HasGroup())
			{
				ShowGenericDB("PfKAUIGenericDB", _JoinAnotherClanText.GetLocalizedString(), null, "OnJoinAnotherClanYes", "KillGenericDB", null, null);
			}
			else
			{
				OnJoinAnotherClanYes();
			}
		}
	}

	private void OnLeaveClanYes()
	{
		KillGenericDB();
		SetInteractive(interactive: false);
		if (mMyMemberInfo != null && mMyMemberInfo.RoleID == 3 && mGroupMembers.Count > 1)
		{
			mPendingJoinRequests = null;
			string[] array = _SelectFriendDBUrl.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], AssetEventHandler, typeof(GameObject));
		}
		else
		{
			LeaveClan();
		}
	}

	private void LeaveClan()
	{
		WsWebService.LeaveGroup(new LeaveGroupRequest
		{
			GroupID = mClan.GroupID,
			UserID = UserInfo.pInstance.UserID
		}, GroupsEventHandler, null);
	}

	private void OnBackBtnClicked()
	{
		mTxtClanName.SetText(mClan.Name.ToString());
		int type = (int)mClan.Type;
		if (_ClanTypeText != null && _ClanTypeText.Length > type)
		{
			mTxtClanType.SetText(_ClanTypeText[type].GetLocalizedString());
		}
		mTxtDescription.SetText(mClan.Description.ToString());
		mClanData = new ClanData(mClan, FindItem("ClanCrestTemplate"));
		mClanData.Load();
		_UiClansCreate.gameObject.SetActive(value: false);
		SetVisibility(inVisible: true);
		_Menu.SetVisibility(inVisible: true);
	}

	private void OnJoinAnotherClanYes()
	{
		KillGenericDB();
		if (mClan.Type == GroupType.InviteOnly)
		{
			SetInteractive(interactive: false);
			string[] array = _JoinRequestDBUrl.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], AssetEventHandler, typeof(GameObject));
		}
		else
		{
			OnJoinClanRequestDBYes();
		}
	}

	private void OnJoinClanRequestDBYes()
	{
		string message = null;
		if (mUiClansRequestDB != null)
		{
			message = mUiClansRequestDB.GetMessage();
			KAUI.RemoveExclusive(mUiClansRequestDB);
			Object.Destroy(mUiClansRequestDB.gameObject);
		}
		SetInteractive(interactive: false);
		WsWebService.JoinGroup(new JoinGroupRequest
		{
			GroupID = mClan.GroupID,
			Message = message
		}, GroupsEventHandler, null);
	}

	private void OnJoinClanRequestDBNo()
	{
		if (mUiClansRequestDB != null)
		{
			KAUI.RemoveExclusive(mUiClansRequestDB);
			Object.Destroy(mUiClansRequestDB.gameObject);
		}
		SetInteractive(interactive: true);
	}

	public void OnClanMemberSelected(KAWidget inWidget)
	{
		ClanMemberData clanMemberData = (ClanMemberData)inWidget.GetUserData();
		if (clanMemberData != null)
		{
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage("OnClanMemberSelected", clanMemberData, SendMessageOptions.DontRequireReceiver);
				return;
			}
			_UiClanMemberDB.Show(mClan, mMyMemberInfo, clanMemberData._Member);
			_UiClanMemberDB.transform.position = new Vector3(inWidget.transform.position.x, inWidget.transform.position.y, _UiClanMemberDB.transform.position.z);
		}
	}

	private void ShowGenericDB(string inGenericDBName, string inString, string inTitle, string inYesMessage, string inNoMessage, string inOkMessage, string inCloseMessage)
	{
		mKAUIGenericDB = GameUtilities.DisplayGenericDB(inGenericDBName, inString, inTitle, base.gameObject, inYesMessage, inNoMessage, inOkMessage, inCloseMessage);
	}

	private void KillJoinRequestDB()
	{
		if (mUiSelectFriendDB._FriendRequestMenu.GetNumItems() == 0)
		{
			if (mUiSelectFriendDB._UseMask)
			{
				KAUI.RemoveExclusive(mUiSelectFriendDB);
			}
			Object.Destroy(mUiSelectFriendDB.gameObject);
		}
		else
		{
			mUiSelectFriendDB.SetVisibility(inVisible: true);
		}
		KillGenericDB();
	}

	private void KillGenericDB()
	{
		if (mKAUIGenericDB != null)
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mMemberInfoDirty && (KAUI._GlobalExclusiveUI == null || KAUI._GlobalExclusiveUI == UiClans.pInstance))
		{
			mMemberInfoDirty = false;
			GetClanMembers();
		}
	}

	public void OnRoleModified(GroupMember inMemberData)
	{
		KAWidget selectedItem = _Menu.GetSelectedItem();
		if (selectedItem != null)
		{
			ClanMemberData clanMemberData = (ClanMemberData)selectedItem.GetUserData();
			if (clanMemberData != null && clanMemberData._Member == inMemberData)
			{
				int num = (inMemberData.RoleID.HasValue ? inMemberData.RoleID.Value : 0);
				if (_UserRoleText != null && _UserRoleText.Length > num)
				{
					clanMemberData._Item.FindChildItem("TxtMemberType").SetText(_UserRoleText[num].GetLocalizedString());
				}
			}
			else
			{
				foreach (KAWidget item in _Menu.GetItems())
				{
					clanMemberData = (ClanMemberData)item.GetUserData();
					if (clanMemberData != null && clanMemberData._Member == inMemberData)
					{
						int num2 = (inMemberData.RoleID.HasValue ? inMemberData.RoleID.Value : 0);
						if (_UserRoleText != null && _UserRoleText.Length > num2)
						{
							clanMemberData._Item.FindChildItem("TxtMemberType").SetText(_UserRoleText[num2].GetLocalizedString());
						}
					}
					if (clanMemberData != null && clanMemberData._Member == inMemberData)
					{
						clanMemberData._Item.FindChildItem("TxtMemberType").SetText(inMemberData.RoleID.HasValue ? ((UserRole)inMemberData.RoleID.Value).ToString() : "");
					}
				}
			}
		}
		if (inMemberData == mMyMemberInfo)
		{
			DetermineAuthority();
		}
	}

	public void OnMemberRemoved(GroupMember inMemberData)
	{
		KAWidget selectedItem = _Menu.GetSelectedItem();
		if (selectedItem != null)
		{
			_Menu.RemoveWidget(selectedItem);
		}
		mGroupMembers.Remove(inMemberData);
		mClan.TotalMemberCount--;
		Group.AddGroup(mClan);
		UpdatePlayerRanks();
		UpdateClanMemberCount();
	}

	public void UpdateClanMemberCount()
	{
		mTxtSelectDescription.SetVisibility(mMyMemberInfo == null || mClan.TotalMemberCount > 1);
		if (mClan.TotalMemberCount.HasValue)
		{
			mTxtMemberCount.SetText(mClan.TotalMemberCount.Value + (mClan.MemberLimit.HasValue ? ("/" + mClan.MemberLimit.Value) : ""));
		}
		if (mBtnInvite != null && mClan.TotalMemberCount.HasValue && mClan.MemberLimit.HasValue)
		{
			mBtnInvite.SetDisabled(mClan.TotalMemberCount.Value == mClan.MemberLimit.Value);
		}
	}

	public void UpdatePlayerRanks()
	{
		for (int i = 0; i < _Menu.GetItemCount(); i++)
		{
			_Menu.GetItemAt(i).FindChildItem("TxtMemberRank").SetText(i + 1 + ".");
		}
	}

	private void PopulateMenu(GroupMember[] inMembers)
	{
		int value = 0;
		mGroupMembers = new List<GroupMember>(inMembers);
		if (mGroupMembers != null)
		{
			value = mGroupMembers.Count;
			if (UserProfile.pProfileData.InGroup(mClan.GroupID))
			{
				List<GroupMember> list = new List<GroupMember>(mGroupMembers);
				UserAchievementInfo userAchievementInfoByType = UserRankData.GetUserAchievementInfoByType(11);
				if (userAchievementInfoByType != null && userAchievementInfoByType.AchievementPointTotal.HasValue)
				{
					int value2 = userAchievementInfoByType.AchievementPointTotal.Value;
					int num = -1;
					int num2 = -1;
					for (int i = 0; i < list.Count; i++)
					{
						GroupMember groupMember = list[i];
						if (groupMember.UserID.ToString() == UserInfo.pInstance.UserID)
						{
							if (!groupMember.Rank.HasValue)
							{
								num = i;
								groupMember.Rank = ((num2 != -1) ? (num2 + 1) : (num + 1));
								groupMember.RankTrend = 0;
							}
							break;
						}
						if (groupMember.Points < value2)
						{
							if (num2 == -1)
							{
								num2 = i;
							}
							groupMember.Rank++;
							groupMember.RankTrend--;
						}
					}
					if (num != -1)
					{
						if (num2 != -1)
						{
							GroupMember item = list[num];
							list.RemoveAt(num);
							list.Insert(num2, item);
						}
						mGroupMembers = list;
					}
				}
			}
			foreach (GroupMember mGroupMember in mGroupMembers)
			{
				KAWidget kAWidget = DuplicateWidget(_Menu._Template);
				kAWidget.SetVisibility(inVisible: true);
				_Menu.AddWidget(kAWidget);
				kAWidget.FindChildItem("TxtMemberName").SetText(mGroupMember.DisplayName);
				kAWidget.FindChildItem("TxtMemberRank").SetText(mGroupMember.Rank.HasValue ? mGroupMember.Rank.Value.ToString() : "--");
				kAWidget.FindChildItem("AniMemberTrophies").SetText(mGroupMember.Points.HasValue ? mGroupMember.Points.Value.ToString() : "0");
				((KAToggleButton)kAWidget.FindChildItem("IcoOnlineStatus")).SetChecked(mGroupMember.Online);
				int num3 = (mGroupMember.RoleID.HasValue ? mGroupMember.RoleID.Value : 0);
				if (_UserRoleText != null && _UserRoleText.Length > num3)
				{
					kAWidget.FindChildItem("TxtMemberType").SetText(_UserRoleText[num3].GetLocalizedString());
				}
				if (mGroupMember.RankTrend.HasValue)
				{
					int value3 = mGroupMember.RankTrend.Value;
					if (value3 < 0)
					{
						KAWidget kAWidget2 = kAWidget.FindChildItem("AniClanRankDown");
						kAWidget2.SetVisibility(inVisible: true);
						kAWidget2.SetText(Mathf.Abs(value3).ToString());
					}
					else if (value3 > 0)
					{
						KAWidget kAWidget3 = kAWidget.FindChildItem("AniClanRankUp");
						kAWidget3.SetVisibility(inVisible: true);
						kAWidget3.SetText(value3.ToString());
					}
					else
					{
						kAWidget.FindChildItem("AniClanRankEqual").SetVisibility(inVisible: true);
					}
				}
				else
				{
					kAWidget.FindChildItem("AniClanRankEqual").SetVisibility(inVisible: true);
				}
				ClanMemberData userData = new ClanMemberData(mGroupMember);
				kAWidget.SetUserData(userData);
				if (mGroupMember.UserID.ToString() == UserInfo.pInstance.UserID)
				{
					mMyMemberInfo = mGroupMember;
					kAWidget.FindChildItem("AniPlayerHighlight").SetVisibility(inVisible: true);
				}
			}
			if (mTxtSelectDescription != null)
			{
				mTxtSelectDescription.SetVisibility(mMyMemberInfo == null || mGroupMembers.Count > 1);
			}
		}
		if (mMyMemberInfo != null && !UserProfile.pProfileData.InGroup(mMyMemberInfo.GroupID))
		{
			UserProfile.pProfileData.ReplaceGroup(0, mMyMemberInfo.GroupID, (!mMyMemberInfo.RoleID.HasValue) ? UserRole.Member : ((UserRole)mMyMemberInfo.RoleID.Value));
			UiClans.pInstance.SetClan(mClan, switchTabs: false);
			Group.UpdateMemberCount(mClan, 1);
		}
		mTxtClanName.SetText(mClan.Name.ToString());
		int type = (int)mClan.Type;
		if (_ClanTypeText != null && _ClanTypeText.Length > type)
		{
			mTxtClanType.SetText(_ClanTypeText[type].GetLocalizedString());
		}
		mTxtDescription.SetText(mClan.Description.ToString());
		mTxtTotalPoints.SetText(mClan.Points.HasValue ? mClan.Points.Value.ToString() : "0");
		if (mClan.TotalMemberCount.HasValue)
		{
			mClan.TotalMemberCount = value;
		}
		mTxtMemberCount.SetText(value + (mClan.MemberLimit.HasValue ? ("/" + mClan.MemberLimit.Value) : ""));
		if (mBtnInvite != null && mClan.TotalMemberCount.HasValue && mClan.MemberLimit.HasValue)
		{
			mBtnInvite.SetDisabled(mClan.TotalMemberCount.Value == mClan.MemberLimit.Value);
		}
		if (_MessageObject != null)
		{
			_MessageObject.SendMessage("OnMembersDetailsReady", mGroupMembers.ToArray(), SendMessageOptions.DontRequireReceiver);
		}
	}

	public void DetermineAuthority()
	{
		bool flag = mMyMemberInfo != null;
		if (mBtnLeaveClan != null)
		{
			mBtnLeaveClan.SetVisibility(flag);
		}
		if (mBtnJoinClan != null)
		{
			mBtnJoinClan.SetVisibility(!flag);
		}
		if (mBtnJoinRequests != null)
		{
			mBtnJoinRequests.SetVisibility(inVisible: false);
		}
		bool flag2 = mClan.MemberLimit.HasValue && mClan.TotalMemberCount.HasValue && mClan.TotalMemberCount.Value >= mClan.MemberLimit.Value;
		if (mBtnJoinClan != null)
		{
			mBtnJoinClan.SetDisabled(flag2 || mClan.Type == GroupType.Closed);
		}
		if (flag)
		{
			if (mBtnInvite != null)
			{
				mBtnInvite.SetVisibility(mMyMemberInfo.RoleID.HasValue && mClan.HasPermission((UserRole)mMyMemberInfo.RoleID.Value, "Invite") && !flag2);
			}
			if (mBtnJoinRequests != null)
			{
				mBtnJoinRequests.SetVisibility(mMyMemberInfo.RoleID.HasValue && mClan.HasPermission((UserRole)mMyMemberInfo.RoleID.Value, "Approve Join Request"));
			}
			if (mBtnEdit != null)
			{
				mBtnEdit.SetVisibility(mMyMemberInfo.RoleID.HasValue && mClan.HasPermission((UserRole)mMyMemberInfo.RoleID.Value, "Edit Group"));
			}
			if (mBtnJoinRequests != null && mBtnJoinRequests.GetVisibility())
			{
				mIsGetRequestCount = true;
				WsWebService.GetPendingJoinRequest(new GetPendingJoinRequest
				{
					GroupID = mClan.GroupID
				}, GroupsEventHandler, null);
			}
			else if (mAniRequestCount != null)
			{
				mAniRequestCount.SetVisibility(inVisible: false);
			}
		}
		else if (mAniRequestCount != null)
		{
			mAniRequestCount.SetVisibility(inVisible: false);
		}
	}

	private void SetJoinRequestCount(int inCount)
	{
		mJoinRequestCount = inCount;
		if (mAniRequestCount != null)
		{
			mAniRequestCount.SetText(mJoinRequestCount.ToString());
		}
		mAniRequestCount.SetVisibility(mJoinRequestCount > 0);
	}
}
