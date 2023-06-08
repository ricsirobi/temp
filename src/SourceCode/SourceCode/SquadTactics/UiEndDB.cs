using System;
using System.Collections.Generic;
using System.Linq;
using SOD.Event;
using UnityEngine;

namespace SquadTactics;

public class UiEndDB : KAUI
{
	public enum QuestionDataIndex
	{
		MAPPING,
		ID,
		CATEGORY,
		SUB,
		QUESTION,
		ANSWERS,
		CORRECT,
		TYPE,
		URL
	}

	[Serializable]
	public class STAchievementInfo
	{
		[Tooltip("Achievement Container Name")]
		public string _AchievementBoxName;

		public int _AchievementID;

		public float _AppearanceChance;
	}

	public class ResultInfo
	{
		public LevelRewardType _RewardType;

		public int _UnlockedCreditChests;

		public int _LockedChests;

		public List<string> _MissedObjectives;
	}

	public UiRewards _UiRewards;

	public UiChestMenu _UiChestMenu;

	public LocaleString _LevelCompletedText = new LocaleString("[REVIEW] Level Completed");

	public LocaleString _LevelLostText = new LocaleString("[REVIEW] Level Lost");

	public LocaleString _GameWonText = new LocaleString("You Won");

	public LocaleString _GameLostText = new LocaleString("You Lost");

	public LocaleString _UnopenedChestsWarningText = new LocaleString("You have unopened chests \n Do you wish to discard these rewards ?");

	public LocaleString _UnopenedChestWarningText = new LocaleString("You have an unopened chest \n Do you wish to discard the reward ?");

	public LocaleString _UnclaimedItemsWarningText = new LocaleString("[REVIEW] - You have unclaimed items \n Do you wish to dismantle ?");

	public LocaleString _UnclaimedChestAndItemsText = new LocaleString("[REVIEW] Discard unopened chests and dismantle unclaimed items?");

	public float _RewardTransitionTime = 0.5f;

	public int _CreditRedeemItemFetchCount = 10;

	public string _QuizFilePath = "RS_DATA/STQuizDataDO.txt";

	public string _TriviaQuizFilePath = "RS_DATA/STTriviaQuizDataDO.txt";

	public string _MysteryBoxBundlePath = "RS_DATA/PfUiMysteryBoxDO.unity3d/PfUiMysteryBoxDO";

	public string _GenericDBName = "PfUiSTGenericDB";

	public AudioClip _RewardChestOpenSFX;

	public AudioClip _CreditChestOpenSFX;

	private UiBattleBackpack mBattleBackPack;

	private UiRewardScreen mUiRewardScreen;

	private UiBattleBackpackMenu mBattleBackPackMenu;

	private UiRewardMenu mRewardMenu;

	private string mDefaultQuestionKey = string.Empty;

	private static Dictionary<string, List<QuizQuestion>> mQuestionMap = new Dictionary<string, List<QuizQuestion>>();

	private static Dictionary<string, List<QuizQuestion>> mTriviaQuestionMap = new Dictionary<string, List<QuizQuestion>>();

	private bool mShowTriviaQuestion = true;

	private KAWidget mGemsTotal;

	private KAWidget mCoinsTotal;

	private KAWidget mContinueBtn;

	private KAWidget mRestartBtn;

	private KAWidget mPlayAgainBtn;

	private KAWidget mLevelStatus;

	private Vector3 mOriginalUiRewardPositon;

	private List<AchievementReward> mRewards;

	private List<AchievementReward> mDisplayRewards;

	private static QuizData mSTQuizData = null;

	private static QuizData mSTTriviaQuizData = null;

	private Dictionary<QuestionDataIndex, int> mQuizIndexInfo = new Dictionary<QuestionDataIndex, int>();

	private Dictionary<QuestionDataIndex, int> mTriviaQuizIndexInfo = new Dictionary<QuestionDataIndex, int>();

	private string mExitAction;

	private KAWidget mItemToSell;

	private List<UserItemStatsMap> pItemStatsMap;

	private UiItemSellInfoDB mUiItemSellInfoDB;

	private KAUIGenericDB mObjectivesEndDB;

	private ResultInfo mResultInfo;

	private UiResultXP mResultsXpDB;

	private bool mIsExtraChestOpened;

	private bool mEnableStatsDB;

	private UIEventRewardAchievement mUIEventRewardAchievement;

	public UiStatCompareDB pStatDB { get; set; }

	public static QuizData pSTQuizData => mSTQuizData;

	public static QuizData pSTTriviaQuizData => mSTTriviaQuizData;

	protected override void Start()
	{
		base.Start();
		pItemStatsMap = new List<UserItemStatsMap>();
		mBattleBackPack = (UiBattleBackpack)_UiList[0];
		mUiRewardScreen = (UiRewardScreen)_UiList[1];
		pStatDB = (UiStatCompareDB)_UiList[2];
		mUiItemSellInfoDB = (UiItemSellInfoDB)_UiList[3];
		mObjectivesEndDB = (KAUIGenericDB)_UiList[4];
		mResultsXpDB = (UiResultXP)_UiList[5];
		mUIEventRewardAchievement = base.gameObject.GetComponentInChildren<UIEventRewardAchievement>();
		if (mSTQuizData == null)
		{
			RsResourceManager.Load(_QuizFilePath, OnQuizQuestionsLoaded);
		}
		if (mSTTriviaQuizData == null)
		{
			RsResourceManager.Load(_TriviaQuizFilePath, OnTriviaQuizQuestionsLoaded);
		}
		mOriginalUiRewardPositon = _UiRewards.GetPosition();
		mPlayAgainBtn = FindItem("PlayAgainBtn");
		mContinueBtn = FindItem("ContinueBtn");
		mRestartBtn = FindItem("RestartBtn");
		mLevelStatus = FindItem("STLevelState");
		mGemsTotal = FindItem("GemsTotal");
		mCoinsTotal = FindItem("CoinsTotal");
		Money.AddNotificationObject(base.gameObject);
		mBattleBackPackMenu = (UiBattleBackpackMenu)mBattleBackPack.GetMenuByIndex(1);
		mRewardMenu = (UiRewardMenu)mUiRewardScreen.GetMenuByIndex(0);
		mBattleBackPackMenu.pTargetMenu = mRewardMenu;
		mBattleBackPack.pMessageObject = base.gameObject;
		mUiRewardScreen.pMessageObject = base.gameObject;
	}

