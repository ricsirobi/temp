using UnityEngine;

public class LabPortalTrigger : ObTriggerDragonCheck
{
	public LocaleString _NoActiveText = new LocaleString("You do not have a current experiment.");

	public LocaleString _NotificationTitleText = new LocaleString("Notification");

	public override void OnTriggerEnter(Collider inCollider)
	{
		if (AvAvatar.IsCurrentPlayer(inCollider.gameObject))
		{
			AvAvatar.pState = AvAvatarState.PAUSED;
			KAUICursorManager.SetDefaultCursor("Loading");
			if (ScientificExperiment.pUseExperimentCheat)
			{
				base.OnTriggerEnter(inCollider);
				return;
			}
			AvAvatar.SetUIActive(inActive: false);
			LabData.Load(XMLLoaded);
		}
	}

	private void XMLLoaded(bool inSucess)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (inSucess)
		{
			if (ScientificExperiment.GetActiveExperiment() != null)
			{
				LoadLab();
			}
			else
			{
				GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _NoActiveText.GetLocalizedString(), _NotificationTitleText.GetLocalizedString(), base.gameObject, "Reset");
			}
		}
		else
		{
			Reset();
		}
	}

	private void LoadLab()
	{
		Reset();
		base.OnTriggerEnter(AvAvatar.pObject.GetComponent<Collider>());
	}

	private void Reset()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
	}
}
