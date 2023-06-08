using System;
using System.Collections;
using System.Collections.Generic;
using KnowledgeAdventure.Multiplayer.Events;
using KnowledgeAdventure.Multiplayer.Model;
using UnityEngine;

public class KAUiBejeweled : KAUI
{
	public delegate void BoosterDelegate(ElementMatchGame.BoosterType intype);

	private KAWidget mTxtHighScore;

	private KAWidget mTxtTimer;

	private KAToggleButton mPauseBtn;

	private KAWidget mTimerBar;

	private KAWidget mHintArrow;

	private KAWidget mSelectBox;

	private KAWidget mMultiplayerModesOverlay;

	private KAWidget mMMOLobbyOverlay;

	private KAWidget mWelcomePlayBtn;

	private KAWidget mResumeBtn;

	private KAWidget mExitBtn;

	private KAWidget mGameModeOverlay;

	private KAWidget mBtnMultiplayerMode;

	private KAWidget mBtnSinglePlayerMode;

	private KAWidget mInstructText;

	private KAWidget mPauseOverlay;

	private KAWidget mTurnTimerBar;

	private bool mObjSelected;

	private KAUIGenericDB mUiGenericDB;

	private ElementMatchMMOClient mMMOClient;

	public HotKey[] _HotKeys;

	[NonSerialized]
	public int pGamePlayTime;

	public int pTurnTime;

	public LocaleString _PurchaseBoosterText = new LocaleString("Purchase Boosters to help you in the game");

	public UiCountDown _UiCountDown;

	public string _ExitMarker = "pfAvatarStartMarker";

	public string _ExitLevel = "HubSchoolDO";

	public float _InfoDisplayTime = 2f;

	private Queue<string> mInfoMessageQueue = new Queue<string>();

	private float mTextSetTime;

	public UiBuyPopup _BoosterBuyUi;

	public float _TimerFlashFrequency = 0.4f;

	private bool mFlashing;

	private float mTimeDelay;

	private bool mBuyAtStart;

	private static KAUiBejeweled mInstance;

	public LocaleString _MMODisabledOnDeviceText = new LocaleString("MMO disabled. You can enable it from settings.");

	public LocaleString _MMODisabledOnServerText = new LocaleString("MMO disabled. You can enable it from web account.");

	public UiGSBuddySelect _BuddySelectScreen;

	public LocaleString _ReadyText;

	public LocaleString _NotReadyText;

	public AudioClip[] _HelpVO;

	private KAWidget mBtnBack;

	private KAWidget mBtnHelp;

	private KAWidget mBtnReady;

	private KAWidget mBtnNotReady;

	private KAWidget mBtnInvite;

	private KAWidget mTxtOpponentStatus;

	private KAWidget[] mStatus;

	private KAWidget[] mName;

	private KAWidget[] mPicture;

	private KAToggleButton mBtnTrainingLevel;

	public static KAUiBejeweled pInstance => mInstance;

	public static event BoosterDelegate UseBooster;

