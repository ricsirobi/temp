using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class UiGraphicSettings : KAUI
{
	public enum GraphicSettingState
	{
		Default,
		Modified,
		GamePlayed
	}

	[Serializable]
	public class GraphicOptions
	{
		public LocaleString _OptionText;

		public string _Option;
	}

	[Serializable]
	public class GraphicsMapper
	{
		public GraphicSettingType _GraphicSettingType;

		public LocaleString _GraphicSettingsText;

		public List<GraphicOptions> _Options;
	}

	public LocaleString _ApplyChangesText = new LocaleString("These changes may take some time to apply.");

	public LocaleString _RestartGameText = new LocaleString("These changes require game to restart.");

	public LocaleString _GameCrashText = new LocaleString("[Review]Your recent crash may have been caused by your graphic settings.");

	public LocaleString _ConfirmText = new LocaleString("[Review]Keep these changes?");

	public LocaleString _DefaultText = new LocaleString("[Review]Reset to default?");

	public LocaleString _SettingUpdatedText = new LocaleString("[Review]Settings changed.");

	public List<GraphicsMapper> _GraphicSettings;

	public string _LevelToLoad = "LoginDM";

	private KAUIMenu mGraphicSettingsMenu;

	private KAUIGenericDB mKAUIGenericDB;

	private KAWidget mCloseBtn;

	private KAWidget mApplyBtn;

	private KAWidget mDefaultBtn;

	private KAWidget mGraphicSettingsBlockFilter;

	private bool mIsLoginScene;

	private bool mReloadFont;

	private bool mProcessBundleUpdate;

	private string mLastCrashInfoKey = "CrashInfoKey";

	private static bool mIsBundleReloadRequired;

	private bool mIsBackBtnAllowed = true;

	public static bool pIsBundleReloadRequired => mIsBundleReloadRequired;

	public bool pIsBackBtnAllowed => mIsBackBtnAllowed;

	protected override void Start()
	{
		mCloseBtn = FindItem("BtnClose");
		mApplyBtn = FindItem("BtnApply");
		mDefaultBtn = FindItem("BtnDefault");
		mGraphicSettingsBlockFilter = FindItem("GraphicsSettingsBlocker");
		mGraphicSettingsMenu = GetMenuByIndex(0);
		mIsLoginScene = RsResourceManager.pCurrentLevel == _LevelToLoad;
		Init();
		CheckButtonState();
		EnableUIGraphicSettings(enable: true);
		base.Start();
	}

	private void Init()
	{
		List<UserGraphicsSetting> graphicsSettings = ProductConfig.GetGraphicsSettings();
		if (graphicsSettings == null)
		{
			return;
		}
		foreach (UserGraphicsSetting graphicSetting in graphicsSettings)
		{
			GraphicsProperty graphicsProperties = ProductConfig.GetGraphicsProperties(graphicSetting._Type);
			if (graphicsProperties == null)
			{
				continue;
			}
			GraphicsMapper graphicsMapper = _GraphicSettings.Find((GraphicsMapper item) => item._GraphicSettingType == graphicSetting._Type);
			if (graphicsMapper == null)
			{
				continue;
			}
			UiGraphicSettingItem uiGraphicSettingItem = (UiGraphicSettingItem)mGraphicSettingsMenu.AddWidget(graphicSetting._Type.ToString());
			uiGraphicSettingItem.Init(graphicsMapper._GraphicSettingType, graphicsMapper._GraphicSettingsText.GetLocalizedString());
			if (!(uiGraphicSettingItem.pOptionMenu != null))
			{
				continue;
			}
			string[] pAvailableSettings = graphicsProperties.pAvailableSettings;
			foreach (string availableSetting in pAvailableSettings)
			{
				GraphicOptions graphicOptions = graphicsMapper._Options.Find((GraphicOptions entry) => availableSetting.Contains(entry._Option));
				if (graphicOptions != null)
				{
					KAWidget kAWidget = uiGraphicSettingItem.pOptionMenu.AddWidget(graphicOptions._Option);
					kAWidget.SetText(graphicOptions._OptionText.GetLocalizedString());
					UiGraphicsOptionMenuWidgetData uiGraphicsOptionMenuWidgetData = new UiGraphicsOptionMenuWidgetData(graphicOptions._Option);
					kAWidget.SetUserData(uiGraphicsOptionMenuWidgetData);
					if (graphicSetting._Value.Contains(graphicOptions._Option))
					{
						uiGraphicSettingItem.UpdateData(this, uiGraphicsOptionMenuWidgetData);
					}
				}
			}
		}
	}

	public void RefreshLocale()
	{
		foreach (KAWidget item in mGraphicSettingsMenu.GetItems())
		{
			UiGraphicSettingItem settingItem = item as UiGraphicSettingItem;
			GraphicsMapper graphicsMapper = _GraphicSettings.Find((GraphicsMapper entry) => entry._GraphicSettingType == settingItem.pGraphicSettingType);
			if (graphicsMapper != null)
			{
				string currentSetting = "";
				switch (settingItem.pGraphicSettingType)
				{
				case GraphicSettingType.Shadow:
					currentSetting = ProductConfig.GetShadowQuality();
					break;
				case GraphicSettingType.Effects:
					currentSetting = ProductConfig.GetEffectQuality();
					break;
				case GraphicSettingType.Texture:
					currentSetting = ProductConfig.GetBundleQuality();
					break;
				}
				settingItem.RefreshLocale(graphicsMapper, currentSetting);
			}
		}
	}

	public void CheckButtonState()
	{
		mDefaultBtn.SetDisabled(IsDefaultSettingsApplied() && CompareMenuSettings(compareToDefault: true, updateSetting: false));
		mApplyBtn.SetDisabled(CompareMenuSettings(compareToDefault: false, updateSetting: false));
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mCloseBtn)
		{
			SetVisibility(inVisible: false);
			KAUI.RemoveExclusive(this);
		}
		else if (inWidget == mApplyBtn)
		{
			if (IsBundleTypeUpdated())
			{
				LocaleString inText = (mIsLoginScene ? _ApplyChangesText : _RestartGameText);
				ShowGenericDB(inText, _ConfirmText, "OnApplySettings");
			}
			else
			{
				OnApplySettings();
			}
		}
		else if (inWidget == mDefaultBtn)
		{
			if (IsDefaultSettingsApplied())
			{
				CompareMenuSettings(compareToDefault: false, updateSetting: true);
				mDefaultBtn.SetDisabled(isDisabled: true);
			}
			else if (!IsBundleTypeDefault())
			{
				LocaleString inText2 = (mIsLoginScene ? _ApplyChangesText : _RestartGameText);
				ShowGenericDB(inText2, _ConfirmText, "ResetToDefault");
			}
			else
			{
				ResetToDefault();
			}
		}
	}

	private void ShowGenericDB(LocaleString inText, LocaleString secondaryInText, string yesMessage)
	{
		mKAUIGenericDB = GameUtilities.DisplayGenericDB("PfKAUIGenericDBGraphics", inText.GetLocalizedString(), null, base.gameObject, yesMessage, "OnCloseDB", null, "OnCloseDB", inDestroyOnClick: true);
		KAWidget kAWidget = mKAUIGenericDB.FindItem("TxtConfirm");
		if (kAWidget != null)
		{
			kAWidget.SetText(secondaryInText.GetLocalizedString());
		}
	}

	private void ApplyChanges()
	{
		List<UserGraphicsSetting> list = new List<UserGraphicsSetting>();
		bool flag = false;
		bool flag2 = false;
		foreach (KAWidget item in mGraphicSettingsMenu.GetItems())
		{
			UiGraphicSettingItem uiGraphicSettingItem = item as UiGraphicSettingItem;
			if (uiGraphicSettingItem.GetUserData() is UiGraphicsOptionMenuWidgetData uiGraphicsOptionMenuWidgetData)
			{
				switch (uiGraphicSettingItem.pGraphicSettingType)
				{
				case GraphicSettingType.Texture:
					mIsBundleReloadRequired = !uiGraphicsOptionMenuWidgetData._Option.Equals(ProductConfig.GetBundleQuality());
					break;
				case GraphicSettingType.Effects:
					flag = !uiGraphicsOptionMenuWidgetData._Option.Equals(ProductConfig.GetEffectQuality());
					break;
				case GraphicSettingType.Shadow:
					flag2 = !uiGraphicsOptionMenuWidgetData._Option.Equals(ProductConfig.GetShadowQuality());
					break;
				}
				list.Add(new UserGraphicsSetting(uiGraphicSettingItem.pGraphicSettingType, uiGraphicsOptionMenuWidgetData._Option));
			}
		}
		if (list.Count > 0)
		{
			ProductConfig.SetGraphicsSettings(list);
			int value = ((!IsDefaultSettingsApplied()) ? ((mIsLoginScene || mIsBundleReloadRequired) ? 1 : 2) : 0);
			PlayerPrefs.SetInt("SafeAppClose", value);
		}
		else
		{
			UtDebug.LogError("Failed to update GraphicsSettings.");
		}
		if ((flag || mIsBundleReloadRequired) && GlowManager.pInstance != null)
		{
			GlowManager.pInstance.ApplyEffectSettings();
		}
		if (flag)
		{
			EffectSettings.pInstance.RefreshSavedProfile();
			EffectsManager[] array = UnityEngine.Object.FindObjectsOfType<EffectsManager>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].LoadProfile();
			}
		}
		if (mIsBundleReloadRequired)
		{
			if (mIsLoginScene)
			{
				OnClick(mCloseBtn);
				GameDataConfig.OptimizeTerrain(QualitySettings.GetQualityLevel());
			}
			ProcessBundleUpdate();
		}
		else if (flag2)
		{
			UtUtilities.SetRealTimeShadowDisabled(!UtPlatform.IsRealTimeShadowEnabled());
		}
	}

	public void ProcessBundleUpdate()
	{
		mProcessBundleUpdate = true;
		if (mIsLoginScene)
		{
			RsResourceManager.ReInitManifest();
			UiLogin.pInstance.EnablePlayButtons(enable: false);
			KAUICursorManager.SetExclusiveLoadingGear(status: true);
			UiLogin.pInstance.EnableBundleUpdateUI(enable: false);
			PrefetchManager.pInstance.ReInit();
			if (UtPlatform.IsiOS())
			{
				UiLogin.pInstance.ClearPreloadBundles = true;
				PrefetchManager.pInstance.SetVisibility(inVisible: false);
			}
			else
			{
				PrefetchManager.pInstance.ClearPreloadBundles(UiLogin.pInstance.OnPrefetchComplete);
			}
		}
		else if (!string.IsNullOrEmpty(_LevelToLoad))
		{
			GameUtilities.LoadLoginLevel(showRegstration: false, fullReset: false);
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (inVisible)
		{
			CompareMenuSettings(compareToDefault: false, updateSetting: true);
			mIsBackBtnAllowed = false;
		}
		else
		{
			Invoke("EnableBackButton", Time.deltaTime);
		}
	}

	private void EnableBackButton()
	{
		mIsBackBtnAllowed = true;
	}

	protected override void Update()
	{
		base.Update();
		if (mProcessBundleUpdate && PrefetchManager.pInstance != null && RsResourceManager.ManifestLoadState == ManifestLoadState.LOADED && UtWWWAsync.pIsVersionDownloadComplete)
		{
			PrefetchManager.Init(ignoreGetAssetVersion: false, null, forceLoad: true, OnPrefetchDataReady);
			mProcessBundleUpdate = false;
			mIsBundleReloadRequired = false;
		}
		if (mReloadFont && FontManager.pInstance.pIsReady)
		{
			if (UtPlatform.IsiOS())
			{
				UiLogin.pInstance.EnablePlayButtons(enable: true);
				UiLogin.pInstance.EnableBundleUpdateUI(enable: true);
			}
			KAUICursorManager.SetExclusiveLoadingGear(status: false);
			FontManager.pInstance.ReplaceFont();
			mReloadFont = false;
		}
	}

	private void OnPrefetchDataReady()
	{
		if (FontManager.pInstance != null)
		{
			FontManager.pInstance.PreloadAllFonts(replaceFont: true);
		}
		mReloadFont = true;
		if (!UtPlatform.IsiOS())
		{
			UiLogin.pInstance.StartPrefetch();
		}
	}

	public void ShowCrashWarning()
	{
		if (PlayerPrefs.GetInt("SafeAppClose") == 2)
		{
			PlayerPrefs.SetInt("SafeAppClose", 0);
			if (!UtPlatform.IsStandAlone() || CheckCrashLogs())
			{
				ShowGenericDB(_GameCrashText, _DefaultText, "ResetToDefault");
			}
		}
		else if (UtPlatform.IsStandAlone())
		{
			PlayerPrefs.SetString(mLastCrashInfoKey, DateTime.Now.ToString());
		}
	}

	private void ResetToDefault()
	{
		CompareMenuSettings(compareToDefault: true, updateSetting: true);
		OnApplySettings();
	}

	private void OnApplySettings()
	{
		ApplyChanges();
		CheckButtonState();
	}

	private bool IsDefaultSettingsApplied()
	{
		foreach (UserGraphicsSetting graphicsSetting in ProductConfig.GetGraphicsSettings())
		{
			if (!ProductConfig.GetGraphicsProperties(graphicsSetting._Type).Default.Equals(graphicsSetting._Value))
			{
				return false;
			}
		}
		return true;
	}

	private bool CompareMenuSettings(bool compareToDefault, bool updateSetting)
	{
		foreach (KAWidget item in mGraphicSettingsMenu.GetItems())
		{
			UiGraphicSettingItem settingItem = item as UiGraphicSettingItem;
			string text = null;
			text = ((!compareToDefault) ? ProductConfig.GetGraphicsSettings().Find((UserGraphicsSetting item) => item._Type == settingItem.pGraphicSettingType)._Value : ProductConfig.GetGraphicsProperties(settingItem.pGraphicSettingType).Default);
			if (!(settingItem.GetUserData() as UiGraphicsOptionMenuWidgetData)._Option.Equals(text))
			{
				if (!updateSetting)
				{
					return false;
				}
				settingItem.UpdateWidget(text);
			}
		}
		return true;
	}

	private bool IsBundleTypeUpdated()
	{
		foreach (KAWidget item in mGraphicSettingsMenu.GetItems())
		{
			UiGraphicSettingItem uiGraphicSettingItem = item as UiGraphicSettingItem;
			if (uiGraphicSettingItem.pGraphicSettingType == GraphicSettingType.Texture && uiGraphicSettingItem.GetUserData() is UiGraphicsOptionMenuWidgetData uiGraphicsOptionMenuWidgetData)
			{
				return !uiGraphicsOptionMenuWidgetData._Option.Equals(ProductConfig.GetBundleQuality());
			}
		}
		return false;
	}

	private bool IsBundleTypeDefault()
	{
		if (ProductConfig.GetGraphicsProperties(GraphicSettingType.Texture).Default.Equals(ProductConfig.GetBundleQuality()))
		{
			return true;
		}
		return false;
	}

	private bool CheckCrashLogs()
	{
		string @string = PlayerPrefs.GetString(mLastCrashInfoKey);
		if (!string.IsNullOrEmpty(@string) && DateTime.TryParse(@string, out var result))
		{
			if (UtPlatform.IsStandaloneWindows())
			{
				string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				folderPath = folderPath + "Low/" + Application.companyName + "/" + Application.productName + "/Crashes";
				DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
				if (directoryInfo != null && directoryInfo.Exists)
				{
					DirectoryInfo[] directories = directoryInfo.GetDirectories("Crash*", SearchOption.AllDirectories);
					if (directories != null && directories.Length != 0)
					{
						DateTime creationTime = directories.OrderByDescending((DirectoryInfo entry) => entry.CreationTime).ToList()[0].CreationTime;
						if ((creationTime - result).Seconds > 0)
						{
							PlayerPrefs.SetString(mLastCrashInfoKey, creationTime.ToString());
							return true;
						}
					}
				}
			}
			else if (UtPlatform.IsStandaloneOSX())
			{
				DirectoryInfo directoryInfo2 = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Library/Logs/DiagnosticReports"));
				if (directoryInfo2 != null && directoryInfo2.Exists)
				{
					FileInfo[] files = directoryInfo2.GetFiles("*.crash");
					if (files != null && files.Length != 0)
					{
						List<FileInfo> list = files.ToList().FindAll((FileInfo entry) => entry.Name.Contains("DOMain"));
						if (list != null && list.Count > 0)
						{
							list = list.OrderByDescending((FileInfo entry) => entry.CreationTime).ToList();
							DateTime creationTime2 = list[0].CreationTime;
							if ((creationTime2 - result).Seconds > 0)
							{
								PlayerPrefs.SetString(mLastCrashInfoKey, creationTime2.ToString());
								return true;
							}
						}
					}
				}
			}
		}
		PlayerPrefs.SetString(mLastCrashInfoKey, DateTime.Now.ToString());
		return false;
	}

	public void EnableUIGraphicSettings(bool enable)
	{
		mGraphicSettingsMenu?.SetVisibility(enable);
		mGraphicSettingsBlockFilter?.SetVisibility(!enable);
	}
}
