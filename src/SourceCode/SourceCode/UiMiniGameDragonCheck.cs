using UnityEngine;

public class UiMiniGameDragonCheck : ObTriggerDragonCheck
{
	protected virtual void OnButtonClick()
	{
		UiMiniGameDragonAgeUp._OnMiniGameLoadStart = DoTriggerAction;
		OnTriggerEnter(null);
	}

	public override void DoTriggerAction(GameObject other)
	{
		base.DoTriggerAction(other);
		UiMiniGameDragonAgeUp._OnMiniGameLoadStart = null;
	}

	protected override void ConfirmTrigger()
	{
		base.ConfirmTrigger();
		uiGenericDB.OnMessageReceived += OnMessageReceived;
	}

	protected override void OnDragonCustomizationDone()
	{
		mTriggeredCollider = null;
		base.OnDragonCustomizationDone();
	}

	private void OnMessageReceived(string message)
	{
		UiMiniGameDragonAgeUp._OnMiniGameLoadStart = null;
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

	protected override void ShowPetTiredDialog(bool isLowEnergy)
	{
		base.ShowPetTiredDialog(isLowEnergy);
		UiPetEnergyGenericDB.pInstance.OnMessageReceived += OnPetTiredMessageReceived;
		UiPetEnergyGenericDB.pInstance._OKMessage = "CloseAndShowAgeUpPrompt";
		UiPetEnergyGenericDB.pInstance._NoMessage = "OnCloseDB";
	}

	private void OnPetTiredMessageReceived(string message)
	{
		UiPetEnergyGenericDB.pInstance.OnMessageReceived -= OnMessageReceived;
		if (!(message == "CloseAndShowAgeUpPrompt"))
		{
			if (message == "OnCloseDB")
			{
				CloseDialogBox();
			}
		}
		else
		{
			CloseAndShowAgeUpPrompt();
		}
	}
}