	public void Initialize()
	{
		mTxtHighScore = FindItem("TxtHighScore");
		mTxtTimer = FindItem("TxtTimer");
		mTimerBar = FindItem("TimerBar");
		mPauseBtn = (KAToggleButton)FindItem("PauseBtn");
		mMultiplayerModesOverlay = FindItem("MultiplayerModesOverlay");
		mMMOLobbyOverlay = FindItem("MMOLobbyOverlay");
		mWelcomePlayBtn = FindItem("WelcomePlayBtn");
		mResumeBtn = FindItem("BtnPauseResume");
		mExitBtn = FindItem("BtnPauseExit");
		mGameModeOverlay = FindItem("GameModeOverlay");
		mInstructText = FindItem("TxtMessage");
		mInstance = this;
		mInfoMessageQueue.Enqueue("");
		mTxtHighScore.SetText("0");
		mPauseOverlay = FindItem("PauseOverlay");
		mTurnTimerBar = FindItem("TurnTimerBar");
		KAWidget kAWidget = FindItem("ChallengeInfo");
		if (null != kAWidget)
		{
			kAWidget.SetVisibility(inVisible: false);
		}
		InitGameModesScreen();
		if (null != mTurnTimerBar)
		{
			mTurnTimerBar.AttachToCursor(0f, 0f);
			mTurnTimerBar.SetVisibility(inVisible: false);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mExitBtn)
		{
			((ElementMatchGame)TileMatchPuzzleGame.pInstance).ExitGame();
		}
		else if (inWidget == mWelcomePlayBtn)
		{
			CloseBoosterBuy();
		}
		else if (inWidget == mPauseBtn)
		{
			PauseGame(pause: true);
		}
		else if (inWidget == mResumeBtn)
		{
			PauseGame(pause: false);
			if (mObjSelected)
			{
				mSelectBox.SetVisibility(inVisible: true);
			}
		}
		else if (inWidget.name == "Boost02Btn")
		{
			if (KAUiBejeweled.UseBooster != null)
			{
				KAUiBejeweled.UseBooster(ElementMatchGame.BoosterType.BOOSTER_SPIKE);
			}
			else
			{
				Debug.Log("Match 3: null == UseBooster");
			}
		}
		else if (inWidget.name == "Boost03Btn")
		{
			if (KAUiBejeweled.UseBooster != null)
			{
				KAUiBejeweled.UseBooster(ElementMatchGame.BoosterType.BOOSTER_TERROR);
			}
			else
			{
				Debug.Log("Match 3: null == UseBooster");
			}
		}
		else if (null != mGameModeOverlay && mGameModeOverlay.GetVisibility())
		{
			ModeSelectOnClick(inWidget);
		}
		else if (null != mMultiplayerModesOverlay && mMultiplayerModesOverlay.GetVisibility())
		{
			MultiplayerModeSelectOnClick(inWidget);
		}
		else if (null != mMMOLobbyOverlay && mMMOLobbyOverlay.GetVisibility())
		{
			MMOLobbyOnClick(inWidget);
		}
	}

	private void CloseBoosterBuy()
	{
		_BoosterBuyUi.SetVisibility(isVisible: false);
		if (null != _BoosterBuyUi)
		{
			UiBuyPopup.OnBoosterBuyClose -= CloseBoosterBuy;
		}
		if (mBuyAtStart)
		{
			_UiCountDown.SetVisibility(inVisible: true);
			_UiCountDown.StartCountDown(inStart: true);
			mTextSetTime = _InfoDisplayTime;
			mBuyAtStart = false;
		}
		else
		{
			TileMatchPuzzleGame.pInstance.SetGameState(TileMatchPuzzleGame.GameState.PLAYING);
		}
	}

