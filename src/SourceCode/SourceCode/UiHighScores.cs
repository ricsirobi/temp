using UnityEngine;

public class UiHighScores : KAUI
{
	public GameInfo[] _GameInfoList;

	public LocaleString _MultiplayerText = new LocaleString("Multi-Player");

	public LocaleString _SingleplayerText = new LocaleString("Single Player");

	public LocaleString _HighScoresText = new LocaleString("HIGH SCORES");

	public LocaleString _MyScoresText = new LocaleString("MY SCORES");

	public LocaleString _CoinsWonText = new LocaleString("COINS WON");

	public LocaleString _BuddiesText = new LocaleString("BUDDIES");

	public UiReward _Reward;

	private int mCurrentGameId;

	private int mCurrentLevelId;

	private UiHighScoresMenu mMenu;

	private HighScoreDisplayPage mHighScoresDisplayPage;

	private bool mParseThroughAllGames = true;

	private bool mNavigateFromRewardsPage;

	private bool mWebServiceCompleted = true;

	private bool mIsMultiPlayer;

	private KAWidget mLoadingStatusDisplayItem;

	private KAWidget mPrevPageBtn;

	private KAWidget mNextPageBtn;

	private KAWidget mBackGround;

	private KAWidget mPlayerModeText;

	private KAWidget mScrollRtBtnGame;

	private KAWidget mScrollLtBtnGame;

	private KAWidget mScrollRtBtnSub;

	private KAWidget mScrollLtBtnSub;

	private KAWidget mScrollRtBtnMode;

	private KAWidget mScrollLtBtnMode;

	private KAWidget mBuddyzBackground;

	private KAWidget mHighScoresBackground;

	private KAWidget mMyScoresBackground;

	private bool mStartComplete;

	private bool mLaunchInterface;

	private eLaunchPage mDisplayType;

	public int GetCurrentGameID()
	{
		return mCurrentGameId;
	}

	public int GetCurrentLevelID()
	{
		return mCurrentLevelId;
	}

	protected override void Start()
	{
		base.Start();
		mMenu = (UiHighScoresMenu)GetMenu("UiHighScoresMenu");
		mLoadingStatusDisplayItem = FindItem("Loading");
		mLoadingStatusDisplayItem.SetVisibility(inVisible: false);
		mPrevPageBtn = FindItem("ScrollLtBtn");
		mNextPageBtn = FindItem("ScrollRtBtn");
		mPrevPageBtn.SetVisibility(inVisible: false);
		mNavigateFromRewardsPage = false;
		mBackGround = FindItem("Background");
		mBuddyzBackground = mBackGround.FindChildItem("MyBuddyScores");
		mHighScoresBackground = mBackGround.FindChildItem("HighScores");
		mMyScoresBackground = mBackGround.FindChildItem("MyScores");
		mPlayerModeText = FindItem("TxtGameTitle").FindChildItem("TxtPlayerMode");
		mScrollRtBtnGame = FindItem("ScrollRtBtnGame");
		mScrollLtBtnGame = FindItem("ScrollLtBtnGame");
		mScrollRtBtnSub = FindItem("ScrollRtBtnSub");
		mScrollLtBtnSub = FindItem("ScrollLtBtnSub");
		mScrollRtBtnMode = FindItem("ScrollRtBtnMode");
		mScrollLtBtnMode = FindItem("ScrollLtBtnMode");
		SetDataFromCurrentGameSettings();
		if (HighScores.pInstance.GameID == 0)
		{
			mParseThroughAllGames = true;
		}
		else
		{
			mParseThroughAllGames = false;
			mScrollRtBtnGame.SetVisibility(inVisible: false);
			mScrollLtBtnGame.SetVisibility(inVisible: false);
			if (_GameInfoList[mCurrentGameId]._GameLevels.Length == 1)
			{
				mScrollRtBtnSub.SetVisibility(inVisible: false);
				mScrollLtBtnSub.SetVisibility(inVisible: false);
			}
			if (HighScores.pInstance.IsMultiPlayer)
			{
				mPlayerModeText.SetTextByID(_MultiplayerText._ID, _MultiplayerText._Text);
			}
			else
			{
				mPlayerModeText.SetTextByID(_SingleplayerText._ID, _SingleplayerText._Text);
			}
		}
		mStartComplete = true;
	}

