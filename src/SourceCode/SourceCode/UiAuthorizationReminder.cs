using UnityEngine;

public class UiAuthorizationReminder : KAUI
{
	public LocaleString _ReminderText = new LocaleString("Your account has not been authorized yet. Enter an email address to send a reminder.");

	public LocaleString _InvalidEmailText = new LocaleString("Email address is not in valid format.");

	public LocaleString _ReminderEmailSuccessText = new LocaleString("An authorization reminder email has been sent.");

	public LocaleString _ReminderEmailFailText = new LocaleString("There was an error in sending a mail to the address you specified.");

	private KAEditBox mTxtEmail;

	private KAWidget mTxtInstruction;

	private bool mIsReset;

	private GameObject mCallbackObject;

	public GameObject pCallbackObject
	{
		set
		{
			mCallbackObject = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		mTxtEmail = (KAEditBox)FindItem("TxtEmail");
		mTxtInstruction = FindItem("TxtInstruction");
		if (mTxtInstruction != null)
		{
			mTxtInstruction.SetText(_ReminderText.GetLocalizedString());
		}
	}

	public override void OnClick(KAWidget inItem)
	{
		base.OnClick(inItem);
		if (inItem == mTxtEmail)
		{
			ResetText();
		}
		else if (inItem.name == "BtnSubmit")
		{
			string text = mTxtEmail.GetText();
			if (!mTxtEmail.IsValidText() || text == mTxtEmail._DefaultText.GetLocalizedString())
			{
				SetState(KAUIState.DISABLED);
				GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _InvalidEmailText.GetLocalizedString(), base.gameObject, "OnErrorDBClose");
			}
			else
			{
				KAUICursorManager.SetDefaultCursor("Loading");
				SetState(KAUIState.DISABLED);
				WsWebService.SendAccountActivationReminder(text, UiLogin.pPassword, ServiceEventHandler, null);
			}
		}
		else if (inItem.name == "BtnClose")
		{
			OnDBClose();
		}
	}

	public void OnEnable()
	{
		if (mTxtEmail != null)
		{
			mTxtEmail.SetText("Email");
			mIsReset = false;
		}
	}

	public void ResetText()
	{
		if (!mIsReset && !mTxtEmail.HasFocus())
		{
			mTxtEmail.SetText("");
			mIsReset = true;
		}
	}

	public void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType != WsServiceType.SEND_ACCOUNT_ACTIVATION_REMINDER)
		{
			return;
		}
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			KAUICursorManager.SetDefaultCursor("Arrow", showHideSystemCursor: false);
			if (inObject != null)
			{
				if ((ActivationReminderResponse)inObject == ActivationReminderResponse.ACTIVATION_REMINDER_SENT)
				{
					GameUtilities.DisplayOKMessage("PfKAUIGenericDBSm", _ReminderEmailSuccessText.GetLocalizedString(), base.gameObject, "OnDBClose");
				}
				else
				{
					GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _ReminderEmailFailText.GetLocalizedString(), base.gameObject, "OnErrorDBClose");
				}
			}
			break;
		case WsServiceEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow", showHideSystemCursor: false);
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _ReminderEmailFailText.GetLocalizedString(), base.gameObject, "OnErrorDBClose");
			break;
		}
	}

	public void OnErrorDBClose()
	{
		SetState(KAUIState.INTERACTIVE);
	}

	public void OnDBClose()
	{
		SetVisibility(inVisible: false);
		if (mCallbackObject != null)
		{
			mCallbackObject.SendMessage("OnDBClose");
		}
	}
}
