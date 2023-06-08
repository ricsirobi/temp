using System;
using System.Collections.Generic;
using KnowledgeAdventure.Multiplayer;
using KnowledgeAdventure.Multiplayer.Events;
using KnowledgeAdventure.Multiplayer.Model;
using UnityEngine;

public class GauntletMMOClient : MonoBehaviour
{
	public static uint LOG_MASK = 128u;

	private static string LOBBY_EXT = "gsExt";

	private static string GAME_EXT = "GauntletGameExtension";

	private static float GAMEDATA_RELAY_INTERVAL = 0.2f;

	private const string USER_GENDER = "GSG";

	private GauntletMMOState mMMOState;

	private float mTimer;

	private GauntletRailShootManager mGameManager;

	private KAUiGauntletShooterHUD mGameHud;

	private int[] mLevelPieceData;

	private float mGameDataRelayTimer;

	private int mSelectedCourse = 1;

	private int mRelayedScore;

	private int mRelayedAccuracy;

	private List<GauntletMMOPlayer> mPlayers = new List<GauntletMMOPlayer>();

	private GauntletMMORoomType mRoomType;

	private IMMOClient mClient = MMOClientFactory.Instance;

	private bool mJoinGameRoomOnLogin;

	private static GauntletMMOClient mInstance = null;

	public List<GauntletMMOPlayer> pPlayers => mPlayers;

	public GauntletMMORoomType pRoomType
	{
		get
		{
			return mRoomType;
		}
		set
		{
			mRoomType = value;
		}
	}

	public IMMOClient pClient
	{
		get
		{
			return mClient;
		}
		set
		{
			mClient = value;
		}
	}

	public bool pJoinGameRoomOnLogin
	{
		get
		{
			return mJoinGameRoomOnLogin;
		}
		set
		{
			mJoinGameRoomOnLogin = value;
		}
	}

	public static GauntletMMOClient pInstance => mInstance;

	public GauntletMMOState pMMOState
	{
		get
		{
			return mMMOState;
		}
		set
		{
			mMMOState = value;
		}
	}

	public static void Init(GauntletMMORoomType inRoomType)
	{
		if (pInstance == null)
		{
			mInstance = new GameObject("GauntletMMOClient").AddComponent<GauntletMMOClient>();
			mInstance.RegisterEvents();
		}
		mInstance.mGameManager = GauntletRailShootManager.pInstance;
		mInstance.mGameHud = mInstance.mGameManager._HUDObject.GetComponent<KAUiGauntletShooterHUD>();
		mInstance.SetMMOState(GauntletMMOState.CONNECTING);
		mInstance.mGameManager.pGameType = GSGameType.HEADTOHEAD;
		mInstance.pRoomType = inRoomType;
		mInstance.pClient = MMOClientFactory.Instance;
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["GSG"] = (int)AvatarData.GetGender();
		mInstance.pClient.SetUserVariables(dictionary);
		if (!mInstance.pClient.IsConnected.Value)
		{
			string ipAdrress = GauntletRailShootManager.pInstance._DefaultMMOIPAddress;
			int serverPort = GauntletRailShootManager.pInstance._DefaultMMOPortNumber;
			if (ProductConfig.pInstance.MMOServer != null)
			{
				ipAdrress = ProductConfig.pInstance.MMOServer;
				serverPort = ProductConfig.pInstance.MMOServerPort.Value;
			}
			mInstance.pJoinGameRoomOnLogin = true;
			mInstance.Connect(ipAdrress, serverPort);
		}
		else if (MainStreetMMOClient.pInstance.pState == MMOClientState.IN_ROOM)
		{
			mInstance.AddUserToRoom();
		}
		else if (inRoomType == GauntletMMORoomType.JOIN_BUDDY)
		{
			mInstance.pJoinGameRoomOnLogin = true;
		}
	}

	private void Connect(string ipAdrress, int serverPort)
	{
		try
		{
			mClient.Connect(ipAdrress, serverPort);
		}
		catch (Exception ex)
		{
			UtDebug.LogError(ex.Message);
		}
	}

