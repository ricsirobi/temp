using System.Collections.Generic;
using UnityEngine;

public class CoBundleItemData : KAWidgetUserData
{
	public ItemTextureResData _ItemTextureData = new ItemTextureResData();

	public ItemAudioResData _ItemRVOData = new ItemAudioResData();

	public bool mLoadingCalled;

	protected int mCurLoadingBundleIdx;

	protected int mNumBundles = -1;

	protected bool mMakePixelPerfect;

	public List<string> _BundleNames = new List<string>();

	public bool pIsReady;

	public Texture _IconTexture => _ItemTextureData._Texture;

	public AudioClip _Dlg => _ItemRVOData._Clip;

	public void CopyData(CoBundleItemData s)
	{
		_ItemTextureData.CopyData(s._ItemTextureData);
		_ItemRVOData.CopyData(s._ItemRVOData);
	}

	public virtual bool IsDownloaded()
	{
		return mNumBundles != -1;
	}

	public virtual void OnAllDownloaded()
	{
		KAWidget item = GetItem();
		if (item != null)
		{
			item.SetTexture(_ItemTextureData._Texture, mMakePixelPerfect);
			item.name = _ItemTextureData._ResName;
			item.SetState(KAUIState.INTERACTIVE);
			if (_ItemRVOData._Clip != null)
			{
				item._HoverInfo._Clip._AudioClip = _ItemRVOData._Clip;
				item._HoverInfo._Clip._Settings._Pool = "VO_Pool";
				item._HoverInfo._Clip._Settings._Priority = 0;
			}
			pIsReady = true;
			item.SetVisibility(inVisible: true);
		}
	}

	public virtual void OnBundleProgress(string inURL, float inProgress)
	{
	}

	public virtual void OnBundleError(string inURL)
	{
		_ItemTextureData.OnBundleReady(inURL, null);
		_ItemRVOData.OnBundleReady(inURL, null);
		mCurLoadingBundleIdx++;
		if (mCurLoadingBundleIdx >= mNumBundles)
		{
			OnAllDownloaded();
		}
	}

	public virtual void OnBundleReady(string inURL, AssetBundle bd)
	{
		_ItemTextureData.OnBundleReady(inURL, bd);
		_ItemRVOData.OnBundleReady(inURL, bd);
		mCurLoadingBundleIdx++;
		if (mCurLoadingBundleIdx >= mNumBundles)
		{
			OnAllDownloaded();
		}
	}

	public void OnResLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.PROGRESS:
			OnBundleProgress(inURL, inProgress);
			break;
		case RsResourceLoadEvent.COMPLETE:
			OnBundleReady(inURL, (AssetBundle)inObject);
			break;
		case RsResourceLoadEvent.ERROR:
			OnBundleError(inURL);
			break;
		}
	}

	public CoBundleItemData()
	{
	}

	public CoBundleItemData(string tn, string sn)
	{
		_ItemTextureData.Init(tn);
		_ItemRVOData.Init(sn);
	}

	public virtual bool IsNotLoaded()
	{
		return !mLoadingCalled;
	}

	public virtual void LoadResource()
	{
		pIsReady = false;
		mLoadingCalled = true;
		mNumBundles = 0;
		_BundleNames.Clear();
		mCurLoadingBundleIdx = 0;
		_ItemTextureData.LoadBundle(this);
		_ItemRVOData.LoadBundle(this);
		if (mNumBundles == 0)
		{
			pIsReady = true;
		}
	}

	public virtual void InstanceData()
	{
	}

	public virtual void ReleaseData()
	{
	}

	public virtual void ShowLoadingItem(bool inShow)
	{
	}

	public bool FindBundle(string bname)
	{
		foreach (string bundleName in _BundleNames)
		{
			if (bundleName == bname)
			{
				return true;
			}
		}
		return false;
	}

	public void LoadBundle(string bname)
	{
		if (!FindBundle(bname))
		{
			mNumBundles++;
			_BundleNames.Add(bname);
			RsResourceManager.Load(bname, OnResLoadingEvent);
		}
	}
}
