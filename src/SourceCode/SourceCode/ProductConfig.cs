using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using KA.Framework;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "ProductConfig", Namespace = "")]
public class ProductConfig
{
	public string ContentServerURL;

	public string ContentServerV2URL;

	public string ContentServerV3URL;

	public string ContentServerV4URL;

	public string AnalyticsServerURL;

	public string ConfigurationServerURL;

	public string MembershipServerURL;

	public string AuthenticationServerURL;

	public string AuthenticationServerV3URL;

	public string AvatarWebServiceURL;

	public string MessagingServerURL;

	public string AchievementServerURL;

	public string AchievementServerV2URL;

	public string ItemStoreServerURL;

	public string MissionServerURL;

	public string SubscriptionServerURL;

	public string FacebookServerURL;

	public string TrackServerURL;

	public string ScoreServerURL;

	public string RatingServerURL;

	public string RatingServerV2URL;

	public string LocaleServerURL;

	public string LocaleServerV2URL;

	public string UserServerURL;

	public string MessageServerURL;

	public string MessageServerV2URL;

	public string MessageServerV3URL;

	public string ChatServerURL;

	public string ChallengeServerURL;

	public string InviteServerV2URL;

	public string RegistrationServerV3URL;

	public string RegistrationServerV4URL;

	public string PushNotificationURL;

	public string GroupServerURL;

	public string GroupServerV2URL;

	public string PaymentServerURL;

	public string PaymentServerV2URL;

	public string PrizeCodeServerURL;

	public string PrizeCodeServerV2URL;

	public string TokenExpiredURL;

	public LocaleString TokenExpiredText;

	public LocaleString LoginFromOtherLocationText;

	[XmlElement(ElementName = "Manifests")]
	public string[] Manifests;

	[XmlElement(ElementName = "RootURL")]
	public string[] RootURL;

	[XmlElement(ElementName = "ContentDataURL")]
	public string[] ContentDataURL;

	[XmlElement(ElementName = "DataURL")]
	public string[] DataURL;

	[XmlElement(ElementName = "SceneURL")]
	public string[] SceneURL;

	[XmlElement(ElementName = "SharedDataURL")]
	public string[] SharedDataURL;

	[XmlElement(ElementName = "SoundURL")]
	public string[] SoundURL;

	[XmlElement(ElementName = "MoviesURL")]
	public string[] MoviesURL;

	[XmlElement(ElementName = "LocalRootURL")]
	public string[] LocalRootURL;

	[XmlElement(ElementName = "LocalContentDataURL")]
	public string[] LocalContentDataURL;

	[XmlElement(ElementName = "LocalDataURL")]
	public string[] LocalDataURL;

	[XmlElement(ElementName = "LocalSceneURL")]
	public string[] LocalSceneURL;

	[XmlElement(ElementName = "LocalSharedDataURL")]
	public string[] LocalSharedDataURL;

	[XmlElement(ElementName = "LocalSoundURL")]
	public string[] LocalSoundURL;

	[XmlElement(ElementName = "LocalMoviesURL")]
	public string[] LocalMoviesURL;

	public string AppURL;

	public string Token;

	public string DevLogin;

	public string DevPass;

	public string ConsolePassword;

	public string MMOServer;

	public bool MMOForceVersion;

	public string MMOServerVersion;

	public int? MMOServerPort;

	public int? MMOIdleTimeout;

	public string LogEventServer;

	public bool? DisableCache;

	public LocaleString DisconnectText;

	public LocaleString DisconnectTitleText;

	public bool? UseUDP;

	public bool MMODebug;

	public int MMOZoneInfoUpdateInterval;

	public int? MMOHttpPort;

	public bool MMOUseBlueBox;

	public int? MMOBlueBoxPollingRate;

	public int? MMOUDPPollingRate;

	public bool? EnableErrorLog;

	public bool? EnableDebugLog;

	public LocaleString ProductRuleFailedText;

	[XmlElement(ElementName = "PlatformSettings")]
	public PlatformSettings[] PlatformSettings;

	[XmlElement(ElementName = "Locale")]
	public Locale[] Locale;

	public long UnityCacheSize;

	public bool? EnablePlayfab;

	private static string mAppName = "DWADragonsMain";

	private static string mProductName = "DWADragons";

	private static string mToken = string.Empty;

	private static string mApiKey = "68AA3C28-DDC9-4C3E-A9C7-334D9F46E966";

	private static string mSecret = "AF5989E1-80C4-4A77-A906-C026C52EE5CE";

