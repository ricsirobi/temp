using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KA.Framework;
using UnityEngine;

public class UiLogin : KAUI
{
	public const string REM_PASSWORD = "REM_PASSWORD";

	public const string REM_USERNAME = "REM_USER_NAME";

	public const string GUEST_ACCOUNT_CREATED = "GUEST_ACC_CREATED";

	public const string APP_LAUNCH_COUNT = "APP_LAUNCH_COUNT";

	public const string REM_LANGUAGE = "REM_LANGUAGE";

	public const int MAX_GUEST_USER_FRIENDS = 5;

	public UiServerChange _ServerChangeUI;

	public UiRegister _RegisterUI;

	public KAWidget _AccountRecover;

	public UIForgotLoginPanel _ForgotLoginPanelUI;

	public LoginStrings _Strings;

	public UiLanguageMenu _LanguageMenuUI;

	public UiGraphicSettings _GraphicSettingsUI;

	private static bool mHasUserJustRegistered = false;

	private static bool mIsGuestUser = false;

	private static bool mGuestUserFirstLaunch = false;

	private static bool mShowRegistrationPage = false;

	private static string mUserName = "";

	private static string mPassword = "";

	private static string mChildID = "";

	private static string mLocale = "en-US";

	private static ParentLoginInfo mParentInfo = null;

	private static UiLogin mInstance = null;

	private static string mMacAddress = "KAI_MAC_03";

	private KAUIGenericDB mKAUIGenericDB;

	private bool mPrefetchStarted;

	private bool mEnablePlayButton = true;

	private bool mShowCrashWarningOnDBClose;

	private bool mCanLoadNextScene;

	private KAWidget mBtnQuitGame;

	private KAWidget mBtnLogout;

	private KAWidget mBtnLogin;

	private KAWidget mBtnRegister;

	private KAWidget mBtnPlayAsGuest;

	private KAWidget mBtnPlay;

	private KAWidget mBtnReleaseNotes;

	private KAWidget mBtnForums;

	private KAWidget mBtnGraphics;

	private KAEditBox mTxtEmail;

	private KAEditBox mTxtPassword;

	private KAWidget mTxtAccountRecover;

	private KAWidget mLanguageText;

	private KAWidget mAniLanguageSelection;

	public string _GooglePlayMoreAppsLink = "market://search?q=pub:Knowledge Adventure, Inc.";

	public static int _RewardForRegistering = 0;

	private string mStoredUserName = "";

	private bool mIsUserNameEdited;

	private KAUIGenericDB mUiGenericDB;

	private LoginManager mLoginManager = new LoginManager();

	public PoolInfo[] _TurnOffPoolInfo;

	private LoginContent mLoginContent;

	public UiReleaseNotes _ReleaseNotesUI;

	public string[] _EmptyUIList;

	private bool mResetAds;

	private DateTime mPrefetchStartTime;

	public List<RemoveSupportReminder> _RemoveSupportReminders;

	public bool ClearPreloadBundles { get; set; }

	public static UiLogin pInstance => mInstance;

	public static bool pHasUserJustRegistered
	{
		get
		{
			return mHasUserJustRegistered;
		}
		set
		{
			mHasUserJustRegistered = value;
		}
	}

	public static bool pGuestUserFirstLaunch
	{
		get
		{
			return mGuestUserFirstLaunch;
		}
		set
		{
			mGuestUserFirstLaunch = value;
		}
	}

	public static ParentLoginInfo pParentInfo
	{
		get
		{
			return mParentInfo;
		}
		set
		{
			mParentInfo = value;
		}
	}

	public static string pLocale
	{
		get
		{
			return mLocale;
		}
		set
		{
			mLocale = value;
		}
	}

	public static string pUserName
	{
		get
		{
			return mUserName;
		}
		set
		{
			mUserName = value;
		}
	}

	public static string pPassword
	{
		get
		{
			return mPassword;
		}
		set
		{
			mPassword = value;
		}
	}

	public static string pChildID
	{
		get
		{
			return mChildID;
		}
		set
		{
			mChildID = value;
		}
	}

	public static string pMacAddress => mMacAddress;

	public static bool pIsGuestUser
	{
		get
		{
			return mIsGuestUser;
		}
		set
		{
			mIsGuestUser = value;
		}
	}

	public static bool pShowRegistrationPage
	{
		get
		{
			return mShowRegistrationPage;
		}
		set
		{
			mShowRegistrationPage = value;
		}
	}

	public KAEditBox pTxtEmail => mTxtEmail;

	public KAEditBox pTxtPassword => mTxtPassword;

	private void LoadAdsXML()
	{
		RsResourceManager.Load(GameConfig.GetKeyData("LoginContentAsset"), XmlLoadEventHandler);
	}

