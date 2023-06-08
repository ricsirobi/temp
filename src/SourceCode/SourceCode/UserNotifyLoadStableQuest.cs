using UnityEngine;

public class UserNotifyLoadStableQuest : UserNotify
{
	public static bool StartStableQuestOnStableLoad;

	public override void OnWaitBeginImpl()
	{
		if (StartStableQuestOnStableLoad)
		{
			StartStableQuestOnStableLoad = false;
			StableManager.pInstance.SendMessage("OpenStableQuest", SendMessageOptions.RequireReceiver);
			UiStableQuestMain.OnStableQuestUIHandler += OnStableQuestUIClosed;
		}
		else
		{
			OnWaitEnd();
		}
	}

	public void OnStableQuestUIClosed()
	{
		UiStableQuestMain.OnStableQuestUIHandler -= OnStableQuestUIClosed;
		OnWaitEnd();
	}
}
