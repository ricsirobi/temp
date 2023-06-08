using System;
using System.Collections.Generic;
using UnityEngine;

public static class NotificationData
{
	private static Dictionary<string, string> mNotificationData = new Dictionary<string, string>();

	public static int noOfChildData = 0;

	public static string CurrentUserID = "";

	public static string CurrentUserName = "";

	public static int idxUserDataDate = 0;

	public static int idxUserDataName = 1;

	public static int idxUserDataMessage = 2;

	private static DateTime LastPurchaseTime = DateTime.Now;

	public static void SetCurrentUser(string id, string name)
	{
		CurrentUserID = id;
		CurrentUserName = name;
	}

	public static void SetNotification(DateTime noticationTime, string uId, string message)
	{
		if (!string.IsNullOrEmpty(uId) && uId.Contains(CurrentUserID))
		{
			string text = noticationTime.ToString();
			string text2 = text.Length.ToString("00");
			string text3 = CurrentUserName.Length.ToString("00");
			string text4 = message.Length.ToString("000");
			string value = text2 + text + text3 + CurrentUserName + text4 + message;
			if (mNotificationData.ContainsKey(uId))
			{
				mNotificationData[uId] = value;
			}
			else
			{
				mNotificationData.Add(uId, value);
			}
		}
	}

	private static void ValidateData()
	{
		if (mNotificationData.Count <= 0)
		{
			return;
		}
		foreach (string item in new List<string>(mNotificationData.Keys))
		{
			string text = mNotificationData[item];
			int num = 0;
			int length = int.Parse(text.Substring(num, 2));
			num += 2;
			if ((DateTime.Parse(text.Substring(num, length)) - DateTime.Now).TotalSeconds <= 0.0)
			{
				mNotificationData.Remove(item);
			}
		}
		noOfChildData = mNotificationData.Count;
	}

	public static void SaveData()
	{
		ValidateData();
		noOfChildData = mNotificationData.Count;
		string text = noOfChildData.ToString("0000");
		if (noOfChildData > 0)
		{
			foreach (string key in mNotificationData.Keys)
			{
				string text2 = mNotificationData[key];
				string text3 = key.Length.ToString("0000");
				text = text + text3 + key;
				text3 = text2.Length.ToString("0000");
				text = text + text3 + text2;
			}
		}
		PlayerPrefs.SetString("NDATA", text);
		PlayerPrefs.SetString("PDATE", LastPurchaseTime.ToString());
	}

	public static void LoadData()
	{
		string @string = PlayerPrefs.GetString("NDATA", "0000");
		int num = 0;
		noOfChildData = int.Parse(@string.Substring(num, 4));
		num += 4;
		if (noOfChildData > 0)
		{
			while (num < @string.Length)
			{
				string s = @string.Substring(num, 4);
				num += 4;
				int num2 = int.Parse(s);
				string key = @string.Substring(num, num2);
				num += num2;
				string s2 = @string.Substring(num, 4);
				num += 4;
				num2 = int.Parse(s2);
				string value = @string.Substring(num, num2);
				num += num2;
				if (mNotificationData.ContainsKey(key))
				{
					mNotificationData[key] = value;
				}
				else
				{
					mNotificationData.Add(key, value);
				}
			}
		}
		string string2 = PlayerPrefs.GetString("PDATE", "NODATE");
		if (!string2.Equals("NODATE"))
		{
			LastPurchaseTime = DateTime.Parse(string2);
		}
	}

	public static string[,] GetUserData()
	{
		if (noOfChildData <= 0)
		{
			return null;
		}
		string[,] array = new string[noOfChildData, 3];
		Dictionary<string, string>.ValueCollection values = mNotificationData.Values;
		int num = 0;
		foreach (string item in values)
		{
			int num2 = 0;
			int num3 = int.Parse(item.Substring(num2, 2));
			num2 += 2;
			array[num, idxUserDataDate] = item.Substring(num2, num3);
			num2 += num3;
			num3 = int.Parse(item.Substring(num2, 2));
			num2 += 2;
			array[num, idxUserDataName] = item.Substring(num2, num3);
			num2 += num3;
			num3 = int.Parse(item.Substring(num2, 3));
			num2 += 3;
			array[num, idxUserDataMessage] = item.Substring(num2, num3);
			num++;
		}
		return array;
	}

	public static void RemoveNotification(string uId)
	{
		if (mNotificationData.ContainsKey(uId))
		{
			mNotificationData.Remove(uId);
		}
	}

	public static void SetLastPurchaseTime()
	{
		LastPurchaseTime = DateTime.Now;
	}

	public static int GetLastPurchaseDuration()
	{
		return Convert.ToInt32(DateTime.Now.Subtract(LastPurchaseTime).TotalSeconds);
	}

	public static bool IsNotificationAvailable(string key)
	{
		return mNotificationData.ContainsKey(key);
	}

	public static DateTime GetNotificationTime(string key)
	{
		if (mNotificationData.Count > 0 && mNotificationData.ContainsKey(key))
		{
			string text = mNotificationData[key];
			int num = 0;
			int length = int.Parse(text.Substring(num, 2));
			num += 2;
			return DateTime.Parse(text.Substring(num, length));
		}
		return default(DateTime);
	}
}
