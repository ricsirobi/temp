using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using KA.Framework;
using Microsoft.AppCenter.Unity.Crashes;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class UtWWWAsync : KAMonoBase
{
	public enum VersionsDownloadState
	{
		NONE,
		ASSET_VERSION_DOWNLOADING,
		DONE
	}

	public class WWWProcess : UtIWWWAsync
	{
		private UnityWebRequest mWebRequest;

		private WWWForm mForm;

		private RsResourceType mResourceType;

		private bool mIgnoreAssetVersion;

		private string mURL = "";

		private string mLocaleURL = "";

		private UtWWWEventHandler mEventDelegate;

		private bool mProcessProgressEvents;

		private float mLastProgress;

		private float mProgressAmount = 0.05f;

		private int mNumRetries;

		private float mTimer;

		private float mTimeSent;

		private bool mDisableCache;

		private bool mIsDownload;

		private bool mIsDownloadOnly;

		private int mSendFinalEventCountdown = 1;

		private Hash128 mHash;

		private const string TIME_OUT_ERROR = "Empty reply from server";

		private const string DATA_TAMPERED_ERROR = "Data Validation Failed";

		private const string RESPONSE_SIGNATURE_KEYNAME = "SIGNATURE";

		private Texture2D mTexture;

		private bool mFromCache;

		public bool pDownloadStarted => !mIsDownload;

		public bool pIsDownloadOnly => mIsDownloadOnly;

		public UnityWebRequest pWebRequest => mWebRequest;

		public string pURL
		{
			get
			{
				if (mWebRequest == null)
				{
					return null;
				}
				return mURL;
			}
		}

		public RsResourceType pResourcetype => mResourceType;

		public bool pIsDone
		{
			get
			{
				if (mWebRequest == null || mWebRequest.downloadHandler == null)
				{
					return false;
				}
				return mWebRequest.downloadHandler.isDone;
			}
		}

		public float pProgress
		{
			get
			{
				if (mWebRequest == null)
				{
					return 0f;
				}
				if (!mIsDownload)
				{
					return mWebRequest.uploadProgress;
				}
				return mWebRequest.downloadProgress;
			}
		}

		public string pError
		{
			get
			{
				if (mWebRequest == null)
				{
					return null;
				}
				return mWebRequest.error;
			}
		}

		public string pData
		{
			get
			{
				if (mWebRequest == null || mWebRequest.downloadHandler == null)
				{
					return null;
				}
				return mWebRequest.downloadHandler.text;
			}
		}

		public byte[] pBytes
		{
			get
			{
				if (mWebRequest == null || mWebRequest.downloadHandler == null)
				{
					return null;
				}
				return mWebRequest.downloadHandler.data;
			}
		}

		public Texture pTexture
		{
			get
			{
				if (mTexture == null && mWebRequest != null && mWebRequest.downloadHandler != null && mWebRequest.downloadHandler.data != null)
				{
					mTexture = new Texture2D(2, 2);
					mTexture.LoadImage(mWebRequest.downloadHandler.data);
				}
				return mTexture;
			}
		}

		public AudioClip pAudioClip
		{
			get
			{
				if (mWebRequest == null || mWebRequest.downloadHandler == null)
				{
					return null;
				}
				return ((DownloadHandlerAudioClip)mWebRequest.downloadHandler).audioClip;
			}
		}

		public AssetBundle pAssetBundle
		{
			get
			{
				if (mWebRequest == null || mWebRequest.downloadHandler == null)
				{
					return null;
				}
				return ((DownloadHandlerAssetBundle)mWebRequest.downloadHandler).assetBundle;
			}
		}

		public bool pFromCache => mFromCache;

		public static AssetVersion MakeGetVersionCall(string inURL, out bool useLocalAsset)
		{
			string key = inURL.Split('/')[^1];
			useLocalAsset = false;
			AssetVersion value;
			bool flag = pVersionList.TryGetValue(key, out value);
			if (flag)
			{
				if (pDefaultVersionList.TryGetValue(inURL, out var value2))
				{
					string localeLanguage = UtUtilities.GetLocaleLanguage();
					AssetVersion.Variant closestVariant = value.GetClosestVariant(localeLanguage);
					AssetVersion.Variant closestVariant2 = value2.GetClosestVariant(localeLanguage);
					if (closestVariant.Locale == closestVariant2.Locale && closestVariant.Version == closestVariant2.Version)
					{
						useLocalAsset = true;
					}
				}
			}
			else if (!flag)
			{
				return AssetVersion.CreateDefault(inURL);
			}
			return value;
		}

		public void DownloadBundle(string url, Hash128 hash, UtWWWEventHandler callback, bool sendProgressEvents, bool disableCache, bool downloadOnly)
		{
			mHash = hash;
			Download(url, RsResourceType.ASSET_BUNDLE, callback, sendProgressEvents, disableCache, downloadOnly, inIgnoreAssetVersion: false);
		}

		public void Download(string inURL, RsResourceType inType, UtWWWEventHandler inCallback, bool inSendProgressEvents, bool inDisableCache, bool inDownLoadOnly, bool inIgnoreAssetVersion)
		{
			mTexture = null;
			mWebRequest = null;
			mResourceType = inType;
			mURL = inURL;
			mEventDelegate = inCallback;
			mProcessProgressEvents = inSendProgressEvents;
			mTimeSent = Time.realtimeSinceStartup;
			mIgnoreAssetVersion = inIgnoreAssetVersion;
			mIsDownloadOnly = inDownLoadOnly;
			if (!inDownLoadOnly)
			{
				if (ProductConfig.pIsReady && ProductConfig.pInstance.DisableCache.HasValue)
				{
					mDisableCache = ProductConfig.pInstance.DisableCache.Value;
				}
				mDisableCache |= inDisableCache;
			}
			if (mResourceType != RsResourceType.ASSET_BUNDLE || !mSuspendLoading)
			{
				StartDownload(inDownLoadOnly);
			}
		}

		public void StartDownload(bool inDownLoadOnly)
		{
			mIsDownload = true;
			if (!UtPlatform.IsEditor() && !mDoneAppXML && ProductConfig.pInstance == null)
			{
				if (mURL.Split('/')[^1] == ProductConfig.pAppName + ".xml")
				{
					SendResourceWebRequest(mURL);
					mDoneAppXML = true;
				}
			}
			else if (mResourceType != RsResourceType.ASSET_BUNDLE)
			{
				if (mIgnoreAssetVersion || pVersionList.Count > 0)
				{
					int version = 0;
					if (mIgnoreAssetVersion)
					{
						mLocaleURL = RsResourceManager.ParseURL(mURL, useLocalAsset: false);
					}
					else
					{
						bool useLocalAsset;
						AssetVersion assetVersion = MakeGetVersionCall(mURL, out useLocalAsset);
						version = assetVersion.GetVersion(UtUtilities.GetLocaleLanguage());
						mLocaleURL = ProcessLocale(mURL, assetVersion, useLocalAsset);
					}
					SendResourceWebRequest(mLocaleURL, version);
				}
				else
				{
					InitializeAssetVersionLists();
				}
			}
			else if (mResourceType == RsResourceType.ASSET_BUNDLE)
			{
				mURL = RsResourceManager.ParseURL(mURL, useLocalAsset: false);
				SendBundleWebRequest(mURL);
			}
			else
			{
				mURL = RsResourceManager.ParseURL(mURL, useLocalAsset: false);
				SendResourceWebRequest(mURL);
			}
		}

		private void SendBundleWebRequest(string url)
		{
			if (!IsLocalPath(url))
			{
				string text = url;
				Hash128 hash = mHash;
				url = text + "?v=" + hash.ToString();
			}
			if (mDisableCache)
			{
				mWebRequest = UnityWebRequestAssetBundle.GetAssetBundle(url);
			}
			else
			{
				mWebRequest = UnityWebRequestAssetBundle.GetAssetBundle(url, mHash);
				mFromCache = true;
			}
			UtDebug.Log("Url bundle before web request: " + url + " web request is not null " + (mWebRequest != null), 100);
			mWebRequest.SendWebRequest();
		}

		private void SendResourceWebRequest(string inURL, int version = 0)
		{
			if (mResourceType == RsResourceType.AUDIO)
			{
				string text = string.Empty;
				int num = inURL.LastIndexOf('.');
				if (num != -1)
				{
					text = inURL.Substring(num + 1).ToLower();
				}
				if (text == "wav")
				{
					mWebRequest = UnityWebRequestMultimedia.GetAudioClip(inURL, AudioType.WAV);
				}
				else
				{
					mWebRequest = UnityWebRequestMultimedia.GetAudioClip(inURL, AudioType.OGGVORBIS);
				}
			}
			else
			{
				bool flag = IsLocalPath(inURL);
				if (!UtPlatform.IsEditor() && !flag)
				{
					inURL += ((mResourceType == RsResourceType.XML) ? ("?t=" + DateTime.Now.Ticks) : ("?v=" + version));
				}
				if (mResourceType == RsResourceType.XML || mResourceType == RsResourceType.TEXT_ASSET || mResourceType == RsResourceType.IMAGE || mResourceType == RsResourceType.BINARY)
				{
					mWebRequest = UnityWebRequest.Get(inURL);
				}
			}
			UtDebug.Log("Url  other resource before web request: " + inURL + " resourceType " + mResourceType, 100);
			mWebRequest.SendWebRequest();
		}

		public void OnSceneLoaded(string inLevel)
		{
			if (mResourceType == RsResourceType.IMAGE && mWebRequest != null && mIsDownload)
			{
				UtDebug.LogError("Downloading texture during scene transition is not allowed. Aborting the texture download : " + mURL, debugLevel);
				mWebRequest.Abort();
				Kill();
				mEventDelegate(UtAsyncEvent.ERROR, this);
			}
		}

		public void PostForm(string inURL, WWWForm inForm, UtWWWEventHandler inCallback, bool inSendProgressEvents)
		{
			mWebRequest = null;
			mURL = inURL;
			mEventDelegate = inCallback;
			mProcessProgressEvents = inSendProgressEvents;
			mTimeSent = Time.realtimeSinceStartup;
			mForm = inForm;
			StartPost();
		}

		private IEnumerator DownloadProductRule()
		{
			if (ProductConfig.pInstance == null)
			{
				ProductConfig.Init();
			}
			WWWForm wWWForm = new WWWForm();
			wWWForm.AddField("apiKey", ProductConfig.pApiKey);
			mProductRuleRequest = UnityWebRequest.Post(ProductConfig.pInstance.AuthenticationServerV3URL + "GetRules", wWWForm);
			yield return mProductRuleRequest.SendWebRequest();
			bool flag = false;
			if (mProductRuleRequest.isDone && !mProductRuleRequest.isNetworkError && !mProductRuleRequest.isHttpError && !string.IsNullOrEmpty(mProductRuleRequest.downloadHandler.text))
			{
				mProductRuleData = UtUtilities.DeserializeFromXml(TripleDES.DecryptUnicode(UtUtilities.DeserializeFromXml(mProductRuleRequest.downloadHandler.text, typeof(string)) as string, WsWebService.pSecret), typeof(GetProductRulesResponse)) as GetProductRulesResponse;
				if (mProductRuleData == null)
				{
					flag = true;
				}
				else if (mAction != null)
				{
					mAction();
					mAction = null;
				}
			}
			else
			{
				flag = true;
			}
			mProductRuleRequest.Dispose();
			mProductRuleRequest = null;
			if (flag)
			{
				UtUtilities.ShowServerError(ProductConfig.pInstance.ProductRuleFailedText, StartProductRuleDownload, destroyOnClick: true);
			}
		}

		private void StartProductRuleDownload()
		{
			if (mProductRuleRequest == null)
			{
				pInstance.StartCoroutine(DownloadProductRule());
			}
		}

		private bool IsRuleValid(Rule rule)
		{
			if (mURL.ToLower().Contains(rule.UrlContains.ToLower()) && (rule.EnableAll || (rule.Enable != null && !string.IsNullOrEmpty(Array.Find(rule.Enable, (string url) => mURL.IndexOf(url, StringComparison.OrdinalIgnoreCase) >= 0)))))
			{
				return true;
			}
			return false;
		}

		private void StartPost()
		{
			if (mProductRuleData == null)
			{
				mAction = (Action)Delegate.Combine(mAction, (Action)delegate
				{
					StartPost();
				});
				StartProductRuleDownload();
			}
			else
			{
				mWebRequest = UnityWebRequest.Post(mURL, mForm);
				mWebRequest.SendWebRequest();
			}
		}

		private bool IsDataTampered()
		{
			if (mProductRuleData.Rules == null || mProductRuleData.Rules.ResponseHashValidationRules == null || mWebRequest.downloadHandler == null || string.IsNullOrEmpty(mWebRequest.downloadHandler.text))
			{
				return false;
			}
			if (mProductRuleData.Rules.ResponseHashValidationRules.Find(IsRuleValid) != null)
			{
				string value = "";
				string md5Hash = WsMD5Hash.GetMd5Hash(mProductRuleData.GlobalSecretKey + mWebRequest.downloadHandler.text);
				Dictionary<string, string> responseHeaders = mWebRequest.GetResponseHeaders();
				if (responseHeaders != null)
				{
					if (responseHeaders.TryGetValue("SIGNATURE", out value))
					{
						return md5Hash != value;
					}
					return true;
				}
				return true;
			}
			return false;
		}

		public bool Update()
		{
			if (mProductRuleData == null && !mIsDownload)
			{
				return false;
			}
			if (!mIgnoreAssetVersion && mIsDownload && mVersionDownloadState == VersionsDownloadState.ASSET_VERSION_DOWNLOADING)
			{
				return false;
			}
			if (mWebRequest != null)
			{
				if (pIsDone || !string.IsNullOrEmpty(mWebRequest.error))
				{
					if (!mIsDownload && string.IsNullOrEmpty(mWebRequest.error) && IsDataTampered())
					{
						return ProcessError("Data Validation Failed", new InvalidDataException());
					}
					if (mSendFinalEventCountdown < 1)
					{
						if (mEventDelegate != null)
						{
							if (!string.IsNullOrEmpty(mWebRequest.error))
							{
								return ProcessError(mWebRequest.error, new Exception());
							}
							UtUtilities._ConnectedToInternet = true;
							try
							{
								UtDebug.Log("*****UtWWWAsync processing done. Total time for " + mWebRequest.url + " = " + (Time.realtimeSinceStartup - mTimeSent), debugLevel);
								mEventDelegate(UtAsyncEvent.COMPLETE, this);
							}
							catch (Exception ex)
							{
								Dictionary<string, string> dictionary = new Dictionary<string, string>();
								dictionary.Add("URL", mWebRequest.url);
								dictionary.Add("Error Text", ex.Message);
								Crashes.TrackError(ex, dictionary);
								UtDebug.LogError("System exception caught: " + ex.ToString());
							}
						}
						return true;
					}
					mSendFinalEventCountdown--;
					return false;
				}
				if (Mathf.Approximately(mLastProgress, pProgress))
				{
					mTimer += Time.deltaTime;
					if (mTimer > (float)(mIsDownload ? ProductSettings.pInstance.GetEnvironmentDetails()._DownloadTimeOutInSecs : ProductSettings.pInstance.GetEnvironmentDetails()._PostTimeOutInSecs))
					{
						mTimer = 0f;
						return ProcessError("Empty reply from server", new TimeoutException());
					}
				}
				else
				{
					mTimer = 0f;
				}
				if (pProgress > mLastProgress + mProgressAmount)
				{
					mLastProgress = pProgress;
					UtDebug.Log("*****UtWWWAsync progress update: " + mWebRequest.url + " is " + Mathf.Round(mLastProgress * 100f) + "%", debugLevel);
				}
				if (mProcessProgressEvents && mEventDelegate != null)
				{
					try
					{
						mEventDelegate(UtAsyncEvent.PROGRESS, this);
					}
					catch (Exception ex2)
					{
						Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
						dictionary2.Add("URL", mWebRequest.url);
						dictionary2.Add("Error Text", ex2.Message);
						dictionary2.Add("Progress", Mathf.Round(mLastProgress * 100f) + "%");
						Crashes.TrackError(ex2, dictionary2);
						UtDebug.LogError("System exception caught: " + ex2.ToString());
					}
				}
				return false;
			}
			if (mResourceType == RsResourceType.ASSET_BUNDLE)
			{
				if (mSuspendLoading)
				{
					return false;
				}
				mURL = RsResourceManager.ParseURL(mURL, useLocalAsset: false);
				SendBundleWebRequest(mURL);
				return false;
			}
			if (pIsVersionDownloadComplete)
			{
				if (pVersionList.Count > 0)
				{
					bool useLocalAsset;
					AssetVersion assetVersion = MakeGetVersionCall(mURL, out useLocalAsset);
					mLocaleURL = ProcessLocale(mURL, assetVersion, useLocalAsset);
					SendResourceWebRequest(mLocaleURL, assetVersion.GetVersion(UtUtilities.GetLocaleLanguage()));
				}
				else
				{
					mURL = RsResourceManager.ParseURL(mURL, useLocalAsset: false);
					SendResourceWebRequest(mURL);
				}
				return false;
			}
			return true;
		}

		public void Kill()
		{
			if (mWebRequest != null)
			{
				if (pFromCache)
				{
					mWebRequest.Dispose();
				}
				else
				{
					AddToWebRequestDisposeList(mWebRequest);
				}
				mWebRequest = null;
				mTexture = null;
			}
		}

		protected bool ProcessError(string errorText, Exception exception)
		{
			try
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary.Add("URL", mWebRequest.url);
				dictionary.Add("Error Text", errorText);
				dictionary.Add("Progress", mLastProgress.ToString());
				if (mNumRetries > 0)
				{
					dictionary.Add("Retry Count", mNumRetries.ToString());
				}
				Crashes.TrackError(exception, dictionary);
				if (UtUtilities.OnServerError(errorText, mWebRequest.url, ref mNumRetries))
				{
					UtDebug.LogError(mWebRequest.url + " " + errorText + "\nStarting retry attempt " + mNumRetries);
					if (!mIsDownload)
					{
						StartPost();
					}
					else
					{
						Kill();
						StartDownload(mIsDownloadOnly);
					}
					return false;
				}
				UtDebug.LogError(mWebRequest.url + " " + errorText);
				mEventDelegate(UtAsyncEvent.ERROR, this);
			}
			catch (Exception ex)
			{
				Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
				dictionary2.Add("URL", mWebRequest.url);
				dictionary2.Add("Error Text", errorText);
				dictionary2.Add("Progress", mLastProgress.ToString());
				if (mNumRetries > 0)
				{
					dictionary2.Add("Retry Count", mNumRetries.ToString());
				}
				Crashes.TrackError(ex, dictionary2);
				UtDebug.LogError("Exception caught: " + ex.ToString());
			}
			return true;
		}
	}

	private class UtAsyncOperation
	{
		public string mURL;

		public AssetBundleRequest mBundleRequest;

		public RsResourceEventHandler mEventDelegate;

		private object mUserData;

		private int mSendFinalEventCountdown = 1;

		private bool mLoadPending = true;

		private AssetBundle mInBundle;

		private string mAssetName;

		private Type mType;

		public bool pLoadPending => mLoadPending;

		public UtAsyncOperation(string inURL, AssetBundle inBundle, string inAssetName, Type inType, RsResourceEventHandler inCallback, object inUserData = null)
		{
			mURL = inURL;
			mEventDelegate = inCallback;
			mInBundle = inBundle;
			mAssetName = inAssetName;
			mType = inType;
			if (!mSuspendLoading)
			{
				mBundleRequest = inBundle.LoadAssetAsync(inAssetName, inType);
				mLoadPending = false;
			}
			if (inUserData != null)
			{
				mUserData = inUserData;
			}
		}

		public void StartLoad()
		{
			mBundleRequest = mInBundle.LoadAssetAsync(mAssetName, mType);
			mLoadPending = false;
		}

		public void Kill()
		{
			mEventDelegate = null;
			mBundleRequest = null;
		}

		public bool ProcessLoad()
		{
			if (mBundleRequest != null && mBundleRequest.isDone)
			{
				if (mEventDelegate == null)
				{
					return true;
				}
				if (mSendFinalEventCountdown < 1)
				{
					try
					{
						UnityEngine.Object asset = mBundleRequest.asset;
						if (asset != null)
						{
							try
							{
								mEventDelegate(mURL, RsResourceLoadEvent.COMPLETE, 1f, asset, mUserData);
							}
							catch (Exception ex)
							{
								Dictionary<string, string> dictionary = new Dictionary<string, string>();
								dictionary.Add("URL", mURL);
								dictionary.Add("Error Text", ex.Message);
								Crashes.TrackError(ex, dictionary);
								UtDebug.LogError("System exception caught: " + ex.ToString());
							}
						}
						else
						{
							try
							{
								UtDebug.LogError("ERROR: Asset not found for async load from bundle: " + mURL);
								mEventDelegate(mURL, RsResourceLoadEvent.ERROR, 0f, null, mUserData);
							}
							catch (Exception ex2)
							{
								Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
								dictionary2.Add("URL", mURL);
								dictionary2.Add("Error Text", ex2.Message);
								Crashes.TrackError(ex2, dictionary2);
								UtDebug.LogError("System exception caught: " + ex2.ToString());
							}
						}
						return true;
					}
					catch (Exception ex3)
					{
						Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
						dictionary3.Add("URL", mURL);
						dictionary3.Add("Error Text", ex3.Message);
						Crashes.TrackError(ex3, dictionary3);
						UtDebug.LogError("System exception caught: " + mURL + " : " + ex3.ToString());
						return true;
					}
				}
				mSendFinalEventCountdown--;
			}
			return false;
		}
	}

	public const int SEND_FINAL_EVENT_COUNTDOWN = 1;

	public static int debugLevel = 80;

	private static Dictionary<string, AssetVersion> mVersionList = new Dictionary<string, AssetVersion>();

	private static Dictionary<string, AssetVersion> mDefaultVersionList = new Dictionary<string, AssetVersion>();

	private static GetProductRulesResponse mProductRuleData = null;

	private static UnityWebRequest mProductRuleRequest = null;

	private static Action mAction = null;

	private static bool mSuspendLoading = false;

	private static bool mDoneAppXML = false;

	private static GameObject mInstance = null;

	private static VersionsDownloadState mVersionDownloadState = VersionsDownloadState.NONE;

	private LinkedList<UtIWWWAsync> mPendingProcessList = new LinkedList<UtIWWWAsync>();

	private LinkedList<UtAsyncOperation> mPendingLoadList = new LinkedList<UtAsyncOperation>();

	private List<UtIWWWAsync> mPendingProcessBundleList = new List<UtIWWWAsync>();

	private List<UnityWebRequest> mWebRequestDisposeList = new List<UnityWebRequest>();

	public static Dictionary<string, AssetVersion> pVersionList => mVersionList;

	public static Dictionary<string, AssetVersion> pDefaultVersionList => mDefaultVersionList;

	public static UtWWWAsync pInstance
	{
		get
		{
			UtWWWAsync utWWWAsync = null;
			if (mInstance == null)
			{
				mInstance = new GameObject("WWWAsyncDownloader");
				UnityEngine.Object.DontDestroyOnLoad(mInstance);
				utWWWAsync = mInstance.AddComponent<UtWWWAsync>();
				UtMobileUtilities.AddToPersistentScriptList(utWWWAsync);
			}
			else
			{
				utWWWAsync = mInstance.GetComponent<UtWWWAsync>();
			}
			return utWWWAsync;
		}
	}

	public static int pPendingProcessCount => pInstance.mPendingProcessList.Count;

	public static int pPendingBundleProcessCount => pInstance.mPendingProcessBundleList.Count;

	public static bool pIsVersionDownloadComplete => mVersionDownloadState == VersionsDownloadState.DONE;

	public static int pPendingLoadCount => pInstance.mPendingLoadList.Count;

	private void Start()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	public static void InitializeAssetVersionLists(bool force = false)
	{
		if (mVersionDownloadState <= VersionsDownloadState.NONE || force)
		{
			mVersionDownloadState = VersionsDownloadState.DONE;
			Cache currentCacheForWriting = Caching.currentCacheForWriting;
			currentCacheForWriting.expirationDelay = 2592000;
			LoadDefaultVersionList();
			mVersionDownloadState = VersionsDownloadState.ASSET_VERSION_DOWNLOADING;
			Load(ProductConfig.pInstance.GetDataURL("") + "/" + ProductSettings.pInstance._AssetVersionFileName, RsResourceType.XML, CDNAssetVersionEventHandler, inSendProgressEvents: false, inDisableCache: false, inDownLoadOnly: false, inIgnoreAssetVersion: true);
		}
	}

	private static void CDNAssetVersionEventHandler(UtAsyncEvent inEvent, UtIWWWAsync inWWWInterface)
	{
		switch (inEvent)
		{
		case UtAsyncEvent.COMPLETE:
			CreateVersionList(UtUtilities.DeserializeFromXml<AssetVersionList>(inWWWInterface.pData).AssetVersions);
			mVersionDownloadState = VersionsDownloadState.DONE;
			break;
		case UtAsyncEvent.ERROR:
			mVersionDownloadState = VersionsDownloadState.DONE;
			break;
		}
	}

	public static UtIWWWAsync Load(string inURL, RsResourceType inType, UtWWWEventHandler inCallback = null, bool inSendProgressEvents = false, bool inDisableCache = false, bool inDownLoadOnly = false, bool inIgnoreAssetVersion = false)
	{
		UtIWWWAsync utIWWWAsync = new WWWProcess();
		pInstance.mPendingProcessList.AddLast(utIWWWAsync);
		if (inType == RsResourceType.ASSET_BUNDLE)
		{
			pInstance.mPendingProcessBundleList.Add(utIWWWAsync);
		}
		UtDebug.Log("*****UtWWWAsync downloading " + inURL, debugLevel);
		utIWWWAsync.Download(inURL, inType, inCallback, inSendProgressEvents, inDisableCache, inDownLoadOnly, inIgnoreAssetVersion);
		return utIWWWAsync;
	}

	public static UtIWWWAsync LoadBundle(string url, Hash128 hash, UtWWWEventHandler callback = null, bool sendProgressEvents = false, bool disableCache = false, bool downloadOnly = false)
	{
		UtIWWWAsync utIWWWAsync = new WWWProcess();
		pInstance.mPendingProcessList.AddLast(utIWWWAsync);
		pInstance.mPendingProcessBundleList.Add(utIWWWAsync);
		UtDebug.Log("*****WWWAsync downloading " + url, debugLevel);
		utIWWWAsync.DownloadBundle(url, hash, callback, sendProgressEvents, disableCache, downloadOnly);
		return utIWWWAsync;
	}

	public static UtIWWWAsync Post(string inURL, WWWForm inForm, UtWWWEventHandler inCallback, bool inSendProgressEvents)
	{
		UtIWWWAsync utIWWWAsync = new WWWProcess();
		pInstance.mPendingProcessList.AddLast(utIWWWAsync);
		UtDebug.Log("*****UtWWWAsync posting to " + inURL + "\n" + WebUtility.UrlDecode(Encoding.ASCII.GetString(inForm.data, 0, inForm.data.Length)), debugLevel);
		utIWWWAsync.PostForm(inURL, inForm, inCallback, inSendProgressEvents);
		return utIWWWAsync;
	}

	public static void SuspendLoad(bool suspend)
	{
		mSuspendLoading = suspend;
		if (mSuspendLoading)
		{
			return;
		}
		foreach (WWWProcess mPendingProcessBundle in pInstance.mPendingProcessBundleList)
		{
			if (mPendingProcessBundle.pDownloadStarted)
			{
				mPendingProcessBundle.StartDownload(mPendingProcessBundle.pIsDownloadOnly);
			}
		}
		foreach (UtAsyncOperation mPendingLoad in pInstance.mPendingLoadList)
		{
			if (mPendingLoad.pLoadPending)
			{
				mPendingLoad.StartLoad();
			}
		}
	}

	public static void Unload(string inURL)
	{
		LinkedListNode<UtIWWWAsync> linkedListNode = null;
		for (LinkedListNode<UtIWWWAsync> linkedListNode2 = pInstance.mPendingProcessList.First; linkedListNode2 != null; linkedListNode2 = linkedListNode)
		{
			linkedListNode = linkedListNode2.Next;
			if (linkedListNode2.Value != null && linkedListNode2.Value.pURL == inURL)
			{
				pInstance.mPendingProcessList.Remove(linkedListNode2);
				if (linkedListNode2.Value.pResourcetype == RsResourceType.ASSET_BUNDLE)
				{
					pInstance.mPendingProcessBundleList.Remove(linkedListNode2.Value);
				}
				if (linkedListNode2.Value.pWebRequest != null)
				{
					linkedListNode2.Value.Kill();
					break;
				}
			}
		}
	}

	public static void DebugDump()
	{
		string text = "=== UtWWWAsync Pending List ===\n";
		LinkedListNode<UtIWWWAsync> linkedListNode = pInstance.mPendingProcessList.First;
		while (linkedListNode != null)
		{
			LinkedListNode<UtIWWWAsync> next = linkedListNode.Next;
			text = ((linkedListNode.Value == null) ? (text + "NULL ENTRY\n") : ((linkedListNode.Value.pURL == null) ? (text + "NULL URL\n") : (text + linkedListNode.Value.pURL + "\n")));
			linkedListNode = next;
		}
		UtDebug.Log(text);
		text = "=== Ut Pending Load List ===\n";
		LinkedListNode<UtAsyncOperation> linkedListNode2 = pInstance.mPendingLoadList.First;
		while (linkedListNode2 != null)
		{
			LinkedListNode<UtAsyncOperation> next2 = linkedListNode2.Next;
			text = ((linkedListNode2.Value == null) ? (text + "NULL ENTRY\n") : ((linkedListNode2.Value.mURL == null) ? (text + "NULL URL\n") : (text + linkedListNode2.Value.mURL + "\n")));
			linkedListNode2 = next2;
		}
		UtDebug.Log(text);
	}

	public static void KillAsyncLoads()
	{
		foreach (UtAsyncOperation mPendingLoad in pInstance.mPendingLoadList)
		{
			mPendingLoad?.Kill();
		}
		pInstance.mPendingLoadList.Clear();
	}

	public static void KillAllProcess()
	{
		if (pInstance.mPendingProcessList == null)
		{
			return;
		}
		foreach (UtIWWWAsync mPendingProcess in pInstance.mPendingProcessList)
		{
			mPendingProcess?.Kill();
		}
		pInstance.mPendingProcessList.Clear();
		pInstance.mPendingProcessBundleList.Clear();
	}

	public static bool KillProcess(string inURL)
	{
		if (pInstance.mPendingProcessList == null)
		{
			return false;
		}
		bool result = false;
		LinkedListNode<UtIWWWAsync> linkedListNode = pInstance.mPendingProcessList.First;
		while (linkedListNode != null)
		{
			LinkedListNode<UtIWWWAsync> next = linkedListNode.Next;
			if (linkedListNode.Value != null && RsResourceManager.ParseURL(linkedListNode.Value.pURL, useLocalAsset: false) == inURL)
			{
				result = true;
				linkedListNode.Value.Kill();
				pInstance.mPendingProcessList.Remove(linkedListNode);
				if (linkedListNode.Value.pURL != null && linkedListNode.Value.pResourcetype == RsResourceType.ASSET_BUNDLE)
				{
					pInstance.mPendingProcessBundleList.Remove(linkedListNode.Value);
				}
			}
			linkedListNode = next;
		}
		return result;
	}

	public static UtIWWWAsync GetProcessFor(string inURL)
	{
		if (pInstance.mPendingProcessList == null)
		{
			return null;
		}
		UtIWWWAsync result = null;
		foreach (UtIWWWAsync mPendingProcess in pInstance.mPendingProcessList)
		{
			if (mPendingProcess != null && mPendingProcess.pWebRequest != null && mPendingProcess.pURL == inURL)
			{
				result = mPendingProcess;
				break;
			}
		}
		return result;
	}

	public static void AsyncLoad(string inURL, AssetBundle inBundle, string inAssetName, Type inType, RsResourceEventHandler inCallback, object inUserData = null)
	{
		UtAsyncOperation value = new UtAsyncOperation(inURL, inBundle, inAssetName, inType, inCallback, inUserData);
		pInstance.mPendingLoadList.AddLast(value);
	}

	private IEnumerator ProcessAsyncOperation(UtAsyncOperation inOperation)
	{
		while (!inOperation.ProcessLoad())
		{
			yield return 0;
		}
	}

	private void Update()
	{
		if (mPendingProcessList != null)
		{
			LinkedListNode<UtIWWWAsync> linkedListNode = mPendingProcessList.First;
			while (linkedListNode != null)
			{
				LinkedListNode<UtIWWWAsync> next = linkedListNode.Next;
				if (linkedListNode.Value != null)
				{
					if (linkedListNode.Value.Update())
					{
						mPendingProcessList.Remove(linkedListNode);
						if (linkedListNode.Value.pURL != null && linkedListNode.Value.pResourcetype == RsResourceType.ASSET_BUNDLE)
						{
							mPendingProcessBundleList.Remove(linkedListNode.Value);
						}
						if (linkedListNode.Value.pWebRequest != null)
						{
							linkedListNode.Value.Kill();
						}
					}
				}
				else
				{
					mPendingProcessList.Remove(linkedListNode);
				}
				linkedListNode = next;
			}
		}
		if (mPendingLoadList != null)
		{
			LinkedListNode<UtAsyncOperation> linkedListNode2 = mPendingLoadList.First;
			while (linkedListNode2 != null)
			{
				LinkedListNode<UtAsyncOperation> next2 = linkedListNode2.Next;
				if (linkedListNode2.Value != null)
				{
					if (linkedListNode2.Value.ProcessLoad())
					{
						mPendingLoadList.Remove(linkedListNode2);
					}
				}
				else
				{
					mPendingLoadList.Remove(linkedListNode2);
				}
				linkedListNode2 = next2;
			}
		}
		if (mWebRequestDisposeList == null)
		{
			return;
		}
		for (int i = 0; i < mWebRequestDisposeList.Count; i++)
		{
			if (mWebRequestDisposeList[i].isDone)
			{
				mWebRequestDisposeList[i].Dispose();
				mWebRequestDisposeList.RemoveAt(i);
				i--;
			}
		}
	}

	public static void LoadDefaultVersionList()
	{
		if (pDefaultVersionList.Count != 0)
		{
			return;
		}
		string text = null;
		text += "_Standalone";
		TextAsset textAsset = RsResourceManager.LoadAssetFromResources(ProductSettings.pInstance._AssetVersionFileName.Replace(".xml", text)) as TextAsset;
		if (!(textAsset != null))
		{
			return;
		}
		AssetVersionList assetVersionList = UtUtilities.DeserializeFromXml<AssetVersionList>(textAsset.text);
		if (assetVersionList != null && assetVersionList.AssetVersions != null)
		{
			AssetVersion[] assetVersions = assetVersionList.AssetVersions;
			foreach (AssetVersion assetVersion in assetVersions)
			{
				pDefaultVersionList.Add(assetVersion.AssetName, assetVersion);
			}
		}
		else
		{
			Debug.LogError("ERROR: default version list could not be deserialized!!");
		}
	}

	public static bool CreateVersionList(AssetVersion[] inData)
	{
		if (inData != null)
		{
			foreach (AssetVersion assetVersion in inData)
			{
				string text = RemovePathFromAssetVersion(assetVersion);
				if (pVersionList.ContainsKey(text))
				{
					UtDebug.LogWarning("AssetVersion already exists for " + text + "!");
				}
				else
				{
					pVersionList.Add(text, assetVersion);
				}
			}
			return true;
		}
		return false;
	}

	public static string RemovePathFromAssetVersion(AssetVersion inAssetVersion)
	{
		return inAssetVersion.AssetName.Replace("RS_DATA/", "").Replace("RS_SCENE/", "").Replace("RS_SHARED/", "")
			.Replace("RS_CONTENT/", "")
			.Replace("RS_SOUND/", "")
			.Replace("RS_MOVIES/", "");
	}

	public static string ProcessLocale(string inURL, AssetVersion av, bool useLocalAsset)
	{
		string text = "";
		if (av.Variants.Length > 1)
		{
			text = ".en-us";
			AssetVersion.Variant variant = Array.Find(av.Variants, (AssetVersion.Variant x) => x.Locale.Equals(UtUtilities.GetLocaleLanguage(), StringComparison.OrdinalIgnoreCase));
			if (variant != null && !variant.Locale.Equals("en-us", StringComparison.OrdinalIgnoreCase))
			{
				text = "." + variant.Locale;
			}
		}
		int num = inURL.LastIndexOf('.');
		if (num != -1)
		{
			string text2 = inURL.Substring(num);
			inURL = inURL.Replace(text2, text + text2);
		}
		inURL = RsResourceManager.ParseURL(inURL, useLocalAsset);
		return inURL;
	}

	public static bool IsLocalPath(string filePath)
	{
		if (filePath.ToLower().Contains("file:"))
		{
			return true;
		}
		return false;
	}

	public static void AddToWebRequestDisposeList(UnityWebRequest webRequest)
	{
		pInstance.mWebRequestDisposeList.Add(webRequest);
	}

	private void OnSceneLoaded(Scene inScene, LoadSceneMode inLoadSceneMode)
	{
		LinkedListNode<UtIWWWAsync> linkedListNode = null;
		for (LinkedListNode<UtIWWWAsync> linkedListNode2 = pInstance.mPendingProcessList.First; linkedListNode2 != null; linkedListNode2 = linkedListNode)
		{
			linkedListNode = linkedListNode2.Next;
			if (linkedListNode2.Value != null)
			{
				linkedListNode2.Value.OnSceneLoaded(inScene.name);
			}
		}
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}
}
