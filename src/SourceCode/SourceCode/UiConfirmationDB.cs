using UnityEngine;

public class UiConfirmationDB : KAUIGenericDB
{
	public LocaleString _SecurityWordText = new LocaleString("Type \"{SECURITY_WORD}\" here.");

	private KAWidget mSecurityHeadingTxt;

	private KAEditBox mSecurityEditBox;

	private string mSecurityWord;

	private string mErrorTitle;

	private string mErrorMessage;

	private KAUIGenericDB mGenericDB;

	public void SetSecurityText(string securityHeading, string securityWord)
	{
		mSecurityHeadingTxt = FindItem("TxtSecurityHeading");
		mSecurityEditBox = (KAEditBox)FindItem("TxtSecurityWord");
		EventDelegate.Add(mSecurityEditBox.pInput.onChange, ValidateInputAgainstSecurityWord);
		ValidateInputAgainstSecurityWord();
		mSecurityHeadingTxt.SetText(securityHeading);
		mSecurityWord = securityWord;
		mSecurityEditBox.SetText(_SecurityWordText.GetLocalizedString().Replace("{SECURITY_WORD}", mSecurityWord));
	}

	public void SetErrorText(string errorTitle, string errorMessage)
	{
		mErrorTitle = errorTitle;
		mErrorMessage = errorMessage;
	}

	public void ValidateInputAgainstSecurityWord()
	{
		if (mSecurityEditBox.pInput.value == mSecurityWord)
		{
			SetButtonDisabled(inDisable: false, inYesBtn: true, inNoBtn: false, inOKBtn: false, inCloseBtn: false);
		}
		else
		{
			SetButtonDisabled(inDisable: true, inYesBtn: true, inNoBtn: false, inOKBtn: false, inCloseBtn: false);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (inWidget.name == "YesBtn" && _YesMessage.Length > 0)
		{
			if (!string.IsNullOrEmpty(mSecurityWord))
			{
				string text = mSecurityEditBox.GetText();
				if (mSecurityWord.Equals(text))
				{
					if (_MessageObject != null)
					{
						_MessageObject.SendMessage(_YesMessage, null, SendMessageOptions.DontRequireReceiver);
					}
					Object.Destroy(base.gameObject);
					return;
				}
			}
			ShowErrorDialog();
		}
		else
		{
			base.OnClick(inWidget);
		}
	}

	private void OKClicked()
	{
		if (mGenericDB != null)
		{
			KAUI.RemoveExclusive(mGenericDB);
			Object.Destroy(mGenericDB.gameObject);
		}
		SetVisibility(inVisible: true);
		KAUI.SetExclusive(this);
	}

	private void ShowErrorDialog()
	{
		KAUI.RemoveExclusive(this);
		SetVisibility(inVisible: false);
		mGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Error");
		mGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mGenericDB.SetText(mErrorMessage, interactive: false);
		mGenericDB.SetTitle(mErrorTitle);
		mGenericDB._MessageObject = base.gameObject;
		mGenericDB._OKMessage = "OKClicked";
		mGenericDB.SetDestroyOnClick(isDestroy: false);
		KAUI.SetExclusive(mGenericDB);
	}

	protected override void OnDestroy()
	{
		EventDelegate.Remove(mSecurityEditBox.pInput.onChange, ValidateInputAgainstSecurityWord);
		base.OnDestroy();
	}
}
