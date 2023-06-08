using System.Collections.Generic;
using KnowledgeAdventure.Multiplayer.Events;
using UnityEngine;

public class RacingMMOClient : MMOClient
{
	public delegate void ResponseEvent(MMOExtensionResponseReceivedEventArgs responseArgs);

	public delegate void MessageEvent(MMOMessageReceivedEventArgs messageArgs);

	private const string CMD_JOIN_USER_TO_ROOM = "dr.JD";

	public const string CMD_USER_ACK = "dr.UACK";

	public const string CMD_ALL_READY_ACK = "dr.ARACK";

	private const string CMD_SEND_RESULT = "dr.AR";

	private const string CMD_PAUSE_COUNTDOWN = "dr.PCD";

	private const string CMD_PING_USER = "dr.PNG";

	private const string CMD_INVITE = "dr.IV";

	private const string CMD_SELECT_VISIT = "dr.VR";

	private const string CMD_PLAYER_READY = "dr.PR";

	private const string CMD_PLAYER_READY_STATE = "dr.PS";

	private const string COMMAND_NAME = "RA";

	public const char MESSAGE_SEPARATOR = ':';

	private const string COLLECT_OBJECT = "COB";

	private const string DESTRUCTIBLE_OBJECT = "DEO";

	private const string POWERUP_USE = "PEU";

	private const string POWERUP_ADD = "PLA";

	private const string POWERUP_RESET = "PR";

	private const string POWERUP_REMOVE = "RP";

	private const string GAME_MODE = "GM";

	private const string QUICK_JOIN = "QJ";

	private const string GAME_HOST = "GH";

	private const string GAME_COMPLETE = "GC";

	private const string DISTANCE = "DT";

	private const string READYTORACE = "RTR";

	private const string USER_LEFT = "UL";

	private const string TRACK_ID = "TID";

	private const string LOBBY_TIME = "LT";

	private const string JOIN_ROOM = "JR";

	private const string PAUSE_STATE = "PS";

	private const string SCENE_TRANSITION = "ST";

	private const string LOADING_STATUS = "LS";

	private const string ROOMID = "RID";

	public const string PLAYER_READY = "IMR";

	public bool mIsSendDone;

	private bool mIsInitialized;

	private GameObject mObject;

	public bool mReceivedPlayerStates;

	private LevelManager mLevelManager;

	private MainMenu mMainMenu;

	private float mLastLoadValue;

	public bool mLoadingRaceScene;

	public event ResponseEvent OnReponseEvent;

	public event MessageEvent OnMessageEvent;

	public static RacingMMOClient Init(MainMenu uiMainMenu = null, LevelManager levelManager = null)
	{
		RacingMMOClient racingMMOClient = new GameObject("RacingMMOClient").AddComponent<RacingMMOClient>();
		racingMMOClient.mMainMenu = uiMainMenu;
		racingMMOClient.mLevelManager = levelManager;
		if (uiMainMenu != null)
		{
			if (MainStreetMMOClient.pInstance != null && MainStreetMMOClient.pInstance.pState == MMOClientState.DISCONNECTING)
			{
				MainStreetMMOClient.pInstance.Connect();
			}
			else
			{
				MainStreetMMOClient.Init();
			}
		}
		MainStreetMMOClient.AddClient(racingMMOClient);
		return racingMMOClient;
	}

	public override void Destroy()
	{
		if (mObject != null)
		{
			Object.Destroy(mObject);
		}
		MainStreetMMOClient.RemoveClient(this);
		mObject = null;
		mIsInitialized = false;
		mMainMenu = null;
	}

	private void OnDisable()
	{
		if (mLevelManager != null)
		{
			mLevelManager.Unregister();
		}
		Destroy();
	}

	private void Update()
	{
		if (MainStreetMMOClient.pIsReady)
		{
			if (!mIsInitialized)
			{
				mIsInitialized = true;
				MainStreetMMOClient.pInstance.AddExtensionResponseEventHandler("RA", RacingResponseEventHandler);
				MainStreetMMOClient.pInstance.AddMessageReceivedEventHandler(RacingMessageEventHandler);
			}
			if (!mReceivedPlayerStates)
			{
				mReceivedPlayerStates = true;
				RequestPlayerStates();
			}
		}
	}

	public override void OnClose()
	{
	}

	public void ResetLoadingState()
	{
		mLoadingRaceScene = false;
		mLastLoadValue = -1f;
	}

	public override void Reset()
	{
		Object.Destroy(mObject);
		mIsInitialized = false;
	}

	public override void Disconnected()
	{
	}

	public override void AddPlayer(MMOAvatar avatar)
	{
		if (mMainMenu != null)
		{
			mMainMenu.AddPlayer(avatar);
		}
	}

