using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class MissionManagerDO : MissionManager, IUnlock
{
	[Serializable]
	public class GenderTag
	{
		public string _Tag;

		public LocaleString _MaleText;

		public LocaleString _FemaleText;
	}

	[Serializable]
	public class InputTag
	{
		public string _Tag;

		public LocaleString _OnlineText;

		public LocaleString _MobileText;
	}

	[Serializable]
	public class ScientificQuestPhaseInfo
	{
		public string _PhaseKeyword;

		public LocaleString _PhaseTitleText;
	}

	public enum SceneLockEvents
	{
		TASK_STARTED,
		MISSION_ACCEPTED,
		TASK_COMPLETE,
		MISSION_COMPLETE
	}

	[Serializable]
	public class SceneUnlockInfo
	{
		public string _SceneName;

		public int _ID;

		public LocaleString _TitleText;

		public SceneLockEvents _UnlockCondition;

		public string _ParentScene;

		public int _Priority;
	}

	[Serializable]
	public class NPCQuestInfo
	{
		public string _GameObjectName;

		public List<string> _Scenes;

		public int _MissionGroupID;

		public LocaleString _NPCName;
	}

	[Serializable]
	public class PetStageQuestData
	{
		public int _MissionID;

		public RaisedPetStage _StageRequired;
	}

	[Serializable]
	public class FUEMission
	{
		public int _MissionID;

		public int _VersionID;
	}

	[Serializable]
	public class MissionOverridePetMeter
	{
		public int _MissionID;

		public int _VersionID;

		public SanctuaryPetMeterType _MeterType;
	}

	[Serializable]
	public class QuestBranchMissions
	{
		public int _GroupId;

		public List<int> _MissionIds;
	}

	public class UpdateCompletedMissionData
	{
		public WsServiceEventHandler _Callback;

		public object _UserData;

		public UpdateCompletedMissionData(WsServiceEventHandler inCallback, object inUserData)
		{
			_Callback = inCallback;
			_UserData = inUserData;
		}
	}

	public const string ExperimentResultKey = "Lab_Result";

	public const string ExperimentResultIDKey = "Lab_Result_ID";

	public const string ExperimentImageUrlKey = "Lab_Image_Url";

	public static MissionArrow _QuestArrow = null;

	public string _TrackedTaskKeyName = "TrackedTask";

	public PortalSetting _PortalSetting;

	public NPCQuestInfo[] _NPCQuestInfo;

	public SceneUnlockInfo[] _SceneUnlockInfo;

	public PetStageQuestData[] _PetStageQuestData;

	public List<GenderTag> _GenderTag;

	public List<InputTag> _InputTag;

	public LocaleString _TrueText = new LocaleString("true");

	public LocaleString _FalseText = new LocaleString("false");

	public LocaleString _AcceptMissionText = new LocaleString("Accept {{MISSION}} mission to unlock this scene.");

	public LocaleString _CompleteMissionText = new LocaleString("Complete {{MISSION}} mission to unlock this scene.");

	public LocaleString _StartTaskText = new LocaleString("Start {{TASK}} task to unlock this scene.");

	public LocaleString _CompleteTaskText = new LocaleString("Complete {{TASK}} task to unlock this scene.");

	public ScientificQuestPhaseInfo[] _ScientificQuestPhases;

	public List<int> _HatchDragonTasks;

	public List<FUEMission> _FUEMissions;

	public List<MissionOverridePetMeter> _OverrideMissionPetMeters;

	public QuestBranchMissions _QuestBranchMissions;

	private List<AchievementReward> mCurrentRewards;

	private List<int> mPendingInventoryAdditions = new List<int>();

	private bool mCheckDragonTicketsPending;

	private static int mTrackedTaskID = -1;

	private AvAvatarState mLastAvatarState;

	private UnlockManager.OnSceneUnlockedCallBack mOnSceneUnlockedCallback;

	public override void Start()
	{
		base.Start();
		AddUnlockInfo();
	}

	protected override void Initialize()
	{
		base.Initialize();
		LoadCurrentActiveTask();
	}

	protected override void Clear()
	{
		base.Clear();
		mTrackedTaskID = -1;
	}

	public override void Update()
	{
		base.Update();
		UiToolbar uiToolbar = null;
		if (AvAvatar.pToolbar == null || !AvAvatar.pToolbar.activeInHierarchy || (uiToolbar = AvAvatar.pToolbar.GetComponent<UiToolbar>()) == null || !uiToolbar.enabled)
		{
			return;
		}
		UpdateInventoryAdditionStatus();
		if (mCheckDragonTicketsPending && mPendingInventoryAdditions.Count <= 0)
		{
			mCheckDragonTicketsPending = false;
			if (!CheckDragonTickets(mCurrentRewards))
			{
				CheckCustomArmorSuit();
			}
		}
	}

	private void UpdateInventoryAdditionStatus()
	{
		for (int num = mPendingInventoryAdditions.Count - 1; num >= 0; num--)
		{
			if (CommonInventoryData.pInstance.FindItem(mPendingInventoryAdditions[num]) != null)
			{
				mPendingInventoryAdditions.RemoveAt(num);
			}
		}
	}

	public override bool AutoCompleteTask(Task task)
	{
		if (task.pData.AutoComplete != null)
		{
			for (int i = 0; i < task.pData.AutoComplete.Pairs.Count; i++)
			{
				TaskPair taskPair = task.pData.AutoComplete.Pairs[i];
				switch (taskPair.Key)
				{
				case "RaisedPetStage":
					if (SanctuaryManager.pCurPetData != null)
					{
						RaisedPetStage raisedPetStage = (RaisedPetStage)Enum.Parse(typeof(RaisedPetStage), taskPair.Value, ignoreCase: true);
						if (SanctuaryManager.pCurPetData.pStage >= raisedPetStage)
						{
							return true;
						}
					}
					break;
				case "Mounted":
					if (SanctuaryManager.pCurPetInstance != null)
					{
						bool flag = UtStringUtil.Parse(taskPair.Value, inDefault: false);
						if (SanctuaryManager.pCurPetInstance.pIsMounted == flag)
						{
							return true;
						}
					}
					break;
				case "CategoryItem":
				{
					int num = UtStringUtil.Parse(taskPair.Value, 0);
					if (num > 0)
					{
						UserItemData[] items = CommonInventoryData.pInstance.GetItems(num);
						if (items != null && items.Length != 0)
						{
							return true;
						}
					}
					break;
				}
				}
			}
		}
		else
		{
			string type = task.pData.Type;
			if (type == "Action")
			{
				bool result = false;
				for (int j = 0; j < task.pData.Objectives.Count; j++)
				{
					TaskObjective taskObjective = task.pData.Objectives[j];
					string text = taskObjective.Get<string>("Name");
					if (text == "CompleteTutorial" && ProductData.TutorialComplete(taskObjective.Get<string>("ItemName")))
					{
						result = true;
					}
					else if (text == "JoinClan" && UserProfile.pProfileData.HasGroup())
					{
						result = true;
					}
				}
				return result;
			}
			if (type == "Collect")
			{
				bool result2 = true;
				foreach (TaskObjective objective in task.pData.Objectives)
				{
					string text2 = objective.Get<string>("Name");
					int num2 = task.pPayload.Get<int>(text2 + "CollectedCount");
					if (!task.CheckForCompletion("Collect", text2, num2, ""))
					{
						result2 = false;
						break;
					}
				}
				return result2;
			}
		}
		return base.AutoCompleteTask(task);
	}

	public override bool CanEngage(MissionAction inAction)
	{
		return !UiJournal.pIsJournalActive;
	}

	public override string FormatText(int inTextID, string defaultText, Task inTask = null)
	{
		string text = base.FormatText(inTextID, defaultText, inTask);
		for (int i = 0; i < _GenderTag.Count; i++)
		{
			GenderTag genderTag = _GenderTag[i];
			if (!string.IsNullOrEmpty(genderTag._Tag))
			{
				text = text.Replace(genderTag._Tag, (AvatarData.GetGender() == Gender.Female) ? genderTag._FemaleText.GetLocalizedString() : genderTag._MaleText.GetLocalizedString());
			}
		}
		for (int j = 0; j < _InputTag.Count; j++)
		{
			InputTag inputTag = _InputTag[j];
			if (!string.IsNullOrEmpty(inputTag._Tag))
			{
				string text2 = (KAInput.pInstance.IsTouchInput() ? inputTag._MobileText.GetLocalizedString() : inputTag._OnlineText.GetLocalizedString());
				text = text.Replace(inputTag._Tag, text2);
				string oldValue = CultureInfo.CurrentCulture.TextInfo.ToTitleCased(inputTag._Tag);
				text2 = CultureInfo.CurrentCulture.TextInfo.ToTitleCased(text2);
				text = text.Replace(oldValue, text2);
			}
		}
		text = text.Replace("{{Name}}", AvatarData.pInstance.DisplayName);
		if (SanctuaryManager.pCurPetData != null)
		{
			text = text.Replace("{{dragon name}}", SanctuaryManager.pCurPetData.Name);
		}
		return text;
	}

	public override string FormatActionText(int inTextID, string defaultText)
	{
		string text = base.FormatActionText(inTextID, defaultText);
		Mission inMission = null;
		if (mActions[0]._Object is Task)
		{
			inMission = GetRootMission(mActions[0]._Object as Task);
		}
		else if (mActions[0]._Object is Mission)
		{
			inMission = GetRootMission(mActions[0]._Object as Mission);
		}
		Task hypothesisTask = GetHypothesisTask(inMission);
		if (hypothesisTask != null)
		{
			string value = hypothesisTask.pPayload.Get<string>(UiMissionHypothesisDB._HypothesisCorrectKey);
			bool result = false;
			if (!string.IsNullOrEmpty(value) && bool.TryParse(value, out result))
			{
				text = text.Replace("{{result}}", result ? _TrueText.GetLocalizedString() : _FalseText.GetLocalizedString());
			}
		}
		return text;
	}

	protected override bool CanResumeTasks()
	{
		if (PetRankData.pIsReady)
		{
			return base.CanResumeTasks();
		}
		return false;
	}

	public override bool IsLocked(Mission mission)
	{
		if (!Mission.pLocked)
		{
			return false;
		}
		for (int i = 0; i < _PetStageQuestData.Length; i++)
		{
			PetStageQuestData petStageQuestData = _PetStageQuestData[i];
			if (petStageQuestData._MissionID == mission.MissionID)
			{
				RaisedPetData currentPetData = GetCurrentPetData();
				if (currentPetData == null || currentPetData.pStage != petStageQuestData._StageRequired)
				{
					return true;
				}
			}
		}
		return base.IsLocked(mission);
	}

	public override UserAchievementInfo GetUserAchievementInfoByType(int pointType)
	{
		RaisedPetData currentPetData = GetCurrentPetData();
		if (pointType == 8)
		{
			return PetRankData.GetUserAchievementInfo(currentPetData);
		}
		return base.GetUserAchievementInfoByType(pointType);
	}

	protected override void OnMissionAcceptComplete(Mission mission)
	{
		if (mission.AcceptanceRewards == null)
		{
			return;
		}
		for (int i = 0; i < mission.AcceptanceRewards.Count; i++)
		{
			AchievementReward achievementReward = mission.AcceptanceRewards[i];
			if (achievementReward.PointTypeID.Value == 6)
			{
				mPendingInventoryAdditions.Add(achievementReward.ItemID);
			}
		}
		mCurrentRewards = mission.AcceptanceRewards;
		mCheckDragonTicketsPending = true;
	}

	public bool OverridePetMeter(SanctuaryPetMeterType type)
	{
		bool result = false;
		foreach (MissionOverridePetMeter overrideMissionPetMeter in _OverrideMissionPetMeters)
		{
			if (type == overrideMissionPetMeter._MeterType)
			{
				Mission mission = MissionManager.pInstance.GetMission(overrideMissionPetMeter._MissionID);
				if (mission != null && mission.pStarted && !mission.pCompleted && mission.VersionID >= overrideMissionPetMeter._VersionID)
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	public override void ShowMissionResults(Mission mission)
	{
		MissionCompletedResult missionCompletedResult = null;
		if (mission != null && mMissionResults.ContainsKey(mission.MissionID))
		{
			missionCompletedResult = mMissionResults[mission.MissionID];
		}
		base.ShowMissionResults(mission);
		if (missionCompletedResult == null || missionCompletedResult.Rewards == null)
		{
			return;
		}
		for (int i = 0; i < missionCompletedResult.Rewards.Length; i++)
		{
			AchievementReward achievementReward = missionCompletedResult.Rewards[i];
			if (achievementReward.PointTypeID.Value == 6)
			{
				mPendingInventoryAdditions.Add(achievementReward.ItemID);
			}
		}
		mCurrentRewards = new List<AchievementReward>(missionCompletedResult.Rewards);
		if (mission.pData.Reward == null || string.IsNullOrEmpty(mission.pData.Reward.Asset))
		{
			mCheckDragonTicketsPending = true;
		}
	}

	protected override void OnDependencyTasksStarted(List<Task> tasks)
	{
		if (tasks != null && tasks.Count > 0)
		{
			SetCurrentActiveTask(tasks[0].TaskID);
		}
	}

	protected override void OnRewardClose()
	{
		base.OnRewardClose();
		mCheckDragonTicketsPending = true;
	}

	public override void Activate(Task inTask)
	{
		base.Activate(inTask);
		if (_QuestArrow != null)
		{
			_QuestArrow.SetupTarget();
		}
	}

	public bool CompletedHatchTask()
	{
		if (_HatchDragonTasks != null)
		{
			for (int i = 0; i < _HatchDragonTasks.Count; i++)
			{
				Task task = GetTask(_HatchDragonTasks[i]);
				if (task == null || !task.pCompleted)
				{
					continue;
				}
				for (int j = 0; j < base.pActiveTasks.Count; j++)
				{
					if (task.TaskID == base.pActiveTasks[j].TaskID)
					{
						return false;
					}
				}
				return true;
			}
		}
		return false;
	}

	private bool CheckDragonTickets(List<AchievementReward> achievementRewards)
	{
		if (achievementRewards != null && achievementRewards.Count > 0 && UserNotifyDragonTicket.pInstance != null)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < achievementRewards.Count; i++)
			{
				AchievementReward achievementReward = achievementRewards[i];
				if (achievementReward.PointTypeID.Value == 6)
				{
					list.Add(achievementReward.ItemID);
				}
			}
			if (list.Count > 0)
			{
				return UserNotifyDragonTicket.pInstance.CheckTickets(list, OnDragonTicketProcessed);
			}
		}
		return false;
	}

	private void OnDragonTicketProcessed(bool success)
	{
		CheckCustomArmorSuit();
	}

	private void CheckCustomArmorSuit()
	{
		if (mCurrentRewards != null && mCurrentRewards.Count > 0)
		{
			List<int> rewardItemIdList = new List<int>();
			for (int i = 0; i < mCurrentRewards.Count; i++)
			{
				AchievementReward achievementReward = mCurrentRewards[i];
				if (achievementReward.PointTypeID.Value == 6)
				{
					rewardItemIdList.Add(achievementReward.ItemID);
				}
			}
			if (CommonInventoryData.pIsReady && rewardItemIdList.Count > 0)
			{
				UserItemData[] items = CommonInventoryData.pInstance.GetItems(657);
				if (items != null && items.Length != 0)
				{
					UserItemData[] array = Array.FindAll(items, (UserItemData userItemData) => rewardItemIdList.Contains(userItemData.Item.ItemID));
					if (array != null && array.Length != 0)
					{
						AvAvatar.pState = AvAvatarState.PAUSED;
						AvAvatar.SetUIActive(inActive: false);
						UiAvatarItemCustomization.Init(array, null, OnCloseItemCustomization, multiItemCustomizationUI: false);
					}
				}
			}
		}
		mCurrentRewards = null;
	}

	public void OnCloseItemCustomization(KAUISelectItemData inItem)
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
	}

	private RaisedPetData GetCurrentPetData()
	{
		RaisedPetData raisedPetData = null;
		if (RsResourceManager.pCurrentLevel == "ProfileSelectionDO" && ProfilePetManager.pCurPetData != null)
		{
			return ProfilePetManager.pCurPetData;
		}
		return SanctuaryManager.pCurPetData;
	}

	public static Task GetPlayerActiveTask()
	{
		Task task = null;
		if (MissionManager.pInstance != null && MissionManager.pIsReady && mTrackedTaskID != -1)
		{
			task = MissionManager.pInstance.GetTask(mTrackedTaskID);
			if (task != null && !task._Active && MissionManager.pInstance.pPreviousTask == null)
			{
				Mission rootMission = MissionManager.pInstance.GetRootMission(task);
				List<Task> tasks = new List<Task>();
				MissionManager.pInstance.GetNextTask(rootMission, ref tasks);
				if (tasks.Count > 0)
				{
					for (int i = 0; i < tasks.Count; i++)
					{
						Task task2 = tasks[i];
						if (task2._Active || task2.pActivateOnClick)
						{
							SetCurrentActiveTask(task2.TaskID);
							return task2;
						}
					}
				}
				task = null;
			}
			if (task == null && NPCAvatar._Engaged == null)
			{
				SetCurrentActiveTask(-1);
			}
		}
		return task;
	}

	public override Task GetActiveTask(GameObject inObject)
	{
		Task task = GetPlayerActiveTask();
		if (task == null)
		{
			task = GetNextActiveTask();
		}
		if (task != null && task.IsTaskObject(inObject))
		{
			return task;
		}
		return base.GetActiveTask(inObject);
	}

	public static Task GetNextActiveTask()
	{
		if (MissionManager.pInstance != null && MissionManager.pIsReady)
		{
			for (int i = 0; i < MissionManager.pInstance.pActiveTasks.Count; i++)
			{
				Task task = MissionManager.pInstance.pActiveTasks[i];
				Mission rootMission = MissionManager.pInstance.GetRootMission(task);
				if (rootMission != null && !rootMission.pData.Hidden)
				{
					return task;
				}
			}
		}
		return null;
	}

	public static void LoadCurrentActiveTask()
	{
		if (ProductData.pPairData != null)
		{
			mTrackedTaskID = ProductData.pPairData.GetIntValue(((MissionManagerDO)MissionManager.pInstance)._TrackedTaskKeyName, -1);
		}
	}

	public static void SetCurrentActiveTask(int inTaskID, bool waitForRefresh = false)
	{
		if (ProductData.pPairData != null && mTrackedTaskID != inTaskID)
		{
			ProductData.pPairData.SetValueAndSave(((MissionManagerDO)MissionManager.pInstance)._TrackedTaskKeyName, inTaskID.ToString());
			mTrackedTaskID = inTaskID;
			if (waitForRefresh && ((MissionManagerDO)MissionManager.pInstance).mRefreshMissions)
			{
				MissionManager.pInstance.StopCoroutine("SetCurrentActiveTaskAfterRefresh");
				MissionManager.pInstance.StartCoroutine("SetCurrentActiveTaskAfterRefresh");
			}
			else
			{
				_QuestArrow?.SetupTarget();
			}
		}
	}

	private IEnumerator SetCurrentActiveTaskAfterRefresh()
	{
		int taskID = mTrackedTaskID;
		while (mRefreshMissions)
		{
			yield return new WaitForEndOfFrame();
		}
		SetCurrentActiveTask(taskID);
	}

	public static string GetPortalToScene(string sceneFrom, string sceneTo)
	{
		if (((MissionManagerDO)MissionManager.pInstance)._PortalSetting == null)
		{
			return string.Empty;
		}
		return ((MissionManagerDO)MissionManager.pInstance)._PortalSetting.FindPortalToScene(sceneFrom, sceneTo);
	}

	public static Task GetNewQuest()
	{
		return MissionManager.pInstance.GetTasks(-1)?.Find((Task x) => !x._Mission.Repeatable && !x.pPayload.Started);
	}

	public static GameObject GetNearestNPCWithAvailableTask()
	{
		List<GameObject> nPCsForAvailableTask = GetNPCsForAvailableTask();
		if (nPCsForAvailableTask.Count > 0 && _QuestArrow != null)
		{
			return _QuestArrow.GetNearestTarget(nPCsForAvailableTask);
		}
		return null;
	}

	public static string GetSceneForNPC(int groupId)
	{
		NPCQuestInfo nPCQuestInfo = Array.Find((MissionManager.pInstance as MissionManagerDO)._NPCQuestInfo, (NPCQuestInfo q) => q._MissionGroupID == groupId);
		if (nPCQuestInfo != null && nPCQuestInfo._Scenes.Count > 0)
		{
			return nPCQuestInfo._Scenes[0];
		}
		return string.Empty;
	}

	public static HashSet<string> GetAllPortalsInScene(string scene)
	{
		if (((MissionManagerDO)MissionManager.pInstance)._PortalSetting == null)
		{
			return new HashSet<string>();
		}
		return ((MissionManagerDO)MissionManager.pInstance)._PortalSetting.GetAllPortalsInScene(scene);
	}

	public static bool IsCollectTaskInCurrentScene(Task inTask)
	{
		List<TaskSetup> setups = inTask.GetSetups();
		if (inTask.pData != null && inTask.pData.Type == "Collect" && setups != null)
		{
			for (int i = 0; i < setups.Count; i++)
			{
				TaskSetup taskSetup = setups[i];
				if (RsResourceManager.pCurrentLevel.Equals(taskSetup.Scene))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static GameObject GetTaskTargetInCurrentScene(out Task outTask)
	{
		GameObject gameObject = null;
		outTask = null;
		if (MissionManager.pIsReady && MissionManager.pInstance != null && MissionManager.pInstance.pActiveTasks != null && MissionManager.pInstance.pActiveTasks.Count > 0)
		{
			for (int i = 0; i < MissionManager.pInstance.pActiveTasks.Count; i++)
			{
				Task task = MissionManager.pInstance.pActiveTasks[i];
				if (MissionManager.pInstance.GetRootMission(task).pData.Hidden)
				{
					continue;
				}
				if (IsCollectTaskInCurrentScene(task))
				{
					gameObject = GetCollectiblesForTask(task);
					if (gameObject != null)
					{
						outTask = task;
						break;
					}
				}
				gameObject = GetTargetForTask(task);
				if (gameObject != null)
				{
					outTask = task;
					return gameObject;
				}
			}
		}
		return gameObject;
	}

	public static List<Task> GetTasksInCurrentScene()
	{
		List<Task> list = new List<Task>();
		if (MissionManager.pIsReady && MissionManager.pInstance != null && MissionManager.pInstance.pActiveTasks != null && MissionManager.pInstance.pActiveTasks.Count > 0)
		{
			for (int i = 0; i < MissionManager.pInstance.pActiveTasks.Count; i++)
			{
				Task task = MissionManager.pInstance.pActiveTasks[i];
				if (MissionManager.pInstance.GetRootMission(task).pData.Hidden)
				{
					continue;
				}
				if (IsCollectTaskInCurrentScene(task))
				{
					list.Add(task);
				}
				else
				{
					if (task.pData == null || task.pData.Objectives == null)
					{
						continue;
					}
					for (int j = 0; j < task.pData.Objectives.Count; j++)
					{
						TaskObjective taskObjective = task.pData.Objectives[j];
						string value = taskObjective.Get<string>("Scene");
						if (RsResourceManager.pCurrentLevel.Equals(value))
						{
							list.Add(task);
						}
						else if (GameObject.Find(taskObjective.Get<string>("NPC")) != null)
						{
							list.Add(task);
						}
					}
				}
			}
		}
		return list;
	}

	public static List<Task> GetTasksAccessibleFromCurrentScene()
	{
		List<Task> list = new List<Task>();
		if (MissionManager.pIsReady && MissionManager.pInstance != null && MissionManager.pInstance.pActiveTasks != null && MissionManager.pInstance.pActiveTasks.Count > 0)
		{
			for (int i = 0; i < MissionManager.pInstance.pActiveTasks.Count; i++)
			{
				Task task = MissionManager.pInstance.pActiveTasks[i];
				if (MissionManager.pInstance.GetRootMission(task).pData.Hidden || task.pData == null || task.pData.Objectives == null)
				{
					continue;
				}
				for (int j = 0; j < task.pData.Objectives.Count; j++)
				{
					string sceneTo = task.pData.Objectives[j].Get<string>("Scene");
					if (!string.IsNullOrEmpty(GetPortalToScene(RsResourceManager.pCurrentLevel, sceneTo)))
					{
						list.Add(task);
					}
				}
			}
		}
		return list;
	}

	public static bool IsNPCQuestRewardAvailable(string inNPCName)
	{
		if (MissionManager.pInstance != null && MissionManager.pInstance.pActiveTasks != null)
		{
			for (int i = 0; i < MissionManager.pInstance.pActiveTasks.Count; i++)
			{
				Task task = MissionManager.pInstance.pActiveTasks[i];
				if (task.pData == null || task.pData.Objectives == null)
				{
					continue;
				}
				for (int j = 0; j < task.pData.Objectives.Count; j++)
				{
					TaskObjective objective = task.pData.Objectives[j];
					if (IsNPCRewardAvailable(task, objective, inNPCName))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private static bool IsNPCRewardAvailable(Task task, TaskObjective objective, string inNPCName)
	{
		if (task != null && task.pData != null && (task.pData.Type == "Meet" || task.pData.Type == "Delivery" || task.pData.Type == "Escort" || task.pData.Type == "Follow"))
		{
			if ((task.pData.Type == "Follow" || task.pData.Type == "Escort") && !objective.Get<string>("Scene").Equals(RsResourceManager.pCurrentLevel))
			{
				return false;
			}
			return objective.Get<string>("NPC").Contains(inNPCName);
		}
		return false;
	}

	public static GameObject GetCollectiblesForTask(Task inTask)
	{
		GameObject gameObject = null;
		float num = float.MaxValue;
		GameObject gameObject2 = null;
		if (inTask != null && inTask.pData != null && inTask.pData.Type == "Collect")
		{
			List<TaskSetup> setups = inTask.GetSetups();
			if (setups != null && setups.Count > 0)
			{
				for (int i = 0; i < setups.Count; i++)
				{
					TaskSetup taskSetup = setups[i];
					if (taskSetup.pObject != null)
					{
						ObCollect[] componentsInChildren = taskSetup.pObject.GetComponentsInChildren<ObCollect>();
						if (componentsInChildren == null || componentsInChildren.Length == 0)
						{
							continue;
						}
						for (int j = 0; j < componentsInChildren.Length; j++)
						{
							if (!componentsInChildren[j].pCollected)
							{
								float num2 = Vector3.Distance(AvAvatar.position, componentsInChildren[j].transform.position);
								if (num2 < num)
								{
									gameObject = componentsInChildren[j].gameObject;
									num = num2;
								}
							}
						}
					}
					else if (!string.IsNullOrEmpty(taskSetup.Scene) && gameObject2 == null)
					{
						string portalToScene = GetPortalToScene(RsResourceManager.pCurrentLevel, taskSetup.Scene);
						if (!string.IsNullOrEmpty(portalToScene))
						{
							gameObject2 = GameObject.Find(portalToScene);
						}
					}
				}
			}
			else
			{
				for (int k = 0; k < inTask.pData.Objectives.Count; k++)
				{
					gameObject = GameObject.Find(inTask.pData.Objectives[k].Get<string>("Name"));
					if (gameObject != null)
					{
						break;
					}
				}
			}
		}
		if (gameObject == null)
		{
			gameObject = gameObject2;
		}
		return gameObject;
	}

	public static GameObject GetTargetForTask(Task inTask)
	{
		GameObject gameObject = null;
		if (inTask != null && inTask.pData != null && inTask.pData.Objectives != null && inTask.pData.Objectives.Count > 0)
		{
			for (int i = 0; i < inTask.pData.Objectives.Count; i++)
			{
				TaskObjective taskObjective = inTask.pData.Objectives[i];
				string text = taskObjective.Get<string>("Scene");
				string text2 = taskObjective.Get<string>("Location");
				if ((string.IsNullOrEmpty(text) || text.Equals(RsResourceManager.pCurrentLevel) || (inTask.pData.Type == "Visit" && string.IsNullOrEmpty(taskObjective.Get<string>("Name")))) && !string.IsNullOrEmpty(text2))
				{
					gameObject = ((!(text2 == "MyDragon") || !(SanctuaryManager.pCurPetInstance != null)) ? GameObject.Find(text2) : SanctuaryManager.pCurPetInstance.gameObject);
					if (gameObject != null)
					{
						break;
					}
				}
				else if ((inTask.pData.Type == "Follow" || inTask.pData.Type == "Escort") && !string.IsNullOrEmpty(text) && !text.Equals(RsResourceManager.pCurrentLevel))
				{
					break;
				}
				if (inTask.pIsTimedTask && !inTask._Active)
				{
					string value = taskObjective.Get<string>("StartObject");
					if (!string.IsNullOrEmpty(value))
					{
						gameObject = GameObject.Find(value);
						if (gameObject != null)
						{
							break;
						}
					}
				}
				string value2 = taskObjective.Get<string>("NPC");
				if (inTask.pData.Type == "Visit" || (inTask.pData.Type == "Chase" && inTask._Active) || ((inTask.pData.Type == "Follow" || inTask.pData.Type == "Escort") && inTask._Active))
				{
					value2 = taskObjective.Get<string>("Name");
				}
				if (!string.IsNullOrEmpty(value2) && (string.IsNullOrEmpty(text) || text.Equals(RsResourceManager.pCurrentLevel)))
				{
					gameObject = GameObject.Find(value2);
					if (gameObject != null)
					{
						break;
					}
				}
			}
		}
		return gameObject;
	}

	public static GameObject GetPortalForTask(Task inTask)
	{
		GameObject result = null;
		if (inTask != null && inTask.pData != null && inTask.pData.Objectives != null && inTask.pData.Objectives.Count > 0)
		{
			for (int i = 0; i < inTask.pData.Objectives.Count; i++)
			{
				string sceneTo = inTask.pData.Objectives[i].Get<string>("Scene");
				string portalToScene = GetPortalToScene(RsResourceManager.pCurrentLevel, sceneTo);
				if (!string.IsNullOrEmpty(portalToScene))
				{
					result = GameObject.Find(portalToScene);
				}
			}
		}
		return result;
	}

	public static List<Task> GetTasks(int groupID, string inTaskType, string inKey, string inValue)
	{
		List<Task> list = new List<Task>();
		List<Mission> allMissions = MissionManager.pInstance.GetAllMissions(groupID);
		if (allMissions != null)
		{
			for (int i = 0; i < allMissions.Count; i++)
			{
				list.AddRange(GetTasks(allMissions[i], inTaskType, inKey, inValue));
			}
		}
		return list;
	}

	public static List<Task> GetTasks(Mission inMission, string inTaskType, string inKey, string inValue)
	{
		List<Task> list = new List<Task>();
		if (inMission.Tasks != null)
		{
			for (int i = 0; i < inMission.Tasks.Count; i++)
			{
				Task task = inMission.Tasks[i];
				if (task.Match(inTaskType, inKey, inValue))
				{
					list.Add(task);
				}
			}
		}
		if (inMission.Missions != null)
		{
			for (int j = 0; j < inMission.Missions.Count; j++)
			{
				List<Task> tasks = GetTasks(inMission.Missions[j], inTaskType, inKey, inValue);
				list.AddRange(tasks);
			}
		}
		return list;
	}

	public static string GetNPCName(string gameObjectName)
	{
		string result = gameObjectName;
		MissionManagerDO missionManagerDO = MissionManager.pInstance as MissionManagerDO;
		if (missionManagerDO != null && missionManagerDO._NPCQuestInfo != null && missionManagerDO._NPCQuestInfo.Length != 0)
		{
			NPCQuestInfo nPCQuestInfo = Array.Find(missionManagerDO._NPCQuestInfo, (NPCQuestInfo q) => q._GameObjectName == gameObjectName);
			if (nPCQuestInfo != null)
			{
				result = missionManagerDO.FormatText(nPCQuestInfo._NPCName._ID, nPCQuestInfo._NPCName._Text);
			}
		}
		return result;
	}

	public static string GetNPCObjectName(int groupID)
	{
		string result = "";
		MissionManagerDO missionManagerDO = MissionManager.pInstance as MissionManagerDO;
		if (missionManagerDO != null && groupID != 0 && missionManagerDO._NPCQuestInfo != null && missionManagerDO._NPCQuestInfo.Length != 0)
		{
			NPCQuestInfo nPCQuestInfo = Array.Find(missionManagerDO._NPCQuestInfo, (NPCQuestInfo q) => q._MissionGroupID == groupID);
			if (nPCQuestInfo != null)
			{
				result = nPCQuestInfo._GameObjectName;
			}
		}
		return result;
	}

	public void AddUnlockInfo()
	{
		UnlockManager.Add(this);
	}

	public void RemoveUnlockInfo()
	{
		UnlockManager.Remove(this);
	}

	public bool IsSceneUnlocked(string sceneName, bool inShowUi = false, UnlockManager.OnSceneUnlockedCallBack onSceneUnlocked = null)
	{
		if (MissionManager.pInstance == null)
		{
			return false;
		}
		SceneUnlockInfo[] array = Array.FindAll(_SceneUnlockInfo, (SceneUnlockInfo x) => x._SceneName.Equals(sceneName) && x._ID > 0);
		if (array == null || array.Length == 0)
		{
			return true;
		}
		string text = "";
		foreach (SceneUnlockInfo sceneUnlockInfo in array)
		{
			switch (sceneUnlockInfo._UnlockCondition)
			{
			case SceneLockEvents.MISSION_COMPLETE:
			{
				Mission mission2 = MissionManager.pInstance.GetMission(sceneUnlockInfo._ID);
				if (mission2 != null && !mission2.pCompleted)
				{
					if (!inShowUi)
					{
						text = _CompleteMissionText.GetLocalizedString();
						text = text.Replace("{{MISSION}}", FormatText(sceneUnlockInfo._TitleText._ID, sceneUnlockInfo._TitleText.GetLocalizedString()));
					}
					break;
				}
				return true;
			}
			case SceneLockEvents.MISSION_ACCEPTED:
			{
				Mission mission = MissionManager.pInstance.GetMission(sceneUnlockInfo._ID);
				if (mission != null && mission.pMustAccept && !mission.Accepted)
				{
					if (!inShowUi)
					{
						text = _AcceptMissionText.GetLocalizedString();
						text = text.Replace("{{MISSION}}", FormatText(sceneUnlockInfo._TitleText._ID, sceneUnlockInfo._TitleText.GetLocalizedString()));
					}
					break;
				}
				return true;
			}
			case SceneLockEvents.TASK_COMPLETE:
			{
				Task task2 = MissionManager.pInstance.GetTask(sceneUnlockInfo._ID);
				if (task2 != null && !task2.pCompleted)
				{
					if (!inShowUi)
					{
						text = _CompleteTaskText.GetLocalizedString();
						text = text.Replace("{{TASK}}", FormatText(sceneUnlockInfo._TitleText._ID, sceneUnlockInfo._TitleText.GetLocalizedString()));
					}
					break;
				}
				return true;
			}
			case SceneLockEvents.TASK_STARTED:
			{
				Task task = MissionManager.pInstance.GetTask(sceneUnlockInfo._ID);
				if (task != null && !task.pStarted && !task.pCompleted)
				{
					if (!inShowUi)
					{
						text = _StartTaskText.GetLocalizedString();
						text = text.Replace("{{TASK}}", FormatText(sceneUnlockInfo._TitleText._ID, sceneUnlockInfo._TitleText.GetLocalizedString()));
					}
					break;
				}
				return true;
			}
			}
		}
		if (!inShowUi)
		{
			DisplayOKMessage(text, onSceneUnlocked);
		}
		return false;
	}

	public string UpdatedScene(string scene, List<string> taskObjectiveScenes)
	{
		if (taskObjectiveScenes.Count > 0)
		{
			for (int i = 0; i < taskObjectiveScenes.Count; i++)
			{
				if (taskObjectiveScenes[i].Equals(scene))
				{
					return scene;
				}
				if (IsSceneRelated(taskObjectiveScenes[i], scene) && UnlockManager.IsSceneUnlocked(taskObjectiveScenes[i], inShowUi: true))
				{
					return taskObjectiveScenes[i];
				}
			}
		}
		SceneUnlockInfo[] array = Array.FindAll(_SceneUnlockInfo, (SceneUnlockInfo x) => x._ParentScene.Equals(scene) && x._Priority >= 0 && UnlockManager.IsSceneUnlocked(x._SceneName, inShowUi: true));
		if (array != null && array.Length != 0)
		{
			SceneUnlockInfo sceneUnlockInfo = array[0];
			for (int j = 1; j < array.Length; j++)
			{
				if (array[j]._Priority > sceneUnlockInfo._Priority)
				{
					sceneUnlockInfo = array[j];
				}
			}
			return sceneUnlockInfo._SceneName;
		}
		return scene;
	}

	public List<string> GetTaskObjectiveScenes()
	{
		List<string> list = new List<string>();
		Task playerActiveTask = GetPlayerActiveTask();
		if (playerActiveTask != null)
		{
			for (int i = 0; i < playerActiveTask.pData.Objectives.Count; i++)
			{
				string text = playerActiveTask.pData.Objectives[i].Get<string>("Scene");
				if (!string.IsNullOrEmpty(text))
				{
					list.Add(text);
				}
			}
		}
		return list;
	}

	private bool IsSceneRelated(string taskObjectiveScene, string scene)
	{
		SceneUnlockInfo sceneUnlockInfo = Array.Find(_SceneUnlockInfo, (SceneUnlockInfo x) => x._SceneName.Equals(taskObjectiveScene));
		SceneUnlockInfo sceneUnlockInfo2 = Array.Find(_SceneUnlockInfo, (SceneUnlockInfo x) => x._SceneName.Equals(scene));
		if (sceneUnlockInfo != null && !string.IsNullOrEmpty(sceneUnlockInfo._ParentScene) && sceneUnlockInfo._ParentScene.Equals(scene))
		{
			return true;
		}
		if (sceneUnlockInfo2 != null && taskObjectiveScene.Equals(sceneUnlockInfo2._ParentScene))
		{
			return true;
		}
		return false;
	}

	private void DisplayOKMessage(string text, UnlockManager.OnSceneUnlockedCallBack onSceneUnlocked)
	{
		mOnSceneUnlockedCallback = onSceneUnlocked;
		GameUtilities.DisplayOKMessage("PfKAUIGenericDB", text, base.gameObject, "OnDBClose");
		if (AvAvatar.pState != AvAvatarState.PAUSED)
		{
			mLastAvatarState = AvAvatar.pState;
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
		}
	}

	private void OnDBClose()
	{
		mOnSceneUnlockedCallback?.Invoke(success: false);
		if (mLastAvatarState != 0)
		{
			AvAvatar.SetUIActive(inActive: true);
			AvAvatar.pState = mLastAvatarState;
			mLastAvatarState = AvAvatarState.NONE;
		}
	}

	public static List<GameObject> GetNPCsForAvailableTask()
	{
		List<Task> tasks = MissionManager.pInstance.GetTasks(-1);
		List<GameObject> list = new List<GameObject>();
		if (tasks != null)
		{
			for (int i = 0; i < tasks.Count; i++)
			{
				Task task = tasks[i];
				GameObject gameObject = null;
				if (!task._Mission.Repeatable && !task.pPayload.Started)
				{
					string nPCObjectName = GetNPCObjectName(task._Mission.GroupID);
					if (!string.IsNullOrEmpty(nPCObjectName))
					{
						gameObject = GameObject.Find(nPCObjectName);
					}
				}
				if (gameObject != null && !list.Contains(gameObject))
				{
					list.Add(gameObject);
				}
			}
		}
		return list;
	}

	public Mission GetRootScientificMission(Mission inMission)
	{
		Mission result = (IsScientificMission(inMission) ? inMission : null);
		if (inMission != null && inMission._Parent != null && IsScientificMission(inMission._Parent))
		{
			return GetRootMission(inMission._Parent);
		}
		return result;
	}

	public override Mission GetRootMission(Task inTask)
	{
		if (inTask != null && IsScientificMission(inTask._Mission))
		{
			return GetRootScientificMission(inTask._Mission);
		}
		return base.GetRootMission(inTask);
	}

	public override Mission GetRootMission(Mission inMission)
	{
		if (IsScientificMission(inMission))
		{
			return GetRootScientificMission(inMission);
		}
		return base.GetRootMission(inMission);
	}

	public static bool IsScientificMission(Mission inMission)
	{
		return inMission?.Name.Contains("Quest_Lab") ?? false;
	}

	public static bool IsHypothesisTask(Task inTask)
	{
		if (inTask.pData != null && inTask.pData.Type == "Meet" && inTask.pData.Objectives != null)
		{
			for (int i = 0; i < inTask.pData.Objectives.Count; i++)
			{
				if (inTask.pData.Objectives[i].Get<string>("Type") == "Hypothesis")
				{
					return true;
				}
			}
		}
		return false;
	}

	public static Task GetHypothesisTask(Mission inMission)
	{
		Task result = null;
		Mission rootMission = MissionManager.pInstance.GetRootMission(inMission);
		if (inMission != null && IsScientificMission(rootMission) && rootMission.MissionRule != null && rootMission.MissionRule.Criteria != null && rootMission.MissionRule.Criteria.RuleItems != null)
		{
			for (int i = 0; i < rootMission.MissionRule.Criteria.RuleItems.Count; i++)
			{
				RuleItem ruleItem = rootMission.MissionRule.Criteria.RuleItems[i];
				Task task = rootMission.GetTask(ruleItem.ID);
				if (ruleItem.Type == RuleItemType.Task && IsHypothesisTask(task))
				{
					result = task;
					break;
				}
			}
		}
		return result;
	}

	public static bool GetScientificQuestPhase(object inMissionOrTask, RuleItemType inType, int inPhaseIdx, out object outPhase, out RuleItemType outType)
	{
		Mission mission = null;
		outPhase = null;
		outType = RuleItemType.Mission;
		MissionManagerDO missionManagerDO = MissionManager.pInstance as MissionManagerDO;
		if (missionManagerDO == null || inPhaseIdx >= missionManagerDO._ScientificQuestPhases.Length || inPhaseIdx < 0)
		{
			return false;
		}
		ScientificQuestPhaseInfo scientificQuestPhaseInfo = missionManagerDO._ScientificQuestPhases[inPhaseIdx];
		switch (inType)
		{
		case RuleItemType.Task:
			mission = missionManagerDO.GetRootMission(inMissionOrTask as Task);
			break;
		case RuleItemType.Mission:
			mission = missionManagerDO.GetRootMission(inMissionOrTask as Mission);
			break;
		}
		if (mission != null && IsScientificMission(mission) && mission.MissionRule != null && mission.MissionRule.Criteria != null && mission.MissionRule.Criteria.RuleItems != null)
		{
			for (int i = 0; i < mission.MissionRule.Criteria.RuleItems.Count; i++)
			{
				RuleItem ruleItem = mission.MissionRule.Criteria.RuleItems[i];
				if (ruleItem.Type == RuleItemType.Task)
				{
					Task task = mission.GetTask(ruleItem.ID);
					if (task != null && task.Name.Contains(scientificQuestPhaseInfo._PhaseKeyword))
					{
						outPhase = task;
						outType = ruleItem.Type;
						return true;
					}
				}
				else if (ruleItem.Type == RuleItemType.Mission)
				{
					Mission mission2 = mission.GetMission(ruleItem.ID);
					if (mission2 != null && mission2.Name.Contains(scientificQuestPhaseInfo._PhaseKeyword))
					{
						outPhase = mission2;
						outType = ruleItem.Type;
						return true;
					}
				}
			}
		}
		return false;
	}

	public static string GetHypothesisResult(Task inTask)
	{
		string result = "";
		if (IsHypothesisTask(inTask))
		{
			string value = inTask.pPayload.Get<string>(UiMissionHypothesisDB._HypothesisCorrectKey);
			bool result2 = false;
			if (!string.IsNullOrEmpty(value) && bool.TryParse(value, out result2))
			{
				result = (result2 ? ((MissionManagerDO)MissionManager.pInstance)._TrueText.GetLocalizedString() : ((MissionManagerDO)MissionManager.pInstance)._FalseText.GetLocalizedString());
			}
		}
		return result;
	}

	public static void UpdateCompletedMission(Mission inMission, WsServiceEventHandler inCallback, object inUserData)
	{
		if (MissionManager.pIsReady && UserInfo.pIsReady)
		{
			MissionRequestFilter missionRequestFilter = new MissionRequestFilter();
			missionRequestFilter.MissionID = inMission.MissionID;
			missionRequestFilter.GetCompletedMission = true;
			UpdateCompletedMissionData inUserData2 = new UpdateCompletedMissionData(inCallback, inUserData);
			WsWebService.GetUserMissionState(UserInfo.pInstance.UserID, missionRequestFilter, GetUserMissionStateEventHandler, inUserData2);
		}
	}

	private static void GetUserMissionStateEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			UserMissionStateResult userMissionStateResult = (UserMissionStateResult)inObject;
			Mission mission = null;
			if (userMissionStateResult != null && userMissionStateResult.Missions != null)
			{
				List<Mission> missions = userMissionStateResult.Missions;
				if (missions != null && missions.Count > 0)
				{
					mission = missions[0];
					((MissionManagerDO)MissionManager.pInstance).PopulateMissionData(mission);
				}
			}
			else
			{
				UtDebug.Log("WEB SERVICE CALL GetUserMissionState RETURNED NO DATA!!!", Mission.LOG_MASK);
			}
			UpdateCompletedMissionData updateCompletedMissionData2 = inUserData as UpdateCompletedMissionData;
			if (updateCompletedMissionData2._Callback != null)
			{
				updateCompletedMissionData2._Callback(inType, inEvent, inProgress, mission, updateCompletedMissionData2._UserData);
			}
			break;
		}
		case WsServiceEvent.ERROR:
		{
			Debug.LogError("WEB SERVICE CALL GetUserMissionState FAILED!!!");
			UpdateCompletedMissionData updateCompletedMissionData = inUserData as UpdateCompletedMissionData;
			if (updateCompletedMissionData._Callback != null)
			{
				updateCompletedMissionData._Callback(inType, inEvent, inProgress, null, updateCompletedMissionData._UserData);
			}
			break;
		}
		}
	}

	public static string GetQuestHeading(Mission inMission)
	{
		string result = "";
		if (inMission != null)
		{
			Mission rootMission = MissionManager.pInstance.GetRootMission(inMission);
			if (rootMission.pData != null && rootMission.pData.Title != null)
			{
				result = rootMission.pData.Title.GetLocalizedString();
			}
		}
		return result;
	}

	public static string GetQuestHeading(Task inTask)
	{
		string result = "";
		if (inTask != null)
		{
			Mission rootMission = MissionManager.pInstance.GetRootMission(inTask);
			if (rootMission.pData != null && rootMission.pData.Title != null)
			{
				result = rootMission.pData.Title.GetLocalizedString();
			}
		}
		return result;
	}

	public static void AddQuantityToObjectiveString(ref string objectiveStr, Task lTask)
	{
		if ((!(lTask.pData.Type == "Collect") && !(lTask.pData.Type == "Delivery")) || lTask.pData.Objectives.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < lTask.pData.Objectives.Count; i++)
		{
			TaskObjective taskObjective = lTask.pData.Objectives[i];
			int num = taskObjective.Get<int>("Quantity");
			if (num <= 0)
			{
				continue;
			}
			if (lTask.pData.Type == "Collect")
			{
				if (lTask.pCompleted)
				{
					objectiveStr = objectiveStr + " (" + num + " / " + num + ")";
				}
				else
				{
					objectiveStr = objectiveStr + " (" + taskObjective._Collected + " / " + num + ")";
				}
			}
			else if (lTask.pData.Type == "Delivery")
			{
				int itemID = taskObjective.Get<int>("ItemID");
				UserItemData userItemData = CommonInventoryData.pInstance.FindItem(itemID);
				if (userItemData != null)
				{
					objectiveStr = objectiveStr + " (" + userItemData.Quantity + " / " + num + ")";
				}
			}
		}
	}

	public List<Task> GetTasksFilterLocked(string taskType, string key, string value)
	{
		List<Task> tasks = GetTasks(taskType, key, value);
		List<Task> list = new List<Task>();
		for (int i = 0; i < tasks.Count; i++)
		{
			Task task = tasks[i];
			if (!IsLocked(task._Mission))
			{
				list.Add(task);
			}
		}
		return list;
	}

	public static void GetRequiredRankForMission(Mission inMission, int inRankType, out int outMinRank, out int outMaxRank)
	{
		outMinRank = -1;
		outMaxRank = -1;
		List<string> prerequisites = inMission.MissionRule.GetPrerequisites<string>(PrerequisiteRequiredType.Rank);
		for (int i = 0; i < prerequisites.Count; i++)
		{
			string[] array = prerequisites[i].Split(',');
			if (array.Length > 1 && int.Parse(array[0]) == inRankType)
			{
				if (!string.IsNullOrEmpty(array[1]))
				{
					outMinRank = int.Parse(array[1]);
				}
				if (array.Length > 2 && !string.IsNullOrEmpty(array[2]))
				{
					outMaxRank = int.Parse(array[2]);
				}
			}
		}
	}

	public void ShowQuestArrow(bool enable)
	{
		if (_QuestArrow != null)
		{
			_QuestArrow.transform.parent.gameObject.SetActive(enable);
		}
	}
}