	protected override void Update()
	{
		if (mLaunchInterface)
		{
			mLaunchInterface = false;
			if (mDisplayType == eLaunchPage.PAYOUT)
			{
				_Reward.SetVisibility(inVisible: true);
				SetVisibility(isVisible: false);
			}
			else if (mDisplayType == eLaunchPage.HIGHSCORE || mDisplayType == eLaunchPage.BUDDYSCORE)
			{
				_Reward.SetVisibility(inVisible: false);
				SetVisibility(isVisible: true);
			}
		}
	}

	public void SetCurrentPage()
	{
		mNavigateFromRewardsPage = true;
		SetDataFromCurrentGameSettings();
		SetVisibility(isVisible: true);
		mPrevPageBtn.SetVisibility(inVisible: true);
		mPrevPageBtn.FindChildItem("TxtPageBack").SetTextByID(_CoinsWonText._ID, _CoinsWonText._Text);
		mNextPageBtn.SetVisibility(inVisible: true);
		mNextPageBtn.FindChildItem("TxtNextBack").SetTextByID(_MyScoresText._ID, _MyScoresText._Text);
	}

	public override void SetVisibility(bool isVisible)
	{
		base.SetVisibility(isVisible);
		if (isVisible && mStartComplete)
		{
			SetCurrentDisplayPage(HighScoreDisplayPage.ALLSCORES);
			LoadGameDataSummary(mCurrentGameId, mCurrentLevelId);
		}
	}

	public void SetDataFromCurrentGameSettings()
	{
		if (HighScores.pInstance == null)
		{
			return;
		}
		for (int i = 0; i < _GameInfoList.Length; i++)
		{
			if (HighScores.pInstance.GameID != _GameInfoList[i]._GameID)
			{
				continue;
			}
			mCurrentGameId = i;
			int num = 0;
			GameLevelInfo[] gameLevels = _GameInfoList[i]._GameLevels;
			foreach (GameLevelInfo gameLevelInfo in gameLevels)
			{
				if (HighScores.pInstance.Difficulty == (int)gameLevelInfo._Difficulty && HighScores.pInstance.IsMultiPlayer == gameLevelInfo._IsMultiPlayer && HighScores.pInstance.Level == gameLevelInfo._Level)
				{
					mCurrentLevelId = num;
				}
				num++;
			}
		}
	}

	private GameDataSummary GetGameDataSummary(int GameIndex, int level)
	{
		if (GameIndex < _GameInfoList.Length && level < _GameInfoList[GameIndex]._GameLevels.Length)
		{
			return _GameInfoList[GameIndex]._GameLevels[level].GetGameDataSummary(mHighScoresDisplayPage);
		}
		return null;
	}

	private void HandleLevelAndModeArrows()
	{
		if (_GameInfoList[mCurrentGameId]._GameLevels.Length == 1)
		{
			mScrollRtBtnSub.SetVisibility(inVisible: false);
			mScrollLtBtnSub.SetVisibility(inVisible: false);
		}
		else
		{
			mScrollRtBtnSub.SetVisibility(inVisible: true);
			mScrollLtBtnSub.SetVisibility(inVisible: true);
		}
		bool flag = false;
		GameLevelInfo[] gameLevels = _GameInfoList[mCurrentGameId]._GameLevels;
		for (int i = 0; i < gameLevels.Length; i++)
		{
			flag = gameLevels[i]._IsMultiPlayer;
		}
		if (flag)
		{
			mScrollRtBtnMode.SetVisibility(inVisible: true);
			mScrollLtBtnMode.SetVisibility(inVisible: true);
			mPlayerModeText.SetVisibility(inVisible: true);
		}
		else
		{
			mScrollRtBtnMode.SetVisibility(inVisible: false);
			mScrollLtBtnMode.SetVisibility(inVisible: false);
			mPlayerModeText.SetVisibility(inVisible: false);
		}
	}

