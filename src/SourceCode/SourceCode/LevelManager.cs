using System;
using System.Collections.Generic;
using KnowledgeAdventure.Multiplayer.Events;
using KnowledgeAdventure.Multiplayer.Model;
using UnityEngine;

public class LevelManager : MonoBehaviour, IConsumable
{
	public string _GameModuleName = "DragonRacing";

	public string _SinglePlayerGameModuleName = "DragonRacingSinglePlayer";

	public int _RechargeEnergyStoreID = 93;

	public int _RechargeEnergyItemID = 8257;

	public LocaleString _WaitText = new LocaleString("Please wait while others join.");

	public LocaleString _WaitTimeText = new LocaleString("You have {{NumSeconds}} seconds left.");

	public LocaleString _WaitForResultText = new LocaleString("Race will end in {{NumSeconds}} seconds.");

	public LocaleString _PayoutFailText = new LocaleString("Failed to get race rewards. Please try again later.");

	public LocaleString _ExitChallengeText = new LocaleString("Are you sure you want to lose this challenge?");

	public int _NumLaps = 3;

	public MiniMapManager _MiniMapManager;

	public List<RacingPositionSensor> _RacingPositionSensors;

	public RacingCheckpoint[] _RacingCheckpoints;

	public Transform _SpawnPointsTransform;

	public float _StunPointPercentage = 0.2f;

	public LocaleString _ExitSinglePlayerRaceText = new LocaleString("Do you really want to quit the race?");

	public LocaleString _ExitMultiPlayerRaceText = new LocaleString("[Review]Do you really want to quit the race? This may penalize you for next race.");

	public LocaleString _ForceExitRaceText = new LocaleString("You have been logged out.");

	public float[] _FlyingBoost;

	public string _TrackName = "Track 01";

	public int _TrackID = 1;

	public float _InitPushSpeed = 1.5f;

	public GameObject _DirectionalArrow;

	public Vector3 _DirectionArrowOffset;

	public LocaleString _PlayerLeftMsgTxt = new LocaleString("{{MMOPlr}} left the race.");

	public AudioClip _MusRaceSound;

	public int _SinglePlayerAchievementTaskID = 161;

	public int _SinglePlayerClanAchievementTaskID = 189;

	public int _FirstPlaceAchievementID = 162;

	public int _FirstPlaceClanAchievementID = 190;

	public LocaleString _JoinRaceFailedText = new LocaleString("Unable to join race now. Please try again. You will exit racing in {{NUM_SECONDS}} seconds.");

	public float _RaceTimeoutDuration = 60f;

	public float _TransitionTime = 30f;

	public float _TimeToExitRacing = 5f;

	public UiConsumable _UiConsumable;

	public PowerUpManager _PowerUpManager;

	public bool _DisableMoodParticle = true;

	private bool mPlayerReady;

	private bool mAllReady;

	private bool mRacingBegan;

	private bool mRacingFinished;

	private bool mError;

	private KAWidget mWingFlap;

	private KAWidget mDragonBrake;

	private double mCountDownStartTime;

	private bool mShowCountDown;

	private RacingMMOClient mRacingMMOClient;

	private float mElapsedTime;

	private bool mLoadTimeChecked;

	private bool mPetMountEventAdded;

	private UiRacingMessage mUiInGame;

	private List<PlayerData> mPlayerData = new List<PlayerData>();

	private UiRaceTrackHUD mUiRaceHUD;

	private UiRaceTrackEquipments mUiEquipments;

	private const string START_GAME = "STA";

	private const string DISTANCE = "DT";

	private const string CONSUMABLE = "CON";

	private const string GAME_RESULTS = "GR";

	private const string END_TIMER = "ET";

	private const char MESSAGE_SEPARATOR = ':';

	private const string READYTORACE = "RTR";

	private bool mIsRaceEndWait;

	public UiRaceResults _UiRaceResults;

	public PoolInfo[] _TurnOffPoolInfo;

	private UiCountDown mUiCountdown;

	private RacingComparer mComparer = new RacingComparer();

	private bool mStartedCountDown;

	public KAUIPadButtons _flyingUI;

	private Ob3DArrow mArrowComp;

	private float mRaceStartTimeout;

	private KAUIGenericDB mGameInitDB;

	public LocaleString _MMOReadyText = new LocaleString("Viking {{MMOPlr}} ready to Race.");

	private KAUIGenericDB mGenericDB;

	private KAUIGenericTimerDB mGenericTimerDB;

	private float mPlayerIdleTime;

	private bool mIdleWarningShowing;

	private PowerUpHelper mPowerUpHelper = new PowerUpHelper();

	private Rect mWindowRect = new Rect(20f, 20f, 320f, 180f);

	public bool _ShowProbabilityChart;

	public bool _ShowPlayerStatus;

	public float pElapsedTime
	{
		get
		{
			return mElapsedTime;
		}
		set
		{
			mElapsedTime = value;
		}
	}

	public bool pIsInGame
	{
		get
		{
			if (mRacingBegan)
			{
				return !mRacingFinished;
			}
			return false;
		}
	}

	public UiRacingMessage pUiInGame => mUiInGame;

	public UiRaceTrackHUD pUiRaceHUD => mUiRaceHUD;

	public List<PlayerData> pPlayerData => mPlayerData;

