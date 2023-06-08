using System.Collections.Generic;

public class UiNPCMissionBoardDaily : UiNPCMissionBoard
{
	public override void PopulateItems(List<Task> tasksToOffer, OnClose inHandler)
	{
		base.PopulateItems(tasksToOffer, inHandler);
		MissionDailyMissions missionDailyMissions = null;
		for (int i = 0; i < mItemInfo.Count; i++)
		{
			KAWidget kAWidget = mItemInfo[i];
			Mission rootMission = MissionManager.pInstance.GetRootMission(tasksToOffer[i]);
			if (missionDailyMissions == null)
			{
				missionDailyMissions = MissionManager.pInstance.GetDailyMissions(rootMission.GroupID);
			}
			if (missionDailyMissions == null)
			{
				continue;
			}
			MissionDailyMission missionDailyMission = missionDailyMissions.Missions.Find((MissionDailyMission m) => m.MissionID == rootMission.MissionID);
			if (missionDailyMission != null && missionDailyMission.Completed < rootMission.Completed)
			{
				kAWidget.SetInteractive(isInteractive: false);
				KAToggleButton kAToggleButton = (KAToggleButton)kAWidget.FindChildItem("MissionStatus");
				if (kAToggleButton != null)
				{
					kAToggleButton.SetChecked(isChecked: true);
				}
			}
		}
	}
}