	private void EnableLevelAndModeArrows(bool inEnable)
	{
		if (mScrollRtBtnSub != null && mScrollRtBtnSub.GetVisibility())
		{
			mScrollRtBtnSub.SetDisabled(!inEnable);
		}
		if (mScrollLtBtnSub != null && mScrollLtBtnSub.GetVisibility())
		{
			mScrollLtBtnSub.SetDisabled(!inEnable);
		}
		if (mScrollRtBtnMode != null && mScrollRtBtnMode.GetVisibility())
		{
			mScrollRtBtnMode.SetDisabled(!inEnable);
		}
		if (mScrollLtBtnMode != null && mScrollLtBtnMode.GetVisibility())
		{
			mScrollLtBtnMode.SetDisabled(!inEnable);
		}
	}

	public void LoadGameDataSummary(int GameIndex, int LevelIndex)
	{
		HandleLevelAndModeArrows();
		EnableLevelAndModeArrows(inEnable: false);
		mIsMultiPlayer = _GameInfoList[GameIndex]._GameLevels[LevelIndex]._IsMultiPlayer;
		if (_GameInfoList.Length <= GameIndex)
		{
			return;
		}
		GameInfo gameInfo = _GameInfoList[GameIndex];
		if (gameInfo._GameLevels.Length <= LevelIndex)
		{
			return;
		}
		GameLevelInfo gameLevelInfo = gameInfo._GameLevels[LevelIndex];
		FindItem("TxtGameTitle").SetTextByID(gameInfo._TitleTextID, gameInfo._Title);
		FindItem("TxtGameTitle").FindChildItem("TxtSubCategory").SetTextByID(gameLevelInfo._TitleTextID, gameLevelInfo._Title);
		FindItem("TxtScore").SetVisibility(gameLevelInfo._Key != "time");
		FindItem("TxtTime").SetVisibility(gameLevelInfo._Key == "time");
		GameDataSummary gameDataSummary = GetGameDataSummary(GameIndex, LevelIndex);
		if (gameDataSummary != null && gameDataSummary.GameDataList != null)
		{
			mMenu.SetMenuData(gameDataSummary, null);
			return;
		}
		string userID = UserInfo.pInstance.UserID;
		switch (mHighScoresDisplayPage)
		{
		case HighScoreDisplayPage.MYBUDDYSCORES:
			if (gameLevelInfo != null && gameInfo != null)
			{
				int? score2 = null;
				if (HighScores.GetGameData(0) != null && GetCurrentGameConfiguration(gameInfo, gameLevelInfo))
				{
					score2 = int.Parse(HighScores.GetGameData(gameLevelInfo._Key).Item);
				}
				WsWebService.GetGameDataByGame(userID, gameInfo._GameID, gameLevelInfo._IsMultiPlayer, (int)gameLevelInfo._Difficulty, gameLevelInfo._Level, gameLevelInfo._Key, gameLevelInfo._Count, gameLevelInfo._AscendingOrder, score2, buddyFilter: true, ServiceEventHandler, null);
			}
			break;
		case HighScoreDisplayPage.ALLSCORES:
			if (gameLevelInfo != null && gameInfo != null)
			{
				int? score = null;
				if (HighScores.GetGameData(0) != null && GetCurrentGameConfiguration(gameInfo, gameLevelInfo))
				{
					score = int.Parse(HighScores.GetGameData(gameLevelInfo._Key).Item);
				}
				WsWebService.GetGameDataByGame(userID, gameInfo._GameID, gameLevelInfo._IsMultiPlayer, (int)gameLevelInfo._Difficulty, gameLevelInfo._Level, gameLevelInfo._Key, gameLevelInfo._Count, gameLevelInfo._AscendingOrder, score, buddyFilter: false, ServiceEventHandler, null);
			}
			break;
		case HighScoreDisplayPage.MYSCORES:
			WsWebService.GetGameDataByUser(userID, gameInfo._GameID, gameLevelInfo._IsMultiPlayer, (int)gameLevelInfo._Difficulty, gameLevelInfo._Level, gameLevelInfo._Key, gameLevelInfo._Count, gameLevelInfo._AscendingOrder, ServiceEventHandler, null);
			break;
		}
		mMenu.RemoveHighScores();
	}