	private static int mProductID = 51;

	private static int mProductGroupID = 9;

	private static string mProductVersion = "1.14.0";

	private static string mServerType = "NA";

	private static string[] mNonBundledLevels = new string[3] { "Startup", "SceneScripts", "Transition" };

	private static ProductConfig mInstance = null;

	private static bool mDevFlag = false;

	private static bool mTokenFromHTML = true;

	private static PlatformSettings mPlatformSettings = null;

	private static List<UserGraphicsSetting> mGraphicsSettings = null;

	private const string GraphicsSettingsKey = "GraphicsSettings";

	private static string mBundleQuality = null;

	private static string mEffectQuality = null;

	private static string mShadowQuality = null;

	public static string pAppName => mAppName;

	public static string pProductName => mProductName;

	public static string pToken
	{
		get
		{
			return mToken;
		}
		set
		{
			mToken = value;
		}
	}

	public static string pApiKey => mApiKey;

	public static string pSecret => mSecret;

	public static int pProductID => mProductID;

	public static int pProductGroupID => mProductGroupID;

	public static string pProductVersion => mProductVersion;

	public static string pServerType
	{
		get
		{
			return mServerType;
		}
		set
		{
			mServerType = value;
		}
	}

	public static string[] pNonBundledLevels => mNonBundledLevels;

	public static ProductConfig pInstance => mInstance;

	public static bool pIsReady
	{
		get
		{
			if (mInstance != null)
			{
				return mTokenFromHTML;
			}
			return false;
		}
	}

	public static bool pDevFlag
	{
		get
		{
			return mDevFlag;
		}
		set
		{
			mDevFlag = value;
		}
	}

	public static void Destroy()
	{
		mInstance = null;
	}

	private static string GetAppXMLName()
	{
		string text = ProductSettings.pInstance.pXMLPath;
		if (PlayerPrefs.HasKey("BuildServerType"))
		{
			string @string = PlayerPrefs.GetString("BuildServerType");
			if (!string.IsNullOrEmpty(@string))
			{
				if (string.Compare(@string, "dev", StringComparison.OrdinalIgnoreCase) == 0)
				{
					text = (UtPlatform.IsEditor() ? ProductSettings.pInstance.GetEnvironmentDetails(KA.Framework.Environment.DEV)._EditorMainXMLPath : ProductSettings.pInstance.GetEnvironmentDetails(KA.Framework.Environment.DEV)._MainXMLPath);
				}
				else if (string.Compare(@string, "qa", StringComparison.OrdinalIgnoreCase) == 0)
				{
					text = (UtPlatform.IsEditor() ? ProductSettings.pInstance.GetEnvironmentDetails(KA.Framework.Environment.QA)._EditorMainXMLPath : ProductSettings.pInstance.GetEnvironmentDetails(KA.Framework.Environment.QA)._MainXMLPath);
				}
				else if (string.Compare(@string, "staging", StringComparison.OrdinalIgnoreCase) == 0)
				{
					text = (UtPlatform.IsEditor() ? ProductSettings.pInstance.GetEnvironmentDetails(KA.Framework.Environment.STAGING)._EditorMainXMLPath : ProductSettings.pInstance.GetEnvironmentDetails(KA.Framework.Environment.STAGING)._MainXMLPath);
				}
				else if (string.Compare(@string, "live", StringComparison.OrdinalIgnoreCase) == 0)
				{
					text = (UtPlatform.IsEditor() ? ProductSettings.pInstance.GetEnvironmentDetails(KA.Framework.Environment.LIVE)._EditorMainXMLPath : ProductSettings.pInstance.GetEnvironmentDetails(KA.Framework.Environment.LIVE)._MainXMLPath);
				}
			}
		}
		if (!UtPlatform.IsEditor())
		{
			if (UtPlatform.IsAndroid())
			{
				text = text + "Android/" + mProductVersion + "/" + mAppName;
			}
			else if (UtPlatform.IsiOS())
			{
				text = text + "iOS/" + mProductVersion + "/" + mAppName;
			}
			else if (UtPlatform.IsWSA())
			{
				text = text + "WSA/" + mProductVersion + "/" + mAppName;
			}
			else if (UtPlatform.IsSteamMac())
			{
				text = text + "SteamMac/" + mProductVersion + "/" + mAppName;
			}
			else if (UtPlatform.IsSteamWindows())
			{
				text = text + "Steam/" + mProductVersion + "/" + mAppName;
			}
			else if (UtPlatform.IsStandaloneOSX())
			{
				text = text + "MAC/" + mProductVersion + "/" + mAppName;
			}
			else if (UtPlatform.IsStandaloneWindows())
			{
				text = text + "WIN/" + mProductVersion + "/" + mAppName;
			}
			else if (UtPlatform.IsXBox())
			{
				text = text + "XBox/" + mProductVersion + "/" + mAppName;
			}
		}
		return text;
	}

