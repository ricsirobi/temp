using System;
using System.Collections.Generic;
using SOD.Event;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionManager : MonoBehaviour
{
	public class AcceptMissionUserData
	{
		public Mission _Mission;

		public AcceptMissionCallback _Callback;

		public AcceptMissionUserData(Mission mission, AcceptMissionCallback callback)
		{
			_Mission = mission;
			_Callback = callback;
		}
	}

	public enum ActionType
	{
		OFFER,
		TASK_END,
		MISSION_END,
		MISSION_COMPLETE
	}

	public class Action
	{
		public List<MissionAction> _Actions;

		public object _Object;

		public ActionType _Type;

		public MissionActionEventHandler _EventHandler;

		public Action(List<MissionAction> actions, object inObject, ActionType end)
		{
			_Actions = actions;
			_Object = inObject;
			_Type = end;
			_EventHandler = null;
		}

		public Action(List<MissionAction> actions, object inObject, ActionType end, MissionActionEventHandler eventHandler)
			: this(actions, inObject, end)
		{
			_EventHandler = eventHandler;
		}

		public bool HasAsset(string inAsset)
		{
			if (_Actions != null)
			{
				return _Actions.Find((MissionAction action) => action.Asset.Contains(inAsset)) != null;
			}
			return false;
		}

		public bool IsDone()
		{
			if (_Actions != null)
			{
				return _Actions.Find((MissionAction action) => !action._Played) == null;
			}
			return true;
		}
	}

	[Serializable]
	public class DailyMissions
	{
		public string _EventName;

		public int _GroupID;
	}

	public static GameObject pObject = null;

	public static MissionManager pInstance;

	private static bool mIsReady = true;

	public bool _AutoUpdate = true;

	public bool _UseLegacyMissionData;

	public float _EngagementDistance = 6f;

	public float _IgnoreAvatarResetDistance = 5f;

	public List<MissionGroupSettings> _GroupSettings = new List<MissionGroupSettings>();

	public List<MissionRewardData> _RewardData;

	public List<DailyMissions> _DailyMissions = new List<DailyMissions>();

	public List<int> _AnalyticEventMissions;

	public string _DailyMissionResetTime;

	public string _DailyMissionNotificationTime;

	private DateTime mDailyMissionResetTime;

	public LocaleString _DeliveryConfirmationText = new LocaleString("Are you sure you want to give %items_list% from your inventory?");

	public LocaleString _DeliveryInsufficientText = new LocaleString("You need to deliver %items_list%.");

	public LocaleString _TaskFailedTitleText = new LocaleString("Task Failed");

	public LocaleString _PendingTaskCompletionTitleText = new LocaleString("Pending Task");

	public LocaleString _PendingTaskCompletionText = new LocaleString("You must finish your current task to start this one.");

	public LocaleString _DailyQuestNotificationText = new LocaleString("Make sure to complete your daily quests by the end of the day to earn the daily rewards!");

	public LocaleString _SetTaskFailedText = new LocaleString("Your progress failed to sync with our server. Please try again.");

	public LocaleString _SetTaskFailedTitleText = new LocaleString("Progress Sync Failed ");

	private Task mTaskInProgress;

	private bool mMissionsRequested;

	private int mMissionCalls;

	protected List<Mission> mMissions;

	private bool mResumeTasks;

	private bool mResumeTasksPending;

	protected bool mRefreshMissions;

	private bool mDoAction;

	private bool mWaitListCompleted;

	private int mActionObjectLoadCount;

	private List<Mission> mDependencyMissions = new List<Mission>();

	private GameObject mActionObject;

	private SnChannel mChannel;

	private bool mDialogInterruptable;

	private bool mSetup;

	private float mActualDeltaTime;

	private GameObject mEventMessageObject;

	private bool mIsTimedTaskPaused;

	private bool mIsForceUpdateTimedTask;

	private bool mIsTimedTaskActivePending;

	private Task mActiveChaseTask;

	private Task mActiveTimedTask;

	private List<Task> mActiveTasks = new List<Task>();

	protected List<Action> mActions = new List<Action>();

	protected int mMissionCallbacks;

	protected Dictionary<int, MissionCompletedResult> mMissionResults = new Dictionary<int, MissionCompletedResult>();

	[NonSerialized]
	private Mission mMissionWaitingForCallback;

	private MissionCompletedResult mCurrentMissionResult;

	private SetTimedMissionTaskStateResult mTimedMissionResult;

	protected Dictionary<int, bool> mRandomMissionStatus = new Dictionary<int, bool>();

	private Task mPreviousTask;

	private UserMissionStateResult mDailyMissionStateResult;

	private bool mMissionActionPending;

	private static MissionEventHandler mMissionEventHandlers = null;

	public static bool pIsReady
	{
		get
		{
			if (mIsReady && pInstance != null)
			{
				return pInstance.IsReady();
			}
			return false;
		}
	}

	public DateTime pDailyMissionResetTime => mDailyMissionResetTime;

	public GameObject pActionObject => mActionObject;

	public GameObject pEventMessageObject
	{
		get
		{
			return mEventMessageObject;
		}
		set
		{
			mEventMessageObject = value;
		}
	}

	public bool pIsForceUpdateTimedTask => mIsForceUpdateTimedTask;

	public Task pActiveChaseTask => mActiveChaseTask;

	public Task pActiveTimedTask => mActiveTimedTask;

	public List<Task> pActiveTasks => mActiveTasks;

	public Task pPreviousTask => mPreviousTask;

	public UserMissionStateResult pDailyMissionStateResult => mDailyMissionStateResult;

	public static void AddMissionEventHandler(MissionEventHandler inHandler)
	{
		RemoveMissionEventHandler(inHandler);
		mMissionEventHandlers = (MissionEventHandler)Delegate.Combine(mMissionEventHandlers, inHandler);
	}

	public static void RemoveMissionEventHandler(MissionEventHandler inHandler)
	{
		mMissionEventHandlers = (MissionEventHandler)Delegate.Remove(mMissionEventHandlers, inHandler);
	}

	public static void Init()
	{
		if (pInstance != null)
		{
			pInstance.GetUserMissionState();
			pInstance.Initialize();
		}
	}

	public static void Reset()
	{
		if (pInstance != null)
		{
			if (pInstance.mChannel != null)
			{
				pInstance.mChannel.Stop();
			}
			pInstance.mActiveTimedTask = null;
			pInstance.mActiveChaseTask = null;
			if (pInstance.mActiveTasks != null)
			{
				for (int i = 0; i < pInstance.mActiveTasks.Count; i++)
				{
					pInstance.mActiveTasks[i].CleanUp();
				}
				pInstance.mActiveTasks.Clear();
			}
			if (pInstance.mActions != null)
			{
				pInstance.mActions.Clear();
			}
			if (pInstance.mMissions != null)
			{
				pInstance.mMissions.Clear();
			}
			if (pInstance.mMissionResults != null)
			{
				pInstance.mMissionResults.Clear();
			}
			if (pInstance.mRandomMissionStatus != null)
			{
				pInstance.mRandomMissionStatus.Clear();
			}
			if (pInstance.mActionObject != null)
			{
				UnityEngine.Object.Destroy(pInstance.mActionObject);
				pInstance.mActionObject = null;
			}
			pInstance.mMissions = null;
			pInstance.mEventMessageObject = null;
			pInstance.mMissionWaitingForCallback = null;
			pInstance.mCurrentMissionResult = null;
			mMissionEventHandlers = null;
			pInstance.mDailyMissionStateResult = null;
			pInstance.mMissionsRequested = false;
			pInstance.mResumeTasks = false;
			pInstance.mSetup = false;
			pInstance.mMissionCallbacks = 0;
			pInstance.Clear();
		}
		mIsReady = false;
	}

	public virtual void Start()
	{
		if (pInstance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		mIsReady = false;
		pObject = base.gameObject;
		UnityEngine.Object.DontDestroyOnLoad(pObject);
		pInstance = this;
		CoCommonLevel.WaitListCompleted += OnWaitListCompleted;
		Task.OnSetTaskFailed = OnSetTaskFailed;
	}

	private void OnSetTaskFailed(Task task)
	{
		mTaskInProgress = task;
		GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _SetTaskFailedText.GetLocalizedString(), _SetTaskFailedTitleText.GetLocalizedString(), base.gameObject, "OnRetryYes", "OnRetryNo", null, null, inDestroyOnClick: true, updatePriority: true);
		KAUICursorManager.SetDefaultCursor("Arrow");
	}

	private void OnRetryYes()
	{
		mTaskInProgress.SaveTaskState();
		KAUICursorManager.SetDefaultCursor("Loading");
	}

	private void OnRetryNo()
	{
		GameUtilities.LoadLoginLevel();
	}

	public void OnWaitListCompleted()
	{
		mWaitListCompleted = true;
	}

	public List<Mission> GetMissions(int groupID, bool completed)
	{
		if (mMissions != null)
		{
			List<Mission> missions = new List<Mission>();
			for (int i = 0; i < mMissions.Count; i++)
			{
				Mission mission = mMissions[i];
				if (mission.GroupID == groupID || (groupID == -1 && CanAutoStart(mission)))
				{
					CheckForMissionCompletion(mission);
					GetNextMission(mission, completed, ref missions);
				}
			}
			return missions;
		}
		return null;
	}

	public List<Mission> GetAllMissions(int groupID)
	{
		if (mMissions != null)
		{
			List<Mission> list = new List<Mission>();
			for (int i = 0; i < mMissions.Count; i++)
			{
				Mission mission = mMissions[i];
				if (mission.GroupID == groupID || (groupID == -1 && CanAutoStart(mission)))
				{
					CheckForMissionCompletion(mission);
					list.Add(mission);
				}
			}
			return list;
		}
		return null;
	}

	public void GetNextMission(Mission mission, bool completed, ref List<Mission> missions)
	{
		if (mission.Completed > 0 && !mission.Repeatable)
		{
			return;
		}
		for (int i = 0; i < mission.MissionRule.Criteria.RuleItems.Count; i++)
		{
			RuleItem ruleItem = mission.MissionRule.Criteria.RuleItems[i];
			if (mission.MissionRule.Criteria.Ordered && ruleItem.Complete > 0)
			{
				continue;
			}
			if (ruleItem.Type == RuleItemType.Task)
			{
				missions.Add(mission);
			}
			else if (ruleItem.Type == RuleItemType.Mission)
			{
				Mission mission2 = mission.GetMission(ruleItem.ID);
				if (mission2 == null)
				{
					Debug.LogError("Mission data not found " + ruleItem.ID);
				}
				else if (mission2.Completed > 0)
				{
					if (completed)
					{
						missions.Add(mission2);
					}
				}
				else
				{
					GetNextMission(mission2, completed, ref missions);
				}
			}
			if (mission.MissionRule.Criteria.Ordered)
			{
				break;
			}
		}
	}

	public Mission GetMission(int missionID)
	{
		return GetMission(missionID, mMissions);
	}

	public static bool IsMissionCompleted(int missionId)
	{
		if (missionId > 0 && pInstance != null)
		{
			Mission mission = pInstance.GetMission(missionId);
			if (mission != null)
			{
				return mission.pCompleted;
			}
		}
		return false;
	}

	private Mission GetMission(int missionID, List<Mission> missions)
	{
		if (missions != null)
		{
			for (int i = 0; i < missions.Count; i++)
			{
				Mission mission = missions[i];
				if (mission.MissionID == missionID)
				{
					return mission;
				}
				if (mission.Missions != null)
				{
					Mission mission2 = GetMission(missionID, mission.Missions);
					if (mission2 != null)
					{
						return mission2;
					}
				}
			}
		}
		return null;
	}

	public virtual Mission GetRootMission(Task inTask)
	{
		if (inTask != null && inTask._Mission != null)
		{
			return GetRootMission(inTask._Mission);
		}
		return null;
	}

	public virtual Mission GetRootMission(Mission inMission)
	{
		if (inMission != null && inMission._Parent != null)
		{
			return GetRootMission(inMission._Parent);
		}
		return inMission;
	}

	public MissionDailyMissions GetDailyMissions(int groupID)
	{
		MissionDailyMissions result = null;
		string stringValue = ProductData.pPairData.GetStringValue("DailyMission" + groupID, null);
		if (!string.IsNullOrEmpty(stringValue))
		{
			result = UtUtilities.DeserializeFromXml<MissionDailyMissions>(stringValue);
		}
		return result;
	}

	public virtual bool CanAutoStart(Mission mission)
	{
		if (_GroupSettings != null)
		{
			MissionGroupSettings missionGroupSettings = _GroupSettings.Find((MissionGroupSettings group) => group._GroupID == mission.GroupID);
			if (missionGroupSettings != null)
			{
				return missionGroupSettings._AutoStart;
			}
		}
		return true;
	}

	public void AcceptMission(int missionID, AcceptMissionCallback callback)
	{
		Mission mission = GetMission(missionID);
		AcceptMission(mission, callback);
	}

	public void AcceptMission(Mission mission, AcceptMissionCallback callback)
	{
		AcceptMissionUserData inUserData = new AcceptMissionUserData(mission, callback);
		while (mission != null && !mission.MissionRule.GetPrerequisite<bool>(PrerequisiteRequiredType.Accept))
		{
			mission = mission._Parent;
		}
		if (mission != null)
		{
			if (Mission.pSave)
			{
				if (mission.pTimedMission)
				{
					WsWebService.AcceptTimedMission(UserInfo.pInstance.UserID, mission.MissionID, AcceptMissionEventHandler, inUserData);
				}
				else
				{
					WsWebService.AcceptMission(UserInfo.pInstance.UserID, mission.MissionID, AcceptMissionEventHandler, inUserData);
				}
			}
			else
			{
				AcceptMissionEventHandler(WsServiceType.ACCEPT_MISSION, WsServiceEvent.COMPLETE, 1f, true, inUserData);
			}
		}
		else
		{
			AcceptMissionEventHandler(WsServiceType.ACCEPT_MISSION, WsServiceEvent.COMPLETE, 1f, false, inUserData);
		}
	}

	private void AcceptMissionEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			AcceptMissionUserData acceptMissionUserData2 = (AcceptMissionUserData)inUserData;
			bool flag = (bool)inObject;
			if (flag)
			{
				Mission mission = acceptMissionUserData2._Mission;
				while (mission != null && !mission.MissionRule.GetPrerequisite<bool>(PrerequisiteRequiredType.Accept))
				{
					mission = mission._Parent;
				}
				mission.Accepted = true;
				List<Task> tasks = new List<Task>();
				GetNextTask(acceptMissionUserData2._Mission, ref tasks);
				for (int i = 0; i < tasks.Count; i++)
				{
					Task task = tasks[i];
					if (task.Completed == 0 && !task._Mission.pMustAccept)
					{
						StartTask(task);
					}
				}
				if (acceptMissionUserData2._Mission.AcceptanceRewards != null)
				{
					GameUtilities.AddRewards(acceptMissionUserData2._Mission.AcceptanceRewards.ToArray());
				}
				OnMissionAcceptComplete(acceptMissionUserData2._Mission);
				CheckForTaskCompletion("Action", "AcceptMission", acceptMissionUserData2._Mission.MissionID);
			}
			acceptMissionUserData2._Callback(flag, acceptMissionUserData2._Mission);
			break;
		}
		case WsServiceEvent.ERROR:
		{
			AcceptMissionUserData acceptMissionUserData = (AcceptMissionUserData)inUserData;
			acceptMissionUserData._Callback(success: false, acceptMissionUserData._Mission);
			break;
		}
		}
	}

	protected virtual void OnMissionAcceptComplete(Mission mission)
	{
	}

	private List<Task> GetTasksInternal(int groupID)
	{
		if (mMissions != null)
		{
			List<Task> tasks = new List<Task>();
			int i = 0;
			for (int count = mMissions.Count; i < count; i++)
			{
				Mission mission = mMissions[i];
				if (mission.GroupID == groupID || (groupID == -1 && CanAutoStart(mission)))
				{
					CheckForMissionCompletion(mission);
					GetNextTask(mission, ref tasks);
				}
			}
			return tasks;
		}
		return null;
	}

	public List<Task> GetTasks(int groupID)
	{
		return GetTasksInternal(groupID);
	}

	public void GetNextTask(Mission mission, ref List<Task> tasks)
	{
		if (mission == null || (mission.Completed > 0 && !mission.Repeatable) || IsLocked(mission) || mission.MissionRule.Criteria.RuleItems.Count == 0)
		{
			return;
		}
		RuleItem ruleItem = mission.MissionRule.Criteria.RuleItems[mission.MissionRule.Criteria.RuleItems.Count - 1];
		int i = 0;
		for (int count = mission.MissionRule.Criteria.RuleItems.Count; i < count; i++)
		{
			RuleItem ruleItem2 = mission.MissionRule.Criteria.RuleItems[i];
			if (mission.MissionRule.Criteria.Ordered && ruleItem2.Complete > ruleItem.Complete)
			{
				continue;
			}
			if (ruleItem2.Type == RuleItemType.Task)
			{
				Task task = mission.GetTask(ruleItem2.ID);
				if (task == null)
				{
					Debug.LogError("Task not found " + ruleItem2.ID + " for mission " + mission.MissionID);
				}
				else
				{
					tasks.Add(task);
				}
			}
			else if (ruleItem2.Type == RuleItemType.Mission)
			{
				Mission mission2 = mission.GetMission(ruleItem2.ID);
				GetNextTask(mission2, ref tasks);
			}
			if (mission.MissionRule.Criteria.Ordered)
			{
				break;
			}
		}
	}

	public List<Task> GetTasks(string taskType, string key, string value)
	{
		return GetTasks(taskType, key, value, -1);
	}

	public List<Task> GetTasks(string taskType, string key, string value, int groupID)
	{
		List<Task> list = new List<Task>();
		for (int i = 0; i < mActiveTasks.Count; i++)
		{
			Task task = mActiveTasks[i];
			if (task.Match(taskType, key, value) && task.Completed == 0 && (groupID == -1 || task._Mission.GroupID == groupID))
			{
				list.Add(task);
			}
		}
		return list;
	}

	public static bool IsTaskActive(string taskType, string key = null, string value = null)
	{
		if (pInstance != null)
		{
			List<Task> tasks = pInstance.GetTasks(taskType, key, value);
			if (tasks != null)
			{
				return tasks.Count > 0;
			}
			return false;
		}
		return false;
	}

	public Task GetTask(int taskID)
	{
		if (mMissions == null)
		{
			return null;
		}
		return GetTask(taskID, mMissions);
	}

	private Task GetTask(int taskID, List<Mission> missions)
	{
		int count = missions.Count;
		for (int i = 0; i < count; i++)
		{
			Mission mission = missions[i];
			Task missionTask = GetMissionTask(taskID, mission);
			if (missionTask != null)
			{
				return missionTask;
			}
			if (mission.Missions != null)
			{
				missionTask = GetTask(taskID, mission.Missions);
				if (missionTask != null)
				{
					return missionTask;
				}
			}
		}
		return null;
	}

	public Task GetMissionTask(int taskID, Mission mission)
	{
		if (mission.Tasks != null)
		{
			for (int i = 0; i < mission.Tasks.Count; i++)
			{
				if (mission.Tasks[i].TaskID == taskID && mission.Tasks[i] != null)
				{
					return mission.Tasks[i];
				}
			}
		}
		return null;
	}

	public virtual void StartTask(Task task)
	{
		if (!NPCAvatar._Engaged && task._Active)
		{
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
		}
		if (!task._Active && CanStartTask(task))
		{
			mIsReady = false;
			mActiveTasks.Add(task);
			task.Start();
			List<MissionAction> offers = task.GetOffers(unplayed: true);
			if (offers != null)
			{
				mMissionActionPending = true;
			}
			if (mPreviousTask != null && GetRootMission(mPreviousTask) == GetRootMission(task))
			{
				SendMissionEvent(MissionEvent.TASK_END_COMPLETE, mPreviousTask);
				mPreviousTask = null;
			}
			mMissionActionPending = false;
			SendMissionEvent(MissionEvent.TASK_STARTED, task);
			AddAction(offers, task, ActionType.OFFER, null, forcedDoAction: false);
		}
	}

	public void CheckForAutoTaskCompletion(Mission mission, RuleItem ruleItem)
	{
		if (ruleItem.Type == RuleItemType.Task && ruleItem.Complete == 0)
		{
			Task task = mission.GetTask(ruleItem.ID);
			if (task == null)
			{
				Debug.LogError("Task not found " + ruleItem.ID + " for mission " + mission.MissionID);
			}
			else if (AutoCompleteTask(task))
			{
				UtDebug.LogFastM(Mission.LOG_MASK, "Task ", task.TaskID, " marked for completion by CheckForAutoTaskCompletion.");
				mMissionCallbacks++;
				task.Completed++;
				task.Complete(MissionCompleteCallback);
				AvAvatar.pState = AvAvatarState.PAUSED;
				AvAvatar.SetUIActive(inActive: false);
				ruleItem.Complete++;
			}
		}
		else if (ruleItem.Type == RuleItemType.Mission && ruleItem.Complete == 0)
		{
			CheckForMissionCompletion(mission.GetMission(ruleItem.ID));
		}
	}

	public virtual bool AutoCompleteTask(Task task)
	{
		return false;
	}

	private void AddToDependencyMissionsList(Mission mission)
	{
		List<int> prerequisites = mission.MissionRule.GetPrerequisites<int>(PrerequisiteRequiredType.Mission);
		for (int i = 0; i < prerequisites.Count; i++)
		{
			Mission mission2 = GetMission(prerequisites[i]);
			if (mission2 != null && mission2.Completed <= 0)
			{
				mDependencyMissions.Add(mission);
			}
		}
	}

	public void ResumeTasks()
	{
		if (mMissions == null)
		{
			return;
		}
		List<Task> tasks = new List<Task>();
		for (int i = 0; i < mMissions.Count; i++)
		{
			if (mMissions[i] != null && CanAutoStart(mMissions[i]))
			{
				CheckForMissionCompletion(mMissions[i]);
				GetNextTask(mMissions[i], ref tasks);
				if (!mDependencyMissions.Contains(mMissions[i]) && mMissions[i].Completed == 0 && !mMissions[i].pMustAccept)
				{
					AddToDependencyMissionsList(mMissions[i]);
				}
			}
		}
		for (int j = 0; j < tasks.Count; j++)
		{
			Task task = tasks[j];
			if (task.Completed < task._Mission.MissionRule.Criteria.Repeat && (task.pStarted || (!task._Mission.pMustAccept && CanAutoStart(task._Mission))) && CanStartTask(task))
			{
				if (!task.pStarted && mPreviousTask == null)
				{
					StartTask(task);
				}
				else if (!mActiveTasks.Contains(task))
				{
					mActiveTasks.Add(task);
					task.Start();
					ResumeTask(task);
				}
			}
		}
	}

	public virtual void ResumeTask(Task task)
	{
		List<MissionAction> offers = task.GetOffers(unplayed: true);
		if (offers != null && offers.Count > 0)
		{
			for (int i = 0; i < offers.Count; i++)
			{
				offers[i]._Played = true;
			}
		}
	}

	public List<MissionAction> GetOffers(MissionActionType type, string npcName, bool unplayed)
	{
		List<MissionAction> list = new List<MissionAction>();
		for (int i = 0; i < mActiveTasks.Count; i++)
		{
			MissionAction offer = mActiveTasks[i].GetOffer(type, npcName, unplayed);
			if (offer != null)
			{
				list.Add(offer);
			}
		}
		return list;
	}

	private void AddMissionComplete(Mission mission)
	{
		Action item = new Action(null, mission, ActionType.MISSION_COMPLETE, null);
		UtDebug.Log("AddAction " + ActionType.MISSION_COMPLETE, Mission.LOG_MASK);
		mActions.Add(item);
	}

	public void AddAction(List<MissionAction> actions, object inObject, ActionType type, MissionActionEventHandler inEventHandler = null, bool forcedDoAction = true, UiMissionActionDB missionActionDB = null)
	{
		if (actions == null)
		{
			return;
		}
		Action action = new Action(actions, inObject, type, inEventHandler);
		for (int i = 0; i < actions.Count; i++)
		{
			UtDebug.Log("AddAction " + actions[i].Type.ToString() + " " + ((actions[i].Type == MissionActionType.Popup) ? actions[i].GetLocalizedString() : actions[i].Asset), Mission.LOG_MASK);
		}
		mActions.Add(action);
		if (mActions.Count == 1 && mMissionWaitingForCallback == null)
		{
			if (mWaitListCompleted || forcedDoAction)
			{
				DoAction(action, missionActionDB);
			}
			else
			{
				mDoAction = true;
			}
		}
	}

	public void DoAction(Action action, UiMissionActionDB missionActionDB = null)
	{
		if (mMissionCallbacks > 0 || mCurrentMissionResult != null)
		{
			return;
		}
		if (action._Type == ActionType.MISSION_COMPLETE)
		{
			EndAction();
			return;
		}
		MissionAction missionAction = null;
		for (int i = 0; i < action._Actions.Count; i++)
		{
			MissionAction missionAction2 = action._Actions[i];
			if (missionAction2._Played)
			{
				continue;
			}
			if (missionAction != null && missionAction.Priority < missionAction2.Priority)
			{
				return;
			}
			missionAction = missionAction2;
			UtDebug.Log("DoAction " + missionAction2.Type.ToString() + " " + ((missionAction2.Type == MissionActionType.Popup) ? missionAction2.GetLocalizedString() : missionAction2.Asset), Mission.LOG_MASK);
			if (action._Type == ActionType.OFFER)
			{
				SendMissionEvent(MissionEvent.OFFER, missionAction2);
			}
			else if (action._Type == ActionType.TASK_END)
			{
				SendMissionEvent(MissionEvent.TASK_END, missionAction2);
			}
			else if (action._Type == ActionType.MISSION_END)
			{
				SendMissionEvent(MissionEvent.MISSION_END, missionAction2);
			}
			if (missionAction2.Type == MissionActionType.VO)
			{
				mDialogInterruptable = action._Type == ActionType.TASK_END || action._Type == ActionType.MISSION_END;
			}
			else
			{
				mDialogInterruptable = false;
			}
			if (missionAction2.Type == MissionActionType.VO || missionAction2.Type == MissionActionType.Popup)
			{
				if (!string.IsNullOrEmpty(missionAction2.NPC) && CanEngage(missionAction2))
				{
					GameObject gameObject = GameObject.Find(missionAction2.NPC);
					if (gameObject != null)
					{
						NPCAvatar component = gameObject.GetComponent<NPCAvatar>();
						if (component != null)
						{
							bool flag = false;
							if (NPCAvatar._Engaged == gameObject)
							{
								flag = true;
							}
							else if (NPCAvatar._Engaged == null)
							{
								float num = _EngagementDistance;
								Vector3 position = gameObject.transform.position;
								Vector3 position2 = AvAvatar.position;
								Vector3 vector = position - position2;
								ObClickable component2 = gameObject.GetComponent<ObClickable>();
								if (component2 != null)
								{
									vector += gameObject.transform.TransformDirection(component2._RangeOffset);
									num = component2._Range;
								}
								if (vector.magnitude <= num)
								{
									flag = true;
								}
							}
							if (flag)
							{
								missionAction2._Played = true;
								if (missionAction2.Type == MissionActionType.VO)
								{
									component.LoadAndPlayVO(missionAction2.Asset, mDialogInterruptable);
									continue;
								}
								component.StartMissionEngagement();
								if (missionActionDB != null)
								{
									mActionObject = missionActionDB.gameObject;
									SetupMissionActionObject(missionActionDB, missionAction2);
								}
								else
								{
									mActionObjectLoadCount++;
									KAUICursorManager.SetDefaultCursor("Loading");
									ShowActionPopup(missionAction2);
								}
								continue;
							}
						}
						else
						{
							gameObject.SendMessage("OnDoAction", missionAction2, SendMessageOptions.DontRequireReceiver);
						}
					}
				}
				if ((bool)NPCAvatar._Engaged)
				{
					return;
				}
			}
			missionAction2._Played = true;
			mActionObjectLoadCount++;
			KAUICursorManager.SetDefaultCursor("Loading");
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
			string[] array = missionAction2.Asset.Split('/');
			if (missionAction2.Type == MissionActionType.CutScene)
			{
				bool flag2 = false;
				if (array[2].EndsWith("FAO") && (SanctuaryManager.pCurPetInstance == null || SanctuaryManager.pCurPetInstance.pTypeInfo._Flightless || SanctuaryManager.pCurPetInstance.pAge < RaisedPetData.GetAgeIndex(RaisedPetStage.ADULT)))
				{
					flag2 = true;
				}
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], CutSceneLoadEvent, typeof(GameObject), inDontDestroy: false, flag2);
			}
			else if (missionAction2.Type == MissionActionType.VO)
			{
				if (SnChannel.pTurnOffSoundGroup)
				{
					VOLoadEvent(array[0] + "/" + array[1], RsResourceLoadEvent.COMPLETE, 1f, null, null);
				}
				else
				{
					RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], VOLoadEvent, typeof(AudioClip));
				}
			}
			else if (missionAction2.Type == MissionActionType.Popup)
			{
				if (missionActionDB != null)
				{
					mActionObjectLoadCount--;
					mActionObject = missionActionDB.gameObject;
					KAUICursorManager.SetDefaultCursor("Arrow");
					SetupMissionActionObject(missionActionDB, missionAction2);
				}
				else
				{
					ShowActionPopup(missionAction2);
				}
			}
			else if (missionAction2.Type == MissionActionType.Movie)
			{
				KAUICursorManager.SetDefaultCursor("Arrow");
				MovieManager.SetBackgroundColor(Color.black);
				MovieManager.Play(missionAction2.Asset, OnMovieStarted, OnMoviePlayed, skipMovie: true);
			}
		}
		if (missionAction == null)
		{
			EndAction();
		}
	}

	public void ShowActionPopup(MissionAction inAction)
	{
		if (inAction.Type != MissionActionType.Popup)
		{
			return;
		}
		string[] array = inAction.Asset.Split('/');
		if (array.Length == 1)
		{
			GameObject gameObject = (GameObject)RsResourceManager.LoadAssetFromResources(array[^1]);
			if (gameObject != null)
			{
				PopupLoadEvent(array[^1], RsResourceLoadEvent.COMPLETE, 1f, gameObject, inAction);
			}
			else
			{
				PopupLoadEvent(array[^1], RsResourceLoadEvent.ERROR, 0f, null, null);
			}
		}
		else
		{
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], PopupLoadEvent, typeof(GameObject), inDontDestroy: false, inAction);
		}
	}

	public void EndAction()
	{
		Action action = mActions[0];
		if (action._Type == ActionType.TASK_END)
		{
			Task task = (Task)action._Object;
			CheckForMissionCompletion(task._Mission);
			mPreviousTask = task;
		}
		if (action.IsDone())
		{
			if (mActionObjectLoadCount > 0 || mActionObject != null)
			{
				return;
			}
			mActions.RemoveAt(0);
			UtDebug.Log("EndAction " + action._Type, Mission.LOG_MASK);
			if (action._Type == ActionType.MISSION_COMPLETE)
			{
				SendMissionEvent(MissionEvent.MISSION_COMPLETE, action._Object);
				Mission mission = (Mission)action._Object;
				AvAvatar.pState = AvAvatarState.PAUSED;
				AvAvatar.SetUIActive(inActive: false);
				if (mMissionCallbacks > 0)
				{
					KAUICursorManager.SetDefaultCursor("Loading");
					mMissionWaitingForCallback = mission;
				}
				else
				{
					ShowMissionResults(mission);
				}
				return;
			}
			if (mMissionCallbacks <= 0 && CanEngage(null) && !NPCAvatar._Engaged && action._Type == ActionType.OFFER)
			{
				KAUICursorManager.SetDefaultCursor("Arrow");
				AvAvatar.pState = AvAvatarState.IDLE;
				AvAvatar.SetUIActive(inActive: true);
			}
			if (action._Type == ActionType.OFFER)
			{
				SendMissionEvent(MissionEvent.OFFER_COMPLETE, action);
			}
			else if (action._Type == ActionType.MISSION_END)
			{
				SendMissionEvent(MissionEvent.MISSION_END_COMPLETE, action);
			}
			if (mActions.Count > 0)
			{
				DoAction(mActions[0]);
			}
			else if (mActions.Count == 0 && action._Type == ActionType.TASK_END)
			{
				KAUICursorManager.SetDefaultCursor("Arrow");
				Task task2 = (Task)action._Object;
				task2.CleanUp();
				StartNextTask(task2._Mission);
			}
		}
		else if (mActionObjectLoadCount == 0 && mActionObject == null)
		{
			DoAction(action);
		}
	}

	public AudioClip GetHelp(string npcName)
	{
		int i;
		for (i = 0; i < mActiveTasks.Count; i++)
		{
			MissionGroupSettings missionGroupSettings = _GroupSettings.Find((MissionGroupSettings group) => group._GroupID == mActiveTasks[i]._Mission.GroupID);
			if (missionGroupSettings != null)
			{
				MissionHelp missionHelp = missionGroupSettings._HelpVOs.Find((MissionHelp help) => help._Name == npcName);
				if (missionHelp != null)
				{
					return missionHelp._VOs[UnityEngine.Random.Range(0, missionHelp._VOs.Length)];
				}
			}
		}
		return null;
	}

	public virtual bool CanEngage(MissionAction inAction)
	{
		return true;
	}

	public virtual void AddToolbar(GameObject gameObject)
	{
	}

	public virtual bool IsReady()
	{
		return true;
	}

	protected virtual void Initialize()
	{
	}

	protected virtual void Clear()
	{
	}

	public virtual string FormatText(int inTextID, string defaultText, Task inTask = null)
	{
		string text = StringTable.GetStringData(inTextID, defaultText);
		if (inTask != null)
		{
			text = text.Replace("{{NPC}}", inTask.GetObjectiveValue<string>("NPC"));
		}
		return text;
	}

	public virtual string FormatActionText(int inTextID, string defaultText)
	{
		return FormatText(inTextID, defaultText);
	}

	public static bool MissionActionPending()
	{
		if (pInstance != null)
		{
			if (!pInstance.mMissionActionPending)
			{
				return pInstance.mActions.Count > 0;
			}
			return true;
		}
		return false;
	}

	public virtual void Update()
	{
		mActualDeltaTime += Time.deltaTime;
		if (!mMissionsRequested && _AutoUpdate)
		{
			GetUserMissionState();
		}
		if (RsResourceManager.pLevelLoading)
		{
			return;
		}
		ProcessTasksResume();
		if (mDoAction && mWaitListCompleted)
		{
			mDoAction = false;
			if (mActions.Count > 0)
			{
				DoAction(mActions[0]);
			}
		}
		if (!mIsReady && mMissionCalls == 0)
		{
			if (mActiveTasks.Count <= 0)
			{
				return;
			}
			mIsReady = true;
			int i = 0;
			for (int count = mActiveTasks.Count; i < count; i++)
			{
				Task task = mActiveTasks[i];
				if (mSetup)
				{
					task.Setup();
				}
				if (!task.pIsReady)
				{
					UtDebug.Log("Task " + task.TaskID + " not ready.", Mission.LOG_MASK);
					mIsReady = false;
				}
			}
			mSetup = false;
		}
		else
		{
			if (mMissionWaitingForCallback != null)
			{
				return;
			}
			if (Input.GetKeyUp(KeyCode.Space) && mDialogInterruptable && mChannel != null)
			{
				mChannel.Stop();
			}
			if (mActiveChaseTask != null && !mActiveChaseTask._Active && mActiveTimedTask != null && mIsTimedTaskActivePending && !mActiveTimedTask._Active)
			{
				mIsTimedTaskActivePending = false;
				mActiveChaseTask = null;
				mActiveTimedTask._Active = true;
			}
			if (mActiveTimedTask != null && CanUpdateTimedTask())
			{
				mActiveTimedTask.Update(mActualDeltaTime);
			}
			UiToolbar uiToolbar = null;
			if (AvAvatar.pToolbar == null || !AvAvatar.pToolbar.activeInHierarchy || (uiToolbar = AvAvatar.pToolbar.GetComponent<UiToolbar>()) == null || !uiToolbar.enabled)
			{
				mActualDeltaTime = 0f;
				return;
			}
			for (int j = 0; j < mActiveTasks.Count; j++)
			{
				if (!mActiveTasks[j].pIsTimedTask)
				{
					mActiveTasks[j].Update(mActualDeltaTime);
				}
			}
			mActualDeltaTime = 0f;
			if ((AvAvatar.pState == AvAvatarState.IDLE || AvAvatar.pState == AvAvatarState.MOVING) && mWaitListCompleted)
			{
				if (mRefreshMissions)
				{
					mRefreshMissions = false;
					RefreshMissions();
					ProcessTasksResume();
				}
				CheckForCompletion();
				UpdateDailyMissionNotification();
			}
		}
	}

	public void ProcessTasksResume()
	{
		if (mResumeTasks && CanResumeTasks())
		{
			mResumeTasks = false;
			mIsReady = true;
			mSetup = false;
			ResumeTasks();
		}
	}

	public void AbortTimedTask()
	{
		if (mActiveTimedTask != null)
		{
			mActiveTimedTask.Failed = true;
		}
	}

	public void SetTimedTaskUpdate(bool inState, bool inForceUpdate = false)
	{
		UtDebug.Log("Time task is updating : " + inState + " with forceUpdate: " + inForceUpdate);
		mIsTimedTaskPaused = !inState;
		mIsForceUpdateTimedTask = inForceUpdate;
	}

	public virtual bool CanUpdateTimedTask()
	{
		if (!mIsTimedTaskPaused && !RsResourceManager.pLevelLoadingScreen)
		{
			if (!mIsForceUpdateTimedTask)
			{
				if (AvAvatar.pState != AvAvatarState.PAUSED)
				{
					return AvAvatar.pState != AvAvatarState.NONE;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	protected virtual bool CanResumeTasks()
	{
		if (UserRankData.pIsReady && CommonInventoryData.pIsReady)
		{
			return ParentData.pIsReady;
		}
		return false;
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	public virtual void OnSceneLoaded(Scene newScene, LoadSceneMode loadSceneMode)
	{
		if (!RsResourceManager.pIsTransition)
		{
			mChannel = null;
			mWaitListCompleted = false;
			mDoAction = true;
			mIsReady = mActiveTasks.Count == 0;
			mSetup = !mIsReady;
			mCurrentMissionResult = null;
			for (int i = 0; i < mActiveTasks.Count; i++)
			{
				mActiveTasks[i].OnLevelLoaded();
			}
			if (mActions.Count > 0 && mActions[0]._Type == ActionType.TASK_END)
			{
				((Task)mActions[0]._Object).OnLevelLoaded();
			}
			CheckForTaskCompletion("Visit", RsResourceManager.pLastLevel, 0);
		}
	}

	public void ClearActions()
	{
		mActions.Clear();
	}

	public void Collect(GameObject gameObject)
	{
		for (int i = 0; i < mActiveTasks.Count; i++)
		{
			if (mActiveTasks[i]._Active && mActiveTasks[i].Collect(gameObject))
			{
				SendMissionEvent(MissionEvent.COLLECTED, mActiveTasks[i]);
			}
		}
	}

	public void Collect(string collectedObjectName)
	{
		for (int i = 0; i < mActiveTasks.Count; i++)
		{
			if (mActiveTasks[i]._Active && mActiveTasks[i].Collect(collectedObjectName))
			{
				SendMissionEvent(MissionEvent.COLLECTED, mActiveTasks[i]);
			}
		}
	}

	public void FailTask(Task inTask)
	{
		if (!mActiveTasks.Exists((Task t) => t.TaskID == inTask.TaskID))
		{
			return;
		}
		inTask.Fail();
		mActiveTasks.Remove(inTask);
		Mission mission = CheckForMissionCompletion(inTask._Mission);
		if (mission == null)
		{
			return;
		}
		List<Task> tasks = new List<Task>();
		GetNextTask(mission, ref tasks);
		for (int i = 0; i < tasks.Count; i++)
		{
			Task task = tasks[i];
			if (task.Completed == 0 && !task._Mission.pMustAccept && CanAutoStart(task._Mission))
			{
				StartTask(task);
			}
		}
	}

	public bool CheckForTaskCompletion(string taskType, object value1)
	{
		return CheckForTaskCompletion(taskType, value1, "", "");
	}

	public bool CheckForTaskCompletion(string taskType, object value1, object value2)
	{
		return CheckForTaskCompletion(taskType, value1, value2, "");
	}

	public bool CheckForTaskCompletion(string taskType, object value1, object value2, object value3)
	{
		bool result = false;
		for (int i = 0; i < mActiveTasks.Count; i++)
		{
			if (mActiveTasks[i].CheckForCompletion(taskType, value1, value2, value3))
			{
				result = true;
			}
		}
		return result;
	}

	public Task GetCompletedTask(string taskType, object value1, object value2)
	{
		for (int i = 0; i < mActiveTasks.Count; i++)
		{
			if (mActiveTasks[i].CheckForCompletion(taskType, value1, value2, ""))
			{
				return mActiveTasks[i];
			}
		}
		return null;
	}

	public virtual void CheckForCompletion()
	{
		int i;
		for (i = 0; i < mActiveTasks.Count; i++)
		{
			if (mActiveTasks[i].Completed == 0 && AutoCompleteTask(mActiveTasks[i]))
			{
				mActiveTasks[i].Completed++;
			}
			if (mActiveTasks[i].Completed <= 0 && !mActiveTasks[i].Failed)
			{
				continue;
			}
			if (mActiveTasks[i].Completed > 0)
			{
				mMissionCallbacks++;
				if (AvAvatar.pState != AvAvatarState.PAUSED)
				{
					AvAvatar.pState = AvAvatarState.PAUSED;
				}
				if (AvAvatar.GetUIActive())
				{
					AvAvatar.SetUIActive(inActive: false);
				}
				if (UICursorManager.GetCursorName() != "Loading")
				{
					KAUICursorManager.SetDefaultCursor("Loading");
				}
				mActiveTasks[i].Complete(MissionCompleteCallback);
				pInstance.GetRootMission(mActiveTasks[i]);
				mActiveTasks[i]._Mission.MissionRule.Criteria.RuleItems.Find((RuleItem item) => item.Type == RuleItemType.Task && item.ID == mActiveTasks[i].TaskID).Complete++;
				SendMissionEvent(MissionEvent.TASK_COMPLETE, mActiveTasks[i]);
				AddAction(mActiveTasks[i].GetEnds(), mActiveTasks[i], ActionType.TASK_END);
			}
			else
			{
				mActiveTasks[i].Fail();
				SendMissionEvent(MissionEvent.TASK_FAIL, mActiveTasks[i]);
				string statusText = "";
				if (mActiveTasks[i].pData != null && mActiveTasks[i].pData.Failure != null)
				{
					statusText = FormatText(mActiveTasks[i].pData.Failure.ID, mActiveTasks[i].pData.Failure.Text, mActiveTasks[i]);
				}
				if (mActiveTasks[i].pIsTimedTask || mActiveTasks[i].pData.Type == "Chase")
				{
					string objectiveValue = mActiveTasks[i].GetObjectiveValue<string>("ResetMarker");
					string objectiveValue2 = mActiveTasks[i].GetObjectiveValue<string>("Scene");
					if (!string.IsNullOrEmpty(objectiveValue) && objectiveValue2.Equals(RsResourceManager.pCurrentLevel))
					{
						AvAvatar.TeleportToObject(objectiveValue, 0f, doTeleportFx: true, _IgnoreAvatarResetDistance);
					}
				}
				UiMissionStatusDB.Show(_TaskFailedTitleText.GetLocalizedString(), statusText);
				Mission notCompleteMission = CheckForMissionCompletion(mActiveTasks[i]._Mission);
				StartNextTask(notCompleteMission);
			}
			if (mActiveTasks[i] == mActiveTimedTask)
			{
				mActiveTimedTask = null;
			}
			else if (mActiveTasks[i] == mActiveChaseTask && !mIsTimedTaskActivePending)
			{
				mActiveChaseTask = null;
			}
			mActiveTasks.RemoveAt(i);
			i--;
		}
	}

	public virtual void StartNextTask(Mission notCompleteMission)
	{
		int num = -1;
		if (notCompleteMission != null)
		{
			List<Task> tasks = new List<Task>();
			GetNextTask(notCompleteMission, ref tasks);
			num = tasks.Count;
			for (int i = 0; i < tasks.Count; i++)
			{
				Task task = tasks[i];
				if (task.Completed == 0 && !task._Mission.pMustAccept && CanAutoStart(task._Mission))
				{
					StartTask(task);
				}
			}
		}
		if (mPreviousTask != null)
		{
			SendMissionEvent(MissionEvent.TASK_END_COMPLETE, mPreviousTask);
			mPreviousTask = null;
		}
		if (num > 0)
		{
			return;
		}
		if (mResumeTasksPending)
		{
			mResumeTasksPending = false;
			if (CanResumeTasks())
			{
				ResumeTasks();
			}
			return;
		}
		List<Task> tasks2 = new List<Task>();
		for (int j = 0; j < mDependencyMissions.Count; j++)
		{
			GetNextTask(mDependencyMissions[j], ref tasks2);
		}
		List<Task> list = new List<Task>();
		for (int k = 0; k < tasks2.Count; k++)
		{
			if (mDependencyMissions.Contains(tasks2[k]._Mission))
			{
				mDependencyMissions.Remove(tasks2[k]._Mission);
			}
			if (tasks2[k]._Mission != null && tasks2[k]._Mission.pData != null && !tasks2[k]._Mission.pData.Hidden)
			{
				list.Add(tasks2[k]);
			}
			mActiveTasks.Add(tasks2[k]);
			tasks2[k].Start();
			ResumeTask(tasks2[k]);
		}
		if (list.Count > 0)
		{
			OnDependencyTasksStarted(list);
		}
	}

	protected virtual void OnDependencyTasksStarted(List<Task> tasks)
	{
	}

	public void CheckForMissionCompletion()
	{
		for (int i = 0; i < mMissions.Count; i++)
		{
			CheckForMissionCompletion(mMissions[i]);
		}
	}

	private Mission CheckForMissionCompletion(Mission mission)
	{
		if (mission == null || (mission.Completed > 0 && !mission.Repeatable) || IsLocked(mission) || (!mission.Accepted && mission.pMustAccept))
		{
			return null;
		}
		MissionRule missionRule = mission.MissionRule;
		int num = 0;
		int num2 = 0;
		num = ((!(missionRule.Criteria.Type == "all")) ? missionRule.Criteria.Min : missionRule.Criteria.RuleItems.Count);
		if (missionRule.Criteria.RuleItems != null && !missionRule.Criteria.Ordered)
		{
			int i = 0;
			for (int count = missionRule.Criteria.RuleItems.Count; i < count; i++)
			{
				RuleItem ruleItem = missionRule.Criteria.RuleItems[i];
				if (ruleItem.Complete >= 0)
				{
					num2 += ruleItem.Complete;
				}
			}
		}
		else if (missionRule.Criteria.RuleItems != null && missionRule.Criteria.Ordered)
		{
			int j = 0;
			for (int count2 = missionRule.Criteria.RuleItems.Count; j < count2; j++)
			{
				RuleItem ruleItem2 = missionRule.Criteria.RuleItems[j];
				if (ruleItem2.Complete <= 0)
				{
					return mission;
				}
				if (ruleItem2.Complete >= 0)
				{
					num2 += ruleItem2.Complete;
				}
			}
		}
		if (num2 < num)
		{
			return mission;
		}
		int num3 = missionRule.Criteria.Repeat * num;
		if (num2 < num3)
		{
			int k = 0;
			for (int count3 = mission.Missions.Count; k < count3; k++)
			{
				mission.Missions[k].Reset();
			}
			int l = 0;
			for (int count4 = mission.Tasks.Count; l < count4; l++)
			{
				mission.Tasks[l].Reset();
			}
			return mission;
		}
		UtDebug.Log("Mission " + mission.MissionID + " marked for completion by CheckForMissionCompletion.", Mission.LOG_MASK);
		if (mission.Tasks != null)
		{
			foreach (Task task2 in mission.Tasks)
			{
				if (!task2.pCompleted)
				{
					task2._Active = false;
					mActiveTasks.Remove(task2);
					task2.CleanUp();
				}
			}
		}
		if (mission.Missions != null)
		{
			foreach (Mission mission2 in mission.Missions)
			{
				if (mission2.pCompleted)
				{
					continue;
				}
				foreach (Task task3 in mission2.Tasks)
				{
					if (!task3.pCompleted)
					{
						task3._Active = false;
						mActiveTasks.Remove(task3);
						task3.CleanUp();
					}
				}
				mission2.CleanUp();
			}
		}
		mission.Completed++;
		AddAction(mission.GetEnds(), mission, ActionType.MISSION_END);
		if (!Mission.pSave && mission.Rewards != null)
		{
			MissionCompletedResult missionCompletedResult = new MissionCompletedResult();
			missionCompletedResult.MissionID = mission.MissionID;
			missionCompletedResult.Rewards = mission.Rewards.ToArray();
			mMissionResults.Add(mission.MissionID, missionCompletedResult);
		}
		AddMissionComplete(mission);
		if (mission._Parent != null)
		{
			mission._Parent.MissionRule.Criteria.RuleItems.Find((RuleItem item) => item.Type == RuleItemType.Mission && item.ID == mission.MissionID).Complete++;
			return CheckForMissionCompletion(mission._Parent);
		}
		if (CheckMissionHasRandomization(mission) && mRandomMissionStatus != null)
		{
			if (mRandomMissionStatus.ContainsKey(mission.MissionID))
			{
				mRandomMissionStatus[mission.MissionID] = true;
			}
			else
			{
				mRandomMissionStatus.Add(mission.MissionID, value: true);
			}
		}
		if (mission.Repeatable)
		{
			mission.Reset();
			List<Task> tasks = new List<Task>();
			GetNextTask(mission, ref tasks);
			for (int m = 0; m < tasks.Count; m++)
			{
				Task task = tasks[m];
				if (task.Completed == 0 && !task._Mission.pMustAccept && CanAutoStart(task._Mission))
				{
					StartTask(task);
				}
			}
		}
		return null;
	}

	private bool CheckMissionHasRandomization(Mission mission)
	{
		if (mission != null)
		{
			if (mission.pData != null && mission.pData.Random)
			{
				return true;
			}
			bool flag = CheckMissionTaskHasRandomSetup(mission.Tasks);
			if (!flag && mission.Missions != null && mission.Missions.Count > 0)
			{
				foreach (Mission mission2 in mission.Missions)
				{
					flag = CheckMissionHasRandomization(mission2);
					if (flag)
					{
						break;
					}
				}
			}
			return flag;
		}
		return false;
	}

	private bool ReloadRequiredForRandom(int missionID)
	{
		if (mRandomMissionStatus != null && mRandomMissionStatus.Count > 0 && mRandomMissionStatus.ContainsKey(missionID))
		{
			return mRandomMissionStatus[missionID];
		}
		return false;
	}

	private bool CheckMissionTaskHasRandomSetup(List<Task> inTasks)
	{
		if (inTasks != null && inTasks.Count > 0)
		{
			foreach (Task inTask in inTasks)
			{
				if (inTask.pData != null && inTask.pData.RandomSetups != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void MissionCompleteCallback(SetTaskStateResult result)
	{
		mMissionCallbacks--;
		if (result != null && result.MissionsCompleted != null)
		{
			for (int i = 0; i < result.MissionsCompleted.Length; i++)
			{
				mMissionResults.Add(result.MissionsCompleted[i].MissionID, result.MissionsCompleted[i]);
			}
			if (result != null && result is SetTimedMissionTaskStateResult setTimedMissionTaskStateResult && setTimedMissionTaskStateResult.DailyReward != null && setTimedMissionTaskStateResult.DailyReward.Length != 0)
			{
				mTimedMissionResult = setTimedMissionTaskStateResult;
			}
		}
		if (mMissionCallbacks == 0 && mMissionWaitingForCallback != null)
		{
			ShowMissionResults(mMissionWaitingForCallback);
			mMissionWaitingForCallback = null;
		}
		if (mMissionCallbacks == 0 && mActions.Count > 0)
		{
			DoAction(mActions[0]);
		}
	}

	public virtual void ShowMissionResults(Mission mission)
	{
		if (mission != null)
		{
			if (mission.pData.RemoveItem != null)
			{
				for (int i = 0; i < mission.pData.RemoveItem.Count; i++)
				{
					CommonInventoryData.pInstance.RemoveItem(mission.pData.RemoveItem[i].ItemID, updateServer: false, mission.pData.RemoveItem[i].Quantity);
				}
				CommonInventoryData.pInstance.ClearSaveCache();
			}
			if (mMissionResults.ContainsKey(mission.MissionID))
			{
				MissionCompletedResult missionCompletedResult = mMissionResults[mission.MissionID];
				mMissionResults.Remove(mission.MissionID);
				if (missionCompletedResult.Rewards != null && missionCompletedResult.Rewards.Length != 0)
				{
					if (mission.pData.Reward != null && !string.IsNullOrEmpty(mission.pData.Reward.Asset))
					{
						mCurrentMissionResult = missionCompletedResult;
						string[] array = mission.pData.Reward.Asset.Split('/');
						if (array.Length == 1)
						{
							GameObject gameObject = (GameObject)RsResourceManager.LoadAssetFromResources(array[^1]);
							if (gameObject != null)
							{
								MissionRewardLoadEvent(array[^1], RsResourceLoadEvent.COMPLETE, 1f, gameObject, null);
							}
							else
							{
								MissionRewardLoadEvent(array[^1], RsResourceLoadEvent.ERROR, 0f, null, null);
							}
						}
						else
						{
							RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], MissionRewardLoadEvent, typeof(GameObject));
						}
						return;
					}
					GameUtilities.AddRewards(missionCompletedResult.Rewards);
				}
				if (mTimedMissionResult != null)
				{
					OnRewardClose();
					return;
				}
			}
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
		SendMissionEvent(MissionEvent.MISSION_REWARDS_COMPLETE, mission);
		if (mPreviousTask != null)
		{
			mPreviousTask.CleanUp();
		}
		mission?.CleanUp();
		if (mMissionCallbacks <= 0)
		{
			AvAvatar.pState = AvAvatarState.IDLE;
		}
		if (mActions.Count > 0)
		{
			DoAction(mActions[0]);
		}
		else if (mission != null)
		{
			StartNextTask(mission._Parent);
		}
		if (!AvAvatar.GetUIActive() && mMissionCallbacks <= 0 && mActions.Count == 0 && mActionObject == null)
		{
			AvAvatar.SetUIActive(inActive: true);
		}
	}

	protected void PopulateMissionData(Mission inMission, bool timedMission = false)
	{
		if (inMission == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(inMission.Static))
		{
			inMission.pStaticDataReady = true;
		}
		if (timedMission)
		{
			inMission.pTimedMission = true;
		}
		if (inMission.Missions != null)
		{
			for (int i = 0; i < inMission.Missions.Count; i++)
			{
				if (inMission.Missions[i] != null)
				{
					inMission.Missions[i]._Parent = inMission;
					PopulateMissionData(inMission.Missions[i], timedMission);
				}
			}
		}
		if (inMission.Tasks == null)
		{
			return;
		}
		for (int j = 0; j < inMission.Tasks.Count; j++)
		{
			if (inMission.Tasks[j] != null)
			{
				inMission.Tasks[j]._Mission = inMission;
			}
		}
	}

	protected void UpdateMissionData(Mission inMission)
	{
		if (inMission != null)
		{
			Mission toMission = mMissions.Find((Mission m) => m.MissionID == inMission.MissionID);
			UpdateMissionStatic(inMission, toMission);
		}
	}

	protected void UpdateMissionStatic(Mission fromMission, Mission toMission)
	{
		if (fromMission == null || toMission == null)
		{
			return;
		}
		if (fromMission.Missions.Count != toMission.Missions.Count || fromMission.Tasks.Count != toMission.Tasks.Count)
		{
			UtDebug.Log("Data Mismatch for the mission" + toMission.Name + " :: ID" + toMission.MissionID, Mission.LOG_MASK);
			return;
		}
		if (toMission.MissionID != fromMission.MissionID)
		{
			toMission = GetMission(fromMission.MissionID);
			UtDebug.Log("Order Mismatch Observed for the mission" + toMission.Name + " :: ID" + toMission.MissionID, Mission.LOG_MASK);
		}
		if (!string.IsNullOrEmpty(fromMission.Static))
		{
			toMission.pStatic = fromMission.Static;
		}
		if (toMission.Missions != null)
		{
			for (int i = 0; i < toMission.Missions.Count && i < fromMission.Missions.Count; i++)
			{
				if (toMission.Missions[i] != null && fromMission.Missions[i] != null)
				{
					UpdateMissionStatic(fromMission.Missions[i], toMission.Missions[i]);
				}
			}
		}
		if (toMission.Tasks == null || fromMission.Tasks == null)
		{
			return;
		}
		for (int j = 0; j < fromMission.Tasks.Count; j++)
		{
			if (string.IsNullOrEmpty(fromMission.Tasks[j].Static))
			{
				continue;
			}
			if (toMission.Tasks[j].TaskID == fromMission.Tasks[j].TaskID)
			{
				toMission.Tasks[j].Static = fromMission.Tasks[j].Static;
				continue;
			}
			UtDebug.Log("Order Mismatch Observed for the tasks in Mission" + toMission.MissionID, Mission.LOG_MASK);
			Task missionTask = GetMissionTask(fromMission.Tasks[j].TaskID, toMission);
			if (missionTask != null)
			{
				missionTask.Static = fromMission.Tasks[j].Static;
			}
			else
			{
				UtDebug.Log("task (" + fromMission.Tasks[j].TaskID + ") not found in mission" + toMission.MissionID, Mission.LOG_MASK);
			}
		}
	}

	private void GetUserMissionState()
	{
		if (!UserInfo.pIsReady)
		{
			return;
		}
		mMissionsRequested = true;
		mIsReady = false;
		MissionRequestFilterV2 missionRequestFilterV = new MissionRequestFilterV2();
		if (_UseLegacyMissionData)
		{
			WsWebService.GetUserMissionStateV2(UserInfo.pInstance.UserID, missionRequestFilterV, GetUserMissionStateEventHandler, null);
			mMissionCalls++;
		}
		else
		{
			WsWebService.GetUserActiveMissionState(UserInfo.pInstance.UserID, GetUserMissionStateEventHandler, null);
			WsWebService.GetUserUpcomingMissionState(UserInfo.pInstance.UserID, GetUserMissionStateEventHandler, null);
			WsWebService.GetUserCompletedMissionState(UserInfo.pInstance.UserID, GetUserMissionStateEventHandler, null);
			mMissionCalls += 3;
		}
		if (!EventManager.pIsReady || _DailyMissions == null || _DailyMissions.Count <= 0)
		{
			return;
		}
		List<int> list = new List<int>();
		foreach (DailyMissions dailyMission in _DailyMissions)
		{
			if (dailyMission._EventName == string.Empty)
			{
				list.Add(dailyMission._GroupID);
				continue;
			}
			EventManager eventManager = EventManager.Get(dailyMission._EventName);
			if (eventManager != null && eventManager.EventInProgress() && !eventManager.GracePeriodInProgress())
			{
				list.Add(dailyMission._GroupID);
			}
		}
		missionRequestFilterV.MissionGroupIDs = list.ToArray();
		if (missionRequestFilterV.MissionGroupIDs.Length != 0)
		{
			WsWebService.GetUserTimedMissionState(UserInfo.pInstance.UserID, missionRequestFilterV, GetUserMissionStateEventHandler, null);
			mMissionCalls++;
		}
	}

	private void GetUserMissionStateEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			mMissionCalls--;
			if (mMissions == null)
			{
				mMissions = new List<Mission>();
			}
			UserMissionStateResult userMissionStateResult = (UserMissionStateResult)inObject;
			if (userMissionStateResult != null && userMissionStateResult.Missions != null)
			{
				mMissions.AddRange(userMissionStateResult.Missions);
				for (int i = 0; i < userMissionStateResult.Missions.Count; i++)
				{
					PopulateMissionData(userMissionStateResult.Missions[i], inType == WsServiceType.GET_USER_TIMED_MISSION_STATE);
				}
				if (inType == WsServiceType.GET_USER_TIMED_MISSION_STATE)
				{
					HandleDailyMissionData(userMissionStateResult);
				}
			}
			else
			{
				UtDebug.Log("WEB SERVICE CALL GetUserMissionState RETURNED NO DATA!!!", Mission.LOG_MASK);
			}
			if (mMissionCalls == 0)
			{
				if (_UseLegacyMissionData)
				{
					KAUICursorManager.SetDefaultCursor("Loading");
					LoadMissionData(-1, OnMissionStaticDataReady);
				}
				else
				{
					mResumeTasks = true;
				}
			}
			break;
		}
		case WsServiceEvent.ERROR:
			mMissionCalls--;
			Debug.LogError("WEB SERVICE CALL GetUserMissionState FAILED!!!");
			break;
		}
	}

	public void OnMissionStaticDataReady(List<Mission> missions)
	{
		if (missions == null)
		{
			UtDebug.LogError("LoadMissionData returned null");
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
		mResumeTasks = true;
	}

	public void LoadMissionData(int groupID, MissionStaticLoadCallback callback = null, int missionID = -1, bool loadCompleted = false)
	{
		bool flag = false;
		MissionRequestFilterV2 missionRequestFilterV = new MissionRequestFilterV2();
		missionRequestFilterV.MissionPair = new List<MissionPair>();
		if (mMissions != null)
		{
			if (missionID != -1)
			{
				Mission mission = mMissions.Find((Mission m) => m.MissionID == missionID);
				if (mission != null)
				{
					MissionPair missionPair = new MissionPair();
					missionPair.MissionID = mission.MissionID;
					missionPair.VersionID = ((mission.Accepted || mission.Completed > 0) ? mission.VersionID : (-1));
					missionRequestFilterV.MissionPair.Add(missionPair);
					flag = true;
				}
			}
			else
			{
				for (int i = 0; i < mMissions.Count; i++)
				{
					Mission mission2 = mMissions[i];
					if (((groupID == -1 && CanAutoStart(mission2)) || mission2.GroupID == groupID) && (mission2.Accepted || !IsLocked(mission2) || !mission2.pMustAccept) && (mission2.Completed <= 0 || mission2.Repeatable || (loadCompleted && mission2.pCompleted)) && (!mission2.pStaticDataReady || ReloadRequiredForRandom(mission2.MissionID)))
					{
						MissionPair missionPair2 = new MissionPair();
						missionPair2.MissionID = mission2.MissionID;
						missionPair2.VersionID = ((!mission2.pMustAccept || mission2.Completed > 0) ? mission2.VersionID : (-1));
						missionRequestFilterV.MissionPair.Add(missionPair2);
						flag = true;
					}
				}
			}
		}
		if (flag)
		{
			WsWebService.GetUserMissionStateV2(UserInfo.pInstance.UserID, missionRequestFilterV, GetUserMissionStaticEventHandler, callback);
		}
		else
		{
			callback(null);
		}
	}

	private void GetUserMissionStaticEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			UserMissionStateResult userMissionStateResult = (UserMissionStateResult)inObject;
			if (userMissionStateResult != null && userMissionStateResult.Missions != null)
			{
				for (int i = 0; i < userMissionStateResult.Missions.Count; i++)
				{
					UpdateMissionData(userMissionStateResult.Missions[i]);
				}
			}
			else
			{
				UtDebug.Log("WEB SERVICE CALL GetUserMissionState RETURNED NO DATA!!!", Mission.LOG_MASK);
			}
			((MissionStaticLoadCallback)inUserData)?.Invoke(userMissionStateResult.Missions);
			break;
		}
		case WsServiceEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			Debug.LogError("WEB SERVICE CALL GetUserMissionState FAILED!!!");
			break;
		}
	}

	private void CutSceneLoadEvent(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			KAUICursorManager.SetDefaultCursor("Arrow");
			mActionObjectLoadCount--;
			if (inObject != null)
			{
				CoAnimController componentInChildren;
				if ((bool)inUserData)
				{
					componentInChildren = ((GameObject)inObject).GetComponentInChildren<CoAnimController>();
					if (componentInChildren != null && componentInChildren._PostCSAvatarMarker != null)
					{
						AvAvatar.SetPosition(componentInChildren._PostCSAvatarMarker.position);
						AvAvatar.mTransform.rotation = componentInChildren._PostCSAvatarMarker.rotation;
					}
					if (mActions[0]._EventHandler != null)
					{
						mActions[0]._EventHandler(MissionActionEvent.CUTSCENE_COMPLETE, mActionObject);
					}
					EndAction();
					break;
				}
				mActionObject = UnityEngine.Object.Instantiate((GameObject)inObject);
				mActionObject.name = ((GameObject)inObject).name;
				componentInChildren = mActionObject.GetComponentInChildren<CoAnimController>();
				if (componentInChildren != null)
				{
					componentInChildren._MessageObject = base.gameObject;
					componentInChildren.CutSceneStart();
				}
				else
				{
					Debug.LogError("No CoAnimController found! CutScene won't end properly! " + inURL);
				}
				if (mActions[0]._EventHandler != null)
				{
					mActions[0]._EventHandler(MissionActionEvent.CUTSCENE_LOADED, mActionObject);
				}
			}
			else
			{
				UtDebug.Log("CutSceneLoadEvent inObject is null!", Mission.LOG_MASK);
				EndAction();
			}
			break;
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("Error loading CutScene! " + inURL);
			KAUICursorManager.SetDefaultCursor("Arrow");
			mActionObjectLoadCount--;
			EndAction();
			break;
		}
	}

	public void OnCutSceneDone()
	{
		CoAnimController componentInChildren = mActionObject.GetComponentInChildren<CoAnimController>();
		if (componentInChildren != null)
		{
			componentInChildren.CutSceneDone();
		}
		if (mActions[0]._EventHandler != null)
		{
			mActions[0]._EventHandler(MissionActionEvent.CUTSCENE_COMPLETE, mActionObject);
		}
		UnityEngine.Object.Destroy(mActionObject);
		mActionObject = null;
		EndAction();
	}

	public bool IsCutScenePlaying(string cutscene)
	{
		for (int i = 0; i < mActions.Count; i++)
		{
			MissionAction missionAction = ((mActions[i]._Actions != null) ? mActions[i]._Actions.Find((MissionAction action) => action.Asset.Contains(cutscene)) : null);
			if (missionAction != null && !missionAction._Played)
			{
				return true;
			}
		}
		if (mActionObjectLoadCount > 0)
		{
			return true;
		}
		if (mActionObject == null)
		{
			return false;
		}
		return mActionObject.name == cutscene;
	}

	public virtual void OnDestroy()
	{
		CoCommonLevel.WaitListCompleted -= OnWaitListCompleted;
	}

	private void VOLoadEvent(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			KAUICursorManager.SetDefaultCursor("Arrow");
			mActionObjectLoadCount--;
			if (inObject != null)
			{
				mChannel = SnChannel.Play((AudioClip)inObject, "VO_Pool", inForce: true, base.gameObject);
				if (mChannel != null)
				{
					mChannel.LoadTriggers(inURL + ".cue");
				}
			}
			else
			{
				UtDebug.Log("VOLoadEvent inObject is null!", Mission.LOG_MASK);
				EndAction();
			}
			break;
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("Error loading VO! " + inURL);
			KAUICursorManager.SetDefaultCursor("Arrow");
			mActionObjectLoadCount--;
			EndAction();
			break;
		}
	}

	public virtual void OnSnEvent(SnEvent inEvent)
	{
		if (inEvent.mType == SnEventType.END || inEvent.mType == SnEventType.END_QUEUE || inEvent.mType == SnEventType.STOP)
		{
			if (mChannel != null)
			{
				mChannel = null;
			}
			if (mActions.Count > 0 && mActions[0] != null && (inEvent.mClip == null || mActions[0].HasAsset(inEvent.mClip.name)))
			{
				EndAction();
			}
		}
	}

	public void SetupMissionActionObject(UiMissionActionDB uiMissionActionDB, MissionAction action)
	{
		uiMissionActionDB._MessageObject = base.gameObject;
		uiMissionActionDB._CloseMessage = "OnPopupOK";
		uiMissionActionDB._YesMessage = "OnPopupYes";
		uiMissionActionDB._NoMessage = "OnPopupOK";
		if (action == null)
		{
			action = mActions[0]._Actions.Find((MissionAction a) => a.Type == MissionActionType.Popup);
		}
		if (action.ItemID > 0)
		{
			uiMissionActionDB.SetItemID(action.ItemID);
		}
		uiMissionActionDB.SetText(FormatActionText(action.ID, action.Text));
		uiMissionActionDB.SetNPCIcon(action.NPC);
		uiMissionActionDB.SetVisibility(inVisible: true);
	}

	private void PopupLoadEvent(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			if (AvAvatar.pAvatarCam != null)
			{
				AvAvatar.pAvatarCam.SendMessage("EnableCameraControl", false, SendMessageOptions.DontRequireReceiver);
			}
			KAUICursorManager.SetDefaultCursor("Arrow");
			mActionObjectLoadCount--;
			mActionObject = UnityEngine.Object.Instantiate((GameObject)inObject);
			mActionObject.name = ((GameObject)inObject).name;
			UiMissionActionDB component = mActionObject.GetComponent<UiMissionActionDB>();
			if (component != null)
			{
				SetupMissionActionObject(component, inUserData as MissionAction);
			}
			else
			{
				Debug.LogError("No UiMissionActionDB found! Popup will not function properly! " + inURL);
			}
			if (mActions[0]._EventHandler != null)
			{
				mActions[0]._EventHandler(MissionActionEvent.POPUP_LOADED, mActionObject);
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("Error loading Popup! " + inURL);
			KAUICursorManager.SetDefaultCursor("Arrow");
			mActionObjectLoadCount--;
			EndAction();
			break;
		}
	}

	private void OnPopupOK()
	{
		if (mActions.Count > 0 && mActions[0]._EventHandler != null)
		{
			mActions[0]._EventHandler(MissionActionEvent.POPUP_COMPLETE, mActionObject);
		}
		if (AvAvatar.pAvatarCam != null)
		{
			AvAvatar.pAvatarCam.SendMessage("EnableCameraControl", true, SendMessageOptions.DontRequireReceiver);
		}
		if (mActionObject != null)
		{
			UiMissionActionDB component = mActionObject.GetComponent<UiMissionActionDB>();
			if (component != null && mActionObject.transform.parent != null)
			{
				component.SetVisibility(inVisible: false);
			}
			else
			{
				UnityEngine.Object.Destroy(mActionObject);
				RsResourceManager.UnloadUnusedAssets();
			}
			mActionObject = null;
			EndAction();
		}
	}

	public void OnPopupYes()
	{
		if (mActions.Count > 0 && mActions[0]._Type == ActionType.OFFER && mActions[0]._Object is Task inTask)
		{
			Activate(inTask);
		}
		OnPopupOK();
	}

	private void OnMovieStarted()
	{
		if (mActions.Count > 0 && mActions[0]._EventHandler != null)
		{
			mActions[0]._EventHandler(MissionActionEvent.MOVIE_LOADED, MovieManager.pInstance);
		}
	}

	private void OnMoviePlayed()
	{
		mActionObjectLoadCount--;
		if (mActions.Count > 0 && mActions[0]._EventHandler != null)
		{
			mActions[0]._EventHandler(MissionActionEvent.MOVIE_COMPLETE, MovieManager.pInstance);
		}
		EndAction();
	}

	private void MissionRewardLoadEvent(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			mActionObject = UnityEngine.Object.Instantiate((GameObject)inObject);
			mActionObject.name = ((GameObject)inObject).name;
			UiMissionRewardDB component = mActionObject.GetComponent<UiMissionRewardDB>();
			if (component != null)
			{
				component.SetRewards(mCurrentMissionResult.Rewards, _RewardData);
				component._MessageObject = base.gameObject;
				component._CloseMessage = "OnRewardClose";
			}
			else
			{
				Debug.LogError("No UiMissionRewardDB found! MissionRewardDB will not function properly! " + inURL);
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			if (mMissionCallbacks <= 0)
			{
				AvAvatar.pState = AvAvatarState.IDLE;
				AvAvatar.SetUIActive(inActive: true);
			}
			mCurrentMissionResult = null;
			if (mActions.Count > 0)
			{
				DoAction(mActions[0]);
			}
			Debug.LogError("Error loading MissionRewardDB! " + inURL);
			break;
		}
	}

	protected virtual void OnRewardClose()
	{
		UnityEngine.Object.Destroy(mActionObject);
		RsResourceManager.UnloadUnusedAssets();
		mActionObject = null;
		Mission mission = ((mCurrentMissionResult != null) ? GetMission(mCurrentMissionResult.MissionID) : null);
		SendMissionEvent(MissionEvent.MISSION_REWARDS_COMPLETE, mission);
		mCurrentMissionResult = null;
		if (mPreviousTask != null)
		{
			mPreviousTask.CleanUp();
		}
		mission?.CleanUp();
		if (mMissionCallbacks <= 0)
		{
			AvAvatar.pState = AvAvatarState.IDLE;
		}
		if (mActions.Count > 0)
		{
			DoAction(mActions[0]);
		}
		else if (mission != null)
		{
			StartNextTask(mission._Parent);
		}
		if (!AvAvatar.GetUIActive() && mMissionCallbacks <= 0 && mActions.Count == 0)
		{
			AvAvatar.SetUIActive(inActive: true);
		}
		if (mTimedMissionResult != null && mTimedMissionResult.DailyReward != null)
		{
			GameUtilities.AddRewards(mTimedMissionResult.DailyReward, inUseRewardManager: false, inImmediateShow: false);
			string keyData = GameConfig.GetKeyData("DailyQuestRewardAsset");
			if (!string.IsNullOrEmpty(keyData))
			{
				KAUICursorManager.SetDefaultCursor("Loading");
				string[] array = keyData.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], DailyMissionRewardLoadEvent, typeof(GameObject));
			}
		}
	}

	private void DailyMissionRewardLoadEvent(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.name = ((GameObject)inObject).name;
			UiDailyQuestRewards component = obj.GetComponent<UiDailyQuestRewards>();
			if (component != null)
			{
				component.Init(mTimedMissionResult);
			}
			else
			{
				UtDebug.LogError("No UiMissionRewardDB found! MissionRewardDB will not function properly! " + inURL);
			}
			mTimedMissionResult = null;
			break;
		}
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			mTimedMissionResult = null;
			break;
		}
	}

	public void OnNextRank(int rank)
	{
		SendMissionEvent(MissionEvent.REFRESH_MISSIONS, null);
		mResumeTasks = false;
		mResumeTasksPending = true;
	}

	public void SendMissionEvent(MissionEvent inEvent, object inObject)
	{
		if (mEventMessageObject != null)
		{
			mEventMessageObject.SendMessage("OnMissionEvent", inEvent, SendMessageOptions.DontRequireReceiver);
		}
		if (mMissionEventHandlers != null)
		{
			mMissionEventHandlers(inEvent, inObject);
		}
	}

	public virtual UserAchievementInfo GetUserAchievementInfoByType(int pointType)
	{
		return UserRankData.GetUserAchievementInfoByType(pointType);
	}

	public virtual bool IsLocked(Mission mission)
	{
		if (!Mission.pLocked)
		{
			return false;
		}
		if (mission.pMemberOnly && !SubscriptionInfo.pIsMember)
		{
			return true;
		}
		List<string> prerequisites = mission.MissionRule.GetPrerequisites<string>(PrerequisiteRequiredType.Rank);
		for (int i = 0; i < prerequisites.Count; i++)
		{
			string[] array = prerequisites[i].Split(',');
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			if (array.Length > 1)
			{
				num = int.Parse(array[0]);
				if (!string.IsNullOrEmpty(array[1]))
				{
					num2 = int.Parse(array[1]);
				}
				if (array.Length > 2 && !string.IsNullOrEmpty(array[2]))
				{
					num3 = int.Parse(array[2]);
				}
			}
			if (num > 0)
			{
				UserAchievementInfo userAchievementInfoByType = GetUserAchievementInfoByType(num);
				if (num2 > 0 && (userAchievementInfoByType == null || userAchievementInfoByType.RankID < num2))
				{
					return true;
				}
				if (num3 > 0 && userAchievementInfoByType != null && userAchievementInfoByType.RankID > num3)
				{
					return true;
				}
			}
		}
		List<int> prerequisites2 = mission.MissionRule.GetPrerequisites<int>(PrerequisiteRequiredType.Mission);
		for (int j = 0; j < prerequisites2.Count; j++)
		{
			Mission mission2 = GetMission(prerequisites2[j]);
			if (mission2 != null && mission2.Completed <= 0)
			{
				return true;
			}
		}
		string prerequisite = mission.MissionRule.GetPrerequisite<string>(PrerequisiteRequiredType.DateRange);
		if (!string.IsNullOrEmpty(prerequisite))
		{
			string[] array2 = prerequisite.Split(',');
			DateTime dateTime = Convert.ToDateTime(array2[0], UtUtilities.GetCultureInfo("en-US").DateTimeFormat);
			DateTime dateTime2 = Convert.ToDateTime(array2[1], UtUtilities.GetCultureInfo("en-US").DateTimeFormat);
			if (ServerTime.pCurrentTime <= dateTime || ServerTime.pCurrentTime >= dateTime2)
			{
				return true;
			}
		}
		for (int k = 0; k < mission.MissionRule.Prerequisites.Count; k++)
		{
			PrerequisiteItem prerequisiteItem = mission.MissionRule.Prerequisites[k];
			if (prerequisiteItem.Type == PrerequisiteRequiredType.Item)
			{
				int num4 = UtStringUtil.Parse(prerequisiteItem.Value, -1);
				if (num4 != -1 && CommonInventoryData.pInstance.GetQuantity(num4) + ParentData.pInstance.pInventory.GetQuantity(num4) < prerequisiteItem.Quantity)
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual void ObjectActivated(string inObjectName)
	{
		for (int i = 0; i < mActiveTasks.Count; i++)
		{
			mActiveTasks[i].ObjectActivated(inObjectName);
		}
	}

	public virtual void Activate(Task inTask)
	{
		if (!mActiveTasks.Contains(inTask))
		{
			return;
		}
		inTask._Active = true;
		if (inTask.pData.Type == "Chase")
		{
			mActiveChaseTask = inTask;
			if (mActiveTimedTask != null && mActiveTimedTask._Active)
			{
				mIsTimedTaskActivePending = true;
				mActiveTimedTask._Active = false;
			}
		}
		else if (inTask.pIsTimedTask)
		{
			if (inTask.pData.Type == "Collect")
			{
				inTask.FinishSetup();
			}
			mActiveTimedTask = inTask;
		}
	}

	public virtual bool CanActivate(Task inTask)
	{
		if (inTask.pData.Type == "Chase" || inTask.pIsTimedTask)
		{
			if ((inTask == mActiveChaseTask || inTask == mActiveTimedTask) && inTask._Active)
			{
				return false;
			}
			if ((mActiveChaseTask != null && mActiveChaseTask._Active) || (inTask.pData.Type != "Chase" && mActiveTimedTask != null && mActiveTimedTask._Active))
			{
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _PendingTaskCompletionText.GetLocalizedString(), _PendingTaskCompletionTitleText.GetLocalizedString(), base.gameObject, null, null, "OnDBClose", null, inDestroyOnClick: true);
				return false;
			}
		}
		else if (inTask.pData.Type == "Follow" || inTask.pData.Type == "Escort")
		{
			string objectiveValue = inTask.GetObjectiveValue<string>("Scene");
			if (!string.IsNullOrEmpty(objectiveValue) && objectiveValue != RsResourceManager.pCurrentLevel)
			{
				return false;
			}
		}
		return true;
	}

	public virtual Task GetActiveTask(GameObject inObject)
	{
		Task result = null;
		for (int i = 0; i < mActiveTasks.Count; i++)
		{
			if (mActiveTasks[i].IsTaskObject(inObject))
			{
				result = mActiveTasks[i];
				break;
			}
		}
		return result;
	}

	public void RefreshMissions(bool whenActive = false)
	{
		if (whenActive)
		{
			mRefreshMissions = true;
			return;
		}
		mResumeTasks = true;
		SendMissionEvent(MissionEvent.REFRESH_MISSIONS, null);
		if (RsResourceManager.pLevelLoading)
		{
			ProcessTasksResume();
		}
	}

	public virtual bool CanStartTask(Task task)
	{
		return true;
	}

	public bool RefreshDailyMission()
	{
		if (CheckDailyMissionsReset() && _DailyMissions != null && _DailyMissions.Count > 0)
		{
			mIsReady = false;
			mDailyMissionStateResult = null;
			MissionRequestFilterV2 missionRequestFilterV = new MissionRequestFilterV2();
			missionRequestFilterV.MissionGroupIDs = new int[_DailyMissions.Count];
			int i;
			for (i = 0; i < _DailyMissions.Count; i++)
			{
				mMissions.RemoveAll((Mission mission) => mission.GroupID == _DailyMissions[i]._GroupID);
				missionRequestFilterV.MissionGroupIDs[i] = _DailyMissions[i]._GroupID;
			}
			WsWebService.GetUserTimedMissionState(UserInfo.pInstance.UserID, missionRequestFilterV, GetUserMissionStateEventHandler, null);
			mMissionCalls++;
			return true;
		}
		return false;
	}

	public void HandleDailyMissionData(UserMissionStateResult userMissionStateResult)
	{
		if (userMissionStateResult != null)
		{
			DateTime pSTTimeFromUTC = UtUtilities.GetPSTTimeFromUTC(ServerTime.pCurrentTime);
			if (!TimeSpan.TryParse(_DailyMissionResetTime, out var result))
			{
				result = new TimeSpan(0L);
			}
			if (pSTTimeFromUTC.TimeOfDay > result)
			{
				result = result.Add(new TimeSpan(24, 0, 0));
			}
			mDailyMissionResetTime = pSTTimeFromUTC.Add(result.Subtract(pSTTimeFromUTC.TimeOfDay));
			mDailyMissionStateResult = userMissionStateResult;
			mDailyMissionStateResult.Missions = null;
			mDailyMissionStateResult.UserTimedAchievement.Sort((UserTimedAchievement achievement1, UserTimedAchievement achievement2) => achievement1.Sequence.CompareTo(achievement2.Sequence));
			UpdateDailyMissionNotificationData();
		}
	}

	public bool CheckDailyMissionsReset()
	{
		return UtUtilities.GetPSTTimeFromUTC(ServerTime.pCurrentTime) > mDailyMissionResetTime;
	}

	public bool IsCurrentDayCompleted(int requiredCount, int groupId)
	{
		List<Mission> allMissions = pInstance.GetAllMissions(groupId);
		if (allMissions == null)
		{
			return false;
		}
		int num = 0;
		foreach (Mission item in allMissions)
		{
			if (item.Completed > 0)
			{
				num++;
			}
		}
		return num >= requiredCount;
	}

	public bool IsNotificationRequired(MissionGroup mg)
	{
		List<Mission> allMissions = pInstance.GetAllMissions(mg.MissionGroupID);
		if (allMissions == null)
		{
			return false;
		}
		int num = 0;
		bool flag = false;
		foreach (Mission item in allMissions)
		{
			if (item.Completed > 0)
			{
				num++;
			}
			else if (!flag && item.pStarted)
			{
				flag = true;
			}
		}
		return num < mg.CompletionCount && flag;
	}

	public void UpdateDailyMissionNotificationData()
	{
		DateTime pSTTimeFromUTC = UtUtilities.GetPSTTimeFromUTC(ServerTime.pCurrentTime);
		if (!TimeSpan.TryParse(_DailyMissionNotificationTime, out var result))
		{
			result = new TimeSpan(0L);
		}
		if (pSTTimeFromUTC.TimeOfDay < result)
		{
			foreach (MissionGroup item in mDailyMissionStateResult.MissionGroup)
			{
				if (IsNotificationRequired(item))
				{
					TimeSpan value = result.Subtract(pSTTimeFromUTC.TimeOfDay);
					NotificationData.SetNotification(DateTime.Now.Add(value), UserInfo.pInstance.UserID + "_DailyQuest", _DailyQuestNotificationText.GetLocalizedString());
					return;
				}
			}
		}
		NotificationData.RemoveNotification(UserInfo.pInstance.UserID + "_DailyQuest");
	}

	public void UpdateDailyMissionNotification()
	{
		if (NotificationData.IsNotificationAvailable(UserInfo.pInstance.UserID + "_DailyQuest") && !FUEManager.pIsFUERunning)
		{
			TimeSpan timeSpan = NotificationData.GetNotificationTime(UserInfo.pInstance.UserID + "_DailyQuest").Subtract(DateTime.Now);
			if (CheckDailyMissionsReset())
			{
				NotificationData.RemoveNotification(UserInfo.pInstance.UserID + "_DailyQuest");
			}
			else if (timeSpan.TotalSeconds < 0.0)
			{
				NotificationData.RemoveNotification(UserInfo.pInstance.UserID + "_DailyQuest");
				UiChatHistory.AddSystemNotification(pInstance._DailyQuestNotificationText.GetLocalizedString(), new MessageInfo());
			}
		}
	}

	public bool CanCompleteTask(Task task, string taskType)
	{
		if (taskType == "Delivery")
		{
			if (task.pCompleted)
			{
				return false;
			}
			for (int i = 1; i <= task.pData.Objectives.Count; i++)
			{
				TaskObjective taskObjective = task.pData.Objectives[i - 1];
				int num = taskObjective.Get<int>("ItemID");
				if (num > 0)
				{
					UserItemData userItemData = CommonInventoryData.pInstance.FindItem(num);
					int num2 = 0;
					if (userItemData != null)
					{
						num2 = userItemData.Quantity;
					}
					if (taskObjective.Get<int>("Quantity") > num2)
					{
						return false;
					}
				}
			}
		}
		return true;
	}
}
