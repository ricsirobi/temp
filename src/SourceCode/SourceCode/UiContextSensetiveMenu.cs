public class UiContextSensetiveMenu : KAUIMenu
{
	private UiContextSensitive mContextSensitiveUI;

	public UiContextSensitive pContextSensitiveUI
	{
		set
		{
			mContextSensitiveUI = value;
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (mContextSensitiveUI != null)
		{
			mContextSensitiveUI.OnClick(inWidget);
		}
	}
}
