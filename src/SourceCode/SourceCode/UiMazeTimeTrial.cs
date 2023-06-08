using System;
using System.Collections.Generic;
using UnityEngine;

public class UiMazeTimeTrial : KAUI
{
	public LocaleString _SelectTTModeText = new LocaleString("Select a mode:");

	public LocaleString _TimedChallengeText = new LocaleString("Timed Run");

	public LocaleString _NormalChallengeText = new LocaleString("Normal Play");

	public LocaleString _TotalTimeText = new LocaleString("Total Time: ");

	public LocaleString _PersonalBestTimeText = new LocaleString("Personal Best: ");

	public LocaleString _ConfirmQuitChallengeText = new LocaleString("This challenge will be lost if you exit. Are you sure you want to exit?");

	public MazeTimeTrialManager _GameManager;

	public UiTimeTrialHUD _KAUIHud;

	public UiToolbar _KAUIToolbar;

	public UiDragonsEndDB _KAUIEndDB;

	public UITimeTrialChallengeResults _ChallengeResults;

	private float mGameStartTime;

	private float mFinalTime;

	private ChallengeInfo mActiveChallenge;

	private LocaleString mCachedWarningDBText;

	protected override void Start()
	{
		base.Start();
		if (_GameManager == null)
		{
			Debug.LogError("Unable to find mGameManager");
		}
		else
		{
			_GameManager._UiEventRewardDB.pCurrentGameID = _GameManager._GameID;
		}
	}

	public void UpdateTime()
	{
		if ((bool)_KAUIHud)
		{
			_GameManager.pTime = Time.time - mGameStartTime;
			_KAUIHud.UpdateTimeDisplay(_GameManager.pTime);
		}
	}

	public void Init(MazeTimeTrialManager inManager)
	{
		_GameManager = inManager;
		SetVisibility(inVisible: true);
	}

	public void InitChallenge()
	{
		mActiveChallenge = ChallengeInfo.pActiveChallenge;
		if (mActiveChallenge == null)
		{
			return;
		}
		if (mActiveChallenge.ChallengeGameInfo.GameID == _GameManager._GameID && mActiveChallenge.ChallengeGameInfo.GameLevelID == _GameManager._LevelID)
		{
			SetVisibility(inVisible: false);
			if ((bool)_GameManager._QuestObject)
			{
				_GameManager._QuestObject.SetActive(value: false);
			}
			if ((bool)_GameManager._FinishObject)
			{
				_GameManager._FinishObject.SetActive(value: true);
			}
			_GameManager._TimeTrialEndMarker.SetMessageObject(_GameManager.gameObject);
			_GameManager._UiCountdown.StartCountDown(inStart: true);
			_GameManager._UiCountdown.OnCountdownDone += StartTimeTrial;
			if ((bool)_KAUIToolbar)
			{
				mCachedWarningDBText = _KAUIToolbar._WarningDBText;
				_KAUIToolbar._WarningDBText = _ConfirmQuitChallengeText;
			}
		}
		RsResourceManager.LoadLevelStarted += LoseChallenge;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		switch (inWidget.name)
		{
		case "BtnDWDragonsClose":
			SetVisibility(inVisible: false);
			ExitGame();
			break;
		case "BtnDWDragonsTimedChallenge":
			SetVisibility(inVisible: false);
			if ((bool)_GameManager._QuestObject)
			{
				_GameManager._QuestObject.SetActive(value: false);
			}
			if ((bool)_GameManager._FinishObject)
			{
				_GameManager._FinishObject.SetActive(value: true);
			}
			_GameManager._TimeTrialEndMarker.SetMessageObject(_GameManager.gameObject);
			_GameManager._UiCountdown.StartCountDown(inStart: true);
			_GameManager._UiCountdown.OnCountdownDone += StartTimeTrial;
			break;
		case "BtnDWDragonsNormalChallenge":
			SetVisibility(inVisible: false);
			if ((bool)_GameManager._QuestObject)
			{
				_GameManager._QuestObject.SetActive(value: true);
			}
			if ((bool)_GameManager._FinishObject)
			{
				_GameManager._FinishObject.SetActive(value: false);
			}
			AvAvatar.SetUIActive(inActive: true);
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.pLevelState = AvAvatarLevelState.NORMAL;
			AvAvatar.pSubState = AvAvatarSubState.NORMAL;
			break;
		}
	}

	private void StartTimeTrial()
	{
		_GameManager._UiCountdown.OnCountdownDone -= StartTimeTrial;
		_GameManager.pTimeTrialActive = true;
		mGameStartTime = Time.time;
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.pLevelState = AvAvatarLevelState.NORMAL;
		AvAvatar.pSubState = AvAvatarSubState.NORMAL;
		SetUIActive(active: false);
		if ((bool)_KAUIHud)
		{
			_KAUIHud.SetVisibility(inVisible: true);
			if (mActiveChallenge != null)
			{
				if (mActiveChallenge.ChallengeGameInfo.GameID == _GameManager._GameID && mActiveChallenge.ChallengeGameInfo.GameLevelID == _GameManager._LevelID)
				{
					_KAUIHud.StartTimer(showChallengeTime: true);
					_KAUIHud.UpdateChallengeTimeDisplay(mActiveChallenge.Points);
				}
			}
			else
			{
				_KAUIHud.StartTimer(showChallengeTime: false);
				WsWebService.GetGameDataByUser(UserInfo.pInstance.UserID, _GameManager._GameID, isMultiplayer: false, 0, _GameManager._LevelID, "time", 1, ascendingOrder: true, ServiceEventHandler, null);
			}
		}
		else
		{
			Debug.LogError("Unable to find UiTimeTrialHUD");
		}
	}

