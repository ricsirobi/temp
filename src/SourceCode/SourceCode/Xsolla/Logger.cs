using System.Collections.Generic;

namespace Xsolla;

public class Logger
{
	public static bool isLogRequired;

	public static void Log(string message)
	{
		if (isLogRequired)
		{
			UtDebug.Log(message);
		}
	}

	public static void Log(string elemName, Dictionary<string, object> dictToLog)
	{
		if (dictToLog == null)
		{
			Log("Empty dict");
			return;
		}
		if (dictToLog.Count > 0)
		{
			foreach (KeyValuePair<string, object> item in dictToLog)
			{
				Log(elemName + " key =" + item.Key + " value = " + item.Value);
			}
			return;
		}
		Log("Empty dict");
	}
}
