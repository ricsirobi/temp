public class UiAvatarName : KAUI
{
	public delegate void OnPressedOK(bool nameChanged);

	public LocaleString _AvatarNameSame = new LocaleString("You entered the same name.  Please choose something different.");

	public LocaleString _AvatarNameInvalid = new LocaleString("Name is invalid, please try some other names.");

	public LocaleString _GeneralError = new LocaleString("Sorry, unable to update Avatar name. Please try again later.");

	public LocaleString _ChangeSuccessfullText = new LocaleString("Your Viking's name has been changed.");

	private int mChangeVikingNameTicketID;

	private int mChangeVikingNameStoreID;

	private bool mEnablePurchase;

	private KAEditBox mEditLabel;

	private KAWidget mOKBtn;

	private KAWidget mCloseBtn;

	private bool mAvatarNameEntered;

	private string mAvatarName;

	private string mCachedName;

	private string mSuggestedName = "";

	private KAUIGenericDB mUiGenericDB;

	public bool _AllowNameSuggestions;

	public event OnPressedOK OnNameChanged;

	protected override void Start()
	{
		base.Start();
		mEditLabel = (KAEditBox)FindItem("TxtEditName");
		mOKBtn = FindItem("BtnOk");
		mCloseBtn = FindItem("BtnClose");
		KAUI.SetExclusive(this);
		if (mCloseBtn != null)
		{
			mCloseBtn.SetVisibility(mEnablePurchase);
		}
	}

	public override void OnClick(KAWidget inItem)
	{
		base.OnClick(inItem);
		if (inItem == mEditLabel)
		{
			if (!mAvatarNameEntered)
			{
				mAvatarNameEntered = true;
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
		else
		{
			if (!(inItem == mOKBtn))
			{
				return;
			}
			mAvatarName = mEditLabel.GetText().Trim();
			if (string.IsNullOrEmpty(mAvatarName) || !mAvatarNameEntered || mAvatarName == mEditLabel._DefaultText.GetLocalizedString())
			{
				ShowMessage(_AvatarNameInvalid.GetLocalizedString(), isSomeOtherError: false);
				return;
			}
			if (mEnablePurchase && mAvatarName == AvatarData.pInstance.DisplayName)
			{
				ShowMessage(_AvatarNameSame.GetLocalizedString(), isSomeOtherError: false);
				return;
			}
			mCachedName = AvatarData.pInstance.DisplayName;
			AvatarData.pInstance.DisplayName = mAvatarName;
			AvatarData.pInstance.IsSuggestedAvatarName = mAvatarName.Equals(mSuggestedName);
			if (mEnablePurchase)
			{
				WsWebService.SetDisplayName(new SetDisplayNameRequest
				{
					DisplayName = mAvatarName,
					ItemID = mChangeVikingNameTicketID,
					StoreID = mChangeVikingNameStoreID
				}, ServiceEventHandler, null);
			}
			else
			{
				WsWebService.SetAvatar(AvatarData.pInstance, ServiceEventHandler, null);
			}
			KAUICursorManager.SetDefaultCursor("Loading");
			SetState(KAUIState.NOT_INTERACTIVE);
		}
	}

	public void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			if (inObject != null)
			{
				SetAvatarResult setAvatarResult = (SetAvatarResult)inObject;
				if (setAvatarResult.Success)
				{
					KAUICursorManager.SetDefaultCursor("Arrow", showHideSystemCursor: false);
					if (AvAvatar.pToolbar != null)
					{
						UiToolbar component = AvAvatar.pToolbar.GetComponent<UiToolbar>();
						if (component != null)
						{
							component.DisplayName();
						}
					}
					if (mEnablePurchase)
					{
						mEnablePurchase = false;
						Money.UpdateMoneyFromServer();
					}
					AvatarData.SetDisplayName(mAvatarName);
					UserInfo.pInstance.Username = mAvatarName;
					ShowSuccessMessage();
					break;
				}
				AvatarData.pInstance.DisplayName = mCachedName;
				if (setAvatarResult.StatusCode == AvatarValidationResult.AvatarDisplayNameInvalid)
				{
					SuggestionResult suggestionResult = null;
					if (setAvatarResult.Suggestions != null && setAvatarResult.Suggestions.Suggestion != null)
					{
						suggestionResult = setAvatarResult.Suggestions;
					}
					if (_AllowNameSuggestions && suggestionResult != null && suggestionResult.Suggestion.Length != 0)
					{
						ShowSuggestedNames(suggestionResult);
					}
					else
					{
						ShowMessage(_AvatarNameInvalid.GetLocalizedString(), isSomeOtherError: false);
					}
				}
				else
				{
					ShowMessage(_GeneralError.GetLocalizedString(), isSomeOtherError: true);
				}
			}
			else
			{
				UtDebug.LogError("ERROR: Failed to change avatar display name.  inObject is null!");
				AvatarData.pInstance.DisplayName = mCachedName;
				ShowMessage(_GeneralError.GetLocalizedString(), isSomeOtherError: true);
				mEnablePurchase = false;
			}
			break;
		case WsServiceEvent.ERROR:
			AvatarData.pInstance.DisplayName = mCachedName;
			UtDebug.LogError("ERROR: Failed to change avatar display name. WsServiceEvent returned ERROR");
			ShowMessage(_GeneralError.GetLocalizedString(), isSomeOtherError: true);
			mEnablePurchase = false;
			break;
		}
	}

	private void OnSuggestedNameSelected(string name)
	{
		SetVisibility(inVisible: true);
		SetState(KAUIState.INTERACTIVE);
		if (!string.IsNullOrEmpty(name))
		{
			mSuggestedName = name;
			mEditLabel.SetText(name);
		}
	}

	private void ShowSuggestedNames(SuggestionResult result)
	{
		SetVisibility(inVisible: false);
		UiNameSuggestion.Init(result, OnSuggestedNameSelected);
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

	private void ShowMessage(string msg, bool isSomeOtherError)
	{
		KAUICursorManager.SetDefaultCursor("Arrow", showHideSystemCursor: false);
		SetVisibility(inVisible: false);
		mUiGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Error");
		mUiGenericDB._MessageObject = base.gameObject;
		if (!isSomeOtherError)
		{
			mUiGenericDB._OKMessage = "OnAvatarNameErrorOkClick";
		}
		else
		{
			mUiGenericDB._OKMessage = "OnOtherErrorOkClick";
		}
		mUiGenericDB.SetText(msg, interactive: false);
		mUiGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		KAUI.SetExclusive(mUiGenericDB);
	}

	private void OnAvatarNameErrorOkClick()
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

	private void OnOtherErrorOkClick()
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
			this.OnNameChanged(nameChanged: false);
		}
	}

	public void UpdatePurchaseDetails(int ticketID, int storeID)
	{
		mChangeVikingNameTicketID = ticketID;
		mChangeVikingNameStoreID = storeID;
		mEnablePurchase = true;
	}
}
