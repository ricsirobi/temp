using System;
using System.Collections;
using UnityEngine;

public class UiOptions : KAUI
{
	public static OnOptionsChanged OnOptions = null;

	public static string _Credits_exitLevel = "";

	public static Action<bool> OnSetNPCNameTagVisbility;

	public GameObject _ScreenSizeGroup;

	public GameObject _MobileControlGroup;

	public GameObject _KeyboardControlGroup;

	public GameObject _ServerTypeGroup;

	public LocaleString _CalibrateTitleText = new LocaleString("Accelerometer Calibration");

	public LocaleString _CalibrateText = new LocaleString("The accelerometer will now be calibrated. Hold the device as you require then press OK to calibrate.");

	public LocaleString _CalibrateCountDownText = new LocaleString("Calibrating in {0} seconds...");

	public LocaleString _CalibrationDoneText = new LocaleString("Accelerometer Calibrated!");

	public LocaleString _MMOEnableWarningText = new LocaleString("Enabling MMO may affect your game's performance. Do you want to go ahead?");

	public LocaleString _MMODisableWarningText = new LocaleString("Do you really want to disable MMO?");

	public LocaleString _ServerErrorTitleText = new LocaleString("Error");

	public LocaleString _ServerErrorBodyText = new LocaleString("Something went wrong when trying to delete your account, please contact customer support if the issue persists");

	public LocaleString _MultiplayerDisabledOnServerText = new LocaleString("You need to turn multiplayer on. You can do that from your account at www.schoolofdragons.com.");

	public LocaleString _MMOTitleText = new LocaleString("MMO Warning");

	public LocaleString _DailyQuestInProgressText = new LocaleString("You have daily quests that are in progress. Do you still want to exit");

	public LocaleString _DailyQuestDBTitleText = new LocaleString("Alert");

	public LocaleString _DeleteAccountPromptText = new LocaleString("If you want to delete your account, please type {{SECURITY_WORD}} in the field below. This action cannot be undone once completed.");

	public LocaleString _DeleteAccountTitleText = new LocaleString("Delete Account");

	public LocaleString _DeleteAccountHeadingText = new LocaleString("Type {{SECURITY_WORD}} in the field above");

	public LocaleString _DeleteAccountSecurityWordText = new LocaleString("DELETE");

	public LocaleString _DeleteAccountRequestConfirmationText = new LocaleString("An email with a link to complete account deletion has been sent");

	public LocaleString _DeleteAccountRequestConfirmationTitleText = new LocaleString("Email Sent");

	public LocaleString _DeleteGuestAccountPromptText = new LocaleString("To delete your account, please type {{SECURITY_WORD}} in the field below. This action cannot be undone.");

	public LocaleString _DeleteGuestAccountRequestConfirmationTitleText = new LocaleString("Account Deleted");

	public LocaleString _DeleteGuestAccountSuccessText = new LocaleString("Your request for Account Deletion was successful, and your Guest Account has been deleted. Thank you for playing School of Dragons!");

	private string mExitLevelName = "ProfileSelectionDO";

	public string _ProfileLevelName = "ProfileSelectionDO";

	public string _ExitGuestLevelName = "LoginDM";

	public string _CreditsLevelName = "CreditsDO";

	public KAUI _HotKeysPopUp;

	public KAUI _GraphicSettingsUI;

	public UiSettingsMenu _GameplaySettingsMenu;

	public UiSettingsMenu _AccountSettingMenu;

	public KAWidget _NameSettingsGroup;

	private KAUIGenericDB mKAUIGenericDB;

	private int mCountdownTimer;

	public GameObject TabPageAudioVisual;

	public GameObject TabPageGameplay;

	public GameObject TabPageAccount;

	private KAWidget mBtnBack;

	private KAWidget mBtnCustomizeHUD;

	private KAWidget mBtnExitGame;

	private KAWidget mBtnVikingList;

	private KAWidget mBtnCredits;

	private KAWidget mBtnRedeemCode;

	private KAWidget mBtnDeleteAccount;

	private KAToggleButton mBtnQuestArrow;

