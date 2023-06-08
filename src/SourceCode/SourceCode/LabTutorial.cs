using System;
using System.Collections.Generic;

public class LabTutorial : InteractiveTutManager
{
	protected bool mCheckForItemAdded;

	protected UiContextSensitive mPrevCrucibleCSMUI;

	protected UiContextSensitive mPrevTherometerCSMUI;

	protected bool mCachedHeating;

	public static bool InTutorial;

	public ScientificExperiment _Manager;

	public override void Awake()
	{
		_StepStartedEvent = (StepStartedEvent)Delegate.Combine(_StepStartedEvent, new StepStartedEvent(OnStepStarted));
		base.Awake();
	}

	public override void SetTutDBButtonStates(bool inBtnNext, bool inBtnBack, bool inBtnYes, bool inBtnNo, bool inBtnDone, bool inBtnClose)
	{
		base.SetTutDBButtonStates(inBtnNext, inBtnBack, inBtnYes, inBtnNo, inBtnDone, inBtnClose);
		switch (GetStepName())
		{
		case "EntersInLab":
			base.SetTutDBButtonStates(inBtnNext: true, inBtnBack: false, inBtnYes: false, inBtnNo: false, inBtnDone: false, inBtnClose: false);
			break;
		case "ClickReset":
		case "DragFirewood":
		case "DragRockSalt":
		case "PlaceRubber":
		case "PlaceGold":
		case "PlaceGlass":
		case "PlaceIron":
		case "PlaceSilver":
			base.SetTutDBButtonStates(inBtnNext: false, inBtnBack: false, inBtnYes: false, inBtnNo: false, inBtnDone: false, inBtnClose: false);
			break;
		case "Continue":
			base.SetTutDBButtonStates(inBtnNext: false, inBtnBack: false, inBtnYes: false, inBtnNo: false, inBtnDone: true, inBtnClose: false);
			break;
		}
	}

	public override void Update()
	{
		base.Update();
		ProcessStep(GetStepName());
	}

	public virtual void ProcessStep(string stepName)
	{
		switch (stepName)
		{
		case "DragRockSalt":
			HandleDragItemStep("Rock Salt");
			break;
		case "Toolbox":
			HandleToolboxStep();
			break;
		case "CickThermometerIcon":
			HandleThermometerClickStep();
			break;
		case "ClickFireIcon":
			HandleClickFireIcon();
			break;
		case "HeatUp":
			HandleHeatUp();
			break;
		case "ClickReset":
			HandleClickReset();
			break;
		}
	}

	public void OnStepStarted(int stepIdx, string stepName)
	{
		mCheckForItemAdded = false;
		InitStep(stepIdx, stepName);
		if (mTutorialDB != null)
		{
			KAWidget kAWidget = mTutorialDB.FindItem("NPCIcon");
			if (kAWidget != null)
			{
				kAWidget.SetSprite("IcoDWDragonsDialogueGobber");
			}
		}
	}

	private void CrucibleHeatCSMBlink()
	{
		if (SECrucibleContextSensitive.CSMUI != null)
		{
			KAWidget kAWidget = SECrucibleContextSensitive.CSMUI.FindItem("Heat");
			if (kAWidget != null)
			{
				kAWidget.PlayAnim("Flash");
				kAWidget.SetDisabled(isDisabled: false);
			}
			kAWidget = SECrucibleContextSensitive.CSMUI.FindItem("Freeze");
			if (kAWidget != null)
			{
				kAWidget.SetDisabled(isDisabled: true);
			}
			kAWidget = SECrucibleContextSensitive.CSMUI.FindItem("Pestle");
			if (kAWidget != null)
			{
				kAWidget.SetDisabled(isDisabled: true);
			}
			kAWidget = SECrucibleContextSensitive.CSMUI.FindItem("Water");
			if (kAWidget != null)
			{
				kAWidget.SetDisabled(isDisabled: true);
			}
		}
	}

