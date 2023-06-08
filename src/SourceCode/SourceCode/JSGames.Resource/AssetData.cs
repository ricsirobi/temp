using System;

namespace JSGames.Resource;

public abstract class AssetData
{
	public string _ResName = "";

	public string _ResUrl = "";

	public object _ResObject;

	public Action<AssetData> _Callback;

	public void Init(Action<AssetData> callback, string url)
	{
		_Callback = callback;
		Init(url);
	}

	public virtual void Init(string url)
	{
		_ResUrl = "";
		_ResObject = null;
		if (!string.IsNullOrEmpty(url))
		{
			_ResName = url.Split('/')[^1];
			_ResUrl = url;
		}
		else
		{
			UtDebug.LogError("=====  Empty URL   =====");
		}
	}

	protected void OnAssetLoadingEvent(string url, RsResourceLoadEvent resourceLoadEvent, float progress, object obj, object userData)
	{
		switch (resourceLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			OnAssetLoaded(url, obj);
			break;
		case RsResourceLoadEvent.ERROR:
			if (_Callback != null)
			{
				_Callback(this);
			}
			break;
		}
	}

	public virtual void LoadAsset(UrlItemData urlItemData, string assetType)
	{
		if (!string.IsNullOrEmpty(_ResUrl))
		{
			urlItemData.LoadAsset(_ResUrl, assetType);
		}
	}

	public virtual bool OnAssetLoaded(string url, object obj)
	{
		if (!string.IsNullOrEmpty(_ResUrl) && url == _ResUrl && _ResObject == null)
		{
			if (obj != null)
			{
				_ResObject = obj;
				LoadContent(obj);
			}
			if (_Callback != null)
			{
				_Callback(this);
			}
			return true;
		}
		if (_Callback != null)
		{
			_Callback(this);
		}
		return false;
	}

	protected virtual void LoadContent(object obj)
	{
	}
}
