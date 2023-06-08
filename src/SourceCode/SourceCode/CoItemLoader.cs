using UnityEngine;

public class CoItemLoader
{
	protected string mPrefResName = "";

	protected string mTextResName = "";

	protected ItemResReadyCallback mCallback;

	protected ItemPrefabResData m3DData = new ItemPrefabResData();

	protected ItemTextureResData mTexData = new ItemTextureResData();

	public bool pIsLoading;

	public GameObject GetObject()
	{
		return m3DData._Prefab.gameObject;
	}

	public void ItemResReady(ItemResNameData resData)
	{
		if (pIsLoading && m3DData.IsDataLoaded() && mTexData.IsDataLoaded())
		{
			pIsLoading = false;
			if (mCallback != null)
			{
				mCallback(this);
			}
		}
	}

	public virtual void LoadItem(ItemData item, ItemResReadyCallback callback)
	{
		if (pIsLoading)
		{
			Debug.LogError("Already loading");
			return;
		}
		pIsLoading = true;
		mCallback = callback;
		string text = "";
		if (item.Texture != null && item.Texture.Length != 0)
		{
			text = item.Texture[0].TextureName;
		}
		mPrefResName = item.AssetName;
		mTextResName = text;
		m3DData.Init(ItemResReady, mPrefResName);
		mTexData.Init(ItemResReady, mTextResName);
		m3DData.LoadData();
		mTexData.LoadData();
	}

	public void OnLoadItemDataReady(int itemID, ItemData dataItem, object inUserData)
	{
		pIsLoading = false;
		LoadItem(dataItem, mCallback);
	}

	public virtual void LoadItem(int itemID, ItemResReadyCallback callback)
	{
		if (pIsLoading)
		{
			Debug.LogError("Already loading");
			return;
		}
		mCallback = callback;
		pIsLoading = true;
		ItemData.Load(itemID, OnLoadItemDataReady, null);
	}
}