	private void SetUIActive(bool active)
	{
		AvAvatar.SetUIActive(!active);
		UiToolbar component = AvAvatar.pToolbar.GetComponent<UiToolbar>();
		if (!component)
		{
			return;
		}
		component.SetButtonsVisiblity(active);
		try
		{
			component.FindItem("BackBtn").SetInteractive(isInteractive: true);
			component.FindItem("BtnOpenMap").SetVisibility(active);
			component.FindItem("BtnOpenQuestBranches").SetVisibility(active);
			component._UiAvatarCSM.gameObject.SetActive(active);
			UiAvatarControls.pInstance.FindItem("DragonFire").SetVisibility(inVisible: false);
			UiAvatarControls.pInstance.FindItem("DragonMount").SetVisibility(inVisible: false);
			component._UiChatHistory.gameObject.SetActive(active);
			if (component._UiAvatarCSM.gameObject.activeSelf)
			{
				component.FindItem("CoinTotalBtn").SetInteractive(isInteractive: true);
				component.FindItem("TxtCoinAmount").SetVisibility(inVisible: true);
				component.FindItem("StoreBtn").SetInteractive(isInteractive: true);
			}
		}
		catch (Exception message)
		{
			UtDebug.LogError(message);
		}
	}

	private void OnEndDBClose()
	{
		CheckMissionActive();
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.pLevelState = AvAvatarLevelState.NORMAL;
		AvAvatar.pSubState = AvAvatarSubState.NORMAL;
	}

	private void CheckMissionActive()
	{
		Mission mission = MissionManager.pInstance.GetMission(_GameManager._FirstTimeMissionID);
		if (mission != null && !mission.pCompleted)
		{
			List<Task> tasks = new List<Task>();
			MissionManager.pInstance.GetNextTask(mission, ref tasks);
			if (tasks.Count > 0)
			{
				tasks[0].Completed++;
			}
		}
		else
		{
			_GameManager._UiEventRewardDB.gameObject.SetActive(value: true);
		}
	}

	public void EndTimeTrial()
	{
		if (_GameManager.pTimeTrialActive)
		{
			_GameManager.pTimeTrialActive = false;
			_KAUIHud.StopTimer();
			_KAUIHud.SetVisibility(inVisible: false);
			AvAvatar.pState = AvAvatarState.PAUSED;
			SetUIActive(active: true);
			if ((bool)_KAUIEndDB)
			{
				SetHighScoreData();
			}
			_GameManager._FinishObject.SetActive(value: false);
		}
	}

	private void SetHighScoreData()
	{
		_KAUIEndDB.Initialize();
		_KAUIEndDB.SetVisibility(Visibility: true);
		HighScores.SetCurrentGameSettings(_GameManager._GameModuleName, _GameManager._GameID, isMultiPlayer: false, 0, _GameManager._LevelID);
		mFinalTime = (float)(Math.Truncate((double)_GameManager.pTime * 100.0) / 100.0);
		int num = Mathf.FloorToInt(mFinalTime * 100f);
		HighScores.AddGameData("time", num.ToString());
		_KAUIEndDB.SetHighScoreData(num, "time", inAscendingOrder: true);
		_KAUIEndDB.SetGameSettings(_GameManager._GameModuleName, base.gameObject, "any");
		WsWebService.GetGameDataByUser(UserInfo.pInstance.UserID, _GameManager._GameID, isMultiplayer: false, 0, _GameManager._LevelID, "time", 1, ascendingOrder: true, ServiceEventHandler, null);
		string inGrade = _GameManager.CalculateGrade();
		_KAUIEndDB.SetResultData("TotalTime", _TotalTimeText._Text, GameUtilities.FormatTime(mFinalTime));
		_KAUIEndDB.SetGrade(inGrade, _GameManager.GradeColor(inGrade));
		WsWebService.ApplyPayout((_GameManager._MembersHaveDifferentPayout && SubscriptionInfo.pIsMember) ? (_GameManager._GameModuleName + "Member") : _GameManager._GameModuleName, _GameManager.GradeToPayout(inGrade), ServiceEventHandler, null);
		SetupChallenge(num / 100);
	}

