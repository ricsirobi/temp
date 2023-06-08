using System.Collections.Generic;
using KnowledgeAdventure.Multiplayer.Events;
using UnityEngine;

public class MOBALobbyManager : MonoBehaviour
{
	public UiMOBASkirmishLobby _SkirmishLobby;

	public UiMOBAClanLobby _ClanLobby;

	public UiMOBAPreGameLobby _PreGameLobby;

	public LocaleString _TeamInviteMessageText = new LocaleString("{{PlayerName}} has invited you to join their party.");

	public LocaleString _TeamLeaderAbandonMessageText = new LocaleString("The team leader has left.");

	public LocaleString _KickedFromTeamMessageText = new LocaleString("You have been kicked from the team.");

	private MOBAMMOClient mClient;

	private string mCurrentInviter;

	private const string USER_ID = "UID";

	private const string ISCLAN = "ISCLAN";

	private const string PLAYER_INVITE = "IN";

	private const string PLAYER_ACK = "UACK";

	private const string TEAM_UPDATED = "TU";

	private const string TEAM_ADDED = "TA";

	private const string TEAM_READY = "RD";

	private const string TEAM_NOTREADY = "NRD";

	private const string TEAM_CHALLENGE = "TCH";

	private const string CANCEL_CHALLENGE = "CNCH";

	private const string JOIN_ACK = "JACK";

	private const string PLAYER_JOIN = "JN";

	private const string LOBBY_TIME = "TMR";

	private const string PLAYER_KICKED = "KL";

	private const string TEAM_COLLAPSED = "TC";

	private const string ENTER_PREGAME_LOBBY = "EPL";

	private const string CMD_JOIN = "moba.JN";

	private const string CMD_PLAYER_INVITE = "moba.IN";

	private const string CMD_PLAYER_ACK = "moba.UACK";

	private const string CMD_TEAM_READY = "moba.RD";

	private const string CMD_TEAM_NOTREADY = "moba.NRD";

	private const string CMD_JOIN_ACK = "moba.JACK";

	private const string CMD_LEAVE_TEAM = "moba.LT";

	private const string CMD_KICK_PLAYER = "moba.KP";

	private const string CMD_TEAM_CHALLENGE = "moba.TCH";

	private const string CMD_ACCEPT_CHALLENGE = "moba.CHACK";

	private const string CMD_CANCEL_CHALLENGE = "moba.CNCH";

	private const string CMD_ENTER_QUICKMATCH = "moba.QM";

	private const string CMD_JOIN_GAMEROOM = "moba.JGR";

	private const string CMD_PAUSE_TIMER = "moba.PTMR";

	private const string CMD_RESUME_TIMER = "moba.RTMR";

	private bool mIsJoiningPreGame;

	private List<string> mPendingChallengeInvites = new List<string>();

	private MOBATeam mCurrentTeam;

	private static MOBALobbyManager mInstance;

	public MOBATeam pCurrentTeam
	{
		get
		{
			return mCurrentTeam;
		}
		set
		{
			mCurrentTeam = value;
			if (_ClanLobby.GetVisibility())
			{
				_ClanLobby.UpdateMyTeam(mCurrentTeam);
			}
			else if (_SkirmishLobby.GetVisibility())
			{
				_SkirmishLobby.UpdateMyTeam(mCurrentTeam);
			}
		}
	}

	public static MOBALobbyManager pInstance => mInstance;

	private void Start()
	{
		mInstance = this;
		RsResourceManager.DestroyLoadScreen();
		mClient = MOBAMMOClient.Init(this);
		mClient.OnMessageEvent += LobbyMessageHandler;
		mClient.OnReponseEvent += LobbyExtensionResponse;
		if (UtPlatform.IsMobile() || UtPlatform.IsWSA())
		{
			KAInput.ShowJoystick(UiJoystick.pInstance.pPos, inShow: false);
		}
		_ClanLobby.SetVisibility(inVisible: false);
		_SkirmishLobby.SetVisibility(inVisible: false);
		_PreGameLobby.SetVisibility(inVisible: false);
		UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
		if (UserProfile.pProfileData.HasGroup())
		{
			ShowLobby(MOBALobbyType.CLAN_LOBBY);
		}
		else
		{
			ShowLobby(MOBALobbyType.SKIRMISH_LOBBY);
		}
	}

	private void OnDestroy()
	{
		mInstance = null;
	}

	public void ShowLobby(MOBALobbyType lobbyType)
	{
		if (lobbyType == MOBALobbyType.CLAN_LOBBY && UserProfile.pProfileData.HasGroup())
		{
			_ClanLobby.Init();
			_SkirmishLobby.SetVisibility(inVisible: false);
		}
		else if (lobbyType == MOBALobbyType.SKIRMISH_LOBBY)
		{
			_SkirmishLobby.Init();
			_ClanLobby.SetVisibility(inVisible: false);
		}
	}

