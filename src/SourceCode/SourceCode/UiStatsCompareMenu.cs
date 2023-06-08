using System.Collections.Generic;
using SquadTactics;
using UnityEngine;

public class UiStatsCompareMenu : KAUIMenu
{
	private enum StatCompareResult
	{
		Equal,
		Greater,
		Lesser
	}

	public Color _LowColor = Color.red;

	public Color _HighColor = Color.green;

	private readonly string mNA = "--";

	public void Populate(ItemStat[] equippedStats, ItemStat[] unequippedStats, bool showCompare = false)
	{
		List<ItemStat> list = new List<ItemStat>();
		if (equippedStats != null)
		{
			list.AddRange(equippedStats);
		}
		List<ItemStat> list2 = new List<ItemStat>();
		if (unequippedStats != null)
		{
			list2.AddRange(unequippedStats);
		}
		if (list.Count == 0 && list2.Count == 0)
		{
			KAWidget kAWidget = DuplicateWidget(_Template);
			AddWidget(kAWidget);
			kAWidget.SetVisibility(inVisible: true);
			ShowStatInfo(kAWidget, null, null, null, null);
			return;
		}
		for (int i = 0; i < Settings.pInstance._StatInfo.Length; i++)
		{
			StStatInfo statInfo = Settings.pInstance.GetStatInfoByID(i);
			if (statInfo == null || !statInfo._Display)
			{
				continue;
			}
			ItemStat itemStat = null;
			ItemStat itemStat2 = null;
			if (list.Count > 0)
			{
				itemStat = list.Find((ItemStat x) => x.ItemStatID == statInfo._StatID);
			}
			if (list2.Count > 0)
			{
				itemStat2 = list2.Find((ItemStat x) => x.ItemStatID == statInfo._StatID);
			}
			if (itemStat == null && itemStat2 == null)
			{
				continue;
			}
			KAWidget kAWidget2 = DuplicateWidget(_Template);
			AddWidget(kAWidget2);
			kAWidget2.SetVisibility(inVisible: true);
			string text = null;
			string localizedString = statInfo._DisplayText.GetLocalizedString();
			string text2 = null;
			string diffVal = null;
			int result = 0;
			int result2 = 0;
			if (itemStat != null)
			{
				text = itemStat.Value;
				int.TryParse(text, out result);
				list.Remove(itemStat);
			}
			if (itemStat2 != null)
			{
				text2 = itemStat2.Value;
				int.TryParse(text2, out result2);
				list2.Remove(itemStat2);
			}
			if (statInfo._Stat == SquadTactics.Stat.HEALTH)
			{
				result *= (int)statInfo._Value;
				if (result != 0)
				{
					text = result.ToString();
				}
				result2 *= (int)statInfo._Value;
				if (result2 != 0)
				{
					text2 = result2.ToString();
				}
			}
			StatCompareResult statCompareResult = ((result != result2) ? ((result2 > result) ? StatCompareResult.Greater : StatCompareResult.Lesser) : StatCompareResult.Equal);
			if (statCompareResult != 0)
			{
				diffVal = Mathf.Abs(result - result2).ToString();
			}
			ShowStatInfo(kAWidget2, text, localizedString, text2, diffVal, statCompareResult, showCompare);
			if (list.Count == 0 && list2.Count == 0)
			{
				break;
			}
		}
	}

	private void ShowStatInfo(KAWidget inWidget, string baseStat, string statName, string compareStat, string diffVal, StatCompareResult compareResult = StatCompareResult.Equal, bool showCompare = false)
	{
		KAWidget kAWidget = inWidget.FindChildItem("BaseStat");
		if (kAWidget != null)
		{
			kAWidget.SetText((baseStat != null) ? baseStat : mNA);
		}
		KAWidget kAWidget2 = inWidget.FindChildItem("StatName");
		if (kAWidget2 != null)
		{
			kAWidget2.SetText((statName != null) ? statName : mNA);
		}
		KAWidget kAWidget3 = inWidget.FindChildItem("ComparisonStat");
		kAWidget3.SetVisibility(showCompare);
		if (showCompare)
		{
			kAWidget3.SetText((compareStat != null) ? compareStat : mNA);
		}
		KAWidget kAWidget4 = inWidget.FindChildItem("StatDifference");
		kAWidget4.SetVisibility(showCompare);
		if (showCompare)
		{
			kAWidget4.SetText((diffVal != null) ? diffVal : mNA);
			switch (compareResult)
			{
			case StatCompareResult.Greater:
				kAWidget4.GetLabel().color = _HighColor;
				kAWidget2.GetLabel().color = _HighColor;
				break;
			case StatCompareResult.Lesser:
				kAWidget4.GetLabel().color = _LowColor;
				kAWidget2.GetLabel().color = _LowColor;
				break;
			default:
				kAWidget4.GetLabel().color = Color.white;
				kAWidget2.GetLabel().color = Color.white;
				break;
			}
		}
		else
		{
			kAWidget2.GetLabel().color = Color.white;
		}
		KAWidget kAWidget5 = inWidget.FindChildItem("DownArrow");
		if (kAWidget5 != null)
		{
			kAWidget5.SetVisibility(showCompare && diffVal != null && compareResult == StatCompareResult.Lesser);
		}
		KAWidget kAWidget6 = inWidget.FindChildItem("UpArrow");
		if (kAWidget6 != null)
		{
			kAWidget6.SetVisibility(showCompare && diffVal != null && compareResult == StatCompareResult.Greater);
		}
	}
}
