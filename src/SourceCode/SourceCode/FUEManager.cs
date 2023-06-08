using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(ObStatus))]
public class FUEManager : KAMonoBase
{
	public HUDAction[] _HUDAction;

	public List<TaskStartMarker> _TaskStartMarker;

	public List<AvatarStateBetweenTasks> _AvatarStateBetweenTasks;

	public TaskInputInfo[] _TaskInputInfo;

	public UICSMInfo[] _UICSMInfo;

	public FlightTutorial _FlightTutorial;

	public int _FlightTutorialTask = 3429;

	public DragonWeaponTutorial _WeaponTutorial;

	public int _WeaponTutorialTask = 3430;

	public AgeUpTutorial _AgeUpTutorial;

	public int _AgeUpTutorialTask = 5935;

	public JumpTutorial _JumpTutorial;

	public int _JumpTutorialTask = 5931;

	public BranchUiTutorial _BranchUiTutorial;

	public int _BranchUiTutorialTask = 6619;

	public string _DragonFireBtn = "DragonFire";

	public static FUEManager pInstance;

	public static bool pIsFUERunning;

	public List<string> _TrackedCutscenes = new List<string>();

	public List<int> _TrackedOfferIDs = new List<int>();

	private List<AvatarStateBetweenTasks> mCurrentActiveStates;

	private Task mActiveTaskOnSceneLoad;

	private TaskObjective mLastCollectedObjective;

	private DateTime mLastCollectedTimestamp;

	private string mLastFTUECutscene;

	public void Awake()
	{
		pInstance = this;
	}

	private void Start()
	{
		MissionManager.AddMissionEventHandler(OnMissionEvent);
		CoCommonLevel.WaitListCompleted += OnWaitListCompleted;
		UiJournal.JournalClosed += OnJournalClosed;
		StartCoroutine(CheckForReadyStatus());
		RewardManager.pDisabled = true;
		SetIsFUERunning();
		if (!pIsFUERunning)
		{
			AnalyticAgent.LogFTUEEvent(FTUEEvent.COMPLETED);
			PlayerPrefs.DeleteKey("FTUE_NEWUSER");
		}
	}

	private void OnDestroy()
	{
		if (AvAvatar.pObject != null)
		{
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if (component != null)
			{
				component.OnPetFlyingStateChanged -= OnFlyingStateChanged;
			}
		}
		MissionManager.RemoveMissionEventHandler(OnMissionEvent);
		CoCommonLevel.WaitListCompleted -= OnWaitListCompleted;
		UiJournal.JournalClosed -= OnJournalClosed;
		RewardManager.pDisabled = false;
		pIsFUERunning = false;
		pInstance = null;
	}

	public static bool IsInputEnabled(string inputName)
	{
		bool result = true;
		if (pInstance == null || MissionManager.pInstance == null)
		{
			return result;
		}
		TaskInputInfo taskInputInfo = Array.Find(pInstance._TaskInputInfo, (TaskInputInfo element) => element._InputName == inputName);
		if (taskInputInfo != null)
		{
			Task task = MissionManager.pInstance.GetTask(taskInputInfo._TaskID);
			if (task != null && !task.pStarted && !task.pCompleted)
			{
				result = false;
			}
		}
		return result;
	}

	public static bool IsCSMEnabled(string inUIName, string inCSMName)
	{
		bool result = true;
		if (pInstance == null || MissionManager.pInstance == null)
		{
			return result;
		}
		inUIName = inUIName.Substring(0, inUIName.IndexOf("_"));
		UICSMInfo uICSMInfo = Array.Find(pInstance._UICSMInfo, (UICSMInfo element) => element._UIName == inUIName);
		if (uICSMInfo != null)
		{
			TaskCSMInfo taskCSMInfo = Array.Find(uICSMInfo._TaskCSMInfo, (TaskCSMInfo element) => element._CSMName == inCSMName);
			if (taskCSMInfo != null)
			{
				Task task = MissionManager.pInstance.GetTask(taskCSMInfo._TaskID);
				if (task != null && !task.pStarted && !task.pCompleted)
				{
					result = false;
				}
			}
		}
		return result;
	}