	private void AddToGameSummary(GameDataSummary datasummary)
	{
		GameInfo[] gameInfoList = _GameInfoList;
		foreach (GameInfo gameInfo in gameInfoList)
		{
			if (datasummary.GameID != gameInfo._GameID)
			{
				continue;
			}
			GameLevelInfo[] gameLevels = gameInfo._GameLevels;
			foreach (GameLevelInfo gameLevelInfo in gameLevels)
			{
				if (gameLevelInfo._Level == datasummary.GameLevel && gameLevelInfo._IsMultiPlayer == datasummary.IsMultiplayer && gameLevelInfo._Difficulty == (HighScoresDifficulty)datasummary.Difficulty)
				{
					gameLevelInfo.SetGameDataSummary(datasummary, mHighScoresDisplayPage);
					return;
				}
			}
		}
	}

	public void ScrollNextModeSelection()
	{
		int num;
		if (mIsMultiPlayer)
		{
			mIsMultiPlayer = false;
			mPlayerModeText.SetTextByID(_SingleplayerText._ID, _SingleplayerText._Text);
			num = -1;
		}
		else
		{
			mIsMultiPlayer = true;
			mPlayerModeText.SetTextByID(_MultiplayerText._ID, _MultiplayerText._Text);
			num = 1;
		}
		int i = mCurrentLevelId;
		int level = _GameInfoList[mCurrentGameId]._GameLevels[i]._Level;
		for (int difficulty = (int)_GameInfoList[mCurrentGameId]._GameLevels[i]._Difficulty; _GameInfoList[mCurrentGameId]._GameLevels[i]._Level != level || _GameInfoList[mCurrentGameId]._GameLevels[i]._IsMultiPlayer != mIsMultiPlayer || _GameInfoList[mCurrentGameId]._GameLevels[i]._Difficulty != (HighScoresDifficulty)difficulty; i += num)
		{
		}
		mCurrentLevelId = i;
		LoadGameDataSummary(mCurrentGameId, mCurrentLevelId);
	}

	public void ScrollNextGameSelection(int dir)
	{
		if (HighScores.pInstance.GameID != 0)
		{
			return;
		}
		mPlayerModeText.SetTextByID(_SingleplayerText._ID, _SingleplayerText._Text);
		if (dir > 0)
		{
			if (mParseThroughAllGames)
			{
				if (mCurrentGameId < _GameInfoList.Length - 1)
				{
					mCurrentGameId++;
				}
				else
				{
					mCurrentGameId = 0;
				}
			}
			mCurrentLevelId = 0;
		}
		else
		{
			if (mParseThroughAllGames)
			{
				if (mCurrentGameId > 0)
				{
					mCurrentGameId--;
				}
				else
				{
					mCurrentGameId = _GameInfoList.Length - 1;
				}
			}
			mCurrentLevelId = 0;
		}
		LoadGameDataSummary(mCurrentGameId, mCurrentLevelId);
	}

