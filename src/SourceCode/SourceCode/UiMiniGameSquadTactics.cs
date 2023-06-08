public class UiMiniGameSquadTactics : ObTriggerSquadTactics
{
	public void OnButtonClick()
	{
		OnTriggerEnter(null);
	}

	protected override void ConfirmTrigger()
	{
		base.ConfirmTrigger();
		uiGenericDB.OnMessageReceived += OnMessageReceived;
	}

	private void OnMessageReceived(string message)
	{
		uiGenericDB.OnMessageReceived -= OnMessageReceived;
		if (!(message == "OnConfirmDBYes"))
		{
			if (message == "CloseDialogBox")
			{
				CloseDialogBox();
			}
		}
		else
		{
			OnConfirmDBYes();
		}
	}
}
