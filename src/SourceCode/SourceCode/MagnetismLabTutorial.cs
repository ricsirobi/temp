using System.Collections.Generic;

public class MagnetismLabTutorial : LabTutorial
{
	public override void InitStep(int stepIdx, string stepName)
	{
		if (_Manager == null)
		{
			return;
		}
		switch (stepName)
		{
		case "EntersInLab":
			base.InitStep(stepIdx, "EntersInLab");
			HandleButtonDisable("BtnReset", disable: true);
			if (_Manager._MainUI._ExperimentItemMenu != null)
			{
				_Manager._MainUI._ExperimentItemMenu.SetInteractive(interactive: false);
			}
			break;
		case "EntersInLab2":
		case "EntersInLab3":
		case "EntersInLab4":
			HandleButtonInteraction("BtnJournal", interactive: false);
			HandleButtonDisable("BtnReset", disable: true);
			break;
		case "TestSilver":
		case "TestGold":
		case "TestIron":
		{
			HandleButtonInteraction("BtnJournal", interactive: false);
			HandleButtonDisable("BtnDirectionLeft", disable: true);
			HandleButtonDisable("BtnDirectionRight", disable: true);
			HandleButtonDisable("BtnReset", disable: true);
			if (!(_Manager._MainUI._ExperimentItemMenu != null))
			{
				break;
			}
			_Manager._MainUI._ExperimentItemMenu.SetInteractive(interactive: true);
			string value = stepName.Replace("Test", "");
			List<KAWidget> items = _Manager._MainUI._ExperimentItemMenu.GetItems();
			for (int i = 0; i < items.Count; i++)
			{
				if (!items[i].name.Contains(value))
				{
					items[i].SetInteractive(isInteractive: false);
				}
				else
				{
					items[i].SetInteractive(isInteractive: true);
				}
			}
			break;
		}
		case "ClickReset":
			HandleButtonDisable("BtnReset", disable: false);
			HandleButtonAnimation("BtnReset", play: true);
			if (_Manager._MainUI._ExperimentItemMenu != null)
			{
				_Manager._MainUI._ExperimentItemMenu.SetInteractive(interactive: false);
			}
			break;
		case "ClickNextTask":
			HandleButtonDisable("BtnDirectionRight", disable: false);
			HandleButtonAnimation("BtnDirectionRight", play: true);
			HandleButtonDisable("BtnReset", disable: true);
			if (_Manager._MainUI._ExperimentItemMenu != null)
			{
				_Manager._MainUI._ExperimentItemMenu.SetInteractive(interactive: false);
			}
			break;
		case "Continue":
			HandleButtonInteraction("BtnJournal", interactive: false);
			HandleButtonDisable("BtnReset", disable: true);
			break;
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
		case "TestSilver":
		case "TestGold":
		case "TestIron":
		{
			if (!(_Manager._MainUI._ExperimentItemMenu != null))
			{
				break;
			}
			string value = stepName.Replace("Test", "");
			List<KAWidget> items = _Manager._MainUI._ExperimentItemMenu.GetItems();
			for (int i = 0; i < items.Count; i++)
			{
				if (!items[i].name.Contains(value))
				{
					items[i].SetInteractive(isInteractive: false);
				}
				else
				{
					items[i].SetInteractive(isInteractive: true);
				}
			}
			break;
		}
		}
	}

	public override void Exit()
	{
		HandleButtonDisable("BtnReset", disable: false);
		if (_Manager._MainUI._ExperimentItemMenu != null)
		{
			_Manager._MainUI._ExperimentItemMenu.SetInteractive(interactive: true);
			List<KAWidget> items = _Manager._MainUI._ExperimentItemMenu.GetItems();
			for (int i = 0; i < items.Count; i++)
			{
				items[i].SetInteractive(isInteractive: true);
			}
		}
		base.Exit();
	}

	public override void SetTutDBButtonStates(bool inBtnNext, bool inBtnBack, bool inBtnYes, bool inBtnNo, bool inBtnDone, bool inBtnClose)
	{
		if (mCurrentTutDBIndex >= 0 && mCurrentTutIndex < _TutSteps.Length)
		{
			base.SetTutDBButtonStates(inBtnNext, inBtnBack: false, inBtnYes, inBtnDone, inBtnDone: false, inBtnClose);
		}
	}
}
