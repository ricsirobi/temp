using System;

namespace Xsolla;

public class StringHelper
{
	public static string DateFormat(DateTime pDate)
	{
		return $"{pDate:dd.MM.yyyy}";
	}

	public static string PrepareFormatString(string pInnerString)
	{
		string text = pInnerString;
		int num = 0;
		while (text.Contains("{{"))
		{
			string oldValue = text.Substring(text.IndexOf("{{", 0) + 1, text.IndexOf("}}", 0) - text.IndexOf("{{", 0));
			text = text.Replace(oldValue, num.ToString());
			num++;
		}
		return text;
	}
}
