using System.Collections.Generic;

public class UiDailyMissionProgressMenu : KAUIMenu
{
	public string _MissionCompletedSprite;

	public void Populate(int targetMissionCount)
	{
		List<Mission> allMissions = MissionManager.pInstance.GetAllMissions(UiDailyQuests.pMissionGroup);
		int num = allMissions.FindAll((Mission mission) => mission.Completed > 0)?.Count ?? 0;
		ClearMenu();
		for (int i = 0; i < targetMissionCount; i++)
		{
			KAWidget kAWidget = DuplicateWidget(_Template);
			kAWidget.SetVisibility(inVisible: true);
			if (!string.IsNullOrEmpty(_MissionCompletedSprite) && i < num)
			{
				kAWidget.SetSprite(_MissionCompletedSprite);
			}
			AddWidget(kAWidget);
		}
	}
}
