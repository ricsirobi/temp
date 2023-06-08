using System;
using System.Collections.Generic;
using UnityEngine;

public class FontManager : MonoBehaviour
{
	public class FontData
	{
		private int mRefCount;

		private string mFontName = string.Empty;

		private UIFont mFont;

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

		public string pFontName
		{
			get
			{
				return mFontName;
			}
			set
			{
				mFontName = value;
			}
		}

		public UIFont pFont
		{
			get
			{
				return mFont;
			}
			set
			{
				mFont = value;
			}
		}
	}

	[Serializable]
	public class FontBundleMapping
	{
		public string _FontBundleName;

		public List<UIFont> _Font;
	}

	[Serializable]
	public class FontMappingInfo
	{
		public List<string> _LocaleKeys;

		public FontBundleMapping[] _BundleInfo;
	}

	private static List<FontData> mFontData = new List<FontData>();

	private static Transform mRootObject = null;

	private static FontManager mInstance = null;

	public bool _UseDynamicFonts;

	public FontMappingInfo[] _FontMappingInfo;

	private UIFont mPrevFont;

	private UIFont mMainFont;

	private bool mReplaceFont;

	private FontBundleMapping mBundleInfo;

	private FontMappingInfo mCurrentFontMapping;

	private const string mIdentifier = "--Ref";

	private int mLoadingCount = -1;

	public static List<FontData> pFontData => mFontData;

	public static FontManager pInstance => mInstance;

	public UIFont pMainFont => mMainFont;

	public bool pIsReady
	{
		get
		{
			if (!_UseDynamicFonts || mLoadingCount <= 0)
			{
				return true;
			}
			return false;
		}
	}

	public void Awake()
	{
		mInstance = this;
		UnityEngine.Object.DontDestroyOnLoad(this);
	}

	public static void AddReference(UILabel inLabel)
	{
		if (inLabel.ignoreFontChange || inLabel == null || inLabel.bitmapFont == null)
		{
			return;
		}
		if (mRootObject == null)
		{
			mRootObject = new GameObject("FontGroup").transform;
			UnityEngine.Object.DontDestroyOnLoad(mRootObject.gameObject);
		}
		string text = inLabel.bitmapFont.name;
		text = text.Replace("--Ref", "");
		FontData fontData = FindFontData(text);
		if (inLabel.bitmapFont.material != null && inLabel.bitmapFont.material.mainTexture != null)
		{
			DestroyTexture(inLabel.bitmapFont.material.mainTexture);
		}
		if (fontData == null)
		{
			fontData = CreateFontData(text, inLabel.bitmapFont.gameObject);
			if (pInstance != null && pInstance.pMainFont != null && pInstance.IsMappedToMainFont(fontData.pFontName))
			{
				fontData.pFont.replacement = pInstance.pMainFont;
			}
		}
		inLabel.bitmapFont = fontData.pFont;
		fontData.pRefCount++;
	}

	public static void RemoveReference(UILabel inLabel)
	{
		if (inLabel == null || inLabel.bitmapFont == null || inLabel.ignoreFontChange)
		{
			return;
		}
		FontData fontData = FindFontData(inLabel.bitmapFont.name.Replace("--Ref", ""));
		if (fontData == null)
		{
			return;
		}
		fontData.pRefCount--;
		if (fontData.pRefCount <= 0)
		{
			fontData.pRefCount = 0;
			if (fontData.pFont != null)
			{
				UnityEngine.Object.Destroy(fontData.pFont.gameObject);
			}
			fontData.pFont = null;
			mFontData.Remove(fontData);
		}
	}

