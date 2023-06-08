using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using KA.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RsResourceManager
{
	private class BundleResource : Resource
	{
		private string mBundleName;

		public BundleResource(string bundleURL, RsResourceEventHandler callback = null, bool dontDestroy = false, bool disableCache = false, bool downloadOnly = false, object userData = null)
			: base(bundleURL, callback, RsResourceType.ASSET_BUNDLE, dontDestroy, disableCache, downloadOnly, inIgnoreAssetVersion: false, userData)
		{
			mBundleName = GetUnityBundleName(bundleURL);
		}

		public override void Load()
		{
			if (mManifestLoadState != ManifestLoadState.LOADED)
			{
				LoadManifests(OnManifestsLoaded);
			}
			else
			{
				LoadDependencies(GetCurrentBundleVariant(mBundleName), OnDependenciesLoaded, mDontDestroy);
			}
		}

		private void OnManifestsLoaded(bool success)
		{
			if (success)
			{
				LoadDependencies(mBundleName, OnDependenciesLoaded, mDontDestroy);
			}
			else
			{
				EventHandler(UtAsyncEvent.ERROR, null);
			}
		}

		private void OnDependenciesLoaded(RsAssetLoader assetLoader, RsResourceLoadEvent loadEvent, float progress, object userData)
		{
			if (loadEvent == RsResourceLoadEvent.COMPLETE || loadEvent == RsResourceLoadEvent.ERROR)
			{
				string currentBundleVariant = GetCurrentBundleVariant(mBundleName);
				UtWWWAsync.LoadBundle(hash: GetBundleHash(currentBundleVariant), url: ResolveURL(currentBundleVariant), callback: base.EventHandler, sendProgressEvents: true, disableCache: mDisableCache, downloadOnly: mDownloadOnly);
			}
			if (loadEvent == RsResourceLoadEvent.ERROR)
			{
				UtDebug.LogError("ERROR Loading Dependant Bundle: " + mBundleName);
			}
		}

		public override bool Release(bool forceRelease = false, bool ignoreLoading = false)
		{
			bool num = base.Release(forceRelease);
			if (num)
			{
				UnloadDependencies(mBundleName);
			}
			return num;
		}
	}

	private class ManifestBundleResource : Resource
	{
		public ManifestBundleResource(string bundleURL, RsResourceEventHandler callback)
			: base(bundleURL, callback, RsResourceType.ASSET_BUNDLE, inDontDestroy: true, inDisableCache: true, inDownloadOnly: false, inIgnoreAssetVersion: true)
		{
		}

		public override void Load()
		{
			UtWWWAsync.LoadBundle(ResolveURL(mURL), default(Hash128), base.EventHandler, sendProgressEvents: true, disableCache: true);
		}
	}

	private class Resource
	{
		protected string mURL = "";

		private RsResourceType mType;

		private object mResource;

		private bool mAdditive;

		protected bool mDontDestroy;

		protected bool mDownloadOnly;

		protected bool mDisableCache;

		private bool mIgnoreAssetVersion;

		private bool mLoading;

		private bool mLoadCancelled;

		private RsResourceEventHandler mEventDelegate;

		private AssetList mAssets;

		private int mRefCount;

		private uint mLogMask = 20u;

		private int mMaxTextureSize = 1000;

		private object mUserData;

		public object pUserdata => mUserData;

		public string pURL => mURL;

		public object pResource => mResource;

		public bool pAdditive
		{
			get
			{
				return mAdditive;
			}
			set
			{
				mAdditive = value;
			}
		}

		public bool pDontDestroy
		{
			get
			{
				return mDontDestroy;
			}
			set
			{
				mDontDestroy = value;
			}
		}

		public bool pDownloadOnly
		{
			get
			{
				return mDownloadOnly;
			}
			set
			{
				mDownloadOnly = value;
			}
		}

		public bool pIsLoading => mLoading;

		public int pRefCount
		{
			get
			{
				return mRefCount;
			}
			set
			{
				mRefCount = value;
			}
		}

		public Resource(string inURL, object inResource, bool inDontDestroy)
		{
			mURL = inURL;
			mResource = inResource;
			mDontDestroy = inDontDestroy;
			pRefCount++;
			SetSourceInfo();
		}

		public Resource(string inURL, RsResourceEventHandler inCallback, RsResourceType inType, bool inDontDestroy = false, bool inDisableCache = false, bool inDownloadOnly = false, bool inIgnoreAssetVersion = false, object inUserData = null)
		{
			mUserData = inUserData;
			mURL = inURL;
			mType = inType;
			mDontDestroy = inDontDestroy;
			mDownloadOnly = inDownloadOnly;
			mDisableCache = inDisableCache;
			mIgnoreAssetVersion = inIgnoreAssetVersion;
			mEventDelegate = inCallback;
			mLoading = true;
			pRefCount++;
			SetSourceInfo();
		}

		public virtual void Load()
		{
			UtWWWAsync.Load(ResolveURL(mURL), mType, EventHandler, inSendProgressEvents: true, mDisableCache, mDownloadOnly, mIgnoreAssetVersion);
		}

		private void SetSourceInfo()
		{
		}

		private void Reset()
		{
			pRefCount = 0;
			mURL = null;
			mType = RsResourceType.NONE;
			mResource = null;
			mDontDestroy = false;
			mEventDelegate = null;
			mAssets = null;
		}

		public virtual bool Release(bool forceRelease = false, bool ignoreLoading = false)
		{
			pRefCount--;
			if (pRefCount < 0)
			{
				pRefCount = 0;
			}
			bool flag = forceRelease || pRefCount == 0;
			if (mLoading && !ignoreLoading)
			{
				UtDebug.Log("NOTE: Loading in progess, can not release resource, all callbacks terminated instead - URL: " + mURL);
				mDontDestroy = false;
				return false;
			}
			AssetBundle assetBundle = mResource as AssetBundle;
			if (mType == RsResourceType.ASSET_BUNDLE)
			{
				if (flag)
				{
					if (assetBundle != null)
					{
						assetBundle.Unload(unloadAllLoadedObjects: true);
						Reset();
					}
					return true;
				}
			}
			else if (mType == RsResourceType.IMAGE)
			{
				if (flag)
				{
					Texture texture = mResource as Texture;
					if (texture != null)
					{
						UnityEngine.Object.Destroy(texture);
					}
					Reset();
					return true;
				}
			}
			else if (mType == RsResourceType.XML)
			{
				Reset();
				return true;
			}
			return false;
		}

		public bool ReleaseBundleData()
		{
			if (mLoading)
			{
				return false;
			}
			AssetBundle assetBundle = mResource as AssetBundle;
			if (assetBundle != null)
			{
				assetBundle.Unload(unloadAllLoadedObjects: false);
				UnloadUnusedAssets();
				return true;
			}
			return false;
		}

		public void ReleaseLoadProcessingData(bool force)
		{
			mEventDelegate = null;
			mAssets = null;
			if (mDownloadOnly || force)
			{
				mDownloadOnly = false;
				if (!mDontDestroy || force)
				{
					mLoadedResources.Remove(this);
				}
			}
		}

		public void RemoveLoadProcessingDataCallback()
		{
			mEventDelegate = null;
			mAssets = null;
		}

		public void AddAsset(string inAsset, RsResourceEventHandler inCallback, Type inAssetType, object inUserData)
		{
			if (!string.IsNullOrEmpty(inAsset))
			{
				if (mAssets == null)
				{
					mAssets = new AssetList();
				}
				mAssets.AddAsset(inAsset, inCallback, inAssetType, inUserData);
			}
		}

		private bool HasDelegate(RsResourceEventHandler inCallback)
		{
			if (mEventDelegate != null)
			{
				Delegate[] invocationList = mEventDelegate.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					if (invocationList[i].Equals(inCallback))
					{
						return true;
					}
				}
			}
			return false;
		}

		public void AddDelegate(RsResourceEventHandler inCallback)
		{
			mEventDelegate = (RsResourceEventHandler)Delegate.Combine(mEventDelegate, inCallback);
		}

		protected void EventHandler(UtAsyncEvent inEvent, UtIWWWAsync inFileReader)
		{
			if (mLoadCancelled)
			{
				UtDebug.LogError("ERROR: Resource.EventHandler called after load has been cancelled!!", 10);
				return;
			}
			object @object = GetObject(inFileReader, inEvent);
			if (inEvent == UtAsyncEvent.COMPLETE && @object == null && !mDownloadOnly)
			{
				inEvent = UtAsyncEvent.ERROR;
			}
			switch (inEvent)
			{
			case UtAsyncEvent.PROGRESS:
				if (mEventDelegate != null)
				{
					mEventDelegate(mURL, RsResourceLoadEvent.PROGRESS, inFileReader.pProgress, @object, mUserData);
				}
				if (mAssets != null)
				{
					mAssets.Process(mURL, RsResourceLoadEvent.PROGRESS, inFileReader.pProgress, @object);
				}
				break;
			case UtAsyncEvent.COMPLETE:
				mResource = @object;
				mLoading = false;
				if (@object is Texture2D)
				{
					LogResourceInfo(mResource);
				}
				if (mEventDelegate != null)
				{
					mEventDelegate(mURL, RsResourceLoadEvent.COMPLETE, inFileReader.pProgress, @object, mUserData);
				}
				if (mAssets != null)
				{
					mAssets.Process(mURL, RsResourceLoadEvent.COMPLETE, inFileReader.pProgress, @object);
				}
				ReleaseLoadProcessingData(force: false);
				break;
			case UtAsyncEvent.ERROR:
				mLoading = false;
				if (mEventDelegate != null)
				{
					mEventDelegate(mURL, RsResourceLoadEvent.ERROR, 0f, null, mUserData);
				}
				if (mAssets != null)
				{
					mAssets.Process(mURL, RsResourceLoadEvent.ERROR, 0f, null);
				}
				ReleaseLoadProcessingData(force: true);
				break;
			}
		}

		private void LogResourceInfo(object obj)
		{
			Texture2D texture2D = (Texture2D)obj;
			if (texture2D.width >= mMaxTextureSize || texture2D.height >= mMaxTextureSize)
			{
				UtDebug.Log("Texture Url: " + mURL, mLogMask);
				UtDebug.Log("Texture Width: " + texture2D.width + " Texture Height: " + texture2D.height, mLogMask);
				UtDebug.Log("Texture format: " + texture2D.format, mLogMask);
				UtDebug.Log("Texture Quality: " + ProductConfig.GetBundleQuality(), mLogMask);
			}
		}

		private object GetObject(UtIWWWAsync inFileReader, UtAsyncEvent inEvent)
		{
			if (inFileReader == null)
			{
				return null;
			}
			if (inFileReader.pIsDone)
			{
				if (!string.IsNullOrEmpty(inFileReader.pError))
				{
					return null;
				}
				if (mDownloadOnly && !mDontDestroy && inEvent == UtAsyncEvent.COMPLETE)
				{
					if (mType == RsResourceType.ASSET_BUNDLE)
					{
						AssetBundle pAssetBundle = inFileReader.pAssetBundle;
						if (pAssetBundle != null)
						{
							pAssetBundle.Unload(unloadAllLoadedObjects: true);
						}
					}
					return null;
				}
				return mType switch
				{
					RsResourceType.ASSET_BUNDLE => inFileReader.pAssetBundle, 
					RsResourceType.XML => inFileReader.pData, 
					RsResourceType.IMAGE => inFileReader.pTexture, 
					RsResourceType.TEXT_ASSET => inFileReader.pData, 
					RsResourceType.AUDIO => inFileReader.pAudioClip, 
					RsResourceType.NONSTREAMING_AUDIO => inFileReader.pAudioClip, 
					RsResourceType.BINARY => inFileReader.pBytes, 
					_ => inFileReader.pBytes, 
				};
			}
			return mType switch
			{
				RsResourceType.AUDIO => inFileReader.pAudioClip, 
				RsResourceType.NONSTREAMING_AUDIO => null, 
				_ => null, 
			};
		}
	}

	private class AssetList
	{
		private class Asset
		{
			public string mName = "";

			public Type mType;

			public RsResourceEventHandler mEventDelegate;

			public object mUserData;

			public Asset(string inName, RsResourceEventHandler inCallback, Type inType, object inUserData)
			{
				mName = inName;
				mType = inType;
				mEventDelegate = inCallback;
				mUserData = inUserData;
			}

			public void Process(string inBundleURL, RsResourceLoadEvent inEvent, float inProgress, object inBundle)
			{
				if (mEventDelegate == null)
				{
					return;
				}
				string inURL = string.Empty;
				if (!string.IsNullOrEmpty(inBundleURL) && !string.IsNullOrEmpty(mName))
				{
					inURL = inBundleURL + "/" + mName;
				}
				if (inBundleURL == null)
				{
					inBundleURL = string.Empty;
				}
				if (mName == null)
				{
					mName = string.Empty;
				}
				switch (inEvent)
				{
				case RsResourceLoadEvent.PROGRESS:
					mEventDelegate(inURL, RsResourceLoadEvent.PROGRESS, inProgress, null, mUserData);
					break;
				case RsResourceLoadEvent.COMPLETE:
					if (inBundle == null)
					{
						mEventDelegate(inURL, RsResourceLoadEvent.ERROR, 1f, inBundle, mUserData);
					}
					else if (mType == null || !inBundle.GetType().Equals(typeof(AssetBundle)))
					{
						mEventDelegate(inURL, RsResourceLoadEvent.COMPLETE, 1f, inBundle, mUserData);
					}
					else
					{
						UtWWWAsync.AsyncLoad(inURL, (AssetBundle)inBundle, mName, mType, mEventDelegate, mUserData);
					}
					break;
				case RsResourceLoadEvent.ERROR:
					mEventDelegate(inURL, RsResourceLoadEvent.ERROR, 0f, null, mUserData);
					break;
				}
			}
		}

		private List<Asset> mList = new List<Asset>();

		public void AddAsset(string inName, RsResourceEventHandler inCallback, Type inType, object inUserData)
		{
			mList.Add(new Asset(inName, inCallback, inType, inUserData));
		}

		public void Process(string inBundleURL, RsResourceLoadEvent inEvent, float inProgress, object inBundle)
		{
			foreach (Asset m in mList)
			{
				m.Process(inBundleURL, inEvent, inProgress, inBundle);
			}
		}
	}

	private class ResourceList : LinkedList<Resource>
	{
		public LinkedListNode<Resource> FindNode(string inURL)
		{
			for (LinkedListNode<Resource> linkedListNode = base.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
			{
				if (linkedListNode.Value != null && linkedListNode.Value.pURL == inURL)
				{
					return linkedListNode;
				}
			}
			return null;
		}
	}

	public const string DATA_FOLDER = "data";

	public const string SCENE_FOLDER = "scene";

	public const string CONTENT_DATA_FOLDER = "contentdata";

	public const string SHARED_DATA_FOLDER = "shareddata";

	public const string SOUND_FOLDER = "sound";

	public const string MOVIE_FOLDER = "movies";

	private const float MAX_PROGRESS = 0.85f;

	public const string LOAD_PROGRESS_TASK = "LevelLoad";

	private const int LOG_PRIORITY = 10;

	private static ResourceList mLoadedResources;

	private static ResourceList mLastAdditiveResources;

	private static BundleResource mCurrentLevelResource;

	private static BundleResource mLastLevelResource;

	private static string mCurrentLevel;

	private static string mLastLevel;

	private static string mLevelToLoad;

	private static string mLevelAdditiveToLoad;

	private static GameObject mLevelProgress;

	private static bool mSkipMMOLogin;

	private const string MANIFEST_NAME = "AssetBundleManifest";

	private static List<AssetBundleManifest> mManifests;

	private const string DEFAULT_VARIANT = "en-us";

	private static ManifestLoadState mManifestLoadState;

	private static Dictionary<string, List<string>> mBundlesWithVariant;

	private static string[] mManifestBundlesNames;

	private static List<ManifestLoadCallback> mManifestLoadCallbacks;

	private static StringBuilder mBundleNameBuilder;

	private static bool mIsTransition;

	private static bool mEditorSimulateMobile;

	private static bool mUnloadUnusedAssets;

	private static bool mLoadingLevel;

	private static bool mForceDisableLevelReload;

	public static bool mReleaseBundleData;

	private static string[] BUCKET_ASSETS_TAG;

	private static string mBundleID;

	public static Dictionary<string, List<string>> mResourceBucket;

	public static ManifestLoadState ManifestLoadState => mManifestLoadState;

	public static bool pIsTransition => mIsTransition;

	public static string pCurrentLevel => mCurrentLevel;

	public static string pLastLevel => mLastLevel;

	public static string pLevelToLoad
	{
		get
		{
			return mLevelToLoad;
		}
		set
		{
			mLevelToLoad = value;
		}
	}

	public static bool pLevelLoading => mLevelToLoad != "";

	public static bool pLevelAdditiveLoading => mLevelAdditiveToLoad != "";

	public static bool pLevelLoadingScreen => mLevelProgress != null;

	public static bool pForceDisableLevelReload
	{
		get
		{
			return mForceDisableLevelReload;
		}
		set
		{
			mForceDisableLevelReload = value;
		}
	}

	public static string pDefaultBucketID { get; set; }

	public static event RsResourceLoadLevelStarted LoadLevelStarted;

	public static event RsResourceLoadLevelStarted LoadLevelCompleted;

	static RsResourceManager()
	{
		mLoadedResources = new ResourceList();
		mLastAdditiveResources = new ResourceList();
		mCurrentLevelResource = null;
		mLastLevelResource = null;
		mCurrentLevel = "";
		mLastLevel = "";
		mLevelToLoad = "";
		mLevelAdditiveToLoad = "";
		mLevelProgress = null;
		mSkipMMOLogin = false;
		mManifests = new List<AssetBundleManifest>();
		mManifestLoadState = ManifestLoadState.NOT_LOADED;
		mBundlesWithVariant = new Dictionary<string, List<string>>();
		mManifestLoadCallbacks = new List<ManifestLoadCallback>();
		mBundleNameBuilder = new StringBuilder(100);
		mIsTransition = false;
		mEditorSimulateMobile = false;
		RsResourceManager.LoadLevelStarted = null;
		RsResourceManager.LoadLevelCompleted = null;
		mUnloadUnusedAssets = false;
		mLoadingLevel = false;
		mForceDisableLevelReload = false;
		mReleaseBundleData = false;
		BUCKET_ASSETS_TAG = new string[1] { "RS_SOUND" };
		mBundleID = string.Empty;
		mResourceBucket = new Dictionary<string, List<string>>();
		mCurrentLevel = SceneManager.GetActiveScene().name;
	}

	public static string GetUnityBundleName(string jsBundleName)
	{
		mBundleNameBuilder.Length = 0;
		mBundleNameBuilder.Append(jsBundleName);
		mBundleNameBuilder.Replace("RS_CONTENT", "contentdata");
		mBundleNameBuilder.Replace("RS_DATA", "data");
		mBundleNameBuilder.Replace("RS_SCENE", "scene");
		mBundleNameBuilder.Replace("RS_SHARED", "shareddata");
		mBundleNameBuilder.Replace("RS_SOUND", "sound");
		mBundleNameBuilder.Replace("RS_MOVIES", "movies");
		return mBundleNameBuilder.ToString();
	}

	public static string FormatBundleURL(string bundleURL)
	{
		mBundleNameBuilder.Length = 0;
		mBundleNameBuilder.Append(bundleURL);
		if (bundleURL.StartsWith("contentdata/"))
		{
			mBundleNameBuilder.Remove(0, "contentdata/".Length);
			mBundleNameBuilder.Insert(0, "RS_CONTENT/");
		}
		else if (bundleURL.StartsWith("data/"))
		{
			mBundleNameBuilder.Remove(0, "data/".Length);
			mBundleNameBuilder.Insert(0, "RS_DATA/");
		}
		else if (bundleURL.StartsWith("scene/"))
		{
			mBundleNameBuilder.Remove(0, "scene/".Length);
			mBundleNameBuilder.Insert(0, "RS_SCENE/");
		}
		else if (bundleURL.StartsWith("shareddata/"))
		{
			mBundleNameBuilder.Remove(0, "shareddata/".Length);
			mBundleNameBuilder.Insert(0, "RS_SHARED/");
		}
		else if (bundleURL.StartsWith("sound/"))
		{
			mBundleNameBuilder.Remove(0, "sound/".Length);
			mBundleNameBuilder.Insert(0, "RS_SOUND/");
		}
		else if (bundleURL.StartsWith("movies/"))
		{
			mBundleNameBuilder.Remove(0, "movies/".Length);
			mBundleNameBuilder.Insert(0, "RS_MOVIES/");
		}
		int num = mBundleNameBuilder.ToString().LastIndexOf(".unity3d");
		if (num != -1)
		{
			mBundleNameBuilder.Remove(num, ".unity3d".Length);
		}
		if (mBundleNameBuilder.ToString().StartsWith("RS_") && mBundleNameBuilder.ToString().LastIndexOf('.') == -1)
		{
			string[] array = mBundleNameBuilder.ToString().Split('/');
			if (array.Length > 1)
			{
				array[1] = array[1].ToLowerInvariant();
			}
			string text = "";
			for (int i = 0; i < array.Length; i++)
			{
				text = text + array[i] + ((i < array.Length - 1) ? "/" : "");
			}
			mBundleNameBuilder.Length = 0;
			mBundleNameBuilder.Append(text);
			UtDebug.Log("Bundle Url is formatted to: " + mBundleNameBuilder.ToString(), 100);
		}
		return mBundleNameBuilder.ToString();
	}

	public static bool IsBundle(string assetName)
	{
		return !assetName.Contains(".");
	}

	public static bool Compare(string url1, string url2)
	{
		url1 = FormatBundleURL(url1);
		url2 = FormatBundleURL(url2);
		return url1 == url2;
	}

	public static bool IsBundleCached(string bundleNameWithoutVariant)
	{
		if (mManifestLoadState != ManifestLoadState.LOADED)
		{
			return false;
		}
		string currentBundleVariant = GetCurrentBundleVariant(GetUnityBundleName(bundleNameWithoutVariant));
		if (GetManifestContainingBundle(currentBundleVariant, out var hash) == null)
		{
			return false;
		}
		return Caching.IsVersionCached(currentBundleVariant, hash);
	}

	private static bool IsManifest(string bundleName)
	{
		if (mManifestBundlesNames != null)
		{
			return Array.IndexOf(mManifestBundlesNames, bundleName) != -1;
		}
		return false;
	}

	private static void LoadManifests(ManifestLoadCallback manifestLoadCallback)
	{
		if (mManifestLoadState == ManifestLoadState.LOADED)
		{
			manifestLoadCallback(success: true);
			return;
		}
		if (mManifestLoadState == ManifestLoadState.LOADING)
		{
			mManifestLoadCallbacks.Add(manifestLoadCallback);
			return;
		}
		mManifestLoadState = ManifestLoadState.LOADING;
		mManifestLoadCallbacks.Add(manifestLoadCallback);
		List<string> list = new List<string>();
		list.Add("RS_ROOT/" + ProductSettings.pInstance._AppName.ToLowerInvariant());
		string[] manifests = ProductConfig.pInstance.Manifests;
		if (manifests != null)
		{
			string[] array = manifests;
			foreach (string text in array)
			{
				if (!string.IsNullOrEmpty(text))
				{
					list.Add("RS_ROOT/" + text);
				}
			}
		}
		mManifestBundlesNames = list.ToArray();
		new RsAssetLoader().Load(mManifestBundlesNames, null, OnManifestsLoaded, inDontDestroy: true, inIgnoreAssetVersion: true);
	}

	public static void ReInitManifest()
	{
		string[] array = mManifestBundlesNames;
		foreach (string inURL in array)
		{
			LinkedListNode<Resource> linkedListNode = mLoadedResources.FindNode(inURL);
			if (linkedListNode != null && linkedListNode.Value.Release(forceRelease: true))
			{
				mLoadedResources.Remove(linkedListNode);
			}
		}
		mManifestLoadCallbacks = new List<ManifestLoadCallback>();
		mManifests.Clear();
		mManifestLoadState = ManifestLoadState.NOT_LOADED;
		LoadManifests(null);
	}

	private static void OnManifestsLoaded(RsAssetLoader assetLoader, RsResourceLoadEvent loadEvent, float progress, object userDat)
	{
		switch (loadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			string[] pAssets = assetLoader.pAssets;
			for (int i = 0; i < pAssets.Length; i++)
			{
				AssetBundleManifest assetBundleManifest = LoadAssetFromBundle(pAssets[i], "AssetBundleManifest", typeof(AssetBundleManifest)) as AssetBundleManifest;
				if (!mManifests.Contains(assetBundleManifest))
				{
					mManifests.Add(assetBundleManifest);
					CacheVariants(assetBundleManifest);
				}
			}
			mManifestLoadState = ManifestLoadState.LOADED;
			foreach (ManifestLoadCallback mManifestLoadCallback in mManifestLoadCallbacks)
			{
				mManifestLoadCallback?.Invoke(success: true);
			}
			mManifestLoadCallbacks = null;
			break;
		}
		case RsResourceLoadEvent.ERROR:
			mManifestLoadState = ManifestLoadState.NOT_LOADED;
			foreach (ManifestLoadCallback mManifestLoadCallback2 in mManifestLoadCallbacks)
			{
				mManifestLoadCallback2?.Invoke(success: true);
			}
			mManifestLoadCallbacks.Clear();
			break;
		}
	}

	private static Hash128 GetBundleHash(string bundleName)
	{
		Hash128 hash = default(Hash128);
		GetManifestContainingBundle(bundleName, out hash);
		return hash;
	}

	public static AssetBundleManifest GetManifestContainingBundle(string bundleName)
	{
		Hash128 hash = default(Hash128);
		return GetManifestContainingBundle(bundleName, out hash);
	}

	public static AssetBundleManifest GetManifestContainingBundle(string bundleName, out Hash128 hash)
	{
		hash = default(Hash128);
		foreach (AssetBundleManifest mManifest in mManifests)
		{
			hash = mManifest.GetAssetBundleHash(bundleName);
			if (hash.isValid)
			{
				return mManifest;
			}
		}
		return null;
	}

	private static void CacheVariants(AssetBundleManifest manifest)
	{
		string[] allAssetBundlesWithVariant = manifest.GetAllAssetBundlesWithVariant();
		for (int i = 0; i < allAssetBundlesWithVariant.Length; i++)
		{
			SplitBundleNameAndVariant(allAssetBundlesWithVariant[i], out var bundleNameWithoutVariant, out var variant);
			if (!mBundlesWithVariant.TryGetValue(bundleNameWithoutVariant, out var value))
			{
				value = new List<string>();
			}
			value.Add(variant);
			mBundlesWithVariant[bundleNameWithoutVariant] = value;
		}
	}

	private static void SplitBundleNameAndVariant(string bundleNameWithVariant, out string bundleNameWithoutVariant, out string variant)
	{
		int num = bundleNameWithVariant.LastIndexOf(".");
		if (num < 0)
		{
			bundleNameWithoutVariant = bundleNameWithVariant;
			variant = string.Empty;
		}
		else
		{
			bundleNameWithoutVariant = bundleNameWithVariant.Substring(0, num);
			variant = bundleNameWithVariant.Substring(num + 1);
		}
	}

	private static void AddVariantSuffixes(string[] bundleNames)
	{
		int i = 0;
		for (int num = bundleNames.Length; i < num; i++)
		{
			bundleNames[i] = GetCurrentBundleVariant(bundleNames[i]);
		}
	}

	private static string GetCurrentBundleVariant(string bundleName)
	{
		SplitBundleNameAndVariant(bundleName, out var bundleNameWithoutVariant, out var _);
		if (mBundlesWithVariant.TryGetValue(bundleNameWithoutVariant, out var value))
		{
			string text = UtUtilities.GetLocaleLanguage().ToLower();
			int i = 0;
			for (int count = value.Count; i < count; i++)
			{
				if (text.Contains(value[i]))
				{
					return bundleNameWithoutVariant + "." + value[i];
				}
			}
			if (value.Contains("en-us"))
			{
				return bundleNameWithoutVariant + "." + "en-us";
			}
			return bundleNameWithoutVariant + "." + value[0];
		}
		return bundleName;
	}

	private static void LoadDependencies(string bundleName, RsAssetLoader.LoadEventHandler loadEventHandler, bool dontDestroyDependencies = false)
	{
		string[] dependencies = GetDependencies(bundleName);
		if (dependencies != null && dependencies.Length != 0)
		{
			AddVariantSuffixes(dependencies);
			new RsAssetLoader().Load(dependencies, null, loadEventHandler, dontDestroyDependencies);
		}
		else
		{
			loadEventHandler(null, RsResourceLoadEvent.COMPLETE, 1f, null);
		}
	}

	private static void UnloadDependencies(string bundleName)
	{
		string[] dependencies = GetDependencies(bundleName);
		if (dependencies != null && dependencies.Length != 0)
		{
			AddVariantSuffixes(dependencies);
			string[] array = dependencies;
			for (int i = 0; i < array.Length; i++)
			{
				Unload(array[i]);
			}
		}
	}

	public static string[] GetDependencies(string bundleName)
	{
		AssetBundleManifest manifestContainingBundle = GetManifestContainingBundle(bundleName);
		if (manifestContainingBundle != null)
		{
			return manifestContainingBundle.GetDirectDependencies(bundleName);
		}
		return null;
	}

	public static RsResourceLoadEvent GetCurrentLoadEvent(string inURL)
	{
		inURL = FormatBundleURL(inURL);
		LinkedListNode<Resource> linkedListNode = mLoadedResources.FindNode(inURL);
		if (linkedListNode != null)
		{
			if (linkedListNode.Value.pIsLoading)
			{
				return RsResourceLoadEvent.PROGRESS;
			}
			if (linkedListNode.Value.pResource != null)
			{
				return RsResourceLoadEvent.COMPLETE;
			}
			return RsResourceLoadEvent.ERROR;
		}
		return RsResourceLoadEvent.NONE;
	}

	public static bool IsResourceAvailable(string inURL)
	{
		if (mLoadedResources.FindNode(inURL) != null)
		{
			return true;
		}
		return false;
	}

	public static object PrefetchBundle(string inURL, RsResourceEventHandler inCallback, bool inDontDestroy, object inUserData)
	{
		inURL = FormatBundleURL(inURL);
		string[] array = inURL.Split('/')[^1].Split('.');
		if (mLastLevelResource != null && mLastLevelResource.pURL.Contains(array[0]))
		{
			inCallback?.Invoke(inURL, RsResourceLoadEvent.COMPLETE, 1f, null, inUserData);
			return null;
		}
		bool inDownloadOnly = true;
		if (array.Length == 2 && array[1].Equals("xml", StringComparison.OrdinalIgnoreCase))
		{
			inDownloadOnly = false;
		}
		return Load(inURL, inCallback, RsResourceType.NONE, inDontDestroy, inDisableCache: false, inDownloadOnly, inIgnoreAssetVersion: false, inUserData);
	}

	public static object Load(string inURL, RsResourceEventHandler inCallback, RsResourceType inType = RsResourceType.NONE, bool inDontDestroy = false, bool inDisableCache = false, bool inDownloadOnly = false, bool inIgnoreAssetVersion = false, object inUserData = null, bool ignoreReferenceCount = false)
	{
		if (string.IsNullOrEmpty(inURL))
		{
			return null;
		}
		inURL = FormatBundleURL(inURL);
		if (inType == RsResourceType.NONE)
		{
			string text = inURL;
			string text2 = string.Empty;
			int num = text.LastIndexOf('.');
			if (num != -1)
			{
				text2 = text.Substring(num + 1).ToLower();
			}
			if (ProductConfig.pInstance != null)
			{
				Locale[] locale = ProductConfig.pInstance.Locale;
				for (int i = 0; i < locale.Length; i++)
				{
					if (text2.Contains(locale[i].ID.ToLower()))
					{
						inType = RsResourceType.ASSET_BUNDLE;
					}
				}
			}
			switch (text2)
			{
			case "":
				inType = RsResourceType.ASSET_BUNDLE;
				break;
			case "xml":
				inType = RsResourceType.XML;
				break;
			case "jpg":
			case "png":
				inType = RsResourceType.IMAGE;
				break;
			case "ogg":
				inType = RsResourceType.AUDIO;
				break;
			case "txt":
				inType = RsResourceType.TEXT_ASSET;
				break;
			case "bin":
				inType = RsResourceType.BINARY;
				break;
			}
		}
		LinkedListNode<Resource> linkedListNode = mLoadedResources.FindNode(inURL);
		if (linkedListNode != null)
		{
			if (linkedListNode.Value.pDownloadOnly && inDownloadOnly)
			{
				if (linkedListNode.Value.pIsLoading)
				{
					linkedListNode.Value.pDontDestroy = inDontDestroy;
					linkedListNode.Value.pDownloadOnly = inDownloadOnly;
					if (inCallback != null)
					{
						string[] array = inURL.Split('/');
						if (array.Length == 3)
						{
							linkedListNode.Value.AddAsset(array[^1], inCallback, null, inUserData);
						}
						else
						{
							linkedListNode.Value.AddDelegate(inCallback);
						}
					}
				}
				return null;
			}
			if (!ignoreReferenceCount)
			{
				linkedListNode.Value.pRefCount++;
			}
			if (linkedListNode.Value.pIsLoading)
			{
				if (linkedListNode.Value.pDownloadOnly)
				{
					linkedListNode.Value.pDontDestroy = inDontDestroy;
					linkedListNode.Value.pDownloadOnly = false;
				}
				if (inCallback != null)
				{
					string[] array2 = inURL.Split('/');
					if (array2.Length == 3)
					{
						linkedListNode.Value.AddAsset(array2[^1], inCallback, null, inUserData);
					}
					else
					{
						linkedListNode.Value.AddDelegate(inCallback);
					}
				}
				return null;
			}
			inCallback?.Invoke(inURL, RsResourceLoadEvent.COMPLETE, 1f, linkedListNode.Value.pResource, inUserData);
			return linkedListNode.Value.pResource;
		}
		Resource resource = null;
		resource = ((inType != RsResourceType.ASSET_BUNDLE) ? new Resource(inURL, inCallback, inType, inDontDestroy, inDisableCache, inDownloadOnly, inIgnoreAssetVersion, inUserData) : ((!IsManifest(inURL)) ? ((Resource)new BundleResource(inURL, inCallback, inDontDestroy, inDisableCache, inDownloadOnly, inUserData)) : ((Resource)new ManifestBundleResource(inURL, inCallback))));
		mLoadedResources.AddLast(resource);
		resource.Load();
		return null;
	}

	public static object LoadAdditive(string inURL, RsResourceEventHandler inCallback, object inUserData)
	{
		inURL = FormatBundleURL(inURL);
		LinkedListNode<Resource> linkedListNode = mLastAdditiveResources.FindNode(inURL);
		if (linkedListNode != null)
		{
			mLoadedResources.AddLast(linkedListNode);
		}
		object result = Load(inURL, inCallback, RsResourceType.NONE, inDontDestroy: false, inDisableCache: false, inDownloadOnly: false, inIgnoreAssetVersion: false, inUserData);
		if (linkedListNode == null)
		{
			linkedListNode = mLoadedResources.FindNode(inURL);
			linkedListNode.Value.pAdditive = true;
		}
		return result;
	}

	private static IEnumerator LoadAdditiveEditor(string inURL, RsResourceEventHandler inCallback, object inUserData)
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		inCallback(inURL, RsResourceLoadEvent.COMPLETE, 1f, null, inUserData);
	}

	public static bool Unload(string inURL, bool splitURL = true, bool force = false, bool ignoreLoading = false)
	{
		if (string.IsNullOrEmpty(inURL))
		{
			return false;
		}
		inURL = FormatBundleURL(inURL);
		if (splitURL && !inURL.StartsWith("http://"))
		{
			string[] array = inURL.Split('/');
			if (array.Length >= 2)
			{
				inURL = array[0] + "/" + array[1];
			}
		}
		LinkedListNode<Resource> linkedListNode = mLoadedResources.FindNode(inURL);
		if (linkedListNode != null && linkedListNode.Value.Release(force, ignoreLoading))
		{
			mLoadedResources.Remove(linkedListNode);
			return true;
		}
		return false;
	}

	private static void ReleaseResources(bool inPostponeAdditiveRelease)
	{
		LinkedListNode<Resource> linkedListNode = mLoadedResources.First;
		while (linkedListNode != null)
		{
			LinkedListNode<Resource> next = linkedListNode.Next;
			bool flag = true;
			if (linkedListNode.Value != null)
			{
				if (inPostponeAdditiveRelease && linkedListNode.Value.pAdditive && !linkedListNode.Value.pDontDestroy)
				{
					mLastAdditiveResources.AddLast(linkedListNode.Value);
				}
				else if (linkedListNode.Value.pDontDestroy || linkedListNode.Value.pDownloadOnly)
				{
					flag = false;
					if (linkedListNode.Value.pDownloadOnly)
					{
						linkedListNode.Value.RemoveLoadProcessingDataCallback();
					}
				}
				else
				{
					flag = linkedListNode.Value.Release();
				}
			}
			if (flag)
			{
				mLoadedResources.Remove(linkedListNode);
			}
			linkedListNode = next;
		}
	}

	public static bool ReleaseBundleData(string inURL)
	{
		if (UtPlatform.IsMobile())
		{
			if (!mReleaseBundleData)
			{
				return false;
			}
			if (string.IsNullOrEmpty(inURL))
			{
				return false;
			}
			inURL = FormatBundleURL(inURL);
			string[] array = inURL.Split('/');
			if (array.Length >= 2)
			{
				inURL = array[0] + "/" + array[1];
			}
			LinkedListNode<Resource> linkedListNode = mLoadedResources.FindNode(inURL);
			if (linkedListNode != null && linkedListNode.Value.ReleaseBundleData())
			{
				mLoadedResources.Remove(linkedListNode);
				return true;
			}
		}
		return false;
	}

	public static List<string> GetResourcesList()
	{
		List<string> list = new List<string>();
		for (LinkedListNode<Resource> linkedListNode = mLoadedResources.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			string text = "";
			text = " : ref count = " + linkedListNode.Value.pRefCount;
			string item = linkedListNode.Value.pURL + text;
			list.Add(item);
		}
		return list;
	}

	public static bool IsLoading()
	{
		for (LinkedListNode<Resource> linkedListNode = mLoadedResources.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			if (linkedListNode.Value.pIsLoading)
			{
				return true;
			}
		}
		return false;
	}

	private static void ReleaseAdditiveResources()
	{
		foreach (Resource mLastAdditiveResource in mLastAdditiveResources)
		{
			mLastAdditiveResource?.Release(forceRelease: true);
		}
		mLastAdditiveResources.Clear();
	}

	public static bool SetDontDestroy(string inURL, bool inDontDestroy)
	{
		inURL = FormatBundleURL(inURL);
		LinkedListNode<Resource> linkedListNode = mLoadedResources.FindNode(inURL);
		if (linkedListNode != null)
		{
			linkedListNode.Value.pDontDestroy = inDontDestroy;
			return true;
		}
		return false;
	}

	public static object LoadAssetFromResources(string inAssetName, bool isPrefab = true)
	{
		object obj = Resources.Load(inAssetName);
		if (obj == null)
		{
			obj = LoadAssetFromBundle(ProductSettings.pInstance._Resource + inAssetName);
		}
		return obj;
	}

	public static object LoadAssetFromBundle(string inAssetPath, Type inType = null)
	{
		string[] array = inAssetPath.Split('/');
		UtDebug.Assert(array.Length == 3, "MALFORMED ASSET PATH: " + inAssetPath);
		if (array.Length == 3)
		{
			return LoadAssetFromBundle(array[0] + "/" + array[1], array[2], inType);
		}
		return null;
	}

	public static object LoadAssetFromBundle(string inBundlePath, string inAssetName, Type inType)
	{
		inBundlePath = FormatBundleURL(inBundlePath);
		LinkedListNode<Resource> linkedListNode = mLoadedResources.FindNode(inBundlePath);
		if (linkedListNode != null)
		{
			if (!linkedListNode.Value.pIsLoading)
			{
				if (linkedListNode.Value.pResource is AssetBundle assetBundle && assetBundle != null)
				{
					linkedListNode.Value.pRefCount++;
					if (inType != null)
					{
						return assetBundle.LoadAsset(inAssetName, inType);
					}
					return assetBundle.LoadAsset(inAssetName);
				}
				UtDebug.LogError("ERROR: Object not an AssetBundle: " + inBundlePath, 10);
			}
			else
			{
				UtDebug.LogError("ERROR: Object reference is null: " + inBundlePath, 10);
			}
		}
		return null;
	}

	public static object LoadAssetFromBundle(string inBundleURL, RsResourceEventHandler inCallback, Type inType, bool inDontDestroy = false, object inUserData = null)
	{
		string[] array = inBundleURL.Split('/');
		if (array.Length == 3)
		{
			return LoadAssetFromBundle(array[0] + "/" + array[1], array[2], inCallback, inType, inDontDestroy, inUserData);
		}
		return null;
	}

	public static object LoadAssetFromBundle(string inBundleURL, string inAssetName, RsResourceEventHandler inCallback, Type inType, bool inDontDestroy = false, object inUserData = null)
	{
		inBundleURL = FormatBundleURL(inBundleURL);
		LinkedListNode<Resource> linkedListNode = mLoadedResources.FindNode(inBundleURL);
		if (linkedListNode != null)
		{
			if (linkedListNode.Value.pIsLoading)
			{
				if (inCallback != null)
				{
					linkedListNode.Value.AddAsset(inAssetName, inCallback, inType, inUserData);
				}
				linkedListNode.Value.pRefCount++;
				return null;
			}
			object pResource = linkedListNode.Value.pResource;
			if (pResource != null)
			{
				if (pResource.GetType().Equals(typeof(AssetBundle)))
				{
					AssetBundle assetBundle = (AssetBundle)pResource;
					if (assetBundle != null)
					{
						linkedListNode.Value.pRefCount++;
						string inURL = inBundleURL + "/" + inAssetName;
						if (inType == null)
						{
							UtWWWAsync.AsyncLoad(inURL, assetBundle, inAssetName, typeof(UnityEngine.Object), inCallback, inUserData);
						}
						else
						{
							UtWWWAsync.AsyncLoad(inURL, assetBundle, inAssetName, inType, inCallback, inUserData);
						}
					}
					else
					{
						UtDebug.LogError("ERROR: NULL Bundle: " + inBundleURL, 10);
					}
				}
				else
				{
					UtDebug.LogError("ERROR: Object not an AssetBundle: " + inBundleURL, 10);
				}
			}
			else
			{
				string inURL2 = inBundleURL + "/" + inAssetName;
				inCallback(inURL2, RsResourceLoadEvent.ERROR, 0f, null, inUserData);
				UtDebug.LogError("ERROR: Object reference is null: " + inBundleURL, 10);
			}
			return null;
		}
		AddToBucket(inBundleURL);
		BundleResource bundleResource = new BundleResource(inBundleURL);
		bundleResource.Load();
		bundleResource.AddAsset(inAssetName, inCallback, inType, inUserData);
		mLoadedResources.AddLast(bundleResource);
		return null;
	}

	public static void LoadLevel(string inLevel, bool skipMMOLogin = false)
	{
		if (mLevelToLoad != "")
		{
			return;
		}
		SnChannel.KillAll();
		mSkipMMOLogin = skipMMOLogin;
		mLevelToLoad = inLevel;
		if (mLevelProgress == null)
		{
			UnityEngine.Object @object = (UtPlatform.IsMobile() ? ((GameObject)LoadAssetFromResources("PfUiLoadScreenMobile")) : ((GameObject)LoadAssetFromResources("PfUiLoadScreen")));
			if (@object != null)
			{
				mLevelProgress = UnityEngine.Object.Instantiate(@object) as GameObject;
				mLevelProgress.name = "PfUiLoadScreen";
				UnityEngine.Object.DontDestroyOnLoad(mLevelProgress);
			}
			else
			{
				mLevelProgress = null;
			}
		}
		if (mLevelProgress != null)
		{
			AddLoadProgressTask("LevelLoad");
			AddLoadProgressTask("CommonLevel");
			AvLoadingUI component = mLevelProgress.GetComponent<AvLoadingUI>();
			if (component != null)
			{
				if (AvAvatar.pObject != null)
				{
					Transform marker = mLevelProgress.transform.Find("PfMarkerAvatar");
					component.SetupAvatar(marker);
				}
				component.SetLoadProcessor(LoadLevelReal, inLevel);
			}
			else
			{
				LoadLevelReal(inLevel);
			}
		}
		else
		{
			LoadLevelReal(inLevel);
		}
	}

	public static void LoadLevelAdditive(string levelName, object inUserData)
	{
		mLevelAdditiveToLoad = levelName;
		LoadAdditive("RS_SCENE/" + mLevelAdditiveToLoad, LoadLevelAdditiveCallback, inUserData);
	}

	private static void LoadLevelReal(string inLevel)
	{
		UtDebug.Assert(mLevelToLoad == inLevel, "Level to load does not match what is passed to LoadLevelReal!");
		if (RsResourceManager.LoadLevelStarted != null)
		{
			RsResourceManager.LoadLevelStarted(inLevel);
		}
		ReleaseResources(inPostponeAdditiveRelease: true);
		UtWWWAsync.KillAsyncLoads();
		if (mCurrentLevel != inLevel)
		{
			mLastLevel = mCurrentLevel;
		}
		mIsTransition = true;
		SceneManager.LoadScene("Transition");
	}

	public static void ProcessTransitionLevel()
	{
		UtDebug.Assert(!string.IsNullOrEmpty(mLevelToLoad), "Level to load is null when calling ProcessTransitionLevel!");
		if (mLastLevelResource != null)
		{
			mLastLevelResource.Release(forceRelease: true);
		}
		if (mCurrentLevelResource != null)
		{
			mCurrentLevelResource.Release(forceRelease: true);
		}
		mLastLevelResource = (mCurrentLevelResource = null);
		UnloadUnusedAssets(canGCCollect: true);
	}

	public static void TransitionToLevel()
	{
		mIsTransition = false;
		ProcessLoadLevel(mLevelToLoad);
	}

	private static void ProcessLoadLevel(string inLevel)
	{
		ReleaseResources(inPostponeAdditiveRelease: true);
		if (!UtPlatform.IsEditor() || mEditorSimulateMobile)
		{
			if (Array.IndexOf(ProductConfig.pNonBundledLevels, inLevel) < 0)
			{
				string text = "RS_SCENE/" + inLevel.ToLowerInvariant();
				Resource resource = null;
				if (mCurrentLevelResource != null && mCurrentLevelResource.pURL == text)
				{
					resource = mCurrentLevelResource;
				}
				if (resource == null && mLastLevelResource != null && mLastLevelResource.pURL == text)
				{
					resource = mLastLevelResource;
					mLastLevelResource = mCurrentLevelResource;
					mCurrentLevelResource = resource as BundleResource;
				}
				if (resource != null)
				{
					if (resource.pIsLoading)
					{
						UtDebug.LogError("ERROR: Attempting to load a level that is still loading from a preload: " + resource.pURL, 10);
						resource.AddDelegate(LoadLevelCallback);
					}
					else
					{
						ProcessLevelLoad(inLevel);
					}
					return;
				}
				UpdateLoadProgress("LevelLoad", 0.01f);
				if (mLastLevelResource != null)
				{
					mLastLevelResource.Release(forceRelease: true);
				}
				mLastLevelResource = mCurrentLevelResource;
				LinkedListNode<Resource> linkedListNode = mLoadedResources.FindNode(text);
				if (linkedListNode != null)
				{
					if (linkedListNode.Value.pIsLoading)
					{
						if (linkedListNode.Value.pDownloadOnly)
						{
							mCurrentLevelResource = linkedListNode.Value as BundleResource;
							mCurrentLevelResource.pDontDestroy = false;
							mCurrentLevelResource.pDownloadOnly = false;
							mCurrentLevelResource.AddDelegate(LoadLevelCallback);
							mLoadedResources.Remove(linkedListNode);
						}
					}
					else
					{
						UtDebug.LogError("ERROR: Attempting to load a level that is loaded in the preFetch loading: " + text, 10);
					}
				}
				else
				{
					mCurrentLevelResource = new BundleResource(text, LoadLevelCallback);
					mCurrentLevelResource.Load();
				}
			}
			else
			{
				if (mLastLevelResource != null)
				{
					mLastLevelResource.Release(forceRelease: true);
				}
				mLastLevelResource = mCurrentLevelResource;
				mCurrentLevelResource = null;
				ProcessLevelLoad(inLevel);
			}
		}
		else
		{
			ProcessLevelLoad(inLevel);
		}
	}

	public static object PreloadLevel(string inLevel, RsResourceEventHandler inCallback)
	{
		if (!UtPlatform.IsEditor() || mEditorSimulateMobile)
		{
			string text = "RS_SCENE/" + inLevel;
			Resource resource = null;
			if (mCurrentLevelResource != null && mCurrentLevelResource.pURL == text)
			{
				if (mCurrentLevelResource.pIsLoading)
				{
					UtDebug.LogError("ERROR: Attempting to preload a level that is still loading: " + text);
					mCurrentLevelResource.AddDelegate(inCallback.Invoke);
					return null;
				}
				resource = mCurrentLevelResource;
			}
			if (mLastLevelResource != null && mLastLevelResource.pURL == text)
			{
				if (mLastLevelResource.pIsLoading)
				{
					mLastLevelResource.AddDelegate(inCallback.Invoke);
					return null;
				}
				resource = mLastLevelResource;
			}
			if (resource != null)
			{
				object pResource = resource.pResource;
				inCallback?.Invoke(text, RsResourceLoadEvent.COMPLETE, 1f, pResource, null);
				return pResource;
			}
			if (mLastLevelResource != null)
			{
				mLastLevelResource.Release();
			}
			mLastLevelResource = new BundleResource(text, inCallback.Invoke);
			mLastLevelResource.Load();
		}
		return null;
	}

	public static void AddLoadProgressTask(string inTask)
	{
		if (mLevelProgress != null)
		{
			mLevelProgress.SendMessage("AddTask", inTask);
		}
	}

	public static void UpdateLoadProgress(string inTask, float inProgress)
	{
		if (mLevelProgress != null)
		{
			mLevelProgress.SendMessage("UpdateTask", new KeyValuePair<string, float>(inTask, inProgress));
		}
	}

	public static bool DestroyLoadScreen()
	{
		if (RsResourceManager.LoadLevelCompleted != null)
		{
			RsResourceManager.LoadLevelCompleted(mCurrentLevel);
		}
		UnloadUnusedAssets(canGCCollect: true);
		if (mLevelProgress != null)
		{
			Component[] components = mLevelProgress.GetComponents<MonoBehaviour>();
			Component[] array = components;
			if (array != null)
			{
				components = array;
				foreach (Component component in components)
				{
					MethodInfo method = component.GetType().GetMethod("DestroyLoadScreen");
					if (method != null && !(bool)method.Invoke(component, null))
					{
						return false;
					}
				}
			}
			ReleaseAdditiveResources();
			UnityEngine.Object.Destroy(mLevelProgress);
			mLevelProgress = null;
		}
		return true;
	}

	private static void LoadLevelAsyncCallback(string inLevel, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			mSkipMMOLogin = false;
			mLevelToLoad = "";
			mLoadingLevel = false;
			UnloadSceneData();
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("***************** Error downloading ****************" + inLevel, 10);
			break;
		case RsResourceLoadEvent.PROGRESS:
			break;
		}
	}

	private static void ProcessLevelLoad(string inLevel)
	{
		UpdateLoadProgress("LevelLoad", 1f);
		if (mCurrentLevel != inLevel)
		{
			mLastLevel = mCurrentLevel;
			mCurrentLevel = inLevel;
		}
		UtWWWAsync.KillAsyncLoads();
		mLoadingLevel = true;
		if (!mSkipMMOLogin)
		{
			MainStreetMMOPlugin.LoadLevel(mCurrentLevel);
		}
		AsyncOperation newTarget = SceneManager.LoadSceneAsync(inLevel);
		RsAsyncMonitor.pInstance.AddTarget(newTarget, inLevel, LoadLevelAsyncCallback);
	}

	public static void UnloadSceneData()
	{
		if (mCurrentLevelResource != null)
		{
			AssetBundle assetBundle = mCurrentLevelResource.pResource as AssetBundle;
			if (assetBundle != null)
			{
				assetBundle.Unload(unloadAllLoadedObjects: false);
				UnloadUnusedAssets(canGCCollect: true);
			}
			else
			{
				UtDebug.LogError("Could not unload scene data. :: URL :: " + mCurrentLevelResource.pURL);
			}
		}
	}

	public static void UnloadUnusedAssets(bool canGCCollect = false)
	{
		if (!mUnloadUnusedAssets)
		{
			mUnloadUnusedAssets = true;
			UtBehaviour.RunCoroutine(UnloadAssets(canGCCollect));
		}
	}

	private static IEnumerator UnloadAssets(bool canGCCollect = false)
	{
		while (UtWWWAsync.pPendingLoadCount > 0 || UtWWWAsync.pPendingBundleProcessCount > 0 || mLoadingLevel || UtWWWAsync.pPendingProcessCount > 0)
		{
			yield return null;
		}
		UtWWWAsync.SuspendLoad(suspend: true);
		AsyncOperation op = Resources.UnloadUnusedAssets();
		while (!op.isDone)
		{
			yield return null;
		}
		if (canGCCollect)
		{
			GC.Collect();
		}
		UtWWWAsync.SuspendLoad(suspend: false);
		mUnloadUnusedAssets = false;
	}

	private static void LoadLevelCallback(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.PROGRESS:
			UpdateLoadProgress("LevelLoad", inProgress);
			break;
		case RsResourceLoadEvent.COMPLETE:
			ProcessLevelLoad(mLevelToLoad);
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("***************** Error downloading ****************" + inURL, 10);
			mLevelToLoad = "";
			mSkipMMOLogin = false;
			break;
		}
	}

	private static void LoadLevelAdditiveCallback(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			SceneManager.LoadScene(mLevelAdditiveToLoad, LoadSceneMode.Additive);
			mLevelAdditiveToLoad = "";
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Loading Additive level " + mLevelAdditiveToLoad + " failed", 10);
			mLevelAdditiveToLoad = "";
			break;
		}
	}

	public static string ResolveURL(string inURL)
	{
		if (!string.IsNullOrEmpty(inURL))
		{
			if (inURL.IndexOf("file://") == 0 || inURL.IndexOf("http://") == 0)
			{
				return inURL;
			}
			inURL = FormatBundleURL(inURL);
			if (ProductConfig.pInstance == null)
			{
				ProductConfig.Init();
			}
			if (!UtPlatform.IsMobile())
			{
				return ParseURL(inURL, useLocalAsset: false);
			}
			return inURL;
		}
		UtDebug.LogError("ERROR: ResolveURL URL is null");
		return "";
	}

	public static string ParseURL(string inURL, bool useLocalAsset)
	{
		if (string.IsNullOrEmpty(inURL))
		{
			return null;
		}
		inURL = FormatBundleURL(inURL);
		if (inURL.IndexOf("RS_ROOT") == 0)
		{
			if (useLocalAsset)
			{
				return inURL.Replace("RS_ROOT", ProductConfig.pInstance.GetLocalRootURL(inURL));
			}
			return inURL.Replace("RS_ROOT", ProductConfig.pInstance.GetRootURL(inURL));
		}
		if (inURL.IndexOf("RS_CONTENT") == 0)
		{
			if (useLocalAsset)
			{
				return inURL.Replace("RS_CONTENT", ProductConfig.pInstance.GetLocalContentDataURL(inURL));
			}
			return inURL.Replace("RS_CONTENT", ProductConfig.pInstance.GetContentDataURL(inURL));
		}
		if (inURL.IndexOf("RS_DATA") == 0)
		{
			if (useLocalAsset)
			{
				return inURL.Replace("RS_DATA", ProductConfig.pInstance.GetLocalDataURL(inURL));
			}
			return inURL.Replace("RS_DATA", ProductConfig.pInstance.GetDataURL(inURL));
		}
		if (inURL.IndexOf("RS_SCENE") == 0)
		{
			if (useLocalAsset)
			{
				return inURL.Replace("RS_SCENE", ProductConfig.pInstance.GetLocalSceneURL(inURL));
			}
			return inURL.Replace("RS_SCENE", ProductConfig.pInstance.GetSceneURL(inURL));
		}
		if (inURL.IndexOf("RS_SOUND") == 0)
		{
			if (useLocalAsset)
			{
				return inURL.Replace("RS_SOUND", ProductConfig.pInstance.GetLocalSoundURL(inURL));
			}
			return inURL.Replace("RS_SOUND", ProductConfig.pInstance.GetSoundURL(inURL));
		}
		if (inURL.IndexOf("RS_SHARED") == 0)
		{
			if (useLocalAsset)
			{
				return inURL.Replace("RS_SHARED", ProductConfig.pInstance.GetLocalSharedDataURL(inURL));
			}
			return inURL.Replace("RS_SHARED", ProductConfig.pInstance.GetSharedDataURL(inURL));
		}
		if (inURL.IndexOf("RS_MOVIES") == 0)
		{
			if (useLocalAsset)
			{
				return inURL.Replace("RS_MOVIES", ProductConfig.pInstance.GetLocalMoviesURL(inURL));
			}
			return inURL.Replace("RS_MOVIES", ProductConfig.pInstance.GetMoviesURL(inURL));
		}
		if (inURL.IndexOf("RS_APP") == 0)
		{
			return inURL.Replace("RS_APP", ProductConfig.pInstance.AppURL);
		}
		return inURL;
	}

	public static void AddToBucket(string inBundleURL)
	{
		if (!string.IsNullOrEmpty(pDefaultBucketID) && !inBundleURL.Equals(mBundleID) && ShouldTrackAsset(inBundleURL))
		{
			AddEntry(pDefaultBucketID, inBundleURL);
		}
	}

	private static bool ShouldTrackAsset(string inBundleURL)
	{
		bool result = false;
		for (int i = 0; i < BUCKET_ASSETS_TAG.Length; i++)
		{
			if (inBundleURL.Contains(BUCKET_ASSETS_TAG[i]))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public static object BucketLoadAssetFromBundle(string inBucketId, string inBundleURL, RsResourceEventHandler inCallback, Type inType, bool inDontDestroy = false, object inUserData = null)
	{
		AddEntry(inBucketId, inBundleURL);
		return LoadAssetFromBundle(inBundleURL, inCallback, inType, inDontDestroy, inUserData);
	}

	public static object BucketLoadAssetFromBundle(string inBucketId, string inBundleURL, string inAssetName, RsResourceEventHandler inCallback, Type inType, bool inDontDestroy = false, object inUserData = null)
	{
		AddEntry(inBucketId, inBundleURL);
		return LoadAssetFromBundle(inBundleURL, inAssetName, inCallback, inType, inDontDestroy, inUserData);
	}

	public static object BucketLoadAssetFromBundle(string inBucketId, string inAssetPath, Type inType = null)
	{
		AddEntry(inBucketId, inAssetPath);
		return LoadAssetFromBundle(inAssetPath, inType);
	}

	public static object BucketLoad(string inBucketId, string inURL, RsResourceEventHandler inCallback, RsResourceType inType = RsResourceType.NONE, bool inDontDestroy = false, bool inDisableCache = false, bool inDownloadOnly = false, bool inIgnoreAssetVersion = false, object inUserData = null)
	{
		AddEntry(inBucketId, inURL);
		return Load(inURL, inCallback, inType, inDontDestroy, inDisableCache, inDownloadOnly, inIgnoreAssetVersion, inUserData);
	}

	private static void AddEntry(string inBucketId, string inBundleURL)
	{
		mBundleID = inBundleURL;
		if (mResourceBucket.ContainsKey(inBucketId))
		{
			List<string> list = mResourceBucket[inBucketId];
			if (!mResourceBucket[inBucketId].Contains(inBundleURL))
			{
				list.Add(inBundleURL);
				mResourceBucket[inBucketId] = list;
			}
		}
		else
		{
			List<string> list2 = new List<string>();
			list2.Add(inBundleURL);
			mResourceBucket.Add(inBucketId, list2);
		}
	}

	public static void Release(string inBucketID, bool inUnloadUnusedAssets = true)
	{
		UtDebug.LogWarning("@@ Releaseing Resource for: " + inBucketID);
		if (mResourceBucket.ContainsKey(inBucketID))
		{
			List<string> list = mResourceBucket[inBucketID];
			for (int i = 0; i < list.Count; i++)
			{
				Unload(list[i]);
			}
			mResourceBucket[inBucketID].Clear();
		}
		if (inUnloadUnusedAssets)
		{
			UnloadUnusedAssets();
		}
	}

	public static void ReleaseAllBucket()
	{
		foreach (KeyValuePair<string, List<string>> item in mResourceBucket)
		{
			Release(item.Key, inUnloadUnusedAssets: false);
		}
		UnloadUnusedAssets();
	}
}