	private KAToggleButton mBtnFlightInverted;

	private KAWidget mBtnOpenNameVisibilityMenu;

	private KAToggleButton mBtnToggleNameVisibilitySettings;

	private KAToggleButton mBtnNameSettings;

	private KAToggleButton mBtnYourName;

	private KAToggleButton mBtnOtherNames;

	private KAToggleButton mBtnNPCNames;

	private KAToggleButton mBtnFullScreen;

	private KAToggleButton mBtnFlightControlTouch;

	private KAToggleButton mBtnMMO;

	private KAToggleButton mToggleGameplaySettingsTab;

	private KAToggleButton mToggleAudioVisualSettingsTab;

	private KAToggleButton mToggleAccountSettingsTab;

	private ApiTokenStatus mTokenStatus;

	private bool mChangeScene;

	private bool mIsFullScreen;

	private bool mMMOStatus;

	private bool mDisableWidgetsForVikingSelect;

	private static bool mIsQuestArrowStatusRead = false;

	private static bool mShowQuestArrow = true;

	private static bool mIsFlightInvertedStatusRead = false;

	private static bool mIsFlightInverted = false;

	private static bool mIsTiltSteerStatusRead = false;

	private static bool mIsTiltSteer = false;

	private static bool mIsCalibratedXRead = false;

	private static float mCalibratedX = 0f;

	private static bool mIsCalibratedYRead = false;

	private static float mCalibratedY = 0f;

	private static bool mIsCalibratedZRead = false;

	private static float mCalibratedZ = 0f;

	[NonSerialized]
	public string _BundlePath = string.Empty;

	public static float pCalibratedX
	{
		get
		{
			if (!mIsCalibratedXRead && UserInfo.pIsReady)
			{
				mCalibratedX = PlayerPrefs.GetFloat("CalibratedX" + UserInfo.pInstance.ParentUserID, 0f);
				mIsCalibratedXRead = true;
			}
			return mCalibratedX;
		}
		set
		{
			PlayerPrefs.SetFloat("CalibratedX" + UserInfo.pInstance.ParentUserID, value);
			mCalibratedX = value;
			mIsCalibratedXRead = true;
		}
	}

	public static float pCalibratedY
	{
		get
		{
			if (!mIsCalibratedYRead && UserInfo.pIsReady)
			{
				mCalibratedY = PlayerPrefs.GetFloat("CalibratedY" + UserInfo.pInstance.ParentUserID, 0f);
				mIsCalibratedYRead = true;
			}
			return mCalibratedY;
		}
		set
		{
			PlayerPrefs.SetFloat("CalibratedY" + UserInfo.pInstance.ParentUserID, value);
			mCalibratedY = value;
			mIsCalibratedYRead = true;
		}
	}

	public static float pCalibratedZ
	{
		get
		{
			if (!mIsCalibratedZRead && UserInfo.pIsReady)
			{
				mCalibratedZ = PlayerPrefs.GetFloat("CalibratedZ" + UserInfo.pInstance.ParentUserID, 0f);
				mIsCalibratedZRead = true;
			}
			return mCalibratedZ;
		}
		set
		{
			PlayerPrefs.SetFloat("CalibratedZ" + UserInfo.pInstance.ParentUserID, value);
			mCalibratedZ = value;
			mIsCalibratedZRead = true;
		}
	}

	public static bool pIsTiltSteer
	{
		get
		{
			if (!mIsTiltSteerStatusRead && UserInfo.pIsReady)
			{
				mIsTiltSteer = PlayerPrefs.GetInt("TiltSteer" + UserInfo.pInstance.ParentUserID, 0) == 1;
				mIsTiltSteerStatusRead = true;
			}
			return mIsTiltSteer;
		}
		set
		{
			PlayerPrefs.SetInt("TiltSteer" + UserInfo.pInstance.ParentUserID, value ? 1 : 0);
			mIsTiltSteer = value;
			mIsTiltSteerStatusRead = true;
		}
	}

