using System.Collections.Generic;
using UnityEngine;

public class NPCAvatarDaily : NPCAvatar
{
	protected override bool ShowMissionBoard()
	{
		if (MissionManager.pInstance.pDailyMissionStateResult != null && MissionManager.pInstance.pDailyMissionStateResult.UserTimedAchievement != null && MissionManager.pInstance.pDailyMissionStateResult.UserTimedAchievement.Count > 0 && !string.IsNullOrEmpty(GetMissionBoardAsset()))
		{
			StartEngagement(clipGiven: true);
			string[] array = GetMissionBoardAsset().Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnDailyQuestUILoaded, typeof(GameObject));
		}
		else
		{
			OnMissionBoardClosed();
		}
		return true;
	}

	public void OnDailyQuestUILoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			UiDailyQuests.pMissionGroup = _MissionGroupID;
			GameObject obj = Object.Instantiate((GameObject)inObject);
			obj.name = "PfUiDailyQuestDO";
			obj.GetComponent<UiDailyQuests>().pOnUiClosed = OnMissionBoardClosed;
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
		case RsResourceLoadEvent.ERROR:
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	protected override List<Task> GetOfferedTasks()
	{
		List<Task> tasks = MissionManager.pInstance.GetTasks(_MissionGroupID);
		if (tasks != null)
		{
			for (int i = 0; i < tasks.Count; i++)
			{
				int parentMissionId = tasks[i]._Mission.MissionID;
				int taskId = tasks[i].TaskID;
				if (!tasks[i]._Mission.MissionRule.Criteria.Ordered)
				{
					tasks.RemoveAll((Task t) => t._Mission.MissionID == parentMissionId && t.TaskID != taskId);
				}
			}
		}
		return tasks;
	}
}
