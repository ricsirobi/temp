using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using KA.Framework;
using Newtonsoft.Json;
using UnityEngine;

public class ProductStartup : MonoBehaviour
{
	public enum State
	{
		ENVIRONMENT_CHECK,
		UNINITIALIZED,
		WAITING_FOR_SERVER_DOWN,
		WAITING_FOR_APP_VERSION_CHECK,
		WAITING_FOR_TOKEN_CHECK,
		WAITING_FOR_PREFETCH_CHECK,
		WAITING_FOR_LOCALIZATION,
		WAITING_FOR_FONTBUNDLE,
		WAITING_FOR_INITIALIZATION,
		WAITING_FOR_COMPATIBILITY_CHECK,
		READY
	}

	private VersionStatus mVersionStatus = VersionStatus.NoUpgradeRequired;

	private bool mIsProductVersionCheckComplete;

	public string _ProductVersionAssetPath = "RS_DATA/ProductVersion.xml";

	public string _ProductUpdateAssetPath = "RS_DATA/ProductUpgradeURL.xml";

	private static State mState = State.UNINITIALIZED;

	private static bool mIsCompatibilityCheckDone = false;

	private ProductUpdateData mUpdateURLData;

	private RsAssetLoader mPreloader;

	private static KAUIGenericDB mGenericDB = null;

	private ProductVersion mRemoteVersionData;

	public static bool pIsReady => mState == State.READY;

	public static bool pCanLogin => mState >= State.WAITING_FOR_PREFETCH_CHECK;

	public static State pState
	{
		get
		{
			return mState;
		}
		set
		{
			mState = value;
		}
	}

	public static bool pIsCompatibilityCheckDone
	{
		set
		{
			mIsCompatibilityCheckDone = value;
		}
	}

