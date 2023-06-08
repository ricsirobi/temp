using System;
using System.Collections.Generic;
using UnityEngine;

public class UiSelectName : KAUI
{
	public enum Status
	{
		Loaded,
		Accepted,
		Closed
	}

	public enum FailStatus
	{
		None,
		Same,
		Taken,
		Invalid,
		ServerError,
		ValidationFailed,
		GetNameSuggestionFail
	}

	public delegate void Callback(Status status, string selectedName, bool suggestedNameSelected, UiSelectName uiSelectName);

	public LocaleString _CheckingNameText = new LocaleString("Checking Name...");

	public LocaleString _ConfirmationText = new LocaleString("Are you sure you want this to be your Viking?");

	public LocaleString _ConfirmationTitleText = new LocaleString("Confirm Viking");

	public LocaleString _DuplicateNameText = new LocaleString("This username is already taken. Please try another.");

	public LocaleString _EnterValidNameText = new LocaleString("Please enter a valid name");

	public LocaleString _ValidationFailedText = new LocaleString("Sorry, unable to validate name. Please try again later.");

	public LocaleString _GeneralErrorText = new LocaleString("Sorry, unable to update Avatar name. Please try again later.");

	public LocaleString _ServerErrorText = new LocaleString("Sorry, there was a problem while communicating with the server. Please try again later.");

	public LocaleString _ChangeSuccessfullText = new LocaleString("Your Viking's name has been changed.");

	public LocaleString _AvatarNameSameText = new LocaleString("You entered the same name.  Please choose something different.");

	public LocaleString _NameSuggestionFailedText = new LocaleString("Sorry, unable to provide name suggestions.");

	private KAUIGenericDB mGenericDB;

	private KAWidget mBtnDone;

	private KAWidget mBtnBack;

	private KAWidget mTxtStatus;

	private KAUIMenu mMenu;

	private KAEditBox mTxtName;

	private string mSelectedName;

	private string[] mSuggestedNames;

	private FailStatus mFailStatus;

	private int mChangeVikingNameTicketID;

	private int mChangeVikingNameStoreID;

	private bool mEnablePurchase;

	private bool mHideBackButton;

	private string mCachedName;

	private Callback mCallBack;

	private DateTime mChooseNameStartTime;

	private int mNameValidationFailCount;

	public bool Independent { get; set; }

	public static void Init(Callback callback, string name, string[] suggestedNames, FailStatus failStatus = FailStatus.None, bool independent = false)
	{
		List<object> list = new List<object>();
		list.Add(name);
		list.Add(suggestedNames);
		list.Add(callback);
		list.Add(failStatus);
		list.Add(independent);
		KAUICursorManager.SetDefaultCursor("Loading");
		RsResourceManager.LoadAssetFromBundle(GameConfig.GetKeyData("SelectNameAsset"), OnBundleReady, typeof(GameObject), inDontDestroy: false, list);
	}

