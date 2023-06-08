using UnityEngine;

public class UiLoginAds : KAUI
{
	private KAWidget mAdImage;

	private KAUIMenu mTabMenu;

	private KAWidget mVideoBtn;

	private KAWidget mActiveTabWidget;

	private string[] mUrls;

	private string[] mVideoUrls;

	private int mCurrentAdIndex;

	private float mRefreshRate;

	private float mFadeTime = 1f;

	public KAUI _LoadingGearUI;

	protected override void Start()
	{
		base.Start();
		mAdImage = FindItem("BtnAd");
		mVideoBtn = FindItem("BtnViewVideo");
		mTabMenu = _MenuList[0];
	}

	public void Init(AdSection adSection)
	{
		if (adSection.AdAttributes == null || adSection.AdAttributes.Length == 0)
		{
			return;
		}
		mUrls = new string[adSection.AdAttributes.Length];
		for (int i = 0; i < mUrls.Length; i++)
		{
			mUrls[i] = adSection.AdAttributes[i].URL;
		}
		mVideoUrls = new string[adSection.AdAttributes.Length];
		for (int j = 0; j < mVideoUrls.Length; j++)
		{
			if (string.IsNullOrEmpty(adSection.AdAttributes[j].VideoURL))
			{
				mVideoUrls[j] = string.Empty;
			}
			else
			{
				mVideoUrls[j] = adSection.AdAttributes[j].VideoURL;
			}
		}
		for (int k = 0; k < mUrls.Length; k++)
		{
			mTabMenu.AddWidget(mTabMenu._Template.name).SetVisibility(inVisible: true);
		}
		mCurrentAdIndex = 0;
		mAdImage.SetVisibility(inVisible: true);
		ShowVideoBtn(mCurrentAdIndex);
		if (_LoadingGearUI != null)
		{
			_LoadingGearUI.SetVisibility(inVisible: true);
		}
		mAdImage.SetTextureFromURL(mUrls[mCurrentAdIndex], null, null, ignoreReferenceCount: true);
		mActiveTabWidget = mTabMenu.GetItemAt(mCurrentAdIndex);
		mActiveTabWidget.SetDisabled(isDisabled: true);
		mRefreshRate = adSection.RefreshRate;
		mFadeTime = adSection.FadeTime;
		InvokeRepeating("UpdateDisplay", mRefreshRate, mRefreshRate);
	}

	private void UpdateDisplay()
	{
		if (mUrls != null)
		{
			mCurrentAdIndex++;
			if (mCurrentAdIndex == mUrls.Length)
			{
				mCurrentAdIndex = 0;
			}
			UpdateDisplayByIndex(mCurrentAdIndex);
		}
	}

	private void UpdateDisplayByIndex(int index)
	{
		if (mUrls != null)
		{
			mActiveTabWidget.SetDisabled(isDisabled: false);
			mCurrentAdIndex = index;
			mActiveTabWidget = mTabMenu.GetItemAt(mCurrentAdIndex);
			mActiveTabWidget.SetDisabled(isDisabled: true);
			mAdImage.StopColorBlendTo();
			if (_LoadingGearUI != null)
			{
				_LoadingGearUI.SetVisibility(inVisible: true);
			}
			mAdImage.SetTextureFromURL(mUrls[index], base.gameObject, null, ignoreReferenceCount: true);
			ShowVideoBtn(index);
		}
	}

	private void ShowVideoBtn(int index)
	{
		if (mVideoUrls[index] == string.Empty)
		{
			mVideoBtn.SetVisibility(inVisible: false);
		}
		else
		{
			mVideoBtn.SetVisibility(inVisible: true);
		}
	}

	private void OnTextureLoaded(KAWidget widget)
	{
		if (!(widget != mAdImage))
		{
			if (_LoadingGearUI != null)
			{
				_LoadingGearUI.SetVisibility(inVisible: false);
			}
			Color color = widget.GetUITexture().color;
			color.a = 0f;
			widget.ColorBlendTo(end: new Color(color.r, color.g, color.b, 1f), start: color, duration: mFadeTime);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mAdImage)
		{
			UpdateDisplay();
			CancelInvoke("UpdateDisplay");
			InvokeRepeating("UpdateDisplay", mRefreshRate, mRefreshRate);
		}
		else if (inWidget == mTabMenu.GetClickedItem())
		{
			UpdateDisplayByIndex(mTabMenu.GetSelectedItemIndex());
			CancelInvoke("UpdateDisplay");
			InvokeRepeating("UpdateDisplay", mRefreshRate, mRefreshRate);
		}
		else if (inWidget == mVideoBtn)
		{
			MovieManager.SetBackgroundColor(Color.black);
			MovieManager.Play(mVideoUrls[mCurrentAdIndex], OnMovieStarted, VideoFinished, skipMovie: true);
		}
	}

	private void VideoFinished()
	{
	}

	private void OnMovieStarted()
	{
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		CancelInvoke("UpdateDisplay");
	}

	public void Reset()
	{
		mAdImage.SetVisibility(inVisible: false);
		mActiveTabWidget = null;
		mTabMenu.ClearItems();
		CancelInvoke("UpdateDisplay");
		mCurrentAdIndex = 0;
		mRefreshRate = 0f;
		mFadeTime = 0f;
	}

	protected override void UpdateVisibility(bool inVisible)
	{
		base.UpdateVisibility(inVisible);
		if (mTabMenu != null && mTabMenu.pMenuGrid != null)
		{
			mTabMenu.pMenuGrid.Reposition();
		}
	}
}
