using System.Collections.Generic;
using UnityEngine;

public class UiNPCQuestDetails : KAUI
{
	public delegate void OnClose();

	public delegate void OnMissionAccept();

	public LocaleString _AcceptErrorMessageText = new LocaleString("An error occurred while accepting the task!");

	public RewardWidget _RewardWidget;

	public UiMissionActionDB _UIMissionAction;

	private KAWidget mTxtQuestHeading;

	private KAWidget mTxtObjective;

	private KAWidget mTxtStory;

	private OnClose mOnCloseHandler;

	private OnMissionAccept mOnMissionAcceptHandler;

	private Task mCurrentTask;

	private KAUIGenericDB mUiGenericDB;

	private bool mSetupComplete;

	private bool mIsInitialized;

	private List<AchievementReward> mMissionRewards;

	private int mMissionRewardsToLoad;

	private int mMissionRewardLoadCount;

	protected override void Start()
	{
		base.Start();
		Initialize();
		if (UtPlatform.IsiOS())
		{
			KAWidget kAWidget = FindItem("BtnAccept");
			KAWidget kAWidget2 = FindItem("BtnDecline");
			if (kAWidget != null && kAWidget2 != null)
			{
				Vector3 localPosition = kAWidget2.transform.localPosition;
				kAWidget2.transform.localPosition = kAWidget.transform.localPosition;
				kAWidget.transform.localPosition = localPosition;
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mIsInitialized && !mSetupComplete && mCurrentTask != null && mOnCloseHandler != null)
		{
			SetupDetailsUi();
		}
	}

	public void Initialize()
	{
		mTxtObjective = FindItem("TxtObjective");
		mTxtStory = FindItem("TxtStory");
		mTxtQuestHeading = FindItem("TxtQuestHeading");
		mIsInitialized = true;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "BtnBack" || inWidget.name == "BtnDecline")
		{
			GoBack();
		}
		else if (inWidget.name == "BtnAccept")
		{
			AcceptQuest();
		}
	}

	public void ShowTaskDetails(Task inTask, OnClose inOnCloseHandler, OnMissionAccept inOnMissionAcceptHandler = null)
	{
		mCurrentTask = inTask;
		mOnCloseHandler = inOnCloseHandler;
		mOnMissionAcceptHandler = inOnMissionAcceptHandler;
		if (mIsInitialized)
		{
			SetupDetailsUi();
		}
	}

	private void SetupDetailsUi()
	{
		if (mCurrentTask == null)
		{
			GoBack();
		}
		if (mTxtObjective != null)
		{
			mTxtObjective.SetText(GetTaskObjectives(mCurrentTask));
		}
		if (mTxtStory != null)
		{
			mTxtStory.SetText(GetTaskStory(mCurrentTask));
		}
		if (mTxtQuestHeading != null)
		{
			mTxtQuestHeading.SetText(GetTaskName(mCurrentTask));
		}
		Mission rootMission = MissionManager.pInstance.GetRootMission(mCurrentTask);
		KAWidget kAWidget = FindItem("BkgIcon");
		if (kAWidget != null)
		{
			string text = ((rootMission != null) ? rootMission.pData.Icon : mCurrentTask._Mission.pData.Icon);
			kAWidget.SetVisibility(inVisible: false);
			if (!string.IsNullOrEmpty(text))
			{
				if (text.StartsWith("http://"))
				{
					kAWidget.SetTextureFromURL(text, base.gameObject);
				}
				else
				{
					string[] array = text.Split('/');
					if (array.Length >= 3)
					{
						kAWidget.SetTextureFromBundle(array[0] + "/" + array[1], array[2], base.gameObject);
					}
				}
			}
		}
		mMissionRewards = new List<AchievementReward>();
		mMissionRewards.AddRange(rootMission.Rewards);
		FilterRewards(rootMission.Rewards);
		KAWidget kAWidget2 = FindItem("BtnAccept");
		bool active = mCurrentTask._Active;
		if (kAWidget2 != null)
		{
			kAWidget2.SetState(active ? KAUIState.DISABLED : KAUIState.INTERACTIVE);
		}
		List<MissionAction> offers = mCurrentTask.GetOffers(unplayed: false);
		if (offers != null && offers.Count > 0)
		{
			List<MissionAction> list = offers.FindAll((MissionAction o) => o.Type == MissionActionType.Popup || o.Type == MissionActionType.VO);
			if (list != null && list.Count > 0)
			{
				foreach (MissionAction item in list)
				{
					item._Played = false;
				}
				if (_UIMissionAction != null)
				{
					_UIMissionAction.SetVisibility(inVisible: true);
				}
				else
				{
					SetState(KAUIState.DISABLED);
				}
				MissionManager.pInstance.AddAction(list, null, MissionManager.ActionType.OFFER, OnOfferAction, forcedDoAction: true, _UIMissionAction);
			}
		}
		mSetupComplete = true;
	}

