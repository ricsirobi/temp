using KnowledgeAdventure.Multiplayer.Events;
using SOD.Event;
using UnityEngine;

public class UiRaceResults : KAUI
{
	public int _GameID;

	public EndGameAchievements[] _EndGameAchievements;

	public UiRaceRewards _RewardScreen;

	public LocaleString _ChallengeTryAgainText = new LocaleString("Nice Try! Why dont you try again.");

	public LocaleString _ChallengeCompleteText = new LocaleString("Amazing! you beat [Name]'s challenge");

	public float _PayoutCallWaitTime = 60f;

	public UiDragonsEndDB _ResultUI;

	public UIEventRewardAchievement _UIEventRewardAchievement;

	private int mRewardCoins;

	private int mRewardTrophies;

	private int mRewardPoints;

	private int mDragonPoints;

	private KAWidget mTrackName;

	private KAWidget mOkBtn;

	private KAWidget mBtnChallenge;

	private string mTxtChallengeResult = string.Empty;

	private LevelManager mLevelManager;

	private UiRaceResultsMenu mMenu;

	private int mPlayerPos;

	private string mCurrentModuleName;

	private bool mApplyPayoutDone;

	private float mPayoutCallTimer;

	private bool mIsPayoutStuck;

	private bool mPayoutDone;

	private KAUIGenericDB mKAUIGenericDB;

	private float mRaceEndTime;

	private bool mIsReplayGame;

	private bool mResultShown;

	private AchievementReward[] mAchievementRewards;

	private bool mCanEnableResultUI;

	protected override void Start()
	{
		base.Start();
		mMenu = (UiRaceResultsMenu)GetMenu("UiRaceResultsMenu");
		mTrackName = FindItem("TxtTrackName");
		mOkBtn = FindItem("BtnOK");
		mBtnChallenge = FindItem("BtnChallenge");
		SetVisibility(inVisible: false);
	}

