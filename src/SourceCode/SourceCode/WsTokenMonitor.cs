using System;
using UnityEngine;

public class WsTokenMonitor : KAMonoBase
{
	public delegate void TokenStatus(ApiTokenStatus inStatus);

	private static WsTokenMonitor mInstance = null;

	private static float mLastCheckTime = 0f;

	private static bool mSilentTokenCheckInProgress = false;

	private static bool mMonitorToken = true;

	private static bool mRefreshServerTime = false;

	private static bool mReloadSceneAllowed = true;

	private static bool mCheckToken = false;

	private static bool mHaveCheckedToken = false;

	public float _TokenCheckSeconds = 120f;

	public float _TokenRecheckSeconds = 30f;

	public int _TokenRecheckThreshold = 2;

	public LocaleString _FBLoginErrorText = new LocaleString("An error occurred.  Try Again?");

	private int mFailedTokenCheckCount;

	public string _TokenCheckStartSceneName = "ProfileSelectionDO";

	private float mOnPauseReloadingThreshold = 120f;

	private static DateTime mTimeWhenItWasMinimized;

	private bool mCachedReloadSceneAllowed;

	public const string SafeAppCloseKey = "SafeAppClose";

	private KAUIGenericDB mKAUIGenericDB;

	public static bool pReloadSceneAllowed
	{
		get
		{
			return mReloadSceneAllowed;
		}
		set
		{
			mReloadSceneAllowed = value;
			mTimeWhenItWasMinimized = DateTime.Now;
		}
	}

	public static bool pCheckToken
	{
		get
		{
			return mCheckToken;
		}
		set
		{
			mCheckToken = value;
		}
	}

	public static bool pHaveCheckedToken
	{
		get
		{
			return mHaveCheckedToken;
		}
		set
		{
			mHaveCheckedToken = value;
		}
	}

	public static event TokenStatus OnTokenStatus;

