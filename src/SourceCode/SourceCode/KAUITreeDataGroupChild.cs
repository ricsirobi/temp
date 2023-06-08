using System;
using System.Collections.Generic;

[Serializable]
public class KAUITreeDataGroupChild
{
	public string _Name;

	public string _URL;

	public LocaleString _DisplayText;

	public List<TreeMapData> _DataList;

	public KAUITreeDataGroupChild(string inName, LocaleString inLocaleString, string inURL, TreeMapData[] inDataList)
	{
		_Name = inName;
		_DisplayText = inLocaleString;
		_URL = inURL;
		_DataList = new List<TreeMapData>(inDataList);
	}

	public KAUITreeDataGroupChild(string inName, LocaleString inLocaleString)
	{
		_Name = inName;
		_DisplayText = inLocaleString;
		_URL = "";
		_DataList = new List<TreeMapData>();
	}

	public LocaleString GetValue(string inKey)
	{
		foreach (TreeMapData data in _DataList)
		{
			if (data._Key == inKey)
			{
				return data._Text;
			}
		}
		return null;
	}

	public void AddData(string inName, LocaleString inLocaleString)
	{
		if (_DataList != null)
		{
			_DataList.Add(new TreeMapData(inName, inLocaleString));
		}
	}
}
