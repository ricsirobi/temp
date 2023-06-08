public class CropFarmItem : FarmItem
{
	private bool mIsHarvestParamsSet;

	public override void ProcessCurrentStage()
	{
		if (base.pFarmManager == null || !base.pFarmManager.pIsReady || !mInitialized || base.pFarmManager.pIsBuildMode || !base.pIsRuleSetInitialized || !base.pIsStateSet)
		{
			return;
		}
		base.ProcessCurrentStage();
		if (GetTimeLeftInCurrentStage() > 0f)
		{
			return;
		}
		bool flag = base.pCurrentStage != null && base.pCurrentStage._Name.Equals(GetHarvestStageName());
		if (!mIsHarvestParamsSet && flag)
		{
			mIsHarvestParamsSet = true;
			if (base.pFarmManager._PlantTutorial != null)
			{
				base.pFarmManager._PlantTutorial.SendMessage("TutorialManagerAsyncMessage", "ReadyForHarvest");
			}
			if (base.pFarmManager._HarvestTutorial != null)
			{
				base.pFarmManager._HarvestTutorial.SendMessage("StartTutorial");
			}
			UpdateItemWithStage(base.pCurrentStage);
			UpdateContextIcon();
		}
	}

	public override void UpdateContextIcon()
	{
		if (base.pIsBuildMode && base.pClickable != null)
		{
			base.pClickable._RollOverCursorName = "Activate";
		}
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		if (base.pCurrentStage != null && base.pCurrentStage._Name.Equals("Harvest") && base.pFarmManager._HarvestTutorial != null)
		{
			base.pFarmManager._HarvestTutorial.SendMessage("TutorialManagerAsyncMessage", "GOClicked");
		}
	}

	protected override void OnContextAction(string inActionName)
	{
		base.OnContextAction(inActionName);
		switch (inActionName)
		{
		case "Add Water":
			GotoNextStage();
			if (base.pFarmManager._PlantTutorial != null)
			{
				base.pFarmManager._PlantTutorial.SendMessage("TutorialManagerAsyncMessage", "PlantWatered");
			}
			break;
		case "Harvest":
		case "WitheredHarvest":
			if (base.pFarmManager._HarvestTutorial != null)
			{
				base.pFarmManager._HarvestTutorial.SendMessage("TutorialManagerAsyncMessage", "SickleClicked");
			}
			KAUICursorManager.SetDefaultCursor("Arrow");
			GotoNextStage();
			break;
		case "Boost":
		case "Revive":
			if (CheckGemsAvailable(GetSpeedupCost()))
			{
				GotoNextStage(speedup: true);
			}
			break;
		case "AdBtn":
			if (AdManager.pInstance.AdAvailable(base.pFarmManager._AdEventType, AdType.REWARDED_VIDEO))
			{
				AdManager.DisplayAd(base.pFarmManager._AdEventType, AdType.REWARDED_VIDEO, base.gameObject);
			}
			break;
		}
	}

	public override void Deserialize(FarmItemState inFarmItemState)
	{
		mInitialized = true;
	}

	protected override void DoHarvest()
	{
		base.DoHarvest();
		if (CanDestroyOnHarvest())
		{
			base.pFarmManager.RemoveRoomObject(base.gameObject, isDestroy: true, noSaveInServer: true);
		}
	}

	protected override string GetHarvestStageName()
	{
		return "Harvest";
	}

	public override void SetSpeedupCostText()
	{
		if (!(base.pUI != null))
		{
			return;
		}
		KAWidget kAWidget = base.pUI.FindItem(_SpeedupActionName);
		if (kAWidget == null)
		{
			kAWidget = base.pUI.FindItem("Revive");
		}
		if (kAWidget != null)
		{
			UILabel uILabel = kAWidget.FindChildNGUIItem("Text") as UILabel;
			if (uILabel != null)
			{
				int speedupCost = GetSpeedupCost();
				uILabel.text = ((speedupCost > 0) ? GetSpeedupCost().ToString() : "");
			}
		}
	}

	protected override void OnMenuActive(ContextSensitiveStateType inMenuType)
	{
		base.OnMenuActive(inMenuType);
		if (InteractiveTutManager._CurrentActiveTutorialObject != null && InteractiveTutManager._CurrentActiveTutorialObject == base.pFarmManager._PlantTutorial)
		{
			KAWidget kAWidget = base.pUI.FindItem("AdBtn");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
			}
		}
	}
}
