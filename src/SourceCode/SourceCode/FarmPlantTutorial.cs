using UnityEngine;

public class FarmPlantTutorial : FarmingTutorialBase
{
	public string _StoreTutorialKeyName = "FarmStoreTutorial";

	public int _DragonNipInstantHarvestItemID = 8255;

	private GameObject mFarmManagerObj;

	protected override void EnableAllItems()
	{
	}

	protected override void OnStepStarted(int stepIdx, string stepName)
	{
		base.OnStepStarted(stepIdx, stepName);
		switch (stepIdx)
		{
		case 0:
			HighlightFarmSlots(highlight: true);
			HideStoreButton();
			break;
		case 3:
			if (CommonInventoryData.pInstance.FindItem(_DragonNipInstantHarvestItemID) == null)
			{
				CommonInventoryData.pInstance.AddItem(_DragonNipInstantHarvestItemID, updateServer: true);
			}
			break;
		}
		HandleStepButtons();
	}

	protected override void OnStepEnded(int stepIdx, string stepName, bool tutQuit)
	{
		base.OnStepEnded(stepIdx, stepName, tutQuit);
		switch (stepIdx)
		{
		case 0:
			HighlightFarmSlots(highlight: false);
			HideStoreButton();
			break;
		case 3:
			if (CommonInventoryData.pInstance.FindItem(_DragonNipInstantHarvestItemID) != null)
			{
				CommonInventoryData.pInstance.RemoveItem(_DragonNipInstantHarvestItemID, updateServer: true);
			}
			break;
		}
		HandleStepButtons();
	}

	public override void SetTutDBButtonStates(bool inBtnNext, bool inBtnBack, bool inBtnYes, bool inBtnNo, bool inBtnDone, bool inBtnClose)
	{
		switch (mCurrentTutIndex)
		{
		case 0:
			base.SetTutDBButtonStates(inBtnNext: false, inBtnBack: false, inBtnYes: false, inBtnNo: false, inBtnDone: false, inBtnClose: false);
			break;
		case 1:
			base.SetTutDBButtonStates(inBtnNext: false, inBtnBack: false, inBtnYes: false, inBtnNo: false, inBtnDone: false, inBtnClose: false);
			break;
		case 2:
			base.SetTutDBButtonStates(inBtnNext: false, inBtnBack: false, inBtnYes: false, inBtnNo: false, inBtnDone: false, inBtnClose: false);
			break;
		case 3:
			base.SetTutDBButtonStates(inBtnNext: false, inBtnBack: false, inBtnYes: false, inBtnNo: false, inBtnDone: false, inBtnClose: false);
			break;
		}
	}

	private void HighlightFarmSlots(bool highlight)
	{
		if (mFarmManagerObj == null)
		{
			mFarmManagerObj = GameObject.Find("PfFarmManager");
		}
		if (!(mFarmManagerObj != null))
		{
			return;
		}
		foreach (FarmItem pFarmItem in mFarmManagerObj.GetComponent<FarmManager>().pFarmItems)
		{
			if (!(pFarmItem is FarmSlot))
			{
				pFarmItem.HighlightObject(canShowHightlight: false);
			}
			else
			{
				pFarmItem.HighlightObject(highlight);
			}
		}
	}

	public override void OnWaitListCompleted()
	{
		base.OnWaitListCompleted();
		if (!mDeleteInstance && ProductData.TutorialComplete(_StoreTutorialKeyName))
		{
			if (!ProductData.TutorialComplete(_TutIndexKeyName))
			{
				ShowTutorial();
			}
			else
			{
				DeleteInstance();
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (Application.isEditor && KAInput.GetKeyUp(KeyCode.Z))
		{
			ShowTutorial();
		}
	}

	public override void LateUpdate()
	{
		base.LateUpdate();
		if (mCurrentTutIndex == 0 && mTutorialStepHandler != null && AvAvatar.pToolbar.activeSelf)
		{
			mTutorialStepHandler.SetupTutorialStep();
		}
		HandleStepButtons();
	}

	private void HandleStepButtons()
	{
		if (mDeleteInstance)
		{
			return;
		}
		GameObject gameObject = GameObject.Find("PfFarmManager");
		if (!(gameObject != null))
		{
			return;
		}
		foreach (FarmItem pFarmItem in gameObject.GetComponent<FarmManager>().pFarmItems)
		{
			if (!(pFarmItem is FarmSlot))
			{
				pFarmItem.SetInteractiveEnabledData("Boost", isEnabled: true);
				pFarmItem.SetInteractiveEnabledData("Add Water", isEnabled: true);
				if (mCurrentTutIndex == 2)
				{
					pFarmItem.SetInteractiveEnabledData("Boost", isEnabled: false);
				}
				else if (mCurrentTutIndex == 3)
				{
					pFarmItem.SetInteractiveEnabledData("Add Water", isEnabled: false);
				}
			}
		}
	}

	private void HideStoreButton()
	{
		if (!(AvAvatar.pToolbar != null) || !AvAvatar.pToolbar.activeSelf)
		{
			return;
		}
		UiToolbar component = AvAvatar.pToolbar.GetComponent<UiToolbar>();
		if (component != null)
		{
			KAWidget kAWidget = component.FindItem("StoreBtn");
			if (kAWidget != null && kAWidget.GetVisibility())
			{
				kAWidget.SetVisibility(inVisible: false);
			}
		}
	}
}
