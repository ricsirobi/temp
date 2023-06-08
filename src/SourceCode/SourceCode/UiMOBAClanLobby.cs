using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UiMOBAClanLobby : KAUI
{
	public KAUIMenu mLeaderboardMenu;

	public KAUIMenu mTopLeaderboardMenu;

	public GameObject _ChatWindow;

	private static UiMOBAClanLobby mInstance;

	private static string mUserID;

	private static Group mClan;

	private ClanOrderBY mOrderBy = ClanOrderBY.POINTS;

	public UiMOBAMyTeam _MyTeamUI;

	private GroupMember[] mClanMembers;

	private bool mClanDataLoaded;

	private UiMOBAPlayerListMenu mPlayersMenu;

	private UiMOBAChallengeMenu mTeamsMenu;

	private UiMOBAInviteMenu mInvitesMenu;

	private List<MMOAvatar> mPlayersToBeAdded = new List<MMOAvatar>();

	private List<string> mTeamsToBeAdded = new List<string>();

	private List<string> mChallengesToBeAdded = new List<string>();

	public static UiMOBAClanLobby pInstance => mInstance;

	public static string pUserID => mUserID;

	protected override void Start()
	{
		base.Start();
		mInstance = this;
		mPlayersMenu = (UiMOBAPlayerListMenu)GetMenu("UiMOBAPlayerListMenu");
		mTeamsMenu = (UiMOBAChallengeMenu)GetMenu("UiMOBAChallengeMenu");
		mInvitesMenu = (UiMOBAInviteMenu)GetMenu("UiMOBAInviteMenu");
		if (string.IsNullOrEmpty(mUserID))
		{
			mUserID = UserInfo.pInstance.UserID;
		}
		if (mClan == null)
		{
			Group.Reset();
			Group.Init(includeMemberCount: true);
		}
	}

	protected override void OnDestroy()
	{
		mClan = null;
		base.OnDestroy();
	}

	protected override void Update()
	{
		base.Update();
		if (Group.pIsReady && mClan == null)
		{
			mClan = Group.GetGroup(UserProfile.pProfileData.GetGroupID());
			StartCoroutine(PopulateMenu(Group.pGroups));
		}
		if (mPlayersToBeAdded.Count > 0 && mClanDataLoaded)
		{
			MMOAvatar mMOAvatar = mPlayersToBeAdded[0];
			mPlayersToBeAdded.RemoveAt(0);
			string text = mMOAvatar.mUserID;
			for (int i = 0; i < mClanMembers.Length; i++)
			{
				if (text.CompareTo(mClanMembers[i].UserID) == 0)
				{
					mPlayersMenu.AddPlayerToList(mMOAvatar.pUserID, mMOAvatar.pAvatarData.mInstance.DisplayName);
				}
			}
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

	private IEnumerator PopulateMenu(List<Group> inGroups)
	{
		inGroups = ((mOrderBy == ClanOrderBY.MEMBER_COUNT) ? inGroups.OrderByDescending((Group o) => o.TotalMemberCount).ToList() : ((mOrderBy != ClanOrderBY.POINTS) ? inGroups.OrderBy((Group o) => o.Name).ToList() : inGroups.OrderByDescending((Group o) => o.Points).ToList()));
		foreach (Group g in inGroups)
		{
			yield return 0;
			if (g.TotalMemberCount.HasValue && g.TotalMemberCount.Value != 0)
			{
				ClanData clanData = new ClanData(g);
				mLeaderboardMenu._OptimizedMenu.AddUserData(null, clanData);
				if (clanData._Group.Rank < 4)
				{
					mTopLeaderboardMenu._OptimizedMenu.AddUserData(null, clanData);
				}
			}
		}
	}

	public void Init()
	{
		SetVisibility(inVisible: true);
		base.gameObject.SetActive(value: true);
		SetState(KAUIState.INTERACTIVE);
		UiChatHistory._IsVisible = true;
		if (_ChatWindow != null)
		{
			_ChatWindow.SetActive(value: true);
		}
		if (mPlayersMenu != null)
		{
			mPlayersMenu.ClearItems();
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
		if (inWidget.name == "BtnSkirmishLobby")
		{
			MOBALobbyManager.pInstance.ShowLobby(MOBALobbyType.SKIRMISH_LOBBY);
		}
		else if (inWidget.name == "BtnQuickMatch")
		{
			MOBALobbyManager.pInstance.AddToQuickMatch(isClan: true);
		}
	}
}
