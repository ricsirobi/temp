using System;
using System.Collections.Generic;
using KnowledgeAdventure.Multiplayer.Events;
using UnityEngine;

public class GauntletRailShootManager : GmGameManager, IConsumable
{
	public const uint LOG_MASK = 8u;

	private static GauntletRailShootManager mInstance;

	public int _HeadToHeadAchievementID = 64;

	public int _SinglePlayerAchievementID = 155;

	public int _ClanAchievementID = 183;

	public TextAsset _TutorialAsset;

	public string _ExitMarkerName;

	public string _ExitLevelName;

	public List<GauntletLevelData> _LevelDataList;

	public GauntletMinMax _GeneratedLevelPieceCount;

	public GameObject _GauntletArm;

	public GameObject _CrossHairPrefab;

	public GauntletController _GauntletController;

	public GRSAccuracyScoreData[] _AccuracyPointsData;

	public float _GameEndScoreDisplayTime = 3f;

	public float _TopScoreDisplayTime = 3f;

	public LocaleString _GameScoreTitleText;

	public LocaleString _GameScoreText;

	public LocaleString _GameAccuracyPercentText;

	public LocaleString _GameAccuracyScoreText;

	public LocaleString _GameTotalScoreText;

	public LocaleString _TopScoreTitleText;

	public LocaleString _NoScoresYetText;

	public LocaleString _ScoreStringText;

	public LocaleString _NameStringText;

	public LocaleString _RestartGameTitleText;

	public LocaleString _RestartGameText;

	public LocaleString _MathCorrectAnswerText;

	public LocaleString _MathPointsText;

	public LocaleString _MathBonusText;

	public LocaleString _PetNotFoundText;

	public LocaleString _PetNotOlderText;

	public AudioClip _BkgMusic;

	public AudioClip _ScoreScreenVO;

	public AudioClip[] _PoseVOs;

	public GSPositiveVOCriteria _PositiveVOCriteria;

	public int _PositiveVOTargetCount = 10;

	public int _PositiveVOPointsThreshold = 2000;

	public AudioClip[] _HelpVO;

	public GameObject _MenuCameraObject;

	public UiGSLevelSelection _LevelSelectionScreen;

	public UiGSMultiplayerScreen _MultiplayerMenuScreen;

	public UiGauntletMMOLobby _MMOLobbyUI;

	public UiGSMultiplayerResults _MMOResultsUI;

	public UiGauntletShooterScore _EndScoreUI;

	public UiDragonsEndDB _EndDBUI;

	public UiChooseADragon _ChooseDragonUI;

	public TPTutorialManager _TPTutorialManager;

	public string _DefaultMMOIPAddress = "10.1.1.90";

	public int _DefaultMMOPortNumber = 9339;

	public float _ConnectionTimeout = 10f;

	public LocaleString _ClickReadyMsgText;

	public LocaleString _WaitForOthersText;

	public LocaleString _SecondsText;

	public LocaleString _LobbyCountDownText;

	public LocaleString _ConnectionTimeOutText;

	public LocaleString _ConnectionErrorText;

	public LocaleString _BuddyInviteFailedText;

	public LocaleString _OwnerLeftErrorText;

	public LocaleString _RankLockedText;

	public Color _MaskColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	public int _GameUnlockRank = 5;

	public int _SocialMultiplayerWithBuddyAchievementID = 110;

	public int _SocialMultiplayerWithOthersAchievementID = 111;

	public GameObject _PfTargetText;

	public UiGameModeScreen _GameModeScreen;

	public string _AnswerImgSize = "85x85";

	public int _CorrectTargetPercentage = 75;

	public int _NumCorrectAnswers = 5;

	public int _NumDistractAnswers = 5;

	public int _FiringFrenzyLevelID;

	public int _RatingCategoryID;

	public GUISkin _NicknameSkin;

	public LocaleString _EntranceFeeText = new LocaleString("Entrance fee of 10 coins is required to play. Pay now with 10 Coins?");

	public LocaleString _NotEnoughFeeText = new LocaleString("You do not have enough Coins to pay, buy more now?");

	public LocaleString _ChallengeTryAgainText = new LocaleString("Nice Try! Why dont you try again.");

	public LocaleString _ChallengeCompleteText = new LocaleString("Amazing! you beat [Name]'s challenge");

	public LocaleString _HTHTieMessageText = new LocaleString("It's a Tie!");

	public LocaleString _HTHWinMessageText = new LocaleString("You Won!");

	public LocaleString _HTHLostMessageText = new LocaleString("You Lost!");

	public float _MinEnergyValue = 15f;

	public int _CrossBowUnlockDurationInHours = 1;

	private KAUIGenericDB mKAUIGenericDB;

	private GameObject mPetEnergyMsgObject;

	public int _StoreID = 93;

	public LocaleString _PurchaseProcessingText = new LocaleString("Processing purchase.");

	public LocaleString _NotEnoughGemsText = new LocaleString("You do not have enough gems to pay, Please buy more!");

	public List<GSPurchaseData> _PurchaseDatas;

	public AudioClip _FireSFX;

	public AudioClip _SFXWrongTargetHit;

	public GameObject _Hit3DScorePrefab;

	public int _RetryAttemptsForNonMembers = 2;

	public int _RetryAttemptsForMembers;

	public string TIMES_PLAYED = "FF_PLAYED";

	public string UNLOCK_DURATION = "FF_TIMEREMAINING";

	public LocaleString _ExceededPlayTime = new LocaleString("You have exceeded {{NUM_ATTEMPT}} free attempts.");

	public List<GSCameraOffsetData> _AvatarCameraOffsetList;

	public int _CrossbowUnlockTaskID;

	public LocaleString _GameEndWaitText = new LocaleString("Waiting for other player to finish.");

	public const string LAST_CROSSBOW_GAMETIME_KEY = "LCGT";

	public const string CROSSBOW_TIMES_PLAYED = "CB_PLAYED";

	public UiConsumable _UiConsumable;

	public PowerUpManager _PowerUpManager;

	public Vector3 _MMOConsumablePos = new Vector3(0f, 115f, 20f);

	[NonSerialized]
	public List<GameObject> _CurrentActiveTargets = new List<GameObject>();

	private GauntletLevelData mLevelData;

	private GameObject mPreviousLevelPiece;

	private List<GameObject> mLevelPieceList = new List<GameObject>();

	private int mTrackPieceIndex;

	private int mNumCrossedTrackPiece;

	private int mNumProjectilesShot;

	private int mNumProjectilesHit;

	private Vector3 mNextPiecePos = Vector3.zero;

	private Vector3 mNextPieceDir = Vector3.zero;

	private GameObject mCurrentSplineObject;

	private bool mIsShowCountDown;

	private float mControllerPrevSpeed;

	private GRSScore[] mScores;

	private float mGameEndScoreTimer;

	private float mTopScoreDisplayTimer;

	private KAUIGenericDB mUiGenericDB;

	private bool mIsCursorVisible = true;

	private GauntletControlModifier mInControlModifier;

	private int mConsecutiveHitCount;

	private int mPositiveVOThreshold;

	private KAWidget mUiGenericDBText;

	private string mGenericDBText;

	private bool mGenericDBText_UseDots;

	private int mGenericDBText_DotCount;

	private float mGenericDBDisplayTimer;

	private float mGenericDBUpdateTimer;

	private bool mShowMultiplayerCountdown;

	private string mTxtGameScore;

	private string mTxtGameAccuracyPercent;

	private string mTxtGameAccuracyScore;

	private string mTxtGameTotalScore;

	private string mTxtTopScoreTitle;

	private string mTxtNoScoresYet;

	private string mTxtScoreString;

	private string mTxtNameString;

	private string mTxtMathCorrectAnswers;

	private KAUiGauntletShooterHUD mGameHud;

	private int mCurrentQuestionNum;

	private CmInterface mCM;

	private CmMCManyToMany mContent;

	private int mMinNumQuestionsPerRound = 10;

	private AssetBundle mAssets;

	private int mCorrectAnswersCount;

	private int mMathScore;

	private float mExtraTime;

	private float mTime;

	private GSPurchaseType mPurchaseType;

	private int mTotalScore;

	private HighScoresDifficulty mDifficulty;

	private GSGameType mGameType = GSGameType.HEADTOHEAD;

	private string mHostPlayer;

	private bool mIsGameRunning;

	private bool mIsMultiplayer;

	private GameObject mOldAvatarObject;

	private bool mIsMathContent;

	public float _ChallengeFlashTimeInterval;

	public int _ChallengeFlashLoopCount;

	public AudioClip _ChallengeCompleteSFX;

	private int mChallengePoints;

	private bool mChallengeAchieved;

	private bool mIsPlayingChallenge;

	private Vector3 mPrvScale = Vector3.one;

	private string mOldRunAnim;

	private string mOldIdleAnim;

	private bool pIsTutDone;

	public string _TutorialKey = "TPTutorialKey";

	private float mPrvFixedDT;

	private int mNumAmmoInAir;

	private int mPlayerPos;

	private GauntletMMOPlayer[] mPlayers;

	private bool mOpponentLeft;

	private PowerUpHelper mPowerUpHelper = new PowerUpHelper();

	public static GauntletRailShootManager pInstance => mInstance;

	public int pTotalScore => mTotalScore;

	public GSGameType pGameType
	{
		get
		{
			return mGameType;
		}
		set
		{
			mGameType = value;
		}
	}

	public string pHostPlayer => mHostPlayer;

	public bool pIsGameRunning
	{
		get
		{
			return mIsGameRunning;
		}
		set
		{
			mIsGameRunning = value;
		}
	}

	public bool pIsMultiplayer
	{
		get
		{
			return mIsMultiplayer;
		}
		set
		{
			mIsMultiplayer = value;
		}
	}

	public GameObject pOldAvatarObject => mOldAvatarObject;

	public bool pIsMathContent => mIsMathContent;

	public float pAccuracy
	{
		get
		{
			if (mNumProjectilesShot > 0)
			{
				return (float)mNumProjectilesHit / (float)(mNumProjectilesShot - mNumAmmoInAir) * 100f;
			}
			return 0f;
		}
	}

	public int pPlayerPos => mPlayerPos;

	private int pRetryAttempts
	{
		get
		{
			if (!SubscriptionInfo.pIsMember)
			{
				return _RetryAttemptsForNonMembers;
			}
			return _RetryAttemptsForMembers;
		}
	}

	public bool pOpponentLeft
	{
		get
		{
			return mOpponentLeft;
		}
		set
		{
			mOpponentLeft = value;
		}
	}

	public static bool pIsCrossbowLevel
	{
		get
		{
			if (!(pInstance != null))
			{
				return false;
			}
			return pInstance.mGameType == GSGameType.CROSSBOW_ARROW;
		}
	}

	public bool pIsCrossbowUnlocked
	{
		get
		{
			if (MissionManager.pInstance != null)
			{
				return MissionManager.pInstance.GetTask(_CrossbowUnlockTaskID)?.pCompleted ?? false;
			}
			return false;
		}
	}