	private void FilterRewards(List<AchievementReward> inRewards)
	{
		if (inRewards.Find((AchievementReward t) => t.ItemID != 0) != null)
		{
			mMissionRewardsToLoad = inRewards.FindAll((AchievementReward t) => t.ItemID != 0).Count;
			for (int i = 0; i < inRewards.Count; i++)
			{
				if (inRewards[i].ItemID != 0)
				{
					ItemData.Load(inRewards[i].ItemID, OnRewardLoaded, i);
				}
			}
		}
		else
		{
			_RewardWidget.SetRewards(mMissionRewards.ToArray(), MissionManager.pInstance._RewardData);
		}
	}

	private void OnRewardLoaded(int itemID, ItemData dataItem, object inUserData)
	{
		int index = (int)inUserData;
		if (dataItem.HasAttribute("Gender"))
		{
			string text = "U";
			if (AvatarData.GetGender() == Gender.Male)
			{
				text = "F";
			}
			else if (AvatarData.GetGender() == Gender.Female)
			{
				text = "M";
			}
			if (dataItem.GetAttribute("Gender", "") == text)
			{
				mMissionRewards.RemoveAt(index);
			}
		}
		mMissionRewardLoadCount++;
		if (mMissionRewardLoadCount == mMissionRewardsToLoad)
		{
			_RewardWidget.SetRewards(mMissionRewards.ToArray(), MissionManager.pInstance._RewardData);
		}
	}

	private void OnTextureLoaded(KAWidget widget)
	{
		widget.SetVisibility(inVisible: true);
	}

	private void OnOfferAction(MissionActionEvent inEvent, object inObject)
	{
		if (!GetVisibility())
		{
			return;
		}
		if (inEvent == MissionActionEvent.POPUP_LOADED && inObject != null)
		{
			GameObject gameObject = inObject as GameObject;
			if (gameObject != null)
			{
				UiMissionActionDB componentInChildren = gameObject.GetComponentInChildren<UiMissionActionDB>();
				if (componentInChildren != null)
				{
					KAWidget kAWidget = componentInChildren.FindItem("CloseBtn");
					if (kAWidget != null)
					{
						kAWidget.gameObject.SetActive(value: false);
					}
				}
			}
			SetState(KAUIState.INTERACTIVE);
		}
		else if (inEvent == MissionActionEvent.VO_COMPLETE)
		{
			SetState(KAUIState.INTERACTIVE);
		}
	}

