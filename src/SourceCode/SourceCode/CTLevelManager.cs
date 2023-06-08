using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTLevelManager : IMGLevelManager
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

	public UiIncredibleMachinesInventoryMenu _InventoryMenu;

	public UiIncredibleMachinesLevelSelectionMenu _LevelSelectionMenu;

	public UiIncredibleMachinesLevelSelection _LevelSelection;

	public UIIncredibleMachines _UIIncredibleMachines;

	public UiIncredibleMachineHelp _UIIncredibleMachinesHelp;

	public bool _DestroyPreviousObjects = true;

	public float _SingleGameExitDelay = 5f;

	public bool _UnlockAllLevels;

	public string _XMLPath = "RS_DATA/IMLevelDataDO.xml";

	public string _TutorialKeyName = "IncredibleMachineTutorial";

	public int _StarsAchievementTaskID;

	public LocaleString _ServerErrorTitleText = new LocaleString("An Error Occurred");

	public LocaleString _ServerErrorBodyText = new LocaleString("Something went wrong while attempting to save your progress to the server.\n\nError code: {0}\n\nWould you like to try saving again?");

	private static IMLevelData mLevelData;

	private SAVESTEP mSaveStep;

	private bool mInitialized;

	private int mCurrentLevelIndex = -1;

	private LevelData mCurrentLevelData;

	private int mLastPlayedLevelIndex = -1;

	public static string pLevelToLoad;

	private bool mSingleLevelGame;

	public UiIncredibleMachinesEndDB _GameEndDBUI;

	public string _GameModuleName = "DOIncredibleMachine";

	public string _CompletedGameModuleName = "DOIncredibleMachineCompleted";

	public int _GameID = 99;

	public int _MaxScore = 1000;

	public int _MaxScorePerItem = 100;

	public int _MaxBonusScoreForTime = 1000;

	public int _MaxScorePerStar = 1000;

	public int _MinLevelScore = 100;

	public float _MaxGoalTime = 10f;

	public float _GracePeriod = 4f;

	public float _DoubleClickInterval = 0.3f;

	public AudioClip _BgMusic;

	public string _AmbMusicPool = "AmbMusic_Pool";

	private int mLevelNum;

	private int mLevelScore;

	private int mHighScore;

	private int mItemEfficieny;

	private int mPreviousStarsCollected;

	private int mClickCount;

	private bool mStartDoubleClickTimer;

	private float mDoubleClickTime;

	private int mLayerAvatarName;

	private int mLayerSelfLit;

	private int mDibbles;

	private int mXP;

	private bool mShowTutorial;

	private int mTimeBonus;

	private bool mItemLoading;

	private AchievementReward[] mAchievementRewards;

	private string mModuleName;

	private AvAvatarState mPrevAvatarState;

	private float mTimeTaken;

	public static IMLevelData pLevelData => mLevelData;

	public int pCurrentLevelIndex
	{
		get
		{
			return mCurrentLevelIndex;
		}
		set
		{
			mCurrentLevelIndex = value;
		}
	}

	public bool pSingleLevelGame => mSingleLevelGame;

	public float pTimer => mTimeTaken;

	public int pUsedCraftTraptionsItem => placedObjects.Count;

	public int pItemEfficiency => mItemEfficieny;

	public int pScore => mLevelScore;

	public int pHighScore => mHighScore;

	public int pDibbles => mDibbles;

	public int pXP => mXP;

	protected virtual void Awake()
	{
		IMGLevelManager.mInstance = this;
	}

	protected override void Start()
	{
		base.Start();
		SnChannel.PausePool(_AmbMusicPool);
		SnChannel.Play(_BgMusic, "Music_Pool", inForce: true);
		if (!UtPlatform.IsRealTimeShadowEnabled())
		{
			UtUtilities.SetRealTimeShadowDisabled();
		}
		if (mLevelData == null)
		{
			RsResourceManager.Load(_XMLPath, XMLDownloaded);
		}
		AvAvatar.SetActive(inActive: false);
		mPrevAvatarState = AvAvatar.pState;
		AvAvatar.pState = AvAvatarState.PAUSED;
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: false);
		}
		if (ProductData.pIsReady && !ProductData.TutorialComplete(_TutorialKeyName))
		{
			mShowTutorial = true;
		}
		if (IMLevelProgress.pInstance != null)
		{
			levelNumber = IMLevelProgress.pInstance.pCurrentLevel.ToString();
		}
		mLayerAvatarName = LayerMask.NameToLayer("AvatarName");
		mLayerSelfLit = LayerMask.NameToLayer("SelfLit");
		Physics2D.IgnoreLayerCollision(mLayerAvatarName, mLayerAvatarName, ignore: true);
		Physics2D.IgnoreLayerCollision(mLayerAvatarName, mLayerSelfLit, ignore: true);
	}

	private void XMLDownloaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inFile, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			mLevelData = UtUtilities.DeserializeFromXml<IMLevelData>((string)inFile);
			break;
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("Failed to Load IM LEVEL DATA XML");
			break;
		}
	}

	private void InitGame()
	{
		RsResourceManager.DestroyLoadScreen();
		UICursorManager.pVisibility = true;
		mInitialized = true;
		if (!string.IsNullOrEmpty(pLevelToLoad))
		{
			mCurrentLevelData = FetchCurrentLevelData(pLevelToLoad);
			pLevelToLoad = string.Empty;
		}
		if (mCurrentLevelData == null)
		{
			ShowLevelMenu();
			return;
		}
		mSingleLevelGame = true;
		SetupLevel();
	}

	public void SetupLevel()
	{
		ResetTimer();
		_UIIncredibleMachines.SetVisibility(inVisible: true);
		GoalManager.pInstance.ClearGoals();
		mCurrentLevelData = mLevelData.Levels[pCurrentLevelIndex];
		if (mCurrentLevelData != null)
		{
			_UIIncredibleMachines.SetState(KAUIState.DISABLED);
			SetUpInventory();
			SetupStaticItems();
		}
		else
		{
			Debug.LogError("[IM] Current Level Data is NULL for :: " + pLevelToLoad);
		}
	}

	public void ShowHelpScreen()
	{
		KAUI.SetExclusive(_UIIncredibleMachinesHelp);
		_UIIncredibleMachinesHelp.SetVisibility(inVisible: true);
	}

	public int GetLevelNumber(int idx)
	{
		int num = 1;
		for (int i = 0; i < idx && i < mLevelData.Levels.Length; i++)
		{
			if (!mLevelData.Levels[i].IsMissionLevel)
			{
				num++;
			}
		}
		return num;
	}

	public void ShowLevelMenu()
	{
		_UIIncredibleMachines.SetVisibility(inVisible: false);
		_LevelSelection.PopulateLevels(mLevelData);
		SetCurrentPage();
	}

	private void SetCurrentPage()
	{
		List<KAWidget> items = _LevelSelection._UiLevelSelectionMenu.GetItems();
		int num = 1;
		for (int i = 0; i < items.Count; i++)
		{
			IncredibleMachineLevelUserData incredibleMachineLevelUserData = (IncredibleMachineLevelUserData)items[i].GetUserData();
			if (mCurrentLevelIndex == incredibleMachineLevelUserData._Index)
			{
				num = incredibleMachineLevelUserData._Level;
				break;
			}
		}
		int inPageNumber = Mathf.CeilToInt((float)num / (float)_LevelSelection._UiLevelSelectionMenu.GetNumItemsPerPage());
		_LevelSelection._UiLevelSelectionMenu.GoToPage(inPageNumber, instant: true);
		_LevelSelection.SetVisibility(inVisible: true);
	}

	public void SetUpInventory()
	{
		if (mCurrentLevelData.IMInventoryItems != null)
		{
			IMInventoryItemData[] iMInventoryItems = mCurrentLevelData.IMInventoryItems;
			foreach (IMInventoryItemData iMInventoryItemData in iMInventoryItems)
			{
				KAWidget kAWidget = _InventoryMenu.AddWidget("TemplateTool");
				IncredibleMachineUserData incredibleMachineUserData = new IncredibleMachineUserData
				{
					Asset = iMInventoryItemData.Asset,
					Quantity = iMInventoryItemData.Quantity
				};
				kAWidget.SetTextureFromBundle(iMInventoryItemData.Icon);
				kAWidget.SetUserData(incredibleMachineUserData);
				kAWidget.name = iMInventoryItemData.AssetName;
				kAWidget.FindChildItem("ItemCount").SetText(incredibleMachineUserData.Quantity.ToString());
			}
		}
		else
		{
			string levelName = pLevelToLoad;
			if (pCurrentLevelIndex > 0)
			{
				levelName = mLevelData.Levels[pCurrentLevelIndex].LevelName;
			}
			Debug.LogError("[IM] InventoryItems is NULL for :: " + levelName);
		}
	}

	public LevelData FetchCurrentLevelData(string levelName)
	{
		int num = -1;
		LevelData[] levels = mLevelData.Levels;
		foreach (LevelData levelData in levels)
		{
			num++;
			if (levelData.LevelName == levelName && IMLevelProgress.pInstance.IsLevelUnlocked(levelData))
			{
				mCurrentLevelIndex = num;
				return levelData;
			}
		}
		return null;
	}

	public void SetupStaticItems()
	{
		List<string> list = new List<string>();
		KAUICursorManager.SetDefaultCursor("Loading");
		if (mCurrentLevelData.StaticItems != null)
		{
			IMStaticItemData[] staticItems = mCurrentLevelData.StaticItems;
			foreach (IMStaticItemData iMStaticItemData in staticItems)
			{
				list.Add(iMStaticItemData.Asset);
			}
		}
		else
		{
			string levelName = pLevelToLoad;
			if (pCurrentLevelIndex > 0)
			{
				levelName = mLevelData.Levels[pCurrentLevelIndex].LevelName;
			}
			Debug.LogError("[IM] StaticItemData is NULL for :: " + levelName);
		}
		if (list.Count > 0)
		{
			mItemLoading = true;
			new RsAssetLoader().Load(list.ToArray(), null, BundleLoadEventHandler);
			return;
		}
		string levelName2 = pLevelToLoad;
		if (pCurrentLevelIndex > 0)
		{
			levelName2 = mLevelData.Levels[pCurrentLevelIndex].LevelName;
		}
		Debug.LogError("[IM] StaticItemData asset list is empty for :: " + levelName2);
	}

	private void BundleLoadEventHandler(RsAssetLoader inLoader, RsResourceLoadEvent inEvent, float inProgress, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (_DestroyPreviousObjects)
			{
				DestroyPreviousObjects();
			}
			CreateStaticItems();
			KAUICursorManager.SetDefaultCursor("Arrow");
			_UIIncredibleMachines.SetState(KAUIState.INTERACTIVE);
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.SetTimedTaskUpdate(inState: true, inForceUpdate: true);
			}
			if (mShowTutorial)
			{
				ShowHelpScreen();
				ProductData.AddTutorial(_TutorialKeyName);
				mShowTutorial = false;
			}
			mItemLoading = false;
			break;
		case RsResourceLoadEvent.ERROR:
		{
			string levelName = pLevelToLoad;
			if (pCurrentLevelIndex > 0)
			{
				levelName = mLevelData.Levels[pCurrentLevelIndex].LevelName;
			}
			Debug.LogError("[IM] Failed to load asset list for :: " + levelName);
			KAUICursorManager.SetDefaultCursor("Arrow");
			mItemLoading = false;
			break;
		}
		}
	}

	private void CreateStaticItems()
	{
		if (mCurrentLevelData.StaticItems != null && activeContainer != null)
		{
			IMStaticItemData[] staticItems = mCurrentLevelData.StaticItems;
			foreach (IMStaticItemData iMStaticItemData in staticItems)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromBundle(iMStaticItemData.Asset, typeof(GameObject)));
				gameObject.name = iMStaticItemData.AssetName;
				gameObject.transform.parent = activeContainer;
				gameObject.transform.localPosition = StringToVector3(iMStaticItemData.Position);
				gameObject.transform.localEulerAngles = StringToVector3(iMStaticItemData.Rotation);
				gameObject.transform.localScale = StringToVector3(iMStaticItemData.Scale);
				Rigidbody2D component = gameObject.GetComponent<Rigidbody2D>();
				if (component != null)
				{
					component.isKinematic = iMStaticItemData.IsKinematic;
				}
				ObjectBase component2 = gameObject.GetComponent<ObjectBase>();
				if (component2 != null)
				{
					component2._PivotPositionSlider = (iMStaticItemData.PivotPosition.HasValue ? iMStaticItemData.PivotPosition.Value : 0f);
					component2._PivotRotationSlider = (iMStaticItemData.PivotRotation.HasValue ? iMStaticItemData.PivotRotation.Value : 0f);
					component2._PivotAngleLimit = (iMStaticItemData.PivotAngleLimit.HasValue ? iMStaticItemData.PivotAngleLimit.Value : 0f);
					component2._ObjectProperty = iMStaticItemData.ObjectProperty;
					GoalManager.pInstance.AssignToGoalInfoList(component2);
					activeObjects.Add(gameObject.GetComponent<ObjectBase>());
					gameObject.GetComponent<ObjectBase>().Setup();
				}
			}
		}
		else
		{
			string levelName = pLevelToLoad;
			if (pCurrentLevelIndex > 0)
			{
				levelName = mLevelData.Levels[pCurrentLevelIndex].LevelName;
			}
			Debug.LogError("[IM] activeContainer may be Null. Failed to create static items for :: " + levelName);
		}
	}

	private void DestroyPreviousObjects()
	{
		for (int i = 0; i < activeObjects.Count; i++)
		{
			UnityEngine.Object.Destroy(activeObjects[i].gameObject);
		}
		activeObjects.Clear();
		placedObjects.Clear();
	}

	private Vector3 StringToVector3(string stringValue)
	{
		string[] array = stringValue.Split(',');
		float[] array2 = new float[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			float.TryParse(array[i], out array2[i]);
		}
		return new Vector3(array2[0], array2[1], array2[2]);
	}

	protected virtual void Update()
	{
		if (!mInitialized && (!UserInfo.pIsReady || IMLevelProgress.pIsReady) && mLevelData != null && GoalManager.pInstance != null)
		{
			InitGame();
		}
		if (!inPlayMode && !mShowTutorial && !_InventoryMenu.pItemLoading && !mItemLoading)
		{
			mTimeTaken += Time.deltaTime;
			CheckForDoubleClick();
		}
	}

	public void CheckForDoubleClick()
	{
		if (Input.GetMouseButtonDown(0))
		{
			mClickCount++;
			mStartDoubleClickTimer = true;
			if (mClickCount >= 2 && mDoubleClickTime < _DoubleClickInterval)
			{
				InputManager.pInstance.RemoveSelectedLevelItem();
			}
		}
		if (mStartDoubleClickTimer)
		{
			mDoubleClickTime += Time.deltaTime;
			if (mDoubleClickTime >= _DoubleClickInterval)
			{
				mStartDoubleClickTimer = false;
				mDoubleClickTime = 0f;
				mClickCount = 0;
			}
		}
	}

	public static void UnlockAllLevels()
	{
		if (mLevelData != null)
		{
			int num = mLevelData.Levels.Length - 1;
			if (num != -1)
			{
				IMLevelProgress.SaveCurrentLevel(num);
			}
			if (IMGLevelManager.pInstance != null && ((CTLevelManager)IMGLevelManager.pInstance)._LevelSelectionMenu != null && ((CTLevelManager)IMGLevelManager.pInstance)._LevelSelectionMenu.GetVisibility())
			{
				((CTLevelManager)IMGLevelManager.pInstance).ShowLevelMenu();
			}
		}
	}

	public void PlayNextLevel()
	{
		int nextLevel = GetNextLevel();
		if (mLevelData.Levels[nextLevel].MemberOnly && !UnlockManager.IsSceneUnlocked(_GameModuleName, inShowUi: false, delegate(bool success)
		{
			if (success)
			{
				PlayNextLevel();
			}
		}))
		{
			OnMainMenu();
			return;
		}
		if (_LevelSelection.GetVisibility())
		{
			_LevelSelection.SetVisibility(inVisible: false);
		}
		_InventoryMenu.ClearItems();
		RestartScene();
		DestroyPreviousObjects();
		mCurrentLevelIndex = nextLevel;
		SetupLevel();
	}

	public int GetNextLevel()
	{
		for (int i = mCurrentLevelIndex + 1; i < mLevelData.Levels.Length; i++)
		{
			if (!mLevelData.Levels[i].IsMissionLevel)
			{
				return i;
			}
		}
		return -1;
	}

	public override void RemoveItem(ObjectBase item)
	{
		base.RemoveItem(item);
		_InventoryMenu.AddItemBack(item.gameObject);
		UnityEngine.Object.Destroy(item.gameObject);
	}

	public void CalculateScore()
	{
		mTimeBonus = ((mTimeTaken > _GracePeriod) ? (_MaxBonusScoreForTime - (int)((float)_MaxBonusScoreForTime * (mTimeTaken - _GracePeriod) / (_MaxGoalTime - _GracePeriod))) : _MaxBonusScoreForTime);
		if (mTimeBonus < 0)
		{
			mTimeBonus = 0;
		}
		int num = GetStarsCollected() * _MaxScorePerStar;
		mLevelScore = _MaxScore - pUsedCraftTraptionsItem * _MaxScorePerItem + mTimeBonus + num + mCurrentLevelData.DifficultyBonus.GetValueOrDefault(0);
		mItemEfficieny = _MaxScore - pUsedCraftTraptionsItem * _MaxScorePerItem;
		if (mLevelScore < 0)
		{
			mLevelScore = _MinLevelScore;
		}
		if (mLevelScore > mHighScore)
		{
			mHighScore = mLevelScore;
		}
		if (IMLevelProgress.pInstance != null && IMLevelProgress.pInstance.pMissionLevel)
		{
			mLevelScore = 1;
		}
	}

	public override void RestartScene()
	{
		InputManager.pInstance.HideFeedback(0f);
		RemovePlacedItems();
		base.RestartScene();
		ResetScene();
	}

	public void RemovePlacedItems()
	{
		List<ObjectBase> list = new List<ObjectBase>();
		list.AddRange(placedObjects);
		foreach (ObjectBase item in list)
		{
			RemoveItem(item);
		}
	}

	public void OnMainMenu()
	{
		ClearScene();
	}

	public void ClearScene()
	{
		_InventoryMenu.ClearItems();
		if (mCurrentLevelIndex > mLastPlayedLevelIndex)
		{
			ShowLevelMenu();
		}
		else
		{
			_UIIncredibleMachines.SetVisibility(inVisible: false);
			_LevelSelection.SetVisibility(inVisible: true);
		}
		RestartScene();
		DestroyPreviousObjects();
	}

	private void ResetTimer()
	{
		mTimeTaken = 0f;
	}

	public override void EnableScene()
	{
		_UIIncredibleMachines.DisableButtons(Play: true, Reset: false, Inventory: true, Help: true);
		_InventoryMenu.SetState(KAUIState.DISABLED);
		InputManager.pInstance.HideFeedback(0f);
		base.EnableScene();
		Physics2D.IgnoreLayerCollision(mLayerAvatarName, mLayerAvatarName, ignore: false);
		Physics2D.IgnoreLayerCollision(mLayerAvatarName, mLayerSelfLit, ignore: false);
	}

	public override void ResetScene()
	{
		_UIIncredibleMachines.DisableButtons(Play: false, Reset: true, Inventory: false, Help: false);
		_InventoryMenu.SetState(KAUIState.INTERACTIVE);
		base.ResetScene();
		Physics2D.IgnoreLayerCollision(mLayerAvatarName, mLayerAvatarName, ignore: true);
		Physics2D.IgnoreLayerCollision(mLayerAvatarName, mLayerSelfLit, ignore: true);
	}

	public void StopScene()
	{
		foreach (ObjectBase activeObject in activeObjects)
		{
			PhysicsObject component = activeObject.GetComponent<PhysicsObject>();
			if ((bool)component)
			{
				if (component.rigidbody2D != null)
				{
					component.rigidbody2D.gravityScale = 0f;
					component.rigidbody2D.velocity = new Vector2(0f, 0f);
					component.rigidbody2D.freezeRotation = true;
				}
				if (component.name == "PfRope2D")
				{
					((CTRopeHolder)component).MakeLinksStatic();
				}
			}
			else if ((bool)base.rigidbody2D)
			{
				base.rigidbody2D.isKinematic = true;
			}
		}
	}

	public override void GoalReached()
	{
		if (!goalReached)
		{
			MissionManager.pInstance?.CheckForTaskCompletion("Game", _GameModuleName, mCurrentLevelData.LevelName);
		}
		foreach (Star item in activeObjects.FindAll((ObjectBase t) => t.GetType() == typeof(Star)))
		{
			item.gameObject.SetActive(value: false);
		}
		MissionManager.pInstance?.SetTimedTaskUpdate(inState: false);
		base.GoalReached();
		if (!mSingleLevelGame)
		{
			ProcessGameEndScreen();
		}
		else
		{
			StartCoroutine(WaitForSingleGameExitDelay());
		}
	}

	private IEnumerator WaitForSingleGameExitDelay()
	{
		yield return new WaitForSeconds(_SingleGameExitDelay);
		QuitGame();
	}

	private void ProcessGameEndScreen()
	{
		_UIIncredibleMachines.SetVisibility(inVisible: false);
		CalculateScore();
		mPreviousStarsCollected = IMLevelProgress.GetStarsCollected(mCurrentLevelData.LevelName);
		mLevelNum = GetLevelNumber(mCurrentLevelIndex);
		HighScores.SetCurrentGameSettings(_GameModuleName, _GameID, isMultiPlayer: false, 0, mLevelNum);
		HighScores.AddGameData("highscore", mLevelScore.ToString());
		_GameEndDBUI.SetInteractive(interactive: false);
		_GameEndDBUI.SetVisibility(Visibility: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		StartPairDataSave();
	}

	private void StartPairDataSave()
	{
		mSaveStep = SAVESTEP.PAIRDATA_START;
		if (IMLevelProgress.pInstance == null)
		{
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", string.Format(_ServerErrorBodyText.GetLocalizedString(), (int)mSaveStep), _ServerErrorTitleText.GetLocalizedString(), base.gameObject, null, null, "QuitGame", null, inDestroyOnClick: true);
			int num = (int)mSaveStep;
			UtDebug.LogError("Incredible Machine failed to save at step " + num);
		}
		else
		{
			IMLevelProgress.OnPairDataSaved = (Action<bool, PairData, object>)Delegate.Combine(IMLevelProgress.OnPairDataSaved, new Action<bool, PairData, object>(OnPairDataSaved));
			IMLevelProgress.pInstance.Save(mCurrentLevelData.LevelName, starsCollected, (IMLevelProgress.pInstance.GetLastLevelPlayed() < mLevelNum) ? mLevelNum : (-1));
		}
	}

	private void OnPairDataSaved(bool success, PairData pData, object inUserData)
	{
		mSaveStep = SAVESTEP.PAIRDATA_END;
		IMLevelProgress.OnPairDataSaved = (Action<bool, PairData, object>)Delegate.Remove(IMLevelProgress.OnPairDataSaved, new Action<bool, PairData, object>(OnPairDataSaved));
		if (success)
		{
			StartAchievementTaskSave();
			return;
		}
		int num = (int)mSaveStep;
		UtDebug.LogError("Incredible Machine failed to save at step " + num);
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
		QuitGame();
	}

	private void StartAchievementTaskSave()
	{
		if (starsCollected == 3)
		{
			mSaveStep = SAVESTEP.ACHIEVEMENT_START;
			UserAchievementTask.Set(new AchievementTask[1]
			{
				new AchievementTask(_StarsAchievementTaskID, mCurrentLevelData.LevelName)
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
				UtDebug.Log("Achievement already earned for level " + mCurrentLevelIndex);
			}
			break;
		case WsServiceEvent.ERROR:
		{
			int num = (int)mSaveStep;
			UtDebug.LogError("Incredible Machine failed to save at step " + num);
			ShowAchievementSaveRetryPrompt();
			break;
		}
		}
	}

	private void SetPayout()
	{
		mSaveStep = SAVESTEP.PAYOUT_START;
		mModuleName = _GameModuleName;
		if (starsCollected <= mPreviousStarsCollected)
		{
			mModuleName = _CompletedGameModuleName;
		}
		if (SubscriptionInfo.pIsMember)
		{
			mModuleName += "Member";
		}
		WsWebService.ApplyPayout(mModuleName, mLevelScore, ServiceEventHandler, null);
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
			UtDebug.LogError("Incredible Machine failed to save at step " + num);
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
		WsWebService.ApplyPayout(mModuleName, mLevelScore, ServiceEventHandler, null);
	}

	private void ShowGameEndDBUI()
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (_GameEndDBUI != null)
		{
			KAUI.SetExclusive(_GameEndDBUI);
			_GameEndDBUI.SetAdRewardData(mModuleName, mLevelScore);
			_GameEndDBUI.SetHighScoreData(mLevelScore, "highscore");
			_GameEndDBUI.SetResultData(mItemEfficieny, mTimeBonus, GetStarsCollected(), mLevelScore);
			_GameEndDBUI.SetGameSettings(_GameModuleName, base.gameObject, "any");
			_GameEndDBUI.SetRewardDisplay(mAchievementRewards);
			_GameEndDBUI.SetVisibility(Visibility: true);
			_GameEndDBUI.SetInteractive(interactive: true);
		}
		mAchievementRewards = null;
		mSaveStep = SAVESTEP.NONE;
	}

	private void OnReplayGame()
	{
		KAUI.RemoveExclusive(_GameEndDBUI);
		_InventoryMenu.ClearItems();
		RestartScene();
		DestroyPreviousObjects();
		SetupLevel();
	}

	public void QuitGame()
	{
		AvAvatar.SetActive(inActive: true);
		AvAvatar.pState = mPrevAvatarState;
		UnityEngine.Object.Destroy(base.transform.root.gameObject);
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
}
