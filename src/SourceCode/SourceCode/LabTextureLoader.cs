using System;
using System.Collections.Generic;
using UnityEngine;

public class LabTextureLoader
{
	public class TexLoader
	{
		public string mKey;

		public string mBundlePath;

		public ItemTextureResData mResData;

		public TexLoader(string inKey, string inBundlePath)
		{
			mKey = inKey;
			mBundlePath = inBundlePath;
			mResData = new ItemTextureResData();
		}
	}

	public delegate void OnLoaded(bool inSuccess);

	private List<TexLoader> mList;

	private OnLoaded mCallback;

	public RsResourceLoadEvent pState;

	public bool AddTextureData(string inKey, string inPath)
	{
		if (string.IsNullOrEmpty(inKey) || string.IsNullOrEmpty(inPath) || Get(inKey) != null || pState != 0)
		{
			return false;
		}
		if (mList == null)
		{
			mList = new List<TexLoader>();
		}
		TexLoader texLoader = new TexLoader(inKey, inPath);
		mList.Add(texLoader);
		texLoader.mResData.Init(ItemResEventHandler, inPath);
		return true;
	}

	public void Load(OnLoaded inCallback)
	{
		if (inCallback != null)
		{
			if (mCallback == null)
			{
				mCallback = inCallback;
			}
			else
			{
				mCallback = (OnLoaded)Delegate.Combine(mCallback, inCallback);
			}
		}
		for (int i = 0; i < mList.Count; i++)
		{
			mList[i].mResData.LoadData();
			pState = RsResourceLoadEvent.PROGRESS;
		}
	}

	public void ItemResEventHandler(ItemResNameData inResData)
	{
		if (pState == RsResourceLoadEvent.ERROR)
		{
			return;
		}
		if (!inResData.IsDataLoaded())
		{
			pState = RsResourceLoadEvent.ERROR;
			return;
		}
		for (int i = 0; i < mList.Count; i++)
		{
			if (mList[i] != null && !mList[i].mResData.IsDataLoaded())
			{
				pState = RsResourceLoadEvent.PROGRESS;
				return;
			}
		}
		pState = RsResourceLoadEvent.COMPLETE;
		if (mCallback != null)
		{
			mCallback(inSuccess: true);
		}
	}

	public Texture GetTexture(string inKey)
	{
		if (string.IsNullOrEmpty(inKey))
		{
			return null;
		}
		TexLoader texLoader = Get(inKey);
		if (texLoader != null && texLoader.mResData != null)
		{
			return texLoader.mResData._Texture;
		}
		return null;
	}

	public TexLoader Get(string inKey)
	{
		if (string.IsNullOrEmpty(inKey) || mList == null)
		{
			return null;
		}
		for (int i = 0; i < mList.Count; i++)
		{
			if (mList[i] != null && mList[i].mKey == inKey)
			{
				return mList[i];
			}
		}
		return null;
	}
}