	public static ProductConfig Init()
	{
		if (mInstance == null)
		{
			mAppName = ProductSettings.pInstance._AppName;
			mNonBundledLevels = ProductSettings.pInstance._NonBundledLevels;
			ProductDetails productDetails = ProductSettings.pInstance.GetProductDetails();
			if (productDetails == null)
			{
				UtDebug.LogError("ERROR: Product settings missing for " + Application.platform.ToString() + " platform");
				return null;
			}
			mProductGroupID = ProductSettings.pInstance._ProductGroupID;
			mProductID = productDetails._ProductID;
			mProductName = productDetails._ProductName;
			mProductVersion = productDetails._ProductVersion;
			mSecret = productDetails._Secret;
			mApiKey = productDetails._ApiKey;
			string appXMLName = GetAppXMLName();
			if (!Application.isEditor)
			{
				if (string.IsNullOrEmpty(appXMLName))
				{
					UtDebug.LogError("Product Data XML file not found in path. Check settings in ProductSettings!");
					return null;
				}
				appXMLName += ".xml";
				UtWWWAsync.Load(appXMLName, RsResourceType.XML, EventHandler);
				return null;
			}
			if (!ProductSettings.pInstance._EnableEnvironmentSelectionInEditor)
			{
				appXMLName = mAppName;
			}
			if (File.Exists(Application.dataPath + "/Resources/" + appXMLName + ".xml"))
			{
				using (StreamReader textReader = File.OpenText(Application.dataPath + "/Resources/" + appXMLName + ".xml"))
				{
					mInstance = new XmlSerializer(typeof(ProductConfig)).Deserialize(textReader) as ProductConfig;
					pToken = mInstance.Token;
					SetupBundlePath();
				}
				WsWebService.Init();
				SetServerType();
				return mInstance;
			}
			return InitDefault();
		}
		return mInstance;
	}

	public static void SetupBundlePath()
	{
		string text = "file://" + Application.dataPath + "/../Bundles/" + UtPlatform.GetCurrentPlatformFolderSuffix() + "/" + ProductSettings.pInstance._AppName.ToLowerInvariant();
		if (mInstance.RootURL != null)
		{
			for (int i = 0; i < mInstance.RootURL.Length; i++)
			{
				if (mInstance.RootURL[i].StartsWith("."))
				{
					mInstance.RootURL[i] = mInstance.RootURL[i].Replace(".", text);
				}
			}
		}
		if (mInstance.ContentDataURL != null)
		{
			for (int i = 0; i < mInstance.ContentDataURL.Length; i++)
			{
				if (mInstance.ContentDataURL[i].StartsWith("."))
				{
					mInstance.ContentDataURL[i] = mInstance.ContentDataURL[i].Replace(".", text);
				}
			}
		}
		if (mInstance.DataURL != null)
		{
			for (int i = 0; i < mInstance.DataURL.Length; i++)
			{
				if (mInstance.DataURL[i].StartsWith("."))
				{
					mInstance.DataURL[i] = mInstance.DataURL[i].Replace(".", text);
				}
			}
		}
		if (mInstance.SceneURL != null)
		{
			for (int i = 0; i < mInstance.SceneURL.Length; i++)
			{
				if (mInstance.SceneURL[i].StartsWith("."))
				{
					mInstance.SceneURL[i] = mInstance.SceneURL[i].Replace(".", text);
				}
			}
		}
		if (mInstance.SharedDataURL != null)
		{
			for (int i = 0; i < mInstance.SharedDataURL.Length; i++)
			{
				if (mInstance.SharedDataURL[i].StartsWith("."))
				{
					mInstance.SharedDataURL[i] = mInstance.SharedDataURL[i].Replace(".", text);
				}
			}
		}
		if (mInstance.SoundURL != null)
		{
			for (int i = 0; i < mInstance.SoundURL.Length; i++)
			{
				if (mInstance.SoundURL[i].StartsWith("."))
				{
					mInstance.SoundURL[i] = mInstance.SoundURL[i].Replace(".", text);
				}
			}
		}
		if (mInstance.MoviesURL != null)
		{
			for (int i = 0; i < mInstance.MoviesURL.Length; i++)
			{
				if (mInstance.MoviesURL[i].StartsWith("."))
				{
					mInstance.MoviesURL[i] = mInstance.MoviesURL[i].Replace(".", text);
				}
			}
		}
		if (mInstance.AppURL != null && mInstance.AppURL.StartsWith("."))
		{
			mInstance.AppURL = mInstance.AppURL.Replace(".", "file://" + Application.dataPath);
		}
		UpdateLocalURLs(text);
		mInstance.UpdateURLs();
	}

