using System.Collections;
using UnityEngine;

public class ObTaskOverrides : ObStatus
{
	public CoCommonLevel _CoCommonLevel;

	public TaskOverride[] _TaskOverrides;

	private void Start()
	{
		base.pIsReady = true;
		Task task = MissionManagerDO.GetPlayerActiveTask();
		Mission mission = null;
		if (task == null)
		{
			task = MissionManagerDO.GetNextActiveTask();
		}
		if (task != null)
		{
			mission = MissionManager.pInstance.GetRootMission(task);
		}
		TaskOverride[] taskOverrides = _TaskOverrides;
		foreach (TaskOverride taskOverride in taskOverrides)
		{
			Task task2 = ((taskOverride._EndTask._TaskID > 0) ? MissionManager.pInstance.GetTask(taskOverride._EndTask._TaskID) : null);
			if (task2 != null && ((taskOverride._EndTask._TaskStatus == TaskState.STARTED && (task2.pStarted || task2.pCompleted)) || (taskOverride._EndTask._TaskStatus == TaskState.COMPLETED && task2.pCompleted)))
			{
				continue;
			}
			Task task3 = ((taskOverride._BeginTask._TaskID > 0) ? MissionManager.pInstance.GetTask(taskOverride._BeginTask._TaskID) : null);
			if (task3 != null && ((taskOverride._BeginTask._TaskStatus == TaskState.STARTED && (task3.pStarted || task3.pCompleted)) || (taskOverride._BeginTask._TaskStatus == TaskState.COMPLETED && task3.pCompleted)))
			{
				Mission rootMission = MissionManager.pInstance.GetRootMission(task3);
				Mission rootMission2 = MissionManager.pInstance.GetRootMission(task2);
				Transform[] markers = taskOverride._Markers;
				if (markers != null && markers.Length != 0)
				{
					_CoCommonLevel._AvatarStartMarker = taskOverride._Markers;
				}
				if (taskOverride._MountableNPC != null)
				{
					_CoCommonLevel._AvatarStartState = AvAvatarState.IDLE;
					_CoCommonLevel._AvatarStartSubState = AvAvatarSubState.NORMAL;
					StartCoroutine(MountOnReady(taskOverride));
					break;
				}
				_CoCommonLevel._AvatarStartState = taskOverride._StartStateOverride;
				_CoCommonLevel._AvatarStartSubState = taskOverride._StartSubStateOverride;
				if (mission != null && ((rootMission != null && rootMission.MissionID == mission.MissionID) || (rootMission2 != null && rootMission2.MissionID == mission.MissionID)))
				{
					break;
				}
			}
		}
	}

	private IEnumerator MountOnReady(TaskOverride taskOverride)
	{
		while (!SanctuaryManager.pInstance.pSetFollowAvatar)
		{
			yield return new WaitForEndOfFrame();
		}
		if (taskOverride._MountableNPC == null)
		{
			yield return null;
		}
		if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pIsMounted)
		{
			SanctuaryManager.pCurPetInstance.OnFlyDismount(AvAvatar.pObject);
		}
		taskOverride._MountableNPC.StartMount(taskOverride._PetSpecialSkillType);
	}
}
