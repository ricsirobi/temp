using System.Collections.Generic;

public class StoreData
{
	public delegate void OnStoreDataLoaded(StoreData inStoreData);

	public OnStoreDataLoaded _StoreDataLoadedCallback;

	public int _ID;

	public bool _IsLoading;

	public ItemsInStoreData _Data;

	public ItemData[] _Items;

	public float _LastLoadedTime;

	private Dictionary<StoreFilter, StoreCategoryData> mCategoryData = new Dictionary<StoreFilter, StoreCategoryData>();

	public void SetStoreData(ItemsInStoreData idata)
	{
		_Data = idata;
		_IsLoading = false;
		if (_Data == null || _Data.Items == null)
		{
			_Items = new ItemData[0];
			return;
		}
		_Items = _Data.Items;
		GenerateSortList();
		CheckItemsStatus();
	}

	public void AddCategoryData(StoreFilter inFilter, StoreCategoryData inCategoryData)
	{
		if (inFilter == null || inCategoryData == null)
		{
			return;
		}
		StoreCategoryData storeCategoryData = FindCategoryData(inFilter);
		if (storeCategoryData == null)
		{
			if (mCategoryData == null)
			{
				mCategoryData = new Dictionary<StoreFilter, StoreCategoryData>();
			}
			mCategoryData.Add(inFilter, inCategoryData);
		}
		else
		{
			storeCategoryData.AddData(inCategoryData._Items);
		}
	}

	public StoreCategoryData FindCategoryData(StoreFilter inFilter)
	{
		if (inFilter == null || mCategoryData == null || mCategoryData.Count == 0)
		{
			return null;
		}
		foreach (KeyValuePair<StoreFilter, StoreCategoryData> mCategoryDatum in mCategoryData)
		{
			if (mCategoryDatum.Key.IsSame(inFilter))
			{
				return mCategoryDatum.Value;
			}
		}
		return null;
	}

	public StoreCategoryData FindCategoryData(int cat)
	{
		return FindCategoryData(new StoreFilter(cat));
	}

	public StoreCategoryData FindCategoryData(string cat)
	{
		foreach (StoreCategoryData value in mCategoryData.Values)
		{
			if (value._CategoryName == cat)
			{
				return value;
			}
		}
		return null;
	}

	public ItemData FindItem(int itemID)
	{
		if (_Items == null || _Items.Length == 0)
		{
			return null;
		}
		ItemData[] items = _Items;
		foreach (ItemData itemData in items)
		{
			if (itemData.ItemID == itemID)
			{
				return itemData;
			}
		}
		return null;
	}

	public ItemData FindItem(string textureName, int categoryId = -1)
	{
		if (_Items == null || _Items.Length == 0)
		{
			return null;
		}
		StoreCategoryData storeCategoryData = FindCategoryData(categoryId);
		if (storeCategoryData != null)
		{
			for (int i = 0; i < storeCategoryData._Items.Count; i++)
			{
				if (storeCategoryData._Items[i].Texture != null && storeCategoryData._Items[i].Texture.Length != 0 && storeCategoryData._Items[i].Texture[0].TextureName.Contains(textureName))
				{
					return storeCategoryData._Items[i];
				}
			}
		}
		else
		{
			for (int j = 0; j < _Items.Length; j++)
			{
				if (_Items[j].Texture != null && _Items[j].Texture.Length != 0 && _Items[j].Texture[0].TextureName.Contains(textureName))
				{
					return _Items[j];
				}
			}
		}
		return null;
	}

	private void CheckItemsStatus()
	{
		ItemData[] items = _Items;
		for (int i = 0; i < items.Length; i++)
		{
			items[i].CheckNewState();
		}
		if (_Data == null)
		{
			return;
		}
		if (_Data.SalesAtStore != null)
		{
			ItemsInStoreDataSale[] salesAtStore = _Data.SalesAtStore;
			foreach (ItemsInStoreDataSale itemsInStoreDataSale in salesAtStore)
			{
				int[] categoryIDs;
				if (itemsInStoreDataSale.CategoryIDs != null)
				{
					categoryIDs = itemsInStoreDataSale.CategoryIDs;
					foreach (int cat in categoryIDs)
					{
						StoreCategoryData storeCategoryData = FindCategoryData(cat);
						if (storeCategoryData == null)
						{
							continue;
						}
						foreach (ItemData item in storeCategoryData._Items)
						{
							item.AddSale(itemsInStoreDataSale);
						}
					}
				}
				if (itemsInStoreDataSale.ItemIDs == null)
				{
					continue;
				}
				categoryIDs = itemsInStoreDataSale.ItemIDs;
				foreach (int num in categoryIDs)
				{
					items = _Items;
					foreach (ItemData itemData in items)
					{
						if (itemData.ItemID == num)
						{
							itemData.AddSale(itemsInStoreDataSale);
						}
					}
				}
			}
		}
		if (_Data.PopularItems == null || _Data.PopularItems.Length == 0)
		{
			return;
		}
		PopularStoreItem[] popularItems = _Data.PopularItems;
		foreach (PopularStoreItem popularStoreItem in popularItems)
		{
			if (popularStoreItem == null)
			{
				continue;
			}
			items = _Items;
			foreach (ItemData itemData2 in items)
			{
				if (itemData2.ItemID == popularStoreItem.ItemID)
				{
					itemData2.PopularRank = popularStoreItem.Rank;
				}
			}
		}
	}

	private void GenerateSortList()
	{
		if (_Items == null)
		{
			UtDebug.LogError("No item found in store");
			return;
		}
		ItemData[] items = _Items;
		foreach (ItemData itemData in items)
		{
			ItemData.AddToCache(itemData);
			if (itemData.Category == null)
			{
				StoreCategoryData storeCategoryData = FindCategoryData(-1);
				if (storeCategoryData == null)
				{
					storeCategoryData = new StoreCategoryData();
					storeCategoryData._CategoryName = "No Category";
					AddCategoryData(new StoreFilter(-1), storeCategoryData);
				}
				storeCategoryData._Items.Add(itemData);
				continue;
			}
			ItemDataCategory[] category = itemData.Category;
			foreach (ItemDataCategory itemDataCategory in category)
			{
				StoreCategoryData storeCategoryData2 = FindCategoryData(itemDataCategory.CategoryId);
				if (storeCategoryData2 == null)
				{
					storeCategoryData2 = new StoreCategoryData();
					storeCategoryData2._CategoryName = itemDataCategory.CategoryName;
					mCategoryData.Add(new StoreFilter(itemDataCategory.CategoryId), storeCategoryData2);
				}
				storeCategoryData2.AddData(itemData);
			}
		}
	}
}
