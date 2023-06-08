using UnityEngine;

public class ItemResNameData
{
	public string _ResFullName = "";

	public string _ResName = "";

	public string _ResBundleName = "";

	public Object _ResObject;

	public string _Location = "RS_DATA/";

	public AssetBundle _Bundle;

	public ItemResEventHandler _Callback;

	protected bool mLoaded;

	public void Init(ItemResEventHandler callback, string resName)
	{
		_Callback = callback;
		Init(resName);
	}

	public virtual void Init(string resName)
	{
		_ResBundleName = "";
		_ResObject = null;
		if (resName != null && resName.Length > 0)
		{
			if (resName.Contains(".unity3d"))
			{
				resName = resName.Replace(".unity3d", "");
			}
			_ResFullName = resName;
			string[] array = resName.Split('/');
			if (array.Length == 3)
			{
				_Location = array[0];
				_ResBundleName = array[0] + "/" + array[1];
				_ResName = array[2];
			}
			else if (array.Length != 2)
			{
				if (resName != "NULL")
				{
					UtDebug.LogError("Invalid resource " + resName);
				}
				_ResBundleName = "";
				_ResName = "";
			}
			else
			{
				_ResBundleName = _Location + array[0];
				_ResName = array[1];
			}
			mLoaded = false;
		}
		else
		{
			mLoaded = true;
		}
	}

	public void Init(ItemResEventHandler callback, string resName, string location)
	{
		_Callback = callback;
		Init(location, resName);
	}

	public void Init(string location, string resName)
	{
		_ResObject = null;
		_Location = location;
		Init(resName);
	}

	public void CopyData(ItemResNameData s)
	{
	}

	public void OnResLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			OnBundleReady(inURL, (AssetBundle)inObject);
			break;
		case RsResourceLoadEvent.ERROR:
			if (_Callback != null)
			{
				_Callback(this);
			}
			break;
		}
	}

	public virtual void LoadData()
	{
		if (!string.IsNullOrEmpty(_ResBundleName))
		{
			RsResourceManager.Load(_ResBundleName, OnResLoadingEvent);
		}
	}

	public virtual void LoadBundle(CoBundleItemData iData)
	{
		if (!string.IsNullOrEmpty(_ResBundleName))
		{
			iData.LoadBundle(_ResBundleName);
		}
	}

	public virtual bool OnBundleReady(string inURL, AssetBundle bd)
	{
		if (!string.IsNullOrEmpty(_ResBundleName) && RsResourceManager.Compare(inURL, _ResBundleName) && _ResObject == null)
		{
			mLoaded = true;
			if (bd != null)
			{
				_Bundle = bd;
				_ResObject = LoadRes(bd);
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

	public virtual Object LoadRes(AssetBundle bd)
	{
		return null;
	}

	public virtual bool IsDataLoaded()
	{
		return mLoaded;
	}
}
