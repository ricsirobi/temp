using System.Collections.Generic;

public class ConsumableItems
{
	public class ConsumableData
	{
		public int _CategoryID;

		public int _ItemID;

		public ConsumableData(int categoryID, int itemID)
		{
			_CategoryID = categoryID;
			_ItemID = itemID;
		}
	}

	private List<ConsumableData> mConsumableItems = new List<ConsumableData>();

	public static ConsumableItems mInstance;

	public ConsumableItems()
	{
		mInstance = this;
	}

	public List<ConsumableData> GetConsumablesList()
	{
		return mInstance.mConsumableItems;
	}

	public void AddConsumableItem(int CategoryID, int ItemID)
	{
		if (mInstance.mConsumableItems != null)
		{
			foreach (ConsumableData mConsumableItem in mInstance.mConsumableItems)
			{
				if (mConsumableItem._CategoryID == CategoryID)
				{
					mInstance.mConsumableItems.Remove(mConsumableItem);
					break;
				}
			}
		}
		ConsumableData item = new ConsumableData(CategoryID, ItemID);
		mInstance.mConsumableItems.Add(item);
	}

	public ConsumableData GetItembyIndex(int index)
	{
		return mInstance.mConsumableItems[index];
	}

	public ConsumableData GetConsumableByCategory(int categoryid)
	{
		foreach (ConsumableData mConsumableItem in mInstance.mConsumableItems)
		{
			if (mConsumableItem._CategoryID == categoryid)
			{
				return mConsumableItem;
			}
		}
		return null;
	}
}