	public void Initialize(MMOExtensionResponseReceivedEventArgs args, LevelManager levelManager = null)
	{
		if (_ResultUI != null && _ResultUI.GetVisibility())
		{
			return;
		}
		if (mLevelManager == null)
		{
			mLevelManager = levelManager;
			InitSetup();
		}
		mBtnChallenge.SetVisibility(args == null);
		mBtnChallenge.SetDisabled(isDisabled: false);
		ChallengeInfo pActiveChallenge = ChallengeInfo.pActiveChallenge;
		if (pActiveChallenge != null)
		{
			switch (ChallengeInfo.CheckForChallengeCompletion(_GameID, mLevelManager._TrackID, 0, (int)mLevelManager.pElapsedTime, isTimerUsedAsPoints: true))
			{
			case ChallengeResultState.LOST:
				mTxtChallengeResult = _ChallengeTryAgainText.GetLocalizedString();
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
							mTxtChallengeResult = localizedString;
							flag = true;
						}
					}
					if (!flag)
					{
						mTxtChallengeResult = string.Empty;
						WsWebService.GetDisplayNameByUserID(pActiveChallenge.UserID.ToString(), ServiceEventHandler, null);
					}
				}
				else
				{
					mTxtChallengeResult = localizedString;
				}
				break;
			}
			}
		}
		else
		{
			mTxtChallengeResult = string.Empty;
		}
		ChallengeInfo.pActiveChallenge = null;
		UiChallengeInvite.SetData(_GameID, mLevelManager._TrackID, 0, (int)mLevelManager.pElapsedTime);
		AvAvatar.pState = AvAvatarState.PAUSED;
		if (args == null)
		{
			ShowSinglePlayerResult();
		}
		else if (!mResultShown)
		{
			SetVisibility(inVisible: true);
			mMenu.SetVisibility(inVisible: true);
			ShowMultiPlayerResult(args);
		}
	}

	private void InitSetup()
	{
		if (mTrackName != null && mLevelManager._TrackName != null)
		{
			mTrackName.SetText(mLevelManager._TrackName);
			mTrackName.SetVisibility(inVisible: true);
		}
		if (mOkBtn != null && !mPayoutDone)
		{
			mOkBtn.SetInteractive(isInteractive: false);
			KAUICursorManager.SetDefaultCursor("Loading");
		}
	}

	protected override void Update()
	{
		base.Update();
		if (GetVisibility() && UiJoystick.pInstance != null && UiJoystick.pInstance.GetVisibility())
		{
			KAInput.ShowJoystick(UiJoystick.pInstance.pPos, inShow: false);
		}
		if (mPayoutDone && mCanEnableResultUI)
		{
			if (UICursorManager.GetCursorName() != "Arrow")
			{
				KAUICursorManager.SetDefaultCursor("Arrow");
			}
			_ResultUI.SetRewardDisplay(mAchievementRewards);
			mCanEnableResultUI = false;
			if (_UIEventRewardAchievement != null)
			{
				_UIEventRewardAchievement.SetReward(_GameID, RsResourceManager.pCurrentLevel, GetEventRewardType(), base.gameObject);
			}
			else
			{
				AchievementRewardProcessed();
			}
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (!(inWidget == null))
		{
			if (inWidget == mOkBtn)
			{
				SetVisibility(inVisible: false);
				InitResultUI();
				mIsPayoutStuck = false;
				mResultShown = true;
			}
			else if (inWidget == mBtnChallenge)
			{
				SetVisibility(inVisible: false);
				UiChallengeInvite.Show(base.gameObject, "OnChallengeDone");
			}
		}
	}

	private void OnChallengeDone()
	{
		if (UiChallengeInvite.pIsChallengeSent)
		{
			mBtnChallenge.SetDisabled(isDisabled: true);
		}
		SetVisibility(inVisible: true);
	}

	private void AchievementRewardProcessed()
	{
		_UIEventRewardAchievement.SetVisibility(inVisible: false);
		_ResultUI.SetInteractive(interactive: true);
	}

	private EventRewardType GetEventRewardType()
	{
		EventRewardType result = EventRewardType.PlayedLevel;
		if (!RacingManager.pIsSinglePlayer)
		{
			switch (mPlayerPos)
			{
			case 1:
				result = EventRewardType.FirstPlace;
				break;
			case 2:
				result = EventRewardType.SecondPlace;
				break;
			case 3:
				result = EventRewardType.ThirdPlace;
				break;
			}
			UtDebug.Log("GetEventRewardType :: Player Position :: " + mPlayerPos);
		}
		return result;
	}

	private void ShowSinglePlayerResult()
	{
		mRaceEndTime = mLevelManager.pElapsedTime;
		InitResultUI();
	}

	private void ShowMultiPlayerResult(MMOExtensionResponseReceivedEventArgs args)
	{
		int num = 0;
		mMenu.ClearItems();
		int num2 = mLevelManager.pPlayerData.Count;
		for (int i = 3; i < args.ResponseDataObject.Count; i += 3)
		{
			string s = (string)args.ResponseDataObject[(i + 2).ToString()];
			int result = 0;
			if (int.TryParse(s, out result) && result < mLevelManager._NumLaps)
			{
				num2--;
			}
		}
		foreach (PlayerData pPlayerDatum in mLevelManager.pPlayerData)
		{
			if (pPlayerDatum.mAvatarRacing.pLapsCompleted == -2)
			{
				num2--;
			}
		}
		if (args != null)
		{
			for (int j = 3; j < args.ResponseDataObject.Count; j += 3)
			{
				string text = (string)args.ResponseDataObject[j.ToString()];
				PlayerData playerFromName = mLevelManager.GetPlayerFromName(text);
				string s2 = (string)args.ResponseDataObject[(j + 1).ToString()];
				string s3 = (string)args.ResponseDataObject[(j + 2).ToString()];
				int result2 = 0;
				if (int.TryParse(s3, out result2))
				{
					if (playerFromName.mAvatar == null && num2 == 0)
					{
						playerFromName.mResultState = RaceState.FINISHED;
					}
					else if (result2 < mLevelManager._NumLaps)
					{
						playerFromName.mResultState = RaceState.NOT_FINISHED;
						if (text == AvatarData.pInstance.DisplayName && !mApplyPayoutDone)
						{
							mOkBtn.SetInteractive(isInteractive: true);
							KAUICursorManager.SetDefaultCursor("Arrow");
							mApplyPayoutDone = true;
							mPayoutDone = true;
						}
						continue;
					}
				}
				playerFromName.mResultState = RaceState.FINISHED;
				float result3 = 0f;
				float.TryParse(s2, out result3);
				num++;
				AddResultMenuItem(num.ToString(), text, GameUtilities.FormatTime(result3), GetTrophyFromPos(num));
				if (text == AvatarData.pInstance.DisplayName && !mApplyPayoutDone)
				{
					mPlayerPos = num;
					mRaceEndTime = result3;
					_ResultUI.SetAdRewardData(mCurrentModuleName, mPlayerPos);
					WsWebService.ApplyPayout(mCurrentModuleName, mPlayerPos, ServiceEventHandler, null);
					mApplyPayoutDone = true;
					UtDebug.Log("ShowMultiPlayerResult :: Player Position :: " + mPlayerPos);
				}
				if (playerFromName != null && playerFromName.mAvatarRacing != null)
				{
					playerFromName.mAvatarRacing.SetPositionSprite(num);
					if (playerFromName.mAvatarRacing.enabled)
					{
						playerFromName.mAvatarRacing.enabled = false;
					}
				}
			}
		}
		foreach (PlayerData pPlayerDatum2 in mLevelManager.pPlayerData)
		{
			if (args.ResponseDataObject.ContainsValue(pPlayerDatum2.mUserId) || pPlayerDatum2.mResultState != RaceState.NOT_FINISHED)
			{
				continue;
			}
			num++;
			AddResultMenuItem(num.ToString(), pPlayerDatum2.mName, "DNF", GetTrophyFromPos(-1));
			if (pPlayerDatum2.mAvatarRacing != null)
			{
				pPlayerDatum2.mAvatarRacing.SetPositionSprite(num);
				if (pPlayerDatum2.mAvatarRacing.enabled)
				{
					pPlayerDatum2.mAvatarRacing.enabled = false;
				}
			}
		}
	}

	private void AddResultMenuItem(string rank, string name, string time, string trophyCnt)
	{
		KAWidget kAWidget = DuplicateWidget(mMenu._Template);
		if (kAWidget != null)
		{
			kAWidget.name = name;
			kAWidget.FindChildItem("TxtTopRankNum").SetText(rank);
			kAWidget.FindChildItem("TxtTopRankPlayerName").SetText(name);
			kAWidget.FindChildItem("TxtTopRankScore").SetText(time);
			KAWidget kAWidget2 = kAWidget.FindChildItem("TopRankTrophies");
			kAWidget2.SetText(trophyCnt);
			kAWidget2.SetVisibility(inVisible: true);
			kAWidget.SetVisibility(inVisible: true);
			mMenu.AddWidget(kAWidget);
		}
	}

	private void ShowRewardScreen()
	{
		HighScores.SetCurrentGameSettings(mLevelManager._GameModuleName, _GameID, !RacingManager.pIsSinglePlayer, 0, mLevelManager._TrackID);
		AvatarRacing component = AvAvatar.pObject.GetComponent<AvatarRacing>();
		int num = (int)(mRaceEndTime * 100f);
		if (component.pLapsCompleted >= mLevelManager._NumLaps)
		{
			HighScores.AddGameData("time", num.ToString());
		}
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.CheckForTaskCompletion("Game", mLevelManager._GameModuleName);
		}
		if (_ResultUI != null)
		{
			string inResult = "any";
			if (!RacingManager.pIsSinglePlayer)
			{
				inResult = ((mPlayerPos != 1) ? "lose" : "win");
			}
			_ResultUI.SetHighScoreData(num, "time", inAscendingOrder: true);
			_ResultUI.BroadcastMessage("UpdateRaceCompleteStatus", component.pLapsCompleted >= mLevelManager._NumLaps);
			_ResultUI.SetGameSettings(mCurrentModuleName, base.gameObject, inResult);
			if (!RacingManager.pIsSinglePlayer)
			{
				_ResultUI.SetResultData("Data0", null, mPlayerPos.ToString());
			}
			if (component != null)
			{
				_ResultUI.SetResultData("Data1", null, GameUtilities.FormatTime(component.pBestLapTime));
			}
			_ResultUI.SetResultData("Data2", null, GameUtilities.FormatTime((float)num / 100f));
			_ResultUI.SetRewardDisplay(mAchievementRewards);
			_ResultUI.SetVisibility(Visibility: true);
			if (!string.IsNullOrEmpty(mTxtChallengeResult))
			{
				_ResultUI.SetRewardMessage(mTxtChallengeResult);
			}
		}
		else
		{
			Debug.LogError("NO END RESULT COMPONENT FOUND!!!");
		}
	}

	public void SetXPAndCoins(int position, LevelManager levelManager)
	{
		mApplyPayoutDone = false;
		mPlayerPos = (RacingManager.pIsSinglePlayer ? position : (position - 1));
		mCurrentModuleName = (RacingManager.pIsSinglePlayer ? levelManager._SinglePlayerGameModuleName : levelManager._GameModuleName);
		if (RacingManager.pIsSinglePlayer || !MainStreetMMOClient.pIsMMOEnabled)
		{
			_ResultUI.SetAdRewardData(mCurrentModuleName, mPlayerPos);
			WsWebService.ApplyPayout(mCurrentModuleName, mPlayerPos, ServiceEventHandler, null);
		}
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.APPLY_PAYOUT:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				if (mIsPayoutStuck)
				{
					if (mKAUIGenericDB != null)
					{
						Object.Destroy(mKAUIGenericDB.gameObject);
						mKAUIGenericDB = null;
					}
					mIsPayoutStuck = false;
				}
				mAchievementRewards = (AchievementReward[])inObject;
				if (mAchievementRewards != null)
				{
					AchievementReward[] array = mAchievementRewards;
					foreach (AchievementReward achievementReward in array)
					{
						switch (achievementReward.PointTypeID)
						{
						case 2:
							mRewardCoins = achievementReward.Amount.Value;
							Money.AddToGameCurrency(mRewardCoins);
							break;
						case 8:
							mDragonPoints = achievementReward.Amount.Value;
							SanctuaryManager.pInstance.AddXP(mDragonPoints);
							break;
						case 1:
							mRewardPoints = achievementReward.Amount.Value;
							UserRankData.AddPoints(mRewardPoints);
							break;
						case 11:
							mRewardTrophies = achievementReward.Amount.Value;
							UserRankData.AddPoints(11, mRewardTrophies);
							break;
						}
					}
				}
				else
				{
					UtDebug.Log("reward data is null!!!");
				}
				mOkBtn.SetInteractive(isInteractive: true);
				KAUICursorManager.SetDefaultCursor("Arrow");
				mPayoutDone = true;
				break;
			case WsServiceEvent.ERROR:
				UtDebug.Log("Error fetching reward Data!!!");
				if (!mIsPayoutStuck)
				{
					mOkBtn.SetInteractive(isInteractive: true);
					KAUICursorManager.SetDefaultCursor("Arrow");
					mPayoutDone = true;
				}
				break;
			case WsServiceEvent.PROGRESS:
				if (!mIsPayoutStuck)
				{
					if (mPayoutCallTimer >= _PayoutCallWaitTime && mKAUIGenericDB == null)
					{
						KAUICursorManager.SetDefaultCursor("Arrow");
						mPayoutCallTimer = 0f;
						mIsPayoutStuck = true;
						mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDBSm", "PayoutFail");
						mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
						mKAUIGenericDB.SetTextByID(mLevelManager._PayoutFailText._ID, mLevelManager._PayoutFailText._Text, interactive: false);
						mKAUIGenericDB._OKMessage = "ClickedOk";
						mKAUIGenericDB.SetDestroyOnClick(isDestroy: true);
						KAUI.SetExclusive(mKAUIGenericDB);
					}
					else
					{
						mPayoutCallTimer += Time.deltaTime;
					}
				}
				break;
			}
			break;
		case WsServiceType.GET_DISPLAYNAME_BY_USER_ID:
			if (inEvent == WsServiceEvent.COMPLETE && inObject != null && !string.IsNullOrEmpty((string)inObject))
			{
				string localizedString = _ChallengeCompleteText.GetLocalizedString();
				localizedString = localizedString.Replace("[Name]", (string)inObject);
				mTxtChallengeResult = localizedString;
				if (!string.IsNullOrEmpty(mTxtChallengeResult) && _ResultUI != null)
				{
					_ResultUI.SetRewardMessage(mTxtChallengeResult);
				}
			}
			break;
		}
	}

	private void ClickedOk()
	{
		SetInteractive(interactive: true);
		mOkBtn.SetInteractive(isInteractive: true);
		mKAUIGenericDB = null;
		mPayoutDone = true;
	}

	private void UpdateTrophyCount(PlayerData inData)
	{
		KAWidget kAWidget = ((inData == null) ? mMenu.FindItem(AvatarData.pInstance.DisplayName) : mMenu.FindItem(inData.mName));
		if (!(kAWidget != null))
		{
			return;
		}
		KAWidget kAWidget2 = kAWidget.FindChildItem("TopRankTrophies");
		if (kAWidget2 != null)
		{
			if (inData != null)
			{
				kAWidget2.SetText(inData.mTrophyCount.ToString());
			}
			else
			{
				UserAchievementInfo userAchievementInfoByType = UserRankData.GetUserAchievementInfoByType(11);
				kAWidget2.SetText((userAchievementInfoByType != null && userAchievementInfoByType.AchievementPointTotal.HasValue) ? userAchievementInfoByType.AchievementPointTotal.Value.ToString() : "0");
			}
			kAWidget2.SetVisibility(inVisible: true);
		}
	}

	private string GetTrophyFromPos(int inPos)
	{
		EndGameAchievements[] endGameAchievements = _EndGameAchievements;
		foreach (EndGameAchievements endGameAchievements2 in endGameAchievements)
		{
			if (endGameAchievements2._Pos == inPos)
			{
				return endGameAchievements2._TrophyCount;
			}
		}
		return string.Empty;
	}

	private void InitResultUI()
	{
		SetVisibility(inVisible: false);
		mMenu.SetVisibility(inVisible: false);
		_ResultUI.SetInteractive(interactive: false);
		ShowRewardScreen();
		mCanEnableResultUI = true;
		if (UICursorManager.GetCursorName() != "Loading")
		{
			KAUICursorManager.SetDefaultCursor("Loading");
		}
		_ResultUI.SetTitle(mLevelManager._TrackName);
	}

	public void OnStoreOpened()
	{
		if (!UtMobileUtilities.CanLoadInCurrentScene(UiType.Store, UILoadOptions.AUTO))
		{
			mLevelManager.DestroyRacingComp();
			mLevelManager.EnableDragonInput(inEnable: true);
			RacingManager.ProcessExitRacing();
			KAUIStore._ExitLevelName = RsResourceManager.pLastLevel;
		}
	}

	private void OnEndDBClose()
	{
		mLevelManager.DestroyRacingComp();
		if (!mIsReplayGame)
		{
			mLevelManager.EnableDragonInput(inEnable: true);
			RacingManager.Instance.ExitRacing();
		}
		else
		{
			PathManager.Destroy();
			RsResourceManager.LoadLevel(RsResourceManager.pCurrentLevel);
		}
	}

	private void OnReplayGame()
	{
		if (SanctuaryManager.pCurPetInstance.IsActionAllowed(PetActions.RACING))
		{
			mIsReplayGame = true;
			OnEndDBClose();
		}
		else
		{
			UiPetEnergyGenericDB.Show(base.gameObject, "DestroyDB", "DestroyDB", isLowEnergy: true);
		}
	}

	private void DestroyDB()
	{
		mLevelManager.DestroyKAUIDB();
		_ResultUI.SetVisibility(Visibility: true);
	}
}