	private void ResetCrucibleCSM(bool disable)
	{
		if (SECrucibleContextSensitive.CSMUI != null)
		{
			KAWidget kAWidget = SECrucibleContextSensitive.CSMUI.FindItem("Heat");
			if (kAWidget != null)
			{
				kAWidget.PlayAnim("Normal");
				kAWidget.SetDisabled(disable);
			}
			kAWidget = SECrucibleContextSensitive.CSMUI.FindItem("Freeze");
			if (kAWidget != null)
			{
				kAWidget.SetDisabled(disable);
			}
			kAWidget = SECrucibleContextSensitive.CSMUI.FindItem("Pestle");
			if (kAWidget != null)
			{
				kAWidget.SetDisabled(disable);
			}
			kAWidget = SECrucibleContextSensitive.CSMUI.FindItem("Water");
			if (kAWidget != null)
			{
				kAWidget.SetDisabled(disable);
			}
		}
	}

	protected virtual void PlayPointerAnim(string pointerName, bool play = true)
	{
		KAWidget kAWidget = _Manager._MainUI.FindItem(pointerName);
		if (kAWidget != null)
		{
			if (play)
			{
				kAWidget.SetVisibility(inVisible: true);
				kAWidget.PlayAnim("Play");
			}
			else
			{
				kAWidget.SetVisibility(inVisible: false);
			}
		}
	}

	protected virtual void InitDragStep(string itemName)
	{
		if (_Manager == null || _Manager._MainUI == null)
		{
			return;
		}
		SetExclusiveInMenu(itemName);
		if (_Manager._CrucibleTriggerBig != null)
		{
			ObContextSensitive component = _Manager._CrucibleTriggerBig.GetComponent<ObContextSensitive>();
			if (component != null)
			{
				component.enabled = false;
			}
		}
		EnableToolboxCSM(inEnable: false);
		mCheckForItemAdded = true;
		PlayPointerAnim("AniTestItemPointer");
	}

	protected virtual void InitToolboxClickStep(string itemName)
	{
		if (_Manager == null || _Manager._MainUI == null)
		{
			return;
		}
		PlayPointerAnim("AniToolboxPointer", play: false);
		SEToolboxContextSensitive.pInstance.AllowDestroy = false;
		if (SEToolboxContextSensitive.CSMUI != null)
		{
			KAWidget kAWidget = SEToolboxContextSensitive.CSMUI.FindItem(itemName);
			if (kAWidget != null)
			{
				kAWidget.PlayAnim("Flash");
			}
		}
		EnableToolboxCSM(inEnable: false);
	}

	public virtual void InitStep(int stepIdx, string stepName)
	{
		switch (stepName)
		{
		case "EntersInLab":
			InTutorial = true;
			HandleButtonInteraction("BtnJournal", interactive: false);
			if (_Manager != null && _Manager._MainUI != null && _Manager._MainUI._ExperimentItemMenu != null)
			{
				_Manager._MainUI._ExperimentItemMenu.SetInteractive(interactive: false);
			}
			HandleButtonDisable("BtnReset", disable: true);
			HandleButtonDisable("BtnDirectionLeft", disable: true);
			HandleButtonDisable("BtnDirectionRight", disable: true);
			EnableToolboxCSM(inEnable: false);
			break;
		case "DragRockSalt":
			InitDragStep("Rock Salt");
			PlayPointerAnim("AniCruciblePointer");
			ResetCrucibleCSM(disable: true);
			break;
		case "Toolbox":
			if (!(_Manager == null) && !(_Manager._MainUI == null))
			{
				EnableToolboxCSM(inEnable: true);
				PlayPointerAnim("AniToolboxPointer");
			}
			break;
		case "CickThermometerIcon":
			InitToolboxClickStep("ThermoMeter");
			break;
		case "ClickFireIcon":
			CrucibleHeatCSMBlink();
			EnableToolboxCSM(inEnable: false);
			RemoveToolboxCSM();
			if (_Manager._CrucibleTriggerBig != null)
			{
				ObContextSensitive component2 = _Manager._CrucibleTriggerBig.GetComponent<ObContextSensitive>();
				if (component2 != null)
				{
					component2.enabled = true;
				}
			}
			HandleButtonDisable("BtnReset", disable: true);
			_Manager._MainUI.SetInteractive(interactive: true);
			PlayPointerAnim("AniCruciblePointer", play: false);
			EnableToolboxCSM(inEnable: false);
			break;
		case "HeatUp":
			if (_Manager._CrucibleTriggerBig != null)
			{
				ObContextSensitive component3 = _Manager._CrucibleTriggerBig.GetComponent<ObContextSensitive>();
				if (component3 != null)
				{
					component3.enabled = true;
				}
			}
			EnableToolboxCSM(inEnable: false);
			break;
		case "ClickReset":
			HandleButtonAnimation("BtnReset", play: true);
			HandleButtonDisable("BtnBmBack", disable: true);
			HandleButtonDisable("BtnJournal", disable: true);
			if (_Manager._CrucibleTriggerBig != null)
			{
				ObContextSensitive component = _Manager._CrucibleTriggerBig.GetComponent<ObContextSensitive>();
				if (component != null)
				{
					component.enabled = false;
				}
			}
			EnableToolboxCSM(inEnable: false);
			break;
		case "ClickNextTask":
			HandleButtonInteraction("BtnReset", interactive: false);
			HandleButtonAnimation("BtnReset", play: false);
			HandleButtonAnimation("BtnDirectionLeft", play: false);
			HandleButtonAnimation("BtnDirectionRight", play: true);
			EnableToolboxCSM(inEnable: false);
			break;
		case "Continue":
			HandleButtonAnimation("BtnDirectionLeft", play: false);
			HandleButtonAnimation("BtnDirectionRight", play: false);
			HandleButtonInteraction("BtnJournal", interactive: true);
			_Manager._MainUI.SetInteractive(interactive: false);
			InTutorial = false;
			_Manager.pCrucible.OnTutorialDone();
			break;
		}
	}