	public static ProductConfig InitDefault()
	{
		mInstance = new ProductConfig();
		string text = string.Concat("file://" + Application.dataPath + "/../Bundles/", UtPlatform.GetCurrentPlatformFolderSuffix(), "/", ProductSettings.pInstance._AppName.ToLowerInvariant());
		mInstance.RootURL = new string[1] { text };
		mInstance.ContentDataURL = new string[1] { text + "/contentdata" };
		mInstance.DataURL = new string[1] { text + "/data" };
		mInstance.SceneURL = new string[1] { text + "/scene" };
		mInstance.SoundURL = new string[1] { text + "/sound" };
		mInstance.MoviesURL = new string[1] { text + "/movies" };
		mInstance.LocalRootURL = new string[1] { text };
		mInstance.LocalContentDataURL = new string[1] { text + "/contentdata" };
		mInstance.LocalDataURL = new string[1] { text + "/data" };
		mInstance.LocalSceneURL = new string[1] { text + "/scene" };
		mInstance.LocalSoundURL = new string[1] { text + "/sound" };
		mInstance.LocalMoviesURL = new string[1] { text + "/movies" };
		mInstance.AppURL = "file://" + Application.dataPath + "/";
		return mInstance;
	}

	public static void EventHandler(UtAsyncEvent inEvent, UtIWWWAsync inFileReader)
	{
		switch (inEvent)
		{
		case UtAsyncEvent.COMPLETE:
		{
			using StringReader stringReader = new StringReader(inFileReader.pData);
			mInstance = UtUtilities.DeserializeFromXml(TripleDES.DecryptASCII(stringReader.ReadToEnd(), ProductSettings.pInstance.pXMLSecret), typeof(ProductConfig)) as ProductConfig;
			if (!string.IsNullOrEmpty(mInstance.Token))
			{
				pToken = mInstance.Token;
			}
			else if (PlayerPrefs.HasKey("TOKEN"))
			{
				pToken = PlayerPrefs.GetString("TOKEN");
				PlayerPrefs.DeleteKey("TOKEN");
			}
			mInstance.CreatePlatformSpecificURL();
			mInstance.UpdateURLs();
			WsWebService.Init();
			SetServerType();
			break;
		}
		case UtAsyncEvent.ERROR:
			InitDefault();
			break;
		}
	}

	private void CreatePlatformSpecificURL()
	{
		if (UtPlatform.IsAndroid())
		{
			UpdateLocalURLs("jar:file://" + Application.dataPath + "!/assets");
			AppURL = AppURL.Replace(".", "file://" + Application.dataPath + "!/assets");
		}
		else if (UtPlatform.IsiOS())
		{
			UpdateLocalURLs("file://" + Application.dataPath + "/Raw");
			AppURL = mInstance.AppURL.Replace(".", "file://" + Application.dataPath + "/Raw");
		}
		else if (UtPlatform.IsStandaloneWindows())
		{
			UpdateLocalURLs("file://" + Application.dataPath + "/StreamingAssets");
			AppURL = mInstance.AppURL.Replace(".", "file://" + Application.dataPath + "/StreamingAssets");
		}
		else if (UtPlatform.IsStandaloneOSX())
		{
			UpdateLocalURLs("file://" + Application.streamingAssetsPath);
			AppURL = mInstance.AppURL.Replace(".", "file://" + Application.streamingAssetsPath);
		}
		else if (UtPlatform.IsWSA())
		{
			UpdateLocalURLs("file://" + Application.dataPath + "/StreamingAssets");
			AppURL = mInstance.AppURL.Replace(".", "file://" + Application.dataPath + "/StreamingAssets");
		}
		else if (UtPlatform.IsXBox())
		{
			UpdateLocalURLs(Application.dataPath + "/StreamingAssets");
			AppURL = mInstance.AppURL.Replace(".", Application.dataPath + "/StreamingAssets");
		}
	}

