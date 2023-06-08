namespace StableAbility;

public class AlphaRallyAbility : BaseAbility
{
	public int _ItemID;

	public LocaleString _ServerErrorTitleText = new LocaleString("An Error Occurred");

	public LocaleString _ServerErrorBodyText = new LocaleString("Something went wrong while attempting to activate Alpha Rally. Would you like to try again?");

	public override void ActivateAbility()
	{
		base.ActivateAbility();
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.pState = AvAvatarState.PAUSED;
		KAUICursorManager.SetDefaultCursor("Loading");
		CommonInventoryData.pInstance.AddItem(_ItemID);
		CommonInventoryData.pInstance.Save(InventorySaveEventHandler, null);
	}

	private void InventorySaveEventHandler(bool success, object inUserData)
	{
		if (success)
		{
			UserItemData inUserItemData = CommonInventoryData.pInstance.FindItem(_ItemID);
			RewardMultiplierManager.pInstance?.UseRewardMultiplier(inUserItemData, OnUseItem);
		}
		else
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _ServerErrorBodyText.GetLocalizedString(), _ServerErrorTitleText.GetLocalizedString(), base.gameObject, "OnActivateAbilityRetry", "OnCloseDB", "", "", inDestroyOnClick: true);
		}
	}

	private void OnUseItem(bool success, object inUserData)
	{
		if (!success)
		{
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _ServerErrorBodyText.GetLocalizedString(), _ServerErrorTitleText.GetLocalizedString(), base.gameObject, "OnUseItemRetry", "OnCloseDB", "", "", inDestroyOnClick: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			return;
		}
		StableAbilityManager.pInstance.SaveAbility(this);
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		KAUICursorManager.SetDefaultCursor("Arrow");
		ShowCutScene();
		AvAvatar.pToolbar.GetComponent<UiToolbar>().FetchRewardMultiplier();
	}

	private void OnActivateAbilityRetry()
	{
		ActivateAbility();
	}

	private void OnUseItemRetry()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		UserItemData inUserItemData = CommonInventoryData.pInstance.FindItem(_ItemID);
		RewardMultiplierManager.pInstance?.UseRewardMultiplier(inUserItemData, OnUseItem);
	}

	private void OnCloseDB()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		KAUICursorManager.SetDefaultCursor("Arrow");
	}
}
