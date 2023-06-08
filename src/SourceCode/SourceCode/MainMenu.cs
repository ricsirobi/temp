using System;
using System.Collections.Generic;
using KnowledgeAdventure.Multiplayer.Events;
using KnowledgeAdventure.Multiplayer.Model;
using SOD.Event;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
	public string _PrefetchList = "RS_DATA/RacingPrefetchList.xml";

	public GameModeData[] _GameModeData;

	public TrackData[] _TrackData;

	public List<EventTrackData> _EventTrackData;

	public RaceTypeData[] _RaceTypeData;

	public UiRacingMultiplayer _MultiplayerMainUi;

	public UiRacingSingleplayer _RaceSelectUi;

	public UiRacingLobby _LobbyUi;

	public UiRacingEquip _EquipMenu;

	public LocaleString _LobbyTransitionText = new LocaleString("You are joining a game room");

	public GameObject _LobbyPhotoManager;

	public PetPictureData[] _PictureData;

	public float _LobbyLockedTime = 5f;

	public LocaleString _NotEnoughLobbyPlayerText = new LocaleString("You are the only player in Lobby. You need more players!");

	public LocaleString _PoorNetworkConnectionText = new LocaleString("Your network connection seems slow. Please try again.");

	public string _LastLevelToLoad = "HubSchoolDO";

	public string _ExitMarkerName = "PfMarker_RacingExit";

	private bool mIsSendingInvite;

	private bool mIsInviteStep;

	private static GameObject mUiGenericDB;

	private bool mLevelReady;

	private int mCurrentTrackSelectionIndex = -1;

	private int mCurrentGameModeSelectionIndex = -1;

	public int _TrackStoreId = 77;

	public int mCashTotal;

	public BuyItem _TrackToBuy = new BuyItem();

	private StoreData mStoreData;

	public ItemData _SelectedTrackItemData;

	private RacingMMOClient mRacingMMOClient;

	private const string USER_LEFT = "UL";

	private const string TRACK_ID = "TID";

	private const string LOBBY_TIME = "LT";

	private const string JOIN_ROOM = "JR";

	private const string PAUSE_STATE = "PS";

	private const string PLAYER_READY_STATE = "PS";

	private const string SCENE_TRANSITION = "ST";

	private const string LOADING_STATUS = "LS";

	private const string ROOMID = "RID";

	private KAUIGenericDB mKAUIGenericDB;

	private bool mNotEnoughLobbyPlayer;

	private bool mIsPrefetchDone;

	private AvPhotoManager mPhotoManager;

	private Dictionary<string, Texture2D> mPictureCache = new Dictionary<string, Texture2D>();

	private bool mNetworkError;

	private bool mShowLobby;

	public float _DisplayTime = 2f;

	public float _ReadyStateUpdateFrequency = 10f;

	private float mUpdatePlayerState;

	private bool mIsPrefetchStarted;

	public List<TrackData> pActiveTrackData { get; private set; }

	public RacingMMOClient pRacingMMOClient => mRacingMMOClient;

	public TrackData pCurrentTrackData
	{
		get
		{
			if (mCurrentTrackSelectionIndex != -1)
			{
				return pActiveTrackData[mCurrentTrackSelectionIndex % pActiveTrackData.Count];
			}
			return null;
		}
	}

	public GameModeData pCurrentGameModeData
	{
		get
		{
			if (mCurrentGameModeSelectionIndex != -1)
			{
				return _GameModeData[mCurrentGameModeSelectionIndex];
			}
			return null;
		}
	}

	public void Start()
	{
		if (!string.IsNullOrEmpty(_PrefetchList))
		{
			PrefetchManager.Init(ignoreGetAssetVersion: true, _PrefetchList);
		}
		UiChatHistory._IsVisible = false;
		mRacingMMOClient?.Destroy();
		mRacingMMOClient = RacingMMOClient.Init(this);
		mRacingMMOClient.OnReponseEvent += ExtensionResponse;
		mRacingMMOClient.OnMessageEvent += MessageResponse;
		_MultiplayerMainUi.SetVisibility(inVisible: false);
		_RaceSelectUi.SetVisibility(inVisible: false);
		_LobbyUi.SetVisibility(inVisible: false);
		_EquipMenu.SetVisibility(inVisible: false);
		MissionManager.pInstance?.SetTimedTaskUpdate(inState: false);
		MainStreetMMOClient.pInstance?.SetJoinAllowed(MMOJoinStatus.NOT_ALLOWED, forceUpdate: true);
		if (!string.IsNullOrEmpty(MainStreetMMOClient.pInstance?.GetOwnerIDForCurrentLevel()))
		{
			mIsSendingInvite = true;
			mIsInviteStep = true;
			return;
		}
		pActiveTrackData = new List<TrackData>(Array.FindAll(_TrackData, (TrackData x) => x._Visible));
		EventManager eventManager = EventManager.GetActiveEvent();
		EventTrackData eventTrackData = _EventTrackData.Find((EventTrackData t) => t._EventName == eventManager?._EventName);
		bool flag = false;
		if ((bool)eventManager)
		{
			flag = eventManager.EventInProgress() && !eventManager.GracePeriodInProgress();
		}
		if (flag && eventTrackData != null && eventTrackData._TrackData.Count > 0)
		{
			pActiveTrackData = eventTrackData._TrackData;
		}
		AvAvatar.pStartLocation = _ExitMarkerName;
	}

	private void UpdateUser(string userID, bool isReady)
	{
		if (!mShowLobby && null != _MultiplayerMainUi && _MultiplayerMainUi.GetVisibility())
		{
			_MultiplayerMainUi.SetPlayerState(userID, isReady);
		}
		else if (_LobbyUi != null && (mShowLobby || _LobbyUi.GetVisibility()))
		{
			_LobbyUi.SetPlayerState(userID, isReady);
		}
	}

	private void MessageResponse(MMOMessageReceivedEventArgs args)
	{
		if (!mNetworkError && args.MMOMessage.MessageType == MMOMessageType.User)
		{
			string[] array = args.MMOMessage.MessageText.Split(':');
			if (array[0] == "IMR")
			{
				SetPlayerReady(array[1], array[2]);
			}
		}
	}

	private void ExtensionResponse(MMOExtensionResponseReceivedEventArgs args)
	{
		if (mNetworkError)
		{
			return;
		}
		switch (args.ResponseDataObject["2"].ToString())
		{
		case "TID":
		{
			string gameMode = args.ResponseDataObject["3"].ToString();
			string s2 = args.ResponseDataObject["4"].ToString();
			string s3 = args.ResponseDataObject["5"].ToString();
			SetRaceInfo(gameMode, int.Parse(s2), int.Parse(s3));
			break;
		}
		case "PS":
		{
			Dictionary<string, MMOAvatar> pPlayerList = MainStreetMMOClient.pInstance.pPlayerList;
			int count = args.ResponseDataObject.Count;
			if (pPlayerList.Count <= 0 || count <= 3)
			{
				break;
			}
			List<string> list = new List<string>();
			for (int i = 3; i < count; i++)
			{
				list.Add((string)args.ResponseDataObject[i.ToString()]);
			}
			{
				foreach (KeyValuePair<string, MMOAvatar> item in pPlayerList)
				{
					if (list.Contains(item.Key))
					{
						UpdateUser(item.Value.pUserID, isReady: true);
					}
					else
					{
						UpdateUser(item.Value.pUserID, isReady: false);
					}
				}
				break;
			}
		}
		case "JR":
			MoveNext(GameScreen.GAME_HOST_JOIN_SCREEN);
			break;
		case "LT":
		{
			string s = args.ResponseDataObject["3"].ToString();
			UpdateTime(int.Parse(s));
			break;
		}
		case "ST":
			mRacingMMOClient.mLoadingRaceScene = true;
			StartGame();
			break;
		}
	}

	private void InitUi()
	{
		RsResourceManager.DestroyLoadScreen();
		if (RacingManager.pIsSinglePlayer || !MainStreetMMOClient.pIsMMOEnabled)
		{
			if (!RacingManager.pIsSinglePlayer)
			{
				RacingManager.pIsSinglePlayer = true;
			}
			if (UiChatHistory._IsVisible)
			{
				UiChatHistory._IsVisible = false;
			}
			_RaceSelectUi.EnableUi(this);
			_RaceSelectUi.SetState(KAUIState.INTERACTIVE);
		}
		else
		{
			_MultiplayerMainUi.EnableUi(this);
			_MultiplayerMainUi.SetState(KAUIState.INTERACTIVE);
		}
	}

	public void SetCurrentGameMode(GameModes gamemode)
	{
		int num = 0;
		GameModeData[] gameModeData = _GameModeData;
		for (int i = 0; i < gameModeData.Length; i++)
		{
			if (gameModeData[i]._GameMode == gamemode)
			{
				mCurrentGameModeSelectionIndex = num;
				break;
			}
			num++;
		}
	}

	public void SetCurrentSelectionIndex(int trackId)
	{
		mCurrentTrackSelectionIndex = trackId;
		if (_RaceSelectUi.enabled && _RaceSelectUi.GetVisibility())
		{
			_RaceSelectUi.UpdateTrackInfo();
		}
	}

	public void SetPlayerReady(string userId, string isReady)
	{
		if (!mShowLobby && (bool)_MultiplayerMainUi && _MultiplayerMainUi.GetVisibility())
		{
			_MultiplayerMainUi.SetPlayerState(userId, (isReady == "True") ? true : false);
		}
		else if ((bool)_LobbyUi && (mShowLobby || _LobbyUi.GetVisibility()))
		{
			_LobbyUi.SetPlayerState(userId, (isReady == "True") ? true : false);
		}
	}

	private void OnDestroy()
	{
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: true);
		}
	}

	private void OnLevelReady()
	{
		mLevelReady = true;
		AvAvatar.pState = AvAvatarState.PAUSED;
		CheckPrefetch();
	}

	private void OnPrefetchDone()
	{
		if (!mIsInviteStep)
		{
			InitUi();
		}
		if (ChallengeInfo.pActiveChallenge != null)
		{
			RacingManager.pIsSinglePlayer = true;
			mCurrentTrackSelectionIndex = pActiveTrackData.IndexOf(pActiveTrackData.Find((TrackData t) => t._GameLevelID == ChallengeInfo.pActiveChallenge.ChallengeGameInfo.GameLevelID));
			MainStreetMMOClient.pInstance.pNotifyObjOnIdlePopUpClose = null;
			MainStreetMMOClient.pInstance.Disconnect();
			SetCurrentSelectionIndex(mCurrentTrackSelectionIndex);
			StartGame();
		}
		mIsInviteStep = false;
	}

	private void CheckPrefetch()
	{
		if (!mIsPrefetchDone)
		{
			if (!mIsPrefetchStarted && PrefetchManager.pIsVersionDownloadComplete)
			{
				mIsPrefetchStarted = true;
				PrefetchManager.StartPrefetch();
				PrefetchManager.ShowDownloadProgress();
				UICursorManager.pVisibility = false;
			}
			if (mIsPrefetchStarted && !mIsPrefetchDone && PrefetchManager.pInstance != null && PrefetchManager.pIsReady)
			{
				mIsPrefetchDone = true;
				PrefetchManager.Kill();
				OnPrefetchDone();
				UICursorManager.pVisibility = true;
			}
		}
	}

	private void Update()
	{
		if (mNetworkError)
		{
			return;
		}
		if (mLevelReady)
		{
			if (mIsSendingInvite)
			{
				string ownerIDForCurrentLevel = MainStreetMMOClient.pInstance.GetOwnerIDForCurrentLevel();
				if (MainStreetMMOClient.pInstance.pState == MMOClientState.IN_ROOM && !string.IsNullOrEmpty(ownerIDForCurrentLevel))
				{
					mIsSendingInvite = false;
					UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
					SendVisitMessage(ownerIDForCurrentLevel);
					return;
				}
			}
			CheckPrefetch();
			if (MainStreetMMOClient.pIsReady && pRacingMMOClient != null && Time.time - mUpdatePlayerState > _ReadyStateUpdateFrequency)
			{
				pRacingMMOClient.mReceivedPlayerStates = false;
				mUpdatePlayerState = Time.time;
			}
		}
		if (AvAvatar.mNetworkVelocity != Vector3.zero)
		{
			AvAvatar.mNetworkVelocity = Vector3.zero;
		}
		if (MainStreetMMOClient.pIsReady && MMOTimeManager.pInstance != null && MMOTimeManager.pInstance.pPingCount > 0 && MMOTimeManager.pInstance.GetLastLocalTimeDifference() > 20.0)
		{
			mNetworkError = true;
			if (_LobbyUi.GetVisibility())
			{
				MainStreetMMOClient.pInstance.Logout();
			}
			ShowKAUIDialog("PfKAUIGenericDBSm", "High Ping", "", "", "", "ConfirmYes", destroyDB: true, _PoorNetworkConnectionText, _MultiplayerMainUi.gameObject);
		}
	}

	private void SendVisitMessage(string ownerID)
	{
		mRacingMMOClient.SendVisitMessage(ownerID);
	}

	public void SendJoinData(int inRoomId, RaceType inRaceType, Themes inTheme, int inThemeIdx, bool inIsPublic, int inMinPlayer)
	{
		mRacingMMOClient.SendJoinData(inRoomId, (int)inRaceType, 0, 0, inThemeIdx, inIsPublic, inMinPlayer);
	}

	public void MoveNext(GameScreen gameScreen)
	{
		switch (gameScreen)
		{
		case GameScreen.GAME_MODE_SCREEN:
			if (RacingManager.pIsSinglePlayer)
			{
				_RaceSelectUi.EnableUi(this);
			}
			else
			{
				_MultiplayerMainUi.EnableUi(this);
			}
			break;
		case GameScreen.GAME_THEME_SCREEN:
			_RaceSelectUi.EnableUi(this);
			break;
		case GameScreen.GAME_HOST_JOIN_SCREEN:
			_MultiplayerMainUi.enabled = true;
			_MultiplayerMainUi.EnableUi(this);
			mNotEnoughLobbyPlayer = false;
			break;
		case GameScreen.GAME_LOBBY_SCREEN:
			if (!mShowLobby)
			{
				mShowLobby = true;
				_MultiplayerMainUi.LobbyTransition();
				mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
				mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: false);
				mKAUIGenericDB.SetTextByID(_LobbyTransitionText._ID, _LobbyTransitionText._Text, interactive: false);
				KAUI.SetExclusive(mKAUIGenericDB);
				Invoke("DisplayLobbyUI", _DisplayTime);
			}
			break;
		}
	}

	private void DisplayLobbyUI()
	{
		mShowLobby = false;
		UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
		_MultiplayerMainUi.enabled = false;
		_MultiplayerMainUi.SetVisibility(inVisible: false);
		UiChatHistory._IsVisible = false;
		if (_MultiplayerMainUi._ChatWindow != null)
		{
			_MultiplayerMainUi._ChatWindow.SetActive(value: false);
		}
		if (!_LobbyUi.enabled)
		{
			_LobbyUi.enabled = true;
		}
		if (mCurrentTrackSelectionIndex < 0)
		{
			mCurrentTrackSelectionIndex = 0;
		}
		_LobbyUi.EnableUi(this);
	}

	public void StartGame()
	{
		if (!mNotEnoughLobbyPlayer)
		{
			UiChatHistory._IsVisible = false;
			if (_LobbyUi._ChatWindow != null)
			{
				_LobbyUi._ChatWindow.SetActive(value: false);
			}
			ClearPictureCache();
			RacingManager.MarkTransitionTime();
			RsResourceManager.LoadLevel(pCurrentTrackData._TrackSceneName);
		}
	}

	private void OnDisable()
	{
		mRacingMMOClient.OnReponseEvent -= ExtensionResponse;
		mRacingMMOClient.OnMessageEvent -= MessageResponse;
		mRacingMMOClient.Destroy();
	}

	private void OnClose()
	{
		UnityEngine.Object.Destroy(mUiGenericDB);
		mUiGenericDB = null;
		_MultiplayerMainUi.SetInteractive(interactive: true);
	}

	public void SetRaceInfo(string gameMode, int trackId, int themeId)
	{
		mCurrentTrackSelectionIndex = trackId;
		mCurrentGameModeSelectionIndex = themeId;
	}

	public void AddPlayer(MMOAvatar avatar)
	{
		if (_LobbyUi != null && (mShowLobby || _LobbyUi.GetVisibility()))
		{
			_LobbyUi.AddPlayer(avatar);
		}
		else if (!mShowLobby && null != _MultiplayerMainUi)
		{
			_MultiplayerMainUi.AddPlayerToList(avatar);
		}
	}

	public void RemovePlayer(MMOAvatar avatar)
	{
		if (_LobbyUi != null && _LobbyUi.GetVisibility())
		{
			_LobbyUi.RemovePlayer(avatar);
		}
		else if (_MultiplayerMainUi != null && _MultiplayerMainUi.GetVisibility())
		{
			_MultiplayerMainUi.RemovePlayer(avatar);
		}
		else
		{
			UtDebug.Log("REMOVING PLAYER FAILED");
		}
	}

	public void ShowLobby()
	{
		MoveNext(GameScreen.GAME_LOBBY_SCREEN);
	}

	public void UpdateTime(int count)
	{
		if (_LobbyUi.GetVisibility())
		{
			if (mNotEnoughLobbyPlayer)
			{
				return;
			}
			if ((float)count <= _LobbyLockedTime && _LobbyUi.pPlayers.Count < 1 && !mNotEnoughLobbyPlayer)
			{
				mNotEnoughLobbyPlayer = true;
				DestroyKAUIDB();
				ShowKAUIDialog("PfKAUIGenericDBSm", "NotEnoughPlayer", string.Empty, string.Empty, "OnNoPlayerOk", string.Empty, destroyDB: true, _NotEnoughLobbyPlayerText);
			}
			_LobbyUi.UpdateTime(count);
		}
		else
		{
			mNotEnoughLobbyPlayer = false;
		}
		if (_EquipMenu != null && _EquipMenu.GetVisibility())
		{
			_EquipMenu.UpdateTime(count);
		}
	}

	private void OnNoPlayerOk()
	{
		mNotEnoughLobbyPlayer = false;
		_LobbyUi.ConfirmYes();
	}

	public void ShowKAUIDialog(string assetName, string dbName, string yesMessage, string noMessage, string okMessage, string closeMessage, bool destroyDB, LocaleString localeString, GameObject msgObject = null)
	{
		if (mKAUIGenericDB != null)
		{
			DestroyKAUIDB();
		}
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB(assetName, dbName);
		if (mKAUIGenericDB != null)
		{
			if (msgObject == null)
			{
				msgObject = base.gameObject;
			}
			mKAUIGenericDB.SetMessage(msgObject, yesMessage, noMessage, okMessage, closeMessage);
			mKAUIGenericDB.SetDestroyOnClick(destroyDB);
			mKAUIGenericDB.SetButtonVisibility(!string.IsNullOrEmpty(yesMessage), !string.IsNullOrEmpty(noMessage), !string.IsNullOrEmpty(okMessage), !string.IsNullOrEmpty(closeMessage));
			mKAUIGenericDB.SetTextByID(localeString._ID, localeString._Text, interactive: false);
			KAUI.SetExclusive(mKAUIGenericDB);
		}
	}

	public void DestroyKAUIDB()
	{
		if (!(mKAUIGenericDB == null))
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			UnityEngine.Object.DestroyImmediate(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}

	private void GetTrackData()
	{
		ItemStoreDataLoader.Load(_TrackStoreId, OnStoreLoaded);
	}

	private void OnStoreLoaded(StoreData sd)
	{
		mStoreData = sd;
	}

	public int IsUnlocked(int trackId)
	{
		int result = -1;
		_SelectedTrackItemData = null;
		if (mStoreData == null)
		{
			return result;
		}
		for (int i = 0; i < mStoreData._Items.Length; i++)
		{
			if (mStoreData._Items[i].ItemID == trackId)
			{
				_SelectedTrackItemData = mStoreData._Items[i];
				if (CommonInventoryData.pInstance.FindItem(_SelectedTrackItemData.ItemID) == null)
				{
					return mStoreData._Items[i].FinalCashCost;
				}
			}
		}
		return result;
	}

	public int IsTrackUnlocked(int trackIndex)
	{
		int result = -1;
		if (pActiveTrackData.Count < trackIndex || trackIndex < 0)
		{
			return result;
		}
		return IsUnlocked(pActiveTrackData[trackIndex]._ItemID);
	}

	public void OnCloseBuyTrack()
	{
		UnityEngine.Object.Destroy(mUiGenericDB);
		mUiGenericDB = null;
		_LobbyUi.SetInteractive(interactive: true);
		mRacingMMOClient.PauseGame(isPause: false);
	}

	private void ShowBuyPopUp()
	{
		if (_SelectedTrackItemData == null)
		{
			Debug.LogError("Selected Locked track is null");
			return;
		}
		_TrackToBuy._Item = _SelectedTrackItemData;
		_TrackToBuy._Icon = null;
	}

	public void PurchaseSuccessful()
	{
	}

	private Vector3 GetPicCamOffsetFromPetType(int inTypeId)
	{
		if (_PictureData == null || _PictureData.Length == 0)
		{
			UtDebug.LogWarning("No PictureData Provided!!!");
			return Vector3.zero;
		}
		PetPictureData[] pictureData = _PictureData;
		foreach (PetPictureData petPictureData in pictureData)
		{
			if (petPictureData._PetTypeID == inTypeId)
			{
				return petPictureData._CamOffset;
			}
		}
		return Vector3.zero;
	}

	public Texture2D GetDragonPicture(GameObject inGameObject, KAWidget inWidget, string inUserID)
	{
		if (mPictureCache.ContainsKey(inUserID))
		{
			return mPictureCache[inUserID];
		}
		Texture2D dstTexture = new Texture2D(256, 256, TextureFormat.ARGB32, mipChain: false);
		dstTexture.name = "DragonPicture-" + inUserID;
		Color[] pixels = ((Texture2D)(inWidget?.GetTexture()))?.GetPixels();
		dstTexture.SetPixels(pixels);
		dstTexture.Apply();
		if (_LobbyPhotoManager == null)
		{
			UtDebug.LogWarning("No Photo Manager Present in the scene!!!");
			return dstTexture;
		}
		if (mPhotoManager == null)
		{
			mPhotoManager = _LobbyPhotoManager.GetComponent<AvPhotoManager>();
		}
		if (inGameObject == null)
		{
			return dstTexture;
		}
		SanctuaryPet component = inGameObject.GetComponent<SanctuaryPet>();
		if ((bool)component && (bool)mPhotoManager)
		{
			mPhotoManager._HeadShotCamOffset = GetPicCamOffsetFromPetType(component.pTypeInfo._TypeID);
			string headBoneName = component.GetHeadBoneName();
			if (!string.IsNullOrEmpty(headBoneName))
			{
				Transform transform = component.FindBoneTransform(headBoneName);
				if (transform != null)
				{
					mPhotoManager.TakeAShot(inGameObject, ref dstTexture, transform);
					mPictureCache[inUserID] = dstTexture;
				}
				else
				{
					UtDebug.LogError("NO HEAD BONE FOUND!!!");
				}
			}
			else
			{
				UtDebug.LogError("NO HEAD BONE NAME PROVIDED!!");
			}
		}
		else
		{
			UtDebug.LogError("NO SANCTUARY PET IN RACING DRAGON!!");
		}
		return dstTexture;
	}

	public void ClearPictureCache()
	{
		mPictureCache.Clear();
	}

	public void ShowEquipScreen(GameScreen inLastScreen)
	{
		if ((bool)_EquipMenu)
		{
			UiChatHistory._IsVisible = false;
			_EquipMenu.Show(this, inLastScreen);
			switch (inLastScreen)
			{
			case GameScreen.GAME_HOST_JOIN_SCREEN:
				_MultiplayerMainUi.enabled = false;
				_MultiplayerMainUi.SetVisibility(inVisible: false);
				break;
			case GameScreen.GAME_MODE_SCREEN:
				_RaceSelectUi.enabled = false;
				_RaceSelectUi.SetVisibility(inVisible: false);
				break;
			case GameScreen.GAME_LOBBY_SCREEN:
				_LobbyUi.enabled = false;
				_LobbyUi.SetVisibility(inVisible: false);
				break;
			case GameScreen.GAME_THEME_SCREEN:
				break;
			}
		}
	}
}