	public void RegisterEvents()
	{
		UtDebug.Log("Registering GauntletShooter events !!!", LOG_MASK);
		mClient.IsQueueMode = true;
		mClient.Initialize();
		mClient.IsDebugMode = false;
		mClient.Connected += OnConnectedDummy;
		mClient.Error += OnServerErrorDummy;
		mClient.UserJoinedRoom += OnUserJoinedRoomDummy;
		mClient.UserLeftRoom += OnUserLeftRoomDummy;
		mClient.ExtensionResponseReceived += OnServerMessageReceivedDummy;
		mClient.JoinedRoom += OnJoinedRoomDummy;
	}

	public void UnRegisterEvents()
	{
		UtDebug.Log("Unregistering GauntletShooter events!!!", LOG_MASK);
		mClient.Connected -= OnConnectedDummy;
		mClient.Error -= OnServerErrorDummy;
		mClient.UserJoinedRoom -= OnUserJoinedRoomDummy;
		mClient.UserLeftRoom -= OnUserLeftRoomDummy;
		mClient.ExtensionResponseReceived -= OnServerMessageReceivedDummy;
		mClient.JoinedRoom -= OnJoinedRoomDummy;
	}

	private void OnConnectedDummy(object sender, MMOConnectedEventArgs e)
	{
		UtDebug.Log("OnConnectedDummy : " + Time.frameCount);
		if (mInstance != null)
		{
			mInstance.OnConnected(sender, e);
		}
	}

	private void OnUserJoinedRoomDummy(object sender, MMOUserJoinedRoomEventArgs e)
	{
		UtDebug.Log("OnUserJoinedRoomDummy : " + Time.frameCount);
		if (mInstance != null)
		{
			mInstance.OnUserJoinedRoom(sender, e);
		}
	}

	private void OnServerErrorDummy(object sender, MMOErrorEventArgs e)
	{
		UtDebug.Log("OnServerErrorDummy : " + Time.frameCount);
		if (mInstance != null)
		{
			mInstance.OnServerError(sender, e);
		}
	}

	private void OnUserLeftRoomDummy(object sender, MMOUserLeftRoomEventArgs e)
	{
		UtDebug.Log("OnUserLeftRoomDummy : " + Time.frameCount);
		if (mInstance != null)
		{
			mInstance.OnUserLeftRoom(sender, e);
		}
	}

	private void OnServerMessageReceivedDummy(object sender, MMOExtensionResponseReceivedEventArgs e)
	{
		UtDebug.Log("OnServerMessageReceivedDummy : " + Time.frameCount);
		if (mInstance != null)
		{
			mInstance.OnServerMessageReceived(sender, e);
		}
	}

	private static void OnJoinedRoomDummy(object sender, MMOJoinedRoomEventArgs args)
	{
		UtDebug.Log("OnJoinedRoomDummy : " + Time.frameCount);
		if (mInstance != null)
		{
			mInstance.OnJoinedRoom(sender, args);
		}
	}

	private string GetOpponentName()
	{
		foreach (GauntletMMOPlayer mPlayer in mPlayers)
		{
			if (mPlayer._UserID != UserInfo.pInstance.UserID)
			{
				return mPlayer.pName;
			}
		}
		return "";
	}

	private string GetOpponentUserID()
	{
		foreach (GauntletMMOPlayer mPlayer in mPlayers)
		{
			if (mPlayer._UserID != UserInfo.pInstance.UserID)
			{
				return mPlayer._UserID;
			}
		}
		return "";
	}

	private void OnConnected(object sender, MMOConnectedEventArgs e)
	{
		SetMMOState(GauntletMMOState.JOINING_ROOM);
	}

	private void OnUserJoinedRoom(object sender, MMOUserJoinedRoomEventArgs e)
	{
		UtDebug.Log("UserJoinedRoom", LOG_MASK);
	}

