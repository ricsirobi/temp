using System;
using System.Collections.Generic;
using KnowledgeAdventure.Multiplayer.Events;
using UnityEngine;

public class ArenaFrenzyMMOClient : MMOClient
{
	public enum MultiplayerGameStates
	{
		INITIAL,
		WAITING_FOR_OPPONENT,
		PLAYER_READY,
		GAME_STARTED,
		GAME_FINISHED
	}

	public class MMOPlayer
	{
		public MMOAvatar _MMOAvatar;

		public MultiplayerGameStates _GameState;

		public int _LastUpdatedScore;
	}

	public delegate void ResponseEvent(MMOExtensionResponseReceivedEventArgs responseArgs);

	public delegate void MessageEvent(MMOMessageReceivedEventArgs messageArgs);

	public static uint LOG_MASK = 16u;

	public const string ROOM_GROUP_ID = "RGI";

	public const string MAX_PLAYER_COUNT = "MPC";

	public const string ROOM_MAP_ID = "RMI";

	public const string HOST_CHALLENGE = "HC";

	public const string JOIN_BUDDY = "JB";

	public const string IS_LOCKED = "IL";

	public const string CMD_EXTN = "afg";

	public const string CMD_JOIN_USER_TO_ROOM = "af.JD";

	public const string CMD_PLAYER_WAITING = "afg.PW";

	public const string CMD_PLAYER_READY = "afg.PR";

	public const string CMD_GAME_END = "afg.GE";

	public const string CMD_LOCK_ROOM = "afg.LR";

	public const string CMD_TARGET_HIT = "afg.TDATA";

	public const string START_GAME = "SG";

	public const string MAP_DATA = "MD";

	public const string SCORE_DATA = "SCR";

	public const string TARGET_DATA = "TID";

	public const string JOIN_BUDDY_FAILED = "JBF";

	public const string TARGET_HIT = "THIT";

	public const string TEAM_DATA = "TEAMID";

	public const string END_GAME = "EG";

	public const string PLAYER_LEFT = "PL";

	public const string USER_COUNT = "UC";

	private MMOPlayer mOpponent;

	private string mJoinHostID = string.Empty;

	private bool mIsHost;

	private int mStartGameCount;

	private static ArenaFrenzyMMOClient mInstance = null;

	public string pJoinHostID => mJoinHostID;

	public bool pIsHost => mIsHost;

	public static ArenaFrenzyMMOClient pInstance => mInstance;

	public event ResponseEvent OnReponseEvent;

	public event MessageEvent OnMessageEvent;

