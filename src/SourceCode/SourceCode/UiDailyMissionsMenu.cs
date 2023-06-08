using System.Collections.Generic;

public class UiDailyMissionsMenu : KAUIMenu
{
	public string _InProgressSprite;

	public string _CompletedSprite;

	public UiNPCQuestDetails _QuestDetailsUi;

	private UiDailyQuests mParentUI;

	private bool mRefreshUI;

	public void Populate()
	{
		if (mParentUI == null)
		{
			mParentUI = (UiDailyQuests)_ParentUi;
		}
		List<Mission> allMissions = MissionManager.pInstance.GetAllMissions(UiDailyQuests.pMissionGroup);
		ClearMenu();
		foreach (Mission item in allMissions)
		{
			KAWidget kAWidget = DuplicateWidget(_Template);
			MissionWidgetData missionWidgetData = new MissionWidgetData();
			missionWidgetData._Mission = item;
			kAWidget.SetUserData(missionWidgetData);
			kAWidget.SetText(MissionManagerDO.GetQuestHeading(item));
			UISprite uISprite = (UISprite)kAWidget.FindChildNGUIItem("IcoStatus");
			if (item.Completed > 0)
			{
				uISprite.UpdateSprite(_CompletedSprite);
			}
			else if (item.pStarted)
			{
				uISprite.UpdateSprite(_InProgressSprite);
			}
			else
			{
				kAWidget.SetInteractive(isInteractive: true);
			}
			AddWidget(kAWidget);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		MissionWidgetData missionWidgetData = (MissionWidgetData)inWidget.GetUserData();
		if (missionWidgetData != null)
		{
			List<Task> tasks = new List<Task>();
			MissionManager.pInstance.GetNextTask(missionWidgetData._Mission, ref tasks);
			if (tasks.Count > 0)
			{
				ShowQuestDetails(tasks[0], OnQuestDetailsClose);
			}
		}
	}

	public void ShowQuestDetails(Task inTask, UiNPCQuestDetails.OnClose inOnCloseDelegate)
	{
		if (inTask != null && _QuestDetailsUi != null)
		{
			mParentUI.SetVisibility(inVisible: false);
			_QuestDetailsUi.SetVisibility(inVisible: true);
			_QuestDetailsUi.ShowTaskDetails(inTask, inOnCloseDelegate);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mRefreshUI)
		{
			Populate();
			AvAvatar.SetUIActive(inActive: false);
			AvAvatar.pState = AvAvatarState.PAUSED;
			mParentUI.SetVisibility(inVisible: true);
			mRefreshUI = false;
		}
	}

	public void OnQuestDetailsClose()
	{
		mRefreshUI = true;
	}
}