	private void OnServerError(object sender, MMOErrorEventArgs e)
	{
		Debug.LogError("ServerError " + e.ErrorMessage + " " + e.ErrorType);
		if (mInstance != null && e.ErrorType == MMOErrorType.ConnectionError)
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			mGameManager.DisplayGenericDialog(base.gameObject, mGameManager._ConnectionErrorText._ID, mGameManager._ConnectionErrorText._Text, useDots: false, 1f, IsYesBtn: false, IsNoBtn: false, IsOKBtn: false, IsCloseBtn: false, isExclusiveUI: true);
			mGameManager.ShowMultiplayerMenu();
		}
	}

	private void OnUserLeftRoom(object sender, MMOUserLeftRoomEventArgs e)
	{
		UtDebug.Log("UserLeftRoom", LOG_MASK);
	}

	private void OnServerMessageReceived(object sender, MMOExtensionResponseReceivedEventArgs e)
	{
		try
		{
			string text = "";
			Dictionary<string, object> responseDataObject = e.ResponseDataObject;
			foreach (KeyValuePair<string, object> item in responseDataObject)
			{
				UtDebug.Log("---------- Data received from client: " + item.Value.ToString() + " ----------" + sender.ToString());
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			switch (responseDataObject["0"].ToString())
			{
			case "EM":
				if (responseDataObject["1"].ToString().Equals("JBRF"))
				{
					mGameManager.DisplayGenericDialog(base.gameObject, mGameManager._BuddyInviteFailedText._ID, mGameManager._BuddyInviteFailedText._Text, useDots: false, 1f, IsYesBtn: false, IsNoBtn: false, IsOKBtn: false, IsCloseBtn: false, isExclusiveUI: true);
					KAUICursorManager.SetDefaultCursor("Arrow");
					mGameManager._LevelSelectionScreen.SetInteractive(interactive: true);
					mGameManager._GauntletController._Pause = false;
					mGameManager.pIsGameRunning = false;
					mGameManager._MMOLobbyUI.SetVisibility(t: false);
					mGameManager._LevelSelectionScreen.SetVisibility(visible: true);
					SetMMOState(GauntletMMOState.NOT_CONNECTED);
				}
				break;
			case "UJR":
			{
				mSelectedCourse = int.Parse(responseDataObject["2"].ToString());
				mGameManager._MMOLobbyUI.SetCourseSelected(mSelectedCourse);
				int num2;
				for (num2 = 3; num2 < responseDataObject.Count; num2++)
				{
					text = responseDataObject[num2.ToString()].ToString();
					num2++;
					bool isReady2 = bool.Parse(responseDataObject[num2.ToString()].ToString());
					num2++;
					int inGender2 = int.Parse(responseDataObject[num2.ToString()].ToString());
					GauntletMMOPlayer mMOPlayer = GetMMOPlayer(text);
					if (mMOPlayer == null)
					{
						mMOPlayer = GauntletMMOPlayer.CreatePlayer(text, inGender2);
						mMOPlayer._IsReady = isReady2;
						mPlayers.Add(mMOPlayer);
						mGameManager._MMOLobbyUI.OnUserJoined(mMOPlayer);
					}
				}
				mGameManager._MMOLobbyUI.LockRedPlanetLevel(!SubscriptionInfo.pIsMember);
				if (!SubscriptionInfo.pIsMember && mPlayers.Count == 1)
				{
					mSelectedCourse = 1;
					mGameManager._MMOLobbyUI.SetCourseSelected(mSelectedCourse);
					pInstance.OnCourseChanged(mSelectedCourse);
				}
				if (mPlayers.Count == 1)
				{
					mGameManager._MMOLobbyUI.SetLobbyMessage(mGameManager._WaitForOthersText._Text, mGameManager._WaitForOthersText._ID);
				}
				else
				{
					UpdateReadyStatus(null, isReady: false);
				}
				mGameManager._MMOLobbyUI.ShowInviteBtn(mRoomType == GauntletMMORoomType.HOST_FOR_BUDDY && mPlayers.Count == 1);
				break;
			}
			case "ULR":
				text = responseDataObject["2"].ToString();
				if (pMMOState == GauntletMMOState.IN_ROOM && (pRoomType == GauntletMMORoomType.JOIN_ANY || pRoomType == GauntletMMORoomType.JOIN_BUDDY))
				{
					DestroyMMO(inLogout: true);
					mGameManager.DisplayGenericDialog(base.gameObject, mGameManager._OwnerLeftErrorText._ID, mGameManager._OwnerLeftErrorText._Text, useDots: false, 1f, IsYesBtn: false, IsNoBtn: false, IsOKBtn: false, IsCloseBtn: false, isExclusiveUI: true);
					mGameManager._LevelSelectionScreen.SetInteractive(interactive: true);
					mGameManager._MMOLobbyUI.SetVisibility(t: false);
					mGameManager._LevelSelectionScreen.SetVisibility(visible: true);
					break;
				}
				foreach (GauntletMMOPlayer mPlayer in mPlayers)
				{
					if (mPlayer._UserID == text)
					{
						if (pMMOState != GauntletMMOState.GAME_COMPLETED && pMMOState == GauntletMMOState.IN_GAME)
						{
							StartCoroutine(mGameHud.ShowOpponentLeftMessage());
						}
						mPlayer.DestroyMe();
						mPlayers.Remove(mPlayer);
						break;
					}
				}
				mGameManager._MMOLobbyUI.ShowInviteBtn(mRoomType == GauntletMMORoomType.HOST_FOR_BUDDY && mPlayers.Count == 1);
				break;
			case "LUR":
				text = responseDataObject["2"].ToString();
				UpdateReadyStatus(text, isReady: true);
				break;
			case "LUNR":
				text = responseDataObject["2"].ToString();
				UpdateReadyStatus(text, isReady: false);
				break;
			case "RLCD":
			{
				int courseSelected = int.Parse(responseDataObject["2"].ToString());
				mGameManager._MMOLobbyUI.SetCourseSelected(courseSelected);
				break;
			}
			case "LCDU":
			{
				SetMMOState(GauntletMMOState.ROOM_COUNTDOWN);
				int num6 = int.Parse(responseDataObject["2"].ToString());
				if (num6 <= 3)
				{
					mGameManager._MMOLobbyUI.LockLobby();
				}
				string stringData = StringTable.GetStringData(mGameManager._LobbyCountDownText._ID, mGameManager._LobbyCountDownText._Text);
				string stringData2 = StringTable.GetStringData(mGameManager._SecondsText._ID, mGameManager._SecondsText._Text);
				mGameManager._MMOLobbyUI.SetLobbyMessage(stringData + num6 + " " + stringData2, 0);
				break;
			}
			case "LCDP":
				SetMMOState(GauntletMMOState.IN_ROOM);
				mGameManager._MMOLobbyUI.SetState(KAUIState.INTERACTIVE);
				UpdateReadyStatus(null, isReady: false);
				break;
			case "LCDD":
				text = responseDataObject["2"].ToString();
				if (!(text == UserInfo.pInstance.UserID))
				{
					break;
				}
				mLevelPieceData = mGameManager.GenerateLevelByType(GSGameType.TRAINING);
				if (mLevelPieceData != null)
				{
					for (int j = 0; j < mLevelPieceData.Length; j++)
					{
						dictionary.Add(j.ToString(), mLevelPieceData[j].ToString());
					}
					SendExtensionMessageToRoom(GAME_EXT, "GLL", dictionary);
					mGameManager.GenerateLevel(mLevelPieceData, GSGameType.TRAINING);
					mGameManager._MMOLobbyUI.SetVisibility(t: false);
					mGameHud.SetVisibility(t: false);
				}
				break;
			case "GLL":
			{
				mLevelPieceData = new int[responseDataObject.Count - 2];
				for (int i = 2; i < responseDataObject.Count; i++)
				{
					mLevelPieceData[i - 2] = int.Parse(responseDataObject[i.ToString()].ToString());
				}
				mGameManager.GenerateLevel(mLevelPieceData, GSGameType.TRAINING);
				mGameManager._MMOLobbyUI.SetVisibility(t: false);
				mGameHud.SetVisibility(t: false);
				SendExtensionMessageToRoom(GAME_EXT, "GLLD", dictionary);
				break;
			}
			case "GCDS":
				mGameManager._GauntletController._Pause = false;
				SetMMOState(GauntletMMOState.GAME_COUNTDOWN_STARTED);
				mGameHud.ShowGameEndTimer(isShow: false, 0);
				mGameHud.SetVisibility(t: true);
				mGameManager._MMOLobbyUI.SetState(KAUIState.INTERACTIVE);
				mGameManager._MMOLobbyUI.SetVisibility(t: false);
				mGameHud.SetOpponentName(GetOpponentName());
				mGameHud.PlayCountdown(int.Parse(responseDataObject["2"].ToString()));
				mGameManager._MMOLobbyUI._BuddySelectScreen.SetVisibility(t: false);
				break;
			case "GCDU":
				mGameHud.PlayCountdown(int.Parse(responseDataObject["2"].ToString()));
				break;
			case "GS":
				SetMMOState(GauntletMMOState.IN_GAME);
				mGameHud.StopCountdown();
				mRelayedScore = 0;
				mRelayedAccuracy = 0;
				break;
			case "RGD":
				mGameHud.SetOpponentScore(int.Parse(responseDataObject["2"].ToString()), int.Parse(responseDataObject["3"].ToString()));
				break;
			case "GC":
			{
				mGameHud.ShowWaitingForGameEndMessage(Visibility: false);
				SetMMOState(GauntletMMOState.GAME_COMPLETED);
				AvAvatar.pObject = mGameManager.pOldAvatarObject;
				List<GauntletMMOPlayer> list = new List<GauntletMMOPlayer>();
				int num3;
				for (num3 = 2; num3 < responseDataObject.Count; num3++)
				{
					text = responseDataObject[num3.ToString()].ToString();
					int num4 = int.Parse(responseDataObject[(num3 + 1).ToString()].ToString());
					int num5 = int.Parse(responseDataObject[(num3 + 2).ToString()].ToString());
					int inGender3 = int.Parse(responseDataObject[(num3 + 3).ToString()].ToString());
					GauntletMMOPlayer gauntletMMOPlayer2 = GauntletMMOPlayer.CreatePlayer(text, inGender3);
					gauntletMMOPlayer2._Score = num4 + mGameManager.GetAccuracyScore(num5, num4);
					gauntletMMOPlayer2._Accuracy = num5;
					list.Add(gauntletMMOPlayer2);
					num3 += 3;
				}
				mGameHud.SetVisibility(t: false);
				mGameManager.pIsGameRunning = false;
				mGameManager._GauntletController._Pause = true;
				mGameManager.SetResults(list.ToArray());
				UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
				mGameManager.SetCursorVisible(isVisible: true);
				break;
			}
			case "GECDU":
				SetMMOState(GauntletMMOState.FINAL_COUNTDOWN);
				mGameHud.ShowGameEndTimer(isShow: true, int.Parse(responseDataObject["2"].ToString()));
				break;
			case "PA":
			{
				mGameManager.OnPlayAgain();
				mSelectedCourse = int.Parse(responseDataObject["2"].ToString());
				mGameManager._MMOLobbyUI.SetCourseSelected(mSelectedCourse);
				int num;
				for (num = 3; num < responseDataObject.Count; num++)
				{
					text = responseDataObject[num.ToString()].ToString();
					num++;
					bool isReady = bool.Parse(responseDataObject[num.ToString()].ToString());
					num++;
					int inGender = int.Parse(responseDataObject[num.ToString()].ToString());
					GauntletMMOPlayer gauntletMMOPlayer = GetMMOPlayer(text);
					if (gauntletMMOPlayer == null)
					{
						gauntletMMOPlayer = GauntletMMOPlayer.CreatePlayer(text, inGender);
						mPlayers.Add(gauntletMMOPlayer);
					}
					mGameManager._MMOLobbyUI.OnUserJoined(gauntletMMOPlayer);
					gauntletMMOPlayer._IsReady = isReady;
					mGameManager._MMOLobbyUI.OnUserJoined(gauntletMMOPlayer);
				}
				if (mPlayers.Count == 1)
				{
					mGameManager._MMOLobbyUI.SetLobbyMessage(mGameManager._WaitForOthersText._Text, mGameManager._WaitForOthersText._ID);
				}
				else
				{
					UpdateReadyStatus(null, isReady: false);
				}
				mGameManager._MMOLobbyUI.ShowInviteBtn(mRoomType == GauntletMMORoomType.HOST_FOR_BUDDY && mPlayers.Count == 1);
				mGameManager.PayByCoinsForMultiplayerAgain();
				break;
			}
			}
		}
		catch (Exception ex)
		{
			UtDebug.LogError("Exception caught!!! " + ex.ToString());
		}
	}

	public void SendExtensionMessageToRoom(string inExtension, string inMessage, Dictionary<string, object> inParams)
	{
		if (inParams == null)
		{
			inParams = new Dictionary<string, object>();
		}
		mClient.SendExtensionMessage(inExtension, "gs." + inMessage, inParams, mClient.LastJoinedRoom, useUDP: false);
	}

	public void SendExtMessage(string inCommand, string inType, Dictionary<string, object> d)
	{
		mClient.SendExtensionMessage("", "gs." + inType, d);
	}

	private GauntletMMOPlayer GetMMOPlayer(string inUserID)
	{
		foreach (GauntletMMOPlayer mPlayer in mPlayers)
		{
			if (mPlayer._UserID == inUserID)
			{
				return mPlayer;
			}
		}
		return null;
	}

	private void OnJoinedRoom(object sender, MMOJoinedRoomEventArgs args)
	{
		if (args == null)
		{
			UtDebug.Log("Joined room, but args is null");
		}
		else if (pMMOState >= GauntletMMOState.GAME_COUNTDOWN_STARTED)
		{
			UtDebug.Log("Game is already in progress" + sender.ToString());
		}
		else if (!args.RoomJoined.RoomName.StartsWith("GauntletDO"))
		{
			if (mJoinGameRoomOnLogin)
			{
				AddUserToRoom();
			}
		}
		else
		{
			SetMMOState(GauntletMMOState.IN_ROOM);
			mGameManager.OnJoinedRoom();
			mGameManager._MMOLobbyUI.DisableInvite(mRoomType != GauntletMMORoomType.HOST_FOR_BUDDY);
			mGameManager._MMOLobbyUI.DisableCourseSelection(mRoomType != GauntletMMORoomType.HOST_FOR_BUDDY && mRoomType != GauntletMMORoomType.HOST_FOR_ANY);
		}
	}

	public void AddUserToRoom()
	{
		switch (mRoomType)
		{
		case GauntletMMORoomType.JOIN_ANY:
			SendExtMessage(LOBBY_EXT, "JAR", null);
			break;
		case GauntletMMORoomType.JOIN_BUDDY:
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("0", mGameManager.pHostPlayer);
			SendExtMessage(LOBBY_EXT, "JBR", dictionary);
			break;
		}
		case GauntletMMORoomType.HOST_FOR_ANY:
			SendExtMessage(LOBBY_EXT, "HRFA", null);
			break;
		case GauntletMMORoomType.HOST_FOR_BUDDY:
			SendExtMessage(LOBBY_EXT, "HRFB", null);
			break;
		}
		SetMMOState(GauntletMMOState.JOINING_ROOM);
	}

	public void DestroyMMO(bool inLogout)
	{
		if (mClient == null)
		{
			return;
		}
		if (inLogout)
		{
			mClient.Logout(forcelogout: false);
		}
		UnRegisterEvents();
		if (!inLogout)
		{
			mClient.SendExtensionMessage("le", "JA", null);
		}
		foreach (GauntletMMOPlayer mPlayer in mPlayers)
		{
			mPlayer.DestroyMe();
		}
		mPlayers.Clear();
		UnityEngine.Object.Destroy(base.gameObject);
		mInstance = null;
	}

	private void OnApplicationQuit()
	{
		if (mGameManager != null)
		{
			mGameManager.ProcessUserLeft();
		}
		UnRegisterEvents();
		DisconnectServer();
	}

	public void DisconnectServer()
	{
		UtDebug.Log("Client logging out!!", LOG_MASK);
		mClient.Disconnect();
	}

	public void SetMMOState(GauntletMMOState inState)
	{
		if (pMMOState != inState)
		{
			pMMOState = inState;
			if (pMMOState == GauntletMMOState.CONNECTING)
			{
				mTimer = GauntletRailShootManager.pInstance._ConnectionTimeout;
			}
		}
	}

	public void Update()
	{
		if (mTimer > 0f)
		{
			mTimer -= Time.deltaTime;
			if (mTimer <= 0f && (pMMOState == GauntletMMOState.CONNECTING || pMMOState == GauntletMMOState.JOINING_ROOM))
			{
				mGameManager.DisplayGenericDialog(base.gameObject, mGameManager._ConnectionTimeOutText._ID, mGameManager._ConnectionTimeOutText._Text, useDots: false, 1f, IsYesBtn: false, IsNoBtn: false, IsOKBtn: false, IsCloseBtn: false, isExclusiveUI: true);
				DestroyMMO(inLogout: true);
				KAUICursorManager.SetDefaultCursor("Arrow");
				mGameManager._LevelSelectionScreen.SetInteractive(interactive: true);
				mGameManager._MultiplayerMenuScreen.SetInteractive(interactive: true);
			}
		}
		if (pMMOState == GauntletMMOState.IN_GAME)
		{
			mGameDataRelayTimer -= Time.deltaTime;
			int score = mGameHud.GetScore();
			int num = (int)mGameManager.pAccuracy;
			if (mGameDataRelayTimer <= 0f && (score != mRelayedScore || num != mRelayedAccuracy))
			{
				mRelayedScore = score;
				mRelayedAccuracy = num;
				mGameDataRelayTimer = GAMEDATA_RELAY_INTERVAL;
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				dictionary.Add("0", score);
				dictionary.Add("1", num);
				SendExtensionMessageToRoom(GAME_EXT, "RGD", dictionary);
			}
		}
	}

	public void OnPlayerReady()
	{
		UpdateReadyStatus(UserInfo.pInstance.UserID, isReady: true);
		SendExtensionMessageToRoom(GAME_EXT, "LUR", null);
	}

	public void OnPlayerNotReady()
	{
		UpdateReadyStatus(UserInfo.pInstance.UserID, isReady: false);
		SendExtensionMessageToRoom(GAME_EXT, "LUNR", null);
	}

	public void UpdateReadyStatus(string inUserID, bool isReady)
	{
		int num = 0;
		bool flag = false;
		foreach (GauntletMMOPlayer mPlayer in mPlayers)
		{
			if (mPlayer._UserID == inUserID)
			{
				mPlayer._IsReady = isReady;
				mGameManager._MMOLobbyUI.UpdateReadyStatus(mPlayer);
			}
			if (mPlayer._IsReady)
			{
				num++;
				if (mPlayer._UserID == UserInfo.pInstance.UserID)
				{
					flag = true;
				}
			}
		}
		if (mPlayers.Count > 1 && mPlayers.Count - 1 == num && !flag)
		{
			mGameManager._MMOLobbyUI.SetLobbyMessage(mGameManager._ClickReadyMsgText._Text, mGameManager._ClickReadyMsgText._ID);
		}
		else
		{
			mGameManager._MMOLobbyUI.SetLobbyMessage(mGameManager._WaitForOthersText._Text, mGameManager._WaitForOthersText._ID);
		}
	}

	public void OnGameComplete()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("0", mGameHud.GetScore());
		dictionary.Add("1", ((int)mGameManager.pAccuracy).ToString());
		SendExtensionMessageToRoom(GAME_EXT, "GC", dictionary);
		mGameHud.ShowWaitingForGameEndMessage(Visibility: true);
	}

	public void OnCourseChanged(int intype)
	{
		mSelectedCourse = intype;
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("0", mSelectedCourse);
		SendExtensionMessageToRoom(GAME_EXT, "RLCD", dictionary);
	}

	public void PlayGameAgain()
	{
		SendExtensionMessageToRoom(GAME_EXT, "PA", null);
	}
}