	public static bool pShowQuestArrow
	{
		get
		{
			if (!mIsQuestArrowStatusRead && UserInfo.pIsReady)
			{
				mShowQuestArrow = PlayerPrefs.GetInt("ShowQuestArrow" + UserInfo.pInstance.ParentUserID, 1) == 1;
				mIsQuestArrowStatusRead = true;
			}
			return mShowQuestArrow;
		}
		set
		{
			PlayerPrefs.SetInt("ShowQuestArrow" + UserInfo.pInstance.ParentUserID, value ? 1 : 0);
			mShowQuestArrow = value;
			mIsQuestArrowStatusRead = true;
		}
	}

	public static bool pIsFlightInverted
	{
		get
		{
			if (!mIsFlightInvertedStatusRead && UserInfo.pIsReady)
			{
				mIsFlightInverted = PlayerPrefs.GetInt("FlightInverted" + UserInfo.pInstance.ParentUserID, 0) == 1;
				mIsFlightInvertedStatusRead = true;
			}
			return mIsFlightInverted;
		}
		set
		{
			PlayerPrefs.SetInt("FlightInverted" + UserInfo.pInstance.ParentUserID, value ? 1 : 0);
			mIsFlightInverted = value;
			mIsFlightInvertedStatusRead = true;
		}
	}

	protected override void Start()
	{
		base.Start();
		if (FUEManager.pInstance != null)
		{
			FUEManager.pInstance.SetupHUD(base.gameObject.name);
		}
		Initialize();
	}

