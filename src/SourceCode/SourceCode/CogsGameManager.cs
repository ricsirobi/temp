using System;
using System.Collections.Generic;
using UnityEngine;

public class CogsGameManager : KAMonoBase
{
	private enum SAVESTEP
	{
		NONE,
		PAIRDATA_START,
		PAIRDATA_END,
		ACHIEVEMENT_START,
		ACHIEVEMENT_END,
		PAYOUT_START,
		PAYOUT_END
	}

	public UiCogs _MainUI;

	public UiCogsHelp _CogsHelpUI;

	public float _AdjustCogOffset = 0.2f;

	public float _CollisionCheckOffset = 0.01f;

	public List<CogObject> _StartCogs = new List<CogObject>();

	public List<CogObject> _CogsContainer = new List<CogObject>();

	public UiCogsGameEndDB _GameEndDBUI;

	public ParticleSystem _ResetParticle;

	public ParticleSystem _ExplodeParticle;

	public string _GameModuleName = "DOCogs";

	public string _CompletedGameModuleName = "DOCogsCompleted";

	public ObClickable _StartLeverClickable;

	public string _StartLeverAnimationName = "LeverActive";

	public string _TutorialKeyName = "CogsTutorial";

	public AudioClip _CogTurningSFX;

	public AudioClip _CogBrokenSFX;

	public AudioClip _CogSnapSFX;

	public AudioClip _ResetSFX;

	public AudioClip _GameCompletedSFX;

	public AudioClip _CogStartSFX;

	public AudioClip _BoardFlipSFX;

	public AudioClip _BgMusic;

	public string _AmbMusicPool = "AmbMusic_Pool";

	public int _GameID = 57;

	public List<CogObject> _VictoryCogs = new List<CogObject>();

	public GameObject _GameBoard;

	public float _TimeToCheckForWin = 2f;

	public float _DelayTimeToStart = 1f;

	public int _ScoreMovesFactor = 50;

	public int _ScoreTimeFactor = 50;

	public int _ScoreSumFactor = 80;

	public float _WinFlashDuration = 4f;

	public float _WinFlashRate = 0.3f;

	public int _StarsAchievementTaskID;

	public LocaleString _ServerErrorTitleText = new LocaleString("An Error Occurred");

	public LocaleString _ServerErrorBodyText = new LocaleString("Something went wrong while attempting to save your progress to the server.\n\nError code: {0}\n\nWould you like to try saving again?");

	private SAVESTEP mSaveStep;

	private float mTimeCheckForWin;

	private bool mIsPlaying;

	private bool mGameCompleted;

	private CogObject mLastAlignedCog;

	private bool mStartLeverPressed;

	private float mStartLeverTimer;

	private bool mShowTutorial;

	private bool mCheckForGameComplete = true;

	private List<CogObject> mCogValidationList = new List<CogObject>();

	private int mPreviousStarsCollected;

	private int mLevelStars = -1;

	private string mModuleName;

	private AchievementReward[] mAchievementRewards;

	[NonSerialized]
	public List<CogObject> _DefectiveMachines = new List<CogObject>();

	private static CogsGameManager mInstance;

	public float pLapsedTime { get; set; }

	public float pGoalTime { get; set; }

	public bool pIsTimerStarted { get; set; }

	public int pCurrentMoves { get; set; }

	public int pGoalMoves { get; set; }

	public bool pIsResetRequired { get; set; }

	public bool pIsGameCompleted => mGameCompleted;

	public bool pIsInitialized { get; set; }

	public bool pIsPlaying => !_StartLeverClickable._Active;

	public static CogsGameManager pInstance => mInstance;

