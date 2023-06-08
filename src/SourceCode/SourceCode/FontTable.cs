using System.Collections.Generic;
using UnityEngine;

public class FontTable
{
	public static FontTable pInstance;

	private Dictionary<string, FontInstance> mDictionary;

	public static UIFont GetFontAtlas(string baseFont, UIFont defaultFont)
	{
		if (baseFont == "" || pInstance == null)
		{
			return defaultFont;
		}
		if (pInstance.mDictionary.ContainsKey(baseFont))
		{
			return pInstance.mDictionary[baseFont]._FontAtlas;
		}
		return defaultFont;
	}

	public static Font GetFont(string baseFont, Font defaultFont)
	{
		if (baseFont == "" || pInstance == null)
		{
			return defaultFont;
		}
		if (pInstance.mDictionary.ContainsKey(baseFont))
		{
			return pInstance.mDictionary[baseFont]._Font;
		}
		return defaultFont;
	}

	public static FontInstance GetFont(string baseFont)
	{
		if (baseFont == "" || pInstance == null)
		{
			return null;
		}
		if (pInstance.mDictionary.ContainsKey(baseFont))
		{
			return pInstance.mDictionary[baseFont];
		}
		return null;
	}

	public static FontTable CreateFontTable(FontData[] fd)
	{
		FontTable fontTable = new FontTable();
		fontTable.mDictionary = new Dictionary<string, FontInstance>();
		if (fd != null)
		{
			foreach (FontData fontData in fd)
			{
				FontInstance fontInstance = new FontInstance();
				fontInstance.Init(fontData.LocaleFont);
				fontInstance._FontData = fontData;
				LocaleData.AddFont(fontInstance);
				fontTable.mDictionary[fontData.BaseFont] = fontInstance;
			}
		}
		return fontTable;
	}
}