	private static string UpdateProductVersion(string inPath)
	{
		return inPath.Replace("{Version}", mProductVersion);
	}

	private void UpdateURLs()
	{
		int num = 0;
		if (mInstance.RootURL != null)
		{
			for (num = 0; num < mInstance.RootURL.Length; num++)
			{
				mInstance.RootURL[num] = UpdateProductVersion(mInstance.RootURL[num]);
			}
		}
		if (mInstance.ContentDataURL != null)
		{
			for (num = 0; num < mInstance.ContentDataURL.Length; num++)
			{
				mInstance.ContentDataURL[num] = UpdateProductVersion(mInstance.ContentDataURL[num]);
			}
		}
		if (mInstance.DataURL != null)
		{
			for (num = 0; num < mInstance.DataURL.Length; num++)
			{
				mInstance.DataURL[num] = UpdateProductVersion(mInstance.DataURL[num]);
			}
		}
		if (mInstance.SceneURL != null)
		{
			for (num = 0; num < mInstance.SceneURL.Length; num++)
			{
				mInstance.SceneURL[num] = UpdateProductVersion(mInstance.SceneURL[num]);
			}
		}
		if (mInstance.SharedDataURL != null)
		{
			for (num = 0; num < mInstance.SharedDataURL.Length; num++)
			{
				mInstance.SharedDataURL[num] = UpdateProductVersion(mInstance.SharedDataURL[num]);
			}
		}
		if (mInstance.SoundURL != null)
		{
			for (num = 0; num < mInstance.SoundURL.Length; num++)
			{
				mInstance.SoundURL[num] = UpdateProductVersion(mInstance.SoundURL[num]);
			}
		}
		if (mInstance.MoviesURL != null)
		{
			for (num = 0; num < mInstance.MoviesURL.Length; num++)
			{
				mInstance.MoviesURL[num] = UpdateProductVersion(mInstance.MoviesURL[num]);
			}
		}
	}

	private static void UpdateLocalURLs(string inLocalPath)
	{
		int num = 0;
		for (num = 0; num < mInstance.LocalRootURL.Length; num++)
		{
			mInstance.LocalRootURL[num] = mInstance.LocalRootURL[num].Replace(".", inLocalPath);
		}
		for (num = 0; num < mInstance.LocalContentDataURL.Length; num++)
		{
			mInstance.LocalContentDataURL[num] = mInstance.LocalContentDataURL[num].Replace(".", inLocalPath);
		}
		for (num = 0; num < mInstance.LocalDataURL.Length; num++)
		{
			mInstance.LocalDataURL[num] = mInstance.LocalDataURL[num].Replace(".", inLocalPath);
		}
		for (num = 0; num < mInstance.LocalSceneURL.Length; num++)
		{
			mInstance.LocalSceneURL[num] = mInstance.LocalSceneURL[num].Replace(".", inLocalPath);
		}
		for (num = 0; num < mInstance.LocalSharedDataURL.Length; num++)
		{
			mInstance.LocalSharedDataURL[num] = mInstance.LocalSharedDataURL[num].Replace(".", inLocalPath);
		}
		for (num = 0; num < mInstance.LocalSoundURL.Length; num++)
		{
			mInstance.LocalSoundURL[num] = mInstance.LocalSoundURL[num].Replace(".", inLocalPath);
		}
		for (num = 0; num < mInstance.LocalMoviesURL.Length; num++)
		{
			mInstance.LocalMoviesURL[num] = mInstance.LocalMoviesURL[num].Replace(".", inLocalPath);
		}
	}

	public string GetRootURL(string url)
	{
		uint num = UtKey.Get(url) % (uint)RootURL.Length;
		url = RootURL[num];
		string bundleQuality = GetBundleQuality();
		if (!string.IsNullOrEmpty(bundleQuality))
		{
			url = url.Replace("/Mid/", "/" + bundleQuality + "/");
		}
		return url;
	}

	public string GetContentDataURL(string inURL)
	{
		uint num = UtKey.Get(inURL) % (uint)ContentDataURL.Length;
		string text = ContentDataURL[num];
		string bundleQuality = GetBundleQuality();
		if (!string.IsNullOrEmpty(bundleQuality))
		{
			text = text.Replace("/Mid/", "/" + bundleQuality + "/");
		}
		return text;
	}

	public string GetDataURL(string inURL)
	{
		uint num = UtKey.Get(inURL) % (uint)DataURL.Length;
		string text = DataURL[num];
		string bundleQuality = GetBundleQuality();
		if (!string.IsNullOrEmpty(bundleQuality))
		{
			text = text.Replace("/Mid/", "/" + bundleQuality + "/");
		}
		return text;
	}

