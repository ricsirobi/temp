public class FarmAnimalTutorial : FarmingTutorialBase
{
	public int _SheepPenItemID = 7233;

	public int _SheepInventoryID = 7231;

	public int _SheepFeedInventoryID = 8037;

	public KAWidget[] _EnableWidgets;

	private string mToolbarStore = "";

	private string mToolbarStoreCategory = "";

	protected override void OnStepStarted(int stepIdx, string stepName)
	{
		base.OnStepStarted(stepIdx, stepName);
		FarmManager farmManager = MyRoomsIntMain.pInstance as FarmManager;
		switch (stepIdx)
		{
		case 0:
			if (CommonInventoryData.pInstance.FindItem(_SheepPenItemID) == null)
			{
				CommonInventoryData.pInstance.AddItem(_SheepPenItemID, updateServer: true);
			}
			if ((bool)farmManager && (bool)farmManager._UiToolbar && farmManager._AnimalsStoreInfo != null)
			{
				mToolbarStore = farmManager._UiToolbar._Store;
				mToolbarStoreCategory = farmManager._UiToolbar._StoreCategory;
				farmManager._UiToolbar._Store = farmManager._AnimalsStoreInfo._Store;
				farmManager._UiToolbar._StoreCategory = farmManager._AnimalsStoreInfo._Category;
			}
			SetBackgroundActive(isGlobalClickActive: false, isCSMVisible: false);
			break;
		case 1:
		{
			bool flag = false;
			if (farmManager != null)
			{
				foreach (FarmItem pFarmItem in farmManager.pFarmItems)
				{
					AnimalPenFarmItem animalPenFarmItem = pFarmItem as AnimalPenFarmItem;
					if (animalPenFarmItem != null)
					{
						flag = true;
						animalPenFarmItem.RefreshCSM();
					}
				}
			}
			if (!flag)
			{
				mCurrentTutDBIndex = 0;
				mCurrentTutIndex = 0;
				StartTutorialStep();
			}
			else if (CommonInventoryData.pInstance.FindItem(_SheepInventoryID) == null)
			{
				CommonInventoryData.pInstance.AddItem(_SheepInventoryID, updateServer: true);
			}
			break;
		}
		case 2:
			if (CommonInventoryData.pInstance.FindItem(_SheepFeedInventoryID) == null)
			{
				CommonInventoryData.pInstance.AddItem(_SheepFeedInventoryID, updateServer: true);
			}
			break;
		}
	}

	public override void Update()
	{
		base.Update();
		if (mCSMManager != null && mCurrentTutIndex == 0)
		{
			mCSMManager.SetVisibility(isVisible: false);
		}
	}

	private void OnStoreClosed()
	{
		if (CommonInventoryData.pInstance != null && CommonInventoryData.pInstance.FindItem(_SheepInventoryID) != null && InteractiveTutManager._CurrentActiveTutorialObject != null)
		{
			InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", "BuyWhiteSheepAndExit");
		}
	}

	public override void Exit()
	{
		base.Exit();
		FarmManager farmManager = MyRoomsIntMain.pInstance as FarmManager;
		if ((bool)farmManager && (bool)farmManager._UiToolbar)
		{
			farmManager._UiToolbar._Store = mToolbarStore;
			farmManager._UiToolbar._StoreCategory = mToolbarStoreCategory;
		}
	}

	protected override void RestoreUI()
	{
		base.RestoreUI();
		KAWidget[] enableWidgets = _EnableWidgets;
		foreach (KAWidget kAWidget in enableWidgets)
		{
			if (kAWidget != null)
			{
				kAWidget.SetDisabled(isDisabled: false);
			}
		}
	}

	protected override void OnStepEnded(int stepIdx, string stepName, bool tutQuit)
	{
		base.OnStepEnded(stepIdx, stepName, tutQuit);
		if (stepIdx == 3)
		{
			if (AvAvatar.pToolbar != null)
			{
				KAUI component = AvAvatar.pToolbar.GetComponent<KAUI>();
				DisableOrEnableAllItems(component, flag: false);
			}
			FarmManager farmManager = MyRoomsIntMain.pInstance as FarmManager;
			if (farmManager.pFarmToolbar != null)
			{
				DisableOrEnableAllItems(farmManager.pFarmToolbar, flag: false);
			}
		}
	}
}
