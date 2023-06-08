using System;
using System.Collections.Generic;
using UnityEngine;

public class LocaleStrings : ScriptableObject
{
	[Serializable]
	public class LocaleStringKey
	{
		public string _Key;

		public LocaleString _Text;
	}

	public List<LocaleStringKey> _LocaleStrings = new List<LocaleStringKey>();

	private static LocaleStrings mInstance;

	public static LocaleStrings pInstance
	{
		get
		{
			mInstance = Resources.Load("LocaleStrings") as LocaleStrings;
			if (mInstance == null)
			{
				mInstance = ScriptableObject.CreateInstance<LocaleStrings>();
			}
			return mInstance;
		}
	}

	public string GetString(string key, string defaultText)
	{
		if (_LocaleStrings != null)
		{
			LocaleStringKey localeStringKey = _LocaleStrings.Find((LocaleStringKey a) => a._Key == key);
			if (localeStringKey != null)
			{
				return localeStringKey._Text.GetLocalizedString();
			}
		}
		return defaultText;
	}
}
