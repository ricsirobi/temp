public class UiHotkeys : KAUI
{
	public LocaleString TitleText = new LocaleString("HotKeys");

	public override void OnClick(KAWidget inWidget)
	{
		if (inWidget.name.Equals("BtnClose"))
		{
			SetVisibility(inVisible: false);
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (!inVisible)
		{
			KAUI.RemoveExclusive(this);
		}
	}
}
