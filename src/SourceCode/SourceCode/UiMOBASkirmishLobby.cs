using System.Collections.Generic;
using UnityEngine;

public class UiMOBASkirmishLobby : KAUI
{
	public GameObject _ChatWindow;

	public UiMOBAMyTeam _MyTeamUI;

	private GroupMember[] mClanMembers;

	private bool mClanDataLoaded;

	private UiMOBAPlayerListMenu mPlayersMenu;

	private UiMOBAChallengeMenu mTeamsMenu;

	private UiMOBAInviteMenu mInvitesMenu;

	private List<MMOAvatar> mPlayersToBeAdded = new List<MMOAvatar>();

	private List<string> mTeamsToBeAdded = new List<string>();

	private List<string> mChallengesToBeAdded = new List<string>();

	private KAWidget mClanLobbyBtn;

	protected override void Start()
	{
		base.Start();
		mPlayersMenu = (UiMOBAPlayerListMenu)GetMenu("UiMOBAPlayerListMenu");
		mTeamsMenu = (UiMOBAChallengeMenu)GetMenu("UiMOBAChallengeMenu");
		mInvitesMenu = (UiMOBAInviteMenu)GetMenu("UiMOBAInviteMenu");
		mClanLobbyBtn = FindItem("BtnClanLobby");
	}

	protected override void Update()
	{
		if (mPlayersToBeAdded.Count > 0)
		{
			MMOAvatar mMOAvatar = mPlayersToBeAdded[0];
			mPlayersToBeAdded.RemoveAt(0);
			mPlayersMenu.AddPlayerToList(mMOAvatar.pUserID, mMOAvatar.pAvatarData.mInstance.DisplayName);
		}
		if (mTeamsToBeAdded.Count > 0)
		{
			string leaderId = mTeamsToBeAdded[0];
			mTeamsToBeAdded.RemoveAt(0);
			mTeamsMenu.AddTeamToList(leaderId);
		}
		if (mChallengesToBeAdded.Count > 0)
		{
			string leaderId2 = mChallengesToBeAdded[0];
			mChallengesToBeAdded.RemoveAt(0);
			mInvitesMenu.AddChallengeInvite(leaderId2);
		}
	}

	public void Init()
	{
		SetVisibility(inVisible: true);
		SetState(KAUIState.INTERACTIVE);
		mClanLobbyBtn.SetDisabled(!UserProfile.pProfileData.HasGroup());
		UiChatHistory._IsVisible = true;
		if (_ChatWindow != null)
		{
			_ChatWindow.SetActive(value: true);
		}
		string groupID = UserProfile.pProfileData.GetGroupID();
		if (!string.IsNullOrEmpty(groupID))
		{
			WsWebService.GetMembersByGroupID(groupID, GroupsEventHandler, null);
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		_MyTeamUI.SetVisibility(inVisible);
		if (_ChatWindow != null)
		{
			_ChatWindow.SetActive(inVisible);
		}
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.pNotifyObjOnIdlePopUpClose = (inVisible ? base.gameObject : null);
		}
	}

	private void OnCloseIdlePopUp()
	{
		if (mPlayersMenu != null)
		{
			mPlayersMenu.ClearItems();
		}
	}

	private void GroupsEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType == WsServiceType.GET_MEMBERS_BY_GROUP_ID && inEvent == WsServiceEvent.COMPLETE)
		{
			mClanDataLoaded = true;
			mClanMembers = (GroupMember[])inObject;
		}
	}

	public void UpdateMyTeam(MOBATeam team)
	{
		_MyTeamUI.pTeam = team;
	}

	public void AddPlayer(MMOAvatar player)
	{
		mPlayersToBeAdded.Add(player);
	}

	public void RemovePlayer(MMOAvatar player)
	{
		mPlayersMenu.RemovePlayerFromList(player.pUserID);
	}

	public void AddTeam(string leaderId)
	{
		mTeamsToBeAdded.Add(leaderId);
	}

	public void RemoveTeam(string leaderId)
	{
		mTeamsMenu.RemoveTeamFromList(leaderId);
	}

	public void AddChallengeInvite(string leaderId)
	{
		mChallengesToBeAdded.Add(leaderId);
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (inWidget.name == "BtnClanLobby")
		{
			MOBALobbyManager.pInstance.ShowLobby(MOBALobbyType.CLAN_LOBBY);
		}
		else if (inWidget.name == "BtnQuickMatch")
		{
			MOBALobbyManager.pInstance.AddToQuickMatch(isClan: false);
		}
	}
}
