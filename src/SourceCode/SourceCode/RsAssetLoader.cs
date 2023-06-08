using System.Collections.Generic;

public class RsAssetLoader
{
	public enum State
	{
		NOT_LOADED,
		LOADING_PREP,
		LOADING,
		READY
	}

	public delegate void LoadEventHandler(RsAssetLoader inAssetLoader, RsResourceLoadEvent inEvent, float inProgress, object userData);

	private int mErrorCount;

	private State mState;

	private int mTotalPending;

	private List<string> mPendingAssets = new List<string>();

	private LoadEventHandler mEventHandler;

	private object mUserData;

	public int pErrorCount => mErrorCount;

	public State pState => mState;

	public bool pIsReady => mState == State.READY;

	public string[] pAssets { get; private set; }

	public void Unload(string[] inAssetList, string inDataLocation)
	{
		for (int i = 0; i < inAssetList.Length; i++)
		{
			string[] array = inAssetList[i].Split('/');
			RsResourceManager.Unload((inDataLocation != null) ? (inDataLocation + "/" + array[0]) : (array[0] + ((array.Length > 1) ? ("/" + array[1]) : "")));
		}
	}

	public void Reset()
	{
		mState = State.NOT_LOADED;
	}

	public void Load(string[] inAssetList, string inDataLocation = null, LoadEventHandler inCallback = null, bool inDontDestroy = false, bool inIgnoreAssetVersion = false, object userData = null)
	{
		if (mState != 0)
		{
			return;
		}
		pAssets = inAssetList;
		mUserData = userData;
		mErrorCount = 0;
		mTotalPending = 0;
		mPendingAssets.Clear();
		mEventHandler = inCallback;
		mState = State.LOADING_PREP;
		for (int i = 0; i < inAssetList.Length; i++)
		{
			string[] array = inAssetList[i].Split('/');
			string bundleURL = ((inDataLocation != null) ? (inDataLocation + "/" + array[0]) : (array[0] + ((array.Length > 1) ? ("/" + array[1]) : "")));
			bundleURL = RsResourceManager.FormatBundleURL(bundleURL);
			mPendingAssets.Add(bundleURL);
			RsResourceManager.Load(bundleURL, ResourceEventHandler, RsResourceType.NONE, inDontDestroy);
		}
		if (mPendingAssets.Count == 0)
		{
			mState = State.READY;
			if (mEventHandler != null)
			{
				mEventHandler(this, (mErrorCount > 0) ? RsResourceLoadEvent.ERROR : RsResourceLoadEvent.COMPLETE, 1f, mUserData);
			}
		}
		else
		{
			mTotalPending = mPendingAssets.Count;
			mState = State.LOADING;
		}
	}

	public TYPE Get<TYPE>(string inAsset) where TYPE : class
	{
		return Get<TYPE>(inAsset, null);
	}

	public TYPE Get<TYPE>(string inAsset, string inDataLocation) where TYPE : class
	{
		if (!pIsReady)
		{
			return null;
		}
		return (TYPE)RsResourceManager.LoadAssetFromBundle((inDataLocation != null) ? (inDataLocation + "/" + inAsset) : inAsset, typeof(TYPE));
	}

	public void ResourceEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent != RsResourceLoadEvent.PROGRESS)
		{
			switch (inEvent)
			{
			case RsResourceLoadEvent.ERROR:
				UtDebug.LogError("ERROR: RS ASSET LOADER SKIPPING ITEM: " + inURL);
				mErrorCount++;
				break;
			case RsResourceLoadEvent.COMPLETE:
				break;
			default:
				return;
			}
			int num = 0;
			while (num < mPendingAssets.Count)
			{
				string[] array = mPendingAssets[num].Split('/');
				if (inURL.Contains(array[1]))
				{
					mPendingAssets.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
			if (mPendingAssets.Count == 0 && mState != State.LOADING_PREP && mState != State.READY)
			{
				mState = State.READY;
				if (mEventHandler != null)
				{
					mEventHandler(this, (mErrorCount > 0) ? RsResourceLoadEvent.ERROR : RsResourceLoadEvent.COMPLETE, 1f, mUserData);
				}
			}
		}
		else if (mEventHandler != null)
		{
			float num2 = mTotalPending - mPendingAssets.Count;
			num2 += inProgress;
			num2 /= (float)mTotalPending;
			mEventHandler(this, RsResourceLoadEvent.PROGRESS, num2, mUserData);
		}
	}
}
