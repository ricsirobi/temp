using System;
using System.Collections;
using System.Net.NetworkInformation;
using System.Timers;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectivityMonitor : MonoBehaviour
{
	public delegate void OnInternetAvailable();

	public delegate void OnInternetUnavailable();

	public string[] _ScenesToIgnore = new string[2] { "Startup", "LoginDM" };

	public float _ConnectionTimeout = 5f;

	private float mCurrentTime;

	private bool mIsConnected = true;

	private static bool mCheckingForConnectivity;

	private AvAvatarState mCachedState = AvAvatarState.IDLE;

	private bool mConnectionLostErrorMsg;

	private bool mNoConnectionAtStartup = true;

	private bool mEnableCheck = true;

	private NetworkInterface[] mOldNetworkInterfaces;

	private Timer mConnectionCheckTimer;

	public bool _InternetOn = true;

	private UiErrorHandler mErrorDB;

	public LocaleString _NoNetworkText = new LocaleString("Could not connect to internet. Please check your connection and try again.");

	public LocaleString _NoNetworkTitleText = new LocaleString("No Internet");

	public LocaleString _NetworkCheckText = new LocaleString("Checking Connection....");

	public LocaleString _RetryButtonText = new LocaleString("Retry");

	public LocaleString _ReLoginText = new LocaleString("Unable to load data! Please login again.");

	public LocaleString _ReLoginTitleText = new LocaleString("Error Retry");

	public LocaleString _NoConnectionAtStartupText = new LocaleString("You need Internet Connection to play!");

	public static OnInternetAvailable OnConnectedHandlers;

	public static OnInternetUnavailable OnDisconnectedHandlers;

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		mCurrentTime = _ConnectionTimeout;
		mIsConnected = true;
	}

	private void Start()
	{
		mIsConnected = UtUtilities.IsConnectedToWWW();
		mConnectionCheckTimer = new Timer();
		mConnectionCheckTimer.Interval = _ConnectionTimeout * 1000f;
		mConnectionCheckTimer.Elapsed += UpdateNetworkStatus;
		mConnectionCheckTimer.Enabled = mEnableCheck;
		mOldNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene newScene, LoadSceneMode loadSceneMode)
	{
		mIsConnected = !UtUtilities.IsConnectedToWWW() || !_InternetOn;
		mErrorDB = null;
		if (!string.IsNullOrEmpty(Array.Find(_ScenesToIgnore, (string element) => element.Equals(RsResourceManager.pCurrentLevel, StringComparison.OrdinalIgnoreCase))))
		{
			mEnableCheck = false;
		}
		else
		{
			mEnableCheck = true;
		}
		if (mConnectionCheckTimer != null)
		{
			mConnectionCheckTimer.Enabled = mEnableCheck;
		}
	}

	private void Update()
	{
		if (!mEnableCheck)
		{
			return;
		}
		mCurrentTime += Time.deltaTime;
		if (!(mCurrentTime >= _ConnectionTimeout))
		{
			return;
		}
		mCurrentTime = 0f;
		bool flag = UtUtilities.IsConnectedToWWW() && _InternetOn;
		if (mIsConnected != flag || mCheckingForConnectivity)
		{
			if (flag)
			{
				if (OnConnectedHandlers != null)
				{
					mCheckingForConnectivity = false;
					OnConnectedHandlers();
					mIsConnected = flag;
				}
				else
				{
					mIsConnected = flag;
					OnConnectionAvailable();
				}
			}
			else if (OnDisconnectedHandlers != null)
			{
				OnDisconnectedHandlers();
				mIsConnected = flag;
			}
			else if (OnConnectionUnavailable())
			{
				mIsConnected = flag;
			}
		}
		else if (!flag && RsResourceManager.pLevelLoadingScreen && !mConnectionLostErrorMsg)
		{
			LoadingScreenError();
		}
		if (flag)
		{
			mNoConnectionAtStartup = false;
		}
	}

	private WWW GetTestPingRequest()
	{
		string text = ProductConfig.pInstance.ContentServerURL;
		if (text.EndsWith("/"))
		{
			text = text.Substring(0, text.Length - 1);
		}
		return new WWW(text);
	}

	public static void ForceCheck()
	{
		mCheckingForConnectivity = true;
	}

	public static void AddConnectionHandler(OnInternetAvailable connectedHandler)
	{
		OnConnectedHandlers = (OnInternetAvailable)Delegate.Combine(OnConnectedHandlers, connectedHandler);
	}

	public static void AddDisconnectionHandler(OnInternetUnavailable disconnectedHandler)
	{
		OnDisconnectedHandlers = (OnInternetUnavailable)Delegate.Combine(OnDisconnectedHandlers, disconnectedHandler);
	}

	public static void RemoveConnectionHandler(OnInternetAvailable connectedHandler)
	{
		OnConnectedHandlers = (OnInternetAvailable)Delegate.Remove(OnConnectedHandlers, connectedHandler);
		ForceCheck();
	}

	public static void RemoveDisconnectionHandler(OnInternetUnavailable disconnectedHandler)
	{
		OnDisconnectedHandlers = (OnInternetUnavailable)Delegate.Remove(OnDisconnectedHandlers, disconnectedHandler);
		ForceCheck();
	}

	public void OnConnectionAvailable()
	{
		mCheckingForConnectivity = false;
		if (mErrorDB != null && mErrorDB.pErrorMessageType == UiErrorHandler.ErrorMessageType.CONNECTION_UNAVAILABLE)
		{
			mErrorDB.ExitDB();
			AvAvatar.pState = mCachedState;
			mErrorDB = null;
		}
	}

	private void LoadingScreenError()
	{
		if (PrefetchManager.pInstance == null)
		{
			mErrorDB = UiErrorHandler.ShowErrorUI(UiErrorHandler.ErrorMessageType.PREFETCH_ERROR);
			if (mErrorDB != null)
			{
				mConnectionLostErrorMsg = true;
				mErrorDB.SetText(_ReLoginText.GetLocalizedString());
				mErrorDB.pOnUiErrorActionEventHandler = ForceLogin;
			}
		}
	}

	public void ForceLogin()
	{
		mConnectionLostErrorMsg = false;
		if (mErrorDB != null)
		{
			mErrorDB.ExitDB();
		}
		mErrorDB = null;
		RsResourceManager.DestroyLoadScreen();
		if (UtPlatform.IsMobile() || UtPlatform.IsWSA())
		{
			StartCoroutine(LoadLoginScene());
		}
	}

	private IEnumerator LoadLoginScene()
	{
		if (RsResourceManager.pLevelLoading)
		{
			yield return 0;
		}
		GameUtilities.LoadLoginLevel(showRegstration: false, fullReset: false);
	}

	public bool OnConnectionUnavailable()
	{
		UtDebug.LogWarning("No internet connection available!");
		mCheckingForConnectivity = false;
		if (mIsConnected)
		{
			mCachedState = AvAvatar.pState;
			AvAvatar.pState = AvAvatarState.PAUSED;
		}
		if (RsResourceManager.pLevelLoadingScreen && !mConnectionLostErrorMsg)
		{
			LoadingScreenError();
		}
		if (mErrorDB == null)
		{
			mErrorDB = UiErrorHandler.ShowErrorUI(UiErrorHandler.ErrorMessageType.CONNECTION_UNAVAILABLE, mNoConnectionAtStartup ? 10 : (-1));
		}
		if (mErrorDB != null && mErrorDB.pErrorMessageType == UiErrorHandler.ErrorMessageType.CONNECTION_UNAVAILABLE)
		{
			mErrorDB.SetButtonVisibility(visible: true);
			mErrorDB.pOnUiErrorActionEventHandler = OnRetry;
			mErrorDB.pOnUiErrorExitEventHandler = OnMsgExit;
			mErrorDB.SetText(mNoConnectionAtStartup ? _NoConnectionAtStartupText.GetLocalizedString() : _NoNetworkText.GetLocalizedString());
		}
		return true;
	}

	public void OnMsgExit()
	{
		if (mErrorDB != null)
		{
			mErrorDB.ExitDB();
		}
		mErrorDB = null;
		AvAvatar.pState = mCachedState;
	}

	public void OnRetry()
	{
		mErrorDB.SetButtonVisibility(visible: false);
		mErrorDB.SetText(_NetworkCheckText.GetLocalizedString());
		mErrorDB.pOnUiErrorActionEventHandler = null;
		mCheckingForConnectivity = true;
		StartCoroutine(TestConnection());
	}

	private void UpdateNetworkStatus(object source, ElapsedEventArgs e)
	{
		NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
		bool flag = false;
		if (allNetworkInterfaces == null || mOldNetworkInterfaces == null)
		{
			if (allNetworkInterfaces != mOldNetworkInterfaces)
			{
				flag = true;
			}
		}
		else if (allNetworkInterfaces.Length != mOldNetworkInterfaces.Length)
		{
			flag = true;
		}
		else
		{
			for (int i = 0; i < mOldNetworkInterfaces.Length; i++)
			{
				if (mOldNetworkInterfaces[i].Name != allNetworkInterfaces[i].Name || mOldNetworkInterfaces[i].OperationalStatus != allNetworkInterfaces[i].OperationalStatus)
				{
					flag = true;
					break;
				}
			}
		}
		mOldNetworkInterfaces = allNetworkInterfaces;
		if (flag)
		{
			if (mErrorDB != null && mErrorDB.GetVisibility())
			{
				mErrorDB.SetButtonVisibility(visible: false);
				mErrorDB.SetText(_NetworkCheckText.GetLocalizedString());
				mErrorDB.pOnUiErrorActionEventHandler = null;
			}
			StartCoroutine(TestConnection());
		}
	}

	private IEnumerator TestConnection()
	{
		if (ProductConfig.pInstance != null)
		{
			bool prevNetworkCheck = mEnableCheck;
			Timer timer = mConnectionCheckTimer;
			ConnectivityMonitor connectivityMonitor = this;
			bool flag = false;
			connectivityMonitor.mEnableCheck = false;
			timer.Enabled = flag;
			WWW testPing = GetTestPingRequest();
			yield return testPing;
			UtUtilities._ConnectedToInternet = string.IsNullOrEmpty(testPing.error);
			mConnectionCheckTimer.Enabled = (mEnableCheck = prevNetworkCheck);
			mCheckingForConnectivity = true;
		}
	}
}
