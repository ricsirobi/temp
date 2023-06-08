public class SpectrumLabTutorial : LabTutorial
{
	public override void Start()
	{
		base.Start();
		HandleButtonInteraction("BtnJournal", interactive: false);
	}

	public override void InitStep(int stepIdx, string stepName)
	{
		switch (stepName)
		{
		case "PlaceSpectrumOxygen":
			LabTutorial.InTutorial = true;
			HandleButtonDisable("BtnReset", disable: true);
			HandleButtonDisable("BtnDirectionLeft", disable: true);
			HandleButtonDisable("BtnDirectionRight", disable: true);
			PlayPointerAnim("AniTestItemPointer");
			PlayPointerAnim("AniSpectrometerCruciblePointer");
			SetExclusiveInMenu("SpectrumOxygen");
			break;
		case "ClickNextTask":
			HandleButtonAnimation("BtnDirectionRight", play: true);
			break;
		case "ClickReset":
			HandleButtonAnimation("BtnReset", play: true);
			break;
		case "HeatHydrogen":
			SetExclusiveInMenu("SpectrumHydrogen");
			break;
		case "HeatWater":
			SetExclusiveInMenu("SpectrumWater");
			break;
		case "Continue":
			HandleButtonAnimation("BtnDirectionLeft", play: false);
			HandleButtonAnimation("BtnDirectionRight", play: false);
			HandleButtonInteraction("BtnJournal", interactive: true);
			SetExclusiveInMenu(string.Empty);
			_Manager._MainUI.SetInteractive(interactive: false);
			LabTutorial.InTutorial = false;
			_Manager.pCrucible.OnTutorialDone();
			break;
		default:
			base.InitStep(stepIdx, stepName);
			break;
		}
	}

	public override void ProcessStep(string stepName)
	{
		if (_Manager == null || _Manager.pExperiment == null || _Manager.pCrucible == null || _Manager._Spectrum == null || _Manager._MainUI == null || _Manager.pCurrentDragon == null)
		{
			return;
		}
		if (stepName.Contains("Place"))
		{
			string inItemName = stepName.Replace("Place", "");
			if (_Manager.pCrucible.HasItemInCrucible(inItemName))
			{
				PlayPointerAnim("AniTestItemPointer", play: false);
				PlayPointerAnim("AniSpectrometerCruciblePointer", play: false);
				StartNextTutorial();
			}
		}
		if (stepName.Contains("Heat"))
		{
			HandleHeatUp();
			if (!_Manager._MainUI.pElectricFlow && _Manager.pExperiment.IsTaskDone(stepName))
			{
				StartNextTutorial();
			}
		}
		HandleButtonDisable("BtnBmBack", disable: false);
		base.ProcessStep(stepName);
	}

	public override void HandleHeatUp()
	{
		if (mPrevCrucibleCSMUI == null && SECrucibleContextSensitive.CSMUI != null)
		{
			KAWidget kAWidget = SECrucibleContextSensitive.CSMUI.FindItem("Heat");
			if (kAWidget != null)
			{
				kAWidget.PlayAnim("Flash");
			}
		}
		mPrevCrucibleCSMUI = SECrucibleContextSensitive.CSMUI;
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
		base.Exit();
	}
}