	private string GetAvatarSpawnMarker(int inCurrentTaskID)
	{
		string result = "";
		for (int i = 0; i < _TaskStartMarker.Count; i++)
		{
			TaskStartMarker taskStartMarker = _TaskStartMarker[i];
			if (taskStartMarker._TaskIDs != null && taskStartMarker._TaskIDs.Contains(inCurrentTaskID))
			{
				if (taskStartMarker._Marker != null)
				{
					result = taskStartMarker._Marker.name;
				}
				else
				{
					Debug.Log("taskStartMarker._Marker is NULL at index  " + i);
				}
				break;
			}
		}
		return result;
	}

	private List<AvatarStateBetweenTasks> GetAvatarStateBetweenTasks()
	{
		List<AvatarStateBetweenTasks> list = new List<AvatarStateBetweenTasks>();
		for (int i = 0; i < _AvatarStateBetweenTasks.Count; i++)
		{
			AvatarStateBetweenTasks avatarStateBetweenTasks = _AvatarStateBetweenTasks[i];
			Task task = MissionManager.pInstance.GetTask(avatarStateBetweenTasks._StartTask);
			Task task2 = MissionManager.pInstance.GetTask(avatarStateBetweenTasks._EndTask);
			if (task == null || task2 == null)
			{
				return null;
			}
			if ((task.pStarted || task.pCompleted) && !task2.pCompleted)
			{
				list.Add(avatarStateBetweenTasks);
			}
		}
		return list;
	}

	private void SetAvatarActionState(AvatarStateBetweenTasks currentState)
	{
		if (currentState != null && currentState._Actions != null)
		{
			for (int i = 0; i < currentState._Actions.Count; i++)
			{
				DoAction(currentState._Actions[i], currentState._MountablePetName, currentState._MountPillionNPC);
			}
		}
	}

	private void SetAvatarActionState(List<AvatarStateBetweenTasks> currentStates, bool checkForReloadOnly = false)
	{
		if (currentStates == null || currentStates.Count == 0)
		{
			return;
		}
		for (int i = 0; i < currentStates.Count; i++)
		{
			AvatarStateBetweenTasks avatarStateBetweenTasks = currentStates[i];
			if (avatarStateBetweenTasks != null && (!checkForReloadOnly || !avatarStateBetweenTasks._SetOnReloadOnly))
			{
				SetAvatarActionState(avatarStateBetweenTasks);
			}
		}
	}

