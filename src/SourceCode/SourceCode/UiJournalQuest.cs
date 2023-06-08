using System;
using System.Collections.Generic;
using UnityEngine;

public class UiJournalQuest : KAUI, IJournal
{
	[Serializable]
	public class ScientificQuestPhaseInfo
	{
		public string _PhaseKeyword;

		public KAWidget _PhaseButton;

		public LocaleString _PhaseTitleText;
	}

	public UiJournalQuestMenu _QuestMenu;

	public int _SpaceBetweenQuestData = 20;

	public int _ExperimentObjectivesLineCount;

	public int _DefaultObjectivesCellHeight = 55;

	public int _ExperimentObjectivesCellHeight = 110;

	public float _DefaultResultImageScale = 1f;

	public float _ExpandResultImageScale = 2f;

	public float _ScaleAnimTime = 0.2f;

	public Color _ObjectiveCompletedColor = Color.green;

	public Color _ObjectiveDefaultColor = Color.black;

	public Color _ExperimentResultColor = Color.blue;

	public GameObject _ScientificQuestTab;

	public KAUIMenu _ObjectivesMenu;

	public LocaleString _FastTravelTitleText = new LocaleString("Confirm Fast Travel");

	public LocaleString _FastTravelConfirmationText = new LocaleString("Are you sure you want to fast travel ?");

	public LocaleString _FastTravelNotificationText = new LocaleString("You are already at the quest destination");

	public LocaleString _FastTravelNotificationTitleText = new LocaleString("Notification");

	public LocaleString _NoQuestsText = new LocaleString("No Quests Available");

	public LocaleString _HypothesisTitle = new LocaleString("My Hypothesis");

	public LocaleString _ObjectiveTitle = new LocaleString("Objectives");

	public LocaleString _ConclusionTitle = new LocaleString("Conclusion");

	public LocaleString _ScientificPhasePunctuation = new LocaleString(" - ");

	public ScientificQuestPhaseInfo[] _ScientificQuestButtons;

	public RewardWidget _XPRewardWidget;

	public UiWorldMap _UiWorldMap;

	private KAWidget mTxtWho;

	private KAWidget mTxtWhoStatic;

	private KAWidget mTxtObjective;

	private KAWidget mTxtObjectiveStatic;

	private KAWidget mTxtStory;

	private KAWidget mTxtStoryStatic;

	private KAWidget mTxtQuestHeading;

	private KAWidget mBtnQuestOffer;

	private KAWidget mBookmarkMagnifier;

	private KAWidget mTxtQuestListHeadingStatic;

	private KAWidget mNPCIcon;

	private KAWidget mBtnFastTravel;

	private KAWidget mResultQuestGrp;

	private KAWidget mAniQuestResult;

	private KAWidget mAniQuestResultDB;

	private KAWidget mBtnExpandResults;

	private bool mScientificQuestMode = true;

	private bool mResultScaleUp = true;

	private SubquestUserData mCurrentSelection;

	private KAUI mOfferUi;

	private string mFastTravelLocation;

