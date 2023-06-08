using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using KA.Framework;
using UnityEngine;

public class PrefetchManager : KAUI
{
	public class BundleDownloadProgress
	{
		public float _Progress;

		public string _URL;

		public int _RetryCount;

		public float _Size;
	}

	public enum ErrorType
	{
		NONE,
		BUNDLE_CACHED_FAILED
	}

	public enum State
	{
		NONE,
		PREFETCH_DATA_DOWNLOADING,
		WAIT_FOR_START,
		BUNDLE_DOWNLOADING,
		WAIT,
		DONE
	}

	[XmlRoot]
	public class PrefetchList
	{
		[XmlArray]
		public string[] BundleNames;

		[XmlArray]
		public string[] DownloadStates;
	}

	private const uint LOG_MASK = 8u;

	private const float IDLE_TIME_OUT = 20f;

	public string _PrefetchListXml = "RS_DATA/PrefetchList.xml";

	private static List<string> mPrefetchXMLList;

	private static int mPrefetchCount;

	public bool _ShowDownloadProgressAlways;

	public int _BundleDownloadCount = 4;

	public int _FailedBundlesRetryCount = 3;

	public float _SamplingTimeInSecs = 1f;

	public float _FirstSamplingTimeInSecs = 1f;

	public LocaleString _OfflineFailedText = new LocaleString("You lost your Internet connection! Please connect to the Internet to continue downloading.");

	public LocaleString _OnlineFailedText = new LocaleString("You couldn't connect to the School of Dragons. Please try to connect again later.");

	public LocaleString _SkipText = new LocaleString("You couldn't download! Continue offline, or try again later.");

	public Color _MaskColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	public GameObject _Splash;

	public LocaleString _MinuteText = new LocaleString("minute");

	public LocaleString _MinutesText = new LocaleString("minutes");

	public LocaleString _DownLoadComplete = new LocaleString("Download Complete");

	private bool mPaused;

	private int mCompletedCount = -1;

	private int mDownloadBundleCount;

	private PrefetchList mPrefetchList;

	private KAWidget mProgressBar;

	private KAWidget mMessageGroup;

	private KAWidget mTimeRemainingMsgGroup;

	private KAWidget mDownloadMsg;

	private KAWidget mTxtPercentage;

	private AssetVersion[] mCurrentAssetVersionArray;

	private int mLastBundleIdx;

	private KAUIGenericDB mGenericDB;

	private List<BundleDownloadProgress> mBundlesDownloading;

	private ErrorType mErrorType;

	private string mProgressText = "";

	private UITexture mSplash;

	private State mState;

	private float mLastCallbackTime = float.MaxValue;

	private LocaleString[] mLoadingTexts;

	private int mLoadTextIndex;

	private float mDelayBwLoadTextsInSecs;

	private static PrefetchManager mInstance;

	private float mPrevLoadedBundlesSize;

	private float mSamplingTimer;

	private float mCurrDownloadingBundlesSize;

	private float mDownloadedBundlesSize;

	private int mBundleLoadCallCount;

	private bool mFinishedMovie;

	private bool mNetworkDisconnect;

	private bool mReloadBundles;

	private RsAssetLoader mPreloader;

	private List<string> mPreloadBundles;

	private Action mReloadEventHandler;

	public float _SplashTime = 10f;

	public float _LoadScreenTime = 3f;

	private KAWidget mBackground;

	private float mSplashTime;

	private float mLoadScreenTime;

	private bool mDownloadNewBundles;

	private List<string> mDownloadCompleteAssetList = new List<string>();

	public bool pPaused
	{
		get
		{
			return mPaused;
		}
		set
		{
			if (value != mPaused && mCompletedCount < mDownloadBundleCount)
			{
				mLastCallbackTime = Time.time + 20f;
				mPaused = value;
				if (!mPaused)
				{
					DownloadBundles();
				}
			}
		}
	}

	public int pCompletedCount => mCompletedCount;

