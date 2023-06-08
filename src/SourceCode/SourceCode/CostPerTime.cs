using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "CostPerTime", Namespace = "")]
public class CostPerTime
{
	[XmlElement(ElementName = "CostItems")]
	public List<CostPerMinite> CostItems;

	public int GetCost(int duration)
	{
		int num = 0;
		if (CostItems != null)
		{
			CostPerMinite costPerMinite = null;
			foreach (CostPerMinite costItem in CostItems)
			{
				if (costItem.pCompletionItem != null)
				{
					costPerMinite = costItem;
					int num2 = duration / costItem.Duration;
					num += num2 * costItem.pCompletionItem.FinalCashCost;
					duration -= num2 * costItem.Duration;
				}
				if (duration <= 0)
				{
					break;
				}
			}
			if (costPerMinite != null && duration > 0)
			{
				num += costPerMinite.pCompletionItem.FinalCashCost;
			}
		}
		return num;
	}

	public Dictionary<int, int> GetCostItems(int duration)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		if (CostItems != null)
		{
			CostPerMinite costPerMinite = null;
			foreach (CostPerMinite costItem in CostItems)
			{
				if (costItem.pCompletionItem != null)
				{
					costPerMinite = costItem;
					int num = duration / costItem.Duration;
					dictionary.Add(costItem.ItemID, num);
					duration -= num * costItem.Duration;
				}
				if (duration <= 0)
				{
					break;
				}
			}
			if (costPerMinite != null && duration > 0)
			{
				if (dictionary.ContainsKey(costPerMinite.ItemID))
				{
					dictionary[costPerMinite.ItemID] = dictionary[costPerMinite.ItemID] + 1;
				}
				else
				{
					dictionary.Add(costPerMinite.ItemID, 1);
				}
			}
		}
		return dictionary;
	}
}
