using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemStoreDataLoader
{
	public delegate void OnStoreListLoaded(List<StoreData> inStoreData, object inUserData);

	private static List<StoreData> mStores = new List<StoreData>();

	private object mUserData;

	private OnStoreListLoaded mCallback;

	private List<StoreData> mStoreData;

	private int[] mStoreIDs;

	private List<int> mStoreIDsToLoad;

	public ItemStoreDataLoader(int[] inStoreIDs, OnStoreListLoaded inCallback, object inUserData)
	{
		mStoreIDs = inStoreIDs;
		mCallback = inCallback;
		mUserData = inUserData;
	}

	private static StoreData FindStore(int storeID)
	{
		foreach (StoreData mStore in mStores)
		{
			if (mStore._ID == storeID)
			{
				return mStore;
			}
		}
		return null;
	}

	private static void RemoveStoreData(StoreData inStoreData)
	{
		if (mStores != null)
		{
			mStores.Remove(inStoreData);
		}
	}

	private static void AddStoreData(StoreData inStoreData)
	{
		if (mStores == null)
		{
			mStores = new List<StoreData>();
		}
		mStores.Add(inStoreData);
	}

	public static void RemoveAllStoreData()
	{
		UtDebug.Log("STORE DATA COUNT: " + mStores.Count + ", REMOVING ALL STORE DATA!!");
		mStores.Clear();
	}

	public static ItemData[] GetItems(int storeID)
	{
		return FindStore(storeID)?._Items;
	}

	public static ItemData GetItem(int inItemID)
	{
		foreach (StoreData mStore in mStores)
		{
			if (mStore != null)
			{
				ItemData itemData = mStore.FindItem(inItemID);
				if (itemData != null)
				{
					return itemData;
				}
			}
		}
		return null;
	}

	public static ItemData GetItem(int inItemID, int inStoreID)
	{
		return FindStore(inStoreID)?.FindItem(inItemID);
	}

	public static List<StoreData> GetAllStores()
	{
		return mStores;
	}

	public void LoadStore(float inForceLoadTime)
	{
		if (mStoreIDs == null || mStoreIDs.Length == 0)
		{
			if (mCallback != null)
			{
				mCallback(null, mUserData);
			}
			return;
		}
		int[] array = mStoreIDs;
		foreach (int num in array)
		{
			StoreData storeData = FindStore(num);
			bool flag = false;
			if (storeData == null)
			{
				flag = true;
			}
			else if (!storeData._IsLoading)
			{
				float num2 = Mathf.Max(0f, Time.time - storeData._LastLoadedTime);
				if (inForceLoadTime >= 0f && num2 > inForceLoadTime)
				{
					flag = true;
					RemoveStoreData(storeData);
				}
			}
			if (mStoreData == null)
			{
				mStoreData = new List<StoreData>();
			}
			if (flag)
			{
				if (mStoreIDsToLoad == null)
				{
					mStoreIDsToLoad = new List<int>();
				}
				mStoreIDsToLoad.Add(num);
				StoreData storeData2 = new StoreData();
				storeData2._ID = num;
				storeData2._Items = null;
				storeData2._IsLoading = true;
				storeData2._LastLoadedTime = Time.time;
				mStoreData.Add(storeData2);
				AddStoreData(storeData2);
			}
			else if (storeData._IsLoading)
			{
				mStoreData.Add(storeData);
				storeData._StoreDataLoadedCallback = (StoreData.OnStoreDataLoaded)Delegate.Combine(storeData._StoreDataLoadedCallback, new StoreData.OnStoreDataLoaded(OnStoreDataLoaded));
			}
			else
			{
				mStoreData.Add(storeData);
			}
		}
		if (mStoreIDsToLoad == null || mStoreIDsToLoad.Count == 0)
		{
			CheckAllStoreLoaded();
		}
		else
		{
			WsWebService.GetStore(mStoreIDsToLoad.ToArray(), GetStoreEventHandler, mStoreIDsToLoad);
		}
	}

	public void GetStoreEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			GetStoreResponse getStoreResponse = (GetStoreResponse)inObject;
			ItemsInStoreData[] array = null;
			if (getStoreResponse != null)
			{
				array = getStoreResponse.Stores;
			}
			if (array == null)
			{
				break;
			}
			ItemsInStoreData[] array2 = array;
			foreach (ItemsInStoreData itemsInStoreData in array2)
			{
				StoreData storeData2 = FindStore(itemsInStoreData.ID.Value);
				if (storeData2 != null)
				{
					storeData2._Items = itemsInStoreData.Items;
					storeData2.SetStoreData(itemsInStoreData);
					storeData2._IsLoading = false;
					if (storeData2._StoreDataLoadedCallback != null)
					{
						storeData2._StoreDataLoadedCallback(storeData2);
						storeData2._StoreDataLoadedCallback = null;
					}
				}
			}
			CheckAllStoreLoaded();
			break;
		}
		case WsServiceEvent.ERROR:
			foreach (int item in (List<int>)inUserData)
			{
				StoreData storeData = FindStore(item);
				if (storeData != null)
				{
					storeData._IsLoading = false;
					RemoveStoreData(storeData);
				}
			}
			mCallback(null, mUserData);
			break;
		}
	}

	private static void SingleStoreLoaded(List<StoreData> inStoreData, object inUserData)
	{
		StoreData.OnStoreDataLoaded onStoreDataLoaded = (StoreData.OnStoreDataLoaded)inUserData;
		if (onStoreDataLoaded != null)
		{
			if (inStoreData == null || inStoreData.Count == 0)
			{
				onStoreDataLoaded(null);
			}
			else
			{
				onStoreDataLoaded(inStoreData[0]);
			}
		}
	}

	public void CheckAllStoreLoaded()
	{
		if (mCallback == null)
		{
			return;
		}
		if (mStoreIDs == null || mStoreIDs.Length == 0)
		{
			mCallback(null, mUserData);
			return;
		}
		int[] array = mStoreIDs;
		for (int i = 0; i < array.Length; i++)
		{
			StoreData storeData = FindStore(array[i]);
			if (storeData == null || storeData._IsLoading)
			{
				return;
			}
		}
		mCallback(mStoreData, mUserData);
	}

	public void OnStoreDataLoaded(StoreData inStoreData)
	{
		if (mStoreIDs != null && mStoreIDs.Length != 0)
		{
			CheckAllStoreLoaded();
		}
	}

	public static void Load(int[] inStoreIDs, OnStoreListLoaded inCallback, object inUserData, float inForceTimeLoad = -1f)
	{
		if (inStoreIDs == null || inStoreIDs.Length == 0)
		{
			inCallback?.Invoke(null, inUserData);
		}
		else
		{
			new ItemStoreDataLoader(inStoreIDs, inCallback, inUserData).LoadStore(inForceTimeLoad);
		}
	}

	public static void Load(int storeID, StoreData.OnStoreDataLoaded inCallback, float inTimeToForceStoreLoad = -1f)
	{
		Load(new int[1] { storeID }, SingleStoreLoaded, inCallback, inTimeToForceStoreLoad);
	}
}