	private void Awake()
	{
		AvAvatar.pState = AvAvatarState.NONE;
		AvAvatar.pSubState = AvAvatarSubState.NORMAL;
		mUiCountdown = UnityEngine.Object.FindObjectOfType(typeof(UiCountDown)) as UiCountDown;
		if (AvAvatar.pObject != null)
		{
			ChatBubble componentInChildren = AvAvatar.pObject.GetComponentInChildren<ChatBubble>();
			if (componentInChildren != null && componentInChildren.gameObject != null)
			{
				componentInChildren.gameObject.SetActive(value: false);
			}
		}
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: false);
		}
		mPowerUpHelper._CanActivatePowerUp = CanActivatePowerUp;
		mPowerUpHelper._CanApplyEffect = CanApplyEffect;
		mPowerUpHelper._GetParentObject = GetShieldParent;
		mPowerUpHelper._IsMMOUser = IsMMOUser;
		if (_PowerUpManager != null)
		{
			_PowerUpManager.Init(this, mPowerUpHelper);
			if (RacingManager.pIsSinglePlayer)
			{
				_PowerUpManager.DestroyCollectables();
			}
		}
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.LoadLevelComplete(RsResourceManager.pCurrentLevel);
			MMOTimeManager._Period = 2f;
		}
	}

	private int GetGameMode()
	{
		if (RacingManager.pIsSinglePlayer)
		{
			return 0;
		}
		return 1;
	}

	private void Collect(GameObject gameObject)
	{
		if (_UiConsumable != null)
		{
			int num = GetMainPlayer().mAvatarRacing.pPosition;
			if (!RacingManager.pIsSinglePlayer || MainStreetMMOClient.pIsMMOEnabled)
			{
				num--;
			}
			Consumable consumableOnProbability = ConsumableData.GetConsumableOnProbability("Racing", "Game", num, GetGameMode());
			if (consumableOnProbability != null)
			{
				_UiConsumable.RegisterConsumable(consumableOnProbability);
			}
			else
			{
				UtDebug.LogError("ERROR! did not find a suitable consumable");
			}
			if (MainStreetMMOClient.pIsReady)
			{
				MainStreetMMOClient.pInstance.SendPublicMMOMessage("CON:" + gameObject.name + ":" + MMOTimeManager.pInstance.GetServerTime() + ":" + UserInfo.pInstance.UserID);
			}
		}
	}

	private void OnDisable()
	{
		MMOTimeManager._Period = 5f;
		Unregister();
	}

	public void Unregister()
	{
		if (mRacingMMOClient != null && !RacingManager.pIsSinglePlayer)
		{
			mRacingMMOClient.OnReponseEvent -= ExtensionResponse;
			mRacingMMOClient.OnMessageEvent -= MessageResponse;
		}
	}

	private void Start()
	{
		if (_UiConsumable != null)
		{
			_UiConsumable.SetGameData(this, "Racing", GetGameMode());
		}
		AvAvatar.SetActive(inActive: true);
		if (!RacingManager.pIsSinglePlayer)
		{
			mRacingMMOClient = RacingMMOClient.Init(null, this);
			mRacingMMOClient.OnReponseEvent += ExtensionResponse;
			mRacingMMOClient.OnMessageEvent += MessageResponse;
		}
		InitializeRacePositionSensors();
		RacingCheckpoint.Sort(_RacingCheckpoints);
		UpdateRacingUI();
		ShowRacingUI(inShow: false);
		if (_MiniMapManager != null)
		{
			_MiniMapManager.EnableMiniMapCamera(active: false);
		}
		mRacingBegan = false;
		mRacingFinished = false;
		AddPlayersToList();
		SetPlayerInitialPosOnTrack();
		ObAvatarRespawnDO._MessageObject = base.gameObject;
		ObAvatarRespawnDO._HitMessage = "OnHitWater";
		ObCollisionMessage._MessageObject = base.gameObject;
		ObCollisionMessage._CollisionMessage = "OnHitWall";
		if (_DirectionalArrow != null)
		{
			InitDirectionArrow();
		}
		if (mWingFlap == null)
		{
			if (_flyingUI == null)
			{
				GameObject gameObject = GameObject.Find("PfUiAvatarBtn");
				if (gameObject != null)
				{
					_flyingUI = gameObject.GetComponent<KAUIPadButtons>();
				}
			}
			if (_flyingUI != null)
			{
				_flyingUI.FindItem("Jump").SetVisibility(inVisible: false);
				_flyingUI.FindItem("DragonMount").SetVisibility(inVisible: false);
				mWingFlap = _flyingUI.FindItem("WingFlap");
				mWingFlap.SetState(KAUIState.DISABLED);
				mDragonBrake = _flyingUI.FindItem("DragonBrake");
				mDragonBrake.SetState(KAUIState.DISABLED);
			}
		}
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.pIgnoreIdleTimeOut = true;
		}
		EnableDragonInput(inEnable: false);
		if (!ProductData.pPairData.GetBoolValue(AnalyticEvent.PLAY_RACING.ToString(), defaultVal: false))
		{
			AnalyticAgent.LogEvent("AppsFlyer", AnalyticEvent.PLAY_RACING, new Dictionary<string, string>());
			ProductData.pPairData.SetValueAndSave(AnalyticEvent.PLAY_RACING.ToString(), true.ToString());
		}
		SnChannel.AddToTurnOffPools(_TurnOffPoolInfo);
	}

	private void DisableControls(bool isDisable)
	{
		if (UiAvatarControls.pInstance != null)
		{
			UiAvatarControls.pInstance.DisableAllDragonControls(isDisable);
			UiAvatarControls.pInstance.EnableDragonFireButton(!isDisable);
		}
		if (_UiConsumable != null)
		{
			_UiConsumable.SetVisibility(!isDisable);
		}
	}

	private void InitializeRacePositionSensors()
	{
		for (int i = 0; i < _RacingPositionSensors.Count; i++)
		{
			if (i == 0)
			{
				_RacingPositionSensors[i]._LastSensor = _RacingPositionSensors[_RacingPositionSensors.Count - 1];
			}
			else
			{
				_RacingPositionSensors[i]._LastSensor = _RacingPositionSensors[i - 1];
			}
			if (i == _RacingPositionSensors.Count - 1)
			{
				_RacingPositionSensors[i]._NextSensor = _RacingPositionSensors[0];
			}
			else
			{
				_RacingPositionSensors[i]._NextSensor = _RacingPositionSensors[i + 1];
			}
			_RacingPositionSensors[i].Initialize(i);
		}
	}

	private void UpdateMultiplayer()
	{
		if (RacingManager.Instance.State != RacingManagerState.Racing || KAInput.anyKey || KAInput.touchCount > 0)
		{
			mPlayerIdleTime = 0f;
			if (mIdleWarningShowing)
			{
				mIdleWarningShowing = false;
				if (mGenericTimerDB != null && mGenericTimerDB.gameObject != null)
				{
					KAUI.RemoveExclusive(mGenericDB);
					mGenericTimerDB.Destroy();
					mGenericTimerDB = null;
				}
			}
		}
		else
		{
			mPlayerIdleTime += Time.deltaTime;
			if (!mIdleWarningShowing && mPlayerIdleTime > RacingManager.Instance._ShowIdleWarningIn)
			{
				mIdleWarningShowing = ShowIdleWarning();
			}
		}
		if (mPlayerIdleTime > RacingManager.Instance._PlayerIdleTimeOut || RacingManager.Instance.TimeElapsedGamePause > RacingManager.Instance._GamePauseTimeOutInSec)
		{
			if (RacingManager.Instance.State != RacingManagerState.RaceFinish)
			{
				RacingManager.Instance.RemovePreviousPenalty();
				RacingManager.Instance.TrySetFinishReason(DNFType.RaceTimeOut);
				QuitRace();
			}
			return;
		}
		if (mError || RsResourceManager.pLevelLoadingScreen)
		{
			if (mError && mGenericDB != null && Time.time - mRaceStartTimeout >= _TimeToExitRacing)
			{
				QuitRace();
			}
			return;
		}
		if (!mShowCountDown && mStartedCountDown && MMOTimeManager.pInstance != null && MMOTimeManager.pInstance.GetServerTime() > mCountDownStartTime)
		{
			mShowCountDown = true;
			mUiCountdown.StartCountDown(inStart: true);
			CloseDB();
			if (RacingManager.Instance != null)
			{
				RacingManager.Instance.State = RacingManagerState.RaceCountdown;
			}
		}
		if (Time.frameCount % 20 == 0)
		{
			CheckPlayerList();
		}
		if (mRacingBegan && !mRacingFinished)
		{
			CheckforNumPlayers();
		}
		if (mAllReady && MainStreetMMOClient.pInstance.pState != MMOClientState.IN_ROOM && mGenericDB == null)
		{
			ShowQuitConfirmDB(isForceQuit: true);
		}
		if (mAllReady)
		{
			return;
		}
		if (!mLoadTimeChecked)
		{
			mLoadTimeChecked = true;
			if (RacingManager.TimeSinceTransition() > _TransitionTime)
			{
				ShowError();
			}
		}
		else if (!mPlayerReady)
		{
			Dictionary<string, object> inParams = new Dictionary<string, object>();
			MainStreetMMOClient.pInstance.SendExtensionMessageToRoom("dr.UACK", inParams);
			mPlayerReady = true;
		}
		else if (GetPlayersReadyCount() == mPlayerData.Count)
		{
			Dictionary<string, object> inParams2 = new Dictionary<string, object>();
			MainStreetMMOClient.pInstance.SendExtensionMessageToRoom("dr.ARACK", inParams2);
			mAllReady = true;
		}
		else if (Time.time - mRaceStartTimeout > _RaceTimeoutDuration)
		{
			ShowError();
		}
		if (mGenericDB == null && !mAllReady && !mRacingBegan && !mRacingFinished && !mShowCountDown)
		{
			mGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDBSm", "WaitingDB");
			mGenericDB.SetText(_WaitText.GetLocalizedString(), interactive: false);
			mGenericDB.SetVisibility(inVisible: true);
			mGenericDB.SetExclusive();
			if (RacingManager.Instance != null)
			{
				RacingManager.Instance.State = RacingManagerState.WaitingForPlayers;
			}
		}
	}

	private bool ShowIdleWarning()
	{
		if (mGenericTimerDB != null)
		{
			return false;
		}
		string localizedString = RacingManager.Instance._PlayerIdleWarningText.GetLocalizedString();
		mGenericTimerDB = (KAUIGenericTimerDB)GameUtilities.DisplayGenericDB("PfKAUIGenericDBSmTimer", localizedString, "", base.gameObject, "", "", "", "");
		double second = RacingManager.Instance._PlayerIdleTimeOut - RacingManager.Instance._ShowIdleWarningIn;
		mGenericTimerDB.Init(showTimer: true, second, TimeFormat.SS, autoCloseOnTimeUp: false);
		return true;
	}

	private void ShowError()
	{
		mError = true;
		if (mGenericDB != null)
		{
			UnityEngine.Object.Destroy(mGenericDB.gameObject);
		}
		mRaceStartTimeout = Time.time;
		mGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDBSm", "WaitingDB");
		string localizedString = _JoinRaceFailedText.GetLocalizedString();
		localizedString = localizedString.Replace("{{NUM_SECONDS}}", _TimeToExitRacing.ToString());
		mGenericDB.SetText(localizedString, interactive: false);
		mGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mGenericDB._OKMessage = "QuitRace";
		mGenericDB._MessageObject = base.gameObject;
		mGenericDB.SetVisibility(inVisible: true);
		mGenericDB.SetExclusive();
	}

	private void UpdateSingleplayer()
	{
		if (!mStartedCountDown)
		{
			StartCountdown();
		}
	}

	private void Update()
	{
		if (!mPetMountEventAdded)
		{
			if (!RacingManager.pIsSinglePlayer)
			{
				mRaceStartTimeout = Time.time;
			}
			AddPetMountEvent();
		}
		AvAvatarController.mForceBraking = !mRacingBegan || mRacingFinished;
		if (RacingManager.pIsSinglePlayer)
		{
			UpdateSingleplayer();
		}
		else
		{
			UpdateMultiplayer();
		}
		if (mUiCountdown.enabled && mUiCountdown.IsCountDownOver())
		{
			DisableControls(isDisable: false);
			mUiCountdown.StartCountDown(inStart: false);
			if (!mRacingBegan)
			{
				StartRace();
			}
		}
		else if (!mRacingBegan && !mRacingFinished)
		{
			DisableControls(isDisable: true);
		}
	}

	private void SetPlayerInitialPosOnTrack()
	{
		int num = 0;
		using (List<PlayerData>.Enumerator enumerator = mPlayerData.GetEnumerator())
		{
			while (enumerator.MoveNext() && !(enumerator.Current.mUserName == WsWebService.pUserToken))
			{
				num++;
			}
		}
		num = num % _SpawnPointsTransform.childCount + 1;
		Transform transform = _SpawnPointsTransform.Find(num.ToString());
		if (transform == null)
		{
			transform = _SpawnPointsTransform.Find("1");
		}
		if (transform != null)
		{
			AvAvatar.position = transform.position;
			AvAvatar.mTransform.rotation = transform.rotation;
		}
		AvAvatarController avAvatarController = (AvAvatarController)AvAvatar.pObject.GetComponent(typeof(AvAvatarController));
		if ((bool)avAvatarController)
		{
			avAvatarController.pVelocity = Vector3.zero;
			avAvatarController.pFlightSpeed = 0f;
			if (avAvatarController.pFlyingGlidingMode)
			{
				avAvatarController.pFlyingGlidingMode = false;
			}
		}
	}

	private void ShowRacingUI(bool inShow)
	{
		if (mUiInGame != null)
		{
			mUiInGame.SetVisibility(inShow);
		}
		if (mUiRaceHUD != null)
		{
			mUiRaceHUD.SetUiVisibility(inShow);
		}
		if (UiAvatarControls.pInstance != null)
		{
			UiAvatarControls.pInstance.EnableDragonFireButton(inShow);
		}
	}

	private void ExtensionResponse(MMOExtensionResponseReceivedEventArgs args)
	{
		switch (args.ResponseDataObject["2"].ToString())
		{
		case "STA":
		{
			UtDebug.Log("START");
			mRacingMMOClient.ResetLoadingState();
			string message = args.ResponseDataObject["3"].ToString();
			if (args.ResponseDataObject.ContainsKey("4"))
			{
				mCountDownStartTime = Convert.ToDouble(args.ResponseDataObject["4"].ToString());
			}
			StartGame(message);
			break;
		}
		case "GR":
		{
			PlayerData player = GetPlayer(UserInfo.pInstance.UserID);
			if (player.mResultState != 0 && player.mResultState != RaceState.IN_RACE && args.ResponseDataObject.Count > 3)
			{
				mIsRaceEndWait = false;
				mUiInGame.SetVisibility(inVisible: false);
				mUiEquipments.SetVisibility(inVisible: false);
				_UiRaceResults.Initialize(args, this);
				if (SanctuaryManager.pInstance.pPetMeter != null)
				{
					SanctuaryManager.pInstance.pPetMeter.SetVisibility(inVisible: false);
				}
			}
			break;
		}
		case "ET":
			if (mUiRaceHUD.GetVisibility())
			{
				ShowRaceEndTimer(args.ResponseDataObject["3"].ToString());
			}
			break;
		}
	}

	private void MessageResponse(MMOMessageReceivedEventArgs args)
	{
		if (args.MMOMessage.Sender.Username == ProductConfig.pToken || args.MMOMessage.Sender.Username == WsWebService.pUserToken || args.MMOMessage.MessageType != MMOMessageType.User)
		{
			return;
		}
		string[] array = args.MMOMessage.MessageText.Split(':');
		if (array[0] == "DT")
		{
			SetDistance(array[1], array[2]);
		}
		else if (array.Length > 2 && array[0] == "CON")
		{
			string n = array[1];
			double num = Convert.ToDouble(array[2]);
			GameObject gameObject = null;
			if (_PowerUpManager != null && _PowerUpManager._Collectables != null)
			{
				Transform transform = _PowerUpManager._Collectables.transform.Find(n);
				if (transform != null)
				{
					gameObject = transform.gameObject;
				}
			}
			if (gameObject == null)
			{
				gameObject = GameObject.Find(n);
			}
			if (!(gameObject != null))
			{
				return;
			}
			ObCollect component = gameObject.GetComponent<ObCollect>();
			if (component.pCollected)
			{
				return;
			}
			string userID = array[3];
			PlayerData player = GetPlayer(userID);
			if (player != null && Vector3.Distance(gameObject.transform.position, player.mAvatar.GetPredictedPosition()) <= 40f)
			{
				double num2 = (MMOTimeManager.pInstance.GetServerTime() - num) / 1000.0;
				double num3 = (double)component._RegenTime - num2;
				if (num3 > 0.0)
				{
					component._RegenTime = (float)num3;
					component.OnItemCollected();
				}
			}
		}
		else
		{
			if (!(array[0] == "RTR"))
			{
				return;
			}
			SetMMOReadyStatus(array[1]);
			if (!mRacingBegan)
			{
				bool result = false;
				bool.TryParse(array[2], out result);
				if (result)
				{
					StartRace();
				}
			}
		}
	}

	public void SetDistance(string userId, string distance)
	{
		mComparer.pComparisonMethod = RacingComparer.ComparisonMethod.DISTANCE_COVERED;
		foreach (PlayerData mPlayerDatum in mPlayerData)
		{
			if (mPlayerDatum.mAvatar != null && mPlayerDatum.mAvatar.pUserID.Equals(userId))
			{
				mPlayerDatum.mAvatarRacing.pDistanceCovered = float.Parse(distance);
			}
		}
		mPlayerData.Sort(mComparer);
		for (int i = 0; i < mPlayerData.Count; i++)
		{
			mPlayerData[i].mAvatarRacing.pPosition = i + 1;
		}
	}

	private void AddPlayersToList()
	{
		mPlayerData.Clear();
		AvatarRacing avatarRacing = AvAvatar.pObject.AddComponent<AvatarRacing>();
		avatarRacing.pType = AvatarRacing.Type.USER;
		PlayerData playerData = new PlayerData(null, AvatarData.pInstance.DisplayName, avatarRacing, RaceState.IDLE);
		playerData.mUserName = WsWebService.pUserToken;
		avatarRacing.Init(mRacingMMOClient, this);
		mPlayerData.Add(playerData);
		if (_MiniMapManager != null)
		{
			_MiniMapManager.AddDisplayObject(AvAvatar.pObject, avatarRacing, isUser: true);
		}
		foreach (KeyValuePair<string, MMOAvatar> pPlayer in MainStreetMMOClient.pInstance.pPlayerList)
		{
			avatarRacing = pPlayer.Value.gameObject.AddComponent<AvatarRacing>();
			playerData = new PlayerData(pPlayer.Value, pPlayer.Value.pAvatarData.mInstance.DisplayName, avatarRacing, RaceState.IDLE);
			playerData.mUserName = pPlayer.Key;
			playerData.mAvatarRacing.pType = AvatarRacing.Type.MMO;
			mPlayerData.Add(playerData);
			if (_MiniMapManager != null)
			{
				_MiniMapManager.AddDisplayObject(pPlayer.Value.gameObject, avatarRacing, isUser: false);
			}
		}
		mComparer.pComparisonMethod = RacingComparer.ComparisonMethod.TOKEN_ID;
		mPlayerData.Sort(mComparer);
	}

	private void UpdateRacingUI()
	{
		GameObject gameObject = GameObject.Find("PfUiGameInstance/PfUiRacingMessage");
		mUiInGame = gameObject.GetComponent(typeof(UiRacingMessage)) as UiRacingMessage;
		gameObject = GameObject.Find("PfUiGameInstance/PfUiRaceTrackHUD");
		mUiRaceHUD = gameObject.GetComponent(typeof(UiRaceTrackHUD)) as UiRaceTrackHUD;
		if (mUiRaceHUD != null)
		{
			mUiRaceHUD.Init(this);
		}
		gameObject = GameObject.Find("PfUiGameInstance/PfUiRaceTrackEquipments");
		mUiEquipments = gameObject.GetComponent(typeof(UiRaceTrackEquipments)) as UiRaceTrackEquipments;
	}

	private void AddPetMountEvent()
	{
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pCurPetInstance.SetMoodParticleIgnore(_DisableMoodParticle);
			mPetMountEventAdded = true;
			RsResourceManager.DestroyLoadScreen();
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.pSubState = AvAvatarSubState.NORMAL;
			SanctuaryPet.AddMountEvent(SanctuaryManager.pCurPetInstance, PetReady);
			SanctuaryManager.pCurPetInstance.Mount(AvAvatar.pObject, PetSpecialSkillType.FLY);
			AvAvatar.pSubState = AvAvatarSubState.FLYING;
			AvAvatarController avAvatarController = (AvAvatarController)AvAvatar.pObject.GetComponent(typeof(AvAvatarController));
			if ((bool)avAvatarController)
			{
				avAvatarController.SetFlyingState(FlyingState.Hover);
			}
			SetPlayerInitialPosOnTrack();
			if (_MusRaceSound != null)
			{
				SnChannel.Play(_MusRaceSound, "Music_Pool", inForce: true, null);
			}
		}
		else
		{
			AvAvatar.pState = AvAvatarState.NONE;
			AvAvatar.pSubState = AvAvatarSubState.NORMAL;
		}
	}

	public void PetReady(bool mount, PetSpecialSkillType type)
	{
		AvatarRacing component = AvAvatar.pObject.GetComponent<AvatarRacing>();
		if (component != null)
		{
			component.PetReady(mount, type);
		}
	}

	public PlayerData GetMainPlayer()
	{
		foreach (PlayerData mPlayerDatum in mPlayerData)
		{
			if (mPlayerDatum.mAvatarRacing.pType == AvatarRacing.Type.USER)
			{
				return mPlayerDatum;
			}
		}
		return null;
	}

	public PlayerData GetPlayerFromUserName(string userName)
	{
		foreach (PlayerData mPlayerDatum in mPlayerData)
		{
			if (mPlayerDatum.mUserName.Equals(userName))
			{
				return mPlayerDatum;
			}
		}
		return null;
	}

	public PlayerData GetPlayer(string userID)
	{
		foreach (PlayerData mPlayerDatum in mPlayerData)
		{
			if (mPlayerDatum.mUserId.Equals(userID))
			{
				return mPlayerDatum;
			}
		}
		return null;
	}

	public PlayerData GetPlayerFromName(string inName)
	{
		foreach (PlayerData mPlayerDatum in mPlayerData)
		{
			if (mPlayerDatum.mName.Equals(inName))
			{
				return mPlayerDatum;
			}
		}
		return null;
	}

	public void StartGame(string message)
	{
		if (mGameInitDB != null)
		{
			UnityEngine.Object.Destroy(mGameInitDB.gameObject);
			mGameInitDB = null;
		}
		StartCountdown();
	}

	private void ForceQuitRace()
	{
		EnableDragonInput(inEnable: true);
		AvAvatarController.mForceBraking = false;
		mRacingBegan = false;
		mRacingFinished = true;
		if (RacingManager.Instance != null)
		{
			RacingManager.Instance.State = RacingManagerState.RaceFinish;
		}
		if (_MiniMapManager != null)
		{
			_MiniMapManager.EnableMiniMapCamera(active: false);
		}
		if (mGenericDB != null)
		{
			KAUI.RemoveExclusive(mGenericDB);
			UnityEngine.Object.Destroy(mGenericDB.gameObject);
			AvAvatar.pState = AvAvatar.pPrevState;
		}
		DestroyRacingComp();
		RacingManager.Instance.ExitRacing();
	}

	private int GetPlayersReadyCount()
	{
		int num = 0;
		foreach (PlayerData mPlayerDatum in mPlayerData)
		{
			if (mPlayerDatum.mAvatar != null)
			{
				if (mPlayerDatum.mResultState == RaceState.NOT_FINISHED || mPlayerDatum.mAvatar.pIsSanctuaryPetReady)
				{
					if (!mPlayerDatum.mAvatarRacing.pIsReady)
					{
						ShowMMOPlayer(mPlayerDatum, inShow: true);
						mPlayerDatum.mAvatarRacing.pIsReady = true;
					}
					num++;
				}
			}
			else
			{
				if (!mPlayerDatum.mReadyToRace)
				{
					mPlayerDatum.mReadyToRace = true;
				}
				num++;
			}
		}
		return num;
	}

	private void CheckPlayerList()
	{
		if (!MainStreetMMOClient.pIsReady)
		{
			return;
		}
		Dictionary<string, MMOAvatar> pPlayerList = MainStreetMMOClient.pInstance.pPlayerList;
		foreach (PlayerData mPlayerDatum in mPlayerData)
		{
			if (mPlayerDatum != null && !pPlayerList.ContainsKey(mPlayerDatum.mUserName))
			{
				RemovePlayer(mPlayerDatum.mAvatar);
			}
		}
	}

	private void ShowMMOPlayer(PlayerData inData, bool inShow)
	{
		if (inData.mAvatar == null || inData.mAvatar.pSanctuaryPet == null || inData.mAvatar.gameObject == null || inData.mAvatar.pSanctuaryPet.gameObject == null)
		{
			return;
		}
		Component[] componentsInChildren = inData.mAvatar.gameObject.GetComponentsInChildren(typeof(Renderer));
		Renderer[] componentsInChildren2 = inData.mAvatar.pSanctuaryPet.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
		Renderer[] array = componentsInChildren2;
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			Component[] array2 = componentsInChildren;
			foreach (Component component in array2)
			{
				((Renderer)component).enabled = inShow;
				UtDebug.Log("Rend object " + component.gameObject.name + " for MMO " + inData.mName + " set to " + inShow);
			}
		}
		if (array != null && array.Length != 0)
		{
			componentsInChildren2 = array;
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)componentsInChildren2[i];
				skinnedMeshRenderer.enabled = inShow;
				UtDebug.Log("Rend object " + skinnedMeshRenderer.gameObject.name + " for MMO PET " + inData.mName + " set to " + inShow);
			}
		}
	}

	private void UpdateMMOPlayerTrophyCount()
	{
		int num = 0;
		foreach (PlayerData mPlayerDatum in mPlayerData)
		{
			if (mPlayerDatum.mAvatar != null && mPlayerDatum.mAvatar.pProfileData != null)
			{
				UserAchievementInfo userAchievementInfoByType = UserRankData.GetUserAchievementInfoByType(mPlayerDatum.mAvatar.pProfileData.AvatarInfo.Achievements, 11);
				mPlayerDatum.mTrophyCount = ((userAchievementInfoByType != null && userAchievementInfoByType.AchievementPointTotal.HasValue) ? userAchievementInfoByType.AchievementPointTotal.Value : 0);
				num++;
			}
		}
	}

	public void Close()
	{
		mUiInGame.enabled = false;
	}

	public void SetMMOReadyStatus(string userId)
	{
		PlayerData player = GetPlayer(userId);
		if (player != null && !player.mReadyToRace)
		{
			player.mReadyToRace = true;
		}
	}

	public void GameComplete()
	{
		if (RacingManager.Instance != null)
		{
			RacingManager.Instance.State = RacingManagerState.RaceFinish;
		}
		mRacingFinished = true;
		EnableDragonInput(inEnable: false);
		_UiConsumable.SetVisibility(inVisible: false);
		_PowerUpManager.DeactivateAllPowerUps();
		if (_MiniMapManager != null)
		{
			_MiniMapManager.EnableMiniMapCamera(active: false);
		}
		if (mArrowComp != null)
		{
			mArrowComp.Show(inShow: false);
		}
		if (RacingManager.pIsSinglePlayer)
		{
			mUiInGame.SetVisibility(inVisible: false);
			mUiEquipments.SetVisibility(inVisible: false);
			if (SanctuaryManager.pInstance.pPetMeter != null)
			{
				SanctuaryManager.pInstance.pPetMeter.SetVisibility(inVisible: false);
			}
		}
		mUiRaceHUD.SetVisibility(inVisible: false);
		SetRaceState(RaceState.FINISHED, UserInfo.pInstance.UserID);
		if (UiAvatarControls.pInstance != null)
		{
			UiAvatarControls.pInstance.EnableDragonFireButton(inEnable: false);
		}
		if (AvAvatar.IsPlayerOnRide())
		{
			AvAvatar.pObject.GetComponentInChildren<AvatarRacing>().enabled = false;
			AvAvatarController componentInChildren = AvAvatar.pObject.GetComponentInChildren<AvAvatarController>();
			if (componentInChildren != null)
			{
				componentInChildren.StopFlying();
			}
		}
		else
		{
			AvAvatar.pObject.GetComponent<AvatarRacing>().enabled = false;
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if (component != null)
			{
				component.StopFlying();
			}
		}
		if ((bool)SanctuaryManager.pCurPetInstance)
		{
			SanctuaryManager.pCurPetInstance.UpdateActionMeters(PetActions.RACING, 1f, doUpdateSkill: true);
		}
		mUiRaceHUD.ChallengeItemVisible(visible: false);
	}

	public void StartRace()
	{
		TimeHackPrevent.Set(AvAvatarLevelState.RACING.ToString());
		if (_UiConsumable != null)
		{
			_UiConsumable.SetVisibility(inVisible: true);
		}
		mElapsedTime = 0f;
		mRacingBegan = true;
		mAllReady = true;
		mStartedCountDown = true;
		if (_MiniMapManager != null)
		{
			_MiniMapManager.EnableMiniMapCamera(active: true);
		}
		SetRaceState(RaceState.IN_RACE);
		ShowRacingUI(inShow: true);
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: true, inForceUpdate: true);
		}
		AvAvatar.SetActive(inActive: true);
		EnableDragonInput(inEnable: true);
		if (mArrowComp != null)
		{
			mArrowComp.Show(inShow: true);
		}
		if (RacingManager.Instance != null)
		{
			RacingManager.Instance.State = RacingManagerState.Racing;
		}
		if (ChallengeInfo.pActiveChallenge != null)
		{
			mUiRaceHUD.UpdateChallengePoints(ChallengeInfo.pActiveChallenge.Points);
			mUiRaceHUD.ChallengeItemVisible(visible: true);
		}
	}

	public void StartCountdown()
	{
		if (!mStartedCountDown)
		{
			mStartedCountDown = true;
		}
		if (mUiRaceHUD != null)
		{
			mUiRaceHUD.EnableBackButton(enable: false);
		}
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		component.SetFlyingState(FlyingState.TakeOffGliding);
		component.SetPushSpeed(_InitPushSpeed);
		KAInput.pInstance.EnableInputType("DragonMount", InputType.UI_BUTTONS, inEnable: false);
		if (AvAvatar.IsPlayerOnRide())
		{
			AvAvatar.pObject.GetComponentInChildren<AvatarRacing>().enabled = true;
		}
		else
		{
			AvAvatar.pObject.GetComponent<AvatarRacing>().enabled = true;
		}
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.ResetDisconnectTimer();
		}
		if (RacingManager.pIsSinglePlayer)
		{
			mUiCountdown.StartCountDown(inStart: true);
		}
	}

	private void CheckforNumPlayers()
	{
		int num = mPlayerData.Count;
		foreach (PlayerData mPlayerDatum in mPlayerData)
		{
			if (mPlayerDatum.mResultState == RaceState.NOT_FINISHED)
			{
				num--;
			}
		}
		if (num <= 1)
		{
			CloseDB();
			AvatarRacing component = AvAvatar.pObject.GetComponent<AvatarRacing>();
			if (component != null)
			{
				component.OnGameComplete();
			}
		}
	}

	public void RemovePlayer(MMOAvatar avatar)
	{
		if (avatar == null)
		{
			return;
		}
		PlayerData player = GetPlayer(avatar.pUserID);
		if (player != null && player.mResultState != RaceState.NOT_FINISHED)
		{
			if (player.mResultState != RaceState.FINISHED)
			{
				SetRaceState(RaceState.NOT_FINISHED, avatar.pUserID);
			}
			player.mAvatarRacing.pLapsCompleted = -2;
			string text = StringTable.GetStringData(_PlayerLeftMsgTxt._ID, _PlayerLeftMsgTxt._Text);
			if (text.Contains("{{MMOPlr}}"))
			{
				text = text.Replace("{{MMOPlr}}", player.mName);
			}
			mUiInGame.SetPlayerLeftText(text, 2f);
		}
	}

	private void SetRaceState(RaceState state, string userID = null)
	{
		if (!string.IsNullOrEmpty(userID))
		{
			PlayerData player = GetPlayer(userID);
			if (player != null && player.mUserId == userID)
			{
				player.mResultState = state;
				UtDebug.Log("State for:: " + player.mName + " ::to::" + player.mResultState);
			}
			return;
		}
		foreach (PlayerData mPlayerDatum in mPlayerData)
		{
			if (mPlayerDatum.mResultState != RaceState.NOT_FINISHED)
			{
				mPlayerDatum.mResultState = state;
			}
			UtDebug.Log("State for ALL:: " + mPlayerDatum.mName + " ::to::" + mPlayerDatum.mResultState);
		}
	}

	private void ShowRaceEndTimer(string inMessage)
	{
		if (inMessage == null || inMessage.Length <= 0)
		{
			return;
		}
		if (inMessage == "0")
		{
			CheckForGameComplete();
			return;
		}
		if (!mIsRaceEndWait)
		{
			mIsRaceEndWait = true;
		}
		PlayerData player = GetPlayer(UserInfo.pInstance.UserID);
		string stringData = StringTable.GetStringData(_WaitTimeText._ID, _WaitTimeText._Text);
		if (player != null && player.mResultState == RaceState.FINISHED)
		{
			stringData = StringTable.GetStringData(_WaitForResultText._ID, _WaitForResultText._Text);
		}
		stringData = stringData.Replace("{{NumSeconds}}", inMessage);
		mUiInGame.SetText(null, stringData, isAppend: false, visibility: true);
	}

	private void CheckForGameComplete()
	{
		CloseDB();
		PlayerData player = GetPlayer(UserInfo.pInstance.UserID);
		if (player != null && player.mResultState != RaceState.FINISHED && player.mAvatarRacing != null)
		{
			_PowerUpManager.DeactivateAllPowerUps();
			player.mAvatarRacing.OnGameComplete();
		}
		mUiInGame.SetVisibility(inVisible: false);
		mUiEquipments.SetVisibility(inVisible: false);
		if (SanctuaryManager.pInstance.pPetMeter != null)
		{
			SanctuaryManager.pInstance.pPetMeter.SetVisibility(inVisible: false);
		}
	}

	public void EnableDragonInput(bool inEnable)
	{
		if ((bool)mWingFlap)
		{
			mWingFlap.SetVisibility(inVisible: true);
			mWingFlap.SetState(KAUIState.INTERACTIVE);
		}
		if ((bool)mDragonBrake)
		{
			mDragonBrake.SetVisibility(inVisible: true);
			mDragonBrake.SetState(KAUIState.INTERACTIVE);
		}
		KAInput.pInstance.EnableInputType("WingFlap", InputType.ALL, inEnable);
		KAInput.pInstance.EnableInputType("Horizontal", InputType.ALL, inEnable);
		KAInput.pInstance.EnableInputType("Vertical", InputType.ALL, inEnable);
		KAInput.pInstance.EnableInputType("DragonBrake", InputType.ALL, inEnable);
		KAInput.pInstance.EnableInputType("DragonMount", InputType.UI_BUTTONS, inEnable: false);
		KAInput.pInstance.EnableInputType("Jump", InputType.UI_BUTTONS, inEnable: false);
		UiAvatarControls.EnableTiltControls(inEnable);
		if (UiAvatarControls.pInstance != null)
		{
			UiAvatarControls.pInstance.EnableDragonFireButton(inEnable);
			UiAvatarControls.pInstance.SetVisibility(inEnable);
		}
	}

	private void InitDirectionArrow()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(_DirectionalArrow);
		if (gameObject != null)
		{
			gameObject.name = "RacingArrow";
			gameObject.transform.parent = AvAvatar.pAvatarCamTransform;
			mArrowComp = gameObject.GetComponent<Ob3DArrow>();
			gameObject.transform.localPosition = _DirectionArrowOffset;
			mArrowComp._ShowAlways = true;
		}
	}

	public void UpdateDirectionArrow(Vector3 inDirection)
	{
		if (!(mArrowComp == null) && mArrowComp.pVisible)
		{
			if (mArrowComp.pTarget == null)
			{
				mArrowComp.pTarget = AvAvatar.pObject;
			}
			if (mArrowComp.transform.parent == null && AvAvatar.pAvatarCamTransform != null)
			{
				mArrowComp.transform.parent = AvAvatar.pAvatarCamTransform;
				mArrowComp.transform.localPosition = _DirectionArrowOffset;
			}
			if (mArrowComp._TargetDirection != inDirection)
			{
				mArrowComp._TargetDirection = inDirection;
			}
		}
	}

	public void SetPausedState(bool isPause)
	{
	}

	private void ShowMMOInitDB()
	{
		mGameInitDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDBLgRacing", "InitMMO");
		if (mGameInitDB != null)
		{
			mGameInitDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: false);
			mGameInitDB.SetDestroyOnClick(isDestroy: false);
			AddPlayerReady();
		}
	}

	private void AddPlayerReady()
	{
		if (mGameInitDB == null)
		{
			return;
		}
		int num = 1;
		foreach (PlayerData mPlayerDatum in mPlayerData)
		{
			KAWidget kAWidget = mGameInitDB.FindItem("MMO" + num);
			if (kAWidget != null)
			{
				kAWidget.name = mPlayerDatum.mName;
			}
		}
	}

	public void ShowQuitConfirmDB(bool isForceQuit = false)
	{
		AvAvatar.pState = AvAvatarState.PAUSED;
		mGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDBSm", "QuitConfirmDB");
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: false);
		}
		if (!isForceQuit)
		{
			mGenericDB.SetMessage(base.gameObject, "QuitRace", "CloseDB", "", "");
			mGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
			if (ChallengeInfo.pActiveChallenge != null)
			{
				mGenericDB.SetText(_ExitChallengeText.GetLocalizedString(), interactive: false);
			}
			else if (RacingManager.pIsSinglePlayer || RacingManager.Instance._PenaltyDurationGroup.Length == 0)
			{
				mGenericDB.SetText(_ExitSinglePlayerRaceText.GetLocalizedString(), interactive: false);
			}
			else
			{
				mGenericDB.SetText(_ExitMultiPlayerRaceText.GetLocalizedString(), interactive: false);
				RacingManager.Instance.TrySetFinishReason(DNFType.RaceExit);
			}
		}
		else
		{
			mGenericDB.SetMessage(base.gameObject, "", "", "ForceQuitRace", "");
			mGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			mGenericDB.SetText(_ForceExitRaceText.GetLocalizedString(), interactive: false);
		}
		mGenericDB.SetExclusive();
	}

	private void QuitRace()
	{
		EnableDragonInput(inEnable: true);
		AvAvatarController.mForceBraking = false;
		mRacingBegan = false;
		mRacingFinished = true;
		if (RacingManager.Instance != null)
		{
			RacingManager.Instance.State = RacingManagerState.RaceFinish;
		}
		if (_MiniMapManager != null)
		{
			_MiniMapManager.EnableMiniMapCamera(active: false);
		}
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: false);
		}
		if (mGenericDB != null)
		{
			UnityEngine.Object.Destroy(mGenericDB.gameObject);
			AvAvatar.pState = AvAvatar.pPrevState;
		}
		if (ChallengeInfo.pActiveChallenge != null)
		{
			ChallengeInfo.CheckForChallengeCompletion(_UiRaceResults._GameID, _TrackID, 0, ChallengeInfo.pActiveChallenge.Points + 1, isTimerUsedAsPoints: true);
			ChallengeInfo.pActiveChallenge = null;
		}
		if (!RacingManager.pIsSinglePlayer)
		{
			if (UiAvatarControls.pInstance != null)
			{
				UiAvatarControls.pInstance.EnableDragonFireButton(inEnable: false);
			}
			if (!mError)
			{
				KAUICursorManager.SetDefaultCursor("Loading");
				_UiRaceResults._ResultUI.SetAdRewardData(_GameModuleName, 0);
				WsWebService.ApplyPayout(_GameModuleName, 0, ServiceEventHandler, null);
			}
			else
			{
				DestroyRacingComp();
				RacingManager.Instance.ExitRacing();
			}
		}
		else
		{
			DestroyRacingComp();
			RacingManager.Instance.ExitRacing();
		}
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType != WsServiceType.APPLY_PAYOUT)
		{
			return;
		}
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			AchievementReward[] array = (AchievementReward[])inObject;
			if (array != null)
			{
				AchievementReward[] array2 = array;
				foreach (AchievementReward achievementReward in array2)
				{
					switch (achievementReward.PointTypeID)
					{
					case 2:
						Money.AddToGameCurrency(achievementReward.Amount.Value);
						break;
					case 8:
						SanctuaryManager.pInstance.AddXP(achievementReward.Amount.Value);
						break;
					case 1:
						UserRankData.AddPoints(achievementReward.Amount.Value);
						break;
					case 11:
						UserRankData.AddPoints(11, achievementReward.Amount.Value);
						break;
					}
				}
			}
			DestroyRacingComp();
			KAUICursorManager.SetDefaultCursor("Arrow");
			RacingManager.Instance.ExitRacing();
			break;
		}
		case WsServiceEvent.ERROR:
			UtDebug.Log("Error fetching reward Data!!!");
			KAUICursorManager.SetDefaultCursor("Arrow");
			RacingManager.Instance.ExitRacing();
			break;
		}
	}

	private void OnApplicationQuit()
	{
		if (!RacingManager.pIsSinglePlayer)
		{
			_UiRaceResults._ResultUI.SetAdRewardData(_GameModuleName, 0);
			WsWebService.ApplyPayout(_GameModuleName, 0, null, null);
		}
	}

	private void CloseDB()
	{
		if (mGenericDB != null)
		{
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.SetTimedTaskUpdate(inState: true, inForceUpdate: true);
			}
			AvAvatar.pState = AvAvatar.pPrevState;
			UnityEngine.Object.Destroy(mGenericDB.gameObject);
			mGenericDB = null;
			RacingManager.Instance.TrySetFinishReason(DNFType.Default);
		}
	}

	public void DestroyRacingComp()
	{
		foreach (PlayerData mPlayerDatum in mPlayerData)
		{
			if (mPlayerDatum.mAvatarRacing != null)
			{
				UnityEngine.Object.Destroy(mPlayerDatum.mAvatarRacing);
			}
		}
	}

	public void OnConsumableUpdated(Consumable inConsumable)
	{
		if (_PowerUpManager != null)
		{
			PowerUp powerUp = _PowerUpManager.InitPowerUp(inConsumable.name, isMMO: false);
			if (powerUp != null)
			{
				powerUp.Activate();
			}
			else
			{
				UtDebug.LogError("Power Up is null");
			}
		}
	}

	public void ShowKAUIDialog(string assetName, string dbName, string yesMessage, string noMessage, string okMessage, string closeMessage, bool destroyDB, LocaleString localeString, GameObject msgObject = null)
	{
		if (mGenericDB != null)
		{
			DestroyKAUIDB();
		}
		mGenericDB = GameUtilities.CreateKAUIGenericDB(assetName, dbName);
		if (mGenericDB != null)
		{
			if (msgObject == null)
			{
				msgObject = base.gameObject;
			}
			mGenericDB.SetMessage(msgObject, yesMessage, noMessage, okMessage, closeMessage);
			mGenericDB.SetDestroyOnClick(destroyDB);
			mGenericDB.SetButtonVisibility(!string.IsNullOrEmpty(yesMessage), !string.IsNullOrEmpty(noMessage), !string.IsNullOrEmpty(okMessage), !string.IsNullOrEmpty(closeMessage));
			mGenericDB.SetTextByID(localeString._ID, localeString._Text, interactive: false);
			KAUI.SetExclusive(mGenericDB);
		}
	}

	public void DestroyKAUIDB()
	{
		if (!(mGenericDB == null))
		{
			KAUI.RemoveExclusive(mGenericDB);
			UnityEngine.Object.DestroyImmediate(mGenericDB.gameObject);
			mGenericDB = null;
		}
	}

	private void OnDestroy()
	{
		AvAvatarController.mForceBraking = false;
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: true);
		}
	}

	private void OnGUI()
	{
		if (_ShowProbabilityChart)
		{
			mWindowRect = GUI.Window(5, mWindowRect, DoMyWindow, "Probability Chart");
		}
		if (_ShowPlayerStatus)
		{
			mWindowRect = GUI.Window(5, mWindowRect, DoMyWindow, "Player Status");
		}
	}

	private void DoMyWindow(int windowID)
	{
		if (_ShowProbabilityChart)
		{
			PlayerData mainPlayer = GetMainPlayer();
			ConsumableType consumableTypeByGame = ConsumableData.GetConsumableTypeByGame("Racing", "Game");
			if (consumableTypeByGame != null && mainPlayer != null && mainPlayer.mAvatarRacing != null)
			{
				float num = 30f;
				Consumable[] consumables = consumableTypeByGame.Consumables;
				foreach (Consumable consumable in consumables)
				{
					if (consumable != null && (consumable.Mode == 2 || consumable.Mode == GetGameMode()))
					{
						GUI.Label(new Rect(20f, num, 200f, 20f), consumable.name + " : " + consumable.Probabilities[mainPlayer.mAvatarRacing.pPosition].Val + "%");
						num += 20f;
					}
				}
			}
		}
		if (_ShowPlayerStatus)
		{
			float num2 = 30f;
			foreach (PlayerData mPlayerDatum in mPlayerData)
			{
				if (mPlayerDatum != null && mPlayerDatum.mAvatar != null)
				{
					GUI.Label(new Rect(0f, num2, 200f, 20f), mPlayerDatum.mName + " : " + mPlayerDatum.mAvatar.pIsSanctuaryPetReady);
					num2 += 20f;
				}
			}
			GUI.Label(new Rect(0f, num2, 200f, 20f), "Ready status = " + mAllReady);
		}
		GUI.DragWindow(new Rect(0f, 0f, 10000f, 10000f));
	}

	public bool CanApplyEffect(MMOMessageReceivedEventArgs args, string powerUpType)
	{
		AvatarRacing component = AvAvatar.pObject.GetComponent<AvatarRacing>();
		WeaponManager componentInChildren = SanctuaryManager.pCurPetInstance.GetComponentInChildren<WeaponManager>();
		if (component != null && !RacingManager.pIsSinglePlayer && componentInChildren != null)
		{
			PlayerData playerFromUserName = GetPlayerFromUserName(args.MMOMessage.Sender.Username);
			foreach (PlayerData pPlayerDatum in pPlayerData)
			{
				if (pPlayerDatum == playerFromUserName && componentInChildren.IsValidTarget(pPlayerDatum.mAvatar.transform))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CanActivatePowerUp(string powerUpType)
	{
		if (AvAvatar.pObject.GetComponent<AvatarRacing>() != null && !RacingManager.pIsSinglePlayer)
		{
			return true;
		}
		return false;
	}

	public bool IsMMOUser(MMOMessageReceivedEventArgs args)
	{
		if (args != null && !RacingManager.pIsSinglePlayer)
		{
			if (args.MMOMessage.Sender.Username == ProductConfig.pToken || args.MMOMessage.Sender.Username == WsWebService.pUserToken)
			{
				return false;
			}
			if (GetPlayerFromUserName(args.MMOMessage.Sender.Username) != null)
			{
				return true;
			}
		}
		return false;
	}

	public GameObject GetShieldParent(MMOMessageReceivedEventArgs args)
	{
		if (args != null && !RacingManager.pIsSinglePlayer)
		{
			if (args.MMOMessage.Sender.Username == ProductConfig.pToken || args.MMOMessage.Sender.Username == WsWebService.pUserToken)
			{
				return AvAvatar.pObject;
			}
			PlayerData playerFromUserName = GetPlayerFromUserName(args.MMOMessage.Sender.Username);
			if (playerFromUserName != null)
			{
				return playerFromUserName.mAvatarRacing.gameObject;
			}
		}
		return null;
	}

	public RacingPositionSensor GetNearestRacingPositionSensor(Vector3 inPosition)
	{
		RacingPositionSensor result = null;
		float num = float.PositiveInfinity;
		for (int i = 0; i < _RacingPositionSensors.Count; i++)
		{
			float num2 = Vector3.Distance(inPosition, _RacingPositionSensors[i].transform.position);
			if (num2 < num)
			{
				num = num2;
				result = _RacingPositionSensors[i];
			}
		}
		return result;
	}

	public Vector3 GetNearestPointOnLine(Vector3 linePoint, Vector3 lineDirection, Vector3 point)
	{
		lineDirection.Normalize();
		float num = Vector3.Dot(point - linePoint, lineDirection);
		return linePoint + lineDirection * num;
	}

	public float GetRaceCompletedAmount(Vector3 inPosition, int currentLap, int currentCheckpointID)
	{
		RacingPositionSensor nearestRacingPositionSensor = GetNearestRacingPositionSensor(inPosition);
		Vector3 position = nearestRacingPositionSensor._NextSensor.transform.position;
		Vector3 position2 = nearestRacingPositionSensor._LastSensor.transform.position;
		RacingCheckpoint racingCheckpoint = ((currentCheckpointID == 0) ? _RacingCheckpoints[currentCheckpointID] : _RacingCheckpoints[currentCheckpointID - 1]);
		if (racingCheckpoint._ValidRacingSensors.Contains(nearestRacingPositionSensor))
		{
			Vector3 lineDirection = position - position2;
			Vector3 nearestPointOnLine = GetNearestPointOnLine(position2, lineDirection, inPosition);
			float a = Vector3.Distance(position2, position);
			float value = Vector3.Distance(nearestPointOnLine, position);
			float num = Mathf.InverseLerp(a, 0f, value);
			float num2 = (float)(currentCheckpointID * 100 + nearestRacingPositionSensor.pIndex) + num;
			num2 += (float)(currentLap * 1000);
			UtDebug.Log("Completed amount is: " + num2);
			return num2;
		}
		return (float)(currentCheckpointID * 100 + racingCheckpoint._ValidRacingSensors[0].pIndex) + (float)(currentLap * 1000);
	}
}