	protected void HandleButtonAnimation(string itemName, bool play, string animName = "Flash")
	{
		KAWidget kAWidget = _Manager._MainUI.FindItem(itemName);
		if (kAWidget != null && kAWidget.pAnim2D != null)
		{
			if (play)
			{
				kAWidget.SetDisabled(isDisabled: false);
				kAWidget.PlayAnim(animName);
			}
			else if (kAWidget.pAnim2D.pCurrentAnimInfo != null)
			{
				kAWidget.StopAnim();
			}
		}
	}

	protected void HandleButtonInteraction(string itemName, bool interactive)
	{
		KAWidget kAWidget = _Manager._MainUI.FindItem(itemName);
		if (kAWidget != null)
		{
			kAWidget.SetInteractive(interactive);
		}
	}

	protected void HandleButtonDisable(string itemName, bool disable)
	{
		KAWidget kAWidget = _Manager._MainUI.FindItem(itemName);
		if (kAWidget != null)
		{
			kAWidget.SetDisabled(disable);
		}
	}

	private void HandleClickReset()
	{
	}

	public virtual void HandleHeatUp()
	{
		if (_Manager.pCrucible.pHeating)
		{
			SetTutorialBoardVisible(flag: false);
		}
		if (_Manager.pExperiment != null && _Manager.pExperiment.IsTaskDone("BurnRockSalt"))
		{
			StartNextTutorial();
		}
		mPrevCrucibleCSMUI = SECrucibleContextSensitive.CSMUI;
	}

	private void HandleClickFireIcon()
	{
		_Manager._MainUI._ExperimentItemMenu.SetInteractive(interactive: false);
		if (mCachedHeating)
		{
			StartNextTutorial();
		}
		mCachedHeating = _Manager.pCrucible.pHeating;
	}

	protected void RemoveToolboxCSM()
	{
		SEToolboxContextSensitive.pInstance.AllowDestroy = true;
		if (SEToolboxContextSensitive.CSMUI != null)
		{
			((SEToolboxContextSensitive)SEToolboxContextSensitive.CSMUI.pContextSensitiveObj).DestroyMe();
		}
	}

	private void HandleThermometerClickStep()
	{
		if (_Manager != null && _Manager._MainUI != null && _Manager._MainUI.IsVisible(LabTool.THERMOMETER))
		{
			StartNextTutorial();
			return;
		}
		if (mPrevTherometerCSMUI == null && SEToolboxContextSensitive.CSMUI != null)
		{
			KAWidget kAWidget = SEToolboxContextSensitive.CSMUI.FindItem("ThermoMeter");
			if (kAWidget != null)
			{
				kAWidget.PlayAnim("Flash");
			}
			KAWidget kAWidget2 = SEToolboxContextSensitive.CSMUI.FindItem("WeighingMachine");
			KAWidget kAWidget3 = SEToolboxContextSensitive.CSMUI.FindItem("Clock");
			kAWidget2.SetState(KAUIState.NOT_INTERACTIVE);
			kAWidget3.SetState(KAUIState.NOT_INTERACTIVE);
		}
		mPrevTherometerCSMUI = SEToolboxContextSensitive.CSMUI;
	}