	public void EnterPreGame(MOBATeam oppTeam)
	{
		_ClanLobby.SetVisibility(inVisible: false);
		_SkirmishLobby.SetVisibility(inVisible: false);
		_PreGameLobby.Init(oppTeam);
	}

	private void LobbyMessageHandler(MMOMessageReceivedEventArgs args)
	{
		Debug.Log("MOBA Lobby message event handler");
		_ = args.MMOMessage.MessageType;
		_ = 2;
	}

	private void LobbyExtensionResponse(MMOExtensionResponseReceivedEventArgs args)
	{
		int num = 3;
		string text = args.ResponseDataObject["2"].ToString();
		Debug.Log("Welcome TO MOBA ROOM" + text);
		switch (text)
		{
		case "IN":
		{
			string text7 = (string)args.ResponseDataObject["3"];
			WsWebService.GetDisplayNameByUserID(text7, TeamInviteHandler, text7);
			break;
		}
		case "RD":
		{
			Debug.Log("new Team ready" + (string)args.ResponseDataObject[num.ToString()]);
			string text3 = (string)args.ResponseDataObject[num.ToString()];
			if (bool.Parse((string)args.ResponseDataObject[(num + 1).ToString()]) && _ClanLobby.GetVisibility())
			{
				if (pCurrentTeam == null || (pCurrentTeam != null && pCurrentTeam.pMemberIds[0] != text3))
				{
					_ClanLobby.AddTeam(text3);
				}
			}
			else if (_SkirmishLobby.GetVisibility() && (pCurrentTeam == null || (pCurrentTeam != null && pCurrentTeam.pMemberIds[0] != text3)))
			{
				_SkirmishLobby.AddTeam(text3);
			}
			break;
		}
		case "NRD":
		{
			string text5 = (string)args.ResponseDataObject[num.ToString()];
			if (bool.Parse((string)args.ResponseDataObject[(num + 1).ToString()]) && _ClanLobby != null)
			{
				if (pCurrentTeam == null || (pCurrentTeam != null && pCurrentTeam.pMemberIds[0] != text5))
				{
					_ClanLobby.RemoveTeam(text5);
				}
			}
			else if (_SkirmishLobby != null && (pCurrentTeam == null || (pCurrentTeam != null && pCurrentTeam.pMemberIds[0] != text5)))
			{
				_SkirmishLobby.RemoveTeam(text5);
			}
			break;
		}
		case "TU":
		{
			Debug.Log("Updating team");
			List<string> list2 = new List<string>();
			for (int j = num; j < args.ResponseDataObject.Count; j++)
			{
				Debug.Log("Team member's" + j / num + ":" + (string)args.ResponseDataObject[j.ToString()]);
				string text8 = (string)args.ResponseDataObject[j.ToString()];
				if (!string.IsNullOrEmpty(text8))
				{
					list2.Add(text8);
				}
			}
			if (list2.Count == 1)
			{
				pCurrentTeam = null;
				break;
			}
			MOBATeam mOBATeam2 = new MOBATeam();
			mOBATeam2.pMemberIds = list2;
			mOBATeam2.pName = "TEAM - " + list2[0];
			pCurrentTeam = mOBATeam2;
			break;
		}
		case "KL":
		{
			KAUIGenericDB kAUIGenericDB2 = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
			kAUIGenericDB2._MessageObject = base.gameObject;
			KAUI.SetExclusive(kAUIGenericDB2, new Color(0.5f, 0.5f, 0.5f, 0.5f));
			kAUIGenericDB2.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			kAUIGenericDB2.SetText(_KickedFromTeamMessageText.GetLocalizedString(), interactive: false);
			kAUIGenericDB2.SetDestroyOnClick(isDestroy: true);
			pCurrentTeam = null;
			break;
		}
		case "TC":
			if (UserInfo.pInstance.UserID != pCurrentTeam.pMemberIds[0] && !mIsJoiningPreGame)
			{
				KAUIGenericDB kAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
				kAUIGenericDB._MessageObject = base.gameObject;
				KAUI.SetExclusive(kAUIGenericDB, new Color(0.5f, 0.5f, 0.5f, 0.5f));
				kAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
				kAUIGenericDB.SetText(_TeamLeaderAbandonMessageText.GetLocalizedString(), interactive: false);
				kAUIGenericDB.SetDestroyOnClick(isDestroy: true);
			}
			pCurrentTeam = null;
			break;
		case "TCH":
		{
			Debug.Log("Received Challenge Team" + (string)args.ResponseDataObject[num.ToString()]);
			string text6 = (string)args.ResponseDataObject[num.ToString()];
			if (_ClanLobby.GetVisibility())
			{
				_ClanLobby.AddChallengeInvite(text6);
			}
			else if (_SkirmishLobby.GetVisibility())
			{
				_SkirmishLobby.AddChallengeInvite(text6);
			}
			mPendingChallengeInvites.Add(text6);
			break;
		}
		case "JN":
		{
			Debug.Log("JOIN ACCEPTED");
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
			dictionary2.Add("MEMBER", (string)args.ResponseDataObject[num.ToString()]);
			MainStreetMMOClient.pInstance.SendExtensionMessageToRoom("moba.JACK", dictionary2);
			break;
		}
		case "TMR":
		{
			string text4 = args.ResponseDataObject[num.ToString()].ToString();
			Debug.Log("LOBBY TIME :" + text4);
			if (_PreGameLobby != null)
			{
				_PreGameLobby.UpdateTimer(int.Parse(text4));
			}
			break;
		}
		case "EPL":
		{
			string value = (string)args.ResponseDataObject[num.ToString()];
			List<string> list = new List<string>();
			for (int i = num + 1; i < args.ResponseDataObject.Count; i++)
			{
				string text2 = (string)args.ResponseDataObject[i.ToString()];
				if (!string.IsNullOrEmpty(text2))
				{
					list.Add(text2);
				}
			}
			MOBATeam mOBATeam = new MOBATeam();
			mOBATeam.pMemberIds = list;
			mOBATeam.pName = "TEAM - " + list[0];
			EnterPreGame(mOBATeam);
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("JOINDATA", value);
			MainStreetMMOClient.pInstance.SendExtensionMessage("moba.JGR", dictionary);
			mIsJoiningPreGame = true;
			break;
		}
		}
	}