	private void Initialize()
	{
		mBtnBack = FindItem("BtnBack");
		mBtnExitGame = FindItem("BtnExitGame");
		mBtnVikingList = FindItem("BtnVikingList");
		mBtnCustomizeHUD = FindItem("BtnCustomiseHUD");
		mBtnCredits = FindItem("CreditsBtn");
		mToggleGameplaySettingsTab = (KAToggleButton)FindItem("TglTabGameplay");
		mToggleAudioVisualSettingsTab = (KAToggleButton)FindItem("TglTabAudioVisual");
		mToggleAccountSettingsTab = (KAToggleButton)FindItem("TglTabAccount");
		_GameplaySettingsMenu.Init();
		mBtnMMO = _GameplaySettingsMenu.FindItem("BtnMMO") as KAToggleButton;
		if ((bool)mBtnMMO)
		{
			string key = "USE_MMO" + UserInfo.pInstance.UserID;
			mMMOStatus = UserInfo.pInstance.MultiplayerEnabled && (PlayerPrefs.HasKey(key) ? (PlayerPrefs.GetInt(key, 1) == 1) : UtPlatform.GetMMODefaultState());
			mBtnMMO.SetChecked(mMMOStatus);
			MainStreetMMOClient.pIsMMOEnabled = mMMOStatus;
		}
		mBtnToggleNameVisibilitySettings = _GameplaySettingsMenu.FindItem("BtnToggleNameVisibilitySettings") as KAToggleButton;
		mBtnFullScreen = _GameplaySettingsMenu.FindItem("BtnFullScreen") as KAToggleButton;
		if ((bool)mBtnFullScreen)
		{
			mBtnFullScreen.SetChecked(!Screen.fullScreen);
		}
		mBtnFlightControlTouch = _GameplaySettingsMenu.FindItem("BtnFlightControlTouch") as KAToggleButton;
		if ((bool)mBtnFlightControlTouch)
		{
			mBtnFlightControlTouch.SetChecked(pIsTiltSteer);
		}
		mBtnQuestArrow = _GameplaySettingsMenu.FindItem("BtnQuestArrow") as KAToggleButton;
		if ((bool)mBtnQuestArrow)
		{
			mBtnQuestArrow.SetChecked(!pShowQuestArrow);
		}
		mBtnFlightInverted = _GameplaySettingsMenu.FindItem("BtnFlightInverted") as KAToggleButton;
		if ((bool)mBtnFlightInverted)
		{
			mBtnFlightInverted.SetChecked(pIsFlightInverted);
		}
		_AccountSettingMenu.Init();
		mBtnDeleteAccount = _AccountSettingMenu.FindItem("BtnDeleteAccount");
		if (mDisableWidgetsForVikingSelect)
		{
			mBtnVikingList.SetDisabled(isDisabled: true);
			mBtnCredits.SetDisabled(isDisabled: true);
		}
		ToggleSettingsTab(SettingTabs.AudioVisual);
		mBtnYourName = (KAToggleButton)FindItem("BtnYourName");
		if ((bool)mBtnYourName)
		{
			mBtnYourName.SetChecked(!AvatarData.pDisplayYourName);
		}
		mBtnOtherNames = (KAToggleButton)FindItem("BtnOtherNames");
		if ((bool)mBtnOtherNames)
		{
			mBtnOtherNames.SetChecked(!AvatarData.pDisplayOtherName);
		}
		mBtnNPCNames = (KAToggleButton)FindItem("BtnNPCNames");
		if ((bool)mBtnNPCNames)
		{
			mBtnNPCNames.SetChecked(!AvatarData.pDisplayNPCName);
		}
		if (UtPlatform.IsMobile() && AvAvatar.pObject != null)
		{
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if (component != null && component.pIsPlayerGliding && mBtnCustomizeHUD != null)
			{
				mBtnCustomizeHUD.SetState(KAUIState.DISABLED);
			}
		}
		if (RsResourceManager.pCurrentLevel == _CreditsLevelName)
		{
			mBtnCredits.SetDisabled(isDisabled: true);
		}
		mBtnRedeemCode = FindItem("BtnRedeemCode");
		_NameSettingsGroup.SetVisibility(inVisible: false);
		KAUI.SetExclusive(this);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mToggleAudioVisualSettingsTab)
		{
			ToggleSettingsTab(SettingTabs.AudioVisual);
		}
		else if (inWidget == mToggleGameplaySettingsTab)
		{
			ToggleSettingsTab(SettingTabs.Gameplay);
		}
		else if (inWidget == mToggleAccountSettingsTab)
		{
			ToggleSettingsTab(SettingTabs.Account);
		}
		else if (inWidget == mBtnToggleNameVisibilitySettings || inWidget.name == "BtnNameVisibility")
		{
			_NameSettingsGroup.SetVisibility(!_NameSettingsGroup.GetVisibility());
		}
		else if (inWidget == mBtnFullScreen)
		{
			Screen.fullScreen = !Screen.fullScreen;
			mBtnFullScreen.SetChecked(!Screen.fullScreen);
		}
		else if (inWidget == mBtnFlightControlTouch)
		{
			ResetCalibration();
			if (pIsTiltSteer)
			{
				PromptCalibration();
			}
		}
		else if (inWidget == mBtnQuestArrow)
		{
			pShowQuestArrow = !pShowQuestArrow;
			mBtnQuestArrow.SetChecked(!pShowQuestArrow);
		}
		else if (inWidget == mBtnFlightInverted)
		{
			pIsFlightInverted = !pIsFlightInverted;
			mBtnFlightInverted.SetChecked(pIsFlightInverted);
		}
		else if (inWidget == mBtnYourName)
		{
			int value = ((!AvatarData.pDisplayYourName) ? 1 : 0);
			PlayerPrefs.SetInt("DYN" + UserInfo.pInstance.UserID, value);
			AvatarData.pDisplayYourName = !AvatarData.pDisplayYourName;
			if (RsResourceManager.pCurrentLevel != "MovieTheater")
			{
				AvAvatar.SetDisplayNameVisible(AvatarData.pDisplayYourName);
			}
		}
		else if (inWidget == mBtnOtherNames)
		{
			int value2 = ((!AvatarData.pDisplayOtherName) ? 1 : 0);
			PlayerPrefs.SetInt("DON" + UserInfo.pInstance.UserID, value2);
			AvatarData.pDisplayOtherName = !AvatarData.pDisplayOtherName;
			if (MainStreetMMOClient.pInstance != null)
			{
				MainStreetMMOClient.pInstance.SetOtherDisplayNamesVisible(AvatarData.pDisplayOtherName);
			}
		}
		else if (inWidget == mBtnNPCNames)
		{
			int value3 = ((!AvatarData.pDisplayNPCName) ? 1 : 0);
			PlayerPrefs.SetInt("DNN" + UserInfo.pInstance.UserID, value3);
			AvatarData.pDisplayNPCName = !AvatarData.pDisplayNPCName;
			OnSetNPCNameTagVisbility?.Invoke(AvatarData.pDisplayNPCName);
		}
		else if (inWidget == mBtnBack)
		{
			Destroy();
			if (RsResourceManager.pCurrentLevel != "ProfileSelectionDO")
			{
				AvAvatar.SetUIActive(inActive: true);
				AvAvatar.pState = AvAvatarState.IDLE;
			}
			OnOptions?.Invoke();
		}
		else if (inWidget == mBtnExitGame)
		{
			if (!CheckDailyQuestInProgress(isExitGame: true))
			{
				ExitGame();
			}
		}
		else if (inWidget == mBtnVikingList)
		{
			if (!CheckDailyQuestInProgress(isExitGame: false))
			{
				LoadProfileSelection();
			}
		}
		else if (inWidget == mBtnDeleteAccount)
		{
			string text = (UiLogin.pIsGuestUser ? _DeleteGuestAccountPromptText.GetLocalizedString() : _DeleteAccountPromptText.GetLocalizedString());
			text = text.Replace("{{SECURITY_WORD}}", _DeleteAccountSecurityWordText.GetLocalizedString());
			UiConfirmationDB uiConfirmationDB = GameUtilities.DisplayGenericDB("PfUiConfirmationDB", text, _DeleteAccountTitleText.GetLocalizedString(), base.gameObject, "DeleteAccount", "DeleteAccountCancel", null, null, inDestroyOnClick: true) as UiConfirmationDB;
			if (!(uiConfirmationDB == null))
			{
				string localizedString = _DeleteAccountHeadingText.GetLocalizedString();
				localizedString = localizedString.Replace("{{SECURITY_WORD}}", _DeleteAccountSecurityWordText.GetLocalizedString());
				uiConfirmationDB.SetSecurityText(localizedString, _DeleteAccountSecurityWordText.GetLocalizedString());
			}
		}
		else if (inWidget == mBtnCustomizeHUD)
		{
			SetVisibility(inVisible: false);
			UiCustomizeHUD.Load(base.gameObject);
		}
		else if (inWidget == mBtnCredits)
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			SetVisibility(inVisible: false);
			if (AvAvatar.pObject != null)
			{
				AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
				if (component != null && component.pIsPlayerGliding)
				{
					component.OnGlideLanding();
				}
			}
			if (MainStreetMMOClient.pInstance != null)
			{
				MainStreetMMOClient.pInstance.SetJoinAllowed(MMOJoinStatus.ALLOWED);
			}
			if (RsResourceManager.pCurrentLevel != _CreditsLevelName)
			{
				_Credits_exitLevel = RsResourceManager.pCurrentLevel;
			}
			RsResourceManager.LoadLevel(_CreditsLevelName);
		}
		else if (inWidget == mBtnMMO)
		{
			mBtnMMO.SetChecked(!mBtnMMO.IsChecked());
			if (!UserInfo.pInstance.MultiplayerEnabled)
			{
				SetVisibility(inVisible: false);
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _MultiplayerDisabledOnServerText.GetLocalizedString(), _ServerErrorTitleText.GetLocalizedString(), base.gameObject, null, null, "OnCloseDB", null, inDestroyOnClick: true);
			}
			else
			{
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", mBtnMMO.IsChecked() ? _MMODisableWarningText.GetLocalizedString() : _MMOEnableWarningText.GetLocalizedString(), _MMOTitleText.GetLocalizedString(), base.gameObject, "OnMMOChangeYes", "OnMMOChangeNo", null, null, inDestroyOnClick: true);
			}
		}
		else if (inWidget.name == "BtnGraphicSettings")
		{
			_GraphicSettingsUI.SetVisibility(inVisible: true);
			KAUI.SetExclusive(_GraphicSettingsUI);
		}
		else if (inWidget.name == "BtnHotKeys")
		{
			_HotKeysPopUp.SetVisibility(inVisible: true);
			KAUI.SetExclusive(_HotKeysPopUp);
		}
		else if (inWidget.name == "BtnRegister")
		{
			RegisterUser();
		}
		else if (UtPlatform.IsiOS() && inWidget.name == "BtnRestorePurchases")
		{
			if (IAPManager.pIsReady)
			{
				IAPManager.pInstance.RestorePurchases();
			}
		}
		else if (inWidget.name == "BtnDev")
		{
			PlayerPrefs.SetString("BuildServerType", "Dev");
			GameUtilities.DisplayOKMessage("PfKAUIGenericDBSm", "Server type changed to Dev. Quit & launch the app", null, "");
		}
		else if (inWidget.name == "BtnQA")
		{
			PlayerPrefs.SetString("BuildServerType", "QA");
			GameUtilities.DisplayOKMessage("PfKAUIGenericDBSm", "Server type changed to QA. Quit & launch the app", null, "");
		}
		else if (inWidget.name == "BtnStaging")
		{
			PlayerPrefs.SetString("BuildServerType", "Staging");
			GameUtilities.DisplayOKMessage("PfKAUIGenericDBSm", "Server type changed to Staging. Quit & launch the app", null, "");
		}
		else if (inWidget == mBtnRedeemCode)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfUiEnterCodeDB"));
			if (gameObject != null)
			{
				UiPrizeCodeEnterDB component2 = gameObject.GetComponent<UiPrizeCodeEnterDB>();
				if (component2 != null)
				{
					component2._MessageObject = base.gameObject;
				}
			}
		}
		else if (inWidget.name == "BtnContactUs" || inWidget.name == "BtnFAQ")
		{
			if (inWidget.name == "BtnContactUs")
			{
				DragonsZendesk.OpenCreateRequest();
			}
			else
			{
				DragonsZendesk.OpenHelpCenter();
			}
			SetInteractive(interactive: false);
			DragonsZendesk.pOnClosedDelegate = (DragonsZendesk.OnClosed)Delegate.Combine(DragonsZendesk.pOnClosedDelegate, new DragonsZendesk.OnClosed(OnSupportClosed));
		}
	}

	private void ToggleSettingsTab(SettingTabs settingTab)
	{
		TabPageAudioVisual.SetActive(settingTab == SettingTabs.AudioVisual);
		TabPageGameplay.SetActive(settingTab == SettingTabs.Gameplay);
		TabPageAccount.SetActive(settingTab == SettingTabs.Account);
	}

	private void OnSupportClosed()
	{
		DragonsZendesk.pOnClosedDelegate = (DragonsZendesk.OnClosed)Delegate.Remove(DragonsZendesk.pOnClosedDelegate, new DragonsZendesk.OnClosed(OnSupportClosed));
		SetInteractive(interactive: true);
		KAUI.SetExclusive(this);
	}

	private void ExitGame()
	{
		mExitLevelName = GameConfig.GetKeyData("LoginScene");
		GameUtilities.Logout(fullReset: false);
		UserRankData.Reset();
		UserInfo.Reset();
		ProductData.Reset();
		SubscriptionInfo.Reset();
		CommonInventoryData.Reset();
		PairData.Reset();
		RaisedPetData.Reset();
		UserProfile.Reset();
		BuddyList.Reset();
		Money.Reset();
		Group.Reset();
		ParentData.Reset();
		IAPManager.Reset();
		if (PlayerPrefs.GetInt("SafeAppClose") == 2)
		{
			PlayerPrefs.SetInt("SafeAppClose", 1);
		}
		DestroyDB();
		KAUICursorManager.SetDefaultCursor("Loading");
		WsTokenMonitor.OnTokenStatus += TokenStatus;
		WsTokenMonitor.pCheckToken = true;
		WsTokenMonitor.ForceCheckToken();
		SetVisibility(inVisible: false);
	}

	private void LoadProfileSelection()
	{
		mExitLevelName = _ProfileLevelName;
		DestroyDB();
		KAUICursorManager.SetDefaultCursor("Loading");
		WsTokenMonitor.OnTokenStatus += TokenStatus;
		WsTokenMonitor.pCheckToken = true;
		WsTokenMonitor.ForceCheckToken();
		SetVisibility(inVisible: false);
	}

	private void RegisterUser()
	{
		Destroy();
		SetVisibility(inVisible: false);
		GameUtilities.LoadLoginLevel(showRegstration: true);
	}

	private void OnCloseDB()
	{
		SetVisibility(inVisible: true);
	}

	private void TokenStatus(ApiTokenStatus inStatus)
	{
		mChangeScene = true;
		mTokenStatus = inStatus;
		WsTokenMonitor.OnTokenStatus -= TokenStatus;
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (inStatus == ApiTokenStatus.TokenValid)
		{
			SubstanceCustomization.pInstance = null;
			RsResourceManager.LoadLevel(UiLogin.pIsGuestUser ? _ExitGuestLevelName : mExitLevelName);
		}
	}

	private void OnMMOChangeYes()
	{
		mMMOStatus = !mMMOStatus;
		mBtnMMO.SetChecked(!mBtnMMO.IsChecked());
		PlayerPrefs.SetInt("USE_MMO" + UserInfo.pInstance.UserID, mMMOStatus ? 1 : 0);
		MainStreetMMOClient.pIsMMOEnabled = !MainStreetMMOClient.pIsMMOEnabled;
	}

	private void OnMMOChangeNo()
	{
	}

	private void DeleteAccount()
	{
		WsWebService.DeleteAccount(ServiceEventHandler, null);
		ShowBusy(isBusy: true);
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserdata)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			ShowBusy(isBusy: false);
			if (inObject == null)
			{
				break;
			}
			if ((MembershipUserStatus)inObject == MembershipUserStatus.Success)
			{
				bool pIsGuestUser = UiLogin.pIsGuestUser;
				string inText = (pIsGuestUser ? _DeleteGuestAccountSuccessText.GetLocalizedString() : _DeleteAccountRequestConfirmationText.GetLocalizedString());
				string inTitle = (pIsGuestUser ? _DeleteGuestAccountRequestConfirmationTitleText.GetLocalizedString() : _DeleteAccountRequestConfirmationTitleText.GetLocalizedString());
				if (!pIsGuestUser)
				{
					GameUtilities.Logout();
				}
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", inText, inTitle, base.gameObject, null, null, "OnConfirmAccountDeletionDBClosed", null, inDestroyOnClick: true);
			}
			else
			{
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _ServerErrorBodyText.GetLocalizedString(), _ServerErrorTitleText.GetLocalizedString(), base.gameObject, null, null, "OnCloseDB", null, inDestroyOnClick: true);
			}
			break;
		case WsServiceEvent.ERROR:
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _ServerErrorBodyText.GetLocalizedString(), _ServerErrorTitleText.GetLocalizedString(), base.gameObject, null, null, "OnCloseDB", null, inDestroyOnClick: true);
			ShowBusy(isBusy: false);
			break;
		}
	}

	private void OnConfirmAccountDeletionDBClosed()
	{
		GameUtilities.LoadLoginLevel();
	}

	private void DeleteAccountCancel()
	{
		OnCloseDB();
	}

	private void ShowBusy(bool isBusy)
	{
		if (isBusy)
		{
			KAUICursorManager.SetDefaultCursor("Loading");
		}
		else
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
		SetInteractive(!isBusy);
	}

	private void LateUpdate()
	{
		if (mChangeScene)
		{
			mChangeScene = false;
			if (mTokenStatus == ApiTokenStatus.TokenValid)
			{
				GameUtilities.Logout(fullReset: false);
				Destroy();
			}
		}
	}

	private void ResetCalibration()
	{
		pIsTiltSteer = !pIsTiltSteer;
		AdjustFlightControls();
		mBtnFlightControlTouch.SetChecked(pIsTiltSteer);
	}

	public void OnCalibrate()
	{
		mCountdownTimer = 5;
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: false);
		StartCoroutine("ShowCountdown");
	}

	public void DisableWidgetsForVikingSelect(bool enable)
	{
		mDisableWidgetsForVikingSelect = enable;
	}

	private IEnumerator ShowCountdown()
	{
		while (mCountdownTimer >= 0)
		{
			mKAUIGenericDB.SetText(string.Format(_CalibrateCountDownText.GetLocalizedString(), mCountdownTimer), interactive: false);
			mCountdownTimer--;
			yield return new WaitForSeconds(1f);
		}
		Vector3 acceleration = Input.acceleration;
		KAInput.pInstance.SetCalibration(acceleration.x, acceleration.y, acceleration.z, InputType.ACCELEROMETER);
		pCalibratedX = acceleration.x;
		pCalibratedY = acceleration.y;
		pCalibratedZ = acceleration.z;
		mKAUIGenericDB.SetText(_CalibrationDoneText.GetLocalizedString(), interactive: false);
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mKAUIGenericDB._OKMessage = "OnCalibrateOK";
	}

	private void OnCalibrateOK()
	{
		DestroyDB();
		OnOptions?.Invoke();
	}

	private void DestroyDB()
	{
		KAUI.RemoveExclusive(mKAUIGenericDB);
		if (mKAUIGenericDB != null)
		{
			mKAUIGenericDB.Destroy();
		}
		mKAUIGenericDB = null;
	}

	protected override void Update()
	{
		base.Update();
		if (Screen.fullScreen != mIsFullScreen)
		{
			mIsFullScreen = Screen.fullScreen;
			if (mBtnFullScreen != null)
			{
				mBtnFullScreen.SetChecked(mIsFullScreen);
			}
		}
	}

	public void Destroy()
	{
		SetVisibility(inVisible: false);
		UnityEngine.Object.Destroy(base.gameObject);
		RsResourceManager.Unload(_BundlePath);
	}

	private void OnCustomizeHUDClosed()
	{
		SetVisibility(inVisible: true);
	}

	public void PromptCalibration()
	{
		if (mKAUIGenericDB == null)
		{
			mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
		}
		mKAUIGenericDB.SetTitle(_CalibrateTitleText.GetLocalizedString());
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB.SetExclusive();
		mKAUIGenericDB._OKMessage = "OnCalibrate";
		mKAUIGenericDB.SetText(_CalibrateText.GetLocalizedString(), interactive: false);
	}

	private bool CheckDailyQuestInProgress(bool isExitGame)
	{
		if (FUEManager.pIsFUERunning || MissionManager.pInstance.pDailyMissionStateResult == null || MissionManager.pInstance._DailyMissions == null || MissionManager.pInstance._DailyMissions.Count == 0 || MissionManager.pInstance.pDailyMissionStateResult.UserTimedAchievement == null || MissionManager.pInstance.pDailyMissionStateResult.UserTimedAchievement.Count == 0)
		{
			return false;
		}
		foreach (MissionManager.DailyMissions dailyMission in MissionManager.pInstance._DailyMissions)
		{
			if (MissionManager.pInstance.GetAllMissions(dailyMission._GroupID).Find((Mission m) => m.Completed == 0 && m.pStarted) != null)
			{
				if (mKAUIGenericDB == null)
				{
					mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Alert");
				}
				mKAUIGenericDB.SetTitle(_DailyQuestDBTitleText.GetLocalizedString());
				mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
				mKAUIGenericDB._MessageObject = base.gameObject;
				mKAUIGenericDB.SetExclusive();
				mKAUIGenericDB._YesMessage = (isExitGame ? "ExitGame" : "LoadProfileSelection");
				mKAUIGenericDB._NoMessage = "DestroyDB";
				mKAUIGenericDB.SetText(_DailyQuestInProgressText.GetLocalizedString(), interactive: false);
				return true;
			}
		}
		return false;
	}

	private void AdjustFlightControls()
	{
		if (UiAvatarControls.pInstance != null)
		{
			UiAvatarControls.EnableTiltControls(AvAvatar.pSubState == AvAvatarSubState.FLYING);
		}
	}

	private void OnRedeemPrizeCode()
	{
		Destroy();
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		OnOptions?.Invoke();
	}
}
