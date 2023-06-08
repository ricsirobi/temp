using System.Collections.Generic;

public class StoreCategoryData
{
	public string _CategoryName = "";

	public List<ItemData> _Items = new List<ItemData>();

	public void AddData(List<ItemData> inItems)
	{
		if (inItems != null)
		{
			if (_Items == null)
			{
				_Items = new List<ItemData>();
			}
			_Items.AddRange(inItems);
		}
	}

	public void AddData(ItemData inItem)
	{
		if (inItem != null)
		{
			if (_Items == null)
			{
				_Items = new List<ItemData>();
			}
			_Items.Add(inItem);
		}
	}
}
