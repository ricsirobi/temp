using System;
using System.Collections.Generic;
using UnityEngine;

public class UiClansMemberDB : KAUI
{
	[Serializable]
	public class ButtonPositions
	{
		public int _ButtonCount;

		public Vector2[] _Positions;
	}

	public UiClansDetails _UiClansDetails;

	public ButtonPositions[] _ButtonPositions;

	public Vector2[][] _Position;

	public ClansAssignRoleResultInfo[] _AssignRoleResultInfo;

	public ClansRemoveMemberResultInfo[] _RemoveMemberResultInfo;

	public LocaleString _VisitErrorText = new LocaleString("Cannot visit at this time.");

	public LocaleString _PromoteToLeaderText = new LocaleString("Are you sure you want to give up leadership and become just a member?");

	public LocaleString _RemoveMemberConfirmText = new LocaleString("Are you sure you want to remove this member from your clan?");

	private KAWidget mVisitBtn;

	private KAWidget mPromoteToLeaderBtn;

	private KAWidget mPromoteToElderBtn;

	private KAWidget mDemoteToMemberBtn;

	private KAWidget mRemoveBtn;

	private KAWidget mBkg01;

	private KAWidget mBkg02;

	private KAWidget mBkg03;

	private Group mClan;

	private GroupMember mMemberData;

	private GroupMember mMyMemberData;

	private KAUIGenericDB mKAUIGenericDB;

	private bool mIsJoinBuddy;