	public override void RemovePlayer(MMOAvatar avatar)
	{
		if (mMainMenu != null)
		{
			mMainMenu.RemovePlayer(avatar);
		}
		if (mLevelManager != null)
		{
			mLevelManager.RemovePlayer(avatar);
		}
	}

	public void PauseGame(bool isPause)
	{
		new Dictionary<string, object>().Add("PS", isPause);
	}

	public void ObjectCollected(string objName)
	{
	}

	public void DestructibleHit(string objName)
	{
	}

	public void SendPowerUpInfo(string userId, string powerupName, string targetuserId, Vector3 pos)
	{
		UtDebug.Log("Sending the powerup info " + targetuserId + " : " + powerupName + " : " + userId);
	}

	public bool SendLoadingStatus(string userId, float progress, bool checkPrevValue)
	{
		if (!MainStreetMMOClient.pIsReady || !mLoadingRaceScene)
		{
			return false;
		}
		if (checkPrevValue && progress == mLastLoadValue)
		{
			return false;
		}
		mLastLoadValue = progress;
		return true;
	}

	public void AddPowerUpInfo(string userId, string powerupName, string itemName)
	{
	}

	public void RemovePowerUpInfo(string userId, string powerupName)
	{
	}

	public void SendPowerupReset(string userId)
	{
	}

	public void ResetUser()
	{
		if (AvAvatar.pObject != null)
		{
			UtDebug.Log("Resetting the user : " + AvAvatar.pObject.name);
		}
		else
		{
			UtDebug.Log("Resetting the user.");
		}
		if (!RacingManager.pIsSinglePlayer)
		{
			mIsSendDone = false;
		}
	}

	public void SendDistance(string userId, float distance)
	{
		MainStreetMMOClient.pInstance.SendPublicExtensionMessage("DT:" + userId + ":" + distance);
	}

	public void SendReadyToRace(string userId, string state)
	{
		MainStreetMMOClient.pInstance.SendPublicExtensionMessage("RTR:" + userId + ":" + state);
	}

	public void SendResult(float time, float distance, int lapsCompleted)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("UN", AvatarData.pInstance.DisplayName);
		dictionary.Add("CT", time);
		dictionary.Add("FD", distance);
		dictionary.Add("LC", lapsCompleted);
		MainStreetMMOClient.pInstance.SendExtensionMessageToRoom("dr.AR", dictionary);
	}

	private void RequestPlayerStates()
	{
		MainStreetMMOClient.pInstance.SendExtensionMessage("dr.PS", null);
	}

	public void SendPlayerReady(bool isReady, bool isToRoom = false)
	{
		MainStreetMMOClient.pInstance.SendPublicMMOMessage("IMR:" + UserInfo.pInstance.UserID + ":" + isReady);
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("IMR", isReady);
		if (!isToRoom)
		{
			MainStreetMMOClient.pInstance.SendExtensionMessage("dr.PR", dictionary);
		}
		else
		{
			MainStreetMMOClient.pInstance.SendExtensionMessageToRoom("dr.PR", dictionary);
		}
	}

	public void SendPlayerCount(int playerCount)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("CNT", playerCount);
		MainStreetMMOClient.pInstance.SendExtensionMessage("dr.PRC", dictionary);
	}

	public override void RemoveAll()
	{
		if (MainStreetMMOClient.pInstance.pRoomName.Contains("RacingDragon"))
		{
			mMainMenu.ShowLobby();
		}
	}

	public void SendJoinData(int roomId, int raceType, int gameMode, int theme, int themeIdx, bool isPublic, int minPlayers)
	{
		mIsSendDone = true;
		string value = "0#0#" + themeIdx + "#" + raceType + "#" + (isPublic ? "0" : "1") + "#" + minPlayers;
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("RID", roomId.ToString());
		dictionary.Add("GM", value);
		dictionary.Add("QJ", true);
		MainStreetMMOClient.pInstance.SendExtensionMessage("dr.JD", dictionary);
	}

	public void SendInviteUpdate(int roomId)
	{
		new Dictionary<string, object>().Add("RI", roomId);
	}

	public void SendVisitMessage(string ownerID)
	{
		new Dictionary<string, object>().Add("UN", ownerID);
	}

	private void RacingResponseEventHandler(object sender, MMOExtensionResponseReceivedEventArgs args)
	{
		if (this.OnReponseEvent != null)
		{
			this.OnReponseEvent(args);
		}
	}

	private void RacingMessageEventHandler(object sender, MMOMessageReceivedEventArgs args)
	{
		if (this.OnMessageEvent != null)
		{
			this.OnMessageEvent(args);
		}
	}
}
