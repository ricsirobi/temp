public class UiPrizeCodeDB : KAUIGenericDB
{
	public KAWidget _Image;

	internal void Show(string Msg, string ImageName, PrizeCodeMessage MessageObj)
	{
		SetText(Msg, interactive: true);
		SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		string[] array = ImageName.Split('/');
		_Image.SetTextureFromBundle(array[0] + "/" + array[1], array[2]);
		KAUI.SetExclusive(this, WsUserMessage.pInstance._MaskColor);
	}

	public void OnClickOk()
	{
		WsUserMessage.pInstance.OnClose();
	}

	protected override void OnDestroy()
	{
		KAUI.RemoveExclusive(this);
	}
}
