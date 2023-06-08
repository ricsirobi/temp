using System.Collections.Generic;
using System.Linq;

namespace Xsolla;

public class ActivePurchase
{
	public enum Part
	{
		TOKEN,
		INFO,
		ITEM,
		PID,
		XPS,
		INVOICE,
		PROCEED,
		NULL,
		PAYMENT_MANAGER,
		PAYMENT_MANAGER_REPLACED
	}

	private Dictionary<Part, Dictionary<string, object>> purchase;

	public Part lastAdded { get; private set; }

	public int counter { get; private set; }

	public ActivePurchase()
	{
		purchase = new Dictionary<Part, Dictionary<string, object>>();
		lastAdded = Part.NULL;
		counter = 0;
	}

	public void Add(Part part, Dictionary<string, object> map)
	{
		purchase.Add(part, map);
		lastAdded = part;
		counter++;
	}

	public void RemoveAllExceptToken()
	{
		foreach (Part item in new List<Part>(purchase.Keys))
		{
			if (item != 0)
			{
				Remove(item);
			}
		}
		lastAdded = Part.TOKEN;
		counter = 1;
	}

	public void RemoveLast()
	{
		if (IsActive() && purchase.ContainsKey(lastAdded))
		{
			Remove(lastAdded);
			counter--;
		}
	}

	public void Remove(Part part)
	{
		if (purchase.ContainsKey(part))
		{
			purchase.Remove(part);
			counter--;
		}
	}

	public bool ContainsKey(Part part)
	{
		return purchase.ContainsKey(part);
	}

	public Dictionary<string, object> GetPart(Part part)
	{
		return purchase[part];
	}

	public Dictionary<string, object> GetMergedMap()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		IEnumerator<KeyValuePair<Part, Dictionary<string, object>>> enumerator = purchase.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Logger.Log(enumerator.Current.ToString());
			dictionary = dictionary.Concat(enumerator.Current.Value).ToDictionary((KeyValuePair<string, object> d) => d.Key, (KeyValuePair<string, object> d) => d.Value);
		}
		return dictionary;
	}

	public bool IsActive()
	{
		return lastAdded != Part.NULL;
	}
}