	private static void FontBundleReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			string[] array = inURL.Split('/');
			RsResourceManager.SetDontDestroy(array[0] + "/" + array[1], inDontDestroy: true);
			GameObject gameObject = UnityEngine.Object.Instantiate(inObject as GameObject);
			gameObject.transform.parent = mRootObject;
			mInstance.mBundleInfo = inUserData as FontBundleMapping;
			mInstance.mPrevFont = mInstance.mMainFont;
			mInstance.mMainFont = gameObject.GetComponent<UIFont>();
			if (!mInstance.mReplaceFont)
			{
				mInstance.ReplaceFont();
			}
			UtDebug.Log("mInstance.mLoadingCount " + mInstance.mLoadingCount);
			if (mInstance.mLoadingCount > 0)
			{
				mInstance.mLoadingCount--;
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("###### Could not load Font Bundle " + inURL);
			if (mInstance.mLoadingCount > 0)
			{
				mInstance.mLoadingCount--;
			}
			break;
		}
	}

	public void ReplaceFont()
	{
		if (mBundleInfo == null || mMainFont == null)
		{
			return;
		}
		if (mPrevFont != null)
		{
			UnityEngine.Object.Destroy(mPrevFont.gameObject);
		}
		for (int i = 0; i < mBundleInfo._Font.Count; i++)
		{
			if (mBundleInfo._Font[i] != null)
			{
				FontData fontData = FindFontData(mBundleInfo._Font[i].name);
				if (fontData != null)
				{
					fontData.pFont.replacement = mMainFont;
				}
			}
		}
	}

	private static FontData FindFontData(string inFontName)
	{
		if (mFontData == null)
		{
			return null;
		}
		return mFontData.Find((FontData fontData) => fontData != null && fontData.pFontName == inFontName);
	}

	private static FontData CreateFontData(string name, GameObject gameObject)
	{
		FontData fontData = new FontData();
		fontData.pFontName = name;
		GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
		gameObject2.transform.parent = mRootObject;
		fontData.pFont = gameObject2.GetComponent<UIFont>();
		fontData.pFont.name = name + "--Ref";
		if (fontData.pFont.material != null && !fontData.pFont.material.mainTexture.name.Contains("--Ref"))
		{
			fontData.pFont.material.mainTexture.name += "--Ref";
		}
		mFontData.Add(fontData);
		return fontData;
	}

	public void PreloadAllFonts(bool replaceFont = false)
	{
		mLoadingCount = 0;
		if (!_UseDynamicFonts)
		{
			return;
		}
		FontMappingInfo[] fontMappingInfo = pInstance._FontMappingInfo;
		foreach (FontMappingInfo fontMappingInfo2 in fontMappingInfo)
		{
			if (fontMappingInfo2._LocaleKeys.Contains(UtUtilities.GetLocaleLanguage()))
			{
				mCurrentFontMapping = fontMappingInfo2;
				break;
			}
		}
		if (mCurrentFontMapping == null || mCurrentFontMapping._BundleInfo.Length == 0)
		{
			return;
		}
		mLoadingCount = mCurrentFontMapping._BundleInfo.Length;
		mReplaceFont = replaceFont;
		FontBundleMapping[] bundleInfo = mCurrentFontMapping._BundleInfo;
		foreach (FontBundleMapping fontBundleMapping in bundleInfo)
		{
			if (fontBundleMapping._Font != null && fontBundleMapping._Font.Count > 0)
			{
				string[] array = fontBundleMapping._FontBundleName.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], FontBundleReady, typeof(GameObject), inDontDestroy: false, fontBundleMapping);
			}
		}
	}

	public bool IsMappedToMainFont(string fontName)
	{
		if (mCurrentFontMapping != null)
		{
			FontBundleMapping[] bundleInfo = mCurrentFontMapping._BundleInfo;
			foreach (FontBundleMapping fontBundleMapping in bundleInfo)
			{
				if (fontBundleMapping._Font != null && fontBundleMapping._Font.Count > 0 && (bool)fontBundleMapping._Font.Find((UIFont x) => x.name == fontName))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static void DestroyTexture(Texture inTexture)
	{
		inTexture = null;
	}
}
