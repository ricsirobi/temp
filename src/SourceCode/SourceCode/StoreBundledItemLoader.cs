using System.Collections.Generic;

public class StoreBundledItemLoader
{
	private ItemStoreDataLoader.OnStoreListLoaded mCallback;

	private List<StoreData> mStoreData;

	private object mUserData;

	private bool mStopLoading;

	public void Load(int[] inStoreIDs, ItemStoreDataLoader.OnStoreListLoaded inCallback, object inUserData, float inForceTimeLoad = -1f)
	{
		mCallback = inCallback;
		ItemStoreDataLoader.Load(inStoreIDs, OnStoreListLoaded, inUserData, inForceTimeLoad = -1f);
	}

	private void OnStoreListLoaded(List<StoreData> inStoreData, object inUserData)
	{
		mStoreData = inStoreData;
		mUserData = inUserData;
		foreach (StoreData inStoreDatum in inStoreData)
		{
			if (inStoreDatum != null)
			{
				ItemData[] items = inStoreDatum._Items;
				for (int i = 0; i < items.Length; i++)
				{
					items[i].LoadBundledItems(BundledItemEventHandler);
				}
			}
		}
	}

	private void BundledItemEventHandler(List<ItemData> dataItem, int inItemID)
	{
		if (mStopLoading)
		{
			return;
		}
		foreach (StoreData mStoreDatum in mStoreData)
		{
			if (mStoreDatum == null)
			{
				continue;
			}
			ItemData[] items = mStoreDatum._Items;
			foreach (ItemData itemData in items)
			{
				if (itemData != null && (itemData.pLoadBundleItemsState == RsResourceLoadEvent.NONE || itemData.pLoadBundleItemsState == RsResourceLoadEvent.PROGRESS))
				{
					return;
				}
			}
		}
		mStopLoading = true;
		if (mCallback != null)
		{
			mCallback(mStoreData, mUserData);
		}
		mCallback = null;
	}
}
