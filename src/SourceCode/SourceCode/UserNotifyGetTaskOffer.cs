using System;
using System.Collections.Generic;

public class UserNotifyGetTaskOffer : UserNotify
{
	[Serializable]
	public class OfferedMission
	{
		public int _MissionID;

		public int[] _TaskIDs;
	}

	public OfferedMission[] _OfferedMissions;

	private int mCurrentTask = -1;

	private int mCurrentMission;

	private int mCurrentTaskID = -1;

	public override void OnWaitBeginImpl()
	{
		if (_OfferedMissions.Length != 0)
		{
			CheckForMissions();
		}
		else
		{
			OnWaitEnd();
		}
	}

	private void CheckForMissions()
	{
		mCurrentTask++;
		if (mCurrentTask < _OfferedMissions[mCurrentMission]._TaskIDs.Length)
		{
			CheckForOfferedTasks();
			return;
		}
		mCurrentMission++;
		mCurrentTask = 0;
		if (mCurrentMission < _OfferedMissions.Length)
		{
			CheckForOfferedTasks();
		}
		else
		{
			OnWaitEnd();
		}
	}

	private void CheckForOfferedTasks()
	{
		Mission mission = MissionManager.pInstance.GetMission(_OfferedMissions[mCurrentMission]._MissionID);
		if (mission != null && mission.Completed <= 0)
		{
			Task task = mission.GetTask(_OfferedMissions[mCurrentMission]._TaskIDs[mCurrentTask]);
			if (task != null && task.Completed <= 0 && MissionManager.pInstance.pActiveTasks.Contains(task))
			{
				List<MissionAction> offers = task.GetOffers(unplayed: false);
				foreach (MissionAction item in offers)
				{
					item._Played = false;
				}
				if (offers != null && offers.Count > 0)
				{
					mCurrentTaskID = task.TaskID;
					MissionManager.AddMissionEventHandler(OnMissionEvent);
					MissionManager.pInstance.ClearActions();
					MissionManager.pInstance.AddAction(offers, task, MissionManager.ActionType.OFFER);
					return;
				}
			}
		}
		CheckForMissions();
	}

	public void OnMissionEvent(MissionEvent inEvent, object inObject)
	{
		if (inEvent != MissionEvent.OFFER_COMPLETE)
		{
			return;
		}
		MissionManager.Action action = (MissionManager.Action)inObject;
		if (action._Object != null && action._Object.GetType() == typeof(Task))
		{
			Task task = (Task)action._Object;
			if (task != null && task.TaskID == mCurrentTaskID)
			{
				CheckForMissions();
			}
		}
	}

	protected override void OnWaitEnd()
	{
		MissionManager.RemoveMissionEventHandler(OnMissionEvent);
		base.OnWaitEnd();
	}
}
