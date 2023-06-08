using UnityEngine;

public class UiNewFeaturesDB : KAUIGenericDB
{
	public Texture _Image;

	public LocaleString _MessageText = new LocaleString("");

	public LocaleString _MessageMobileText = new LocaleString("");

	public LocaleString _TitleText = new LocaleString("");

	protected override void Start()
	{
		base.Start();
		if (UtPlatform.IsMobile() && !string.IsNullOrEmpty(_MessageMobileText.GetLocalizedString()))
		{
			_MessageText = _MessageMobileText;
		}
		SetTitle(_TitleText.GetLocalizedString());
		SetText(_MessageText.GetLocalizedString(), interactive: false);
		SetDestroyOnClick(isDestroy: true);
		SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		KAUI.SetExclusive(this);
		KAWidget kAWidget = FindItem("Image");
		if (kAWidget != null)
		{
			kAWidget.SetTexture(_Image);
		}
	}
}
