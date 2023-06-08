using System.Collections.Generic;

namespace Xsolla;

public abstract class XsollaObjectsManager<T> where T : IXsollaObject
{
	protected List<T> itemsList;

	protected Dictionary<string, T> itemsMap;

	public int Count { get; private set; }

	public XsollaObjectsManager()
	{
		itemsList = new List<T>();
		itemsMap = new Dictionary<string, T>();
		Count = 0;
	}

	public void AddItem(T item)
	{
		if (!itemsMap.ContainsKey(item.GetKey()))
		{
			itemsList.Add(item);
			itemsMap.Add(item.GetKey(), item);
			Count++;
		}
	}

	public List<T> GetItemsList()
	{
		return itemsList;
	}

	public List<string> GetTitleList()
	{
		List<string> list = new List<string>(itemsList.Count);
		foreach (T items in itemsList)
		{
			list.Add(items.GetName());
		}
		return list;
	}

	public Dictionary<string, string> GetNamesDict()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(itemsList.Count);
		foreach (T items in itemsList)
		{
			dictionary.Add(items.GetKey(), items.GetName());
		}
		return dictionary;
	}

	private static string tryIt(T t)
	{
		return t.GetName();
	}

	public T GetItemByPosition(int position)
	{
		return itemsList[position];
	}

	public T GetItemByKey(string key)
	{
		return itemsMap[key];
	}

	public int GetCount()
	{
		return Count;
	}
}