	private void PopulateQuestions(string text, QuizData quizData, Dictionary<QuestionDataIndex, int> indexInfo)
	{
		List<QuizQuestion> list = new List<QuizQuestion>();
		QuizQuestion quizQuestion = null;
		List<QuizAnswer> list2 = new List<QuizAnswer>();
		CSVFileReader cSVFileReader = new CSVFileReader(text);
		if (cSVFileReader == null)
		{
			Debug.LogError("Failed to create csv reader");
		}
		bool flag = true;
		while (flag)
		{
			List<string> list3 = new List<string>();
			flag = cSVFileReader.ReadRow(list3);
			if (flag)
			{
				if (list3.Count <= 0)
				{
					continue;
				}
				if (list3[0].ToUpper().Contains(QuestionDataIndex.MAPPING.ToString()))
				{
					if (quizData.TypeToPrefabMap == null)
					{
						quizData.TypeToPrefabMap = new Dictionary<string, string>();
					}
					quizData.TypeToPrefabMap[list3[1]] = list3[2];
					continue;
				}
				if (list3[0].ToUpper().Contains(QuestionDataIndex.ID.ToString()))
				{
					GetIndexes(list3, indexInfo);
					continue;
				}
				if (indexInfo.ContainsKey(QuestionDataIndex.ID) && !string.IsNullOrEmpty(list3[indexInfo[QuestionDataIndex.ID]]) && ((indexInfo.ContainsKey(QuestionDataIndex.QUESTION) && !string.IsNullOrEmpty(list3[indexInfo[QuestionDataIndex.QUESTION]])) || (indexInfo.ContainsKey(QuestionDataIndex.URL) && !string.IsNullOrEmpty(list3[indexInfo[QuestionDataIndex.URL]]))))
				{
					quizQuestion = new QuizQuestion();
					quizQuestion.ID = list3[indexInfo[QuestionDataIndex.ID]];
					if (indexInfo.ContainsKey(QuestionDataIndex.SUB))
					{
						quizQuestion.Category = list3[indexInfo[QuestionDataIndex.SUB]].Split(',');
					}
					else if (indexInfo.ContainsKey(QuestionDataIndex.CATEGORY))
					{
						quizQuestion.Category = list3[indexInfo[QuestionDataIndex.CATEGORY]].Split(',');
					}
					if (indexInfo.ContainsKey(QuestionDataIndex.QUESTION))
					{
						quizQuestion.QuestionText = new LocaleString(list3[indexInfo[QuestionDataIndex.QUESTION]]);
					}
					if (indexInfo.ContainsKey(QuestionDataIndex.TYPE))
					{
						quizQuestion.Type = list3[indexInfo[QuestionDataIndex.TYPE]];
					}
					if (string.IsNullOrEmpty(quizQuestion.Type))
					{
						quizQuestion.Type = "Text";
					}
					if (indexInfo.ContainsKey(QuestionDataIndex.URL))
					{
						quizQuestion.ImageURL = list3[indexInfo[QuestionDataIndex.URL]];
					}
				}
				if (quizQuestion == null)
				{
					continue;
				}
				if ((indexInfo.ContainsKey(QuestionDataIndex.ANSWERS) && !string.IsNullOrEmpty(list3[indexInfo[QuestionDataIndex.ANSWERS]])) || (indexInfo.ContainsKey(QuestionDataIndex.URL) && !string.IsNullOrEmpty(list3[indexInfo[QuestionDataIndex.URL]])))
				{
					QuizAnswer quizAnswer = new QuizAnswer();
					if (indexInfo.ContainsKey(QuestionDataIndex.ANSWERS))
					{
						quizAnswer.AnswerText = new LocaleString(list3[indexInfo[QuestionDataIndex.ANSWERS]]);
					}
					if (indexInfo.ContainsKey(QuestionDataIndex.CORRECT) && !string.IsNullOrEmpty(list3[indexInfo[QuestionDataIndex.CORRECT]]))
					{
						quizAnswer.IsCorrect = true;
					}
					if (indexInfo.ContainsKey(QuestionDataIndex.URL))
					{
						quizAnswer.ImageURL = list3[indexInfo[QuestionDataIndex.URL]];
					}
					list2.Add(quizAnswer);
				}
				else if (list2 != null && list2.Count > 0)
				{
					quizQuestion.Answers = list2.ToArray();
					list.Add(quizQuestion);
					quizQuestion = null;
					list2.Clear();
				}
			}
			else if (quizQuestion != null && list2 != null && list2.Count > 0)
			{
				quizQuestion.Answers = list2.ToArray();
				list.Add(quizQuestion);
				quizQuestion = null;
				list2.Clear();
			}
		}
		cSVFileReader.Dispose();
		quizData.Questions = list.ToArray();
		list.Clear();
		indexInfo.Clear();
	}

