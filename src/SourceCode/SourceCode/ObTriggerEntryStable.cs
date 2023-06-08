using UnityEngine;

public class ObTriggerEntryStable : ObTrigger
{
	public int _HatchEggTaskID = 793;

	public LocaleString _CompleteHatchEggTaskText = new LocaleString("You must first hatch a dragon egg before you can enter stables!");

	private KAUIGenericDB mKAUIGenericDB;

	public override void DoTriggerAction(GameObject other)
	{
		if (MissionManager.pIsReady && MissionManager.pInstance != null)
		{
			Task task = MissionManager.pInstance.GetTask(_HatchEggTaskID);
			if (task != null)
			{
				if (!task.pCompleted)
				{
					ShowMessage(_CompleteHatchEggTaskText.GetLocalizedString());
					return;
				}
			}
			else
			{
				UtDebug.Log("Received a null task for TaskID = " + _HatchEggTaskID);
			}
		}
		StableManager.Init();
		base.DoTriggerAction(other);
	}

	private void ShowMessage(string message)
	{
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "HatchDragonEgg");
		mKAUIGenericDB.SetText(message, interactive: false);
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._OKMessage = "OkMessage";
	}

	private void OkMessage()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		Object.Destroy(mKAUIGenericDB.gameObject);
	}
}