	private void AcceptQuest()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		SetState(KAUIState.DISABLED);
		MissionManager.pInstance.AcceptMission(mCurrentTask._Mission, AcceptMissionCallback);
	}

	private void AcceptMissionCallback(bool success, Mission mission)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		SetState(KAUIState.INTERACTIVE);
		if (success)
		{
			if (mission != null && !MissionManager.pInstance.GetRootMission(mission).pData.Hidden)
			{
				List<Task> tasks = new List<Task>();
				MissionManager.pInstance.GetNextTask(mission, ref tasks);
				if (tasks.Count > 0)
				{
					MissionManagerDO.SetCurrentActiveTask(tasks[0].TaskID);
				}
			}
			GoBack();
			mOnMissionAcceptHandler?.Invoke();
		}
		else
		{
			ShowError();
		}
	}

	public void ShowError()
	{
		if (!(UiErrorHandler.pInstance != null))
		{
			mUiGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "PfKAUIGenericDB");
			mUiGenericDB._MessageObject = base.gameObject;
			mUiGenericDB._CloseMessage = "OnAcceptMissionErrorClose";
			mUiGenericDB.SetText(_AcceptErrorMessageText.GetLocalizedString(), interactive: false);
			mUiGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: true);
			mUiGenericDB.SetDestroyOnClick(isDestroy: true);
			KAUI.SetExclusive(mUiGenericDB, new Color(0.5f, 0.5f, 0.5f, 0.5f));
		}
	}

	public void OnAcceptMissionErrorClose()
	{
		if (mUiGenericDB != null)
		{
			Object.Destroy(mUiGenericDB.gameObject);
			mUiGenericDB = null;
		}
		GoBack();
	}

	private void GoBack()
	{
		SetVisibility(inVisible: false);
		if (mOnCloseHandler != null)
		{
			mOnCloseHandler();
		}
		mOnCloseHandler = null;
		mCurrentTask = null;
		mSetupComplete = false;
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.gameObject.SendMessage("OnPopupOK", SendMessageOptions.DontRequireReceiver);
		}
		if (InteractiveTutManager._CurrentActiveTutorialObject != null)
		{
			InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", "AcceptDone");
			InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", "MessageBoardActionClosed");
		}
	}

	private string GetTaskName(Task inTask)
	{
		string localizedString = inTask.Name;
		if (!inTask._Mission.MissionRule.Criteria.Ordered && inTask._Mission.pData != null && inTask._Mission.pData.Title != null)
		{
			localizedString = inTask._Mission.pData.Title.GetLocalizedString();
		}
		else if (inTask.pData != null && inTask.pData.Title != null)
		{
			localizedString = inTask.pData.Title.GetLocalizedString();
		}
		Mission mission = null;
		for (Mission mission2 = inTask._Mission; mission2 != null; mission2 = mission2._Parent)
		{
			mission = mission2;
		}
		if (mission != null && mission.pData != null && mission.pData.Title != null)
		{
			localizedString = mission.pData.Title.GetLocalizedString();
		}
		return MissionManager.pInstance.FormatText(0, localizedString);
	}

	private string GetTaskObjectives(Task inTask)
	{
		string text = "";
		if (!inTask._Mission.MissionRule.Criteria.Ordered && inTask._Mission.MissionRule.Criteria.RuleItems != null && inTask._Mission.MissionRule.Criteria.RuleItems.Count > 1 && inTask._Mission.pData != null)
		{
			foreach (RuleItem ruleItem in inTask._Mission.MissionRule.Criteria.RuleItems)
			{
				if (ruleItem.Type == RuleItemType.Task)
				{
					Task task = inTask._Mission.GetTask(ruleItem.ID);
					if (task != null && task.pData != null && task.pData.Title != null)
					{
						text = text + task.pData.Title.GetLocalizedString() + "\n";
					}
				}
			}
		}
		else if (inTask != null && inTask.pData != null && inTask.pData.Title != null)
		{
			text = inTask.pData.Title.GetLocalizedString();
		}
		return MissionManager.pInstance.FormatText(0, text);
	}

	private string GetTaskStory(Task inTask)
	{
		if (inTask != null && inTask.pData != null && inTask.pData.Description != null)
		{
			return MissionManager.pInstance.FormatText(0, inTask.pData.Description.GetLocalizedString());
		}
		return "";
	}
}