	public int pDownloadBundleCount => mDownloadBundleCount;

	public float TotalBundlesSize { get; set; }

	public static bool pIsReady
	{
		get
		{
			if (mInstance != null && mInstance.mState == State.DONE && mInstance.mFinishedMovie)
			{
				if (mInstance.mPreloader != null)
				{
					return mInstance.mPreloader.pIsReady;
				}
				return true;
			}
			return false;
		}
	}

	public static bool pIsVersionDownloadComplete
	{
		get
		{
			if (mInstance != null)
			{
				return mInstance.mState > State.PREFETCH_DATA_DOWNLOADING;
			}
			return false;
		}
	}

	public static bool pError => mInstance.mErrorType != ErrorType.NONE;

	public State pState
	{
		get
		{
			return mState;
		}
		set
		{
			mState = value;
			if (mState == State.DONE)
			{
				SetVisibility(_ShowDownloadProgressAlways);
				ShowMovie();
				if (mReloadBundles)
				{
					mReloadBundles = false;
					PostPrefetch();
				}
			}
		}
	}

	public static PrefetchManager pInstance => mInstance;

	public static void Init(bool ignoreGetAssetVersion = false, string prefetchXML = null, bool forceLoad = false, Action eventHandler = null)
	{
		if ((!pIsReady || forceLoad) && mInstance != null)
		{
			if (!string.IsNullOrEmpty(prefetchXML))
			{
				mInstance._PrefetchListXml = prefetchXML;
			}
			mPrefetchCount = 0;
			mInstance.StartDownloadingPrefetchData(eventHandler);
		}
	}

	public static void Init(List<string> prefetchXMLList)
	{
		if (!pIsReady && mInstance != null)
		{
			mPrefetchXMLList = prefetchXMLList;
			mPrefetchCount = mPrefetchXMLList.Count;
			mInstance.StartDownloadingPrefetchData(null);
		}
	}