	private void HandleToolboxStep()
	{
		if (SEToolboxContextSensitive.CSMUI != null)
		{
			StartNextTutorial();
		}
	}

	protected void HandleDragItemStep(string itemName)
	{
		if (!mCheckForItemAdded || _Manager == null || _Manager.pCrucible == null || !_Manager.pCrucible.HasItemInCrucible(itemName))
		{
			return;
		}
		List<LabTestObject> crucibleItems = _Manager.pCrucible.GetCrucibleItems(itemName);
		if (crucibleItems != null || crucibleItems.Count != 0)
		{
			crucibleItems[0].pRespawnOnCrucibleExit = true;
			crucibleItems[0].ForceStopDestroy();
			SetExclusiveInMenu(string.Empty);
			_Manager._MainUI.SetState(KAUIState.NOT_INTERACTIVE);
			_Manager._MainUI._ExperimentItemMenu.SetState(KAUIState.NOT_INTERACTIVE);
			mCheckForItemAdded = false;
			KAWidget kAWidget = _Manager._MainUI.FindItem("AniTestItemPointer");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
			}
			KAWidget kAWidget2 = _Manager._MainUI.FindItem("AniCruciblePointer");
			if (kAWidget2 != null)
			{
				kAWidget2.SetVisibility(inVisible: false);
			}
			StartNextTutorial();
		}
	}

	protected void SetExclusiveInMenu(string inTestItemName)
	{
		if (_Manager == null || _Manager._MainUI == null || _Manager._MainUI._ExperimentItemMenu == null)
		{
			return;
		}
		List<KAWidget> items = _Manager._MainUI._ExperimentItemMenu.GetItems();
		if (items == null)
		{
			return;
		}
		_Manager._MainUI._ExperimentItemMenu.SetInteractive(!string.IsNullOrEmpty(inTestItemName));
		foreach (KAWidget item in items)
		{
			if (item != null)
			{
				if (string.IsNullOrEmpty(inTestItemName))
				{
					item.SetDisabled(isDisabled: false);
				}
				else if ((item.GetUserData() as ScienceExperimentData).mLabItem.Name == inTestItemName)
				{
					item.SetDisabled(isDisabled: false);
				}
				else
				{
					item.SetDisabled(isDisabled: true);
				}
			}
		}
	}

	public override void OnGenericInfoBoxExit()
	{
		base.OnGenericInfoBoxExit();
		_Manager._MainUI.SetInteractive(interactive: true);
		_Manager._MainUI._ExperimentItemMenu.SetInteractive(interactive: true);
		KAWidget kAWidget = _Manager._MainUI.FindItem("BtnBmBack");
		if (kAWidget != null)
		{
			kAWidget.SetDisabled(isDisabled: false);
		}
		kAWidget = _Manager._MainUI.FindItem("BtnJournal");
		if (kAWidget != null)
		{
			kAWidget.SetDisabled(isDisabled: false);
		}
		if (_Manager._CrucibleTriggerBig != null)
		{
			ObContextSensitive component = _Manager._CrucibleTriggerBig.GetComponent<ObContextSensitive>();
			if (component != null)
			{
				component.enabled = true;
			}
		}
		EnableToolboxCSM(inEnable: true);
	}

	protected void EnableToolboxCSM(bool inEnable)
	{
		if (_Manager == null || _Manager._ToolboxTrigger == null)
		{
			return;
		}
		SEToolboxContextSensitive component = _Manager._ToolboxTrigger.GetComponent<SEToolboxContextSensitive>();
		if (!(component == null))
		{
			component.enabled = inEnable;
			if (component.pClickable != null)
			{
				component.pClickable.enabled = inEnable;
			}
		}
	}

	public override void Exit()
	{
		HandleButtonAnimation("BtnDirectionLeft", play: true, "Normal");
		HandleButtonAnimation("BtnDirectionRight", play: true, "Normal");
		HandleButtonAnimation("BtnReset", play: true, "Normal");
		HandleButtonInteraction("BtnReset", interactive: true);
		ResetCrucibleCSM(disable: false);
		base.Exit();
	}
}
