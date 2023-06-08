using System;
using System.Collections.Generic;

[Serializable]
public class StoreFilter
{
	public enum StoreUserOptions
	{
		NONE,
		SHOW,
		HIDE
	}

	public enum StoreFilterIcon
	{
		Priority,
		Sale,
		LimitedAvailability,
		New,
		Popular
	}

	public enum SortType
	{
		None,
		Popular,
		Sale,
		Limited,
		New
	}

	public int[] _CategoryIDs;

	public int[] _ExcludeCategoryIDs;

	public StoreUserOptions _New;

	public StoreUserOptions _Sale;

	public StoreUserOptions _LimitedAvailablity;

	public StoreUserOptions _Popular;

	public StoreFilterIcon _IconOption;

	public bool _DisableRankCheck;

	public SortType _SortType;

	[NonSerialized]
	public float _Discount;

	public StoreFilter(int inCategoryID)
	{
		_CategoryIDs = new int[1];
		_CategoryIDs[0] = inCategoryID;
	}

	public bool HasCategory(int inCategoryID)
	{
		if (_CategoryIDs == null || _CategoryIDs.Length == 0)
		{
			return false;
		}
		int[] categoryIDs = _CategoryIDs;
		for (int i = 0; i < categoryIDs.Length; i++)
		{
			if (categoryIDs[i] == inCategoryID)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsSame(StoreFilter inFilter)
	{
		if (inFilter != null && UtUtilities.IsSameList(_CategoryIDs, inFilter._CategoryIDs) && _New == inFilter._New && _Sale == inFilter._Sale && _LimitedAvailablity == inFilter._LimitedAvailablity)
		{
			return _Popular == inFilter._Popular;
		}
		return false;
	}

	public bool CanFilter(ItemData inData)
	{
		if (inData == null)
		{
			return false;
		}
		bool flag = true;
		if (_CategoryIDs != null && _CategoryIDs.Length != 0)
		{
			flag = false;
			int[] categoryIDs = _CategoryIDs;
			foreach (int num in categoryIDs)
			{
				if (num == 0 || inData.HasCategory(num))
				{
					flag = true;
					break;
				}
			}
		}
		if (_ExcludeCategoryIDs != null)
		{
			int[] categoryIDs = _ExcludeCategoryIDs;
			foreach (int categoryID in categoryIDs)
			{
				if (inData.HasCategory(categoryID))
				{
					flag = false;
					break;
				}
			}
		}
		if (!flag)
		{
			return false;
		}
		if (_New != 0)
		{
			if (inData.IsNew && _New != StoreUserOptions.SHOW)
			{
				return false;
			}
			if (!inData.IsNew && _New != StoreUserOptions.HIDE)
			{
				return false;
			}
		}
		if (_Sale != 0)
		{
			bool flag2 = inData.IsOnSale();
			if (flag2 && _Sale != StoreUserOptions.SHOW)
			{
				return false;
			}
			if (!flag2 && _Sale != StoreUserOptions.HIDE)
			{
				return false;
			}
		}
		if (_LimitedAvailablity != 0)
		{
			bool flag3 = inData.GetAvailability()?.EndDate.HasValue ?? false;
			if (flag3 && _LimitedAvailablity != StoreUserOptions.SHOW)
			{
				return false;
			}
			if (!flag3 && _LimitedAvailablity != StoreUserOptions.HIDE)
			{
				return false;
			}
		}
		if (_Popular != 0)
		{
			if (inData.pIsPopular && _Popular != StoreUserOptions.SHOW)
			{
				return false;
			}
			if (!inData.pIsPopular && _Popular != StoreUserOptions.HIDE)
			{
				return false;
			}
		}
		return true;
	}

	public static List<ItemData> Filter(ItemData[] inItemData, StoreFilter inFilter)
	{
		if (inItemData == null || inItemData.Length == 0)
		{
			return null;
		}
		List<ItemData> list = null;
		if (inFilter == null)
		{
			if (list == null)
			{
				list = new List<ItemData>();
			}
			list.AddRange(inItemData);
			return list;
		}
		foreach (ItemData itemData in inItemData)
		{
			if (inFilter.CanFilter(itemData))
			{
				if (list == null)
				{
					list = new List<ItemData>();
				}
				list.Add(itemData);
			}
		}
		return list;
	}
}