	private void Awake()
	{
		mInstance = this;
		RsResourceManager.DestroyLoadScreen();
		SnChannel.PausePool(_AmbMusicPool);
		SnChannel.Play(_BgMusic, "Music_Pool", inForce: true);
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: false);
		}
		if (ProductData.pIsReady && !ProductData.TutorialComplete(_TutorialKeyName))
		{
			mShowTutorial = true;
			ProductData.AddTutorial(_TutorialKeyName);
		}
	}

	private void Update()
	{
		if (!pIsInitialized && CogsLevelManager.pInstance.pIsReady)
		{
			pIsInitialized = true;
			pIsResetRequired = false;
			for (int i = 0; i < _StartCogs.Count; i++)
			{
				_StartCogs[i].pGear.Reset();
				_StartCogs[i].pGear.machine.ResetMachine();
			}
			_StartLeverClickable._Active = true;
			mStartLeverPressed = false;
			mGameCompleted = false;
			mCheckForGameComplete = true;
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.SetTimedTaskUpdate(inState: true, inForceUpdate: true);
			}
			if (mShowTutorial)
			{
				ShowHelpScreen();
				mShowTutorial = false;
			}
			pGoalMoves = CogsLevelManager.pInstance.pCurrentLevelData.GoalMoves;
			pGoalTime = CogsLevelManager.pInstance.pCurrentLevelData.GoalTime;
			mTimeCheckForWin = _TimeToCheckForWin;
			ResetCounters();
			_MainUI.ResetUI();
		}
		if (pIsInitialized && pIsTimerStarted && !mIsPlaying && !mGameCompleted)
		{
			pLapsedTime += Time.deltaTime;
		}
		if (mStartLeverPressed)
		{
			mStartLeverTimer -= Time.deltaTime;
			if (mStartLeverTimer < 0f)
			{
				mStartLeverPressed = false;
				SetMachines(inActive: true);
			}
		}
		if (!mIsPlaying || (mGameCompleted && mCheckForGameComplete) || _DefectiveMachines.Count != 0)
		{
			return;
		}
		if (mCheckForGameComplete)
		{
			mCheckForGameComplete = false;
			mGameCompleted = true;
			for (int j = 0; j < _VictoryCogs.Count; j++)
			{
				if (_VictoryCogs[j].pParent == null || _VictoryCogs[j].pIsMachineDefective)
				{
					mGameCompleted = false;
				}
			}
			if (mGameCompleted)
			{
				SnChannel.Play(_GameCompletedSFX);
				_MainUI.FlashMoveCount(_WinFlashRate, _WinFlashDuration);
				for (int k = 0; k < _CogsContainer.Count; k++)
				{
					_CogsContainer[k].SetMachineParticles(inPlay: true);
				}
			}
		}
		mTimeCheckForWin -= Time.deltaTime;
		if (!(mTimeCheckForWin < 0f))
		{
			return;
		}
		SnChannel.AcquireChannel("SFX_Pool", inForce: true).pLoop = false;
		SnChannel.StopPool("SFX_Pool");
		mTimeCheckForWin = _TimeToCheckForWin;
		if (mGameCompleted)
		{
			MissionManager.pInstance?.CheckForTaskCompletion("Game", _GameModuleName, CogsLevelManager.pInstance.pCurrentLevelData.LevelName);
			if (string.IsNullOrEmpty(CogsLevelManager.pLevelToLoad))
			{
				_GameEndDBUI.SetVisibility(Visibility: false);
				_GameEndDBUI.SetInteractive(interactive: false);
				KAUICursorManager.SetDefaultCursor("Loading");
				mPreviousStarsCollected = CogsLevelProgress.pInstance.GetStarsCollected(CogsLevelManager.pInstance.pCurrentLevelData.LevelName);
				mLevelStars = CalculateLevelStars();
				StartPairDataSave();
			}
			else
			{
				CogsLevelManager.pInstance.QuitGame();
			}
			UtDebug.Log("Level Completed...");
		}
		else
		{
			_StartLeverClickable._Active = true;
			mCheckForGameComplete = true;
			UtDebug.LogError("Level Not Cleared");
		}
		SetMachines(inActive: false);
	}

	private void StartPairDataSave()
	{
		mSaveStep = SAVESTEP.PAIRDATA_START;
		if (CogsLevelProgress.pInstance == null)
		{
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", string.Format(_ServerErrorBodyText.GetLocalizedString(), (int)mSaveStep), _ServerErrorTitleText.GetLocalizedString(), base.gameObject, null, null, "OnClickSaveAttemptCancel", null, inDestroyOnClick: true);
			int num = (int)mSaveStep;
			UtDebug.LogError("Cogs failed to save at step " + num);
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
		else
		{
			CogsLevelProgress.OnPairDataSaved = (Action<bool, PairData, object>)Delegate.Combine(CogsLevelProgress.OnPairDataSaved, new Action<bool, PairData, object>(OnPairDataSaved));
			CogsLevelProgress.pInstance.Save(mLevelStars);
		}
	}

	private void OnPairDataSaved(bool success, PairData pData, object inUserData)
	{
		mSaveStep = SAVESTEP.PAIRDATA_END;
		CogsLevelProgress.OnPairDataSaved = (Action<bool, PairData, object>)Delegate.Remove(CogsLevelProgress.OnPairDataSaved, new Action<bool, PairData, object>(OnPairDataSaved));
		if (success)
		{
			StartAchievementTaskSave();
			return;
		}
		int num = (int)mSaveStep;
		UtDebug.LogError("Cogs failed to save at step " + num);
		ShowPairDataSaveRetryPrompt();
	}

	private void ShowPairDataSaveRetryPrompt()
	{
		GameUtilities.DisplayGenericDB("PfKAUIGenericDB", string.Format(_ServerErrorBodyText.GetLocalizedString(), (int)mSaveStep), _ServerErrorTitleText.GetLocalizedString(), base.gameObject, "OnClickPairDataSaveRetry", "OnClickSaveAttemptCancel", "", "", inDestroyOnClick: true);
		KAUICursorManager.SetDefaultCursor("Arrow");
	}

	private void OnClickPairDataSaveRetry()
	{
		StartPairDataSave();
	}

	private void OnClickSaveAttemptCancel()
	{
		mSaveStep = SAVESTEP.NONE;
		CogsLevelManager.pInstance.QuitGame();
	}

	private void StartAchievementTaskSave()
	{
		if (UserInfo.pIsReady && mLevelStars == 3 && CogsLevelProgress.pInstance != null && mPreviousStarsCollected < mLevelStars)
		{
			UserAchievementTask.Set(new AchievementTask[1]
			{
				new AchievementTask(_StarsAchievementTaskID, CogsLevelManager.pInstance.pCurrentLevelData.LevelName)
			}, displayRewards: false, OnAchievementTaskSet, null);
		}
		else
		{
			SetPayout();
		}
	}

	private void OnAchievementTaskSet(WsServiceEvent inEvent, int achievementTypeID, bool success, object inUserData)
	{
		mSaveStep = SAVESTEP.ACHIEVEMENT_END;
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			SetPayout();
			if (!success)
			{
				UtDebug.Log("Achievement already earned for level " + CogsLevelManager.pInstance.pCurrentLevelIndex);
			}
			break;
		case WsServiceEvent.ERROR:
		{
			int num = (int)mSaveStep;
			UtDebug.LogError("Cogs failed to save at step " + num);
			ShowAchievementSaveRetryPrompt();
			break;
		}
		}
	}

	private void SetPayout()
	{
		mSaveStep = SAVESTEP.PAYOUT_START;
		mModuleName = _GameModuleName;
		if (mLevelStars <= mPreviousStarsCollected)
		{
			mModuleName = _CompletedGameModuleName;
		}
		if (SubscriptionInfo.pIsMember)
		{
			mModuleName += "Member";
		}
		WsWebService.ApplyPayout(mModuleName, mLevelStars, ServiceEventHandler, null);
	}

	private void ShowAchievementSaveRetryPrompt()
	{
		GameUtilities.DisplayGenericDB("PfKAUIGenericDB", string.Format(_ServerErrorBodyText.GetLocalizedString(), (int)mSaveStep), _ServerErrorTitleText.GetLocalizedString(), base.gameObject, "OnClickRetryAchievementSet", "OnClickSaveAttemptCancel", "", "", inDestroyOnClick: true);
		KAUICursorManager.SetDefaultCursor("Arrow");
	}

	private void OnClickRetryAchievementSet()
	{
		StartAchievementTaskSave();
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType != WsServiceType.APPLY_PAYOUT)
		{
			return;
		}
		mSaveStep = SAVESTEP.PAYOUT_END;
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			if (inObject != null)
			{
				mAchievementRewards = (AchievementReward[])inObject;
				GameUtilities.AddRewards(mAchievementRewards, inUseRewardManager: false, inImmediateShow: false);
				ShowGameEndDBUI();
			}
			break;
		case WsServiceEvent.ERROR:
		{
			int num = (int)mSaveStep;
			UtDebug.LogError("Cogs failed to save at step " + num);
			ShowPayoutSaveRetryPrompt();
			break;
		}
		}
	}

	private void ShowPayoutSaveRetryPrompt()
	{
		GameUtilities.DisplayGenericDB("PfKAUIGenericDB", string.Format(_ServerErrorBodyText.GetLocalizedString(), (int)mSaveStep), _ServerErrorTitleText.GetLocalizedString(), base.gameObject, "OnClickRetryPayout", "OnClickSaveAttemptCancel", "", "", inDestroyOnClick: true);
		KAUICursorManager.SetDefaultCursor("Arrow");
	}

	public void OnClickRetryPayout()
	{
		WsWebService.ApplyPayout(mModuleName, mLevelStars, ServiceEventHandler, null);
		KAUICursorManager.SetDefaultCursor("Loading");
	}

	private void ShowGameEndDBUI()
	{
		if (CogsLevelManager.pInstance.pCurrentLevelData == null)
		{
			UtDebug.Log("Test Level Completed... ");
			return;
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
		if ((bool)_GameEndDBUI)
		{
			KAUI.SetExclusive(_GameEndDBUI);
			_GameEndDBUI.SetGameSettings(_GameModuleName, base.gameObject, "any");
			_GameEndDBUI.SetAdRewardData(mModuleName, mLevelStars);
			_GameEndDBUI.SetHighScoreData(mLevelStars, "highscore");
			_GameEndDBUI.SetResultData(pCurrentMoves, pLapsedTime, mLevelStars, mLevelStars);
			_GameEndDBUI.SetRewardDisplay(mAchievementRewards);
			_GameEndDBUI.SetVisibility(Visibility: true);
			_GameEndDBUI.SetInteractive(interactive: true);
		}
		mModuleName = null;
		mLevelStars = -1;
		mAchievementRewards = null;
		mSaveStep = SAVESTEP.NONE;
	}

	public void OnMainMenu()
	{
		ResetGameplay();
		DestroyCogs();
		MissionManager.pInstance?.SetTimedTaskUpdate(inState: false);
		_MainUI.SetVisibility(inVisible: false);
		CogsLevelManager.pInstance.ResetLevelData();
		CogsLevelManager.pInstance.RotateMenu("OnLevelSelectionMenu");
	}

	public void ShowHelpScreen()
	{
		KAUI.SetExclusive(_CogsHelpUI);
		_CogsHelpUI.SetVisibility(inVisible: true);
	}

	private void OnClick(GameObject inGameObject)
	{
		mStartLeverPressed = true;
		_StartLeverClickable.animation.Play(_StartLeverAnimationName);
		mStartLeverTimer = _DelayTimeToStart;
		_MainUI.SetHelperButtons(inActive: false);
		_StartLeverClickable._Active = false;
	}

	private void LateUpdate()
	{
		if (pIsResetRequired)
		{
			pIsResetRequired = false;
			ResetCogs(inPartially: true);
		}
		if (mIsPlaying && _DefectiveMachines.Count > 0)
		{
			SnChannel.Play(_CogBrokenSFX, "SFX_Pool", inForce: true).pLoop = true;
			if (_ExplodeParticle != null)
			{
				_ExplodeParticle.Play();
			}
			for (int i = 0; i < _StartCogs.Count; i++)
			{
				_StartCogs[i].pGear.machine.StopMachine();
			}
			mGameCompleted = false;
			mIsPlaying = false;
			ResetCogs(inPartially: true);
			_MainUI.SetHelperButtons(inActive: true);
		}
	}

	public void OnReplayGame()
	{
		ResetGameplay();
		int num = 0;
		while (num < _CogsContainer.Count)
		{
			if (_CogsContainer[num]._CogType == CogType.INVENTORY_COG)
			{
				_MainUI._CogsItemMenu.AddItem(_CogsContainer[num].pCachedCog);
				UnityEngine.Object.Destroy(_CogsContainer[num].gameObject);
				_CogsContainer.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
		SnChannel.Play(_ResetSFX);
		_ResetParticle?.Play(withChildren: true);
		_MainUI.SetVisibility(inVisible: true);
		ResetCogs();
	}

	private void ResetGameplay()
	{
		mCheckForGameComplete = true;
		_StartLeverClickable._Active = false;
		_ExplodeParticle?.Stop();
		SnChannel.AcquireChannel("SFX_Pool", inForce: true).pLoop = false;
		SnChannel.StopPool("SFX_Pool");
		KAUI.RemoveExclusive(_GameEndDBUI);
		_GameEndDBUI.SetVisibility(Visibility: false);
		SetMachines(inActive: false);
		mGameCompleted = false;
		mStartLeverPressed = false;
		mLastAlignedCog = null;
		ResetCounters();
		_MainUI.ResetUI();
		_DefectiveMachines.Clear();
	}

	private void OnNextLevel()
	{
		ResetGameplay();
		DestroyCogs();
		CogsLevelManager.pInstance.LoadNextLevel();
	}

	private void DestroyCogs()
	{
		for (int i = 0; i < _CogsContainer.Count; i++)
		{
			UnityEngine.Object.Destroy(_CogsContainer[i].gameObject);
		}
		_CogsContainer.Clear();
		_StartCogs.Clear();
		_VictoryCogs.Clear();
		_MainUI._CogsItemMenu.ClearItems();
	}

	private void ResetCounters()
	{
		pLapsedTime = 0f;
		pCurrentMoves = 0;
		pIsTimerStarted = false;
		mTimeCheckForWin = _TimeToCheckForWin;
		_MainUI.UpdateMoveCounter();
	}

	public void SetMachines(bool inActive)
	{
		mIsPlaying = inActive;
		if (!inActive)
		{
			mTimeCheckForWin = _TimeToCheckForWin;
			ResetCogs(inPartially: true);
		}
		else
		{
			SnChannel.Play(_CogStartSFX);
			pCurrentMoves++;
			_MainUI.UpdateMoveCounter();
			CalculateCogs();
		}
		for (int i = 0; i < _StartCogs.Count; i++)
		{
			if (mIsPlaying && _StartCogs[i].pIsMachineDefective)
			{
				_DefectiveMachines.Add(_StartCogs[i]);
			}
			else
			{
				SetGFMachine(_StartCogs[i], inActive);
			}
		}
		if (_DefectiveMachines.Count == 0 && mSaveStep == SAVESTEP.NONE)
		{
			_MainUI.SetHelperButtons(!inActive);
		}
	}

	private void SetGFMachine(CogObject inMachine, bool inActive)
	{
		if (inActive)
		{
			SnChannel.Play(_CogTurningSFX, "SFX_Pool", inForce: true).pLoop = true;
			inMachine.pGear.machine.RecalculateGears();
			inMachine.pGear.machine.ReAlignCogs();
			inMachine.pGear.machine.StartMachine();
			for (int i = 0; i < _CogsContainer.Count; i++)
			{
				if (_CogsContainer[i]._CogType == CogType.STATIC_COG && _CogsContainer[i]._IsRachetAttached && IsAttachedToStartCog(_CogsContainer[i]))
				{
					PlayRatchetAnim(_CogsContainer[i], inPlay: true);
				}
			}
		}
		else
		{
			inMachine.pGear.machine.StopMachine();
		}
	}

	public void ExplodeBadCogs(List<CogObject> inMachines)
	{
		if (_ExplodeParticle != null)
		{
			_ExplodeParticle.Play(withChildren: true);
		}
		for (int i = 0; i < inMachines.Count; i++)
		{
			ResetMachine(inMachines[i]);
		}
	}

	private void ResetMachine(CogObject inMachine)
	{
		int num = 0;
		while (num < _CogsContainer.Count)
		{
			if (_CogsContainer[num]._CogType == CogType.INVENTORY_COG && _CogsContainer[num]._ConnectedMachineCogs.Contains(inMachine))
			{
				_MainUI._CogsItemMenu.AddItem(_CogsContainer[num].pCachedCog);
				UnityEngine.Object.Destroy(_CogsContainer[num].gameObject);
				_CogsContainer.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}

	private void ResetCogs(bool inPartially = false)
	{
		for (int i = 0; i < _CogsContainer.Count; i++)
		{
			CogObject cogObject = _CogsContainer[i];
			cogObject.pIsMachineDefective = false;
			cogObject.SetMachineParticles(inPlay: false);
			if (inPartially)
			{
				int num = 0;
				while (num < cogObject._InContactList.Count)
				{
					if (cogObject._InContactList[num] == null)
					{
						cogObject._InContactList.RemoveAt(num);
					}
					else
					{
						num++;
					}
				}
				int num2 = 0;
				while (num2 < cogObject._InContactRatchetList.Count)
				{
					if (cogObject._InContactRatchetList[num2] == null)
					{
						cogObject._InContactRatchetList.RemoveAt(num2);
					}
					else
					{
						num2++;
					}
				}
			}
			else
			{
				cogObject._InContactList.Clear();
				cogObject._InContactRatchetList.Clear();
			}
			if (cogObject._CogType != CogType.START_COG)
			{
				cogObject._ConnectedMachineCogs.Clear();
				cogObject.pParent = null;
				cogObject.pGear.DrivenBy = null;
				if (cogObject._CogType == CogType.STATIC_COG && cogObject._IsRachetAttached)
				{
					PlayRatchetAnim(cogObject, inPlay: false);
				}
			}
		}
	}

	private void PlayRatchetAnim(CogObject cogObject, bool inPlay)
	{
		Animation componentInChildren = cogObject._AttachedRachet.GetComponentInChildren<Animation>();
		if (componentInChildren != null && componentInChildren.GetClipCount() > 0)
		{
			if (inPlay)
			{
				componentInChildren.Play();
			}
			else
			{
				componentInChildren.Stop();
			}
		}
	}

	private bool IsAttachedToStartCog(CogObject cogObject)
	{
		while (cogObject.pParent != null)
		{
			cogObject = cogObject.pParent;
		}
		if (cogObject._CogType == CogType.START_COG)
		{
			return true;
		}
		return false;
	}

	public void AlignCog(CogObject inCog, bool snapPositions = false)
	{
		for (int i = 0; i < _CogsContainer.Count; i++)
		{
			if (!(_CogsContainer[i] != inCog))
			{
				continue;
			}
			float num = (inCog.transform.position - _CogsContainer[i].transform.position).magnitude - inCog.pGear.radius - _CogsContainer[i].pGear.radius;
			if (num < _AdjustCogOffset)
			{
				if (mLastAlignedCog != _CogsContainer[i])
				{
					mLastAlignedCog = _CogsContainer[i];
					SnChannel.Play(_CogSnapSFX);
				}
				inCog.pGear.gearGen.AlignPositions(_CogsContainer[i].pGear.gearGen, snapPositions);
				break;
			}
			if (mLastAlignedCog == _CogsContainer[i] && num >= _AdjustCogOffset)
			{
				mLastAlignedCog = null;
			}
		}
	}

	private bool AssignCogMachine(CogObject inObjCog, List<CogObject> inMachines, bool isCCW)
	{
		if (inObjCog._InContactList.Count == 0)
		{
			return true;
		}
		foreach (GameObject inContact in inObjCog._InContactList)
		{
			CogObject component = inContact.GetComponent<CogObject>();
			if (component._CogType == CogType.START_COG)
			{
				continue;
			}
			foreach (CogObject inMachine in inMachines)
			{
				if (!component._ConnectedMachineCogs.Contains(inMachine))
				{
					if (component.pParent == null)
					{
						component.pParent = inObjCog;
						component.pIsCCW = !isCCW;
					}
					component._ConnectedMachineCogs.Add(inMachine);
					AssignCogMachine(component, inMachines, !isCCW);
				}
			}
		}
		return true;
	}

	private bool CalculateCogs()
	{
		mCogValidationList.Clear();
		for (int i = 0; i < _StartCogs.Count; i++)
		{
			if (_StartCogs[i]._InContactList.Count > 0 && _StartCogs[i].pIsCCW.HasValue)
			{
				AssignCogMachine(_StartCogs[i], _StartCogs[i]._ConnectedMachineCogs, _StartCogs[i].pIsCCW.Value);
			}
		}
		for (int j = 0; j < _CogsContainer.Count; j++)
		{
			CogObject cogObject = _CogsContainer[j];
			if (_CogsContainer[j]._CogType == CogType.START_COG)
			{
				continue;
			}
			if (cogObject.pIsMachineDefective)
			{
				foreach (CogObject connectedMachineCog in cogObject._ConnectedMachineCogs)
				{
					connectedMachineCog.pIsMachineDefective = true;
				}
			}
			else
			{
				if (cogObject._ConnectedMachineCogs.Count <= 0)
				{
					continue;
				}
				cogObject.transform.parent = cogObject._ConnectedMachineCogs[0].transform.parent;
				foreach (GameObject obj in cogObject._InContactList)
				{
					if (!cogObject._ConnectedMachineCogs.Exists((CogObject x) => x.gameObject == obj))
					{
						continue;
					}
					CogObject cogObject2 = (cogObject.pParent = obj.GetComponent<CogObject>());
					cogObject.transform.parent = obj.transform.parent;
					if (cogObject2.pIsCCW.Value == !cogObject.pIsCCW.Value)
					{
						break;
					}
					foreach (CogObject connectedMachineCog2 in cogObject._ConnectedMachineCogs)
					{
						connectedMachineCog2.pIsMachineDefective = true;
					}
					break;
				}
				if (!IsRotationAllowed(cogObject._RotateDirection, cogObject.pIsCCW.Value))
				{
					foreach (CogObject connectedMachineCog3 in cogObject._ConnectedMachineCogs)
					{
						connectedMachineCog3.pIsMachineDefective = true;
					}
				}
				else if (cogObject.pParent != null)
				{
					cogObject.pGear.DrivenBy = cogObject.pParent.pGear;
				}
			}
		}
		return true;
	}

	public bool IsRotationAllowed(RotateDirection inDirection, bool isDirectionCCW)
	{
		if (inDirection == RotateDirection.ALL)
		{
			return true;
		}
		if (isDirectionCCW)
		{
			return inDirection == RotateDirection.CCW;
		}
		return inDirection == RotateDirection.CW;
	}

	private void OnDestroy()
	{
		SnChannel.StopPool("Music_Pool");
		SnChannel.PlayPool(_AmbMusicPool);
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: true);
		}
	}

	private int CalculateLevelStars()
	{
		int num = 1;
		if (pCurrentMoves <= CogsLevelManager.pInstance.pCurrentLevelData.GoalMoves)
		{
			num++;
		}
		if (pLapsedTime <= (float)CogsLevelManager.pInstance.pCurrentLevelData.GoalTime)
		{
			num++;
		}
		return num;
	}
}
