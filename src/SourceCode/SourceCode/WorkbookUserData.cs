using System;
using System.Collections.Generic;

[Serializable]
public class WorkbookUserData : KAUITreeListItemData
{
	[Serializable]
	public class WorkbookMapData
	{
		public string _Key;

		public LocaleString _Text;

		public WorkbookMapData(string inKey, LocaleString inValue)
		{
			_Key = inKey;
			_Text = inValue;
		}
	}

	public string _URL;

	public List<WorkbookMapData> _DataList;

	public WorkbookUserData(KAUITreeListItemData inParent, string inName, LocaleString inLocaleString, bool inCollapsed, List<KAUITreeListItemData> inChildList, string inUrl, List<WorkbookMapData> inDataList)
		: base(inParent, inName, inLocaleString, inCollapsed, inChildList)
	{
		_DataList = inDataList;
		_URL = inUrl;
	}

	public LocaleString GetValue(string inKey)
	{
		if (_DataList != null)
		{
			foreach (WorkbookMapData data in _DataList)
			{
				if (data._Key == inKey)
				{
					return data._Text;
				}
			}
		}
		return null;
	}
}
