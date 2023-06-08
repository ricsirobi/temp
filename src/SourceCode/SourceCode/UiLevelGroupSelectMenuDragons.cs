using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UiLevelGroupSelectMenuDragons : KAUIMenu
{
	public class KAUIFlightSchoolItemData : KAWidgetUserData
	{
		public int _SceneID;

		public int _LevelID;

		public int _PreviousHighScore;

		public KAUIFlightSchoolItemData(int scene, int lvl, int score)
		{
			_SceneID = scene;
			_LevelID = lvl;
			_PreviousHighScore = score;
		}
	}

	public static bool gUnlockAll;

	public int mCurrentSceneId;

	public bool _IsHeroDragon;

	public GameObject _LevelFailUI;

	public GameObject _LevelQuitUI;

	public int _CurLevelGroupIdx;

	public AudioClip _NoneMemberClip;

	public GUISkin _NameSkin;

	public KAUI _MainUI;

	public KAWidget _DrgonPropParent;

	public LocaleString _EntranceFeeText = new LocaleString("Entrance fee of {{GEMS}} gems is required to play. Pay now or wait for {{LOCKTIME}}?");

	public LocaleString _NotEnoughFeeText = new LocaleString("You do not have enough gems to pay, Please buy more!");

	public LocaleString _LevelPurchaseProcessingText = new LocaleString("Processing level purchase.");

	public LocaleString _LevelPurchaseSuccessfulText = new LocaleString("Level purchase successful.");

	public LocaleString _LevelPurchaseFailedText = new LocaleString("Level purchase failed.");

	public LocaleString _LockedItemClickText = new LocaleString("You need to beat previous levels to unlock this level.");

	protected bool mIsLevelReady;

	protected KAWidget mParentButton;

	private bool bCanCreateButtons;

	private bool mIsHeroLevelsPopulated;

	protected ObstacleCourseLevelManager mLevelManager;

	private KAUIGenericDB mKAUIGenericDB;

	private bool mGemUnlockSaved;

	private string mCurrentThemeName = "";

	protected int mLastPlayedLevelNum;

	public string CurrentThemeName => mCurrentThemeName;

	public void LoadLevelGroup(GameObject levelGroup)
	{
		if (levelGroup != null)
		{
			levelGroup.SetActive(value: true);
			levelGroup.SendMessage("InitGame", SendMessageOptions.RequireReceiver);
		}
	}

	protected override void Start()
	{
		base.Start();
	}

	protected virtual void OnEnable()
	{
		bCanCreateButtons = true;
		KAInput.pInstance.EnableInputType("Jump", InputType.UI_BUTTONS, inEnable: false);
		mLevelManager = base.transform.root.GetComponent<ObstacleCourseLevelManager>();
		mIsHeroLevelsPopulated = false;
		ObstacleCourseLevelManager.mMenuState = FSMenuState.FS_STATE_LEVELSELECT;
	}

	protected virtual void OnDisable()
	{
		ClearItems();
	}

	protected override void Update()
	{
		if (_IsHeroDragon)
		{
			if (mLevelManager.mGradeDataReady && SanctuaryManager.pCurPetInstance != null && !mIsHeroLevelsPopulated)
			{
				PopulateHeroLevels();
				mIsHeroLevelsPopulated = true;
			}
		}
		else if (mLevelManager.mGradeDataReady)
		{
			if (SanctuaryManager.pCurPetInstance != null && bCanCreateButtons)
			{
				CreateButtons();
				if (mLevelManager._FlightSchoolIntroTut != null && !mLevelManager._FlightSchoolIntroTut.TutorialComplete())
				{
					mLevelManager._FlightSchoolIntroTut.ShowTutorial();
					if (mLevelManager._FlightSchoolIntroTut.CanShowSelectLevelTutorial())
					{
						mLevelManager._FlightSchoolIntroTut.StartNextTutorial();
					}
				}
				bCanCreateButtons = false;
			}
			ObstacleCourseLevelManager.ScoreData.mCount = 0;
			ObstacleCourseLevelManager.ScoreData.mDataReturnFail = false;
		}
		TimeSpan timeSpan = ServerTime.pCurrentTime - mLevelManager.pLastPlayedTime;
		if (mGemUnlockSaved || !(timeSpan.TotalMinutes >= (double)mLevelManager._LevelUnlockTimeInMinutes))
		{
			return;
		}
		mLevelManager.SaveGemUnlock();
		string inWidgetName = "Level " + (mLevelManager.pLastUnlockedLevel + 1);
		KAWidget kAWidget = FindItem(inWidgetName);
		if (kAWidget != null)
		{
			KAWidget kAWidget2 = kAWidget.FindChildItem("LockedIconGems");
			if (kAWidget2 != null)
			{
				kAWidget2.SetVisibility(inVisible: false);
			}
		}
		mGemUnlockSaved = true;
	}

	private void CreateButtons()
	{
		int num = 1;
		if (mLevelManager != null)
		{
			num = mLevelManager.pLastUnlockedLevel;
		}
		LevelGroupData[] array = null;
		SceneLevelGroupData[] array2 = null;
		if (mLevelManager.pGameMode == FSGameMode.FLIGHT_MODE)
		{
			array2 = mLevelManager._AdultData;
		}
		else if (mLevelManager.pGameMode == FSGameMode.GLIDE_MODE)
		{
			array2 = mLevelManager._TeenData;
		}
		else if (mLevelManager.pGameMode == FSGameMode.FLIGHT_SUIT_MODE)
		{
			array2 = mLevelManager._FlightSuitData;
		}
		int num2 = 0;
		SceneLevelGroupData[] array3 = array2;
		foreach (SceneLevelGroupData obj in array3)
		{
			array = obj._GroupData;
			int num3 = 0;
			if (obj._DragonName.Length == 0)
			{
				LevelGroupData[] array4 = array;
				foreach (LevelGroupData levelGroupData in array4)
				{
					KAWidget kAWidget = DuplicateWidget(_Template);
					kAWidget.name = "Level " + (num3 + 1);
					kAWidget.SetTexture(levelGroupData._Icon);
					kAWidget.FindChildItem("TxtLevelName").SetText(levelGroupData._LevelNameText.GetLocalizedString());
					int num4 = mLevelManager.GetPlayerHighScore(num3, isHeroDragon: false);
					if (mLevelManager._DragonSelectionUi.pSelectedTicketID == 0)
					{
						num4 = mLevelManager.GetPlayerDragonScore(num3);
					}
					KAUIFlightSchoolItemData userData = new KAUIFlightSchoolItemData(num2, num3, num4);
					kAWidget.SetUserData(userData);
					string text = null;
					string text2 = null;
					GradeSystem[] fSGrades = levelGroupData._FSGrades;
					mLevelManager.pLevelMenu.mCurrentSceneId = 0;
					for (int k = 0; k < fSGrades.Length; k++)
					{
						if (num4 >= fSGrades[k]._PointNeeded)
						{
							text = fSGrades[k]._Grade;
							text2 = fSGrades[k]._BGColor;
							break;
						}
						text = fSGrades[^1]._Grade;
						text2 = fSGrades[^1]._BGColor;
					}
					if (num3 > num)
					{
						KAWidget kAWidget2 = kAWidget.FindChildItem("LockedIcon");
						if (kAWidget2 != null)
						{
							kAWidget2.SetVisibility(inVisible: true);
						}
					}
					if (num3 <= num && num4 != 0)
					{
						string widgetName = "AniGrade" + text2;
						KAWidget kAWidget3 = kAWidget.FindChildItem(widgetName);
						if (kAWidget3 != null)
						{
							kAWidget3.SetVisibility(inVisible: true);
							if (text != null)
							{
								kAWidget3.SetText(text);
							}
						}
					}
					if (!SubscriptionInfo.pIsMember && mLevelManager.pLastUnlockedLevel >= mLevelManager._InitialNonMemberUnlockedLevel && mLevelManager._DragonSelectionUi.pSelectedTicketID == 0 && num3 == num && num3 != 0 && (ServerTime.pCurrentTime - mLevelManager.pLastPlayedTime).TotalMinutes < (double)mLevelManager._LevelUnlockTimeInMinutes)
					{
						KAWidget kAWidget4 = kAWidget.FindChildItem("LockedIconGems");
						if (kAWidget4 != null)
						{
							kAWidget4.SetVisibility(inVisible: true);
						}
						string widgetName2 = "AniGrade" + text2;
						KAWidget kAWidget5 = kAWidget.FindChildItem(widgetName2);
						if (kAWidget5 != null)
						{
							kAWidget5.SetVisibility(inVisible: false);
						}
					}
					kAWidget.SetVisibility(inVisible: true);
					AddWidget(kAWidget);
					num3++;
				}
			}
			num2++;
		}
	}

	private int GetCurrentThemeID()
	{
		int result = 0;
		if (mLevelManager == null)
		{
			return result;
		}
		for (int i = 0; i < SanctuaryData.pInstance._HeroDragonData.Length; i++)
		{
			if (mLevelManager._DragonSelectionUi.pSelectedTicketID == SanctuaryData.pInstance._HeroDragonData[i]._ItemID)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	private void PopulateHeroLevels()
	{
		if (GetCurrentThemeID() == 0)
		{
			return;
		}
		DisplayHeroDragonFeature();
		int num = 1;
		if (mLevelManager != null)
		{
			num = mLevelManager.pLastUnlockedHeroLevel;
		}
		LevelGroupData[] array = null;
		SceneLevelGroupData[] array2 = null;
		switch (mLevelManager.pGameMode)
		{
		case FSGameMode.FLIGHT_MODE:
			array2 = mLevelManager._AdultData;
			break;
		case FSGameMode.GLIDE_MODE:
			array2 = mLevelManager._TeenData;
			break;
		case FSGameMode.FLIGHT_SUIT_MODE:
			array2 = mLevelManager._FlightSuitData;
			break;
		}
		int num2 = 0;
		SceneLevelGroupData[] array3 = array2;
		foreach (SceneLevelGroupData sceneLevelGroupData in array3)
		{
			array = sceneLevelGroupData._GroupData;
			int num3 = 0;
			bool flag = false;
			HeroPetData heroDragonFromID = SanctuaryData.GetHeroDragonFromID(mLevelManager._DragonSelectionUi.pSelectedTicketID);
			for (int j = 0; j < sceneLevelGroupData._DragonName.Length; j++)
			{
				if (sceneLevelGroupData._DragonName[j] == heroDragonFromID._Name)
				{
					flag = true;
				}
			}
			if (flag)
			{
				LevelGroupData[] array4 = array;
				foreach (LevelGroupData levelGroupData in array4)
				{
					KAWidget kAWidget = DuplicateWidget(_Template);
					kAWidget.name = "Level " + (num3 + 1);
					kAWidget.SetTexture(levelGroupData._Icon);
					kAWidget.FindChildItem("TxtLevelName").SetText(levelGroupData._LevelNameText.GetLocalizedString());
					int playerHighScore = mLevelManager.GetPlayerHighScore(num3, isHeroDragon: true);
					KAUIFlightSchoolItemData userData = new KAUIFlightSchoolItemData(num2, num3, playerHighScore);
					kAWidget.SetUserData(userData);
					string text = null;
					string text2 = null;
					GradeSystem[] fSGrades = levelGroupData._FSGrades;
					for (int l = 0; l < fSGrades.Length; l++)
					{
						if (playerHighScore >= fSGrades[l]._PointNeeded)
						{
							text = fSGrades[l]._Grade;
							text2 = fSGrades[l]._BGColor;
							break;
						}
						text = fSGrades[^1]._Grade;
						text2 = fSGrades[^1]._BGColor;
					}
					if (num3 > num)
					{
						KAWidget kAWidget2 = kAWidget.FindChildItem("LockedIcon");
						if (kAWidget2 != null)
						{
							kAWidget2.SetVisibility(inVisible: true);
						}
						kAWidget.SetDisabled(isDisabled: true);
					}
					if (num3 <= num && playerHighScore != 0)
					{
						string widgetName = "AniGrade" + text2;
						KAWidget kAWidget3 = kAWidget.FindChildItem(widgetName);
						if (kAWidget3 != null)
						{
							kAWidget3.SetVisibility(inVisible: true);
							if (text != null)
							{
								kAWidget3.SetText(text);
							}
						}
					}
					kAWidget.SetVisibility(inVisible: true);
					AddWidget(kAWidget);
					num3++;
				}
			}
			num2++;
		}
	}

	private void DisplayHeroDragonFeature()
	{
		int pSelectedTicketID = mLevelManager._DragonSelectionUi.pSelectedTicketID;
		HeroPetData heroDragonFromID = SanctuaryData.GetHeroDragonFromID(pSelectedTicketID);
		if (heroDragonFromID == null)
		{
			return;
		}
		KAWidget kAWidget = _DrgonPropParent.FindChildItem("TxtDragonName");
		if (kAWidget != null)
		{
			kAWidget.SetText(heroDragonFromID._Name);
		}
		kAWidget = _DrgonPropParent.FindChildItem("TxtDragonType");
		if (kAWidget != null)
		{
			kAWidget.SetText(heroDragonFromID._DragonType);
		}
		kAWidget = _DrgonPropParent.FindChildItem("TxtStrengthVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(heroDragonFromID._Strength);
		}
		kAWidget = _DrgonPropParent.FindChildItem("TxtSpeedVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(heroDragonFromID._Speed);
		}
		kAWidget = _DrgonPropParent.FindChildItem("TxtEnduranceVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(heroDragonFromID._Endurance);
		}
		kAWidget = _DrgonPropParent.FindChildItem("TxtArmorVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(heroDragonFromID._Armor);
		}
		kAWidget = _DrgonPropParent.FindChildItem("TxtFireVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(heroDragonFromID._Fire);
		}
		kAWidget = _DrgonPropParent.FindChildItem("TxtShotLimitVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(heroDragonFromID._ShotLimit);
		}
		kAWidget = _DrgonPropParent.FindChildItem("TxtWingSpanVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(heroDragonFromID._WingSpan);
		}
		kAWidget = _DrgonPropParent.FindChildItem("TxtLengthVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(heroDragonFromID._Length);
		}
		kAWidget = _DrgonPropParent.FindChildItem("TxtWeightVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(heroDragonFromID._Weight);
		}
		kAWidget = _DrgonPropParent.FindChildItem("IcoDragonFlightTexture");
		if (kAWidget != null)
		{
			kAWidget.pBackground.UpdateSprite(heroDragonFromID._DragonSpriteName);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = _DrgonPropParent.FindChildItem("IcoDragonClass");
		if (kAWidget != null)
		{
			DragonClassInfo dragonClassInfo = SanctuaryData.GetDragonClassInfo(heroDragonFromID._DragonClass);
			kAWidget.pBackground.UpdateSprite(dragonClassInfo._IconSprite);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = _DrgonPropParent.FindChildItem("IcoCompletionStar");
		if (kAWidget != null)
		{
			bool visibility = false;
			if (mLevelManager.pFlightModeLevelUnlockingDataMap.ContainsKey(pSelectedTicketID))
			{
				visibility = mLevelManager.pFlightModeLevelUnlockingDataMap[pSelectedTicketID].IsAllHeroLevelsPlayed() && mLevelManager.pFlightModeLevelUnlockingDataMap[pSelectedTicketID].IsAllLevelsPlayed();
			}
			kAWidget.SetVisibility(visibility);
		}
		if (heroDragonFromID._Tip == null || heroDragonFromID._Tip.Length == 0)
		{
			return;
		}
		for (int i = 0; i < heroDragonFromID._Tip.Length; i++)
		{
			kAWidget = _DrgonPropParent.FindChildItem("Tip" + i);
			if (kAWidget != null && !string.IsNullOrEmpty(heroDragonFromID._Tip[i].GetLocalizedString()))
			{
				kAWidget.SetText(heroDragonFromID._Tip[i].GetLocalizedString());
			}
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		KAUIFlightSchoolItemData kAUIFlightSchoolItemData = (KAUIFlightSchoolItemData)item.GetUserData();
		int levelID = kAUIFlightSchoolItemData._LevelID;
		mLevelManager.mCurrentLevelHighScore = kAUIFlightSchoolItemData._PreviousHighScore;
		TutorialManager.StopTutorials();
		mLevelManager.mIsCurrentLevelSpecial = false;
		if (mLevelManager.pGameMode == FSGameMode.FLIGHT_SUIT_MODE || SanctuaryManager.pCurPetInstance.IsActionAllowed(PetActions.FLIGHTSCHOOL))
		{
			if (mLevelManager._FlightSchoolIntroTut != null && !mLevelManager._FlightSchoolIntroTut.TutorialComplete())
			{
				mLevelManager._FlightSchoolIntroTut.SetTutorialBoardVisible(flag: false);
			}
			if (levelID > mLastPlayedLevelNum)
			{
				ShowLockedItemClickDB();
				return;
			}
			mLevelManager.pLevelMenu.mCurrentSceneId = kAUIFlightSchoolItemData._SceneID;
			_CurLevelGroupIdx = levelID;
			string text = null;
			switch (mLevelManager.pGameMode)
			{
			case FSGameMode.FLIGHT_MODE:
				text = mLevelManager._AdultData[mLevelManager.pLevelMenu.mCurrentSceneId]._SceneName;
				break;
			case FSGameMode.GLIDE_MODE:
				text = mLevelManager._TeenData[mLevelManager.pLevelMenu.mCurrentSceneId]._SceneName;
				break;
			case FSGameMode.FLIGHT_SUIT_MODE:
				text = mLevelManager._FlightSuitData[mLevelManager.pLevelMenu.mCurrentSceneId]._SceneName;
				break;
			}
			if (mLevelManager.pGameMode == FSGameMode.FLIGHT_SUIT_MODE)
			{
				SanctuaryManager.pCurPetInstance.OnFlyDismountImmediate(AvAvatar.pObject);
				SanctuaryManager.pInstance.DestoryCurrentPet();
			}
			if (mLevelManager.pLevelMenu.mCurrentThemeName != text)
			{
				GameObject gameObject = GameObject.Find("ThemeRoot");
				if (gameObject != null)
				{
					UnityEngine.Object.Destroy(gameObject);
				}
				mLevelManager.pLevelMenu.mCurrentThemeName = text;
				mLevelManager._AvatarCamera.SetLookAt(AvAvatar.mTransform, null, 0f);
				if (SanctuaryManager.pCurPetInstance != null)
				{
					SanctuaryManager.pCurPetInstance.SetMoodParticleIgnore(mLevelManager._DisableMoodParticle);
				}
				RsResourceManager.LoadAdditive("RS_SCENE/" + mLevelManager.pLevelMenu.mCurrentThemeName, OnAdditiveLevelLoadingEvent, null);
				UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
				SetInteractive(interactive: false);
				mIsLevelReady = false;
				mLevelManager.pCurrentLevel = levelID;
				return;
			}
			GameObject obj = GameObject.Find("!PfOCLevelGroups");
			Transform transform = null;
			switch (mLevelManager.pGameMode)
			{
			case FSGameMode.FLIGHT_MODE:
				transform = UtUtilities.FindChildTransform(obj, mLevelManager.pLevelMenu.mLevelManager._AdultData[mLevelManager.pLevelMenu.mCurrentSceneId]._GroupData[_CurLevelGroupIdx]._GroupName, inactive: true);
				break;
			case FSGameMode.GLIDE_MODE:
				transform = UtUtilities.FindChildTransform(obj, mLevelManager.pLevelMenu.mLevelManager._TeenData[mLevelManager.pLevelMenu.mCurrentSceneId]._GroupData[_CurLevelGroupIdx]._GroupName, inactive: true);
				break;
			case FSGameMode.FLIGHT_SUIT_MODE:
				transform = UtUtilities.FindChildTransform(obj, mLevelManager.pLevelMenu.mLevelManager._FlightSuitData[mLevelManager.pLevelMenu.mCurrentSceneId]._GroupData[_CurLevelGroupIdx]._GroupName, inactive: true);
				break;
			}
			LoadLevelGroup(transform.gameObject);
			mIsLevelReady = true;
			KAUICursorManager.SetDefaultCursor();
		}
		else
		{
			UiPetEnergyGenericDB.Show(base.gameObject, null, null, isLowEnergy: true);
		}
	}

	private bool CanShowNotEnoughGemsDB(KAWidget inItem)
	{
		if (inItem == null)
		{
			return false;
		}
		if (inItem.pUI != null && inItem.pUI.GetComponent<UiObstacleCourseMenu>() != null && mLevelManager.pLastUnlockedLevel >= mLevelManager._InitialNonMemberUnlockedLevel)
		{
			return mLevelManager._DragonSelectionUi.pSelectedTicketID == 0;
		}
		return false;
	}

	private void ShowLockedItemClickDB()
	{
		KAUIGenericDB kAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
		kAUIGenericDB.SetText(_LockedItemClickText.GetLocalizedString(), interactive: false);
		kAUIGenericDB._MessageObject = base.gameObject;
		kAUIGenericDB._OKMessage = "DestroyMessageDB";
		kAUIGenericDB.SetDestroyOnClick(isDestroy: true);
		kAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		KAUI.SetExclusive(kAUIGenericDB, new Color(0.5f, 0.5f, 0.5f, 0.5f));
	}

	public void OnAdditiveLevelLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			SceneManager.LoadScene(mLevelManager.pLevelMenu.mCurrentThemeName, LoadSceneMode.Additive);
			SceneManager.sceneLoaded += OnSceneLoaded;
			break;
		case RsResourceLoadEvent.PROGRESS:
		case RsResourceLoadEvent.ERROR:
			break;
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		GameObject obj = GameObject.Find("!PfOCLevelGroups");
		Transform transform = null;
		switch (mLevelManager.pGameMode)
		{
		case FSGameMode.FLIGHT_MODE:
			transform = UtUtilities.FindChildTransform(obj, mLevelManager._AdultData[mLevelManager.pLevelMenu.mCurrentSceneId]._GroupData[_CurLevelGroupIdx]._GroupName, inactive: true);
			break;
		case FSGameMode.GLIDE_MODE:
			transform = UtUtilities.FindChildTransform(obj, mLevelManager._TeenData[mLevelManager.pLevelMenu.mCurrentSceneId]._GroupData[_CurLevelGroupIdx]._GroupName, inactive: true);
			break;
		case FSGameMode.FLIGHT_SUIT_MODE:
			transform = UtUtilities.FindChildTransform(obj, mLevelManager._FlightSuitData[mLevelManager.pLevelMenu.mCurrentSceneId]._GroupData[_CurLevelGroupIdx]._GroupName, inactive: true);
			break;
		}
		LoadLevelGroup(transform.gameObject);
		GameDataConfig.OptimizeTerrain(QualitySettings.GetQualityLevel());
		mIsLevelReady = true;
		KAUICursorManager.SetDefaultCursor();
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	public void DisplayTradeinUI(string inOkMsg, string inNoMsg)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDB"));
		mKAUIGenericDB = gameObject.GetComponent<KAUIGenericDB>();
		mKAUIGenericDB._MessageObject = base.gameObject;
		KAUI.SetExclusive(mKAUIGenericDB, new Color(0.5f, 0.5f, 0.5f, 0.5f));
		if (Money.pCashCurrency >= mLevelManager.pLevelUnLockCost)
		{
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
			string localizedString = _EntranceFeeText.GetLocalizedString();
			localizedString = localizedString.Replace("{{GEMS}}", mLevelManager.pLevelUnLockCost.ToString());
			TimeSpan timeSpan = ServerTime.pCurrentTime - mLevelManager.pLastPlayedTime;
			int num = (int)((double)mLevelManager._LevelUnlockTimeInMinutes - timeSpan.TotalMinutes);
			int num2 = num / 60;
			num %= 60;
			if (num == 0)
			{
				num = 1;
			}
			localizedString = localizedString.Replace("{{LOCKTIME_HOUR}}", num2.ToString());
			localizedString = localizedString.Replace("{{LOCKTIME_MINUTE}}", num.ToString());
			mKAUIGenericDB.SetText(localizedString, interactive: false);
			mKAUIGenericDB._YesMessage = inOkMsg;
			mKAUIGenericDB._NoMessage = inNoMsg;
		}
		else
		{
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
			mKAUIGenericDB.SetText(_NotEnoughFeeText.GetLocalizedString(), interactive: false);
			mKAUIGenericDB._YesMessage = "BuyGemsOnline";
			mKAUIGenericDB._NoMessage = "KillGenericDB";
		}
	}

	private void NotEnoughGems()
	{
		KillGenericDB();
	}

	private void PayGemsAndUnlockLevel()
	{
		PurchaseLevel();
	}

	private void PurchaseLevel()
	{
		CommonInventoryData.pInstance.AddPurchaseItem(mLevelManager._LevelUnlockItemID, 1, ItemPurchaseSource.FLIGHT_SCHOOL.ToString());
		CommonInventoryData.pInstance.DoPurchase(2, mLevelManager._StoreID, LevelPurchaseDone);
		KillGenericDB();
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: false);
		mKAUIGenericDB.SetText(_LevelPurchaseProcessingText.GetLocalizedString(), interactive: false);
		KAUI.SetExclusive(mKAUIGenericDB);
	}

	public void LevelPurchaseDone(CommonInventoryResponse ret)
	{
		KillGenericDB();
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		if (ret != null && ret.Success)
		{
			mKAUIGenericDB.SetText(_LevelPurchaseSuccessfulText.GetLocalizedString(), interactive: false);
			KAWidget kAWidget = FindItem("Level " + (mLevelManager.pLastUnlockedLevel + 1));
			if (kAWidget != null)
			{
				KAWidget kAWidget2 = kAWidget.FindChildItem("LockedIconGems");
				if (kAWidget2 != null)
				{
					kAWidget2.SetVisibility(inVisible: false);
				}
			}
			mLevelManager.SaveGemUnlock();
		}
		else
		{
			mKAUIGenericDB.SetText(_LevelPurchaseFailedText.GetLocalizedString(), interactive: false);
		}
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._OKMessage = "KillGenericDB";
		KAUI.SetExclusive(mKAUIGenericDB);
	}

	private void KillGenericDB()
	{
		if (!(mKAUIGenericDB == null))
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			UnityEngine.Object.DestroyImmediate(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}
}