	public void Awake()
	{
		mInstance = this;
		mPositiveVOThreshold = _PositiveVOPointsThreshold;
		ObAmmo.pOnAmmoMissed = (ObAmmo.AmmoMissed)Delegate.Combine(ObAmmo.pOnAmmoMissed, new ObAmmo.AmmoMissed(OnAmmoMissed));
		mPrvScale = AvAvatar.mTransform.localScale;
		mPrvFixedDT = Time.fixedDeltaTime;
		Time.fixedDeltaTime = 0.02f;
		AvAvatar.SetActive(inActive: false);
		ItemStoreDataLoader.Load(_StoreID, OnStoreLoaded);
		mPowerUpHelper._CanActivatePowerUp = CanActivatPowerUp;
		mPowerUpHelper._ApplyExtraTime = ApplyExtraTime;
		mPowerUpHelper._CanApplyEffect = CanApplyEffect;
		mPowerUpHelper._FireProjectile = FireProjectile;
		mPowerUpHelper._GetParentObject = GetParentObjectForPowerUp;
		mPowerUpHelper._OnBombExplode = OnBombExplode;
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: false);
		}
		if (_PowerUpManager != null)
		{
			_PowerUpManager.Init(this, mPowerUpHelper);
		}
		AvAvatar.pLevelState = AvAvatarLevelState.TARGETPRACTICE;
	}

	public bool CanPlayCrossBowLevel()
	{
		if (SubscriptionInfo.pIsMember)
		{
			return true;
		}
		bool flag = true;
		if (ProductData.pPairData != null)
		{
			string value = ProductData.pPairData.GetValue("LCGT");
			int intValue = ProductData.pPairData.GetIntValue("CB_PLAYED", 0);
			if (!string.IsNullOrEmpty(value) && value != "___VALUE_NOT_FOUND___")
			{
				DateTime minValue = DateTime.MinValue;
				minValue = DateTime.Parse(value, UtUtilities.GetCultureInfo("en-US"));
				bool num = (ServerTime.pCurrentTime - minValue).TotalHours > (double)_CrossBowUnlockDurationInHours;
				if (num)
				{
					ProductData.pPairData.SetValueAndSave("CB_PLAYED", "0");
				}
				flag = num || intValue < 2;
			}
		}
		if (!flag)
		{
			DisplayTradeUI(GSPurchaseType.CROSSBOW_NON_MEMBER_PLAY);
		}
		return flag;
	}

	public override void OnLevelReady()
	{
		if (_GauntletController != null && SanctuaryManager.pCurPetInstance != null && !_GauntletController.pPetLoaded)
		{
			LocaleString petNotFoundText = _PetNotFoundText;
			KAUIGenericDB kAUIGenericDB = DisplayTextBox(petNotFoundText._ID, petNotFoundText._Text);
			if (kAUIGenericDB != null)
			{
				kAUIGenericDB._OKMessage = "OnExitLoadLevel";
			}
		}
		else
		{
			Init();
		}
	}

	public void InitController()
	{
		if (_GauntletController != null)
		{
			_GauntletController.Init();
		}
	}

	private void Init()
	{
		mTxtGameScore = StringTable.GetStringData(_GameScoreText._ID, _GameScoreText._Text);
		mTxtGameAccuracyPercent = StringTable.GetStringData(_GameAccuracyPercentText._ID, _GameAccuracyPercentText._Text);
		mTxtGameAccuracyScore = StringTable.GetStringData(_GameAccuracyScoreText._ID, _GameAccuracyScoreText._Text);
		mTxtGameTotalScore = StringTable.GetStringData(_GameTotalScoreText._ID, _GameTotalScoreText._Text);
		mTxtTopScoreTitle = StringTable.GetStringData(_TopScoreTitleText._ID, _TopScoreTitleText._Text);
		mTxtNoScoresYet = StringTable.GetStringData(_NoScoresYetText._ID, _NoScoresYetText._Text);
		mTxtScoreString = StringTable.GetStringData(_ScoreStringText._ID, _ScoreStringText._Text);
		mTxtNameString = StringTable.GetStringData(_NameStringText._ID, _NameStringText._Text);
		mTxtMathCorrectAnswers = StringTable.GetStringData(_MathCorrectAnswerText._ID, _MathCorrectAnswerText._Text);
		if (_UiConsumable != null)
		{
			_UiConsumable.SetVisibility(inVisible: false);
		}
		if ((bool)_HUDObject)
		{
			mGameHud = _HUDObject.GetComponent<KAUiGauntletShooterHUD>();
		}
		mOldAvatarObject = AvAvatar.pObject;
		_LevelSelectionScreen.InitializeSettings();
		if (SanctuaryManager.pInstance.pPetMeter != null)
		{
			SanctuaryManager.pInstance.pPetMeter.gameObject.SetActive(value: false);
		}
		MainStreetMMOClient.Init();
		if (!string.IsNullOrEmpty(ChallengeInfo.pRecommendedGameExitSceneName))
		{
			_ExitLevelName = ChallengeInfo.pRecommendedGameExitSceneName;
		}
		if (!string.IsNullOrEmpty(ChallengeInfo.pRecommendedGameExitMarkerName))
		{
			_ExitMarkerName = ChallengeInfo.pRecommendedGameExitMarkerName;
		}
		ChallengeInfo.pRecommendedGameExitSceneName = null;
		ChallengeInfo.pRecommendedGameExitMarkerName = null;
		if (ChallengeInfo.pActiveChallenge != null)
		{
			mIsPlayingChallenge = true;
			int gameLevelID = ChallengeInfo.pActiveChallenge.ChallengeGameInfo.GameLevelID;
			int num = 0;
			if (ChallengeInfo.pActiveChallenge.ChallengeGameInfo.GameDifficultyID.HasValue)
			{
				num = ChallengeInfo.pActiveChallenge.ChallengeGameInfo.GameDifficultyID.Value;
			}
			mChallengePoints = ChallengeInfo.pActiveChallenge.Points;
			if (!mChallengeAchieved && mChallengePoints > 0)
			{
				mGameHud.UpdateChallengePoints(mChallengePoints);
			}
			mDifficulty = (HighScoresDifficulty)num;
			if (gameLevelID == _FiringFrenzyLevelID)
			{
				mIsMathContent = false;
				GenerateLevelByGameType();
			}
			else
			{
				mIsMathContent = true;
			}
		}
		else if (!_GauntletController._IsToothless)
		{
			ShowLevelSelectionMenu();
			VisitBuddy();
		}
		else
		{
			TryPlayToothless();
		}
		pIsTutDone = ProductData.TutorialComplete(_TutorialKey);
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pCurPetInstance.SetMoodParticleIgnore(isIgnore: true);
		}
	}

	public void VisitBuddy()
	{
		if (!(MainStreetMMOClient.pInstance != null))
		{
			return;
		}
		string ownerIDForCurrentLevel = MainStreetMMOClient.pInstance.GetOwnerIDForCurrentLevel();
		if (!string.IsNullOrEmpty(ownerIDForCurrentLevel) && ownerIDForCurrentLevel != UserInfo.pInstance.UserID)
		{
			if (GauntletMMOClient.pInstance != null)
			{
				GauntletMMOClient.pInstance.DestroyMMO(inLogout: true);
			}
			mHostPlayer = ownerIDForCurrentLevel;
			ShowLevelSelectionMenu();
			_HUDObject.SendMessage("SetCountdownVisibility", false, SendMessageOptions.DontRequireReceiver);
			_HUDObject.SendMessage("SetVisibility", false, SendMessageOptions.DontRequireReceiver);
			mIsMultiplayer = true;
			MainStreetMMOClient.pInstance.RemoveOwnerIDForLevel("GauntletDO");
			if (!UtPlatform.IsWSA())
			{
				GauntletMMOClient.Init(GauntletMMORoomType.JOIN_BUDDY);
			}
			KAUICursorManager.SetDefaultCursor("Loading");
			_LevelSelectionScreen.SetInteractive(interactive: false);
		}
	}

	private KAUIGenericDB DisplayTextBox(int msgid, string msgtxt)
	{
		if (mUiGenericDB != null)
		{
			UnityEngine.Object.Destroy(mUiGenericDB.gameObject);
		}
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDB"));
		mUiGenericDB = gameObject.GetComponent<KAUIGenericDB>();
		mUiGenericDB._MessageObject = base.gameObject;
		mUiGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mUiGenericDB.SetTextByID(msgid, msgtxt, interactive: false);
		mUiGenericDB._OKMessage = "ExitGame";
		KAUI.SetExclusive(mUiGenericDB, _MaskColor);
		return mUiGenericDB;
	}

	private void ShowTutorial()
	{
		if (_TPTutorialManager != null)
		{
			_TPTutorialManager.InitTutorial();
			_TPTutorialManager.ShowTutorial();
			TPTutorialManager tPTutorialManager = _TPTutorialManager;
			tPTutorialManager._StepEndedEvent = (StepEndedEvent)Delegate.Combine(tPTutorialManager._StepEndedEvent, new StepEndedEvent(TutorialStepEndEvent));
		}
	}

	public void TutorialStepEndEvent(int stepIdx, string stepName, bool tutQuit)
	{
		if (_TPTutorialManager != null && stepIdx >= _TPTutorialManager._TutSteps.Length - 1)
		{
			if (mInControlModifier != null)
			{
				mInControlModifier.ResumeStep(isForceResume: true);
			}
			pIsTutDone = true;
			ProductData.AddTutorial(_TutorialKey);
		}
	}

	public void ShowLevelSelectionMenu()
	{
		mIsGameRunning = false;
		mGameType = GSGameType.UNKNOWN;
		ResetLevel();
		if (_BkgMusic != null)
		{
			SnChannel.Play(_BkgMusic, "Music_Pool", inForce: true);
		}
		TutorialManager.StopTutorials();
		SnChannel.StopPool("GSTargetTower_Pool");
		SnChannel.StopPool("GSTarget_Pool");
		SnChannel.StopPool("GSTargetChar_Pool");
		SnChannel.StopPool("GSFire_Pool");
		SnChannel.StopPool("DEFAULT_POOL");
		_LevelSelectionScreen.SetVisibility(visible: true);
		_MenuCameraObject.SetActive(value: true);
		if (_GauntletController != null)
		{
			_GauntletController.gameObject.SetActive(value: false);
			_GauntletController.SetDisabled(isDisabled: true);
		}
		UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
		SetCursorVisible(isVisible: true);
	}

	public int[] GenerateLevelByType(GSGameType gameType)
	{
		return GenerateLevel(GetLevelDataByType(gameType), canLoadLevel: false);
	}

	public void GenerateLevel(int[] inPieceIndex, GSGameType gameType)
	{
		GauntletLevelData levelDataByType = GetLevelDataByType(gameType);
		int num = inPieceIndex.Length;
		levelDataByType._TrackPieces = new GameObject[num + 1];
		levelDataByType._TrackPieces[0] = levelDataByType._StartPiece;
		levelDataByType._TrackPieces[num] = levelDataByType._EndPiece;
		int num2 = 0;
		GameObject[] array = new GameObject[levelDataByType._RotatingChamberPieces.Length + levelDataByType._BuildingPieces.Length];
		GameObject[] rotatingChamberPieces = levelDataByType._RotatingChamberPieces;
		foreach (GameObject gameObject in rotatingChamberPieces)
		{
			array[num2++] = gameObject;
		}
		rotatingChamberPieces = levelDataByType._BuildingPieces;
		foreach (GameObject gameObject2 in rotatingChamberPieces)
		{
			array[num2++] = gameObject2;
		}
		for (int j = 1; j < inPieceIndex.Length; j++)
		{
			levelDataByType._TrackPieces[j] = array[inPieceIndex[j]];
		}
		LoadLevel(levelDataByType);
	}

	private int[] GenerateLevel(GauntletLevelData inLevelData, bool canLoadLevel)
	{
		if (inLevelData != null)
		{
			int num = UnityEngine.Random.Range(_GeneratedLevelPieceCount._Min, _GeneratedLevelPieceCount._Max);
			int[] array = new int[num + 1];
			int num2 = ((inLevelData._RotatingChamberPieces.Length == 0) ? (-1) : UnityEngine.Random.Range(1, num));
			inLevelData._TrackPieces = new GameObject[num + 2];
			inLevelData._TrackPieces[0] = inLevelData._StartPiece;
			inLevelData._TrackPieces[num + 1] = inLevelData._EndPiece;
			List<GameObject> list = new List<GameObject>(inLevelData._BuildingPieces);
			for (int i = 0; i < num; i++)
			{
				if (i + 1 == num2)
				{
					int num3 = UnityEngine.Random.Range(0, inLevelData._RotatingChamberPieces.Length);
					inLevelData._TrackPieces[i + 1] = inLevelData._RotatingChamberPieces[num3];
					array[i + 1] = num3;
					continue;
				}
				if (list.Count == 0)
				{
					list = new List<GameObject>(inLevelData._BuildingPieces);
				}
				int index = UnityEngine.Random.Range(0, list.Count);
				inLevelData._TrackPieces[i + 1] = list[index];
				UtDebug.Log("Loaded Building Piece:: " + list[index].name, 8u);
				for (int j = 0; j < inLevelData._BuildingPieces.Length; j++)
				{
					if (list[index] == inLevelData._BuildingPieces[j])
					{
						array[i + 1] = inLevelData._RotatingChamberPieces.Length + j;
					}
				}
				list.RemoveAt(index);
			}
			if (canLoadLevel)
			{
				LoadLevel(inLevelData);
			}
			return array;
		}
		return null;
	}

	public void SetGameType(GSGameType type)
	{
		mDifficulty = ((type == GSGameType.REDPLANET) ? HighScoresDifficulty.EASY : HighScoresDifficulty.MEDIUM);
		mGameType = type;
		if (_GameModeScreen != null)
		{
			_GameModeScreen.SetVisibility(inVisible: true);
		}
		else
		{
			_UiConsumable.SetGameData(this, "TargetPractice", 0, inForceUpdate: true);
			GenerateLevelByGameType();
		}
		UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
	}

	public void GenerateLevelByGameType()
	{
		GauntletLevelData levelDataByType = GetLevelDataByType(mGameType);
		if (levelDataByType != null && levelDataByType._StartPiece != null && levelDataByType._EndPiece != null)
		{
			mIsMultiplayer = false;
			GenerateLevel(levelDataByType, canLoadLevel: true);
			_GauntletController._Pause = false;
		}
		else
		{
			ShowLevelSelectionMenu();
		}
	}

	private GauntletLevelData GetLevelDataByType(GSGameType gameType)
	{
		return _LevelDataList.Find((GauntletLevelData data) => data._GameType == gameType);
	}

	public int GetAccuracyScore(int accuracyPercent, int totalScore)
	{
		GRSAccuracyScoreData[] accuracyPointsData = _AccuracyPointsData;
		foreach (GRSAccuracyScoreData gRSAccuracyScoreData in accuracyPointsData)
		{
			if (accuracyPercent >= gRSAccuracyScoreData._Range._Min && accuracyPercent <= gRSAccuracyScoreData._Range._Max)
			{
				return (int)(gRSAccuracyScoreData._Multiplier * (float)Mathf.Abs(totalScore));
			}
		}
		return 0;
	}

	private AchievementTask GetSocialAchievement()
	{
		if (GauntletMMOClient.pInstance != null && BuddyList.pIsReady)
		{
			foreach (GauntletMMOPlayer pPlayer in GauntletMMOClient.pInstance.pPlayers)
			{
				if (BuddyList.pInstance.GetBuddyStatus(pPlayer._UserID) == BuddyStatus.Approved)
				{
					return new AchievementTask(_SocialMultiplayerWithBuddyAchievementID);
				}
			}
		}
		return new AchievementTask(_SocialMultiplayerWithOthersAchievementID);
	}

	private void OnGameComplete()
	{
		_UiConsumable.SetVisibility(inVisible: false);
		_PowerUpManager.DeactivateAllPowerUps();
		if ((bool)_GauntletController)
		{
			_GauntletController.Speed = 0f;
		}
		if (pIsMathContent && mCM != null)
		{
			mCM.EndRound(completed: true);
		}
		DestroyGenericDialog();
		List<AchievementTask> list = new List<AchievementTask>();
		list.Add(UserProfile.pProfileData.GetGroupAchievement(_ClanAchievementID));
		if (mIsMultiplayer)
		{
			if (GauntletMMOClient.pInstance != null)
			{
				GauntletMMOClient.pInstance.OnGameComplete();
			}
			list.Add(new AchievementTask(_HeadToHeadAchievementID));
			list.Add(GetSocialAchievement());
		}
		else
		{
			if (!SubscriptionInfo.pIsMember && pIsCrossbowLevel)
			{
				ProductData.pPairData.SetValueAndSave("LCGT", ServerTime.pCurrentTime.ToString(UtUtilities.GetCultureInfo("en-US")));
				int num = ProductData.pPairData.GetIntValue("CB_PLAYED", 0) + 1;
				ProductData.pPairData.SetValueAndSave("CB_PLAYED", num.ToString());
			}
			list.Add(new AchievementTask(_SinglePlayerAchievementID));
			mIsGameRunning = false;
			if (_GauntletController != null)
			{
				_GauntletController.SetDisabled(isDisabled: true);
			}
			if (_HUDObject != null)
			{
				_HUDObject.SendMessage("SetVisibility", false, SendMessageOptions.DontRequireReceiver);
			}
			if (_ScoreScreenVO != null)
			{
				SnChannel.Play(_ScoreScreenVO, "VO_Pool", inForce: true);
			}
			int num2 = 0;
			if (mGameHud != null)
			{
				num2 = mGameHud.GetScore();
			}
			int accuracyPercent = (int)pAccuracy;
			int accuracyScore = GetAccuracyScore(accuracyPercent, num2);
			mTotalScore = num2 + accuracyScore;
			ShowHighscores();
			if (_EndScoreUI != null)
			{
				AddGameResult(num2, accuracyPercent, accuracyScore);
				ChallengeInfo pActiveChallenge = ChallengeInfo.pActiveChallenge;
				if (pActiveChallenge != null)
				{
					switch (ChallengeInfo.CheckForChallengeCompletion(_GameModuleID, _FiringFrenzyLevelID, (int)mDifficulty, mTotalScore, isTimerUsedAsPoints: false))
					{
					case ChallengeResultState.LOST:
						SetChallengeText(_ChallengeTryAgainText.GetLocalizedString());
						break;
					case ChallengeResultState.WON:
					{
						string localizedString = _ChallengeCompleteText.GetLocalizedString();
						if (localizedString.Contains("[Name]"))
						{
							bool flag = false;
							if (BuddyList.pIsReady)
							{
								Buddy buddy = BuddyList.pInstance.GetBuddy(pActiveChallenge.UserID.ToString());
								if (buddy != null && !string.IsNullOrEmpty(buddy.DisplayName))
								{
									localizedString = localizedString.Replace("[Name]", buddy.DisplayName);
									SetChallengeText(localizedString);
									flag = true;
								}
							}
							if (!flag)
							{
								SetChallengeText("");
								WsWebService.GetDisplayNameByUserID(pActiveChallenge.UserID.ToString(), ServiceEventHandler, null);
							}
						}
						else
						{
							SetChallengeText(localizedString);
						}
						break;
					}
					}
				}
				else
				{
					SetChallengeText("");
				}
				if (_EndDBUI != null)
				{
					_EndDBUI.AllowChallenge(mTotalScore > 0 && !pIsCrossbowLevel);
				}
				ChallengeInfo.pActiveChallenge = null;
				UiChallengeInvite.SetData(_GameModuleID, _FiringFrenzyLevelID, (int)mDifficulty, mTotalScore);
			}
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			SetCursorVisible(isVisible: true);
			if (!_GauntletController._IsToothless)
			{
				string moduleName = GetModuleName(isMultiplayer: false);
				_EndDBUI.SetAdRewardData(moduleName, mTotalScore);
				if (mTotalScore > 0)
				{
					WsWebService.ApplyPayout(moduleName, mTotalScore, ServiceEventHandler, null);
				}
				else
				{
					_EndDBUI.SetRewardDisplay(null);
				}
			}
		}
		if (mGameHud != null)
		{
			mGameHud.ChallengeItemVisible(visible: false);
		}
		UserAchievementTask.Set(list.ToArray());
		if (!_GauntletController._IsToothless && (bool)SanctuaryManager.pCurPetInstance)
		{
			SanctuaryManager.pCurPetInstance.UpdateActionMeters(PetActions.TARGETPRACTICE, 1f, doUpdateSkill: true);
		}
	}

	private string GetModuleName(bool isMultiplayer)
	{
		if (GauntletMMOClient.pInstance != null && GauntletMMOClient.pInstance.pRoomType == GauntletMMORoomType.HOST_FOR_BUDDY)
		{
			return _ChallengeFriendGameModuleName;
		}
		if (isMultiplayer)
		{
			return _GameModuleName + "Multiplayer";
		}
		if (SubscriptionInfo.pIsMember)
		{
			return _GameModuleName + "Member";
		}
		return _GameModuleName;
	}

	private void TryPlayToothless(bool replay = false)
	{
		int intValue = ProductData.pPairData.GetIntValue(TIMES_PLAYED, 0);
		if (intValue < pRetryAttempts || pRetryAttempts <= 0)
		{
			ProductData.pPairData.SetValueAndSave(TIMES_PLAYED, (intValue + 1).ToString());
			if (replay)
			{
				ProcessDBClose(replayGame: true);
			}
			else
			{
				SetGameType(GSGameType.TRAINING);
			}
		}
		else
		{
			string localizedString = _ExceededPlayTime.GetLocalizedString();
			localizedString = localizedString.Replace("{{NUM_ATTEMPT}}", pRetryAttempts.ToString());
			DisplayGenericDialog(base.gameObject, "", "OnExit", "", "", 0, localizedString, useDots: false, -1f, IsYesBtn: false, IsNoBtn: false, IsOKBtn: true, IsCloseBtn: false, isExclusiveUI: true, -1);
		}
	}

	private void OnReplayGame()
	{
		if (_GauntletController._IsToothless)
		{
			TryPlayToothless(replay: true);
		}
		else if (pIsCrossbowLevel)
		{
			if (CanPlayCrossBowLevel())
			{
				ProcessDBClose(replayGame: true);
			}
		}
		else
		{
			ProcessDBClose(replayGame: true);
		}
	}

	private void OnEndDBClose()
	{
		ProcessDBClose();
	}

	private void ProcessDBClose(bool replayGame = false)
	{
		if (pIsMultiplayer)
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			GauntletMMOClient.pInstance.PlayGameAgain();
			return;
		}
		AvAvatar.pObject.transform.parent = mOldAvatarObject.transform;
		AvAvatar.pObject = mOldAvatarObject;
		if (SanctuaryManager.pInstance.pPetMeter != null)
		{
			SanctuaryManager.pInstance.pPetMeter.gameObject.SetActive(value: false);
		}
		if (replayGame)
		{
			ResetLevel();
			SetGameType(mGameType);
		}
		else if (!_GauntletController._IsToothless)
		{
			_GauntletController.ProcessExit(ShowLevelSelectionMenu);
		}
		else
		{
			ExitGame();
		}
	}

	private void SetChallengeText(string challengeText)
	{
		_EndDBUI.SetRewardMessage(challengeText);
	}

	public void SetResults(GauntletMMOPlayer[] inPlayers)
	{
		mPlayers = inPlayers;
		if (mPlayers != null)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			bool flag = false;
			GauntletMMOPlayer gauntletMMOPlayer = null;
			for (int i = 0; i < mPlayers.Length; i++)
			{
				int score = mPlayers[i]._Score;
				if (num2 < score)
				{
					num2 = score;
					num = i;
				}
				if (mPlayers[i]._UserID == UserInfo.pInstance.UserID)
				{
					gauntletMMOPlayer = mPlayers[i];
				}
				if (i > 0 && mPlayers[i]._Score == mPlayers[i - 1]._Score)
				{
					flag = true;
				}
			}
			if (!flag && (bool)pInstance)
			{
				mPlayerPos = ((mPlayers[num]._UserID == UserInfo.pInstance.UserID) ? (-1) : (-2));
				string moduleName = GetModuleName(isMultiplayer: true);
				_EndDBUI.SetAdRewardData(moduleName, mPlayerPos);
				WsWebService.ApplyPayout(moduleName, mPlayerPos, ServiceEventHandler, null);
			}
			num3 = mGameHud.GetScore();
			int accuracy = gauntletMMOPlayer._Accuracy;
			int accuracyScore = GetAccuracyScore(accuracy, num3);
			mTotalScore = gauntletMMOPlayer._Score;
			ShowHighscores();
			AddGameResult(num3, accuracy, accuracyScore);
			if (flag)
			{
				_EndDBUI.SetRewardMessage(_HTHTieMessageText.GetLocalizedString());
			}
			else if (mPlayerPos == -1)
			{
				_EndDBUI.SetRewardMessage(_HTHWinMessageText.GetLocalizedString());
			}
			else
			{
				_EndDBUI.SetRewardMessage(_HTHLostMessageText.GetLocalizedString());
			}
		}
		if (mPlayers != null)
		{
			GauntletMMOPlayer[] array = mPlayers;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].DestroyMe();
			}
			mPlayers = null;
		}
	}

	private void AddGameResult(int gameScore, int accuracyPercent, int accuracyScore)
	{
		string text = _GameModuleName;
		if (mIsMultiplayer)
		{
			text += "_Multiplayer";
		}
		AvAvatar.pObject = mOldAvatarObject;
		string inResult = "any";
		if (mIsMultiplayer)
		{
			inResult = ((pPlayerPos == -1) ? "win" : "lose");
		}
		_EndDBUI.SetGameSettings(text, base.gameObject, inResult);
		_EndDBUI.SetRewardDisplay(null);
		_EndDBUI.SetVisibility(Visibility: true);
		_EndDBUI.SetResultData("GameScore", mTxtGameScore, gameScore.ToString());
		_EndDBUI.SetResultData("AccuracyPercent", mTxtGameAccuracyPercent, accuracyPercent.ToString());
		_EndDBUI.SetResultData("AccuracyScore", mTxtGameAccuracyScore, accuracyScore.ToString());
		_EndDBUI.SetResultData("TotalScore", mTxtGameTotalScore, mTotalScore.ToString());
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType == WsServiceType.APPLY_PAYOUT)
		{
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
				AchievementReward[] array = null;
				if (inObject != null)
				{
					array = (AchievementReward[])inObject;
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
							case 11:
							{
								int value = achievementReward.Amount.Value;
								UserRankData.AddPoints(11, value);
								break;
							}
							}
						}
					}
				}
				if (_EndDBUI != null)
				{
					_EndDBUI.SetRewardDisplay(array);
				}
				break;
			}
			case WsServiceEvent.ERROR:
				UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
				UtDebug.Log("reward data is null!!!");
				break;
			}
		}
		if (inType == WsServiceType.GET_DISPLAYNAME_BY_USER_ID && inEvent == WsServiceEvent.COMPLETE && inObject != null && !string.IsNullOrEmpty((string)inObject))
		{
			string localizedString = _ChallengeCompleteText.GetLocalizedString();
			localizedString = localizedString.Replace("[Name]", (string)inObject);
			SetChallengeText(localizedString);
		}
	}

	public void ShowHighscores()
	{
		HighScores.SetCurrentGameSettings(_GameModuleName, _GameModuleID, isMultiPlayer: false, 0, _FiringFrenzyLevelID);
		HighScores.AddGameData("highscore", mTotalScore.ToString());
		_EndDBUI.SetHighScoreData(mTotalScore, "highscore");
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.CheckForTaskCompletion("Game", _GameModuleName);
		}
	}

	public void PlayAgain()
	{
		OnRestartYes();
	}

	public void OnCloseChallenge()
	{
		OnHighScoresDone();
	}

	public void LoadRecommendedGame(string inLevelName)
	{
		AvAvatar.pObject = mOldAvatarObject;
		RsResourceManager.LoadLevel(inLevelName);
	}

	public void OnHighScoresDone()
	{
		if (KAUI._GlobalExclusiveUI != null)
		{
			mTopScoreDisplayTimer = _TopScoreDisplayTime;
		}
		else if (mIsPlayingChallenge)
		{
			OnExit();
		}
		else
		{
			PopupRestartMessageDB();
		}
	}

	private void PopupRestartMessageDB()
	{
		SetCursorVisible(isVisible: true);
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSm"));
		mUiGenericDB = gameObject.GetComponent<KAUIGenericDB>();
		mUiGenericDB._MessageObject = base.gameObject;
		mUiGenericDB._YesMessage = "OnRestartYes";
		mUiGenericDB._NoMessage = "OnRestartNo";
		mUiGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
		mUiGenericDB.SetTitle(_RestartGameTitleText.GetLocalizedString());
		mUiGenericDB.SetTextByID(_RestartGameText._ID, _RestartGameText._Text, interactive: false);
		KAUI.SetExclusive(mUiGenericDB);
	}

	public void AddScore(int inScore)
	{
		if (mGameHud != null)
		{
			mGameHud.AddScore(inScore);
		}
		if (mGameHud != null)
		{
			int score = mGameHud.GetScore();
			if (_PositiveVOCriteria == GSPositiveVOCriteria.ON_POINTS_THRESHOLD && score > mPositiveVOThreshold)
			{
				mPositiveVOThreshold += _PositiveVOPointsThreshold;
				PlayPositiveAudio();
			}
			if (!mChallengeAchieved && mChallengePoints > 0 && score > mChallengePoints)
			{
				mChallengeAchieved = true;
				mGameHud.FlashChallengeItem(_ChallengeFlashTimeInterval, _ChallengeFlashLoopCount);
				SnChannel.Play(_ChallengeCompleteSFX, "SFX_Pool", inForce: true, null);
			}
		}
	}

	public void ResetScore()
	{
		if (mGameHud != null)
		{
			mGameHud.SetScore(0);
			mGameHud.SetAccuracy(0);
		}
		mMathScore = 0;
		mCorrectAnswersCount = 0;
	}

	private void ResetLevel()
	{
		mLevelData = null;
		if (_GauntletController != null)
		{
			_GauntletController._CanShoot = false;
			_GauntletController.transform.position = Vector3.zero;
			_GauntletController.transform.rotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
		}
		if (mPreviousLevelPiece != null)
		{
			UnityEngine.Object.Destroy(mPreviousLevelPiece);
		}
		foreach (GameObject mLevelPiece in mLevelPieceList)
		{
			UnityEngine.Object.Destroy(mLevelPiece);
		}
		mLevelPieceList.Clear();
		mCurrentSplineObject = null;
		mTrackPieceIndex = 0;
		mNumCrossedTrackPiece = 0;
		mNumProjectilesShot = 0;
		mNumProjectilesHit = 0;
		mNextPiecePos = Vector3.zero;
		mTotalScore = 0;
		ResetScore();
		if (pIsMathContent && mCM != null)
		{
			LoadContentFileReady();
		}
	}

	private void LoadLevel(GauntletLevelData inLevelData)
	{
		if (inLevelData == null)
		{
			return;
		}
		if (_HUDObject != null)
		{
			_HUDObject.SendMessage("SetVisibility", true, SendMessageOptions.DontRequireReceiver);
		}
		if (_GauntletController != null)
		{
			_GauntletController.Init();
			_MenuCameraObject.SetActive(value: false);
			_GauntletController.SetDisabled(isDisabled: false);
			_GauntletController.Speed = 5f;
			AvAvatar.pObject = _GauntletController.gameObject;
			SnChannel.StopPool("Music_Pool");
			SnChannel.StopPool("VO_Pool");
			if (SanctuaryManager.pInstance.pPetMeter != null)
			{
				SanctuaryManager.pInstance.pPetMeter.gameObject.SetActive(value: false);
			}
			_GauntletController.gameObject.SetActive(value: true);
		}
		mLevelData = inLevelData;
		if (mIsMultiplayer)
		{
			mShowMultiplayerCountdown = true;
		}
	}

	public void OnAmmoMissed()
	{
		if (mNumAmmoInAir > 0)
		{
			mNumAmmoInAir--;
			if (mGameHud != null)
			{
				mGameHud.SetAccuracy((int)pAccuracy);
			}
		}
	}

	public override void OnStartGame()
	{
		base.OnStartGame();
		if (_GauntletController != null)
		{
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.SetTimedTaskUpdate(inState: true, inForceUpdate: true);
			}
			_UiConsumable.ResetButtonStates();
			_GauntletController._CanShoot = true;
			_GauntletController.Speed = mControllerPrevSpeed;
			mOldIdleAnim = SanctuaryManager.pCurPetInstance._MountIdleAnim;
			mOldRunAnim = SanctuaryManager.pCurPetInstance._MountRunAnim;
			SanctuaryManager.pCurPetInstance._MountIdleAnim = "IdleStand";
			SanctuaryManager.pCurPetInstance._MountRunAnim = "Walk";
			if (!mIsMultiplayer && mInControlModifier != null)
			{
				mInControlModifier.ResumeStep(isForceResume: true);
			}
			mHostPlayer = "";
			if (!mChallengeAchieved && mChallengePoints > 0)
			{
				mGameHud.ChallengeItemVisible(visible: true);
			}
		}
	}

	public void ProjectileShot()
	{
		mNumProjectilesShot++;
		mNumAmmoInAir++;
	}

	public void OnProjectileHit(bool isHitTarget, GameObject inProjectile)
	{
		if (isHitTarget)
		{
			mNumProjectilesHit++;
			if (mNumAmmoInAir > 0)
			{
				mNumAmmoInAir--;
			}
			if (mGameHud != null)
			{
				mGameHud.SetAccuracy((int)pAccuracy);
			}
			mConsecutiveHitCount++;
			if (_PositiveVOCriteria == GSPositiveVOCriteria.ON_X_TARGET_HITS && mConsecutiveHitCount >= _PositiveVOTargetCount)
			{
				mConsecutiveHitCount = 0;
				PlayPositiveAudio();
			}
		}
		else
		{
			mConsecutiveHitCount = 0;
		}
	}

	public void Update()
	{
		if (_GauntletController._ExtraTime && !_GauntletController._Pause)
		{
			mTime += Time.deltaTime;
			if (mTime >= mExtraTime)
			{
				mTime = 0f;
				mExtraTime = 0f;
				ApplyExtraTime(extraTime: false, 0f);
			}
			mGameHud.ShowExtraTime(mExtraTime - mTime);
		}
		if (mIsShowCountDown && _GauntletController != null && _GauntletController.SplineObject != null && _HUDObject != null)
		{
			mIsShowCountDown = false;
			_HUDObject.SendMessage("StartCountDown");
			mControllerPrevSpeed = _GauntletController.Speed;
			if (_GauntletController != null)
			{
				_GauntletController._CanShoot = false;
				_GauntletController.Speed = 0f;
			}
		}
		LoadTrack(force: false);
		if (mCurrentSplineObject == null && mLevelPieceList.Count > 0)
		{
			GauntletLevelPiece component = mLevelPieceList[0].GetComponent<GauntletLevelPiece>();
			if (component != null)
			{
				mCurrentSplineObject = component._StartSpline;
				_GauntletController.SetSplineObject(mCurrentSplineObject);
				if (pIsMathContent && mNumCrossedTrackPiece > 0 && mNumCrossedTrackPiece < mLevelData._TrackPieces.Length - 1)
				{
					if (!IsCurrentTrackBonusLevel())
					{
						DisplayQuestion();
					}
					else if ((bool)mGameHud)
					{
						mGameHud.SetBonusFlash(3f);
					}
				}
				if (mShowMultiplayerCountdown)
				{
					mShowMultiplayerCountdown = false;
					mIsGameRunning = true;
					mControllerPrevSpeed = _GauntletController.Speed;
					_GauntletController._CanShoot = false;
					_GauntletController.Speed = 0f;
					_GauntletController.SetPosOnSpline(0f);
				}
			}
		}
		if (mGameEndScoreTimer > 0f)
		{
			mGameEndScoreTimer -= Time.deltaTime;
			if (mGameEndScoreTimer <= 0f)
			{
				ShowHighscores();
			}
		}
		if (mTopScoreDisplayTimer > 0f)
		{
			mTopScoreDisplayTimer -= Time.deltaTime;
			if (mTopScoreDisplayTimer <= 0f)
			{
				SetBillBoardTopScoresText("", "", "");
				OnHighScoresDone();
			}
		}
		if (mIsGameRunning && mIsCursorVisible && KAUI.GetGlobalMouseOverItem() == null && !_GauntletController._Pause)
		{
			SetCursorVisible(isVisible: false);
		}
		if (!mIsCursorVisible && KAUI.GetGlobalMouseOverItem() != null)
		{
			SetCursorVisible(isVisible: true);
		}
		if (UICursorManager.GetCursorName() == "Activate")
		{
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
		}
		UpdateGenericDB();
		if (_GauntletController.pCurrentControlModifier != null)
		{
			if (_GauntletController.pCurrentControlModifier.pState == GauntletControlStates.HALT)
			{
				_UiConsumable.AddDisabledConsumables(new List<string>(), new List<string>());
				return;
			}
			_UiConsumable.AddDisabledConsumables(new List<string>(), new List<string> { "Smart Bomb" });
		}
	}

	public void SetCursorVisible(bool isVisible)
	{
		mIsCursorVisible = isVisible;
		if ((bool)UICursorManager.pCursorManager)
		{
			UICursorManager.pCursorManager.SetVisibility(isVisible);
		}
	}

	public void OnRestartYes()
	{
		DestroyGenericDialog();
		PayByCoinsAndRestartLevel();
	}

	private void PayByCoinsAndRestartLevel()
	{
		DestroyGenericDialog();
		ResetLevel();
		GenerateLevel(GetLevelDataByType(pGameType), canLoadLevel: true);
	}

	private void NotEnoughCoins()
	{
		ExitGame();
	}

	public void PayByCoinsForMultiplayerAgain()
	{
		DestroyGenericDialog();
	}

	public void OnRestartNo()
	{
		DestroyGenericDialog();
		mIsGameRunning = false;
		ResetLevel();
		_LevelSelectionScreen.SetVisibility(visible: true);
		if (_BkgMusic != null)
		{
			SnChannel.Play(_BkgMusic, "Ambient_Pool", inForce: true);
		}
		TutorialManager.StopTutorials();
		SnChannel.StopPool("GSTargetTower_Pool");
		SnChannel.StopPool("GSTarget_Pool");
		SnChannel.StopPool("GSTargetChar_Pool");
		SnChannel.StopPool("GSFire_Pool");
		SnChannel.StopPool("DEFAULT_POOL");
		_MenuCameraObject.SetActive(value: true);
		if (_GauntletController != null)
		{
			_GauntletController.gameObject.SetActive(value: false);
			_GauntletController.SetDisabled(isDisabled: true);
		}
	}

	public void ExitGame()
	{
		ProcessExitGame();
		UtUtilities.LoadLevel(_ExitLevelName);
	}

	public void ProcessExitGame()
	{
		AvAvatar.pLevelState = AvAvatarLevelState.NORMAL;
		DestroyGenericDialog();
		TutorialManager.StopTutorials();
		_LevelSelectionScreen.SetVisibility(visible: false);
		_MultiplayerMenuScreen.SetVisibility(inVisible: false);
		_MMOLobbyUI.SetVisibility(t: false);
		_MMOResultsUI.SetVisibility(t: false);
		if (mGameHud != null)
		{
			mGameHud.gameObject.SetActive(value: false);
		}
		ResetAvatarAndDragon();
		KAUICursorManager.SetDefaultCursor("Arrow");
		Time.fixedDeltaTime = mPrvFixedDT;
		if (_GauntletController._IsToothless)
		{
			ProductData.pPairData.SetValueAndSave(UNLOCK_DURATION, ServerTime.pCurrentTime.ToString(UtUtilities.GetCultureInfo("en-US")));
		}
	}

	public void ResetAvatarAndDragon()
	{
		if (mOldAvatarObject != null)
		{
			AvAvatar.pObject = mOldAvatarObject;
			AvAvatar.mTransform.localScale = mPrvScale;
			AvAvatar.SetParentTransform(null);
			AvAvatar.pStartLocation = _ExitMarkerName;
			AvAvatar.pObject.GetComponent<AvAvatarController>().enabled = true;
			SanctuaryManager.pCurPetInstance._MountIdleAnim = mOldIdleAnim;
			SanctuaryManager.pCurPetInstance._MountRunAnim = mOldRunAnim;
			SanctuaryManager.pCurPetInstance.enabled = true;
			SanctuaryManager.pCurPetInstance.SetAvatar(AvAvatar.pObject.transform, SpawnTeleportEffect: false);
			SanctuaryManager.pCurPetInstance.OnFlyDismount(AvAvatar.pObject);
		}
	}

	public void OnStoreOpened()
	{
		if (!UtMobileUtilities.CanLoadInCurrentScene(UiType.Store, UILoadOptions.AUTO))
		{
			ProcessExitGame();
		}
	}

	public override void OnExit()
	{
		_UiConsumable.SetVisibility(inVisible: false);
		_PowerUpManager.DeactivateAllPowerUps();
		ApplyExtraTime(extraTime: false, 0f);
		if (ChallengeInfo.pActiveChallenge != null)
		{
			ChallengeInfo.CheckForChallengeCompletion(_GameModuleID, _FiringFrenzyLevelID, (int)mDifficulty, 0, isTimerUsedAsPoints: false);
			ChallengeInfo.pActiveChallenge = null;
		}
		Input.ResetInputAxes();
		AvAvatar.pObject = mOldAvatarObject;
		if (_TPTutorialManager != null && _TPTutorialManager.IsTutorialRunning())
		{
			_TPTutorialManager.SkipTutorial();
		}
		if (pIsMathContent && (bool)mGameHud)
		{
			mGameHud.HideQuestion();
		}
		mIsMathContent = false;
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: false);
		}
		if (SanctuaryManager.pInstance.pPetMeter != null)
		{
			SanctuaryManager.pInstance.pPetMeter.gameObject.SetActive(value: false);
		}
		if (mIsMultiplayer)
		{
			ShowMultiplayerMenu();
		}
		else if (!_GauntletController._IsToothless)
		{
			_GauntletController.ProcessExit(ShowLevelSelectionMenu);
		}
		else
		{
			ExitGame();
		}
	}

	public void OnSplineEndReached(GameObject inSplineObject)
	{
		if (mLevelPieceList.Count <= 0 || !(inSplineObject == mCurrentSplineObject))
		{
			return;
		}
		if (mPreviousLevelPiece != null)
		{
			UnityEngine.Object.Destroy(mPreviousLevelPiece);
		}
		mPreviousLevelPiece = mLevelPieceList[0];
		mLevelPieceList.RemoveAt(0);
		mNumCrossedTrackPiece++;
		if (mNumCrossedTrackPiece >= mLevelData._TrackPieces.Length)
		{
			OnGameComplete();
			return;
		}
		if (_PositiveVOCriteria == GSPositiveVOCriteria.ON_SWITCH_ROOMS && mNumCrossedTrackPiece > 1 && mNumCrossedTrackPiece < mLevelData._TrackPieces.Length - 1)
		{
			PlayPositiveAudio();
		}
		mCurrentSplineObject = null;
		if (pIsMathContent)
		{
			if (mNumCrossedTrackPiece > 1 && mNumCrossedTrackPiece < mLevelData._TrackPieces.Length - 1 && !IsCurrentTrackBonusLevel())
			{
				mCurrentQuestionNum++;
			}
			else if ((bool)mGameHud)
			{
				mGameHud.HideQuestion();
			}
		}
		if (mNumCrossedTrackPiece == mLevelData._TrackPieces.Length - 1)
		{
			if (_GauntletController != null)
			{
				_GauntletController._CanShoot = false;
				mGameHud.HideFireButton();
			}
			_UiConsumable.SetVisibility(inVisible: false);
		}
	}

	private void PlayPositiveAudio()
	{
		int num = _PoseVOs.Length;
		if (num > 0)
		{
			int num2 = UnityEngine.Random.Range(0, num);
			SnChannel.Play(_PoseVOs[num2], "VO_Pool", inForce: true);
		}
	}

	public void LoadTrack(bool force)
	{
		if (mLevelData == null || mLevelData._TrackPieces == null || mTrackPieceIndex >= mLevelData._TrackPieces.Length || (mLevelPieceList.Count > mLevelData._PreLoadPieceCount._Min && (!force || mLevelPieceList.Count > mLevelData._PreLoadPieceCount._Max)))
		{
			return;
		}
		bool flag = true;
		GameObject gameObject = UnityEngine.Object.Instantiate(mLevelData._TrackPieces[mTrackPieceIndex]);
		GauntletLevelPiece component = gameObject.GetComponent<GauntletLevelPiece>();
		if (component != null)
		{
			if (mIsMultiplayer)
			{
				Transform transform = gameObject.transform.Find("PfScoreScreen");
				if (transform != null)
				{
					transform.gameObject.SetActive(value: false);
				}
			}
			if (mTrackPieceIndex == 0)
			{
				mNextPieceDir = -gameObject.transform.forward;
			}
			gameObject.transform.position = mNextPiecePos;
			gameObject.transform.forward = -mNextPieceDir;
			if (component._ExitDirection == GauntletPieceDirection.TOP)
			{
				flag = false;
				mNextPieceDir = -gameObject.transform.forward;
			}
			else if (component._ExitDirection == GauntletPieceDirection.LEFT)
			{
				mNextPieceDir = gameObject.transform.right;
			}
			else
			{
				mNextPieceDir = -gameObject.transform.right;
			}
			mNextPiecePos += mNextPieceDir * mLevelData._TrackPieceOffset;
		}
		mLevelPieceList.Add(gameObject);
		mTrackPieceIndex++;
		if (!flag)
		{
			LoadTrack(force: true);
		}
	}

	public void ShowTodaysTopScore()
	{
		GameData.GetGameDataByGameForDayRange(_GameModuleID, isMultiPlayer: false, 0, 1, "highscore", 10, ascendingOrder: false, GetTopScoreEventHandler, null);
	}

	public void GetTopScoreEventHandler(GameDataSummary gdata, object inUserData)
	{
		mScores = null;
		if (gdata != null && gdata.GameDataList != null)
		{
			mScores = new GRSScore[gdata.GameDataList.Length];
			int num = 0;
			GameData[] gameDataList = gdata.GameDataList;
			foreach (GameData gameData in gameDataList)
			{
				mScores[num] = new GRSScore();
				string text = gameData.UserName;
				if (Nicknames.pInstance != null)
				{
					string nickname = Nicknames.pInstance.GetNickname(gameData.UserID.ToString());
					if (!string.IsNullOrEmpty(nickname))
					{
						text = nickname;
					}
				}
				mScores[num]._Name = text;
				mScores[num]._Score = gameData.Value;
				num++;
			}
		}
		SetBillBoardMyScoreText("", "");
		mTopScoreDisplayTimer = _TopScoreDisplayTime;
		ShowTopScore();
	}

	private void ShowTopScore()
	{
		string text = "";
		string text2 = "";
		if (mScores != null)
		{
			text2 = mTxtScoreString + "\n";
			text = text + "\t" + mTxtNameString + "\n";
			for (int i = 0; i < mScores.Length; i++)
			{
				text = text + (i + 1) + "\t" + mScores[i]._Name + "\n";
				text2 = text2 + mScores[i]._Score + "\n";
			}
			SetBillBoardTopScoresText(mTxtTopScoreTitle, text, text2);
		}
		else
		{
			SetBillBoardMyScoreText(mTxtTopScoreTitle, mTxtNoScoresYet);
		}
		mScores = null;
	}

	private void SetTextToMesh(TextMesh inTextMesh, string inText)
	{
		if (inTextMesh != null)
		{
			FontInstance font = FontTable.GetFont(inTextMesh.font.name);
			if (font != null)
			{
				inTextMesh.font = font._Font;
				inTextMesh.GetComponent<Renderer>().material = font._Font.material;
			}
			inTextMesh.text = inText;
		}
	}

	private void SetBillBoardTopScoresText(string inTitle, string inText, string inScore)
	{
		if (mPreviousLevelPiece != null)
		{
			Transform transform = mPreviousLevelPiece.transform.Find("PfScoreScreen/TextTitle");
			Transform transform2 = mPreviousLevelPiece.transform.Find("PfScoreScreen/TextName");
			Transform transform3 = mPreviousLevelPiece.transform.Find("PfScoreScreen/TextTopScore");
			SetTextToMesh(transform.GetComponent<TextMesh>(), inTitle);
			SetTextToMesh(transform2.GetComponent<TextMesh>(), inText);
			SetTextToMesh(transform3.GetComponent<TextMesh>(), inScore);
		}
	}

	private void SetBillBoardMyScoreText(string inTitle, string inText)
	{
		if (mPreviousLevelPiece != null)
		{
			Transform transform = mPreviousLevelPiece.transform.Find("PfScoreScreen/TextTitle");
			Transform transform2 = mPreviousLevelPiece.transform.Find("PfScoreScreen/TextScore");
			SetTextToMesh(transform.GetComponent<TextMesh>(), inTitle);
			SetTextToMesh(transform2.GetComponent<TextMesh>(), inText);
		}
	}

	public void OnPlayTutorial(GauntletControlModifier inControlModifier)
	{
		if (mIsMultiplayer)
		{
			inControlModifier.ResumeStep(isForceResume: true);
			_GauntletController.transform.rotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
		}
		else if (!pIsTutDone)
		{
			ShowTutorial();
			mInControlModifier = inControlModifier;
		}
		else
		{
			inControlModifier.ResumeStep(isForceResume: true);
		}
	}

	public void OnPlayCountDown(GauntletControlModifier inControlModifier)
	{
		if (mLevelPieceList.Count > 0 || mPreviousLevelPiece != null)
		{
			if (mIsMultiplayer)
			{
				inControlModifier.ResumeStep(isForceResume: false);
				_GauntletController.transform.rotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
			}
			else
			{
				mInControlModifier = inControlModifier;
				mIsGameRunning = true;
				mIsShowCountDown = true;
			}
		}
	}

	public override void OnHelp()
	{
		base.OnHelp();
		int num = _HelpVO.Length;
		if (num > 0)
		{
			int num2 = UnityEngine.Random.Range(0, num);
			SnChannel.Play(_HelpVO[num2], "VO_Pool", inForce: false);
		}
	}

	public void OnJoinedRoom()
	{
		_EndDBUI.SetVisibility(Visibility: false);
		KAUICursorManager.SetDefaultCursor("Arrow");
		_MultiplayerMenuScreen.SetInteractive(interactive: true);
		_MultiplayerMenuScreen.SetVisibility(inVisible: false);
		_LevelSelectionScreen.SetInteractive(interactive: true);
		_LevelSelectionScreen.SetVisibility(visible: false);
		_MMOLobbyUI.SetVisibility(t: true);
	}

	public void DisplayGenericDialog(GameObject gO, string closeCallBack, string OKCallback, string yesCallBack, string noCallBack, int textID, string text, bool useDots, float displaytime, bool IsYesBtn, bool IsNoBtn, bool IsOKBtn, bool IsCloseBtn, bool isExclusiveUI, int inMessageIdentifier)
	{
		DestroyGenericDialog();
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSm"));
		mUiGenericDB = gameObject.GetComponent<KAUIGenericDB>();
		mUiGenericDB._MessageObject = gO;
		mUiGenericDB._CloseMessage = closeCallBack;
		mUiGenericDB._OKMessage = OKCallback;
		mUiGenericDB._YesMessage = yesCallBack;
		mUiGenericDB._NoMessage = noCallBack;
		mUiGenericDB.SetTextByID(textID, text, interactive: false);
		mUiGenericDB.SetButtonVisibility(IsYesBtn, IsNoBtn, IsOKBtn, IsCloseBtn);
		if (isExclusiveUI)
		{
			KAUI.SetExclusive(mUiGenericDB, _MaskColor);
		}
		mUiGenericDBText = mUiGenericDB.FindItem("TxtDialog");
		mGenericDBDisplayTimer = displaytime;
		mGenericDBText_UseDots = useDots;
		mGenericDBText = mUiGenericDBText.GetText();
		mGenericDBText_DotCount = 0;
		mUiGenericDB._MessageIdentifier = inMessageIdentifier;
	}

	public void DisplayGenericDialog(GameObject gO, int textID, string text, bool useDots, float displaytime, bool IsYesBtn, bool IsNoBtn, bool IsOKBtn, bool IsCloseBtn, bool isExclusiveUI)
	{
		DisplayGenericDialog(gO, textID, text, useDots, displaytime, IsYesBtn, IsNoBtn, IsOKBtn, IsCloseBtn, isExclusiveUI, -1);
	}

	public void DisplayGenericDialog(GameObject gO, int textID, string text, bool useDots, float displaytime, bool IsYesBtn, bool IsNoBtn, bool IsOKBtn, bool IsCloseBtn, bool isExclusiveUI, int inMessageIdentifier)
	{
		DisplayGenericDialog(gO, "OnClose", "OnOK", "OnYes", "OnNo", textID, text, useDots, displaytime, IsYesBtn, IsNoBtn, IsOKBtn, IsCloseBtn, isExclusiveUI, inMessageIdentifier);
	}

	public void DestroyGenericDialog()
	{
		if (mUiGenericDB != null)
		{
			KAUI.RemoveExclusive(mUiGenericDB);
			UnityEngine.Object.Destroy(mUiGenericDB.gameObject);
		}
	}

	private void UpdateGenericDB()
	{
		if (mGenericDBDisplayTimer > 0f)
		{
			mGenericDBDisplayTimer -= Time.deltaTime;
			if (mGenericDBDisplayTimer <= 0f)
			{
				DestroyGenericDialog();
			}
		}
		if (!mGenericDBText_UseDots || !(mUiGenericDB != null) || !(mUiGenericDBText != null))
		{
			return;
		}
		if (mGenericDBUpdateTimer > 0.15f)
		{
			mGenericDBUpdateTimer = 0f;
			string text = mUiGenericDBText.GetText();
			if (mGenericDBText_DotCount > 2)
			{
				text = mGenericDBText;
				mGenericDBText_DotCount = 0;
			}
			else
			{
				text += ".";
				mGenericDBText_DotCount++;
			}
			mUiGenericDBText.SetText(text);
		}
		else
		{
			mGenericDBUpdateTimer += Time.deltaTime;
		}
	}

	public void ShowMultiplayerMenu()
	{
		SetCursorVisible(isVisible: true);
		mIsGameRunning = false;
		if (GauntletMMOClient.pInstance != null)
		{
			GauntletMMOClient.pInstance.DestroyMMO(inLogout: true);
		}
		ResetLevel();
		_MMOResultsUI.SetVisibility(t: false);
		_MMOLobbyUI.SetVisibility(t: false);
		if (_BkgMusic != null)
		{
			SnChannel.Play(_BkgMusic, "Ambient_Pool", inForce: true);
		}
		TutorialManager.StopTutorials();
		SnChannel.StopPool("GSTargetTower_Pool");
		SnChannel.StopPool("GSTarget_Pool");
		SnChannel.StopPool("GSTargetChar_Pool");
		SnChannel.StopPool("GSFire_Pool");
		SnChannel.StopPool("DEFAULT_POOL");
		_MultiplayerMenuScreen.SetVisibility(inVisible: true);
		_MultiplayerMenuScreen.SetInteractive(interactive: true);
		_MenuCameraObject.SetActive(value: true);
		if (_GauntletController != null)
		{
			_GauntletController.gameObject.SetActive(value: false);
			_GauntletController.SetDisabled(isDisabled: true);
		}
		pOpponentLeft = false;
	}

	public void OnPlayAgain()
	{
		_EndDBUI.SetVisibility(Visibility: false);
		mIsGameRunning = false;
		ResetLevel();
		_MMOResultsUI.SetInteractive(interactive: true);
		_MMOResultsUI.SetVisibility(t: false);
		if (KAUIStore.pInstance == null)
		{
			_MMOLobbyUI.SetVisibility(t: true);
			_MenuCameraObject.SetActive(value: true);
		}
		if (_BkgMusic != null)
		{
			SnChannel.Play(_BkgMusic, "Ambient_Pool", inForce: true);
		}
		if (_GauntletController != null)
		{
			_GauntletController.gameObject.SetActive(value: false);
			_GauntletController.SetDisabled(isDisabled: true);
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
	}

	private void OnStoreClosed()
	{
		if (!_GauntletController.gameObject.activeSelf)
		{
			_EndDBUI.SetVisibility(Visibility: false);
			_MMOLobbyUI.SetVisibility(t: true);
			_MenuCameraObject.SetActive(value: true);
		}
	}

	public void ProcessUserLeft()
	{
		if (_PowerUpManager != null)
		{
			_PowerUpManager.DeactivateAllPowerUps();
		}
		if (pIsMultiplayer && !pOpponentLeft && GauntletMMOClient.pInstance != null && GauntletMMOClient.pInstance.pMMOState == GauntletMMOState.IN_GAME)
		{
			string moduleName = GetModuleName(isMultiplayer: true);
			_EndDBUI.SetAdRewardData(moduleName, -2);
			WsWebService.ApplyPayout(moduleName, -2, null, null);
		}
	}

	private void SetUpContent()
	{
		if (!CmLessonData.pSetByLessonMenu)
		{
			CmLessonData.pContentFile = "TbBtCaptures.xml";
			CmLessonData.pContentTableName = "tnLessonsMultiplyL1";
			CmLessonData.pAssetBundleName = "LessonsMultiply";
			CmLessonData.pMinQuestionsPerRound = 10;
			mMinNumQuestionsPerRound = 10;
		}
		else if (CmLessonData.pMinQuestionsPerRound == 0)
		{
			CmLessonData.pMinQuestionsPerRound = 10;
		}
		if (CmLessonData.pAssetBundleName == "null")
		{
			InitializeContent();
		}
		else
		{
			RsResourceManager.Load("RS_CONTENT/Assets/" + CmLessonData.pAssetBundleName, AssetBundleReady);
		}
	}

	public void AssetBundleReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inFile, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			mAssets = (AssetBundle)inFile;
			InitializeContent();
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Level content assest bundle could not be loaded from the given url : " + inURL);
			break;
		}
	}

	protected virtual void InitializeContent()
	{
		mCM = new CmInterface();
		mCM.InitCM(base.gameObject, "InitialContentReady");
	}

	protected virtual void InitialContentReady()
	{
		mMinNumQuestionsPerRound = mCM.GetMinQuestionsPerRound();
		LoadContent();
	}

	protected virtual void LoadContent()
	{
		mContent = null;
		mCM.LoadContentFile("LoadMathContent");
	}

	protected virtual void LoadContentFileReady()
	{
		string[] imgSize = new string[1] { _AnswerImgSize };
		string[] actorName = new string[1] { "Host" };
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("NumCorrect", _NumCorrectAnswers);
		dictionary.Add("NumDistract", _NumDistractAnswers);
		dictionary.Add("UseDisplay", true);
		dictionary.Add("AllowDups", true);
		mContent = mCM.GetContent<CmMCManyToMany>(mMinNumQuestionsPerRound, bUnique: true, "MCContentMany", actorName, imgSize, dictionary);
		mCurrentQuestionNum = 0;
		mCM.BeginRound();
	}

	private void DisplayQuestion()
	{
		string question = "";
		if (mContent.pGroupList[mCurrentQuestionNum].pDisplayArea.pText != null && mContent.pGroupList[mCurrentQuestionNum].pDisplayArea.pText.Count != 0)
		{
			string text = mContent.pGroupList[mCurrentQuestionNum].pDisplayArea.pText[0];
			for (int i = 1; i < mContent.pGroupList[mCurrentQuestionNum].pDisplayArea.pText.Count; i++)
			{
				text = text + " " + mContent.pGroupList[mCurrentQuestionNum].pDisplayArea.pText[i];
			}
			question = text;
		}
		if ((bool)mGameHud)
		{
			mGameHud.SetQuestion(question);
		}
	}

	private int GetAnswerIdx(List<CmAnswerItem> answerList)
	{
		int num = 0;
		foreach (CmAnswerItem answer in answerList)
		{
			if (answer.pbCorrect)
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	private void ShuffleAndListAnswers(List<CmAnswerItem> answerList, out List<CmAnswerItem> correctAnswers, out List<CmAnswerItem> wrongAnswers)
	{
		correctAnswers = new List<CmAnswerItem>();
		wrongAnswers = new List<CmAnswerItem>();
		foreach (CmAnswerItem answer in answerList)
		{
			if (answer.pbCorrect)
			{
				correctAnswers.Add(answer);
			}
			else
			{
				wrongAnswers.Add(answer);
			}
		}
		Shuffle(ref correctAnswers);
		Shuffle(ref wrongAnswers);
	}

	public Texture GetAnswerTexture(CmAnswerItem answer)
	{
		string text = "";
		if (answer.pImage != null && mAssets != null)
		{
			text = answer.pImage[_AnswerImgSize];
			Texture texture = (Texture)mAssets.LoadAsset(text);
			if (texture != null)
			{
				return texture;
			}
			Debug.LogError("Texture " + text + " not found in bundle");
		}
		return null;
	}

	public void Shuffle<T>(ref List<T> shuffleList)
	{
		System.Random random = new System.Random();
		for (int num = shuffleList.Count - 1; num >= 0; num--)
		{
			int index = random.Next(0, num + 1);
			T value = shuffleList[num];
			shuffleList[num] = shuffleList[index];
			shuffleList[index] = value;
		}
	}

	public void ReadTargets(GameObject[] targets)
	{
		List<GauntletTargetCharacter> shuffleList = new List<GauntletTargetCharacter>();
		foreach (GameObject gameObject in targets)
		{
			if ((bool)gameObject)
			{
				GauntletTargetCharacter component = gameObject.GetComponent<GauntletTargetCharacter>();
				if ((bool)component)
				{
					shuffleList.Add(component);
				}
			}
		}
		if (shuffleList.Count <= 0)
		{
			return;
		}
		Shuffle(ref shuffleList);
		ShuffleAndListAnswers(mContent.pGroupList[mCurrentQuestionNum].pAnswerList, out var correctAnswers, out var wrongAnswers);
		List<CmAnswerItem> list = mContent.pGroupList[mCurrentQuestionNum].pAnswerList;
		if (shuffleList.Count > 2)
		{
			int num = (int)((float)(shuffleList.Count * _CorrectTargetPercentage) / 100f);
			for (int j = 0; j < num; j++)
			{
				int index = UnityEngine.Random.Range(0, shuffleList.Count);
				int index2 = UnityEngine.Random.Range(0, correctAnswers.Count);
				CmAnswerItem cmAnswerItem = correctAnswers[index2];
				ApplyAnswerOnTarget(shuffleList[index], cmAnswerItem);
				shuffleList.Remove(shuffleList[index]);
				if (correctAnswers.Count > num - j)
				{
					correctAnswers.Remove(cmAnswerItem);
				}
			}
			list = wrongAnswers;
		}
		foreach (GauntletTargetCharacter item in shuffleList)
		{
			int index3 = UnityEngine.Random.Range(0, list.Count);
			CmAnswerItem answer = list[index3];
			ApplyAnswerOnTarget(item, answer);
		}
	}

	public void ApplyAnswerOnTarget(GauntletTargetCharacter target, CmAnswerItem answer)
	{
		if (answer.pImage == null)
		{
			if ((bool)_PfTargetText)
			{
				target.SetMathTargetByText(answer, _PfTargetText);
			}
		}
		else
		{
			target.SetMathTargetByImage(answer, GetAnswerTexture(answer));
		}
	}

	public void UpdateMathScore(CmAnswerItem answer, int score)
	{
		if (answer.pbCorrect)
		{
			mCorrectAnswersCount++;
			mCM.SetResultSuccess(answer.pRK);
		}
		else
		{
			mCM.SetResultFailure(answer.pRK);
		}
		mMathScore += score;
	}

	private string GetMathScoreText()
	{
		return mTxtMathCorrectAnswers + "\t" + mCorrectAnswersCount + "\n ";
	}

	private bool IsCurrentTrackBonusLevel()
	{
		GameObject[] rotatingChamberPieces = mLevelData._RotatingChamberPieces;
		foreach (GameObject gameObject in rotatingChamberPieces)
		{
			if (mLevelData._TrackPieces[mNumCrossedTrackPiece] == gameObject)
			{
				return true;
			}
		}
		return false;
	}

	public void HandleTargetHit(GameObject inTarget, int inScore, string inAnimName, GameObject inProjectile, bool isPowerUp = false)
	{
		Show3DTargetHitScore(inTarget.transform.position, inScore);
		if (!isPowerUp)
		{
			OnProjectileHit(inScore > 0, inProjectile);
		}
		AddScore(inScore);
		if (inAnimName != null && inAnimName.Length > 0)
		{
			Component[] componentsInChildren = inTarget.GetComponentsInChildren<Animation>();
			componentsInChildren = componentsInChildren;
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				((Animation)componentsInChildren[i]).CrossFade(inAnimName, 0.2f);
			}
		}
	}

	public void Show3DTargetHitScore(Vector3 inPosition, int inScore)
	{
		TargetHit3DScore.Show3DHitScore(_Hit3DScorePrefab, inPosition, inScore);
	}

	public void PlayNegativeSFX()
	{
		PlaySFX(_SFXWrongTargetHit);
	}

	private void PlaySFX(AudioClip inClip)
	{
		if (inClip != null)
		{
			SnChannel.Play(inClip, "GSFire_Pool", inForce: true);
		}
	}

	private void OnDestroy()
	{
		ObAmmo.pOnAmmoMissed = (ObAmmo.AmmoMissed)Delegate.Remove(ObAmmo.pOnAmmoMissed, new ObAmmo.AmmoMissed(OnAmmoMissed));
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: true);
		}
	}

	public Vector3 GetCameraOffsetForPet(int _PetTypeID, GSGameType gameType = GSGameType.UNKNOWN)
	{
		GSCameraOffsetData gSCameraOffsetData = _AvatarCameraOffsetList.Find((GSCameraOffsetData data) => data._GameType == ((gameType == GSGameType.UNKNOWN) ? pGameType : gameType));
		if (gSCameraOffsetData != null)
		{
			GSCameraOffsetData.AvatarCameraOffset[] avatarCameraOffset = gSCameraOffsetData._AvatarCameraOffset;
			foreach (GSCameraOffsetData.AvatarCameraOffset avatarCameraOffset2 in avatarCameraOffset)
			{
				if (avatarCameraOffset2._PetTypeID == _PetTypeID)
				{
					return avatarCameraOffset2._CameraOffset;
				}
			}
		}
		return Vector3.zero;
	}

	public bool IsPetTooTired()
	{
		if (SanctuaryManager.pCurPetInstance != null)
		{
			return SanctuaryManager.pCurPetInstance.GetMeterValue(SanctuaryPetMeterType.ENERGY) < _MinEnergyValue;
		}
		return false;
	}

	public void ProcessPetTired(GameObject msgObject)
	{
		mPetEnergyMsgObject = msgObject;
		if (IsPetTooTired())
		{
			DisplayTradeUI(GSPurchaseType.PET_ENERGY);
		}
		else
		{
			PetEnergyProcessDone();
		}
	}

	private void PetEnergyProcessDone()
	{
		KillGenericDB();
		if (mPetEnergyMsgObject != null)
		{
			mPetEnergyMsgObject.SendMessage("PetEnergyProcessed", SendMessageOptions.DontRequireReceiver);
		}
	}

	private void CrossbowPayGemsForPlayDone()
	{
		KillGenericDB();
		if (!_LevelSelectionScreen.GetVisibility())
		{
			_GauntletController.ProcessExit(ShowLevelSelectionMenu);
		}
	}

	private void KillGenericDB()
	{
		if (mKAUIGenericDB != null)
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			UnityEngine.Object.DestroyImmediate(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}

	private void DisplayTradeUI(GSPurchaseType type)
	{
		GSPurchaseData gSPurchaseData = _PurchaseDatas.Find((GSPurchaseData data) => data._Type == type);
		if (gSPurchaseData != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDB"));
			mKAUIGenericDB = (KAUIGenericDB)gameObject.GetComponent("KAUIGenericDB");
			mKAUIGenericDB._MessageObject = base.gameObject;
			KAUI.SetExclusive(mKAUIGenericDB, new Color(0.5f, 0.5f, 0.5f, 0.5f));
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
			string purchaseMessageText = GetPurchaseMessageText(type);
			mKAUIGenericDB.SetText(purchaseMessageText, interactive: false);
			mKAUIGenericDB._YesMessage = gSPurchaseData._YesMessage;
			mKAUIGenericDB._NoMessage = gSPurchaseData._NoMessage;
			mPurchaseType = type;
		}
	}

	private string GetPurchaseMessageText(GSPurchaseType type)
	{
		string text = "";
		GSPurchaseData gSPurchaseData = _PurchaseDatas.Find((GSPurchaseData data) => data._Type == type);
		if (gSPurchaseData != null)
		{
			text = gSPurchaseData._PruchaseText.GetLocalizedString();
			text = text.Replace("{{GEMS}}", gSPurchaseData.mGemCost.ToString());
			if (type == GSPurchaseType.CROSSBOW_NON_MEMBER_PLAY)
			{
				string value = ProductData.pPairData.GetValue("LCGT");
				DateTime minValue = DateTime.MinValue;
				TimeSpan timeSpan = DateTime.Parse(value, UtUtilities.GetCultureInfo("en-US")).AddHours(_CrossBowUnlockDurationInHours) - ServerTime.pCurrentTime;
				text = text.Replace("{{COUNT}}", ProductData.pPairData.GetIntValue("CB_PLAYED", 0).ToString());
				text = text.Replace("{{TIME}}", timeSpan.Hours + ":" + timeSpan.Minutes + ":" + timeSpan.Seconds);
			}
		}
		return text;
	}

	private int GetGemCost(GSPurchaseType type)
	{
		return _PurchaseDatas.Find((GSPurchaseData data) => data._Type == type)?.mGemCost ?? 0;
	}

	private void PayGemsForPurchase()
	{
		GSPurchaseData gSPurchaseData = _PurchaseDatas.Find((GSPurchaseData data) => data._Type == mPurchaseType);
		if (gSPurchaseData != null)
		{
			KillGenericDB();
			if (Money.pCashCurrency >= GetGemCost(mPurchaseType))
			{
				PurchaseItem(mPurchaseType, OnPurchaseDone);
			}
			else
			{
				ShowBuyGemsPopup(_NotEnoughGemsText, "BuyGemsOnline", gSPurchaseData._NoMessage);
			}
		}
	}

	private void ShowBuyGemsPopup(LocaleString textMessage, string yesMessage, string noMessage)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDB"));
		mKAUIGenericDB = (KAUIGenericDB)gameObject.GetComponent("KAUIGenericDB");
		mKAUIGenericDB._MessageObject = base.gameObject;
		KAUI.SetExclusive(mKAUIGenericDB, new Color(0.5f, 0.5f, 0.5f, 0.5f));
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
		mKAUIGenericDB.SetText(textMessage.GetLocalizedString(), interactive: false);
		mKAUIGenericDB._YesMessage = yesMessage;
		mKAUIGenericDB._NoMessage = noMessage;
	}

	private void BuyGemsOnline()
	{
		KillGenericDB();
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
	}

	public void OnIAPStoreClosed()
	{
		DisplayTradeUI(mPurchaseType);
	}

	private void PurchaseItem(GSPurchaseType type, PurchaseEventHandler callback)
	{
		GSPurchaseData gSPurchaseData = _PurchaseDatas.Find((GSPurchaseData data) => data._Type == type);
		if (gSPurchaseData != null)
		{
			CommonInventoryData.pInstance.AddPurchaseItem(gSPurchaseData._ItemID, 1, ItemPurchaseSource.GAUNTLET_SHOOTER.ToString());
			CommonInventoryData.pInstance.DoPurchase(2, gSPurchaseData._StoreID, callback);
			KillGenericDB();
			mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: false);
			mKAUIGenericDB.SetText(_PurchaseProcessingText.GetLocalizedString(), interactive: false);
			KAUI.SetExclusive(mKAUIGenericDB);
		}
	}

	public void OnPurchaseDone(CommonInventoryResponse ret)
	{
		GSPurchaseData gSPurchaseData = _PurchaseDatas.Find((GSPurchaseData data) => data._Type == mPurchaseType);
		if (gSPurchaseData == null)
		{
			return;
		}
		KillGenericDB();
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		if (ret != null && ret.Success)
		{
			mKAUIGenericDB.SetText(gSPurchaseData._SuccessfulText.GetLocalizedString(), interactive: false);
			switch (mPurchaseType)
			{
			case GSPurchaseType.PET_ENERGY:
			{
				float num = 1f;
				num = SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.ENERGY, SanctuaryManager.pCurPetData);
				SanctuaryManager.pCurPetInstance.SetMeter(SanctuaryPetMeterType.ENERGY, num);
				break;
			}
			case GSPurchaseType.CROSSBOW_NON_MEMBER_PLAY:
				ProductData.pPairData.SetValueAndSave("CB_PLAYED", "0");
				break;
			}
		}
		else
		{
			mKAUIGenericDB.SetText(gSPurchaseData._FailText.GetLocalizedString(), interactive: false);
		}
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._OKMessage = gSPurchaseData._OnPurchaseCompleteMessage;
		KAUI.SetExclusive(mKAUIGenericDB);
	}

	public void OnStoreLoaded(StoreData sd)
	{
		foreach (GSPurchaseData purchaseData in _PurchaseDatas)
		{
			ItemData[] items = sd._Items;
			foreach (ItemData itemData in items)
			{
				if (itemData.ItemID == purchaseData._ItemID && sd._ID == purchaseData._StoreID)
				{
					purchaseData.mGemCost = itemData.FinalCashCost;
				}
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

	public void ApplyExtraTime(bool extraTime, float duration)
	{
		mExtraTime += duration;
		_GauntletController._ExtraTime = extraTime;
		if (!extraTime)
		{
			mTime = 0f;
			mExtraTime = 0f;
			mGameHud.ShowExtraTime(0f);
		}
		if (_GauntletController.pCurrentControlModifier != null)
		{
			_GauntletController.pCurrentControlModifier._PauseStates = extraTime;
		}
	}

	public void Collect()
	{
		if (_UiConsumable != null)
		{
			Consumable consumableOnProbability = ConsumableData.GetConsumableOnProbability("TargetPractice", "Game", 1);
			if (consumableOnProbability != null)
			{
				_UiConsumable.RegisterConsumable(consumableOnProbability);
			}
			else
			{
				Debug.LogError("ERROR! did not find a suitable consumable");
			}
		}
	}

	public void FireProjectile()
	{
		_GauntletController.FireAProjectile(isPowerUp: true);
	}

	public void OnBombExplode(Vector3 inBombPos, float radius)
	{
		if (_CurrentActiveTargets.Count > 0)
		{
			foreach (GameObject currentActiveTarget in _CurrentActiveTargets)
			{
				if (!(currentActiveTarget != null))
				{
					continue;
				}
				GauntletTarget[] componentsInChildren = currentActiveTarget.GetComponentsInChildren<GauntletTarget>();
				if (componentsInChildren != null)
				{
					GauntletTarget[] array = componentsInChildren;
					foreach (GauntletTarget gauntletTarget in array)
					{
						HandleTargetHit(gauntletTarget.gameObject, gauntletTarget._HitScore, gauntletTarget._HitAnim, null, isPowerUp: true);
						gauntletTarget.ActivateTarget(active: false);
						gauntletTarget.HandleHitBehaviour();
					}
				}
			}
		}
		else
		{
			UtDebug.LogError("Target Count is ZERO");
		}
		_CurrentActiveTargets.Clear();
	}

	public GameObject GetParentObjectForPowerUp(MMOMessageReceivedEventArgs args)
	{
		return _GauntletController._AvatarCamera.gameObject;
	}

	public void EnteredHallWay()
	{
		_UiConsumable.SetVisibility(inVisible: true);
	}

	public bool CanApplyEffect(MMOMessageReceivedEventArgs args, string powerUpType)
	{
		if (powerUpType == "Yaknog" && GauntletMMOClient.pInstance != null)
		{
			return true;
		}
		return false;
	}

	public bool CanActivatPowerUp(string powerUpType)
	{
		if (powerUpType == "Yaknog" && GauntletMMOClient.pInstance != null)
		{
			return true;
		}
		return false;
	}
}