	private void Init()
	{
		base.gameObject.SetActive(value: true);
		mVisitBtn = FindItem("VisitBtn");
		mPromoteToLeaderBtn = FindItem("PromoteToLeaderBtn");
		mPromoteToElderBtn = FindItem("PromoteToElderBtn");
		mDemoteToMemberBtn = FindItem("DemoteBtn");
		mRemoveBtn = FindItem("RemoveBtn");
		mBkg01 = FindItem("BkgMyClan1");
		mBkg02 = FindItem("BkgMyClan2");
		mBkg03 = FindItem("BkgMyClan4");
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "CloseBtn")
		{
			base.gameObject.SetActive(value: false);
		}
		else if (inWidget == mVisitBtn)
		{
			SetInteractive(interactive: false);
			mIsJoinBuddy = true;
		}
		else if (inWidget == mDemoteToMemberBtn)
		{
			AssignRole(UserRole.Member);
		}
		else if (inWidget == mPromoteToElderBtn)
		{
			AssignRole(UserRole.Elder);
		}
		else if (inWidget == mPromoteToLeaderBtn)
		{
			ShowGenericDB("PfKAUIGenericDB", _PromoteToLeaderText.GetLocalizedString(), null, "OnPromoteLeaderYes", "KillGenericDB", null, "");
		}
		else if (inWidget == mRemoveBtn)
		{
			ShowGenericDB("PfKAUIGenericDB", _RemoveMemberConfirmText.GetLocalizedString(), null, "OnRemoveMemberYes", "KillGenericDB", null, "");
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mIsJoinBuddy && BuddyList.pIsReady)
		{
			mIsJoinBuddy = false;
			BuddyList.pInstance.JoinBuddy(mMemberData.UserID.ToString(), BuddyListEventHandler);
		}
	}

	private void OnPromoteLeaderYes()
	{
		KillGenericDB();
		AssignRole(UserRole.Leader);
	}

	private void OnRemoveMemberYes()
	{
		KillGenericDB();
		SetInteractive(interactive: false);
		WsWebService.RemoveMember(new RemoveMemberRequest
		{
			GroupID = mMemberData.GroupID,
			RemoveUserID = mMemberData.UserID
		}, GroupsEventHandler, null);
	}

	private void AssignRole(UserRole newRole)
	{
		SetInteractive(interactive: false);
		WsWebService.AssignRole(new AssignRoleRequest
		{
			GroupID = mMemberData.GroupID,
			MemberID = mMemberData.UserID,
			NewRole = newRole
		}, GroupsEventHandler, newRole);
	}

	private void GroupsEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.ASSIGN_ROLE:
			switch (inEvent)
			{
			default:
				return;
			case WsServiceEvent.COMPLETE:
			{
				AssignRoleResult assignRoleResult = (AssignRoleResult)inObject;
				if (assignRoleResult == null)
				{
					break;
				}
				SetInteractive(interactive: true);
				if (assignRoleResult.Success)
				{
					UserRole value = (UserRole)inUserData;
					mMemberData.RoleID = (int)value;
					_UiClansDetails.OnRoleModified(mMemberData);
					if (assignRoleResult.InitiatorNewRole.HasValue)
					{
						mMyMemberData.RoleID = (int)assignRoleResult.InitiatorNewRole.Value;
						UserProfile.pProfileData.Groups[0].RoleID = mMyMemberData.RoleID;
						_UiClansDetails.OnRoleModified(mMyMemberData);
					}
					Show(mClan, mMyMemberData, mMemberData);
				}
				ClansAssignRoleResultInfo[] assignRoleResultInfo = _AssignRoleResultInfo;
				foreach (ClansAssignRoleResultInfo clansAssignRoleResultInfo in assignRoleResultInfo)
				{
					if (clansAssignRoleResultInfo._Status == assignRoleResult.Status)
					{
						ShowGenericDB("PfKAUIGenericDB", clansAssignRoleResultInfo._StatusText.GetLocalizedString(), null, null, null, "KillGenericDB", "");
						break;
					}
				}
				return;
			}
			case WsServiceEvent.ERROR:
				break;
			}
			SetInteractive(interactive: true);
			break;
		case WsServiceType.REMOVE_MEMBER:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				RemoveMemberResult removeMemberResult = (RemoveMemberResult)inObject;
				if (removeMemberResult != null)
				{
					SetInteractive(interactive: true);
					string inOkMessage = (removeMemberResult.Success ? "OnRemoveMember" : "KillGenericDB");
					bool flag = false;
					ClansRemoveMemberResultInfo[] removeMemberResultInfo = _RemoveMemberResultInfo;
					foreach (ClansRemoveMemberResultInfo clansRemoveMemberResultInfo in removeMemberResultInfo)
					{
						if (clansRemoveMemberResultInfo._Status == removeMemberResult.Status)
						{
							flag = true;
							ShowGenericDB("PfKAUIGenericDB", clansRemoveMemberResultInfo._StatusText.GetLocalizedString(), null, null, null, inOkMessage, "");
							break;
						}
					}
					if (!flag && removeMemberResult.Success)
					{
						OnRemoveMember();
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

	private void OnRemoveMember()
	{
		KillGenericDB();
		_UiClansDetails.OnMemberRemoved(mMemberData);
		base.gameObject.SetActive(value: false);
	}

	public virtual void BuddyListEventHandler(WsServiceType inType, object inResult)
	{
		SetInteractive(interactive: true);
		if (inResult == null || (JoinBuddyResultType)inResult == JoinBuddyResultType.JoinFailedCommon)
		{
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _VisitErrorText.GetLocalizedString(), null, null, "");
		}
		else if ((JoinBuddyResultType)inResult == JoinBuddyResultType.JoinSuccess)
		{
			UiClans.pInstance.Exit(isVistingMember: true);
		}
	}

	public void Show(Group inGroup, GroupMember inMyData, GroupMember inMemberData)
	{
		if (inMyData == null && inMemberData != null && inMemberData.RoleID.HasValue)
		{
			Init();
			mMemberData = inMemberData;
			List<KAWidget> list = new List<KAWidget>();
			list.Add(mVisitBtn);
			mVisitBtn.SetDisabled(!mMemberData.Online);
			mPromoteToLeaderBtn.SetVisibility(inVisible: false);
			mDemoteToMemberBtn.SetVisibility(inVisible: false);
			mPromoteToElderBtn.SetVisibility(inVisible: false);
			mRemoveBtn.SetVisibility(inVisible: false);
			mBkg01.SetVisibility(list.Count == 1);
			mBkg02.SetVisibility(list.Count == 2);
			mBkg03.SetVisibility(list.Count > 2);
			SetButtonPositions(list);
		}
		else if (inMemberData != null && inMyData != inMemberData && inMemberData.RoleID.HasValue)
		{
			Init();
			mClan = inGroup;
			mMyMemberData = inMyData;
			mMemberData = inMemberData;
			UserRole value = (UserRole)inMyData.RoleID.Value;
			int value2 = inMemberData.RoleID.Value;
			List<KAWidget> list2 = new List<KAWidget> { mVisitBtn };
			mVisitBtn.SetDisabled(!mMemberData.Online);
			if (value2 == 2 && inGroup.HasPermission(value, "Assign Leader"))
			{
				list2.Add(mPromoteToLeaderBtn);
			}
			else
			{
				mPromoteToLeaderBtn.SetVisibility(inVisible: false);
			}
			if (value2 == 2 && inGroup.HasPermission(value, "Demote Elder"))
			{
				list2.Add(mDemoteToMemberBtn);
			}
			else
			{
				mDemoteToMemberBtn.SetVisibility(inVisible: false);
			}
			if (value2 == 1 && inGroup.HasPermission(value, "Assign Elder"))
			{
				list2.Add(mPromoteToElderBtn);
			}
			else
			{
				mPromoteToElderBtn.SetVisibility(inVisible: false);
			}
			if (inGroup.HasPermission(value, "Remove Member"))
			{
				list2.Add(mRemoveBtn);
			}
			else
			{
				mRemoveBtn.SetVisibility(inVisible: false);
			}
			mBkg01.SetVisibility(list2.Count == 1);
			mBkg02.SetVisibility(list2.Count == 2);
			mBkg03.SetVisibility(list2.Count > 2);
			SetButtonPositions(list2);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void SetButtonPositions(List<KAWidget> inButtons)
	{
		ButtonPositions[] buttonPositions = _ButtonPositions;
		foreach (ButtonPositions buttonPositions2 in buttonPositions)
		{
			if (buttonPositions2._ButtonCount == inButtons.Count)
			{
				for (int j = 0; j < inButtons.Count; j++)
				{
					inButtons[j].SetVisibility(inVisible: true);
					inButtons[j].SetPosition(buttonPositions2._Positions[j].x, buttonPositions2._Positions[j].y);
				}
				break;
			}
		}
	}

	public override void SetInteractive(bool interactive)
	{
		base.SetInteractive(interactive);
		UiClans.pInstance.SetInteractive(interactive);
		_UiClansDetails.SetInteractive(interactive);
		KAUICursorManager.SetDefaultCursor(interactive ? "Arrow" : "Loading");
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
}
