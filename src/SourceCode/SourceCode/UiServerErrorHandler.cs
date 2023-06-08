public class UiServerErrorHandler : UiErrorHandler
{
	public static AvAvatarState pPreviousState;

	private int mCurrentRetryCount;

	private bool mBootToLoginScreen;

	protected override void Awake()
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (UiErrorHandler.mInstance != null)
		{
			pOnUiErrorExitEventHandler?.Invoke();
			ExitDB();
			return;
		}
		base.Awake();
		UiErrorHandler.mInstance = this;
		if (RsResourceManager.pCurrentLevel == GameConfig.GetKeyData("LoginScene"))
		{
			UiLogin.pInstance?.SetVisibility(inVisible: true);
			RsResourceManager.DestroyLoadScreen();
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (!(inWidget.name != "OKBtn"))
		{
			if (mBootToLoginScreen)
			{
				ExitDB();
				mBootToLoginScreen = false;
				GameUtilities.LoadLoginLevel(showRegstration: false, fullReset: false);
			}
			else if (UiErrorHandler.GetHandleAction(base.pErrorMessageType) != Action.RECHECK_TOKEN)
			{
				base.OnClick(inWidget);
			}
			else
			{
				HandleError();
			}
		}
	}

	protected override void HandleError()
	{
		UiErrorHandler.pHandleErrors = false;
		switch (UiErrorHandler.GetHandleAction(base.pErrorMessageType))
		{
		case Action.PUSH_TO_LOGIN:
			if (RsResourceManager.pCurrentLevel != GameConfig.GetKeyData("LoginScene"))
			{
				KAUICursorManager.SetDefaultCursor("Arrow");
				GameUtilities.LoadLoginLevel(showRegstration: false, fullReset: false);
			}
			break;
		case Action.RECHECK_TOKEN:
			if (mCurrentRetryCount == _AllowedRetryAttempts)
			{
				mBootToLoginScreen = true;
				mCurrentRetryCount = 0;
				UiErrorHandler.pHandleErrors = true;
				SetButtonVisibility(visible: true);
				SetText(ErrorMessageType.NONE);
				WsTokenMonitor.StartTokenTimer();
				return;
			}
			mCurrentRetryCount++;
			KAUICursorManager.SetDefaultCursor("Loading");
			SetButtonVisibility(visible: false);
			SetText(string.Format(GetErrorText(ErrorMessageType.RECHECK_TOKEN), mCurrentRetryCount, _AllowedRetryAttempts));
			WsTokenMonitor.OnTokenStatus += OnTokenStatus;
			WsTokenMonitor.ForceCheckToken();
			break;
		}
		UiErrorHandler.pHandleErrors = true;
	}

	private void OnTokenStatus(ApiTokenStatus inStatus)
	{
		WsTokenMonitor.OnTokenStatus -= OnTokenStatus;
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (inStatus == ApiTokenStatus.TokenValid)
		{
			mCurrentRetryCount = 0;
			if ((bool)AvAvatar.pObject)
			{
				AvAvatar.pState = pPreviousState;
			}
			pPreviousState = AvAvatarState.NONE;
			WsTokenMonitor.StartTokenTimer();
			ExitDB();
		}
		else
		{
			HandleError();
		}
	}
}