	private static void OnBundleReady(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
			UiSelectName uiSelectName = null;
			if (gameObject != null)
			{
				uiSelectName = gameObject.GetComponent<UiSelectName>();
			}
			if (uiSelectName != null && inUserData != null)
			{
				List<object> list = (List<object>)inUserData;
				uiSelectName.SetNames(list[0] as string, list[1] as string[]);
				uiSelectName.SetCallback(list[2] as Callback);
				uiSelectName.SetFailStatus((FailStatus)list[3]);
				uiSelectName.Independent = (bool)list[4];
				((Callback)list[2])?.Invoke(Status.Loaded, list[0] as string, suggestedNameSelected: false, uiSelectName);
			}
			AnalyticAgent.LogFTUEEvent(FTUEEvent.CHARACTER_NAME_STARTED);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			if (inUserData != null)
			{
				((Callback)((List<object>)inUserData)[2])?.Invoke(Status.Closed, null, suggestedNameSelected: false, null);
			}
			break;
		}
	}

	public void SetNames(string name, string[] suggestedNames)
	{
		mSelectedName = name;
		mSuggestedNames = suggestedNames;
	}

	public void HideBackButton(bool hide)
	{
		if (mBtnBack != null)
		{
			mBtnBack.SetVisibility(!hide);
		}
		mHideBackButton = hide;
	}

	public void SetFailStatus(FailStatus status)
	{
		mFailStatus = status;
	}

	public void SetCallback(Callback callback)
	{
		mCallBack = callback;
	}

	public void DisplayNames()
	{
		if (mMenu != null)
		{
			mMenu.ClearItems();
			if (mSuggestedNames != null)
			{
				string[] array = mSuggestedNames;
				foreach (string text in array)
				{
					ShowInMenu(text);
				}
			}
		}
		if (mTxtName != null)
		{
			mTxtName.SetText(mSelectedName);
		}
	}

	private void ValidateName(string name)
	{
		NameValidationRequest nameValidationRequest = new NameValidationRequest
		{
			Category = NameCategory.Default,
			Name = name
		};
		ShowMessageDB(_CheckingNameText, null, "", "", processingDB: true);
		WsWebService.ValidateName(nameValidationRequest, NameValidationEventHandler, name);
	}

	private void NameValidationEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType != WsServiceType.VALIDATE_NAME)
		{
			return;
		}
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			DestroyMessageDB();
			NameValidationResponse nameValidationResponse = (NameValidationResponse)inObject;
			if (nameValidationResponse != null && string.IsNullOrEmpty(nameValidationResponse.ErrorMessage))
			{
				ShowMessageDB(_ConfirmationText, _ConfirmationTitleText, "OnConfirmation", "DestroyMessageDB", processingDB: false);
				break;
			}
			UtDebug.Log("Error! failed validating name");
			DisplayFailStatus(FailStatus.ValidationFailed);
			break;
		}
		case WsServiceEvent.ERROR:
			DestroyMessageDB();
			UtDebug.Log("Error! failed validating name");
			DisplayFailStatus(FailStatus.ValidationFailed);
			break;
		}
	}

	private void OnConfirmation()
	{
		DestroyMessageDB();
		LogFTUENameCompleteEvent();
		if (Independent)
		{
			SetName();
		}
		else
		{
			CloseUI(Status.Accepted);
		}
	}

	protected override void Start()
	{
		base.Start();
		mMenu = _MenuList[0];
		mTxtName = (KAEditBox)FindItem("TxtEditName");
		EventDelegate.Add(mTxtName.pInput.onChange, OnChange);
		mBtnBack = FindItem("BtnBack");
		mBtnDone = FindItem("BtnDone");
		mTxtStatus = FindItem("TxtStatus");
		HideBackButton(mHideBackButton);
		UpdateAcceptButton();
		DisplayFailStatus(mFailStatus);
		if (mSuggestedNames == null)
		{
			SetVisibility(inVisible: false);
			KAUICursorManager.SetDefaultCursor("Loading");
			WsWebService.GetDefaultNameSuggestion(ServiceEventHandler, null);
		}
		else
		{
			DisplayNames();
			KAUI.SetExclusive(this);
		}
	}

	private void OnChange()
	{
		EventDelegate.Remove(mTxtName.pInput.onChange, OnChange);
	}

	private void DisplayFailStatus(FailStatus failStatus)
	{
		mTxtStatus?.SetText(GetFailStatus(failStatus));
		if (failStatus != 0)
		{
			mNameValidationFailCount++;
		}
	}

	private string GetFailStatus(FailStatus status)
	{
		return status switch
		{
			FailStatus.Invalid => _EnterValidNameText.GetLocalizedString(), 
			FailStatus.Taken => _DuplicateNameText.GetLocalizedString(), 
			FailStatus.Same => _AvatarNameSameText.GetLocalizedString(), 
			FailStatus.ServerError => _ServerErrorText.GetLocalizedString(), 
			FailStatus.ValidationFailed => _ValidationFailedText.GetLocalizedString(), 
			FailStatus.GetNameSuggestionFail => _NameSuggestionFailedText.GetLocalizedString(), 
			_ => null, 
		};
	}

	private void ShowInMenu(string name)
	{
		KAWidget kAWidget = mMenu.AddWidget("SuggestedName");
		kAWidget.SetText(name);
		kAWidget.SetVisibility(inVisible: true);
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item.name == "SuggestedName")
		{
			mTxtName.SetText(item.GetText());
			UpdateAcceptButton();
		}
		else if (item.name == "BtnDone")
		{
			string text = mTxtName.GetText();
			if (AvatarData.pInstance.DisplayName != null && AvatarData.pInstance.DisplayName.Equals(text, StringComparison.OrdinalIgnoreCase))
			{
				DisplayFailStatus(FailStatus.Same);
			}
			else if (!SuggestedName(text))
			{
				ValidateName(mTxtName.GetText());
			}
			else if (Independent)
			{
				SetName();
			}
			else
			{
				CloseUI(Status.Accepted);
			}
		}
		else if (item.name == "BtnBack")
		{
			CloseUI(Status.Closed);
		}
	}

	public override void OnSelect(KAWidget widget, bool inSelected)
	{
		widget.OnSelect(inSelected);
		if (widget == mTxtName && widget.pParentWidget != null)
		{
			UpdateAcceptButton();
		}
	}

	public override void OnSubmit(KAWidget widget)
	{
		widget.OnSubmit();
		if (widget == mTxtName && widget.pParentWidget != null)
		{
			UpdateAcceptButton();
		}
	}

	public override void OnInput(KAWidget inWidget, string inText)
	{
		base.OnInput(inWidget, inText);
		if (inWidget == mTxtName)
		{
			UpdateAcceptButton();
		}
	}

	private void UpdateAcceptButton()
	{
		if (mBtnDone != null)
		{
			mBtnDone.SetDisabled(string.IsNullOrEmpty(mTxtName.GetText()));
		}
	}

	private void SetName()
	{
		string text = mTxtName.GetText();
		mCachedName = AvatarData.pInstance.DisplayName;
		AvatarData.pInstance.DisplayName = text;
		AvatarData.pInstance.IsSuggestedAvatarName = SuggestedName(text);
		KAUICursorManager.SetDefaultCursor("Loading");
		SetInteractive(interactive: false);
		if (mEnablePurchase)
		{
			WsWebService.SetDisplayName(new SetDisplayNameRequest
			{
				DisplayName = text,
				ItemID = mChangeVikingNameTicketID,
				StoreID = mChangeVikingNameStoreID
			}, ServiceEventHandler, text);
		}
		else
		{
			WsWebService.SetAvatar(AvatarData.pInstance, ServiceEventHandler, text);
		}
	}

	public void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.SET_AVATAR:
		case WsServiceType.SET_DISPLAY_NAME:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				KAUICursorManager.SetDefaultCursor("Arrow");
				SetInteractive(interactive: true);
				if (inObject != null)
				{
					SetAvatarResult setAvatarResult = (SetAvatarResult)inObject;
					if (setAvatarResult.Success)
					{
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
							Money.UpdateMoneyFromServer();
						}
						AvatarData.SetDisplayName((string)inUserData);
						UserInfo.pInstance.Username = (string)inUserData;
						SetVisibility(inVisible: false);
						GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _ChangeSuccessfullText.GetLocalizedString(), base.gameObject, "OnPressedOK");
					}
					else
					{
						AvatarData.pInstance.DisplayName = mCachedName;
						if (setAvatarResult.StatusCode == AvatarValidationResult.AvatarDisplayNameInvalid && setAvatarResult.Suggestions != null && setAvatarResult.Suggestions.Suggestion != null)
						{
							SetNames(mTxtName.GetText(), setAvatarResult.Suggestions.Suggestion);
							DisplayNames();
							DisplayFailStatus(FailStatus.Taken);
						}
						else
						{
							UtDebug.LogError("ERROR: Failed to change avatar display name. Status " + setAvatarResult.StatusCode);
							HideBackButton(hide: false);
							DisplayFailStatus(FailStatus.ServerError);
						}
					}
				}
				else
				{
					UtDebug.LogError("ERROR: Failed to change avatar display name.  inObject is null!");
					AvatarData.pInstance.DisplayName = mCachedName;
					DisplayFailStatus(FailStatus.ServerError);
					HideBackButton(hide: false);
				}
				break;
			case WsServiceEvent.ERROR:
				KAUICursorManager.SetDefaultCursor("Arrow");
				SetInteractive(interactive: true);
				AvatarData.pInstance.DisplayName = mCachedName;
				UtDebug.LogError("ERROR: Failed to change avatar display name. WsServiceEvent returned ERROR");
				HideBackButton(hide: false);
				DisplayFailStatus(FailStatus.ServerError);
				break;
			}
			break;
		case WsServiceType.GET_DEFAULT_NAME_SUGGESTION:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				KAUICursorManager.SetDefaultCursor("Arrow");
				SetVisibility(inVisible: true);
				KAUI.SetExclusive(this);
				if (inObject != null)
				{
					DisplayNameUniqueResponse displayNameUniqueResponse = (DisplayNameUniqueResponse)inObject;
					if (displayNameUniqueResponse.Suggestions != null)
					{
						SetNames(null, displayNameUniqueResponse.Suggestions.Suggestion);
						DisplayNames();
						break;
					}
				}
				DisplayFailStatus(FailStatus.GetNameSuggestionFail);
				break;
			case WsServiceEvent.ERROR:
				KAUICursorManager.SetDefaultCursor("Arrow");
				SetVisibility(inVisible: true);
				KAUI.SetExclusive(this);
				DisplayFailStatus(FailStatus.GetNameSuggestionFail);
				break;
			}
			break;
		}
	}

	private bool SuggestedName(string selectedName)
	{
		if (mSuggestedNames == null)
		{
			return false;
		}
		return Array.Find(mSuggestedNames, (string name) => name.Equals(selectedName, StringComparison.OrdinalIgnoreCase)) != null;
	}

	public void UpdatePurchaseDetails(int ticketID, int storeID)
	{
		mChangeVikingNameTicketID = ticketID;
		mChangeVikingNameStoreID = storeID;
		mEnablePurchase = true;
	}

	private void OnPressedOK()
	{
		CloseUI(Status.Accepted);
	}

	public void CloseUI(Status status)
	{
		if (status == Status.Accepted)
		{
			LogFTUENameCompleteEvent();
		}
		KAUI.RemoveExclusive(this);
		UnityEngine.Object.Destroy(base.gameObject);
		mCallBack?.Invoke(status, mTxtName.GetText(), SuggestedName(mTxtName.GetText()), null);
	}

	private void LogFTUENameCompleteEvent()
	{
		TimeSpan timeSpan = DateTime.Now - mChooseNameStartTime;
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("duration", timeSpan.Seconds);
		dictionary.Add("failCount", mNameValidationFailCount);
		AnalyticAgent.LogFTUEEvent(FTUEEvent.CHARACTER_NAME_COMPLETED, dictionary);
	}

	private void ShowMessageDB(LocaleString message, LocaleString title, string yesMessage, string noMessage, bool processingDB)
	{
		mGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "MessageDB");
		if (message != null)
		{
			mGenericDB.SetText(message.GetLocalizedString(), interactive: false);
		}
		if (title != null)
		{
			mGenericDB.SetTitle(title.GetLocalizedString());
		}
		mGenericDB._MessageObject = base.gameObject;
		mGenericDB._YesMessage = yesMessage;
		mGenericDB.SetDestroyOnClick(isDestroy: true);
		mGenericDB.SetButtonVisibility(!processingDB, !processingDB, inOKBtn: false, inCloseBtn: false);
		KAUI.SetExclusive(mGenericDB, new Color(0.5f, 0.5f, 0.5f, 0.5f));
	}

	private void DestroyMessageDB()
	{
		if (!(mGenericDB == null))
		{
			KAUI.RemoveExclusive(mGenericDB);
			UnityEngine.Object.Destroy(mGenericDB.gameObject);
		}
	}
}
