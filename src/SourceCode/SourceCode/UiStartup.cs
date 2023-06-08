using System.Collections.Generic;
using PlayFab;
using UnityEngine;

public class UiStartup : KAUI
{
	private enum ErrorType
	{
		NONE,
		PROCESS_INCOMPLETE,
		NO_CONNECTION,
		MEMORY_NOT_SUFFICIENT
	}

	public string _StartLevel;

	public LocaleString _InsufficientMemoryText = new LocaleString("For the BEST experience, we recommend a minimum of {X} of RAM and 4GB of available storage.  Your device has {Y} of RAM --your game play experience may not be optimal and may be unstable.");

	public LocaleString _LessThan1GBText = new LocaleString("less than 1 Gb");

	public LocaleString _InsufficientStorageText = new LocaleString("Please make sure your device has 1 GB of storage space.");

	public long _RequiredStorageSpace = 1024L;

	private int mInitWaitCount = 10;

	private bool mLoadingFirstScene;

	private bool mLoadFirstScene = true;

	public float _DeviceMemoryRequired = 1230f;

	private bool mLoginParent;

	private KAUIGenericDB mGenericDB;

	private ErrorType mErrorType;

	private bool mCompatibilityChecked;

	protected override void Awake()
	{
		base.Awake();
		mErrorType = ErrorType.NONE;
	}

	private new void Start()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
	}

	private void CheckDeviceCompatibility()
	{
		ProductStartup.pIsCompatibilityCheckDone = true;
	}

	private void ShowIncompatibleText(bool insufficientStorage)
	{
		string empty = string.Empty;
		if (insufficientStorage)
		{
			empty = _InsufficientStorageText.GetLocalizedString();
		}
		else
		{
			long unityTotalDeviceMemory = UtMobileUtilities.GetUnityTotalDeviceMemory();
			string newValue = ((unityTotalDeviceMemory > 255) ? UtMobileUtilities.FormatBytes(unityTotalDeviceMemory * 1024 * 1024) : _LessThan1GBText.GetLocalizedString());
			string newValue2 = UtMobileUtilities.FormatBytes((long)_DeviceMemoryRequired * 1024 * 1024);
			empty = _InsufficientMemoryText.GetLocalizedString().Replace("{X}", newValue2).Replace("{Y}", newValue);
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
		ShowErrorDB(empty, ErrorType.MEMORY_NOT_SUFFICIENT);
	}

	protected override void Update()
	{
		base.Update();
		if (ProductStartup.pState == ProductStartup.State.WAITING_FOR_COMPATIBILITY_CHECK && !mCompatibilityChecked)
		{
			mCompatibilityChecked = true;
			CheckDeviceCompatibility();
		}
		if (mErrorType == ErrorType.MEMORY_NOT_SUFFICIENT || ((UtPlatform.IsMobile() || UtPlatform.IsWSA()) && !UtUtilities.IsConnectedToWWW()))
		{
			return;
		}
		if (ProductStartup.pState != 0 && mInitWaitCount > 0)
		{
			mInitWaitCount--;
			if (mInitWaitCount == 0)
			{
				ProductConfig.Init();
			}
		}
		if (!ProductStartup.pCanLogin || mLoadingFirstScene || (KAInput.pInstance != null && !KAInput.pInstance.pIsReady))
		{
			return;
		}
		if (ProductConfig.pInstance != null && ProductConfig.pInstance.EnablePlayfab.HasValue)
		{
			PlayfabManager<PlayFabManagerDO>.EnablePlayfab = ProductConfig.pInstance.EnablePlayfab.Value;
			if (PlayfabManager<PlayFabManagerDO>.Instance != null)
			{
				PlayfabManager<PlayFabManagerDO>.Instance.SetPlayfabTitleId();
			}
		}
		else
		{
			PlayFabSettings.TitleId = string.Empty;
		}
		if (!mLoginParent)
		{
			mLoginParent = true;
			if (PlayerPrefs.HasKey("REM_USER_NAME"))
			{
				string value = TripleDES.DecryptUnicode(PlayerPrefs.GetString("REM_USER_NAME"), ProductConfig.pSecret);
				string text = TripleDES.DecryptUnicode(PlayerPrefs.GetString("REM_PASSWORD"), ProductConfig.pSecret);
				if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(text))
				{
					UiLogin.pPassword = text;
					mLoadFirstScene = true;
					return;
				}
			}
			_StartLevel = GameConfig.GetKeyData("LoginScene");
		}
		if (mLoadFirstScene && ProductStartup.pIsReady)
		{
			LoadFirstScene();
		}
	}

	public virtual void LoadFirstScene()
	{
		mLoadFirstScene = false;
		mLoadingFirstScene = true;
		SetVisibility(inVisible: false);
		AvAvatar.SetActive(inActive: false);
		AnalyticAgent.LogFTUEEvent(FTUEEvent.LOAD_FIRST_SCENE);
		RsResourceManager.LoadLevel(_StartLevel);
	}

	private void ShowErrorDB(string message, ErrorType inErrorType, string okMessage = "OnDBClose")
	{
		if (mGenericDB == null && mErrorType == ErrorType.NONE)
		{
			UICursorManager.pVisibility = true;
			KAUICursorManager.SetDefaultCursor("Arrow");
			mGenericDB = GameUtilities.DisplayOKMessage("PfKAUIGenericDB", message, base.gameObject, okMessage, updatePriority: true);
			mErrorType = inErrorType;
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("name", message);
			AnalyticAgent.LogFTUEEvent(FTUEEvent.INCOMPATIBLE_ERROR_SHOWN, dictionary);
		}
	}

	public void OnDBClose()
	{
		if (mGenericDB != null)
		{
			ProductStartup.pIsCompatibilityCheckDone = true;
			KAUICursorManager.SetDefaultCursor("Loading");
			mGenericDB.Destroy();
			mGenericDB = null;
		}
		mErrorType = ErrorType.NONE;
	}

	private bool IsDeviceSupported()
	{
		return true;
	}
}
