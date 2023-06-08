using System;
using UnityEngine;

public class AgeUpTutorial : InteractiveTutManager
{
	public PetCSM _PetCSM;

	public override void ShowTutorial()
	{
		if (_PetCSM == null)
		{
			_PetCSM = UnityEngine.Object.FindObjectOfType<PetCSM>();
		}
		if (_PetCSM != null)
		{
			_StepStartedEvent = (StepStartedEvent)Delegate.Combine(_StepStartedEvent, new StepStartedEvent(OnStepStart));
			_StepEndedEvent = (StepEndedEvent)Delegate.Combine(_StepEndedEvent, new StepEndedEvent(OnStepEnd));
			AvAvatar.pInputEnabled = false;
			base.ShowTutorial();
		}
		else
		{
			UtDebug.LogError("_PetCSM is not defined, and no PetCSM object was found in the scene!");
		}
	}

	public void OnStepStart(int stepIdx, string stepName)
	{
		switch (stepName)
		{
		case "ShowAgeUp":
			ObContextSensitive._OnBundleLoadSuccess = (ObContextSensitive.BundleLoadSuccess)Delegate.Combine(ObContextSensitive._OnBundleLoadSuccess, new ObContextSensitive.BundleLoadSuccess(OnCSMLoaded));
			_PetCSM.SendMessage("OnActivate");
			break;
		case "AgeUpWindow":
			UiDragonsAgeUp.pBundleDownloadCallback = (DragonAgeUpConfig.OnDragonAgeUpDone)Delegate.Combine(UiDragonsAgeUp.pBundleDownloadCallback, new DragonAgeUpConfig.OnDragonAgeUpDone(OnAgeUpLoaded));
			break;
		case "AgeUpConfirm":
			UiDragonsAgeUp.pInstance.pUiDragonAgeUpConfirm.FindItem("NoBtn").SetState(KAUIState.DISABLED);
			UiDragonsAgeUp.pInstance.pUiDragonAgeUpConfirm.FindItem("YesBtn").PlayAnim("Flash");
			break;
		}
	}

	private void OnAgeUpLoaded()
	{
		UiDragonsAgeUp.pBundleDownloadCallback = (DragonAgeUpConfig.OnDragonAgeUpDone)Delegate.Remove(UiDragonsAgeUp.pBundleDownloadCallback, new DragonAgeUpConfig.OnDragonAgeUpDone(OnAgeUpLoaded));
		UiDragonsAgeUp.pInstance.FindItem("OKBtn").SetVisibility(inVisible: false);
		UiDragonsAgeUp.pInstance.FindItem("AdultUpgradeBtn").SetInteractive(isInteractive: false);
		UiDragonsAgeUp.pInstance.FindItem("TitanUpgradeBtn").SetInteractive(isInteractive: false);
	}

	private void OnCSMLoaded()
	{
		ObContextSensitive._OnBundleLoadSuccess = (ObContextSensitive.BundleLoadSuccess)Delegate.Remove(ObContextSensitive._OnBundleLoadSuccess, new ObContextSensitive.BundleLoadSuccess(OnCSMLoaded));
		KAWidget kAWidget = null;
		foreach (UiContextSensetiveMenu pUIContextSensitiveMenu in _PetCSM.pUI.pUIContextSensitiveMenuList)
		{
			kAWidget = pUIContextSensitiveMenu.FindItem(_PetCSM._AgeUpCSItemName);
		}
		if (kAWidget != null)
		{
			foreach (KAWidget pMenuGridWidget in _PetCSM.pUI.pMenuGridWidgets)
			{
				if (pMenuGridWidget != kAWidget)
				{
					pMenuGridWidget.SetState(KAUIState.DISABLED);
				}
			}
		}
		kAWidget.PlayAnim("Flash");
	}

	public void OnStepEnd(int stepIdx, string stepName, bool tutQuit)
	{
		if (stepName == "AgeUpConfirm")
		{
			Exit();
		}
	}
}