	private void GetIndexes(List<string> indexLine, Dictionary<QuestionDataIndex, int> indexInfo)
	{
		for (int i = 0; i < indexLine.Count; i++)
		{
			string text = indexLine[i];
			text = text.ToUpper();
			foreach (QuestionDataIndex value in Enum.GetValues(typeof(QuestionDataIndex)))
			{
				if (text.Contains(value.ToString()))
				{
					indexInfo[value] = i;
				}
			}
		}
	}

	private void OnQuizQuestionsLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inFile, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			mSTQuizData = new QuizData();
			PopulateQuestions((string)inFile, mSTQuizData, mQuizIndexInfo);
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.Log("Failed to Load Quiz questions");
			break;
		}
	}

	private void OnTriviaQuizQuestionsLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inFile, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			mSTTriviaQuizData = new QuizData();
			PopulateQuestions((string)inFile, mSTTriviaQuizData, mTriviaQuizIndexInfo);
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.Log("Failed to Load Trivia Quiz questions");
			break;
		}
	}

	public void PopulateQuestionMap(QuizData quizData, Dictionary<string, List<QuizQuestion>> questionMap)
	{
		if (quizData == null || quizData.Questions == null || quizData.Questions.Length == 0)
		{
			return;
		}
		QuizQuestion[] questions = quizData.Questions;
		foreach (QuizQuestion quizQuestion in questions)
		{
			if (quizQuestion.Category == null)
			{
				if (!questionMap.ContainsKey(mDefaultQuestionKey))
				{
					questionMap[mDefaultQuestionKey] = new List<QuizQuestion>();
				}
				questionMap[mDefaultQuestionKey].Add(quizQuestion);
				continue;
			}
			string[] category = quizQuestion.Category;
			foreach (string key in category)
			{
				if (!questionMap.ContainsKey(key))
				{
					questionMap[key] = new List<QuizQuestion>();
				}
				questionMap[key].Add(quizQuestion);
			}
		}
	}

	private QuizQuestion GetQuestionFromPool(Dictionary<string, List<QuizQuestion>> questionMap, string selctedCategory, bool isTrivia = false)
	{
		List<QuizQuestion> list = new List<QuizQuestion>();
		if (!string.IsNullOrEmpty(selctedCategory) && questionMap.ContainsKey(selctedCategory))
		{
			list.AddRange(questionMap[selctedCategory]);
		}
		else
		{
			foreach (KeyValuePair<string, List<QuizQuestion>> item in questionMap)
			{
				list.AddRange(item.Value);
			}
		}
		QuizQuestion quizQuestion = list[UnityEngine.Random.Range(0, list.Count)];
		if (quizQuestion.Category == null)
		{
			if (questionMap.ContainsKey(mDefaultQuestionKey))
			{
				questionMap[mDefaultQuestionKey].Remove(quizQuestion);
				if (questionMap[mDefaultQuestionKey].Count == 0)
				{
					questionMap.Remove(mDefaultQuestionKey);
				}
			}
		}
		else
		{
			string[] category = quizQuestion.Category;
			foreach (string key in category)
			{
				questionMap[key].Remove(quizQuestion);
				if (questionMap[key].Count == 0)
				{
					questionMap.Remove(key);
				}
			}
		}
		list.Clear();
		return quizQuestion;
	}

	private void OnSelectBattleBackpackItem(KAWidget inWidget)
	{
		if (!(inWidget != null) || !mEnableStatsDB)
		{
			return;
		}
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)inWidget.GetUserData();
		if (kAUISelectItemData._ItemData != null && kAUISelectItemData._ItemData.AssetName != null)
		{
			bool showDBRight = kAUISelectItemData._Menu == mBattleBackPackMenu;
			bool showDiscardBtn = false;
			if (!kAUISelectItemData._Disabled)
			{
				showDiscardBtn = kAUISelectItemData._Menu == mBattleBackPackMenu;
			}
			pStatDB.OpenStatsDB(inWidget, pStatDB._EquippedSlots, base.gameObject, showDBRight, showEquip: false, CanUnequip: false, showSellBtn: true, showDiscardBtn);
		}
	}

	private ItemStat[] GetItemStatsFromUserItemData(UserItemData userItemData)
	{
		return userItemData?.ItemStats;
	}

	private ItemTier? GetItemTierFromUserItemData(UserItemData userItemData)
	{
		return userItemData?.ItemTier;
	}

	private void OnDeselectItem()
	{
		pStatDB.SetVisibility(inVisible: false);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mContinueBtn)
		{
			int rewardItemCount = GetRewardItemCount();
			if (mResultInfo._UnlockedCreditChests > 0 || rewardItemCount > 0)
			{
				ShowCloseConfirmationDB("ShowMainMenu", rewardItemCount);
				return;
			}
			mExitAction = "ShowMainMenu";
			ProcessReward();
		}
		else if (inWidget == mRestartBtn)
		{
			if (mResultInfo._UnlockedCreditChests > 0)
			{
				ShowCloseConfirmationDB("RestartLevel");
			}
			else
			{
				RestartLevel();
			}
		}
		else if (inWidget == mPlayAgainBtn)
		{
			if (mResultInfo._UnlockedCreditChests > 0)
			{
				ShowCloseConfirmationDB("PlayAgain");
			}
			else
			{
				PlayAgain();
			}
		}
	}

	public void ShowGameResult(ResultInfo resultInfo)
	{
		if (resultInfo == null)
		{
			UtDebug.LogError("Result data is should not be null");
			return;
		}
		mResultInfo = resultInfo;
		if (mObjectivesEndDB != null)
		{
			mObjectivesEndDB._MessageObject = base.gameObject;
			mObjectivesEndDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			mObjectivesEndDB._OKMessage = "OkButtonPressed";
			((UIObjectiveTreeView)mObjectivesEndDB._MenuList[0])?.Populate();
			mObjectivesEndDB.SetTitle((mResultInfo._RewardType == LevelRewardType.LevelCompletion) ? _GameWonText._Text : _GameLostText._Text);
			List<AchievementTask> list = new List<AchievementTask>();
			list.Add(new AchievementTask(LevelManager.pInstance._PlayDTAchievementID));
			if (mResultInfo._RewardType == LevelRewardType.LevelCompletion)
			{
				list.Add(new AchievementTask(LevelManager.pInstance._WinDTAchievementID));
			}
			else
			{
				list.Add(new AchievementTask(LevelManager.pInstance._LoseDTAchievementID));
			}
			UserAchievementTask.Set(list.ToArray());
			mObjectivesEndDB.transform.parent = null;
			mObjectivesEndDB.OnParentSetVisiblity(inVisible: true);
			KAUI.SetExclusive(mObjectivesEndDB);
			mObjectivesEndDB.SetVisibility(inVisible: true);
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
	}

	public void UpdateObjectivesDisplay()
	{
		List<Objective> list = new List<Objective>();
		List<Objective> list2 = new List<Objective>();
		list.AddRange(GameManager.pInstance.GetFinalObjectives().FindAll((Objective x) => x._IsMandatory));
		list2.AddRange(GameManager.pInstance.GetFinalObjectives().FindAll((Objective x) => !x._IsMandatory));
		KAUIMenu kAUIMenu = mObjectivesEndDB._MenuList[0];
		foreach (Objective item in list)
		{
			KAWidget kAWidget = kAUIMenu.AddWidget("Mandatory", new ObjectiveData(item));
			kAWidget.SetVisibility(inVisible: true);
			UpdateObjectiveStatus(item, kAWidget);
		}
		if (list2.Count <= 0)
		{
			return;
		}
		KAUIMenu kAUIMenu2 = mObjectivesEndDB._MenuList[1];
		foreach (Objective item2 in list2)
		{
			KAWidget kAWidget2 = kAUIMenu2.AddWidget("Optional", new ObjectiveData(item2));
			kAWidget2.SetVisibility(inVisible: true);
			UpdateObjectiveStatus(item2, kAWidget2);
		}
	}

	private void UpdateObjectiveStatus(Objective inObjective, KAWidget inWidget)
	{
		inWidget.FindChildItem("FailedSprite").SetVisibility(inObjective.pObjectiveStatus != ObjectiveStatus.COMPLETED);
		inWidget.FindChildItem("CompletedSprite").SetVisibility(inObjective.pObjectiveStatus == ObjectiveStatus.COMPLETED);
		KAWidget kAWidget = inWidget.FindChildItem("TxtName");
		string finalObjectiveStr = GameManager.pInstance.GetSceneData().GetFinalObjectiveStr(inObjective);
		kAWidget.SetText(finalObjectiveStr);
		kAWidget.GetLabel().color = ((inObjective.pObjectiveStatus == ObjectiveStatus.COMPLETED) ? Color.green : Color.red);
	}

	private void OkButtonPressed()
	{
		KAUI.RemoveExclusive(mObjectivesEndDB);
		mObjectivesEndDB.SetVisibility(inVisible: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		if (mUIEventRewardAchievement != null)
		{
			EventRewardType rewardType = ((mResultInfo._RewardType == LevelRewardType.LevelCompletion) ? EventRewardType.GameWin : EventRewardType.PlayedLevel);
			mUIEventRewardAchievement.SetReward(GameManager.pInstance._GameID, RsResourceManager.pCurrentLevel, rewardType, base.gameObject);
		}
		else
		{
			AchievementRewardProcessed();
		}
	}

	private void AchievementRewardProcessed()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		if (mResultsXpDB != null)
		{
			mResultsXpDB.InitUI(this);
		}
		ApplyLevelRewards();
	}

	public void SetRewards()
	{
		mRestartBtn.SetVisibility(inVisible: false);
		mPlayAgainBtn.SetVisibility(inVisible: false);
		mLevelStatus.SetText((mResultInfo._RewardType == LevelRewardType.LevelCompletion) ? _LevelCompletedText._Text : _LevelLostText._Text);
		UiChest uiChest = (UiChest)_UiChestMenu.DuplicateWidget(_UiChestMenu._RewardChestTemplate);
		_UiChestMenu.AddWidget(uiChest);
		uiChest.pIsRewardChest = true;
		uiChest.InitChest();
		if (_RewardChestOpenSFX != null)
		{
			SnChannel.Play(_RewardChestOpenSFX, "STSFX_Pool", inForce: true);
		}
		_UiRewards.SetPosition(uiChest.transform.position.x, uiChest.transform.position.y);
		for (int i = 0; i < mResultInfo._UnlockedCreditChests; i++)
		{
			KAWidget kAWidget = _UiChestMenu.DuplicateWidget(_UiChestMenu._QuizChestTemplate);
			UiChest uiChest2 = (UiChest)kAWidget.FindChildItem("BtnChest");
			_UiChestMenu.AddWidget(kAWidget);
			uiChest2.InitChest();
			if (mQuestionMap.Keys.Count == 0)
			{
				PopulateQuestionMap(mSTQuizData, mQuestionMap);
			}
			if (mTriviaQuestionMap.Keys.Count == 0)
			{
				PopulateQuestionMap(mSTTriviaQuizData, mTriviaQuestionMap);
			}
			if (mTriviaQuestionMap.Keys.Count > 0 && mShowTriviaQuestion && (UnityEngine.Random.Range(0, 2) > 0 || i == mResultInfo._UnlockedCreditChests + mResultInfo._MissedObjectives.Count - 1))
			{
				uiChest2.pQuestion = GetQuestionFromPool(mTriviaQuestionMap, GameManager.pInstance._TriviaQuestionCategory, isTrivia: true);
				uiChest2.pHasTriviaQuestion = true;
				mShowTriviaQuestion = false;
			}
			else if (mQuestionMap.Keys.Count > 0)
			{
				uiChest2.pQuestion = GetQuestionFromPool(mQuestionMap, GameManager.pInstance._QuestionCategory);
			}
			if (i == GameManager.pInstance._CreditChestsAvailable - 1 && AdManager.pInstance.AdSupported(_UiChestMenu._AdEventType, AdType.REWARDED_VIDEO) && AdManager.pInstance.IsDailyLimitAvailable(_UiChestMenu._AdEventType, showErrorMessage: false))
			{
				uiChest2.ShowAdBtn(enable: true);
				_UiChestMenu.pVideoAdchestItem = uiChest2;
			}
		}
		string text = "";
		if (mResultInfo._RewardType != LevelRewardType.LevelCompletion)
		{
			foreach (string missedObjective in mResultInfo._MissedObjectives)
			{
				text = text + missedObjective + "\n";
			}
		}
		for (int j = 0; j < mResultInfo._LockedChests; j++)
		{
			KAWidget kAWidget2 = _UiChestMenu.DuplicateWidget(_UiChestMenu._QuizChestTemplate);
			UiChest uiChest3 = (UiChest)kAWidget2.FindChildItem("BtnChest");
			if (mResultInfo._RewardType != LevelRewardType.LevelCompletion)
			{
				uiChest3.pMissedObjectiveText = text;
			}
			else if (mResultInfo._MissedObjectives.Count > j)
			{
				uiChest3.pMissedObjectiveText = mResultInfo._MissedObjectives[j];
			}
			_UiChestMenu.AddWidget(kAWidget2);
			uiChest3.pIsLockedChest = true;
			uiChest3.InitChest();
		}
		SetInteractive(interactive: false);
		SetVisibility(inVisible: true);
		mBattleBackPack.gameObject.SetActive(value: true);
		mUiRewardScreen.gameObject.SetActive(value: true);
		mBattleBackPack.Reposition();
	}

	private void AddRewardItemToMenu()
	{
		if (pItemStatsMap.Count > 0)
		{
			UserItemData userItemData = new UserItemData();
			userItemData.ItemID = pItemStatsMap[0].Item.ItemID;
			userItemData.Item = pItemStatsMap[0].Item;
			userItemData.Quantity = 1;
			userItemData.Uses = pItemStatsMap[0].Item.Uses;
			userItemData.ItemTier = pItemStatsMap[0].ItemTier;
			userItemData.ItemStats = pItemStatsMap[0].ItemStats;
			KAUISelectItemData kAUISelectItemData = new KAUISelectItemData(mRewardMenu, userItemData, mRewardMenu._WHSize, 1, InventoryTabType.ITEM);
			kAUISelectItemData._UserInventoryID = pItemStatsMap[0].UserItemStatsMapID;
			kAUISelectItemData._IsBattleReady = pItemStatsMap[0].ItemStats != null;
			mRewardMenu.AddToTargetMenu(kAUISelectItemData, mRewardMenu);
		}
	}

	private void ShowRewardTransition(float moveTimer)
	{
		_UiRewards.ScaleTo(Vector3.zero, _UiRewards.GetScale(), moveTimer);
		_UiRewards.MoveTo(new Vector2(mOriginalUiRewardPositon.x, mOriginalUiRewardPositon.y), moveTimer);
		_UiRewards.OnMoveToDone += OnReachedOrginalPosition;
		_UiRewards.ShowRewards(mRewards, mDisplayRewards, mIsExtraChestOpened);
	}

	private void OnReachedOrginalPosition(KAWidget item)
	{
		SetInteractive(interactive: true);
	}

	private void OnChestLocked(UiChest chestItem)
	{
		if (!chestItem.pIsRewardChest)
		{
			mResultInfo._UnlockedCreditChests--;
		}
	}

	private void OnChestOpened(UiChest chestItem)
	{
		if (!chestItem.pIsRewardChest)
		{
			mIsExtraChestOpened = true;
			ApplyRewards(LevelRewardType.ExtraChest, GameManager.pInstance.pRaisedPetEntityMap);
			return;
		}
		mIsExtraChestOpened = false;
		mEnableStatsDB = false;
		AddRewardItemToMenu();
		mEnableStatsDB = true;
		ShowRewardTransition(_RewardTransitionTime);
	}

	private void ApplyLevelRewards()
	{
		ApplyRewards(mResultInfo._RewardType, GameManager.pInstance.pRaisedPetEntityMap);
	}

	public void ShowLevelRewards()
	{
		SetRewards();
	}

	private void OnInitChestOpen(UiChest chestItem)
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		SetInteractive(interactive: false);
		if (!chestItem.pIsRewardChest)
		{
			if (_CreditChestOpenSFX != null)
			{
				SnChannel.Play(_CreditChestOpenSFX, "STSFX_Pool", inForce: true);
			}
			mResultInfo._UnlockedCreditChests--;
		}
	}

	private void ApplyRewards(LevelRewardType type, List<RaisedPetEntityMap> raisedPetEntityMaps)
	{
		WsWebService.ApplyRewards(new ApplyRewardsRequest
		{
			LevelDifficultyID = LevelManager.pInstance.GetLevelInfo().LevelDifficultyID,
			LevelID = LevelManager.pInstance.GetLevelInfo().LevelID,
			GameID = GameManager.pInstance._GameID,
			LevelRewardType = type,
			RaisedPetEntityMaps = raisedPetEntityMaps.ToArray(),
			AvatarGender = AvatarData.GetGender(),
			Locale = UtUtilities.GetLocaleLanguage()
		}, RewardsEventHandler, type);
	}

	private void ShowCloseConfirmationDB(string OkMessage, int itemCount = 0)
	{
		mExitAction = OkMessage;
		if (itemCount > 0 && mResultInfo._UnlockedCreditChests > 0)
		{
			mUiItemSellInfoDB.Initialize(mRewardMenu, base.gameObject, "ProcessReward", _UnclaimedChestAndItemsText.GetLocalizedString());
		}
		else if (itemCount > 0)
		{
			mUiItemSellInfoDB.Initialize(mRewardMenu, base.gameObject, "ProcessReward", _UnclaimedItemsWarningText.GetLocalizedString());
		}
		else
		{
			GameUtilities.DisplayGenericDB(_GenericDBName, (mResultInfo._UnlockedCreditChests == 1) ? _UnopenedChestWarningText.GetLocalizedString() : _UnopenedChestsWarningText.GetLocalizedString(), "", base.gameObject, "ProcessReward", "CloseWarningDB", "", "", inDestroyOnClick: true);
		}
	}

	private int GetRewardItemCount()
	{
		int num = 0;
		foreach (KAWidget item in mRewardMenu.GetItems())
		{
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)item.GetUserData();
			if (kAUISelectItemData != null && kAUISelectItemData._ItemID != 0)
			{
				num++;
			}
		}
		return num;
	}

	private void CloseEndDB()
	{
		_UiChestMenu.ClearItems();
		SetVisibility(inVisible: false);
	}

	private void ShowMainMenu()
	{
		CloseEndDB();
		GameManager.pInstance.LoadMainMenu();
	}

	private void RestartLevel()
	{
		CloseEndDB();
		GameManager.pInstance.Restart();
	}

	private void PlayAgain()
	{
		CloseEndDB();
		if (LevelManager.pInstance != null)
		{
			LevelManager.pInstance.LoadLevel();
		}
	}

	private void LogRewardEvents(int rewardID)
	{
		UiChest uiChest = (UiChest)_UiChestMenu.GetSelectedItem().FindChildItem("BtnChest");
		if (!(uiChest != null) || _UiChestMenu.pQuizResultData == null)
		{
			return;
		}
		string text = "";
		foreach (string answer in _UiChestMenu.pQuizResultData._SelectedAnswers)
		{
			int num = -1;
			num = Array.FindIndex(uiChest.pQuestion.Answers, (QuizAnswer ans) => ans.AnswerText._Text.Equals(answer));
			text = text + (string.IsNullOrEmpty(text) ? "Answer" : ",") + num;
		}
	}

	private void RewardsEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.ERROR:
			SetInteractive(interactive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			UtDebug.LogError("!!!" + inType.ToString() + " failed!!!!");
			break;
		case WsServiceEvent.COMPLETE:
			if (inObject != null)
			{
				ApplyRewardsResponse applyRewardsResponse = (ApplyRewardsResponse)inObject;
				if (applyRewardsResponse.Status == Status.Success)
				{
					if (mRewards == null)
					{
						mRewards = new List<AchievementReward>();
					}
					else
					{
						mRewards.Clear();
					}
					if (mDisplayRewards == null)
					{
						mDisplayRewards = new List<AchievementReward>();
					}
					else
					{
						mDisplayRewards.Clear();
					}
					if (applyRewardsResponse.AchievementRewards != null)
					{
						mRewards.AddRange(applyRewardsResponse.AchievementRewards.ToList());
					}
					if (applyRewardsResponse.RewardedItemStatsMap != null)
					{
						AchievementReward achievementReward = new AchievementReward();
						achievementReward.Amount = 1;
						achievementReward.ItemID = applyRewardsResponse.RewardedItemStatsMap.Item.ItemID;
						achievementReward.PointTypeID = 6;
						mDisplayRewards.Add(achievementReward);
						if ((LevelRewardType)inUserData != LevelRewardType.ExtraChest && applyRewardsResponse.RewardedItemStatsMap.Item.BluePrint == null)
						{
							pItemStatsMap.Add(applyRewardsResponse.RewardedItemStatsMap);
						}
						else
						{
							UserItemData userItemData = new UserItemData();
							userItemData.ItemID = applyRewardsResponse.RewardedItemStatsMap.Item.ItemID;
							userItemData.Item = applyRewardsResponse.RewardedItemStatsMap.Item;
							userItemData.Quantity = 1;
							userItemData.Uses = applyRewardsResponse.RewardedItemStatsMap.Item.Uses;
							userItemData.ItemTier = applyRewardsResponse.RewardedItemStatsMap.ItemTier;
							userItemData.ItemStats = applyRewardsResponse.RewardedItemStatsMap.ItemStats;
							if (applyRewardsResponse.RewardedItemStatsMap.Item.BluePrint == null)
							{
								pItemStatsMap.Add(applyRewardsResponse.RewardedItemStatsMap);
								KAUISelectItemData kAUISelectItemData = new KAUISelectItemData(mRewardMenu, userItemData, mRewardMenu._WHSize, 1, InventoryTabType.ITEM);
								kAUISelectItemData._UserInventoryID = applyRewardsResponse.RewardedItemStatsMap.UserItemStatsMapID;
								kAUISelectItemData._IsBattleReady = applyRewardsResponse.RewardedItemStatsMap.ItemStats != null;
								mEnableStatsDB = false;
								mRewardMenu.AddToTargetMenu(kAUISelectItemData, mRewardMenu);
								mEnableStatsDB = true;
							}
							else
							{
								CommonInventoryData.pInstance.AddToCategories(userItemData);
							}
						}
					}
					if (applyRewardsResponse.CommonInventoryResponse != null)
					{
						CommonInventoryResponseItem[] commonInventoryIDs = applyRewardsResponse.CommonInventoryResponse.CommonInventoryIDs;
						foreach (CommonInventoryResponseItem commonInventoryResponseItem in commonInventoryIDs)
						{
							if (commonInventoryResponseItem.CommonInventoryID != 0)
							{
								AchievementReward achievementReward2 = new AchievementReward();
								achievementReward2.Amount = 1;
								achievementReward2.ItemID = commonInventoryResponseItem.ItemID;
								achievementReward2.PointTypeID = 6;
								mDisplayRewards.Add(achievementReward2);
								CommonInventoryData.pInstance.AddItem(commonInventoryResponseItem.ItemID, updateServer: false, commonInventoryResponseItem.CommonInventoryID, 1);
								CommonInventoryData.pInstance.ClearSaveCache();
							}
						}
						if (applyRewardsResponse.CommonInventoryResponse.UserGameCurrency != null && applyRewardsResponse.CommonInventoryResponse.UserGameCurrency.CashCurrency > Money.pCashCurrency)
						{
							AchievementReward achievementReward3 = new AchievementReward();
							achievementReward3.Amount = applyRewardsResponse.CommonInventoryResponse.UserGameCurrency.CashCurrency - Money.pCashCurrency;
							achievementReward3.PointTypeID = 5;
							mDisplayRewards.Add(achievementReward3);
							Money.SetMoney(applyRewardsResponse.CommonInventoryResponse.UserGameCurrency);
						}
					}
					if (mIsExtraChestOpened)
					{
						ShowRewardTransition(_RewardTransitionTime);
					}
					else
					{
						GameUtilities.AddRewards(mRewards.ToArray(), inUseRewardManager: false);
						mResultsXpDB.PopulateXPData(mRewards);
					}
				}
				else
				{
					UtDebug.LogError("!!!" + applyRewardsResponse.Status);
				}
			}
			else
			{
				UtDebug.LogError("!!!" + inType.ToString() + " did not return valid object!!!!");
			}
			SetInteractive(interactive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	private void ProcessReward()
	{
		SetInteractive(interactive: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		ProcessRewardedItemsRequest processRewardedItemsRequest = new ProcessRewardedItemsRequest();
		List<ItemActionTypeMap> list = new List<ItemActionTypeMap>();
		foreach (KAWidget item in mRewardMenu.GetItems())
		{
			KAUISelectItemData kAUISelectItemData = item.GetUserData() as KAUISelectItemData;
			if (kAUISelectItemData._ItemData.ItemID > 0)
			{
				ItemActionTypeMap itemActionTypeMap = new ItemActionTypeMap();
				itemActionTypeMap.ID = kAUISelectItemData._UserInventoryID;
				itemActionTypeMap.InventoryMax = kAUISelectItemData._ItemData.InventoryMax;
				itemActionTypeMap.ItemUses = kAUISelectItemData._ItemData.Uses;
				itemActionTypeMap.Action = ((GetUserItemStatData(kAUISelectItemData._UserInventoryID) == null) ? ActionType.SellInventoryItem : ActionType.SellRewardBinItem);
				list.Add(itemActionTypeMap);
			}
		}
		foreach (KAWidget item2 in mBattleBackPackMenu.GetItems())
		{
			KAUISelectItemData kAUISelectItemData2 = item2.GetUserData() as KAUISelectItemData;
			if (GetUserItemStatData(kAUISelectItemData2._UserInventoryID) != null)
			{
				ItemActionTypeMap itemActionTypeMap2 = new ItemActionTypeMap();
				itemActionTypeMap2.ID = kAUISelectItemData2._UserInventoryID;
				itemActionTypeMap2.InventoryMax = kAUISelectItemData2._ItemData.InventoryMax;
				itemActionTypeMap2.ItemUses = kAUISelectItemData2._ItemData.Uses;
				itemActionTypeMap2.Action = ActionType.MoveToInventory;
				list.Add(itemActionTypeMap2);
			}
		}
		processRewardedItemsRequest.ItemsActionMap = list.ToArray();
		WsWebService.ProcessRewardedItems(processRewardedItemsRequest, ProcessRewardsEventHandler, null);
	}

	private void ProcessRewardsEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.ERROR:
			SetInteractive(interactive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			UtDebug.LogError("!!!" + inType.ToString() + " failed!!!!");
			SendMessage(mExitAction, null, SendMessageOptions.DontRequireReceiver);
			break;
		case WsServiceEvent.COMPLETE:
			if (inObject != null)
			{
				ProcessRewardedItemsResponse processRewardedItemsResponse = (ProcessRewardedItemsResponse)inObject;
				if (processRewardedItemsResponse.SoldRewardBinItems.HasValue && processRewardedItemsResponse.SoldRewardBinItems.Value)
				{
					OnItemsSold(isSuccess: true, null);
				}
				if (processRewardedItemsResponse.SoldInventoryItems.HasValue && processRewardedItemsResponse.SoldInventoryItems.Value)
				{
					foreach (KAWidget item in mRewardMenu.GetItems())
					{
						KAUISelectItemData kAUISelectItemData = item.GetUserData() as KAUISelectItemData;
						if (kAUISelectItemData._UserItemData != null)
						{
							CommonInventoryData.pInstance.RemoveItem(kAUISelectItemData._UserItemData, kAUISelectItemData._Quantity);
						}
					}
				}
				if (processRewardedItemsResponse.MovedRewardBinItems != null)
				{
					foreach (KAWidget item2 in mBattleBackPackMenu.GetItems())
					{
						KAUISelectItemData kAUISelectItemData2 = item2.GetUserData() as KAUISelectItemData;
						CommonInventoryResponseRewardBinItem[] movedRewardBinItems = processRewardedItemsResponse.MovedRewardBinItems;
						foreach (CommonInventoryResponseRewardBinItem commonInventoryResponseRewardBinItem in movedRewardBinItems)
						{
							if (kAUISelectItemData2._UserInventoryID == commonInventoryResponseRewardBinItem.UserItemStatsMapID)
							{
								UserItemStatsMap userItemStatData = GetUserItemStatData(kAUISelectItemData2._UserInventoryID);
								UserItemData userItemData = new UserItemData();
								userItemData.UserInventoryID = commonInventoryResponseRewardBinItem.CommonInventoryID;
								userItemData.ItemID = userItemStatData.Item.ItemID;
								userItemData.Item = userItemStatData.Item;
								userItemData.ItemStats = userItemStatData.ItemStats;
								userItemData.Quantity = 1;
								userItemData.Uses = userItemStatData.Item.Uses;
								userItemData.ItemTier = userItemStatData.ItemTier;
								CommonInventoryData.pInstance.AddToCategories(userItemData);
							}
						}
					}
				}
				if (processRewardedItemsResponse.CommonInventoryResponse != null)
				{
					if (processRewardedItemsResponse.CommonInventoryResponse.UserGameCurrency != null)
					{
						Money.SetMoney(processRewardedItemsResponse.CommonInventoryResponse.UserGameCurrency);
					}
					if (processRewardedItemsResponse.CommonInventoryResponse.CommonInventoryIDs != null)
					{
						CommonInventoryResponseItem[] commonInventoryIDs = processRewardedItemsResponse.CommonInventoryResponse.CommonInventoryIDs;
						foreach (CommonInventoryResponseItem commonInventoryResponseItem in commonInventoryIDs)
						{
							CommonInventoryData.pInstance.AddItem(commonInventoryResponseItem.ItemID, updateServer: false, commonInventoryResponseItem.CommonInventoryID, 1);
						}
						CommonInventoryData.pInstance.ClearSaveCache();
					}
				}
			}
			else
			{
				UtDebug.LogError("!!!" + inType.ToString() + " did not return valid object!!!!");
			}
			SetInteractive(interactive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			SendMessage(mExitAction, null, SendMessageOptions.DontRequireReceiver);
			break;
		}
	}

	public UserItemStatsMap GetUserItemStatData(int id)
	{
		return pItemStatsMap.Find((UserItemStatsMap item) => item.UserItemStatsMapID == id);
	}

	public ItemStat[] GetItemStatData(int id)
	{
		UserItemStatsMap userItemStatData = GetUserItemStatData(id);
		if (userItemStatData != null)
		{
			return userItemStatData.ItemStats;
		}
		return CommonInventoryData.pInstance.GetItemStatsByUserInventoryID(id);
	}

	public ItemTier? GetItemTierInfo(int id)
	{
		UserItemStatsMap userItemStatData = GetUserItemStatData(id);
		if (userItemStatData != null)
		{
			return userItemStatData.ItemTier;
		}
		return CommonInventoryData.pInstance.GetItemTierByUserInventoryID(id);
	}

	private void OnMoneyUpdated()
	{
		if (Money.pIsReady)
		{
			mGemsTotal.SetText(Money.pCashCurrency.ToString());
			mCoinsTotal.SetText(Money.pGameCurrency.ToString());
		}
	}

	private void OnDiscardClicked(KAWidget widget)
	{
		pStatDB.SetVisibility(inVisible: false);
		mBattleBackPackMenu.OnDoubleClick(widget);
	}

	private void OnSellClicked(KAWidget widget)
	{
		pStatDB.SetVisibility(inVisible: false);
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)widget.GetUserData();
		if (mBattleBackPackMenu != null && !mBattleBackPackMenu.IsTaskComplete(kAUISelectItemData))
		{
			mBattleBackPackMenu.ShowTaskItemPopUp();
			return;
		}
		mItemToSell = widget;
		mUiItemSellInfoDB.Initialize(kAUISelectItemData._ItemData, base.gameObject, "OnSellItem");
	}

	private void OnSellItem()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)mItemToSell.GetUserData();
		if (GetUserItemStatData(kAUISelectItemData._UserInventoryID) == null)
		{
			CommonInventoryData.pInstance.AddSellItem(kAUISelectItemData._UserInventoryID, 1);
			CommonInventoryData.pInstance.DoSell(OnItemsSold);
			return;
		}
		mExitAction = "";
		WsWebService.ProcessRewardedItems(new ProcessRewardedItemsRequest
		{
			ItemsActionMap = new List<ItemActionTypeMap>
			{
				new ItemActionTypeMap
				{
					ID = kAUISelectItemData._UserInventoryID,
					InventoryMax = kAUISelectItemData._ItemData.InventoryMax,
					ItemUses = kAUISelectItemData._ItemData.Uses,
					Action = ActionType.SellRewardBinItem
				}
			}.ToArray()
		}, ProcessRewardsEventHandler, null);
	}

	private void OnItemsSold(bool isSuccess, CommonInventoryResponse ret)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (isSuccess && mItemToSell != null)
		{
			mItemToSell.SetTexture(null);
			mItemToSell.SetInteractive(isInteractive: false);
			if (mBattleBackPackMenu.GetItems().Contains(mItemToSell))
			{
				mBattleBackPackMenu.pKAUISelect.AddWidgetData(mItemToSell, null);
				mBattleBackPackMenu.MoveItemToBottom(mItemToSell);
			}
			else
			{
				mRewardMenu.pKAUISelect.AddWidgetData(mItemToSell, null);
				mRewardMenu.MoveItemToBottom(mItemToSell);
			}
			pStatDB.SetVisibility(inVisible: false);
			mItemToSell = null;
		}
		UtDebug.Log("QuickSell - Reward : Items sold status " + isSuccess);
	}
}
