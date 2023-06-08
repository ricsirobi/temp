using UnityEngine;

public class FishingData : MonoBehaviour
{
	private const int mStoreID = 94;

	private StoreData mStoreData;

	private static bool mIsReady;

	private static FishingData mInstance;

	public static bool pIsReady => mIsReady;

	public static FishingData pInstance => mInstance;

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	public void LoadStoreData()
	{
		ItemStoreDataLoader.Load(94, OnStoreLoaded);
	}

	public static void Init()
	{
		if (mInstance == null)
		{
			mInstance = new GameObject("FishingData").AddComponent<FishingData>();
		}
	}

	public void OnStoreLoaded(StoreData sd)
	{
		if (sd != null)
		{
			mStoreData = sd;
			if (mStoreData == null || mStoreData._Items == null)
			{
				UtDebug.LogError("Fishing Store doesnt have items");
			}
			mIsReady = true;
		}
	}

	public string GetFishAssetPath(int fishID)
	{
		if (mStoreData != null)
		{
			ItemData itemData = mStoreData.FindItem(fishID);
			if (itemData != null)
			{
				return itemData.AssetName;
			}
		}
		return null;
	}

	public string GetFishName(int fishID)
	{
		if (mStoreData != null)
		{
			ItemData itemData = mStoreData.FindItem(fishID);
			if (itemData != null)
			{
				return itemData.ItemName;
			}
		}
		return "";
	}
}