	private void Awake()
	{
		if (mInstance == null)
		{
			mInstance = this;
			UnityEngine.Object.DontDestroyOnLoad(this);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		StartTokenTimer();
		AdManager pInstance = AdManager.pInstance;
		pInstance.OnAppPaused = (Action<bool>)Delegate.Combine(pInstance.OnAppPaused, new Action<bool>(OnAdPaused));
	}

	public static void StartTokenTimer()
	{
		mMonitorToken = true;
		mLastCheckTime = Time.time;
	}

	private void OnAdPaused(bool isAdShown)
	{
		if (isAdShown && mReloadSceneAllowed)
		{
			mCachedReloadSceneAllowed = true;
			mReloadSceneAllowed = false;
		}
	}

	private void Update()
	{
		if (ProductConfig.pIsReady && mMonitorToken && Time.time > mLastCheckTime + (mSilentTokenCheckInProgress ? _TokenRecheckSeconds : _TokenCheckSeconds))
		{
			CheckToken();
			mLastCheckTime = Time.time;
		}
		if (mRefreshServerTime && UtUtilities.IsConnectedToWWW())
		{
			mRefreshServerTime = false;
			ServerTime.Init(inForceInit: true);
		}
	}

	public static void ForceCheckToken()
	{
		if (mCheckToken)
		{
			mHaveCheckedToken = false;
			WsWebService.IsValidApiToken(ServiceEventHandler, null);
		}
		else
		{
			mHaveCheckedToken = true;
		}
		mLastCheckTime = Time.time;
	}

	public static void CheckToken()
	{
		if (mCheckToken)
		{
			WsWebService.IsValidApiToken(ServiceEventHandler, null);
		}
		else
		{
			mHaveCheckedToken = true;
		}
	}

	private static void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType != WsServiceType.IS_VALID_API_TOKEN)
		{
			return;
		}
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			if (inObject == null)
			{
				UtDebug.LogError("WEB SERVICE CALL IS_VALID_TOKEN RETURNED NO DATA!!!");
				UiErrorHandler.ShowErrorUI(UiErrorHandler.ErrorMessageType.VALIDATE_TOKEN_FAILED);
				break;
			}
			ApiTokenStatus apiTokenStatus = (ApiTokenStatus)inObject;
			WsTokenMonitor.OnTokenStatus?.Invoke(apiTokenStatus);
			switch (apiTokenStatus)
			{
			case ApiTokenStatus.UserLoggedInFromAnotherLocation:
				mHaveCheckedToken = false;
				mInstance.OnUserLoggedInFromAnotherLocation();
				mMonitorToken = false;
				break;
			default:
				mHaveCheckedToken = false;
				mInstance.OnTokenExpired();
				mMonitorToken = false;
				break;
			case ApiTokenStatus.TokenValid:
				mHaveCheckedToken = true;
				break;
			}
			break;
		}
		case WsServiceEvent.ERROR:
			UtDebug.LogError("Web service call IS_VALID_TOKEN failed, retrying...");
			WsTokenMonitor.OnTokenStatus?.Invoke(ApiTokenStatus.TokenNotFound);
			if (!mSilentTokenCheckInProgress && !(UiErrorHandler.pInstance != null))
			{
				mSilentTokenCheckInProgress = true;
				OnTokenStatus += mInstance.OnTokenReChecked;
			}
			break;
		}
	}

	private void OnTokenReChecked(ApiTokenStatus inStatus)
	{
		switch (inStatus)
		{
		case ApiTokenStatus.TokenValid:
			UtDebug.Log("Token check successful, resuming game.");
			mSilentTokenCheckInProgress = false;
			mFailedTokenCheckCount = 0;
			OnTokenStatus -= OnTokenReChecked;
			return;
		case ApiTokenStatus.UserLoggedInFromAnotherLocation:
			mHaveCheckedToken = false;
			mInstance.OnUserLoggedInFromAnotherLocation();
			mMonitorToken = false;
			mSilentTokenCheckInProgress = false;
			mFailedTokenCheckCount = 0;
			OnTokenStatus -= OnTokenReChecked;
			return;
		case ApiTokenStatus.TokenExpired:
			mHaveCheckedToken = false;
			mInstance.OnTokenExpired();
			mMonitorToken = false;
			mSilentTokenCheckInProgress = false;
			mFailedTokenCheckCount = 0;
			OnTokenStatus -= OnTokenReChecked;
			return;
		}
		mFailedTokenCheckCount++;
		if (mFailedTokenCheckCount >= mInstance._TokenRecheckThreshold)
		{
			UtDebug.LogError("Failed to re-check token!");
			OnTokenStatus -= OnTokenReChecked;
			mLastCheckTime = Time.time;
			mSilentTokenCheckInProgress = false;
			mFailedTokenCheckCount = 0;
			UiServerErrorHandler.pPreviousState = AvAvatar.pState;
			UiErrorHandler.ShowErrorUI(UiErrorHandler.ErrorMessageType.VALIDATE_TOKEN_FAILED);
			mMonitorToken = false;
		}
	}

	private void OnTokenExpired()
	{
		UtDebug.Log("---- Token Expired -----");
		if ((bool)AvAvatar.pObject)
		{
			AvAvatar.pState = AvAvatarState.PAUSED;
		}
		ObClickable.pGlobalActive = false;
		KAUICursorManager.SetDefaultCursor("Arrow");
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDB"));
		mKAUIGenericDB = gameObject.GetComponent<KAUIGenericDB>();
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._OKMessage = "OnOK";
		mKAUIGenericDB._TextMessage = "OnOK";
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mKAUIGenericDB.SetText(ProductConfig.pInstance.TokenExpiredText.GetLocalizedString(), interactive: true);
		KAUI.SetExclusive(mKAUIGenericDB, new Color(1f, 1f, 1f, 0.5f), updatePriority: true);
	}

	private void OnUserLoggedInFromAnotherLocation()
	{
		UtDebug.Log("Logged in from another location");
		if ((bool)AvAvatar.pObject)
		{
			AvAvatar.pState = AvAvatarState.PAUSED;
		}
		ObClickable.pGlobalActive = false;
		KAUICursorManager.SetDefaultCursor("Arrow");
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDB"));
		mKAUIGenericDB = gameObject.GetComponent<KAUIGenericDB>();
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._OKMessage = "OnOK";
		mKAUIGenericDB._TextMessage = "OnOK";
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mKAUIGenericDB.SetText(ProductConfig.pInstance.LoginFromOtherLocationText.GetLocalizedString(), interactive: true);
		KAUI.SetExclusive(mKAUIGenericDB, new Color(1f, 1f, 1f, 0.5f), updatePriority: true);
	}

	private void OnOK()
	{
		if (mKAUIGenericDB != null)
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
		RsResourceManager.DestroyLoadScreen();
		mMonitorToken = true;
		GameUtilities.LoadLoginLevel(showRegstration: false, fullReset: false);
	}

	public void ServiceLoginEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.LOGIN_PARENT:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				ParentLoginInfo parentLoginInfo = (ParentLoginInfo)inObject;
				if (parentLoginInfo != null && parentLoginInfo.Status == MembershipUserStatus.Success)
				{
					ProductConfig.pToken = parentLoginInfo.ApiToken;
					WsWebService.LoginChild(ProductConfig.pToken, UserInfo.pInstance.UserID, UtUtilities.GetLocaleLanguage(), ServiceLoginEventHandler, null);
					if (PlayfabManager<PlayFabManagerDO>.Instance != null)
					{
						PlayfabManager<PlayFabManagerDO>.Instance.ParentToken = parentLoginInfo.ApiToken;
						PlayfabManager<PlayFabManagerDO>.Instance.LoginUser(parentLoginInfo.UserID, guest: false, null);
					}
				}
				else
				{
					ShowError();
				}
				break;
			}
			case WsServiceEvent.ERROR:
				ShowError();
				break;
			}
			break;
		case WsServiceType.LOGIN_CHILD:
		{
			if (inEvent != WsServiceEvent.COMPLETE && inEvent != WsServiceEvent.ERROR)
			{
				break;
			}
			string text = (string)inObject;
			if (!string.IsNullOrEmpty(text))
			{
				if (mKAUIGenericDB != null)
				{
					KAUI.RemoveExclusive(mKAUIGenericDB);
					UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
					mKAUIGenericDB = null;
				}
				WsWebService.SetToken(text);
				mMonitorToken = true;
				KAUICursorManager.SetDefaultCursor("Arrow");
			}
			else
			{
				ShowError();
			}
			break;
		}
		}
	}

	public void ShowError()
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		mKAUIGenericDB.SetInteractive(interactive: true);
		mKAUIGenericDB.SetTextByID(_FBLoginErrorText._ID, _FBLoginErrorText._Text, interactive: true);
	}

	private void OnApplicationQuit()
	{
		ResetSafeCrashData();
	}

	private void ResetSafeCrashData()
	{
		if (!UtPlatform.IsStandAlone() && PlayerPrefs.GetInt("SafeAppClose") == 2)
		{
			PlayerPrefs.SetInt("SafeAppClose", 0);
		}
	}

	private void OnApplicationPause(bool isPause)
	{
		if (isPause)
		{
			ResetSafeCrashData();
		}
		bool flag = mReloadSceneAllowed && GameDataConfig.CanReloadOnMinimize();
		if (!isPause)
		{
			if (ServerTime.pIsReady)
			{
				mRefreshServerTime = true;
			}
			if (RsResourceManager.pForceDisableLevelReload)
			{
				RsResourceManager.pForceDisableLevelReload = false;
				return;
			}
			double totalSeconds = (DateTime.Now - mTimeWhenItWasMinimized).TotalSeconds;
			if (flag && totalSeconds > (double)mOnPauseReloadingThreshold)
			{
				AvAvatar.RestoreAvatar();
				if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pMountedState)
				{
					SanctuaryManager.pCurPetInstance.OnFlyDismount(AvAvatar.pObject);
				}
				RsResourceManager.LoadLevel(RsResourceManager.pCurrentLevel);
			}
			if (mCachedReloadSceneAllowed)
			{
				mReloadSceneAllowed = true;
				mCachedReloadSceneAllowed = false;
			}
		}
		else
		{
			mTimeWhenItWasMinimized = DateTime.Now;
		}
	}

	private void OnDestroy()
	{
		AdManager pInstance = AdManager.pInstance;
		pInstance.OnAppPaused = (Action<bool>)Delegate.Remove(pInstance.OnAppPaused, new Action<bool>(OnAdPaused));
	}
}