	private void TeamInviteHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE && inType == WsServiceType.GET_DISPLAYNAME_BY_USER_ID)
		{
			mCurrentInviter = (string)inUserData;
			KAUIGenericDB kAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
			kAUIGenericDB._MessageObject = base.gameObject;
			KAUI.SetExclusive(kAUIGenericDB, new Color(0.5f, 0.5f, 0.5f, 0.5f));
			kAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
			string localizedString = _TeamInviteMessageText.GetLocalizedString();
			localizedString = localizedString.Replace("{{PlayerName}}", (string)inObject);
			kAUIGenericDB.SetText(localizedString, interactive: false);
			kAUIGenericDB._YesMessage = "AcceptTeamInvitation";
			kAUIGenericDB.SetDestroyOnClick(isDestroy: true);
		}
	}

	private void AcceptTeamInvitation()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("LEADER", mCurrentInviter);
		MainStreetMMOClient.pInstance.SendExtensionMessageToRoom("moba.UACK", dictionary);
		mCurrentInviter = null;
	}

	public void SendTeamInvite(string userID)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("UID", userID);
		MainStreetMMOClient.pInstance.SendExtensionMessageToRoom("moba.IN", dictionary);
	}

	public void LeaveTeam()
	{
		MainStreetMMOClient.pInstance.SendExtensionMessageToRoom("moba.LT", null);
		pCurrentTeam = null;
	}

	public void KickPlayer(string userID)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("UID", userID);
		MainStreetMMOClient.pInstance.SendExtensionMessageToRoom("moba.KP", dictionary);
	}

	public void SetTeamReady(bool isReady, bool isClan)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("ISCLAN", isClan.ToString());
		if (isReady)
		{
			MainStreetMMOClient.pInstance.SendExtensionMessageToRoom("moba.RD", dictionary);
		}
		else
		{
			MainStreetMMOClient.pInstance.SendExtensionMessageToRoom("moba.NRD", dictionary);
		}
	}

	public void SendChallenge(string leaderId)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("UID", leaderId);
		if (mPendingChallengeInvites.Contains(leaderId))
		{
			mPendingChallengeInvites.Remove(leaderId);
			MainStreetMMOClient.pInstance.SendExtensionMessage("moba.CHACK", dictionary);
		}
		else
		{
			MainStreetMMOClient.pInstance.SendExtensionMessageToRoom("moba.TCH", dictionary);
		}
	}

	public void AcceptChallenge(string leaderId)
	{
		mPendingChallengeInvites.Remove(leaderId);
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("UID", leaderId);
		MainStreetMMOClient.pInstance.SendExtensionMessageToRoom("moba.CHACK", dictionary);
	}

	public void AddToQuickMatch(bool isClan)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("ISCLAN", isClan.ToString());
		MainStreetMMOClient.pInstance.SendExtensionMessageToRoom("moba.QM", dictionary);
	}

	public void AddPlayer(MMOAvatar player)
	{
		if (UserProfile.pProfileData.HasGroup() && _ClanLobby != null)
		{
			_ClanLobby.AddPlayer(player);
		}
		if (_SkirmishLobby != null)
		{
			_SkirmishLobby.AddPlayer(player);
		}
	}

	public void RemovePlayer(MMOAvatar player)
	{
		if (_ClanLobby != null)
		{
			_ClanLobby.RemovePlayer(player);
		}
		if (_SkirmishLobby != null)
		{
			_SkirmishLobby.RemovePlayer(player);
		}
	}

	public void PauseTimer(bool bPause)
	{
		if (bPause)
		{
			MainStreetMMOClient.pInstance.SendExtensionMessageToRoom("moba.PTMR", null);
		}
		else
		{
			MainStreetMMOClient.pInstance.SendExtensionMessageToRoom("moba.RTMR", null);
		}
	}
}
