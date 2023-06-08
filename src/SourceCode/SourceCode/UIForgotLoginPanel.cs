public class UIForgotLoginPanel : KAUI
{
	public UiAccountRecover _PasswordRecoverUI;

	public UiAccountRecover _UsernameRecoverUI;

	public KAButton _ForgotPasswordBtn;

	public KAButton _ForgotUsernameBtn;

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == _ForgotPasswordBtn)
		{
			_PasswordRecoverUI?.SetVisibility(inVisible: true);
			SetVisibility(inVisible: false);
		}
		else if (inWidget == _ForgotUsernameBtn)
		{
			_UsernameRecoverUI?.SetVisibility(inVisible: true);
			SetVisibility(inVisible: false);
		}
		else if (inWidget.name == _BackButtonName)
		{
			OnClose();
		}
	}

	private void OnClose()
	{
		UiLogin.pInstance?.SetInteractive(interactive: true);
		SetVisibility(inVisible: false);
	}
}
