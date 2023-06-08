using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;

public class UtStringUtil
{
	public static string Wordwrap(string str, int lineLength)
	{
		if (lineLength <= 0 || str.Length <= lineLength)
		{
			return str;
		}
		ArrayList arrayList = new ArrayList();
		while (str.Length > lineLength)
		{
			bool flag = false;
			for (int num = lineLength; num >= 0; num--)
			{
				if (str.Substring(num, 1) == " ")
				{
					arrayList.Add(str.Substring(0, num));
					str = str.Substring(num + 1);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				arrayList.Add(str.Substring(0, lineLength));
				str = str.Substring(lineLength);
			}
		}
		arrayList.Add(str);
		string text = "";
		for (int num = 0; num < arrayList.Count; num++)
		{
			string text2 = (string)arrayList[num];
			text2 = text2.TrimStart(' ');
			text = text + text2 + "\n";
		}
		return text;
	}

	public static string SplitLine(string line, int maxCharacters)
	{
		if (maxCharacters <= 0 || line.Length < maxCharacters)
		{
			return line;
		}
		char[] array = line.ToCharArray();
		bool flag = true;
		bool flag2 = false;
		int i = 0;
		int num = 0;
		for (; i < array.Length; i++)
		{
			if (flag)
			{
				num++;
				if (array[i] == '\n')
				{
					num = 0;
				}
				if (num >= maxCharacters && char.IsWhiteSpace(array[i]))
				{
					array[i] = '\n';
					flag = false;
					flag2 = false;
				}
			}
			else if (!char.IsWhiteSpace(array[i]))
			{
				flag = true;
				num = 0;
			}
			else if (array[i] != '\n')
			{
				array[i] = '\0';
			}
			else
			{
				if (!flag2)
				{
					array[i] = '\0';
				}
				flag2 = true;
			}
		}
		return new string(array.Where((char c) => c != '\0').ToArray());
	}

	public static int CountLines(string str)
	{
		return str.Split("\n"[0]).Length;
	}

	public static TYPE Parse<TYPE>(string inString, TYPE inDefault)
	{
		try
		{
			Type typeFromHandle = typeof(TYPE);
			if (typeFromHandle.Equals(typeof(int)))
			{
				return (TYPE)(object)int.Parse(inString);
			}
			if (typeFromHandle.Equals(typeof(float)))
			{
				return (TYPE)(object)float.Parse(inString);
			}
			if (typeFromHandle.Equals(typeof(bool)))
			{
				if (inString.Equals("t", StringComparison.OrdinalIgnoreCase) || inString.Equals("1", StringComparison.OrdinalIgnoreCase) || inString.Equals("true", StringComparison.OrdinalIgnoreCase))
				{
					return (TYPE)(object)true;
				}
				if (inString.Equals("f", StringComparison.OrdinalIgnoreCase) || inString.Equals("0", StringComparison.OrdinalIgnoreCase) || inString.Equals("false", StringComparison.OrdinalIgnoreCase))
				{
					return (TYPE)(object)false;
				}
			}
			else
			{
				if (typeFromHandle.Equals(typeof(string)))
				{
					return (TYPE)(object)inString;
				}
				if (typeFromHandle.Equals(typeof(DateTime)))
				{
					return (TYPE)(object)DateTime.Parse(inString, UtUtilities.GetCultureInfo("en-US"));
				}
			}
			return inDefault;
		}
		catch
		{
			return inDefault;
		}
	}

	public static bool IsRTL(string inString)
	{
		return new Regex("[\u0600-ۿ]|[ݐ-ݿ]|[ﭐ-ﰿ]|[ﹰ-ﻼ]").IsMatch(inString);
	}

	public static bool FindNextTag(string line, int iStart, out int tagStart, out int tagEnd)
	{
		tagStart = -1;
		tagEnd = -1;
		int length = line.Length;
		tagStart = iStart;
		while (tagStart < length && line[tagStart] != '[' && line[tagStart] != '(' && line[tagStart] != '{' && line[tagStart] != '<')
		{
			tagStart++;
		}
		if (tagStart == length)
		{
			return false;
		}
		bool flag = false;
		for (tagEnd = tagStart + 1; tagEnd < length; tagEnd++)
		{
			char c = line[tagEnd];
			if (c == ']' || c == ')' || c == '}' || c == '>')
			{
				if (flag)
				{
					return FindNextTag(line, tagEnd + 1, out tagStart, out tagEnd);
				}
				return true;
			}
			if (c > 'ÿ')
			{
				flag = true;
			}
		}
		return false;
	}
}
