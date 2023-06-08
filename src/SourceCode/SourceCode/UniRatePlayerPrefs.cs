using System;
using UnityEngine;

public class UniRatePlayerPrefs
{
	public static void SetDate(string key, DateTime value)
	{
		PlayerPrefs.SetString(key, value.Ticks.ToString());
	}

	public static DateTime GetDate(string key)
	{
		string @string = PlayerPrefs.GetString(key);
		long result = 0L;
		if (long.TryParse(@string, out result))
		{
			return new DateTime(result);
		}
		return DateTime.MaxValue;
	}
}
