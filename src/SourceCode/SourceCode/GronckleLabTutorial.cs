public class GronckleLabTutorial : LabTutorial
{
	protected bool mStartedAction;

	protected bool mBlinkIcon;

	public void HandleBlinkItemCSM(LabToolContextSensitive csm, string blinkItemName, string blinkAnim = "Flash")
	{
		if (csm == null)
		{
			return;
		}
		CrucibleContextMenuData menuDataForType = csm.GetMenuDataForType(ExperimentType.GRONCKLE_IRON);
		if (menuDataForType == null)
		{
			return;
		}
		if (mBlinkIcon && csm.mCSMUI == null)
		{
			mBlinkIcon = false;
		}
		if (mBlinkIcon || !(csm.mCSMUI != null) || menuDataForType._Menus == null)
		{
			return;
		}
		ContextSensitiveState[] menus = menuDataForType._Menus;
		for (int i = 0; i < menus.Length; i++)
		{
			string[] currentContextNamesList = menus[i]._CurrentContextNamesList;
			foreach (string text in currentContextNamesList)
			{
				KAWidget kAWidget = csm.mCSMUI.FindItem(text);
				if (kAWidget != null)
				{
					if (text.Equals(blinkItemName))
					{
						kAWidget.SetDisabled(isDisabled: false);
						kAWidget.PlayAnim(blinkAnim);
					}
					else
					{
						kAWidget.PlayAnim("Normal");
						kAWidget.SetDisabled(isDisabled: true);
					}
				}
			}
		}
		mBlinkIcon = true;
	}

	private void ResetCSM(LabToolContextSensitive csm, bool interactive, string blinkAnim = "Flash")
	{
		if (csm == null)
		{
			return;
		}
		CrucibleContextMenuData menuDataForType = csm.GetMenuDataForType(ExperimentType.GRONCKLE_IRON);
		if (menuDataForType == null || !(csm.mCSMUI != null) || menuDataForType._Menus == null)
		{
			return;
		}
		ContextSensitiveState[] menus = menuDataForType._Menus;
		for (int i = 0; i < menus.Length; i++)
		{
			string[] currentContextNamesList = menus[i]._CurrentContextNamesList;
			foreach (string inWidgetName in currentContextNamesList)
			{
				KAWidget kAWidget = csm.mCSMUI.FindItem(inWidgetName);
				if (kAWidget != null)
				{
					kAWidget.PlayAnim("Normal");
					kAWidget.SetDisabled(interactive);
				}
			}
		}
	}

	public override void ProcessStep(string stepName)
	{
		if (_Manager == null || _Manager.pExperiment == null || _Manager.pCrucible == null || _Manager._Gronckle == null || _Manager._MainUI == null || _Manager.pCurrentDragon == null)
		{
			return;
		}
		switch (stepName)
		{
		case "PourWater":
			HandleBlinkItemCSM(SECrucibleContextSensitive.pInstance, "Water");
			if (mBlinkIcon)
			{
				PlayPointerAnim("AniCruciblePointer", play: false);
			}
			if (_Manager._MainUI._ExperimentItemMenu != null)
			{
				_Manager._MainUI._ExperimentItemMenu.SetInteractive(interactive: false);
			}
			if (_Manager._WaterStream != null)
			{
				if (!mStartedAction)
				{
					mStartedAction = _Manager._WaterStream.isEmitting;
				}
				if (mStartedAction && !_Manager._WaterStream.isEmitting)
				{
					StartNextTutorial();
				}
			}
			break;
		case "Freeze":
			HandleBlinkItemCSM(SECrucibleContextSensitive.pInstance, "Freeze");
			if (mBlinkIcon)
			{
				PlayPointerAnim("AniCruciblePointer", play: false);
			}
			if (_Manager._MainUI._ExperimentItemMenu != null)
			{
				_Manager._MainUI._ExperimentItemMenu.SetInteractive(interactive: false);
			}
			if (!mStartedAction)
			{
				mStartedAction = _Manager.pCrucible.pFreezing;
			}
			if (!_Manager.pCrucible.pPaused && _Manager.pCrucible.pTemperature < -100f)
			{
				_Manager.pCrucible.Freeze(inFreeze: false);
				_Manager.pCrucible.pPaused = true;
			}
			if (mStartedAction && !_Manager.pCrucible.pFreezing)
			{
				StartNextTutorial();
			}
			break;
		case "FreezeGold":
			if (_Manager.pCrucible.pFreezing)
			{
				_Manager.pCrucible.pPaused = false;
			}
			goto case "Freeze";
		case "MeltIce":
			HandleBlinkItemCSM(SECrucibleContextSensitive.pInstance, "Heat");
			if (mBlinkIcon)
			{
				PlayPointerAnim("AniCruciblePointer", play: false);
			}
			if (_Manager._MainUI._ExperimentItemMenu != null)
			{
				_Manager._MainUI._ExperimentItemMenu.SetInteractive(interactive: false);
			}
			if (_Manager.pCrucible.pPaused && _Manager.pCurrentDragon.IsAnimationPlaying(_Manager._BreatheAnim))
			{
				_Manager.pCrucible.pPaused = false;
			}
			if (_Manager.pExperiment.IsTaskDone(stepName))
			{
				StartNextTutorial();
			}
			break;
		case "DragFirewood":
		case "DragDough":
		case "DragGold":
		{
			string itemName = stepName.Replace("Drag", "");
			if (_Manager._Gronckle.IsItemInBelly(itemName))
			{
				SetExclusiveInMenu(string.Empty);
				PlayPointerAnim("AniTestItemPointer", play: false);
				PlayPointerAnim("AniDragonPointer", play: false);
				StartNextTutorial();
				if (_Manager._MainUI._ExperimentItemMenu != null)
				{
					_Manager._MainUI._ExperimentItemMenu.SetInteractive(interactive: false);
				}
			}
			break;
		}
		case "BarfFirewood":
		case "BarfDough":
		case "BarfGold":
			HandleBlinkItemCSM(SEToolboxContextSensitive.pInstance, "Feather");
			if (_Manager._MainUI.pCurrentCursor == UiScienceExperiment.Cursor.FEATHER)
			{
				RemoveToolboxCSM();
			}
			else
			{
				EnableToolboxCSM(inEnable: true);
			}
			if (_Manager._Gronckle.pTriggeredVomit)
			{
				PlayPointerAnim("AniDragonPointer", play: false);
			}
			if (_Manager.pExperiment.IsTaskDone(stepName) || (stepName.Equals("BarfGold") && _Manager._Gronckle.pIsEmptyBelly))
			{
				StartNextTutorial();
			}
			break;
		case "CoolAsh":
			if (_Manager.pExperiment.IsTaskDone(stepName))
			{
				_Manager._MainUI.ShowExperimentDirection(1);
				StartNextTutorial();
				_Manager.pCrucible.pPaused = false;
			}
			break;
		default:
			base.ProcessStep(stepName);
			break;
		}
		HandleButtonDisable("BtnBmBack", disable: false);
	}

	public override void Exit()
	{
		_Manager._MainUI.SetInteractive(interactive: true);
		_Manager.pCrucible.pPaused = false;
		HandleButtonAnimation("BtnReset", play: false);
		HandleButtonDisable("BtnBmBack", disable: false);
		HandleButtonDisable("BtnDirectionLeft", disable: false);
		HandleButtonDisable("BtnDirectionRight", disable: false);
		EnableToolboxCSM(inEnable: true);
		ResetCSM(SECrucibleContextSensitive.pInstance, interactive: true);
		base.Exit();
	}

	protected override void PlayPointerAnim(string pointerName, bool play = true)
	{
	}

	public override void InitStep(int stepIdx, string stepName)
	{
		if (_Manager == null)
		{
			return;
		}
		mBlinkIcon = false;
		mStartedAction = false;
		switch (stepName)
		{
		case "EntersInLab":
			base.InitStep(stepIdx, "EntersInLab");
			HandleButtonAnimation("BtnDirectionLeft", play: false);
			HandleButtonAnimation("BtnDirectionRight", play: false);
			ResetCSM(SECrucibleContextSensitive.pInstance, interactive: false);
			break;
		case "PourWater":
			base.InitStep(stepIdx, "EntersInLab");
			if (_Manager._MainUI != null && _Manager._MainUI._ExperimentItemMenu != null)
			{
				_Manager._MainUI._ExperimentItemMenu.SetInteractive(interactive: false);
			}
			break;
		case "Freeze":
		case "CoolAsh":
			if (_Manager._CrucibleTriggerBig != null)
			{
				ObContextSensitive component2 = _Manager._CrucibleTriggerBig.GetComponent<ObContextSensitive>();
				if (component2 != null)
				{
					component2.enabled = true;
				}
			}
			break;
		case "FreezeGold":
			if (_Manager._CrucibleTriggerBig != null)
			{
				ObContextSensitive component = _Manager._CrucibleTriggerBig.GetComponent<ObContextSensitive>();
				if (component != null)
				{
					component.enabled = true;
				}
			}
			if (_Manager.pCrucible != null)
			{
				_Manager.pCrucible.pPaused = true;
			}
			break;
		case "DragFirewood":
		case "DragDough":
		case "DragGold":
		{
			if (string.Equals(stepName, "DragGold"))
			{
				_Manager._MainUI.ShowExperimentDirection(1);
			}
			base.InitStep(stepIdx, "EntersInLab");
			string itemName = stepName.Replace("Drag", "");
			InitDragStep(itemName);
			PlayPointerAnim("AniDragonPointer");
			break;
		}
		case "BarfFirewood":
		case "BarfDough":
		case "BarfGold":
			InitToolboxClickStep("Feather");
			PlayPointerAnim("AniDragonPointer");
			break;
		default:
			base.InitStep(stepIdx, stepName);
			break;
		}
	}
}
