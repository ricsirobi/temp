using System;
using System.Collections;
using System.Collections.Generic;
using KA.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UiLoading : KAUI
{
	private static int mCurScreenIdx = 0;

	private static int[] mSplashScreenIndexArray;

	private static bool mShowWelcome = true;

	public KAWidget _LoadScreenBackground;

	public string _LoadScreenDataFileName = "RS_DATA/LoadScreenData.xml";

	public Texture[] _Screens;

	public Texture _WelcomeTexture;

	public Texture _WelcomeBackTexture;

	public PartnerLoadingTexture[] _PartnerTextures;

	private bool mProgressDirty;

	private UtProgress mProgress = new UtProgress();

	public KAWidget mProgressBar;

	private bool mGetLoadingScreen = true;

	private bool mCanUpdateLoadScreen;

	private Texture mLoadScreenImage;

	private LoadScreen mLoadScreen;

	private KAWidget mTxtLoading;

	private LocaleString[] mLoadingTexts;

	private int mLoadTextIndex;

	private float mDelayBwLoadTextsInSecs;

	public bool _GetGenderFromUserInfo = true;

	protected override void Start()
	{
		base.Start();
		if (_Screens != null && _Screens.Length != 0 && mSplashScreenIndexArray == null)
		{
			mSplashScreenIndexArray = UtUtilities.GenerateShuffledInts(_Screens.Length);
		}
		if (mShowWelcome)
		{
			bool flag = false;
			if (_PartnerTextures != null && _PartnerTextures.Length != 0 && !string.IsNullOrEmpty(UserInfo.pInstance.Partner) && !SubscriptionInfo.pIsMember)
			{
				PartnerLoadingTexture[] partnerTextures = _PartnerTextures;
				foreach (PartnerLoadingTexture partnerLoadingTexture in partnerTextures)
				{
					if (UserInfo.pInstance.Partner.Equals(partnerLoadingTexture._Partner, StringComparison.OrdinalIgnoreCase))
					{
						_LoadScreenBackground.SetTexture(partnerLoadingTexture._Texture);
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				ShowWelcomeScreen();
			}
		}
		else
		{
			UpdateLoadScreen();
		}
		mProgressBar = FindItem("LsBar");
		if (mProgressBar != null)
		{
			mProgressBar.SetProgressLevel(0f);
		}
		LoadScreenData.Init(_LoadScreenDataFileName, _GetGenderFromUserInfo);
	}

	private void ShowWelcomeScreen()
	{
		mShowWelcome = false;
		if (!AvatarData.pInitializedFromPreviousSave && _WelcomeTexture != null)
		{
			_LoadScreenBackground.SetTexture(_WelcomeTexture);
		}
		else if (_WelcomeBackTexture != null)
		{
			_LoadScreenBackground.SetTexture(_WelcomeBackTexture);
		}
		else
		{
			UpdateLoadScreen();
		}
	}

	protected override void Update()
	{
		base.Update();
		if (LoadScreenData.pIsReady && mGetLoadingScreen)
		{
			mLoadScreen = LoadScreenData.GetLoadScreen();
			if (mLoadScreen != null)
			{
				mGetLoadingScreen = false;
				LoadImage(mLoadScreen.Name);
			}
			else if (mSplashScreenIndexArray != null)
			{
				mGetLoadingScreen = false;
				UpdateLoadScreen();
			}
		}
		if (mCanUpdateLoadScreen)
		{
			mCanUpdateLoadScreen = false;
			UpdateLoadScreen();
		}
		UpdateProgress();
	}

	private IEnumerator UpdateLoadText()
	{
		while (mLoadingTexts != null && mLoadTextIndex < mLoadingTexts.Length)
		{
			if (mLoadingTexts[mLoadTextIndex] != null)
			{
				mTxtLoading.SetText(mLoadingTexts[mLoadTextIndex].GetLocalizedString());
			}
			mLoadTextIndex++;
			yield return new WaitForSeconds(mDelayBwLoadTextsInSecs);
		}
	}

	private void UpdateLoadScreen()
	{
		if (mLoadScreenImage != null)
		{
			_LoadScreenBackground.SetTexture(mLoadScreenImage);
		}
		else if (_Screens != null && _Screens.Length != 0 && mSplashScreenIndexArray != null)
		{
			_LoadScreenBackground.SetTexture(_Screens[mSplashScreenIndexArray[mCurScreenIdx]]);
			mCurScreenIdx++;
			if (mCurScreenIdx >= mSplashScreenIndexArray.Length)
			{
				mCurScreenIdx = 0;
			}
		}
	}

	private void UpdateProgress()
	{
		if (mProgress != null && mProgressBar != null && mProgressDirty)
		{
			mProgressDirty = false;
			mProgress.Recalculate();
			mProgressBar.SetProgressLevel(mProgress.pProgress);
		}
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
		UICursorManager.pVisibility = false;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
		UICursorManager.pVisibility = true;
	}

	private void OnSceneLoaded(Scene newScene, LoadSceneMode loadSceneMode)
	{
		if (mProgress.pTasks.ContainsKey("CommonLevel") && UnityEngine.Object.FindObjectOfType(typeof(CoCommonLevel)) as CoCommonLevel == null)
		{
			mProgress.UpdateTask("CommonLevel", 1f, inForceRecalculate: false);
			mProgressDirty = true;
			UpdateProgress();
		}
	}

	private void AddTask(string inTask)
	{
		if (inTask == "LevelLoad")
		{
			mLoadingTexts = null;
			LoadingTextData.GetLoadTextData(LoadingTextType.SCENE_LOAD, RsResourceManager.pLevelToLoad, ref mLoadingTexts, ref mDelayBwLoadTextsInSecs);
			if (mLoadingTexts != null && mLoadingTexts.Length != 0)
			{
				UtUtilities.Shuffle(mLoadingTexts);
				mLoadTextIndex = 0;
				if (mTxtLoading == null)
				{
					mTxtLoading = FindItem("TxtLoading");
				}
				StartCoroutine(UpdateLoadText());
			}
		}
		mProgress.AddTask(inTask, inForceRecalculate: false);
		mProgressDirty = true;
	}

	private void UpdateTask(KeyValuePair<string, float> inUpdate)
	{
		mProgress.UpdateTask(inUpdate.Key, inUpdate.Value, inForceRecalculate: false);
		mProgressDirty = true;
	}

	private void LoadImage(string inFilename)
	{
		RsResourceManager.Load(inFilename, LoadScreenImageEventHandler, RsResourceType.NONE, inDontDestroy: true);
	}

	private void LoadScreenImageEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent.Equals(RsResourceLoadEvent.COMPLETE))
		{
			if (inObject is Texture)
			{
				mLoadScreenImage = (Texture)inObject;
				UnityEngine.Object.DontDestroyOnLoad(mLoadScreenImage);
			}
			mCanUpdateLoadScreen = true;
		}
		else if (inEvent.Equals(RsResourceLoadEvent.ERROR))
		{
			mCanUpdateLoadScreen = true;
		}
	}

	protected override void OnDestroy()
	{
		UICursorManager.pVisibility = true;
		if (mLoadScreen != null)
		{
			RsResourceManager.Unload(mLoadScreen.Name, splitURL: false);
		}
		base.OnDestroy();
	}
}
