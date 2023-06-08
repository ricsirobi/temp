using UnityEngine;

public class SkrillLabTutorial : LabTutorial
{
	public float _WaitSecsForCrucibleCheck = 2f;

	private float mElapsedSecsForCrucibleCheck;

	public override void ProcessStep(string stepName)
	{
		if (_Manager == null || _Manager.pExperiment == null || _Manager.pCrucible == null || _Manager._MainUI == null || _Manager.pCurrentDragon == null)
		{
			return;
		}
		switch (stepName)
		{
		case "OpenSatchel":
			base.ProcessStep("Toolbox");
			break;
		case "SelectOhmMeter":
			if (_Manager._MainUI.IsVisible(LabTool.OHMMETER))
			{
				base.InitStep(0, "EntersInLab");
				StartNextTutorial();
				RemoveToolboxCSM();
			}
			break;
		case "PlaceRubber":
		case "PlaceWood":
		case "PlaceGlass":
		case "PlaceIron":
		case "PlaceSilver":
		{
			string text = stepName.Replace("Place", "");
			if (_Manager.pCrucible.HasItemInCrucible(text))
			{
				if (_Manager._MainUI._ExperimentItemMenu != null)
				{
					_Manager._MainUI._ExperimentItemMenu.SetInteractive(interactive: false);
				}
				mElapsedSecsForCrucibleCheck += Time.deltaTime;
				if (mElapsedSecsForCrucibleCheck > _WaitSecsForCrucibleCheck)
				{
					StartNextTutorial();
					mElapsedSecsForCrucibleCheck = 0f;
				}
			}
			else
			{
				InitDragStep(text);
				PlayPointerAnim("AniDragonPointer");
				mElapsedSecsForCrucibleCheck = 0f;
			}
			break;
		}
		case "BreatheElectricity":
			if (_Manager._MainUI.pElectricFlow)
			{
				base.InitStep(0, "EntersInLab");
				StartNextTutorial();
			}
			break;
		case "TestIron":
		case "TestGlass":
		case "TestWood":
		case "TestRubber":
		case "TestSilver":
			if (!_Manager._MainUI.pElectricFlow && _Manager.pExperiment.IsTaskDone(stepName))
			{
				StartNextTutorial();
			}
			break;
		default:
			base.ProcessStep(stepName);
			break;
		}
		HandleButtonDisable("BtnBmBack", disable: false);
	}

	protected override void PlayPointerAnim(string pointerName, bool play = true)
	{
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
		SetExclusiveInMenu(null);
		EnableCrucibleInteraction(state: true);
		base.Exit();
	}

	public override void InitStep(int stepIdx, string stepName)
	{
		if (_Manager == null)
		{
			return;
		}
		switch (stepName)
		{
		case "OpenSatchel":
			EnableCrucibleInteraction(state: false);
			base.InitStep(stepIdx, "EntersInLab");
			base.InitToolboxClickStep("DisableTooboxClose");
			base.InitStep(stepIdx, "Toolbox");
			if (_Manager._MainUI._ExperimentItemMenu != null)
			{
				_Manager._MainUI._ExperimentItemMenu.SetInteractive(interactive: false);
			}
			break;
		case "SelectOhmMeter":
			base.InitStep(stepIdx, "EntersInLab");
			break;
		case "PlaceRubber":
		case "PlaceWood":
		case "PlaceGlass":
		case "PlaceIron":
		case "PlaceSilver":
			if (_Manager._MainUI._ExperimentItemMenu != null)
			{
				_Manager._MainUI._ExperimentItemMenu.SetInteractive(interactive: false);
			}
			break;
		case "BreatheElectricity":
			EnableCrucibleInteraction(state: true);
			break;
		case "TestIron":
		case "TestGlass":
		case "TestWood":
		case "TestRubber":
		case "TestSilver":
			EnableCrucibleInteraction(state: false);
			base.InitStep(stepIdx, "EntersInLab");
			break;
		case "ClickNextTask":
			base.InitStep(stepIdx, "EntersInLab");
			HandleButtonAnimation("BtnDirectionRight", play: true);
			break;
		default:
			base.InitStep(stepIdx, stepName);
			break;
		}
	}

	private void EnableCrucibleInteraction(bool state)
	{
		if (_Manager._CrucibleTriggerBig != null)
		{
			ObContextSensitive component = _Manager._CrucibleTriggerBig.GetComponent<ObContextSensitive>();
			if (component != null)
			{
				component.enabled = state;
			}
			MeshCollider component2 = _Manager._CrucibleTriggerBig.GetComponent<MeshCollider>();
			if (component2 != null)
			{
				component2.enabled = state;
			}
		}
	}
}
