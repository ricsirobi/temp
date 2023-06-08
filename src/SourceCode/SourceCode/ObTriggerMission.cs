using System.Collections.Generic;
using UnityEngine;

public class ObTriggerMission : ObTrigger
{
	public int _TaskID = -1;

	public override void DoTriggerAction(GameObject other)
	{
		if (_TaskID == -1 || MissionManager.pInstance == null)
		{
			return;
		}
		Task task = MissionManager.pInstance.pActiveTasks.Find((Task t) => t.TaskID == _TaskID);
		if (task == null || task._Active || !MissionManager.pInstance.CanActivate(task))
		{
			return;
		}
		List<MissionAction> offers = task.GetOffers(unplayed: false);
		if (offers == null || offers.Count <= 0)
		{
			return;
		}
		List<MissionAction> list = offers.FindAll((MissionAction o) => o.Type == MissionActionType.Popup);
		if (list == null || list.Count <= 0)
		{
			return;
		}
		foreach (MissionAction item in offers)
		{
			item._Played = false;
		}
		MissionManager.pInstance.AddAction(offers, task, MissionManager.ActionType.OFFER);
	}
}
