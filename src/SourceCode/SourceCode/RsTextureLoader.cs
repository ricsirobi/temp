using System.Collections.Generic;
using UnityEngine;

public class RsTextureLoader
{
	private class RsTexture
	{
		public string _URL = string.Empty;

		public Texture _Texture;
	}

	public delegate void LoadEventHandler(RsTextureLoader inAssetLoader, RsResourceLoadEvent inEvent);

	private int mErrorCount;

	private int mPendingAssets;

	private List<string> mTextureUrl = new List<string>();

	private List<RsTexture> mTextures;

	private string mAvatarName = "";

	private LoadEventHandler mEventHandler;

	public void Load(string[] inTextureURLs, LoadEventHandler inCallback, string avatarName)
	{
		Release();
		mAvatarName = avatarName;
		mTextureUrl.Clear();
		mTextureUrl.AddRange(inTextureURLs);
		mErrorCount = 0;
		mPendingAssets = mTextureUrl.Count;
		mEventHandler = inCallback;
		mTextures = new List<RsTexture>();
		if (mTextureUrl.Count == 0)
		{
			if (mEventHandler != null)
			{
				mEventHandler(this, (mErrorCount > 0) ? RsResourceLoadEvent.ERROR : RsResourceLoadEvent.COMPLETE);
			}
			return;
		}
		for (int i = 0; i < mTextureUrl.Count; i++)
		{
			mTextureUrl[i] = UtUtilities.GetImageURL(mTextureUrl[i]);
			RsResourceManager.Load(mTextureUrl[i], ResourceEventHandler, RsResourceType.IMAGE, inDontDestroy: true);
		}
	}

	public void Release()
	{
		if (mTextures == null)
		{
			return;
		}
		foreach (string item in mTextureUrl)
		{
			RsResourceManager.Unload(item, splitURL: false);
		}
		mTextures.Clear();
		mTextures = null;
	}

	public void ResourceEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("ERROR: RS ASSET LOADER SKIPPING ITEM: " + inURL);
			mErrorCount++;
			break;
		case RsResourceLoadEvent.COMPLETE:
		{
			RsTexture rsTexture = new RsTexture();
			rsTexture._URL = inURL;
			rsTexture._Texture = (Texture)inObject;
			if (rsTexture._Texture != null)
			{
				rsTexture._Texture.name = "AvatarTexture-" + mAvatarName + "-" + mTextures.Count;
			}
			if (mTextures != null)
			{
				mTextures.Add(rsTexture);
			}
			break;
		}
		default:
			return;
		}
		mPendingAssets--;
		if (mPendingAssets == 0 && mEventHandler != null)
		{
			mEventHandler(this, (mErrorCount > 0) ? RsResourceLoadEvent.ERROR : RsResourceLoadEvent.COMPLETE);
		}
	}

	public Texture GetLoadedTexture(string inURL)
	{
		foreach (RsTexture mTexture in mTextures)
		{
			if (mTexture._URL.Contains(inURL))
			{
				return mTexture._Texture;
			}
		}
		return null;
	}
}