	protected override void Start()
	{
		base.Start();
		Initialize();
		ShowKeys(!UtPlatform.IsMobile());
		if (null != mHintArrow)
		{
			mHintArrow.SetVisibility(inVisible: false);
		}
		DisableBtns();
		if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pIsMounted)
		{
			SanctuaryManager.pCurPetInstance.OnFlyDismountImmediate(AvAvatar.pObject);
		}
		if (null != _UiCountDown)
		{
			_UiCountDown.OnCountdownDone += HandleOnCountdownDone;
		}
		TileMatchPuzzleGame.RegisterGameStateCallback(OnGameStateChangeDelegate);
		StartCoroutine(DelayedGameStart());
	}

	private IEnumerator DelayedGameStart()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		ChallengeInfo pActiveChallenge = ChallengeInfo.pActiveChallenge;
		if (mGameModeOverlay != null && pActiveChallenge == null)
		{
			mGameModeOverlay.SetVisibility(inVisible: true);
		}
		else
		{
			StartSinglePlayerGame();
		}
	}

	private void OnGameStateChangeDelegate(TileMatchPuzzleGame.GameState inNewState, TileMatchPuzzleGame.GameState inPrvState)
	{
		ElementMatchGame elementMatchGame = (ElementMatchGame)TileMatchPuzzleGame.pInstance;
		switch (inNewState)
		{
		case TileMatchPuzzleGame.GameState.PAUSED:
			DisableBtns();
			elementMatchGame.UpdateBoosterUI(forceDisable: true);
			break;
		case TileMatchPuzzleGame.GameState.PLAYING:
			EnableBtns();
			if (elementMatchGame.IsTutorialComplete())
			{
				elementMatchGame.UpdateBoosterUI();
			}
			else if (elementMatchGame._TutManager.GetStepName() == "UsingBooster")
			{
				elementMatchGame.UpdateBoosterUI();
			}
			else
			{
				elementMatchGame.UpdateBoosterUI(forceDisable: true);
			}
			break;
		}
	}

	private void ShowBoosterBuyAtStart()
	{
		mBuyAtStart = true;
		ShowBoosterBuy();
	}

	public void StartSinglePlayerGame()
	{
		ElementMatchGame elementMatchGame = (ElementMatchGame)TileMatchPuzzleGame.pInstance;
		if (UiSocialBib.pInstance != null)
		{
			UiSocialBib.pInstance.SetVisibility(inVisible: false);
		}
		elementMatchGame.pGameMode = ElementMatchGame.GameModes.SINGLE_PLAYER;
		SetTurnTime(0f);
		if (null == elementMatchGame._TutManager || elementMatchGame._TutManager.TutorialComplete())
		{
			SetVisibility(inVisible: true);
			ShowBoosterBuyAtStart();
			TileMatchPuzzleGame.RegisterScoreChangeCallback(OnScoreUpdate);
			DisableBtns();
			ShowInstructText("");
		}
		else
		{
			TileMatchPuzzleGame.RegisterScoreChangeCallback(OnScoreUpdate);
			DisableBtns();
			elementMatchGame.StartTut();
		}
	}

	public void ShowBoosterBuy()
	{
		if (null != _BoosterBuyUi)
		{
			TileMatchPuzzleGame.pInstance.SetGameState(TileMatchPuzzleGame.GameState.PAUSED);
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.SetTimedTaskUpdate(inState: false);
			}
			_BoosterBuyUi.SetVisibility(isVisible: true);
			UiBuyPopup.OnBoosterBuyClose += CloseBoosterBuy;
		}
		mInstructText.SetText(_PurchaseBoosterText.GetLocalizedString());
	}

	public void OnScoreUpdate(float inNewScore)
	{
		ElementMatchGame elementMatchGame = (ElementMatchGame)TileMatchPuzzleGame.pInstance;
		if (!(null != elementMatchGame) || !(null != elementMatchGame._TutManager) || elementMatchGame._TutManager.TutorialComplete())
		{
			UpdateScore((int)inNewScore);
			if (elementMatchGame.pGameMode == ElementMatchGame.GameModes.HEAD_TO_HEAD)
			{
				UiSocialBib.SetScore((int)inNewScore, isOpponent: false);
			}
		}
	}

	private void HandleOnCountdownDone()
	{
		_UiCountDown.SetVisibility(inVisible: false);
		if (null != mTurnTimerBar)
		{
			mTurnTimerBar.SetVisibility(inVisible: true);
		}
		mTurnTimerBar.AttachToCursor(0f, 0f);
		EnableBtns();
	}

	protected override void Update()
	{
		base.Update();
		if (_HotKeys != null && _HotKeys.Length != 0)
		{
			HotKey[] hotKeys = _HotKeys;
			foreach (HotKey hotKey in hotKeys)
			{
				if (KAInput.GetButtonUp(hotKey._Key))
				{
					OnClick(hotKey._Widget);
					break;
				}
			}
		}
		if (mInfoMessageQueue.Count > 0)
		{
			mTextSetTime -= Time.deltaTime;
			if (mTextSetTime < 0.01f)
			{
				mInstructText.SetText(mInfoMessageQueue.Dequeue());
				mTextSetTime = _InfoDisplayTime;
			}
		}
		else if (!string.IsNullOrEmpty(mInstructText.GetText()))
		{
			mTextSetTime -= Time.deltaTime;
			if (mTextSetTime < 0.01f)
			{
				mInstructText.SetText("");
			}
		}
		if (mFlashing)
		{
			FlashTurnTimer();
		}
	}

	public void PlayAgain()
	{
		SetInteractive(interactive: true);
		TileMatchPuzzleGame.UnRegisterScoreChangeCallback(OnScoreUpdate);
		if (mGameModeOverlay != null)
		{
			mGameModeOverlay.SetVisibility(inVisible: true);
		}
		else
		{
			StartSinglePlayerGame();
		}
		UpdateScore(0);
	}

	public void StartNewGame()
	{
		_UiCountDown.SetVisibility(inVisible: true);
		_UiCountDown.StartCountDown(inStart: true);
	}

	public void SetTime(float time)
	{
		float progressLevel = time / (float)pGamePlayTime;
		int num = (int)time / 60;
		int num2 = (int)time % 60;
		mTxtTimer.SetText(num.ToString("d2") + ":" + num2.ToString("d2"));
		mTimerBar.SetProgressLevel(progressLevel);
	}

	public void SetTurnTime(float leftTime)
	{
		float progressLevel = leftTime / (float)pTurnTime;
		mTurnTimerBar.SetProgressLevel(progressLevel);
	}

	public void UpdateScore(int score)
	{
		mTxtHighScore.SetText(score.ToString());
	}

	public void ExitGame()
	{
		TileMatchPuzzleGame.RegisterGameStateCallback(OnGameStateChangeDelegate);
		TileMatchPuzzleGame.UnRegisterScoreChangeCallback(OnScoreUpdate);
		if (null != _UiCountDown)
		{
			_UiCountDown.OnCountdownDone -= HandleOnCountdownDone;
		}
		base.gameObject.SetActive(value: false);
		TutorialManager.StopTutorials();
		if (_ExitMarker != "")
		{
			AvAvatar.pStartLocation = _ExitMarker;
		}
		AvAvatar.SetActive(inActive: false);
		RsResourceManager.LoadLevel(_ExitLevel);
	}

	private void PauseGame(bool pause)
	{
		if (pause)
		{
			KAUI.SetExclusive(this);
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.SetTimedTaskUpdate(inState: false);
			}
			mPauseOverlay.SetVisibility(inVisible: true);
			TileMatchPuzzleGame.pInstance.SetGameState(TileMatchPuzzleGame.GameState.PAUSED);
		}
		else
		{
			KAUI.RemoveExclusive(this);
			ElementMatchGame elementMatchGame = (ElementMatchGame)TileMatchPuzzleGame.pInstance;
			if (MissionManager.pInstance != null && elementMatchGame.IsTutorialComplete())
			{
				MissionManager.pInstance.SetTimedTaskUpdate(inState: true, inForceUpdate: true);
			}
			mPauseOverlay.SetVisibility(inVisible: false);
			TileMatchPuzzleGame.pInstance.SetGameState(TileMatchPuzzleGame.GameState.PLAYING);
		}
		if (null != mTurnTimerBar)
		{
			mTurnTimerBar.SetVisibility(!pause);
		}
	}

	public void EnableBtns()
	{
		mPauseBtn.SetInteractive(isInteractive: true);
	}

	public void DisableBtns()
	{
		mPauseBtn.SetInteractive(isInteractive: false);
	}

	public void ChallengeItemVisible(bool show)
	{
		FindItem("ChallengeInfo").SetVisibility(show);
	}

	public void SetChallengeScore(int score)
	{
		KAWidget kAWidget = FindItem("ChallengeScore");
		if (null != kAWidget)
		{
			kAWidget.SetText(score.ToString());
		}
	}

	public void SetChallengeText(string message)
	{
		KAWidget kAWidget = FindItem("ChallengeInfo");
		if (null != kAWidget)
		{
			kAWidget.SetText(message);
		}
	}

	public void UpdateBooster(ElementMatchGame.BoosterType inType, int count, bool forceDisable)
	{
		KAWidget kAWidget = null;
		switch (inType)
		{
		case ElementMatchGame.BoosterType.BOOSTER_SPIKE:
			kAWidget = FindItem("Boost02Btn");
			break;
		case ElementMatchGame.BoosterType.BOOSTER_TERROR:
			kAWidget = FindItem("Boost03Btn");
			break;
		}
		if (forceDisable)
		{
			kAWidget.SetDisabled(forceDisable);
		}
		else
		{
			kAWidget.SetDisabled(isDisabled: false);
		}
		kAWidget.SetText(count.ToString());
	}

	private void InitGameModesScreen()
	{
		mBtnMultiplayerMode = FindItem("BtnMultiplayerMode");
		mBtnSinglePlayerMode = FindItem("BtnSinglePlayerMode");
	}

	private void MultiplayerModeSelectOnClick(KAWidget item)
	{
		if (item.name == "BtnBackToModeSelect" && null != mGameModeOverlay)
		{
			mMultiplayerModesOverlay.SetVisibility(inVisible: false);
			mGameModeOverlay.SetVisibility(inVisible: true);
		}
		else if (item.name == "BtnJoin")
		{
			InitMMOLobbyScreen();
			mMultiplayerModesOverlay.SetVisibility(inVisible: false);
			mMMOLobbyOverlay.SetVisibility(inVisible: true);
			mMMOClient.JoinGameRoom(ElementMatchMMOClient.ElementMatchRoomTypes.JOIN_ANY);
		}
		else if (item.name == "BtnStart")
		{
			InitMMOLobbyScreen();
			mMultiplayerModesOverlay.SetVisibility(inVisible: false);
			mMMOLobbyOverlay.SetVisibility(inVisible: true);
			mMMOClient.JoinGameRoom(ElementMatchMMOClient.ElementMatchRoomTypes.HOST_FOR_ANY);
		}
		else if (item.name == "BtnPlayAFriend" && null != mMMOLobbyOverlay)
		{
			InitMMOLobbyScreen();
			mMultiplayerModesOverlay.SetVisibility(inVisible: false);
			mMMOLobbyOverlay.SetVisibility(inVisible: true);
			mMMOClient.JoinGameRoom(ElementMatchMMOClient.ElementMatchRoomTypes.HOST_FOR_BUDDY);
		}
	}

	private void ModeSelectOnClick(KAWidget item)
	{
		if (item == mBtnMultiplayerMode)
		{
			if (!UserInfo.pInstance.MultiplayerEnabled)
			{
				ShowMessage(_MMODisabledOnServerText);
			}
			else if (!MainStreetMMOClient.pIsMMOEnabled)
			{
				ShowMessage(_MMODisabledOnDeviceText);
			}
			else
			{
				if (!(mMultiplayerModesOverlay != null))
				{
					return;
				}
				mGameModeOverlay.SetVisibility(inVisible: false);
				mMultiplayerModesOverlay.SetVisibility(inVisible: true);
				if (mMMOClient == null)
				{
					mMMOClient = InitMMOClient<ElementMatchMMOClient>("ElementMatchMMOClient");
					Debug.Log("Initialized MMO Client");
					if (MainStreetMMOClient.pInstance != null && MainStreetMMOClient.pInstance.pState == MMOClientState.DISCONNECTING)
					{
						MainStreetMMOClient.pInstance.Connect();
					}
					else
					{
						MainStreetMMOClient.Init();
					}
				}
			}
		}
		else if (item == mBtnSinglePlayerMode)
		{
			mGameModeOverlay.SetVisibility(inVisible: false);
			StartSinglePlayerGame();
		}
		else if (item.name == "CloseBtn")
		{
			((ElementMatchGame)TileMatchPuzzleGame.pInstance).ExitGame();
		}
	}

	public static T InitMMOClient<T>(string inGameObjectName) where T : MMOClient
	{
		T val = new GameObject(inGameObjectName).AddComponent<T>();
		MainStreetMMOClient.AddClient(val);
		return val;
	}

	public void ShowInstructText(string text)
	{
		mInfoMessageQueue.Enqueue(text);
	}

	public void ClearMessageQueue()
	{
		mInstructText.SetText(string.Empty);
		mInfoMessageQueue.Clear();
	}

	private void ShowMessage(LocaleString inText)
	{
		if (mUiGenericDB == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSm"));
			mUiGenericDB = gameObject.GetComponent<KAUIGenericDB>();
			mUiGenericDB._MessageObject = base.gameObject;
			mUiGenericDB._CloseMessage = "OnCloseDB";
			mUiGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: true);
			mUiGenericDB.SetTextByID(inText._ID, inText._Text, interactive: false);
			KAUI.SetExclusive(mUiGenericDB, new Color(0.5f, 0.5f, 0.5f, 0.5f));
		}
	}

	public void OnCloseDB()
	{
		if (mUiGenericDB != null)
		{
			KAUI.RemoveExclusive(mUiGenericDB);
			UnityEngine.Object.Destroy(mUiGenericDB.gameObject);
			mUiGenericDB = null;
		}
	}

	private void FlashTurnTimer()
	{
		mTimeDelay -= Time.deltaTime;
		if (mTimeDelay < 0.1f)
		{
			mTimeDelay = _TimerFlashFrequency;
			if (null != mTurnTimerBar)
			{
				mTurnTimerBar.SetVisibility(!mTurnTimerBar.GetVisibility());
			}
		}
	}

	public void PlayFlash(bool show)
	{
		mFlashing = show;
		if (mFlashing)
		{
			mTimeDelay = _TimerFlashFrequency;
		}
		else
		{
			mTimeDelay = 0f;
		}
	}

	public void ShowTutorialArrow(Vector3 inPosition, bool left, bool right, bool up, bool down)
	{
		KAWidget kAWidget = FindItem("TutArrowLeft");
		KAWidget kAWidget2 = null;
		if (null != kAWidget)
		{
			kAWidget.SetVisibility(left);
		}
		if (left)
		{
			kAWidget2 = kAWidget;
		}
		kAWidget = FindItem("TutArrowRight");
		if (null != kAWidget)
		{
			kAWidget.SetVisibility(right);
		}
		if (right)
		{
			kAWidget2 = kAWidget;
		}
		kAWidget = FindItem("TutArrowUp");
		if (null != kAWidget)
		{
			kAWidget.SetVisibility(up);
		}
		if (up)
		{
			kAWidget2 = kAWidget;
		}
		kAWidget = FindItem("TutArrowDown");
		if (null != kAWidget)
		{
			kAWidget.SetVisibility(down);
		}
		if (down)
		{
			kAWidget2 = kAWidget;
		}
		Vector3 vector = Camera.main.WorldToScreenPoint(inPosition);
		if ((bool)kAWidget2)
		{
			kAWidget2.SetToScreenPosition(vector.x, (float)Screen.height - vector.y);
		}
	}

	private void ShowKeys(bool isVisible)
	{
		HotKey[] hotKeys = _HotKeys;
		for (int i = 0; i < hotKeys.Length; i++)
		{
			hotKeys[i]._Widget.FindChildItem("TxtKey").SetVisibility(isVisible);
		}
	}

	private void InitMMOLobbyScreen()
	{
		mBtnBack = FindItem("btnBack");
		mBtnHelp = FindItem("btnHelp");
		mBtnReady = FindItem("btnReady");
		mBtnNotReady = FindItem("btnNotReady");
		mBtnInvite = FindItem("btnInvite");
		mTxtOpponentStatus = FindItem("txtOpponentStatus");
		mBtnTrainingLevel = (KAToggleButton)FindItem("btnTrainingLevel");
		mStatus = new KAWidget[2];
		mStatus[0] = FindItem("txtPlayerStatus");
		mStatus[1] = FindItem("txtOpponentStatus");
		mName = new KAWidget[2];
		mName[0] = FindItem("txtPlayerName");
		mName[1] = FindItem("txtOpponentName");
		mPicture = new KAWidget[2];
		mPicture[0] = FindItem("PicPlayer");
		mPicture[1] = FindItem("PicOpponent");
		mMMOClient.OnMessageEvent += MMOMessageEventHandler;
	}

	private void MMOLobbyOnClick(KAWidget item)
	{
		if (item == mBtnBack && null != mGameModeOverlay)
		{
			mMMOClient.OnMessageEvent += MMOMessageEventHandler;
			mMultiplayerModesOverlay.SetVisibility(inVisible: true);
			mGameModeOverlay.SetVisibility(inVisible: false);
			mMMOLobbyOverlay.SetVisibility(inVisible: false);
			mMMOClient.LeaveGameRoom();
		}
		else if (item == mBtnHelp)
		{
			if (_HelpVO != null)
			{
				int num = UnityEngine.Random.Range(0, _HelpVO.Length);
				SnChannel.Play(_HelpVO[num], "VO_Pool", inForce: false);
			}
		}
		else if (item == mBtnReady)
		{
			mBtnReady.SetVisibility(inVisible: false);
			mBtnNotReady.SetVisibility(inVisible: true);
			mMMOClient.SetPlayerReady(inIsReady: true);
		}
		else if (item == mBtnNotReady)
		{
			mBtnReady.SetVisibility(inVisible: true);
			mBtnNotReady.SetVisibility(inVisible: false);
			mMMOClient.SetPlayerReady(inIsReady: false);
		}
		else if (item == mBtnInvite)
		{
			SetVisibility(inVisible: false);
			SetState(KAUIState.DISABLED);
			_BuddySelectScreen.SetVisibility(t: true);
		}
		else
		{
			_ = item == mBtnTrainingLevel;
		}
	}

	private void MMOMessageEventHandler(MMOMessageReceivedEventArgs args)
	{
		if (args.MMOMessage.MessageType != MMOMessageType.User)
		{
			return;
		}
		string[] array = args.MMOMessage.MessageText.Split(':');
		if (mMMOClient.IsGamePlayer(array[1]))
		{
			if (array[0] == "mm.PR")
			{
				SetPlayerReady(array[1], array[2]);
			}
			else if (array[0] == "mm.SG")
			{
				StartMultiplayerGame();
			}
		}
	}

	public void StartMultiplayerGame()
	{
		((ElementMatchGame)TileMatchPuzzleGame.pInstance).pGameMode = ElementMatchGame.GameModes.HEAD_TO_HEAD;
		if (UiSocialBib.pInstance != null)
		{
			UiSocialBib.pInstance.SetVisibility(inVisible: true);
		}
		SetVisibility(inVisible: true);
		ShowBoosterBuy();
		TileMatchPuzzleGame.RegisterScoreChangeCallback(OnScoreUpdate);
		DisableBtns();
		if (null != _UiCountDown)
		{
			_UiCountDown.OnCountdownDone += HandleOnCountdownDone;
		}
	}

	public void SetPlayerReady(string inUserId, string inIsReady)
	{
		bool flag = ((inIsReady == "True") ? true : false);
		if (mTxtOpponentStatus != null)
		{
			mTxtOpponentStatus.SetText(flag ? _ReadyText.GetLocalizedString() : _NotReadyText.GetLocalizedString());
		}
	}
}
