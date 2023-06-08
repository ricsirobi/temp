using UnityEngine;

public class ObTriggerDragons : ObTrigger
{
	public bool _NeedPet = true;

	public int _MinPetAge = 3;

	public LocaleString _NoPetText = new LocaleString("You need a adult dragon to go in flight school.");

	private void ShowMessage()
	{
		KAUIGenericDB kAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
		kAUIGenericDB.SetText(_NoPetText.GetLocalizedString(), interactive: false);
		kAUIGenericDB._MessageObject = base.gameObject;
		kAUIGenericDB._OKMessage = "DestroyMessageDB";
		kAUIGenericDB.SetDestroyOnClick(isDestroy: true);
		kAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		KAUI.SetExclusive(kAUIGenericDB, new Color(0.5f, 0.5f, 0.5f, 0.5f));
	}

	public override void DoTriggerAction(GameObject other)
	{
		if (_NeedPet)
		{
			if (null == SanctuaryManager.pCurPetInstance)
			{
				ShowMessage();
				return;
			}
			if (SanctuaryManager.pCurPetInstance.pAge < _MinPetAge)
			{
				ShowMessage();
				return;
			}
		}
		base.DoTriggerAction(other);
	}
}
