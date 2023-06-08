using System.Collections.Generic;

public class TitrationLabTutorial : LabTutorial
{
	public override void Start()
	{
		base.Start();
		HandleButtonInteraction("BtnJournal", interactive: false);
		KAWidget[] componentsInChildren = _Manager._MainUI._TitrationWidgetGroup.GetComponentsInChildren<KAWidget>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SetDisabled(isDisabled: true);
		}
	}

	public override void InitStep(int stepIdx, string stepName)
	{
		switch (stepName)
		{
		case "DragSoap":
			LabTutorial.InTutorial = true;
			HandleButtonDisable("BtnReset", disable: true);
			HandleButtonDisable("BtnDirectionLeft", disable: true);
			HandleButtonDisable("BtnDirectionRight", disable: true);
			PlayPointerAnim("AniTestItemPointer");
			PlayPointerAnim("AniCruciblePointer");
			SetExclusiveInMenu("Soap");
			break;
		case "DragCabbage":
			HandleButtonDisable("BtnReset", disable: true);
			PlayPointerAnim("AniNextTestItemPointer");
			PlayPointerAnim("AniCruciblePointer");
			HandleButtonDisable("BtnDirectionLeft", disable: true);
			HandleButtonDisable("BtnDirectionRight", disable: true);
			SetExclusiveInMenu("Cabbage");
			break;
		case "AddAcid10":
			PlayPointerAnim("AniTitrationPointer10");
			HandleButtonDisable("BtnTitration_-10", disable: false);
			SetExclusiveInMenu("None");
			break;
		case "AddAcid5":
			PlayPointerAnim("AniTitrationPointer10", play: false);
			PlayPointerAnim("AniTitrationPointer5");
			HandleButtonDisable("BtnTitration_-10", disable: true);
			HandleButtonDisable("BtnTitration_-5", disable: false);
			break;
		case "Record":
			HandleButtonDisable("BtnTitration_-5", disable: true);
			HandleButtonDisable("BtnReset", disable: true);
			PlayPointerAnim("AniTitrationPointer5", play: false);
			break;
		case "ClickReset":
			HandleButtonAnimation("BtnReset", play: true);
			break;
		case "Finish":
		{
			LabTutorial.InTutorial = false;
			_Manager.pCrucible.OnTutorialDone();
			HandleButtonInteraction("BtnJournal", interactive: true);
			HandleButtonAnimation("BtnReset", play: false);
			HandleButtonDisable("BtnBmBack", disable: false);
			HandleButtonDisable("BtnDirectionLeft", disable: false);
			HandleButtonDisable("BtnDirectionRight", disable: false);
			_Manager.pCrucible.pPaused = false;
			_Manager._MainUI.SetInteractive(interactive: true);
			_Manager._MainUI._ExperimentItemMenu.SetInteractive(interactive: true);
			List<KAWidget> items = _Manager._MainUI._ExperimentItemMenu.GetItems();
			for (int i = 0; i < items.Count; i++)
			{
				items[i].SetDisabled(isDisabled: false);
			}
			KAWidget[] componentsInChildren = _Manager._MainUI._TitrationWidgetGroup.GetComponentsInChildren<KAWidget>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].SetDisabled(isDisabled: false);
			}
			break;
		}
		default:
			base.InitStep(stepIdx, stepName);
			break;
		}
	}

	public override void ProcessStep(string stepName)
	{
		if (_Manager == null || _Manager.pExperiment == null || _Manager.pCrucible == null || _Manager._Titration == null || _Manager._MainUI == null || _Manager.pCurrentDragon == null)
		{
			return;
		}
		if (!(stepName == "DragSoap"))
		{
			if (stepName == "DragCabbage" && _Manager.pCrucible.HasItemInCrucible("Cabbage"))
			{
				PlayPointerAnim("AniNextTestItemPointer", play: false);
				PlayPointerAnim("AniCruciblePointer", play: false);
				StartNextTutorial();
			}
		}
		else if (_Manager.pCrucible.HasItemInCrucible("Soap"))
		{
			PlayPointerAnim("AniTestItemPointer", play: false);
			PlayPointerAnim("AniCruciblePointer", play: false);
			StartNextTutorial();
		}
		HandleButtonDisable("BtnBmBack", disable: false);
		base.ProcessStep(stepName);
	}
}