	public void XmlLoadEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (inObject != null)
			{
				mLoginContent = UtUtilities.DeserializeFromXml<LoginContent>((string)inObject);
			}
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("LoginContent data file missing!!!");
			break;
		}
	}

	public void LoadLocaleAds()
	{
		RsResourceManager.Unload(GameConfig.GetKeyData("LoginContentAsset"), splitURL: true, force: true);
		mLoginContent = null;
		LoadAdsXML();
		mResetAds = true;
		_ReleaseNotesUI.pInitDone = false;
	}

	private void PopulateUiContent()
	{
		if (mLoginContent == null || mLoginContent.AdSections == null || mLoginContent.AdSections.Length == 0)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < _UiList.Length; i++)
		{
			UiLoginAds uiLoginAds = _UiList[i] as UiLoginAds;
			if (uiLoginAds == null)
			{
				continue;
			}
			for (int j = 0; j < mLoginContent.AdSections.Length; j++)
			{
				AdSection adSection = mLoginContent.AdSections[j];
				if (!uiLoginAds.gameObject.name.Equals(adSection.UiName))
				{
					continue;
				}
				List<AdAttributes> list = new List<AdAttributes>();
				if (adSection.AdAttributes != null && adSection.AdAttributes.Length != 0)
				{
					for (int k = 0; k < adSection.AdAttributes.Length; k++)
					{
						if (adSection.AdAttributes[k].MemberType.HasValue && ((adSection.AdAttributes[k].MemberType == MembershipType.Member && !SubscriptionInfo.pIsMember) || (adSection.AdAttributes[k].MemberType == MembershipType.NonMember && SubscriptionInfo.pIsMember)))
						{
							continue;
						}
						if (UserInfo.pIsReady && UserInfo.pInstance.CreationDate.HasValue)
						{
							TimeSpan timeSpan = ServerTime.pCurrentTime - UserInfo.pInstance.CreationDate.Value;
							if (adSection.AdAttributes[k].DaysOlder.HasValue && timeSpan.Days < adSection.AdAttributes[k].DaysOlder)
							{
								continue;
							}
						}
						list.Add(adSection.AdAttributes[k]);
					}
				}
				if (list.Count <= 0)
				{
					break;
				}
				AdSection adSection2 = new AdSection();
				adSection2.FadeTime = adSection.FadeTime;
				adSection2.RefreshRate = adSection.RefreshRate;
				adSection2.UiName = adSection.UiName;
				adSection2.AdAttributes = list.ToArray();
				uiLoginAds.Init(adSection2);
				string[] emptyUIList = _EmptyUIList;
				for (int l = 0; l < emptyUIList.Length; l++)
				{
					if (emptyUIList[l].Equals(uiLoginAds.gameObject.name))
					{
						flag = true;
					}
				}
				break;
			}
		}
		if (mLoginContent.ReleaseNotes != null)
		{
			if (mBtnReleaseNotes != null)
			{
				mBtnReleaseNotes.SetDisabled(!flag);
			}
			_ReleaseNotesUI.Init(mLoginContent.ReleaseNotes, flag);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		mInstance = this;
		if (AvatarData.pInstanceInfo != null)
		{
			AvatarData.pInstanceInfo.mInstance = null;
		}
	}

	protected override void Start()
	{
		base.Start();
		SnChannel.AddToTurnOffPools(_TurnOffPoolInfo);
		SetState(KAUIState.DISABLED);
		KAWidget kAWidget = FindItem("TxtProductVersion");
		if (kAWidget != null)
		{
			kAWidget.SetText("V" + ProductConfig.pProductVersion + ProductConfig.pServerType);
		}
		mTxtEmail = (KAEditBox)FindItem("TxtLoginEmail");
		mTxtPassword = (KAEditBox)FindItem("TxtLoginPassword");
		mTxtAccountRecover = _AccountRecover;
		mBtnReleaseNotes = FindItem("BtnReleaseNotes");
		mBtnGraphics = FindItem("BtnGraphicSettings");
		mBtnLogout = FindItem("BtnLogout");
		mBtnLogin = FindItem("BtnLogin");
		mBtnPlayAsGuest = FindItem("BtnGuest");
		mBtnRegister = FindItem("BtnRegister");
		mBtnPlay = FindItem("BtnPlay");
		mBtnQuitGame = FindItem("BtnQuitGame");
		mBtnQuitGame.SetVisibility(!UtPlatform.IsiOS());
		mBtnForums = FindItem("BtnForums");
		mBtnPlay.SetDisabled(!PrefetchManager.pIsReady);
		mBtnPlayAsGuest.SetDisabled(!PrefetchManager.pIsReady);
		mAniLanguageSelection = FindItem("AniLanguage");
		FindItem("IcoGame").SetVisibility(UtPlatform.IsiOS());
		if (mBtnGraphics != null)
		{
			mBtnGraphics.SetVisibility(!UtPlatform.IsXBox());
		}
		LoadAdsXML();
		mResetAds = true;
		if (UtPlatform.IsStandAlone())
		{
			kAWidget = FindItem("TxtPerformance");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
			}
		}
		if (mParentInfo != null && !mIsGuestUser && mParentInfo.Status == MembershipUserStatus.Success)
		{
			SubscriptionInfo.Init();
			UserInfo.Init();
			SetButtons(loggedIn: true);
		}
		else if (!string.IsNullOrEmpty(ProductConfig.pToken) && !mIsGuestUser)
		{
			WsWebService.SetToken(ProductConfig.pToken);
			SubscriptionInfo.Init();
			UserInfo.Init();
			SetButtons(loggedIn: true);
		}
		else
		{
			SetButtons(loggedIn: false);
		}
		mLocale = UtUtilities.GetLocaleLanguage();
		mMacAddress = GetUniqueId();
		if (WsWebService.pTokenStatus != ApiTokenStatus.TokenValid)
		{
			WsWebService.pTokenStatus = ApiTokenStatus.TokenValid;
		}
		StartCoroutine(WaitForProductConfigFinish());
		if (mParentInfo == null)
		{
			CheckForConnection(ConnectivityErrorLocation.APPLICATION_FIRST_LAUNCH);
		}
		if (UtPlatform.IsMobile())
		{
			AdManager.DisplayAd(AdEventType.MAIN_MENU, AdOption.FULL_SCREEN);
			KAInput.pInstance.ShowInputs(inShow: false);
		}
		else if (UtPlatform.IsWSA())
		{
			KAInput.pInstance.ShowInputs(inShow: false);
		}
		mLanguageText = FindItem("TxtLanguage");
		mLanguageText.SetText(UtUtilities.GetCultureInfo(UtUtilities.GetLocaleLanguage()).Parent.NativeName);
		if (!(_LanguageMenuUI != null))
		{
			return;
		}
		UiLanguageMenu languageMenuUI = _LanguageMenuUI;
		languageMenuUI.onItemSelected = (KAUIDropDownMenu.OnItemSelected)Delegate.Combine(languageMenuUI.onItemSelected, new KAUIDropDownMenu.OnItemSelected(OnLanguageSelected));
		int idx;
		for (idx = 0; idx < ProductConfig.pInstance.Locale.Length; idx++)
		{
			KAWidget kAWidget2 = _LanguageMenuUI.AddWidget("LanguageItem_" + idx);
			try
			{
				string fontBundleName = Array.Find(FontManager.pInstance._FontMappingInfo, (FontManager.FontMappingInfo p) => p._LocaleKeys.Contains(ProductConfig.pInstance.Locale[idx].ID))._BundleInfo[0]._FontBundleName;
				kAWidget2.SetFont(fontBundleName);
			}
			catch (Exception ex)
			{
				UtDebug.LogError("Encountered an error while loading the font bundle for this locale: " + ProductConfig.pInstance.Locale[idx].ID + ". Caught exception: " + ex);
			}
			kAWidget2.SetVisibility(inVisible: true);
			kAWidget2.SetText(UtUtilities.GetCultureInfo(ProductConfig.pInstance.Locale[idx].ID).Parent.NativeName);
			kAWidget2.SetUserDataInt(idx);
		}
	}

	public void OnLanguageSelected(KAWidget widget, KAUIDropDownMenu dropDown)
	{
		if (dropDown is UiLanguageMenu)
		{
			mLanguageText.SetText(widget.GetText());
		}
	}

	private string GetUniqueId()
	{
		return TripleDES.EncryptUnicode(ProductConfig.pApiKey + UtMobileUtilities.GetUniqueID(), ProductConfig.pSecret);
	}

	private void SetLogOut()
	{
		GameUtilities.Logout();
		mUserName = "";
		mPassword = "";
		mChildID = "";
		mTxtEmail.SetText(mUserName);
		mTxtPassword.SetText(mPassword);
		mStoredUserName = "";
		SetState(KAUIState.INTERACTIVE);
		SetButtons(loggedIn: false);
		mResetAds = true;
		mIsGuestUser = true;
		pHasUserJustRegistered = false;
		UnityAnalyticsAgent.pNewUser = true;
	}

	protected virtual void SetButtons(bool loggedIn)
	{
		mBtnLogin.SetVisibility(!loggedIn);
		mBtnRegister.SetVisibility(!loggedIn);
		mTxtAccountRecover.SetVisibility(!loggedIn);
		mBtnLogout.SetVisibility(loggedIn);
		mTxtEmail.SetInteractive(!loggedIn);
		mTxtPassword.SetInteractive(!loggedIn);
		mBtnPlay.SetVisibility(loggedIn);
		mBtnPlayAsGuest.SetVisibility(!loggedIn);
	}

	private IEnumerator WaitForProductConfigFinish()
	{
		while (!ProductConfig.pIsReady)
		{
			yield return 0;
		}
		yield return new WaitForSeconds(0.2f);
		mUserName = "";
		mPassword = "";
		if (UiGraphicSettings.pIsBundleReloadRequired)
		{
			if (!UtPlatform.IsiOS())
			{
				mPrefetchStarted = true;
			}
			mEnablePlayButton = false;
			_GraphicSettingsUI.EnableUIGraphicSettings(enable: false);
			_GraphicSettingsUI.ProcessBundleUpdate();
		}
		else
		{
			PrefetchManager.Init(ignoreGetAssetVersion: false, null, forceLoad: false, OnPrefetchListDownloaded);
			mPrefetchStartTime = DateTime.Now;
			AnalyticAgent.LogFTUEEvent(FTUEEvent.PREFETCH_STARTED);
		}
		if (PlayerPrefs.HasKey("REM_USER_NAME"))
		{
			mUserName = TripleDES.DecryptUnicode(PlayerPrefs.GetString("REM_USER_NAME"), ProductConfig.pSecret);
			mPassword = TripleDES.DecryptUnicode(PlayerPrefs.GetString("REM_PASSWORD"), ProductConfig.pSecret);
			if (string.IsNullOrEmpty(mUserName) || string.IsNullOrEmpty(mPassword))
			{
				PlayerPrefs.DeleteKey("REM_USER_NAME");
				PlayerPrefs.DeleteKey("REM_PASSWORD");
			}
			else
			{
				mStoredUserName = mUserName;
				mTxtEmail.SetText(mUserName);
				mTxtPassword.SetText(mPassword);
			}
		}
		SetState(KAUIState.INTERACTIVE);
		if (mShowRegistrationPage)
		{
			mShowRegistrationPage = false;
			ShowRegistrationPage();
		}
		SetVisibility(inVisible: true);
		if (PlayerPrefs.HasKey("REM_USER_NAME") && UtUtilities.IsConnectedToWWW() && !mShowRegistrationPage && mParentInfo == null)
		{
			OnClick(mBtnLogin);
		}
		else
		{
			_GraphicSettingsUI.ShowCrashWarning();
		}
		RsResourceManager.DestroyLoadScreen();
		List<RemoveSupportReminder> removeSupportReminders = _RemoveSupportReminders;
		if (removeSupportReminders != null && removeSupportReminders.Count > 0)
		{
			_RemoveSupportReminders.RemoveAll((RemoveSupportReminder t) => DateTime.Parse(t._MessageActiveDate, UtUtilities.GetCultureInfo("en-US")) > DateTime.Now);
		}
		if (!UtPlatform.IsSteamMac())
		{
			if (!UtPlatform.IsStandaloneOSX())
			{
				yield break;
			}
			List<RemoveSupportReminder> removeSupportReminders2 = _RemoveSupportReminders;
			if (removeSupportReminders2 == null || removeSupportReminders2.Count <= 0)
			{
				yield break;
			}
		}
		RemoveSupportReminder removeSupportReminder = _RemoveSupportReminders.OrderBy((RemoveSupportReminder t) => DateTime.Parse(t._MessageActiveDate, UtUtilities.GetCultureInfo("en-US")) - DateTime.Now).Last();
		if (!removeSupportReminder._RemindEverySession && !PlayerPrefs.HasKey("RMS_" + removeSupportReminder._MessageActiveDate))
		{
			PlayerPrefs.SetString("RMS_" + removeSupportReminder._MessageActiveDate, "");
			ShowDBLarge(removeSupportReminder._Message.GetLocalizedString(), base.gameObject);
		}
		else if (removeSupportReminder._RemindEverySession)
		{
			ShowDBLarge(removeSupportReminder._Message.GetLocalizedString(), base.gameObject);
		}
	}

	private void LoginParentService()
	{
		mUserName = mTxtEmail.GetText();
		mPassword = mTxtPassword.GetText();
		mStoredUserName = mUserName;
		mIsUserNameEdited = false;
		SetState(KAUIState.DISABLED);
		LoginParent(mUserName, mPassword);
	}

	public void LoginParent(string userName, string password, bool isPostRegistrationLogin = false)
	{
		mUserName = userName;
		mPassword = password;
		KAUICursorManager.SetDefaultCursor("Loading");
		mLoginManager.LoginParent(userName, password, null, "", "", mLocale, ServiceLoginEventHandler, null);
	}

	private void SyncPurchases()
	{
		if (IAPManager.pIsAvailable && IAPManager.pInstance.IsAnyReceiptPending())
		{
			mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: false);
			mKAUIGenericDB.SetText(IAPManager.pInstance._IAPSynchWaitText.GetLocalizedString(), interactive: false);
			mKAUIGenericDB._MessageObject = base.gameObject;
			KAUI.SetExclusive(mKAUIGenericDB);
			IAPManager.pInstance.SyncPendingReceipts(IAPSyncDone);
		}
		else
		{
			LoadNextScene();
		}
	}

	private void IAPSyncDone(bool success)
	{
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mKAUIGenericDB.SetText(success ? IAPManager.pInstance._IAPSynchSuccessText.GetLocalizedString() : IAPManager.pInstance._IAPSynchFailedText.GetLocalizedString(), interactive: false);
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._OKMessage = "LoadNextScene";
	}

	private void LoadNextScene()
	{
		DestroyGenericDialog();
		if (PlayerPrefs.GetInt("SafeAppClose") == 1)
		{
			PlayerPrefs.SetInt("SafeAppClose", 2);
		}
		AnalyticAgent.LogFTUEEvent(FTUEEvent.LOAD_PROFILE_SELECTION);
		RsResourceManager.LoadLevel(GameConfig.GetKeyData("ProfileSelectionScene"));
	}

	public override void OnClick(KAWidget item)
	{
		string text = item.name;
		if (item == mTxtEmail)
		{
			if (mStoredUserName != "" && !mIsUserNameEdited)
			{
				mIsUserNameEdited = true;
			}
		}
		else if (item == mBtnLogin)
		{
			if (UtPlatform.IsMobile())
			{
				AdManager.DisplayAd(AdEventType.LOGIN_TAPPED, AdOption.FULL_SCREEN);
			}
			if (CheckForConnection(ConnectivityErrorLocation.PARENT_LOGIN))
			{
				if (!mTxtEmail.IsValidText() || string.IsNullOrEmpty(mTxtEmail.GetText()) || mTxtEmail.GetText() == mTxtEmail._DefaultText.GetLocalizedString())
				{
					SetState(KAUIState.DISABLED);
					ShowDB(GetStrings()._InvalidLoginUsernameText.GetLocalizedString(), base.gameObject);
				}
				else if (string.IsNullOrEmpty(mTxtPassword.GetText()) || !mTxtPassword.IsValidText() || mTxtPassword.GetText() == mTxtPassword._DefaultText.GetLocalizedString())
				{
					SetState(KAUIState.DISABLED);
					ShowDB(GetStrings()._InvalidPasswordText.GetLocalizedString(), base.gameObject);
				}
				else
				{
					LoginParentService();
				}
			}
		}
		else if (item == mBtnRegister)
		{
			if (UtPlatform.IsMobile())
			{
				AdManager.DisplayAd(AdEventType.REGISTER_TAPPED, AdOption.FULL_SCREEN);
			}
			if (!UtUtilities.IsConnectedToWWW())
			{
				ShowDB(_Strings._NoNetworkRegistrationText.GetLocalizedString(), base.gameObject);
			}
			else
			{
				ShowRegistrationPage();
			}
		}
		else if (item == mTxtAccountRecover)
		{
			if (_ForgotLoginPanelUI != null)
			{
				_ForgotLoginPanelUI.SetVisibility(inVisible: true);
				SetState(KAUIState.DISABLED);
			}
		}
		else if (text == "BtnMoreGames")
		{
			if (CheckForConnection(ConnectivityErrorLocation.LOGIN_SCENE) && UtPlatform.IsMobile())
			{
				AdManager.DisplayAd(AdEventType.MORE_GAMES, AdOption.FULL_SCREEN);
			}
		}
		else if (text == "BtnSettings")
		{
			if (_ServerChangeUI != null)
			{
				SetVisibility(inVisible: false);
				_ServerChangeUI.SetVisibility(inVisible: true);
			}
		}
		else if (item == mBtnQuitGame)
		{
			PopupExitMessageDB();
		}
		else if (item == mBtnPlayAsGuest)
		{
			if (CheckForConnection(ConnectivityErrorLocation.LOGIN_SCENE))
			{
				if (!UtPlatform.IsiOS() && mLocale != UtUtilities.GetLocaleLanguage())
				{
					ItemStoreDataLoader.RemoveAllStoreData();
				}
				LoginGuest();
				AnalyticAgent.LogFTUEEvent(FTUEEvent.PLAY_AS_GUEST);
			}
		}
		else if (item == mBtnPlay)
		{
			if (UtPlatform.IsiOS())
			{
				mCanLoadNextScene = true;
				if (!PrefetchManager.pIsReady && !CheckPrefetchWarning())
				{
					ProcessPrefetch();
				}
			}
			else
			{
				if (mLocale != UtUtilities.GetLocaleLanguage())
				{
					ItemStoreDataLoader.RemoveAllStoreData();
				}
				SyncPurchases();
			}
		}
		else if (item == mBtnLogout)
		{
			SetLogOut();
		}
		else if (item == mBtnReleaseNotes)
		{
			_GraphicSettingsUI.SetVisibility(inVisible: false);
			_ReleaseNotesUI.SetVisibility(inVisible: true);
		}
		else if (item == mBtnGraphics)
		{
			_ReleaseNotesUI.SetVisibility(inVisible: false);
			_GraphicSettingsUI.SetVisibility(inVisible: true);
		}
		else if (item == mBtnForums && !string.IsNullOrEmpty(mLoginContent.ForumURL))
		{
			Application.OpenURL(mLoginContent.ForumURL);
		}
		else if (item == mAniLanguageSelection && _LanguageMenuUI != null)
		{
			_LanguageMenuUI.UpdateState(!_LanguageMenuUI.GetVisibility());
		}
		else if (text == "BtnContactUs" || text == "BtnFAQ")
		{
			if (text == "BtnContactUs")
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
		base.OnClick(item);
	}

	private void OnSupportClosed()
	{
		DragonsZendesk.pOnClosedDelegate = (DragonsZendesk.OnClosed)Delegate.Remove(DragonsZendesk.pOnClosedDelegate, new DragonsZendesk.OnClosed(OnSupportClosed));
		SetInteractive(interactive: true);
	}

	private void OnPrefetchYes()
	{
		PrefetchWarning.pInstance.SaveDefaults(PrefetchManager.pInstance.GetDownloadStates());
		ProcessPrefetch();
	}

	private void OnPrefetchNo()
	{
		EnablePlayButtons(enable: true);
	}

	private bool CheckPrefetchWarning()
	{
		if (PrefetchWarning.pInstance.CheckWarning(PrefetchManager.pInstance.GetDownloadStates()))
		{
			int num = (int)PrefetchManager.pInstance.TotalBundlesSize / 1024;
			string text = _Strings._PrefetchWarningText.GetLocalizedString();
			if (num == 0)
			{
				num = (int)PrefetchManager.pInstance.TotalBundlesSize % 1024;
				text = text.Replace("MB", "KB");
			}
			text = text.Replace("{X}", num.ToString());
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", text, "", base.gameObject, "OnPrefetchYes", "OnPrefetchNo", "", "", inDestroyOnClick: true);
			return true;
		}
		return false;
	}

	private void LoginGuest()
	{
		SetState(KAUIState.DISABLED);
		string validGuestUserName = GetValidGuestUserName();
		KAUICursorManager.SetDefaultCursor("Loading");
		mLoginManager.LoginGuest(ProductConfig.pProductName, validGuestUserName, pLocale, GuestLoginEventHandler, null);
	}

	public static string GetValidGuestUserName()
	{
		string text = pMacAddress;
		KA.Framework.Environment environment = ProductSettings.GetEnvironment();
		KA.Framework.Environment environmentForBundles = ProductConfig.GetEnvironmentForBundles();
		string text2 = string.Empty;
		string text3 = string.Empty;
		int num = -1;
		if (ProductSettings.pInstance != null)
		{
			string contentDataURL = ProductConfig.pInstance.GetContentDataURL("");
			string bundleQuality = ProductConfig.GetBundleQuality();
			bundleQuality = (string.IsNullOrEmpty(bundleQuality) ? "/Mid/" : ("/" + bundleQuality + "/"));
			int num2 = contentDataURL.IndexOf(bundleQuality);
			if (num2 >= 0)
			{
				string text4 = contentDataURL.Substring(0, num2);
				num2 = text4.LastIndexOf("/");
				if (num2 >= 0)
				{
					text3 = text4.Substring(num2 + 1);
				}
			}
			ProductDetails productDetails = ProductSettings.pInstance.GetProductDetails();
			if (productDetails != null)
			{
				text2 = productDetails._ProductVersion;
			}
		}
		if (!string.IsNullOrEmpty(text2) && !string.IsNullOrEmpty(text3))
		{
			num = text2.CompareTo(text3);
		}
		if (num == 0 && environment == KA.Framework.Environment.STAGING && environmentForBundles == KA.Framework.Environment.LIVE)
		{
			text = "dq13" + text + "sl57";
		}
		return text;
	}

	public void GuestLoginEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			pParentInfo = (ParentLoginInfo)inObject;
			if (pParentInfo != null && pParentInfo.ChildList != null && pParentInfo.ChildList.Length != 0)
			{
				ProductConfig.pToken = pParentInfo.ApiToken;
				WsWebService.SetToken(ProductConfig.pToken);
				WsTokenMonitor.pCheckToken = true;
				mIsGuestUser = true;
				UnityAnalyticsAgent.pNewUser = PlayerPrefs.HasKey("FTUE_NEWUSER");
				if (!PlayerPrefs.HasKey("GUEST_ACC_CREATED"))
				{
					PlayerPrefs.SetString("GUEST_ACC_CREATED", "");
					pHasUserJustRegistered = true;
					pGuestUserFirstLaunch = true;
					AnalyticAgent.LogEvent(AnalyticEvent.CLICKED_PLAY_AS_GUEST);
				}
				if (UtPlatform.IsiOS())
				{
					mCanLoadNextScene = true;
					if (!PrefetchManager.pIsReady)
					{
						if (CheckPrefetchWarning())
						{
							KAUICursorManager.SetDefaultCursor();
							SetState(KAUIState.INTERACTIVE);
						}
						else
						{
							ProcessPrefetch();
						}
					}
				}
				else
				{
					SyncPurchases();
				}
			}
			else
			{
				KAUICursorManager.SetDefaultCursor();
				SetState(KAUIState.INTERACTIVE);
				Debug.LogError("WEB SERVICE CALL LoginGuest reture NULL!!!");
			}
			break;
		case WsServiceEvent.ERROR:
			SetState(KAUIState.INTERACTIVE);
			KAUICursorManager.SetDefaultCursor();
			Debug.LogError("WEB SERVICE CALL LoginGuest FAILED!!!");
			break;
		}
	}

	private void ResetAds()
	{
		for (int i = 0; i < _UiList.Length; i++)
		{
			UiLoginAds uiLoginAds = _UiList[i] as UiLoginAds;
			if (uiLoginAds != null)
			{
				uiLoginAds.Reset();
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (ProductConfig.pIsReady && mLoginContent != null && mResetAds)
		{
			ResetAds();
			PopulateUiContent();
			mResetAds = false;
		}
		if (!UtPlatform.IsiOS() && ProductConfig.pIsReady && !mPrefetchStarted && StartPrefetch())
		{
			mPrefetchStarted = true;
			EnableBundleUpdateUI(enable: false);
		}
		if (UtPlatform.IsiOS())
		{
			if (IAPManager.pIsReady && mEnablePlayButton && PrefetchManager.pIsReady)
			{
				mEnablePlayButton = false;
				EnablePlayButtons(enable: true);
				EnableBundleUpdateUI(enable: true);
				LogPrefetchCompleteEvent();
			}
			if (PrefetchManager.pIsReady && mCanLoadNextScene)
			{
				mCanLoadNextScene = false;
				if (mLocale != UtUtilities.GetLocaleLanguage())
				{
					ItemStoreDataLoader.RemoveAllStoreData();
				}
				SyncPurchases();
			}
		}
		else if (mPrefetchStarted && PrefetchManager.pIsReady && mEnablePlayButton && IAPManager.pIsReady && !UiGraphicSettings.pIsBundleReloadRequired)
		{
			mEnablePlayButton = false;
			EnablePlayButtons(enable: true);
			EnableBundleUpdateUI(enable: true);
			LogPrefetchCompleteEvent();
		}
		if (mStoredUserName != "" && mIsUserNameEdited && mStoredUserName != mTxtEmail.GetText())
		{
			mIsUserNameEdited = false;
			mTxtPassword.SetText("");
			mStoredUserName = "";
		}
		if (MemoryManager.pInstance != null && MemoryManager.pInstance.pKAUIGenericDB != null && MemoryManager.pInstance.pKAUIGenericDB != KAUI._GlobalExclusiveUI)
		{
			MemoryManager.pInstance.SendMessage("OnClickedOK", SendMessageOptions.DontRequireReceiver);
		}
		if (!UtPlatform.IsiOS() && mUiGenericDB == null && GetVisibility() && GetState() == KAUIState.INTERACTIVE && Input.GetKeyUp(KeyCode.Escape) && _ReleaseNotesUI.pIsBackBtnAllowed && _LanguageMenuUI.pIsBackBtnAllowed && _GraphicSettingsUI.pIsBackBtnAllowed)
		{
			PopupExitMessageDB();
		}
		if (Input.GetMouseButtonUp(0) && _LanguageMenuUI.IsActive() && KAUIManager.pInstance.pSelectedWidget != mAniLanguageSelection)
		{
			_LanguageMenuUI.UpdateState(isDropped: false);
		}
		if ((IsActive() && UIInput.selection != null && Input.GetKeyDown(KeyCode.Return)) || Input.GetKeyDown(KeyCode.KeypadEnter))
		{
			OnClick(mBtnLogin);
			UIInput.selection.Deselect();
		}
	}

	private void LogPrefetchCompleteEvent()
	{
		if (PlayerPrefs.HasKey("PREFETCH_STARTED") && !PlayerPrefs.HasKey("PREFETCH_COMPLETED"))
		{
			double num = Math.Round((DateTime.Now - mPrefetchStartTime).Duration().TotalMinutes, 2);
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("duration", (int)num);
			AnalyticAgent.LogFTUEEvent(FTUEEvent.PREFETCH_COMPLETED, dictionary);
		}
	}

	public void EnablePlayButtons(bool enable)
	{
		mBtnPlay.SetDisabled(!enable);
		mBtnPlayAsGuest.SetDisabled(!enable);
	}

	public void OnPrefetchComplete()
	{
		mEnablePlayButton = true;
	}

	public void EnableBundleUpdateUI(bool enable)
	{
		_GraphicSettingsUI.EnableUIGraphicSettings(enable);
		mAniLanguageSelection.SetDisabled(!enable);
	}

	private void PopupExitMessageDB()
	{
		SetState(KAUIState.DISABLED);
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSm"));
		mUiGenericDB = gameObject.GetComponent<KAUIGenericDB>();
		mUiGenericDB._MessageObject = base.gameObject;
		mUiGenericDB._YesMessage = "OnExitYes";
		mUiGenericDB._NoMessage = "OnExitNo";
		mUiGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
		mUiGenericDB.SetTextByID(_Strings._ExitConfirmationText._ID, _Strings._ExitConfirmationText._Text, interactive: false);
		KAUI.SetExclusive(mUiGenericDB);
	}

	private void OnExitYes()
	{
		Application.Quit();
	}

	private void OnExitNo()
	{
		SetState(KAUIState.INTERACTIVE);
		DestroyGenericDialog();
	}

	private void ProcessPrefetch()
	{
		if (ProductConfig.pIsReady && StartPrefetch())
		{
			EnableBundleUpdateUI(enable: false);
			EnablePlayButtons(enable: false);
			if (mCanLoadNextScene)
			{
				mBtnLogout.SetDisabled(isDisabled: true);
				mBtnLogin.SetDisabled(isDisabled: true);
				mBtnRegister.SetDisabled(isDisabled: true);
			}
		}
	}

	public void DestroyGenericDialog()
	{
		if (mUiGenericDB != null)
		{
			KAUI.RemoveExclusive(mUiGenericDB);
			UnityEngine.Object.Destroy(mUiGenericDB.gameObject);
			mUiGenericDB = null;
		}
	}

	public void ServiceLoginEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		bool flag = false;
		if (ProductConfig.pInstance.EnableErrorLog.HasValue)
		{
			flag = ProductConfig.pInstance.EnableErrorLog.Value;
		}
		string text = "";
		switch (inType)
		{
		case WsServiceType.LOGIN_PARENT:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				mParentInfo = (ParentLoginInfo)inObject;
				if (mParentInfo != null)
				{
					KAUICursorManager.SetDefaultCursor("Arrow", showHideSystemCursor: false);
					if (mParentInfo.Status == MembershipUserStatus.Success)
					{
						if (mIsGuestUser)
						{
							if (UtPlatform.IsiOS())
							{
								pHasUserJustRegistered = false;
								pGuestUserFirstLaunch = false;
								if (PlayerPrefs.HasKey("GUEST_ACC_CREATED"))
								{
									PlayerPrefs.DeleteKey("GUEST_ACC_CREATED");
								}
							}
							ParentData.Reset();
						}
						ProductConfig.pToken = mParentInfo.ApiToken;
						WsWebService.SetToken(ProductConfig.pToken);
						WsTokenMonitor.pHaveCheckedToken = true;
						WsTokenMonitor.pCheckToken = true;
						mIsUserNameEdited = false;
						mStoredUserName = mUserName;
						PlayerPrefs.SetString("REM_USER_NAME", TripleDES.EncryptUnicode(mUserName, ProductConfig.pSecret));
						PlayerPrefs.SetString("REM_PASSWORD", TripleDES.EncryptUnicode(mPassword, ProductConfig.pSecret));
						mTxtEmail.SetText(mUserName);
						mTxtPassword.SetText(mPassword);
						SetButtons(loggedIn: true);
						RsResourceManager.DestroyLoadScreen();
						SetState(KAUIState.INTERACTIVE);
						SubscriptionInfo.Reset();
						SubscriptionInfo.Init();
						UserInfo.Reset();
						UserInfo.Init();
						mIsGuestUser = false;
						mResetAds = true;
						if (PlayerPrefs.GetString("FTUE_NEWUSER", "") == "")
						{
							AnalyticAgent.LogFTUEEvent(FTUEEvent.LOGIN);
						}
						if (PlayerPrefs.HasKey("FTUE_NEWUSER"))
						{
							UnityAnalyticsAgent.pNewUser = PlayerPrefs.GetString("FTUE_NEWUSER", "") == mUserName;
						}
					}
					else
					{
						SetVisibility(inVisible: true);
						RsResourceManager.DestroyLoadScreen();
						mShowCrashWarningOnDBClose = true;
						switch (mParentInfo.Status)
						{
						case MembershipUserStatus.InvalidUserName:
							ShowDB(GetStrings()._ParentLoginFailedText.GetLocalizedString(), base.gameObject);
							break;
						case MembershipUserStatus.InvalidPassword:
							ShowDB(GetStrings()._ParentLoginFailedText.GetLocalizedString(), base.gameObject);
							break;
						default:
							if (flag)
							{
								string[] array = new string[6];
								int num = (int)inType;
								array[0] = num.ToString();
								array[1] = ":";
								num = (int)inEvent;
								array[2] = num.ToString();
								array[3] = ":";
								num = (int)mParentInfo.Status;
								array[4] = num.ToString();
								array[5] = " ";
								text = string.Concat(array);
							}
							ShowDB(GetStrings()._ErrorText.GetLocalizedString() + " " + text, base.gameObject);
							break;
						}
					}
					if (!mShowCrashWarningOnDBClose)
					{
						_GraphicSettingsUI.ShowCrashWarning();
					}
				}
				else
				{
					if (flag)
					{
						int num = (int)inType;
						string text3 = num.ToString();
						num = (int)inEvent;
						text = text3 + ":" + num + ":return null ";
					}
					KAUICursorManager.SetDefaultCursor("Arrow", showHideSystemCursor: false);
					ShowDB(GetStrings()._ErrorText.GetLocalizedString() + " " + text, base.gameObject);
					Debug.LogError("WEB SERVICE CALL LoginParent return NULL!!!");
					mShowCrashWarningOnDBClose = true;
				}
				break;
			case WsServiceEvent.ERROR:
				if (flag)
				{
					int num = (int)inType;
					string text2 = num.ToString();
					num = (int)inEvent;
					text = text2 + ":" + num + " ";
				}
				SetState(KAUIState.INTERACTIVE);
				KAUICursorManager.SetDefaultCursor("Arrow", showHideSystemCursor: false);
				Debug.LogError("WEB SERVICE CALL LoginParent FAILED!!!");
				ShowDB(GetStrings()._ErrorText.GetLocalizedString() + " " + text, base.gameObject);
				mShowCrashWarningOnDBClose = true;
				break;
			}
			break;
		case WsServiceType.LOG_OFF_PARENT:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				if (inObject != null)
				{
					GUI.enabled = true;
				}
				break;
			case WsServiceEvent.ERROR:
				GUI.enabled = true;
				UtDebug.LogError("WEB SERVICE CALL LogOffParent FAILED!!!");
				break;
			}
			break;
		}
	}

	public void OnSendActivationReminderEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType != WsServiceType.SEND_ACCOUNT_ACTIVATION_REMINDER)
		{
			return;
		}
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			if (inObject == null)
			{
				Debug.LogError("SEND_ACCOUNT_ACTIVATION_REMINDER   COMPLeted ......  inObject = NULL");
			}
			break;
		case WsServiceEvent.ERROR:
			Debug.LogError("SEND_ACCOUNT_ACTIVATION_REMINDER   ERROR ...");
			break;
		}
	}

	private void OnEnable()
	{
		SetState(KAUIState.INTERACTIVE);
	}

	public static void ShowDB(string inMessage, GameObject inMessageObject)
	{
		GameUtilities.DisplayOKMessage("PfKAUIGenericDB", inMessage, inMessageObject, "OnDBClose");
	}

	public static void ShowDBLarge(string inMessage, GameObject inMessageObject)
	{
		GameUtilities.DisplayOKMessage("PfKAUIGenericDBLg", inMessage, inMessageObject, "OnDBClose");
	}

	public void OnDBClose()
	{
		SetState(KAUIState.INTERACTIVE);
		if (mShowCrashWarningOnDBClose)
		{
			mShowCrashWarningOnDBClose = false;
			_GraphicSettingsUI.ShowCrashWarning();
		}
	}

	public static LoginStrings GetStrings()
	{
		return pInstance._Strings;
	}

	public void ShowRegistrationPage()
	{
		if (!(_RegisterUI == null))
		{
			SetState(KAUIState.DISABLED);
			_RegisterUI.SetVisibility(inVisible: true);
			_RegisterUI.SetState(KAUIState.INTERACTIVE);
			_RegisterUI.OnAgeToggled(isParent: true);
		}
	}

	public bool CheckForConnection(ConnectivityErrorLocation location)
	{
		if (!UtUtilities.IsConnectedToWWW())
		{
			mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: false);
			mKAUIGenericDB._MessageObject = base.gameObject;
			KAUI.SetExclusive(mKAUIGenericDB);
			switch (location)
			{
			case ConnectivityErrorLocation.APPLICATION_FIRST_LAUNCH:
				mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
				mKAUIGenericDB._OKMessage = "OnRetry";
				mKAUIGenericDB.SetTextByID(_Strings._ConnectivityErrorInitialText._ID, _Strings._ConnectivityErrorInitialText._Text, interactive: false);
				break;
			case ConnectivityErrorLocation.PARENT_LOGIN:
			case ConnectivityErrorLocation.LOGIN_SCENE:
				mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
				mKAUIGenericDB._OKMessage = "OnRetry";
				mKAUIGenericDB.SetTextByID(_Strings._NoNetworkPlayText._ID, _Strings._NoNetworkPlayText._Text, interactive: false);
				break;
			case ConnectivityErrorLocation.REGISTER_SCENE:
				mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
				mKAUIGenericDB._OKMessage = "OnRetry";
				mKAUIGenericDB.SetTextByID(_Strings._NoNetworkText._ID, _Strings._NoNetworkPlayText._Text, interactive: false);
				break;
			}
			return false;
		}
		return true;
	}

	private void OnRetry()
	{
		KAUI.RemoveExclusive(mKAUIGenericDB);
		mKAUIGenericDB.Destroy();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		PrefetchManager.Kill();
		mInstance = null;
		PrefetchWarning.Kill();
		_LanguageMenuUI.onItemSelected = null;
	}

	private bool IsAppLaunch()
	{
		return RsResourceManager.pLastLevel.Equals("Startup", StringComparison.OrdinalIgnoreCase);
	}

	public bool StartPrefetch()
	{
		if (PrefetchManager.pInstance.pState == PrefetchManager.State.WAIT_FOR_START)
		{
			if (UtPlatform.IsiOS() && ClearPreloadBundles)
			{
				PrefetchManager.pInstance.ClearPreloadBundles(null);
			}
			PrefetchManager.StartPrefetch();
			PrefetchManager.ShowDownloadProgress();
			return true;
		}
		return false;
	}

	private void OnPrefetchListDownloaded()
	{
		if (UtPlatform.IsiOS() && !CheckPrefetchWarning())
		{
			ProcessPrefetch();
		}
	}
}
