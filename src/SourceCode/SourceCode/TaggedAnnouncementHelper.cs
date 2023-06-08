using System.Collections.Generic;
using System.Text.RegularExpressions;

public class TaggedAnnouncementHelper
{
	public Dictionary<string, string> Announcement;

	private static Regex matchKeyValue = new Regex("\\[{2}([\\s\\S])+?\\]{2}=\\[{2}([\\s\\S])+?\\]{2}(\\&{2})?");

	private static Regex matchWord = new Regex("\\[{2}([\\s\\S])+?\\]{2}");

	public TaggedAnnouncementHelper(string text)
	{
		Announcement = Match(text);
	}

	private static Dictionary<string, string> Match(string stringToParse)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (string.IsNullOrEmpty(stringToParse))
		{
			return dictionary;
		}
		foreach (Match item in matchKeyValue.Matches(stringToParse))
		{
			MatchCollection matchCollection = matchWord.Matches(item.Value);
			if (matchCollection.Count == 2)
			{
				string value = matchCollection[0].Value;
				value = value.Substring(2, value.Length - 4);
				string value2 = matchCollection[1].Value;
				value2 = value2.Substring(2, value2.Length - 4);
				dictionary.Add(value, value2);
			}
		}
		return dictionary;
	}
}
