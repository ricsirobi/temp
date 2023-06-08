public class UiFarmName : KAUI
{
	public delegate void OnPressedOK(bool nameChanged);

	public int _MinNameTextLength = 3;

	public LocaleString _FarmNameSameText = new LocaleString("You entered the same name.  Please choose something different.");

	public LocaleString _FarmNameInvalidText = new LocaleString("Name is invalid, please try some other names.");

	public LocaleString _NameNotLongEnoughText = new LocaleString("The Farm name must be at least 3 characters.");

	public LocaleString _GeneralErrorText = new LocaleString("Sorry, unable to update Farm name. Please try again later.");

	public LocaleString _ChangeSuccessfullText = new LocaleString("Your Farm name has been changed.");

	private KAEditBox mEditLabel;

	private KAWidget mOKBtn;

	private KAWidget mCloseBtn;

	private bool mFarmNameEntered;

	private string mFarmName;

	private KAUIGenericDB mUiGenericDB;

	public event OnPressedOK OnNameChanged;

	protected override void Start()
	{
		base.Start();
		mEditLabel = (KAEditBox)FindItem("TxtEditName");
		mOKBtn = FindItem("BtnOk");
		mCloseBtn = FindItem("BtnClose");
		KAUI.SetExclusive(this);
	}

	public override void OnClick(KAWidget inItem)
	{
		base.OnClick(inItem);
		if (inItem == mEditLabel)
		{
			if (!mFarmNameEntered)
			{
				mFarmNameEntered = true;
			}
		}
		else if (inItem == mCloseBtn)
		{
			KAUI.RemoveExclusive(this);
			if (this.OnNameChanged != null)
			{
				this.OnNameChanged(nameChanged: false);
			}
		}
		else if (inItem == mOKBtn)
		{
			mFarmName = mEditLabel.GetText().Trim();
			if (mFarmName.Length < _MinNameTextLength)
			{
				ShowMessage(_NameNotLongEnoughText.GetLocalizedString());
				return;
			}
			if (string.IsNullOrEmpty(mFarmName) || !mFarmNameEntered || mFarmName == mEditLabel._DefaultText.GetLocalizedString())
			{
				ShowMessage(_FarmNameInvalidText.GetLocalizedString());
				return;
			}
			if (mFarmName == AvatarData.pInstance.DisplayName)
			{
				ShowMessage(_FarmNameSameText.GetLocalizedString());
				return;
			}
			KAUICursorManager.SetDefaultCursor("Loading");
			SetState(KAUIState.NOT_INTERACTIVE);
			FarmManager.pCurrentFarmData.SetRoomName(mFarmName);
			FarmManager.pCurrentFarmData.Save(ServiceEventHandler);
		}
	}

	public void ServiceEventHandler(bool success, UserRoomSetResponse responce)
	{
		if (success)
		{
			ShowSuccessMessage();
		}
		else if (responce == null || responce.StatusCode != UserRoomValidationResult.RMFValidationFailed)
		{
			ShowMessage(_GeneralErrorText.GetLocalizedString());
		}
		else
		{
			ShowMessage(_FarmNameInvalidText.GetLocalizedString());
		}
	}

	private void ShowSuccessMessage()
	{
		KAUICursorManager.SetDefaultCursor("Arrow", showHideSystemCursor: false);
		SetVisibility(inVisible: false);
		mUiGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Error");
		mUiGenericDB._MessageObject = base.gameObject;
		mUiGenericDB._OKMessage = "OnCloseSuccessDB";
		mUiGenericDB.SetText(_ChangeSuccessfullText.GetLocalizedString(), interactive: false);
		mUiGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		KAUI.SetExclusive(mUiGenericDB);
	}

	private void OnCloseSuccessDB()
	{
		KAUI.RemoveExclusive(mUiGenericDB);
		if (mUiGenericDB != null)
		{
			mUiGenericDB.Destroy();
		}
		mUiGenericDB = null;
		KAUI.RemoveExclusive(this);
		if (this.OnNameChanged != null)
		{
			this.OnNameChanged(nameChanged: true);
		}
	}

	private void ShowMessage(string msg)
	{
		KAUICursorManager.SetDefaultCursor("Arrow", showHideSystemCursor: false);
		SetVisibility(inVisible: false);
		mUiGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Error");
		mUiGenericDB._MessageObject = base.gameObject;
		mUiGenericDB._OKMessage = "OnErrorOkClick";
		mUiGenericDB.SetText(msg, interactive: false);
		mUiGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		KAUI.SetExclusive(mUiGenericDB);
	}

	private void OnErrorOkClick()
	{
		KAUI.RemoveExclusive(mUiGenericDB);
		if (mUiGenericDB != null)
		{
			mUiGenericDB.Destroy();
		}
		mUiGenericDB = null;
		SetState(KAUIState.INTERACTIVE);
		SetVisibility(inVisible: true);
	}
}