	public string GetSceneURL(string inURL)
	{
		uint num = UtKey.Get(inURL) % (uint)SceneURL.Length;
		string text = SceneURL[num];
		string bundleQuality = GetBundleQuality();
		if (!string.IsNullOrEmpty(bundleQuality))
		{
			text = text.Replace("/Mid/", "/" + bundleQuality + "/");
		}
		return text;
	}

	public string GetSharedDataURL(string inURL)
	{
		uint num = UtKey.Get(inURL) % (uint)SharedDataURL.Length;
		string text = SharedDataURL[num];
		string bundleQuality = GetBundleQuality();
		if (!string.IsNullOrEmpty(bundleQuality))
		{
			text = text.Replace("/Mid/", "/" + bundleQuality + "/");
		}
		return text;
	}

	public string GetSoundURL(string inURL)
	{
		uint num = UtKey.Get(inURL) % (uint)SoundURL.Length;
		string text = SoundURL[num];
		string bundleQuality = GetBundleQuality();
		if (!string.IsNullOrEmpty(bundleQuality))
		{
			text = text.Replace("/Mid/", "/" + bundleQuality + "/");
		}
		return text;
	}

	public string GetMoviesURL(string inURL)
	{
		uint num = UtKey.Get(inURL) % (uint)MoviesURL.Length;
		string text = MoviesURL[num];
		string bundleQuality = GetBundleQuality();
		if (!string.IsNullOrEmpty(bundleQuality))
		{
			text = text.Replace("/Mid/", "/" + bundleQuality + "/");
		}
		return text;
	}

	public string GetLocalRootURL(string url)
	{
		uint num = UtKey.Get(url) % (uint)LocalRootURL.Length;
		url = LocalRootURL[num];
		return url;
	}

	public string GetLocalContentDataURL(string inURL)
	{
		uint num = UtKey.Get(inURL) % (uint)LocalContentDataURL.Length;
		return LocalContentDataURL[num];
	}

	public string GetLocalDataURL(string inURL)
	{
		uint num = UtKey.Get(inURL) % (uint)LocalDataURL.Length;
		return LocalDataURL[num];
	}

	public string GetLocalSceneURL(string inURL)
	{
		uint num = UtKey.Get(inURL) % (uint)LocalSceneURL.Length;
		return LocalSceneURL[num];
	}

	public string GetLocalSharedDataURL(string inURL)
	{
		uint num = UtKey.Get(inURL) % (uint)LocalSharedDataURL.Length;
		return LocalSharedDataURL[num];
	}

	public string GetLocalSoundURL(string inURL)
	{
		uint num = UtKey.Get(inURL) % (uint)LocalSoundURL.Length;
		return LocalSoundURL[num];
	}

	public string GetLocalMoviesURL(string inURL)
	{
		uint num = UtKey.Get(inURL) % (uint)LocalMoviesURL.Length;
		return LocalMoviesURL[num];
	}

	private static void SetServerType()
	{
		if (pInstance.ContentServerURL.Contains("dev."))
		{
			mServerType = "D";
		}
		else if (pInstance.ContentServerURL.Contains("qa."))
		{
			mServerType = "Q";
		}
		else if (pInstance.ContentServerURL.Contains("sodstaging."))
		{
			mServerType = "SOD";
		}
		else if (pInstance.ContentServerURL.Contains("staging."))
		{
			mServerType = "S";
		}
		else
		{
			mServerType = "";
		}
	}

	public static string GetLocale(string inLangID)
	{
		string text = "en-US";
		if (text == inLangID)
		{
			return text;
		}
		if (mInstance == null || mInstance.Locale == null)
		{
			UtDebug.LogError("LocaleMappings not found. Will default to : " + text);
			return text;
		}
		for (int i = 0; i < mInstance.Locale.Length; i++)
		{
			if (string.IsNullOrEmpty(mInstance.Locale[i].ID) || mInstance.Locale[i].Variant == null)
			{
				continue;
			}
			if (inLangID.Equals(mInstance.Locale[i].ID))
			{
				return mInstance.Locale[i].ID;
			}
			for (int j = 0; j < mInstance.Locale[i].Variant.Length; j++)
			{
				if (inLangID.Equals(mInstance.Locale[i].Variant[j]))
				{
					return mInstance.Locale[i].ID;
				}
			}
		}
		return text;
	}

