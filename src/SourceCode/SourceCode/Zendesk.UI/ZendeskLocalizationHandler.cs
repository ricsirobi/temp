using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Zendesk.Internal.Models.Core;

namespace Zendesk.UI;

public class ZendeskLocalizationHandler : MonoBehaviour
{
	public TextAsset translationsFile;

	[HideInInspector]
	public string defaultLocale = "en-us";

	[HideInInspector]
	public int? defaultLocaleIndex;

	public Dictionary<string, string> translationGameObjects = new Dictionary<string, string>();

	private readonly string[] lineSeperator = new string[1] { "\r\n" };

	private List<string> langList = new List<string>();

	public string Locale { get; set; }

	public void SetLocaleISO(int locale = 42)
	{
		int num = (int)Application.systemLanguage;
		if (locale != 42)
		{
			num = locale;
		}
		if (!CheckIfLanguageSupported(num))
		{
			num = 42;
		}
		switch (num)
		{
		case 1:
			Locale = "ar";
			break;
		case 28:
			Locale = "pt-br";
			break;
		case 4:
			Locale = "bg";
			break;
		case 7:
			Locale = "cs";
			break;
		case 17:
			Locale = "he";
			break;
		case 8:
			Locale = "da";
			break;
		case 9:
			Locale = "nl";
			break;
		case 10:
			Locale = "en-US";
			break;
		case 13:
			Locale = "fi";
			break;
		case 14:
			Locale = "fr";
			break;
		case 18:
			Locale = "hu";
			break;
		case 15:
			Locale = "de";
			break;
		case 16:
			Locale = "el";
			break;
		case 20:
			Locale = "id";
			break;
		case 21:
			Locale = "it";
			break;
		case 22:
			Locale = "ja";
			break;
		case 23:
			Locale = "ko";
			break;
		case 26:
			Locale = "no";
			break;
		case 27:
			Locale = "pl";
			break;
		case 29:
			Locale = "ro";
			break;
		case 30:
			Locale = "ru";
			break;
		case 6:
			Locale = "zh-cn";
			break;
		case 34:
			Locale = "es";
			break;
		case 36:
			Locale = "th";
			break;
		case 40:
			Locale = "zh-cn";
			break;
		case 41:
			Locale = "zh-tw";
			break;
		case 37:
			Locale = "tr";
			break;
		case 39:
			Locale = "vi";
			break;
		case 0:
			Locale = "af";
			break;
		case 2:
			Locale = "eu";
			break;
		case 3:
			Locale = "be";
			break;
		case 5:
			Locale = "ca";
			break;
		case 11:
			Locale = "et";
			break;
		case 12:
			Locale = "fo";
			break;
		case 19:
			Locale = "is";
			break;
		case 24:
			Locale = "lv";
			break;
		case 25:
			Locale = "lt";
			break;
		case 32:
			Locale = "sk";
			break;
		case 35:
			Locale = "sv";
			break;
		case 38:
			Locale = "uk";
			break;
		case 31:
			Locale = "hr";
			break;
		default:
			Locale = defaultLocale;
			break;
		}
	}

	public string SetLocaleISOForUserAuth()
	{
		return (int)Application.systemLanguage switch
		{
			1 => "ar", 
			28 => "pt-br", 
			4 => "bg", 
			7 => "cs", 
			17 => "he", 
			8 => "da", 
			9 => "nl", 
			10 => "en-US", 
			13 => "fi", 
			14 => "fr", 
			18 => "hu", 
			15 => "de", 
			16 => "el", 
			20 => "id", 
			21 => "it", 
			22 => "ja", 
			23 => "ko", 
			26 => "no", 
			27 => "pl", 
			29 => "ro", 
			30 => "ru", 
			6 => "zh-cn", 
			34 => "es", 
			36 => "th", 
			40 => "zh-cn", 
			41 => "zh-tw", 
			37 => "tr", 
			39 => "vi", 
			0 => "af", 
			2 => "eu", 
			3 => "be", 
			5 => "ca", 
			11 => "et", 
			12 => "fo", 
			19 => "is", 
			24 => "lv", 
			25 => "lt", 
			32 => "sk", 
			35 => "sv", 
			38 => "uk", 
			31 => "hr", 
			_ => defaultLocale, 
		};
	}

	private bool CheckIfLanguageSupported(int currentLocale)
	{
		foreach (ZendeskLocales value in Enum.GetValues(typeof(ZendeskLocales)))
		{
			if (currentLocale == (int)value)
			{
				return true;
			}
		}
		return false;
	}

	public void ReadData(string helpCenterLocale = "", bool checkSuccess = true)
	{
		translationGameObjects.Clear();
		langList.Clear();
		string[] array = translationsFile.text.Split(lineSeperator, StringSplitOptions.RemoveEmptyEntries);
		bool flag = true;
		int num = -1;
		int? num2 = null;
		Regex regex = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (string.IsNullOrEmpty(text))
			{
				continue;
			}
			string[] array3 = regex.Split(text);
			if (flag)
			{
				if (!string.IsNullOrEmpty(Locale) && array3.Contains(Locale))
				{
					num = Array.FindIndex(array3, (string t) => t.Equals(Locale, StringComparison.InvariantCultureIgnoreCase));
					num2 = 1;
				}
				else if (!string.IsNullOrEmpty(helpCenterLocale) && array3.Contains(helpCenterLocale))
				{
					num = Array.FindIndex(array3, (string t) => t.Equals(helpCenterLocale, StringComparison.InvariantCultureIgnoreCase));
					Locale = helpCenterLocale;
				}
				else
				{
					if (!array3.Any((string x) => x.ToLower() == defaultLocale))
					{
						throw new Exception("<color='red'><b>[ZENDESK ERROR]</b>Control the csv file under Zendesk/Localization folder, locale can not be found.</color>");
					}
					num = Array.FindIndex(array3, (string t) => t.Equals(defaultLocale, StringComparison.InvariantCultureIgnoreCase));
					Locale = defaultLocale;
					num2 = 0;
				}
				if (array3.Any((string x) => x.ToLower() == defaultLocale))
				{
					defaultLocaleIndex = Array.FindIndex(array3, (string t) => t.Equals(defaultLocale, StringComparison.InvariantCultureIgnoreCase));
				}
				langList.AddRange(array3);
				flag = false;
				continue;
			}
			string value = "";
			if (array3.Length > num)
			{
				value = GetRidOfCommaEscapers(array3[num]);
				if (string.IsNullOrEmpty(value) && defaultLocaleIndex.HasValue)
				{
					value = GetRidOfCommaEscapers(array3[defaultLocaleIndex.Value]);
				}
			}
			else if (defaultLocaleIndex.HasValue && array3.Length > defaultLocaleIndex.Value)
			{
				value = GetRidOfCommaEscapers(array3[defaultLocaleIndex.Value]);
			}
			translationGameObjects.Add(array3[0], value);
		}
		if (!checkSuccess)
		{
			return;
		}
		if (num2.HasValue)
		{
			if (num2 == 1)
			{
				Debug.Log(string.Format(translationGameObjects["usdk_locale_set_success_message"], Locale));
			}
			else if (num2 == 0)
			{
				Debug.Log(string.Format(translationGameObjects["usdk_locale_set_default_locale_message"], Locale));
			}
		}
		else
		{
			Debug.Log(string.Format(translationGameObjects["usdk_locale_set_help_center_locale_message"], Locale));
		}
	}

	private static string GetRidOfCommaEscapers(string translation)
	{
		if (translation.StartsWith("\"") && translation.EndsWith("\""))
		{
			return translation.Substring(1, translation.Length - 2);
		}
		return translation;
	}
}
