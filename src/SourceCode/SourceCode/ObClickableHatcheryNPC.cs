using UnityEngine;

public class ObClickableHatcheryNPC : ObClickable
{
	public LocaleString _DragonInfoText = new LocaleString("");

	public SnSound _DragonInfoVO;

	private KAUIGenericDB mGenericDB;

	public override void OnActivate()
	{
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		mGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "InfoData");
		mGenericDB.SetText(_DragonInfoText.GetLocalizedString(), interactive: false);
		mGenericDB._OKMessage = "OkMessage";
		mGenericDB._MessageObject = base.gameObject;
		mGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		KAUI.SetExclusive(mGenericDB);
		if (_DragonInfoVO != null)
		{
			_DragonInfoVO.Play();
		}
	}

	private void OkMessage()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		Object.Destroy(mGenericDB.gameObject);
	}
}