	public void Awake()
	{
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.AddExtensionResponseEventHandler("SG", ArenaFrenzyResponseEventHandler);
			MainStreetMMOClient.pInstance.AddExtensionResponseEventHandler("MD", ArenaFrenzyResponseEventHandler);
			MainStreetMMOClient.pInstance.AddExtensionResponseEventHandler("SCR", ArenaFrenzyResponseEventHandler);
			MainStreetMMOClient.pInstance.AddExtensionResponseEventHandler("TID", ArenaFrenzyResponseEventHandler);
			MainStreetMMOClient.pInstance.AddExtensionResponseEventHandler("JBF", ArenaFrenzyResponseEventHandler);
			MainStreetMMOClient.pInstance.AddExtensionResponseEventHandler("THIT", ArenaFrenzyResponseEventHandler);
			MainStreetMMOClient.pInstance.AddExtensionResponseEventHandler("TEAMID", ArenaFrenzyResponseEventHandler);
			MainStreetMMOClient.pInstance.AddExtensionResponseEventHandler("EG", ArenaFrenzyResponseEventHandler);
			MainStreetMMOClient.pInstance.AddExtensionResponseEventHandler("PL", ArenaFrenzyResponseEventHandler);
			MainStreetMMOClient.pInstance.AddExtensionResponseEventHandler("UC", ArenaFrenzyResponseEventHandler);
			MainStreetMMOClient.pInstance.AddMessageReceivedEventHandler(ElementMatchMessageEventHandler);
		}
		else
		{
			UtDebug.LogError("MMO Client Not Ready.");
		}
		MainStreetMMOClient.AddClient(this);
	}

	public static void Init(bool isHost = false, string joinHostID = "")
	{
		if (mInstance == null)
		{
			mInstance = new GameObject("ArenaFrenzyMMOClient").AddComponent<ArenaFrenzyMMOClient>();
			mInstance.mJoinHostID = joinHostID;
			mInstance.mIsHost = isHost;
		}
		else
		{
			Debug.Log("Error: ArenaFrenzyMMOClient mInstance != null");
		}
		if (null != mInstance)
		{
			mInstance.JoinRoom();
		}
	}

	public void JoinRoom(bool challengeBuddy = false, string joinBuddyID = "")
	{
		if (MainStreetMMOClient.pInstance.pState == MMOClientState.IN_ROOM)
		{
			string pCurrentLevel = RsResourceManager.pCurrentLevel;
			UtDebug.Log(" mmo join request : " + pCurrentLevel);
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("RGI", pCurrentLevel);
			dictionary.Add("MPC", 2);
			dictionary.Add("RMI", 0);
			dictionary.Add("HC", challengeBuddy ? UserInfo.pInstance.UserID : string.Empty);
			dictionary.Add("JB", joinBuddyID);
			dictionary.Add("TEAMID", -1);
			MainStreetMMOClient.pInstance.SendExtensionMessage("", "af.JD", dictionary);
		}
		else
		{
			UtDebug.Log("The client mmo state is " + MainStreetMMOClient.pInstance.pState, LOG_MASK);
		}
	}

	public override void AddPlayer(MMOAvatar avatar)
	{
		if (ArenaFrenzyGame.pInstance != null)
		{
			ArenaFrenzyGame.PlayerInfo playerInfo = new ArenaFrenzyGame.PlayerInfo();
			playerInfo._PlayerUserID = avatar.pUserID;
			playerInfo._MMOAvatar = avatar;
			playerInfo._Name = playerInfo._MMOAvatar.pAvatarData.mInstance.DisplayName;
			UtDebug.Log("Add player Player Name = " + playerInfo._Name);
			ArenaFrenzyGame.pInstance.AddPlayer(playerInfo);
		}
	}

	public override void RemovePlayer(MMOAvatar avatar)
	{
		ArenaFrenzyGame.pInstance.RemovePlayer(avatar.pUserID);
	}

	public override void Disconnected()
	{
	}

	public override void Reset()
	{
		mOpponent = null;
	}

	public void DestroyMMO()
	{
		MainStreetMMOClient.pInstance.RemoveExtensionResponseEventHandler("SG", ArenaFrenzyResponseEventHandler);
		MainStreetMMOClient.pInstance.RemoveExtensionResponseEventHandler("MD", ArenaFrenzyResponseEventHandler);
		MainStreetMMOClient.pInstance.RemoveExtensionResponseEventHandler("SCR", ArenaFrenzyResponseEventHandler);
		MainStreetMMOClient.pInstance.RemoveExtensionResponseEventHandler("TID", ArenaFrenzyResponseEventHandler);
		MainStreetMMOClient.pInstance.RemoveExtensionResponseEventHandler("JBF", ArenaFrenzyResponseEventHandler);
		MainStreetMMOClient.pInstance.RemoveExtensionResponseEventHandler("THIT", ArenaFrenzyResponseEventHandler);
		MainStreetMMOClient.pInstance.RemoveExtensionResponseEventHandler("TEAMID", ArenaFrenzyResponseEventHandler);
		MainStreetMMOClient.pInstance.RemoveExtensionResponseEventHandler("EG", ArenaFrenzyResponseEventHandler);
		MainStreetMMOClient.pInstance.RemoveExtensionResponseEventHandler("PL", ArenaFrenzyResponseEventHandler);
		MainStreetMMOClient.pInstance.RemoveExtensionResponseEventHandler("UC", ArenaFrenzyResponseEventHandler);
		MainStreetMMOClient.RemoveClient(this);
		if (mInstance != null && mInstance.gameObject != null)
		{
			UnityEngine.Object.Destroy(mInstance.gameObject);
		}
	}

	public override void OnJoinedRoom(MMOJoinedRoomEventArgs inJoinedRoomArgs)
	{
		if (!inJoinedRoomArgs.RoomJoined.RoomName.Equals("LIMBO"))
		{
			AvAvatar.pState = AvAvatarState.MOVING;
		}
		else
		{
			UtDebug.Log("JOINED ROOM IS LIMBO");
		}
		UtDebug.Log("Joined room  : " + inJoinedRoomArgs.RoomJoined.RoomName);
		ArenaFrenzyGame.PlayerInfo playerInfo = new ArenaFrenzyGame.PlayerInfo();
		playerInfo._MMOAvatar = null;
		playerInfo._PlayerUserID = UserInfo.pInstance.UserID;
		playerInfo._Name = AvatarData.pInstance.DisplayName;
		if (ArenaFrenzyGame.pInstance != null)
		{
			ArenaFrenzyGame.pInstance.AddPlayer(playerInfo);
		}
		mStartGameCount = 0;
	}

	public void OnDisable()
	{
		DestroyMMO();
		Destroy();
	}

	public bool IsGamePlayer(string inUserID)
	{
		if (!(UserInfo.pInstance.UserID == inUserID))
		{
			if (mOpponent != null && mOpponent._MMOAvatar != null)
			{
				return mOpponent._MMOAvatar.pUserID == inUserID;
			}
			return false;
		}
		return true;
	}

	public bool IsOpponentGameComplete()
	{
		if (mOpponent != null || !(mOpponent._MMOAvatar == null))
		{
			return mOpponent._GameState == MultiplayerGameStates.GAME_FINISHED;
		}
		return true;
	}

	public void SetOpponentGameState()
	{
	}

	protected void SendExtensionMessage(string inType, Dictionary<string, object> d)
	{
		MainStreetMMOClient.pInstance.SendExtensionMessage(inType, d);
	}

	private void ElementMatchMessageEventHandler(object sender, MMOMessageReceivedEventArgs args)
	{
		if (this.OnMessageEvent != null)
		{
			this.OnMessageEvent(args);
		}
	}

	private void ArenaFrenzyResponseEventHandler(object sender, MMOExtensionResponseReceivedEventArgs args)
	{
		if (this.OnReponseEvent != null)
		{
			this.OnReponseEvent(args);
		}
		Dictionary<string, object> responseDataObject = args.ResponseDataObject;
		string text = responseDataObject["0"].ToString();
		UtDebug.Log("cmd : " + text, LOG_MASK);
		switch (text)
		{
		case "UC":
			if (Convert.ToInt32(responseDataObject["1"].ToString()) > 1)
			{
				mIsHost = false;
			}
			else
			{
				mIsHost = true;
			}
			break;
		case "MD":
		{
			int seed = Convert.ToInt32(responseDataObject["1"].ToString());
			int mapID = Convert.ToInt32(responseDataObject["2"].ToString());
			UtDebug.Log("MAP_DATA\nSeed : " + seed + "  map ID : " + mapID);
			if (ArenaFrenzyGame.pInstance != null)
			{
				ArenaFrenzyGame.pInstance._GameHUD.SetMMOStatusMessage(ArenaFrenzyGame.pInstance._GameStartingText);
				ArenaFrenzyGame arenaFrenzyGame = ArenaFrenzyGame.pInstance;
				arenaFrenzyGame._OnMapLoaded = (Action)Delegate.Combine(arenaFrenzyGame._OnMapLoaded, new Action(OnMapLoaded));
				ArenaFrenzyGame.pInstance.LoadMap(mapID, seed);
			}
			break;
		}
		case "SG":
			if (ArenaFrenzyGame.pInstance != null)
			{
				mStartGameCount++;
				if (mStartGameCount > 1)
				{
					UtDebug.Log("START_GAME : = " + mStartGameCount);
				}
				ArenaFrenzyGame.pInstance.ClearTeams();
				int num;
				for (num = 1; num < responseDataObject.Count; num++)
				{
					string text2 = responseDataObject[num.ToString()].ToString();
					num++;
					int teamIdx = Convert.ToInt32(responseDataObject[num.ToString()].ToString());
					UtDebug.Log("PID = " + text2 + " Team ID = " + teamIdx);
					ArenaFrenzyGame.pInstance.AssignPlayerToTeam(text2, teamIdx);
				}
				ArenaFrenzyGame.pInstance.StartGame();
			}
			break;
		case "JBF":
			if (ArenaFrenzyGame.pInstance != null)
			{
				ArenaFrenzyGame.pInstance.JoinBuddyFailed();
			}
			break;
		case "THIT":
			if (ArenaFrenzyGame.pInstance != null)
			{
				UtDebug.Log("User hit : " + responseDataObject["1"].ToString() + "  target ID : " + responseDataObject["2"].ToString());
				string userID = responseDataObject["1"].ToString();
				int inTargetID = Convert.ToInt32(responseDataObject["2"].ToString());
				ArenaFrenzyTarget target = ArenaFrenzyGame.pInstance.GetTarget(inTargetID);
				if (target != null && target.pParentMapElement != null)
				{
					target.pParentMapElement.HandleTargetHitMMO(target, userID);
				}
				else
				{
					UtDebug.Log("No target found!!!");
				}
			}
			break;
		case "EG":
			if (ArenaFrenzyGame.pInstance != null && ArenaFrenzyGame.pInstance.pIsInGame)
			{
				for (int i = 1; i + 1 < responseDataObject.Count; i += 2)
				{
					string playerID = responseDataObject[i.ToString()].ToString();
					string score = responseDataObject[(i + 1).ToString()].ToString();
					ArenaFrenzyGame.pInstance.SetScoreData(playerID, ParseTeamScore(score));
				}
				ArenaFrenzyGame.pInstance.OnGameComplete();
			}
			break;
		}
	}

	public void OnGameComplete(ArenaFrenzyGame.TeamInfo teamData)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("SCR", GetScoreData(teamData));
		MainStreetMMOClient.pInstance.SendExtensionMessageToRoom("afg.GE", dictionary);
	}

	public string GetScoreData(ArenaFrenzyGame.TeamInfo teamData)
	{
		if (teamData == null)
		{
			return string.Empty;
		}
		return teamData._Score + "#" + teamData._AccurateShots + "#" + teamData._TotalShots;
	}

	public ArenaFrenzyGame.TeamInfo ParseTeamScore(string score)
	{
		string[] array = score.Split('#');
		if (array != null && array.Length > 2)
		{
			return new ArenaFrenzyGame.TeamInfo
			{
				_Score = Convert.ToInt32(array[0]),
				_AccurateShots = Convert.ToInt32(array[1]),
				_TotalShots = Convert.ToInt32(array[2])
			};
		}
		return null;
	}

	public void OnTargetHit(int targetID)
	{
		UtDebug.Log("on target hit : " + targetID);
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("TID", targetID);
		MainStreetMMOClient.pInstance.SendExtensionMessageToRoom("afg.TDATA", dictionary);
	}

	public void OnAllPlayersLoaded()
	{
		UtDebug.Log("all players loaded ");
		if (ArenaFrenzyGame.pInstance != null)
		{
			ArenaFrenzyGame.pInstance._GameHUD.WaitingForPlayers(waiting: false);
		}
		MainStreetMMOClient.pInstance.SendExtensionMessageToRoom("afg.PW", null);
	}

	private void OnMapLoaded()
	{
		UtDebug.Log("Map Loaded Sucessfully");
		MainStreetMMOClient.pInstance.SendExtensionMessageToRoom("afg.PR", null);
		if ((bool)ArenaFrenzyGame.pInstance)
		{
			ArenaFrenzyGame arenaFrenzyGame = ArenaFrenzyGame.pInstance;
			arenaFrenzyGame._OnMapLoaded = (Action)Delegate.Remove(arenaFrenzyGame._OnMapLoaded, new Action(OnMapLoaded));
		}
	}
}
