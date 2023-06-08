using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "ServerDownScheduled", Namespace = "")]
public class ServerDownScheduled
{
	[XmlElement(ElementName = "StartTime")]
	public string StartTime;

	[XmlElement(ElementName = "EndTime")]
	public string EndTime;

	[XmlElement(ElementName = "ModifiedTime")]
	public string ModifiedTime;

	[XmlElement(ElementName = "Message")]
	public List<ServerDownMessage> Messages;

	[XmlElement(ElementName = "NoticeBeforeInDays")]
	public int NoticeBeforeInDays;

	[XmlElement(ElementName = "DisplayCount")]
	public int DisplayCount;

	public static bool ShouldShowMessage(string userID)
	{
		if (ServerDown.pInstance == null || ServerDown.pInstance.Scheduled == null || !ServerTime.pIsReady)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(ServerDown.pInstance.Scheduled.StartTime) && !string.IsNullOrEmpty(ServerDown.pInstance.Scheduled.EndTime) && !string.IsNullOrEmpty(ServerDown.pInstance.Scheduled.ModifiedTime) && DateTime.TryParseExact(ServerDown.pInstance.Scheduled.StartTime.Trim(), "MM/dd/yyyy hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
		{
			double totalDays = result.Subtract(UtUtilities.GetPSTTimeFromUTC(ServerTime.pCurrentTime)).TotalDays;
			if (totalDays < 0.0 || totalDays > (double)ServerDown.pInstance.Scheduled.NoticeBeforeInDays)
			{
				return false;
			}
			if (ServerDown.pInstance.Scheduled.DisplayCount < 0)
			{
				return true;
			}
			return SaveServerDownTime(ServerDown.pInstance.Scheduled, userID);
		}
		return false;
	}

	private static bool SaveServerDownTime(ServerDownScheduled serverDownData, string userID)
	{
		char separator = '#';
		DateTime value = DateTime.ParseExact(serverDownData.ModifiedTime.Trim(), "MM/dd/yyyy hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None);
		int result = 1;
		string key = "SERVER_DOWN_" + userID;
		if (PlayerPrefs.HasKey(key))
		{
			string[] array = PlayerPrefs.GetString(key).Split(separator);
			if (array.Length > 1)
			{
				string text = array[0];
				int.TryParse(array[1], out result);
				if (!string.IsNullOrEmpty(text))
				{
					if (DateTime.ParseExact(text, "MM/dd/yyyy hh:mm tt", CultureInfo.InvariantCulture).Subtract(value).Ticks != 0L)
					{
						result = 1;
					}
					else
					{
						if (result >= serverDownData.DisplayCount)
						{
							return false;
						}
						result++;
					}
				}
			}
		}
		string value2 = serverDownData.ModifiedTime + separator + result;
		PlayerPrefs.SetString(key, value2);
		return true;
	}

	public ServerDownMessage GetMessage()
	{
		ServerDownMessage message = ServerDown.pInstance.GetMessage(Messages);
		if (message != null && message.TimeFormat != null)
		{
			DateTime dateTime = DateTime.ParseExact(ServerDown.pInstance.Scheduled.StartTime, "MM/dd/yyyy hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None);
			DateTime dateTime2 = DateTime.ParseExact(ServerDown.pInstance.Scheduled.EndTime, "MM/dd/yyyy hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None);
			ServerDown.pInstance.Scheduled.StartTime = dateTime.ToString(message.TimeFormat);
			ServerDown.pInstance.Scheduled.EndTime = dateTime2.ToString(message.TimeFormat);
		}
		return message;
	}
}
