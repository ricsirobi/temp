public class UiAccountRecover : KAUI
{
	public enum Mode
	{
		Account,
		Password
	}

	public int _MaxLengthEmail = 30;

	public Mode _Mode;

	public UIForgotLoginPanel _ForgotLoginPanel;

	private KAEditBox mTxtEmail;

	private KAEditBox mTxtUsername;

	private MembershipUserStatus mResult = MembershipUserStatus.ProviderError;

	protected override void Start()
	{
		base.Start();
		switch (_Mode)
		{
		case Mode.Account:
			mTxtEmail = (KAEditBox)FindItem("TxtEmail");
			mTxtEmail.pInput.characterLimit = _MaxLengthEmail;
			break;
		case Mode.Password:
			mTxtUsername = (KAEditBox)FindItem("TxtUsername");
			mTxtEmail = ((KAEditBox)FindItem("TxtEmail")) ?? null;
			if ((bool)mTxtEmail)
			{
				mTxtEmail.pInput.characterLimit = _MaxLengthEmail;
			}
			break;
		}
	}

	public override void OnClick(KAWidget inItem)
	{
		base.OnClick(inItem);
		string text = inItem.name;
		if (!(text == "BtnSubmit"))
		{
			if (text == "BtnClose")
			{
				CloseDB();
			}
		}
		else
		{
			if (!UiLogin.pInstance.CheckForConnection(ConnectivityErrorLocation.LOGIN_SCENE))
			{
				return;
			}
			SetState(KAUIState.DISABLED);
			switch (_Mode)
			{
			case Mode.Account:
				if (string.IsNullOrEmpty(mTxtEmail.GetText()) || !mTxtEmail.IsValidText() || mTxtEmail.GetText() == mTxtEmail._DefaultText.GetLocalizedString())
				{
					UiLogin.ShowDB(UiLogin.GetStrings()._InvalidEmailText.GetLocalizedString(), base.gameObject);
					break;
				}
				KAUICursorManager.SetDefaultCursor("Loading");
				WsWebService.RecoverAccount(mTxtEmail.GetText().Trim(), ServiceEventHandler, null);
				break;
			case Mode.Password:
				if (string.IsNullOrEmpty(mTxtEmail.GetText()) || !mTxtEmail.IsValidText() || mTxtEmail.GetText() == mTxtEmail._DefaultText.GetLocalizedString())
				{
					UiLogin.ShowDB(UiLogin.GetStrings()._InvalidEmailText.GetLocalizedString(), base.gameObject);
					break;
				}
				if (string.IsNullOrEmpty(mTxtUsername.GetText()) || mTxtUsername.GetText() == mTxtUsername._DefaultText.GetLocalizedString())
				{
					UiLogin.ShowDB(UiLogin.GetStrings()._InvalidPlayerNameText.GetLocalizedString(), base.gameObject);
					break;
				}
				KAUICursorManager.SetDefaultCursor("Loading");
				WsWebService.ResetPassword(mTxtEmail.GetText().Trim(), mTxtUsername.GetText(), ServiceEventHandler, null);
				break;
			}
		}
	}

	public void OnBecameVisible()
	{
		if (mTxtEmail != null)
		{
			mTxtEmail.SetText(mTxtEmail._DefaultText.GetLocalizedString());
		}
		if (mTxtUsername != null)
		{
			mTxtUsername.SetText(mTxtUsername._DefaultText.GetLocalizedString());
		}
	}

	public void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			KAUICursorManager.SetDefaultCursor("Arrow", showHideSystemCursor: false);
			if (inObject == null)
			{
				break;
			}
			mResult = (MembershipUserStatus)inObject;
			switch (mResult)
			{
			case MembershipUserStatus.Success:
				if (_Mode == Mode.Account)
				{
					UiLogin.ShowDB(UiLogin.GetStrings()._AccountRecoverSuccessText.GetLocalizedString(), base.gameObject);
				}
				else
				{
					UiLogin.ShowDB(UiLogin.GetStrings()._PasswordRecoverSuccessText.GetLocalizedString(), base.gameObject);
				}
				break;
			case MembershipUserStatus.InvalidUserName:
				UiLogin.ShowDB(UiLogin.GetStrings()._InvalidUsernameText.GetLocalizedString(), base.gameObject);
				break;
			case MembershipUserStatus.InvalidEmail:
				UiLogin.ShowDB(UiLogin.GetStrings()._InvalidEmailText.GetLocalizedString(), base.gameObject);
				break;
			default:
				if (_Mode == Mode.Account)
				{
					UiLogin.ShowDB(UiLogin.GetStrings()._AccountRecoverFailedText.GetLocalizedString(), base.gameObject);
				}
				else
				{
					UiLogin.ShowDB(UiLogin.GetStrings()._PasswordRecoverFailedText.GetLocalizedString(), base.gameObject);
				}
				break;
			}
			break;
		case WsServiceEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow", showHideSystemCursor: false);
			UiLogin.ShowDB(UiLogin.GetStrings()._ErrorText.GetLocalizedString(), base.gameObject);
			break;
		}
	}

	public void OnDBClose()
	{
		if (mResult == MembershipUserStatus.Success)
		{
			SetVisibility(inVisible: false);
			UiLogin.pInstance.OnDBClose();
			SetState(KAUIState.INTERACTIVE);
		}
		else
		{
			SetState(KAUIState.INTERACTIVE);
		}
	}

	public void CloseDB()
	{
		mTxtEmail?.SetText("");
		mTxtUsername?.SetText("");
		_ForgotLoginPanel?.SetVisibility(inVisible: true);
		SetVisibility(inVisible: false);
	}
}
