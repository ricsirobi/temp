using System.Collections.Generic;
using KnowledgeAdventure.Multiplayer.Events;
using UnityEngine;

public class ElementMatchMMOClient : MMOClient
{
	public enum MultiplayerGameStates
	{
		INITIAL,
		WAITING_FOR_OPPONENT,
		PLAYER_READY,
		GAME_STARTED,
		GAME_FINISHED
	}

	public enum ElementMatchRoomTypes
	{
		JOIN_ANY,
		JOIN_BUDDY,
		HOST_FOR_ANY,
		HOST_FOR_BUDDY
	}

	public class MMOPlayer
	{
		public MMOAvatar _MMOAvatar;

		public MultiplayerGameStates _GameState;

		public int _LastUpdatedScore;
	}

	public delegate void ResponseEvent(MMOExtensionResponseReceivedEventArgs responseArgs);

	public delegate void MessageEvent(MMOMessageReceivedEventArgs messageArgs);

	public const string JOIN_ANY_ROOM = "mm.JAR";

	public const string JOIN_BUDDY_ROOM = "mm.JBR";

	public const string HOST_ROOM_FOR_ANY = "mm.HRFA";

	public const string HOST_ROOM_FOR_BUDDY = "mm.HRFB";

	public const string PLAYER_READY = "mm.PR";

	public const string PLAYER_NOT_READY = "mm.PNR";

	public const string MSG_START_GAME = "mm.SG";

	public const string MSG_GAME_COMPLETE = "mm.GC";

	public const string EM_COMMAND_NAME = "em";

	public const char MESSAGE_SEPARATOR = ':';

	private MultiplayerGameStates mState;

	private MMOPlayer mOpponent;

	public event ResponseEvent OnReponseEvent;

	public event MessageEvent OnMessageEvent;

	public void Awake()
	{
		MainStreetMMOClient.pInstance.AddExtensionResponseEventHandler("em", ElementMatchResponseEventHandler);
		MainStreetMMOClient.pInstance.AddMessageReceivedEventHandler(ElementMatchMessageEventHandler);
	}

	public override bool IsInvalidState()
	{
		return false;
	}

	public override bool IsSinglePlayer()
	{
		return false;
	}

	public override void AddPlayer(MMOAvatar avatar)
	{
		Debug.Log("PlayerData Added " + avatar.name);
		if (mOpponent == null)
		{
			mOpponent = new MMOPlayer();
			mOpponent._MMOAvatar = avatar;
		}
	}

	public override void RemovePlayer(MMOAvatar avatar)
	{
		if (mOpponent != null && mOpponent._MMOAvatar != null && avatar.name == mOpponent._MMOAvatar.name)
		{
			_ = mState;
			_ = 3;
			mOpponent._GameState = MultiplayerGameStates.GAME_FINISHED;
		}
	}

	public override void Disconnected()
	{
	}

	public override void Reset()
	{
		mOpponent = null;
	}

	public override void Destroy()
	{
		MainStreetMMOClient.RemoveClient(this);
	}

	public override void RemoveAll()
	{
	}

	public override void OnClose()
	{
	}

	public void OnDisable()
	{
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

	public void JoinGameRoom(ElementMatchRoomTypes inRoomType)
	{
		switch (inRoomType)
		{
		case ElementMatchRoomTypes.JOIN_ANY:
			SendExtensionMessage("mm.JAR", null);
			break;
		case ElementMatchRoomTypes.JOIN_BUDDY:
		{
			Dictionary<string, object> d = new Dictionary<string, object>();
			SendExtensionMessage("mm.JBR", d);
			break;
		}
		case ElementMatchRoomTypes.HOST_FOR_ANY:
			SendExtensionMessage("mm.HRFA", null);
			break;
		case ElementMatchRoomTypes.HOST_FOR_BUDDY:
			SendExtensionMessage("mm.HRFB", null);
			break;
		}
	}

	public void LeaveGameRoom()
	{
		MainStreetMMOClient.pInstance.SendExtensionMessage("le", "JA", null);
	}

	public void SetPlayerReady(bool inIsReady)
	{
		MainStreetMMOClient.pInstance.SendPublicMMOMessage("mm.PR:" + UserInfo.pInstance.UserID + ":" + inIsReady);
		SendExtensionMessage(inIsReady ? "mm.PR" : "mm.PNR", null);
	}

	public void UpdateHeadToHeadGameScore()
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

	private void ElementMatchResponseEventHandler(object sender, MMOExtensionResponseReceivedEventArgs args)
	{
		if (this.OnReponseEvent != null)
		{
			this.OnReponseEvent(args);
		}
	}
}
