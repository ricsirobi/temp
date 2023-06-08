public class ObClickableMission : ObClickable
{
	public string _Action = "";

	public override void OnActivate()
	{
		base.OnActivate();
		if (!(MissionManager.pInstance != null))
		{
			return;
		}
		if (!string.IsNullOrEmpty(_Action))
		{
			MissionManager.pInstance.CheckForTaskCompletion("Action", "ClickObject", base.gameObject.name);
			return;
		}
		Task activeTask = MissionManager.pInstance.GetActiveTask(base.gameObject);
		if (activeTask != null && MissionManager.pInstance.CanActivate(activeTask))
		{
			MissionManager.pInstance.Activate(activeTask);
		}
	}
}