	private void OnMissionEvent(MissionEvent missionEvent, object inObject)
	{
		if (!pIsFUERunning)
		{
			return;
		}
		Task task = null;
		switch (missionEvent)
		{
		case MissionEvent.TASK_STARTED:
		{
			task = (Task)inObject;
			if (task == null)
			{
				break;
			}
			DoHUDItemAction(task);
			mCurrentActiveStates = GetAvatarStateBetweenTasks();
			SetAvatarActionState(mCurrentActiveStates, checkForReloadOnly: true);
			TaskInputInfo taskInputInfo = Array.Find(_TaskInputInfo, (TaskInputInfo element) => element._TaskID == task.TaskID);
			if (taskInputInfo != null)
			{
				if (task.TaskID == _WeaponTutorialTask && taskInputInfo._InputName.Equals(_DragonFireBtn))
				{
					UiAvatarControls.pInstance.EnableDragonFireButton(inEnable: true);
				}
				else
				{
					KAInput.pInstance.EnableInputType(taskInputInfo._InputName, InputType.ALL, inEnable: true);
				}
			}
			break;
		}
		case MissionEvent.OFFER:
		case MissionEvent.TASK_END:
		{
			MissionAction missionAction = (MissionAction)inObject;
			if (missionAction != null)
			{
				Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
				if (missionAction.Type == MissionActionType.CutScene && _TrackedCutscenes.Find((string t) => missionAction.Asset.Contains(t)) != null)
				{
					mLastFTUECutscene = missionAction.Asset.Split('/').Last();
					dictionary2.Add("name", mLastFTUECutscene);
					AnalyticAgent.LogFTUEEvent(FTUEEvent.CUTSCENE_STARTED, dictionary2);
				}
				else if (!string.IsNullOrEmpty(mLastFTUECutscene))
				{
					dictionary2.Add("name", mLastFTUECutscene);
					AnalyticAgent.LogFTUEEvent(FTUEEvent.CUTSCENE_ENDED, dictionary2);
					mLastFTUECutscene = null;
				}
			}
			break;
		}
		case MissionEvent.OFFER_COMPLETE:
		{
			MissionManager.Action action = inObject as MissionManager.Action;
			task = ((action != null) ? (action._Object as Task) : null);
			if (task != null)
			{
				if (task.TaskID == _FlightTutorialTask && _FlightTutorial != null)
				{
					if (AvAvatar.pObject != null)
					{
						AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
						if (component != null)
						{
							if (component.pFlyingState == FlyingState.Hover)
							{
								_FlightTutorial.ShowTutorial();
							}
							else
							{
								component.OnPetFlyingStateChanged += OnFlyingStateChanged;
							}
							break;
						}
					}
					_FlightTutorial.ShowTutorial();
				}
				else if (task.TaskID == _WeaponTutorialTask && _WeaponTutorial != null)
				{
					_WeaponTutorial.ShowTutorial();
				}
				else if (task.TaskID == _AgeUpTutorialTask && _AgeUpTutorial != null)
				{
					_AgeUpTutorial.ShowTutorial();
				}
				else if (task.TaskID == _JumpTutorialTask && _JumpTutorial != null)
				{
					_JumpTutorial.ShowTutorial();
				}
				else if (task.TaskID == _BranchUiTutorialTask && _BranchUiTutorial != null)
				{
					_BranchUiTutorial.ShowTutorial();
				}
			}
			if (_TrackedOfferIDs.Contains(task.TaskID))
			{
				Dictionary<string, object> dictionary4 = new Dictionary<string, object>();
				dictionary4.Add("name", task.Name);
				AnalyticAgent.LogFTUEEvent(FTUEEvent.OFFER_CLOSED, dictionary4);
			}
			break;
		}
		case MissionEvent.TASK_COMPLETE:
			task = (Task)inObject;
			if (task != null)
			{
				StopHUDItemAnim(task);
			}
			break;
		case MissionEvent.TASK_END_COMPLETE:
			task = inObject as Task;
			if (task != null)
			{
				Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
				dictionary3.Add("name", task.Name);
				AnalyticAgent.LogFTUEEvent(FTUEEvent.STEP, dictionary3);
				SetIsFUERunning();
				if (UnityAnalyticsAgent.pNewUser && task._Mission.pCompleted && !pIsFUERunning)
				{
					AnalyticAgent.LogFTUEEvent(FTUEEvent.COMPLETED);
					PlayerPrefs.DeleteKey("FTUE_NEWUSER");
				}
				CheckForActionCompletion(task);
			}
			break;
		case MissionEvent.COLLECTED:
		{
			task = (Task)inObject;
			if (task == null)
			{
				break;
			}
			TaskObjective taskObjective = task.pData?.Objectives[task.pData.Objectives.Count - 1];
			int result = -1;
			int.TryParse(taskObjective?.Pairs?.Find((TaskPair t) => t.Key == "Quantity")?.Value, out result);
			if (UnityAnalyticsAgent.pNewUser && taskObjective != null && result > 1)
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				string value = taskObjective?.Pairs?.Find((TaskPair t) => t.Key == "Name")?.Value;
				if (mLastCollectedObjective == taskObjective)
				{
					dictionary.Add("duration", (DateTime.Now - mLastCollectedTimestamp).TotalSeconds);
				}
				else
				{
					mLastCollectedObjective = taskObjective;
					mLastCollectedTimestamp = DateTime.Now;
				}
				dictionary.Add("name", value);
				dictionary.Add("count", taskObjective?._Collected);
				AnalyticAgent.LogFTUEEvent(FTUEEvent.COLLECT_TASK, dictionary);
			}
			break;
		}
		case MissionEvent.TASK_FAIL:
			break;
		}
	}

	private static void SetIsFUERunning()
	{
		pIsFUERunning = false;
		foreach (MissionManagerDO.FUEMission fUEMission in (MissionManager.pInstance as MissionManagerDO)._FUEMissions)
		{
			Mission mission = MissionManager.pInstance.GetMission(fUEMission._MissionID);
			if (mission != null && !mission.pCompleted && mission.VersionID >= fUEMission._VersionID)
			{
				pIsFUERunning = true;
				break;
			}
		}
	}

	private void CheckForActionCompletion(Task task)
	{
		for (int i = 0; i < _AvatarStateBetweenTasks.Count; i++)
		{
			AvatarStateBetweenTasks avatarStateBetweenTasks = _AvatarStateBetweenTasks[i];
			if (task.TaskID == avatarStateBetweenTasks._EndTask)
			{
				for (int j = 0; j < avatarStateBetweenTasks._Actions.Count; j++)
				{
					CompleteAction(avatarStateBetweenTasks._Actions[j], avatarStateBetweenTasks);
				}
			}
		}
	}

	private void CompleteAction(AvatarActions action, AvatarStateBetweenTasks currentstate)
	{
		switch (action)
		{
		case AvatarActions.MOUNT_PILLION:
		{
			if (!(SanctuaryManager.pCurPetInstance != null))
			{
				break;
			}
			GameObject gameObject = GameObject.Find(currentstate._MountablePetName);
			if (!(gameObject != null))
			{
				break;
			}
			MountableNPCPet component = gameObject.GetComponent<MountableNPCPet>();
			if (!(component != null))
			{
				break;
			}
			component.DetachPillionRider();
			for (int i = 0; i < MissionManager.pInstance.pActiveTasks.Count; i++)
			{
				Task task = MissionManager.pInstance.pActiveTasks[i];
				List<TaskSetup> setups = task.GetSetups();
				if (setups == null)
				{
					continue;
				}
				foreach (TaskSetup item in setups)
				{
					item.Setup(task);
				}
			}
			break;
		}
		case AvatarActions.MOUNT:
			if (SanctuaryManager.pCurPetInstance != null)
			{
				SanctuaryManager.pMountedState = false;
				SanctuaryManager.pCurPetInstance.OnFlyDismount(AvAvatar.pObject);
			}
			break;
		}
	}

	private void DoAction(AvatarActions action, string mountablePetName, string pillionNPCName)
	{
		switch (action)
		{
		case AvatarActions.MOUNT:
		{
			GameObject gameObject = GameObject.Find(mountablePetName);
			if (gameObject != null)
			{
				MountableNPCPet component = gameObject.GetComponent<MountableNPCPet>();
				if (component != null)
				{
					component.StartMount();
				}
			}
			break;
		}
		case AvatarActions.MOUNT_PILLION:
		{
			GameObject gameObject2 = GameObject.Find(mountablePetName);
			if (gameObject2 != null)
			{
				MountableNPCPet component2 = gameObject2.GetComponent<MountableNPCPet>();
				if (component2 != null)
				{
					component2.SetPillionRider(pillionNPCName);
				}
			}
			break;
		}
		case AvatarActions.STABLE_DRAGON:
			if (SanctuaryManager.pCurPetInstance != null)
			{
				if (SanctuaryManager.pMountedState || SanctuaryManager.pCurPetInstance.pIsMounted)
				{
					SanctuaryManager.pMountedState = false;
					SanctuaryManager.pCurPetInstance.OnFlyDismountImmediate(AvAvatar.pObject);
					AvAvatar.pSubState = AvAvatarSubState.NORMAL;
				}
				RaisedPetData.SetSelectedPet(-1, unselectOtherPets: true, null, null);
				if (SanctuaryManager.pCurPetInstance != null)
				{
					UnityEngine.Object.Destroy(SanctuaryManager.pCurPetInstance.gameObject);
				}
				SanctuaryManager.pCurPetInstance = null;
				SanctuaryManager.pCurPetData = null;
				if (SanctuaryManager.pInstance != null && SanctuaryManager.pInstance.pPetMeter != null)
				{
					SanctuaryManager.pInstance.pPetMeter.SetVisibility(inVisible: false);
				}
			}
			break;
		}
	}

	public void SetupHUD(string inItemName = "")
	{
		if (!pIsFUERunning)
		{
			return;
		}
		for (int i = 0; i < _HUDAction.Length; i++)
		{
			if (!string.IsNullOrEmpty(inItemName) && _HUDAction[i]._UiObjectName != inItemName)
			{
				continue;
			}
			GameObject gameObject = _HUDAction[i]._UiObject;
			if (gameObject == null)
			{
				gameObject = GameObject.Find(_HUDAction[i]._UiObjectName);
			}
			if (!(gameObject != null))
			{
				continue;
			}
			_HUDAction[i].pUI = gameObject.GetComponent<KAUI>();
			if (_HUDAction[i].pUI == null)
			{
				continue;
			}
			if (_HUDAction[i]._Items.Count > 0)
			{
				for (int j = 0; j < _HUDAction[i].pUI.GetItemCount(); j++)
				{
					SetItemVisibilityRecursive(_HUDAction[i].pUI.FindItemAt(j));
				}
			}
			else
			{
				_HUDAction[i].pUI.SetVisibility(inVisible: false);
			}
			for (int k = 0; k < _HUDAction[i]._Items.Count; k++)
			{
				_HUDAction[i]._Items[k].pTask = MissionManager.pInstance.GetTask(_HUDAction[i]._Items[k]._Task);
				if (_HUDAction[i]._Items[k].pTask != null)
				{
					_HUDAction[i]._Items[k].pWidget = _HUDAction[i]._Items[k]._Widget;
					if (_HUDAction[i]._Items[k].pWidget == null)
					{
						_HUDAction[i]._Items[k].pWidget = _HUDAction[i].pUI.FindItem(_HUDAction[i]._Items[k]._ItemName);
					}
					if (_HUDAction[i]._Items[k].pWidget == null)
					{
						UtDebug.Log("Widget not found for " + _HUDAction[i]._Items[k]._ItemName + "   in :" + _HUDAction[i].pUI);
					}
					else if (_HUDAction[i]._Items[k].pTask.pCompleted)
					{
						_HUDAction[i]._Items[k].pWidget.SetVisibility(inVisible: true);
					}
					else if (_HUDAction[i]._Items[k].pTask.pStarted)
					{
						DoHUDItemAction(_HUDAction[i]._Items[k]);
					}
				}
			}
		}
	}

	private void SetItemVisibilityRecursive(KAWidget item)
	{
		if (item != null)
		{
			item.SetVisibility(inVisible: false);
			for (int i = 0; i < item.pChildWidgets.Count; i++)
			{
				SetItemVisibilityRecursive(item.pChildWidgets[i]);
			}
		}
	}

	private void DoHUDItemAction(Task task)
	{
		StopAllCoroutines();
		for (int i = 0; i < _HUDAction.Length; i++)
		{
			for (int j = 0; j < _HUDAction[i]._Items.Count; j++)
			{
				if (task == _HUDAction[i]._Items[j].pTask)
				{
					DoHUDItemAction(_HUDAction[i]._Items[j]);
				}
			}
		}
	}

	private void StopHUDItemAnim(Task task)
	{
		for (int i = 0; i < _HUDAction.Length; i++)
		{
			List<HUDActionItem> list = _HUDAction[i]._Items.FindAll((HUDActionItem element) => element._Task == task.TaskID && element._Action == HudActions.FLASH);
			if (list == null)
			{
				continue;
			}
			for (int j = 0; j < list.Count; j++)
			{
				KAWidget pWidget = list[j].pWidget;
				if (pWidget != null)
				{
					pWidget.StopAnim("Flash");
					pWidget.PlayAnim("Normal");
				}
			}
		}
	}

	private void DoHUDItemAction(HUDActionItem actionItem)
	{
		if (actionItem.pWidget == null)
		{
			return;
		}
		actionItem.pWidget.SetVisibility(inVisible: true);
		switch (actionItem._Action)
		{
		case HudActions.DISABLE:
			actionItem.pWidget.SetDisabled(isDisabled: true);
			break;
		case HudActions.ENABLE:
			actionItem.pWidget.SetDisabled(isDisabled: false);
			break;
		case HudActions.FLASH:
			actionItem.pWidget.SetDisabled(isDisabled: false);
			if (actionItem._RepeatTimeInterval == 0f)
			{
				actionItem.pWidget.PlayAnim("Flash", -1);
			}
			else
			{
				StartCoroutine(RepeatHUDAction(actionItem.pWidget, actionItem._Time, actionItem._RepeatTimeInterval));
			}
			break;
		case HudActions.INVISIBLE:
			actionItem.pWidget.SetVisibility(inVisible: false);
			break;
		case HudActions.VISIBLE:
			break;
		}
	}

	private IEnumerator RepeatHUDAction(KAWidget widget, float animTime, float delay)
	{
		while (true)
		{
			widget.PlayAnim("Flash", -1);
			yield return new WaitForSeconds(animTime);
			widget.StopAnim("Flash");
			widget.PlayAnim("Normal");
			yield return new WaitForSeconds(delay);
		}
	}

	private void OnStatusReady()
	{
		SetAvatarStartLocation();
	}

	public void OnFlyingStateChanged(FlyingState newState)
	{
		switch (newState)
		{
		case FlyingState.TakeOff:
		case FlyingState.Hover:
		case FlyingState.TakeOffGliding:
			if (_FlightTutorial != null)
			{
				_FlightTutorial.ShowTutorial();
			}
			break;
		}
		AnalyticAgent.LogFTUEEvent(FTUEEvent.FIRST_FLIGHT);
	}

	public Task GetCurrentActiveTask()
	{
		Task task = MissionManagerDO.GetPlayerActiveTask();
		if (task == null)
		{
			task = MissionManagerDO.GetNextActiveTask();
		}
		return task;
	}

	private void SetAvatarStartLocation()
	{
		mActiveTaskOnSceneLoad = GetCurrentActiveTask();
		if (mActiveTaskOnSceneLoad != null)
		{
			string avatarSpawnMarker = GetAvatarSpawnMarker(mActiveTaskOnSceneLoad.TaskID);
			if (!string.IsNullOrEmpty(avatarSpawnMarker))
			{
				AvAvatar.pStartLocation = avatarSpawnMarker;
			}
		}
	}

	private void OnJournalClosed()
	{
		SetAvatarActionState(mCurrentActiveStates);
	}

	private void OnWaitListCompleted()
	{
		CoCommonLevel.WaitListCompleted -= OnWaitListCompleted;
		if (!pIsFUERunning)
		{
			return;
		}
		if (_BranchUiTutorial != null && MissionManager.IsTaskActive("Action", "ItemName", _BranchUiTutorial._TutIndexKeyName))
		{
			_BranchUiTutorial.ShowTutorial();
		}
		SetupHUD();
		mCurrentActiveStates = GetAvatarStateBetweenTasks();
		if (mCurrentActiveStates == null)
		{
			return;
		}
		SetAvatarActionState(mCurrentActiveStates);
		if (mCurrentActiveStates.Exists((AvatarStateBetweenTasks t) => t._AvatarState == "Mounted") && AvAvatar.pObject != null)
		{
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if (component != null)
			{
				component.OnPetFlyingStateChanged += OnFlyingStateChanged;
			}
		}
	}

	private IEnumerator CheckForReadyStatus()
	{
		while (!MissionManager.pIsReady)
		{
			yield return null;
		}
		SetIsFUERunning();
		if (pIsFUERunning)
		{
			OnStatusReady();
		}
		else
		{
			RewardManager.pDisabled = false;
		}
		GetComponent<ObStatus>().pIsReady = true;
	}
}
