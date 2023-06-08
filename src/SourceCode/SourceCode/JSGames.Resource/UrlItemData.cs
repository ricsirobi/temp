using JSGames.UI;
using UnityEngine;

namespace JSGames.Resource;

public class UrlItemData
{
	public TextureData _ItemTextureData = new TextureData();

	public AudioData _ItemRollOverVOData = new AudioData();

	public JSGames.UI.UIWidget _Widget;

	protected int mCurLoadingAssetIndex;

	protected int mAssetCount;

	protected WidgetState mWidgetState = WidgetState.INTERACTIVE;

	public Texture pIconTexture => _ItemTextureData._Texture;

	public AudioClip pRollOverVO => _ItemRollOverVOData._Clip;

	public bool pIsReady { get; private set; }

	public virtual void OnAllDownloaded()
	{
		JSGames.UI.UIWidget widget = _Widget;
		if (widget != null)
		{
			widget.mainTexture = _ItemTextureData._Texture;
			widget.pState = mWidgetState;
			if (_ItemRollOverVOData._Clip != null)
			{
				widget._HoverEffects._Clip._AudioClip = _ItemRollOverVOData._Clip;
				widget._HoverEffects._Clip._Settings._Pool = "VO_Pool";
				widget._HoverEffects._Clip._Settings._Priority = 0;
			}
			pIsReady = true;
			widget.pVisible = true;
			ShowLoadingWidget(show: false);
		}
	}

	public virtual void OnAssetLoadProgress(string url, float progress)
	{
	}

	public virtual void OnAssetLoadFailed(string url)
	{
		_ItemTextureData.OnAssetLoaded(url, null);
		_ItemRollOverVOData.OnAssetLoaded(url, null);
		mCurLoadingAssetIndex++;
		if (mCurLoadingAssetIndex >= mAssetCount)
		{
			OnAllDownloaded();
		}
	}

	public virtual void OnAssetLoaded(string url, object bd)
	{
		_ItemTextureData.OnAssetLoaded(url, bd);
		_ItemRollOverVOData.OnAssetLoaded(url, bd);
		mCurLoadingAssetIndex++;
		if (mCurLoadingAssetIndex >= mAssetCount)
		{
			OnAllDownloaded();
		}
	}

	public void OnAssetLoadingEvent(string url, RsResourceLoadEvent resourceLoadEvent, float progress, object obj, object userData)
	{
		switch (resourceLoadEvent)
		{
		case RsResourceLoadEvent.PROGRESS:
			OnAssetLoadProgress(url, progress);
			break;
		case RsResourceLoadEvent.COMPLETE:
			OnAssetLoaded(url, obj);
			break;
		case RsResourceLoadEvent.ERROR:
			OnAssetLoadFailed(url);
			break;
		}
	}

	public UrlItemData()
	{
	}

	public UrlItemData(string tn, string sn)
	{
		_ItemTextureData.Init(tn);
		_ItemRollOverVOData.Init(sn);
	}

	public virtual void LoadResource()
	{
		pIsReady = false;
		mAssetCount = 0;
		mCurLoadingAssetIndex = 0;
		ShowLoadingWidget(show: true);
		LoadAssetData();
		if (mAssetCount == 0)
		{
			pIsReady = true;
			ShowLoadingWidget(show: false);
		}
	}

	public virtual void ShowLoadingWidget(bool show)
	{
	}

	public void LoadAsset(string url, string assetType)
	{
		mAssetCount++;
		if (assetType.Equals("Image"))
		{
			RsResourceManager.Load(url, OnAssetLoadingEvent, RsResourceType.IMAGE);
		}
		else if (assetType.Equals("Sound"))
		{
			RsResourceManager.Load(url, OnAssetLoadingEvent, RsResourceType.AUDIO);
		}
	}

	protected virtual void LoadAssetData()
	{
		_ItemTextureData.LoadAsset(this, "Image");
		_ItemRollOverVOData.LoadAsset(this, "Sound");
	}
}
