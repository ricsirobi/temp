using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Xsolla;

public class TransactionHelper
{
	public const string KEY_TRANSICTION = "unfinished_transiction";

	public const string KEY_PURCHASE = "unfinished_purchase";

	private const string trackedKey = "tracked_purchase";

	public static void SaveRequest(Dictionary<string, object> dict)
	{
		PlayerPrefs.SetString("unfinished_transiction", DictToString(dict));
	}

	public static void SavePurchase(Dictionary<string, object> dict)
	{
		PlayerPrefs.SetString("unfinished_purchase", DictToString(dict));
	}

	public static Dictionary<string, object> LoadRequest()
	{
		string @string = PlayerPrefs.GetString("unfinished_transiction");
		if (@string != null && !"".Equals(@string))
		{
			return StringToDict(@string);
		}
		return null;
	}

	public static Dictionary<string, object> LoadPurchase()
	{
		string @string = PlayerPrefs.GetString("unfinished_purchase");
		if (@string != null && !"".Equals(@string))
		{
			return StringToDict(@string);
		}
		return null;
	}

	public static void Clear()
	{
		PlayerPrefs.DeleteKey("unfinished_transiction");
		PlayerPrefs.DeleteKey("unfinished_purchase");
	}

	public static bool CheckUnfinished()
	{
		if (PlayerPrefs.HasKey("unfinished_transiction"))
		{
			return PlayerPrefs.HasKey("unfinished_purchase");
		}
		return false;
	}

	public static bool LogPurchase(string invoiceID)
	{
		if (PlayerPrefs.GetString("tracked_purchase", "FALSE") == "FALSE")
		{
			PlayerPrefs.SetString("tracked_purchase", "");
		}
		invoiceID = "<" + invoiceID + ">";
		if (PlayerPrefs.GetString("tracked_purchase").IndexOf(invoiceID) == -1)
		{
			PlayerPrefs.SetString("tracked_purchase", PlayerPrefs.GetString("tracked_purchase") + invoiceID);
			return true;
		}
		return false;
	}

	private static string DictToString(Dictionary<string, object> dict)
	{
		return string.Join("; ", dict.Select((KeyValuePair<string, object> p) => string.Format("{0}, {1}", p.Key, (p.Value != null) ? p.Value.ToString() : "")).ToArray()).ToString();
	}

	private static Dictionary<string, object> StringToDict(string dictString)
	{
		return (from s in dictString.Split(';')
			select s.Split(',')).ToDictionary((Func<string[], string>)((string[] p) => p[0].Trim()), (Func<string[], object>)((string[] p) => p[1].Trim()));
	}
}