	public static void StartPrefetch()
	{
		if (mInstance != null && mInstance.pState == State.WAIT_FOR_START)
		{
			mInstance.pPaused = false;
			if (mInstance.mDownloadBundleCount == 0)
			{
				mInstance.pState = State.DONE;
			}
			else
			{
				mInstance.DownloadBundles();
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (mInstance == null)
		{
			mInstance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void Reset()
	{
		base.enabled = true;
		mLastBundleIdx = 0;
		mBundleLoadCallCount = 0;
		TerminateDownload(force: true);
		mCompletedCount = 0;
		StartCoroutine(ResetProgressUI());
		if (mBundlesDownloading != null)
		{
			mBundlesDownloading.Clear();
		}
	}

	protected override void Start()
	{
		base.Start();
		mProgressBar = FindItem("ProgressBar");
		mTxtPercentage = FindItem("DownloadPercentage");
		mProgressText = mProgressBar.GetText();
		mMessageGroup = FindItem("MessageGroup");
		mTimeRemainingMsgGroup = FindItem("TimeRemainingMessage");
		mDownloadMsg = FindItem("DownloadMessage");
		mBackground = FindItem("AniPrefetchBkg");
		mSplash = _Splash.GetComponent<UITexture>();
		if (mSplash != null)
		{
			mSplash.enabled = false;
		}
		SetInteractive(interactive: false);
	}

	public void ReInit()
	{
		UtWWWAsync.pVersionList.Clear();
		UtWWWAsync.InitializeAssetVersionLists(force: true);
		pInstance.Reset();
		pInstance.pState = State.NONE;
	}

	public void ClearPreloadBundles(Action eventHandler)
	{
		if (mPreloadBundles == null)
		{
			mPreloadBundles = new List<string>();
			if (!string.IsNullOrEmpty(ProductSettings.pInstance._Resource))
			{
				mPreloadBundles.Add(ProductSettings.pInstance._Resource);
			}
			if (ProductSettings.pInstance._PreloadBundles != null && ProductSettings.pInstance._PreloadBundles.Length != 0)
			{
				mPreloadBundles.AddRange(ProductSettings.pInstance._PreloadBundles);
			}
		}
		for (int i = 0; i < mPreloadBundles.Count; i++)
		{
			RsResourceManager.Unload(mPreloadBundles[i], splitURL: true, force: true, ignoreLoading: true);
		}
		mReloadEventHandler = eventHandler;
		mReloadBundles = true;
	}

	private bool IsValidURL(string inURL)
	{
		if (!inURL.Contains("RS_DATA") && !inURL.Contains("RS_SCENE") && !inURL.Contains("RS_SOUND") && !inURL.Contains("RS_SHARED"))
		{
			return inURL.Contains("RS_CONTENT");
		}
		return true;
	}

	public void OnResPrefetchingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		mLastCallbackTime = Time.time + 20f;
		BundleDownloadProgress bundleDownloadProgress = mBundlesDownloading.Find((BundleDownloadProgress bundle) => bundle._URL.Contains(inURL));
		if (bundleDownloadProgress != null)
		{
			bundleDownloadProgress._Progress = inProgress;
		}
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (bundleDownloadProgress != null)
			{
				mBundlesDownloading.Remove(bundleDownloadProgress);
				mDownloadCompleteAssetList.Add(bundleDownloadProgress._URL);
				mDownloadedBundlesSize += bundleDownloadProgress._Size;
				UtDebug.Log("\t\t ========= mDownloadedBundlesSize:" + mDownloadedBundlesSize + "\t completed count:" + (mCompletedCount + 1) + "\ttotal count:" + mDownloadBundleCount);
				UtDebug.Log("^^^^^^Prefetching " + inURL + " completed *******", 8u);
				CheckCompleted();
				CheckDependencyLoad(inURL);
			}
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.Log("^^^^^^Prefetching " + inURL + " failed *******", 8u);
			if (mErrorType == ErrorType.NONE)
			{
				mErrorType = ErrorType.BUNDLE_CACHED_FAILED;
				mLastBundleIdx = 0;
				Debug.Log(" @@@@ ERROR FOR URL : " + inURL);
				ShowErrorDB();
			}
			if (bundleDownloadProgress != null)
			{
				mBundlesDownloading.Remove(bundleDownloadProgress);
				CheckCompleted();
				CheckDependencyLoad(inURL);
			}
			break;
		case RsResourceLoadEvent.PROGRESS:
			UpdateProgressUI();
			break;
		}
	}

	private void CheckDependencyLoad(string inURL)
	{
		string[] dependencies = RsResourceManager.GetDependencies(RsResourceManager.GetUnityBundleName(inURL));
		if (dependencies == null)
		{
			return;
		}
		string[] array = dependencies;
		foreach (string bundleURL in array)
		{
			string formatedBundleUrl = RsResourceManager.FormatBundleURL(bundleURL);
			if (!mDownloadCompleteAssetList.Contains(formatedBundleUrl) && RsResourceManager.IsBundleCached(formatedBundleUrl))
			{
				AssetVersion assetVersion = mCurrentAssetVersionArray.ToList().Find((AssetVersion av) => av.AssetName == formatedBundleUrl);
				if (assetVersion != null)
				{
					mDownloadCompleteAssetList.Add(assetVersion.AssetName);
					CheckCompleted(isDependentBundle: true);
				}
			}
		}
	}

	private void CheckCompleted(bool isDependentBundle = false)
	{
		if (mInstance == null)
		{
			UtDebug.LogWarning(">>>>>>> returing because PrefetchManager.mInstance is null");
			return;
		}
		mCompletedCount++;
		if (!base.enabled)
		{
			return;
		}
		if (!IsConnectedToWWW() || mErrorType != 0)
		{
			if (!isDependentBundle)
			{
				mBundleLoadCallCount--;
			}
			return;
		}
		if (mCompletedCount >= mDownloadBundleCount)
		{
			mErrorType = ErrorType.NONE;
			base.enabled = false;
			pState = State.DONE;
			Screen.sleepTimeout = -2;
		}
		else if (!isDependentBundle)
		{
			mBundleLoadCallCount--;
			mDownloadNewBundles = true;
		}
		UpdateProgressUI();
	}

	private IEnumerator ResetProgressUI()
	{
		if (mProgressBar.GetProgressLevel() == 1f)
		{
			while (!LocaleData.pIsReady || !FontManager.pInstance.pIsReady || !pIsVersionDownloadComplete)
			{
				yield return null;
			}
			mProgressBar.SetText(_DownLoadComplete.GetLocalizedString());
			mProgressBar.SetProgressLevel(100f);
			mTxtPercentage.SetText("100%");
		}
		else
		{
			mProgressBar.SetText("0");
			mProgressBar.SetProgressLevel(0f);
			mTxtPercentage.SetText("0%");
		}
	}

	private void UpdateProgressUI()
	{
		float num = 0f;
		mCurrDownloadingBundlesSize = 0f;
		for (int i = 0; i < mBundlesDownloading.Count; i++)
		{
			num += mBundlesDownloading[i]._Progress;
			mCurrDownloadingBundlesSize += mBundlesDownloading[i]._Progress * mBundlesDownloading[i]._Size;
		}
		float num2 = ((float)mCompletedCount + num) / (float)mDownloadBundleCount;
		if (!(mProgressBar != null))
		{
			return;
		}
		if (num2 < 1f)
		{
			if (string.IsNullOrEmpty(mProgressText))
			{
				mProgressBar.SetText(mCompletedCount + " / " + mDownloadBundleCount);
			}
			else
			{
				mProgressBar.SetText(mProgressText + " " + mCompletedCount + " / " + mDownloadBundleCount);
			}
		}
		else
		{
			num2 = 1f;
			mProgressBar.SetText(_DownLoadComplete.GetLocalizedString());
		}
		if (mTxtPercentage != null)
		{
			int num3 = Mathf.FloorToInt(num2 * 100f);
			mTxtPercentage.SetText(num3 + "%");
		}
		mProgressBar.SetProgressLevel(num2);
	}

	public static void Kill()
	{
		if (mInstance != null)
		{
			UnityEngine.Object.Destroy(mInstance.gameObject);
		}
	}

	protected override void Update()
	{
		if (pState == State.DONE)
		{
			return;
		}
		base.Update();
		if (mSplash != null && mSplash.enabled)
		{
			if (mSplashTime > 0f)
			{
				mSplashTime -= Time.deltaTime;
				if (mSplashTime < 0f)
				{
					mLoadScreenTime = _LoadScreenTime;
					GetLoadScreen();
				}
			}
			if (mLoadScreenTime > 0f)
			{
				mLoadScreenTime -= Time.deltaTime;
				if (mLoadScreenTime < 0f)
				{
					mLoadScreenTime = _LoadScreenTime;
					GetLoadScreen();
				}
			}
		}
		if (mGenericDB != null)
		{
			if (!IsConnectedToWWW() && !mNetworkDisconnect)
			{
				mGenericDB.SetText(_OfflineFailedText.GetLocalizedString(), interactive: false);
				mNetworkDisconnect = true;
			}
			return;
		}
		if (pState == State.WAIT && IsConnectedToWWW())
		{
			OnDBClose();
		}
		if (!IsConnectedToWWW() && mDownloadBundleCount > 0)
		{
			pState = State.WAIT;
			if (mErrorType == ErrorType.NONE)
			{
				mErrorType = ErrorType.BUNDLE_CACHED_FAILED;
				mBundlesDownloading.Clear();
				mBundleLoadCallCount = 0;
				UtWWWAsync.KillAsyncLoads();
				mLastBundleIdx = 0;
				ShowErrorDB();
				return;
			}
		}
		if (mState == State.BUNDLE_DOWNLOADING)
		{
			mSamplingTimer += Time.deltaTime;
			if (mSamplingTimer >= _SamplingTimeInSecs)
			{
				float num = mCurrDownloadingBundlesSize + mDownloadedBundlesSize;
				float num2 = num - mPrevLoadedBundlesSize;
				if (num2 > 0f)
				{
					float num3 = _SamplingTimeInSecs / num2;
					float num4 = (TotalBundlesSize - num) * num3;
					UtDebug.Log("==== loadedBundlesSize:" + num + "\t mDownloadedBundlesSize:" + mDownloadedBundlesSize + "\t mTotalBundlesSize:" + TotalBundlesSize + "\t diffBundlesSizeForSampledTime:" + num2 + "\t secondsRemaining:" + num4);
					if (mTimeRemainingMsgGroup != null)
					{
						string text = "";
						TimeSpan timeSpan = new TimeSpan(0, 0, (int)num4);
						if (timeSpan.Hours >= 1)
						{
							text = $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}";
						}
						else if (timeSpan.Minutes >= 1)
						{
							int num5 = ((timeSpan.Seconds > 0) ? (timeSpan.Minutes + 1) : timeSpan.Minutes);
							text = $"{num5} {_MinutesText.GetLocalizedString()}";
						}
						else if (timeSpan.Minutes < 1)
						{
							text = "< 1 " + _MinuteText.GetLocalizedString();
						}
						mTimeRemainingMsgGroup.SetText(text);
					}
					mPrevLoadedBundlesSize = num;
				}
				mSamplingTimer = 0f;
			}
			if (mDownloadNewBundles)
			{
				mDownloadNewBundles = false;
				DownloadBundles();
			}
		}
		if (mPaused || !(Time.time > mLastCallbackTime) || mState != State.BUNDLE_DOWNLOADING || mErrorType != 0)
		{
			return;
		}
		Debug.LogError("@@@ Prefetch is stuck !!!!!! ");
		AssetVersion[] array = mCurrentAssetVersionArray;
		foreach (AssetVersion av in array)
		{
			if (!IsAssetDownloaded(av.AssetName) && (!RsResourceManager.IsBundle(av.AssetName) || !RsResourceManager.IsBundleCached(av.AssetName) || mBundlesDownloading.Count == 0))
			{
				Debug.Log("Bundle is already cached or version seems to be downloaded from somewhere else try to complete for" + av.AssetName);
				BundleDownloadProgress bundleDownloadProgress = mBundlesDownloading.Find((BundleDownloadProgress bundle) => bundle._URL.Contains(av.AssetName));
				if (bundleDownloadProgress != null)
				{
					mBundlesDownloading.Remove(bundleDownloadProgress);
				}
				CheckCompleted();
			}
		}
	}

	private void StartDownloadingPrefetchData(Action eventHandler)
	{
		pState = State.PREFETCH_DATA_DOWNLOADING;
		if (mPrefetchCount > 0)
		{
			mPrefetchList = null;
			{
				foreach (string mPrefetchXML in mPrefetchXMLList)
				{
					RsResourceManager.Load(mPrefetchXML, PrefetchListEventHandler, RsResourceType.NONE, inDontDestroy: false, inDisableCache: false, inDownloadOnly: false, inIgnoreAssetVersion: false, eventHandler);
				}
				return;
			}
		}
		RsResourceManager.Load(_PrefetchListXml, PrefetchListEventHandler, RsResourceType.NONE, inDontDestroy: false, inDisableCache: false, inDownloadOnly: false, inIgnoreAssetVersion: false, eventHandler);
	}

	private void PrefetchListEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent.Equals(RsResourceLoadEvent.COMPLETE))
		{
			mPrefetchCount--;
			PrefetchList prefetchList = UtUtilities.DeserializeFromXml((string)inObject, typeof(PrefetchList)) as PrefetchList;
			if (mPrefetchList != null)
			{
				List<string> list = mPrefetchList.BundleNames.ToList();
				list.AddRange(prefetchList.BundleNames.ToList());
				mPrefetchList.BundleNames = list.ToArray();
			}
			else
			{
				mPrefetchList = prefetchList;
			}
			if (mPrefetchCount <= 0)
			{
				PrefetchDataDownloaded();
			}
			if (inUserData is Action action)
			{
				action();
			}
		}
		else if (inEvent.Equals(RsResourceLoadEvent.ERROR))
		{
			Debug.LogError("Failed to download Prefetch List : " + inURL);
			PrefetchDataDownloaded();
			if (inUserData is Action action2)
			{
				action2();
			}
		}
	}

	public void PrefetchDataDownloaded()
	{
		pState = State.WAIT_FOR_START;
		mCompletedCount = (mDownloadBundleCount = 0);
		GetDownloadBundleCount();
		if (mProgressBar != null)
		{
			if (mCompletedCount < mDownloadBundleCount)
			{
				if (string.IsNullOrEmpty(mProgressText))
				{
					mProgressBar.SetText(mCompletedCount + " / " + mDownloadBundleCount);
				}
				else
				{
					mProgressBar.SetText(mProgressText + " " + mCompletedCount + " / " + mDownloadBundleCount);
				}
			}
			else
			{
				mProgressBar.SetText(_DownLoadComplete.GetLocalizedString());
			}
			float num = 1f;
			if (mDownloadBundleCount > 0 && mCompletedCount < mDownloadBundleCount)
			{
				num = mCompletedCount / mDownloadBundleCount;
			}
			if (mTxtPercentage != null)
			{
				int num2 = Mathf.FloorToInt(num * 100f);
				mTxtPercentage.SetText(num2 + "%");
			}
		}
		if (mDownloadBundleCount > 0 && !IsConnectedToWWW())
		{
			mErrorType = ErrorType.BUNDLE_CACHED_FAILED;
			ShowErrorDB();
			mCompletedCount = -1;
			Screen.sleepTimeout = -2;
		}
		else if (mDownloadBundleCount == 0)
		{
			Screen.sleepTimeout = -2;
		}
	}

	private void ShowErrorDB()
	{
		if (GetVisibility() && mGenericDB == null)
		{
			StartCoroutine(TestConnection());
			mGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "PfKAUIGenericDB");
			KAUI.SetExclusive(mGenericDB, _MaskColor);
			KAUICursorManager.SetDefaultCursor("Arrow");
			string localizedString = _OnlineFailedText.GetLocalizedString();
			if (!IsConnectedToWWW())
			{
				mNetworkDisconnect = true;
				localizedString = _OfflineFailedText.GetLocalizedString();
			}
			if (mErrorType == ErrorType.BUNDLE_CACHED_FAILED)
			{
				mGenericDB.SetText(localizedString, interactive: false);
				mGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
				mGenericDB._OKMessage = "OnRetry";
			}
			mGenericDB._MessageObject = base.gameObject;
			UnityEngine.Object.DontDestroyOnLoad(mGenericDB.gameObject);
		}
	}

	private IEnumerator TestConnection()
	{
		if (ProductConfig.pInstance != null)
		{
			string text = ProductConfig.pInstance.ContentServerURL;
			if (text.EndsWith("/"))
			{
				text = text.Substring(0, text.Length - 1);
			}
			WWW testPing = new WWW(text);
			yield return testPing;
			UtUtilities._ConnectedToInternet = string.IsNullOrEmpty(testPing.error);
			if (UtUtilities._ConnectedToInternet && mErrorType == ErrorType.BUNDLE_CACHED_FAILED)
			{
				OnDBClose();
			}
		}
	}

	private void TerminateDownload(bool force = false)
	{
		if (!(mErrorType == ErrorType.BUNDLE_CACHED_FAILED || force) || mBundlesDownloading == null)
		{
			return;
		}
		bool flag = false;
		foreach (BundleDownloadProgress item in mBundlesDownloading)
		{
			flag = UtWWWAsync.KillProcess(RsResourceManager.ParseURL(item._URL, useLocalAsset: false));
			RsResourceManager.Unload(item._URL, splitURL: true, force: true, ignoreLoading: true);
		}
		if (flag)
		{
			mBundlesDownloading.Clear();
		}
	}

	private void GetDownloadBundleCount()
	{
		TotalBundlesSize = 0f;
		mDownloadedBundlesSize = 0f;
		List<AssetVersion> list = new List<AssetVersion>();
		if (UtPlatform.IsEditor())
		{
			return;
		}
		foreach (KeyValuePair<string, AssetVersion> pVersion in UtWWWAsync.pVersionList)
		{
			AssetVersion value = pVersion.Value;
			if (IsValidURL(value.AssetName) && (mPrefetchList == null || mPrefetchList.BundleNames == null || mPrefetchList.BundleNames.Length == 0 || Array.IndexOf(mPrefetchList.BundleNames, value.AssetName) >= 0) && RsResourceManager.IsBundle(value.AssetName) && !RsResourceManager.IsBundleCached(value.AssetName))
			{
				mDownloadBundleCount++;
				TotalBundlesSize += value.GetFileSize(UtUtilities.GetLocaleLanguage()) / 1024f;
				list.Add(value);
			}
		}
		mCurrentAssetVersionArray = list.ToArray();
	}

	private void DownloadBundles()
	{
		if (pState != State.BUNDLE_DOWNLOADING)
		{
			mBundlesDownloading = new List<BundleDownloadProgress>();
			Screen.sleepTimeout = -1;
			mLoadingTexts = null;
			LoadingTextData.GetLoadTextData(LoadingTextType.PREFETCH, "", ref mLoadingTexts, ref mDelayBwLoadTextsInSecs);
			if (mLoadingTexts != null && mLoadingTexts.Length != 0)
			{
				UtUtilities.Shuffle(mLoadingTexts);
				mLoadTextIndex = 0;
				StartCoroutine(UpdateLoadText());
				mSamplingTimer = _SamplingTimeInSecs - _FirstSamplingTimeInSecs;
			}
		}
		pState = State.BUNDLE_DOWNLOADING;
		if (mLastBundleIdx == 0)
		{
			mCompletedCount = 0;
			AssetVersion[] array = mCurrentAssetVersionArray;
			for (int i = 0; i < array.Length; i++)
			{
				if (RsResourceManager.IsBundleCached(array[i].AssetName))
				{
					mCompletedCount++;
				}
			}
		}
		while (mLastBundleIdx < mCurrentAssetVersionArray.Length && mBundleLoadCallCount < _BundleDownloadCount && !mPaused)
		{
			AssetVersion av = mCurrentAssetVersionArray[mLastBundleIdx];
			if ((!RsResourceManager.IsBundle(av.AssetName) || !RsResourceManager.IsBundleCached(av.AssetName)) && mBundlesDownloading.Find((BundleDownloadProgress bundle) => bundle._URL.Contains(av.AssetName)) == null)
			{
				mBundleLoadCallCount++;
				BundleDownloadProgress item = new BundleDownloadProgress
				{
					_URL = av.AssetName,
					_Progress = 0f,
					_RetryCount = 0,
					_Size = av.GetFileSize(UtUtilities.GetLocaleLanguage())
				};
				mBundlesDownloading.Add(item);
				RsResourceManager.PrefetchBundle(av.AssetName, OnResPrefetchingEvent, inDontDestroy: false, null);
			}
			mLastBundleIdx++;
		}
	}

	private IEnumerator UpdateLoadText()
	{
		while (mLoadTextIndex < mLoadingTexts.Length)
		{
			mDownloadMsg.SetText(mLoadingTexts[mLoadTextIndex].GetLocalizedString());
			mLoadTextIndex++;
			yield return new WaitForSeconds(mDelayBwLoadTextsInSecs);
		}
	}

	private void GetLoadScreen()
	{
		if (!(mBackground == null))
		{
			LoadScreen loadScreenWithTag = LoadScreenData.GetLoadScreenWithTag("Prefetch");
			if (loadScreenWithTag != null)
			{
				RsResourceManager.Load(loadScreenWithTag.Name, LoadScreenImageEventHandler);
			}
		}
	}

	private void LoadScreenImageEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent.Equals(RsResourceLoadEvent.COMPLETE) && inObject is Texture)
		{
			mBackground.SetTexture((Texture)inObject);
		}
	}

	public void OnDBClose()
	{
		if (IsConnectedToWWW())
		{
			if (mGenericDB != null)
			{
				UnityEngine.Object.Destroy(mGenericDB.gameObject);
			}
			mGenericDB = null;
			KAUI.RemoveExclusive(KAUI._GlobalExclusiveUI);
			if (mErrorType == ErrorType.BUNDLE_CACHED_FAILED)
			{
				DownloadBundles();
			}
			mNetworkDisconnect = false;
			mErrorType = ErrorType.NONE;
		}
		if (!LocaleData.pIsReady)
		{
			KAUICursorManager.SetDefaultCursor("Loading");
		}
	}

	public void OnRetry()
	{
		StartCoroutine(TestConnection());
		OnDBClose();
	}

	private bool IsConnectedToWWW()
	{
		return UtUtilities._ConnectedToInternet;
	}

	public static void ShowDownloadProgress()
	{
		if (mInstance != null)
		{
			mInstance.ShowProgress();
		}
	}

	public static void Show(bool isVisible)
	{
		if (mInstance != null)
		{
			mInstance.SetVisibility(isVisible);
			if (mInstance.mSplash == null)
			{
				mInstance.mSplash = mInstance._Splash.GetComponent<UITexture>();
			}
			if (mInstance.mSplash != null)
			{
				mInstance.mSplash.enabled = isVisible;
			}
			if (isVisible)
			{
				mInstance.ShowErrorDB();
			}
		}
	}

	private void ShowProgress()
	{
		SetVisibility(inVisible: true);
		if (mSplash != null)
		{
			mSplash.enabled = true;
		}
		mSplashTime = _SplashTime;
		if (mMessageGroup != null)
		{
			mMessageGroup.SetVisibility(inVisible: true);
		}
		if (pIsReady && mProgressBar != null)
		{
			mProgressBar.SetProgressLevel(1f);
			mProgressBar.SetText(_DownLoadComplete.GetLocalizedString());
			if (mTxtPercentage != null)
			{
				mTxtPercentage.SetText("100%");
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (mInstance == this)
		{
			mInstance = null;
		}
	}

	private bool IsAssetDownloaded(string inAssetName)
	{
		return !string.IsNullOrEmpty(mDownloadCompleteAssetList.Find((string url) => url == inAssetName));
	}

	private void ShowMovie()
	{
		OnMoviePlayed();
	}

	public void OnMoviePlayed()
	{
		mFinishedMovie = true;
	}

	public void PostPrefetch()
	{
		if (mPreloadBundles != null && mPreloadBundles.Count > 0)
		{
			if (mPreloader == null)
			{
				mPreloader = new RsAssetLoader();
			}
			else
			{
				mPreloader.Reset();
			}
			mPreloader.Load(mPreloadBundles.ToArray(), null, PreloadEventHandler, inDontDestroy: true);
		}
		else if (mReloadEventHandler != null)
		{
			mReloadEventHandler();
			mReloadEventHandler = null;
		}
	}

	public void PreloadEventHandler(RsAssetLoader inAssetLoader, RsResourceLoadEvent inEvent, float inProgress, object inUserData)
	{
		if ((uint)(inEvent - 2) <= 1u && mReloadEventHandler != null)
		{
			mReloadEventHandler();
			mReloadEventHandler = null;
		}
	}

	public string[] GetDownloadStates()
	{
		return mPrefetchList.DownloadStates;
	}
}
