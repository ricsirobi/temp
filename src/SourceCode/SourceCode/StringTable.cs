using System.Collections.Generic;
using JSGames.GlyphMapping;

public class StringTable
{
	public static StringTable pInstance;

	private Dictionary<int, string> mDictionary;

	private GlyphCorrector mGlyphMapper;

	public StringTable(string languageCode)
	{
		mGlyphMapper = new GlyphManager().GetGlyphCorrector(languageCode);
		if (mGlyphMapper != null)
		{
			mGlyphMapper.Initialize();
		}
	}

	public static string GetStringData(int id, string defaultTxt)
	{
		if (id == 0 || pInstance == null)
		{
			return defaultTxt;
		}
		string text = defaultTxt;
		if (pInstance.mDictionary.ContainsKey(id))
		{
			text = pInstance.mDictionary[id];
			if (pInstance.mGlyphMapper != null)
			{
				text = pInstance.mGlyphMapper.GetCharMapping(text);
			}
		}
		return text;
	}

	public static StringTable CreateStringTable(StringData[] sd)
	{
		if (sd != null && sd.Length != 0)
		{
			StringTable stringTable = new StringTable(sd[0].Locale);
			stringTable.mDictionary = new Dictionary<int, string>();
			foreach (StringData stringData in sd)
			{
				stringTable.mDictionary[(int)stringData.ResID] = stringData.LocaleString;
				if (!string.IsNullOrEmpty(stringData.FontName))
				{
					FontInstance fontInstance = new FontInstance();
					fontInstance.Init(stringData.FontName);
					LocaleData.AddFont(fontInstance);
				}
			}
			return stringTable;
		}
		return null;
	}

	public static string ApplyRTLfix(string line, int maxCharacters = 0, bool ignoreNumbers = true)
	{
		if (string.IsNullOrEmpty(line))
		{
			return line;
		}
		int tagStart = -1;
		int num = 0;
		int num2 = 40000;
		num = 0;
		List<string> list = new List<string>();
		while (UtStringUtil.FindNextTag(line, num, out tagStart, out num))
		{
			string text = "@@" + (char)(num2 + list.Count) + "@@";
			list.Add(line.Substring(tagStart, num - tagStart + 1));
			line = line.Substring(0, tagStart) + text + line.Substring(num + 1);
			num = tagStart + 5;
		}
		line = line.Replace("\r\n", "\n");
		line = UtStringUtil.SplitLine(line, maxCharacters);
		line = RTLFixer.Fix(line, showTashkeel: true, !ignoreNumbers);
		for (int i = 0; i < list.Count; i++)
		{
			int length = line.Length;
			for (int j = 0; j < length; j++)
			{
				if (line[j] == '@' && line[j + 1] == '@' && line[j + 2] >= num2 && line[j + 3] == '@' && line[j + 4] == '@')
				{
					int num3 = line[j + 2] - num2;
					if (num3 >= list.Count)
					{
						num3 = list.Count - 1;
					}
					line = line.Substring(0, j) + list[num3] + line.Substring(j + 5);
					break;
				}
			}
		}
		return line;
	}
}