	public void ScrollNextLevelSelection(int dir)
	{
		if (HighScores.pInstance.GameID != 0 && _GameInfoList[mCurrentGameId]._GameLevels.Length == 1)
		{
			return;
		}
		if (dir > 0)
		{
			if (mCurrentLevelId < _GameInfoList[mCurrentGameId]._GameLevels.Length - 1)
			{
				mCurrentLevelId++;
			}
			else
			{
				mCurrentLevelId = 0;
			}
		}
		else if (mCurrentLevelId > 0)
		{
			mCurrentLevelId--;
		}
		else
		{
			mCurrentLevelId = _GameInfoList[mCurrentGameId]._GameLevels.Length - 1;
		}
		int num = mCurrentLevelId;
		int num2 = _GameInfoList[mCurrentGameId]._GameLevels.Length;
		while (_GameInfoList[mCurrentGameId]._GameLevels[num]._IsMultiPlayer != mIsMultiPlayer)
		{
			num += dir;
			if (num < 0)
			{
				num = num2 - 1;
			}
			if (num >= num2)
			{
				num = 0;
			}
		}
		mCurrentLevelId = num;
		LoadGameDataSummary(mCurrentGameId, mCurrentLevelId);
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			mWebServiceCompleted = true;
			GameDataSummary gameDataSummary = (GameDataSummary)inObject;
			mLoadingStatusDisplayItem.SetVisibility(inVisible: false);
			if (gameDataSummary != null)
			{
				mMenu.SetMenuData(gameDataSummary, null);
				AddToGameSummary(gameDataSummary);
				EnableLevelAndModeArrows(inEnable: true);
			}
			break;
		}
		case WsServiceEvent.ERROR:
			mLoadingStatusDisplayItem.SetVisibility(inVisible: false);
			mWebServiceCompleted = true;
			EnableLevelAndModeArrows(inEnable: true);
			break;
		case WsServiceEvent.PROGRESS:
			mLoadingStatusDisplayItem.SetVisibility(inVisible: true);
			mWebServiceCompleted = false;
			break;
		}
	}

	public bool IsWebServiceDone()
	{
		return mWebServiceCompleted;
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item.name == "CloseBtn" && mWebServiceCompleted)
		{
			HighScores.ClearGameData();
			if (HighScores.pInstance != null && HighScores.pInstance.mMessageObject != null)
			{
				HighScores.pInstance.mMessageObject.SendMessage("OnHighScoresDone", null, SendMessageOptions.DontRequireReceiver);
			}
			Object.Destroy(base.gameObject);
		}
		else if (item == mScrollRtBtnSub)
		{
			ScrollNextLevelSelection(1);
		}
		else if (item == mScrollLtBtnSub)
		{
			ScrollNextLevelSelection(-1);
		}
		else if (item == mScrollRtBtnGame)
		{
			ScrollNextGameSelection(1);
		}
		else if (item == mScrollLtBtnGame)
		{
			ScrollNextGameSelection(-1);
		}
		else if (item == mScrollLtBtnMode || item == mScrollRtBtnMode)
		{
			ScrollNextModeSelection();
		}
		else if (item == mNextPageBtn)
		{
			if (mHighScoresDisplayPage == HighScoreDisplayPage.ALLSCORES && mWebServiceCompleted)
			{
				mHighScoresDisplayPage = HighScoreDisplayPage.MYSCORES;
				mPrevPageBtn.FindChildItem("TxtPageBack").SetTextByID(_HighScoresText._ID, _HighScoresText._Text);
				mPrevPageBtn.SetVisibility(inVisible: true);
				mNextPageBtn.FindChildItem("TxtNextBack").SetTextByID(_BuddiesText._ID, _BuddiesText._Text);
				mNextPageBtn.SetVisibility(inVisible: true);
				mMyScoresBackground.SetVisibility(inVisible: true);
				mHighScoresBackground.SetVisibility(inVisible: false);
				LoadGameDataSummary(mCurrentGameId, mCurrentLevelId);
			}
			else if (mHighScoresDisplayPage == HighScoreDisplayPage.MYSCORES && mWebServiceCompleted)
			{
				mHighScoresDisplayPage = HighScoreDisplayPage.MYBUDDYSCORES;
				mPrevPageBtn.FindChildItem("TxtPageBack").SetTextByID(_MyScoresText._ID, _MyScoresText._Text);
				mPrevPageBtn.SetVisibility(inVisible: true);
				mNextPageBtn.SetVisibility(inVisible: false);
				mBuddyzBackground.SetVisibility(inVisible: true);
				mMyScoresBackground.SetVisibility(inVisible: false);
				LoadGameDataSummary(mCurrentGameId, mCurrentLevelId);
			}
			else if (mHighScoresDisplayPage == HighScoreDisplayPage.REWARDS)
			{
				mHighScoresDisplayPage = HighScoreDisplayPage.ALLSCORES;
				mPrevPageBtn.FindChildItem("TxtPageBack").SetTextByID(_CoinsWonText._ID, _CoinsWonText._Text);
				mNextPageBtn.FindChildItem("TxtNextBack").SetTextByID(_MyScoresText._ID, _MyScoresText._Text);
				mHighScoresBackground.SetVisibility(inVisible: true);
				LoadGameDataSummary(mCurrentGameId, mCurrentLevelId);
			}
		}
		else
		{
			if (!(item == mPrevPageBtn))
			{
				return;
			}
			if (mHighScoresDisplayPage == HighScoreDisplayPage.MYBUDDYSCORES && mWebServiceCompleted)
			{
				mHighScoresDisplayPage = HighScoreDisplayPage.MYSCORES;
				mPrevPageBtn.FindChildItem("TxtPageBack").SetTextByID(_HighScoresText._ID, _HighScoresText._Text);
				mPrevPageBtn.SetVisibility(inVisible: true);
				mNextPageBtn.FindChildItem("TxtNextBack").SetTextByID(_BuddiesText._ID, _BuddiesText._Text);
				mNextPageBtn.SetVisibility(inVisible: true);
				mMyScoresBackground.SetVisibility(inVisible: true);
				mBuddyzBackground.SetVisibility(inVisible: false);
				LoadGameDataSummary(mCurrentGameId, mCurrentLevelId);
			}
			else if (mHighScoresDisplayPage == HighScoreDisplayPage.MYSCORES && mWebServiceCompleted)
			{
				mHighScoresDisplayPage = HighScoreDisplayPage.ALLSCORES;
				mPrevPageBtn.SetVisibility(inVisible: false);
				mNextPageBtn.FindChildItem("TxtNextBack").SetTextByID(_MyScoresText._ID, _MyScoresText._Text);
				mNextPageBtn.SetVisibility(inVisible: true);
				if (mNavigateFromRewardsPage)
				{
					mPrevPageBtn.SetVisibility(inVisible: true);
					mPrevPageBtn.FindChildItem("TxtPageBack").SetTextByID(_CoinsWonText._ID, _CoinsWonText._Text);
				}
				mHighScoresBackground.SetVisibility(inVisible: true);
				mMyScoresBackground.SetVisibility(inVisible: false);
				LoadGameDataSummary(mCurrentGameId, mCurrentLevelId);
			}
			else if (mHighScoresDisplayPage == HighScoreDisplayPage.ALLSCORES && mWebServiceCompleted)
			{
				mHighScoresDisplayPage = HighScoreDisplayPage.REWARDS;
				SetVisibility(isVisible: false);
				_Reward.SetVisibility(inVisible: true);
			}
		}
	}

	public HighScoreDisplayPage GetCurrentDisplayPage()
	{
		return mHighScoresDisplayPage;
	}

	public void SetCurrentDisplayPage(HighScoreDisplayPage page)
	{
		mHighScoresDisplayPage = page;
	}

	public void SetLoadingStatus(bool Value)
	{
		mLoadingStatusDisplayItem.SetVisibility(Value);
	}

	public bool GetCurrentGameConfiguration(GameInfo gameInfo, GameLevelInfo levelInfo)
	{
		if (HighScores.pInstance.GameID == gameInfo._GameID && HighScores.pInstance.IsMultiPlayer == levelInfo._IsMultiPlayer && HighScores.pInstance.Difficulty == (int)levelInfo._Difficulty)
		{
			return HighScores.pInstance.Level == levelInfo._Level;
		}
		return false;
	}

	public void LaunchInterface(eLaunchPage displayType)
	{
		mLaunchInterface = true;
		mDisplayType = displayType;
	}
}
