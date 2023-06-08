using System.Text.RegularExpressions;
using UnityEngine;

public class UiChatParentRequest : KAUI
{
	public bool _EnterEmail;

	public LocaleString _ValidEmailErrorText = new LocaleString("Enter a valid email address.");

	public LocaleString _EmailMatchErrorText = new LocaleString("Email address doesn't match.");

	public LocaleString _EmailAcceptedText = new LocaleString("Email sent.");

	public LocaleString _EmailAlreadySendText = new LocaleString("An email was already sent.");

	public LocaleString _EmailInvalidText = new LocaleString("That is not a valid email address.");

	public LocaleString _ServerErrorText = new LocaleString("Server error.");

	private KAWidget mCloseBtn;

	private KAWidget mSendBtn;

	private KAWidget mYesBtn;

	private KAWidget mNoBtn;

	private KAEditBox mEditParentEmail;

	private KAWidget mTxtParentError;

	private KAEditBox mEditConfirmEmail;

	private KAWidget mTxtVerifyError;

	private KAWidget mConfirmEmailBkg;

	private KAUIGenericDB mKAUIGenericDB;

	private string mAssetURL = "";

	protected override void Start()
	{
		base.Start();
		mCloseBtn = FindItem("CloseBtn");
		mSendBtn = FindItem("SendBtn");
		mYesBtn = FindItem("YesBtn");
		mNoBtn = FindItem("NoBtn");
		mEditParentEmail = (KAEditBox)FindItem("TxtParentEmail");
		mTxtParentError = FindItem("TxtParentError");
		mEditConfirmEmail = (KAEditBox)FindItem("TxtConfirmEmail");
		mTxtVerifyError = FindItem("TxtVerifyError");
		mConfirmEmailBkg = FindItem("ConfirmEmailBkg");
		if (UtPlatform.IsiOS() && mYesBtn != null && mNoBtn != null)
		{
			Vector3 localPosition = mNoBtn.transform.localPosition;
			mNoBtn.transform.localPosition = mYesBtn.transform.localPosition;
			mYesBtn.transform.localPosition = localPosition;
		}
		if (_EnterEmail)
		{
			OnClick(mEditParentEmail);
		}
		KAUI.SetExclusive(this);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mCloseBtn || inWidget == mNoBtn)
		{
			Close();
		}
		else if (inWidget == mSendBtn || inWidget == mYesBtn)
		{
			SetInteractive(interactive: false);
			KAUICursorManager.SetDefaultCursor("Loading");
			WsWebService.ChatAuthorizationRequest(mEditParentEmail.GetText().Trim(), EventHandler, null);
		}
	}

	public void Close()
	{
		KAUI.RemoveExclusive(this);
		Object.Destroy(base.gameObject);
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
	}

	protected override void Update()
	{
		base.Update();
		if (mEditParentEmail.HasFocus())
		{
			string text = mEditParentEmail.GetText().Trim();
			if (text.Length > 0)
			{
				if (CheckForValidEmail())
				{
					mTxtParentError.SetText("");
					SetConfirmEnabled(enable: true);
					if (text == mEditConfirmEmail.GetText().Trim())
					{
						mTxtVerifyError.SetText("");
						return;
					}
					mTxtVerifyError.SetTextByID(_EmailMatchErrorText._ID, _EmailMatchErrorText._Text);
					mSendBtn.SetState(KAUIState.NOT_INTERACTIVE);
				}
				else
				{
					mTxtParentError.SetTextByID(_ValidEmailErrorText._ID, _ValidEmailErrorText._Text);
					SetConfirmEnabled(enable: false);
				}
			}
			else
			{
				mTxtParentError.SetText("");
				SetConfirmEnabled(enable: false);
			}
		}
		else
		{
			if (!mEditConfirmEmail.HasFocus())
			{
				return;
			}
			string text2 = mEditParentEmail.GetText().Trim();
			if (text2.Length > 0)
			{
				if (text2 == mEditConfirmEmail.GetText().Trim())
				{
					mTxtVerifyError.SetText("");
					mSendBtn.SetState(KAUIState.INTERACTIVE);
				}
				else
				{
					mTxtVerifyError.SetTextByID(_EmailMatchErrorText._ID, _EmailMatchErrorText._Text);
					mSendBtn.SetState(KAUIState.NOT_INTERACTIVE);
				}
			}
			else
			{
				mTxtVerifyError.SetText("");
				mSendBtn.SetState(KAUIState.NOT_INTERACTIVE);
			}
		}
	}

	private void EventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			switch ((RequestChatAuthorizationResponse)inObject)
			{
			case RequestChatAuthorizationResponse.ACCEPTED:
				ShowKAUIDialog("PfKAUIGenericDBSm", "Email accepted", _EmailAcceptedText, close: true);
				break;
			case RequestChatAuthorizationResponse.ALREADY_EXISTS:
				ShowKAUIDialog("PfKAUIGenericDBSm", "Email already sent", _EmailAlreadySendText, close: true);
				break;
			case RequestChatAuthorizationResponse.INVALID_EMAIL_ADDRESS:
				ShowKAUIDialog("PfKAUIGenericDBSm", "Invalid email", _EmailInvalidText, close: false);
				break;
			default:
				ShowKAUIDialog("PfKAUIGenericDBSm", "Server error", _ServerErrorText, close: false);
				break;
			}
			break;
		case WsServiceEvent.ERROR:
			ShowKAUIDialog("PfKAUIGenericDBSm", "Server error", _ServerErrorText, close: false);
			break;
		}
	}

	private bool CheckForValidEmail()
	{
		string input = mEditParentEmail.GetText().Trim();
		return new Regex("\\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,4}\\b", RegexOptions.IgnoreCase).IsMatch(input);
	}

	private void SetConfirmEnabled(bool enable)
	{
		mTxtVerifyError.SetVisibility(enable);
		mEditConfirmEmail.SetDisabled(!enable);
		mConfirmEmailBkg.SetDisabled(!enable);
		mSendBtn.SetDisabled(!enable);
	}

	private void ShowKAUIDialog(string assetName, string dbName, LocaleString message, bool close)
	{
		if (mKAUIGenericDB != null)
		{
			DestroyKAUIDB();
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
		SetInteractive(interactive: true);
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB(assetName, dbName);
		mKAUIGenericDB._MessageObject = base.gameObject;
		if (close)
		{
			mKAUIGenericDB._OKMessage = "OnClose";
		}
		else
		{
			mKAUIGenericDB._OKMessage = "DestroyKAUIDB";
		}
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mKAUIGenericDB.SetTextByID(message._ID, message._Text, interactive: false);
		KAUI.SetExclusive(mKAUIGenericDB);
	}

	public void DestroyKAUIDB()
	{
		if (!(mKAUIGenericDB == null))
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			Object.Destroy(mKAUIGenericDB.gameObject);
			KAUI.SetExclusive(this);
		}
	}

	public void OnClose()
	{
		DestroyKAUIDB();
		Close();
	}

	private void SetURL(string inURL)
	{
		mAssetURL = inURL;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		RsResourceManager.Unload(mAssetURL);
	}
}
