using UnityEngine;

public class UiLanguageMenu : KAUIDropDownMenu
{
	private bool mStartPrefetching;

	private bool mLanguageLoaded = true;

	private bool mLanguageAndAssetVersionLoaded = true;

	private bool mStringsReplaced;

	private bool mIsBackBtnAllowed = true;

	public bool pIsBackBtnAllowed => mIsBackBtnAllowed;

	public override void SetSelectedWidget(KAWidget inWidget)
	{
		int userDataInt = inWidget.GetUserDataInt();
		string iD = ProductConfig.pInstance.Locale[userDataInt].ID;
		if (!iD.Equals(UtUtilities.GetLocaleLanguage()))
		{
			base.SetSelectedWidget(inWidget);
			KAUICursorManager.SetExclusiveLoadingGear(status: true);
			UtUtilities.SetLocaleLanguage(iD);
			LocaleData.Reset();
			LocaleData.Init();
			UiLogin.pInstance.EnablePlayButtons(enable: false);
			UiLogin.pInstance.EnableBundleUpdateUI(enable: false);
			UiLogin.pLocale = iD;
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
			if (!UtPlatform.IsMobile())
			{
				LoadScreenData._LoadScreenData = null;
			}
			mStartPrefetching = true;
			mLanguageLoaded = false;
			mLanguageAndAssetVersionLoaded = false;
			mStringsReplaced = false;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (UtWWWAsync.pIsVersionDownloadComplete)
		{
			if (mStartPrefetching && PrefetchManager.pInstance != null)
			{
				if (FontManager.pInstance != null)
				{
					FontManager.pInstance.PreloadAllFonts(replaceFont: true);
				}
				PrefetchManager.pInstance.PrefetchDataDownloaded();
				if (UiLogin.pInstance != null)
				{
					UiLogin.pInstance.LoadLocaleAds();
				}
				mStartPrefetching = false;
				if (!UtPlatform.IsiOS())
				{
					UiLogin.pInstance.StartPrefetch();
				}
			}
			if (!mLanguageAndAssetVersionLoaded && LocaleData.pIsReady && FontManager.pInstance.pIsReady)
			{
				KAUICursorManager.SetExclusiveLoadingGear(status: false);
				PlayerPrefs.SetString("REM_LANGUAGE", UtUtilities.GetLocaleLanguage());
				mLanguageAndAssetVersionLoaded = true;
				if (UtPlatform.IsiOS())
				{
					UiLogin.pInstance.EnablePlayButtons(enable: true);
					UiLogin.pInstance.EnableBundleUpdateUI(enable: true);
				}
			}
		}
		if (!mLanguageLoaded && LocaleData.pIsReady && !mStartPrefetching)
		{
			KAWidget[] array = Object.FindObjectsOfType<KAWidget>();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].pNGUIWidgets.Count > 0)
				{
					array[i].BroadcastMessage("ProcessLocaleText", null, SendMessageOptions.DontRequireReceiver);
				}
				if ((bool)array[i].GetComponent<UIInput>())
				{
					UIInput component = array[i].GetComponent<UIInput>();
					component.defaultText = StringTable.GetStringData(component.label.textID, component.label.text);
				}
			}
			if (UiLogin.pInstance._GraphicSettingsUI != null)
			{
				UiLogin.pInstance._GraphicSettingsUI.RefreshLocale();
			}
			mStringsReplaced = true;
			mLanguageLoaded = true;
		}
		if (mStringsReplaced && FontManager.pInstance != null && FontManager.pInstance.pIsReady)
		{
			FontManager.pInstance.ReplaceFont();
			mStringsReplaced = false;
		}
		if (Input.GetKeyUp(KeyCode.Escape) && IsActive() && KAUI.mOnClickFrameCount != Time.frameCount)
		{
			UpdateState(isDropped: false);
		}
	}

	protected override void UpdateVisibility(bool inVisible)
	{
		base.UpdateVisibility(inVisible);
		if (inVisible)
		{
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
}