	protected override void Start()
	{
		base.Start();
		mTxtWho = FindItem("TxtWho");
		mTxtWhoStatic = FindItem("TxtWhoStatic");
		mTxtObjective = FindItem("TxtObjective");
		mTxtObjectiveStatic = FindItem("TxtObjectiveStatic");
		mTxtStory = FindItem("TxtStory");
		mTxtStoryStatic = FindItem("TxtStoryStatic");
		mTxtQuestHeading = FindItem("TxtQuestHeading");
		mBtnQuestOffer = FindItem("BtnQuestOffer");
		mTxtQuestListHeadingStatic = FindItem("TxtQuestListHeading");
		mBookmarkMagnifier = FindItem("BookmarkMagnifier");
		mNPCIcon = FindItem("NPCIcon");
		mBtnFastTravel = FindItem("BtnFastTravel");
		mResultQuestGrp = FindItem("ResultQuestGrp");
		mAniQuestResult = FindItem("AniQuestResult");
		mAniQuestResultDB = FindItem("AniQuestResultDB");
		mBtnExpandResults = FindItem("BtnExpandResults");
		ClearSubquests();
		SetQuestDetailsVisible(inVisible: false);
		if (_ObjectivesMenu != null)
		{
			_ObjectivesMenu.SetVisibility(GetVisibility());
		}
		ExpandOrMinimizeQuestList(inExpand: true);
		if (_UiWorldMap != null)
		{
			_UiWorldMap._MessageObject = base.gameObject;
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (_QuestMenu != null)
		{
			_QuestMenu.SetVisibility(inVisible);
		}
		if (mTxtQuestHeading != null)
		{
			mTxtQuestHeading.SetVisibility(inVisible);
		}
		if (_ObjectivesMenu != null)
		{
			if (_ObjectivesMenu.GetItemCount() > 0)
			{
				_ObjectivesMenu.SetVisibility(inVisible);
			}
			else
			{
				_ObjectivesMenu.SetVisibility(inVisible: false);
			}
		}
		ShowFastTravelMap(show: false);
		if (mCurrentSelection != null && inVisible)
		{
			if (mCurrentSelection._Type == RuleItemType.Mission)
			{
				SetQuestDetails(mCurrentSelection._Mission);
			}
			else if (mCurrentSelection._Type == RuleItemType.Task)
			{
				SetQuestDetails(mCurrentSelection._Task);
			}
		}
	}

	private void UpdateFastTravel()
	{
		if (!(_UiWorldMap != null))
		{
			return;
		}
		mFastTravelLocation = null;
		Task task = null;
		string value = null;
		if (mCurrentSelection != null)
		{
			if (mCurrentSelection._Type == RuleItemType.Task)
			{
				task = mCurrentSelection._Task;
			}
			else if (mCurrentSelection._Type == RuleItemType.Mission)
			{
				Mission mission = mCurrentSelection._Mission;
				task = FindFirstActiveTask(mission, ignoreAccept: true);
			}
		}
		if (task != null)
		{
			value = _UiWorldMap.PlotAndFocusQuestMarker(task);
		}
		ShowFastTravelMap(!string.IsNullOrEmpty(value));
	}

	private void ShowFastTravelMap(bool show)
	{
		if (mBtnFastTravel != null)
		{
			mBtnFastTravel.SetVisibility(show);
		}
		if (_UiWorldMap != null)
		{
			_UiWorldMap.SetVisibility(show);
			UIPanel component = _UiWorldMap.GetComponent<UIPanel>();
			if (component != null)
			{
				component.enabled = show;
			}
		}
	}

	private void OnLocationSelected(string location)
	{
		if (RsResourceManager.pCurrentLevel.Equals(location))
		{
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _FastTravelNotificationText.GetLocalizedString(), _FastTravelNotificationTitleText.GetLocalizedString(), base.gameObject, "", "", "OnRejectFastTravel", "", inDestroyOnClick: true);
			return;
		}
		mFastTravelLocation = location;
		GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _FastTravelConfirmationText.GetLocalizedString(), _FastTravelTitleText.GetLocalizedString(), base.gameObject, "OnAcceptFastTravel", "OnRejectFastTravel", "", "", inDestroyOnClick: true);
	}

	private void OnAcceptFastTravel()
	{
		if (UiJournal.pInstance != null && !string.IsNullOrEmpty(mFastTravelLocation))
		{
			UiJournal.pInstance.GoToScene(mFastTravelLocation);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "BtnBack")
		{
			GoBack();
		}
		else if (inWidget == mBtnQuestOffer)
		{
			ShowOffers();
		}
		else if (inWidget == mBtnExpandResults)
		{
			Vector3 vector = new Vector3(_DefaultResultImageScale, _DefaultResultImageScale, 1f);
			Vector3 vector2 = new Vector3(_ExpandResultImageScale, _ExpandResultImageScale, 1f);
			if (mAniQuestResult != null)
			{
				mAniQuestResult.ScaleTo(mResultScaleUp ? vector : vector2, mResultScaleUp ? vector2 : vector, _ScaleAnimTime);
			}
			if (mAniQuestResultDB != null)
			{
				mAniQuestResultDB.ScaleTo(mResultScaleUp ? vector : vector2, mResultScaleUp ? vector2 : vector, _ScaleAnimTime);
			}
			mResultScaleUp = !mResultScaleUp;
		}
		else
		{
			if (_ScientificQuestButtons == null)
			{
				return;
			}
			ScientificQuestPhaseInfo[] scientificQuestButtons = _ScientificQuestButtons;
			foreach (ScientificQuestPhaseInfo scientificQuestPhaseInfo in scientificQuestButtons)
			{
				if (inWidget.name == scientificQuestPhaseInfo._PhaseButton.name)
				{
					ShowScientificQuestDetails(inWidget);
					break;
				}
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (AvAvatar.pState != 0)
		{
			AvAvatar.pState = AvAvatarState.NONE;
			AvAvatar.SetUIActive(inActive: false);
		}
	}

	private void UpdateScientificButtonsState()
	{
		if (!mScientificQuestMode)
		{
			return;
		}
		if (mCurrentSelection != null && _ScientificQuestButtons != null)
		{
			_ScientificQuestTab.SetActive(value: true);
			bool flag = true;
			for (int i = 0; i < _ScientificQuestButtons.Length; i++)
			{
				SubquestUserData subquestUserData = _ScientificQuestButtons[i]._PhaseButton.GetUserData() as SubquestUserData;
				_ScientificQuestButtons[i]._PhaseButton.SetState(KAUIState.DISABLED);
				if (subquestUserData._Type == RuleItemType.Task)
				{
					_ScientificQuestButtons[i]._PhaseButton.SetState((!flag && !subquestUserData._Task.pCompleted) ? KAUIState.DISABLED : KAUIState.INTERACTIVE);
					flag = subquestUserData._Task.pCompleted;
				}
				else if (subquestUserData._Type == RuleItemType.Mission)
				{
					_ScientificQuestButtons[i]._PhaseButton.SetState((!flag && !subquestUserData._Mission.pCompleted) ? KAUIState.DISABLED : KAUIState.INTERACTIVE);
					flag = subquestUserData._Mission.pCompleted;
				}
			}
		}
		else
		{
			_ScientificQuestTab.SetActive(value: false);
		}
	}

	private void ShowScientificQuestDetails()
	{
		if (_ScientificQuestButtons == null || _ScientificQuestButtons.Length == 0)
		{
			return;
		}
		ScientificQuestPhaseInfo[] scientificQuestButtons = _ScientificQuestButtons;
		foreach (ScientificQuestPhaseInfo scientificQuestPhaseInfo in scientificQuestButtons)
		{
			SubquestUserData subquestUserData = (SubquestUserData)scientificQuestPhaseInfo._PhaseButton.GetUserData();
			if (subquestUserData == null)
			{
				continue;
			}
			if (subquestUserData._Type == RuleItemType.Mission)
			{
				if (subquestUserData._Mission != null && !subquestUserData._Mission.pCompleted)
				{
					ShowScientificQuestDetails(scientificQuestPhaseInfo._PhaseButton);
					return;
				}
			}
			else if (subquestUserData._Type == RuleItemType.Task && subquestUserData._Task != null && !subquestUserData._Task.pCompleted)
			{
				ShowScientificQuestDetails(scientificQuestPhaseInfo._PhaseButton);
				return;
			}
		}
		ShowScientificQuestDetails(_ScientificQuestButtons[0]._PhaseButton);
	}

	private void ShowScientificQuestDetails(KAWidget inWidget)
	{
		SubquestUserData subquestUserData = (mCurrentSelection = (SubquestUserData)inWidget.GetUserData());
		if (subquestUserData != null)
		{
			if (subquestUserData._Type == RuleItemType.Mission)
			{
				SetQuestDetails(subquestUserData._Mission);
			}
			else if (subquestUserData._Type == RuleItemType.Task)
			{
				SetQuestDetails(subquestUserData._Task);
			}
		}
		if (mBookmarkMagnifier == null)
		{
			mBookmarkMagnifier = FindItem("BookmarkMagnifier");
		}
		if (mBookmarkMagnifier != null)
		{
			mBookmarkMagnifier.SetPosition(inWidget.GetPosition().x, inWidget.GetPosition().y);
		}
		UpdateScientificButtonsState();
	}

	private void GoBack()
	{
		SetVisibility(inVisible: false);
		ClearSubquests();
	}

	private void SetQuestDetailsVisible(bool inVisible)
	{
		if (mTxtWho != null)
		{
			mTxtWho.SetVisibility(inVisible);
		}
		if (mTxtWhoStatic != null)
		{
			mTxtWhoStatic.SetVisibility(inVisible);
		}
		if (mTxtObjective != null)
		{
			mTxtObjective.SetVisibility(inVisible);
		}
		if (mTxtObjectiveStatic != null)
		{
			mTxtObjectiveStatic.SetVisibility(inVisible);
		}
		if (mTxtQuestHeading != null)
		{
			mTxtQuestHeading.SetVisibility(inVisible);
		}
		if (mBtnQuestOffer != null)
		{
			mBtnQuestOffer.SetVisibility(inVisible);
		}
		if (_ObjectivesMenu != null)
		{
			_ObjectivesMenu.SetVisibility(inVisible);
		}
		if (mTxtStoryStatic != null)
		{
			mTxtStoryStatic.SetVisibility(inVisible: false);
		}
	}

	private void AdjustWidgetPositions(string inWho, string inObjective, string inStory)
	{
		float y = mTxtWhoStatic.GetLabel().transform.lossyScale.y;
		float y2 = mTxtWho.GetLabel().transform.lossyScale.y;
		float num = y + 2f * y2 + (float)_SpaceBetweenQuestData;
		Vector3 position = mTxtWhoStatic.GetPosition();
		position.y -= num;
		mTxtObjectiveStatic.SetPosition(position.x, position.y);
		float y3 = mTxtObjectiveStatic.GetLabel().transform.lossyScale.y;
		y2 = mTxtObjective.GetLabel().transform.lossyScale.y;
		string[] array = inObjective.Split('\n');
		num = y3 + (float)array.Length * y2 + (float)_SpaceBetweenQuestData;
		position = mTxtObjectiveStatic.GetPosition();
		position.y -= num;
		mTxtStoryStatic.SetPosition(position.x, position.y);
	}

	private string GetHypothesis(Task inTask)
	{
		if (inTask.pPayload != null)
		{
			string text = inTask.pPayload.Get<string>(UiMissionHypothesisDB._HypothesisAnswerKey);
			string text2 = inTask.pPayload.Get<string>(UiMissionHypothesisDB._HypothesisAnswerIDKey);
			int result = 0;
			if (!string.IsNullOrEmpty(text2))
			{
				int.TryParse(text2, out result);
			}
			if (!string.IsNullOrEmpty(text))
			{
				return new LocaleString(text)
				{
					_ID = result
				}.GetLocalizedString();
			}
		}
		return "";
	}

	private bool IsResearchTask(Task inTask)
	{
		if (inTask == null || _ScientificQuestButtons == null)
		{
			return false;
		}
		string phaseKeyword = _ScientificQuestButtons[1]._PhaseKeyword;
		if (string.IsNullOrEmpty(phaseKeyword))
		{
			return false;
		}
		bool flag = false;
		Mission mission = inTask._Mission;
		while (!flag && mission != null)
		{
			flag = mission.Name.Contains(phaseKeyword);
			mission = mission._Parent;
		}
		return flag;
	}

	private bool IsExperimentTask(Task inTask)
	{
		if (inTask == null || _ScientificQuestButtons == null)
		{
			return false;
		}
		string phaseKeyword = _ScientificQuestButtons[3]._PhaseKeyword;
		if (string.IsNullOrEmpty(phaseKeyword))
		{
			return false;
		}
		return inTask._Mission.Name.Contains(phaseKeyword);
	}

	private string GetExperimentResult(Task inTask)
	{
		string result = "";
		if (inTask != null && IsExperimentTask(inTask))
		{
			string text = inTask.pPayload.Get<string>("Lab_Result");
			string text2 = inTask.pPayload.Get<string>("Lab_Result_ID");
			int result2 = 0;
			if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text2) && int.TryParse(text2, out result2))
			{
				result = StringTable.GetStringData(result2, text);
			}
		}
		return result;
	}

	private bool IsConclusionTask(Task inTask)
	{
		if (inTask == null || _ScientificQuestButtons == null)
		{
			return false;
		}
		string phaseKeyword = _ScientificQuestButtons[_ScientificQuestButtons.Length - 1]._PhaseKeyword;
		if (string.IsNullOrEmpty(phaseKeyword))
		{
			return false;
		}
		return inTask.Name.Contains(phaseKeyword);
	}

	private string GetConclusion(Task inTask)
	{
		string text = "";
		if (inTask != null && inTask.pData != null && inTask.pData.Type == "Meet" && inTask.pData.Objectives != null)
		{
			foreach (TaskObjective objective in inTask.pData.Objectives)
			{
				text = objective.Get<string>("Conclusion");
				if (!string.IsNullOrEmpty(text))
				{
					break;
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				Task hypothesisTask = MissionManagerDO.GetHypothesisTask(inTask._Mission);
				if (hypothesisTask != null)
				{
					text = text.Replace("{{hypothesis}}", GetHypothesis(hypothesisTask));
					string hypothesisResult = MissionManagerDO.GetHypothesisResult(hypothesisTask);
					if (!string.IsNullOrEmpty(hypothesisResult))
					{
						text = text.Replace("{{result}}", hypothesisResult);
					}
				}
				text = text.Replace("@@", "\n");
			}
		}
		return text;
	}

	private string GetScientificQuestPhaseName(Task inTask)
	{
		string result = "";
		if (MissionManagerDO.IsScientificMission(inTask._Mission))
		{
			ScientificQuestPhaseInfo[] scientificQuestButtons = _ScientificQuestButtons;
			foreach (ScientificQuestPhaseInfo scientificQuestPhaseInfo in scientificQuestButtons)
			{
				if (inTask.Name.Contains(scientificQuestPhaseInfo._PhaseKeyword))
				{
					return scientificQuestPhaseInfo._PhaseTitleText.GetLocalizedString();
				}
			}
		}
		return result;
	}

	private string GetScientificQuestPhaseName(Mission inMission)
	{
		string result = "";
		if (MissionManagerDO.IsScientificMission(inMission))
		{
			ScientificQuestPhaseInfo[] scientificQuestButtons = _ScientificQuestButtons;
			foreach (ScientificQuestPhaseInfo scientificQuestPhaseInfo in scientificQuestButtons)
			{
				if (inMission.Name.Contains(scientificQuestPhaseInfo._PhaseKeyword))
				{
					return scientificQuestPhaseInfo._PhaseTitleText.GetLocalizedString();
				}
			}
		}
		return result;
	}

	private RuleItem GetPrevScientificTask(Mission inRootMission, int inID)
	{
		foreach (RuleItem ruleItem in inRootMission.MissionRule.Criteria.RuleItems)
		{
			if (ruleItem.ID == inID)
			{
				break;
			}
			if (ruleItem.Type == RuleItemType.Task)
			{
				if (!inRootMission.GetTask(ruleItem.ID).pCompleted)
				{
					return ruleItem;
				}
			}
			else if (ruleItem.Type == RuleItemType.Mission && !inRootMission.GetMission(ruleItem.ID).pCompleted)
			{
				return ruleItem;
			}
		}
		return null;
	}

	private void SetQuestDetails(Task inTask)
	{
		if (inTask == null)
		{
			return;
		}
		string taskWho = GetTaskWho(inTask);
		string taskObjectives = GetTaskObjectives(inTask);
		string taskStory = GetTaskStory(inTask);
		string text = MissionManagerDO.GetQuestHeading(inTask._Mission);
		if (mScientificQuestMode)
		{
			string scientificQuestPhaseName = GetScientificQuestPhaseName(inTask);
			if (!string.IsNullOrEmpty(scientificQuestPhaseName))
			{
				text = text + _ScientificPhasePunctuation.GetLocalizedString() + scientificQuestPhaseName;
			}
		}
		SetQuestDetailsCommon(inTask._Mission, taskWho, taskObjectives, taskStory, text);
		if (mScientificQuestMode && _ScientificQuestButtons.Length != 0 && !inTask.Name.Contains(_ScientificQuestButtons[0]._PhaseKeyword))
		{
			_XPRewardWidget.SetVisibility(inVisible: false);
		}
		else
		{
			_XPRewardWidget.SetVisibility(inVisible: true);
		}
		if (mTxtObjectiveStatic != null)
		{
			mTxtObjectiveStatic.SetText(_ObjectiveTitle.GetLocalizedString());
		}
		if (mScientificQuestMode)
		{
			Mission rootMission = MissionManager.pInstance.GetRootMission(inTask);
			RuleItem prevScientificTask = GetPrevScientificTask(rootMission, inTask.TaskID);
			if (prevScientificTask != null)
			{
				if (prevScientificTask.Type == RuleItemType.Task)
				{
					Task task = rootMission.GetTask(prevScientificTask.ID);
					ShowObjectives(task);
					MissionManagerDO.SetCurrentActiveTask(task.TaskID);
				}
				else if (prevScientificTask.Type == RuleItemType.Mission)
				{
					Mission mission = rootMission.GetMission(prevScientificTask.ID);
					ShowObjectives(mission);
				}
				return;
			}
			ShowObjectives(inTask);
			if (!IsConclusionTask(inTask))
			{
				return;
			}
			string conclusion = GetConclusion(inTask);
			if (!string.IsNullOrEmpty(conclusion) && mTxtStoryStatic != null && mTxtStory != null)
			{
				mTxtStoryStatic.SetText(_ConclusionTitle.GetLocalizedString());
				mTxtStoryStatic.SetVisibility(inVisible: true);
				mTxtStory.SetText(conclusion);
			}
			if (!(mAniQuestResult != null))
			{
				return;
			}
			Task hypothesisTask = MissionManagerDO.GetHypothesisTask(inTask._Mission);
			string text2 = ((hypothesisTask != null) ? hypothesisTask.pPayload.Get<string>("Lab_Image_Url") : inTask.pPayload.Get<string>("Lab_Image_Url"));
			if (text2 != null)
			{
				string[] array = text2.Split('/');
				if (array.Length == 3)
				{
					RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnResultImageLoaded, typeof(Texture2D));
				}
			}
		}
		else
		{
			ShowObjectives(inTask._Mission);
		}
	}

	public void OnResultImageLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			Texture2D inTexture = (Texture2D)inObject;
			if (mResultQuestGrp != null)
			{
				mResultQuestGrp.SetVisibility(GetVisibility());
			}
			if (mAniQuestResult != null)
			{
				mAniQuestResult.SetTexture(inTexture, inPixelPerfect: false, inURL);
				mAniQuestResult.SetVisibility(GetVisibility());
			}
			mResultScaleUp = true;
			break;
		}
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	private void SetQuestDetails(Mission inMission)
	{
		if (inMission == null)
		{
			return;
		}
		string taskWho = GetTaskWho(inMission);
		string taskObjectives = GetTaskObjectives(inMission);
		string taskStory = GetTaskStory(inMission);
		string text = MissionManagerDO.GetQuestHeading(inMission);
		if (mScientificQuestMode)
		{
			string scientificQuestPhaseName = GetScientificQuestPhaseName(inMission);
			if (!string.IsNullOrEmpty(scientificQuestPhaseName))
			{
				text = text + _ScientificPhasePunctuation.GetLocalizedString() + scientificQuestPhaseName;
			}
		}
		SetQuestDetailsCommon(inMission, taskWho, taskObjectives, taskStory, text);
		if (mScientificQuestMode && _ScientificQuestButtons.Length != 0 && !inMission.Name.Contains(_ScientificQuestButtons[0]._PhaseKeyword))
		{
			_XPRewardWidget.SetVisibility(inVisible: false);
		}
		else
		{
			_XPRewardWidget.SetVisibility(inVisible: true);
		}
		if (mScientificQuestMode)
		{
			Mission rootMission = MissionManager.pInstance.GetRootMission(inMission);
			RuleItem prevScientificTask = GetPrevScientificTask(rootMission, inMission.MissionID);
			if (prevScientificTask != null)
			{
				if (prevScientificTask.Type == RuleItemType.Task)
				{
					Task task = rootMission.GetTask(prevScientificTask.ID);
					ShowObjectives(task);
				}
				else if (prevScientificTask.Type == RuleItemType.Mission)
				{
					Mission mission = rootMission.GetMission(prevScientificTask.ID);
					ShowObjectives(mission);
				}
			}
			else
			{
				ShowObjectives(inMission);
			}
		}
		else
		{
			ShowObjectives(inMission);
		}
		if (mTxtObjectiveStatic != null)
		{
			mTxtObjectiveStatic.SetText(_ObjectiveTitle.GetLocalizedString());
		}
	}

	private void SetQuestDetailsCommon(Mission inMission, string lWho, string lObjectives, string lStory, string lHeading)
	{
		SetQuestDetailsVisible(inVisible: true);
		if (mTxtWho != null)
		{
			mTxtWho.SetText(MissionManagerDO.GetNPCName(lWho));
		}
		if (mBtnQuestOffer != null)
		{
			mBtnQuestOffer.SetVisibility(!string.IsNullOrEmpty(lWho));
		}
		if (mNPCIcon != null && !string.IsNullOrEmpty(lWho))
		{
			mNPCIcon.PlayAnim(lWho);
		}
		if (lWho == "PfPlayer")
		{
			KAWidget kAWidget = FindItem("AvatarPic");
			kAWidget?.SetTexture(UiToolbar.pPortraitTexture);
			kAWidget?.SetVisibility(inVisible: true);
			if (mNPCIcon != null)
			{
				mNPCIcon.SetVisibility(inVisible: false);
			}
		}
		if (mTxtStory != null)
		{
			mTxtStory.SetText(lStory);
		}
		if (mTxtQuestHeading != null)
		{
			mTxtQuestHeading.SetText(lHeading);
			KAWidget kAWidget2 = mTxtQuestHeading.FindChildItem("BkgIcon");
			if (kAWidget2 != null)
			{
				Mission rootMission = MissionManager.pInstance.GetRootMission(inMission);
				string text = ((rootMission != null) ? rootMission.pData.Icon : inMission.pData.Icon);
				kAWidget2.SetVisibility(inVisible: false);
				if (!string.IsNullOrEmpty(text))
				{
					if (text.StartsWith("http://"))
					{
						kAWidget2.SetTextureFromURL(text, base.gameObject);
					}
					else
					{
						string[] array = text.Split('/');
						if (array.Length >= 3)
						{
							kAWidget2.SetTextureFromBundle(array[0] + "/" + array[1], array[2], base.gameObject);
						}
					}
				}
			}
		}
		if (mResultQuestGrp != null)
		{
			mResultQuestGrp.SetVisibility(inVisible: false);
		}
		_XPRewardWidget.SetRewards(inMission.Rewards.ToArray(), MissionManager.pInstance._RewardData);
		AdjustWidgetPositions(lWho, lObjectives, lStory);
		SetCurrentTrackedTask();
		UpdateFastTravel();
	}

	private void OnTextureLoaded(KAWidget widget)
	{
		widget.SetVisibility(inVisible: true);
	}

	private void ShowObjectives(Mission inMission)
	{
		if (_ObjectivesMenu != null)
		{
			_ObjectivesMenu.ClearItems();
			PopulateObjectivesMenu(inMission);
		}
	}

	private void PopulateObjectivesMenu(Mission inMission)
	{
		if (!(_ObjectivesMenu != null))
		{
			return;
		}
		float cellHeight = _DefaultObjectivesCellHeight;
		_ObjectivesMenu._DefaultGrid.cellHeight = cellHeight;
		float cellHeight2 = _ExperimentObjectivesCellHeight;
		if (inMission.MissionRule == null || inMission.MissionRule.Criteria == null || inMission.MissionRule.Criteria.RuleItems == null)
		{
			return;
		}
		foreach (RuleItem ruleItem in inMission.MissionRule.Criteria.RuleItems)
		{
			string text = "";
			KAWidget kAWidget = null;
			if (ruleItem.Type == RuleItemType.Task)
			{
				if (ruleItem.Complete > 0 && inMission.MissionRule.Criteria.Ordered)
				{
					continue;
				}
				Task task = inMission.GetTask(ruleItem.ID);
				text = GetTaskCaption(task);
				if (string.IsNullOrEmpty(text))
				{
					continue;
				}
				MissionManagerDO.AddQuantityToObjectiveString(ref text, task);
				text = ((!task.pCompleted) ? (UtUtilities.GetKAUIColorString(_ObjectiveDefaultColor) + text) : (UtUtilities.GetKAUIColorString(_ObjectiveCompletedColor) + text));
				kAWidget = _ObjectivesMenu.AddWidget("Task_" + task.TaskID);
				if (kAWidget != null)
				{
					kAWidget.SetVisibility(inVisible: true);
					kAWidget.SetText(text);
					KAWidget kAWidget2 = kAWidget.FindChildItem("Icon");
					if (kAWidget2 != null)
					{
						kAWidget2.SetVisibility(task.pCompleted);
					}
					if (IsExperimentTask(task))
					{
						kAWidget.GetLabel().maxLineCount = _ExperimentObjectivesLineCount;
						_ObjectivesMenu._DefaultGrid.cellHeight = cellHeight2;
					}
					SetupResearchTaskWidget(kAWidget, task);
					if (inMission.MissionRule.Criteria.Ordered)
					{
						break;
					}
				}
			}
			else
			{
				if (ruleItem.Type != RuleItemType.Mission)
				{
					continue;
				}
				Mission mission = inMission.GetMission(ruleItem.ID);
				if (!mission.pCompleted || !inMission.MissionRule.Criteria.Ordered)
				{
					PopulateObjectivesMenu(mission);
					if (inMission.MissionRule.Criteria.Ordered)
					{
						break;
					}
				}
			}
		}
		_ObjectivesMenu._DefaultGrid.Reposition();
		_ObjectivesMenu.SetVisibility(GetVisibility());
	}

	private void ShowObjectives(Task inTask)
	{
		if (!(_ObjectivesMenu != null))
		{
			return;
		}
		if (mTxtObjective != null)
		{
			mTxtObjective.SetVisibility(inVisible: false);
		}
		_ObjectivesMenu.SetVisibility(GetVisibility());
		_ObjectivesMenu.ClearItems();
		string objectiveStr = GetTaskCaption(inTask);
		if (MissionManagerDO.IsHypothesisTask(inTask))
		{
			string hypothesis = GetHypothesis(inTask);
			if (!string.IsNullOrEmpty(hypothesis))
			{
				if (mTxtObjectiveStatic != null)
				{
					mTxtObjectiveStatic.SetText(_HypothesisTitle.GetLocalizedString());
				}
				objectiveStr = hypothesis;
			}
		}
		MissionManagerDO.AddQuantityToObjectiveString(ref objectiveStr, inTask);
		objectiveStr = ((!inTask.pCompleted) ? (UtUtilities.GetKAUIColorString(_ObjectiveDefaultColor) + objectiveStr) : (UtUtilities.GetKAUIColorString(_ObjectiveCompletedColor) + objectiveStr));
		KAWidget kAWidget = _ObjectivesMenu.AddWidget("Task_" + inTask.TaskID);
		if (kAWidget != null)
		{
			SetupResearchTaskWidget(kAWidget, inTask);
			kAWidget.SetText(objectiveStr);
			kAWidget.SetVisibility(inVisible: true);
			KAWidget kAWidget2 = kAWidget.FindChildItem("Icon");
			if (kAWidget2 != null)
			{
				kAWidget2.SetVisibility(inTask.pCompleted);
			}
		}
	}

	private void SetupResearchTaskWidget(KAWidget inWidget, Task inTask)
	{
		KAWidget kAWidget = inWidget.FindChildItem("NPCIcon");
		if (!(kAWidget != null))
		{
			return;
		}
		kAWidget.SetVisibility(inVisible: false);
		if (!mScientificQuestMode || !IsResearchTask(inTask) || !inTask.pCompleted)
		{
			return;
		}
		inWidget.SetState(KAUIState.INTERACTIVE);
		kAWidget.SetVisibility(inVisible: true);
		kAWidget.pUI.pEvents.OnClick -= OnObjectiveSelected;
		kAWidget.pUI.pEvents.OnClick += OnObjectiveSelected;
		KAWidgetUserData kAWidgetUserData = new KAWidgetUserData();
		kAWidgetUserData._Index = inTask.TaskID;
		kAWidget.SetUserData(kAWidgetUserData);
		foreach (MissionAction end in inTask.pData.Ends)
		{
			if (!string.IsNullOrEmpty(end.NPC) && (end.Type == MissionActionType.Popup || end.Type == MissionActionType.VO))
			{
				kAWidget.PlayAnim(end.NPC);
				break;
			}
		}
	}

	private void OnObjectiveSelected(KAWidget inWidget)
	{
		KAWidgetUserData userData = inWidget.GetUserData();
		if (userData == null)
		{
			return;
		}
		Task task = MissionManager.pInstance.GetTask(userData._Index);
		if (task == null || !task.pCompleted || task.pData == null || task.pData.Ends == null)
		{
			return;
		}
		List<MissionAction> ends = task.pData.Ends;
		if (ends == null || ends.Count <= 0)
		{
			return;
		}
		List<MissionAction> list = ends.FindAll((MissionAction o) => o.Type == MissionActionType.Popup || o.Type == MissionActionType.VO);
		if (list == null || list.Count <= 0)
		{
			return;
		}
		foreach (MissionAction item in list)
		{
			item._Played = false;
		}
		MissionManager.pInstance.AddAction(list, null, MissionManager.ActionType.TASK_END, OnOfferAction);
	}

	public void AddSubquest(Task inTask)
	{
		SubquestUserData subquestUserData = new SubquestUserData();
		subquestUserData._Task = inTask;
		if (_ScientificQuestButtons == null)
		{
			return;
		}
		ScientificQuestPhaseInfo[] scientificQuestButtons = _ScientificQuestButtons;
		foreach (ScientificQuestPhaseInfo scientificQuestPhaseInfo in scientificQuestButtons)
		{
			if (!(scientificQuestPhaseInfo._PhaseButton.GetUserData() is SubquestUserData))
			{
				scientificQuestPhaseInfo._PhaseButton.SetUserData(subquestUserData);
				scientificQuestPhaseInfo._PhaseButton.SetState(KAUIState.INTERACTIVE);
				break;
			}
		}
	}

	public void AddSubquest(Mission inMission)
	{
		SubquestUserData subquestUserData = new SubquestUserData();
		subquestUserData._Mission = inMission;
		subquestUserData._Type = RuleItemType.Mission;
		if (_ScientificQuestButtons == null)
		{
			return;
		}
		ScientificQuestPhaseInfo[] scientificQuestButtons = _ScientificQuestButtons;
		foreach (ScientificQuestPhaseInfo scientificQuestPhaseInfo in scientificQuestButtons)
		{
			if (!(scientificQuestPhaseInfo._PhaseButton.GetUserData() is SubquestUserData))
			{
				scientificQuestPhaseInfo._PhaseButton.SetUserData(subquestUserData);
				scientificQuestPhaseInfo._PhaseButton.SetState(KAUIState.INTERACTIVE);
				break;
			}
		}
	}

	private void ClearSubquests()
	{
		mCurrentSelection = null;
		if (_ScientificQuestButtons != null)
		{
			ScientificQuestPhaseInfo[] scientificQuestButtons = _ScientificQuestButtons;
			foreach (ScientificQuestPhaseInfo obj in scientificQuestButtons)
			{
				obj._PhaseButton.SetUserData(null);
				obj._PhaseButton.SetState(KAUIState.DISABLED);
			}
			if (mBookmarkMagnifier == null)
			{
				mBookmarkMagnifier = FindItem("BookmarkMagnifier");
			}
			if (mBookmarkMagnifier != null && _ScientificQuestButtons.Length != 0 && _ScientificQuestButtons[0] != null && _ScientificQuestButtons[0]._PhaseButton != null)
			{
				mBookmarkMagnifier.SetPosition(_ScientificQuestButtons[0]._PhaseButton.GetPosition().x, _ScientificQuestButtons[0]._PhaseButton.GetPosition().y);
			}
		}
	}

	private string GetTaskObjectives(Task inTask)
	{
		string text = "";
		if (inTask != null && inTask.pData != null && inTask.pData.Title != null)
		{
			text = MissionManager.pInstance.FormatText(inTask.pData.Title.ID, inTask.pData.Title.Text);
		}
		if (inTask != null && inTask.pData != null && inTask.pData.Description != null)
		{
			text = MissionManager.pInstance.FormatText(inTask.pData.Description.ID, inTask.pData.Description.Text);
			if (inTask.pData.Type == "Collect" && inTask.pData.Objectives.Count > 0)
			{
				foreach (TaskObjective objective in inTask.pData.Objectives)
				{
					int num = objective.Get<int>("Quantity");
					if (num >= 1)
					{
						text = text + "(" + objective._Collected + " / " + num + ")";
					}
				}
			}
			else if (inTask.pData.Type == "Delivery" && inTask.pData.Objectives.Count > 0)
			{
				foreach (TaskObjective objective2 in inTask.pData.Objectives)
				{
					int num2 = objective2.Get<int>("ItemID");
					if (num2 > 0)
					{
						UserItemData userItemData = CommonInventoryData.pInstance.FindItem(num2);
						int num3 = objective2.Get<int>("Quantity");
						text = ((userItemData == null) ? (text + "(0/ " + num3 + ")") : (text + "(" + userItemData.Quantity + " / " + num3 + ")"));
						break;
					}
				}
			}
		}
		return text;
	}

	private string GetTaskStory(Task inTask)
	{
		if (inTask != null && inTask.pData != null && inTask.pData.Description != null)
		{
			return MissionManager.pInstance.FormatText(inTask.pData.Description.ID, inTask.pData.Description.Text);
		}
		return "";
	}

	private string GetTaskWho(Task inTask)
	{
		string text = "";
		if (inTask != null)
		{
			if (inTask.pData != null && inTask.pData.Offers != null && inTask.pData.Offers.Count > 0)
			{
				text = inTask.pData.Offers[0].NPC;
			}
			Mission mission = inTask._Mission;
			while (string.IsNullOrEmpty(text) && mission != null)
			{
				if (mission.pData != null && mission.pData.Offers != null && mission.pData.Offers.Count > 0)
				{
					text = mission.pData.Offers[0].NPC;
				}
				mission = mission._Parent;
			}
		}
		return text;
	}

	private string GetTaskWho(Mission inMission)
	{
		string text = "";
		if (inMission != null && inMission.pData != null && inMission.pData.Offers != null && inMission.pData.Offers.Count > 0)
		{
			return inMission.pData.Offers[0].NPC;
		}
		return GetTaskWho(FindFirstActiveTask(inMission));
	}

	private string GetTaskObjectives(Mission inMission)
	{
		string text = "";
		if (inMission != null && inMission.pData != null && inMission.pData.Description != null)
		{
			text = MissionManager.pInstance.FormatText(inMission.pData.Description.ID, inMission.pData.Description.Text);
		}
		if (string.IsNullOrEmpty(text) && inMission.MissionRule != null && inMission.MissionRule.Criteria != null && inMission.MissionRule.Criteria.RuleItems != null)
		{
			foreach (RuleItem ruleItem in inMission.MissionRule.Criteria.RuleItems)
			{
				if (ruleItem.Type == RuleItemType.Task)
				{
					Task task = MissionManager.pInstance.GetTask(ruleItem.ID);
					text = text + GetTaskObjectives(task) + "\n";
				}
				else if (ruleItem.Type == RuleItemType.Mission)
				{
					Mission mission = MissionManager.pInstance.GetMission(ruleItem.ID);
					text += GetTaskObjectives(mission);
				}
			}
		}
		return text;
	}

	private string GetTaskStory(Mission inMission)
	{
		if (inMission != null && inMission.pData != null && inMission.pData.Description != null)
		{
			return MissionManager.pInstance.FormatText(inMission.pData.Description.ID, inMission.pData.Description.Text);
		}
		return "";
	}

	private void ShowOffers()
	{
		if (mCurrentSelection != null && (!mScientificQuestMode || !ShowPreviousScientificQuestOffers()))
		{
			if (mCurrentSelection._Type == RuleItemType.Task)
			{
				ShowTaskOffers(mCurrentSelection._Task);
			}
			else if (mCurrentSelection._Type == RuleItemType.Mission)
			{
				ShowMissionOffers(mCurrentSelection._Mission);
			}
		}
	}

	private bool ShowPreviousScientificQuestOffers()
	{
		bool result = false;
		RuleItem ruleItem = null;
		Mission mission = null;
		if (mCurrentSelection._Type == RuleItemType.Task)
		{
			mission = MissionManager.pInstance.GetRootMission(mCurrentSelection._Task);
			if (mission != null)
			{
				ruleItem = GetPrevScientificTask(mission, mCurrentSelection._Task.TaskID);
			}
		}
		else if (mCurrentSelection._Type == RuleItemType.Mission)
		{
			mission = MissionManager.pInstance.GetRootMission(mCurrentSelection._Mission);
			if (mission != null)
			{
				ruleItem = GetPrevScientificTask(mission, mCurrentSelection._Mission.MissionID);
			}
		}
		if (ruleItem != null)
		{
			if (ruleItem.Type == RuleItemType.Task)
			{
				Task task = mission.GetTask(ruleItem.ID);
				ShowTaskOffers(task);
			}
			else if (ruleItem.Type == RuleItemType.Mission)
			{
				Mission mission2 = mission.GetMission(ruleItem.ID);
				ShowMissionOffers(mission2);
			}
			result = true;
		}
		return result;
	}

	private void ShowMissionOffers(Mission inMission)
	{
		if (inMission == null)
		{
			return;
		}
		List<MissionAction> offers = inMission.pData.Offers;
		if (offers != null && offers.Count > 0)
		{
			List<MissionAction> list = offers.FindAll((MissionAction o) => o.Type == MissionActionType.Popup || o.Type == MissionActionType.VO);
			if (list == null || list.Count <= 0)
			{
				return;
			}
			foreach (MissionAction item in list)
			{
				item._Played = false;
			}
			MissionManager.pInstance.AddAction(list, inMission, MissionManager.ActionType.OFFER, OnOfferAction);
		}
		else
		{
			ShowTaskOffers(FindFirstActiveTask(mCurrentSelection._Mission));
		}
	}

	private void ShowTaskOffers(Task inTask)
	{
		if (inTask == null)
		{
			return;
		}
		List<MissionAction> offers = inTask.GetOffers(unplayed: false);
		if (offers == null || offers.Count <= 0)
		{
			return;
		}
		List<MissionAction> list = offers.FindAll((MissionAction o) => o.Type == MissionActionType.Popup || o.Type == MissionActionType.VO);
		if (list == null || list.Count <= 0)
		{
			return;
		}
		foreach (MissionAction item in list)
		{
			item._Played = false;
		}
		MissionManager.pInstance.AddAction(list, inTask, MissionManager.ActionType.OFFER, OnOfferAction);
	}

	private void SetCurrentTrackedTask()
	{
		if (mCurrentSelection == null)
		{
			return;
		}
		if (mCurrentSelection._Type == RuleItemType.Task && mCurrentSelection._Task._Active)
		{
			MissionManagerDO.SetCurrentActiveTask(mCurrentSelection._Task.TaskID);
		}
		else if (mCurrentSelection._Type == RuleItemType.Mission)
		{
			Mission mission = mCurrentSelection._Mission;
			Task task = FindFirstActiveTask(mission);
			if (task != null)
			{
				MissionManagerDO.SetCurrentActiveTask(task.TaskID);
			}
		}
	}

	private Task FindFirstActiveTask(Mission inMission, bool ignoreAccept = false)
	{
		if (inMission != null && (ignoreAccept || !inMission.pMustAccept))
		{
			List<Task> tasks = new List<Task>();
			MissionManager.pInstance.GetNextTask(inMission, ref tasks);
			if (tasks.Count > 0)
			{
				foreach (Task item in tasks)
				{
					if (item._Active || item.pActivateOnClick)
					{
						return item;
					}
				}
			}
		}
		return null;
	}

	private string GetTaskCaption(Task inTask)
	{
		string text = "";
		if (inTask != null && inTask.pData != null && inTask.pData.Title != null)
		{
			text = MissionManager.pInstance.FormatText(inTask.pData.Title.ID, inTask.pData.Title.Text);
		}
		if (inTask.pCompleted)
		{
			string experimentResult = GetExperimentResult(inTask);
			if (!string.IsNullOrEmpty(experimentResult))
			{
				text = text + "\n\t" + UtUtilities.GetKAUIColorString(_ExperimentResultColor) + experimentResult;
			}
		}
		return text;
	}

	private string GetTaskCaption(Mission inMission)
	{
		string result = "";
		if (inMission != null && inMission.pData != null && inMission.pData.Title != null)
		{
			result = MissionManager.pInstance.FormatText(inMission.pData.Title.ID, inMission.pData.Title.Text);
		}
		return result;
	}

	private void OnOfferAction(MissionActionEvent inEvent, object inObject)
	{
		if (inEvent == MissionActionEvent.POPUP_LOADED && inObject != null)
		{
			GameObject gameObject = inObject as GameObject;
			if (gameObject != null)
			{
				UiMissionActionDB componentInChildren = gameObject.GetComponentInChildren<UiMissionActionDB>();
				if (componentInChildren != null)
				{
					mOfferUi = componentInChildren;
					KAUI.SetExclusive(componentInChildren);
				}
			}
		}
		else if (inEvent == MissionActionEvent.POPUP_COMPLETE && inObject != null && mOfferUi != null)
		{
			KAUI.RemoveExclusive(mOfferUi);
		}
	}

	private void SetScientificQuestMode(bool inIsScientificQuest)
	{
		mScientificQuestMode = inIsScientificQuest;
		if (_ScientificQuestTab != null)
		{
			_ScientificQuestTab.SetActive(mScientificQuestMode);
		}
	}

	private void ExpandOrMinimizeQuestList(bool inExpand)
	{
		if (!(_QuestMenu == null) && GetVisibility())
		{
			_QuestMenu.SetVisibility(inExpand);
			if (_QuestMenu.GetVisibility())
			{
				_QuestMenu.RefreshQuestList(-1);
			}
			if (mTxtQuestListHeadingStatic != null)
			{
				mTxtQuestListHeadingStatic.SetVisibility(inExpand);
			}
		}
	}

	public void ShowQuestDetails(Task inTask)
	{
		if (inTask == null)
		{
			return;
		}
		Mission rootMission = MissionManager.pInstance.GetRootMission(inTask);
		if (rootMission != null)
		{
			if (MissionManagerDO.IsScientificMission(rootMission))
			{
				ShowQuestDetails(rootMission);
			}
			else
			{
				ShowQuestDetails(inTask._Mission);
			}
		}
	}

	public void ShowQuestDetails(Mission inMission)
	{
		if (inMission == null)
		{
			return;
		}
		Mission rootMission = MissionManager.pInstance.GetRootMission(inMission);
		SetScientificQuestMode(MissionManagerDO.IsScientificMission(inMission));
		if (!mScientificQuestMode && inMission.MissionRule.Criteria.Ordered)
		{
			List<Task> tasks = new List<Task>();
			MissionManager.pInstance.GetNextTask(inMission, ref tasks);
			if (tasks != null && tasks.Count > 0)
			{
				SetScientificQuestMode(MissionManagerDO.IsScientificMission(tasks[0]._Mission));
			}
			if (mScientificQuestMode)
			{
				rootMission = MissionManager.pInstance.GetRootMission(tasks[0]._Mission);
			}
		}
		ShowUpdatedMission(rootMission);
	}

	private void GetUserMissionStateEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			if (inObject is Mission mission2)
			{
				int num = (int)inUserData;
				Mission mission3 = ((mission2.MissionID == num) ? mission2 : mission2.GetMission(num));
				if (mission3 == null)
				{
					mission3 = MissionManager.pInstance.GetMission(num);
				}
				ShowUpdatedMission(mission3);
			}
			else
			{
				UtDebug.Log("WEB SERVICE CALL GetUserMissionState RETURNED NO DATA!!!", Mission.LOG_MASK);
				int missionID2 = (int)inUserData;
				Mission mission4 = MissionManager.pInstance.GetMission(missionID2);
				ShowUpdatedMission(mission4);
			}
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		case WsServiceEvent.ERROR:
		{
			Debug.LogError("WEB SERVICE CALL GetUserMissionState FAILED!!!");
			int missionID = (int)inUserData;
			Mission mission = MissionManager.pInstance.GetMission(missionID);
			ShowUpdatedMission(mission);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
		}
	}

	private void ShowUpdatedMission(Mission inMission)
	{
		if (inMission != null)
		{
			if (mScientificQuestMode)
			{
				ClearSubquests();
				SetupScientificQuestScreen(inMission);
				ShowScientificQuestDetails();
			}
			else
			{
				mCurrentSelection = new SubquestUserData();
				mCurrentSelection._Mission = inMission;
				mCurrentSelection._Type = RuleItemType.Mission;
				SetQuestDetails(inMission);
			}
		}
	}

	public void SetupScientificQuestScreen(Mission inScientificMission)
	{
		if (inScientificMission.MissionRule.Criteria.RuleItems.Count == 5)
		{
			foreach (RuleItem ruleItem in inScientificMission.MissionRule.Criteria.RuleItems)
			{
				if (ruleItem.Type == RuleItemType.Task)
				{
					Task task = inScientificMission.GetTask(ruleItem.ID);
					if (task != null)
					{
						AddSubquest(task);
					}
				}
				else if (mScientificQuestMode && ruleItem.Type == RuleItemType.Mission)
				{
					Mission mission = inScientificMission.GetMission(ruleItem.ID);
					if (mission != null)
					{
						AddSubquest(mission);
					}
				}
			}
			return;
		}
		int num = 0;
		foreach (RuleItem ruleItem2 in inScientificMission.MissionRule.Criteria.RuleItems)
		{
			num++;
			if (ruleItem2.Type == RuleItemType.Task)
			{
				Task task2 = inScientificMission.GetTask(ruleItem2.ID);
				KAWidget widgetForScientificPhase = GetWidgetForScientificPhase(task2);
				if (widgetForScientificPhase != null && task2 != null)
				{
					SubquestUserData subquestUserData = new SubquestUserData();
					subquestUserData._Task = task2;
					subquestUserData._Type = RuleItemType.Task;
					widgetForScientificPhase.SetUserData(subquestUserData);
					widgetForScientificPhase.SetState(KAUIState.INTERACTIVE);
				}
			}
			else if (mScientificQuestMode && ruleItem2.Type == RuleItemType.Mission)
			{
				Mission mission2 = inScientificMission.GetMission(ruleItem2.ID);
				KAWidget widgetForScientificPhase2 = GetWidgetForScientificPhase(mission2);
				if (widgetForScientificPhase2 != null && mission2 != null)
				{
					SubquestUserData subquestUserData2 = new SubquestUserData();
					subquestUserData2._Mission = mission2;
					subquestUserData2._Type = RuleItemType.Mission;
					widgetForScientificPhase2.SetUserData(subquestUserData2);
					widgetForScientificPhase2.SetState(KAUIState.INTERACTIVE);
				}
			}
		}
	}

	private KAWidget GetWidgetForScientificPhase(Task inTask)
	{
		if (inTask != null)
		{
			ScientificQuestPhaseInfo[] scientificQuestButtons = _ScientificQuestButtons;
			foreach (ScientificQuestPhaseInfo scientificQuestPhaseInfo in scientificQuestButtons)
			{
				if (inTask.Name.Contains(scientificQuestPhaseInfo._PhaseKeyword))
				{
					return scientificQuestPhaseInfo._PhaseButton;
				}
			}
		}
		return null;
	}

	private KAWidget GetWidgetForScientificPhase(Mission inMission)
	{
		if (inMission != null)
		{
			ScientificQuestPhaseInfo[] scientificQuestButtons = _ScientificQuestButtons;
			foreach (ScientificQuestPhaseInfo scientificQuestPhaseInfo in scientificQuestButtons)
			{
				if (inMission.Name.Contains(scientificQuestPhaseInfo._PhaseKeyword))
				{
					return scientificQuestPhaseInfo._PhaseButton;
				}
			}
		}
		return null;
	}

	public void ShowQuestDetails(int inMissionID)
	{
		Mission mission = MissionManager.pInstance.GetMission(inMissionID);
		ShowQuestDetails(mission);
	}

	public void OnNoActiveQuests()
	{
		if (mTxtQuestHeading != null)
		{
			mTxtQuestHeading.SetText(_NoQuestsText.GetLocalizedString());
			mTxtQuestHeading.SetVisibility(inVisible: true);
		}
	}

	public void ActivateUI(int uiIndex, bool addToList = true)
	{
	}

	public void Clear()
	{
	}

	public void Exit()
	{
	}

	public bool IsBusy()
	{
		return false;
	}

	public void ProcessClose()
	{
	}
}