	private void SetupChallenge(int score)
	{
		ChallengeInfo pActiveChallenge = ChallengeInfo.pActiveChallenge;
		if (pActiveChallenge != null)
		{
			ChallengeInfo.CheckForChallengeCompletion(_GameManager._GameID, _GameManager._LevelID, 0, score, isTimerUsedAsPoints: true);
			DisplayChallengeResults(score, pActiveChallenge);
			if ((bool)_KAUIToolbar)
			{
				_KAUIToolbar._WarningDBText = mCachedWarningDBText;
			}
		}
		ChallengeInfo.pActiveChallenge = null;
		UiChallengeInvite.SetData(_GameManager._GameID, _GameManager._LevelID, 0, score);
	}

	private void DisplayChallengeResults(int score, ChallengeInfo currentChallenge)
	{
		_KAUIEndDB.SetVisibility(Visibility: false);
		_ChallengeResults.gameObject.SetActive(value: true);
		_ChallengeResults.OnResultsClosed += OnCloseChallengeResults;
		_ChallengeResults._PlayerTime.SetText(GameUtilities.FormatTime(mFinalTime));
		_ChallengeResults._OpponentTime.SetText(GameUtilities.FormatTime(mActiveChallenge.Points));
		bool flag = false;
		if (currentChallenge != null)
		{
			flag = currentChallenge.Points > score;
		}
		_ChallengeResults._TitleText.SetText(flag ? _ChallengeResults._WinTitleText._Text : _ChallengeResults._LoseTitleText._Text);
		_ChallengeResults._MessgeText.SetText(flag ? _ChallengeResults._WinMessgeText._Text : _ChallengeResults._LoseMessageText._Text);
		_ChallengeResults._BannerBkg.ColorBlendTo(Color.white, flag ? _ChallengeResults._WinBannerColor : _ChallengeResults._LoseBannerColor, 0f);
		_ChallengeResults._TrophiesResults.SetText(flag ? "+5" : "-5");
		UserAchievementInfo userAchievementInfoByType = UserRankData.GetUserAchievementInfoByType(11);
		int points = (flag ? 5 : ((userAchievementInfoByType != null) ? (-5) : 0));
		if (!flag && userAchievementInfoByType != null && userAchievementInfoByType.AchievementPointTotal.Value < 5)
		{
			points = -userAchievementInfoByType.AchievementPointTotal.Value;
		}
		UserRankData.AddPoints(11, points);
		_ChallengeResults.FindItem("TxtTrophiesAmt").SetText((userAchievementInfoByType != null) ? userAchievementInfoByType.AchievementPointTotal.Value.ToString() : points.ToString());
	}

	private void OnCloseChallengeResults()
	{
		_ChallengeResults.OnResultsClosed -= OnCloseChallengeResults;
		_KAUIEndDB.SetVisibility(Visibility: true);
		_ChallengeResults.gameObject.SetActive(value: false);
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.APPLY_PAYOUT:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				AchievementReward[] array = null;
				UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
				if (inObject != null)
				{
					array = (AchievementReward[])inObject;
					if (array != null)
					{
						GameUtilities.AddRewards(array, inUseRewardManager: false, inImmediateShow: false);
					}
				}
				if (_KAUIEndDB != null)
				{
					_KAUIEndDB.SetRewardDisplay(array);
				}
				break;
			}
			case WsServiceEvent.ERROR:
				UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
				UtDebug.Log("Reward data is null!");
				break;
			}
			break;
		case WsServiceType.GET_GAME_DATA_USER:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				GameDataSummary gameDataSummary = (GameDataSummary)inObject;
				if (gameDataSummary != null)
				{
					_KAUIEndDB.SetResultData("PersonalBest", _PersonalBestTimeText._Text, (gameDataSummary.GameDataList == null) ? GameUtilities.FormatTime(mFinalTime) : GameUtilities.FormatTime((float)gameDataSummary.GameDataList[0].Value / 100f));
					if ((bool)_KAUIHud && gameDataSummary.GameDataList != null)
					{
						_KAUIHud.FindItem("ChallengeInfo").SetVisibility(inVisible: true);
						_KAUIHud.UpdateChallengeTimeDisplay((float)gameDataSummary.GameDataList[0].Value / 100f, "Personal Best");
					}
				}
				break;
			}
			case WsServiceEvent.PROGRESS:
			case WsServiceEvent.ERROR:
				break;
			}
			break;
		}
	}

	private void LoseChallenge(string level)
	{
		mActiveChallenge = ChallengeInfo.pActiveChallenge;
		RsResourceManager.LoadLevelStarted -= LoseChallenge;
		if (mActiveChallenge != null)
		{
			ChallengeInfo.CheckForChallengeCompletion(_GameManager._GameID, _GameManager._LevelID, 0, mActiveChallenge.Points + 1, isTimerUsedAsPoints: true);
		}
	}

	private void ExitGame()
	{
		_GameManager.pTimeTrialActive = false;
		AvAvatar.pLevelState = AvAvatarLevelState.NORMAL;
		AvAvatar.pSubState = AvAvatarSubState.NORMAL;
		if (_GameManager != null)
		{
			UtUtilities.LoadLevel(_GameManager._ExitLevel);
		}
		else
		{
			UtDebug.LogError("GAME MANAGER NULL!!!");
		}
	}
}