	private void Update()
	{
		switch (mState)
		{
		case State.ENVIRONMENT_CHECK:
			if (UtPlatform.IsStandAlone())
			{
				KAUICursorManager.SetDefaultCursor("", showHideSystemCursor: false);
			}
			break;
		case State.UNINITIALIZED:
			KAUICursorManager.SetDefaultCursor("Arrow");
			if (ProductConfig.pIsReady)
			{
				if (PlayerPrefs.HasKey("REM_LANGUAGE"))
				{
					UtUtilities.SetLocaleLanguage(PlayerPrefs.GetString("REM_LANGUAGE"));
				}
				KAInput.Init();
				LocaleData.Init();
				mState = State.WAITING_FOR_LOCALIZATION;
			}
			break;
		case State.WAITING_FOR_LOCALIZATION:
			if (LocaleData.pIsReady)
			{
				if (FontManager.pInstance != null)
				{
					FontManager.pInstance.PreloadAllFonts();
				}
				mState = State.WAITING_FOR_FONTBUNDLE;
			}
			break;
		case State.WAITING_FOR_FONTBUNDLE:
			if (FontManager.pInstance == null || FontManager.pInstance.pIsReady)
			{
				ServerDown.Init();
				mState = State.WAITING_FOR_SERVER_DOWN;
			}
			break;
		case State.WAITING_FOR_SERVER_DOWN:
			if (ServerDown.pIsReady)
			{
				if (!IsServerDown())
				{
					WsTokenMonitor.CheckToken();
				}
				string key = ProductConfig.pAppName + "SystemInfo";
				if (PlayerPrefs.GetInt(key, 0) == 0)
				{
					string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(SystemInfo.operatingSystem + "," + SystemInfo.processorType, ",", SystemInfo.processorCount.ToString()), ",", SystemInfo.systemMemorySize.ToString()), ",", SystemInfo.graphicsDeviceVendor), ",", SystemInfo.graphicsDeviceName), ",", SystemInfo.graphicsMemorySize.ToString()), ",", SystemInfo.graphicsShaderLevel.ToString()), ",", SystemInfo.supportsShadows.ToString());
					PlayerPrefs.SetInt(key, 1);
				}
				if (!PlayerPrefs.HasKey("REM_LANGUAGE"))
				{
					LoadStandaloneSettings();
				}
				CheckProductVersion();
				mState = State.WAITING_FOR_APP_VERSION_CHECK;
			}
			break;
		case State.WAITING_FOR_APP_VERSION_CHECK:
			if (mIsProductVersionCheckComplete && (mVersionStatus == VersionStatus.NoUpgradeRequired || mVersionStatus == VersionStatus.UnknownVersion))
			{
				mState = State.WAITING_FOR_TOKEN_CHECK;
			}
			break;
		case State.WAITING_FOR_TOKEN_CHECK:
			if (mGenericDB != null)
			{
				UnityEngine.Object.Destroy(mGenericDB.gameObject);
			}
			if (IsServerDown())
			{
				base.enabled = false;
				UICursorManager.pVisibility = true;
				KAUICursorManager.SetDefaultCursor("Arrow");
				KAUIGenericDB kAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "ServerDown");
				kAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: false);
				ServerDownMessage message = ServerDown.pInstance.GetMessage();
				if (message != null)
				{
					kAUIGenericDB.SetTitle(message.Title);
					kAUIGenericDB.SetText(message.Text, interactive: false);
				}
			}
			else if (WsTokenMonitor.pHaveCheckedToken)
			{
				mState = State.WAITING_FOR_PREFETCH_CHECK;
			}
			break;
		case State.WAITING_FOR_PREFETCH_CHECK:
		{
			List<string> list = new List<string>();
			if (!string.IsNullOrEmpty(ProductSettings.pInstance._Resource))
			{
				list.Add(ProductSettings.pInstance._Resource);
			}
			if (ProductSettings.pInstance._PreloadBundles != null && ProductSettings.pInstance._PreloadBundles.Length != 0)
			{
				list.AddRange(ProductSettings.pInstance._PreloadBundles);
			}
			if (list.Count > 0)
			{
				mPreloader = new RsAssetLoader();
				mPreloader.Load(list.ToArray(), null, null, inDontDestroy: true);
			}
			GameDataConfig.Init();
			MonetizationData.Init();
			mState = State.WAITING_FOR_INITIALIZATION;
			break;
		}
		case State.WAITING_FOR_INITIALIZATION:
			if (MonetizationData.pIsReady && GameDataConfig.pIsReady && (mPreloader == null || mPreloader.pIsReady))
			{
				KAInput.LoadInput();
				mState = State.WAITING_FOR_COMPATIBILITY_CHECK;
			}
			break;
		case State.WAITING_FOR_COMPATIBILITY_CHECK:
			if (mIsCompatibilityCheckDone)
			{
				mState = State.READY;
				AnalyticAgent.LogFTUEEvent(FTUEEvent.READY);
			}
			break;
		}
	}

	private bool IsServerDown()
	{
		List<int> productDown = ServerDown.pInstance.ProductDown;
		if (productDown != null && productDown.Count > 0)
		{
			return ServerDown.pInstance.ProductDown.Contains(ProductConfig.pProductID);
		}
		return ServerDown.pInstance.Down;
	}

	private void CheckProductVersion()
	{
		RsResourceManager.Load(_ProductVersionAssetPath, ProductVersionEventHandler, RsResourceType.XML, inDontDestroy: false, inDisableCache: false, inDownloadOnly: false, inIgnoreAssetVersion: true);
	}

	private string GetStandaloneSettingPath()
	{
		return Application.dataPath + "/../settings.ini";
	}

	public void LoadStandaloneSettings()
	{
		string standaloneSettingPath = GetStandaloneSettingPath();
		if (!File.Exists(standaloneSettingPath))
		{
			return;
		}
		StreamReader streamReader = new StreamReader(standaloneSettingPath);
		string text = streamReader.ReadToEnd();
		text = text.TrimEnd();
		if (!string.IsNullOrEmpty(text))
		{
			Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(text);
			if (dictionary != null)
			{
				UtUtilities.SetLocaleLanguage((string)dictionary["CurrentLang"]);
			}
		}
		streamReader.Close();
	}

	private string GetAutoUpdater()
	{
		return Application.dataPath + "/../autoupdate-windows.exe";
	}

	private bool IsUpdateAvailable()
	{
		bool result = false;
		try
		{
			Process process = new Process();
			process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
			process.StartInfo.CreateNoWindow = false;
			process.StartInfo.UseShellExecute = true;
			string autoUpdater = GetAutoUpdater();
			process.StartInfo.FileName = autoUpdater;
			process.StartInfo.Arguments = "--mode unattended --unattendedmodeui none";
			process.EnableRaisingEvents = true;
			process.Start();
			process.WaitForExit();
			int exitCode = process.ExitCode;
			UnityEngine.Debug.Log("AutoUpdater ExitCode " + exitCode);
			if (exitCode == 0)
			{
				result = true;
			}
		}
		catch (Exception ex)
		{
			UtDebug.LogError("Update check Error  " + ex);
		}
		return result;
	}

	private void StartUpdateAppVersion()
	{
		try
		{
			Process process = new Process();
			process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
			process.StartInfo.CreateNoWindow = false;
			process.StartInfo.UseShellExecute = true;
			string autoUpdater = GetAutoUpdater();
			process.StartInfo.FileName = autoUpdater;
			process.StartInfo.Arguments = "--mode unattended --unattendedmodebehavior download --unattendedmodeui minimalWithDialogs";
			process.EnableRaisingEvents = true;
			process.Start();
			Application.Quit();
		}
		catch (Exception ex)
		{
			UtDebug.LogError("Update Error  " + ex);
		}
	}

	private void ProductVersionEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			mIsProductVersionCheckComplete = true;
			mRemoteVersionData = UtUtilities.DeserializeFromXml<ProductVersion>((string)inObject);
			if (mRemoteVersionData == null)
			{
				mVersionStatus = VersionStatus.UnknownVersion;
				UtDebug.LogError(" Product version check failed !!");
				break;
			}
			Version version = new Version(mRemoteVersionData.GetProductVersion());
			Version version2 = new Version(ProductConfig.pProductVersion);
			int num = version2.CompareTo(version);
			if (num == 0)
			{
				mVersionStatus = VersionStatus.NoUpgradeRequired;
			}
			else if (num > 0)
			{
				mVersionStatus = VersionStatus.UnknownVersion;
			}
			else if (version2.Major < version.Major || (version2.Major == version.Major && version2.Minor < version.Minor))
			{
				mVersionStatus = VersionStatus.MustUpgrade;
			}
			else
			{
				mVersionStatus = VersionStatus.UpgradeRecommended;
			}
			if (mRemoteVersionData.GetProductInfo() != null && mRemoteVersionData.GetProductInfo().UpgradeState == UpgradeStatus.None)
			{
				mVersionStatus = VersionStatus.UpgradeNotAvailable;
				ShowDB(mRemoteVersionData.GetStatusString(mVersionStatus), base.gameObject, "QuitApp");
			}
			else if (mVersionStatus == VersionStatus.MustUpgrade || mVersionStatus == VersionStatus.UpgradeRecommended)
			{
				RsResourceManager.Load(_ProductUpdateAssetPath, ProductUpdateEventHandler, RsResourceType.XML, inDontDestroy: false, inDisableCache: false, inDownloadOnly: false, inIgnoreAssetVersion: true);
			}
			else if (mVersionStatus == VersionStatus.UnknownVersion)
			{
				UtDebug.Log("CHECK_PRODUCT_VERSION Return Unknown Version");
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			mIsProductVersionCheckComplete = true;
			mVersionStatus = VersionStatus.UnknownVersion;
			UtDebug.LogError(" Product version check failed !!");
			break;
		}
	}

	private void ProductUpdateEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			mUpdateURLData = UtUtilities.DeserializeFromXml<ProductUpdateData>((string)inObject);
			if (mUpdateURLData == null)
			{
				UtDebug.LogError(" Product URL Fetch failed !!");
				break;
			}
			string statusString = mRemoteVersionData.GetStatusString(mVersionStatus);
			if (mVersionStatus == VersionStatus.MustUpgrade)
			{
				ShowDB(statusString, base.gameObject, "DoUpgrade");
			}
			else if (mVersionStatus == VersionStatus.UpgradeRecommended)
			{
				ShowDB(statusString, base.gameObject, null, "DoUpgrade", "OnVersionCheckDBClose");
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError(" Product Update URL file download failed !!");
			break;
		}
	}

	public void QuitApp()
	{
		Application.Quit();
	}

	public void OnVersionCheckDBClose()
	{
		mGenericDB.Destroy();
		mGenericDB = null;
		KAUICursorManager.SetDefaultCursor("Loading");
		UICursorManager.pVisibility = false;
		mState++;
	}

	public void DoUpgrade()
	{
		StartUpdateAppVersion();
	}

	public static void ShowDB(string inMessage, GameObject inMessageObject, string okMessage, string yesMessage = "", string NoMessage = "")
	{
		UICursorManager.pVisibility = true;
		KAUICursorManager.SetDefaultCursor("Arrow");
		mGenericDB = GameUtilities.DisplayGenericDB("PfKAUIGenericDB", inMessage, null, inMessageObject, yesMessage, NoMessage, okMessage, null);
	}
}