	public static bool IsLocaleAvailable(string inLangID)
	{
		string text = "en-US";
		if (text == inLangID)
		{
			return true;
		}
		if (mInstance == null || mInstance.Locale == null)
		{
			UtDebug.LogError("LocaleMappings not found. Will default to : " + text);
			return false;
		}
		for (int i = 0; i < mInstance.Locale.Length; i++)
		{
			if (string.IsNullOrEmpty(mInstance.Locale[i].ID) || mInstance.Locale[i].Variant == null)
			{
				continue;
			}
			if (inLangID.Equals(mInstance.Locale[i].ID))
			{
				return true;
			}
			for (int j = 0; j < mInstance.Locale[i].Variant.Length; j++)
			{
				if (inLangID.Equals(mInstance.Locale[i].Variant[j]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static PlatformSettings GetPlatformSettings()
	{
		if (mPlatformSettings == null && pInstance != null && pInstance.PlatformSettings != null)
		{
			string platformName = UtPlatform.GetPlatformName();
			PlatformSettings platformSettings = null;
			if (UtPlatform.IsAndroid())
			{
				PlatformSettings settingForGPU = GetSettingForGPU();
				if (settingForGPU != null)
				{
					mPlatformSettings = settingForGPU;
					return mPlatformSettings;
				}
			}
			int i = 0;
			for (int num = pInstance.PlatformSettings.Length; i < num; i++)
			{
				if (platformSettings == null && pInstance.PlatformSettings[i].Name == "default")
				{
					platformSettings = pInstance.PlatformSettings[i];
				}
				if (UtPlatform.PlatformCanBeUsed(pInstance.PlatformSettings[i].Platform, platformName))
				{
					mPlatformSettings = pInstance.PlatformSettings[i];
					return mPlatformSettings;
				}
			}
			if (platformSettings != null)
			{
				mPlatformSettings = platformSettings;
			}
		}
		return mPlatformSettings;
	}

	private static PlatformSettings GetSettingForGPU()
	{
		for (int i = 0; i < pInstance.PlatformSettings.Length; i++)
		{
			string[] gPU = pInstance.PlatformSettings[i].GPU;
			if (gPU == null || gPU.Length == 0)
			{
				continue;
			}
			for (int j = 0; j < gPU.Length; j++)
			{
				if (!string.IsNullOrEmpty(gPU[j]) && UtPlatform.GetGPUName().Contains(gPU[j]))
				{
					return pInstance.PlatformSettings[i];
				}
			}
		}
		return null;
	}

	private static GraphicsSettings GetDefaultGraphicsSettings()
	{
		return GetPlatformSettings()?.GraphicsSettings;
	}

	public static List<UserGraphicsSetting> GetGraphicsSettings()
	{
		if (mGraphicsSettings == null)
		{
			mGraphicsSettings = new List<UserGraphicsSetting>();
			AddGraphicsSettings(GraphicSettingType.Texture);
			AddGraphicsSettings(GraphicSettingType.Shadow);
			if (UtPlatform.IsImageEffectSupported())
			{
				AddGraphicsSettings(GraphicSettingType.Effects);
			}
			string @string = PlayerPrefs.GetString("GraphicsSettings");
			if (!string.IsNullOrEmpty(@string))
			{
				LoadSavedGraphicsSettings(@string);
			}
		}
		return mGraphicsSettings;
	}

	private static void LoadSavedGraphicsSettings(string inGraphicsSettings)
	{
		string[] array = inGraphicsSettings.Split('|');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(':');
			if (array2 != null && Enum.IsDefined(typeof(GraphicSettingType), array2[0]))
			{
				GraphicSettingType type = (GraphicSettingType)Enum.Parse(typeof(GraphicSettingType), array2[0]);
				if (type != GraphicSettingType.Effects || UtPlatform.IsImageEffectSupported())
				{
					UserGraphicsSetting userGraphicsSetting = mGraphicsSettings.Find((UserGraphicsSetting item) => item._Type == type);
					if (userGraphicsSetting != null)
					{
						userGraphicsSetting._Value = array2[1];
					}
				}
			}
			else
			{
				UtDebug.LogError("Saved graphic settings are not Loaded properly from PlayerPrefs.");
			}
		}
	}

	public static void SetGraphicsSettings(List<UserGraphicsSetting> graphicsSettings)
	{
		mGraphicsSettings = null;
		mGraphicsSettings = graphicsSettings;
		foreach (UserGraphicsSetting graphicsSetting in graphicsSettings)
		{
			if (graphicsSetting == null)
			{
				continue;
			}
			string value = graphicsSetting._Value;
			switch (graphicsSetting._Type)
			{
			case GraphicSettingType.Texture:
				if (!mBundleQuality.Equals(value))
				{
					mBundleQuality = value;
				}
				break;
			case GraphicSettingType.Effects:
				if (!GetEffectQuality().Equals(value))
				{
					mEffectQuality = value;
				}
				break;
			case GraphicSettingType.Shadow:
				if (!GetShadowQuality().Equals(value))
				{
					mShadowQuality = value;
				}
				break;
			}
		}
		string text = "";
		for (int i = 0; i < mGraphicsSettings.Count; i++)
		{
			text = text + ((i > 0) ? "|" : "") + mGraphicsSettings[i]._Type.ToString() + ":" + mGraphicsSettings[i]._Value;
		}
		if (!string.IsNullOrEmpty(text))
		{
			PlayerPrefs.SetString("GraphicsSettings", text);
		}
	}

	private static void AddGraphicsSettings(GraphicSettingType type)
	{
		GraphicsProperty graphicsProperties = GetGraphicsProperties(type);
		if (graphicsProperties != null && graphicsProperties.pAvailableSettings != null)
		{
			string value = (string.IsNullOrEmpty(graphicsProperties.Default) ? graphicsProperties.pAvailableSettings[0] : graphicsProperties.Default);
			mGraphicsSettings.Add(new UserGraphicsSetting(type, value));
		}
	}

	public static GraphicsProperty GetGraphicsProperties(GraphicSettingType type)
	{
		GraphicsSettings defaultGraphicsSettings = GetDefaultGraphicsSettings();
		if (defaultGraphicsSettings != null)
		{
			switch (type)
			{
			case GraphicSettingType.Texture:
				return defaultGraphicsSettings.TextureSettings;
			case GraphicSettingType.Shadow:
				return defaultGraphicsSettings.ShadowSettings;
			case GraphicSettingType.Effects:
				return defaultGraphicsSettings.EffectSettings;
			}
		}
		else
		{
			UtDebug.LogError("Default Graphics settings are not Loaded properly. Please check DWADragonsMain.XML ");
		}
		return null;
	}

	public static string GetBundleQuality()
	{
		if (string.IsNullOrEmpty(mBundleQuality))
		{
			mBundleQuality = GetGraphicQuality(GraphicSettingType.Texture);
		}
		return mBundleQuality;
	}

	public static void SetBundleQuality(string quality)
	{
		if (!string.IsNullOrEmpty(quality))
		{
			mBundleQuality = quality;
		}
	}

	public static string GetEffectQuality()
	{
		if (string.IsNullOrEmpty(mEffectQuality))
		{
			mEffectQuality = GetGraphicQuality(GraphicSettingType.Effects);
		}
		return mEffectQuality;
	}

	public static string GetShadowQuality()
	{
		if (string.IsNullOrEmpty(mShadowQuality))
		{
			mShadowQuality = GetGraphicQuality(GraphicSettingType.Shadow);
		}
		return mShadowQuality;
	}

	private static string GetGraphicQuality(GraphicSettingType type)
	{
		List<UserGraphicsSetting> graphicsSettings = GetGraphicsSettings();
		if (graphicsSettings != null)
		{
			UserGraphicsSetting userGraphicsSetting = graphicsSettings.Find((UserGraphicsSetting item) => item._Type == type);
			if (userGraphicsSetting != null)
			{
				return userGraphicsSetting._Value;
			}
		}
		return null;
	}

	public static KA.Framework.Environment GetEnvironmentForBundles()
	{
		if (ProductSettings.pInstance == null)
		{
			return KA.Framework.Environment.UNKNOWN;
		}
		string contentDataURL = pInstance.GetContentDataURL("");
		if (string.IsNullOrEmpty(contentDataURL))
		{
			return KA.Framework.Environment.UNKNOWN;
		}
		if (contentDataURL.Contains("dev."))
		{
			return KA.Framework.Environment.DEV;
		}
		if (contentDataURL.Contains("qa."))
		{
			return KA.Framework.Environment.QA;
		}
		if (contentDataURL.Contains("sodstaging."))
		{
			return KA.Framework.Environment.SODSTAGING;
		}
		if (contentDataURL.Contains("staging."))
		{
			return KA.Framework.Environment.STAGING;
		}
		return KA.Framework.Environment.LIVE;
	}
}
